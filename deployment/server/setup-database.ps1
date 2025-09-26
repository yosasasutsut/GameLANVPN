# Database Setup Script for GameLANVPN Server

param(
    [Parameter(Mandatory=$false)]
    [string]$DatabasePath = "C:\GameLANVPN\Data",

    [Parameter(Mandatory=$false)]
    [string]$BackupPath = "C:\GameLANVPN\Backups"
)

Write-Host "🗃️ GameLANVPN Database Setup" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green

# Create directories
Write-Host "📁 Creating directories..." -ForegroundColor Yellow
if (-not (Test-Path $DatabasePath)) {
    New-Item -ItemType Directory -Path $DatabasePath -Force | Out-Null
    Write-Host "✅ Database directory created: $DatabasePath" -ForegroundColor Green
}

if (-not (Test-Path $BackupPath)) {
    New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null
    Write-Host "✅ Backup directory created: $BackupPath" -ForegroundColor Green
}

# Set permissions
Write-Host "🔒 Setting permissions..." -ForegroundColor Yellow
try {
    $acl = Get-Acl $DatabasePath
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS","FullControl","ContainerInherit,ObjectInherit","None","Allow")
    $acl.SetAccessRule($accessRule)
    $acl | Set-Acl $DatabasePath
    Write-Host "✅ Permissions set for IIS_IUSRS" -ForegroundColor Green
} catch {
    Write-Warning "⚠️ Failed to set IIS permissions. You may need to do this manually."
}

# Create database backup script
Write-Host "📋 Creating backup script..." -ForegroundColor Yellow
$backupScript = @"
@echo off
echo Creating database backup...

set TIMESTAMP=%date:~-4,4%%date:~-10,2%%date:~-7,2%_%time:~0,2%%time:~3,2%%time:~6,2%
set TIMESTAMP=%TIMESTAMP: =0%

copy "$DatabasePath\gamelanvpn.db" "$BackupPath\gamelanvpn_backup_%TIMESTAMP%.db"

echo Backup completed: gamelanvpn_backup_%TIMESTAMP%.db
pause
"@

$backupScript | Out-File -FilePath "$DatabasePath\backup-database.bat" -Encoding ASCII

# Create database maintenance script
$maintenanceScript = @"
# GameLANVPN Database Maintenance Script

param(
    [Parameter(Mandatory=`$false)]
    [int]`$RetentionDays = 30
)

Write-Host "🧹 Database Maintenance Started" -ForegroundColor Green

# Clean old backups
Write-Host "Cleaning old backups (older than `$RetentionDays days)..." -ForegroundColor Yellow
Get-ChildItem "$BackupPath\gamelanvpn_backup_*.db" | Where-Object { `$_.CreationTime -lt (Get-Date).AddDays(-`$RetentionDays) } | Remove-Item -Force
Write-Host "✅ Old backups cleaned" -ForegroundColor Green

# Create current backup
Write-Host "Creating new backup..." -ForegroundColor Yellow
`$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
Copy-Item "$DatabasePath\gamelanvpn.db" "$BackupPath\gamelanvpn_backup_`$timestamp.db"
Write-Host "✅ Backup created: gamelanvpn_backup_`$timestamp.db" -ForegroundColor Green

# Check database integrity (requires SQLite CLI)
if (Test-Path "sqlite3.exe") {
    Write-Host "Checking database integrity..." -ForegroundColor Yellow
    `$result = & sqlite3.exe "$DatabasePath\gamelanvpn.db" "PRAGMA integrity_check;"
    if (`$result -eq "ok") {
        Write-Host "✅ Database integrity check passed" -ForegroundColor Green
    } else {
        Write-Host "❌ Database integrity issues found: `$result" -ForegroundColor Red
    }
}

Write-Host "🎉 Maintenance completed" -ForegroundColor Green
"@

$maintenanceScript | Out-File -FilePath "$DatabasePath\maintenance.ps1" -Encoding UTF8

# Create scheduled task for maintenance
Write-Host "⏰ Creating scheduled maintenance task..." -ForegroundColor Yellow
try {
    $action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-File `"$DatabasePath\maintenance.ps1`""
    $trigger = New-ScheduledTaskTrigger -Weekly -DaysOfWeek Sunday -At 2am
    $settings = New-ScheduledTaskSettingsSet -RunOnlyIfNetworkAvailable -WakeToRun
    $principal = New-ScheduledTaskPrincipal -UserID "NT AUTHORITY\SYSTEM" -LogonType ServiceAccount -RunLevel Highest

    Register-ScheduledTask -TaskName "GameLANVPN-DatabaseMaintenance" -Action $action -Trigger $trigger -Settings $settings -Principal $principal -Description "Weekly maintenance for GameLANVPN database"

    Write-Host "✅ Scheduled task created: GameLANVPN-DatabaseMaintenance" -ForegroundColor Green
} catch {
    Write-Warning "⚠️ Failed to create scheduled task: $($_.Exception.Message)"
    Write-Host "💡 You can run maintenance.ps1 manually when needed" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "✅ Database setup completed!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Summary:" -ForegroundColor Cyan
Write-Host "• Database path: $DatabasePath" -ForegroundColor Yellow
Write-Host "• Backup path: $BackupPath" -ForegroundColor Yellow
Write-Host "• Backup script: $DatabasePath\backup-database.bat" -ForegroundColor Yellow
Write-Host "• Maintenance script: $DatabasePath\maintenance.ps1" -ForegroundColor Yellow
Write-Host "• Scheduled maintenance: Every Sunday at 2 AM" -ForegroundColor Yellow
Write-Host ""
Write-Host "💡 To create manual backup: Run backup-database.bat" -ForegroundColor Blue
Write-Host "💡 To run maintenance: Run maintenance.ps1" -ForegroundColor Blue