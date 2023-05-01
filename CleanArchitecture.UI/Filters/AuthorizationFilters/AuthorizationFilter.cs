using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CRUDExample.Filters.SkipFilter;

namespace CRUDExample.Filters.AuthorizationFilters
{
    public class AuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.Filters.OfType<SkipFilterType>().Any())
            {
                return;
            }
            if (!context.HttpContext.Request.Cookies.ContainsKey("Auth-Key") || context.HttpContext.Request.Cookies["Auth-Key"] != "Vlad")
            {
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }
        }
    }
}
