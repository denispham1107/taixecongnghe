# Phố Việt 1.18.0 — Visible customer locations, immediate turn GPS, app icon

## Implemented behavior

- The active pickup and drop-off each have a bright green downward arrow, a white/dark outline, a pulsing green halo and a distinct Vietnamese label (`ĐÓN KHÁCH` / `TRẢ KHÁCH`). Only the current stage's destination is shown. The marker is hidden when its anchor is outside the camera view, behind the camera, beyond the rendered distance, or no job is active. It can appear on a cross street and throughout a turn.
- The marker is a camera-facing mesh with a lightweight unlit overlay shader. Its apparent size remains readable at distance; no dynamic lights or bloom pipeline are needed on phones. It updates after camera movement each frame. The font atlas is refreshed for accented Vietnamese labels. The arrow tip sits near the curb/customer's feet; while the marker is visible in portrait mode, the short notice moves above the touch controls so that it cannot cover the arrow and its label.
- The GPS switches to the committed exit route in the first frame of a left/right turn, including late turns inside the intersection. Remaining distance, map position and heading update throughout the curve. A wrong turn immediately previews the detour to the original address.
- Authoritative city coordinates still advance once at the completed road rebase, preserving street names, traffic, passenger service, crash recovery and rerouting. Previewing the exit does not move the customer or double-advance a city node. Queuing a possible turn does not change the route before the turn actually starts.
- Installed desktop/Android/iOS icons now use a detailed cheerful driver in a red shirt with a complete yellow five-pointed star, a red scooter and Vietnamese street scenery. The full-square opaque source is in `ArtSource/Web/pho-viet-app-icon.png`; built-in imagegen prompt/provenance is recorded in `ArtSource/Web/APP_ICON.md`. `GenerateWebIcons.cjs` produces the 192px, 512px, maskable 512px and Apple 180px assets, plus a consistent legacy SVG wrapper.

## Code map

- `PhoVietJobMarker.cs` and `Resources/PhoVietWaypoint.shader`: visible waypoint geometry, halo, billboard, frustum gating and labels.
- `PhoVietGame.cs`: marker update after camera positioning, including paused/falling/service scenes.
- `PhoVietJobs.cs`: committed-turn presentation route, continuous map pose and remaining distance; existing city/job authority retained.
- `PhoVietRoutes.cs`: create the navigation preview when a turn starts.
- `PhoVietJobGui.cs`: use the current presentation route and rotating player heading; map group clipping remains intact on mobile.
- Web release configuration and browser version expectations advance to 1.18.0.

## Verification

- **1,499 assertions passed across 20 Unity suites**, including 49 new wayfinding checks. Coverage includes both curbs, labels, offscreen/behind-camera culling, a visible cross-street customer, normal and late turns in both directions, immediate detour distance, stable addresses and exactly-once city progression. Existing rider-life, pursuit, traffic, jobs, road turning, mobile camera and art suites also passed.
- The waypoint suite was rerun after final shader/text adjustments and the lower curb anchor used to reduce GPS overlap on portrait phones. Native pickup/drop-off screenshots were visually inspected, including white label readability and contrast against foliage/buildings: `wayfinding-native-pickup.png`, `wayfinding-native-dropoff.png`.
- The generated 192px installed icon was visually inspected. No changes were made to existing Blender models or their pending local source edits.

## Web publication

- Final WebGL build `build_cbd4291e9c2c` succeeded in 156.951 seconds with zero errors and one expected warning disabling the unused Pipeline runtime plugin. Version `1.18.0`, offline cache `c5de355dde0ea558`.
- **65 browser checks passed on the final artifact**: 37 real WebGL/PWA/offline/input checks, eight actual canvas job-selection/GPS checks and 20 fullscreen checks across phone/tablet portrait/landscape. There were no JavaScript or Unity runtime errors. Evidence: `wayfinding-web-results.json`, `wayfinding-ui-results.json`, `fullscreen-game-wayfinding-results.json`.
- The desktop GPS screenshot confirms the new curb arrow in the real WebGL player. An additional native portrait screenshot confirms the active marker and notification positioned above touch controls: `wayfinding-native-phone.png`. UI cards retain their normal overlay priority over the world scene.
- Unity returned to Edit Mode with jobs enabled, 0 Coin and the 1775m record preserved. `git diff --check` passed. Four pre-existing Blender edits, the motorcycle backup and untracked `huongdan.md` are excluded.
- Source/artifact commit `cd739110047f58f153aa4be8e9351ab2ace5347c` was pushed to `origin/main`. GitHub Pages deployed that exact SHA successfully: https://github.com/denispham1107/taixecongnghe/actions/runs/39223683110 .
- **22 live checks passed** at https://denispham1107.github.io/taixecongnghe/ : version/cache, every deployed artifact including the new icons, real published WebAssembly startup, start/pause input and zero JavaScript/Unity runtime errors. Evidence: `wayfinding-live-results.json`.
- The local verification server was stopped after checks completed. Existing Blender edits and untracked handoff files remain unchanged.

Chromium mobile/tablet profiles verify layout and touch behavior; this is not a physical iPhone performance measurement. Existing iOS home-screen entries may retain their previous icon; a newly added entry uses the updated Apple touch icon. Website data was not cleared by the update, preserving the existing Coin wallet.
