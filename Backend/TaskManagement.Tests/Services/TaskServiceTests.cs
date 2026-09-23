using Moq;
using TaskManagement.API.DTOs;
using TaskManagement.API.Exceptions;
using TaskManagement.API.Interfaces;
using TaskManagement.API.Models;
using TaskManagement.API.Services;
using TaskManagement.Tests.Helpers;
using Xunit;

namespace TaskManagement.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<INotificationService> _mockNotificationService;

    public TaskServiceTests()
    {
        _mockNotificationService = new Mock<INotificationService>();
    }

    private static (User manager, User member, User outsider, Team team) SeedSampleTeamData(API.Data.ApplicationDbContext context)
    {
        var manager = new User
        {
            Id = 1,
            Name = "Team Manager",
            Email = "manager@example.com",
            PasswordHash = "hash",
            Role = UserRole.Manager
        };

        var member = new User
        {
            Id = 2,
            Name = "Team Member",
            Email = "member@example.com",
            PasswordHash = "hash",
            Role = UserRole.User
        };

        var outsider = new User
        {
            Id = 3,
            Name = "Outside User",
            Email = "outsider@example.com",
            PasswordHash = "hash",
            Role = UserRole.User
        };

        context.Users.AddRange(manager, member, outsider);

        var team = new Team
        {
            Id = 10,
            Name = "Alpha Team",
            ManagerId = manager.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.Teams.Add(team);

        context.TeamMembers.AddRange(
            new TeamMember { TeamId = team.Id, UserId = manager.Id },
            new TeamMember { TeamId = team.Id, UserId = member.Id }
        );

        context.SaveChanges();
        return (manager, member, outsider, team);
    }

    [Fact]
    public async Task CreateTaskAsync_AsManagerForOwnTeam_Succeeds()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext(nameof(CreateTaskAsync_AsManagerForOwnTeam_Succeeds));
        var (manager, member, _, team) = SeedSampleTeamData(context);
        var taskService = new TaskService(context, _mockNotificationService.Object);

        var dto = new TaskCreateDto
        {
            Title = "Implement Feature X",
            Description = "Feature details",
            TeamId = team.Id,
            AssignedToUserId = member.Id,
            Priority = TaskPriorityConstants.High,
            Status = TaskStatusConstants.ToDo,
            Deadline = DateTime.UtcNow.AddDays(3)
        };

        // Act
        var result = await taskService.CreateTaskAsync(dto, manager.Id, UserRole.Manager);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Implement Feature X", result.Title);
        Assert.Equal(team.Id, result.TeamId);
        Assert.Equal(member.Id, result.AssignedToUserId);

        _mockNotificationService.Verify(n => n.CreateNotificationAsync(
            member.Id,
            It.IsAny<int>(),
            It.Is<string>(s => s.Contains("Implement Feature X")),
            "TaskAssigned"), Times.Once);
    }

    [Fact]
    public async Task CreateTaskAsync_AsManagerAssigningOutsideTeam_ThrowsBadRequestException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext(nameof(CreateTaskAsync_AsManagerAssigningOutsideTeam_ThrowsBadRequestException));
        var (manager, _, outsider, team) = SeedSampleTeamData(context);
        var taskService = new TaskService(context, _mockNotificationService.Object);

        var dto = new TaskCreateDto
        {
            Title = "Task For Outsider",
            TeamId = team.Id,
            AssignedToUserId = outsider.Id, // NOT a member of team
            Priority = TaskPriorityConstants.Medium,
            Status = TaskStatusConstants.ToDo
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
            taskService.CreateTaskAsync(dto, manager.Id, UserRole.Manager));
        Assert.Contains("outside your team", ex.Message);
    }

    [Fact]
    public async Task CreateTaskAsync_AsUser_ThrowsForbiddenException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext(nameof(CreateTaskAsync_AsUser_ThrowsForbiddenException));
        var (_, member, _, team) = SeedSampleTeamData(context);
        var taskService = new TaskService(context, _mockNotificationService.Object);

        var dto = new TaskCreateDto
        {
            Title = "User Attempting Task Creation",
            TeamId = team.Id,
            Priority = TaskPriorityConstants.Low,
            Status = TaskStatusConstants.ToDo
        };

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            taskService.CreateTaskAsync(dto, member.Id, UserRole.User));
    }

    [Fact]
    public async Task UpdateStatusAsync_AsAssignedUser_SucceedsAndDispatchesNotification()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext(nameof(UpdateStatusAsync_AsAssignedUser_SucceedsAndDispatchesNotification));
        var (manager, member, _, team) = SeedSampleTeamData(context);

        var task = new TaskItem
        {
            Id = 100,
            Title = "Fix Bug #404",
            TeamId = team.Id,
            AssignedToUserId = member.Id,
            CreatedByUserId = manager.Id,
            Priority = TaskPriorityConstants.High,
            Status = TaskStatusConstants.ToDo,
            CreatedAt = DateTime.UtcNow
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var taskService = new TaskService(context, _mockNotificationService.Object);
        var statusDto = new TaskStatusUpdateDto { Status = TaskStatusConstants.InProgress };

        // Act
        var result = await taskService.UpdateStatusAsync(task.Id, statusDto, member.Id, UserRole.User);

        // Assert
        Assert.Equal(TaskStatusConstants.InProgress, result.Status);

        // Notification should be sent to creator (manager) since member updated it
        _mockNotificationService.Verify(n => n.CreateNotificationAsync(
            manager.Id,
            task.Id,
            It.Is<string>(s => s.Contains("InProgress") || s.Contains("In Progress")),
            "TaskStatusUpdated"), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_AsUnassignedUser_ThrowsForbiddenException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext(nameof(UpdateStatusAsync_AsUnassignedUser_ThrowsForbiddenException));
        var (manager, member, outsider, team) = SeedSampleTeamData(context);

        var task = new TaskItem
        {
            Id = 200,
            Title = "Private User Task",
            TeamId = team.Id,
            AssignedToUserId = member.Id, // assigned to member
            CreatedByUserId = manager.Id,
            Priority = TaskPriorityConstants.Medium,
            Status = TaskStatusConstants.ToDo,
            CreatedAt = DateTime.UtcNow
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var taskService = new TaskService(context, _mockNotificationService.Object);
        var statusDto = new TaskStatusUpdateDto { Status = TaskStatusConstants.Done };

        // Act & Assert: outsider trying to modify member's task by ID
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            taskService.UpdateStatusAsync(task.Id, statusDto, outsider.Id, UserRole.User));
    }

    [Fact]
    public async Task GetTaskByIdAsync_AsUnassignedUser_ThrowsForbiddenException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext(nameof(GetTaskByIdAsync_AsUnassignedUser_ThrowsForbiddenException));
        var (manager, member, outsider, team) = SeedSampleTeamData(context);

        var task = new TaskItem
        {
            Id = 300,
            Title = "Confidential Assigned Task",
            TeamId = team.Id,
            AssignedToUserId = member.Id,
            CreatedByUserId = manager.Id,
            Priority = TaskPriorityConstants.Urgent,
            Status = TaskStatusConstants.ToDo,
            CreatedAt = DateTime.UtcNow
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var taskService = new TaskService(context, _mockNotificationService.Object);

        // Act & Assert: outsider accessing member's task ID
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            taskService.GetTaskByIdAsync(task.Id, outsider.Id, UserRole.User));
    }

    [Fact]
    public async Task AssignTaskAsync_AsManagerToTeamMember_Succeeds()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext(nameof(AssignTaskAsync_AsManagerToTeamMember_Succeeds));
        var (manager, member, _, team) = SeedSampleTeamData(context);

        var task = new TaskItem
        {
            Id = 400,
            Title = "Unassigned Task",
            TeamId = team.Id,
            CreatedByUserId = manager.Id,
            Priority = TaskPriorityConstants.Low,
            Status = TaskStatusConstants.ToDo,
            CreatedAt = DateTime.UtcNow
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var taskService = new TaskService(context, _mockNotificationService.Object);
        var assignDto = new TaskAssignDto { AssignedToUserId = member.Id };

        // Act
        var result = await taskService.AssignTaskAsync(task.Id, assignDto, manager.Id, UserRole.Manager);

        // Assert
        Assert.Equal(member.Id, result.AssignedToUserId);
        _mockNotificationService.Verify(n => n.CreateNotificationAsync(
            member.Id,
            task.Id,
            It.Is<string>(s => s.Contains("assigned")),
            "TaskAssigned"), Times.Once);
    }

    [Fact]
    public async Task AssignTaskAsync_AsManagerToOutsider_ThrowsBadRequestException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext(nameof(AssignTaskAsync_AsManagerToOutsider_ThrowsBadRequestException));
        var (manager, _, outsider, team) = SeedSampleTeamData(context);

        var task = new TaskItem
        {
            Id = 500,
            Title = "Team Task",
            TeamId = team.Id,
            CreatedByUserId = manager.Id,
            Priority = TaskPriorityConstants.Low,
            Status = TaskStatusConstants.ToDo,
            CreatedAt = DateTime.UtcNow
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var taskService = new TaskService(context, _mockNotificationService.Object);
        var assignDto = new TaskAssignDto { AssignedToUserId = outsider.Id };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            taskService.AssignTaskAsync(task.Id, assignDto, manager.Id, UserRole.Manager));
    }
}
