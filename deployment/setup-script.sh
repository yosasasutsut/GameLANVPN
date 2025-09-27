#!/bin/bash

# GameLAN VPN - Ubuntu Server Setup Script
# This script automates the installation process

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}===============================================${NC}"
echo -e "${GREEN}   GameLAN VPN Server - Auto Setup Script${NC}"
echo -e "${GREEN}===============================================${NC}"

# Check if running as root
if [ "$EUID" -eq 0 ]; then
   echo -e "${RED}Please do not run this script as root!${NC}"
   exit 1
fi

# Function to print status
print_status() {
    echo -e "${YELLOW}[*] $1${NC}"
}

print_success() {
    echo -e "${GREEN}[✓] $1${NC}"
}

print_error() {
    echo -e "${RED}[✗] $1${NC}"
}

# Step 1: Update system
print_status "Updating system packages..."
sudo apt update && sudo apt upgrade -y
sudo apt install -y curl wget git nano ufw
print_success "System updated"

# Step 2: Install Docker
if ! command -v docker &> /dev/null; then
    print_status "Installing Docker..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker $USER
    rm get-docker.sh
    print_success "Docker installed"
else
    print_success "Docker already installed"
fi

# Step 3: Install Docker Compose
if ! command -v docker-compose &> /dev/null; then
    print_status "Installing Docker Compose..."
    sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
    print_success "Docker Compose installed"
else
    print_success "Docker Compose already installed"
fi

# Step 4: Create application directory
print_status "Setting up application directory..."
sudo mkdir -p /opt/gamelanvpn
sudo chown $USER:$USER /opt/gamelanvpn
print_success "Application directory created"

# Step 5: Get application files
cd /opt/gamelanvpn
if [ ! -f "docker-compose.yml" ]; then
    print_status "Please provide the GitHub repository URL (or press Enter to skip):"
    read -r REPO_URL

    if [ ! -z "$REPO_URL" ]; then
        git clone "$REPO_URL" .
        print_success "Repository cloned"
    else
        print_error "No repository provided. Please manually copy files to /opt/gamelanvpn"
        echo "Required files: docker-compose.yml, .env.example, and application source"
        exit 1
    fi
fi

# Step 6: Setup environment file
if [ -f ".env.example" ] && [ ! -f ".env" ]; then
    print_status "Creating .env file..."
    cp .env.example .env

    # Generate random passwords
    DB_PASS=$(openssl rand -base64 32)
    REDIS_PASS=$(openssl rand -base64 32)
    JWT_SECRET=$(openssl rand -base64 64)
    GRAFANA_PASS=$(openssl rand -base64 16)

    # Update .env with generated passwords
    sed -i "s/changeMe123!/$DB_PASS/g" .env
    sed -i "s/redisPass123!/$REDIS_PASS/g" .env
    sed -i "s/your-super-secret-jwt-key-change-this-in-production/$JWT_SECRET/g" .env
    sed -i "s/GRAFANA_PASSWORD=admin/GRAFANA_PASSWORD=$GRAFANA_PASS/g" .env

    print_success ".env file created with secure passwords"
    echo -e "${YELLOW}IMPORTANT: Save these credentials:${NC}"
    echo "Database Password: $DB_PASS"
    echo "Redis Password: $REDIS_PASS"
    echo "Grafana Password: $GRAFANA_PASS"
    echo ""
    echo "Please update SERVER_URL in .env with your domain or IP address"
fi

# Step 7: Setup firewall
print_status "Configuring firewall..."
sudo ufw allow 22/tcp
sudo ufw allow 5000/tcp
sudo ufw allow 5001/tcp
sudo ufw allow 3000/tcp
sudo ufw allow 6112:6119/udp
sudo ufw allow 27015/udp
sudo ufw allow 1234/udp
sudo ufw --force enable
print_success "Firewall configured"

# Step 8: Create systemd service
print_status "Creating systemd service..."
sudo tee /etc/systemd/system/gamelanvpn.service > /dev/null <<EOF
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
User=$USER

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl daemon-reload
sudo systemctl enable gamelanvpn
print_success "Systemd service created"

# Step 9: Create backup script
print_status "Creating backup script..."
mkdir -p /opt/gamelanvpn/backups
cat > /opt/gamelanvpn/backup.sh <<'EOF'
#!/bin/bash
BACKUP_DIR="/opt/gamelanvpn/backups"
mkdir -p $BACKUP_DIR
DATE=$(date +%Y%m%d_%H%M%S)

# Backup PostgreSQL
docker exec gamelanvpn-db pg_dump -U gamelanvpn gamelanvpn > $BACKUP_DIR/db_backup_$DATE.sql

# Keep only last 7 days
find $BACKUP_DIR -name "*.sql" -mtime +7 -delete

echo "Backup completed: db_backup_$DATE.sql"
EOF

chmod +x /opt/gamelanvpn/backup.sh
print_success "Backup script created"

# Step 10: Start services
print_status "Starting GameLAN VPN services..."
cd /opt/gamelanvpn

# Need to be in docker group for this session
newgrp docker <<EONG
docker-compose up -d
EONG

print_success "Services started"

# Final instructions
echo ""
echo -e "${GREEN}===============================================${NC}"
echo -e "${GREEN}   Installation Complete!${NC}"
echo -e "${GREEN}===============================================${NC}"
echo ""
echo "Access points:"
echo "  - API Server: http://$(hostname -I | awk '{print $1}'):5000"
echo "  - Grafana: http://$(hostname -I | awk '{print $1}'):3000"
echo ""
echo "Next steps:"
echo "  1. Edit /opt/gamelanvpn/.env to set your SERVER_URL"
echo "  2. Restart services: cd /opt/gamelanvpn && docker-compose restart"
echo "  3. Check logs: cd /opt/gamelanvpn && docker-compose logs -f"
echo ""
echo "To add daily backups, run:"
echo "  (crontab -l 2>/dev/null; echo '0 2 * * * /opt/gamelanvpn/backup.sh') | crontab -"
echo ""
echo -e "${YELLOW}NOTE: You may need to log out and back in for docker permissions to take effect${NC}"