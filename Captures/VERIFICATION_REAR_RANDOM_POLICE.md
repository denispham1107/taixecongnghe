# PHỐ VIỆT 1.7.0 — Rear entry, random runs, police traffic

Verified 18/10/2026 (Asia/Bangkok), Unity Editor Play Mode.

## Changes verified

- Same-direction racers and drowsy trucks start entirely behind the camera, including their name signs. They move continuously into view through the bottom edge, overtake, then begin their dangerous weave. Approaching vehicles remain active behind the ordinary traffic retirement boundary. Both ordinary and chase cameras are covered.
- Every restart replaces the gameplay random generator with a fresh run seed. Vehicle choice, opening traffic, spawn timing, cross-traffic direction and turning, and dangerous weaving vary. Twenty-four consecutive restarts produced distinct seeds and opening traffic signatures, with more than ten model choices represented.
- Police predict traffic movement and steer or brake around moving, stationary and fallen motorcycles, cars and trucks. Swept collision checks prevent tunneling; unavoidable contacts stop at the impact and transfer a mass-dependent impulse. Police cannot arrest while fallen. Recovery takes exactly 1.2 seconds of game time; pause freezes the timer. Police resume from their actual position after standing up.
- The arrest approach also respects frozen traffic bodies. Ordinary combat, self-defense exemptions, oncoming hazards, crashes, house fire/smoke, turns and chain collisions remain covered by regression checks.

## Automated checks

All six final suites passed: **352 assertions**. Machine-readable results: `tests-v1.7.json`.

| Suite | Passed |
| --- | ---: |
| VerifyGameplay | 23 |
| VerifyCrossTraffic | 28 |
| VerifyPursuit | 39 |
| VerifyReckless | 120 |
| VerifyTrafficDynamics | 82 |
| VerifyRearRandomPolice | 60 |

Includes high-speed police collision checks, moving and opposing obstacles, a 40-second combined chase and the existing 90-second integrated traffic simulation. Test helpers preserve the saved best distance.

## Visual inspection

- `36-rear-entry-mobile-final.png`: 1440 × 900 landscape, rear truck enters from the lower edge. Filename contains an old mobile label; the actual image is landscape.
- `37-overtakes-mobile-final.png`: 1440 × 900 landscape, continuous overtaking trajectory and readable vehicle sign.
- `34-police-avoids-traffic.png`: 1440 × 900 landscape, police steer around a stationary taxi.
- `38-police-fallen-mobile-final.png`: 1440 × 900 landscape, fallen police motorcycle and remaining recovery timer.
- `39-police-fallen-portrait-final.png`: 540 × 960 portrait, fallen motorcycle and recovery HUD fit the phone-shaped Game View.

Portrait was inspected in the Editor. No physical mobile-device build or touch-device session was performed in this update.

## Windows build

- `Builds/Windows/PhoViet.exe`, version 1.7.0: build succeeded, 0 errors, 29.814 seconds.
- One expected warning: Pipeline runtime remote control has no player configuration and is disabled in the standalone game. It does not disable gameplay.
- Hidden headless standalone smoke test stayed running for five seconds, with 0 matching exception/fatal errors in `windows-v1.7-smoke.log`. This checks startup; gameplay was exercised in the Editor tests and visual scenarios above.
- Unity was restored to Play Mode at the Ready screen, time scale 1, traffic surprises and reckless traffic enabled.
