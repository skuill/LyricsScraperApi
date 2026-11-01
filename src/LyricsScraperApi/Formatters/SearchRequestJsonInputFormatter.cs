using LyricsScraperApi.Helpers;
using LyricsScraperApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace LyricsScraperApi.Formatters;

/// <summary>
/// The JSON input formatter of ASP.NET Core with clearer errors for a search request:
/// <list type="bullet">
/// <item>Without <c>requestType</c> it is a validation error (400). System.Text.Json throws
/// <see cref="NotSupportedException"/> for it (the base type is abstract), and MVC turns only
/// <see cref="System.Text.Json.JsonException"/> into a model error, so it would be a 500.</item>
/// <item>An unknown <c>provider</c> gets the list of the available values instead of the System.Text.Json message
/// with the internal type name.</item>
/// </list>
/// </summary>
public sealed class SearchRequestJsonInputFormatter(SystemTextJsonInputFormatter inner)
    : IInputFormatter, IApiRequestFormatMetadataProvider
{
    public const string ProviderFieldName = "provider";

    public const string MissingRequestTypeMessage =
        $"The {Constants.SearchRequestDiscriminatorName} is not specified in the request. "
        + $"Available values: {Constants.ArtistAndSongRequestDiscriminatorValue}, {Constants.UriRequestDiscriminatorValue}.";

    public static readonly string InvalidProviderMessage =
        $"The {ProviderFieldName} is not valid. Available values: {string.Join(", ", Enum.GetNames<ExternalProviderTypeDto>())}.";

    // Key of the System.Text.Json error in the model state: the JSON path of the property.
    private const string ProviderJsonPath = "$." + ProviderFieldName;

    public bool CanRead(InputFormatterContext context) => inner.CanRead(context);

    public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
    {
        InputFormatterResult result;
        try
        {
            result = await inner.ReadAsync(context);
        }
        catch (NotSupportedException)
        {
            context.ModelState.TryAddModelError(Constants.SearchRequestDiscriminatorName, MissingRequestTypeMessage);
            return InputFormatterResult.Failure();
        }

        var providerKey = context.ModelState.Keys.FirstOrDefault(key => key.Equals(ProviderJsonPath, StringComparison.OrdinalIgnoreCase));
        if (providerKey is not null)
        {
            context.ModelState.Remove(providerKey);
            context.ModelState.TryAddModelError(ProviderFieldName, InvalidProviderMessage);
        }

        return result;
    }

    // Content types for the OpenAPI document.
    public IReadOnlyList<string>? GetSupportedContentTypes(string? contentType, Type objectType)
        => inner.GetSupportedContentTypes(contentType, objectType);

    /// <summary>
    /// Replaces the System.Text.Json input formatter of MVC with this one.
    /// </summary>
    public static void Replace(MvcOptions options)
    {
        var formatter = options.InputFormatters.OfType<SystemTextJsonInputFormatter>().Single();
        options.InputFormatters[options.InputFormatters.IndexOf(formatter)] = new SearchRequestJsonInputFormatter(formatter);
    }
}
