using System.Security.Claims;
using Backend.Application.DTOs;
using Backend.Application.Interfaces;
using Backend.Application.Services;
using Backend.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly LoginAttemptService _loginAttemptService;

    public AuthController(IUserService userService, LoginAttemptService loginAttemptService)
    {
        _userService = userService;
        _loginAttemptService = loginAttemptService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto dto)
    {
        await _userService.Register(dto);
        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto dto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        if (ipAddress != null && _loginAttemptService.IsBlocked(ipAddress))
        {
            return BadRequest(new { message = "Too many failed login attempts. Please try again later." });
        }

        try
        {
            var token = await _userService.Login(dto);
            return Ok(new { token });
        }
        catch (ValidationException)
        {
            if (ipAddress != null)
            {
                _loginAttemptService.RegisterFailedAttempt(ipAddress);
            }

            return BadRequest(new { message = "Invalid email or password" });
        }
    }

    [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateUser(UpdateUserDto updateUserDto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var userId = Guid.Parse(userIdClaim);
            await _userService.UpdateUser(userId, updateUserDto);
            return Ok(new { message = "User updated successfully" });
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
    [HttpGet("info")]
    public async Task<IActionResult> GetUserInfo()
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var userId = Guid.Parse(userIdClaim);
            var userInfo = await _userService.GetUserInfo(userId);
            return Ok(userInfo);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("password-reset-request")]
    public async Task<IActionResult> RequestPasswordReset(PasswordResetRequestDto dto)
    {
        try
        {
            await _userService.SendPasswordResetCode(dto.Email);
            return Ok(new { message = "Password reset code sent to your email" });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("password-reset")]
    public async Task<IActionResult> ResetPassword(PasswordResetDto dto)
    {
        try
        {
            await _userService.ResetPassword(dto.Email, dto.ResetCode, dto.NewPassword);
            return Ok(new { message = "Password reset successfully" });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount(DeleteAccountDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID claim is missing from the token");
            }

            var userId = Guid.Parse(userIdClaim);

            await _userService.DeleteAccount(userId, dto.Password);

            return Ok(new { message = "Account deleted successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
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
}