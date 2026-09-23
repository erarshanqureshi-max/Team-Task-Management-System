namespace TaskManagement.API.DTOs;

public class DashboardStatsDto
{
    public int TotalTasks { get; set; }
    public int ToDoTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int DoneTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int HighPriorityTasks { get; set; }
}

public class AdminDashboardDto
{
    public DashboardStatsDto Stats { get; set; } = new();
    public int TotalUsers { get; set; }
    public int TotalTeams { get; set; }
    public List<TaskResponseDto> RecentTasks { get; set; } = new();
}

public class ManagerDashboardDto
{
    public DashboardStatsDto Stats { get; set; } = new();
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int TeamMemberCount { get; set; }
    public List<TaskResponseDto> RecentTasks { get; set; } = new();
}

public class UserDashboardDto
{
    public DashboardStatsDto Stats { get; set; } = new();
    public List<TaskResponseDto> RecentTasks { get; set; } = new();
}
