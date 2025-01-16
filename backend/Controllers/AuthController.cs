using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using backend.Services;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUserService _userService;

    public AuthController(
        IJwtTokenService jwtTokenService, 
        IRefreshTokenService refreshTokenService,
        IUserService userService)
    {
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _userService = userService;
    }

    // TODO: Implement user registration
    [HttpPost("users")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var user = await _userService.RegisterUserAsync(request);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("jwt/create")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Validate credentials
        if (!await _userService.ValidateCredentialsAsync(request.Email, request.Password))
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        // Get user details
        var user = await _userService.GetUserByEmailAsync(request.Email);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
        };

        var accessToken = _jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        await _refreshTokenService.SaveRefreshTokenAsync(
            user.Id,
            refreshToken,
            DateTime.UtcNow.AddDays(7)
        );

        return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout([FromBody] RefreshTokenRequest request)
    {
        _refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken).Wait();
        
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("jwt/refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        Console.WriteLine("RefreshToken");
        // Get refresh token from database
        var storedToken = _refreshTokenService.GetRefreshTokenAsync(request.RefreshToken).Result;
        if (storedToken == null || storedToken.ExpiryDate < DateTime.UtcNow)
        {
            return Ok(new { message = "Invalid or expired refresh token" });
        }

        // Generate new tokens
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, storedToken.UserId),
        };
        var newAccessToken = _jwtTokenService.GenerateAccessToken(claims);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        // Revoke old refresh token and save new one
        await _refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        await _refreshTokenService.SaveRefreshTokenAsync(
            storedToken.UserId,
            newRefreshToken,
            DateTime.UtcNow.AddDays(7)
        );
        return Ok(new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }

    [HttpPost("users/reset_password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var token = await _userService.GeneratePasswordResetTokenAsync(request.Email);
            // In production, send token via email
            return Ok(new { 
                message = "Password reset email sent",
                // Only for testing purposes, remove in production
                token = token 
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("users/reset_password_confirm")]
    public async Task<IActionResult> ResetPasswordConfirm([FromBody] ResetPasswordConfirmRequest request)
    {
        if (request.NewPassword != request.ReNewPassword)
        {
            return BadRequest(new { message = "Passwords do not match" });
        }

        var result = await _userService.ResetPasswordAsync(
            request.Uid,
            request.Token,
            request.NewPassword
        );

        if (!result)
        {
            return BadRequest(new { message = "Invalid reset token or user ID" });
        }

        return Ok(new { message = "Password reset successfully" });
    }

    [Authorize]
    [HttpGet("test")]
    public IActionResult Test()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        return Ok(new { 
            message = "Authentication successful",
            userId,
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
} 