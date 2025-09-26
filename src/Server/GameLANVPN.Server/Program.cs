using GameLANVPN.Server.Hubs;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/gamelanvpn-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

// Add controllers for API endpoints
builder.Services.AddControllers();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseCors();

// Health check endpoint
app.MapHealthChecks("/health");

// SignalR Hub
app.MapHub<GameHub>("/gamehub");

// API routes
app.MapControllers();

// Serve static files for client downloads
app.UseStaticFiles();

// Default route
app.MapGet("/", () => Results.Json(new
{
    Name = "GameLAN VPN Server",
    Version = "0.1.0",
    Status = "Running",
    Endpoints = new
    {
        GameHub = "/gamehub",
        Health = "/health",
        API = "/api"
    }
}));

// API info endpoint
app.MapGet("/api", () => Results.Json(new
{
    Version = "v1",
    Endpoints = new
    {
        Rooms = "/api/rooms",
        Players = "/api/players",
        Stats = "/api/stats"
    }
}));

// Rooms API
app.MapGet("/api/rooms", () =>
{
    // Return active rooms (this would come from a service in production)
    return Results.Json(new { rooms = Array.Empty<object>(), count = 0 });
});

// Server stats API
app.MapGet("/api/stats", () =>
{
    return Results.Json(new
    {
        uptime = DateTime.UtcNow.ToString(),
        activeRooms = 0,
        totalPlayers = 0,
        serverLoad = "Low"
    });
});

Log.Information("Starting GameLAN VPN Server...");
Log.Information("Hub endpoint: {HubEndpoint}", "/gamehub");
Log.Information("Health check: {HealthEndpoint}", "/health");

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}