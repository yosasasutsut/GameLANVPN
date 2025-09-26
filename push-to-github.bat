@echo off
echo ========================================
echo  GameLAN VPN - Push to GitHub v0.1.0
echo ========================================

echo [1/4] Setting remote URL...
git remote set-url origin https://github.com/yosasasutsut/GameLANVPN.git
if %errorlevel% neq 0 (
    echo Adding remote origin...
    git remote add origin https://github.com/yosasasutsut/GameLANVPN.git
)

echo [2/4] Pushing main branch...
git push -u origin main
if %errorlevel% neq 0 (
    echo Push failed! Check if repository exists and you have access.
    pause
    exit /b 1
)

echo [3/4] Pushing version tag...
git push origin v0.1.0
if %errorlevel% neq 0 (
    echo Tag push failed!
    pause
    exit /b 1
)

echo [4/4] Opening repository...
start https://github.com/yosasasutsut/GameLANVPN

echo.
echo ========================================
echo  SUCCESS! Repository pushed to GitHub
echo ========================================
echo.
echo Repository: https://github.com/yosasasutsut/GameLANVPN
echo Actions:    https://github.com/yosasasutsut/GameLANVPN/actions
echo Releases:   https://github.com/yosasasutsut/GameLANVPN/releases
echo.
pause