
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
        string GenerateAccessToken(string userEmail);
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

        public string GenerateAccessToken(string userEmail)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.Email, userEmail),
            new Claim(ClaimTypes.Role, Roles.Admin),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var secretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.SecretKey);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(_jwtSettings.TokenLifeTime);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims,
                expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public CookieOptions GetCookieOptions()
        {
            double tokenLifeTime = TimeSpan.FromMinutes(Convert.ToDouble(_jwtSettings.TokenLifeTime)).TotalMinutes;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(tokenLifeTime)
            };

            return cookieOptions;
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
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
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            };

            return cookieOptions;
        }

        public string GenerateResetPasswordToken(string userEmail)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.Email, userEmail),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var resetPasswordSecretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.ResetPasswordSecretKey);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(resetPasswordSecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(_resetPasswordSettings.TokenLifeTime);

            var token = new JwtSecurityToken(
                issuer: _resetPasswordSettings.Issuer,
                audience: _resetPasswordSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public object ExtractEmailFromResetPasswordToken(string token)
        {
            try
            {
                var resetPasswordSecretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.ResetPasswordSecretKey);
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(resetPasswordSecretKey));

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