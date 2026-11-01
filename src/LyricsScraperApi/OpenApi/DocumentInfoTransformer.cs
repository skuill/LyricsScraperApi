using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Reflection;

namespace LyricsScraperApi.OpenApi;

/// <summary>
/// Title, description and version of the OpenAPI document. The version is the release tag (build argument
/// <c>VERSION</c> of the Dockerfile).
/// </summary>
public sealed class DocumentInfoTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info.Title = "LyricsScraperApi";
        document.Info.Description = "Search for song lyrics across several lyrics sites, built on LyricsScraperNET.";
        document.Info.Version = typeof(DocumentInfoTransformer).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? document.Info.Version;
        return Task.CompletedTask;
    }
}
