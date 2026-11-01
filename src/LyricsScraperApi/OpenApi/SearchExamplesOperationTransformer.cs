using LyricsScraperApi.Formatters;
using LyricsScraperApi.Helpers;
using LyricsScraperApi.Models.Errors;
using LyricsScraperApi.Models.Requests;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace LyricsScraperApi.OpenApi;

/// <summary>
/// Named examples of the search: one per kind of request and real responses for every status code. Without them
/// Swagger UI builds the request example from the first schema without <c>requestType</c>, so "Try it out" fails,
/// and every error example shows <c>"string"</c> placeholders. The texts come from the same constants as the code.
/// </summary>
public sealed class SearchExamplesOperationTransformer : IOpenApiOperationTransformer
{
    public const string OperationId = "SearchLyrics";

    private const string Instance = "POST /api/LyricsScraper";
    private const string TraceId = "00-3ca0c357e049bdf1d21003a19c6c9539-c4e6a1f2b9d37e01-00";
    private const string AllProvidersFailedMessage =
        "No provider could check the search request (site unavailable, timeout or error), see ProviderAttempts";

    private static readonly ArtistAndSongSearchRequestDto SampleRequest = new() { Artist = "Parkway Drive", Song = "Idols And Anchors" };

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (operation.OperationId != OperationId)
        {
            return Task.CompletedTask;
        }

        SetExamples(operation.RequestBody?.Content, new()
        {
            ["artistAndSong"] = Example("Search by artist and song", new JsonObject
            {
                [Constants.SearchRequestDiscriminatorName] = Constants.ArtistAndSongRequestDiscriminatorValue,
                ["artist"] = SampleRequest.Artist,
                ["song"] = SampleRequest.Song,
                ["provider"] = "All",
            }),
            ["uri"] = Example("Search by the address of a lyrics page", new JsonObject
            {
                [Constants.SearchRequestDiscriminatorName] = Constants.UriRequestDiscriminatorValue,
                ["uri"] = "https://genius.com/Parkway-drive-idols-and-anchors-lyrics",
            }),
        });

        SetExamples(Response(operation, StatusCodes.Status200OK), new()
        {
            ["found"] = Example("Lyrics found", new JsonObject
            {
                ["instrumental"] = false,
                ["externalProvider"] = "Lrclib",
                ["lyricText"] = "First line of the lyrics\nSecond line of the lyrics",
            }),
            ["instrumental"] = Example("An instrumental: no text", new JsonObject
            {
                ["instrumental"] = true,
                ["externalProvider"] = "Genius",
                ["lyricText"] = "",
            }),
        });

        SetExamples(Response(operation, StatusCodes.Status400BadRequest), new()
        {
            ["emptyArtist"] = Example("Empty artist", ValidationProblem("artist", "The Artist is not specified in the request.")),
            ["missingRequestType"] = Example("No requestType",
                ValidationProblem(Constants.SearchRequestDiscriminatorName, SearchRequestJsonInputFormatter.MissingRequestTypeMessage)),
            ["unknownProvider"] = Example("Unknown provider",
                ValidationProblem(SearchRequestJsonInputFormatter.ProviderFieldName, SearchRequestJsonInputFormatter.InvalidProviderMessage)),
        });

        SetExamples(Response(operation, StatusCodes.Status404NotFound), new()
        {
            ["notFound"] = Example("No provider has the lyrics", SearchProblem(LyricsScraperErrors.LyricNotFound(SampleRequest), "15.5.5", "Not Found")),
        });

        SetExamples(Response(operation, StatusCodes.Status500InternalServerError), new()
        {
            ["unexpected"] = Example("Unexpected error (no details outside Development)", new JsonObject
            {
                ["type"] = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                ["title"] = "An error occurred",
                ["status"] = StatusCodes.Status500InternalServerError,
                ["instance"] = Instance,
                ["traceId"] = TraceId,
            }),
        });

        SetExamples(Response(operation, StatusCodes.Status503ServiceUnavailable), new()
        {
            ["providersUnavailable"] = Example("Every provider failed: try again later",
                SearchProblem(LyricsScraperErrors.ProvidersUnavailable(SampleRequest, AllProvidersFailedMessage), "15.6.4", "Service Unavailable")),
        });

        return Task.CompletedTask;
    }

    private static IDictionary<string, OpenApiMediaType>? Response(OpenApiOperation operation, int statusCode)
        => operation.Responses?.TryGetValue(statusCode.ToString(), out var response) == true ? response.Content : null;

    private static void SetExamples(IDictionary<string, OpenApiMediaType>? content, Dictionary<string, IOpenApiExample> examples)
    {
        foreach (var mediaType in content?.Values ?? [])
        {
            mediaType.Examples = examples;
        }
    }

    private static OpenApiExample Example(string summary, JsonNode value) => new() { Summary = summary, Value = value };

    private static JsonObject SearchProblem(BaseError error, string rfcSection, string title) => new()
    {
        ["type"] = $"https://tools.ietf.org/html/rfc9110#section-{rfcSection}",
        ["title"] = title,
        ["status"] = error.StatusCode,
        ["detail"] = error.Description,
        ["instance"] = Instance,
        [Constants.ErrorCodeExtensionName] = error.Code,
        ["traceId"] = TraceId,
    };

    private static JsonObject ValidationProblem(string field, string message) => new()
    {
        ["type"] = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        ["title"] = "One or more validation errors occurred.",
        ["status"] = StatusCodes.Status400BadRequest,
        ["instance"] = Instance,
        ["errors"] = new JsonObject { [field] = new JsonArray(message) },
        ["traceId"] = TraceId,
    };
}
