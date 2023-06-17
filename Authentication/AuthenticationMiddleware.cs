using OnlineStoreAPI.Services;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Authentication
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAccessTokenService _accessTokenService;

        public AuthenticationMiddleware(RequestDelegate next, IAccessTokenService accessTokenService)
        {
            _next = next;
            _accessTokenService = accessTokenService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var accessToken = context.Request.Cookies[CookieNames.AccessToken];
            bool isAccessTokenDefined = !string.IsNullOrEmpty(accessToken);

            if (isAccessTokenDefined)
            {
                var principals = _accessTokenService.GetPrincipalsFromAccessToken(accessToken);
                context.User = principals;
            }

            await _next(context);
        }
    }
}