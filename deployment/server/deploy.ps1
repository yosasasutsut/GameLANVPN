# GameLANVPN Server Deployment Script
# This script builds and deploys the server to the installation directory

param(
    [Parameter(Mandatory=$false)]
    [string]$InstallPath = "C:\GameLANVPN",

    [Parameter(Mandatory=$false)]
    [string]$SourcePath = "..\..\src\Server"
)

Write-Host "🚀 GameLANVPN Server Deployment" -ForegroundColor Green
Write-Host "==============================" -ForegroundColor Green

# Check if source path exists
if (-not (Test-Path $SourcePath)) {
    Write-Error "❌ Source path not found: $SourcePath"
    exit 1
}

# Stop service if running
Write-Host "⏹️ Stopping GameLANVPN service..." -ForegroundColor Yellow
try {
    Stop-Service "GameLANVPN" -ErrorAction SilentlyContinue
    Write-Host "✅ Service stopped" -ForegroundColor Green
} catch {
    Write-Host "ℹ️ Service was not running" -ForegroundColor Blue
}

# Build the application
Write-Host "🔨 Building application..." -ForegroundColor Yellow
Set-Location $SourcePath\GameLANVPN.Server
dotnet publish -c Release -o $InstallPath --self-contained false

if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Build failed!"
    exit 1
}

Write-Host "✅ Application built successfully" -ForegroundColor Green

# Create publish info file
$publishInfo = @{
    "DeploymentDate" = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    "Version" = "1.0.0"
    "BuildConfiguration" = "Release"
    "TargetFramework" = "net8.0"
} | ConvertTo-Json -Depth 2

$publishInfo | Out-File -FilePath "$InstallPath\publish-info.json" -Encoding UTF8

# Start service
Write-Host "▶️ Starting GameLANVPN service..." -ForegroundColor Yellow
try {
    Start-Service "GameLANVPN" -ErrorAction Stop
    Write-Host "✅ Service started successfully" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Service failed to start. Check logs for details." -ForegroundColor Red
    Write-Host "💡 You may need to run install-service.bat first" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "✅ Deployment completed!" -ForegroundColor Green
Write-Host "📁 Deployed to: $InstallPath" -ForegroundColor Yellow