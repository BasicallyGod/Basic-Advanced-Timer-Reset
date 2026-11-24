using System.Text.Json;
using Timer.Core;

namespace Bat.Application;

public sealed record Note(Guid Id, int Revision, string Title, string Account, string Body, DateTimeOffset Updated);
public sealed record Reminder(Guid Id, string Action, string Account, DateTimeOffset Due);
public sealed record Workspace(int Format, int Revision, List<Note> Notes, List<Reminder> Reminders,
    List<SessionRow> Sessions, ActiveSession? Active, DateTime? LastSeen);
public interface IWorkspaceStore
{
    Workspace? Load();
    void Save(Workspace workspace);
    string? Notice { get; }
}
public sealed record Request(int Version, string RequestId, string Type, JsonElement Payload);
public sealed record Snapshot(Workspace Workspace, string? ActiveAccount, double ElapsedSeconds, DateTime Now, string? Notice);
public sealed record Reply(string RequestId, bool Ok, Snapshot State, string? Error = null);

// All commands run serially on the host UI thread. Only acknowledged snapshots are durable.
public sealed class WorkspaceService
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly IWorkspaceStore store;
    private readonly Func<DateTime> now;
    private Workspace workspace;
    private TrackingEngine engine;
    private string? notice;
    public WorkspaceService(IWorkspaceStore store, Func<DateTime>? now = null)
    {
        this.store = store; this.now = now ?? (() => DateTime.Now);
        workspace = store.Load() ?? new(1, 0, [], [], [], null, null);
        engine = new(clock: this.now); engine.Sessions.AddRange(workspace.Sessions);
        if (workspace.Active is not null)
        {
            engine.Recover(workspace.Active, workspace.LastSeen);
            notice = "Recovered recording up to its last saved checkpoint. Recording is paused.";
            var recovered = workspace with { Revision = workspace.Revision + 1, Sessions = [.. engine.Sessions], Active = null, LastSeen = this.now() };
            store.Save(recovered); workspace = recovered;
        }
        notice ??= store.Notice;
    }
    public Snapshot Read() => new(workspace, engine.Active?.Account, engine.ActiveElapsed.TotalSeconds, now(), notice);
    public void Checkpoint()
    {
        if (engine.Active is null) return;
        var next = workspace with { Revision = workspace.Revision + 1, Active = engine.Active, LastSeen = now(), Sessions = [.. engine.Sessions] };
        store.Save(next); workspace = next;
    }
    public Reply Execute(Request request)
    {
        try
        {
            if (request.Version != 1 || string.IsNullOrWhiteSpace(request.RequestId) || request.RequestId.Length > 100)
                throw new InvalidOperationException("Unsupported command envelope.");
            var p = request.Payload;
            var notes = workspace.Notes.ToList(); var reminders = workspace.Reminders.ToList();
            var next = workspace with { Revision = workspace.Revision + 1, Notes = notes, Reminders = reminders };
            switch (request.Type)
            {
                case "read": return new(request.RequestId, true, Read());
                case "note.create":
                    notes.Insert(0, new(Guid.NewGuid(), 0, "", Text(p, "account", 100), "", DateTimeOffset.Now)); break;
                case "note.save":
                    var id = p.GetProperty("id").GetGuid();
                    var index = notes.FindIndex(n => n.Id == id);
                    if (index < 0) throw new InvalidOperationException("This note no longer exists.");
                    if (notes[index].Revision != p.GetProperty("revision").GetInt32())
                        throw new InvalidOperationException("The note has changed. Your draft is retained; reopen the saved note before retrying.");
                    notes[index] = notes[index] with { Revision = notes[index].Revision + 1, Title = Text(p, "title", 500), Account = Text(p, "account", 100), Body = Text(p, "body", 1_000_000), Updated = DateTimeOffset.Now }; break;
                case "reminder.create":
                    var action = Text(p, "action", 4000).Trim();
                    if (action.Length == 0) throw new InvalidOperationException("Give the reminder an action.");
                    var due = p.GetProperty("due").GetDateTimeOffset();
                    reminders.Add(new(Guid.NewGuid(), action, Text(p, "account", 100), due)); break;
                case "timer.toggle":
                    // Roll back both engine and workspace if the durable write fails.
                    var before = engine; engine = CopyEngine();
                    try
                    {
                        if (engine.Active is null)
                        {
                            var account = Text(p, "account", 100).Trim();
                            if (account.Length == 0) throw new InvalidOperationException("Choose an account to record.");
                            engine.ToggleAccount(account);
                        }
                        else engine.StopAccount();
                        next = next with { Sessions = [.. engine.Sessions], Active = engine.Active, LastSeen = now() };
                        store.Save(next); workspace = next;
                    }
                    catch { engine = before; throw; }
                    return new(request.RequestId, true, Read());
                default: throw new InvalidOperationException("Unknown command.");
            }
            store.Save(next); workspace = next;
            return new(request.RequestId, true, Read());
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or InvalidOperationException or JsonException or KeyNotFoundException or FormatException or ArgumentException)
        { return new(request.RequestId, false, Read(), e.Message); }
    }
    private TrackingEngine CopyEngine()
    {
        // The closure switches to the real clock after reconstructing the exact active start.
        var live = false;
        var final = new TrackingEngine(clock: () => live ? now() : engine.Active?.Start ?? now());
        final.Sessions.AddRange(engine.Sessions);
        if (engine.Active is { } value) final.ToggleAccount(value.Account);
        live = true; return final;
    }
    private static string Text(JsonElement p, string name, int limit)
    {
        var value = p.GetProperty(name).GetString() ?? "";
        if (value.Length > limit) throw new InvalidOperationException($"{name} is too long.");
        return value;
    }
}
