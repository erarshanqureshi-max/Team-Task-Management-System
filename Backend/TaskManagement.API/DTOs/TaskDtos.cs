using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.DTOs;

public class TaskCreateDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "TeamId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Valid TeamId is required")]
    public int TeamId { get; set; }

    public int? AssignedToUserId { get; set; }

    [Required(ErrorMessage = "Priority is required")]
    [RegularExpression("^(Low|Medium|High|Urgent)$", ErrorMessage = "Priority must be Low, Medium, High, or Urgent")]
    public string Priority { get; set; } = "Medium";

    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("^(To Do|In Progress|Done)$", ErrorMessage = "Status must be 'To Do', 'In Progress', or 'Done'")]
    public string Status { get; set; } = "To Do";

    public DateTime? Deadline { get; set; }
}

public class TaskUpdateDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "TeamId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Valid TeamId is required")]
    public int TeamId { get; set; }

    public int? AssignedToUserId { get; set; }

    [Required(ErrorMessage = "Priority is required")]
    [RegularExpression("^(Low|Medium|High|Urgent)$", ErrorMessage = "Priority must be Low, Medium, High, or Urgent")]
    public string Priority { get; set; } = "Medium";

    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("^(To Do|In Progress|Done)$", ErrorMessage = "Status must be 'To Do', 'In Progress', or 'Done'")]
    public string Status { get; set; } = "To Do";

    public DateTime? Deadline { get; set; }
}

public class TaskStatusUpdateDto
{
    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("^(To Do|In Progress|Done)$", ErrorMessage = "Status must be 'To Do', 'In Progress', or 'Done'")]
    public string Status { get; set; } = string.Empty;
}

public class TaskAssignDto
{
    public int? AssignedToUserId { get; set; }
}

public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public string? AssignedToUserEmail { get; set; }
    public int CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CommentsCount { get; set; }
}

public class TaskFilterDto
{
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateTime? Deadline { get; set; }
    public string? Search { get; set; }
}
