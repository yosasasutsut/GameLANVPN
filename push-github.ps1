# GameLAN VPN - Quick GitHub Push
Write-Host "🚀 GameLAN VPN - Pushing to GitHub..." -ForegroundColor Cyan

# Step 1: Set remote
Write-Host "[1/3] Setting remote URL..." -ForegroundColor Yellow
try {
    git remote set-url origin https://github.com/yosasasutsut/GameLANVPN.git 2>$null
    if ($LASTEXITCODE -ne 0) {
        git remote add origin https://github.com/yosasasutsut/GameLANVPN.git
    }
    Write-Host "✅ Remote configured" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to set remote" -ForegroundColor Red
    exit 1
}

# Step 2: Push main branch
Write-Host "[2/3] Pushing main branch..." -ForegroundColor Yellow
git push -u origin main
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Main branch pushed" -ForegroundColor Green
} else {
    Write-Host "❌ Push failed! Create repository first: https://github.com/new" -ForegroundColor Red
    Start-Process "https://github.com/new"
    Read-Host "Press Enter after creating repository"
    git push -u origin main
}

# Step 3: Push tag
Write-Host "[3/3] Pushing version tag..." -ForegroundColor Yellow
git push origin v0.1.0
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Version tag pushed" -ForegroundColor Green
} else {
    Write-Host "❌ Tag push failed" -ForegroundColor Red
}

# Success
Write-Host "`n🎉 SUCCESS! GameLAN VPN v0.1.0 is now on GitHub!" -ForegroundColor Green
Write-Host "📍 Repository: https://github.com/yosasasutsut/GameLANVPN" -ForegroundColor Cyan
Write-Host "🔧 Actions: https://github.com/yosasasutsut/GameLANVPN/actions" -ForegroundColor Cyan

# Open repository
$response = Read-Host "`nOpen repository in browser? (y/n)"
if ($response -eq 'y') {
    Start-Process "https://github.com/yosasasutsut/GameLANVPN"
}