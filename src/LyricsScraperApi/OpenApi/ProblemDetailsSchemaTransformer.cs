using LyricsScraperApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace LyricsScraperApi.OpenApi;

/// <summary>
/// Describes the fields the API adds to every ProblemDetails: <c>traceId</c>, <c>errorCode</c> (errors of the search)
/// and the shape of <c>errors</c> (validation errors). The generator knows only the properties of the classes.
/// </summary>
public sealed class ProblemDetailsSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        if (!typeof(ProblemDetails).IsAssignableFrom(type))
        {
            return Task.CompletedTask;
        }

        schema.Properties ??= new Dictionary<string, IOpenApiSchema>();
        // The generator writes "integer or string with a number pattern" (numbers may be read from strings),
        // but the API always sends a number.
        schema.Properties["status"] = new OpenApiSchema
        {
            Type = JsonSchemaType.Integer,
            Format = "int32",
            Description = "HTTP status code.",
        };
        schema.Properties["traceId"] = new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Description = "Id of the request in the logs of the service.",
            Examples = [JsonValue.Create("00-3ca0c357e049bdf1d21003a19c6c9539-c4e6a1f2b9d37e01-00")],
        };

        if (type == typeof(ValidationProblemDetails))
        {
            schema.Properties["errors"] = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Description = "Messages per field of the request; \"$\" or \"\" for the request body as a whole.",
                AdditionalProperties = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema { Type = JsonSchemaType.String },
                },
                Examples = [new JsonObject { ["artist"] = new JsonArray("The Artist is not specified in the request.") }],
            };
        }
        else
        {
            schema.Properties[Constants.ErrorCodeExtensionName] = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Description = "Code of a search error, for example: Lyric.NotFound, Lyric.ProvidersUnavailable, Lyric.BadRequest, "
                    + "Lyric.ServiceMisconfigured, Lyric.SearchError. Missing for an unexpected error.",
                Examples = [JsonValue.Create("Lyric.NotFound")],
            };
        }

        return Task.CompletedTask;
    }
}
