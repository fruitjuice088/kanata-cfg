@echo off

set KANATA_EXE=%OneDrive%\apps\kanata.exe
set CONFIG_FILE=%USERPROFILE%\.config\kanata-cfg\main.kbd
tasklist | findstr /i kanata.exe >nul
if %ERRORLEVEL% EQU 0 (
    echo kanata is already running. Exiting...
    timeout /t 3 /nobreak >nul
    exit /b 1
)

"%KANATA_EXE%" -n --cfg "%CONFIG_FILE%"
