using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using TaskManagement.API.Helpers;
using TaskManagement.API.Models;
using Xunit;

namespace TaskManagement.Tests.Services;

public class AuthorizationTests
{
    private readonly IConfiguration _configuration;

    public AuthorizationTests()
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

    [Theory]
    [InlineData("Admin")]
    [InlineData("Manager")]
    [InlineData("User")]
    public void GenerateToken_ContainsRequiredUserIdEmailAndRoleClaims(string role)
    {
        // Arrange
        var user = new User
        {
            Id = 42,
            Name = "Alice In Chains",
            Email = "alice@example.com",
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var (tokenString, expiresAt) = JwtHelper.GenerateToken(user, _configuration);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(tokenString));
        Assert.True(expiresAt > DateTime.UtcNow);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenString);

        var nameIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid" || c.Type == "userId");
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email");
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role");

        Assert.NotNull(nameIdClaim);
        Assert.Equal("42", nameIdClaim.Value);

        Assert.NotNull(emailClaim);
        Assert.Equal("alice@example.com", emailClaim.Value);

        Assert.NotNull(roleClaim);
        Assert.Equal(role, roleClaim.Value);
    }

    [Fact]
    public void GetUserIdAndRole_FromClaimsPrincipal_ExtractsCorrectly()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "99"),
            new(ClaimTypes.Email, "manager@test.com"),
            new(ClaimTypes.Role, UserRole.Manager)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var userId = JwtHelper.GetUserId(principal);
        var role = JwtHelper.GetUserRole(principal);
        var email = JwtHelper.GetUserEmail(principal);

        // Assert
        Assert.Equal(99, userId);
        Assert.Equal(UserRole.Manager, role);
        Assert.Equal("manager@test.com", email);
    }
}
