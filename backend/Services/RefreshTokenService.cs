using System;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class RefreshTokenService: IRefreshTokenService
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveRefreshTokenAsync(string userId, string refreshToken, DateTime expiryDate)
    {
        var token = new RefreshToken
        {
            Token = refreshToken,
            UserId = userId,
            ExpiryDate = expiryDate,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
    }


    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token && !r.IsRevoked);
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token);

        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            await _context.SaveChangesAsync();
        }
    }

}
