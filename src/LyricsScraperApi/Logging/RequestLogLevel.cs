using Serilog.Events;

namespace LyricsScraperApi.Logging;

/// <summary>
/// Level of the "HTTP POST /api/LyricsScraper responded 200 in 1102 ms" line of the Serilog request logging.
/// </summary>
public static class RequestLogLevel
{
    public const string HealthPath = "/api/health";

    /// <summary>
    /// Errors (5xx or an exception) are <see cref="LogEventLevel.Error"/>. A successful health check is
    /// <see cref="LogEventLevel.Verbose"/>, below the minimum level, so the Docker health check every 30 seconds
    /// does not fill the logs. Everything else is <see cref="LogEventLevel.Information"/>.
    /// </summary>
    public static LogEventLevel Get(HttpContext httpContext, double elapsedMilliseconds, Exception? exception)
    {
        if (exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            return LogEventLevel.Error;
        }

        return httpContext.Request.Path.StartsWithSegments(HealthPath)
            ? LogEventLevel.Verbose
            : LogEventLevel.Information;
    }
}
