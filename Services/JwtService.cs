
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OnlineStoreAPI.Models;

namespace OnlineStoreAPI.Services
{
    public interface IJwtService
    {
        CookieOptions GenerateCookieWithToken(string userEmail);
    }

    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        private string GenerateToken(string userEmail)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.Email, userEmail),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
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

        public CookieOptions GenerateCookieWithToken(string userEmail)
        {
            string token = GenerateToken(userEmail);
            var tokenLifeTime = TimeSpan.FromMinutes(Convert.ToDouble(_jwtSettings.TokenLifeTime));

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to `false`, when using HTTP instead of HTTPS
                SameSite = SameSiteMode.None, // Important for cookies between different domains
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)
            };

            return cookieOptions;
        }
    }
}