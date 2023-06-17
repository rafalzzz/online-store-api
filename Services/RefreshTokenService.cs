using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using OnlineStoreAPI.Middleware;
using OnlineStoreAPI.Models;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Services
{
    public interface IRefreshTokenService
    {
        string GenerateRefreshToken(string userEmail);
        int? GetUserIdFromRefreshToken(string refreshToken);
        CookieOptions GetRefreshTokenCookieOptions();
    }

    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly string _errorMessage = "Refresh token validation error";
        private readonly JwtSettings _jwtSettings;
        private readonly IJwtService _jwtService;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RefreshTokenService(
            IOptions<JwtSettings> jwtSettings,
            IJwtService jwtService,
            ILogger<RequestLoggingMiddleware> logger
            )
        {
            _jwtSettings = jwtSettings.Value;
            _jwtService = jwtService;
            _logger = logger;
        }

        private List<Claim> GetRefreshTokenClaims(string userId)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            return claims;
        }

        public string GenerateRefreshToken(string userId)
        {
            List<Claim> claims = GetRefreshTokenClaims(userId);
            string secretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.RefreshTokenSecretKey);
            DateTime expires = DateTime.Now.AddDays(_jwtSettings.RefreshTokenLifeTime);

            return _jwtService.GenerateToken(claims, _jwtSettings.Issuer, _jwtSettings.Audience, secretKey, expires);
        }

        private ClaimsPrincipal GetPrincipalsFromRefreshToken(string token)
        {
            var secretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.RefreshTokenSecretKey);
            return _jwtService.GetPrincipalsFromToken(token, secretKey, _errorMessage);
        }

        public int? GetUserIdFromRefreshToken(string refreshToken)
        {
            var principals = GetPrincipalsFromRefreshToken(refreshToken);
            Claim userIdClaim = principals.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            int userId;
            bool isUserId = int.TryParse(userIdClaim.Value, out userId);

            if (!isUserId)
            {
                return null;
            }

            return userId;
        }

        public CookieOptions GetRefreshTokenCookieOptions()
        {
            DateTimeOffset expires = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenLifeTime);
            return _jwtService.CreateCookieOptions(expires);
        }
    }
}