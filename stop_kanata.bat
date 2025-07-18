@echo off

tasklist | findstr /i kanata.exe >nul
if %ERRORLEVEL% EQU 0 (
    taskkill /f /im kanata.exe 2>nul
    echo kanata stopped successfully.
) else (
    echo kanata is not running.
)
