# 0002 — Windows-first delivery and platform boundaries

Date: 21 November 2026.

Status: **Windows-first priority confirmed by the user; production composition proposed for W01 validation.**

Cross-platform support is a distant possibility. Prioritise Windows delivery while keeping timing, application contracts and storage independent of native window/host types. Retain replaceable platform and editor-host boundaries without building speculative macOS/Linux implementations.

Direct Microsoft WebView2 is the preferred Windows host candidate following the [controlled comparison](../experiments/direct-webview2-host-comparison.md). It displayed the shared editor and exposed native document structure together. The Avalonia comparison's current render/accessibility conflict remains recorded; its earlier success on timing, persistence and compact layout is still useful evidence.

The implementation plan proposes .NET 10 application services, a thin WinForms Windows host and modular responsive web UI using direct WebView2. The thin host owns native lifecycle/integrations, and the UI follows the product design rather than the test shell's controls. W01 must prove the complete frame/pill, packaged assets, command boundary, accessibility and lifecycle before this becomes the production scaffold baseline.

Reuse the pinned Core through explicit application services. Assess shared Storage separately before selecting production document/link storage. Keep authoritative time calculation out of the web client. The experiment's JSON format, sample blocks, shortcut bindings and controls do not define final schemas or product policy.

Consequences: Windows integrations can move forward using the tested host route. A future port will still need another platform host and native validation. Private Core access must be configured explicitly for native CI; repository visibility and credentials are not changed by this decision. Remaining product and adoption gates are listed in the [implementation plan](../implementation-plan.md).
