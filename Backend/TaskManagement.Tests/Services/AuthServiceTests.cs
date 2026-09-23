using Microsoft.Extensions.Configuration;
using TaskManagement.API.DTOs;
using TaskManagement.API.Exceptions;
using TaskManagement.API.Models;
using TaskManagement.API.Services;
using TaskManagement.Tests.Helpers;
using Xunit;

namespace TaskManagement.Tests.Services;

public class AuthServiceTests
{
    private readonly IConfiguration _configuration;

    public AuthServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:Key", "TeamTaskManagementSuperSecretKey_2026_Minimum32CharsLength!"},
            {"Jwt:Issuer", "TaskManagementAPI"},
            {"Jwt:Audience", "TaskManagementClient"},
            {"Jwt:ExpirationHours", "24"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_CreatesUserAndReturnsToken()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext(nameof(RegisterAsync_WithValidData_CreatesUserAndReturnsToken));
        var authService = new AuthService(context, _configuration);

        var request = new RegisterRequestDto
        {
            Name = "John Doe",
            Email = "john@example.com",
            Password = "Password123!",
            Role = UserRole.User
        };

        // Act
        var result = await authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal(UserRole.User, result.Role);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.True(result.ExpiresAt > DateTime.UtcNow);

        // Verify stored in DB
        var userInDb = context.Users.FirstOrDefault(u => u.Email == "john@example.com");
        Assert.NotNull(userInDb);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123!", userInDb.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsBadRequestException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext(nameof(RegisterAsync_WithExistingEmail_ThrowsBadRequestException));
        context.Users.Add(new User
        {
            Name = "Existing",
            Email = "existing@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = UserRole.User
        });
        await context.SaveChangesAsync();

        var authService = new AuthService(context, _configuration);

        var request = new RegisterRequestDto
        {
            Name = "Duplicate User",
            Email = "existing@example.com",
            Password = "Password123!",
            Role = UserRole.User
        };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => authService.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponseWithJwt()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext(nameof(LoginAsync_WithValidCredentials_ReturnsAuthResponseWithJwt));
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!");
        context.Users.Add(new User
        {
            Name = "Manager Jane",
            Email = "jane@example.com",
            PasswordHash = passwordHash,
            Role = UserRole.Manager
        });
        await context.SaveChangesAsync();

        var authService = new AuthService(context, _configuration);

        var loginDto = new LoginRequestDto
        {
            Email = "jane@example.com",
            Password = "CorrectPassword123!"
        };

        // Act
        var result = await authService.LoginAsync(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("jane@example.com", result.Email);
        Assert.Equal(UserRole.Manager, result.Role);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsBadRequestException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext(nameof(LoginAsync_WithInvalidPassword_ThrowsBadRequestException));
        context.Users.Add(new User
        {
            Name = "User Bob",
            Email = "bob@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!"),
            Role = UserRole.User
        });
        await context.SaveChangesAsync();

        var authService = new AuthService(context, _configuration);

        var loginDto = new LoginRequestDto
        {
            Email = "bob@example.com",
            Password = "WrongPassword!"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => authService.LoginAsync(loginDto));
        Assert.Equal("Invalid email or password.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ThrowsBadRequestException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext(nameof(LoginAsync_WithNonExistentEmail_ThrowsBadRequestException));
        var authService = new AuthService(context, _configuration);

        var loginDto = new LoginRequestDto
        {
            Email = "unknown@example.com",
            Password = "Password123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => authService.LoginAsync(loginDto));
    }
}
