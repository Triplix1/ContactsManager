using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ExceptionFilters
{
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }
        public void OnException(ExceptionContext context)
        {
            _logger.LogError("{0}.{1} method \nError:{2} {3}", nameof(ExceptionFilter), nameof(OnException), context.Exception.ToString(), context.Exception.Message);

            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
