using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs;
using TaskManagement.API.Exceptions;
using TaskManagement.API.Interfaces;
using TaskManagement.API.Models;

namespace TaskManagement.API.Services;

public class TeamService : ITeamService
{
    private readonly ApplicationDbContext _context;

    public TeamService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TeamResponseDto>> GetAllTeamsAsync(int currentUserId, string currentUserRole)
    {
        IQueryable<Team> query = _context.Teams
            .Include(t => t.Manager)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.Tasks);

        if (currentUserRole == UserRole.Manager)
        {
            query = query.Where(t => t.ManagerId == currentUserId || t.Members.Any(m => m.UserId == currentUserId));
        }
        else if (currentUserRole == UserRole.User)
        {
            query = query.Where(t => t.Members.Any(m => m.UserId == currentUserId));
        }

        var teams = await query.AsNoTracking().ToListAsync();

        return teams.Select(MapToTeamResponseDto).ToList();
    }

    public async Task<TeamResponseDto> GetTeamByIdAsync(int teamId, int currentUserId, string currentUserRole)
    {
        var team = await _context.Teams
            .Include(t => t.Manager)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.Tasks)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
        {
            throw new NotFoundException("Team not found.");
        }

        if (currentUserRole == UserRole.Manager && team.ManagerId != currentUserId && !team.Members.Any(m => m.UserId == currentUserId))
        {
            throw new ForbiddenException("You do not have permission to view this team.");
        }

        if (currentUserRole == UserRole.User && !team.Members.Any(m => m.UserId == currentUserId))
        {
            throw new ForbiddenException("You do not have permission to view this team.");
        }

        return MapToTeamResponseDto(team);
    }

    public async Task<TeamResponseDto> CreateTeamAsync(TeamCreateDto dto)
    {
        var manager = await _context.Users.FindAsync(dto.ManagerId);
        if (manager == null)
        {
            throw new BadRequestException("Assigned manager does not exist.");
        }

        var team = new Team
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            ManagerId = dto.ManagerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        // Also add the manager as a team member if not already
        var existingMembership = await _context.TeamMembers
            .AnyAsync(tm => tm.TeamId == team.Id && tm.UserId == dto.ManagerId);

        if (!existingMembership)
        {
            _context.TeamMembers.Add(new TeamMember
            {
                TeamId = team.Id,
                UserId = dto.ManagerId
            });
            await _context.SaveChangesAsync();
        }

        return await GetTeamByIdAsync(team.Id, dto.ManagerId, UserRole.Admin);
    }

    public async Task<TeamResponseDto> UpdateTeamAsync(int teamId, TeamUpdateDto dto)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
        {
            throw new NotFoundException("Team not found.");
        }

        var manager = await _context.Users.FindAsync(dto.ManagerId);
        if (manager == null)
        {
            throw new BadRequestException("Assigned manager does not exist.");
        }

        team.Name = dto.Name.Trim();
        team.Description = dto.Description?.Trim();
        team.ManagerId = dto.ManagerId;

        await _context.SaveChangesAsync();

        return await GetTeamByIdAsync(teamId, dto.ManagerId, UserRole.Admin);
    }

    public async Task DeleteTeamAsync(int teamId)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
        {
            throw new NotFoundException("Team not found.");
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
    }

    public async Task AddMemberAsync(int teamId, int userId, int currentUserId, string currentUserRole)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
        {
            throw new NotFoundException("Team not found.");
        }

        if (currentUserRole == UserRole.Manager && team.ManagerId != currentUserId)
        {
            throw new ForbiddenException("You can only manage members of your own team.");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        var alreadyMember = await _context.TeamMembers
            .AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

        if (alreadyMember)
        {
            throw new BadRequestException("User is already a member of this team.");
        }

        _context.TeamMembers.Add(new TeamMember
        {
            TeamId = teamId,
            UserId = userId
        });

        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(int teamId, int userId, int currentUserId, string currentUserRole)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
        {
            throw new NotFoundException("Team not found.");
        }

        if (currentUserRole == UserRole.Manager && team.ManagerId != currentUserId)
        {
            throw new ForbiddenException("You can only manage members of your own team.");
        }

        var membership = await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

        if (membership == null)
        {
            throw new NotFoundException("User is not a member of this team.");
        }

        _context.TeamMembers.Remove(membership);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TeamMemberDto>> GetTeamMembersAsync(int teamId, int currentUserId, string currentUserRole)
    {
        var team = await _context.Teams
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
        {
            throw new NotFoundException("Team not found.");
        }

        if (currentUserRole == UserRole.Manager && team.ManagerId != currentUserId && !team.Members.Any(m => m.UserId == currentUserId))
        {
            throw new ForbiddenException("You do not have access to this team.");
        }

        if (currentUserRole == UserRole.User && !team.Members.Any(m => m.UserId == currentUserId))
        {
            throw new ForbiddenException("You do not have access to this team.");
        }

        return team.Members.Select(m => new TeamMemberDto
        {
            UserId = m.UserId,
            Name = m.User.Name,
            Email = m.User.Email,
            Role = m.User.Role
        }).ToList();
    }

    private static TeamResponseDto MapToTeamResponseDto(Team team)
    {
        return new TeamResponseDto
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            ManagerId = team.ManagerId,
            ManagerName = team.Manager?.Name ?? string.Empty,
            ManagerEmail = team.Manager?.Email ?? string.Empty,
            CreatedAt = team.CreatedAt,
            MemberCount = team.Members?.Count ?? 0,
            TaskCount = team.Tasks?.Count ?? 0,
            Members = team.Members?.Select(m => new TeamMemberDto
            {
                UserId = m.UserId,
                Name = m.User?.Name ?? string.Empty,
                Email = m.User?.Email ?? string.Empty,
                Role = m.User?.Role ?? string.Empty
            }).ToList() ?? new List<TeamMemberDto>()
        };
    }
}
