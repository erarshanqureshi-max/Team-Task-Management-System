using TaskManagement.API.DTOs;

namespace TaskManagement.API.Interfaces;

public interface INotificationService
{
    Task<List<NotificationResponseDto>> GetUserNotificationsAsync(int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
    Task CreateNotificationAsync(int userId, int? taskId, string message, string type);
}
