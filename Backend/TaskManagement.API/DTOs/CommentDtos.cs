using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.DTOs;

public class CommentCreateDto
{
    [Required(ErrorMessage = "Comment content cannot be empty")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Comment content cannot exceed 2000 characters")]
    public string Content { get; set; } = string.Empty;
}

public class CommentResponseDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
