using Backend.Application.DTOs;

namespace Backend.Application.Interfaces;

public interface IUserService
{
    Task Register(UserRegisterDto userRegisterDto);
    Task<string> Login(UserLoginDto userLoginDto);
    Task UpdateUser(Guid userId, UpdateUserDto updateUserDto);
    Task<UserInfoDto> GetUserInfo(Guid userId);
    Task SendPasswordResetCode(string email);
    Task ResetPassword(string email, string resetCode, string newPassword);
    Task DeleteAccount(Guid userId, string password);
}