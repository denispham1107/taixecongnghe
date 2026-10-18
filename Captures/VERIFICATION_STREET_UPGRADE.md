# Phố Việt traffic and Blender street upgrade — 1.12.0

## User selections

The user selected the playful semi-realistic visual direction, with Vietnamese
shop houses, doors, balconies, awnings and storefront props, while keeping mobile
WebGL in mind. They explicitly selected ordinary oncoming traffic being allowed
to cross the yellow centre line into the player's side.

## Traffic

- Ordinary longitudinal motorcycles and cars now randomly choose left or right
  turns at real junctions. Turns follow continuous quarter-circle paths with
  reduced speed and motorcycle lean, then continue along the side road.
- Existing crossing traffic retains its left/right turns into the main road.
  Junction waves now request two, three or five crossing actors as difficulty
  rises, subject to available inactive actors in the existing pool.
- Ordinary oncoming motorcycles, cars and trucks can change adjacent lanes across
  all four lanes, including the player's side, and return. They continue facing
  and travelling in the oncoming direction; they do not become same-direction cars.
- A player route rebase also transforms an active NPC turn's origin and basis,
  allowing the curve to continue through the player's turn.
- Physical traffic contacts, player loss during police pursuit, defensive kicks,
  reckless actors, police avoidance and 1.2-second recovery remain active.

## Live Blender MCP authoring and Unity integration

The live Blender MCP executed the original authoring source and exported all
sixteen assets: twelve named Vietnamese shop houses, three street trees and a
pavement module. Editable source: ArtSource/Blender/PhoViet_Streets.blend and
pho_viet_streets.py. Exports and catalogue: Assets/PhoViet/Art/Streets.

- Houses include facade columns, window frames/mullions/shutters, balcony rails,
  potted plants, awnings with brackets, air conditioners, drain pipes, rooftop
  parapets/water tanks/solar panels and five-point Vietnamese flags.
- Grocery shelves and goods, repair tires/tools, food counters, pots, bowls,
  metal tables and stools distinguish the storefronts.
- Trees have branching trunks, roots, layered foliage and raised planters.
  Pavement has individual tiles, curb stones and drainage grilles.
- Boards and Vietnamese lettering are integral to the facade geometry, parallel
  and flush with the front wall. Lettering is retained without simplification at
  all three LODs and positioned below the balcony slab so accents stay visible.
- One shared 128px atlas material, three LOD meshes per model and shared FBX mesh
  instances are used throughout the thirty-five blocks in five route rows.
  1,260 scenery LOD groups cover every house, tree and pavement segment.
- Original detailed bikes/riders, four-wheel traffic, parked bikes, pedestrians,
  road paint, arrows, signals, street-name labels and connected routes remain.
- FBX metre units and imported axis transforms were verified visually in Unity.
  Both the game-camera view and close facade view were inspected:
  street-upgrade-game.png and street-upgrade-facade.png.
- Pre-existing edits to PhoViet_Cars.blend and PhoViet_Motorcycles.blend were
  preserved and excluded from this release commit.

## Unity verification

**1,194 assertions passed**. Evidence: street-upgrade-Verify*-results.json.

| Suite | Assertions |
| --- | ---: |
| VerifyStreetArt | 186 |
| VerifyRoadDrivers | 45 |
| VerifyTrafficDynamics | 82 |
| VerifyJunctionRoutes | 92 |
| VerifyJunctionRefinements | 105 |
| VerifyBikeArt | 156 |
| VerifyCarArt | 204 |
| VerifyRiderContacts | 45 |
| VerifyRearRandomPolice | 60 |
| VerifyReckless | 120 |
| VerifyCrossTraffic | 28 |
| VerifyPursuit | 39 |
| VerifyGameplay | 23 |
| VerifySafeArea | 9 |

New coverage includes bike/car turns from both travel directions into both side
roads, finite continuous curves, correct exit headings, oncoming lane invasion
and return for bikes/cars/trucks, retained oncoming heading, reset state, all
thirty-five city blocks, metre-scale imported geometry, facade alignment and all
three LODs using the shared material.

## WebGL and publication

- Native build `build_e3fc8ed6a659` succeeded in **194.558 seconds**, **zero errors**,
  one expected Pipeline plugin warning for player builds.
- Version **1.12.0**, offline cache **0ba5ec04f78d004b**.
- The editor scene builder also configures the Blender street factories on a
  future rebuild; its editor-only change compiled successfully.

- **36 local browser checks passed**: real Unity WebAssembly startup, keyboard,
  touch/double-tap smoke, rotated full-viewport canvas, PWA installability, complete
  offline cache and startup with the network disabled. No JavaScript/Unity runtime
  errors. Evidence: street-upgrade-web-results.json. Desktop and mobile browser
  screenshots were visually inspected.
- Mobile/iOS browser profiles emulate viewport and touch. Physical mobile devices
  and hardware frame rates were not tested.
- Unity left Ready, time scale 1, version 1.12.0, eleven four-wheel actors and
  thirty-three bike/rider actors. Best distance 1775 m retained.

- Source and tested artifact commit: `1a8cd826f0b2a9e61ca72fae7460943084665420`.
- GitHub Pages deployment succeeded for that exact commit:
  https://github.com/denispham1107/taixecongnghe/actions/runs/39117818367
- Live game: https://denispham1107.github.io/taixecongnghe/
- **22 live checks passed**: deployed version/cache and every published resource
  match the tested artifact. The real published Unity WebAssembly initializes,
  accepts start/pause input and reports no JavaScript/Unity runtime errors.
  Evidence: street-upgrade-live-results.json.
