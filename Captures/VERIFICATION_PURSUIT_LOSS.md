# Phố Việt 1.7.4 — immediate traffic-collision loss during pursuit

18/10/2026, Asia/Bangkok.

## Confirmed user rule

While CSGT is chasing, collision with ANY traffic vehicle immediately loses the ride. The user explicitly rejected the slowdown/recovery/continued-driving behavior shipped in 1.7.3. This release supersedes that behavior and its verification report.

## Implementation

- Removed the pursuit-specific slowdown and road/vehicle displacement response.
- A swept rider/traffic collision immediately calls the ordinary crash flow: state becomes Falling, speed becomes zero and input is disabled during that same simulation tick. The existing fall animation then ends at GameOver. No waiting for police to catch up after a traffic collision.
- During pursuit, neither a damage cooldown nor a kicked vehicle's grace period can skip a collision. Standing, fallen, crossing, same-direction, oncoming and reckless traffic all use the same collision decision. Actual airborne clearance and adjacent-lane near misses remain clear.
- Crash stops the police siren. Police catching the rider without a traffic collision still uses capture/jail.

## Verification

**277 Unity assertions passed** on the final source:

- `VerifyRiderContacts`: **45**, including immediate loss/input lock for motorcycle, car and large vehicle contacts in both directions, cooldown/grace bypass prevention, both crossing directions, side impact, fallen motorcycles, reckless riders and drowsy trucks. Also covers real ride ticks approaching a stopped car, GameOver transition, restart, near misses and independent police capture.
- `VerifyPursuit`: 39, with the obsolete slowdown expectation replaced by the user's immediate-loss rule; capture/jail checked separately.
- `VerifyRearRandomPolice`: 60, including avoidance/recovery and integrated chase stress that restarts after immediate collision loss or capture.
- `VerifyTrafficDynamics`: 82.
- `VerifyGameplay`: 23.
- `VerifyCrossTraffic`: 28.

Evidence: `pursuit-loss-Verify*-results.json`.

## Release

- Native WebGL build `build_1adcb1fae119`: succeeded, **0 errors**, 178.675 seconds; one warning for the unused Pipeline plugin being disabled in player builds.
- Version **1.7.4**, cache **f35224791a76af40**.
- **22 local WebGL checks passed**: build version/cache, resource integrity, actual Unity WebAssembly startup and start/pause input smoke; no JavaScript/Unity runtime errors. Evidence: `pursuit-loss-local-results.json`.

## Deployment

- Game commit `2e176c52ce37e7750974f370d8bcf2c177f9315a` pushed to main.
- GitHub Pages deployment succeeded: https://github.com/denispham1107/taixecongnghe/actions/runs/39082246356
- Live URL: https://denispham1107.github.io/taixecongnghe/
- Live build version/cache verified as **1.7.4 / f35224791a76af40**.
- **22 live checks passed**: exact deployed resources, real WebAssembly startup, start/pause input smoke and zero JavaScript/Unity runtime errors. Evidence: `pursuit-loss-live-results.json`.
- Unity restored to Ready, time scale 1, version 1.7.4, saved best 1775 preserved.
