# Phố Việt awning repair, pot removal and leafy trees — 1.13.0

## Changes

- All twelve Blender shop models now use closed, thin solid fabric strips with
  adjoining edges, rather than single faces. Both roof and front valance are
  protected from distance simplification along with the Vietnamese lettering.
  Every fabric stripe remains intact at all three LODs.
- Decorative potted plants were removed from the sidewalk and balcony geometry
  of every shop. The street trees remain rooted in their existing pavement beds.
- All three street trees were rebuilt through the live Blender MCP. Rounded
  foliage masses were replaced by individual pointed leaves with folded surfaces,
  small thickness, central veins at close range, varied green tones and rotations.
  Smaller twigs, branching trunks, bark ridges and roots remain visible.
- Each tree uses 864 / 432 / 216 complete leaves across its three LODs, with
  decreasing triangle counts. Distant models keep actual leaf shapes rather than
  reverting to round canopy blobs. The shared 128px atlas remains in use.
- Existing FBX GUIDs and shared mesh references update all 35 blocks across the
  main road and every connected branch without rebuilding gameplay or traffic.
- Editable Blender source and regenerated FBX/catalogues are committed. Pre-existing
  local edits in PhoViet_Cars.blend and PhoViet_Motorcycles.blend are preserved and
  excluded from this release.

## Verification

- **40 Blender checks passed**, including **42,048 sampled ray directions** across
  the actual roof and underside geometry of all twelve shops at all three LODs.
  Support brackets are passed through when checking fabric normals and coverage.
  Evidence: fabric-leaves-blender-results.json.
- **310 Unity assertions passed**: StreetArt 186, JunctionRoutes 92, Gameplay 23,
  SafeArea 9. Scene checks now assess facade direction in each road's local frame
  so rotated active branches are checked correctly in Play Mode.
- Native rendered images inspected: fabric-leaves-game.png, fabric-leaves-tree.png,
  fabric-leaves-awning-lod0.png, fabric-leaves-awning-lod1.png and
  fabric-leaves-awning-lod2.png. These show intact fabric at every forced LOD,
  visible individual leaves and cleared sidewalk/balcony pots.
- The source tree was also inspected using Blender MCP look.
- No gameplay code or collision rules changed.

## Build and release

- WebGL build `build_1cea918e3ea0` succeeded in **18.176 seconds** (incremental),
  **zero errors**, one expected warning disabling the unused Pipeline plugin in
  player builds.
- Version **1.13.0**, offline cache **cd3f9b7d9cf608d8**.

- **36 local browser checks passed**, including actual WebAssembly startup,
  keyboard/touch input smoke, rotated full-viewport canvas, installability,
  complete offline cache and network-disabled startup. No JavaScript/Unity runtime
  errors. Evidence: fabric-leaves-web-results.json. Desktop and phone screenshots
  were visually inspected.
- Mobile/iOS profiles emulate viewport and touch; physical mobile hardware and
  frame-rate performance were not tested.
- Unity left in Play Mode Ready, version 1.13.0, time scale 1, best distance 1775 m.

- Source and tested artifact commit: `954f72f5110f832778a9e97d4af6ca28aca9a8b9`.
- GitHub Pages deployment succeeded for that exact commit:
  https://github.com/denispham1107/taixecongnghe/actions/runs/39122927809
- Live game: https://denispham1107.github.io/taixecongnghe/
- **22 live checks passed**: deployed version/cache and all resources match the
  tested artifact, real Unity WebAssembly initializes and accepts start/pause input,
  and no JavaScript/Unity runtime errors are reported.
  Evidence: fabric-leaves-live-results.json.
