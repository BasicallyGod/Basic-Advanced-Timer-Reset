# BAT Foundation and Repository Review

**21 November 2026 · Source baseline verified · Repository established · Stack evaluation open**

Establish the source baseline, reuse boundaries and repository structure before writing the implementation plan. The starting recommendation is a fresh desktop application repository, with selective reuse of verified engine, storage and platform components. C# and Avalonia are the first stack candidate to evaluate. The native writing experience and data model need further investigation before that becomes a technology commitment.

## Verified starting point

| Area | Finding | Implication |
| --- | --- | --- |
| Desktop baseline | The local `alpha-performance` checkout is clean at `c6d0a67fd3205e355255f9ddf1148f41596f662c`. GitHub's `alpha` and `experimental` branches both pointed to that commit when checked on 21 November 2026. | Use this pinned source for comparison. Stable `main` is pinned separately at `3686ae59a14f4ce30c4f3548119d5dfbbf065ade`; it references the same Core/Storage versions. The user's installed build and representative data still need comparison before migration. |
| Current stack | The desktop project uses C#, Avalonia 11.3.16 and a default .NET 8 target, with a Windows-specific target for Store packaging. | These are observed source versions, not proposed dependency targets for the reset. Select supported target versions during the stack review. |
| Shared packages | The app references `BasicallyGod.Timer.Core` and `BasicallyGod.Timer.Storage` version 0.10.1 from the separate Basic-Timer-Core project. | Reuse can happen through an explicit package boundary. Both cached 0.10.1 package manifests identify source commit `361d0e32748be7c01e616d91176a844f805e8a08`. The database source at that commit declares schema 7. Package identity is verified; behavioural suitability and migration compatibility remain to test. |
| Local Core copies | `core-reference` has database schema 3 and the simpler `DayNote` record. The later `core-night-meetings` and `core-workday-review` snapshots have schema 7 and extended note/meeting fields. | Do not choose a Core copy based on its directory name. Use the verified package source commit above as the baseline; retain other snapshots only as historical comparisons. Test schema compatibility before migration. |
| Current Notes model | Alpha's `NotesModel` uses `DayNote` for plain notes, dated reminders and meetings. Meetings carry start/end context and do not count as recorded work. | The reset's documents, lightweight reminders, meetings and explicit links require a deliberate model and migration boundary. |
| Existing verification | The desktop repo contains an Avalonia headless xUnit test project and existing build/package workflows. | Review and carry forward relevant behavioural checks. The Release application and test assembly compile. The first test run could not connect to testhost and executed zero tests; this is an infrastructure failure, not a passing baseline. An unsandboxed retry also produced no completed results and was stopped. Native test execution remains unresolved; no failing application test has been identified by these attempts. Coverage and behaviour remain unverified. |

## Runtime target candidate

