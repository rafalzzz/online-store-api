
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnlineStoreAPI.Variables;

namespace OnlineStoreAPI.Authorization
{
    public class AdminOnly : TypeFilterAttribute
    {
        public AdminOnly() : base(typeof(RequiresClaimFilter))
        {
            Arguments = new object[] { new Claim(ClaimTypes.Role, Roles.Admin) };
        }

        private class RequiresClaimFilter : IAuthorizationFilter
        {
            readonly Claim _claim;

            public RequiresClaimFilter(Claim claim)
            {
                _claim = claim;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                var hasClaim = context.HttpContext.User.Claims.Any(c => c.Type == _claim.Type && c.Value == _claim.Value);
                if (!hasClaim)
                {
                    context.Result = new ForbidResult();

                }
            }
        }
    }
}