# GameLANVPN Server Deployment Guide

This folder contains everything needed to deploy GameLANVPN Server on Windows Server.

## 🚀 Quick Start

### Prerequisites
- Windows Server 2019/2022
- Administrator privileges
- Internet connection

### Installation Steps

1. **Run Installation Script**
   ```powershell
   # Run as Administrator
   .\install.ps1
   ```

2. **Setup Database**
   ```powershell
   .\setup-database.ps1
   ```

3. **Deploy Application**
   ```powershell
   .\deploy.ps1
   ```

4. **Access Web Interface**
   - Open browser: `http://your-server-ip:5000`
   - Register new users
   - Monitor server status

## 📁 File Structure

```
deployment/server/
├── install.ps1           # Main installation script
├── deploy.ps1           # Application deployment script
├── setup-database.ps1   # Database setup and maintenance
└── README.md           # This file
```

## 🛠️ Installation Scripts

### install.ps1
Main installation script that:
- Installs .NET 8 SDK via Chocolatey
- Configures IIS and ASP.NET Core Hosting Bundle
- Creates application directories
- Sets up Windows Firewall rules
- Creates service installation scripts
- Generates configuration files

**Parameters:**
- `-Domain` : Server domain/IP (default: localhost)
- `-Port` : HTTP port (default: 5000)
- `-DatabasePath` : Database directory (default: C:\GameLANVPN\Data)
- `-InstallPath` : Installation directory (default: C:\GameLANVPN)

**Example:**
```powershell
.\install.ps1 -Domain "gameserver.example.com" -Port 80 -InstallPath "D:\GameLANVPN"
```

### deploy.ps1
Deployment script that:
- Builds the application from source
- Publishes to installation directory
- Manages Windows Service
- Updates configuration

**Parameters:**
- `-InstallPath` : Installation directory (default: C:\GameLANVPN)
- `-SourcePath` : Source code path (default: ..\..\src\Server)

### setup-database.ps1
Database management script that:
- Creates database and backup directories
- Sets up permissions for IIS
- Creates backup and maintenance scripts
- Schedules automatic maintenance

**Parameters:**
- `-DatabasePath` : Database directory (default: C:\GameLANVPN\Data)
- `-BackupPath` : Backup directory (default: C:\GameLANVPN\Backups)

## 🗃️ Database Management

### Automatic Backups
- Scheduled weekly on Sundays at 2 AM
- Keeps backups for 30 days
- Manual backup: Run `backup-database.bat`

### Manual Maintenance
```powershell
# Run database maintenance
cd C:\GameLANVPN\Data
.\maintenance.ps1
```

## 🌐 Web Interface Features

### User Registration
- Username validation (3-50 characters, alphanumeric + underscore/dash)
- Email validation
- Password requirements (minimum 6 characters)
- Real-time username availability check

### Server Monitoring
- Server status indicator
- User count statistics
- Active rooms display
- Real-time updates

### API Endpoints

**Authentication:**
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `GET /api/auth/check/{username}` - Check username availability

**Server Info:**
- `GET /api/stats` - Server statistics
- `GET /api/rooms` - Active rooms
- `GET /health` - Health check

## 🔧 Configuration

### appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=C:\\GameLANVPN\\Data\\GameLANVPN.db"
  },
  "GameLANVPN": {
    "ServerUrl": "https://your-domain.com",
    "Port": 5000,
    "MaxRooms": 100,
    "MaxPlayersPerRoom": 16
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "C:\\GameLANVPN\\Logs\\log-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

### IIS Configuration (web.config)
```xml
<configuration>
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
</configuration>
```

## 🔥 Firewall Rules

The installation script automatically creates these firewall rules:
- **GameLANVPN HTTP**: Port 5000 (or custom port)
- **GameLANVPN HTTPS**: Port 443

## 🛡️ Security Considerations

### Database Security
- SQLite database with proper file permissions
- Regular automated backups
- IIS_IUSRS has access to database directory

### Web Security
- HTTPS recommended for production
- CORS configured for client origins
- Input validation on all API endpoints
- Password hashing with salt

### Network Security
- Configure firewall rules appropriately
- Use reverse proxy (IIS/Nginx) for HTTPS termination
- Consider VPN for administrative access

## 📊 Monitoring & Logs

### Log Locations
- **Application Logs**: `C:\GameLANVPN\Logs\`
- **IIS Logs**: `C:\inetpub\logs\LogFiles\`
- **Windows Event Log**: Applications and Services > GameLANVPN

### Health Monitoring
- Health check endpoint: `/health`
- SignalR hub status: `/gamehub`
- Database connectivity monitoring

## 🔄 Updates & Maintenance

### Updating the Application
1. Run `deploy.ps1` to build and deploy latest version
2. Service automatically restarts
3. Database migrations applied automatically

### Regular Maintenance
- Weekly database maintenance (automated)
- Log rotation (automated)
- Monitor disk space
- Review security logs

## 🆘 Troubleshooting

### Common Issues

**Service won't start:**
- Check logs in `C:\GameLANVPN\Logs\`
- Verify database permissions
- Ensure .NET 8 runtime is installed

**Web interface not accessible:**
- Check firewall rules
- Verify IIS configuration
- Check application logs

**Database errors:**
- Run database integrity check
- Restore from backup if needed
- Check disk space

### Support Commands
```powershell
# Check service status
Get-Service "GameLANVPN"

# View recent logs
Get-Content "C:\GameLANVPN\Logs\log-*.txt" -Tail 50

# Test database connection
sqlite3 "C:\GameLANVPN\Data\GameLANVPN.db" ".tables"
```

## 📞 Support

For issues and support:
1. Check application logs first
2. Review this documentation
3. Check GitHub repository for updates
4. Create issue with detailed logs

---

**GameLANVPN Server v1.0** - Virtual LAN Gaming Made Easy