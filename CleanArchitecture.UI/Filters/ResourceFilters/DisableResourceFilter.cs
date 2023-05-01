using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters
{
    public class DisableResourceFilter : IAsyncResourceFilter
    {
        private readonly ILogger<DisableResourceFilter> _logger;
        private readonly bool _isDisabled;

        public DisableResourceFilter(ILogger<DisableResourceFilter> logger, bool isDisabled = true)
        {
            _logger = logger;
            _isDisabled = isDisabled;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            _logger.LogInformation("{0}.{1} method started", nameof(DisableResourceFilter), nameof(OnResourceExecutionAsync));

            if (_isDisabled)
                context.Result = new StatusCodeResult(501);
            else
                await next();

            _logger.LogInformation("{0}.{1} method finished", nameof(DisableResourceFilter), nameof(OnResourceExecutionAsync));
        }
    }
}
