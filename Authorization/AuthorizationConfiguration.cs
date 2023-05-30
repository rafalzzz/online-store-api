
using System.Security.Claims;
using OnlineStoreAPI.Enums;
using OnlineStoreAPI.Helpers;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Authorization
{
    public class AuthorizationConfiguration
    {
        public AuthorizationConfiguration(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyNames.AdminOnly, policy => policy.RequireClaim(ClaimTypes.Role, UserRole.Admin.ToString()));
            });
        }
    }
}