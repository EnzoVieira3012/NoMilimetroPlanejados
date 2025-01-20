using Backend.Domain.Entities;

namespace Backend.Domain.Interfaces;

public interface ICommentRepository
{
    Task Add(Comment comment);
    Task<Comment?> GetById(Guid id);
    Task Update(Comment comment);
    Task Delete(Comment comment);
}