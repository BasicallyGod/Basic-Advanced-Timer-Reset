# 0001 — Reset foundation and repository

Date: 21 November 2026

Status: repository decision accepted; reuse and desktop/editor selection remain open under O13.

The user selected public `BasicallyGod/Basic-Advanced-Timer-Reset`. Start a fresh application repository with the governing specification, unchanged original brief, discussion history, source baseline and portable design experiments. The existing BAT and shared Core repositories remain behavioural/source references.

Carry forward the trusted recording behaviour through explicit, tested reuse boundaries. Evaluate clock/ledger, SQLite/undo/export and Windows helpers individually. Rebuild presentation around the reset design. Do not import the old application wholesale or imply that the older local Core snapshots represent package 0.10.1.

The shared package manifests agree on source commit `361d0e32748be7c01e616d91176a844f805e8a08` and the pinned database source uses schema 7. This establishes provenance; compatibility with a future document/link model still requires evidence. Installed-build/data comparison is pending.

Evaluate C#/Avalonia with a supported .NET target as the first candidate. .NET 10 LTS is a runtime candidate following the lifecycle review. A native writing slice must demonstrate direct capture, editable Markdown, selection/undo/clipboard, autosave/recovery, resizing and focus restoration alongside timing before the final UI/editor decision. Compare alternatives if that fails.

The initial CI validates design references and reproducible browser builds without production credentials. Native build/release CI, isolated executable/data identity, migration strategy and a project licence follow their respective decisions. Public visibility alone does not select a licence.

See the [foundation review](../design/BAT-foundation-review.md), [baseline](../baselines/source-baseline.json) and [governing specification](../design/BAT-design-specification.md).
