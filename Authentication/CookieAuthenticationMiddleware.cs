
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Authentication
{
    public class CookieAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        private ClaimsPrincipal GetPrincipalsFromToken(string token)
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
            catch (Exception)
            {
                // If token is invalid or expired actions
            }

            return null;
        }

        public CookieAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue(CookieNames.AccessToken, out var accessToken))
            {
                var principals = GetPrincipalsFromToken(accessToken);
                context.User = principals;

            }

            await _next(context);
        }
    }
}