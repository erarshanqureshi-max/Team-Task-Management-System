using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.DTOs;
using TaskManagement.API.Helpers;
using TaskManagement.API.Interfaces;

namespace TaskManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AdminDashboardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminDashboard()
    {
        var dashboard = await _dashboardService.GetAdminDashboardAsync();
        return Ok(ApiResponse<AdminDashboardDto>.Ok(dashboard));
    }

    [HttpGet("manager")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<ManagerDashboardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetManagerDashboard()
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var dashboard = await _dashboardService.GetManagerDashboardAsync(currentUserId);
        return Ok(ApiResponse<ManagerDashboardDto>.Ok(dashboard));
    }

    [HttpGet("user")]
    [ProducesResponseType(typeof(ApiResponse<UserDashboardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserDashboard()
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var dashboard = await _dashboardService.GetUserDashboardAsync(currentUserId);
        return Ok(ApiResponse<UserDashboardDto>.Ok(dashboard));
    }
}
