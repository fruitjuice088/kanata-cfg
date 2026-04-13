using System.Runtime.InteropServices;

namespace MouseJumpUtility;

internal static class Program
{
    private const string MutexName = @"Global\MouseJumpUtility_Mutex";
    private const int TimerInterval = 50;

    private static Mutex? _mutex;
    private static Timer? _timer;
    private static DateTime _lastKeyPressTime = DateTime.MinValue;

    private static readonly (int Key, string Position)[] JumpKeys =
    {
        (0x81, "BottomLeft"),   // F18
        (0x82, "BottomRight"),  // F19
        (0x83, "TopCenter"),    // F20
        (0x84, "Center"),       // F21
        (0x85, "TopLeft"),      // F22
        (0x86, "TopRight")      // F23
    };

    [STAThread]
    private static void Main()
    {
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            Console.WriteLine("MouseJumpUtility is already running.");
            Environment.Exit(1);
            return;
        }

        Console.WriteLine("MouseJumpUtility started. Press Ctrl+C to exit.");

        _timer = new Timer(OnTimerTick, null, 0, TimerInterval);

        // Keep the application running
        var exitEvent = new ManualResetEvent(false);
        Console.CancelKeyPress += (sender, e) => {
            e.Cancel = true;
            exitEvent.Set();
        };
        exitEvent.WaitOne();

        CleanUp();
    }

    private static void OnTimerTick(object? state)
    {
        if ((DateTime.Now - _lastKeyPressTime).TotalMilliseconds < 100) return;

        foreach (var (key, position) in JumpKeys)
        {
            if (NativeMethods.GetAsyncKeyState(key) != 0)
            {
                JumpToPosition(position);
                _lastKeyPressTime = DateTime.Now;
                break;
            }
        }
    }

    private static void JumpToPosition(string position)
    {
        IntPtr window = NativeMethods.GetForegroundWindow();
        if (window == IntPtr.Zero || !NativeMethods.GetWindowRect(window, out var rect)) return;

        var point = GetJumpPoint(position, rect);
        if (point is null) return;

        var (x, y) = point.Value;
        NativeMethods.SetCursorPos(x, y);

        IntPtr targetWindow = NativeMethods.WindowFromPoint(new NativeMethods.POINT { X = x, Y = y });
        if (targetWindow != IntPtr.Zero)
        {
            NativeMethods.PostMessage(targetWindow, 0x0020, targetWindow, (IntPtr)0x02000001); // WM_SETCURSOR
            Thread.Sleep(10);
            NativeMethods.PostMessage(targetWindow, 0x0200, IntPtr.Zero, (IntPtr)((y << 16) | (x & 0xFFFF))); // WM_MOUSEMOVE
            NativeMethods.PostMessage(targetWindow, 0x0020, targetWindow, (IntPtr)0x02000001); // WM_SETCURSOR
        }
    }

    private static (int X, int Y)? GetJumpPoint(string position, NativeMethods.RECT rect)
    {
        return position switch
        {
            "Center" => ((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2),
            "TopLeft" => (rect.Left + 3, rect.Top + 1),
            "TopRight" => (rect.Right - 3, rect.Top + 1),
            "BottomLeft" => (rect.Left + 2, rect.Bottom - 3),
            "BottomRight" => (rect.Right - 2, rect.Bottom - 3),
            "TopCenter" => ((rect.Left + rect.Right) / 2, rect.Top + 15),
            _ => null
        };
    }

    private static void CleanUp()
    {
        _timer?.Dispose();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        Console.WriteLine("MouseJumpUtility stopped.");
    }
}

internal static class NativeMethods
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT { public int X; public int Y; }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll")]
    internal static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    internal static extern IntPtr WindowFromPoint(POINT Point);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    internal static extern short GetAsyncKeyState(int vKey);
}
