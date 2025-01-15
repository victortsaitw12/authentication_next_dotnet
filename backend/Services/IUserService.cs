using backend.Models;

namespace backend.Services;

public interface IUserService
{
    Task<bool> ValidateCredentialsAsync(string email, string password);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> RegisterUserAsync(RegisterRequest request);
    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task<bool> ResetPasswordAsync(string userId, string token, string newPassword);
} 