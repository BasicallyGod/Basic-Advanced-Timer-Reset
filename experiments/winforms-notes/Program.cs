using System.Runtime.InteropServices;
using System.Text.Json;
using Bat.Reset.NotesExperiment;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Timer.Core;

namespace Bat.Reset.HostComparison;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
        var index=Array.IndexOf(args,"--data-dir");
        var path=Path.TrimEndingDirectorySeparator(Path.GetFullPath(index>=0&&args.Length>index+1?args[index+1]:Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"BAT.Reset.HostComparison")));
        Directory.CreateDirectory(path);
        var key=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(path.ToUpperInvariant())))[..16];
        using var instance=new Mutex(true,"Local\\BAT.Reset.HostComparison."+key,out var ownsInstance);
        if(!ownsInstance)return;
        Application.Run(new ComparisonWindow(path));
        instance.ReleaseMutex();
    }
}

internal sealed class ComparisonWindow : Form
{
    private readonly WebView2 web=new() {Dock=DockStyle.Fill};
    private readonly TextBox title=new() {Width=250,PlaceholderText="Untitled note",AccessibleName="Note title"}, account=new() {Width=140,PlaceholderText="Account (optional)",AccessibleName="Note account"};
    private readonly TextBox linkText=new() {Width=150,AccessibleName="Link text"},linkUrl=new() {Width=280,AccessibleName="Link address"};
    private readonly ComboBox recent=new() {Width=180,DropDownStyle=ComboBoxStyle.DropDownList,AccessibleName="Recent notes"};
    private readonly TextBox search=new() {Width=160,PlaceholderText="Search note text",AccessibleName="Search notes"};
    private readonly FlowLayoutPanel links=new() {Dock=DockStyle.Top,AutoSize=true,Visible=false,WrapContents=true};
    private readonly Label clockLabel=new() {AutoSize=true},status=new() {Dock=DockStyle.Bottom,Height=24,Text="Loading editor…"};
    private readonly System.Windows.Forms.Timer save=new() {Interval=250},tick=new() {Interval=1000};
    private readonly NoteStore store;private readonly TrackingEngine clock=new();private readonly string directory;
    private Guid? selected;private bool ready,loading,closing,hotkey;
    private readonly SemaphoreSlim saveGate=new(1,1);
    private static readonly JsonSerializerOptions Json=new() {PropertyNamingPolicy=JsonNamingPolicy.CamelCase};

