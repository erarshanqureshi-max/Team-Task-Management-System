using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.DTOs;
using TaskManagement.API.Helpers;
using TaskManagement.API.Interfaces;
using TaskManagement.API.Models;

namespace TaskManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TaskResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks([FromQuery] TaskFilterDto filter)
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var currentUserRole = JwtHelper.GetUserRole(User);

        var tasks = await _taskService.GetTasksAsync(filter, currentUserId, currentUserRole);
        return Ok(ApiResponse<List<TaskResponseDto>>.Ok(tasks));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(int id)
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var currentUserRole = JwtHelper.GetUserRole(User);

        var task = await _taskService.GetTaskByIdAsync(id, currentUserId, currentUserRole);
        return Ok(ApiResponse<TaskResponseDto>.Ok(task));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTask([FromBody] TaskCreateDto dto)
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var currentUserRole = JwtHelper.GetUserRole(User);

        var task = await _taskService.CreateTaskAsync(dto, currentUserId, currentUserRole);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TaskResponseDto>.Ok(task, "Task created successfully"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskUpdateDto dto)
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var currentUserRole = JwtHelper.GetUserRole(User);

        var task = await _taskService.UpdateTaskAsync(id, dto, currentUserId, currentUserRole);
        return Ok(ApiResponse<TaskResponseDto>.Ok(task, "Task updated successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var currentUserRole = JwtHelper.GetUserRole(User);

        await _taskService.DeleteTaskAsync(id, currentUserId, currentUserRole);
        return Ok(ApiResponse.Ok("Task deleted successfully"));
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] TaskStatusUpdateDto dto)
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var currentUserRole = JwtHelper.GetUserRole(User);

        var task = await _taskService.UpdateStatusAsync(id, dto, currentUserId, currentUserRole);
        return Ok(ApiResponse<TaskResponseDto>.Ok(task, "Task status updated successfully"));
    }

    [HttpPatch("{id}/assign")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTask(int id, [FromBody] TaskAssignDto dto)
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var currentUserRole = JwtHelper.GetUserRole(User);

        var task = await _taskService.AssignTaskAsync(id, dto, currentUserId, currentUserRole);
        return Ok(ApiResponse<TaskResponseDto>.Ok(task, "Task assignment updated successfully"));
    }
}
