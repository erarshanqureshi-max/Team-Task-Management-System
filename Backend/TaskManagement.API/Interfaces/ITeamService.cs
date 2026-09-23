using TaskManagement.API.DTOs;

namespace TaskManagement.API.Interfaces;

public interface ITeamService
{
    Task<List<TeamResponseDto>> GetAllTeamsAsync(int currentUserId, string currentUserRole);
    Task<TeamResponseDto> GetTeamByIdAsync(int teamId, int currentUserId, string currentUserRole);
    Task<TeamResponseDto> CreateTeamAsync(TeamCreateDto dto);
    Task<TeamResponseDto> UpdateTeamAsync(int teamId, TeamUpdateDto dto);
    Task DeleteTeamAsync(int teamId);
    Task AddMemberAsync(int teamId, int userId, int currentUserId, string currentUserRole);
    Task RemoveMemberAsync(int teamId, int userId, int currentUserId, string currentUserRole);
    Task<List<TeamMemberDto>> GetTeamMembersAsync(int teamId, int currentUserId, string currentUserRole);
}
