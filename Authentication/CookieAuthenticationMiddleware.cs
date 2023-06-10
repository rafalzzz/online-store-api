using OnlineStoreAPI.Services;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Authentication
{
    public class CookieAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAccessTokenService _accessTokenService;

        public CookieAuthenticationMiddleware(RequestDelegate next, IAccessTokenService accessTokenService)
        {
            _next = next;
            _accessTokenService = accessTokenService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue(CookieNames.AccessToken, out var accessToken))
            {
                var principals = _accessTokenService.GetPrincipalsFromToken(accessToken);
                context.User = principals;
            }

            await _next(context);
        }
    }
}