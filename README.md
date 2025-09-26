# GameLAN VPN - Virtual LAN Gaming Solution

## 📝 Project Overview
GameLAN VPN เป็น Windows Desktop Application ที่ทำหน้าที่เป็น Virtual Private Network สำหรับเล่นเกมส์ LAN ระหว่างผู้เล่นที่อยู่ต่างสถานที่กัน รองรับเกมส์ เช่น Warcraft III, Red Alert 2: Yuri's Revenge และเกมส์ LAN อื่นๆ

## 🏗️ System Architecture

### Components
1. **Client Application** - WPF Desktop App สำหรับผู้เล่น
2. **Server Hub** - SignalR Server สำหรับจัดการ rooms และ relay packets
3. **Virtual Network Adapter** - จำลอง LAN network ผ่าน Internet

## 🚀 Getting Started

### Prerequisites
- Windows 10/11
- .NET 8.0 SDK
- Visual Studio 2022
- Git

### Installation Steps

1. Clone repository:
```bash
git clone https://github.com/yourusername/GameLANVPN.git
cd GameLANVPN
```

2. Build solution:
```bash
dotnet build
```

3. Run server:
```bash
cd src/Server/GameLANVPN.Server
dotnet run
```

4. Run client:
```bash
cd src/Client/GameLANVPN.UI
dotnet run
```

## 📋 Development Roadmap

### Phase 1: Core Infrastructure ✅
- [x] Project structure setup
- [x] Basic networking interfaces
- [x] SignalR Hub implementation
- [x] UI framework

### Phase 2: Network Implementation 🚧
- [ ] Virtual adapter integration
- [ ] Packet capture and injection
- [ ] NAT traversal (UDP hole punching)
- [ ] Encryption layer

### Phase 3: Room Management
- [ ] Room creation/joining
- [ ] Player management
- [ ] Host migration
- [ ] Room persistence

### Phase 4: Game Support
- [ ] Warcraft III protocol support
- [ ] Red Alert 2 protocol support
- [ ] Game detection system
- [ ] Auto-configuration

### Phase 5: Performance & Features
- [ ] Latency optimization
- [ ] Bandwidth management
- [ ] Chat system
- [ ] Statistics dashboard

## 🛠️ Technical Details

### Network Protocol
- **Transport**: UDP for game packets, TCP for control
- **Encryption**: AES-256 for packet encryption
- **Virtual IP Range**: 10.x.x.x subnet per room

### Packet Flow
1. Game sends LAN broadcast/unicast packet
2. Virtual adapter captures packet
3. Client encrypts and tunnels to server
4. Server relays to target players
5. Target client decrypts and injects to local network

## 🔧 Configuration

### Client Settings
```json
{
  "ServerUrl": "https://your-server.com",
  "DefaultPort": 5000,
  "EncryptionEnabled": true,
  "AutoConnect": false
}
```

### Supported Games
| Game | Protocol | Port | Status |
|------|----------|------|--------|
| Warcraft III | UDP | 6112-6119 | Planned |
| Red Alert 2 | IPX/UDP | 1234 | Planned |
| Counter-Strike 1.6 | UDP | 27015 | Planned |

## 📦 Dependencies
- SharpPcap - Packet capture
- PacketDotNet - Packet parsing
- SignalR - Real-time communication
- MaterialDesignThemes - UI components
- CommunityToolkit.Mvvm - MVVM framework

## 🤝 Contributing
1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

## ⚠️ Important Notes

### Network Adapter Requirements
- Application requires administrator privileges
- May need to install TAP-Windows adapter
- Windows Firewall exceptions may be needed

### Security Considerations
- All traffic is encrypted by default
- Room codes are randomly generated
- No data logging on server

## 📝 License
This project is licensed under the MIT License

## 📞 Support
For issues and questions, please create an issue on GitHub