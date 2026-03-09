using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Intervue.Application.Common.Interfaces;
using Intervue.Infrastructure.Persistence;

namespace Intervue.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that replaces PostgreSQL with EF Core InMemory
/// and mocks the LLM client so integration tests run without Docker/external services.
/// </summary>
public class IntervueWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<ILlmClient> LlmClientMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext options registration (PostgreSQL)
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<IntervueDbContext>))
                .ToList();
            foreach (var d in descriptorsToRemove)
                services.Remove(d);

            // Remove the real ILlmClient registration(s)
            var llmDescriptors = services
                .Where(d => d.ServiceType == typeof(ILlmClient))
                .ToList();
            foreach (var d in llmDescriptors)
                services.Remove(d);

            // Register InMemory database by directly providing options
            // (using AddSingleton instead of AddDbContext avoids duplicate
            // provider-service registrations that cause conflicts)
            var dbName = $"IntervueTestDb_{Guid.NewGuid()}";
            services.AddSingleton<DbContextOptions<IntervueDbContext>>(_ =>
                new DbContextOptionsBuilder<IntervueDbContext>()
                    .UseInMemoryDatabase(dbName)
                    .Options);

            // Register mock LLM client
            services.AddSingleton<ILlmClient>(LlmClientMock.Object);
        });

        builder.UseEnvironment("Development");
    }
}
