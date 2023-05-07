
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OnlineStoreAPI.Models;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Services
{
    public interface IJwtService
    {
        string GenerateToken(string userEmail);
        CookieOptions GetCookieOptions();
        string ExtractUserEmailFromToken(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateToken(string userEmail)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.Email, userEmail),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
            var secretKey = Environment.GetEnvironmentVariable(EnvironmentVariables.SecretKey);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(_jwtSettings.TokenLifeTime);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
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

        public string ExtractUserEmailFromToken(string token)
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
                var userEmailClaim = principal.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email);

                if (userEmailClaim != null)
                {
                    return userEmailClaim.Value;
                }
            }
            catch (Exception)
            {
                // If token is invalid or expired actions
            }

            return null;
        }
    }
}