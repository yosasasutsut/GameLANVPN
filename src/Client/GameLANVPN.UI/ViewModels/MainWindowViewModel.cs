using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameLANVPN.Core.Network;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;

namespace GameLANVPN.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly INetworkManager _networkManager;
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly System.Timers.Timer _refreshTimer;

    [ObservableProperty]
    private string _connectionStatus = "Disconnected";

    [ObservableProperty]
    private string _connectionIcon = "Connection";

    [ObservableProperty]
    private string _roomCode = "";

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private RoomInfo? _currentRoom;

    [ObservableProperty]
    private RoomInfo? _selectedRoom;

    [ObservableProperty]
    private int _latency = 0;

    [ObservableProperty]
    private double _uploadSpeed = 0;

    [ObservableProperty]
    private double _downloadSpeed = 0;

    public ObservableCollection<RoomInfo> Rooms { get; } = new();

    private string _serverUrl = "http://localhost:5000";

    public MainWindowViewModel(INetworkManager networkManager, ILogger<MainWindowViewModel> logger)
    {
        _networkManager = networkManager;
        _logger = logger;

        _networkManager.ConnectionStateChanged += OnConnectionStateChanged;
        _networkManager.PacketReceived += OnPacketReceived;

        _refreshTimer = new System.Timers.Timer(5000);
        _refreshTimer.Elapsed += async (s, e) => await RefreshRooms();
        _refreshTimer.Start();

        _ = Task.Run(ConnectToServerAsync);
    }

    [RelayCommand]
    private async Task ConnectToServer()
    {
        StatusMessage = "Connecting to server...";

        try
        {
            var serverEndpoint = new IPEndPoint(IPAddress.Loopback, 5000);
            var success = await _networkManager.ConnectToServerAsync(serverEndpoint, "");

            if (success)
            {
                StatusMessage = "Connected to server";
                await RefreshRooms();
            }
            else
            {
                StatusMessage = "Failed to connect to server";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Connection error: {ex.Message}";
            _logger.LogError(ex, "Connection failed");
        }
    }

    [RelayCommand]
    private async Task CreateRoom()
    {
        try
        {
            var dialog = new CreateRoomDialog();
            if (dialog.ShowDialog() == true)
            {
                StatusMessage = "Creating room...";

                var roomInfo = await _networkManager.CreateRoomAsync(dialog.GameName, dialog.MaxPlayers);

                if (roomInfo != null)
                {
                    CurrentRoom = roomInfo;
                    RoomCode = roomInfo.RoomCode;
                    StatusMessage = $"Room created: {roomInfo.RoomCode}";

                    await RefreshRooms();
                }
                else
                {
                    StatusMessage = "Failed to create room";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating room: {ex.Message}";
            _logger.LogError(ex, "Failed to create room");
        }
    }

    [RelayCommand]
    private async Task JoinRoom()
    {
        if (string.IsNullOrWhiteSpace(RoomCode) && SelectedRoom == null)
        {
            StatusMessage = "Please enter a room code or select a room";
            return;
        }

        try
        {
            StatusMessage = "Joining room...";

            var roomCodeToJoin = !string.IsNullOrWhiteSpace(RoomCode) ? RoomCode : SelectedRoom!.RoomCode;
            var success = await _networkManager.JoinRoomAsync(roomCodeToJoin);

            if (success)
            {
                CurrentRoom = SelectedRoom ?? new RoomInfo { RoomCode = roomCodeToJoin };
                StatusMessage = $"Joined room: {roomCodeToJoin}";
            }
            else
            {
                StatusMessage = "Failed to join room";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error joining room: {ex.Message}";
            _logger.LogError(ex, "Failed to join room");
        }
    }

    [RelayCommand]
    private async Task LeaveRoom()
    {
        try
        {
            StatusMessage = "Leaving room...";

            await _networkManager.DisconnectAsync();

            CurrentRoom = null;
            RoomCode = "";
            StatusMessage = "Left room";

            await RefreshRooms();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error leaving room: {ex.Message}";
            _logger.LogError(ex, "Failed to leave room");
        }
    }

    [RelayCommand]
    private async Task StartGame()
    {
        if (CurrentRoom == null) return;

        try
        {
            StatusMessage = "Starting game...";

            var gameDetector = new GameDetector();
            var detectedGames = gameDetector.DetectInstalledGames();

            var matchingGame = detectedGames.FirstOrDefault(g =>
                g.Name.Equals(CurrentRoom.GameName, StringComparison.OrdinalIgnoreCase));

            if (matchingGame != null)
            {
                await gameDetector.LaunchGameAsync(matchingGame);
                StatusMessage = $"Game launched: {matchingGame.Name}";
            }
            else
            {
                StatusMessage = $"Game not found: {CurrentRoom.GameName}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error starting game: {ex.Message}";
            _logger.LogError(ex, "Failed to start game");
        }
    }

    [RelayCommand]
    private void Settings()
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.ShowDialog();
    }

    private async Task RefreshRooms()
    {
        try
        {
            if (!_networkManager.IsConnected) return;

            var rooms = await _networkManager.GetAvailableRoomsAsync();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Rooms.Clear();
                foreach (var room in rooms)
                {
                    Rooms.Add(room);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh rooms");
        }
    }

    private void OnConnectionStateChanged(object? sender, ConnectionStateEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ConnectionStatus = e.IsConnected ? "Connected" : "Disconnected";
            ConnectionIcon = e.IsConnected ? "Wifi" : "WifiOff";

            if (!string.IsNullOrEmpty(e.Message))
            {
                StatusMessage = e.Message;
            }
        });
    }

    private void OnPacketReceived(object? sender, PacketReceivedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            DownloadSpeed = CalculateSpeed(e.Data.Length, true);
            UpdateLatency();
        });
    }

    private double CalculateSpeed(int bytes, bool isDownload)
    {
        // Simple speed calculation - should be improved with proper averaging
        return bytes / 1024.0; // KB/s
    }

    private void UpdateLatency()
    {
        // Simulate latency calculation - should be implemented properly
        Latency = new Random().Next(10, 100);
    }
}

