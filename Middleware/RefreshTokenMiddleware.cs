using OnlineStoreAPI.Services;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Middleware
{
    public class RefreshTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public RefreshTokenMiddleware(
            RequestDelegate next,
            IServiceProvider serviceProvider
         )
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        private void GenerateNewAccessToken(
            string refreshToken,
            HttpContext context
            )
        {
            using var scope = _serviceProvider.CreateScope();
            var accessTokenService = scope.ServiceProvider.GetRequiredService<IAccessTokenService>();
            var refreshTokenService = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var userId = refreshTokenService.GetUserIdFromRefreshToken(refreshToken);
            if (userId is null) return;

            var user = userService.GetUserById((int)userId);
            if (user is null) return;

            bool isRefreshTokenActive = userService.CheckUserRefreshToken(user, refreshToken);
            if (!isRefreshTokenActive) return;

            string userRole = userService.GetUserRoleDescription(user.Role);
            string newAccessToken = accessTokenService.GenerateAccessToken(userId.ToString(), userRole);

            CookieOptions accessTokenCookieOptions = accessTokenService.GetAccessTokenCookieOptions();
            context.Response.Cookies.Append(CookieNames.AccessToken, newAccessToken, accessTokenCookieOptions);

            var principals = accessTokenService.GetPrincipalsFromAccessToken(newAccessToken);
            context.User = principals;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var refreshToken = context.Request.Cookies[CookieNames.RefreshToken];
            var accessToken = context.Request.Cookies[CookieNames.AccessToken];
            string endpoint = context.Request.Path;

            bool isRefreshTokenDefined = !string.IsNullOrEmpty(refreshToken);
            bool isAccessTokenNotDefined = string.IsNullOrEmpty(accessToken);
            bool isNotLoginEndpoint = !endpoint.Contains("login");

            if (isRefreshTokenDefined && isAccessTokenNotDefined && isNotLoginEndpoint)
            {
                GenerateNewAccessToken(refreshToken, context);
            }

            await _next(context);
        }
    }
}