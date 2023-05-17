
using System.Security.Claims;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Authorization
{
    public class AuthorizationConfiguration
    {
        public AuthorizationConfiguration(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyNames.AdminOnly, policy => policy.RequireClaim(ClaimTypes.Role, Roles.Admin));
            });
        }
    }
}