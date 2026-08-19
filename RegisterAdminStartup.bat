@echo off
:: ==========================================================
:: สคริปต์ช่วยเพิ่ม AutoUpdateApp ลงใน Startup แบบใช้สิทธิ์ Admin
:: ==========================================================
NET SESSION >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] Please right-click and "Run as administrator".
    echo กรุณาคลิกขวาที่ไฟล์นี้แล้วเลือก "Run as administrator"
    pause
    exit /b
)

:: ตั้งชื่อ Task และดึง Path ของโปรแกรมที่ต้องการรัน
set "TASK_NAME=AutoUpdateApp_Startup"
set "APP_PATH=%~dp0AutoUpdateApp.exe"

echo =================================================
echo Registering Auto Update App to Windows Startup...
echo Path: %APP_PATH%
echo =================================================

:: สร้าง Scheduled Task ให้รันตอนผู้ใช้ Log on ด้วยสิทธิ์สูงสุด (Admin)
schtasks /create /tn "%TASK_NAME%" /tr "\"%APP_PATH%\"" /sc onlogon /rl highest /f

if %errorLevel% equ 0 (
    echo.
    echo [SUCCESS] Successfully added to startup!
    echo โปรแกรมจะเปิดอัตโนมัติ (พร้อมสิทธิ์ Admin) ในครั้งหน้าที่เปิดเครื่อง
) else (
    echo.
    echo [FAILED] Failed to create scheduled task.
)

pause
exit /b
