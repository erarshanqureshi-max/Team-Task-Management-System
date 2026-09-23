using TaskManagement.API.DTOs;

namespace TaskManagement.API.Interfaces;

public interface IDashboardService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync();
    Task<ManagerDashboardDto> GetManagerDashboardAsync(int managerUserId);
    Task<UserDashboardDto> GetUserDashboardAsync(int userId);
}
