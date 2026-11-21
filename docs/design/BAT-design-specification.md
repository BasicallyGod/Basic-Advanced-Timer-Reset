# Basic’s Advanced Timer Design Specification

**Version 1.16 · 21 November 2026 · Working specification**

BAT combines dependable time recording and timesheet review with a capable, searchable notes library. Accounts and reminders connect those workflows. Each workspace must remain useful independently, and the shared working-day timeline must keep recorded work and outstanding reminders understandable together.

The governing experience is **unobtrusive while the user is working elsewhere, immediate and clear when they need BAT**. Design for a PR professional capturing thoughts during calls, entering work retrospectively, planning the next task and transferring a week’s records into an agency timesheet.

This specification is the central reference for product design, prototypes and implementation. It incorporates the original reset brief and the subsequent design discussion. Where they differ, the current requirements below govern. Original sources and prototypes remain references; a prototype’s implementation shortcut does not establish product policy.

## 1 How to use this specification

| Status | Meaning |
| --- | --- |
| Confirmed | A product requirement or direction established in the brief or subsequent user feedback. |
| Accepted interaction | Behaviour the user has tried and endorsed. Styling, placement and implementation details may still be open. |
| Proposal | A specific way to meet a requirement that still needs a prototype or decision. |
| Open | A choice that must be resolved before dependent behaviour is implemented. |

“Confirmed” describes design intent, not native implementation or release readiness. The HTML prototypes use sample data and a simulated clock. Native Windows behaviour, durable storage, notification delivery and migration need their own verification.

Use the requirement IDs when reviewing changes. Check the open decisions before choosing a data model or changing time calculations. Record a resolved choice here rather than allowing different prototypes to accumulate incompatible assumptions.

## 2 Product principles

| ID | Confirmed principle |
| --- | --- |
| P01 | Timesheets and Notes each support a complete, independent workflow. Time entry can be completed without opening Notes; a note can be written and retrieved without creating recorded time. |
| P02 | Connections save effort. Shared context, note links and reminders are optional references to existing records, with consistent updates wherever those records appear. |
| P03 | Capture precedes organisation. Quick note opens ready to type; naming, context, tags, reminders and time-block links can follow later. |
| P04 | The horizontal timeline remains the shared view of the working day. Time blocks and reminder times remain connected to it as the interface becomes quieter. |
| P05 | Recorded hours, descriptions and recall are trusted foundations. Preserve that trust through clear semantics, reversible edits and reconciled totals. |
| P06 | Show a scannable overview and reveal detail through selection. Preserve persistent awareness of outstanding reminders through a bounded list and accurate counts. |
| P07 | Give the active task usable space. Resizing, opening a note and returning from an edit preserve context, selection and drafts. |
| P08 | Keep work organisation adaptable to an agency. Reusable activity labels, project structures and external-code mappings must accommodate the user’s system. |

### Working pattern and equipment

The user estimates that roughly 60 percent of their time blocks are entered retrospectively and 40 percent are placed ahead. They rarely use live recording personally, but want to retain it for users who do. Treat this as evidence for a strong manual workflow, rather than a fixed usage ratio for every user.

The working setup includes a laptop around 2560 × 1440 and two 1080p monitors. A useful compact layout must allow BAT and Float, or a comparable timesheet, to sit beside each other. Include a 1080p screen at 125 percent scaling in native validation.

## 3 Records and their responsibilities

These are logical responsibilities. Final storage structures and relationships remain subject to the open decisions in Section 13.

| Record | Responsibility | Effect on recorded hours |
| --- | --- | --- |
| Work destination | Identifies the account, project, prospect or internal code that work belongs to. May map to an external timesheet code. | Attribution only. |
| Recorded time block | Stores the work interval, destination, short description and activity category where used. | Contributes once to recorded totals. |
| Work placed ahead | Commits intended work over a future interval; BAT assumes it was worked as that time passes unless the user changes it. | Elapsed time becomes recorded automatically, without a second confirmation. Pause and conflict mechanics remain in O01. |
| Meeting | Reserves planned time and carries meeting context. | Contributes when the user logs the work, as established in the original brief. |
| Note | A durable document with a title, Markdown content and optional organising context. | Writing or opening it adds no recorded time. |
| Reminder | Brings an action back at a chosen date and time. Can carry its own details and optionally reference a note. | Moving or completing it adds no recorded time. |
| Tag or collection | Provides optional ways to organise and retrieve notes. | None. |
| Link | Connects existing notes, work records or reminders without making another copy of their content. | None by itself. |

**R01 — Keep relationships explicit.** Linking a note to a block preserves the block’s start, end, destination, category and short description. Editing a reminder’s timing preserves its text, details and note association. Account/project assignment and personal tags have separate purposes.

**R02 — Preserve identity.** The reminder list, timeline marker and note reference address the same reminder. Reopening a note from search, a block, a reminder or its originating meeting opens the same document. Define link behaviour through renames, moves and deletion before choosing the final schema.

The early one-reminder-per-note prototype is a temporary simplification. It is not a product restriction on a note containing several actionable items. Final link cardinalities are part of O03.

## 4 Desktop presence and entry points

**R03 — Keep the pill compact.** Provide clear, distinct access to creating a reminder, Quick note, Add time and the reminder queue. Retain a useful outstanding-reminder count. Creating a reminder must be directly reachable without opening Notes first. Keep the bat identity and product character; use display effects selectively for short readouts and readable type for controls and content. Final placement and prominence are subject to O11.

**R04 — Support manual and live workflows.** Manual time entry is a primary action. Live recording, pause/resume, account switching and break handling remain accessible. The pill can foreground active tracking controls when recording is in use; it should accommodate someone who rarely uses the account scroller or play controls.

