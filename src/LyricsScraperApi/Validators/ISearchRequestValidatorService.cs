using LyricsScraperApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LyricsScraperApi.Validators
{
    public interface ISearchRequestValidatorService
    {
        Task<(bool IsSuccess, IActionResult Result)> ValidateRequest(SearchRequestBase searchRequest);
    }
}
