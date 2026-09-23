using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.API.Models;

[Table("TeamMembers")]
public class TeamMember
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TeamId { get; set; }

    [ForeignKey("TeamId")]
    public Team Team { get; set; } = null!;

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
