# Phố Việt 1.8.0 — original Blender motorcycles, riders and animation

18/10/2026, Asia/Bangkok.

## Approved scope and result

The user selected the proposed semi-realistic playful style before modelling began. All modelling, rigging, animation creation and FBX export ran in the user's live Blender through Blender MCP. No paid model generator or third-party vehicle asset was used.

- Six original designs: player scooter, city scooter, step-through, sport bike, reckless rider bike and traffic-police motorcycle.
- All **33 motorcycles/riders** replaced: 19 gameplay actors and 14 sidewalk motorcycles. Cars/trucks retain their existing visuals.
- Curved bodywork, tyre tread, alloy spokes, brake rotors/calipers, forks, spring shocks, engine/exhaust, mirrors, lamps, seat seams, number plates, adult riders with facial features, helmets, clothing details and full five-point stars on the player's shirt.
- CSGT has a khaki uniform, red/gold shoulder boards, white gloves, black shoes, marked helmet, touring screen/panniers and flashing priority lights. Dynamic brake lights align with the new motorcycle tails.
- One atlas material and 17-bone rig per design. Each has three LODs: approximately 24–27k / 10–12k / under 6k triangles. Meshes, materials and clips are shared across actors.
- Seven authored clips per model: Idle, Ride, KickLeft, KickRight, FallLeft, FallRight, Recover. Wheels use actual signed road speed. Animation pauses with gameplay, resets on recycling, and samples independently of previous renderer visibility. Left/right feet are checked in Unity world space.
- Physics retains the existing vehicle roots. Fallen skins settle on the road, and the police visually rise during their existing recovery window. The immediate-loss-on-traffic-contact rule during pursuit remains unchanged.

## Editable sources

- `ArtSource/Blender/PhoViet_Motorcycles.blend`: all six designs, rigs/actions, packed palettes/font, studio lights/cameras; saved on the ride-animation lineup.
- `ArtSource/Blender/pho_viet_bikes.py`: original procedural authoring source and `rebuild_all()` entry point.
- `Assets/PhoViet/Art/Bikes`: FBX files, atlases, surface masks, materials and metadata.
- `PhoVietBikeImport` imports legacy clips; `PhoVietBikeUpgrade` replaces actors while keeping pool/physics references. Parked bikes formerly baked into street meshes are removed by rebuilding the same street blocks and reusing their persisted geometry/material assets. The combiner protects skinned skeletons.

## Unity verification

**562 assertions passed** on the final source/scene:

| Suite | Assertions |
| --- | ---: |
| VerifyBikeArt | 156 |
| VerifyRiderContacts | 45 |
| VerifyPursuit | 39 |
| VerifyRearRandomPolice | 60 |
| VerifyTrafficDynamics | 82 |
| VerifyGameplay | 23 |
| VerifyCrossTraffic | 28 |
| VerifyReckless | 120 |
| VerifySafeArea | 9 |

Art checks cover all 33 rigs/clips/LODs, parked idling, real wheel/torso movement, both kick directions, frozen pause, falling riders, reuse after a fall, police recovery and unchanged loss rules. A baked player-fall mesh has minimum world height **0.0229 m**, above the road. Evidence: `blender-art-Verify*-results.json`.

Blender and actual Unity renders were inspected. `CaptureBikeEvidence` captures on separate editor frames to include updated GPU skinning; `CaptureBikeActions` also checks baked geometry. Images: `blender-hero.png`, `blender-lineup.png`, `blender-bikes-game.png`, `blender-ride.png`, `blender-kick-right.png`, `blender-victim-fall.png`, `blender-player-fall.png`.

## Web release

- Native WebGL build **build_b7def31fad2d** succeeded: **0 errors**, 232.937 seconds; one expected warning disables the unused Pipeline plugin in player builds.
- Version **1.8.0**, atomic offline cache **e5ab03439ec7d231**.
- **36 real-browser checks passed** on the final artifact: WebAssembly startup, keyboard/touch controls, rotation, PWA installability, complete offline caching and startup with the network disabled. Zero JavaScript/Unity runtime errors. Evidence: `blender-art-web-results.json`.
- GPU-updated Unity frame evidence completed successfully (`blender-art-frame-evidence.json`); the right-kick screenshot was visually inspected with the foot extended on the correct side.
- Browser device profiles simulate viewport/touch; they are not tests on physical iPhones or Android devices.

## Deployment

- Commit `0a61dce99e9caa56890f159142f815a92eb9947d` contains the Blender sources, imported art, updated scene, verification and built WebGL release.
- GitHub Pages deployment succeeded: https://github.com/denispham1107/taixecongnghe/actions/runs/39086869734
- Live game: https://denispham1107.github.io/taixecongnghe/
- **22 live checks passed**: deployed version/cache and all resources match the tested artifact, actual WebAssembly starts, start/pause smoke completes and no JavaScript/Unity runtime errors occur. Evidence: `blender-art-live-results.json`.
- Unity left in Play Mode Ready, time scale 1, version 1.8.0, all 33 bike actors present; saved best distance **1775** retained.
