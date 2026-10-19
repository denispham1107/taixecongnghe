# Phố Việt — vector motorcycle app logo

The user rejected the raster illustration and selected a front-facing motorcycle,
five-pointed yellow star and road, drawn as bold playful vector geometry in
red/yellow/dark teal. The earlier PV monogram draft was not published.

- Editable source: `ArtSource/Web/pho-viet-app-icon.svg`. No embedded image, external
  font, generated illustration or raster reference is used. The obsolete raster
  master was removed; Git retains the previous release history.
- Desktop/Android exports: 192px and 512px PNG. Android maskable export: 512px.
  Apple home-screen export: opaque 180px. PNGs are platform exports of the SVG.
- New `moto` filenames are referenced by the manifest, favicon and Apple touch
  link. Generic legacy URLs also contain the vector artwork for compatibility.
- The icon was visually inspected at 512px and 192px. The scooter, star, mirrors
  and wheel fit inside the central maskable circle; the road/background extend
  beyond that safe area. The star has all five points.
- `GenerateWebIcons.cjs` regenerates the complete family from the vector master.
- `RefreshWebShell` now copies icon assets and refreshes icon links in the built
  HTML while retaining the generated Unity loader/configuration.
- Unity compilation passed. Six asset checks passed: all four gameplay build
  files have unchanged SHA256 values, the SVG contains geometry only, and the
  offline cache changed. This is an icon/shell refresh of 1.18.0; cache ID
  `d768860be0a4d815` identifies the revision.

All **37 browser checks passed**, including the new manifest icon dimensions,
installability, full offline cache, actual WASM startup offline/online, desktop
keyboard and phone touch/rotation, with zero JavaScript/Unity runtime errors.
Evidence: `vector-logo-web-results.json`. `git diff --check` passed.

Published-site verification is recorded after completion. Existing Blender edits,
backup and untracked `huongdan.md` are excluded from this update.
