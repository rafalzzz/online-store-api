namespace OnlineStoreAPI.Middleware
{
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtCookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue("access_token", out var accessToken))
            {
                // Tutaj możemy przeprowadzić dodatkową walidację JWT, np. sprawdzić jego wygaśnięcie lub sygnaturę.
                // Jeśli token jest nieprawidłowy, możemy zakończyć żądanie, wysyłając odpowiedni kod błędu.
                // W przypadku prawidłowego tokena po prostu przekazujemy żądanie dalej.

                context.Request.Headers.Append("Authorization", $"Bearer {accessToken}");
            }

            await _next(context);
        }
    }
}