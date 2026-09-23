using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs;
using TaskManagement.API.Exceptions;
using TaskManagement.API.Interfaces;
using TaskManagement.API.Models;

namespace TaskManagement.API.Services;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _context;

    public CommentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CommentResponseDto>> GetTaskCommentsAsync(int taskId, int currentUserId, string currentUserRole)
    {
        var task = await _context.Tasks
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            throw new NotFoundException("Task not found.");
        }

        await EnsureUserHasTaskAccessAsync(task, currentUserId, currentUserRole);

        return await _context.Comments
            .AsNoTracking()
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentResponseDto
            {
                Id = c.Id,
                TaskId = c.TaskId,
                UserId = c.UserId,
                UserName = c.User.Name,
                UserEmail = c.User.Email,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CommentResponseDto> AddCommentAsync(int taskId, CommentCreateDto dto, int currentUserId, string currentUserRole)
    {
        var task = await _context.Tasks
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            throw new NotFoundException("Task not found.");
        }

        await EnsureUserHasTaskAccessAsync(task, currentUserId, currentUserRole);

        var comment = new Comment
        {
            TaskId = taskId,
            UserId = currentUserId,
            Content = dto.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(currentUserId);

        return new CommentResponseDto
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            UserId = comment.UserId,
            UserName = user?.Name ?? string.Empty,
            UserEmail = user?.Email ?? string.Empty,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }

    private async Task EnsureUserHasTaskAccessAsync(TaskItem task, int currentUserId, string currentUserRole)
    {
        if (currentUserRole == UserRole.Admin)
        {
            return;
        }

        if (currentUserRole == UserRole.Manager)
        {
            // Manager must manage the team or be a member of the team
            if (task.Team.ManagerId == currentUserId)
            {
                return;
            }

            var isMember = await _context.TeamMembers
                .AnyAsync(tm => tm.TeamId == task.TeamId && tm.UserId == currentUserId);

            if (!isMember)
            {
                throw new ForbiddenException("You do not have permission to access comments for this task.");
            }
            return;
        }

        if (currentUserRole == UserRole.User)
        {
            // User can only access if assigned or if member of the team
            if (task.AssignedToUserId == currentUserId)
            {
                return;
            }

            var isMember = await _context.TeamMembers
                .AnyAsync(tm => tm.TeamId == task.TeamId && tm.UserId == currentUserId);

            if (!isMember)
            {
                throw new ForbiddenException("You do not have permission to access comments for this task.");
            }
        }
    }
}
