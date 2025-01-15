using backend.Models;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;
        
        // TODO: Implement proper password hashing
        return user.Password == password;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;

        return user;
    }

    public async Task<User> RegisterUserAsync(RegisterRequest request)
    {
        // Check if user already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new InvalidOperationException("Email already registered");
        }

        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            Password = request.Password, // TODO: Hash password
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Generate a random token
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        // Save token to database
        var resetToken = new PasswordResetToken
        {
            Token = token,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddHours(24), // Token expires in 24 hours
            IsUsed = false
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        // TODO: Send email with reset link
        Console.WriteLine($"Password reset link: http://localhost:3000/auth/password/reset-password-confirmation?token={token}&uid={user.Id}");
        return token;
    }

    public async Task<bool> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        var resetToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token && t.UserId == userId);

        if (resetToken == null || 
            resetToken.IsUsed || 
            resetToken.ExpiryDate < DateTime.UtcNow)
        {
            return false;
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        // Update password and mark token as used
        user.Password = newPassword; // TODO: Hash password
        resetToken.IsUsed = true;
        
        await _context.SaveChangesAsync();
        return true;
    }
} 