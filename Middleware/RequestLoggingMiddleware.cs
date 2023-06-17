using System.Diagnostics;

namespace OnlineStoreAPI.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            await _next(context);
            sw.Stop();

            if (sw.Elapsed.TotalSeconds > 10 || context.Response.StatusCode == 401)
            {
                _logger.LogError($"Request {context.Request?.Method} {context.Request?.Path.Value} took longer than 10 seconds or returned a 401 error. Time elapsed: {sw.Elapsed.TotalSeconds} sec. Response status code: {context.Response.StatusCode}");
            }
        }
    }
}