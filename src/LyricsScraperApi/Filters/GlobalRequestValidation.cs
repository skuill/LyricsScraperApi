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

            var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator
                ?? GetBaseValidator(argumentType, context.HttpContext.RequestServices);
            if (validator == null) continue;

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

    private IValidator? GetBaseValidator(Type argumentType, IServiceProvider services)
    {
        var baseType = argumentType.BaseType;
        while (baseType != null)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(baseType);
            var validator = services.GetService(validatorType) as IValidator;
            if (validator != null)
                return validator;

            baseType = baseType.BaseType;
        }
        return null;
    }
}