**R05 — Capture from the current task.** A desktop shortcut should open Quick note from another application or monitor, including while tracking time. The complete writing surface must be available immediately. A second create-then-open step must not be required to reach the Markdown editor.

### Quick reminder and quick note entry points

The user expects quick reminder capture to be substantially more frequent than quick note capture. Use that as a prioritisation assumption to test. Reminder creation needs prominent, direct access, while urgent note-taking must still open ready to write without passing through reminder scheduling. A user must not become trapped in the other capture process. Preserve typed content if they explicitly change their intended capture type.

This is an unresolved entry-point and discoverability issue, distinct from the unified Add time rule. Past or future is a property of a work interval; choosing a reminder or a note expresses a different capture intention. Account or meeting context can prefill relevant information while leaving the kind of capture clear. Merely mentioning a date, being in a meeting or using one capture type last must not silently switch the other action into it.

**Smallest proposal to test:** expose separate, clearly labelled reminder and note actions, with reminder capture more prominent and distinct global shortcuts for each. Design quick reminders around short action text, an account and precise timing, carrying clear account context when available and making it quick to change. Quick note opens the full editor directly. Creating a reminder does not require creating a full note. Substantial writing belongs in a note with an explicit reminder action or link. Test the compact pill arrangement, account-assignment defaults and discoverability before fixing labels, shortcut bindings or contextual defaults under O11.

Opening BAT, Notes, a reminder or an editing surface must not inadvertently start, stop or reassign live recording. Remember the last workspace and its useful context.

The exact pill arrangement, clock prominence and shortcut bindings remain design choices. Browser demonstrations do not establish global Windows shortcut behaviour.

## 5 Time entry and timeline interaction

**R06 — Make both manual directions quick.** Support “I spent 25 minutes on this account” and “I intend to spend the next half hour on this project.” Keep start, end or duration easy to set accurately. Give a clear preview of the resulting interval and destination.

Use one Add time form. Infer past, current or future behaviour from the entered interval and the current clock; do not require a Past work/Ahead toggle or any other time-entry mode selection. Editing the interval across the current time updates its preview and accounting behaviour within the same form.

Adding work ahead is the user's commitment to record it as time passes. Do not ask for a second confirmation after the interval. Allow correction afterwards through the normal time-editing controls.

**R07 — Keep the timesheet description short and independent.** Typical entries include Press office, Client call, Media engagement, Strategy or an agency-specific label. Offer reusable activity categories without prescribing a fixed taxonomy. A longer call note is optional supporting material, and its content must not replace the short description automatically.

**R08 — Select a block to work with its time.** A normal click selects a block and exposes its time, destination, description and relevant actions. Add note, Link note and Open note are explicit actions. Selecting a block must not automatically create or open a full note.

**R09 — Build direct manipulation into the application.** Dragging should be the primary way to move blocks; dragging their edges should adjust start or end. Retain precise editing controls and keyboard access. Show the proposed landing time and snapping, and make edits reversible. Preserve useful existing add, move, resize, merge, delete, select and undo behaviours when replacing the interface.

Direct manipulation is an implementation requirement recorded for the native build. It does not need to be added to the current visual prototype pass.

**R10 — Make the timeline scale with work.** Arrange lanes around necessary overlap, including meeting reservations, rather than allocating one lane to every account. Overlapping recorded work is a conflict by default; displaying it must not imply valid double recording. Use stable account identity, readable spacing and short labels or indicators where blocks are narrow. Full descriptions remain available on selection and in review. Full-day bounds must include the actual work and reminder times, including work outside nominal office hours.

**Final implementation requirement:** animate transitions between Morning, Afternoon and Full day so the user understands the change in time position and scale. Moving between day parts should read as panning earlier or later; entering Full day should visibly zoom out, and returning to a day part should zoom in. Where day-part spans differ, adjust scale smoothly as part of that movement. Keep work blocks, meeting reservations, reminder markers and axis ticks coordinated on the shared time axis, with stable record identity and selection throughout. Tune duration and easing in the application. Respect reduced-motion preferences with an immediate or suitably reduced transition and clear range labels.

### Work placed ahead

**Confirmed recording policy:** BAT assumes that work placed ahead was worked as its interval passes, unless the user specifies otherwise. A 10:00–10:30 block becomes 30 minutes of recorded work at 10:30 without another prompt. Future portions must remain distinguishable from elapsed recorded time. Retrospective entries continue to record work immediately.

The muted block filling as time advances remains the visual direction to test. A pause control, more clearly integrated with the existing timer, is the user's suggested way to indicate an interruption or early finish. Test clipping a 15-minute block to 12 minutes. Whether pausing removes, retains or shifts its remaining minutes, how resuming works, and which block the control affects remain open in O01.

**Current overlap default:** treat doubled recorded blocks as incorrect, rather than valid parallel billable work. Meeting reservations may overlap because they are not yet committed recorded work. Define how manual blocks, automatically elapsed work and live recording resolve a collision without silently counting the same minutes twice or discarding an existing record. The exact conflict interaction is still open; this default can be revisited through an explicit decision.

### Time entry and interruption experiment

The current `bat-time-flow.html` experiment tests the following proposals for O01. They do not settle native recording policy beyond the confirmed automatic-recording default.

- Enter all work through one Add time form in the day context, with exact typed times, duration shortcuts and a draft placement preview. Past/future behaviour follows the interval automatically. Save commits the interval; Cancel leaves the ledger unchanged. Compact the timeline tracks during editing so the form and its placement remain usable together.
- Pause the currently accruing allocation at the preview clock, preserving its elapsed portion as recorded work. Hold the unused minutes unscheduled, with explicit Resume and Finish actions. Held minutes contribute nothing to recorded totals. Resuming proposes those remaining minutes from a chosen start time; Finish removes the remainder without changing recorded work. Both are reversible.
- When an entry overlaps existing work, show the affected accounts, descriptions and exact before/after intervals before offering Replace overlapping minutes. Retain the existing portions outside the new interval, splitting a block when necessary. Preserve descriptions and note links on retained portions and offer Undo. Keep existing times returns to editing without applying a correction.
- Live recording uses the same ledger. In this experiment, a correction affecting the live interval explicitly pauses its timer, and a live timer stops at the next committed work boundary. These are safeguards to evaluate, not settled native handoff rules. Undo reconciles the restored live interval against the current preview clock and the next work boundary.

