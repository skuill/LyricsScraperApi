using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;

namespace LyricsScraperApi.Handlers;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment) : IExceptionHandler
{
    /// <summary>
    /// Status code for a request that the client closed before the answer (as in nginx).
    /// </summary>
    public const int ClientClosedRequestStatusCode = 499;

    // This method is called when an exception is thrown during the request pipeline.
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("The client closed the request {Path}. TraceId: {TraceId}", httpContext.Request.Path, traceId);
            httpContext.Response.StatusCode = ClientClosedRequestStatusCode;
            return true;
        }

        logger.LogError(
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

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = status,
                Title = "An error occurred",
                // The message of an unexpected exception can show internals, so only Development returns it.
                Detail = status == StatusCodes.Status400BadRequest || environment.IsDevelopment() ? exception.Message : null,
            },
        });
    }
}
