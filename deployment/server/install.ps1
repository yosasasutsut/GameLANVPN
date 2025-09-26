# GameLANVPN Server Installation Script for Windows Server
# Run as Administrator

param(
    [Parameter(Mandatory=$false)]
    [string]$Domain = "localhost",

    [Parameter(Mandatory=$false)]
    [int]$Port = 5000,

    [Parameter(Mandatory=$false)]
    [string]$DatabasePath = "C:\GameLANVPN\Data",

    [Parameter(Mandatory=$false)]
    [string]$InstallPath = "C:\GameLANVPN"
)

Write-Host "🎮 GameLANVPN Server Installation Script" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "❌ This script must be run as Administrator!"
    exit 1
}

# Function to check if a command exists
function Test-Command {
    param($Command)
    try {
        Get-Command $Command -ErrorAction Stop
        return $true
    } catch {
        return $false
    }
}

# Install Chocolatey if not present
if (-not (Test-Command choco)) {
    Write-Host "📦 Installing Chocolatey..." -ForegroundColor Yellow
    Set-ExecutionPolicy Bypass -Scope Process -Force
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
    iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
    refreshenv
}

# Install .NET 8 SDK
if (-not (Test-Command dotnet)) {
    Write-Host "🔧 Installing .NET 8 SDK..." -ForegroundColor Yellow
    choco install dotnet-8.0-sdk -y
    refreshenv
} else {
    Write-Host "✅ .NET SDK is already installed" -ForegroundColor Green
}

# Install IIS and ASP.NET Core Hosting Bundle
Write-Host "🌐 Installing IIS and ASP.NET Core Hosting Bundle..." -ForegroundColor Yellow
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole, IIS-WebServer, IIS-CommonHttpFeatures, IIS-HttpErrors, IIS-HttpLogging, IIS-RequestFiltering, IIS-StaticContent, IIS-DefaultDocument, IIS-DirectoryBrowsing, IIS-ASPNET45 -All

# Download and install ASP.NET Core Hosting Bundle
$hostingBundleUrl = "https://download.visualstudio.microsoft.com/download/pr/8f990fa6-6b13-4c42-a3c2-8df31c3c8e52/f5b3d03080a3a5b4a60e1b9a8c6d2aef/dotnet-hosting-8.0.11-win.exe"
$hostingBundlePath = "$env:TEMP\dotnet-hosting-bundle.exe"
Invoke-WebRequest -Uri $hostingBundleUrl -OutFile $hostingBundlePath
Start-Process -FilePath $hostingBundlePath -ArgumentList "/quiet" -Wait
Remove-Item $hostingBundlePath

# Create installation directory
Write-Host "📁 Creating installation directory..." -ForegroundColor Yellow
if (-not (Test-Path $InstallPath)) {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
}

if (-not (Test-Path $DatabasePath)) {
    New-Item -ItemType Directory -Path $DatabasePath -Force | Out-Null
}

# Create appsettings.Production.json
Write-Host "⚙️ Creating configuration file..." -ForegroundColor Yellow
$appSettings = @{
    "ConnectionStrings" = @{
        "DefaultConnection" = "Data Source=$DatabasePath\GameLANVPN.db"
    }
    "Logging" = @{
        "LogLevel" = @{
            "Default" = "Information"
            "Microsoft.AspNetCore" = "Warning"
        }
    }
    "AllowedHosts" = "*"
    "Serilog" = @{
        "Using" = @("Serilog.Sinks.Console", "Serilog.Sinks.File")
        "MinimumLevel" = "Information"
        "WriteTo" = @(
            @{ "Name" = "Console" },
            @{
                "Name" = "File"
                "Args" = @{
                    "path" = "$InstallPath\Logs\log-.txt"
                    "rollingInterval" = "Day"
                }
            }
        )
    }
    "GameLANVPN" = @{
        "ServerUrl" = "https://$Domain"
        "Port" = $Port
        "MaxRooms" = 100
        "MaxPlayersPerRoom" = 16
    }
}

$appSettings | ConvertTo-Json -Depth 10 | Out-File -FilePath "$InstallPath\appsettings.Production.json" -Encoding UTF8

# Create web.config for IIS
Write-Host "🌐 Creating IIS configuration..." -ForegroundColor Yellow
$webConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\GameLANVPN.Server.dll"
                  stdoutLogEnabled="false"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
"@

$webConfig | Out-File -FilePath "$InstallPath\web.config" -Encoding UTF8

# Create Windows Service installation script
Write-Host "⚙️ Creating service installation script..." -ForegroundColor Yellow
$serviceScript = @"
@echo off
echo Installing GameLANVPN Server as Windows Service...

sc create "GameLANVPN" binPath= "dotnet `"$InstallPath\GameLANVPN.Server.dll`"" start= auto
sc description "GameLANVPN" "GameLANVPN Server - Virtual LAN Gaming Service"
sc start "GameLANVPN"

echo Service installed and started successfully!
pause
"@

$serviceScript | Out-File -FilePath "$InstallPath\install-service.bat" -Encoding ASCII

# Create uninstall script
$uninstallScript = @"
@echo off
echo Uninstalling GameLANVPN Server Service...

sc stop "GameLANVPN"
sc delete "GameLANVPN"

echo Service uninstalled successfully!
pause
"@

$uninstallScript | Out-File -FilePath "$InstallPath\uninstall-service.bat" -Encoding ASCII

# Create firewall rules
Write-Host "🔥 Creating firewall rules..." -ForegroundColor Yellow
New-NetFirewallRule -DisplayName "GameLANVPN HTTP" -Direction Inbound -Protocol TCP -LocalPort $Port -Action Allow -ErrorAction SilentlyContinue
New-NetFirewallRule -DisplayName "GameLANVPN HTTPS" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow -ErrorAction SilentlyContinue

# Create logs directory
if (-not (Test-Path "$InstallPath\Logs")) {
    New-Item -ItemType Directory -Path "$InstallPath\Logs" -Force | Out-Null
}

Write-Host ""
Write-Host "✅ Installation completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Next steps:" -ForegroundColor Cyan
Write-Host "1. Copy the published GameLANVPN.Server files to: $InstallPath"
Write-Host "2. Run install-service.bat to install as Windows Service"
Write-Host "3. Configure IIS site pointing to: $InstallPath"
Write-Host "4. Access web interface at: http://$Domain`:$Port"
Write-Host ""
Write-Host "📁 Installation Path: $InstallPath" -ForegroundColor Yellow
Write-Host "🗃️ Database Path: $DatabasePath" -ForegroundColor Yellow
Write-Host "🌐 Server URL: http://$Domain`:$Port" -ForegroundColor Yellow
Write-Host ""
Write-Host "🔧 Configuration file: $InstallPath\appsettings.Production.json"
Write-Host "📝 Logs location: $InstallPath\Logs"