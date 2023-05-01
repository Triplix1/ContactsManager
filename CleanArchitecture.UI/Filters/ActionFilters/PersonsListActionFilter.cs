using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceContracts.DTO;

namespace CRUDExample.Filters.ActionFilters
{
    public class PersonsListActionFilter : IActionFilter
    {
        public readonly ILogger<PersonsListActionFilter> _logger;

        public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("PersonsListActionFilter.OnActionExecuted method");

            var controller = (PersonsController)context.Controller;

            IDictionary<string, object?>? parameters = (IDictionary<string, object?>?)context.HttpContext.Items["parameters"];

            if (parameters != null)
            {
                if (parameters.ContainsKey("seachBy"))
                {
                    controller.ViewData["CurrentSearchBy"] = Convert.ToString(parameters["searchBy"]);
                }

                if (parameters.ContainsKey("searchString"))
                {
                    controller.ViewData["CurrentSearchString"] = Convert.ToString(parameters["searchString"]);
                }

                if (parameters.ContainsKey("sortBy"))
                {
                    controller.ViewData["CurrentSortBy"] = Convert.ToString(parameters["sortBy"]);
                }

                if (parameters.ContainsKey("sortOrder"))
                {
                    controller.ViewData["CurrentSortOrder"] = Convert.ToString(parameters["sortOrder"]);
                }
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation("PersonsListActionFilter.OnActionExecuting method");

            context.HttpContext.Items["parameters"] = context.ActionArguments;

            if (context.ActionArguments.ContainsKey("searchBy"))
            {
                var value = (string)context.ActionArguments["searchBy"];

                var searchByOptions = new List<string>() {
                    nameof(PersonResponse.PersonName),
                    nameof(PersonResponse.Email),
                    nameof(PersonResponse.DateOfBirth),
                    nameof(PersonResponse.Gender),
                    nameof(PersonResponse.CountryID),
                    nameof(PersonResponse.Address)
                };

                if (!searchByOptions.Any(x => value == x))
                {
                    _logger.LogInformation($"Unexepted value inPersonsListActionFilter.OnActionExecuting - {value}");

                    context.ActionArguments["searchBy"] = nameof(PersonResponse.PersonName);
                }
                else
                {
                    _logger.LogInformation($"Exepted value inPersonsListActionFilter.OnActionExecuting - {value}");
                }
            }
        }
    }
}
