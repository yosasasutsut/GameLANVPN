#!/bin/bash
#=================================================================
# GameLAN VPN Server - Easy Installation Script
# One-line installation: curl -sSL https://yourdomain.com/install.sh | bash
#=================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
INSTALL_DIR="/opt/gamelanvpn"
SERVICE_NAME="gamelanvpn"
SERVICE_USER="gamelanvpn"
DOTNET_VERSION="8.0"

# ASCII Art Logo
print_logo() {
    echo -e "${BLUE}"
    echo "╔═══════════════════════════════════════════╗"
    echo "║      GameLAN VPN Server Installer         ║"
    echo "║           Quick & Easy Setup               ║"
    echo "╚═══════════════════════════════════════════╝"
    echo -e "${NC}"
}

# Print colored messages
print_message() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
    exit 1
}

print_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

# Check if running as root
check_root() {
    if [[ $EUID -ne 0 ]]; then
        print_error "This script must be run as root (use sudo)"
    fi
}

# Detect OS
detect_os() {
    if [[ -f /etc/os-release ]]; then
        . /etc/os-release
        OS=$ID
        OS_VERSION=$VERSION_ID
    else
        print_error "Cannot detect OS"
    fi

    print_message "Detected OS: $OS $OS_VERSION"
}

# Install prerequisites
install_prerequisites() {
    print_message "Installing prerequisites..."

    case $OS in
        ubuntu|debian)
            apt-get update
            apt-get install -y wget curl git sqlite3 nginx certbot python3-certbot-nginx
            ;;
        centos|rhel|fedora)
            yum install -y wget curl git sqlite nginx certbot python3-certbot-nginx
            ;;
        *)
            print_error "Unsupported OS: $OS"
            ;;
    esac
}

# Install .NET 8
install_dotnet() {
    print_message "Installing .NET $DOTNET_VERSION..."

    if command -v dotnet &> /dev/null; then
        print_warning ".NET is already installed"
    else
        wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
        chmod +x dotnet-install.sh
        ./dotnet-install.sh --version latest --runtime aspnetcore
        rm dotnet-install.sh

        # Add to PATH
        echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
        export PATH=$PATH:$HOME/.dotnet
    fi
}

# Download and build server
setup_server() {
    print_message "Setting up GameLAN VPN Server..."

    # Create user
    if ! id "$SERVICE_USER" &>/dev/null; then
        useradd -r -s /bin/false $SERVICE_USER
    fi

    # Create directory
    mkdir -p $INSTALL_DIR
    cd $INSTALL_DIR

    # Clone repository
    if [[ -d "GameLANVPN" ]]; then
        print_warning "Repository already exists, pulling latest..."
        cd GameLANVPN
        git pull
    else
        git clone https://github.com/yosasasutsut/GameLANVPN.git
        cd GameLANVPN
    fi

    # Build server
    print_message "Building server..."
    cd src/Server/GameLANVPN.Server
    dotnet publish -c Release -o $INSTALL_DIR/server

    # Set permissions
    chown -R $SERVICE_USER:$SERVICE_USER $INSTALL_DIR
    chmod +x $INSTALL_DIR/server/GameLANVPN.Server
}

# Create systemd service
create_service() {
    print_message "Creating systemd service..."

    cat > /etc/systemd/system/$SERVICE_NAME.service << EOL
[Unit]
Description=GameLAN VPN Server
After=network.target

[Service]
Type=exec
User=$SERVICE_USER
WorkingDirectory=$INSTALL_DIR/server
ExecStart=$INSTALL_DIR/server/GameLANVPN.Server
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=$SERVICE_NAME
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="ASPNETCORE_URLS=http://localhost:5000"

[Install]
WantedBy=multi-user.target
EOL

    systemctl daemon-reload
    systemctl enable $SERVICE_NAME
}

# Configure nginx reverse proxy
configure_nginx() {
    print_message "Configuring Nginx..."

    read -p "Enter your domain name (e.g., vpn.yourdomain.com): " DOMAIN

    cat > /etc/nginx/sites-available/$SERVICE_NAME << EOL
server {
    listen 80;
    server_name $DOMAIN;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_cache_bypass \$http_upgrade;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }

    location /gamehub {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host \$host;
        proxy_cache_bypass \$http_upgrade;
    }
}
EOL

    ln -sf /etc/nginx/sites-available/$SERVICE_NAME /etc/nginx/sites-enabled/
    nginx -t && systemctl reload nginx

    # Setup SSL
    read -p "Do you want to setup SSL with Let's Encrypt? (y/n): " SETUP_SSL
    if [[ "$SETUP_SSL" == "y" ]]; then
        certbot --nginx -d $DOMAIN
    fi
}

# Configure firewall
configure_firewall() {
    print_message "Configuring firewall..."

    if command -v ufw &> /dev/null; then
        ufw allow 80/tcp
        ufw allow 443/tcp
        ufw allow 5000/tcp
        print_message "Firewall configured (UFW)"
    elif command -v firewall-cmd &> /dev/null; then
        firewall-cmd --permanent --add-service=http
        firewall-cmd --permanent --add-service=https
        firewall-cmd --permanent --add-port=5000/tcp
        firewall-cmd --reload
        print_message "Firewall configured (firewalld)"
    else
        print_warning "No firewall detected, please configure manually"
    fi
}

# Start service
start_service() {
    print_message "Starting GameLAN VPN Server..."
    systemctl start $SERVICE_NAME

    sleep 3

    if systemctl is-active --quiet $SERVICE_NAME; then
        print_message "Server started successfully!"
    else
        print_error "Failed to start server. Check: journalctl -u $SERVICE_NAME -n 50"
    fi
}

# Print summary
print_summary() {
    echo ""
    echo -e "${GREEN}═══════════════════════════════════════════════${NC}"
    echo -e "${GREEN}   Installation Completed Successfully! 🎮      ${NC}"
    echo -e "${GREEN}═══════════════════════════════════════════════${NC}"
    echo ""
    echo "Service Status: systemctl status $SERVICE_NAME"
    echo "View Logs:     journalctl -u $SERVICE_NAME -f"
    echo "Restart:       systemctl restart $SERVICE_NAME"
    echo "Stop:          systemctl stop $SERVICE_NAME"
    echo ""
    echo "Server URL:    http://localhost:5000"
    if [[ -n "$DOMAIN" ]]; then
        echo "Public URL:    https://$DOMAIN"
    fi
    echo ""
    echo "API Endpoints:"
    echo "  Registration: /api/auth/register"
    echo "  Login:       /api/auth/login"
    echo "  Game Hub:    /gamehub"
    echo ""
}

# Main installation flow
main() {
    print_logo
    check_root
    detect_os

    print_message "Starting installation..."
    install_prerequisites
    install_dotnet
    setup_server
    create_service

    read -p "Do you want to configure Nginx reverse proxy? (y/n): " SETUP_NGINX
    if [[ "$SETUP_NGINX" == "y" ]]; then
        configure_nginx
    fi

    configure_firewall
    start_service
    print_summary
}

# Run main function
main "$@"