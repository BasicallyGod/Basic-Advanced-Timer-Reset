using System.Text.Json;
using Bat.Application;

namespace Bat.Persistence;

// W01 only. This is not the legacy database or an approved production schema.
public sealed class DevelopmentStore(string directory) : IWorkspaceStore
{
    private readonly string file = Path.Combine(directory, "workspace.dev.json");
    private bool blocked;
    public string? Notice { get; private set; }
    public Workspace? Load()
    {
        Directory.CreateDirectory(directory);
        if (!File.Exists(file)) return null;
        try { return Parse(file); }
        catch (JsonException)
        {
            // Retain the damaged primary for inspection; recover only a complete known backup.
            if (!File.Exists(file + ".bak")) throw;
            var recovered = Parse(file + ".bak");
            File.Copy(file, file + ".damaged-" + Guid.NewGuid().ToString("N"));
            File.Copy(file + ".bak", file, true);
            Notice = "Recovered the development workspace from its previous complete backup.";
            return recovered;
        }
    }
    private Workspace Parse(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        if (doc.RootElement.GetProperty("format").GetInt32() != 1)
        { blocked = true; throw new InvalidOperationException("Unknown workspace format. Files have been preserved; open them with the matching BAT build."); }
        var data = doc.Deserialize<Workspace>(WorkspaceService.Json) ?? throw new JsonException("Empty workspace.");
        if (data.Notes is null || data.Reminders is null || data.Sessions is null || data.Revision < 0
            || data.Notes.Any(n => n.Id == Guid.Empty || n.Revision < 0 || n.Body is null || n.Title is null || n.Account is null)
            || data.Notes.Select(n => n.Id).Distinct().Count() != data.Notes.Count
            || data.Reminders.Any(r => r.Id == Guid.Empty || r.Action is null || r.Account is null)) throw new JsonException("Invalid workspace records.");
        return data;
    }
    public void Save(Workspace workspace)
    {
        if (blocked) throw new InvalidOperationException("Workspace writes are blocked to preserve an unknown format.");
        // Deterministic failure exercise; the last acknowledged file remains unchanged.
        if (File.Exists(Path.Combine(directory, "simulate-save-failure"))) throw new IOException("Development save failure enabled. Keep this window open and retry after removing the failure marker.");
        var bytes = JsonSerializer.SerializeToUtf8Bytes(workspace, WorkspaceService.Json);
        using (var stream = new FileStream(file + ".pending", FileMode.Create, FileAccess.Write, FileShare.None))
        { stream.Write(bytes); stream.Flush(true); }
        if (File.Exists(file)) File.Replace(file + ".pending", file, file + ".bak");
        else File.Move(file + ".pending", file);
    }
}
