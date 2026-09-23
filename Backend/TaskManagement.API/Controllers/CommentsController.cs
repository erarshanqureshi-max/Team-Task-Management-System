using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.DTOs;
using TaskManagement.API.Helpers;
using TaskManagement.API.Interfaces;

namespace TaskManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks/{taskId}/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<CommentResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComments(int taskId)
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var currentUserRole = JwtHelper.GetUserRole(User);

        var comments = await _commentService.GetTaskCommentsAsync(taskId, currentUserId, currentUserRole);
        return Ok(ApiResponse<List<CommentResponseDto>>.Ok(comments));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CommentResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment(int taskId, [FromBody] CommentCreateDto dto)
    {
        var currentUserId = JwtHelper.GetUserId(User);
        var currentUserRole = JwtHelper.GetUserRole(User);

        var comment = await _commentService.AddCommentAsync(taskId, dto, currentUserId, currentUserRole);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CommentResponseDto>.Ok(comment, "Comment added successfully"));
    }
}
