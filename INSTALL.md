# 🎮 GameLAN VPN Server Installation

## 🚀 Super Quick Install (1 Command!)

### Option 1: Using Docker (Easiest - Recommended)
```bash
# Clone and run with Docker
git clone https://github.com/yosasasutsut/GameLANVPN.git && cd GameLANVPN && docker-compose up -d
```

✅ **That's it!** Server is now running at http://localhost

### Option 2: Using Installation Script
```bash
# Download and run installer
curl -sSL https://raw.githubusercontent.com/yosasasutsut/GameLANVPN/main/quick-setup.sh | bash
```

### Option 3: Manual Installation
```bash
# 1. Clone repository
git clone https://github.com/yosasasutsut/GameLANVPN.git
cd GameLANVPN

# 2. Build and run
cd src/Server/GameLANVPN.Server
dotnet run
```

---

## 📋 System Requirements

### Minimum:
- **RAM:** 512MB
- **CPU:** 1 Core
- **Storage:** 1GB
- **OS:** Linux/Windows/macOS
- **.NET:** 8.0 Runtime

### Recommended:
- **RAM:** 2GB
- **CPU:** 2 Cores
- **Storage:** 10GB
- **OS:** Ubuntu 22.04 LTS

---

## 🔧 Configuration

### Default Ports:
- **80** - Web Interface & API
- **5000** - Alternative HTTP port

### Environment Variables:
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5000
ConnectionStrings__DefaultConnection=Data Source=gamelanvpn.db
```

---

## 🎯 Quick Test

### 1. Check Health
```bash
curl http://localhost/health
```

### 2. Register User
```bash
curl -X POST http://localhost/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@example.com","password":"Test123!","confirmPassword":"Test123!"}'
```

### 3. Login
```bash
curl -X POST http://localhost/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"Test123!"}'
```

---

## 🛠️ Management Commands

### Docker Commands:
```bash
# Start server
docker-compose up -d

# Stop server
docker-compose down

# View logs
docker-compose logs -f

# Restart server
docker-compose restart

# Update server
git pull && docker-compose up -d --build
```

### Direct Commands:
```bash
# Start server
dotnet run

# Run in background
nohup dotnet run > server.log 2>&1 &

# Stop server
pkill -f GameLANVPN.Server
```

---

## 🌐 Access Points

- **Web Interface:** http://localhost
- **API Documentation:** http://localhost/api
- **Health Check:** http://localhost/health
- **Server Info:** http://localhost/info

---

## 📱 Client Connection

### From Windows Client:
1. Open GameLAN VPN Demo
2. Enter server: `http://your-server-ip`
3. Login with registered account

### API Endpoints:
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `GET /api/rooms` - List game rooms
- `WS /gamehub` - WebSocket game hub

---

## 🔒 Security Setup (Optional)

### Enable HTTPS with Let's Encrypt:
```bash
# Install certbot
sudo apt install certbot

# Get SSL certificate
sudo certbot certonly --standalone -d yourdomain.com

# Update docker-compose.yml to use SSL
```

### Firewall Rules:
```bash
# Allow HTTP/HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
```

---

## 🆘 Troubleshooting

### Server not starting?
```bash
# Check logs
docker-compose logs
# or
journalctl -u gamelanvpn -n 50
```

### Port already in use?
```bash
# Change port in docker-compose.yml
ports:
  - "8080:5000"  # Change 8080 to any free port
```

### Database issues?
```bash
# Reset database
rm data/gamelanvpn.db
docker-compose restart
```

---

## 📞 Support

- **GitHub Issues:** https://github.com/yosasasutsut/GameLANVPN/issues
- **Documentation:** https://github.com/yosasasutsut/GameLANVPN/wiki

---

## ✨ Features

✅ User Registration & Authentication
✅ Game Room Creation
✅ Virtual LAN Support
✅ Real-time Chat
✅ SQLite Database
✅ Health Monitoring
✅ Docker Support
✅ Auto-restart on Failure

---

Made with ❤️ for gamers!