using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace LyricsScraperApi.OpenApi;

/// <summary>
/// Enums are sent as names (JsonStringEnumConverter), but the generator writes only the list of values:
/// without <c>"type": "string"</c> Swagger UI shows the type as "any".
/// </summary>
public sealed class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type.IsEnum)
        {
            schema.Type = JsonSchemaType.String;
        }
        return Task.CompletedTask;
    }
}
