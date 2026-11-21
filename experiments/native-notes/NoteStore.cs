using System.Text.Json;

namespace Bat.Reset.NotesExperiment;

public sealed record NoteDocument(Guid Id, string Title, string Account, string Body, DateTimeOffset UpdatedAt);
public sealed record WorkLink(Guid Id, Guid? NoteId, string Description);
public sealed record ExperimentData(int Format, List<NoteDocument> Notes, List<WorkLink> Links);

// Feasibility-only file store, independent of BAT's shared production schema.
public sealed class NoteStore
{
    private readonly string path;
    private readonly object gate = new();
    public ExperimentData Data { get; private set; }
    public bool RecoveredBackup { get; private set; }
    public string? LoadWarning { get; private set; }
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public NoteStore(string directory)
    {
        Directory.CreateDirectory(directory);
        path = Path.Combine(directory, "notes-experiment.json");
        Data = Load();
    }

    private ExperimentData Load()
    {
        if (!File.Exists(path)) return new(1, [], [new(Guid.NewGuid(), null, "Client call · Acme")]);
        try { return Read(path); }
        catch (Exception e) when (e is IOException or JsonException or InvalidDataException)
        {
            if (File.Exists(path + ".backup"))
            {
                try { RecoveredBackup = true; return Read(path + ".backup"); }
                catch (Exception backup) when (backup is IOException or JsonException or InvalidDataException) { }
            }
            LoadWarning = "The saved notes could not be read. The file has been preserved; use a different experiment data folder to continue.";
            return new(1, [], []);
        }
    }

    private static ExperimentData Read(string filename)
    {
        var data = JsonSerializer.Deserialize<ExperimentData>(File.ReadAllText(filename), Json);
        if (data is null || data.Format != 1 || data.Notes is null || data.Links is null || data.Notes.Any(n => n.Body is null || n.Title is null || n.Account is null))
            throw new InvalidDataException("Invalid experiment document.");
        return data;
    }

    public NoteDocument Create(string account = "")
    {
        lock (gate)
        {
            var note = new NoteDocument(Guid.NewGuid(), "", account, "", DateTimeOffset.UtcNow);
            Data.Notes.Add(note);
            Save();
            return note;
        }
    }

    public NoteDocument Update(Guid id, string title, string account, string body)
    {
        lock (gate)
        {
            var index = Data.Notes.FindIndex(n => n.Id == id);
            if (index < 0) throw new KeyNotFoundException("Note not found.");
            var note = new NoteDocument(id, title, account, body, DateTimeOffset.UtcNow);
            Data.Notes[index] = note;
            Save();
            return note;
        }
    }

    public void Link(Guid blockId, Guid noteId)
    {
        lock (gate)
        {
            if (!Data.Notes.Any(n => n.Id == noteId)) throw new KeyNotFoundException("Note not found.");
            var index = Data.Links.FindIndex(b => b.Id == blockId);
            if (index < 0) throw new KeyNotFoundException("Work block not found.");
            Data.Links[index] = Data.Links[index] with { NoteId = noteId };
            Save();
        }
    }

    public IEnumerable<NoteDocument> Search(string account, string phrase) => Data.Notes
        .Where(n => (account == "All accounts" || n.Account == account) &&
            (n.Title.Contains(phrase, StringComparison.OrdinalIgnoreCase) || n.Body.Contains(phrase, StringComparison.OrdinalIgnoreCase)))
        .OrderByDescending(n => n.UpdatedAt);

    private void Save()
    {
        if (LoadWarning is not null) throw new IOException(LoadWarning);
        // Flush the new snapshot before atomically replacing the previous complete file.
        // A crashed write can leave .pending; reopening deliberately uses the last committed file.
        var pending = path + ".pending";
        using (var stream = new FileStream(pending, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            JsonSerializer.Serialize(stream, Data, Json);
            stream.Flush(flushToDisk: true);
        }
        if (File.Exists(path)) File.Replace(pending, path, path + ".backup", ignoreMetadataErrors: true);
        else File.Move(pending, path);
    }
}
