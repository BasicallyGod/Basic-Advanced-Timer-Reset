# Native Notes feasibility — first slice

21 November 2026. Status: **native focus regression fixed; broader stack gate remains open**. Scope: R05, R16–R20 and evidence for O07/O13. No product policy or production database schema is selected by this experiment.

## Candidate under test

An Avalonia desktop shell owns note identity, disk saving, search, explicit work links and the Core clock. A local NativeWebView hosts the existing readable Markdown editor. A native plain-text editor provides the fallback comparison. This keeps the two-stage note-creation flow out of capture and gives writing the main content area.

The experiment uses .NET SDK 10.0.404, Avalonia 12.1.6, Avalonia.Controls.WebView 12.1.2 and TOAST UI Editor 3.2.2. The first native run used the cached Core 0.10.1 package; reproducible builds reference its verified source commit `361d0e32748be7c01e616d91176a844f805e8a08` directly. Package locks pin the public dependencies. This is a test of a candidate, not approval to migrate the app to these versions.

## Evidence gathered

| Check | Result and limit |
| --- | --- |
| Compile | Native app builds with zero warnings/errors on the repo-local .NET 10 SDK. |
| Global capture | Ctrl+Alt+N invoked from a separate application opened the full editor and accepted typing immediately. The running Acme timer continued; the new note stayed unassigned. Final shortcut choice remains O11. |
| Writing and autosave | Text entry saved to the experiment directory without a Save button. Bold and undo worked through real Windows keyboard input. A note reopened by a phrase from its body while timing continued. |
| Readable Markdown and fallback | Source headings, quote, bullets and a link rendered correctly after native plain-text editing. Pasting a whole Markdown note into the rendered editor inserted literal text instead of interpreting its syntax. This needs a deliberate paste policy and further comparison. |
| Work links | A captured note linked to the fixed client-call sample, exposing Open linked note in Timesheets. File inspection confirmed the same note ID and unchanged work-link identity/description. This does not test full time-entry UI or accounting policies. |
| Restart | Both captured notes and the work link survived process close/relaunch against the same test directory. Timer state and navigation state are deliberately not restored across process restart in this harness. |
| Persistence and Core checks | Nine executable checks pass: optional capture metadata, exact Markdown/ID reopening, account/body search, stable work links, incomplete pending write, backup recovery, unknown-format preservation, uninterrupted Core timing during note operations, and exactly 12 recorded minutes after stopping. These run without VSTest; they do not resolve the legacy app's test-host failure. |
| Editor component regression | The browser component check passes rendering, in-list amendment, editing a source link, formatting undo, context restoration and immediate capture, with no external requests. This checks the embedded editor code; it is not native WebView accessibility proof. |
| Attention continuity | The native timer readout and due-reminder row continued updating while writing. The reminder is an in-process 30-second harness; Windows toast delivery, scheduling after sleep/restart and rescheduling remain untested. |
| Accessibility | Desktop inspection returned native shell/source controls reliably but no accessibility text during several focused rendered-editor observations. The child document appeared when the editor was hidden. An ARIA label was corrected to target the visible editor. Recheck with actual assistive technology and the final build; do not call this gate passed. |
| Initial native link-field failure | Ctrl+A initially selected the note even though the address field appeared focused. Explicit field clicking worked. The subsequent focus-boundary fix and native verification below resolve this specific regression; broader accessibility and keyboard gates remain open. |

## Focus-boundary follow-up

The pinned Windows adapter implements `ResignFocus()` as an empty method. Avalonia could mark the address field focused while the child WebView retained Win32 keyboard focus. The experiment now reclaims only its own window's keyboard focus when a native Avalonia control gains logical focus. WebView controls and their children are excluded from that handoff. The change uses the existing package and does not patch its source or depend on a timed delay.

Windows keyboard verification after the fix:

| Scenario | Observed result |
| --- | --- |
| Click source link, Ctrl+A, type URL | The address was selected and replaced immediately, without another field click. The note remained intact. |
| Enter to apply | Writing regained focus; the saved Markdown contained the amended URL. The note ID, other text and linked work description were unchanged. |
| F6, Tab, F6 | F6 reached the title; Tab reached the account field; F6 returned to the original writing selection. |
| Ctrl+K, type provisional URL, Escape | The fields closed and writing regained focus. The provisional URL did not enter the saved note; the previously applied URL remained. |
| Accessibility inspection | Inspection still returned no accessibility text in several rendered-editor observations. The keyboard fix does not close the accessibility gate. |

The component regression also verifies that the F6 handoff leaves Markdown unchanged and restores the editor selection. The real Win32 handoff is established by the Windows interaction checks above, not by that browser test. Full Tab-boundary behaviour, assistive-technology checks and mixed-DPI validation remain outstanding.

Source evidence: [the pinned adapter's empty ResignFocus method](https://github.com/AvaloniaUI/Avalonia.Controls.WebView/blob/3df9183123403dfb0d079f876bed03f8c8c78353/src/Avalonia.Controls.WebView.Core/Win/WebView2/WebView2BaseAdapter.cs#L256), [the pinned Avalonia focus handling](https://github.com/AvaloniaUI/Avalonia.Controls.WebView/blob/3df9183123403dfb0d079f876bed03f8c8c78353/src/Avalonia.Controls.WebView/NativeWebView.cs#L619), and [Microsoft's SetFocus API](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setfocus).

## Gates before a stack decision

1. Resolve rendered-editor accessibility and complete keyboard traversal, clipboard and IME validation in Windows. The specific link-address focus failure is fixed and native Apply/Cancel checks pass. The browser regression remains supporting evidence only.
2. Compare the rendered editor and native source/preview workflow with the same writing tasks. Switching to source currently resets rendered-editor undo history; decide whether that is acceptable for an optional fallback. Test pasted Markdown, pasted source links, mid-list edits and undo after leaving/reopening a note.
3. Native 600/420-pixel windows have now been checked at actual 125% scaling; see [the follow-up results](accessibility-and-scaling.md). Physical 1080p work-area limits and mixed-DPI movement remain unverified because only one higher-resolution display was exposed. Browser widths do not close those remaining checks.
4. Exercise large notes, rapid repeated capture, editor/process failure and forced interruption during saving. The atomic store tests cover complete snapshot recovery, not every editor-to-host failure or production recovery policy.
5. Choose document/link storage and migration only after O03/O06/O07 and accounting decisions are ready. The experiment's JSON store and fixed sample block cannot become the production schema by default.

The recommendation is to retain this candidate for further evaluation, with particular attention to the host boundary and accessibility. The native source fallback is useful for diagnosis; its reliable inspection alone does not establish an inviting writing experience. A final stack selection and implementation plan should follow the remaining evidence.

## Run and inspect

See [experiment setup and scenarios](../../experiments/native-notes/README.md). Data, profiles, screenshots and SDK/source caches are ignored by Git. The experiment has its own executable and data identity and does not open the installed BAT database.

Sources: [Avalonia embedding guide](https://docs.avaloniaui.net/docs/app-development/embedding-web-content), [NativeWebView API](https://docs.avaloniaui.net/controls/web/nativewebview), [WebView package](https://www.nuget.org/packages/Avalonia.Controls.WebView/12.1.2), [Avalonia desktop package](https://www.nuget.org/packages/Avalonia.Desktop/12.1.6), [Microsoft local SDK installer](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script), [pinned Core source](https://github.com/BasicallyGod/Basic-Timer-Core/tree/361d0e32748be7c01e616d91176a844f805e8a08).
