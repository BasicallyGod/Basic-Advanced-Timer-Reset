# Verification

The three browser scenarios preserve the existing prototype regression checks: dashboard and meetings, time entry/accounting experiments, and Notes editing/retrieval. Run after building the fragments. See the root README for browser installation.

Screenshots go to ignored `artifacts/prototypes/`. These checks cover fictional data and a simulated clock; they do not prove native operation. Timing policy experiments remain open in O01 even when their browser checks pass.

The [native Notes experiment](../experiments/native-notes/README.md) adds nine executable persistence/identity/Core-clock checks through `./tools/native-notes.ps1 check`. `npm run test:native-editor` checks the embedded editor's rendering, list/link edits, undo and document context. Its browser component checks remain separate from the native focus/accessibility findings in the feasibility report.

The existing desktop test assembly compiles, but its baseline test execution is not yet established. See the source baseline and foundation review. Native reset tests will be added with its actual components, chosen target and isolated test data.
