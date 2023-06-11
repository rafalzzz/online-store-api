using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OnlineStoreAPI.Middleware;
using OnlineStoreAPI.Models;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Services
{
    public interface IAccessTokenService
    {
        string GenerateAccessToken(string userEmail, string userRole);
        CookieOptions GetAccessTokenCookieOptions();
        CookieOptions RemoveAccessTokenCookieOptions();
        ClaimsPrincipal GetPrincipalsFromToken(string token);
    }

    public class AccessTokenService : IAccessTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IJwtService _jwtService;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public AccessTokenService(
            IOptions<JwtSettings> jwtSettings,
            IJwtService jwtService,
            ILogger<RequestLoggingMiddleware> logger
            )
        {
            _jwtSettings = jwtSettings.Value;
            _jwtService = jwtService;
            _logger = logger;
        }

        private List<Claim> GetAccessTokenClaims(string userId, string userRole)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, userRole),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            return claims;
        }

        public string GenerateAccessToken(string userId, string userRole)
        {
            List<Claim> claims = GetAccessTokenClaims(userId, userRole);
            string secretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.SecretKey);
            DateTime expires = DateTime.Now.AddMinutes(_jwtSettings.TokenLifeTime);

            return _jwtService.GenerateToken(claims, _jwtSettings.Issuer, _jwtSettings.Audience, secretKey, expires);
        }

        public ClaimsPrincipal GetPrincipalsFromToken(string token)
        {
            try
            {
                var secretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.SecretKey);
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _jwtService.GetSigningCredentials(secretKey).Key,
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                return principal;
            }
            catch (Exception exception)
            {
                var errorMessage = $"Access token validation error. Time: {DateTime.Now}. Error message: {exception.Message}";
                _logger.LogError(errorMessage);
            }

            return null;
        }

        private CookieOptions CreateCookieOptions(double tokenLifeTime, bool remove = false)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = remove ? DateTimeOffset.UtcNow.AddDays(-1) : DateTimeOffset.UtcNow.AddMinutes(tokenLifeTime)
            };

            return cookieOptions;
        }

        public CookieOptions GetAccessTokenCookieOptions()
        {
            return CreateCookieOptions(_jwtSettings.TokenLifeTime);
        }

        public CookieOptions RemoveAccessTokenCookieOptions()
        {
            return CreateCookieOptions(_jwtSettings.TokenLifeTime, true);
        }
    }
}