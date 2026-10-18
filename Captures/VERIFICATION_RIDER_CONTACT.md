# Phố Việt 1.7.3 — solid rider contact during pursuit

18/10/2026, Asia/Bangkok.

## Fix

Previously, pursuit impacts only reduced speed. The collision test also skipped all traffic during the 1.1 second damage cooldown, so vehicles could pass through the rider.

Pursuit now checks solid contact independently of that cooldown. A swept relative-motion test finds the earliest contact, stops lateral penetration, and corrects the scrolling road displacement for longitudinal contact. Scenery, traffic, crash effects, police position and travelled distance share that correction. Cooldown still limits repeated damage/audio; it never disables solid pursuit contact. The rider can steer away from an obstacle, and blocking still allows police to close and capture. Ordinary ride impacts retain their falling/game-over behavior.

## Unity verification

**268 assertions passed** on the final source:

- `VerifyRiderContacts`: 36 — same/opposing motorcycles, cars and large vehicles; high-speed swept hits; repeated contact during cooldown; crossing and lateral contact; downed and kicked motorcycles; airborne clearance; adjacent-lane near misses; sustained contact with a stopped car; escape by steering; ordinary crash and pursuit capture.
- `VerifyPursuit`: 39 — kick, chain collisions, pursuit speed, capture, jail and restart.
- `VerifyRearRandomPolice`: 60 — rear entry, randomization, police avoidance, swept collisions and 1.2 second recovery. Its stress test now accepts capture/restart when a rider is physically blocked; damage immunity is no longer treated as a traffic bypass.
- `VerifyTrafficDynamics`: 82 — braking, lane changes, junction turns, mass-based impacts and 90 second traffic stress.
- `VerifyGameplay`: 23.
- `VerifyCrossTraffic`: 28.

Evidence: `rider-contact-Verify*-results.json`. Each file records the actual Unity script result and assertion names.

## Release validation

- Native WebGL build `build_90ee65870a0b`: succeeded, **0 errors**, 187.753 seconds. One expected warning disables the unused Pipeline remote-control plugin in player builds.
- Version **1.7.3**, offline cache **629418cfd000a117**.
- **36 real-browser checks passed** on the staged artifact: WebAssembly startup, keyboard input, touch/double-tap input, mobile rotation, installability, complete offline resource caching and startup with the network disabled. No JavaScript or Unity runtime errors. Actual mobile gameplay screenshot inspected.
- Browser evidence: `rider-contact-web-results.json`. Chrome mobile profiles emulate viewport/touch; this is not physical iPhone hardware testing.

## Deployment

- Game/source commit `15b8c940fae321922d0e6a420bb0429f01de3e79` pushed to `main`.
- GitHub Pages deployment succeeded: https://github.com/denispham1107/taixecongnghe/actions/runs/39081261692
- Live game: https://denispham1107.github.io/taixecongnghe/
- **22 live checks passed**: version/cache match, all precache resources plus service worker/manifest match the staged build, actual Unity WebAssembly initializes, start/pause input smoke completes, and no JavaScript/Unity runtime errors. The deployed font was also compared byte-for-byte separately. Paused game screenshot inspected.
- Live evidence: `rider-contact-live-results.json`; screenshot `browser-rider-contact-live.png`.
- Unity restored to Ready, time scale 1, version 1.7.3; saved best distance 1775 preserved.
- Existing installations download the complete update while online and activate it after old game windows close. No site-data reset is needed.
