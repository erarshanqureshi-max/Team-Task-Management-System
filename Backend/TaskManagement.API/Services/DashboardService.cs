using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs;
using TaskManagement.API.Interfaces;
using TaskManagement.API.Models;

namespace TaskManagement.API.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        var now = DateTime.UtcNow;

        var tasks = await _context.Tasks
            .Include(t => t.Team)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Comments)
            .AsNoTracking()
            .ToListAsync();

        var totalUsers = await _context.Users.CountAsync();
        var totalTeams = await _context.Teams.CountAsync();

        var stats = CalculateStats(tasks, now);

        var recentTasks = tasks
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(MapToDto)
            .ToList();

        return new AdminDashboardDto
        {
            Stats = stats,
            TotalUsers = totalUsers,
            TotalTeams = totalTeams,
            RecentTasks = recentTasks
        };
    }

    public async Task<ManagerDashboardDto> GetManagerDashboardAsync(int managerUserId)
    {
        var now = DateTime.UtcNow;

        var team = await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.ManagerId == managerUserId);

        if (team == null)
        {
            // If manager manages no team yet, check if member of a team
            team = await _context.Teams
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Members.Any(m => m.UserId == managerUserId));
        }

        if (team == null)
        {
            return new ManagerDashboardDto
            {
                Stats = new DashboardStatsDto(),
                TeamId = 0,
                TeamName = "No Assigned Team",
                TeamMemberCount = 0,
                RecentTasks = new List<TaskResponseDto>()
            };
        }

        var tasks = await _context.Tasks
            .Include(t => t.Team)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Comments)
            .Where(t => t.TeamId == team.Id)
            .AsNoTracking()
            .ToListAsync();

        var stats = CalculateStats(tasks, now);

        var recentTasks = tasks
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(MapToDto)
            .ToList();

        return new ManagerDashboardDto
        {
            Stats = stats,
            TeamId = team.Id,
            TeamName = team.Name,
            TeamMemberCount = team.Members.Count,
            RecentTasks = recentTasks
        };
    }

    public async Task<UserDashboardDto> GetUserDashboardAsync(int userId)
    {
        var now = DateTime.UtcNow;

        var tasks = await _context.Tasks
            .Include(t => t.Team)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Comments)
            .Where(t => t.AssignedToUserId == userId)
            .AsNoTracking()
            .ToListAsync();

        var stats = CalculateStats(tasks, now);

        var recentTasks = tasks
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(MapToDto)
            .ToList();

        return new UserDashboardDto
        {
            Stats = stats,
            RecentTasks = recentTasks
        };
    }

    private static DashboardStatsDto CalculateStats(List<TaskItem> tasks, DateTime now)
    {
        return new DashboardStatsDto
        {
            TotalTasks = tasks.Count,
            ToDoTasks = tasks.Count(t => t.Status == TaskStatusConstants.ToDo),
            InProgressTasks = tasks.Count(t => t.Status == TaskStatusConstants.InProgress),
            DoneTasks = tasks.Count(t => t.Status == TaskStatusConstants.Done),
            OverdueTasks = tasks.Count(t => t.Deadline.HasValue && t.Deadline.Value < now && t.Status != TaskStatusConstants.Done),
            HighPriorityTasks = tasks.Count(t => t.Priority == TaskPriorityConstants.High || t.Priority == TaskPriorityConstants.Urgent)
        };
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
