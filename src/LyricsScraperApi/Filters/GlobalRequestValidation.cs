using FluentValidation;
using LyricsScraperApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LyricsScraperApi.Filters;

public class GlobalRequestValidation(IFormatValidation formatValidation) : ActionFilterAttribute
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
            var validationResult = await validator.ValidateAsync(validationContext);

            if (validationResult.IsValid) continue;
            context.Result = new BadRequestObjectResult(formatValidation.FormatValidationErrors(validationResult));
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