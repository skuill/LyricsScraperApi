using FluentValidation;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json;

namespace LyricsScraperApi.OpenApi;

/// <summary>
/// Marks a property as required in the OpenAPI document when its FluentValidation validator has a NotNull or NotEmpty
/// rule for it, so the validators stay the only place of these rules.
/// </summary>
public sealed class ValidatorRequiredSchemaTransformer : IOpenApiSchemaTransformer
{
    private static readonly HashSet<string> RequiredRules = ["NotNullValidator", "NotEmptyValidator"];

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(context.JsonTypeInfo.Type);
        // Validators are scoped services.
        using var scope = context.ApplicationServices.CreateScope();
        if (scope.ServiceProvider.GetService(validatorType) is not IValidator validator)
        {
            return Task.CompletedTask;
        }

        foreach (var member in validator.CreateDescriptor().GetMembersWithValidators())
        {
            if (member.Any(rule => RequiredRules.Contains(rule.Validator.Name)))
            {
                schema.Required ??= new HashSet<string>();
                schema.Required.Add(JsonNamingPolicy.CamelCase.ConvertName(member.Key));
            }
        }

        return Task.CompletedTask;
    }
}
