#!/bin/bash
#=================================================================
# GameLAN VPN Server - SUPER EASY INSTALLATION
# Just run: bash quick-setup.sh
#=================================================================

echo "🎮 GameLAN VPN Server - Quick Setup"
echo "===================================="

# Method 1: Docker (Easiest)
if command -v docker &> /dev/null; then
    echo "✅ Docker detected - Using Docker installation (Recommended)"

    # Install docker-compose if not exists
    if ! command -v docker-compose &> /dev/null; then
        echo "📦 Installing docker-compose..."
        sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
        sudo chmod +x /usr/local/bin/docker-compose
    fi

    # Clone repository
    echo "📥 Downloading GameLAN VPN..."
    git clone https://github.com/yosasasutsut/GameLANVPN.git
    cd GameLANVPN

    # Start with Docker
    echo "🚀 Starting server with Docker..."
    docker-compose up -d

    echo ""
    echo "✅ Installation Complete!"
    echo "========================"
    echo "🌐 Server URL: http://localhost"
    echo "📊 Status: docker-compose ps"
    echo "📝 Logs: docker-compose logs -f"
    echo ""

# Method 2: Direct installation
else
    echo "🔧 Docker not found - Using direct installation"

    # Install .NET 8
    echo "📦 Installing .NET 8..."
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh --version 8.0 --runtime aspnetcore
    rm dotnet-install.sh
    export PATH=$PATH:$HOME/.dotnet

    # Clone and build
    echo "📥 Downloading GameLAN VPN..."
    git clone https://github.com/yosasasutsut/GameLANVPN.git
    cd GameLANVPN/src/Server/GameLANVPN.Server

    echo "🔨 Building server..."
    dotnet build -c Release

    # Run server
    echo "🚀 Starting server..."
    dotnet run --urls http://0.0.0.0:80 &

    echo ""
    echo "✅ Installation Complete!"
    echo "========================"
    echo "🌐 Server URL: http://localhost"
    echo "🛑 Stop server: kill $(ps aux | grep 'GameLANVPN.Server' | grep -v grep | awk '{print $2}')"
    echo ""
fi

echo "📚 API Documentation:"
echo "  POST /api/auth/register - Register new user"
echo "  POST /api/auth/login    - User login"
echo "  WS   /gamehub           - Game connection hub"