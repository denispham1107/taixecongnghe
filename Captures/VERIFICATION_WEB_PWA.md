# Phố Việt WebGL / PWA 1.7.1

Verified 18/10/2026, Asia/Bangkok. Unity 6000.5.6f1.

## Build and source

- Initial complete Unity project imported to `main`: `23416fc`.
- Final WebGL build: `build_befc96788ea1`, Succeeded, 0 errors, 162.002 seconds.
- One warning disables the Pipeline remote-control plugin in the standalone player. Gameplay is independent of that plugin.
- Version 1.7.1, offline cache `1c36e05dd4c9af0e`. Deployment files in `docs` are copied from this exact successful build.
- Gzip with JavaScript decompression fallback works without custom GitHub Pages response headers. Threads are disabled; WASM uses the Safari-compatible MVP feature set.
- Primitive component types are explicitly preserved through `Assets/PhoViet/link.xml`. Browser console tests caught and verified the fix for native colliders stripped from the initial WebGL build.
- Web GUI uses portable button glyphs and a generated five-point star instead of relying on unavailable OS font fallback.
- Portrait cameras track the rider laterally so steering toward an outer lane keeps the character in view. Landscape camera behavior is unchanged.

## Local browser validation

`AgentScripts/VerifyWeb.cjs` passed **36 checks**, using real Chrome with software WebGL rendering. Results: `web-local-results.json`.

- HTTP 200 for shell, manifest, build metadata, service worker and every precached resource.
- Correct 192/512 px icons, scoped standalone manifest, and no manifest parse errors.
- Chrome installation eligibility: **0 installability errors**, tested in a fresh, non-incognito profile. That disposable profile was removed after testing.
- WebAssembly initialization, actual scene rendering and loading-screen dismissal.
- Keyboard start, lane change, kick and pause; shell-to-Unity pause message.
- Entire game cached, network disabled, offline page reloaded and full WebAssembly game initialized again.
- Android profile: actual touch start, lane button and double tap, portrait display and landscape resize.
- iPhone profile: home-screen installation instructions accessible.
- **0 JavaScript / Unity runtime errors** during the final run.

Screenshots in `Captures/browser-local-*.png` were visually inspected, including the ready screen, pause menu and mobile controls. Browser profiles emulate screen size and touch; they do not validate physical Android hardware or the Safari engine on an actual iPhone.

## Reproduce

Build via `PhoViet.Editor.PhoVietWebBuild.Build`, stage all build output in `docs`, serve this project root over localhost, then run:

```text
node AgentScripts/VerifyWeb.cjs http://127.0.0.1:8765/docs/ local
```

Requires Playwright, Sharp and Chrome. `NODE_PATH` can point to the installed Node dependency directory.
