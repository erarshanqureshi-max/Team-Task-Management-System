using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.DTOs;

public class TeamCreateDto
{
    [Required(ErrorMessage = "Team name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Team name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "ManagerId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Valid ManagerId is required")]
    public int ManagerId { get; set; }
}

public class TeamUpdateDto
{
    [Required(ErrorMessage = "Team name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Team name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "ManagerId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Valid ManagerId is required")]
    public int ManagerId { get; set; }
}

public class TeamMemberDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class AddTeamMemberDto
{
    [Required(ErrorMessage = "UserId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Valid UserId is required")]
    public int UserId { get; set; }
}

public class TeamResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public string ManagerEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public int TaskCount { get; set; }
    public List<TeamMemberDto> Members { get; set; } = new();
}