The example starts at 10:45 with room for a 25-minute retrospective entry. Place work at 11:00–11:30, advance to 11:12 and Pause, then advance to 11:20 and try Resume. The 18-minute remainder collides with an existing 11:30 allocation, exposing the proposed correction in context. The preview clock is a test control outside the depicted application.

## 6 Reminders and attention

**R11 — Keep reminders visible as a list.** The working-day dashboard needs a persistent, bounded reminder list as well as timeline markers and the pill count. A selected-item card alone provides too little awareness of the backlog. A long stack of expanded reminders provides too much information.

**Proposal to test:** show three or four compact rows, an obvious due count and an accurate count of additional due items. Each row shows enough title and timing context to identify the action. The selected row exposes its actions or detail. Completing or moving an item reveals the next one without making the window taller.

**R12 — Preserve overdue work.** Unhandled reminders from earlier days remain discoverable in the due queue. Make the scope of counts and filters clear. Distinguish items due now from upcoming items; avoid counting the same item in apparently separate backlog totals. The exact default queue scope, ordering and row count are open in O04.

**R13 — Use points for reminder times.** Place reminders at their scheduled times on the same time axis as work. Avoid duration-shaped reminder blocks. Group nearby points when needed and make the individual titles and exact times easy to reach. Selecting a marker and selecting its list row must stay coordinated.

**R14 — Keep reminders lightweight and link substantial writing.** The normal quick reminder is a short action, its account and a precise date/time: for example, “Write press release for project.” Aim for the ease of one or two short sentences. This is a design guideline, not an arbitrary sentence or character limit. Keep the quick capture surface focused on those essentials, with a small amount of supporting context where useful.

Call notes, briefings, detailed instructions, quotations and source material belong in Notes, with a reminder action or linked reminder attached when needed. Keep long-form editing and formatting in the Notes workspace. Offer an explicit route to develop a reminder draft into a linked note while preserving its content and a concise reminder summary. A short reminder remains useful on its own and must not create a full document automatically. Preserve existing longer reminder content through editing and migration.

An explicit action on selected note text can create a linked reminder while leaving the writing intact. Scheduling remains deliberate; merely typing a day or date in prose does not create a reminder.

Offer a direct way to add a reminder at a chosen timeline time with that time already filled in. This creates the same reminder record as other entry points, without requiring a full note.

**R15 — Make reminders dependable beyond the toast.** Outstanding items remain visible until handled. Native validation must cover notification prominence, behaviour while BAT is in the background and what happens after sleep, restart or a missed scheduled time. The exact Windows notification presentation is still to be designed and tested.

### Accepted rescheduling interaction

The user has endorsed the compact timing behaviour. Its appearance and popup placement remain provisional.

- Show one destination day and time with a clear Move action and Cancel.
- Scrolling over the hours changes hours; scrolling over the minutes changes minutes. This supports a large move such as 09:00 to 15:00 without repeated minute adjustments.
- Retain precise typing, fine adjustments and faster movement while holding the adjustment buttons.
- Keep account, note content, reminder details and editing metadata outside this quick timing task.
- Keep changes provisional until Move. Cancel preserves the saved reminder. Offer Undo after a committed move.
- Make a date change, including a midnight rollover, clear without silently shifting a mistyped past time into a future day.

The prototypes also explore a draft timeline marker, a future starting suggestion for overdue items and specific wheel/hold sensitivity values. Those details are not all settled requirements. Choose and validate the default proposed time, gesture sensitivity and behaviour at date boundaries under O05. Show validation when needed rather than presenting an expired starting value and an immediate warning as the normal opening state.

Editing the reminder’s title, details or account remains available as a separate deliberate action.

## 7 Notes capture and writing

**R16 — Support two entrances to one editor.** From Notes, the user can create a document, name it and write. From Quick note, the cursor is ready for immediate writing and naming can follow. Both create the same kind of durable note with autosave.

An initial captured line may supply the starting title. Keep the title outside the writing page and allow an explicit rename. The exact point at which an automatic title stops updating remains a detail to validate.

**R17 — Organise after capture.** Content alone is enough to keep a note. Account/project assignment, tags, reminders and time-block links are optional subsequent actions. Unassigned notes remain available through recent notes and search. Carry clear client or meeting context when capture was explicitly launched from that context, and allow correction. A running timer’s account must not silently override a different capture context.

**R18 — Prioritise writing space.** Use left-aligned titles and headings, a comfortable text measure and substantially less wasted space. Read and edit the same surface. Begin with headings, bullets, bold, links and block quotes, with autosave while typing. Test pasted links, edits within lists, selection and undo through realistic writing.

**R19 — Keep formatting optional.** Support keyboard shortcuts and selected-text actions. Offer an optional floating formatting bar for button-based editing. It can tuck away at smaller sizes while the underlying capabilities remain reachable. Do not require a permanent toolbar or a separate formatting mode to write.

**R20 — Preserve the writer’s place.** Closing or switching away from a note preserves content, selection and useful navigation context. Returning to the library restores the previous collection, search, filter and position. Timer operation remains accessible and uninterrupted throughout.

Clean copying out of BAT matters for reuse elsewhere. Markdown/plain-text portability and Word-appropriate copying or export need explicit treatment in O07. Images, attachments, tables and a full rich-text feature set are not commitments created by the inspiration screenshots.

