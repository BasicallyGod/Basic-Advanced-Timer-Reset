# Direct Microsoft WebView2 host comparison

21 November 2026. Scope: O07/O13 and R05/R16–R20. Recommendation: **take direct Microsoft WebView2 forward as the stronger Windows host candidate**, keeping the production shell/framework choice explicit.

## Controlled comparison

The new WinForms shell uses the same TOAST UI editor, CSS, Markdown input rules, bridge script and file-store source as the Avalonia experiment. It references the same pinned Core source commit. It has a separate executable/data identity and uses an explicit copy of fictional fixtures. This compares hosting behaviour; it does not replace the design prototype with a WinForms interface.

| Host | Visible writing | Native document inspection |
| --- | --- | --- |
| Avalonia default | Yes | Unreliable/no tree in repeated rendered-editor observations. |
| Avalonia offscreen | Blank in the tested environment | Body, heading, quote, list and link exposed. |
| Microsoft WinForms WebView2 | Yes | Named editable note body, heading, quote, list, source link and native controls exposed together while writing was visible. |

## Verified tasks

- The source link accepted typing immediately in its native address field; Enter applied it. The saved Markdown and a subsequent process restart retained the new URL under the same note ID.
- In a single running instance, Ctrl+Alt+N opened a blank document ready for typing. The Core timer readout continued from five to twelve seconds while capture text was entered. The saved file retained that text after closing/relaunching.
- F6 reached the title field: typing changed the title while leaving the Markdown body unchanged. Returning to writing preserved the document. The host uses the SDK's WinForms keyboard event for pane navigation.
- Native inspection exposed the visible document's structural elements and links, rather than requiring the editor to be hidden. Inspection focus metadata sometimes lagged, so keyboard effects were checked visually and against saved content as well.
- The project builds with zero warnings/errors. Public packages are locked. The existing shared-store/Core checks and editor component regression remain applicable; they do not substitute for native host checks.

An early test launched two processes against the fixture directory. Those runs were discarded for capture/timing conclusions; both were closed and the single-instance checks above were repeated. The final harness enforces one instance per data directory.

## What this resolves and what remains

This provides a Windows route that combines painting and exposed native document structure with the existing writing engine. It is sufficient to stop treating the Avalonia integration as the only viable route. Prefer direct WebView2 in the Windows implementation-plan candidate, and preserve the tested Core boundary.

Do not infer that the entire application should use the comparison shell's controls or layout. The final native shell, global window lifecycle and responsive product UI still need implementation decisions. An actual screen-reader reading/navigation session, full Tab/Shift+Tab behaviour, clipboard/IME, long-document performance, forced editor failures, physical 1080p limits and mixed-DPI movement remain unverified. The comparison is Windows-only and establishes no cross-platform claim.

Reproduction: [comparison setup](../../experiments/winforms-notes/README.md). Sources: [Microsoft's WinForms WebView2 guide](https://learn.microsoft.com/en-us/microsoft-edge/webview2/get-started/winforms), [WebView2 accessibility/API overview](https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/overview-features-apis), [WinForms keyboard API](https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.winforms.webview2), [earlier accessibility/scaling results](accessibility-and-scaling.md).
