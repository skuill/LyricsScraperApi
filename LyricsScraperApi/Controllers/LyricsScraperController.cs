using AutoMapper;
using LyricsScraperApi.Models.Requests;
using LyricsScraperNET;
using LyricsScraperNET.Models.Requests;
using LyricsScraperNET.Models.Responses;
using LyricsScraperNET.Providers.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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

        public LyricsScraperController(ILoggerFactory loggerFactory,
            ILogger<LyricsScraperController> logger,
            IMapper mapper,
            ILyricsScraperClient lyricsScraperClient)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _lyricsScraperClient = lyricsScraperClient ?? throw new ArgumentNullException(nameof(lyricsScraperClient));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.Responses.SearchResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchLyric(
            [FromBody, SwaggerRequestBody("The search request payload", Required = true)] SearchRequestBase searchRequestBase)
        {
            if (searchRequestBase == null)
                return BadRequest();

            ConfigureLyricsScraperClient();

            var lyricsScraperClientRequest = _mapper.Map<SearchRequest>(searchRequestBase);

            var searchResult = await _lyricsScraperClient.SearchLyricAsync(lyricsScraperClientRequest);

            if (searchResult.IsEmpty() && !searchResult.Instrumental || searchResult.ResponseStatusCode == ResponseStatusCode.NoDataFound)
            {
                _logger.LogWarning($"Lyric not found. Search request: {searchRequestBase.ToString()}");
                return NotFound();
            }

            if (searchResult.ResponseStatusCode == ResponseStatusCode.BadRequest)
            {
                _logger.LogWarning($"Lyric not found. Bad search request: {searchRequestBase.ToString()}");
                return BadRequest(searchResult.ResponseMessage);
            }

            if (searchResult.ResponseStatusCode == ResponseStatusCode.Error)
            {
                _logger.LogWarning($"Lyric not found. Error occured. Search request: {searchRequestBase.ToString()}. Search response message: {searchResult.ResponseMessage}");
                return Problem(searchResult.ResponseMessage);
            }

            _logger.LogDebug($"Found lyric. {searchRequestBase}");

            var result = _mapper.Map<Models.Responses.SearchResult>(searchResult);
            return Ok(result);
        }

        private void ConfigureLyricsScraperClient()
        {
            _lyricsScraperClient.WithAllProviders();
            _lyricsScraperClient.RemoveProvider(ExternalProviderType.Musixmatch); // Remove after fixing: https://github.com/skuill/LyricsScraperNET/issues/24
            
            _lyricsScraperClient.WithLogger(_loggerFactory);
        }
    }
}
