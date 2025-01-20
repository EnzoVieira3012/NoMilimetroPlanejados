using System.Security.Claims;
using Backend.Application.DTOs;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Domain.Interfaces;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Backend.Application.Services
{
    // Custom exceptions
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string message) : base(message) { }
    }

    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException(string message) : base(message) { }
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task Register(UserRegisterDto userRegisterDTO)
        {
            var existingUser = await _userRepository.GetByEmail(userRegisterDTO.Email);
            if (existingUser != null)
                throw new UserAlreadyExistsException("User already exists with this email");

            var user = new User
            {
                Name = userRegisterDTO.Name,
                Email = userRegisterDTO.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDTO.Password)
            };

            await _userRepository.Add(user);
        }

        public async Task<string> Login(UserLoginDto userLoginDTO)
        {
            var user = await _userRepository.GetByEmail(userLoginDTO.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDTO.Password, user.PasswordHash))
                throw new InvalidCredentialsException("Invalid email or password");

            // Generate JWT Token
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("yA1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6="); // Chave segura com pelo menos 32 caracteres
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}