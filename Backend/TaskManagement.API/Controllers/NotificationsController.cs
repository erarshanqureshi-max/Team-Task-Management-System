using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.DTOs;
using TaskManagement.API.Helpers;
using TaskManagement.API.Interfaces;

namespace TaskManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<NotificationResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications()
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var notifications = await _notificationService.GetUserNotificationsAsync(currentUserId);
        return Ok(ApiResponse<List<NotificationResponseDto>>.Ok(notifications));
    }

    [HttpPatch("{id}/read")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var currentUserId = JwtHelper.GetUserId(User);
        await _notificationService.MarkAsReadAsync(id, currentUserId);
        return Ok(ApiResponse.Ok("Notification marked as read"));
    }

    [HttpPatch("read-all")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var currentUserId = JwtHelper.GetUserId(User);
        await _notificationService.MarkAllAsReadAsync(currentUserId);
        return Ok(ApiResponse.Ok("All notifications marked as read"));
    }
}
