using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.API.Models;

[Table("Tasks")]
public class TaskItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public int TeamId { get; set; }

    [ForeignKey("TeamId")]
    public Team Team { get; set; } = null!;

    public int? AssignedToUserId { get; set; }

    [ForeignKey("AssignedToUserId")]
    public User? AssignedToUser { get; set; }

    [Required]
    public int CreatedByUserId { get; set; }

    [ForeignKey("CreatedByUserId")]
    public User CreatedByUser { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = TaskPriorityConstants.Medium;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = TaskStatusConstants.ToDo;

    public DateTime? Deadline { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
