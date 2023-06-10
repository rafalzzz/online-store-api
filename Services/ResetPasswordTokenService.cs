
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OnlineStoreAPI.Enums;
using OnlineStoreAPI.Middleware;
using OnlineStoreAPI.Models;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Services
{
    public interface IResetPasswordTokenService
    {
        Task SendResetPasswordToken(string email);
        object ExtractEmailFromResetPasswordToken(string token);
    }

    public class ResetPasswordTokenService : IResetPasswordTokenService
    {
        private readonly IJwtService _jwtService;
        private readonly ResetPasswordSettings _resetPasswordSettings;
        private readonly IEmailService _emailService;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public ResetPasswordTokenService(
            IJwtService jwtService,
            IOptions<ResetPasswordSettings> resetPasswordSettings,
             IEmailService emailService,
            ILogger<RequestLoggingMiddleware> logger
            )
        {
            _jwtService = jwtService;
            _resetPasswordSettings = resetPasswordSettings.Value;
            _emailService = emailService;
            _logger = logger;
        }

        private List<Claim> GetResetPasswordTokenClaims(string userEmail)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, userEmail),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            return claims;
        }

        public string GenerateResetPasswordToken(string userEmail)
        {
            List<Claim> claims = GetResetPasswordTokenClaims(userEmail);
            string resetPasswordSecretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.ResetPasswordSecretKey);
            DateTime expires = DateTime.Now.AddMinutes(_resetPasswordSettings.TokenLifeTime);

            return _jwtService.GenerateToken(claims, _resetPasswordSettings.Issuer, _resetPasswordSettings.Audience, resetPasswordSecretKey, expires);
        }

        public async Task SendResetPasswordToken(string email)
        {
            string token = GenerateResetPasswordToken(email);

            string emailTitle = "Confirm your email";

            string clientUrl = Environment.GetEnvironmentVariable(EnvironmentVariables.ClientUrl);
            string tokenLink = $"{clientUrl}/{token}";
            string emailMessage = $"Click on the link to confirm your email: {tokenLink}";

            await _emailService.SendEmailAsync(email, emailTitle, emailMessage);
        }

        public object ExtractEmailFromResetPasswordToken(string token)
        {
            try
            {
                var resetPasswordSecretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.ResetPasswordSecretKey);
                var key = _jwtService.GetSigningCredentials(resetPasswordSecretKey).Key;

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