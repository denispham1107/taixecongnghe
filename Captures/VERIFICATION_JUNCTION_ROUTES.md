# Phố Việt connected junction routes — 1.10.0

## Confirmed behavior

The user explicitly selected: outer-left lane turns left, outer-right turns right,
middle lanes continue straight. One matching keyboard/button/swipe input in the
arrow zone queues a turn; outside that zone input continues to change lanes.
The user also selected retaining the same outer lane after the turn, including
outer-left after a left turn.

Each intersection has sixteen filled arrows, four on each approach, using exactly
the same material/color as the crosswalks. The selection zone is 12–30 metres before
the junction centre. Moving to a different lane cancels the queued turn; repeating
the chosen input retains it. The automatic curve has a 90-degree heading change,
appropriate corner speed, rider lean and a damped trailing camera. The journey,
difficulty clock, records and police pursuit continue instead of starting a new run.

## Scene and route implementation

- Five pooled rows of seven city blocks. Incoming and outgoing branches include
  houses, Vietnamese food/grocery/repair shops, trees, pedestrians, signals and
  detailed parked motorcycles. The previous junction remains behind the player
  while fresh branches are placed at the next junction. Meshes/materials are shared.
- Forty-eight Vietnamese road names, including the original seven. Entered roads
  are not repeated in the same run; extended names remain available after the list.
- After a turn a rigid rebase transforms scenery, traffic, effects, police, trail
  and camera together. The chosen street becomes the normal forward road. Existing
  traffic continues on its former road with collisions; it is not despawned at the
  turn to clear the player's path.
- Rider contacts include longitudinal position and heading during the curve.
  Pursuit still loses immediately on traffic contact. Corner crashes/arrests stay
  at their real position. Police follow the stored route, avoid traffic, collide
  physically and retain the 1.2-second fall/recovery rule.
- World signs use a separate depth-tested, fog-aware font material, with the dynamic
  atlas refreshed as needed. They no longer draw through buildings on side streets;
  GUI fonts remain independent.
- Existing local edits to both Blender source `.blend` files were present before
  this task, preserved and excluded from the release commit.

## Unity verification

**858 assertions passed**. `routes-Verify*-results.json` contains the results.

| Suite | Assertions |
| --- | ---: |
| VerifyJunctionRoutes | 92 |
| VerifyBikeArt | 156 |
| VerifyCarArt | 204 |
| VerifyRiderContacts | 45 |
| VerifyTrafficDynamics | 82 |
| VerifyRearRandomPolice | 60 |
| VerifyReckless | 120 |
| VerifyCrossTraffic | 28 |
| VerifyPursuit | 39 |
| VerifyGameplay | 23 |
| VerifySafeArea | 9 |

Route tests cover all four painted approaches, scenery and font materials, selection,
cancellation, repeated input, both turn directions, true curved motion, frozen pause,
exact exit lanes, continuing distance, a usable next junction, twelve successive
turns with distinct names, a bounded pool, immediate collision loss during both curves,
corner crash position, continued pursuit and police collision/recovery on the route.

The older gameplay assertion expecting all seven street names to be simultaneously
visible was updated to check that those names remain in the expanded catalogue and
visible signs match the current/next streets. Signal checks now cover all five road rows.

Six actual GPU-rendered views show approach, mid-turn and exit in each direction:
`routes-left-*.png`, `routes-right-*.png`. `routes-frame-evidence.json` records
106 sampled simulation steps; maximum measured camera orientation change was about
6.4 degrees per sampled step after accounting for the rigid coordinate rebase.
Both the native images and desktop/mobile WebGL screenshots were visually inspected.

## WebGL

- Build `build_2bd4c96a9894`: succeeded, **239.102 seconds**, **zero errors**.
- One expected warning disables the unused Pipeline plugin in player builds.
- Version **1.10.0**, offline cache **76daf950dcb6cd26**.
- **36 browser checks passed**: real WebAssembly startup, keyboard/touch smoke,
  rotation, full viewport coverage, installability, complete offline cache and
  network-disabled startup. Zero JavaScript/Unity runtime errors.
- Evidence: `routes-web-results.json`.
- Phone/iOS browser profiles emulate layout and touch; no physical mobile hardware
  was used for these checks.

## Publication

The tested source and WebGL artifact are ready for the authorized GitHub Pages release.
Deployment and live verification are recorded after publication.
