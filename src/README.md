# Windows development frame

The first production-frame slice joins the responsive Timesheets/Notes UI, direct capture, recoverable development storage and the pinned Core timer. W01 remains in progress; this is not a replacement for the installed BAT app.

## Run

On Windows, first run `./tools/native-notes.ps1 setup` to prepare the pinned SDK and authenticated private Core checkout. This uses existing authorised Git access. Then:

```powershell
./tools/windows-frame.ps1 build
./tools/windows-frame.ps1 check
./tools/windows-frame.ps1 run
```

The default data root is the separate local-app-data directory `BAT.Reset.Development`. For disposable review data, use `./tools/windows-frame.ps1 run -DataDirectory artifacts/frame-review`. Never point it at installed BAT data. The script verifies Core commit `361d0e32748be7c01e616d91176a844f805e8a08` and restores the locked WebView2 dependency.

`npm run test:frame` runs the complete UI flow with a simulated host; `windows-frame.ps1 check` runs the actual application/persistence/Core checks. Native UI evidence is recorded separately in the [frame report](../docs/experiments/windows-production-frame.md).

## Boundaries

- `Bat.Application`: commands, envelope/version validation, note revisions, snapshot acknowledgements and Core timing orchestration. It does not know about windows or the DOM.
- `Bat.Persistence`: atomic development snapshots, previous-complete backup recovery and unknown-format preservation. Its format is explicitly provisional under O07.
- `Bat.Windows`: direct WebView2, packaged local-origin assets, native windows/tray, capture shortcuts, single-workspace instance and lifecycle.
- `Bat.Ui`: responsive frame, Markdown writing, local recovery drafts, selection and view state. It receives authoritative clock values from C#.

The development editor started from the verified experiment and uses the existing vendored TOAST UI asset. It now has its own full-frame focus/interaction checks; experiments remain reproducible. No production database or legacy schema is opened or migrated.

## Review controls and recovery

Ctrl+Alt+N opens the full note editor; Ctrl+Alt+R opens the independent reminder form. Bindings are provisional under O11, and registration conflicts produce a tray notice. F6 moves between writing and note title. Ctrl+K edits a source link. F9 cycles 1100/600/420 logical-pixel review widths. Ctrl+Shift+Q or the tray Quit command flushes writing before quitting; the close button hides BAT to its provisional pill. The pill has a taskbar entry during this validation stage.

Creating a file named `simulate-save-failure` inside the selected development data directory makes writes fail deliberately. The editor retains its draft, reports Not saved, and exposes Retry save. Remove only that marker and retry. Recovery drafts are scoped to this app's private WebView profile; acknowledged Markdown and metadata live in `workspace.dev.json`. Incomplete `.pending` files never replace a committed workspace. A corrupt primary is retained separately before a complete `.bak` is recovered. Unknown formats stop loading/writing and preserve files.

Active recording checkpoints every 15 seconds. On restart, the existing Core recovery method records only up to the last acknowledged checkpoint, then pauses and shows a notice. It does not infer hours while BAT was closed. This development recovery policy is not approval of O01's planned-work, pause/remainder or overlap proposals.

Timeline work/meeting examples are fictional and excluded from recorded totals. Actual Core sessions appear under Recent work. Manual entry/editing, planned accounting, reminder completion/rescheduling/desktop alerts, note relationships/tags, migration and exports remain later stages. The date/time field is an initial exact typed control, not a replacement for the accepted hour/minute wheel mover. See the [implementation plan](../docs/implementation-plan.md) for acceptance gates.
