using AutoMapper;
using FluentValidation;
using LyricsScraperApi.Models.Requests;
using LyricsScraperNET;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text;

namespace LyricsScraperApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LyricsScraperController : ControllerBase
    {
        private readonly ILogger<LyricsScraperController> _logger;
        private readonly ILoggerFactory _loggerFactory;

        private readonly IMapper _mapper;
        private readonly ILyricsScraperClient _lyricsScraperClient;
        private readonly IValidator<SearchRequestBase> _searchRequestValidator;

        public LyricsScraperController(ILoggerFactory loggerFactory,
            ILogger<LyricsScraperController> logger,
            IMapper mapper,
            ILyricsScraperClient lyricsScraperClient,
            IValidator<SearchRequestBase> searchRequestValidator)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _lyricsScraperClient = lyricsScraperClient ?? throw new ArgumentNullException(nameof(lyricsScraperClient));
            _searchRequestValidator = searchRequestValidator ?? throw new ArgumentNullException(nameof(searchRequestValidator));
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
            var searchRequestValidation = await ValidateSearchRequest(searchRequestBase);
            if (!searchRequestValidation.IsSuccess)
            {
                return searchRequestValidation.Result;
            }

            // LyricsScraperClient setting up
            ConfigureLyricsScraperClient();

            var lyricsScraperClientRequest = _mapper.Map<SearchRequest>(searchRequestBase);

            var searchResult = await _lyricsScraperClient.SearchLyricAsync(lyricsScraperClientRequest);

            // Search result validation
            var searchResultValidaton = await ValidateSearchResult(searchRequestBase, searchResult);
            if (!searchResultValidaton.IsSuccess)
            {
                return searchResultValidaton.Result;
            }

            _logger.LogDebug($"Found lyric. {searchRequestBase}");
            var result = _mapper.Map<Models.Responses.SearchResult>(searchResult);

            return Ok(result);
        }

        private async Task<(bool IsSuccess, IActionResult Result)> ValidateSearchRequest(SearchRequestBase searchRequest)
        {
            if (searchRequest == null)
                return (false, BadRequest("The search request is empty."));

            var requestValidationResult = await _searchRequestValidator.ValidateAsync(searchRequest);

            if (!requestValidationResult.IsValid)
            {
                StringBuilder validationErrors = new StringBuilder();
                foreach (var failure in requestValidationResult.Errors)
                {
                    validationErrors.AppendLine("Property " + failure.PropertyName + " failed validation. Error was: " + failure.ErrorMessage);
                }
                return (false, BadRequest(validationErrors.ToString()));
            }
            return (true, null);
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
                return (false, Problem(searchResult.ResponseMessage));
            }

            return (true, null);
        }

        private void ConfigureLyricsScraperClient()
        {
            _lyricsScraperClient.WithAllProviders();
            _lyricsScraperClient.RemoveProvider(ExternalProviderType.Musixmatch); // Remove after fixing: https://github.com/skuill/LyricsScraperNET/issues/24
            
            _lyricsScraperClient.WithLogger(_loggerFactory);
        }
    }
}
