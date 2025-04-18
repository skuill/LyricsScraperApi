using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LyricsScraperApi.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        // This method is called when an exception is thrown during the request pipeline.
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
            _logger.LogError(
                exception,
                "Error occurred processing {Path}. TraceId: {TraceId}. Error: {Error}",
                httpContext.Request.Path,
                traceId,
                exception.Message
            );

            var status = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };
            httpContext.Response.StatusCode = status;

            var problemDetails = new ProblemDetails
            {
                Status = status,
                Title = "An error occurred",
                Type = exception.GetType().Name,
                Detail = exception.Message
            };

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
