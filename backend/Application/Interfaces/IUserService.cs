using Backend.Application.DTOs;

namespace Backend.Application.Interfaces;

public interface IUserService
{
    Task Register(UserRegisterDto userRegisterDTO);
    Task<string> Login(UserLoginDto userLoginDTO);
}