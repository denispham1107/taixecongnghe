# Phố Việt 1.17.0 — Directional rider impacts and three lives

## Current rules (supersede earlier immediate-loss releases)

- Every run starts with three lives. Each new traffic crash spends one life and enters the physical falling phase. Contacts within that same falling/get-up sequence can deflect the body again but cannot spend several lives on the same pile-up.
- The first two crashes permit recovery, including during CSGT pursuit. Only the third crash ends the run, after the slide/fall has settled. CSGT capture still ends control immediately, regardless of remaining lives, and transitions to jail. CSGT can also catch the player while the player is down or getting up.
- Once the bike settles, it lies for about 1.2 seconds, then uses the existing skeletal Recover animation while the outer bike rises over 0.65 seconds. Recovery retains the actual settled position; normal steering smoothly merges into the nearest lane. It never teleports back to the original collision point.
- Traffic actively steers/brakes/yields during get-up and for three riding seconds afterward. The rider can also stop for a blocked oncoming vehicle, using the same confirmed physical-yield behavior as job acceptance. This is not collision immunity: another actual contact after returning to Riding can spend the next life.
- Accepted work, the original pickup/drop-off, onboard passenger, Coin wallet and pursuit remain intact. A crash never pays unfinished work. Restart restores all three lives; earned Coin retains the existing persistence rules.
- HUD displays `MẠNG: n / 3`. Crash/get-up messages use the existing single gold-on-teal notice. Ready, help and final-crash text describe the new rules.

The user explicitly confirmed the pursuit behavior, automatic get-up after about 1.2 seconds, three seconds of physical traffic yielding, nearest-lane recovery at the settled position and continuation of the current job before implementation.

## Implementation

- `PhoVietCollisions.cs`: selects the earliest swept player/vehicle OBB contact; restores both bodies to contact time before applying impulses. The old fixed `Crash()` animation path no longer handles real vehicle contacts.
- `PhoVietRiderCrash.cs`: manages lives, impulse response, slide, secondary impacts, get-up, road rebasing and crash-time police pursuit. Impact response uses relative road velocity, vehicle mass, contact side/lever arm, low restitution and friction. Side impacts produce opposite rolls; front/rear impacts produce backward/forward launch and pitch; off-centre contacts produce yaw torque.
- This is a custom game simulation compatible with the existing traffic bodies, rather than a Unity Rigidbody conversion. Gravity is 9.81 m/s²; ground drag is 9 m/s²; horizontal launch speed is capped at 28 m/s to keep severe impacts bounded. Airborne motion, ground friction and shop-wall reaction determine the actual travel distance, rather than a fixed animation offset.
- `PhoVietGame.cs`: shares traffic motion between riding and falling, adds Recovering, pause/resume for crash phases, camera tracking of actual crash position, lives HUD and new end conditions. A normal ride still uses the existing lane and junction controls.
- `PhoVietRiderCrash.cs` preserves an interrupted turn by rebuilding the remaining cubic curve from the settled location to the correct exit lane. Longitudinal rebasing translates the scenery, traffic, camera and police together. A head-on slide across the previous junction also moves the GPS grid node back, keeping the visible next junction and navigation consistent.
- `PhoVietActions.cs`: arrest accepts Riding/Falling/Recovering; pursuit remains physical and can catch a downed rider. Police contact with traffic still uses the existing collision/1.2-second fall logic.
- `PhoVietJobTraffic.cs`: shares traffic avoidance with recovery, even when jobs are disabled. The recovery timer pauses with the simulation.
- `BikeVisualAnimator.cs`: selects the player's left/right fall from impact roll and samples Recover while standing up. Existing Blender rigs, Bone2 skinning, models and art files are preserved.

## Verification

- **1,450 assertions passed across all 19 Unity suites**, freshly executed for this release, including **65 new rider-life checks**. Existing contact tests now check loss of one life rather than the obsolete immediate game-over rule.
- New coverage: four impact directions, stronger/faster and heavier collisions, opposite roll directions, physical secondary impacts without double-spending lives, all three crash/recovery cycles, final game over, pause during fall/get-up, nearest-lane selection without teleporting, three seconds of yielding with jobs disabled, pickup and onboard-passenger preservation, both junction-turn directions, GPS after backward sliding, pursuit continuation and police capture with lives remaining.
- Gameplay, cross traffic, pursuit, reckless drivers, traffic dynamics, rear/random/police behavior, rider contacts, motorcycle/car art, mobile camera, safe area, junction routes/refinements, road drivers, street art, jobs, job edges and the 1.16 mobile refinements all passed. Affected route/job checks were rerun after the backward-junction refinement.
- Evidence: `rider-lives-Verify*-results.json`. Native screenshots show left/right impacts, get-up and riding afterward; `rider-lives-visual-results.json` confirms all four captures completed. Screenshots were visually inspected.
- Police capture distance now checks both ahead and behind: an officer ten metres ahead after a backward fling cannot falsely capture the rider. Rider-life, pursuit and police-traffic suites were rerun after that guard. End-of-run and jail distance text clamp at zero even if the physical position slides behind the starting point.
- `git diff --check` passed. Existing local edits to four Blender source files, the Blender backup and untracked `huongdan.md` remain excluded from this release.

## Web build and publication

- Final WebGL build `build_3b665eb68107` succeeded in 149.134 seconds, with zero errors and one expected warning disabling the unused Pipeline runtime plugin in the player.
- Version `1.17.0`, offline cache `ed34b794b58f4886`.
- **65 browser checks passed on the final artifact**: 37 WebGL/PWA/offline/input checks, eight real canvas job-selection/GPS checks and 20 fullscreen checks across phone/tablet portrait/landscape. There were zero JavaScript or Unity runtime errors. Evidence: `rider-lives-web-results.json`, `rider-lives-ui-results.json`, `fullscreen-game-rider-lives-results.json`.
- The final phone GPS screenshot was visually inspected: the three-life counter, Coin, GPS and unified notification fit without overlap; the mobile hamburger remains hidden.
- Publication and live verification details follow below.

Phone browser profiles verify layout and touch in Chromium; no physical iPhone/iPad performance claim is made.
