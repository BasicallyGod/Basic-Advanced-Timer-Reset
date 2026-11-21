using System.Text.Json;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform;
using Avalonia.Media;
using Avalonia.Threading;
using Timer.Core;

namespace Bat.Reset.NotesExperiment;

internal sealed class MainWindow : Window
{
    private readonly NoteStore store;
    private readonly TrackingEngine clock = new();
    private readonly TextBlock timer = new(), saved = new(), shortcut = new(), reminder = new();
    private readonly TextBox title = new() { PlaceholderText = "Untitled note", FontSize = 25 },
        account = new() { PlaceholderText = "Account (optional)", Width = 155 },
        search = new() { PlaceholderText = "Search titles and note text" },
        source = new() { AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, FontFamily = new FontFamily("Cascadia Mono,Consolas"), FontSize = 15, IsVisible = false };
    private readonly ComboBox filter = new() { ItemsSource = new[] { "All accounts", "Acme", "Beacon", "" }, SelectedIndex = 0, MinWidth = 130 };
    private readonly StackPanel notes = new() { Spacing = 6 }, work = new() { Spacing = 14 }, formatting = new() { Orientation = Orientation.Horizontal, Spacing = 6, IsVisible = false };
    private readonly Grid library = new() { RowDefinitions = new RowDefinitions("Auto,*"), RowSpacing = 16 }, editor = new() { RowDefinitions = new RowDefinitions("Auto,Auto,*"), RowSpacing = 12 };
    private readonly NativeWebView web = new();
    private readonly Grid linkEditor = new() { ColumnDefinitions = new ColumnDefinitions("*,2*,Auto,Auto"), ColumnSpacing = 6, IsVisible = false };
    private readonly TextBox linkText = new() { PlaceholderText="Link text" }, linkUrl = new() { PlaceholderText="https://…" };
    private readonly DispatcherTimer saveDelay = new() { Interval = TimeSpan.FromMilliseconds(250) }, tick = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly Dictionary<Guid, int> revisions = new();
    private readonly Dictionary<Guid, string> positions = new();
    private readonly SemaphoreSlim saveGate = new(1,1);
    private readonly Button timerButton;
    private readonly DateTime reminderAt = DateTime.Now.AddSeconds(30);
    private GlobalCapture? hotkey;
    private Guid? selected;
    private string lastWorkspace = "library";
    private bool browserReady, loading, closing, reminderDone;
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public MainWindow(NoteStore store)
    {
        this.store = store;
        Title = "BAT Reset — Notes feasibility";
        Width = 940; Height = 760; MinWidth = 420; MinHeight = 500;
        Background = Brush.Parse("#F5F6EF");
        RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
        var root = new Grid { RowDefinitions = new RowDefinitions("Auto,Auto,*,Auto"), Margin = new Thickness(18), RowSpacing = 14 };
        var head = new WrapPanel { Orientation = Orientation.Horizontal, ItemSpacing = 8, LineSpacing = 8 };
        head.Children.Add(new TextBlock { Text = "BAT / NOTES LAB", FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,10,0) });
        timerButton=Action("Start timer", ToggleTimer);head.Children.Add(timerButton);
        timer.VerticalAlignment = VerticalAlignment.Center; timer.MinWidth = 110; head.Children.Add(timer);
        head.Children.Add(Action("Notes", () => ShowWorkspace("library")));
        head.Children.Add(Action("Timesheets", () => ShowWorkspace("work")));
        head.Children.Add(Action("Quick note", () => _ = NewNote()));
        root.Children.Add(head);
        reminder.Text = "One test reminder will become due in 30 seconds.";
        reminder.FontSize = 12;
        var attention = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        attention.Children.Add(reminder);
        attention.Children.Add(Action("Done", () => { reminderDone = true; UpdateClock(); }));
        Grid.SetRow(attention,1);root.Children.Add(attention);

