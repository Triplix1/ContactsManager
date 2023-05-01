using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace CRUDExample.Filters.ResultFilters
{
    public class AuthorizationTokenResultFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            context.HttpContext.Response.Cookies.Append("Auth-Key", "Vlad");

            await next();
        }
    }
}
