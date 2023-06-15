using OnlineStoreAPI.Services;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Middleware
{
    public class RefreshTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAccessTokenService _accessTokenService;

        public RefreshTokenMiddleware(RequestDelegate next, IAccessTokenService accessTokenService)
        {
            _next = next;
            _accessTokenService = accessTokenService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            bool accessTokenIsNotAvailable = !context.Request.Cookies.TryGetValue(CookieNames.AccessToken, out var accessToken);

            if (accessTokenIsNotAvailable)
            {
                string userId = context.Session.GetString("UserId");
                string userRole = context.Session.GetString("UserRole");

                bool isUserIdDefined = int.TryParse(userId, out _);
                bool isUserRoleDefined = !string.IsNullOrEmpty(userRole);

                if (isUserIdDefined && isUserRoleDefined)
                {
                    var newToken = _accessTokenService.GenerateAccessToken(userId, userRole);
                    context.Response.Cookies.Append(CookieNames.AccessToken, newToken);
                }
            }

            await _next(context);
        }
    }
}