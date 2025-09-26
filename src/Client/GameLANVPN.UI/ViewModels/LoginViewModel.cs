using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace GameLANVPN.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LoginViewModel> _logger;
    private readonly IConfiguration _configuration;

    [ObservableProperty]
    private string _serverUrl = "http://localhost:5000";

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberServer = true;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private Brush _statusColor = Brushes.Black;

    [ObservableProperty]
    private bool _isLoading = false;

    public event Action? LoginSuccessful;

    public LoginViewModel()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        // Setup logging (simplified for this example)
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<LoginViewModel>();

        // Load configuration
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables();
        _configuration = configBuilder.Build();

        LoadSettings();
    }

    [RelayCommand]
    private async Task Login()
    {
        if (IsLoading) return;

        if (string.IsNullOrWhiteSpace(ServerUrl) ||
            string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ShowError("Please fill in all fields");
            return;
        }

        IsLoading = true;
        ShowStatus("Connecting to server...", Brushes.Blue);

        try
        {
            var loginData = new
            {
                username = Username.Trim(),
                password = Password
            };

            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var serverUrl = ServerUrl.TrimEnd('/');
            var response = await _httpClient.PostAsync($"{serverUrl}/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginResponse != null && loginResponse.User != null)
                {
                    ShowSuccess($"Welcome, {loginResponse.User.Username}!");

                    // Store server URL if remember is checked
                    if (RememberServer)
                    {
                        SaveSettings();
                    }

                    // Store user session information
                    UserSession.Current = new UserSession
                    {
                        UserId = loginResponse.User.Id,
                        Username = loginResponse.User.Username,
                        Email = loginResponse.User.Email,
                        ServerUrl = ServerUrl,
                        LoginTime = DateTime.Now
                    };

                    _logger.LogInformation("User logged in successfully: {Username}", Username);

                    await Task.Delay(1000); // Brief delay to show success message

                    // Trigger successful login event
                    LoginSuccessful?.Invoke();
                }
                else
                {
                    ShowError("Invalid server response");
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ShowError("Invalid username or password");
            }
            else
            {
                ShowError($"Server error: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            ShowError($"Cannot connect to server: {ex.Message}");
            _logger.LogError(ex, "HTTP request failed during login");
        }
        catch (TaskCanceledException)
        {
            ShowError("Connection timeout. Please check server address.");
        }
        catch (Exception ex)
        {
            ShowError($"Login failed: {ex.Message}");
            _logger.LogError(ex, "Unexpected error during login");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Register()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(ServerUrl))
            {
                var serverUrl = ServerUrl.TrimEnd('/');
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = serverUrl,
                    UseShellExecute = true
                });
            }
            else
            {
                ShowError("Please enter server URL first");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Cannot open browser: {ex.Message}");
        }
    }

    private void LoadSettings()
    {
        try
        {
            var savedServerUrl = _configuration["ServerSettings:DefaultServerUrl"];
            if (!string.IsNullOrEmpty(savedServerUrl))
            {
                ServerUrl = savedServerUrl;
            }

            RememberServer = _configuration.GetValue("ServerSettings:RememberServer", true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load settings");
        }
    }

    private void SaveSettings()
    {
        try
        {
            // In a real application, you would save to appsettings.json or user settings
            // For now, we'll just log the intention
            _logger.LogInformation("Would save server URL: {ServerUrl}", ServerUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save settings");
        }
    }

    private void ShowStatus(string message, Brush color)
    {
        StatusMessage = message;
        StatusColor = color;
    }

    private void ShowError(string message)
    {
        ShowStatus(message, Brushes.Red);
    }

    private void ShowSuccess(string message)
    {
        ShowStatus(message, Brushes.Green);
    }
}

// Response models
public class LoginResponse
{
    public string? Message { get; set; }
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? LastLogin { get; set; }
}

// User session management
public class UserSession
{
    public static UserSession? Current { get; set; }

    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
}