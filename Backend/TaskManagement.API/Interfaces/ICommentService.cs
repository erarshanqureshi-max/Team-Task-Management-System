using TaskManagement.API.DTOs;

namespace TaskManagement.API.Interfaces;

public interface ICommentService
{
    Task<List<CommentResponseDto>> GetTaskCommentsAsync(int taskId, int currentUserId, string currentUserRole);
    Task<CommentResponseDto> AddCommentAsync(int taskId, CommentCreateDto dto, int currentUserId, string currentUserRole);
}
