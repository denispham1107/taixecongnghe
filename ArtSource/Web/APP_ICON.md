# Phố Việt — vector motorcycle logo

The current editable source is `pho-viet-app-icon.svg`. It contains SVG paths,
ellipses and rectangles only: no embedded bitmap, external image or font.
The approved design is a front-facing red scooter with a complete five-pointed
yellow star and a road, using dark teal, red, yellow and warm cream.
The main scooter stays within the central maskable circle for Android crops.

Run `node AgentScripts/GenerateWebIcons.cjs` to export 192px and 512px desktop/
Android icons, the 512px maskable icon, and the opaque 180px Apple touch icon.
The PNGs are raster exports of the vector, as required by these install surfaces.
`icon.svg` remains a real editable vector. The new `*-moto-*` URLs distinguish
this artwork from the previous installed image; legacy generic URLs are updated
with the same vector exports for compatibility. No image generation is used.

Web refresh: run `RefreshWebIcons.Main` in Unity, then publish `Builds/WebGL` to
`docs`. It updates icon files, manifest, favicon/Apple links and the offline cache
without rebuilding unchanged Unity gameplay/WASM. The game version stays 1.18.0;
the cache hash identifies this icon revision.
