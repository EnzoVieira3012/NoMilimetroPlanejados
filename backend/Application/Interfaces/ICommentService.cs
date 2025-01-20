using Backend.Application.DTOs;

namespace Backend.Application.Interfaces;

public interface ICommentService
{
    Task AddComment(Guid customerId, CreateCommentDto commentDto);
    Task UpdateComment(Guid commentId, UpdateCommentDto commentDto);
    Task DeleteComment(Guid commentId);
}