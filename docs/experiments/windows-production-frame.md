# Windows production frame progress

21 November 2026. W01 status: **running development slice; acceptance gates remain open**. Relevant requirements: R01–R05, R11–R20, R29, R33–R35. Open contracts: O03/O07/O09/O11/O13.

The complete Windows frame now hosts Timesheets and Notes together. A prominent reminder action opens a short action/account/exact-time form. Quick note opens the Markdown writing surface directly, without requiring a title, account or time block. The timeline remains the dashboard anchor, with a meeting lane and reminder points. Its work/meeting blocks are fictional layout examples; they are excluded from the Core ledger. Reminders precede Recent work when the lower panels stack.

## Implemented boundaries

`Bat.Application` validates versioned commands, stable IDs and expected note revisions. It returns a saved snapshot only after persistence succeeds. The pinned Core engine owns live recording, elapsed values and stopped sessions. The UI receives these values rather than calculating credited hours. Creating, opening, filing and writing notes does not change the timer account or start.

`Bat.Persistence` writes a separate development JSON snapshot through a flushed pending file and atomic replacement, retaining the previous complete backup. An incomplete pending file is ignored on load. Corrupt primary data is preserved before backup recovery; unknown formats stop loading/writing. This is a disposable W01 format, not approval of the final Markdown/metadata or link schema under O07/O03.

The UI keeps an additional recovery draft in the isolated WebView profile, tracks acknowledgement/revision state and exposes Not saved plus Retry save after a failed write. Native Quit asks the UI to flush first, then checkpoints the clock; a failed save prevents quitting. The main header and pill expose Quit, and both window close buttons quit through the same save/checkpoint path. Minimise to pill explicitly keeps BAT running in the background. The provisional native pill provides Open, Reminder, Quick note and Quit; it has a taskbar entry while lifecycle/accessibility are validated. Native shortcuts are provisional Ctrl+Alt+N/R and report registration conflicts.

The host maps packaged assets to a fixed local HTTPS origin, denies cross-origin folder access, rejects other navigation/popups and permission requests, and accepts messages only from the application document. A restrictive content policy blocks network requests and embedded frames. The bridge offers named operations; it does not expose arbitrary native script execution. WebView's profile and recovery data remain in the development root.

## Development recording recovery

Recording checkpoints every 15 seconds. On restart the existing Core `Recover` method records through the last acknowledged checkpoint, then pauses and surfaces a recovery notice. Closed-app time is not silently recorded. This is a conservative development recovery choice; missed-checkpoint handling and installed-app parity still need review before adoption. It does not settle O01 planned work, held remainders, resume or conflicts.

## Verification

The native Windows build succeeds with zero warnings and errors after a fresh dependency restore. Fourteen executable application/persistence checks cover stable note/reminder IDs, revision rejection, independent Core timing, failed-write rollback for notes and recording, exact checkpoint recovery, repeated restart without double counting, incomplete-write isolation, damaged-file recovery, unknown-format preservation and unsupported bridge commands/versions.

The complete-frame browser regression uses a simulated host and covers independent reminder/note capture, immediate body focus, account/body-phrase retrieval, source-link editing, failed-save draft retention/retry, same-ID reload and 600/420-pixel layouts without page overflow. This complements the existing editor regression; it does not substitute for native evidence.

Native inspection of the complete frame confirmed visible writing and an exposed editable document together, source address/caption typing with Enter Apply, 600/420 logical-pixel writing layouts, persisted note/reminder content and a deliberate save failure followed by Retry save. The initial full-frame capture exposed a real native-focus defect: a DOM caret appeared without Windows keyboard focus. The host now acknowledges an explicit focus handoff before opening the editor; immediate typing was repeated successfully. Native accessibility inspection sometimes reports a stale focused element, so the visible typed result and persisted file were checked as well.

The full frame reports DeviceDpi 120 (125%) on one 3840×2560 display with a 3840×2500 work area. Its initial client size is 1375×1000 physical pixels, corresponding to 1100×800 logical pixels. After an explicit compact-window geometry/lifecycle correction, the native pill exposed Open, Reminder and Quick note buttons; clicking Open restored the main frame. Diagnostics confirmed that only the selected surface remained visible and that the pill lay within the monitor work area. This is one-monitor lifecycle evidence, not mixed-monitor validation.

22 November follow-up: the main window's × and the pill's visible Quit button were each exercised in the updated native build, and both processes exited with code 0. The complete-frame browser check verifies that a failed note save produces a negative quit acknowledgement and retains the draft; retrying Quit after storage recovers saves the writing before acknowledging exit. Header controls also remain usable in the 420/600-pixel regression layouts.

## Remaining acceptance work

- Real screen-reader reading/writing, full clipboard/IME/keyboard coverage, renderer failure/reload and long-document performance.
- Physical 1080p work-area fit and mixed-DPI movement; compact logical widths alone do not establish these.
- Native CI with authorised private Core access. The workflow builds/checks this frame when its existing private-Core gate is enabled; a skipped job is not native CI evidence.
- Review final document/metadata/recovery contracts before production storage. Older revision-mismatched local drafts are preserved but currently only reported, rather than offered through a complete conflict/recovery interface.
- Finish window placement, pill styling, single-instance activation, capture defaults and shortcut decisions under O09/O11. A second launch currently reports the existing workspace instead of activating it.
- Complete the remaining W01 interaction demonstration in the final build and review its visual direction with the user. Later Add time/direct manipulation, accepted wheel mover, desktop alerts, Notes organisation/relationships, migration and handoff stages remain in the implementation plan.

See [build and recovery instructions](../../src/README.md) and the [implementation plan](../implementation-plan.md). No installed BAT data has been read or migrated by this frame.
