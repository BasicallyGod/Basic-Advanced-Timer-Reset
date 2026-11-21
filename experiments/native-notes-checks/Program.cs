using Bat.Reset.NotesExperiment;
using Timer.Core;

var root = Path.Combine(Path.GetTempPath(), "bat-notes-checks-" + Guid.NewGuid());
Directory.CreateDirectory(root);
var checks = 0;
void Check(bool condition, string name) { if (!condition) throw new Exception("FAIL: " + name); checks++;Console.WriteLine("PASS: " + name); }
var store = new NoteStore(root);
var note = store.Create();
Check(note.Account == "" && note.Body == "", "capture requires no title or account");
const string body = "## Decisions\n\n> A launch people can join.\n\n- Confirm neighbourhood champions.\n- Chase approval Thursday.\n\n[Source](https://example.com/research)";
store.Update(note.Id, "Client call", "Acme", body);
var reopened = new NoteStore(root);
Check(reopened.Data.Notes.Single().Body == body && reopened.Data.Notes.Single().Id == note.Id, "reopen preserves Markdown and stable identity");
Check(reopened.Search("Acme", "NEIGHBOURHOOD CHAMPIONS").Single().Id == note.Id && !reopened.Search("Beacon", "champions").Any(), "account and case-insensitive body search");
var block = reopened.Data.Links.Single();reopened.Link(block.Id, note.Id);
Check(new NoteStore(root).Data.Links.Single() == block with { NoteId = note.Id }, "link preserves work identity and short description");
File.WriteAllText(Path.Combine(root,"notes-experiment.json.pending"), "{ incomplete write");
Check(new NoteStore(root).Data.Notes.Single().Body == body, "incomplete pending write cannot replace the committed document");
File.WriteAllText(Path.Combine(root,"notes-experiment.json"), "{ damaged committed file");
var recovered = new NoteStore(root);
Check(recovered.RecoveredBackup && recovered.Data.Notes.Single().Body == body, "damaged primary recovers previous complete backup");
var invalid = Path.Combine(root,"unknown-format");Directory.CreateDirectory(invalid);
File.WriteAllText(Path.Combine(invalid,"notes-experiment.json"),"{\"Format\":99,\"Notes\":[],\"Links\":[]}");
var blocked = new NoteStore(invalid);var protectedFile=false;
try{blocked.Create();}catch(IOException){protectedFile=true;}
Check(protectedFile && File.ReadAllText(Path.Combine(invalid,"notes-experiment.json")).Contains("99"), "unknown format is preserved instead of overwritten");
var instant = new DateTime(2026,11,20,10,0,0);var engine = new TrackingEngine(clock:()=>instant);
engine.ToggleAccount("Acme");var active = engine.Active;
instant=instant.AddMinutes(12);
reopened.Update(note.Id,"Client call amended","Acme",body+"\nAdditional decision.");
_ = reopened.Search("Acme","decision").ToList();reopened.Link(block.Id,note.Id);
Check(engine.Active==active && engine.ActiveElapsed==TimeSpan.FromMinutes(12), "note editing, search and linking leave live Core timing unchanged");
engine.StopAccount();
Check(engine.Sessions.Count==1 && engine.Sessions.Single().Duration==TimeSpan.FromMinutes(12), "Core records the exact twelve elapsed minutes once");
Console.WriteLine($"{checks} checks passed. Test data: {root}");
