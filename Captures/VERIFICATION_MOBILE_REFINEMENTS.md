# Phố Việt 1.16.0 — Mobile HUD, opening traffic and earlier navigation

## Confirmed behavior

- The floating web hamburger button is hidden on touch devices, including the installed mobile PWA. Desktop shell controls remain available.
- Gameplay uses one rounded dark teal notification with yellow text. Separate route, crossing, oncoming, reckless, sudden-brake/cut-in and pursuit banners are no longer drawn. Coin, GPS, record, pause and driving controls remain.
- For the first ten seconds of riding, opening vehicle counts, crossing counts and turn-exit counts are halved, rounding up for odd counts. Normal traffic waves advance at half their usual rate; reckless entries share those waves. Paused selection time and stopped passenger service do not advance the opening difficulty clock.
- Accepting any job starts three seconds of traffic yielding, measured in simulation time. Threatening longitudinal vehicles select a clear lane using sampled future separation along the lateral path. Ordinary same-direction traffic retains its normal road half. Vehicles brake/yield if a lane is blocked. For a blocked oncoming vehicle, the player also slows or stops, reserving the other vehicle's stopping distance; both continue yielding while stopped until a lane opens. The player resumes when the path clears. This replaces ordinary driving/weaving for those frames; it does not disable collision detection. Actual player/traffic contact still immediately loses, including during police pursuit.
- Navigation uses time-to-junction based on current speed: an initial prompt at about four seconds, then a reminder at about two seconds. It never queues obsolete prompts. Straight speech is suppressed when a subsequent turn on the planned route is within four seconds, or the direct pickup/drop-off is already very close.

The three-second behavior, the four/two-second speech timing and the single-notification interpretation were explicitly selected by the user before implementation. The user separately selected automatic player braking/stopping for the fully blocked oncoming edge case.

## Code map

- `PhoVietGame.cs`: opening density, traffic dispatch and unified notice drawing.
- `PhoVietExitTraffic.cs`: reduced opening traffic on the new road after turning.
- `PhoVietJobTraffic.cs`: acceptance timer, lane-clearance prediction, steering and yielding.
- `PhoVietTraffic.cs`: resets pooled vehicles' temporary yield state.
- `PhoVietReckless.cs`: resumes an oncoming reckless vehicle's original pace after yielding.
- `PhoVietJobs.cs`: begins/clears the acceptance timer and schedules early navigation prompts with route look-ahead.
- `Assets/WebGLTemplates/PhoVietPWA/app.js`: hides mobile hamburger control.

## Verification

- All **1,335 existing assertions in 17 Unity suites passed** freshly for this release.
- **50 new assertions passed** in `VerifyRideRefinements`: opening density, exact ten-second transition, reduced turn-exit waves, both directions and vehicle types yielding, blocked-lane braking, automatic player stopping and resumption, pause-aware timers, immediate collision loss with/without pursuit, four/two-second navigation and suppression of conflicting straight prompts.
- Total: **1,385 assertions in 18 Unity suites**. Evidence: `mobile-refinements-Verify*-results.json`. Gameplay, pursuit, reckless driving, traffic dynamics, rider contacts, jobs and job edges were rerun after the final blocked-oncoming refinement.
- The exit-wave regression test now runs at mature traffic density and excludes separately delayed crossing vehicles waiting off the main road from its exit-wave spacing assertion.
- **145 shell-layout checks passed** on eight phone/tablet profiles in portrait, landscape and rotate-back, with simulated safe-area insets. The mobile hamburger is hidden in every case. Evidence: `fullscreen-layout-results.json`.
- JavaScript syntax checks and `git diff --check` passed.
- Wallet remained 0 Coin; record remained 1775 m. Existing local Blender edits, Blender backup and untracked `huongdan.md` are preserved and excluded from this release.

## Build and deployment

- Initial WebGL build `build_f46282c398c4` succeeded in 214.952 seconds, with zero errors. One expected warning disables the unused Pipeline runtime plugin in player builds.
- The initial artifact passed 37 real browser checks, including WASM startup, keyboard/touch, rotation, the hidden mobile hamburger, installability and full offline startup. The final build below also includes the subsequently confirmed blocked-oncoming braking behavior.
- Final WebGL build `build_f48f5ed01e7b` succeeded in 162.780 seconds, with zero errors and the same expected Pipeline warning. Version `1.16.0`, offline cache `d300899120578550`.
- **37 final-artifact browser checks passed**, including actual WASM startup, keyboard/touch input, hidden mobile hamburger, rotation, PWA installability, full caching and network-disabled startup. Evidence: `mobile-refinements-web-results.json`.
- **8 real job UI checks passed**: both desktop mouse and phone touch select actual canvas job buttons; GPS and rotation render correctly. Evidence: `mobile-refinements-ui-results.json` and the `mobile-refinements-*-gps.png`/offers/landscape images.
- **20 real WebGL fullscreen checks passed**, including iPhone/iPad portrait and landscape with simulated safe-area insets and no floating hamburger. Evidence: `fullscreen-game-mobile-refinements-results.json`.
- Total real browser checks on the final artifact: **65**, with zero JavaScript or Unity runtime errors. Desktop and phone GPS screenshots were visually inspected: the yellow notice has a dark teal rounded panel and does not overlap GPS.
- Unity is back in Edit Mode. Publication and live verification follow below.

Phone profiles emulate layout and touch in Chromium; they do not constitute a physical iPhone/iPad performance test.
