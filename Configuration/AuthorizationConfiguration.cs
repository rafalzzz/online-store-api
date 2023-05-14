
namespace OnlineStoreAPI.Configuration
{
    public class AuthorizationConfiguration
    {
        public AuthorizationConfiguration(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireClaim("admin", "true"));
            });
        }
    }
}