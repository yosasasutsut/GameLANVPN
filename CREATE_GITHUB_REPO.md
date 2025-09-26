# วิธีสร้าง GitHub Repository

## ขั้นตอนที่ 1: สร้าง Repository บน GitHub
1. ไปที่ https://github.com/new
2. Repository name: `GameLANVPN`
3. Description: `Virtual LAN Gaming Solution - Play LAN games over the Internet`
4. เลือก `Public` หรือ `Private` ตามต้องการ
5. ❌ **ไม่ต้องติ๊ก** "Add a README file"
6. ❌ **ไม่ต้องติ๊ก** "Add .gitignore"
7. ❌ **ไม่ต้องติ๊ก** "Choose a license"
8. คลิก **"Create repository"**

## ขั้นตอนที่ 2: Push โค้ดขึ้น GitHub
เมื่อสร้าง repository เสร็จแล้ว ให้รันคำสั่งนี้:

```bash
cd GameLANVPN
git remote set-url origin https://github.com/yosasasutsut/GameLANVPN.git
git push -u origin main
git push origin v0.1.0
```

## หรือใช้ SSH (ถ้า setup SSH key แล้ว):
```bash
cd GameLANVPN
git remote set-url origin git@github.com:yosasasutsut/GameLANVPN.git
git push -u origin main
git push origin v0.1.0
```

## ขั้นตอนที่ 3: ตรวจสอบ
- GitHub Actions จะทำงานอัตโนมัติเมื่อ push
- Release v0.1.0 จะถูกสร้างอัตโนมัติ
- ตรวจสอบที่: https://github.com/yosasasutsut/GameLANVPN/actions

## สถานะปัจจุบัน
✅ โค้ดพร้อม push (commit: 5e3632a)
✅ Tag v0.1.0 พร้อมแล้ว
⏳ รอสร้าง GitHub repository

## Version 0.1.0 Features
- ✅ Virtual LAN networking
- ✅ Room management system
- ✅ SignalR real-time communication
- ✅ Packet capture and relay
- ✅ WPF UI with Material Design
- ✅ Docker deployment ready
- ✅ CI/CD pipelines configured