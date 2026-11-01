using LyricsScraperApi.Helpers;
using LyricsScraperApi.Models.Errors;
using Microsoft.AspNetCore.Mvc;

namespace LyricsScraperApi.Handlers;

public static class GlobalErrorHandler
{
    /// <summary>
    /// Turns an error into a ProblemDetails response. The ProblemDetailsFactory of the controller adds the
    /// standard title, traceId and the fields from <c>AddProblemDetails</c>, so all errors of the API look the same.
    /// </summary>
    public static IActionResult ToActionResult(this BaseError error, ControllerBase controller)
    {
        var problemDetails = controller.ProblemDetailsFactory.CreateProblemDetails(
            controller.HttpContext,
            statusCode: error.StatusCode,
            detail: error.Description);
        problemDetails.Extensions[Constants.ErrorCodeExtensionName] = error.Code;

        return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
    }
}
