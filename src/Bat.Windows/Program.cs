using System.Runtime.InteropServices;
using System.Text.Json;
using Bat.Application;
using Bat.Persistence;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Bat.Windows;

internal static class Program
{
    [STAThread] static void Main(string[] args)
    {
        System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        System.Windows.Forms.Application.EnableVisualStyles();
        var i = Array.IndexOf(args, "--data-dir");
        var directory = Path.GetFullPath(i >= 0 && args.Length > i + 1 ? args[i + 1] : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BAT.Reset.Development"));
        var key = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(Path.TrimEndingDirectorySeparator(directory).ToUpperInvariant())))[..16];
        using var instance = new Mutex(true, "Local\\BAT.Reset.Development." + key, out var owns);
        if (!owns) { MessageBox.Show("This development workspace is already open.", "BAT Reset"); return; }
        try { System.Windows.Forms.Application.Run(new WorkspaceWindow(directory)); }
        catch (Exception e) { MessageBox.Show("BAT could not open the development workspace. Files are preserved.\n\n" + e.Message, "BAT Reset", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { instance.ReleaseMutex(); }
    }
}

internal sealed class WorkspaceWindow : Form
{
    private const string Origin = "https://bat.local";
    private readonly WebView2 web = new() { Dock = DockStyle.Fill };
    private readonly WorkspaceService service;
    private readonly System.Windows.Forms.Timer tick = new() { Interval = 1000 };
    private readonly NotifyIcon tray;
    private readonly PillWindow pill;
    private int ticks;
    private bool ready, quitting, quitApproved;
    private string? pendingCapture;
    private readonly string directory;
    public WorkspaceWindow(string directory)
    {
        this.directory = directory; service = new(new DevelopmentStore(directory));
        Text = "BAT Reset — development"; ClientSize = new(1100, 800); MinimumSize = new(420, 500);
        AutoScaleDimensions = new(96, 96); AutoScaleMode = AutoScaleMode.Dpi;
        Controls.Add(web);
        ResizeEnd += (_, _) => WriteLayoutDiagnostics();
        pill = new(ActivateWorkspace, () => OpenCapture("reminder"), () => OpenCapture("note"), RequestQuit);
        var menu = new ContextMenuStrip(); menu.Items.Add("Open BAT", null, (_, _) => ActivateWorkspace());
        menu.Items.Add("New reminder", null, (_, _) => OpenCapture("reminder"));
        menu.Items.Add("Quick note", null, (_, _) => OpenCapture("note"));
        menu.Items.Add("Quit BAT", null, (_, _) => RequestQuit());
        tray = new() { Icon = SystemIcons.Application, Text = "BAT Reset development", Visible = true, ContextMenuStrip = menu };
        tray.DoubleClick += (_, _) => ActivateWorkspace();
        Load += async (_, _) => await Initialise();
        tick.Tick += async (_, _) =>
        {
            if (++ticks % 15 == 0) { try { service.Checkpoint(); } catch (Exception e) { await Event("storageError", e.Message); } }
            pill.UpdateState(service.Read()); await Event("clock", service.Read());
        };
        FormClosing += (_, e) =>
        {
            quitting = true;
            if (quitApproved || !ready) return;
            e.Cancel = true; _ = Event("prepareQuit", true);
        };
        FormClosed += (_, _) => { tick.Dispose(); tray.Dispose(); pill.Dispose(); UnregisterHotKey(Handle, 1); UnregisterHotKey(Handle, 2); };
    }
    private async Task Initialise()
    {
        try
        {
            var environment = await CoreWebView2Environment.CreateAsync(null, Path.Combine(directory, "webview-profile"));
            await web.EnsureCoreWebView2Async(environment);
            var core = web.CoreWebView2;
            core.SetVirtualHostNameToFolderMapping("bat.local", Path.Combine(AppContext.BaseDirectory, "Ui"), CoreWebView2HostResourceAccessKind.DenyCors);
            core.Settings.AreDefaultScriptDialogsEnabled = false;
            core.Settings.AreDevToolsEnabled = false;
            core.NavigationStarting += (_, e) => { if (e.Uri != Origin + "/index.html") e.Cancel = true; };
            core.NewWindowRequested += (_, e) => e.Handled = true;
            core.PermissionRequested += (_, e) => e.State = CoreWebView2PermissionState.Deny;
            core.ProcessFailed += async (_, _) => await Event("storageError", "The writing view stopped. Keep BAT open; restart the view after recovering your draft.");
            core.WebMessageReceived += async (_, e) =>
            {
                if (e.Source != Origin + "/index.html") return;
                try
                {
                    var raw = e.WebMessageAsJson;
                    if (raw.Length > 2_100_000) throw new JsonException("Command too large.");
                    var request = JsonSerializer.Deserialize<Request>(raw, WorkspaceService.Json) ?? throw new JsonException("Empty command.");
                    if (request.Type == "host.ready") { ready = true; await Event("state", service.Read()); if (pendingCapture is not null) { await Event("capture", pendingCapture); pendingCapture = null; } return; }
                    if (request.Type == "host.hide") { ShowPill(); return; }
                    if (request.Type == "host.quit") { RequestQuit(); return; }
                    if (request.Type == "host.focus") { web.Focus(); await Event("reply", new Reply(request.RequestId, true, service.Read())); return; }
                    if (request.Type == "host.resize")
                    {
                        var width = ClientSize.Width * 96d / DeviceDpi;
                        ClientSize = new((int)Math.Round((width > 700 ? 600 : width > 450 ? 420 : 1100) * DeviceDpi / 96d), ClientSize.Height);
                        return;
                    }
                    if (request.Type == "host.quitReady")
                    {
                        if (!quitting) return;
                        if (!request.Payload.GetProperty("ok").GetBoolean()) { quitting = false; ActivateWorkspace(); return; }
                        try { service.Checkpoint(); quitApproved = true; Close(); }
                        catch (Exception ex) { quitting = false; ActivateWorkspace(); await Event("storageError", "Recording checkpoint was not saved. Retry before quitting. " + ex.Message); }
                        return;
                    }
                    var reply = service.Execute(request); await Event("reply", reply);
                }
                catch (Exception ex) when (ex is JsonException or InvalidOperationException) { await Event("storageError", ex.Message); }
            };
            web.Source = new(Origin + "/index.html");
            var note = RegisterHotKey(Handle, 1, 0x4003, 0x4E);
            var reminder = RegisterHotKey(Handle, 2, 0x4003, 0x52);
            if (!note || !reminder) tray.ShowBalloonTip(3000, "BAT shortcuts", "A capture shortcut is in use. Use the pill or tray capture actions.", ToolTipIcon.Info);
            tick.Start(); pill.UpdateState(service.Read());
            WriteLayoutDiagnostics();
        }
        catch (Exception ex) { MessageBox.Show("Unable to start the BAT view.\n" + ex.Message); quitting = true; Close(); }
    }
    private Task Event(string type, object payload)
    {
        if (ready && !web.IsDisposed) web.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { type, payload }, WorkspaceService.Json));
        return Task.CompletedTask;
    }
    private void WriteLayoutDiagnostics()
    {
        try
        {
            File.WriteAllText(Path.Combine(directory, "layout-diagnostics.json"), JsonSerializer.Serialize(new
            {
                DeviceDpi, ClientSize.Width, ClientSize.Height, Visible,
                Pill = new { pill.Visible, pill.Bounds },
                Screens = Screen.AllScreens.Select(s => new { s.Bounds, s.WorkingArea })
            }));
        }
        catch (IOException) { /* Diagnostics must not interrupt capture. */ }
        catch (UnauthorizedAccessException) { }
    }
    private void ActivateWorkspace()
    {
        pill.Hide();
        // Clamp restored geometry to the current monitor's work area after a display is removed.
        var area = Screen.FromPoint(Cursor.Position).WorkingArea;
        if (!Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(Bounds))) Location = area.Location;
        Show(); WindowState = FormWindowState.Normal; Activate(); web.Focus();
        WriteLayoutDiagnostics();
    }
    private void ShowPill() { pill.Show(); Hide(); WriteLayoutDiagnostics(); }
    private void RequestQuit() { quitting = true; Close(); }
    private void OpenCapture(string kind) { ActivateWorkspace(); if (!ready) pendingCapture = kind; else _ = Event("capture", kind); }
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x0312) OpenCapture(m.WParam.ToInt32() == 1 ? "note" : "reminder");
        base.WndProc(ref m);
    }
    [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr window, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr window, int id);
}

