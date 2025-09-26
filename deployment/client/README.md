# GameLANVPN Client Installation Package

This folder contains everything needed to build and distribute the GameLANVPN client application for end users.

## 🎮 What is GameLANVPN Client?

GameLANVPN Client is a Windows desktop application that allows users to:
- Connect to GameLANVPN servers
- Play LAN games over the internet
- Join virtual gaming rooms
- Create secure connections with other players

## 📦 Package Contents

### Installation Scripts
- **`build-installer.ps1`** - Main build script for creating client packages
- **`installer.iss`** - Inno Setup script for professional installer

### Features
- **Easy Installation** - One-click setup with desktop shortcut
- **Login Interface** - Clean login screen with server connection
- **Automatic Updates** - Built-in update checking capability
- **Material Design UI** - Modern, user-friendly interface

## 🚀 Building the Client Package

### Prerequisites
1. **Windows 10/11** with PowerShell
2. **.NET 8 SDK** installed
3. **Inno Setup** (optional, for professional installer)

### Quick Build
```powershell
# Run as Administrator
.\build-installer.ps1
```

### Custom Build
```powershell
# Build specific version
.\build-installer.ps1 -Version "1.2.0" -Configuration "Release"

# Custom output path
.\build-installer.ps1 -OutputPath "C:\Releases\GameLANVPN"
```

### Build Parameters
- `-Version` : Application version (default: "1.0.0")
- `-Configuration` : Build configuration (default: "Release")
- `-OutputPath` : Where to place build output (default: ".\installer")
- `-SourcePath` : Source code location (default: "..\..\src\Client")

## 📁 Build Output

After running the build script, you'll get:

```
installer/
├── GameLANVPN-Portable-v1.0.0/          # Portable version
│   ├── GameLANVPN.Client.exe             # Main application
│   ├── GameLANVPN.bat                    # Launcher script
│   ├── appsettings.json                  # Configuration
│   ├── Create-Desktop-Shortcut.bat       # Shortcut creator
│   └── README.txt                        # User instructions
├── GameLANVPN-Portable-v1.0.0.zip        # ZIP for distribution
└── GameLANVPN-Setup-v1.0.0.exe          # Full installer (if Inno Setup available)
```

## 💻 Client Application Features

### Login Interface
- **Server Connection** - Enter server URL (e.g., `http://your-server:5000`)
- **User Authentication** - Login with registered credentials
- **Remember Settings** - Save server address for convenience
- **Registration Link** - Direct link to web registration

### Main Application
- **Room Management** - Join/create gaming rooms
- **Player List** - See connected players
- **Game Detection** - Auto-detect installed games
- **Network Status** - Monitor connection quality

### System Integration
- **Desktop Shortcut** - Easy access from desktop
- **Start Menu Entry** - Professional Windows integration
- **Auto-start Option** - Start with Windows (optional)
- **Tray Minimization** - Run in system tray

## 🛠️ Installation Types

### Portable Version
- **No installation required** - Extract and run
- **Self-contained** - All dependencies included
- **USB-friendly** - Run from removable media
- **Manual shortcuts** - User creates own shortcuts

**Usage:**
1. Download ZIP file
2. Extract anywhere
3. Run `GameLANVPN.bat`
4. Use `Create-Desktop-Shortcut.bat` for convenience

### Full Installer (Inno Setup Required)
- **Professional installer** - MSI-style installation
- **Start Menu integration** - Automatic shortcuts
- **Uninstaller** - Clean removal option
- **Registry integration** - Windows-native experience
- **.NET Runtime Check** - Automatic dependency verification

**Features:**
- Checks for .NET 8 Desktop Runtime
- Creates desktop and Start Menu shortcuts
- Optional auto-start with Windows
- Professional uninstall process

## 📋 System Requirements

### Client Requirements
- **OS**: Windows 10/11 (64-bit)
- **Runtime**: .NET 8 Desktop Runtime
- **Memory**: 512MB RAM minimum
- **Disk**: 100MB free space
- **Network**: Internet connection
- **Privileges**: Administrator (for virtual network features)

### Server Requirements
- Valid GameLANVPN server URL
- Registered user account
- Firewall rules allowing client connection

## 🔧 Configuration

### Default Settings (`appsettings.json`)
```json
{
  "ServerSettings": {
    "DefaultServerUrl": "http://localhost:5000",
    "AutoConnect": false,
    "RememberServer": true
  },
  "ClientSettings": {
    "AutoStartWithWindows": false,
    "MinimizeToTray": true,
    "CheckForUpdates": true
  },
  "Logging": {
    "LogLevel": "Information",
    "LogToFile": true,
    "LogPath": "logs"
  }
}
```

### User Customization
Users can modify settings through:
- Configuration file editing
- In-application settings (future feature)
- Command-line parameters

## 🌐 Server Integration

### Authentication Flow
1. User enters server URL
2. Application connects to `/api/auth/login`
3. Validates credentials via REST API
4. Stores session information
5. Connects to SignalR hub for real-time features

### API Endpoints Used
- `POST /api/auth/login` - User authentication
- `GET /api/rooms` - Available gaming rooms
- `GET /api/stats` - Server statistics
- `SignalR /gamehub` - Real-time communication

## 🚀 Distribution Guide

### GitHub Releases
1. Build installer packages
2. Upload to GitHub Releases
3. Tag with version number
4. Include release notes

### Download Instructions for Users
1. Visit GitHub Releases page
2. Download latest installer
3. Run installer as Administrator
4. Follow setup wizard
5. Launch from desktop shortcut

### Auto-Update Strategy
- Check GitHub API for new releases
- Compare version numbers
- Prompt user for updates
- Download and install automatically

## 🐛 Troubleshooting

### Common Issues

**"Application won't start"**
- Install .NET 8 Desktop Runtime
- Run as Administrator
- Check Windows Defender exclusions

**"Can't connect to server"**
- Verify server URL format
- Check firewall settings
- Ensure server is running

**"Login fails"**
- Verify credentials on web interface
- Check internet connection
- Try different server URL

**"Network features don't work"**
- Run as Administrator
- Check Windows firewall
- Install WinPcap/Npcap if needed

### Log Locations
- **Application Logs**: `logs/` folder
- **Windows Event Log**: Application > GameLANVPN
- **Installation Log**: Installer creates log file

## 🔒 Security Considerations

### Client Security
- HTTPS recommended for server connections
- Credential validation before storage
- Secure communication protocols
- No sensitive data in config files

### Network Security
- VPN-style encrypted tunnels
- Player authentication required
- Room-based access control
- Rate limiting and abuse prevention

## 🆕 Future Enhancements

### Planned Features
- In-app settings management
- Multiple server profiles
- Game library integration
- Voice chat support
- Performance monitoring

### Technical Improvements
- Auto-update mechanism
- Better error reporting
- Improved UI animations
- Plugin architecture

---

**GameLANVPN Client v1.0** - Bringing LAN Gaming to the Modern Era

For support and updates: https://github.com/yosasasutsut/GameLANVPN