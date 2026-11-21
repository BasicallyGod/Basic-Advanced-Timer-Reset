# BAT Windows-first implementation plan

21 November 2026. Planning baseline: design specification 1.17. Status: **ready to begin the production-frame stage; later stages retain explicit gates**.

The user has confirmed Windows-first delivery. Cross-platform support is a distant possibility. Keep platform dependencies behind clear boundaries; spend the initial implementation effort on dependable Windows behaviour and the agreed product flows.

The [design specification](design/BAT-design-specification.md) governs product intent. This plan defines sequence, boundaries and acceptance evidence. It does not resolve an open product decision merely by assigning it to a stage.

## Recommended implementation shape

Use .NET 10 for the application/services and direct Microsoft WebView2 for the Windows web-content host. Begin with a thin WinForms window host, following the [successful comparison](experiments/direct-webview2-host-comparison.md). Its standard test controls are not the production design: responsive web UI modules deliver the actual timeline, Notes and capture surfaces. Prove the complete window/pill composition in stage W01 before treating this shape as final.

The shared Core remains the starting point for timing/ledger reuse, pinned to the verified source. Adapt through application services; do not copy the old presentation/model wholesale. Shared Storage is a separate reuse assessment: schema 7 provenance is verified, but suitability for documents, links, recovery and migration is not established by the clock tests.

| Boundary | Owns | Must remain independent of |
| --- | --- | --- |
| Domain/application services | Work intervals, commands, stable identities, note/reminder relationships, reconciliation | Window handles, WebView controls and DOM state |
| Persistence | Versioned writes, transactions, recovery, search indexes, import/export | Visual selection and window layout |
| Windows platform services | Window/pill lifecycle, tray, shortcuts, startup, notifications, clipboard and monitor placement | Authoritative accounting and document contents |
| Editor/UI client | Writing, timeline interaction, previews, accessible controls and view state | Direct database/file access and authoritative time totals |
| Host bridge | Typed commands/results, document IDs, revisions, errors and readiness | Arbitrary script-to-native execution or UI-derived accounting |

The web client requests operations; C# validates and commits them. The UI receives reconciled snapshots or changes with stable IDs. Avoid independent JavaScript and C# implementations of recorded-hour calculations. Validate payloads, origin and document/revision context; load packaged local assets and deliberately handle external links. The local file experiment is not the final asset-delivery/bridge design.

Future ports can reuse domain logic, documents and substantial UI/editor work, while replacing platform services and the web-content host. A port will still need native accessibility, browser-engine and platform testing. No macOS/Linux adapter is initial scope.

Suggested production layout, created incrementally rather than as empty scaffolding:

```text
src/Bat.Application/       commands, contracts and orchestration
src/Bat.Persistence/       storage, recovery, search and migration adapters
src/Bat.Windows/           composition root, windows and platform services
src/Bat.Ui/                modular UI/editor assets and typed bridge client
tests/                    behavioural, persistence, bridge and UI checks
```

Keep the Core dependency outside the Windows/UI modules. Introduce a separate domain project only where real responsibilities justify it. Experiments remain reproducible references under `experiments/`.

## Delivery stages

| ID | Reviewable outcome | Relevant design | Exit evidence / dependency |
| --- | --- | --- | --- |
| W01 | Production frame and shared state: dashboard/Notes navigation, independent quick reminder and full-editor quick note, one persisted note, reminder and running timer | R01–R05, R16–R20, R29, R33–R35; O07/O09/O11/O13 | Complete window paints and exposes useful native document structure together; capture, restart recovery, error display and focus restoration work. Isolated executable/data identity and authenticated native CI are established. |
| W02 | Trusted time entry and ledger integration: one Add time form, exact editing, elapsed/future distinction, descriptions, live recording and reversible conflicts | R04, R06–R10, R31, R34; O01/O02 | Resolve pause/resume/remainder/overlap policy before authoritative planned transitions. Reconcile manual/live/planned totals with deterministic clock tests, interruption and day-boundary cases. |
| W03 | Working-day dashboard and direct manipulation: inviting populated overview, primary timeline, meeting lane, reminder markers, bounded queue and selected context | R08–R13, R28–R29; O04/O08/O09 | Drag/move/resize and precise keyboard edits act on the same records. Morning/Afternoon/Full day pan/zoom preserves identity and selection; reduced motion works. Stacked reminders precede Recent work. |
| W04 | Dependable reminders: fast action/account/time capture, compact exact mover, completion/Undo and real Windows delivery | R03, R11–R15, R34–R35; O04/O05/O10/O11 | Wheel over hours/minutes, typing and date rollover are tested. Due state survives missed toasts, sleep, restart and clock/timezone changes under an explicit policy. A toast never becomes the sole record of outstanding work. |
| W05 | Useful Notes library: account home, optional organisation, title/body search with snippets, note links and searchable linking to work/meetings/reminders | R01–R02, R16–R24, R34; O03/O06/O07 | Retrieve a note by account/body phrase, edit the same document through every route, and restore search/selection. Test large libraries, duplicate titles, archive/rename/deletion rules and interrupted autosave. |
| W06 | Agency handoff and review: day/week entries, copyable short descriptions, account/project/code grouping, transfer tracking and budget periods | R25–R32; O02/O08 | Every view reconciles to the same recorded ledger. Full notes remain optional supporting material. Float-style weekly manual transfer is usable beside BAT without requiring a direct integration. |
| W07 | Migration and daily-use pilot | R33–R35; O03/O07/O08/O10 | Import copies of representative legacy data, preserve descriptions/context and reconcile totals. Exercise background operation, physical 1080p, mixed DPI and actual assistive technology. Demonstrate backup/restore and a safe return to the original app. |
| W08 | Release readiness and character | R03, R28–R29, R35; O12/O13 | Packaging, update/data compatibility, accessibility, performance, project licence and support/build documentation are ready. Add agreed seasonal touches/Easter eggs with visibility/reduced-motion rules. Release criteria distinguish outstanding feature parity from deferred enhancements. |