    public ComparisonWindow(string directory)
    {
        this.directory=directory;store=new NoteStore(directory);
        Text="BAT Reset — WebView2 host comparison";ClientSize=new(940,760);MinimumSize=new(400,500);KeyPreview=true;
        AutoScaleDimensions=new(96,96);AutoScaleMode=AutoScaleMode.Dpi;
        var head=new FlowLayoutPanel {Dock=DockStyle.Top,AutoSize=true,WrapContents=true,Padding=new(8)};
        head.Controls.Add(Button("Timer",()=>{if(clock.Active is null)clock.ToggleAccount("Acme");else clock.StopAccount();UpdateClock();}));head.Controls.Add(clockLabel);
        head.Controls.Add(Button("Quick note",()=>_=NewNote()));head.Controls.Add(recent);head.Controls.Add(search);
        var metadata=new FlowLayoutPanel {Dock=DockStyle.Top,AutoSize=true,WrapContents=true,Padding=new(8)};
        metadata.Controls.Add(title);metadata.Controls.Add(account);
        metadata.Controls.Add(Button("Link",()=>_=Run("bat.editLink()")));
        metadata.Controls.Add(Button("Link to call",()=>{if(selected is {} id){store.Link(store.Data.Links[0].Id,id);status.Text="Linked to client call";}}));
        metadata.Controls.Add(Button("Open call note",()=>{var id=store.Data.Links[0].NoteId;if(id is {} value)_=Open(store.Data.Notes.Single(n=>n.Id==value));}));
        links.Controls.Add(linkUrl);links.Controls.Add(linkText);links.Controls.Add(Button("Apply",()=>_=ApplyLink()));links.Controls.Add(Button("Cancel",()=>_=CancelLink()));
        Controls.Add(web);Controls.Add(links);Controls.Add(metadata);Controls.Add(head);Controls.Add(status);
        title.TextChanged+=(_,_)=>Schedule();account.TextChanged+=(_,_)=>Schedule();
        search.TextChanged+=(_,_)=>RefreshRecent();recent.SelectedIndexChanged+=(_,_)=>{if(!loading&&recent.SelectedItem is NoteChoice choice)_=Open(store.Data.Notes.Single(n=>n.Id==choice.Id));};
        save.Tick+=async(_,_)=>{save.Stop();await Flush();};tick.Tick+=(_,_)=>UpdateClock();tick.Start();
        Load+=async(_,_)=>{
            var env=await CoreWebView2Environment.CreateAsync(null,Path.Combine(directory,"webview-profile"));
            await web.EnsureCoreWebView2Async(env);
            web.KeyDown+=(_,args)=>{
                if(args.KeyCode==Keys.F6){args.SuppressKeyPress=true;BeginInvoke((Action)(async()=>{await Run("bat.remember()");title.Focus();}));}
            };
            await web.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.invokeCSharpAction=body=>window.chrome.webview.postMessage(body)");
            web.CoreWebView2.WebMessageReceived+=(_,args)=>_=Receive(args.TryGetWebMessageAsString());
            web.CoreWebView2.NavigationStarting+=(_,args)=>{if(!args.Uri.StartsWith("file:",StringComparison.OrdinalIgnoreCase)&&args.Uri!="about:blank")args.Cancel=true;};
            web.CoreWebView2.NewWindowRequested+=(_,args)=>args.Handled=true;
            web.Source=new Uri(Path.Combine(AppContext.BaseDirectory,"Editor/index.html"));
            hotkey=RegisterHotKey(Handle,0xBA72,0x4003,0x4E);
            RefreshRecent();UpdateClock();
        };
        FormClosing+=async(_,args)=>{if(closing)return;args.Cancel=true;await Flush();closing=true;Close();};
        FormClosed+=(_,_)=>{if(hotkey)UnregisterHotKey(Handle,0xBA72);save.Dispose();tick.Dispose();web.Dispose();};
        KeyDown+=async(_,args)=>{
            if(args.KeyCode==Keys.F6&&!web.ContainsFocus){args.SuppressKeyPress=true;web.Focus();await Run("bat.focus()");}
            if(args.KeyCode==Keys.F9){args.SuppressKeyPress=true;Compact();}
            if(links.Visible&&args.KeyCode==Keys.Enter){args.SuppressKeyPress=true;await ApplyLink();}
            if(links.Visible&&args.KeyCode==Keys.Escape){args.SuppressKeyPress=true;await CancelLink();}
        };
    }
    private static Button Button(string text,Action action){var b=new Button {Text=text,AutoSize=true};b.Click+=(_,_)=>action();return b;}
    private async Task Run(string script){if(ready)await web.ExecuteScriptAsync(script);}
    private async Task NewNote(){await Flush();await Open(store.Create());}
    private async Task Open(NoteDocument note)
    {
        await Flush();selected=note.Id;loading=true;title.Text=note.Title;account.Text=note.Account;loading=false;links.Visible=false;
        await Run("bat.open("+JsonSerializer.Serialize(new {id=note.Id,body=note.Body},Json)+")");web.Focus();await Run("bat.focus()");
    }
    private async Task Receive(string body)
    {
        using var document=JsonDocument.Parse(body);var data=document.RootElement;var type=data.GetProperty("type").GetString();
        if(type=="ready"){ready=true;status.Text="Ready · Ctrl+Alt+N capture · F6 controls/writing · F9 widths";if(store.Data.Notes.Count>0)await Open(store.Data.Notes[0]);else await NewNote();return;}
        if(!data.TryGetProperty("id",out var id)||!Guid.TryParse(id.GetString(),out var value)||selected!=value)return;
        if(type=="changed")Schedule();
        if(type=="focusShell")title.Focus();
        if(type=="compactWindow")Compact();
        if(type=="editLink"){linkText.Text=data.GetProperty("text").GetString();linkUrl.Text=data.GetProperty("url").GetString();links.Visible=true;linkUrl.Focus();}
        if(type=="close"){await Flush();recent.Focus();}
    }
    private void Schedule(){if(loading||selected is null)return;save.Stop();save.Start();status.Text="Saving…";}
    private async Task Flush()
    {
        if(!ready)return;await saveGate.WaitAsync();
        try{
            if(selected is not {} id)return;
            var caption=title.Text;var home=account.Text;
            var raw=await web.ExecuteScriptAsync("bat.remember()");using var doc=JsonDocument.Parse(raw);
            if(doc.RootElement.GetProperty("id").GetString()!=id.ToString())return;
            store.Update(id,caption,home,doc.RootElement.GetProperty("body").GetString()??"");
            status.Text="Saved on this device";RefreshRecent();
        }catch(Exception e) when(e is IOException or UnauthorizedAccessException){status.Text="Not saved: "+e.Message;}
        finally{saveGate.Release();}
    }
    private void RefreshRecent()
    {
        loading=true;recent.Items.Clear();foreach(var n in store.Search("All accounts",search.Text))recent.Items.Add(new NoteChoice(n.Id,string.IsNullOrWhiteSpace(n.Title)?n.Body.Split('\n')[0]:n.Title));loading=false;
    }
    private async Task ApplyLink(){if(!Uri.TryCreate(linkUrl.Text,UriKind.Absolute,out var uri)||uri.Scheme is not("http"or"https"))return;await Run("bat.insertLink("+JsonSerializer.Serialize(linkText.Text)+","+JsonSerializer.Serialize(uri.AbsoluteUri)+")");await CancelLink();}
    private async Task CancelLink(){links.Visible=false;web.Focus();await Run("bat.focus()");}
    private void Compact(){var width=ClientSize.Width*96d/DeviceDpi;var target=width>700?600:width>450?420:940;ClientSize=new((int)Math.Round(target*DeviceDpi/96d),ClientSize.Height);}
    private void UpdateClock(){clockLabel.Text=clock.Active is null?"Paused":"Acme · "+clock.ActiveElapsed.ToString(@"hh\:mm\:ss");}
    protected override void WndProc(ref Message m){if(m.Msg==0x0312&&m.WParam.ToInt32()==0xBA72){Show();WindowState=FormWindowState.Normal;Activate();_=NewNote();}base.WndProc(ref m);}
    [DllImport("user32.dll")]private static extern bool RegisterHotKey(IntPtr window,int id,uint modifiers,uint key);
    [DllImport("user32.dll")]private static extern bool UnregisterHotKey(IntPtr window,int id);
    private sealed record NoteChoice(Guid Id,string Caption){public override string ToString()=>Caption;}
}
