# ── Stage 1: Build the application ──────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first (for better Docker layer caching)
COPY MockInterview.sln .
COPY src/MockInterview.Domain/MockInterview.Domain.csproj src/MockInterview.Domain/
COPY src/MockInterview.Application/MockInterview.Application.csproj src/MockInterview.Application/
COPY src/MockInterview.Infrastructure/MockInterview.Infrastructure.csproj src/MockInterview.Infrastructure/
COPY src/MockInterview.Api/MockInterview.Api.csproj src/MockInterview.Api/
COPY tests/MockInterview.UnitTests/MockInterview.UnitTests.csproj tests/MockInterview.UnitTests/
COPY tests/MockInterview.IntegrationTests/MockInterview.IntegrationTests.csproj tests/MockInterview.IntegrationTests/

# Restore NuGet packages
RUN dotnet restore

# Copy all source code and build
COPY . .
RUN dotnet publish src/MockInterview.Api/MockInterview.Api.csproj -c Release -o /app/publish --no-restore

# ── Stage 2: Run the application (smaller image) ───────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "MockInterview.Api.dll"]