The existing app targets .NET 8. Microsoft lists its end of support as 10 November 2026. Evaluate .NET 10 LTS for the native feasibility slice; its published support runs through 14 November 2028. This is a candidate, not a final stack decision. The installed SDK observed during this review is 8.0.425; no SDK upgrade or new desktop scaffold has been performed. Verify the chosen Avalonia/editor packages against the supported target before committing to them. See [Microsoft's support policy](https://dotnet.microsoft.com/en-us/platform/support/policy) and [the .NET 8/9 support announcement](https://devblogs.microsoft.com/dotnet/dotnet-8-9-end-of-support/).

## Reuse assessment to complete

| Area | Starting recommendation | Evidence still needed |
| --- | --- | --- |
| Clock, tracking engine and ledger | Treat the shared components as reuse candidates. Preserve the user's trusted recording and recall behaviour. | Package/source match verified; tests for workday boundaries, manual edits, planned work and live recording. O01 remains open. |
| SQLite, undo and exports | Evaluate the existing storage and export capabilities before replacing them. | Transactions, schema upgrades, link persistence, recoverable edits and reconciled hours against representative data. |
| Windows integration | Inspect tray, shortcuts, startup, notifications, window placement and monitor/scaling helpers for selective reuse. | Actual Windows behaviour and separation from the old UI/model. File presence alone does not establish suitability. |
| Desktop presentation | Build the reset's presentation around its current design specification. | A smaller UI/application boundary, responsive layouts and testable commands. Avoid carrying the whole existing presentation layer forward by default. |
| Notes and search | Design independent documents, short reminders, meeting context and stable links deliberately. | Autosave/recovery, Markdown editing, title/body search, tags, cross-account navigation and portable copying. O03, O06 and O07 apply. |
| Seasonal character | Retain the intention and inspect existing assets/behaviour as references. | New placements and reduced-motion behaviour under O12. Exact decoration code and styling are not preselected. |

The current desktop source includes large `TimerModel`, `Controls`, `Windows` and review-view files. Their responsibilities need inspection before choosing individual pieces to port. A fresh repository should make that selection explicit rather than reproducing the existing app structure wholesale.

## Native feasibility slice

Test one small native slice before committing the UI/editor stack: open a note from another application, type and format a quotation and link, autosave, resize the window, return to a time block and reopen the same note. Keep timing and reminders working throughout.

Include keyboard selection and undo, clipboard behaviour, focus restoration, multiple monitors and 1080p at 125 percent scaling. The HTML prototype demonstrates a writing interaction; its browser editor component does not decide the native editor integration. Compare alternative approaches if this slice exposes a material limitation.

The [first native Notes experiment](../experiments/native-notes-feasibility.md) is now runnable. It establishes direct global capture, saved-note reopening, independent Core timing and atomic snapshot recovery checks. Hosted-editor accessibility, source/clipboard behaviour, native scale changes and failure handling remain gates. The experiment does not select the final stack or production document schema.

## New repository

The user selected the public repository [BasicallyGod/Basic-Advanced-Timer-Reset](https://github.com/BasicallyGod/Basic-Advanced-Timer-Reset), created on 21 November 2026. Its foundation commit carries the design specification, original brief, foundation review, decision records and selected portable prototype sources. Keep the original application available as the behavioural reference.

The shared Core repository can remain a separate dependency if the review validates that boundary. Pin chosen package versions and record their source provenance. Decide how reset-specific document/reminder storage fits before changing shared schemas or introducing another database abstraction.

The repository starts with a short README, project-specific agent instructions, ignored build/data files, portable prototype commands and foundation validation CI. Application build/test CI follows target selection. Use a working main branch and short-lived feature branches initially; release channels and Store publishing can be designed when packaging is ready. Development builds need their own data directory and executable/build identity so they can be exercised alongside the current app.

The selected prototype build scripts have been adapted to repository-relative paths, with local vendor files and licences. Generated previews are reproducible and ignored; source references use pinned URLs or repository-relative links. Exclude real databases, settings containing private material, package credentials, published binaries and transient review output.

## Decisions before the implementation plan

1. Complete the installed-app and representative-data comparison. Desktop Alpha, stable and shared-package source commits are now pinned.
2. Define what is reused, adapted or rebuilt, with test evidence for the ledger and storage boundaries.
3. Evaluate the native writing slice and choose the UI/editor approach and supported target versions.
4. Keep the new repository and governing design material aligned as feasibility evidence arrives.
5. Resolve the product decisions needed for storage and accounting, then write the staged implementation plan against these boundaries.

Repository preparation and the feasibility slice can overlap. Initial documentation can be placed in the new repo before its application scaffold is final. The public repository is established. The final desktop/editor stack and application scaffold remain open.

## Source references

- [Pinned Alpha desktop project](https://github.com/BasicallyGod/Basic-Advanced-Timer/blob/c6d0a67fd3205e355255f9ddf1148f41596f662c/app/src/Timer.Desktop/Timer.Desktop.csproj) and [dependency properties](https://github.com/BasicallyGod/Basic-Advanced-Timer/blob/c6d0a67fd3205e355255f9ddf1148f41596f662c/app/Directory.Build.props).
- [Pinned Notes model](https://github.com/BasicallyGod/Basic-Advanced-Timer/blob/c6d0a67fd3205e355255f9ddf1148f41596f662c/app/src/Timer.Desktop/NotesModel.cs) and [desktop test project](https://github.com/BasicallyGod/Basic-Advanced-Timer/blob/c6d0a67fd3205e355255f9ddf1148f41596f662c/app/tests/Timer.Desktop.Tests/Timer.Desktop.Tests.csproj).
- [Verified shared database source, schema 7](https://github.com/BasicallyGod/Basic-Timer-Core/blob/361d0e32748be7c01e616d91176a844f805e8a08/src/Timer.Storage/TimerDatabase.cs). Package metadata for Core and Storage 0.10.1 both identify this commit.
- [BAT Design Specification](BAT-design-specification.md), the governing product direction and open decisions.
