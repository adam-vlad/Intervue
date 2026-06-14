# ── Stage 1: Build the application ──────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first (for better Docker layer caching)
COPY Intervue.sln .
COPY src/Intervue.Domain/Intervue.Domain.csproj src/Intervue.Domain/
COPY src/Intervue.Application/Intervue.Application.csproj src/Intervue.Application/
COPY src/Intervue.Infrastructure/Intervue.Infrastructure.csproj src/Intervue.Infrastructure/
COPY src/Intervue.Api/Intervue.Api.csproj src/Intervue.Api/
COPY tests/Intervue.UnitTests/Intervue.UnitTests.csproj tests/Intervue.UnitTests/
COPY tests/Intervue.IntegrationTests/Intervue.IntegrationTests.csproj tests/Intervue.IntegrationTests/

# Restore NuGet packages
RUN dotnet restore

# Copy all source code and build
COPY . .
RUN dotnet publish src/Intervue.Api/Intervue.Api.csproj -c Release -o /app/publish --no-restore

# ── Stage 2: Run the application (smaller image) ───────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Intervue.Api.dll"]