        var filters = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), ColumnSpacing = 10 };
        filters.Children.Add(search);Grid.SetColumn(filter,1);filters.Children.Add(filter);
        library.Children.Add(filters);
        var scroll = new ScrollViewer { Content = notes };Grid.SetRow(scroll,1);library.Children.Add(scroll);
        search.TextChanged += (_, _) => RenderLibrary();filter.SelectionChanged += (_, _) => RenderLibrary();

        var documentHead = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"), ColumnSpacing = 10 };
        documentHead.Children.Add(Action("← Back", () => _ = Back()));
        Grid.SetColumn(title,1);documentHead.Children.Add(title);Grid.SetColumn(account,2);documentHead.Children.Add(account);
        editor.Children.Add(documentHead);
        var tools = new WrapPanel { ItemSpacing = 6, LineSpacing = 6 };
        tools.Children.Add(Action("Formatting", () => formatting.IsVisible = !formatting.IsVisible));
        tools.Children.Add(Action("Plain text", () => _ = ToggleSource()));
        tools.Children.Add(Action("Copy text", () => _ = CopyPlain()));
        tools.Children.Add(Action("Link", () => _ = Run("bat.editLink()")));
        tools.Children.Add(Action("Link to client call", () => { if (selected is { } id) SaveAction(() => store.Link(store.Data.Links[0].Id, id)); }));
        foreach (var (label,command) in new[] { ("Bold","bold"), ("Quote","blockQuote"), ("Bullet","bulletList") })
            formatting.Children.Add(Action(label, () => _ = Run($"bat.format({JsonSerializer.Serialize(command)})")));
        formatting.Children.Add(Action("Heading",()=>_=Run("bat.heading()")));
        linkEditor.Children.Add(linkText);Grid.SetColumn(linkUrl,1);linkEditor.Children.Add(linkUrl);
        var applyLink=Action("Apply",()=>_=ApplyLink());Grid.SetColumn(applyLink,2);linkEditor.Children.Add(applyLink);
        var cancelLink=Action("Cancel",()=>{linkEditor.IsVisible=false;web.Focus();_=Run("bat.focus()");});Grid.SetColumn(cancelLink,3);linkEditor.Children.Add(cancelLink);
        void LayoutLinkFields()
        {
            var narrow=Bounds.Width<560;
            linkEditor.ColumnDefinitions=new ColumnDefinitions(narrow?"*,Auto,Auto":"*,2*,Auto,Auto");
            linkEditor.RowDefinitions=new RowDefinitions(narrow?"Auto,Auto":"Auto");
            linkEditor.RowSpacing=6;
            Grid.SetRow(linkUrl,0);Grid.SetColumn(linkUrl,narrow?0:1);Grid.SetColumnSpan(linkUrl,narrow?3:1);
            Grid.SetRow(linkText,narrow?1:0);
            Grid.SetRow(applyLink,narrow?1:0);Grid.SetColumn(applyLink,narrow?1:2);
            Grid.SetRow(cancelLink,narrow?1:0);Grid.SetColumn(cancelLink,narrow?2:3);
        }
        SizeChanged+=(_,_)=>LayoutLinkFields();
        var toolsPanel = new StackPanel { Spacing = 8 };toolsPanel.Children.Add(tools);toolsPanel.Children.Add(formatting);toolsPanel.Children.Add(linkEditor);Grid.SetRow(toolsPanel,1);editor.Children.Add(toolsPanel);
        var writing = new Grid();writing.Children.Add(web);writing.Children.Add(source);Grid.SetRow(writing,2);editor.Children.Add(writing);
        title.TextChanged += (_, _) => ScheduleSave(); account.TextChanged += (_, _) => ScheduleSave();source.TextChanged += (_, _) => ScheduleSave();
        var body = new Grid();body.Children.Add(library);body.Children.Add(work);body.Children.Add(editor);Grid.SetRow(body,2);root.Children.Add(body);
        var foot = new StackPanel { Spacing = 3 };foot.Children.Add(saved);foot.Children.Add(shortcut);saved.FontSize = shortcut.FontSize = 12;Grid.SetRow(foot,3);root.Children.Add(foot);
        Content = root;
        var focusBoundary = new NativeFocusBoundary(this,web);
        AddHandler(InputElement.GotFocusEvent,(_,args)=>{
            if(args.Source is Control control)focusBoundary.ReclaimFor(control);
        },RoutingStrategies.Bubble);
        AddHandler(InputElement.KeyDownEvent,(_,args)=>{
            if(args.Key==Key.F9){args.Handled=true;Width=Width>700?600:Width>450?420:940;}
            if(args.Key==Key.F6&&selected is not null&&!source.IsVisible){args.Handled=true;web.Focus();_=Run("bat.focus()");}
        },RoutingStrategies.Bubble);
        linkEditor.KeyDown += (_,args)=>{
            if(args.Key==Key.Enter){args.Handled=true;_=ApplyLink();}
            else if(args.Key==Key.Escape){args.Handled=true;linkEditor.IsVisible=false;web.Focus();_=Run("bat.focus()");}
        };
        AutomationProperties.SetName(title,"Note title");AutomationProperties.SetName(account,"Note account");AutomationProperties.SetName(source,"Plain Markdown note body");AutomationProperties.SetName(search,"Search notes");
        AutomationProperties.SetName(linkText,"Link text");AutomationProperties.SetName(linkUrl,"Link address");
        AutomationProperties.SetName(web,"Note writing surface");

        web.EnvironmentRequested += (_, args) => {
            if (args is WindowsWebView2EnvironmentRequestedEventArgs windows)
            {
                windows.UserDataFolder = Path.Combine(Program.DataPath, "webview-profile");
                windows.ExperimentalOffscreen=Program.Offscreen;
            }
        };
        web.NavigationStarted += (_, args) => { if (args.Request is { } uri && uri.Scheme != "file" && uri.AbsoluteUri != "about:blank") args.Cancel = true; };
        web.NavigationCompleted += (_, args) => { if (!args.IsSuccess) saved.Text = "Editor could not load. Plain text remains available."; else _ = Receive("{\"type\":\"ready\"}"); };
        web.WebMessageReceived += (_, args) => { if(args.Body is { } message) _ = Receive(message); };
        saveDelay.Tick += async (_, _) => { saveDelay.Stop();await Flush(); };
        tick.Tick += (_, _) => UpdateClock();tick.Start();
        Opened += (_, _) => {
            hotkey = new GlobalCapture(this, () => { Show();WindowState = WindowState.Normal;Activate();_ = NewNote(); });
            shortcut.Text = hotkey.Registered ? "Ctrl+Alt+N · Quick note from another app. Experiment data only." : "Global capture unavailable. Use Quick note here. Experiment data only.";
            web.Source = new Uri(Path.Combine(AppContext.BaseDirectory,"Editor","index.html"));
            RenderLibrary();UpdateClock();
            if (store.LoadWarning is not null) { saved.Text = store.LoadWarning;editor.IsEnabled = false; }
            else if(store.RecoveredBackup) saved.Text = "Recovered the last complete backup.";
        };
        Closing += async (_, args) => { if (closing) return;args.Cancel = true;await Flush();closing = true;hotkey?.Dispose();tick.Stop();Close(); };
        ShowWorkspace("library");
    }

    private static Button Action(string label, Action action)
    {
        var button = new Button { Content = label, Padding = new Thickness(11,7) };
        button.Click += (_, _) => action();return button;
    }
    private async Task Run(string script) { if (browserReady) await web.InvokeScript(script); }
    private async Task NewNote()
    {
        await Flush();
        try { await Open(store.Create()); }
        catch(Exception e) when(e is IOException or UnauthorizedAccessException){saved.Text = "Could not create note: " + e.Message;}
    }
    private async Task Open(NoteDocument note)
    {
        await Flush();selected = note.Id;loading = true;
        title.Text = note.Title;account.Text = note.Account;source.Text = note.Body;loading = false;
        library.IsVisible = work.IsVisible = false;editor.IsVisible = true;linkEditor.IsVisible=false;
        if(browserReady) await LoadEditor(note);
    }
    private async Task LoadEditor(NoteDocument note)
    {
        var data = JsonSerializer.Serialize(new { id = note.Id, body = note.Body, revision = revisions.GetValueOrDefault(note.Id) },Json);
        await Run("bat.open("+data+")");
        if(positions.TryGetValue(note.Id,out var position)) await Run("bat.restore("+position+")");
        web.Focus();await Run("bat.focus()");
    }
    private async Task Receive(string message)
    {
        try
        {
            using var doc = JsonDocument.Parse(message);var data = doc.RootElement;
            if(data.GetProperty("type").GetString() is "ready") { browserReady = true;if(selected is { } id) await LoadEditor(store.Data.Notes.Single(n=>n.Id==id));return; }
            if(!data.TryGetProperty("id",out var key)||!Guid.TryParse(key.GetString(),out var noteId)||selected!=noteId) return;
            if(data.GetProperty("type").GetString()=="compactWindow"){Width=Width>700?600:Width>450?420:940;return;}
            if(data.GetProperty("type").GetString()=="focusShell"){title.Focus(NavigationMethod.Tab);return;}
            if(data.GetProperty("type").GetString()=="editLink")
            {
                linkText.Text=data.GetProperty("text").GetString()??"";linkUrl.Text=data.GetProperty("url").GetString()??"";
                linkEditor.IsVisible=true;linkUrl.Focus();return;
            }
            var rev=data.GetProperty("revision").GetInt32();if(rev<revisions.GetValueOrDefault(noteId))return;
            revisions[noteId]=rev;
            if(!source.IsVisible) {loading=true;source.Text=data.GetProperty("body").GetString()??"";loading=false;ScheduleSave();}
            if(data.GetProperty("type").GetString()=="close") await Back();
        }
        catch(Exception e) when(e is JsonException or InvalidOperationException){saved.Text="Editor message rejected; draft remains in the editor.";}
    }
    private void ScheduleSave()
    {
        if(loading||selected is null)return;
        saved.Text="Saving…";saveDelay.Stop();saveDelay.Start();
    }
    private async Task Flush()
    {
        saveDelay.Stop();await saveGate.WaitAsync();
        try
        {
            if(selected is not { } id)return;
            var noteTitle=title.Text??"";var noteAccount=account.Text??"";var body=source.Text??"";
            if(browserReady&&!source.IsVisible)
            {
                var raw=await web.InvokeScript("JSON.stringify(bat.remember())");
                if(raw is not null)
                {
                    // Adapters may return either JSON text or an encoded JS string.
                    using var outer=JsonDocument.Parse(raw);
                    var text=outer.RootElement.ValueKind==JsonValueKind.String?outer.RootElement.GetString():raw;
                    if(text is not null)
                    {
                        using var doc=JsonDocument.Parse(text);var data=doc.RootElement;
                        if(data.TryGetProperty("id",out var key)&&Guid.TryParse(key.GetString(),out var snapId)&&snapId==id)
                        {
                            body=data.GetProperty("body").GetString()??"";positions[id]=text;
                            if(selected==id){loading=true;source.Text=body;loading=false;}
                        }
                    }
                }
            }
            SaveAction(()=>store.Update(id,noteTitle,noteAccount,body));
        }
        catch(Exception e) when(e is JsonException or InvalidOperationException){saved.Text="Editor snapshot unavailable. Keep this note open and retry saving. "+e.Message;}
        finally{saveGate.Release();}
    }
    private void SaveAction(Action action)
    {
        try {action();saved.Text="Saved on this device · "+DateTime.Now.ToString("HH:mm:ss");}
        catch(Exception e) when(e is IOException or UnauthorizedAccessException){saved.Text="Not saved — "+e.Message;}
    }
    private async Task Back() {await Flush();selected=null;ShowWorkspace(lastWorkspace);}
    private void ShowWorkspace(string workspace)
    {
        if(selected is not null) { _=LeaveFor(workspace);return; }
        lastWorkspace=workspace;editor.IsVisible=false;library.IsVisible=workspace=="library";work.IsVisible=workspace=="work";
        RenderLibrary();RenderWork();
    }
    private async Task LeaveFor(string workspace) {await Flush();selected=null;ShowWorkspace(workspace);}
    private void RenderLibrary()
    {
        notes.Children.Clear();notes.Children.Add(Action("New note",()=>_=NewNote()));
        foreach(var note in store.Search(filter.SelectedItem?.ToString()??"All accounts",search.Text??""))
        {
            var heading=string.IsNullOrWhiteSpace(note.Title)?note.Body.Split('\n')[0].TrimStart('#',' ','>'):note.Title;
            if(string.IsNullOrWhiteSpace(heading))heading="Untitled note";
            var button=Action(heading+"\n"+(string.IsNullOrWhiteSpace(note.Account)?"Unassigned":note.Account)+" · "+Snippet(note.Body,search.Text??""),()=>_=Open(note));
            button.HorizontalAlignment=HorizontalAlignment.Stretch;button.HorizontalContentAlignment=HorizontalAlignment.Left;notes.Children.Add(button);
        }
    }
    private static string Snippet(string body,string phrase)
    {
        var index=string.IsNullOrEmpty(phrase)?0:Math.Max(0,body.IndexOf(phrase,StringComparison.OrdinalIgnoreCase)-35);
        var text=body[index..].Replace('\n',' ');return text.Length>95?text[..95]+"…":text;
    }
    private void RenderWork()
    {
        work.Children.Clear();work.Children.Add(new TextBlock {Text="Return to your work",FontSize=24});
        work.Children.Add(new TextBlock {Text="09:30–10:00 · Acme · Client call",FontSize=17});
        work.Children.Add(new TextBlock {Text="This fixed sample block tests note identity. Writing never changes its time or description.",TextWrapping=TextWrapping.Wrap});
        foreach(var block in store.Data.Links)
        {
            if(block.NoteId is { } id&&store.Data.Notes.SingleOrDefault(n=>n.Id==id) is { } note)
                work.Children.Add(Action("Open linked note",()=>_=Open(note)));
            else work.Children.Add(new TextBlock {Text="No note linked yet. Capture first, then link from the note."});
        }
    }
    private async Task ToggleSource()
    {
        await Flush();source.IsVisible=!source.IsVisible;web.IsVisible=!source.IsVisible;
        if(source.IsVisible)source.Focus();
        else {await Run("bat.plain("+JsonSerializer.Serialize(source.Text??"")+")");web.Focus();await Run("bat.focus()");}
    }
    private async Task CopyPlain()
    {
        await Flush();if(Clipboard is { } clipboard)await clipboard.SetTextAsync(source.Text??"");
    }
    private async Task ApplyLink()
    {
        if(!Uri.TryCreate(linkUrl.Text,UriKind.Absolute,out var uri)||uri.Scheme is not ("http" or "https")){saved.Text="Use a full http or https address.";return;}
        await Run("bat.insertLink("+JsonSerializer.Serialize(linkText.Text??"")+","+JsonSerializer.Serialize(uri.AbsoluteUri)+")");
        linkEditor.IsVisible=false;web.Focus();await Run("bat.focus()");
    }
    private void ToggleTimer() {if(clock.Active is null)clock.ToggleAccount("Acme");else clock.StopAccount();UpdateClock();}
    private void UpdateClock()
    {
        WriteLayoutDiagnostics();
        timer.Text=clock.Active is null?"Timer paused":"Acme · "+clock.ActiveElapsed.ToString(@"hh\:mm\:ss");
        timerButton.Content=clock.Active is null?"Start timer":"Pause timer";
        reminder.Text=reminderDone?"Test reminder completed":DateTime.Now>=reminderAt?"● 1 due · Chase Acme approval":"Test reminder in "+Math.Max(0,(int)(reminderAt-DateTime.Now).TotalSeconds)+"s";
    }
    private string? lastLayout;
    private void WriteLayoutDiagnostics()
    {
        if(!IsVisible)return;
        var data=JsonSerializer.Serialize(new {
            clientWidth=ClientSize.Width,clientHeight=ClientSize.Height,renderScaling=RenderScaling,
            offscreen=Program.Offscreen,
            editorWidth=web.Bounds.Width,editorHeight=web.Bounds.Height,
            screens=Screens.All.Select(s=>new {width=s.Bounds.Width,height=s.Bounds.Height,scaling=s.Scaling,workingWidth=s.WorkingArea.Width,workingHeight=s.WorkingArea.Height})
        },Json);
        if(data==lastLayout)return;lastLayout=data;
        try { File.WriteAllText(Path.Combine(Program.DataPath,"layout-diagnostics.json"),data); }
        catch(Exception error) when(error is IOException or UnauthorizedAccessException) { /* Diagnostics must not interrupt writing. */ }
    }
}
