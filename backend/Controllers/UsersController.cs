using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.DTOs;
using backend.Models;
using backend.Services;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenService _jwtTokenService;

        public UsersController(IUserService userService,IJwtTokenService jwtTokenService ){
            _userService = userService;
            _jwtTokenService = jwtTokenService;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserResponse>> GetCurrentUser()
        {
        
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(token);
            var expirationTime = principal.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            if (expirationTime != null)
            {
                var expirationDateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expirationTime)).UtcDateTime;
                var remainingTime = expirationDateTime - DateTime.UtcNow;
                Console.WriteLine($"Token expires in {remainingTime.TotalMinutes:F2} minutes");
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return BadRequest(new { message = "NameIdentifier claim not found in token" });
            }

            User? user = await _userService.GetUser(userId);
            if (user == null)
            {
                return NotFound();
            }

            UserResponse response = new(){
                CreatedAt = user.CreatedAt,
                Email = user.Email,
                Id = user.Id,
                LastLogin = user.LastLogin
            };

            return Ok(response);
        }
    }
}
