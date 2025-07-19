kanata config (.kbd) for JIS laptop keyboard

keymap-drawer:
![keymap-drawer.svg](./keymap-drawer.svg)


### Install
- Download [kanata.exe](https://github.com/jtroo/kanata/releases) and save it to `%OneDrive%\command\apps\kanata.exe` (recommended)`
- Run the following command as an administrator to register kanata to startup:
    ```ps
    reg add "HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run" /v "kanata" /t REG_SZ /d "C:\Windows\System32\conhost.exe --headless `"$env:USERPROFILE\.config\kanata\start_kanata.bat`""
    ```

### Stop and unregister
- Run `stop_kanata.bat` to manually exit the kanata.
- The following command unregisters kanata from startup:
    ```ps
    reg delete "HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run" /v "kanata" /f
    ```
