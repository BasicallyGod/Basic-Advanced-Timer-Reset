# Native Notes feasibility experiment

This is a runnable Windows experiment for R05 and R16–R20, with evidence for O07/O13. It evaluates Avalonia 12.1.6, NativeWebView 12.1.2 and the existing TOAST UI Editor 3.2.2 inside a .NET 10 desktop shell. These are experiment versions, not a final application architecture decision.

## Setup and run

On Windows, from the repository root in PowerShell:

```powershell
./tools/native-notes.ps1 setup
./tools/native-notes.ps1 check
./tools/native-notes.ps1 run
```

Setup downloads Microsoft's official SDK 10.0.404 into ignored `.tools/dotnet`, checks out shared Core commit `361d0e32748be7c01e616d91176a844f805e8a08` into `.tools/core-source`, and restores pinned packages from NuGet. It needs Git and internet access. The experiment references that pinned source directly, so no private NuGet credentials are needed. Windows needs the Edge WebView2 runtime. No system PATH or system SDK is changed by the script.

The default data directory is `%LOCALAPPDATA%/BAT.Reset.NotesExperiment`. To choose an isolated review folder:

```powershell
./tools/native-notes.ps1 run -DataDirectory ./artifacts/my-notes-review
```

The executable is `Bat.Reset.NotesExperiment`. It never opens the installed app's database. Its JSON format and one sample work-link record are disposable experiment structures, not the proposed production schema. Notes and links persist across process restarts. The live timer, due-reminder harness and navigation state do not persist across restart.

## Try the flow

1. Start the timer, switch to another app and press Ctrl+Alt+N. Capture opens the complete writing surface with focus ready. The binding is an experiment, not the final O11 shortcut choice. A capture has no account until explicitly assigned; the timer account does not silently become note context.
2. Write a note. Title and optional account are outside the writing surface. Use Markdown input or Ctrl+B; the optional Formatting controls provide a comparison. Pasting a Markdown document currently inserts literal text; use Plain text to compare source entry and return to rendered editing.
3. Close the note with Back or Escape. Search its body text in Notes. Open it again or link it to the sample client call, then reopen through Timesheets. The sample work record's identity and description stay independent of the note.
4. Close and relaunch the process against the same experiment directory to check persistence. Resize to a narrow window. Check keyboard focus, clipboard, selection, undo and source links yourself.

F6 moves from writing to the note title and back to writing from the native controls. Tab/Shift+Tab move through those controls. Clicking a source link or pressing Ctrl+K opens link fields with keyboard focus in the address; Enter applies and Escape cancels. These are experiment bindings, not final O11 shortcut choices. See the focus regression results in the feasibility report.

F9 cycles native widths (940/600/420). The local `layout-diagnostics.json` records actual DPI and screen/window dimensions without note content. The optional `--offscreen` launch flag supports the rendering comparison; it remains disabled by default. See [accessibility and scaling results](../../docs/experiments/accessibility-and-scaling.md) before using that mode.

Copy text currently copies Markdown. Plain text is a fallback editor for comparison; switching between source and rendered editing resets the editor's undo history. Full rich/plain comparison, clean Word copying and production export remain open under O07.

## Verification and limits

See [the feasibility report](../../docs/experiments/native-notes-feasibility.md) for observed results and unresolved gates. The console checks exercise real file persistence/recovery, search/link identity and the pinned Core clock without a VSTest host. They do not substitute for native focus, accessibility or scaling checks.

The due reminder is a 30-second in-process harness. It demonstrates that attention state still updates during writing. It is not a Windows toast, production scheduler, rescheduler or sleep/restart delivery test. The fixed client-call card is a link test, not a new Timesheets dashboard design.

The editor loads local files with network requests disabled by a content policy and does not navigate external source links. Clicking a link or pressing Ctrl+K opens explicit native text/address fields; Apply changes the selected link. This route still needs fuller native keyboard/clipboard validation. Error and recovery messages must be resolved before treating this route as production-ready.
