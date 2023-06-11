using OnlineStoreAPI.Services;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Session
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAccessTokenService _accessTokenService;

        public SessionMiddleware(RequestDelegate next, IAccessTokenService accessTokenService)
        {
            _next = next;
            _accessTokenService = accessTokenService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Session.IsAvailable)
            {
                return;
            }

            if (!context.Request.Cookies.TryGetValue(CookieNames.AccessToken, out var accessToken))
            {
                string userId = context.Session.GetString("UserId");
                string userRole = context.Session.GetString("UserRole");

                if (userId is null || userRole is null)
                {
                    return;
                }

                // Wygeneruj nowy token za pomocą usługi AccessTokenService
                var newToken = _accessTokenService.GenerateAccessToken(userId, userRole);

                // Utwórz ciasteczko z nowym tokenem
                context.Response.Cookies.Append(CookieNames.AccessToken, newToken);
            }

            await _next(context);
        }
    }
}