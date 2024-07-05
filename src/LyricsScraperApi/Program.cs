using FluentValidation;
using LyricsScraperApi.Middlewares;
using LyricsScraperApi.Models;
using LyricsScraperApi.Models.Requests;
using LyricsScraperApi.Validators;
using LyricsScraperNET;
using Serilog;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.EnableAnnotations(enableAnnotationsForInheritance: true, enableAnnotationsForPolymorphism: true);
    opts.UseOneOfForPolymorphism();
    opts.SelectSubTypesUsing(baseType =>
    {
        if (baseType == typeof(SearchRequestBase))
        {
            return new[]
            {
                typeof(ArtistAndSongSearchRequest),
                typeof(UriSearchRequest),
            };
        }

        return Enumerable.Empty<Type>();
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<MappingProfile>(); });

builder.Services.AddScoped<ILyricsScraperClient, LyricsScraperClient>();
builder.Services.AddScoped<IValidator<SearchRequestBase>, SearchRequestBaseValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
