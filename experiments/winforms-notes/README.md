# Microsoft WebView2 host comparison

A Windows-only technical comparison with the Avalonia Notes experiment. It reuses the exact editor assets, `NoteStore` source and pinned Core source. Its shell uses Microsoft's WinForms WebView2 control 1.0.4314.35, with .NET 10. It is not the proposed final BAT interface.

Run from the repository root in PowerShell on Windows:

```powershell
./tools/native-notes.ps1 setup
./tools/winforms-notes.ps1 build
./tools/winforms-notes.ps1 run -DataDirectory ./artifacts/my-host-comparison
```

The default data folder is `%LOCALAPPDATA%/BAT.Reset.HostComparison`; the executable identity is `Bat.Reset.HostComparison`. It never opens the installed BAT database. Use a separate directory from a running Avalonia experiment. A mutex permits one comparison process per data directory. The shared format allows an explicit copy of fictional fixtures for comparison; no automatic import or production migration is performed.

Ctrl+Alt+N captures into the full editor. F6 switches writing/native controls; F9 cycles logical window widths. Links open native text/address fields; Enter applies and Escape cancels. The timer uses the pinned Core source. Notes persist, while timer state remains an in-process test harness. Close and relaunch to inspect persistence.

The build excludes the package's unused WPF adapter reference, avoiding an unrelated WindowsBase assembly conflict. Public package versions are locked; setup checks the shared Core commit.

See [comparison findings](../../docs/experiments/direct-webview2-host-comparison.md). The result supports direct WebView2 as the stronger Windows host candidate. Actual screen-reader, IME, full clipboard/keyboard behaviour and multi-monitor validation remain separate gates. UIA inspection is evidence of exposed structure, not accessibility certification. The recent-note dropdown and control styling are harness conveniences, not product design decisions.
