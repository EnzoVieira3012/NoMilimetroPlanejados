using Backend.Application.DTOs;
using Backend.Application.Interfaces;
using Backend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [Authorize]
    [HttpPost("{customerId}")]
    public async Task<IActionResult> AddComment(Guid customerId, CreateCommentDto commentDto)
    {
        try
        {
            await _commentService.AddComment(customerId, commentDto);
            return Ok(new { message = "Comment added successfully" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPut("{commentId}")]
    public async Task<IActionResult> UpdateComment(Guid commentId, UpdateCommentDto commentDto)
    {
        try
        {
            await _commentService.UpdateComment(commentId, commentDto);
            return Ok(new { message = "Comment updated successfully" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        try
        {
            await _commentService.DeleteComment(commentId);
            return Ok(new { message = "Comment deleted successfully" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}