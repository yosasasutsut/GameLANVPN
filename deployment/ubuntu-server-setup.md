# GameLAN VPN - Ubuntu Server Setup Guide

## 📋 System Requirements
- **Ubuntu Server**: 20.04 LTS หรือ 22.04 LTS (แนะนำ)
- **RAM**: ขั้นต่ำ 4GB (แนะนำ 8GB)
- **Storage**: ขั้นต่ำ 20GB
- **CPU**: 2 cores ขึ้นไป
- **Network**: Static IP และเปิด ports ที่จำเป็น

## 🚀 ขั้นตอนการติดตั้ง

### Step 1: Update System
```bash
sudo apt update && sudo apt upgrade -y
sudo apt install -y curl wget git nano ufw
```

### Step 2: ติดตั้ง Docker และ Docker Compose
```bash
# ติดตั้ง Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER

# ติดตั้ง Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Verify installation
docker --version
docker-compose --version

# Re-login หรือ run command นี้
newgrp docker
```

### Step 3: ติดตั้ง .NET 8.0 SDK (Optional - สำหรับ development)
```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET 8.0
sudo apt update
sudo apt install -y dotnet-sdk-8.0
```

### Step 4: Clone Project และ Setup
```bash
# สร้าง directory สำหรับ application
sudo mkdir -p /opt/gamelanvpn
sudo chown $USER:$USER /opt/gamelanvpn
cd /opt/gamelanvpn

# Clone repository
git clone https://github.com/yourusername/GameLANVPN.git .
# หรือ upload files ผ่าน SCP/SFTP

# สร้าง .env file
cp .env.example .env
nano .env
```

### Step 5: แก้ไข Environment Variables
```bash
# แก้ไขค่าเหล่านี้ใน .env file
DB_PASSWORD=StrongPassword123!
REDIS_PASSWORD=RedisStrongPass456!
JWT_SECRET=your-256-bit-secret-key-change-this
SERVER_URL=https://your-domain.com  # หรือ IP ของ server
GRAFANA_PASSWORD=GrafanaAdmin789!
```

### Step 6: Setup Firewall
```bash
# เปิด SSH port
sudo ufw allow 22/tcp

# เปิด ports สำหรับ GameLAN VPN
sudo ufw allow 5000/tcp  # HTTP API
sudo ufw allow 5001/tcp  # HTTPS API
sudo ufw allow 3000/tcp  # Grafana (optional)

# เปิด game ports
sudo ufw allow 6112:6119/udp  # Warcraft III
sudo ufw allow 27015/udp      # Counter-Strike
sudo ufw allow 1234/udp       # Red Alert 2

# Enable firewall
sudo ufw --force enable
sudo ufw status
```

### Step 7: Run Application ด้วย Docker Compose
```bash
cd /opt/gamelanvpn

# Start all services
docker-compose up -d

# ตรวจสอบ status
docker-compose ps

# ดู logs
docker-compose logs -f
```

### Step 8: Setup SSL Certificate (สำหรับ Production)

#### Option A: ใช้ Let's Encrypt
```bash
# ติดตั้ง Certbot
sudo apt install -y certbot python3-certbot-nginx nginx

# Stop docker containers ชั่วคราว
docker-compose down

# Generate certificate
sudo certbot certonly --standalone -d your-domain.com

# Copy certificates
sudo cp /etc/letsencrypt/live/your-domain.com/fullchain.pem /opt/gamelanvpn/certs/
sudo cp /etc/letsencrypt/live/your-domain.com/privkey.pem /opt/gamelanvpn/certs/
sudo chown $USER:$USER /opt/gamelanvpn/certs/*

# Start containers again
docker-compose up -d
```

#### Option B: ใช้ Nginx Reverse Proxy
```bash
# ติดตั้ง Nginx
sudo apt install -y nginx

# สร้าง config file
sudo nano /etc/nginx/sites-available/gamelanvpn
```

เพิ่ม configuration:
```nginx
server {
    listen 80;
    server_name your-domain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name your-domain.com;

    ssl_certificate /etc/letsencrypt/live/your-domain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/your-domain.com/privkey.pem;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /gamehub {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
    }
}
```

