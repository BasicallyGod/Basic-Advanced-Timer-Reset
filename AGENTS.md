# Working on the BAT reset

Read `docs/design/BAT-design-specification.md` and `docs/design/BAT-foundation-review.md` before changing behaviour. The specification governs product intent; decision history and prototypes support it. Keep confirmed requirements, experiments and unresolved choices distinct. Record changes against the relevant R/O identifiers and revision history.

Preserve the trusted ledger and explicit, reversible time edits. Use one Add time form; entered times determine past/current/future behaviour. Never add a Past/Ahead mode toggle. Allow overlapping blocks and preserve their original intervals. Count shared elapsed minutes once, with equal account shares by default, editable later. Do not reject overlaps or trim/stop work automatically. Keep raw durations separate from credited allocations. O01's pause remainder, resume, allocation-override controls and explicit replacement mechanics remain open.

Notes and Timesheets must each work independently. Quick reminders are a short action, account and exact time. Long writing belongs in Notes, with explicit links/reminder actions. Quick note must reach the full editor directly. An ordinary time-block click edits time; note access is explicit.

Keep the timeline central, reminder lists bounded and reminders above Recent work when stacked. Preserve exact hour/minute adjustment, compact widths, search across note titles/bodies and one document across home/related account routes. Respect reduced motion, including planned timeline pan/zoom and seasonal details.

Do not mistake HTML proof for native proof. Do not select a native editor or copy the old application wholesale without the feasibility evidence in the foundation review. Keep package source provenance explicit. No production database migration or schema change until links, recovery and accounting rules are agreed and tested.

Use repository-relative paths and portable commands. Keep real databases, private settings, credentials, generated previews and binaries out of commits. Preserve vendor notices. Never use a user's production BAT data directory for development builds.

Run `npm run validate` and `npm run build:prototypes` for documentation/prototype changes. Run relevant browser checks when changing their interactions. Add native build/test commands once a target is chosen. Report evidence and limitations plainly; a failed test host is not a passing test suite.
