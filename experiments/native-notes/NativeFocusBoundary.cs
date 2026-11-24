using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Bat.Reset.NotesExperiment;

// Avalonia focus and Win32 keyboard focus are separate when a child WebView owns
// input. In WebView 12.1.2 the Windows adapter's ResignFocus() is empty. Reclaim
// only this window's HWND when one of its Avalonia controls receives focus.
internal sealed class NativeFocusBoundary(Window window, NativeWebView web)
{
    private bool reclaiming;

    public void ReclaimFor(Control control)
    {
        Dispatcher.UIThread.VerifyAccess();
        if (reclaiming || control == web || web.IsVisualAncestorOf(control)) return;
        if (!OperatingSystem.IsWindows()) return;
        var handle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle == IntPtr.Zero || GetFocus() == handle) return;
        reclaiming = true;
        try { SetFocus(handle); }
        finally { reclaiming = false; }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetFocus();
    [DllImport("user32.dll")]
    private static extern IntPtr SetFocus(IntPtr window);
}
