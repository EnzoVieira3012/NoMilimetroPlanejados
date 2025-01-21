using Backend.Domain.Entities;

namespace Backend.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetById(Guid id);
    Task<User?> GetByEmail(string email);
    Task Add(User user);
    Task Update(User user);
    Task<User?> GetByPasswordResetCode(string resetCode);
    Task Delete(User user);
}