Enable site:
```bash
sudo ln -s /etc/nginx/sites-available/gamelanvpn /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

### Step 9: Setup Auto-start on Boot
```bash
# Enable Docker to start on boot
sudo systemctl enable docker

# Create systemd service
sudo nano /etc/systemd/system/gamelanvpn.service
```

เพิ่ม content:
```ini
[Unit]
Description=GameLAN VPN Server
After=docker.service
Requires=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=/opt/gamelanvpn
ExecStart=/usr/local/bin/docker-compose up -d
ExecStop=/usr/local/bin/docker-compose down
TimeoutStartSec=0

[Install]
WantedBy=multi-user.target
```

Enable service:
```bash
sudo systemctl daemon-reload
sudo systemctl enable gamelanvpn
```

## 📊 Monitoring และ Maintenance

### ดู Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f gamelanvpn-server
docker-compose logs -f gamelanvpn-db
```

### Backup Database
```bash
# Create backup script
nano /opt/gamelanvpn/backup.sh
```

```bash
#!/bin/bash
BACKUP_DIR="/opt/gamelanvpn/backups"
mkdir -p $BACKUP_DIR
DATE=$(date +%Y%m%d_%H%M%S)

# Backup PostgreSQL
docker exec gamelanvpn-db pg_dump -U gamelanvpn gamelanvpn > $BACKUP_DIR/db_backup_$DATE.sql

# Keep only last 7 days
find $BACKUP_DIR -name "*.sql" -mtime +7 -delete
```

```bash
chmod +x /opt/gamelanvpn/backup.sh

# Add to crontab (daily at 2 AM)
crontab -e
# Add: 0 2 * * * /opt/gamelanvpn/backup.sh
```

### Health Check
```bash
# Check services status
curl http://localhost:5000/health

# Check Docker containers
docker-compose ps

# Check resource usage
docker stats --no-stream
```

## 🔒 Security Best Practices

1. **ใช้ Strong Passwords**: เปลี่ยน passwords ทั้งหมดใน .env
2. **Update System**:
   ```bash
   sudo apt update && sudo apt upgrade -y
   ```
3. **Fail2ban**: ติดตั้งเพื่อป้องกัน brute force
   ```bash
   sudo apt install fail2ban -y
   sudo systemctl enable fail2ban
   ```
4. **Limit SSH Access**:
   ```bash
   # Disable password authentication
   sudo nano /etc/ssh/sshd_config
   # Set: PasswordAuthentication no
   sudo systemctl restart sshd
   ```

## 🛠️ Troubleshooting

### Problem: Container ไม่ start
```bash
# Check logs
docker-compose logs gamelanvpn-server

# Restart containers
docker-compose restart

# Rebuild if needed
docker-compose build --no-cache
docker-compose up -d
```

### Problem: Database connection failed
```bash
# Check PostgreSQL container
docker exec -it gamelanvpn-db psql -U gamelanvpn

# Reset database
docker-compose down -v
docker-compose up -d
```

### Problem: Port already in use
```bash
# Find process using port
sudo lsof -i :5000
sudo kill -9 <PID>
```

## 📞 Service Management Commands

```bash
# Start services
cd /opt/gamelanvpn && docker-compose up -d

# Stop services
cd /opt/gamelanvpn && docker-compose down

# Restart services
cd /opt/gamelanvpn && docker-compose restart

# View logs
cd /opt/gamelanvpn && docker-compose logs -f

# Update application
cd /opt/gamelanvpn
git pull
docker-compose build --no-cache
docker-compose up -d
```

## 🔗 Access Points

- **API Server**: http://your-server-ip:5000
- **SignalR Hub**: http://your-server-ip:5000/gamehub
- **Grafana Dashboard**: http://your-server-ip:3000
  - Default login: admin / [password from .env]

## 📌 Notes
- Server ต้องมี Static IP หรือ Domain Name
- สำหรับ Production ควรใช้ HTTPS เสมอ
- Monitor disk space สำหรับ logs และ database
- ทำ backup database เป็นประจำ