This is dependency order, not a commitment to eight isolated development silos. Shared contracts precede features that use them. Reminder delivery and Notes recovery need early technical work even where their full interfaces arrive later. Pilot the manual-timesheet/capture loop before declaring broad feature parity.

## W01: the next concrete work package

1. Create the Windows application, application-service contracts and packaged UI build using supported pinned dependencies. Give development builds their own identity/data root. Keep both host experiments available for comparison.
2. Build the actual responsive frame from the design direction: timeline remains the Timesheets anchor; Notes has room to write; quick reminder and quick note remain independent. Use fictional, populated examples initially.
3. Define versioned note/reminder IDs, write acknowledgements, editor readiness, revision checks and recoverable save errors. Implement direct capture, background-to-foreground focus, same-note reopening and a running Core clock through those contracts.
4. Use a recoverable development store while production contracts are reviewed. Record the proposed Markdown/metadata boundary under O07. Keep deletion, migration and unresolved link operations unavailable until their rules are settled.
5. Add an accessible, keyboard-operable flow through capture, link fields, navigation and writing. Test clipboard/selection/undo, source-link editing and a real screen-reader reading session in this complete frame. UIA inspection alone is insufficient.
6. Verify the pill's window sizing, topmost/activation behaviour, hide/restore and monitor placement. Test packaged UI rendering at actual 125% scale; separately verify physical 1080p work-area limits and mixed-DPI movement when suitable displays are available.

**W01 demonstration:** start timing; create a lightweight reminder directly; capture a call note without naming/filing first; edit a quote/list/link; leave and retrieve the same note; reopen after process restart; verify timing/account context and reminder identity were not changed by writing. Trigger a save failure and show a recoverable draft rather than a false Saved state.

W01 is complete when this works in the production frame with reproducible builds and test evidence. The comparison shell's successful editor alone does not complete it.

## Decisions at the point they matter

| Gate | Smallest decision required | Blocks |
| --- | --- | --- |
| G1 — host/build | Validate the thin Windows host with the complete UI; enable native CI's private-Core read access or approve another reproducible dependency arrangement | W01 completion and application scaffold acceptance |
| G2 — durable documents | Set Markdown/metadata/recovery boundaries and minimum stable link contracts. Resolve relevant O03/O07 rules before destructive commands or production schema changes | Durable production storage and link lifecycle |
| G3 — time semantics | Resolve the existing pause/remainder/resume/overlap proposals and project rollups under O01/O02 | Authoritative planned-time accounting and relevant exports |
| G4 — attention | Set queue scope/order, scheduling boundaries, overdue defaults and capture hierarchy under O04/O05/O10/O11 | Reminder/capture behaviour acceptance |
| G5 — adoption | Confirm installed-build/data baseline, migration/reconciliation, remaining meetings/break/budget parity and actual native validation | W07 daily use and release |

Until G3 is settled, provisional planned-time experiments stay outside authoritative totals/export. Future UI examples cannot silently become accounting policy. Do not require all thirteen open decisions to be settled before starting W01; resolve only the contracts needed for its durable operations, then obtain review at the relevant gate.

## Verification, builds and data safety

- Domain checks use an injected clock and reconcile exact intervals, boundaries, overlaps and undo. Persistence checks cover interrupted writes, recovery, stable relationships and migration copies.
- Bridge checks cover malformed/stale messages, navigation and process/editor failure. UI checks cover direct capture, searchable retrieval, keyboard interaction, responsive layout and matching records across views.
- Native checks cover screen-reader use, focus/clipboard/IME, real notifications and sleep/restart, tray/shortcuts, scaling and monitor placement. Keep these results distinct from browser regressions.
- Public foundation/editor checks currently run independently. Core is private: native CI is opt-in with read-only access as documented in the [comparison setup](../experiments/winforms-notes/README.md). A skipped native job is not build evidence. Native CI must be functioning before the production-frame milestone is accepted.
- Use only separate development data until migration/pilot gates pass. Import copies, preserve originals, version migrations and define recovery before application upgrades. Do not write to the installed BAT database during development.

No release date is assigned by this plan. Review each running milestone against its exit evidence and adjust the sequence using what it reveals. The first pilot should prove the user's ordinary loop: fast reminders, urgent notes, mostly manual time entry, trustworthy recall and weekly timesheet transfer.
