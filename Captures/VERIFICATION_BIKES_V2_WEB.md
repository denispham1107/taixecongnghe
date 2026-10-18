# Phố Việt 1.15.0 — Motorcycle v2 WebGL release

## Source and scope

- Integrates Claude Code commit `cb95f85` from `xe-may-v2` into `main` by
  fast-forward. No gameplay behavior or art redesign was requested for this release.
- Six new motorcycle/rider designs, 17-bone rigs, seven Legacy clips and three
  LODs are already installed in the saved Unity scene. Skinning is Bone2.
- Parked motorcycles and three shared static branch meshes use the new art.
- Source scene/FBX/controller files were unchanged relative to that commit when
  verification began. Local changes in four `.blend` files, the untracked Blender
  backup and `huongdan.md` are preserved and excluded from the release commit.
- Release version advanced from 1.14.0 to 1.15.0. The browser job verifier accepts
  an optional capture label to avoid overwriting the previous release evidence.

## Verification

- **1,335 assertions passed in all 17 Unity suites**, freshly run for this release,
  including all 8 portrait-camera cases omitted from the original v2 art report.
- BikeArt, CarArt, Gameplay, Pursuit, RiderContacts, Reckless, RearRandomPolice,
  TrafficDynamics, CrossTraffic, JunctionRoutes, JunctionRefinements, RoadDrivers,
  SafeArea, StreetArt, Jobs and JobEdges passed.
- Evidence: `bikes-v2-web-Verify*-results.json`. Jobs were enabled for the job
  suites and restored afterward. The existing 0-Coin wallet and 1775 m record
  were retained. New hero art was visually inspected against the saved native
  evidence; browser visual verification follows with the built artifact.

## Build, browser checks and publication

- WebGL build `build_6513b7de5059`: Succeeded in 25.488 seconds (incremental),
  zero errors. One warning disables the unused Pipeline plugin in player builds.
- Version 1.15.0, cache `b28b2ada2d25ff2e`.
- **36 local browser checks passed**, including actual WebAssembly startup,
  keyboard/touch smoke, outer-lane phone camera scenarios, full-viewport rotation,
  PWA installability, complete offline caching and network-disabled startup.
- No JavaScript or Unity runtime errors. Results: `bikes-v2-web-results.json`.
  The new white player scooter and new rider were visually confirmed in the
  built game (`bikes-v2-web-ready.png`).
- **8 dedicated job UI checks passed**, including actual mouse and touch
  selection through the canvas buttons, GPS display and phone rotation. Results:
  `bikes-v2-web-ui-results.json`; screenshots use the `bikes-v2-web-*` prefix.
  The phone GPS screenshot was visually inspected with the new rider and scooter.
- `git diff --check` and JavaScript syntax checking passed.
- Publication and live verification are recorded after deployment.

Phone/iOS browser profiles emulate layout and touch; no physical device FPS or
physical iPhone/iPad verification is claimed for this release.
