using OnlineStoreAPI.Services;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Authentication
{
    public class CookieAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IJwtService _jwtService;

        public CookieAuthenticationMiddleware(RequestDelegate next, IJwtService jwtService)
        {
            _next = next;
            _jwtService = jwtService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue(CookieNames.AccessToken, out var accessToken))
            {
                var principals = _jwtService.GetPrincipalsFromToken(accessToken);
                context.User = principals;
            }

            await _next(context);
        }
    }
}