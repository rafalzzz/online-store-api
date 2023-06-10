
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OnlineStoreAPI.Middleware;

namespace OnlineStoreAPI.Services
{
    public interface IJwtService
    {
        SigningCredentials GetSigningCredentials(string secretKey);
        string GenerateToken(List<Claim> claims, string issuer, string audience, string secretKey, DateTime expires);
    }

    public class JwtService : IJwtService
    {
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public JwtService(ILogger<RequestLoggingMiddleware> logger)
        {
            _logger = logger;
        }

        public SigningCredentials GetSigningCredentials(string secretKey)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public string GenerateToken(List<Claim> claims, string issuer, string audience, string secretKey, DateTime expires)
        {
            var creds = GetSigningCredentials(secretKey);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}