# Native accessibility and scaling check

21 November 2026. Scope: R18–R20 and O07/O13. **Compact rendering at 125% verified; accessible editor-host selection remains open.**

The experiment's own geometry diagnostic reported `RenderScaling = 1.25`. The test environment exposed one 3840×2560 display, with a 3840×2500 working area. These values came from Avalonia's native screen/window APIs. Screenshot dimensions alone were not used to infer DPI. No Windows display setting was changed.

## Results

| Check | Observed result |
| --- | --- |
| Native 600×760 client at 125% | Writing, account/title controls and action buttons remained usable. The WebView measured approximately 564.8×428.8 logical pixels before opening link fields. |
| Native 420×760 client at 125% | Writing remained readable and scrollable; controls wrapped. The original link row was cramped. It now places the address on its own full-width row, with text/Apply/Cancel below. The revised layout was checked in the native window. |
| Default renderer accessibility | Library/native controls exposed names and values. The rendered writing view still returned no accessibility tree in several desktop inspections, even with an explicit multiline text-box role/name. This is not a screen-reader pass. |
| Offscreen rendering comparison | With the pinned package's supported `ExperimentalOffscreen` option, native inspection exposed the note body, heading, quote, list and source hyperlink. However, the writing area remained visually blank after repeat observation. This mode cannot replace the working default on this evidence. |
| Browser editor semantics | The component regression verifies a named `Markdown note body` text box with `aria-multiline=true`, alongside the existing editing/selection checks. This supports the native comparison but does not close the host accessibility gate. |
| Physical 1080p and mixed DPI | Not tested. Only one display was exposed. The compact client dimensions were exercised at real 125% scaling, but that is not a substitute for physical 1080p work-area limits or moving between monitors at different scales. |

## Reproduction

Run the [native experiment](../../experiments/native-notes/README.md). F9 cycles its own client width through 940, 600 and 420 logical pixels; it does not change system scaling. `layout-diagnostics.json` in the chosen experiment data directory records client dimensions, actual render scale, WebView bounds and available screen dimensions. It contains no note content and is ignored with the rest of experiment data.

For the rendering comparison, add `--offscreen` when launching the built experiment DLL:

```powershell
./.tools/dotnet/dotnet.exe experiments/native-notes/bin/Debug/net10.0/Bat.Reset.NotesExperiment.dll --data-dir ./artifacts/offscreen-review --offscreen
```

The default remains the renderer that paints correctly. Offscreen is an explicitly requested diagnostic option, not the chosen application renderer.

## Decision implication

Follow-up: the [direct Microsoft WebView2 comparison](direct-webview2-host-comparison.md) now demonstrates visible writing and exposed native document structure together. It is the stronger Windows host candidate; this does not retroactively pass the Avalonia modes or establish full screen-reader validation.

Retain the Core timing/persistence findings and the keyboard-focus fix. Keep the desktop/editor-host choice open: the tested default paints correctly but has unreliable accessibility inspection, while the offscreen comparison exposes document semantics but fails to paint here. Resolve that boundary, or compare a different host/editor approach, before committing to this writing stack. An actual assistive-technology reading/navigation check and physical mixed-display validation are still needed.

References: [Avalonia accessibility guidance](https://docs.avaloniaui.net/docs/app-development/accessibility), [pinned offscreen option](https://github.com/AvaloniaUI/Avalonia.Controls.WebView/blob/3df9183123403dfb0d079f876bed03f8c8c78353/src/Avalonia.Controls.WebView.Core/Platform/WindowsWebView2EnvironmentRequestedEventArgs.cs), [earlier native feasibility findings](native-notes-feasibility.md).
