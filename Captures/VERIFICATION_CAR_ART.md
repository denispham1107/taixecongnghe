# Phố Việt four-wheel Blender art — 1.9.0

## Delivered assets and integration

All eleven four-wheel traffic roots now use the six original Blender designs: sedan,
hatchback, taxi, delivery van, medium box truck and drowsy truck. Existing motorcycles,
riders, city geometry, traffic pool, collision dimensions and random spawning are retained.

`ArtSource/Blender/PhoViet_Cars.blend` contains six models, fifteen-bone rigs, seven editable
actions per rig, packed palettes/font and a lit studio with cameras. The reproducible
`pho_viet_cars.py` authoring script uses shared mesh/atlas/FBX helpers from the bike script.
FBX, metadata, textures, materials and import settings are in `Assets/PhoViet/Art/Cars`.

Each model has three skinned LODs, one atlas material and rigid single-bone weights.
Near meshes range from 17,340 to 23,503 triangles; far meshes have fewer than 4,000.
Driving animates suspension/cargo and rotates all four wheels using actual speed. Front
wheels steer with heading changes. Braking pitches the body. Four impact clips respond
to the existing physics impulse, with body rebound, bumper deformation and mirror folding.
Only the visual child is animated; the gameplay root, mass/impulse physics, loss rules,
police navigation, reckless behavior and building-fire effects remain owned by gameplay.

The existing local edit to `PhoViet_Motorcycles.blend`, present before this task, is preserved
and excluded from this release commit.

## Unity verification

**766 assertions passed**:

| Suite | Assertions |
| --- | ---: |
| VerifyCarArt | 204 |
| VerifyBikeArt | 156 |
| VerifyRiderContacts | 45 |
| VerifyTrafficDynamics | 82 |
| VerifyRearRandomPolice | 60 |
| VerifyReckless | 120 |
| VerifyCrossTraffic | 28 |
| VerifyPursuit | 39 |
| VerifyGameplay | 23 |
| VerifySafeArea | 9 |

Evidence: `car-art-Verify*-results.json`. Art tests verify all eleven rigs/LODs/clips,
wheel positions and actual animation, stopped/oncoming wheels, steering, brake pitch,
pause, all impact directions, bumper deformation, pool reuse, actual front/rear collision
hooks and truck building impacts with fire/smoke. Rider-contact tests retain immediate
loss on every traffic contact during pursuit.

Blender renders and actual Unity GPU frames were visually inspected, including window
visibility on both sides. `CaptureCarEvidence` waits real editor frames after posing:
`blender-cars-traffic.png`, `blender-cars-collision.png`, `blender-cars-truck-fire.png`.
Studio views: `blender-cars-lineup.png`, `blender-truck-detail.png`.
Frame evidence: `car-art-frame-evidence.json`.

## WebGL verification

- Build `build_b8a935b1d500`: **Succeeded**, 230.055 seconds, zero errors.
- One expected warning: unused Pipeline plugin disabled in player builds.
- Version **1.9.0**, offline cache **72019f9e4505ca69**.
- **36 browser assertions passed**, including real WebAssembly startup, keyboard/touch,
  pause, rotation, installability, complete offline caching and startup without network.
- No JavaScript or Unity runtime errors. Evidence: `car-art-web-results.json`.
- Browser phone/iOS profiles simulate layout and touch; physical iOS/Android hardware
  was not used for these checks.

## Publication

The source and tested WebGL artifact are ready for the authorized GitHub Pages release.
Deployment and live verification will be recorded after publication.