## 8 Notes organisation and retrieval

The confirmed direction is a nested, interconnected and searchable library. Bear, Drafts and Typora supply interaction references, not a visual template to reproduce.

**R21 — Provide several reliable routes back to a note.** Support recent notes, account/project context and search across titles and body text. Search results need snippets around matches, useful context and enough date information to distinguish repeated titles such as Client call. Make any search scope visible and make a search across all notes easy to reach.

**R22 — Make the organising structure useful without duplicating documents.** Explore a context tree, a compact list of notes or search results, and a generous editor. On narrow windows, tuck navigation away and retain a clear route back. One saved note may appear in several useful collections.

**Confirmed organising direction:** assigning a note to Acme gives it a home in an Acme folder or branch. Nested tags can provide routes into subdivisions within that branch. Capture without an account remains valid. The user needs to see this in practice before settling the detailed behaviour.

Cross-account relevance must preserve that home and make the additional connection understandable. A brainstorming note created for Account One may also be useful to Account Two. Prototype a route from Account Two to that same note, showing its Account One origin and its relationship to Account Two. Do not require a copied document or silently change the home account when adding a reference.

The proposed implementation has three complementary relationships:

| Relationship | Proposed role |
| --- | --- |
| Work context | An optional home account/project reused from BAT’s work destinations and shown as a folder-like branch. Other relevant accounts can reference the same note; the distinction between home and related context must be visible. Exact controls remain to be prototyped. |
| Personal tags | Optional themes, including nested tags, provide subdivisions and overlapping collections. Test how a tag creates a route within an account branch and how cross-account references are represented. Final syntax and editing behaviour remain open. |
| Note links | Explicit connections between documents, with a linked-from view for returning to the source of a connection. Renaming a note should preserve navigation. |

**R23 — Keep text and work metadata distinct.** Markdown headings, quoted hashtags and ordinary prose must not accidentally reassign an account or create a reminder. Define the boundaries between Markdown syntax, optional inline tags and structured work context before integrating with the existing note parser.

A note subdivision or tag does not automatically create a billable project or external timesheet code. Work destinations and writing organisation have related but distinct purposes.

**R24 — Replace the existing-note dropdown.** Linking a note to a time block must work with hundreds of documents. Test a few recent notes plus title/body search, relevant account/project context and match snippets. Include unassigned notes and a clear way to reach other contexts. Reuse the library’s retrieval approach so note linking and note browsing stay consistent.

The next Notes prototype should cover capture, development, organisation, cross-linking and retrieval together. Optional wiki-link syntax, backlink placement and the exact pane layout remain proposals to validate under O06.

## 9 Accounts projects and agency codes

**R25 — Preserve distinct work identities.** Retainers, standalone projects and new-business prospects must remain distinguishable. Allow several prospects to map to one external New Business code while each original block retains its prospect, interval and description.

**R26 — Keep activity separate from work identity.** A project or client identifies where work belongs. A category such as Press office or Client call describes what the user did. Support configurable categories and descriptions suited to the destination timesheet.

**Confirmed account model:** a short-lived project can be an ordinary account. A three-week project needs no special setup or mandatory parent; the user can hide it from active selection when finished while retaining its history. Agency-wide non-billable work can likewise use an ordinary account named Agency time, Non-billable or another user-chosen name. Account-specific administration can remain a description such as Account management on that account's block.

Support optional projects beneath a retained account where an agency needs separate billable destinations. For example, Account Two can contain Press Office and Campaign projects with distinct external codes. Users who need only descriptions can continue entering Press office or Client call without creating those projects. No project hierarchy or special internal-work system is required for the basic workflow.

**Presentation to test:** related work can inherit top-level colour identity. Show a full destination path where the user is choosing or reviewing an entry. Avoid making every subproject another permanent lane or unrelated colour in the overview.

The flat default and optional nested capability are settled. Storage shape, parent/project rollups, budget attribution and external-code defaults remain in O02. A sample containing eight peer accounts alone does not validate those details.

## 10 Day dashboard visual hierarchy and resizing

**R27 — Keep the overview scannable.** Use stable account colours, familiar icons for repeated actions, concise labels and clear selection. Preserve names and descriptions where they identify work or support a decision. Provide accessible names and useful focus/hover detail for icon actions. Use readable Light and Dark treatments.

The account key should represent relevant work in the current view. Short blocks can use abbreviations or indicators, with complete context available through selection. Dense full-day views need spacing, clear time ticks and a workable path to smaller intervals or detailed entries.

**R28 — Bound the amount of visible detail.** Keep the timeline visually primary on the Timesheets dashboard, with one selected-item detail area supporting it. Retain a distinct reminder track and a compact, persistent reminder queue for backlog awareness. A chronological entry list can be folded away during normal timeline work. Weekly handoff keeps the text required for accurate transfer visible.

The landing page must offer useful orientation and clear actions before the user selects a record. An isolated block inspector and sparse example data are insufficient to evaluate that experience. Keep meetings represented when evaluating the whole working day.

**Landing proposal to evaluate:** show recorded time and the last entry, foreground Add time, bring forward the next meeting with access to its notes, and show a few recent entries that can be amended. Reveal block details on selection. Keep the bounded reminder backlog alongside that context. A time-entry suggestion based on the last entry must not assume an unallocated gap was worked or create recorded time without the user's action.

**R29 — Resize without losing work.** Test wider, taller and compact arrangements with the same selected record, timeline position and note draft. Collapse empty lanes. Provide deliberate ways to tuck away less-used navigation or meeting areas and to restore them.

When reminders no longer fit beside the day context, place their list above Recent work in the stacked layout. Outstanding actions take priority over the historical list already represented on the timeline. Keep the meeting or selected-record context reachable above that list.

