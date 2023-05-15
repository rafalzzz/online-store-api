
namespace OnlineStoreAPI.Configuration
{
    public class AuthorizationConfiguration
    {
        public AuthorizationConfiguration(IServiceCollection services)
        {
            services.AddAuthorization();
        }
    }
}