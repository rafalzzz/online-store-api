using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Middleware
{
    public class CookieAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public CookieAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue(CookieNames.AccessToken, out var accessToken))
            {
                context.Request.Headers.Append(Headers.Authorization, $"Bearer {accessToken}");
            }

            await _next(context);
        }
    }
}