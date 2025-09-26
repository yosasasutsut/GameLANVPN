using Microsoft.Win32;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace GameLANVPN.Core.Games;

public class GameDetector
{
    private readonly ILogger<GameDetector>? _logger;

    public GameDetector(ILogger<GameDetector>? logger = null)
    {
        _logger = logger;
    }

    public List<GameInfo> DetectInstalledGames()
    {
        var games = new List<GameInfo>();

        try
        {
            games.AddRange(DetectWarcraftIII());
            games.AddRange(DetectRedAlert2());
            games.AddRange(DetectCounterStrike());
            games.AddRange(DetectAgeOfEmpires());
            games.AddRange(DetectStarcraft());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error detecting games");
        }

        return games;
    }

    private List<GameInfo> DetectWarcraftIII()
    {
        var games = new List<GameInfo>();
        var paths = new[]
        {
            @"C:\Program Files (x86)\Warcraft III",
            @"C:\Program Files\Warcraft III",
            @"C:\Games\Warcraft III",
            GetRegistryPath(@"SOFTWARE\Blizzard Entertainment\Warcraft III", "InstallPath"),
            GetRegistryPath(@"SOFTWARE\WOW6432Node\Blizzard Entertainment\Warcraft III", "InstallPath")
        };

        foreach (var path in paths.Where(p => !string.IsNullOrEmpty(p)))
        {
            var exePath = Path.Combine(path!, "Warcraft III.exe");
            if (File.Exists(exePath))
            {
                games.Add(new GameInfo
                {
                    Name = "Warcraft III",
                    ExecutablePath = exePath,
                    WorkingDirectory = path!,
                    RequiredPorts = new[] { 6112, 6113, 6114, 6115, 6116, 6117, 6118, 6119 },
                    Protocol = NetworkProtocol.UDP,
                    LaunchArguments = "-opengl"
                });
                break;
            }

            var frozenThroneExe = Path.Combine(path!, "Frozen Throne.exe");
            if (File.Exists(frozenThroneExe))
            {
                games.Add(new GameInfo
                {
                    Name = "Warcraft III: The Frozen Throne",
                    ExecutablePath = frozenThroneExe,
                    WorkingDirectory = path!,
                    RequiredPorts = new[] { 6112, 6113, 6114, 6115, 6116, 6117, 6118, 6119 },
                    Protocol = NetworkProtocol.UDP,
                    LaunchArguments = "-opengl"
                });
                break;
            }
        }

        return games;
    }

    private List<GameInfo> DetectRedAlert2()
    {
        var games = new List<GameInfo>();
        var paths = new[]
        {
            @"C:\Program Files (x86)\EA Games\Command & Conquer Red Alert 2",
            @"C:\Program Files\EA Games\Command & Conquer Red Alert 2",
            @"C:\Games\Red Alert 2",
            GetRegistryPath(@"SOFTWARE\Westwood\Red Alert 2", "InstallPath"),
            GetRegistryPath(@"SOFTWARE\WOW6432Node\Westwood\Red Alert 2", "InstallPath")
        };

        foreach (var path in paths.Where(p => !string.IsNullOrEmpty(p)))
        {
            var exePath = Path.Combine(path!, "RA2.exe");
            if (File.Exists(exePath))
            {
                games.Add(new GameInfo
                {
                    Name = "Command & Conquer: Red Alert 2",
                    ExecutablePath = exePath,
                    WorkingDirectory = path!,
                    RequiredPorts = new[] { 1234, 8086 },
                    Protocol = NetworkProtocol.IPX,
                    LaunchArguments = ""
                });
            }

            var yuriExe = Path.Combine(path!, "Yuri.exe");
            if (File.Exists(yuriExe))
            {
                games.Add(new GameInfo
                {
                    Name = "Command & Conquer: Yuri's Revenge",
                    ExecutablePath = yuriExe,
                    WorkingDirectory = path!,
                    RequiredPorts = new[] { 1234, 8086 },
                    Protocol = NetworkProtocol.IPX,
                    LaunchArguments = ""
                });
            }
        }

        return games;
    }

