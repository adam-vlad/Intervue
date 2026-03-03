using Intervue.Application;
using Intervue.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Register Application layer services (MediatR, FluentValidation, Behaviors)
builder.Services.AddApplication();

// Register Infrastructure layer services (Ollama, PdfPig, SHA-256, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Register controllers and Swagger for API documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
