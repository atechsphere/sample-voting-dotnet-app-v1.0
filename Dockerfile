# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/VotingAppSolution.sln", "."]
COPY ["src/VotingApp/VotingApp.csproj", "VotingApp/"]
COPY ["src/VotingApp.Models/VotingApp.Models.csproj", "VotingApp.Models/"]
COPY ["src/VotingApp.Services/VotingApp.Services.csproj", "VotingApp.Services/"]
COPY ["src/VotingApp.Data/VotingApp.Data.csproj", "VotingApp.Data/"]
COPY ["src/VotingApp.Controllers/VotingApp.Controllers.csproj", "VotingApp.Controllers/"]
COPY ["src/VotingApp.Tests/VotingApp.Tests.csproj", "VotingApp.Tests/"]

# Restore dependencies
RUN dotnet restore "VotingAppSolution.sln"

# Copy all source code
COPY src/ .

# Build and publish
RUN dotnet publish "VotingApp/VotingApp.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Test stage
FROM build AS test
WORKDIR /src
RUN dotnet test "VotingApp.Tests/VotingApp.Tests.csproj" \
    --logger "trx" \
    --results-directory /testresults \
    --collect:"XPlat Code Coverage" \
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install system dependencies
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "VotingApp.dll"]
