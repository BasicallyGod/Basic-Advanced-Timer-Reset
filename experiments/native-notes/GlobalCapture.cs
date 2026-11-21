using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Win32;

namespace Bat.Reset.NotesExperiment;

internal sealed class GlobalCapture : IDisposable
{
    private const int Id = 0xBA71;
    private readonly Window window;
    private readonly Action capture;
    private readonly IntPtr handle;
    public bool Registered { get; }
    public GlobalCapture(Window window, Action capture)
    {
        this.window = window;
        this.capture = capture;
        if (!OperatingSystem.IsWindows()) return;
        handle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        // Experiment binding only: Ctrl+Alt+N. MOD_NOREPEAT avoids repeated documents.
        Registered = RegisterHotKey(handle, Id, 0x0001 | 0x0002 | 0x4000, 0x4E);
        if (Registered) Win32Properties.AddWndProcHookCallback(window, Message);
    }
    private IntPtr Message(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == 0x0312 && wParam.ToInt32() == Id) { handled = true; capture(); }
        return IntPtr.Zero;
    }
    public void Dispose()
    {
        if (!Registered) return;
        UnregisterHotKey(handle, Id);
        Win32Properties.RemoveWndProcHookCallback(window, Message);
    }
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint key);
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
