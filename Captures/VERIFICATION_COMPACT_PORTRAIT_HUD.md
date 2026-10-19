# Phố Việt 1.19.0 — compact portrait HUD and wider camera

## User-confirmed behavior

- Remove the portrait bottom left/right buttons and the double-tap instruction box.
- Merge distance, Coin, lives and navigation/map into one horizontal bar at the top;
  use a translucent background and one small notification strip directly below.
- The user selected a wider field of view so objects look approximately 25% smaller
  while the game still fills the display.

## Implementation

- `PhoVietPortraitHud.cs` owns the portrait layout: one 96-unit bar at y=10 in the
  existing 480-unit safe-area coordinate system, followed by a 28-unit notice at
  y=112. The background opacity is 64%; notice opacity is 70%. The pause button,
  street, speed, work stage/distance, turn instruction, reward and an 80×80 map
  fit within the same bar. Long labels use measured ellipsis rather than wrapping
  into other fields. No-work, pickup, boarding, delivery and alighting are handled.
- Portrait navigation no longer draws the previous separate GPS card. The map
  uses thin compact strokes and can show the city grid/player without an active job.
  Existing immediate turn preview, destination and reroute calculations are retained.
- Notifications remain a single gold-on-teal line; crash/recovery messages use the
  same compact strip. They no longer move down over the gameplay scene.
- `PositionCamera` uses `2 * atan(tan(65° / 2) / 0.75)` (about 80.7°) in portrait.
  This makes perspective-projected object dimensions 75% of the previous value.
  The camera position, collision simulation and full-screen viewport are preserved.
  Landscape uses its existing camera and HUD.
- `GameplayPointerAllowed` protects the compact HUD/notification and safe insets,
  while making the former bottom-controls region available for double-tap kicks.
  Portrait swipes that start on the HUD do not steer. Swipes/double taps in the
  world remain available; the compact pause button retains touch input.
- Version configuration and web expectations advance to 1.19.0. The installed
  vector motorcycle icon family is retained.

## Verification

- **1,520 assertions passed in 21 Unity suites**, including 21 new checks covering
  exactly 75% projected rider size in all four lanes, normal/chase cameras,
  unchanged full-screen viewport, bottom-space double taps, HUD exclusion and
  home-indicator safety. Existing traffic, 3 lives, pursuit, collision, jobs,
  routing, waypoint, mobile camera and art suites passed.
- Three native portrait screenshots were visually inspected: pickup, delivery
  with `Coin: 3,450`, and idle with a long street name/maximum Coin value. The
  unified bar, readable text, small notice, visible waypoint and empty bottom
  controls area were confirmed. `compact-hud-visual-results.json`: three frames.
- Browser smoke now swipes instead of tapping the removed steering button.
  The job UI suite checks the cleared bottom panels and uses actual touch on
  the compact pause button.

## Web build

- WebGL build `build_06f6acf08fa3` succeeded in 202.578 seconds with zero errors
  and one expected warning disabling the unused Pipeline runtime plugin.
- Version `1.19.0`, offline cache `33277f41ca629b86`.
- Real browser screenshots of the portrait HUD and actual pause-button touch
  result were visually inspected; the separate desktop HUD was also inspected.
- Unity is in Edit Mode with jobs enabled, 0 Coin and the 1775m record preserved.

- **67 browser checks passed** on the final artifact: 37 WebGL/PWA/offline/input
  checks, 10 real canvas job/HUD checks and 20 fullscreen checks for phone/tablet
  portrait/landscape. There were no JavaScript or Unity runtime errors. Evidence:
  `compact-hud-web-results.json`, `compact-hud-ui-results.json`,
  `fullscreen-game-compact-hud-results.json`.
- The two new browser assertions confirm removal of the bottom control panels
  and successful real-touch activation of the compact portrait pause button.
- `git diff --check` passed. Existing local Blender edits, the backup and untracked
  `huongdan.md` are excluded. Published-site verification is appended below.

## Publication

- Source/artifact commit `c5cb13396faf422c7b7de038fa32c4ebed8addce` was pushed to
  `origin/main`. GitHub Pages deployed that exact SHA successfully:
  https://github.com/denispham1107/taixecongnghe/actions/runs/39276271603 .
- **26 live checks passed** at https://denispham1107.github.io/taixecongnghe/ :
  version/cache, every deployed resource, real WebAssembly startup, start/pause
  input and zero JavaScript/Unity runtime errors. Evidence:
  `compact-hud-live-results.json`.
- The local verification server was stopped. Unity remains in Edit Mode; the
  original Blender edits, backup and untracked handoff document remain separate.

Browser phone/tablet profiles verify layout and touch behavior; no physical iOS
device performance claim is made.
