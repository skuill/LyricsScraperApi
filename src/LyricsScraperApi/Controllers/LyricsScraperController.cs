using LyricsScraperApi.Handlers;
using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.Models.Responses;
using LyricsScraperApi.ResultPattern;
using LyricsScraperApi.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LyricsScraperApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LyricsScraperController(
        ILogger<LyricsScraperController> logger,
        ILyricsScraperService lyricsScraperService) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResultDto))]
        public async Task<IActionResult> GetLyric(
            [FromBody, SwaggerRequestBody("The search request payload", Required = true)] SearchRequestBaseDto searchRequestBase)
        {
            logger.LogInformation("Searching lyric: {Request}", searchRequestBase.ToString());

            var result = await lyricsScraperService.SearchLyricAsync(searchRequestBase);

            return result.Match(
                success =>
                {
                    logger.LogInformation("Successfully retrieved lyric: {Request}", searchRequestBase.ToString());
                    return Ok(result.Value);
                },
                error =>
                {
                    logger.LogError("Failed to retrieve lyric: {Error}", error.Description);
                    return error.ToActionResult();
                });
        }
    }
}
