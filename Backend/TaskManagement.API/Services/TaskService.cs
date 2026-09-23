using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs;
using TaskManagement.API.Exceptions;
using TaskManagement.API.Interfaces;
using TaskManagement.API.Models;

namespace TaskManagement.API.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public TaskService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<List<TaskResponseDto>> GetTasksAsync(TaskFilterDto filter, int currentUserId, string currentUserRole)
    {
        var query = _context.Tasks
            .Include(t => t.Team)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Comments)
            .AsNoTracking();

        // Role-based scoping
        if (currentUserRole == UserRole.Manager)
        {
            query = query.Where(t => t.Team.ManagerId == currentUserId ||
                                     _context.TeamMembers.Any(tm => tm.TeamId == t.TeamId && tm.UserId == currentUserId));
        }
        else if (currentUserRole == UserRole.User)
        {
            query = query.Where(t => t.AssignedToUserId == currentUserId);
        }

        // Filters
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(t => t.Status.ToLower() == filter.Status.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(filter.Priority))
        {
            query = query.Where(t => t.Priority.ToLower() == filter.Priority.Trim().ToLower());
        }

        if (filter.Deadline.HasValue)
        {
            var endOfDay = filter.Deadline.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(t => t.Deadline.HasValue && t.Deadline.Value <= endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search.Trim().ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(searchTerm) ||
                                     (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
        }

        var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

        return tasks.Select(MapToDto).ToList();
    }

    public async Task<TaskResponseDto> GetTaskByIdAsync(int id, int currentUserId, string currentUserRole)
    {
        var task = await _context.Tasks
            .Include(t => t.Team)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            throw new NotFoundException("Task not found.");
        }

        // Role validation
        if (currentUserRole == UserRole.Manager)
        {
            var managesTeam = task.Team.ManagerId == currentUserId;
            var isMember = await _context.TeamMembers
                .AnyAsync(tm => tm.TeamId == task.TeamId && tm.UserId == currentUserId);

            if (!managesTeam && !isMember)
            {
                throw new ForbiddenException("You do not have permission to view tasks outside your team.");
            }
        }
        else if (currentUserRole == UserRole.User)
        {
            if (task.AssignedToUserId != currentUserId)
            {
                throw new ForbiddenException("You do not have permission to view this task.");
            }
        }

        return MapToDto(task);
    }

    public async Task<TaskResponseDto> CreateTaskAsync(TaskCreateDto dto, int currentUserId, string currentUserRole)
    {
        if (currentUserRole == UserRole.User)
        {
            throw new ForbiddenException("Users do not have permission to create tasks.");
        }

        var team = await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == dto.TeamId);

        if (team == null)
        {
            throw new NotFoundException("Specified team was not found.");
        }

        // Manager check
        if (currentUserRole == UserRole.Manager && team.ManagerId != currentUserId)
        {
            throw new ForbiddenException("Managers can only create tasks for their own teams.");
        }

        // Assignee validation
        if (dto.AssignedToUserId.HasValue)
        {
            var assignee = await _context.Users.FindAsync(dto.AssignedToUserId.Value);
            if (assignee == null)
            {
                throw new BadRequestException("Assigned user does not exist.");
            }

            if (currentUserRole == UserRole.Manager)
            {
                var isMember = await _context.TeamMembers
                    .AnyAsync(tm => tm.TeamId == dto.TeamId && tm.UserId == dto.AssignedToUserId.Value);

                if (!isMember && team.ManagerId != dto.AssignedToUserId.Value)
                {
                    throw new BadRequestException("Cannot assign a task to a user outside your team.");
                }
            }
        }

        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            TeamId = dto.TeamId,
            AssignedToUserId = dto.AssignedToUserId,
            CreatedByUserId = currentUserId,
            Priority = dto.Priority,
            Status = dto.Status,
            Deadline = dto.Deadline,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Notify assignee if assigned
        if (task.AssignedToUserId.HasValue)
        {
            await _notificationService.CreateNotificationAsync(
                task.AssignedToUserId.Value,
                task.Id,
                $"You have been assigned to task: '{task.Title}'",
                "TaskAssigned"
            );
        }

        return await GetTaskByIdAsync(task.Id, currentUserId, currentUserRole);
    }

    public async Task<TaskResponseDto> UpdateTaskAsync(int id, TaskUpdateDto dto, int currentUserId, string currentUserRole)
    {
        if (currentUserRole == UserRole.User)
        {
            throw new ForbiddenException("Users do not have permission to update task details.");
        }

        var task = await _context.Tasks
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            throw new NotFoundException("Task not found.");
        }

        if (currentUserRole == UserRole.Manager && task.Team.ManagerId != currentUserId)
        {
            throw new ForbiddenException("Managers can only update tasks within their own team.");
        }

        // If team changed
        if (task.TeamId != dto.TeamId)
        {
            var targetTeam = await _context.Teams.FindAsync(dto.TeamId);
            if (targetTeam == null)
            {
                throw new NotFoundException("Target team not found.");
            }

            if (currentUserRole == UserRole.Manager && targetTeam.ManagerId != currentUserId)
            {
                throw new ForbiddenException("Managers cannot move tasks to teams they do not manage.");
            }

            task.TeamId = dto.TeamId;
        }

        var oldAssigneeId = task.AssignedToUserId;

        // Assignee validation
        if (dto.AssignedToUserId.HasValue)
        {
            var assignee = await _context.Users.FindAsync(dto.AssignedToUserId.Value);
            if (assignee == null)
            {
                throw new BadRequestException("Assigned user does not exist.");
            }

            if (currentUserRole == UserRole.Manager)
            {
                var isMember = await _context.TeamMembers
                    .AnyAsync(tm => tm.TeamId == task.TeamId && tm.UserId == dto.AssignedToUserId.Value);

                if (!isMember && task.Team.ManagerId != dto.AssignedToUserId.Value)
                {
                    throw new BadRequestException("Cannot assign a task to a user outside your team.");
                }
            }
        }

        task.Title = dto.Title.Trim();
        task.Description = dto.Description?.Trim();
        task.AssignedToUserId = dto.AssignedToUserId;
        task.Priority = dto.Priority;
        task.Status = dto.Status;
        task.Deadline = dto.Deadline;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Notification if assignee changed
        if (task.AssignedToUserId.HasValue && task.AssignedToUserId != oldAssigneeId)
        {
            await _notificationService.CreateNotificationAsync(
                task.AssignedToUserId.Value,
                task.Id,
                $"You have been assigned to task: '{task.Title}'",
                "TaskAssigned"
            );
        }

        return await GetTaskByIdAsync(task.Id, currentUserId, currentUserRole);
    }

    public async Task DeleteTaskAsync(int id, int currentUserId, string currentUserRole)
    {
        if (currentUserRole == UserRole.User)
        {
            throw new ForbiddenException("Users do not have permission to delete tasks.");
        }

        var task = await _context.Tasks
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            throw new NotFoundException("Task not found.");
        }

        if (currentUserRole == UserRole.Manager && task.Team.ManagerId != currentUserId)
        {
            throw new ForbiddenException("Managers can only delete tasks belonging to their team.");
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task<TaskResponseDto> UpdateStatusAsync(int id, TaskStatusUpdateDto dto, int currentUserId, string currentUserRole)
    {
        var task = await _context.Tasks
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            throw new NotFoundException("Task not found.");
        }

        // Authorization check
        if (currentUserRole == UserRole.User && task.AssignedToUserId != currentUserId)
        {
            throw new ForbiddenException("You can only update the status of tasks assigned to you.");
        }

        if (currentUserRole == UserRole.Manager)
        {
            var manages = task.Team.ManagerId == currentUserId;
            var isMember = await _context.TeamMembers
                .AnyAsync(tm => tm.TeamId == task.TeamId && tm.UserId == currentUserId);

            if (!manages && !isMember)
            {
                throw new ForbiddenException("You can only update the status of tasks in your team.");
            }
        }

        var oldStatus = task.Status;
        task.Status = dto.Status;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Trigger notification on status update
        if (task.AssignedToUserId.HasValue && task.AssignedToUserId.Value != currentUserId)
        {
            await _notificationService.CreateNotificationAsync(
                task.AssignedToUserId.Value,
                task.Id,
                $"Task '{task.Title}' status changed from '{oldStatus}' to '{dto.Status}'",
                "TaskStatusUpdated"
            );
        }

        // Also notify creator if someone else updated it
        if (task.CreatedByUserId != currentUserId && task.CreatedByUserId != task.AssignedToUserId)
        {
            await _notificationService.CreateNotificationAsync(
                task.CreatedByUserId,
                task.Id,
                $"Task '{task.Title}' status changed from '{oldStatus}' to '{dto.Status}'",
                "TaskStatusUpdated"
            );
        }

        return await GetTaskByIdAsync(task.Id, currentUserId, currentUserRole);
    }

    public async Task<TaskResponseDto> AssignTaskAsync(int id, TaskAssignDto dto, int currentUserId, string currentUserRole)
    {
        if (currentUserRole == UserRole.User)
        {
            throw new ForbiddenException("Users do not have permission to assign tasks.");
        }

        var task = await _context.Tasks
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            throw new NotFoundException("Task not found.");
        }

        if (currentUserRole == UserRole.Manager && task.Team.ManagerId != currentUserId)
        {
            throw new ForbiddenException("Managers can only assign tasks within their own team.");
        }

        if (dto.AssignedToUserId.HasValue)
        {
            var assignee = await _context.Users.FindAsync(dto.AssignedToUserId.Value);
            if (assignee == null)
            {
                throw new BadRequestException("Assigned user does not exist.");
            }

            if (currentUserRole == UserRole.Manager)
            {
                var isMember = await _context.TeamMembers
                    .AnyAsync(tm => tm.TeamId == task.TeamId && tm.UserId == dto.AssignedToUserId.Value);

                if (!isMember && task.Team.ManagerId != dto.AssignedToUserId.Value)
                {
                    throw new BadRequestException("Cannot assign a task to a user outside your team.");
                }
            }
        }

        task.AssignedToUserId = dto.AssignedToUserId;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (task.AssignedToUserId.HasValue)
        {
            await _notificationService.CreateNotificationAsync(
                task.AssignedToUserId.Value,
                task.Id,
                $"You have been assigned to task: '{task.Title}'",
                "TaskAssigned"
            );
        }

        return await GetTaskByIdAsync(task.Id, currentUserId, currentUserRole);
    }

    private static TaskResponseDto MapToDto(TaskItem task)
    {
        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            TeamId = task.TeamId,
            TeamName = task.Team?.Name ?? string.Empty,
            AssignedToUserId = task.AssignedToUserId,
            AssignedToUserName = task.AssignedToUser?.Name,
            AssignedToUserEmail = task.AssignedToUser?.Email,
            CreatedByUserId = task.CreatedByUserId,
            CreatedByUserName = task.CreatedByUser?.Name ?? string.Empty,
            Priority = task.Priority,
            Status = task.Status,
            Deadline = task.Deadline,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CommentsCount = task.Comments?.Count ?? 0
        };
    }
}
