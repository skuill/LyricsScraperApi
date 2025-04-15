using LyricsScraperApi.Models;
using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.Validators;
using LyricsScraperNET;
using LyricsScraperNET.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LyricsScraperApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LyricsScraperController : ControllerBase
    {
        private readonly ILogger<LyricsScraperController> _logger;

        private readonly ILyricsScraperClient _lyricsScraperClient;
        private readonly ISearchRequestValidatorService _searchRequestValidatorService;

        public LyricsScraperController(ILogger<LyricsScraperController> logger,
            ILyricsScraperClient lyricsScraperClient,
            ISearchRequestValidatorService searchRequestValidatorService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lyricsScraperClient = lyricsScraperClient ?? throw new ArgumentNullException(nameof(lyricsScraperClient));
            _searchRequestValidatorService = searchRequestValidatorService ?? throw new ArgumentNullException(nameof(searchRequestValidatorService));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.Responses.SearchResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchLyric(
            [FromBody, SwaggerRequestBody("The search request payload", Required = true)] SearchRequestBase searchRequestBase)
        {
            // Search request validation
            var searchRequestValidation = await _searchRequestValidatorService.ValidateRequest(searchRequestBase);
            if (!searchRequestValidation.IsSuccess)
            {
                return searchRequestValidation.Result;
            }

            var lyricsScraperClientRequest = searchRequestBase.MapToLibrary();

            var searchResult = await _lyricsScraperClient.SearchLyricAsync(lyricsScraperClientRequest);

            // Search result validation
            var searchResultValidaton = await ValidateSearchResult(searchRequestBase, searchResult);
            if (!searchResultValidaton.IsSuccess)
            {
                return searchResultValidaton.Result;
            }

            _logger.LogDebug($"Found lyric. {searchRequestBase}");
            var result = searchResult.MapToApi();

            return Ok(result);
        }

        private async Task<(bool IsSuccess, IActionResult Result)> ValidateSearchResult(SearchRequestBase searchRequest, SearchResult? searchResult)
        {
            if (searchResult.IsEmpty() && !searchResult.Instrumental || searchResult.ResponseStatusCode == ResponseStatusCode.NoDataFound)
            {
                _logger.LogWarning($"Lyric not found. Search request: {searchRequest.ToString()}");
                return (false, NotFound());
            }

            if (searchResult.ResponseStatusCode == ResponseStatusCode.BadRequest)
            {
                _logger.LogWarning($"Lyric not found. Bad search request: {searchRequest.ToString()}");
                return (false, BadRequest(searchResult.ResponseMessage));
            }

            if (searchResult.ResponseStatusCode == ResponseStatusCode.Error)
            {
                _logger.LogWarning($"Lyric not found. Error occured. Search request: {searchRequest.ToString()}. Search response message: {searchResult.ResponseMessage}");
                return (false, StatusCode(500, searchResult.ResponseMessage));
            }

            if (searchResult.ResponseStatusCode == ResponseStatusCode.RegionRestricted)
            {
                _logger.LogInformation($"Lyric not found. The lyrics is not available in your region. Search request: {searchRequest.ToString()}.");
                return (false, StatusCode(403, "The lyrics is not available in your region."));
            }

            return (true, null);
        }
    }
}
