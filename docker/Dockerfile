# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
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

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Expose port 80 (standard HTTP port)
EXPOSE 80

# Install curl for health checks
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Set environment variable to listen on port 80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "VotingApp.dll"]
