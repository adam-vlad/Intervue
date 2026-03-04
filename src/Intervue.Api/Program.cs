using Asp.Versioning;
using Intervue.Application;
using Intervue.Infrastructure;
using Intervue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register Application layer services (MediatR, FluentValidation, Behaviors)
builder.Services.AddApplication();

// Register Infrastructure layer services (Ollama, PdfPig, SHA-256, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Register controllers and Swagger for API documentation
builder.Services.AddControllers();

// API versioning (URL segment: api/v1/...)
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Automatically create/update database tables on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IntervueDbContext>();
    await db.Database.MigrateAsync();
}

// Enable Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
