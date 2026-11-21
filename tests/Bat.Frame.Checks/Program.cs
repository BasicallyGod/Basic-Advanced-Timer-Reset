using System.Text.Json;
using Bat.Application;
using Bat.Persistence;

var root = Path.Combine(Path.GetTempPath(), "Bat.Frame.Checks-" + Guid.NewGuid());
Directory.CreateDirectory(root);
try
{
    var now = new DateTime(2026, 11, 21, 9, 0, 0, DateTimeKind.Local);
    var service = new WorkspaceService(new DevelopmentStore(root), () => now);
    Reply Send(string type, object payload) => service.Execute(new(1, Guid.NewGuid().ToString(), type, JsonSerializer.SerializeToElement(payload, WorkspaceService.Json)));
    void Check(bool pass, string description) { if (!pass) throw new Exception(description); Console.WriteLine("PASS: " + description); }
    Check(Send("timer.toggle", new { account = "Acme" }).Ok, "Core recording starts");
    var start = service.Read().Workspace.Active!.Start;
    now = now.AddMinutes(12);
    var create = Send("note.create", new { account = "" }); var note = create.State.Workspace.Notes.Single();
    var save = Send("note.save", new { id = note.Id, revision = 0, title = "", account = "Beacon", body = "## Call\n\n> A useful quote.\n\n- Chase approval\n\n[Source](https://example.com)" });
    Check(save.Ok && save.State.Workspace.Notes.Single().Revision == 1, "stable note identity and revision acknowledgement");
    Check(!Send("note.save", new { id = note.Id, revision = 0, title = "stale", account = "", body = "stale" }).Ok, "stale note cannot overwrite acknowledged content");
    var reminder = Send("reminder.create", new { action = "Chase approval", account = "Beacon", due = "2026-11-21T15:00:00+00:00" });
    Check(reminder.Ok && reminder.State.Workspace.Reminders.Single().Account == "Beacon", "reminder is independent of full notes and timer account");
    Check(service.Read().Workspace.Active!.Start == start && service.Read().ActiveAccount == "Acme" && service.Read().ElapsedSeconds == 720, "writing and reminder capture leave Core recording unchanged");
    var before = File.ReadAllText(Path.Combine(root, "workspace.dev.json"));
    File.WriteAllText(Path.Combine(root, "simulate-save-failure"), "");
    var failed = Send("note.save", new { id = note.Id, revision = 1, title = "lost", account = "Beacon", body = "draft" });
    Check(!failed.Ok && service.Read().Workspace.Notes.Single().Body.Contains("quote") && File.ReadAllText(Path.Combine(root, "workspace.dev.json")) == before, "failed write changes neither acknowledged memory nor durable note");
    Check(!Send("timer.toggle", new { account = "Acme" }).Ok && service.Read().ActiveAccount == "Acme", "failed timer command rolls back recording state");
    File.Delete(Path.Combine(root, "simulate-save-failure"));
    service.Checkpoint(); now = now.AddHours(1);
    var reopened = new WorkspaceService(new DevelopmentStore(root), () => now);
    Check(reopened.Read().Workspace.Notes.Single().Id == note.Id && reopened.Read().Workspace.Reminders.Single().Id == reminder.State.Workspace.Reminders.Single().Id, "restart preserves note and reminder identity");
    Check(reopened.Read().ActiveAccount is null && reopened.Read().Workspace.Sessions.Sum(r => r.Duration.TotalMinutes) == 12, "Core recovery records only through checkpoint, excluding closed-app time");
    var twice = new WorkspaceService(new DevelopmentStore(root), () => now);
    Check(twice.Read().Workspace.Sessions.Sum(r => r.Duration.TotalMinutes) == 12, "restarting twice cannot double-record recovered time");
    File.WriteAllText(Path.Combine(root, "workspace.dev.json.pending"), "{incomplete");
    Check(new DevelopmentStore(root).Load()!.Notes.Single().Id == note.Id, "incomplete pending write cannot replace committed workspace");
    File.Copy(Path.Combine(root, "workspace.dev.json"), Path.Combine(root, "workspace.dev.json.bak"), true);
    File.WriteAllText(Path.Combine(root, "workspace.dev.json"), "{broken");
    Check(new DevelopmentStore(root).Load()!.Notes.Single().Id == note.Id, "corrupt primary recovers complete backup and preserves damaged file");
    File.WriteAllText(Path.Combine(root, "workspace.dev.json"), "{\"format\":99}");
    var unknown = new DevelopmentStore(root);
    try { unknown.Load(); throw new Exception("unknown format accepted"); } catch (InvalidOperationException) { }
    try { unknown.Save(twice.Read().Workspace); throw new Exception("unknown format overwritten"); } catch (InvalidOperationException) { }
    Check(File.ReadAllText(Path.Combine(root, "workspace.dev.json")) == "{\"format\":99}", "unknown format preserved and writes blocked");
    Check(!Send("unknown", new { }).Ok && !service.Execute(new(99, "bad", "read", default)).Ok, "unknown commands and bridge versions rejected");
}
finally { Directory.Delete(root, true); }
