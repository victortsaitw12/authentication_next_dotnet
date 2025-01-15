using System.Security.Claims;
using backend.Models;

namespace backend.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    // bool ValidateAccessToken(string token);
    // bool ValidateRefreshToken(string token);

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

} 