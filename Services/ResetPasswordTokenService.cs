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
        object GetEmailFromResetPasswordToken(string token);
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

        private string GetResetPasswordSecretKey()
        {
            return Environment.GetEnvironmentVariable(EnvironmentVariables.ResetPasswordSecretKey);
        }

        private SecurityKey GetSigningCredentialsKey(string secretKey)
        {
            return _jwtService.GetSigningCredentials(secretKey).Key;
        }

        private TokenValidationParameters CreateTokenValidationParameters(SecurityKey key)
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _resetPasswordSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _resetPasswordSettings.Audience,
                ValidateLifetime = true
            };
        }

        private JwtSecurityToken ValidateJwtToken(string token, TokenValidationParameters tokenValidationParameters)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityException("Incorrect token");
            }

            return jwtSecurityToken;
        }

        private Claim GetEmailClaim(JwtSecurityToken jwtSecurityToken)
        {
            return jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        }

        private object LogErrorAndReturnTokenStatus(string errorMessage, VerifyResetPasswordToken status)
        {
            _logger.LogError(errorMessage);
            return status;
        }

        public object GetEmailFromResetPasswordToken(string token)
        {
            try
            {
                var resetPasswordSecretKey = GetResetPasswordSecretKey();
                var key = GetSigningCredentialsKey(resetPasswordSecretKey);

                var tokenValidationParameters = CreateTokenValidationParameters(key);
                var jwtSecurityToken = ValidateJwtToken(token, tokenValidationParameters);

                var emailClaim = GetEmailClaim(jwtSecurityToken);
                return emailClaim?.Value;
            }
            catch (SecurityTokenExpiredException)
            {
                string errorMessage = $"Token has expired. Time: {DateTime.Now}.";
                return LogErrorAndReturnTokenStatus(errorMessage, VerifyResetPasswordToken.TokenHasExpired);
            }
            catch (SecurityException exception)
            {
                string errorMessage = $"Reset password token validation error. Time: {DateTime.Now}. Error message: {exception.Message}";
                return LogErrorAndReturnTokenStatus(errorMessage, VerifyResetPasswordToken.TokenValidationError);
            }
            catch (Exception exception)
            {
                string errorMessage = $"Unexpected error during token validation. Time: {DateTime.Now}. Error message: {exception.Message}";
                return LogErrorAndReturnTokenStatus(errorMessage, VerifyResetPasswordToken.TokenValidationError);
            }
        }
    }
}