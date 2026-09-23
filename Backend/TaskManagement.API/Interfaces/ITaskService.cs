using TaskManagement.API.DTOs;

namespace TaskManagement.API.Interfaces;

public interface ITaskService
{
    Task<List<TaskResponseDto>> GetTasksAsync(TaskFilterDto filter, int currentUserId, string currentUserRole);
    Task<TaskResponseDto> GetTaskByIdAsync(int id, int currentUserId, string currentUserRole);
    Task<TaskResponseDto> CreateTaskAsync(TaskCreateDto dto, int currentUserId, string currentUserRole);
    Task<TaskResponseDto> UpdateTaskAsync(int id, TaskUpdateDto dto, int currentUserId, string currentUserRole);
    Task DeleteTaskAsync(int id, int currentUserId, string currentUserRole);
    Task<TaskResponseDto> UpdateStatusAsync(int id, TaskStatusUpdateDto dto, int currentUserId, string currentUserRole);
    Task<TaskResponseDto> AssignTaskAsync(int id, TaskAssignDto dto, int currentUserId, string currentUserRole);
}
