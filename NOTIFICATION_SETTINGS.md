# 🔕 GitHub Notification Settings

## วิธีลด Email แจ้งเตือนจาก Dependabot

### 1. **Dependabot Settings ที่แก้แล้ว:**
✅ เปลี่ยนจาก `weekly` เป็น `monthly`
✅ ลด PR limit จาก 10 เป็น 3
✅ Group minor/patch updates เป็น 1 PR
✅ ปิด major version updates (ยกเว้น security)
✅ Auto-merge สำหรับ safe updates

### 2. **GitHub Account Settings:**

ไปที่ GitHub Settings เพื่อจัดการ notifications:

1. **https://github.com/settings/notifications**
2. ที่ **Email notification preferences**:
   - ✅ เปิด: `Pull request reviews`
   - ❌ ปิด: `Pull requests` (general)
   - ❌ ปิด: `Issues`
   - ✅ เปิด: `Security alerts`

3. **ที่ Repository level**:
   - ไป: https://github.com/yosasasutsut/GameLANVPN
   - กด **Watch** → **Custom**
   - เลือกเฉพาะ:
     - ✅ Releases
     - ✅ Security alerts
     - ❌ Issues
     - ❌ Pull requests

### 3. **Email Filters (Gmail/Outlook):**

สร้าง filter สำหรับจัดการอีเมลอัตโนมัติ:

**Gmail Filter:**
```
From: notifications@github.com
Subject: [yosasasutsut/GameLANVPN]
Has words: dependabot

Action:
- Skip inbox
- Apply label: "GitHub/Dependabot"
- Mark as read (optional)
```

**Outlook Rule:**
```
If: From contains "notifications@github.com"
And: Subject contains "dependabot"
Then: Move to folder "GitHub/Dependencies"
```

## 🔄 **Auto-merge Behavior**

### จะ Auto-merge:
- ✅ Patch updates (1.0.0 → 1.0.1)
- ✅ Minor updates (1.0.0 → 1.1.0)
- ✅ Security updates
- ✅ หลังผ่าน tests เรียบร้อย

### ต้อง Manual review:
- ⚠️ Major updates (1.0.0 → 2.0.0)
- ⚠️ Breaking changes
- ⚠️ New dependencies

## 📊 **Expected Notifications:**

### เดิม (รัวๆ):
```
📧 Weekly: 5-10 dependency PRs
📧 Daily: PR comments, reviews
📧 Total: ~30-50 emails/week
```

### ตอนนี้ (สงบ):
```
📧 Monthly: 1-3 grouped PRs
📧 Important: Security alerts only
📧 Total: ~5-10 emails/month
```

## 🛠️ **Manual Controls:**

### ปิด Dependabot ชั่วคราว:
```bash
# ใน .github/dependabot.yml
# เพิ่ม # หน้าบรรทัดที่ไม่ต้องการ
```

### Re-enable เมื่อต้องการ:
```bash
# ลบ # ออก
```

### Force update ทันที:
```bash
# ไป GitHub repo → Insights → Dependency graph → Dependabot
# กด "Check for updates"
```

## 📱 **Mobile App Settings:**

**GitHub Mobile App:**
- Settings → Notifications
- Turn off: Pull requests, Issues
- Keep on: Releases, Security alerts

---

**ตอนนี้จะได้รับแจ้งเตือนแค่สิ่งสำคัญเท่านั้น!** 🎉