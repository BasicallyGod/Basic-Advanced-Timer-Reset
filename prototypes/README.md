# Interactive design references

Build with `npm run build:prototypes`, then `npm run preview` from the repository root. The server binds only to 127.0.0.1. All editor and icon assets are local. Generated HTML stays out of source control.

| Reference | Explore |
| --- | --- |
| Working day | Timeline, meetings, bounded reminder queue, recent work and links to Notes. |
| Time entry | Unified Add time, past/future inference, placement preview and proposed interruption/collision handling. Use the separate preview clock to advance time. |
| Notes library | Direct writing, account home/related routes, nested tags, body search and note links. |

The wrappers supply a minimal host-state adapter, held in memory until the page is reloaded. They do not provide durable autosave. The editor and clock are design references; native integration, storage, global shortcuts, notifications, timeline range animation and direct block dragging remain unproven.

The canonical design specification takes precedence over these experiments. In particular, pause/remainder and overlap correction policies remain open. The dashboard and Notes entry routes also predate the latest quick-reminder hierarchy decision (O11).

The selected sources were imported from the design discussion on 21 November 2026. Build scripts now resolve files relative to this repository. TOAST UI Editor 3.2.2 and Lucide 1.8.0 are prototype dependencies, not a decision about the native editor or icon framework. Their notices are in `vendor/` and the root third-party notice.