The current range-control proposal offers Morning, Afternoon and Full day, with small icons alongside readable labels and exact ranges available on the controls. Switching ranges changes the visible time window while preserving selection, the reminder backlog and recorded totals. Prototype hours are sample layout choices, not a fixed native working-day policy.

Choose the native minimum size through task testing on 1080p at 125 percent scaling, including side-by-side use with an external timesheet. The prototype’s 600-pixel frame height, 700-pixel compact option and rendering at 320 pixels are experiments, not a native minimum-size specification.

Retain the shared timeline/reminder relationship during visual simplification. The latest dashboard’s reduced text and colour treatment are a candidate visual direction; the user’s subsequent reminder-list feedback is an outstanding requirement against it.

### Seasonal touches and Easter eggs

**Confirmed direction:** reintroduce small seasonal touches and occasional Easter eggs as part of BAT's character. Take inspiration from the original application's approach while allowing a fresh treatment for the reset.

**Placement proposals to explore:** a seasonal detail on the bat or pill, a small dashboard accent, or an occasional subtle response elsewhere in the app. Keep these touches subordinate to writing, time labels, account identity and reminder urgency. They must preserve the compact window and the app's unobtrusive presence during other work. Check Light and Dark appearance and honour reduced-motion preferences.

The seasonal calendar, motifs, placements, visibility preferences and delivery timing remain open under O12. Their inclusion does not commit the reset to reproducing the old decorations exactly.

## 11 Weekly review and manual handoff

**R30 — Support deliberate transfer to an agency timesheet.** Provide a chronological list grouped by day with destination, start/end, duration, activity category and short description. Keep descriptions readable and easy to copy. Let the user track entries already transferred and flag missing descriptions where the destination requires them.

**R31 — Reconcile every view.** Weekly, daily, account and external-code totals derive from the same recorded blocks. Show combined New Business totals with an expandable view of the original prospect blocks. Preserve a path to supporting full notes without making note creation part of time entry.

**R32 — Keep budget pace useful.** Make weekly hours and account budget review reachable. Label the budget period explicitly so a monthly budget is not confused with a weekly total. Budget figures and external-code grouping remain requirements carried forward from the original brief, beyond the small handoff demonstrated so far.

The current Week concept supports manual entry into Float or a similar application. Automatic integration is outside the first reset pass. Future integration must preserve original block detail and reconciled totals.

## 12 Implementation scope and data protection

**R33 — Preserve the reliable foundations.** Explore reusing the existing clock, ledger and shared Core/Storage capabilities as the starting point. Choose the native interface and editor integration deliberately; the browser prototype’s editor component is not a native technology decision.

Complete a foundation and repository review before the implementation plan. Reconcile the source/package baseline, assess reuse boundaries and test a small native writing slice before choosing the UI/editor approach. Set up a new application repository with the governing design material and portable development instructions. The repository strategy and technology recommendations in the [Foundation and Repository Review](BAT-foundation-review.md) remain provisional under O13.

Retain useful existing CSV export and compare the proposed weekly list with the existing Timesheet table before replacing its handoff workflow.

The existing note/account shorthand needs review before Markdown and personal tags are introduced. Structured context, note content and reminder timing must remain unambiguous through editing and migration.

**R34 — Make changes recoverable.** Preserve useful undo across time edits and provide clear reversal for reminder moves. Define what deleting a block, note, tag, meeting or reminder does to its links and dependent records. Test autosave and recovery before relying on BAT as the primary repository for call notes.

**R35 — Prove migration and real Windows behaviour.** Before daily-use adoption, decide how existing accounts, time records and notes come across, then reconcile hours and original descriptions. Validate background operation, multiple monitors, scaling, global capture shortcuts and notification behaviour in the actual application.

The first build excludes advanced recurrence, a full rich-text suite, new external integrations and naming exercises. Broad theme exploration remains outside the first pass. Seasonal touches and Easter eggs are included in the reset direction, with their scope and delivery timing to be settled under O12. Preserve existing useful capabilities unless a replacement decision is explicit. Recent prototypes omitting budgets, meetings, break controls or migration do not establish their removal.

## 13 Open decisions and early issue checks

The recording default and the account model are resolved at the product level. The Notes home-and-reference direction is established for prototyping. O01, O02 and O06 below retain only the remaining interaction, accounting-detail and structural questions.

