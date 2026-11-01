using Microsoft.AspNetCore.OpenApi;
using System.Text.Json.Serialization.Metadata;

namespace LyricsScraperApi.OpenApi;

/// <summary>
/// Names of the schemas in the OpenAPI document without the C# suffixes <c>Dto</c> and <c>Base</c>. The generator
/// puts the name of the base type in front of a derived type, so the derived name drops its base part:
/// <c>UriSearchRequestDto</c> becomes <c>SearchRequestUri</c>. Client generators name their classes after them.
/// </summary>
public static class SchemaReferenceIds
{
    public static string? Create(JsonTypeInfo typeInfo)
    {
        var id = OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo);
        var type = typeInfo.Type;
        if (id is null || type.IsGenericType || !IsApiType(type))
        {
            return id;
        }

        var name = Clean(type);
        if (type.BaseType is { } baseType && IsApiType(baseType))
        {
            var baseName = Clean(baseType);
            if (name.EndsWith(baseName, StringComparison.Ordinal) && name.Length > baseName.Length)
            {
                name = name[..^baseName.Length];
            }
        }

        return name;
    }

    private static bool IsApiType(Type type) => type.Namespace?.StartsWith("LyricsScraperApi", StringComparison.Ordinal) == true;

    private static string Clean(Type type)
    {
        var name = type.Name;
        if (name.EndsWith("Dto", StringComparison.Ordinal)) name = name[..^3];
        if (name.EndsWith("Base", StringComparison.Ordinal)) name = name[..^4];
        return name;
    }
}
