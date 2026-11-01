using LyricsScraperApi.Handlers;
using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.OpenApi;
using LyricsScraperApi.Models.Responses;
using LyricsScraperApi.ResultPattern;
using LyricsScraperApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LyricsScraperApi.Controllers;

// Logs: the request logging of Serilog writes every request (status, time), LyricsScraperService the search result.
[ApiController]
[Route("api/[controller]")]
public class LyricsScraperController(ILyricsScraperService lyricsScraperService) : ControllerBase
{
    /// <summary>
    /// Searches the lyrics of a song by artist and song title or by the address of a lyrics page.
    /// </summary>
    /// <param name="searchRequestBase">The search request payload.</param>
    /// <response code="200">The lyrics are found (or the song is an instrumental).</response>
    /// <response code="400">The request is not valid.</response>
    /// <response code="404">No provider has the lyrics.</response>
    /// <response code="500">The service is misconfigured or failed.</response>
    /// <response code="503">No provider could answer (site down, timeout, error): try again later.</response>
    [HttpPost(Name = SearchExamplesOperationTransformer.OperationId)] // operationId in the OpenAPI document
    [ProducesResponseType<SearchResultDto>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable, "application/problem+json")]
    // CS1573: no <param> for cancellationToken on purpose. Microsoft.AspNetCore.OpenApi 10.0 puts the text of
    // that tag into the description of the request body instead of the searchRequestBase one.
#pragma warning disable CS1573
    public async Task<IActionResult> GetLyric(
        [FromBody] SearchRequestBaseDto searchRequestBase,
        CancellationToken cancellationToken)
#pragma warning restore CS1573
    {
        var result = await lyricsScraperService.SearchLyricAsync(searchRequestBase, cancellationToken);

        return result.Match<SearchResultDto, IActionResult>(Ok, error => error.ToActionResult(this));
    }
}
