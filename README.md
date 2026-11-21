# Basic’s Advanced Timer — Reset

A design-led reset of BAT: trusted timesheets, quick reminders and connected Markdown notes, in a compact desktop application.

This repository contains the agreed product direction, source baseline, interactive browser experiments and a first native Notes feasibility slice. The production desktop application and final UI/editor choice follow the remaining feasibility gates.

- [Design specification](docs/design/BAT-design-specification.md): current requirements, accepted behaviour and open decisions.
- [Foundation review](docs/design/BAT-foundation-review.md): what we can build on and what still needs evidence.
- [Repository decision](docs/decisions/0001-foundation-and-repository.md) and [source baseline](docs/baselines/source-baseline.json).
- [Interactive prototypes](prototypes/README.md): dashboard, time entry and Notes.
- [Windows editor-host comparison](docs/experiments/direct-webview2-host-comparison.md): direct Microsoft WebView2 now combines visible writing with exposed native document structure.
- [Native Notes experiment](experiments/native-notes/README.md) and [feasibility findings](docs/experiments/native-notes-feasibility.md): a runnable Windows slice with persistent notes and the pinned Core clock.

## Run the references

Use Node.js 22 or newer. Foundation validation and prototype builds use only Node built-ins and vendored browser assets; no package installation is needed for these commands.

```sh
npm run validate
npm run build:prototypes
npm run preview
```

Open the localhost URL printed by the preview command. The samples use fictional accounts and a simulated clock. They do not read or migrate an existing BAT database. Browser checks cover the reference flows; they do not prove native timing, notification delivery or data recovery.

For optional browser regression checks, install the pinned dev dependency with `npm install`, run `npx playwright install chromium`, then `npm run test:prototypes`. An existing Chromium browser can be selected with `CHROME_PATH`.

## Next work

Follow the [Windows-first implementation plan](docs/implementation-plan.md), beginning with W01: a production frame joining direct capture, a persisted note/reminder and the Core clock. The [platform decision](docs/decisions/0002-windows-first-delivery.md) records the user's Windows-first priority, proposed host composition and remaining validation gates.

The original [desktop application](https://github.com/BasicallyGod/Basic-Advanced-Timer) and [shared Core](https://github.com/BasicallyGod/Basic-Timer-Core) remain the source references. Their selected commits and observed dependency versions are pinned in the baseline. This repository does not yet modify their schemas or import user data.

## Licensing

The project's licence has not yet been selected. Public visibility does not grant an open-source licence. Third-party prototype assets retain their licences in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
