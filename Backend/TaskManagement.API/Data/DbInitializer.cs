using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Models;

namespace TaskManagement.API.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }

        if (await context.Users.AnyAsync())
        {
            return;
        }

        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

        var adminUser = new User
        {
            Name = "Alex Miller",
            Email = "admin@example.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var managerUser = new User
        {
            Name = "Alice Johnson",
            Email = "manager@example.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.Manager,
            CreatedAt = DateTime.UtcNow.AddDays(-25)
        };

        var standardUser = new User
        {
            Name = "Bob Smith",
            Email = "user@example.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        var designerUser = new User
        {
            Name = "Charlie Brown",
            Email = "charlie@example.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        };

        context.Users.AddRange(adminUser, managerUser, standardUser, designerUser);
        await context.SaveChangesAsync();

        var engineeringTeam = new Team
        {
            Name = "Platform Engineering",
            Description = "Core Web API backend services, database migrations, and microservice integration.",
            ManagerId = managerUser.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        var productTeam = new Team
        {
            Name = "Frontend & Product",
            Description = "Client web application, design systems, and responsive user interfaces.",
            ManagerId = managerUser.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-18)
        };

        context.Teams.AddRange(engineeringTeam, productTeam);
        await context.SaveChangesAsync();

        var teamMembers = new List<TeamMember>
        {
            new() { TeamId = engineeringTeam.Id, UserId = managerUser.Id },
            new() { TeamId = engineeringTeam.Id, UserId = standardUser.Id },
            new() { TeamId = productTeam.Id, UserId = managerUser.Id },
            new() { TeamId = productTeam.Id, UserId = designerUser.Id }
        };

        context.TeamMembers.AddRange(teamMembers);
        await context.SaveChangesAsync();

        var task1 = new TaskItem
        {
            Title = "Implement Database Connection Resilience & Retry Policy",
            Description = "Configure EF Core execution strategy to handle transient network drops during peak transaction load.",
            TeamId = engineeringTeam.Id,
            AssignedToUserId = standardUser.Id,
            CreatedByUserId = managerUser.Id,
            Priority = TaskPriorityConstants.High,
            Status = TaskStatusConstants.Done,
            Deadline = DateTime.UtcNow.AddDays(-2),
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var task2 = new TaskItem
        {
            Title = "Refactor JWT Claim Validation & Expiration Middleware",
            Description = "Ensure TokenValidationParameters enforce ClockSkew.Zero and validate issuer and audience headers.",
            TeamId = engineeringTeam.Id,
            AssignedToUserId = standardUser.Id,
            CreatedByUserId = managerUser.Id,
            Priority = TaskPriorityConstants.Urgent,
            Status = TaskStatusConstants.InProgress,
            Deadline = DateTime.UtcNow.AddDays(2),
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var task3 = new TaskItem
        {
            Title = "Build Task Filtering & Search Pagination Controls",
            Description = "Create multi-select filters on frontend and translate to compound IQueryable WHERE clauses.",
            TeamId = engineeringTeam.Id,
            AssignedToUserId = standardUser.Id,
            CreatedByUserId = managerUser.Id,
            Priority = TaskPriorityConstants.Medium,
            Status = TaskStatusConstants.ToDo,
            Deadline = DateTime.UtcNow.AddDays(5),
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        var task4 = new TaskItem
        {
            Title = "Conduct Accessibility Audit & Mobile Breakpoint Testing",
            Description = "Verify WCAG 2.1 AA compliance for color contrasts, keyboard navigation, and responsive sidebar drawers.",
            TeamId = productTeam.Id,
            AssignedToUserId = designerUser.Id,
            CreatedByUserId = managerUser.Id,
            Priority = TaskPriorityConstants.Low,
            Status = TaskStatusConstants.InProgress,
            Deadline = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow.AddDays(-4)
        };

        var task5 = new TaskItem
        {
            Title = "Q3 Security Patching & NuGet Package Vulnerability Scan",
            Description = "Run dotnet list package --vulnerable and update any outdated dependencies across solution.",
            TeamId = engineeringTeam.Id,
            AssignedToUserId = standardUser.Id,
            CreatedByUserId = adminUser.Id,
            Priority = TaskPriorityConstants.High,
            Status = TaskStatusConstants.ToDo,
            Deadline = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-6)
        };

        context.Tasks.AddRange(task1, task2, task3, task4, task5);
        await context.SaveChangesAsync();

        var comments = new List<Comment>
        {
            new()
            {
                TaskId = task1.Id,
                UserId = managerUser.Id,
                Content = "Reviewed the PR. The execution strategy handles SQL Server timeout codes correctly.",
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new()
            {
                TaskId = task1.Id,
                UserId = standardUser.Id,
                Content = "Merged to main after verifying with integration tests.",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                TaskId = task2.Id,
                UserId = managerUser.Id,
                Content = "Remember to verify the 401 redirect behavior in the Axios interceptor.",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        context.Comments.AddRange(comments);

        var notifications = new List<Notification>
        {
            new()
            {
                UserId = standardUser.Id,
                TaskId = task2.Id,
                Message = "You have been assigned to task: 'Refactor JWT Claim Validation & Expiration Middleware'",
                Type = "TaskAssigned",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                UserId = standardUser.Id,
                TaskId = task1.Id,
                Message = "Task 'Implement Database Connection Resilience & Retry Policy' status changed to 'Done'",
                Type = "TaskStatusUpdated",
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                UserId = designerUser.Id,
                TaskId = task4.Id,
                Message = "You have been assigned to task: 'Conduct Accessibility Audit & Mobile Breakpoint Testing'",
                Type = "TaskAssigned",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            }
        };

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync();
    }
}
