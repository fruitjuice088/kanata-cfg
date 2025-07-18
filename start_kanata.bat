@echo off
chcp 65001 >nul

set KANATA_EXE=%OneDrive%\command\apps\kanata.exe
set CONFIG_FILE=%USERPROFILE%\.config\kanata\main.kbd
tasklist | findstr /i kanata.exe >nul
if %ERRORLEVEL% EQU 0 (
    echo kanata is already running. Exiting...
    timeout /t 3 /nobreak >nul
    exit /b 1
)

"%KANATA_EXE%" -n --cfg "%CONFIG_FILE%"