| ID | Decision or tension | Resolve before |
| --- | --- | --- |
| O01 | Automatic elapsed recording without reconfirmation is settled. Review the time-flow proposals for pausing the accruing block, holding unused minutes, resuming or finishing, explicit overlap replacement and live-timer boundary handling. Recorded overlap is a conflict by default; meeting reservations may overlap. These proposals need user feedback before becoming native policy. | Authoritative time calculations, storage transitions and weekly handoff for planned work. |
| O02 | Ordinary accounts cover standalone projects and agency time; nested billable projects are optional. Define parent/project rollups, budget attribution, external-code defaults and history-preserving hiding. | Committing the account/project schema and building project-based library views. |
| O03 | What links are allowed among notes, blocks, meetings and reminders, and what survives a rename, archive or deletion? Confirm multi-action notes and relevant cardinalities. | Storage and link-navigation contracts. |
| O04 | What does the persistent queue show by default, and how many compact rows fit? Define overdue ordering, upcoming scope, counts and coordination with the selected day. | The next reminder-dashboard prototype. |
| O05 | What future time should an overdue reminder initially propose? Calibrate wheel and hold behaviour, date rollover and popup placement without making the normal interaction busy. | Native rescheduling implementation. |
| O06 | Prototype an account home, nested tag subdivisions and cross-account references to the same note. Decide how home versus related accounts are displayed, how tags map to branches, and how search scope, duplicate titles, links and adaptive panes behave. Review sidebar clutter before making significant structural changes; the current Notes direction has positive user feedback. | Finalising the Notes organisation model; test the alternatives in the dedicated Notes and searchable-linker prototype. |
| O07 | How are Markdown, metadata and internal links stored and copied or exported? Define autosave/recovery and a path to useful reuse in Word or other tools. | Editor/storage integration and import/export commitments. |
| O08 | How do meetings, basic repeat settings, break handling and budget review fit into the refreshed interface? Test moving meetings and make single-occurrence versus series edits and deletion explicit. | Declaring feature parity or removing old controls. |
| O09 | What minimum native size and layout work at 1080p/125 percent scaling beside another app? How are focus and drafts restored after resizing or temporary expansion? | Native layout limits and window behaviour. |
| O10 | What remains due after sleep, restart, an obscured toast or a missed scheduled time? How are actual local time, timezone changes and clock changes handled? | Trusting native reminder delivery and background operation. |
| O11 | How should frequent quick reminders and urgent quick notes be exposed without forcing either through the other workflow? Test prominence, distinct labelled actions and shortcuts, contextual prefilling, draft-preserving recovery and compact pill sizing. Reminder-first frequency is a user-informed design assumption; the final entry-point arrangement is unresolved. | The next quick-capture prototype and final pill/shortcut implementation. |
| O12 | Which seasonal moments, motifs and Easter eggs suit the reset, where should they appear, and what visibility preferences and delivery timing are appropriate? Preserve operational clarity, compact sizing, Light/Dark legibility and reduced-motion behaviour. | Seasonal presentation and implementation planning. |
| O13 | Which desktop/shared-package source baseline, reuse boundaries, UI/editor approach and target versions should underpin the reset? The public repository is BasicallyGod/Basic-Advanced-Timer-Reset; desktop and shared-package commits are pinned in the foundation review. Installed-build comparison, dependency structure, reuse suitability and native editor feasibility remain open. | The implementation plan and application scaffold. |

### Superseded interaction assumptions

- A Past work/Ahead toggle in Add time is superseded by interval-based inference in one form. Use R06.
- Creating a short note and then reopening it is not the required path into the full editor. Use R05 and R16.
- Clicking a time block to create/open a full note automatically is superseded by selection and explicit note actions. Use R08.
- Beginning with a time block is not a prerequisite for call notes. Use R16, R17 and R24.
- A reminder counter and an on-demand popup alone do not provide enough backlog visibility. Use R11 and R12.
- A comprehensive rescheduling form is superseded for quick timing changes by the compact accepted control. Use Section 6.
- Flat eight-account examples and prototype colour palettes do not decide project structure or all window sizes. Use O02 and O09.

## 14 Validation scenarios

Run scenarios against realistic sample data and, where relevant, compare the equivalent task in Alpha. Record the result, actual steps and failures against the requirement IDs. Browser results and native results must be labelled separately.

| Scenario | What it needs to prove |
| --- | --- |
| Lightweight reminder and richer note | Quickly capture “Write press release for project”, assign its account and set an exact time without creating a full note. Then try a reminder requiring substantial background: write that content in Notes and attach a reminder action. Check concise reminder summaries, direct access to the full note and preservation of draft text and existing long reminder content. |
| Seasonal character | Try proposed seasonal details in the pill, dashboard and writing views at compact and larger sizes, in Light and Dark and with reduced motion. Confirm that time labels, account identity, reminder urgency, controls and writing remain clear and usable. |
| Quick capture intention | From another application, capture several short reminders with exact times, then immediately take a call note while the timer continues. Each route opens directly into its intended task. Repeat from different account/meeting contexts and after using the other capture type. Check discovery, steps and any explicit change of intention without losing draft text. Reminder capture must not require a full note, and note capture must not require scheduling. |
| Timeline range continuity | Switch Morning to Afternoon and back, then enter Full day and return to a day part. The movement communicates earlier/later time and changes of scale. Blocks, meetings, reminder markers and axis ticks stay coordinated; selection and totals persist. Check reduced-motion behaviour. This is a final-application interaction requirement, not a claim about animation in the current prototype. |
| Opening BAT | Start with a realistic populated day and no selected block. Identify recorded work, the next meeting, relevant notes and the reminder backlog. Add recent time or amend a description directly. Compare a focused time range with the full day, including overlapping meeting reservations. Confirm that the page supplies useful actions without expanding every record or requiring live tracking. |
| Manual workday | Enter past work and a future 10:00–10:30 block. Let time pass and verify automatic elapsed recording with no second confirmation. Correct it afterwards; test the pause proposal and show its effect on remaining time. Move, resize and describe blocks, including early and late work. Resolve O01 mechanics before asserting collision and pause totals. |
| Conflicting work and overlapping meetings | Introduce a live timer and a manual block during an automatically recording interval. Expose the conflict and test a resolution without silently double counting or discarding work. Allow two overlapping meeting reservations without recording either automatically. |
| Call capture | From another app while tracking Account A, open Quick note and type immediately. Associate the note with Account B or a project later, develop a quotation and source link, then close and resume work. No extra time block or reminder is required to save it. |
| Later time association | Link that note to an existing call block or add its retrospective time from the note. The original short description and duration remain intact unless explicitly edited. |
| Reminder backlog | Use overdue items from earlier days and many upcoming reminders. Keep a bounded list visible, show accurate additional-item counts, move or complete items and reveal the next one. The window does not grow with the backlog. |
| Fast rescheduling | Move a reminder from morning to afternoon by scrolling hours; refine minutes, type an exact time, change the day and cross midnight. Check Move, Cancel, Undo and preservation of reminder content and links. |
| Interconnected notes | Capture unassigned writing, assign a home under Account One and use a nested tag subdivision. Make its brainstorming content discoverable through Account Two while showing its Account One home. Edit the same document from either route; test note links, search, renaming and return navigation. Compare presentations before fixing the model. |
| Large library retrieval | Use hundreds of notes, repeated Client call titles and body-only search phrases. Find the correct note by context and a phrase, then use the same retrieval approach to link it to time. |
| Mixed work structure | Use eight accounts and five projects, including a three-week project set up as an ordinary account and optional Press Office/Campaign projects with separate codes beneath a retained account. Use an ordinary Agency time account for unrelated internal work and descriptions for client-specific administration. Hide the completed project without losing history. Include prospects and validate readable selection and handoff. |
| Weekly handoff | Review a realistic week, correct a missing description and transfer entries into Float or an equivalent tracker. Map two prospects to one external code and reconcile the combined total with the individual blocks. Check a budget with its stated period. |
| Reversal and continuity | Undo a mistaken time edit or reminder move while recording continues. Resize or switch workspaces mid-note and restore context, selection, filters and drafts. |
| Native desktop operation | Test global capture from another app/monitor, background timing, break/account controls, notifications and recovery after sleep/restart. Use the high-resolution laptop and 1080p/125 percent cases. |
| Migration and recovery | Bring representative existing records across, preserve descriptions and links, reconcile hours, and recover a note after interrupted saving. |

