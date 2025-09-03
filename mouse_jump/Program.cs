using System.Runtime.InteropServices;

namespace MouseJumpUtility;

internal sealed class MainApplication : Form
{
    private const string MutexName = @"Global\MouseJumpUtility_Mutex";
    private const int TimerInterval = 50;

    private readonly Mutex _mutex;
    private readonly NotifyIcon _notifyIcon;
    private readonly System.Windows.Forms.Timer _timer;
    private DateTime _lastKeyPressTime = DateTime.MinValue;

    private static readonly (int Key, string Position)[] JumpKeys =
    {
        (0x82, "Center"),      // F19
        (0x83, "TopLeft"),     // F20
        (0x84, "TopRight"),    // F21
        (0x85, "BottomLeft"),  // F22
        (0x86, "BottomRight"), // F23
        (0x87, "TopCenter")    // F24
    };

    public MainApplication()
    {
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            ShowError("MouseJumpUtility is already running.", "Instance Error");
            Environment.Exit(1);
            return;
        }

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "MouseJump Utility\nF19-F24 to move mouse",
            Visible = true,
            ContextMenuStrip = new ContextMenuStrip()
        };
        _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => this.Close());

        _timer = new System.Windows.Forms.Timer { Interval = TimerInterval };
        _timer.Tick += OnTimerTick;
        _timer.Start();

        // Hide the form window
        this.WindowState = FormWindowState.Minimized;
        this.ShowInTaskbar = false;
        this.Opacity = 0;
    }

    private void OnTimerTick(object? sender, EventArgs e)
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
            NativeMethods.PostMessage(targetWindow, 0x0200, IntPtr.Zero, (IntPtr)((y << 16) | (x & 0xFFFF))); // WM_MOUSEMOVE
            NativeMethods.PostMessage(targetWindow, 0x0020, targetWindow, (IntPtr)0x02000001); // WM_SETCURSOR
        }
    }

    private static (int X, int Y)? GetJumpPoint(string position, NativeMethods.RECT rect)
    {
        const int margin = 1;
        return position switch
        {
            "Center" => ((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2),
            "TopLeft" => (rect.Left + margin, rect.Top + margin),
            "TopRight" => (rect.Right - margin, rect.Top + margin),
            "BottomLeft" => (rect.Left + margin, rect.Bottom - margin),
            "BottomRight" => (rect.Right - margin, rect.Bottom - margin),
            "TopCenter" => ((rect.Left + rect.Right) / 2, rect.Top + 15),
            _ => null
        };
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        this.Hide(); // Ensure the form is hidden when loaded
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Clean up resources safely before the form closes.
        _timer?.Stop();
        _timer?.Dispose();
        if (_notifyIcon != null) { _notifyIcon.Visible = false; }
        _notifyIcon?.Dispose();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnFormClosing(e);
    }

    private static void ShowError(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        try
        {
            Application.Run(new MainApplication());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Fatal Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}