// Helper classes
public class CreateRoomDialog : Window
{
    public string GameName { get; set; } = "Warcraft III";
    public int MaxPlayers { get; set; } = 8;

    public CreateRoomDialog()
    {
        Title = "Create Room";
        Width = 300;
        Height = 200;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        // Create simple dialog UI
        var stackPanel = new StackPanel { Margin = new Thickness(20) };

        stackPanel.Children.Add(new System.Windows.Controls.Label { Content = "Game Name:" });
        var gameNameTextBox = new System.Windows.Controls.TextBox { Text = GameName };
        stackPanel.Children.Add(gameNameTextBox);

        stackPanel.Children.Add(new System.Windows.Controls.Label { Content = "Max Players:" });
        var maxPlayersTextBox = new System.Windows.Controls.TextBox { Text = MaxPlayers.ToString() };
        stackPanel.Children.Add(maxPlayersTextBox);

        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 20, 0, 0) };

        var okButton = new System.Windows.Controls.Button { Content = "OK", Margin = new Thickness(0, 0, 10, 0), Padding = new Thickness(20, 5, 20, 5) };
        okButton.Click += (s, e) => {
            GameName = gameNameTextBox.Text;
            if (int.TryParse(maxPlayersTextBox.Text, out int players))
                MaxPlayers = players;
            DialogResult = true;
        };

        var cancelButton = new System.Windows.Controls.Button { Content = "Cancel", Padding = new Thickness(20, 5, 20, 5) };
        cancelButton.Click += (s, e) => DialogResult = false;

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        stackPanel.Children.Add(buttonPanel);

        Content = stackPanel;
    }
}

public class SettingsWindow : Window
{
    public SettingsWindow()
    {
        Title = "Settings";
        Width = 400;
        Height = 300;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        Content = new System.Windows.Controls.TextBlock
        {
            Text = "Settings will be implemented here",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }
}