## 15 Sequence of design work

1. Prototype the bounded reminder queue within the shared timeline dashboard. Keep the accepted compact rescheduling interaction and coordinated markers.
2. Evaluate the dedicated Notes/library prototype covering writing space, left alignment, an account home, nested tag views, cross-account references, note links, search and the replacement for the existing-note dropdown. Refine the model from user feedback before treating its presentation as accepted.
3. Validate automatic planned-time recording, the pause proposal and conflict resolution. Validate ordinary accounts and optional nested billable projects with mixed project/admin examples; settle rollups and code defaults before committing schemas and accounting details.
4. Validate handoff, mappings, budget periods and the broader meeting/break workflows against the same recorded data.
5. Implement and test native interaction, resizing, storage, notification delivery and migration. Direct block manipulation is part of this build work.

Steps can inform one another. Dependencies in Section 13 govern when a tentative choice is safe to turn into authoritative behaviour.

## 16 References and revision control

### Project references

- [Original reset brief](references/BAT-reset-design-brief-original.md), retained unchanged.
- [Discussion decision log](bat-design-decisions.json), supporting history for this specification.
- [Existing Alpha user guide](https://github.com/BasicallyGod/Basic-Advanced-Timer/blob/c6d0a67fd3205e355255f9ddf1148f41596f662c/app/README.md) and [display and Timesheet design notes](https://github.com/BasicallyGod/Basic-Advanced-Timer/blob/c6d0a67fd3205e355255f9ddf1148f41596f662c/docs/design/display-data-presentation.md), local implementation references rather than proof of reset behaviour.
- [Earlier display plan covering seasonal decorations and reduced motion](https://github.com/BasicallyGod/Basic-Advanced-Timer/blob/c6d0a67fd3205e355255f9ddf1148f41596f662c/docs/design/eink-display-plan.md), historical design context for seasonal character rather than a commitment to reproduce that appearance.
- User-supplied editor screenshots informed the discussion. They are not redistributed in this repository; the public interaction references below retain the source context.

### Native feasibility evidence

The first [native Notes slice](../experiments/native-notes-feasibility.md) tests direct capture, file-backed notes, explicit work links and the pinned Core clock. Its partial results inform O07/O13; it does not settle the stack, storage schema, shortcut choices or open product policies.

### Prototype references

| Prototype | Use as a reference for | Qualification |
| --- | --- | --- |
| `bat-time-flow.html` | A 25-minute past entry, a planned half-hour, pausing after 12 minutes, holding/resuming 18 minutes and previewing overlap corrections. Includes Undo, shared note links, the dashboard context and a manually advanced clock. | Behaviour under O01 remains proposed. Browser checks cover exact elapsed totals, held minutes excluded from totals, trim/split corrections with note links retained, live-timer collisions and boundaries, cancellation, Undo, Notes continuity, state restoration and 320–1024 pixel layouts. The clock is simulated; native gestures, background timing and range animation are not implemented by this experiment. |
| `bat-day-dashboard.html` | A useful landing overview, primary manual time entry, next-meeting notes, recent work, a separate meeting lane and bounded reminder backlog. Morning and full-day ranges use eight accounts, eleven seeded work blocks, three meetings and 150 notes. | The layout is a new proposal awaiting user feedback. Browser checks cover time entry/editing and overlap rejection, meeting edits without recorded-hour changes, direct meeting-note capture and reopening, retrieval, reminder reversal, saved-state restoration and 320–1024 pixel layouts. Uses sample data and a fixed clock. Native accounting transitions, dragging, notifications, recurrence, budgets and weekly handoff still require their separate work. |
| `bat-notes-library.html` | An account home and related-account routes to one note, nested tag views, title/body search with snippets, direct writing, links/backlinks and searchable time-block linking. | Notes organisation remains an experiment awaiting user feedback. Uses 150 sample notes and a fixed preview clock; the small Timesheets view supplies integration context and does not replace the full dashboard or its remaining requirements. Browser checks cover cross-account editing, pasted-link editing, in-list amendment, formatting undo, state restoration and layouts from 320 to 1024 pixels. Native storage, notifications and global capture are unverified. |
| `bat-dashboard-hierarchy.html` | Eight-account visual hierarchy, shared timeline, selection and manual handoff layout. | Persistent reminder-list changes and the deeper Notes work are still outstanding. |
| `bat-scroll-reschedule.html` | Accepted hour/minute wheel interaction within the compact mover. | Styling, popup position and gesture calibration remain provisional. |
| `bat-quiet-reschedule.html` | Reduction of quick rescheduling to one destination and fine/faster adjustment. | Later wheel behaviour is demonstrated in the scroll prototype. |
| `bat-two-workspaces.html` | Independent Timesheets and Notes, capture first and optional later association. | Later prototypes restore the timeline’s stronger reminder connection. |

Build the three current references from [portable prototype sources](../../prototypes/README.md). Older named experiments are historical references and are not included in this initial repository. Prototype files demonstrate specific interactions; this specification defines the current combined direction.

### Notes interaction references

- [Bear tags and nested tags](https://bear.app/faq/how-to-use-tags-in-bear/).
- [Bear note links](https://bear.app/faq/how-to-link-notes-together/) and [linked-from navigation](https://bear.app/faq/how-to-use-the-info-panel-table-of-contents-and-backlinks-in-bear/).
- [Typora interaction article supplied by the user](https://notesaboutnotetaking.wordpress.com/2020/05/15/typora-markdown-as-wysiwyg-as-it-can-get/) and [Typora Quick Start](https://support.typora.io/Quick-Start/).

These references inform navigation, search and writing patterns. Their full feature sets, platforms, commercial terms and visual styling are not BAT requirements.

### Revision history

| Version | Date | Change |
| --- | --- | --- |
| 1.0 | 21 November 2026 | Consolidated the reset brief and design discussion into a current specification, with requirement IDs, accepted interactions, open decisions, superseded assumptions and validation scenarios. |
| 1.1 | 21 November 2026 | Confirmed automatic elapsed recording without reconfirmation, a default conflict for doubled recorded work and overlapping meeting reservations. Established ordinary accounts for standalone projects and agency time, with optional nested billable projects. Added the Notes home-account and cross-account-reference direction, narrowed the remaining open decisions and updated validation scenarios. Pause mechanics and the Notes presentation remain to be tested. |
| 1.2 | 21 November 2026 | Added the Notes/library experiment and its browser validation evidence. The presentation and organisation model remain subject to user review; recording, account and reminder policies are unchanged. |
| 1.3 | 21 November 2026 | Recorded positive feedback on the Notes flow and responsive widths. Applied the selected 176-pixel navigation width and writing focus to this prototype; these are prototype preferences, not native size limits. Reinforced the timeline's primary position on Timesheets while retaining the separate reminder track, bounded queue and folded entry list. The revised timeline presentation awaits review. |
| 1.4 | 21 November 2026 | Recorded the need for an inviting, immediately useful landing page and restored meetings to whole-day evaluation. Added a populated day-overview proposal with next-meeting notes, recent entries and primary manual time entry. Its arrangement awaits user feedback; an unallocated gap remains unrecorded until the user explicitly adds work. |
| 1.5 | 21 November 2026 | Recorded positive feedback on the populated dashboard and Notes. Prioritised reminders above Recent work when the layout stacks. Added an Afternoon range and tested icons with labels as range controls. Recorded Notes sidebar clutter as a review topic without making structural changes to that workspace. |
| 1.6 | 21 November 2026 | Added the final implementation requirement for coordinated pan and zoom transitions between Morning, Afternoon and Full day, preserving spatial continuity, record identity and selection, with reduced-motion support. |
| 1.7 | 21 November 2026 | Added the focused time-entry and interruption experiment. Recorded proposed held remainders, resume/finish behaviour, explicit trim/split overlap correction, Undo and live-timer safeguards. O01 stays open for user review; browser evidence does not establish native timer behaviour. |
| 1.8 | 21 November 2026 | Removed the Past work/Ahead toggle following user feedback. Add time is one form; the entered interval determines past, current and future behaviour automatically. Browser checks cover changing the same unsaved interval between past and future before committing. |
| 1.9 | 21 November 2026 | Recorded the quick-reminder versus quick-note entry-point tension as O11. Prioritised direct reminder creation while preserving immediate independent note capture. Added a labelled-actions/shortcuts proposal and a validation scenario; final pill hierarchy and contextual defaults remain open. |
| 1.10 | 21 November 2026 | Clarified reminders as lightweight action/account/time records, with substantial writing kept in linked Notes. Restored seasonal touches and Easter eggs to the reset direction, added O12 and validation scenarios, and clarified the distinction from broader theme exploration. |
| 1.11 | 21 November 2026 | Added the initial Foundation and Repository Review and O13. Established source-baseline reconciliation, reuse assessment, native editor feasibility and new-repository preparation as work before the implementation plan. No final stack or remote repository has been created by this review. |
| 1.12 | 21 November 2026 | Established the user-selected public reset repository, pinned Alpha/stable and Core/Storage 0.10.1 source provenance, and made the selected prototype sources and design references portable. Native feasibility and reuse decisions remain open under O13. |
| 1.13 | 21 November 2026 | Added the runnable native Notes feasibility experiment and its evidence. Recorded passed capture/persistence/timing checks and remaining accessibility, clipboard/source and native scaling gates. Production stack and schema decisions remain open. |
| 1.14 | 21 November 2026 | Recorded the native keyboard-focus fix, verified link Apply/Cancel and F6/Tab navigation, and supporting editor-selection regression. Accessibility and broader native feasibility gates remain open; experiment shortcuts do not settle O11. |
| 1.15 | 21 November 2026 | Recorded native compact-window checks at actual 125% DPI, responsive link fields, explicit editor semantics and the offscreen accessibility/rendering comparison. Physical 1080p, mixed-DPI and assistive-technology gates remain open. |
| 1.16 | 21 November 2026 | Added the direct Microsoft WebView2 host comparison. Recorded visible writing with native document structure and capture/persistence checks. Recommended it as the stronger Windows host candidate; the production shell and full accessibility validation remain open. |

For future changes, update the affected requirement and its status, resolve or add the relevant open decision, revise the affected validation scenario and add a brief revision entry. If a prototype temporarily diverges, label the experiment and its intended test rather than silently changing the governing rule.
