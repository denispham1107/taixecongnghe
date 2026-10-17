# Phố Việt 1.7.2 — edge-to-edge mobile gameplay

18/10/2026, Asia/Bangkok.

## Result

- Mobile and installed-app gameplay uses the entire available app viewport. The persistent web header becomes an optional floating menu. Opening controls overlays the scene without shrinking the canvas.
- Safe-area padding was removed from the world/canvas. CSS cutout/home-indicator values are passed as normalized fractions to Unity; only GUI layout and touch exclusions are inset. The camera and full-screen tints still cover the entire canvas.
- The installed manifest prefers fullscreen. Native fullscreen is requested from the user's play gesture where supported; unsupported browsers use the CSS app viewport. Unity's legacy SetFullscreen fallback is no longer used for unsupported iPhone APIs.
- Resize, orientation, app resume and restored-page events refresh sizing. Installed iOS apps use the larger of the layout and visual viewport heights, avoiding known cases where visualViewport incorrectly subtracts the safe area. Physical screen dimensions are not forced, preserving iPad split-view/window sizing.

## Validation

- Native WebGL build `build_e4f95ceeefd7`: succeeded, **0 errors**, 182.926 seconds. One expected warning disables the unused Pipeline remote-control plugin in the player.
- Subsequent CSS/JS-only updates were copied using `PhoVietWebBuild.RefreshWebShell`, which regenerates the precache list and cache version without relinking unchanged WASM.
- Final staged version **1.7.2**, cache **2dd84d7eb8e3a820**.
- **169 layout assertions passed** across seven device profiles plus a reduced-height WebKit regression profile. Every profile runs portrait, landscape and rotation back. Tests include nonzero top/bottom/side cutouts, no native Fullscreen API, zero canvas edge gaps, safe HUD bridge data, no scroll overflow, and working overlay menus. These layout tests mock only the Unity loader.
- **71 Unity assertions passed**: safe-area input/layout (9), pursuit/combat (39), gameplay (23).
- **36 real-browser gameplay/PWA checks passed** during the 1.7.2 update, including offline startup, installability and touch input. After final web-only sizing adjustments, the focused real-engine fullscreen checks validate the final artifact separately.
- **20 real-engine checks passed** for iPhone and iPad portrait/landscape screenshots, full viewport coverage and usable overlay controls before the final reduced-visual-height guard. The guard itself is covered by the 169-case layout suite; the deployed final artifact is verified separately below.

Evidence: `fullscreen-layout-results.json`, `fullscreen-unity-results.json`, `fullscreen-game-local-results.json`. Actual rendered screenshots: `browser-fullscreen-local-iphone-portrait.png`, `browser-fullscreen-local-iphone-landscape.png`, `browser-fullscreen-local-ipad-portrait.png`, `browser-fullscreen-local-ipad-landscape.png`.

## Platform limits

Chrome device profiles simulate viewport/touch and injected cutout values; they are not tests on physical iPhones/iPads. Fullscreen covers the app's available viewport. Browser/system chrome outside that viewport remains OS-controlled.

- WebKit guidance on rendering under cutouts while keeping controls safe: https://webkit.org/blog/7929/designing-websites-for-iphone-x/
- Known installed-app visual-viewport height bug: https://bugs.webkit.org/show_bug.cgi?id=254868
- Separate OS-controlled status-bar regression on some iOS releases: https://bugs.webkit.org/show_bug.cgi?id=301994

User was asked which launch mode and iOS/iPadOS version produced the screenshots; platform-specific status-bar diagnosis remains dependent on that information.
