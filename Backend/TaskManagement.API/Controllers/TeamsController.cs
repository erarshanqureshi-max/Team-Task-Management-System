using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.DTOs;
using TaskManagement.API.Helpers;
using TaskManagement.API.Interfaces;

namespace TaskManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TeamResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var currentUserRole = JwtHelper.GetUserRole(User);

        var teams = await _teamService.GetAllTeamsAsync(currentUserId, currentUserRole);
        return Ok(ApiResponse<List<TeamResponseDto>>.Ok(teams));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TeamResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var currentUserRole = JwtHelper.GetUserRole(User);

        var team = await _teamService.GetTeamByIdAsync(id, currentUserId, currentUserRole);
        return Ok(ApiResponse<TeamResponseDto>.Ok(team));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<TeamResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] TeamCreateDto dto)
    {
        var team = await _teamService.CreateTeamAsync(dto);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TeamResponseDto>.Ok(team, "Team created successfully"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<TeamResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] TeamUpdateDto dto)
    {
        var team = await _teamService.UpdateTeamAsync(id, dto);
        return Ok(ApiResponse<TeamResponseDto>.Ok(team, "Team updated successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _teamService.DeleteTeamAsync(id);
        return Ok(ApiResponse.Ok("Team deleted successfully"));
    }

    [HttpGet("{id}/members")]
    [ProducesResponseType(typeof(ApiResponse<List<TeamMemberDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMembers(int id)
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var currentUserRole = JwtHelper.GetUserRole(User);

        var members = await _teamService.GetTeamMembersAsync(id, currentUserId, currentUserRole);
        return Ok(ApiResponse<List<TeamMemberDto>>.Ok(members));
    }

    [HttpPost("{id}/members")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMember(int id, [FromBody] AddTeamMemberDto dto)
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var currentUserRole = JwtHelper.GetUserRole(User);

        await _teamService.AddMemberAsync(id, dto.UserId, currentUserId, currentUserRole);
        return Ok(ApiResponse.Ok("Member added to team successfully"));
    }

    [HttpDelete("{id}/members/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(int id, int userId)
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var currentUserRole = JwtHelper.GetUserRole(User);

        await _teamService.RemoveMemberAsync(id, userId, currentUserId, currentUserRole);
        return Ok(ApiResponse.Ok("Member removed from team successfully"));
    }
}
