namespace OnlineStoreAPI.Configuration
{
    public class SessionConfiguration
    {
        public SessionConfiguration(IServiceCollection services, IConfigurationSection jwtSettings)
        {
            if (double.TryParse(jwtSettings["TokenLifeTime"], out double lifetime))
            {
                services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(lifetime * 3);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });
            }
            else
            {
                throw new ArgumentException("TokenLifeTime is not a valid double");
            }
        }
    }
}