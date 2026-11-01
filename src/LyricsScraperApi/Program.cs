using LyricsScraperApi.Extensions;
using LyricsScraperApi.Logging;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting up the application...");

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.AddPresentation();
    builder.Services.AddApplication();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseResponseCompression();
    app.UseSerilogRequestLogging(); // options (levels) in AddPresentation
    // No UseHttpsRedirection: API clients usually do not follow redirects, and the Docker image serves HTTP only
    // (HTTPS belongs to a reverse proxy or to ASPNETCORE_HTTPS_PORTS with a certificate).

    // The API description is available in every environment: the Docker image runs in Production.
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "LyricsScraperApi"));

    app.MapControllers();
    app.MapHealthChecks(RequestLogLevel.HealthPath);

    app.Run();
    return 0;
}
// HostAbortedException: the host was stopped on purpose (for example by WebApplicationFactory in the tests).
catch (Exception ex) when (ex is not HostAbortedException)
{
    // A non-zero exit code tells Docker that the container failed.
    Log.Fatal(ex, "Application start-up failed");
    return 1;
}
finally
{
    // Ensure to flush and close the log
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