    private List<GameInfo> DetectCounterStrike()
    {
        var games = new List<GameInfo>();
        var steamPath = GetSteamPath();

        if (!string.IsNullOrEmpty(steamPath))
        {
            var csPath = Path.Combine(steamPath, "steamapps", "common", "Half-Life", "cstrike");
            var exePath = Path.Combine(csPath, "hl.exe");

            if (File.Exists(exePath))
            {
                games.Add(new GameInfo
                {
                    Name = "Counter-Strike 1.6",
                    ExecutablePath = exePath,
                    WorkingDirectory = Path.Combine(steamPath, "steamapps", "common", "Half-Life"),
                    RequiredPorts = new[] { 27015, 27005 },
                    Protocol = NetworkProtocol.UDP,
                    LaunchArguments = "-game cstrike"
                });
            }
        }

        return games;
    }

    private List<GameInfo> DetectAgeOfEmpires()
    {
        var games = new List<GameInfo>();
        var paths = new[]
        {
            @"C:\Program Files (x86)\Microsoft Games\Age of Empires II",
            @"C:\Program Files\Microsoft Games\Age of Empires II",
            GetRegistryPath(@"SOFTWARE\Microsoft\Microsoft Games\Age of Empires II: The Conquerors Expansion", "EXE Path")
        };

        foreach (var path in paths.Where(p => !string.IsNullOrEmpty(p)))
        {
            var exePath = Path.Combine(path!, "empires2.exe");
            if (File.Exists(exePath))
            {
                games.Add(new GameInfo
                {
                    Name = "Age of Empires II",
                    ExecutablePath = exePath,
                    WorkingDirectory = path!,
                    RequiredPorts = new[] { 2300, 2301, 2302, 2303 },
                    Protocol = NetworkProtocol.UDP
                });
                break;
            }
        }

        return games;
    }

    private List<GameInfo> DetectStarcraft()
    {
        var games = new List<GameInfo>();
        var paths = new[]
        {
            @"C:\Program Files (x86)\StarCraft",
            @"C:\Program Files\StarCraft",
            GetRegistryPath(@"SOFTWARE\Blizzard Entertainment\Starcraft", "InstallPath")
        };

        foreach (var path in paths.Where(p => !string.IsNullOrEmpty(p)))
        {
            var exePath = Path.Combine(path!, "StarCraft.exe");
            if (File.Exists(exePath))
            {
                games.Add(new GameInfo
                {
                    Name = "StarCraft",
                    ExecutablePath = exePath,
                    WorkingDirectory = path!,
                    RequiredPorts = new[] { 6112, 6113, 6114 },
                    Protocol = NetworkProtocol.UDP
                });
                break;
            }
        }

        return games;
    }

    public async Task LaunchGameAsync(GameInfo gameInfo)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = gameInfo.ExecutablePath,
                Arguments = gameInfo.LaunchArguments ?? "",
                WorkingDirectory = gameInfo.WorkingDirectory,
                UseShellExecute = true
            };

            var process = Process.Start(startInfo);
            _logger?.LogInformation("Launched game: {GameName} (PID: {ProcessId})",
                gameInfo.Name, process?.Id);

            // Wait a moment for the game to start
            await Task.Delay(2000);

            if (process != null && !process.HasExited)
            {
                _logger?.LogInformation("Game {GameName} started successfully", gameInfo.Name);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to launch game: {GameName}", gameInfo.Name);
            throw;
        }
    }

    private string? GetRegistryPath(string keyPath, string valueName)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath) ??
                            Registry.CurrentUser.OpenSubKey(keyPath);
            return key?.GetValue(valueName)?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private string? GetSteamPath()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam") ??
                            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam");
            return key?.GetValue("InstallPath")?.ToString();
        }
        catch
        {
            return null;
        }
    }
}

public class GameInfo
{
    public string Name { get; set; } = "";
    public string ExecutablePath { get; set; } = "";
    public string WorkingDirectory { get; set; } = "";
    public int[] RequiredPorts { get; set; } = Array.Empty<int>();
    public NetworkProtocol Protocol { get; set; } = NetworkProtocol.UDP;
    public string? LaunchArguments { get; set; }
    public bool IsInstalled => File.Exists(ExecutablePath);
}

public enum NetworkProtocol
{
    UDP,
    TCP,
    IPX
}