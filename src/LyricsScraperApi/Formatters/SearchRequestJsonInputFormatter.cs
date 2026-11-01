using LyricsScraperApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace LyricsScraperApi.Formatters
{
    /// <summary>
    /// The JSON input formatter of ASP.NET Core with one addition: a search request without <c>requestType</c> is a
    /// validation error (400). System.Text.Json throws <see cref="NotSupportedException"/> for it (the base type is
    /// abstract), and MVC turns only <see cref="System.Text.Json.JsonException"/> into a model error, so it would be a 500.
    /// </summary>
    public sealed class SearchRequestJsonInputFormatter(SystemTextJsonInputFormatter inner)
        : IInputFormatter, IApiRequestFormatMetadataProvider
    {
        public const string MissingRequestTypeMessage =
            $"The {Constants.SearchRequestDiscriminatorName} is not specified in the request. "
            + $"Available values: {Constants.ArtistAndSongRequestDiscriminatorValue}, {Constants.UriRequestDiscriminatorValue}.";

        public bool CanRead(InputFormatterContext context) => inner.CanRead(context);

        public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            try
            {
                return await inner.ReadAsync(context);
            }
            catch (NotSupportedException)
            {
                context.ModelState.TryAddModelError(Constants.SearchRequestDiscriminatorName, MissingRequestTypeMessage);
                return InputFormatterResult.Failure();
            }
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
}
