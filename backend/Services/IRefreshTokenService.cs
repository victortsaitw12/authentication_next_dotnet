using System;
using backend.Models;

namespace backend.Services;

public interface IRefreshTokenService
{

    Task SaveRefreshTokenAsync(string userId, string refreshToken, DateTime expiryDate);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
}
