
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OnlineStoreAPI.Enums;
using OnlineStoreAPI.Middleware;
using OnlineStoreAPI.Models;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(string userEmail, string userRole);
        CookieOptions GetCookieOptions();
        CookieOptions RemoveAccessTokenCookieOptions();
        ClaimsPrincipal GetPrincipalsFromToken(string token);
        string GenerateResetPasswordToken(string userEmail);
        object ExtractEmailFromResetPasswordToken(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ResetPasswordSettings _resetPasswordSettings;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public JwtService(
            IOptions<JwtSettings> jwtSettings,
            IOptions<ResetPasswordSettings> resetPasswordSettings,
            ILogger<RequestLoggingMiddleware> logger
            )
        {
            _jwtSettings = jwtSettings.Value;
            _resetPasswordSettings = resetPasswordSettings.Value;
            _logger = logger;
        }

        private SigningCredentials GetSigningCredentials(string secretKey)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
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

        private string GenerateToken(string userEmail, string issuer, string audience, string secretKey, double tokenLifeTime, string userRole = "")
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, userEmail),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            if (userRole != "")
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var creds = GetSigningCredentials(secretKey);
            var expires = DateTime.Now.AddMinutes(tokenLifeTime);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateAccessToken(string userEmail, string userRole)
        {
            var secretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.SecretKey);
            return GenerateToken(userEmail, _jwtSettings.Issuer, _jwtSettings.Audience, secretKey, _jwtSettings.TokenLifeTime, userRole);
        }

        public CookieOptions GetCookieOptions()
        {
            return CreateCookieOptions(_jwtSettings.TokenLifeTime);
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
                    IssuerSigningKey = GetSigningCredentials(secretKey).Key,
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

        public CookieOptions RemoveAccessTokenCookieOptions()
        {
            return CreateCookieOptions(_jwtSettings.TokenLifeTime, true);
        }

        public string GenerateResetPasswordToken(string userEmail)
        {
            var resetPasswordSecretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.ResetPasswordSecretKey);
            return GenerateToken(userEmail, _resetPasswordSettings.Issuer, _resetPasswordSettings.Audience, resetPasswordSecretKey, _resetPasswordSettings.TokenLifeTime);
        }
        public object ExtractEmailFromResetPasswordToken(string token)
        {
            try
            {
                var resetPasswordSecretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.ResetPasswordSecretKey);
                var key = GetSigningCredentials(resetPasswordSecretKey).Key;

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _resetPasswordSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _resetPasswordSettings.Audience,
                    ValidateLifetime = true
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;

                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityException("Incorrect token");
                }

                var emailClaim = principal.FindFirst(ClaimTypes.Email);
                return emailClaim?.Value;
            }
            catch (SecurityTokenExpiredException)
            {
                var errorMessage = $"Token has expired. Time: {DateTime.Now}.";
                _logger.LogError(errorMessage);
                return VerifyResetPasswordToken.TokenHasExpired;
            }
            catch (SecurityException exception)
            {
                var errorMessage = $"Reset password token validation error. Time: {DateTime.Now}. Error message: {exception.Message}";
                _logger.LogError(errorMessage);
                return VerifyResetPasswordToken.TokenValidationError;
            }
            catch (Exception exception)
            {
                var errorMessage = $"Unexpected error during token validation. Time: {DateTime.Now}. Error message: {exception.Message}";
                _logger.LogError(errorMessage);
                return VerifyResetPasswordToken.TokenValidationError;
            }
        }
    }
}