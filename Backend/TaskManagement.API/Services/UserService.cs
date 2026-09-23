using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs;
using TaskManagement.API.Exceptions;
using TaskManagement.API.Interfaces;

namespace TaskManagement.API.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserResponseDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                TeamName = u.TeamMemberships.Select(tm => tm.Team.Name).FirstOrDefault()
            })
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<UserResponseDto> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                TeamName = u.TeamMemberships.Select(tm => tm.Team.Name).FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        return user;
    }
}
