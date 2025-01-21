using Backend.Application.DTOs;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Domain.Exceptions;
using Backend.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Security.Claims;
using System.Text;

namespace Backend.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly string _jwtSecretKey;

    public UserService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _jwtSecretKey = configuration["JwtSettings:SecretKey"] 
            ?? throw new ArgumentNullException(nameof(configuration), "JWT Secret Key is not configured.");
    }

    public async Task<string> Login(UserLoginDto userLoginDto)
    {
        var user = await _userRepository.GetByEmail(userLoginDto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
        {
            throw new ValidationException("Invalid email or password");
        }

        return GenerateJwtToken(user);
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task Register(UserRegisterDto userRegisterDto)
    {
        var existingUser = await _userRepository.GetByEmail(userRegisterDto.Email);
        if (existingUser != null)
        {
            throw new ValidationException("User already exists with this email");
        }

        var user = new User
        {
            Name = userRegisterDto.Name,
            Email = userRegisterDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password)
        };

        await _userRepository.Add(user);
    }

    public async Task UpdateUser(Guid userId, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.Name))
        {
            user.Name = updateUserDto.Name;
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
        {
            var existingUser = await _userRepository.GetByEmail(updateUserDto.Email);
            if (existingUser != null && existingUser.Id != userId)
            {
                throw new ValidationException("Email is already in use by another user");
            }
            user.Email = updateUserDto.Email;
        }

        await _userRepository.Update(user);
    }

    public async Task<UserInfoDto> GetUserInfo(Guid userId)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        return new UserInfoDto
        {
            Name = user.Name,
            Email = user.Email
        };
    }

    public async Task SendPasswordResetCode(string email)
    {
        var user = await _userRepository.GetByEmail(email);
        if (user == null)
        {
            throw new ValidationException("User with this email does not exist");
        }

        var resetCode = Guid.NewGuid().ToString().Substring(0, 6);
        user.PasswordResetCode = resetCode;
        user.PasswordResetCodeExpiresAt = DateTime.UtcNow.AddMinutes(15);

        await _userRepository.Update(user);

        var client = new SendGridClient(_configuration["SendGrid:ApiKey"]);
        var from = new EmailAddress(_configuration["SendGrid:SenderEmail"], _configuration["SendGrid:SenderName"]);
        var to = new EmailAddress(user.Email, user.Name);
        var replyTo = new EmailAddress(_configuration["SendGrid:ReplyToEmail"]);
        var subject = "Redefinição de Senha - Sistema de Teste";
        var plainTextContent = $"Seu código de redefinição de senha é: {resetCode}";
        var htmlContent = $"<strong>Seu código de redefinição de senha é: {resetCode}</strong>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        msg.ReplyTo = replyTo;

        var response = await client.SendEmailAsync(msg);

        if (!response.IsSuccessStatusCode)
        {
            throw new EmailSendingFailedException("Failed to send password reset email");
        }
    }

    public async Task ResetPassword(string email, string resetCode, string newPassword)
    {
        var user = await _userRepository.GetByEmail(email);
        if (user == null || user.PasswordResetCode != resetCode || user.PasswordResetCodeExpiresAt < DateTime.UtcNow)
        {
            throw new ValidationException("Invalid or expired reset code");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiresAt = null;

        await _userRepository.Update(user);
    }

    public async Task DeleteAccount(Guid userId, string password)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            throw new ValidationException("Invalid password");
        }

        await _userRepository.Delete(user);
    }
}