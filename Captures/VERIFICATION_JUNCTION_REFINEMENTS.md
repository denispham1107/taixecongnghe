# Phố Việt junction refinements — 1.11.0

## Confirmed behavior

The user confirmed that, throughout the open intersection marked by the red box,
the left curb allows a left turn and the right curb allows a right turn. A matching
keyboard/button/swipe input can start the turn immediately without a prior queue.
The earlier arrow-zone queue remains available. The previous choice to retain the
same outer lane on the new road remains in effect.

- Side-road arrows are moved farther from the intersection, beyond the outside
  edge of their crosswalks. All four approaches retain the crosswalk paint material.
- The late-turn window extends seven metres before and after the junction centre.
  Eligibility uses the rider's actual position at the corresponding curb. Middle
  lanes and opposite-direction inputs continue to change lanes normally.
- Late turns follow a tangent-continuous curve, sampled by travelled arc length,
  at reduced corner speed. Rider lean, trailing camera, real traffic collisions,
  distance, street names and pursuit continue through the turn.
- After each turn, a randomized wave requests three to five same-direction vehicles
  from behind and three to five oncoming vehicles from ahead. One of each is tried
  immediately; the rest join at varied short intervals. Vehicle types and lanes vary.
- Rear entrants start entirely behind the camera and catch up before returning to
  ordinary driving. Oncoming entrants start far ahead. Spawn footprints are checked
  against active traffic and police. A full pool defers spawns; active actors are not
  stolen, teleported or removed to make space. All entrants remain collidable.
- Traffic impact during police pursuit still loses immediately. The separate police
  capture/jail flow and police avoidance/fall/recovery behavior remain unchanged.

Existing local changes to both Blender source `.blend` files were preserved and
excluded from the release commit.

## Unity verification

**963 assertions passed**, recorded in `junction-fix-Verify*-results.json`.

| Suite | Assertions |
| --- | ---: |
| VerifyJunctionRefinements | 105 |
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

New checks cover all four arrow approaches on all five road rows, visible clearance
from crosswalks, unqueued left/right turns at five positions across the intersection,
rejection at the wrong curb/middle/past the far edge, road-contained finite curves,
exact exit lanes, continuing journey and the next junction. Pursuit contact during
a late curve loses immediately. Five randomized waves verify both directions,
six to ten total entrants, off-screen rear origins, catch-up, non-overlap, variation,
full-pool deferral and reset behavior.

Five actual GPU-rendered Unity images were inspected: `junction-fix-arrows.png`,
`junction-fix-left-mid.png`, `junction-fix-right-mid.png`, and both
`junction-fix-*-traffic.png`. The capture exercised 247 simulation frames, including
a turn selected near the far side of the intersection. Final wave: four rear and
five oncoming vehicles. No player collision immunity was used for the captures.

## WebGL and publication

- Native WebGL build `build_23fd2a646d0a` succeeded in **222.378 seconds**, with
  **zero errors** and one expected warning disabling the unused Pipeline plugin
  in player builds.
- Version **1.11.0**, offline cache **dc9de09e54b74bdc**.

- **36 local browser checks passed**: real Unity WebAssembly startup, keyboard and
  touch smoke, rotated full-viewport canvas, installability, complete offline cache
  and startup with the network disabled. No JavaScript/Unity runtime errors.
  Evidence: `junction-fix-web-results.json`. Desktop and phone screenshots were
  visually inspected. Phone/iOS profiles emulate viewport and touch; these checks
  did not use physical Android/iOS hardware.

- Source and tested artifact commit: `21e2e1c13beb4ab2feee32c53109a2c4c5a3426e`.
- GitHub Pages deployment succeeded for that exact commit:
  https://github.com/denispham1107/taixecongnghe/actions/runs/39111187991
- Live game: https://denispham1107.github.io/taixecongnghe/
- **22 live checks passed**. Version/cache and every published resource match the
  tested artifact. The deployed Unity WebAssembly initializes and accepts start/pause
  input without JavaScript/Unity runtime errors. Evidence: `junction-fix-live-results.json`.
- Unity left in Play Mode Ready at version 1.11.0, time scale 1, exit traffic enabled,
  eleven four-wheel actors and thirty-three motorcycle/rider actors. Existing best
  distance **1775 m** retained.
