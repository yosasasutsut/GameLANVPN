# GameLANVPN Client Installer Build Script

param(
    [Parameter(Mandatory=$false)]
    [string]$Version = "1.0.0",

    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release",

    [Parameter(Mandatory=$false)]
    [string]$OutputPath = ".\installer",

    [Parameter(Mandatory=$false)]
    [string]$SourcePath = "..\..\src\Client"
)

Write-Host "🎮 GameLANVPN Client Installer Builder" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green

# Check prerequisites
Write-Host "🔍 Checking prerequisites..." -ForegroundColor Yellow

if (-not (Test-Path $SourcePath)) {
    Write-Error "❌ Source path not found: $SourcePath"
    exit 1
}

# Check for dotnet
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "❌ .NET SDK not found. Please install .NET 8 SDK."
    exit 1
}

# Check for Inno Setup (optional but recommended)
$innoSetupPath = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $innoSetupPath)) {
    Write-Warning "⚠️ Inno Setup not found. Will create portable version only."
    Write-Host "💡 Install Inno Setup from https://jrsoftware.org/isdl.php for full installer"
    $createInstaller = $false
} else {
    $createInstaller = $true
}

# Create output directory
Write-Host "📁 Creating output directory..." -ForegroundColor Yellow
if (Test-Path $OutputPath) {
    Remove-Item -Recurse -Force $OutputPath
}
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

# Build the application
Write-Host "🔨 Building application..." -ForegroundColor Yellow
$publishPath = "$OutputPath\publish"

try {
    # Build Core library
    dotnet publish "$SourcePath\GameLANVPN.Core\GameLANVPN.Core.csproj" `
        -c $Configuration `
        -o "$publishPath\core" `
        --self-contained true `
        -r win-x64

    # Build Client library
    dotnet publish "$SourcePath\GameLANVPN.Client\GameLANVPN.Client.csproj" `
        -c $Configuration `
        -o "$publishPath\client" `
        --self-contained true `
        -r win-x64

    Write-Host "✅ Application built successfully" -ForegroundColor Green
} catch {
    Write-Error "❌ Build failed: $($_.Exception.Message)"
    exit 1
}

# Create portable application package
Write-Host "📦 Creating portable package..." -ForegroundColor Yellow
$portablePath = "$OutputPath\GameLANVPN-Portable-v$Version"
New-Item -ItemType Directory -Path $portablePath -Force | Out-Null

# Copy application files
Copy-Item -Recurse "$publishPath\core\*" $portablePath
Copy-Item -Recurse "$publishPath\client\*" $portablePath -Force

# Create launcher script
$launcherScript = @"
@echo off
title GameLANVPN Client
echo Starting GameLANVPN Client...
echo.

REM Check if .NET 8 runtime is available
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET 8 Runtime is required but not found.
    echo Please install .NET 8 Runtime from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

REM Start the application
start "GameLANVPN" GameLANVPN.Client.exe
echo GameLANVPN Client started successfully!

REM Keep window open for debugging
timeout /t 3 >nul
"@

$launcherScript | Out-File -FilePath "$portablePath\GameLANVPN.bat" -Encoding ASCII

# Create configuration file
$configContent = @{
    "ServerSettings" = @{
        "DefaultServerUrl" = "http://localhost:5000"
        "AutoConnect" = $false
        "RememberServer" = $true
    }
    "ClientSettings" = @{
        "AutoStartWithWindows" = $false
        "MinimizeToTray" = $true
        "CheckForUpdates" = $true
    }
    "Logging" = @{
        "LogLevel" = "Information"
        "LogToFile" = $true
        "LogPath" = "logs"
    }
}

$configContent | ConvertTo-Json -Depth 3 | Out-File -FilePath "$portablePath\appsettings.json" -Encoding UTF8

# Create README file
$readmeContent = @"
# GameLANVPN Client v$Version

## Quick Start
1. Double-click GameLANVPN.bat to start the application
2. Enter your server address (e.g., http://your-server:5000)
3. Login with your registered username and password
4. Join or create game rooms to play with friends

## System Requirements
- Windows 10/11 (64-bit)
- .NET 8 Runtime (will be prompted if missing)
- Administrator privileges (for virtual network adapter)

## Configuration
- Edit appsettings.json to customize settings
- Logs are stored in the 'logs' folder

## Troubleshooting
- Run as Administrator if network features don't work
- Check Windows Firewall settings
- Ensure server is accessible

## Support
For help and updates, visit: https://github.com/yosasasutsut/GameLANVPN

Generated on: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Version: $Version
"@

$readmeContent | Out-File -FilePath "$portablePath\README.txt" -Encoding UTF8

# Create desktop shortcut script
$shortcutScript = @"
@echo off
echo Creating desktop shortcut...

set "TARGET=%~dp0GameLANVPN.bat"
set "SHORTCUT=%USERPROFILE%\Desktop\GameLANVPN.lnk"

powershell -Command "& { `
    `$WshShell = New-Object -comObject WScript.Shell; `
    `$Shortcut = `$WshShell.CreateShortcut('%SHORTCUT%'); `
    `$Shortcut.TargetPath = '%TARGET%'; `
    `$Shortcut.WorkingDirectory = '%~dp0'; `
    `$Shortcut.Description = 'GameLANVPN Client - Virtual LAN Gaming'; `
    `$Shortcut.Save() `
}"

if exist "%SHORTCUT%" (
    echo ✅ Desktop shortcut created successfully!
) else (
    echo ❌ Failed to create desktop shortcut
)

echo.
pause
"@

$shortcutScript | Out-File -FilePath "$portablePath\Create-Desktop-Shortcut.bat" -Encoding ASCII

# Zip portable version
Write-Host "🗜️ Creating portable ZIP archive..." -ForegroundColor Yellow
$zipPath = "$OutputPath\GameLANVPN-Portable-v$Version.zip"
Compress-Archive -Path $portablePath -DestinationPath $zipPath -Force

Write-Host ""
Write-Host "✅ Build completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Output Summary:" -ForegroundColor Cyan
Write-Host "• Portable folder: $portablePath" -ForegroundColor Yellow
Write-Host "• Portable ZIP: $zipPath" -ForegroundColor Yellow

if ($createInstaller) {
    Write-Host "• Full installer: Coming next..." -ForegroundColor Yellow
} else {
    Write-Host "• Install Inno Setup for full installer creation" -ForegroundColor Blue
}

Write-Host ""
Write-Host "🚀 Distribution Instructions:" -ForegroundColor Cyan
Write-Host "1. Upload ZIP file to GitHub Releases" -ForegroundColor White
Write-Host "2. Users download and extract anywhere" -ForegroundColor White
Write-Host "3. Run GameLANVPN.bat to start" -ForegroundColor White
Write-Host "4. Use Create-Desktop-Shortcut.bat for convenience" -ForegroundColor White
Write-Host ""
Write-Host "💡 The portable version requires .NET 8 Runtime on target machines" -ForegroundColor Blue