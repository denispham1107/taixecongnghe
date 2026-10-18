# Phố Việt passenger jobs, Coin and navigation — 1.14.0

## Implemented behavior

- Persistent local Coin wallet starts at zero and uses comma thousands separators.
  Only a completed drop-off pays the quoted reward, once. Crashes and restarts do
  not pay unfinished work or erase earned Coin.
- Three random jobs pause the game for selection. Pickups are distinct storefront
  addresses. Rewards start at 20 Coin and increase with combined pickup and trip
  distance; each offer has a different reward.
- A directed grid of 80 m blocks assigns stable street names within each run.
  The GPS plans legal straight/left/right routes and recalculates to the original
  pickup or destination after an incorrect turn or missed curb stop.
- A compact map shows route, target, distance, next direction, curb and reward.
  Three bundled female Vietnamese voice clips give repeated directions offline.
- Correct outer lane and physical curb position trigger automatic stopping.
  The customer walks out from a shop, boards, rides behind the driver and walks
  back into a shop at drop-off. Rear traffic yields during service and for exactly
  three seconds of subsequent riding, including corners; pausing freezes this
  timer. Other traffic contacts retain normal collision handling and any traffic
  collision during a police pursuit still immediately loses.
- Male and female customers were authored through the live Blender MCP, with
  14-bone rigs, Idle/Walk/Board/Sit clips and three detail levels each. Triangle
  counts: male 4688/2338/1091, female 5084/2541/1160. Each uses one shared palette
  material. Editable Blender and Python sources and imported FBX assets are saved.

## Verification

- **1,335 Unity assertions passed** in 17 suites: 116 job/route checks, 17 job
  edge cases and 1,202 existing gameplay, traffic, pursuit, vehicle art, junction,
  streets, safe-area and portrait-camera assertions.
- A simulated complete trip drove through actual junction controls, picked up,
  carried and dropped off a customer, verified no early payment, exact-once
  payment and persistence, then restored the original test wallet and record.
- Edge checks cover wrong-turn rerouting without moving either address, correct
  curb, one active passenger, imported clips/LODs, rear yielding, three-second
  timing through a corner and pause, and immediate pursuit collision loss.
- Native passenger poses inspected through Blender MCP and Unity rendered images:
  passenger-male-seated.png and passenger-female-seated.png. Pose changes deform
  the actual skinned mesh; passengers face forward and sit behind the driver.
- Existing three modified source files PhoViet_Cars.blend,
  PhoViet_Motorcycles.blend and PhoViet_Streets.blend predated this task; their
  local changes are preserved and excluded from this release.

## Release checks

- Final WebGL build `build_754073c7acfe` succeeded in 247.744 seconds, zero
  errors and one expected warning disabling the unused Pipeline plugin in player
  builds. Version 1.14.0, offline cache `91ec2b517ffdb2bb`.
- **36 final browser checks passed**, covering actual WebAssembly initialization,
  keyboard/touch smoke, Android rotation with full-viewport canvas, PWA
  installability, complete offline caching and network-disabled startup.
- **8 dedicated job UI browser checks passed**, including actual mouse and
  touch selection through canvas buttons on desktop and phone, layout rotation
  and no JavaScript/Unity errors. Screenshots `jobs-web-*-offers.png` and
  `jobs-web-*-gps.png` were inspected. Phone GPS sits above the rider, and its
  grid/route lines remain clipped inside the map card.
- Touch selection explicitly reads touch begin/end while the game is paused;
  it shares the same button bounds and safe-area mapping as the drawn cards.
- `git diff --check` passed. GitHub Pages release verification follows below.

Browser phone profiles emulate layout and touch; physical iPhone/iPad testing
has not been performed in this environment.

## Published release

- Source and tested WebGL artifact commit:
  `1cd602d40a84372401137841aeb65f14df839049`, pushed to `origin/main`.
- GitHub Pages workflow completed successfully for that exact commit:
  https://github.com/denispham1107/taixecongnghe/actions/runs/39143443936
- **22 live checks passed** at
  https://denispham1107.github.io/taixecongnghe/ : release version/cache, deployed
  resource contents, actual WebAssembly initialization, start/pause input and no
  JavaScript/Unity runtime errors. Evidence: `jobs-live-results.json`.
- Unity was returned to Ready, jobs enabled, 0 Coin and the existing 1775 m
  record preserved. The local verification HTTP server was stopped.
