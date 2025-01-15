using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace backend.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JWTSettings _jwtSettings;
    

    public JwtTokenService(IOptions<JWTSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        Console.WriteLine(_jwtSettings.SecretKey);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_jwtSettings.AccessTokenExpirationMinutes)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

   public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
   {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        
        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        JwtSecurityToken jwtSecurityToken = (JwtSecurityToken)validatedToken;
        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }
        return principal;
   }

    // public bool ValidateAccessToken(string token)
    // {
    //     return ValidateToken(token, "access");
    // }

    // public bool ValidateRefreshToken(string token)
    // {
    //     return ValidateToken(token, "refresh");
    // }

    // private bool ValidateToken(string token, string expectedTokenType)
    // {
    //     var tokenHandler = new JwtSecurityTokenHandler();
    //     var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

    //     try
    //     {
    //         tokenHandler.ValidateToken(token, new TokenValidationParameters
    //         {
    //             ValidateIssuerSigningKey = true,
    //             IssuerSigningKey = new SymmetricSecurityKey(key),
    //             ValidateIssuer = true,
    //             ValidIssuer = _jwtSettings.Issuer,
    //             ValidateAudience = true,
    //             ValidAudience = _jwtSettings.Audience,
    //             ValidateLifetime = true,
    //             ClockSkew = TimeSpan.Zero
    //         }, out SecurityToken validatedToken);

    //         var jwtToken = (JwtSecurityToken)validatedToken;
    //         var tokenType = jwtToken.Claims.First(x => x.Type == "token_type").Value;

    //         return tokenType == expectedTokenType;
    //     }
    //     catch
    //     {
    //         return false;
    //     }
    // }
} 