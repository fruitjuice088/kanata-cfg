# MouseJumpUtility

This is a utility to jump the mouse cursor to different positions on the active window.

## How to Use

1.  Run the `MouseJumpUtility.exe`.
2.  An icon will appear in the system tray.
3.  Use the following hotkeys to move the mouse:
    *   **F18:** Bottom-Left
    *   **F19:** Bottom-Right
    *   **F20:** Top-Center
    *   **F21:** Center
    *   **F22:** Top-Left
    *   **F23:** Top-Right
4.  Right-click the tray icon and select "Exit" to close the application.

## How to Publish

To publish the application as a single executable file, run the following command from the project root directory:

```shell
dotnet publish -c Release
```

This command creates a single `.exe` file in the `bin\Release\net8.0-windows\win-x64\publish\` directory.
``` 

**Note:** The application requires the .NET 8 Desktop Runtime to be installed on the target machine.
