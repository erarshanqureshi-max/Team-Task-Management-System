using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.API.Models;

namespace TaskManagement.API.Helpers;

public static class JwtHelper
{
    public static (string token, DateTime expiresAt) GenerateToken(User user, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"] ?? "TeamTaskManagementSuperSecretKey_2026_Minimum32CharsLength!";
        var issuer = jwtSettings["Issuer"] ?? "TaskManagementAPI";
        var audience = jwtSettings["Audience"] ?? "TaskManagementClient";
        var expirationHours = int.TryParse(jwtSettings["ExpirationHours"], out var hours) ? hours : 24;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddHours(expirationHours);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("userId", user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role),
            new("role", user.Role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiresAt);
    }

    public static int GetUserId(ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("userId");
        if (claim == null || !int.TryParse(claim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user identity in token.");
        }
        return userId;
    }

    public static string GetUserRole(ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.Role) ?? principal.FindFirst("role");
        return claim?.Value ?? string.Empty;
    }

    public static string GetUserEmail(ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.Email);
        return claim?.Value ?? string.Empty;
    }
}
