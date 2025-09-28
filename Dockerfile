# GameLAN VPN Server - Multi-stage Docker Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy and restore dependencies
COPY src/Server/GameLANVPN.Server/*.csproj ./GameLANVPN.Server/
RUN dotnet restore GameLANVPN.Server/GameLANVPN.Server.csproj

# Copy everything and build
COPY src/Server/GameLANVPN.Server/. ./GameLANVPN.Server/
WORKDIR /source/GameLANVPN.Server
RUN dotnet publish -c Release -o /app

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install SQLite
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app .

# Create data directory
RUN mkdir -p /app/data

# Expose ports
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

# Run the server
ENTRYPOINT ["dotnet", "GameLANVPN.Server.dll"]