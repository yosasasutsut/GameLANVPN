# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Server/GameLANVPN.Server/GameLANVPN.Server.csproj", "src/Server/GameLANVPN.Server/"]
COPY ["src/Server/GameLANVPN.API/GameLANVPN.API.csproj", "src/Server/GameLANVPN.API/"]

# Restore dependencies
RUN dotnet restore "src/Server/GameLANVPN.Server/GameLANVPN.Server.csproj"

# Copy source code
COPY . .

# Build application
WORKDIR "/src/src/Server/GameLANVPN.Server"
RUN dotnet build "GameLANVPN.Server.csproj" -c Release -o /app/build

# Publish application
FROM build AS publish
RUN dotnet publish "GameLANVPN.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install dependencies for network operations
RUN apt-get update && \
    apt-get install -y \
    iputils-ping \
    net-tools \
    iptables \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r gamelanvpn && useradd -r -g gamelanvpn gamelanvpn

# Copy published application
COPY --from=publish /app/publish .

# Configure ports
EXPOSE 5000
EXPOSE 5001
EXPOSE 6112-6119/udp
EXPOSE 27015/udp

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS="http://+:5000;https://+:5001"
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Switch to non-root user
USER gamelanvpn

# Start application
ENTRYPOINT ["dotnet", "GameLANVPN.Server.dll"]