using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Intervue.Application.Common.Interfaces;
using Intervue.Domain.Repositories;
using Intervue.Infrastructure.Configuration;
using Intervue.Infrastructure.Persistence;
using Intervue.Infrastructure.Services;

namespace Intervue.Infrastructure;

/// <summary>
/// Extension method to register all Infrastructure layer services in the DI container.
/// Called from Program.cs: builder.Services.AddInfrastructure(builder.Configuration);
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        // Build Npgsql data source once and enable JSON mappings there (replacement for obsolete GlobalTypeMapper)
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddSingleton(dataSource);

        // Register PostgreSQL database via EF Core
        services.AddDbContext<IntervueDbContext>(options =>
            options.UseNpgsql(dataSource,
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure())
            );

        // Bind Ollama settings from appsettings.json
        services.Configure<OllamaSettings>(
            configuration.GetSection(OllamaSettings.SectionName));

        // Register OllamaClient with HttpClient (uses IHttpClientFactory internally)
        // Timeout set to 10 minutes because local LLM inference can be slow on consumer hardware
        services.AddHttpClient<ILlmClient, OllamaClient>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        // Register PDF extractor and hashing service
        services.AddSingleton<IPdfExtractor, PdfPigExtractor>();
        services.AddSingleton<IHashingService, Sha256HashingService>();

        // Register repositories (backed by PostgreSQL via EF Core)
        services.AddScoped<ICvProfileRepository, EfCvProfileRepository>();
        services.AddScoped<IInterviewRepository, EfInterviewRepository>();

        return services;
    }
}