internal sealed class PillWindow : Form
{
    private readonly Button open = new() { AutoSize = true, Text = "BAT" };
    public PillWindow(Action activate, Action reminder, Action note, Action quit)
    {
        Text = "BAT capture pill"; FormBorderStyle = FormBorderStyle.FixedSingle; ShowInTaskbar = true; TopMost = true;
        AutoScaleDimensions = new(96, 96); AutoScaleMode = AutoScaleMode.Dpi; ClientSize = new(420, 48);
        StartPosition = FormStartPosition.Manual;
        var row = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new(6), WrapContents = false };
        var add = new Button { Text = "+ Reminder", AutoSize = true };
        var write = new Button { Text = "Quick note", AutoSize = true };
        var exit = new Button { Text = "Quit", AccessibleName = "Quit BAT", AutoSize = true };
        open.Click += (_, _) => activate(); add.Click += (_, _) => reminder(); write.Click += (_, _) => note();
        exit.Click += (_, _) => quit();
        row.Controls.AddRange([open, add, write, exit]); Controls.Add(row);
        Shown += (_, _) => { var area = Screen.FromPoint(Cursor.Position).WorkingArea; Location = new(area.Right - Width - 20, area.Top + 20); };
        FormClosing += (_, e) => { e.Cancel = true; quit(); };
    }
    public void UpdateState(Snapshot state)
    {
        var due = state.Workspace.Reminders.Count(r => r.Due <= DateTimeOffset.Now);
        open.Text = $"BAT · {due} due" + (state.ActiveAccount is null ? "" : " · ● " + state.ActiveAccount);
    }
}
