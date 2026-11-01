using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace LyricsScraperApi.Filters;

/// <summary>
/// Validates the action arguments with FluentValidation. Errors are returned as ValidationProblemDetails,
/// the same format as the model binding errors of ASP.NET Core (for example invalid JSON).
/// </summary>
public class GlobalRequestValidation(ProblemDetailsFactory problemDetailsFactory) : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;

            var argumentType = argument.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);

            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator) continue;

            var validationContext = new ValidationContext<object>(argument);
            var validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            if (validationResult.IsValid) continue;

            var modelState = new ModelStateDictionary();
            foreach (var failure in validationResult.Errors)
            {
                // Field names as in the JSON of the request.
                modelState.AddModelError(JsonNamingPolicy.CamelCase.ConvertName(failure.PropertyName), failure.ErrorMessage);
            }

            var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                context.HttpContext, modelState, StatusCodes.Status400BadRequest);
            context.Result = new BadRequestObjectResult(problemDetails);
            return;
        }

        await next();
    }
}
