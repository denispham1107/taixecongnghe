# BÀN GIAO NHANH — Phố Việt

> File này là **điểm bắt đầu cho mọi phiên Codex/Claude Code**. Đọc file này + `git status` + `git log --oneline -5` là đủ để nhận yêu cầu mới. **Không đọc lại toàn bộ `huongdan.md` (≈385 KB) hay toàn bộ mã nguồn**; chỉ mở mục/file liên quan theo bảng tra cứu bên dưới.
>
> Ai vừa sửa/thêm chức năng, build hoặc phát hành **phải cập nhật file này trong cùng công việc** (mục 1–3 và 6), song song với ghi chi tiết vào `huongdan.md`.

Cập nhật lần cuối: **27/10/2026 — Codex**

## 1. Trạng thái hiện tại

| Mục | Giá trị |
| --- | --- |
| Bản đã phát hành | **1.42.0** — commit `be0c6ff`; Pages run `40445290840` success; cache `d7e9bf2a636e37f7`; **26 kiểm tra live đạt/0 lỗi runtime** |
| Bản chơi | <https://denispham1107.github.io/taixecongnghe/> (thư mục `docs/` trên `main`) |
| Repo | <https://github.com/denispham1107/taixecongnghe>, nhánh `main` |
| Unity | 6000.5.6f1, Built-in RP, Legacy Input, IMGUI (`OnGUI`); scene `Assets/PhoViet/Scenes/PhoViet.unity` |
| Simulator iPhone | **21.71:** đã thêm **Apple iPhone 14 Pro / 14 Pro Max**, Dynamic Island + Safe Area dọc/ngang; đang chọn Pro Max ngang, Safe Area bật. Asset chỉ trong Editor, chưa commit/push |
| Chế độ chơi | Chỉ thi quãng đường/kỷ lục. **Công việc/khách/GPS/Coin HUD đang khóa** (`RideJobsReleased=false`, scene `jobsEnabled=false`) |
| Tài liệu bàn giao | `BANGIAO.md`/`AGENTS.md`/`CLAUDE.md` đã commit riêng lên `main` (21.56) |
| Người chơi trong source/scene | **Blue Petshop Supercar** xanh, **đã thu nhỏ15%** thay người áo đỏ+xe máy; Q/chạm hai lần áp sát+húc ngang, Space/vuốt lên nhảy4jet trắng, W/vuốt xuống boost2bô (21.64–21.65). **Đã có trên web1.40.0** |
| Điều khiển/nhảy trong source | **21.65:** scale0,85 (1,761×0,915×3,74m); đổi làn theo quãng đường22,95m/làn, nhanh tỷ lệ Speed/W (17m/s~1,35s;30~0,765s;67,5~0,34s), gồm W giữa chừng; nhảy theo tốc độ cất cánh21.60 giữ nguyên |
| NPC xe máy trong source21.67 | **Urban Rider mới:31actor(17traffic+14parked)+56baked nhánh**, rig19xương/3LOD42k/18,5k/6,7k, cổ+tay+thân đồngbộ; **đã có trên web1.41.0**. PlayerSupercar/CSGT giữ nguyên |
| Thay đổi cục bộ ngoài phát hành | **21.70 âmđộngcơ+70% và21.72 bảnghiệuPetshop30% source xong/chưa build**; Art/Wave/.blend và Captures lịch sử khác giữ cục bộ, không reset/stage toàn bộ; các chức năng21.62–21.65 đã phát hành trong1.40.0 |

## 2. Việc vừa làm gần nhất (mới nhất ở trên)

- **21.72 source/scene xong, chưa web:** Cù Lao Petshop nềnxanh/chữtrắng **30%**:147/490shop phố,693/2310gian river; river cóbiển **cả mặtđường/mặtsông, đủ3LOD** (1386/4620biển).12variantgiữfacade+riverbase/1partbổsung; cùngmaterial, không thêmrenderer (10983), collider/LOD/độnétgiữ nguyên. **682asset+1326performance+160river=2168điềukiện đạt**,9ảnh đãxem, installer repeatbyte-identical. UnityEditMode/scenesạch/jobsfalse; Blenderbackground riêng đãthoát. Cần buildC#/data cùng21.70 khi phát hành; web vẫn1.42.0. Chi tiết huongdan21.72.

- **21.71 Simulator đã dùng được:** hai cấu hình iPhone 14 Pro1179×2556 / Pro Max1290×2796, overlay Dynamic Island, Safe Area đủ4hướng tại `Assets/PhoViet/Editor/DeviceSimulator/`. Unity đã import/menu có cả hai; xem cả hai dọc/ngang+khungSafeArea đạt; kiểm JSON/kích thước/alpha2mẫu/8hướng đạt (`Captures/iphone14-simulator-checks.json`). Giữ package1.0.1, không đổi gameplay/scene/web; chưa commit/push. Chi tiết huongdan21.71.

- **21.70 source xong, chưa build web:** tiếng động cơ player cả cruise/boost tăng70%, AudioSource0,08→**0,136** (chungnguồn, không nhân boost lần2). ChỉPhoVietGame.CreateSound; âm khác/pitch/gameplay giữ nguyên. Compile0lỗi/0warning; PlayMode đọcnormal/boost đều0,136 đạt; đãthoátPlay. Web vẫn1.42.0/âmcũ, cần build lạiC# khi phát hành. Chi tiết huongdan21.70.

- **1.42.0 đã phát hành (21.69):** cache bố cục sông, broadphase va chạm theo tile, gom ánh sáng sau physics;700group shop/tree dùng LOD xa sớm hơn, giữ DPR/texture/AA4/bóng/phảnchiếu. **3.810Unity+16preflight+34repo+29site+31staged+71browser+DPR3+26live đạt**,0runtime. CPUriver0,856→0,037ms, controlledrender61,13→52,80ms (không phải FPS gameplay); ảnh gần45% dướiMAE0. Commitbe0c6ff/Pages40445290840/cached7e9bf2a636e37f7. UnityEditMode/scenesạch/jobsfalse; server đã dừng. Chi tiết huongdan21.69.

- **1.41.0 đã phát hành (21.68):** commit648b163/Pages40420198461 thành công/cachec9db437e230d3824; build0lỗi/3warning đã biết;14preflight+34repo+29site+37web/offline/touch+20fullscreen+31Gitbytechecks+26live đạt,0lỗi runtime. Urban Rider31actor+56baked/cổ xe/tay lái đã có trên web. Unity Edit Mode/scene sạch/jobs khóa, server kiểmtra đã dừng.

- **21.67 đã có trên web1.41.0:** Urban Rider thay31xe máy có người ngoàiCSGT+56baked nhánh; cổ/ghiđông/bánhtrước/tay/IK/thân đồngbộ, đổi lànquintic theo tốcđộNPC, nghiêng/về cân bằng giảmchấn. 7suite/**1.493điều kiện đạt**, ảnh4màu/grip/cua/đêm đãxem; map2K cópadding, đèn không sánglan yếm. Jobs khóa; giữSupercar/CSGT. File/parameter/checks chi tiết huongdan21.67.

- **1.40.0 đã phát hành (21.66):** commit2c118c3/Pages40396554304/cache10d5883fe6b0325f;0lỗi build/3warning. 34repo bundle+29site+7parts,37web/offline/touch+20fullscreen+5offline cuối+27staged bytes+26live đều đạt,0lỗi runtime. Data chia4phần32MiB/ghép SHA trênPages; source/cảnh/gameplay giữ đúng21.62–21.65. Unity Edit Mode/scene sạch, jobs khóa.

- **21.65, đã có trong1.40.0:** Supercar nhỏ15%, đồng bộ collider/axle/đèn/jet; đổi làn nhanh tỷ lệ Speed, W giữa cú cũng tăng nhịp. **5suite/634 điều kiện đạt**, đã lưu/reload scene giữ đúng scale0,85; ảnh dọc đã xem. Unity Edit Mode/scene sạch; chi tiết huongdan21.65.

- **21.64, đã có trong1.40.0:** Blue Petshop Supercar player xanh, 3LOD28k/12,3k/4,5k; yaw collider đúng hướng (nhịp lane đã thay ở21.65); nhảy4jet trắng, W2ống bô. Q=húc như nhánh ô tô đã chốt. **8suite/1.602 điều kiện đạt**, ảnh dọc/ngang/FX/đêm đã xem; Unity Edit Mode/scene sạch; NPC/CSGT/jobs giữ nguyên. Chi tiết huongdan21.64.

- **21.63, đã có trong1.40.0:** mọi xe va chạm văng sông hủy recovery/course, rơi/chìm cả khi đang bay hoặc đã GameOver; tra mọi nhánh/đổi khung sau cua. CSGT chìm thì hết truy đuổi; người chơi đã té thì hết lượt. **8 suite/1.166 điều kiện đạt**, ảnh xác nhận chìm. Unity Edit Mode/scene sạch, jobs khóa; chi tiết huongdan21.63.

- **21.62, đã có trong1.40.0:** Emerald Tyrant rộng4,65m/cao6,081m, rig/Walk/TailSweep bằng BlenderMCP, lịch10s/ngược chiều3m/s; đuôi thật quét4làn, nhảy né, hất cả xe tăng/CSGT. Sửa CSGT tìm đường tới người chơi bị hất sang đường ngang. **69 kiểm tra +96 cuộc gặp +681 hồi quy =846 đạt**; chi tiết huongdan21.62.

- **1.39.0 đã phát hành (21.61):** commit17aa0b2, Pages40268861830 thành công; build0lỗi/3warning,29bundle +37web/offline/touch +20fullscreen +26live đạt,0lỗi runtime. Unity Edit Mode/scene sạch, jobs khóa.

1. **21.60, đã có trên web1.39.0:** đổi làn0,72 s và phục hồi nghiêng chậm; nhảy thấp25% ở tốc độ đầu, tăng đều theo tốc độ lúc cất cánh/W; W giữa không trung giữ cú đang bay. **6 suite/541 điều kiện Unity đạt**, xem trace/ảnh; chi tiết huongdan21.60.
2. **21.58, đã có trên web1.39.0:** khôi phục đúng hero áo đỏ sao vàng/ngón nắm v4 và xe máy Meshy, sơn xanh dương–đen; Q trở lại đạp, nhảy/boost/đèn/khói cũ. NPC28/CSGT giữ nguyên. **4 suite / 229 điều kiện Unity đạt**, xem ảnh trước/sau; người/sao và phần đen trong atlas không đổi. Thay lựa chọn player Petshop21.57.
3. **Đã có trên web1.39.0:** đổi làn mượt như cua (21.55); khu sông/cầu từ ~10–12 ms còn ~0,6 ms/khung trong Editor (21.54).
4. **1.38.0:** mặt sông gương, cầu dài 50,4 m giữa hai giao lộ, căn hộ kính 6–9 tầng ven sông sáng đèn đêm, sửa nhà lơ lửng trên sông, sửa test CSGT chập chờn (21.45–21.53).
5. **1.37.0:** cầu vượt sông 2 làn giữa, rơi sông = kết thúc; xe tăng mỗi 9 giây chạy ngược chiều bắn đạn vào 1/4 làn phía trước (1,0 s → tối thiểu 0,4 s theo tốc độ), bán kính nổ 2,2 m; ánh sáng sương sớm D, mũi tên báo xe sau D (21.35–21.44).
6. **1.36.0:** xe tăng Meshy “Iron Sentinel” (21.33–21.34). **1.35.0:** đổi làn kiểu đánh lái, nhà kính, zoom dọc +10% (21.28–21.32). **1.34.0:** phố Sài Gòn (21.19–21.27).

## 3. Đang dở / cần người dùng quyết








> Mẫu: `Yêu cầu: … · Đã xong: … · Bước kế tiếp: … · File đang sửa: … · Unity/Blender: … · Cập nhật: <ngày giờ>, <agent>`

- **21.72 chờ build/phát hành:** chỉ dùng `pho_viet_petshop_signs.py`/`InstallPetshopSigns.Main` cho bảnghiệu; không chạy installerphố/cầucũ (sẽ mất layer sau hoặc tỷ lệ). Test `VerifyStreetArt` cũ giảđịnh16asset không đại diện28assetphố hiện tại. Simulator21.71/Packages vẫn là sửađổi khác chưacommit, đừng stage mù.
- **21.70 chờ build/phát hành web:** source âmđộngcơ+70% đã kiểm tra; không dùng RefreshWebShell vì C# đã đổi. Web vẫn1.42.0.
- **21.69 đã phát hành1.42.0:** không còn phần tối ưu chờ build; không tự hạ DPR/texture/bóng để tăng FPS. Phép đo là Editor/camera readback và browser giả lập; chưa đo Safari/điện thoại thật. Các probe shadow/instancing chỉ thử tạm, không áp dụng.
- **21.67 đã phát hành1.41.0:** asset/meta+scene/code/2tài liệu nằm trong commit648b163. Khi bảo trì dùng đúng bộ Urban Rider hiện hành. RawMeshyFBX120,7MB giữ cục bộ, không stage/push mù quáng; dependencies không còn raw FBX/FBX dân sự cũ, không thiếu script; game dùng FBX4MB. Không chạy lạiMeshyRiders/Wave installer cũ.
- Không còn chức năng21.62–21.65 chờ phát hành: **đã có trên web1.40.0**. Dữ liệu119,6MB lưu trongGit dạng4parts; Pages ghép/kiểmSHA trước upload. Trên checkout mới, materialize ra_site trước khi chạy server local (15.2–15.4/21.66).
- Bán tải/installer Petshop21.57 đã ngưng dùng từ21.58; không tự cài lại. **Nhánh húc được tái dùng cho Blue Supercar21.64–21.65**. Test/giới hạn cua cực muộn của pickup cũ không đại diện player hiện tại. Chưa thử Safari trên điện thoại thật.
- Các file Wave cũ chưa theo dõi (`UpgradeBikesWave.cs`, `PhoViet_Motorcycles_Wave.blend`…) là lịch sử; **không xóa/commit/dùng** nếu chưa được yêu cầu.

## 4. Luật đã chốt — không tự đổi

- Hỏi lại bằng lựa chọn cụ thể nếu chưa chắc ≥90% ý người dùng.
- Không ba mạng. Té thường → hết lượt. Té khi đang bị CSGT đuổi → nằm chờ CSGT tới bắt thật → tù (không GameOver ngay, không tự dựng xe). Rơi sông/trúng đạn xe tăng → hết lượt, kể cả khi bị đuổi. **CSGT rơi sông → hết truy đuổi; người chơi đã té thì hết lượt**, còn đang chạy thì tiếp tục (21.63).
- Húc/đạp quái xế tự vệ không gọi mới CSGT; húc/đạp xe thường thì bị truy đuổi.
- Phím: **Q húc, Space nhảy, W tăng tốc +125%/4s/hồi2s**; mobile: chạm hai lần húc, vuốt lên nhảy, vuốt xuống tăng tốc; nhận ngay khi đủ ngưỡng.
- Hai làn trái rẽ trái, hai làn phải rẽ phải, giữ số làn sau cua, cua không giảm tốc.
- **Source21.65: player Blue Petshop Supercar** xanh scale0,85, Q=húc thân xe; đã có trên web1.40.0; hero xe máy xanh–đen21.58 là lịch sử. NPC xe máy source21.67 dùngUrbanRider nhiều màu xe/áo, CSGT giữ cả người+xe; NPC4mẫu ô tô×8màu, xe tăng giữ nguyên.
- Phong cách bán hiện thực vui nhộn, tối ưu điện thoại/WebGL.
- Không bật lại công việc khi nâng cấp/sửa lỗi/build. Giữ dữ liệu Coin và mã jobs.
- Không ghi đè/reset file Blender cục bộ, không dựng lại cả scene bằng builder lịch sử.

## 5. Tra cứu nhanh: sửa gì → đọc gì

| Muốn đụng tới | Mục `huongdan.md` | File chính |
| --- | --- | --- |
| Điều khiển, đổi làn, camera, HUD | 2.2, 12, 21.55/21.60 | `PhoVietGame.cs`, `PhoVietGestures.cs`, `PhoViet*Hud.cs` |
| Giao thông, va chạm, văng/té | 7, 21.33 | `PhoVietTraffic.cs`, `PhoVietCollisions.cs`, `PhoVietRiderCrash.cs` |
| Quái xế, xe tải ngủ gật | 2.4, 7.2 | `PhoVietReckless.cs` |
| Khủng long, rig/quất đuôi, nhảy né | 2.12, 7.5, 21.62 | `PhoVietDinosaurs.cs`, `DinosaurVisual.cs`, `Editor/PhoVietDinosaurUpgrade.cs`, `pho_viet_dinosaur.py` |
| Xe tăng, đạn pháo | 21.38–21.40 | `PhoVietTankFire.cs`, `Editor/PhoVietTankFxUpgrade.cs` |
| Đạp, CSGT, bị bắt | 8 | `PhoVietActions.cs`, `PhoVietKickAssist.cs`, `PhoVietPolice.cs`, `PhoVietCrashPoliceNavigation.cs` |
| Nhảy, nóc xe, tăng tốc | 19, 21.60 | `PhoVietJump.cs`, `PhoVietBoost.cs` |
| Giao lộ, rẽ, rebase tọa độ | 6.1, 9 | `PhoVietRoutes.cs`, `PhoVietRoadDrivers.cs`, `PhoVietExitTraffic.cs` |
| Sông, cầu, căn hộ ven sông, xe/CSGT rơi chìm | 21.35–21.37, 21.45–21.54, 21.63 | `PhoVietRiver.cs`, `RiverBlock.cs`, `RiverReflection.cs`, `River*.shader`, `Editor/PhoVietRiverUpgrade.cs`, `AgentScripts/VerifyRiverCrashSinking.cs` |
| Cảnh phố, nhà, cây | 11.4, 21.19–21.28, 21.43 | `VietnamStreet.cs`, `StreetArt.cs`, `Editor/PhoVietStreetUpgrade.cs` |
| Ngày–đêm, đèn, sương | 20, 21.41 | `PhoVietDayNight.cs`, `DayNightLighting.cs`, `MorningFog.cs`, `MorningLooks.cs` |
| Xe đua người chơi/khói4góc+2bô/chuyển làn | 21.64–21.65, 19, 21.60 | `PlayerCarVisual.cs`, `PhoVietSupercarUpgrade.cs`, `CarJumpJetVisual.cs`, `BoostSmokeVisual.cs`, `integrate_supercar_player.py`, `VerifySupercarScalePace.cs` |
| Khôi phục player xe máy xanh–đen (lịch sử) | 21.58, 21.9–21.16 | `RestoreBlueBlackPlayer.cs`, `prepare_hero_blue_black.py`, `BikeVisualAnimator.cs` |
| Xe máy dân sự Urban Rider/cổ xe/tay nắm/đổi làn | **21.67**,7.1,11 | `UrbanRiderMotion.cs`, `BikeVisualAnimator.cs`, `UrbanRider.shader`, `PhoVietTraffic.cs`, `PhoVietRoutes.cs`, `Editor/PhoVietUrbanRiderUpgrade.cs`, `integrate_urban_rider.py`, `prepare_urban_rider_maps.py`, `AgentScripts/InstallUrbanRider.cs`, `VerifyUrbanRider.cs` |
| Model Blender/Meshy → Unity | 11, 13, 21.7, 21.17 | `Editor/PhoVietMeshy*Upgrade.cs`, `ArtSource/Blender/` |
| Công việc/Coin (đang khóa) | 10, 21.1 | `PhoVietJobs.cs`, `PhoVietJobGui.cs` |
| Bảng hiệuPetshop30%, tênnhà2mặt ven sông | **21.72**,11.4 | `pho_viet_petshop_signs.py`, `pho_viet_river_bridge.py`, `Editor/PhoVietPetshopSignsUpgrade.cs`, `AgentScripts/InstallPetshopSigns.cs`, `VerifyPetshopSigns.cs` |
| Âm động cơ player/cruise/boost | **21.70** | `PhoVietGame.cs` (`CreateSound`, `engine`, cuối `Update`) |
| Simulator iPhone 14 Pro/Pro Max/Dynamic Island/Safe Area | **21.71** | `Assets/PhoViet/Editor/DeviceSimulator/` (`README.md`, hai `.device`, `Overlays/`, `LICENSE.md`); `Captures/iphone14-simulator-checks.json` |
| Hiệu năng/lag/cache sông/va chạm/đèn/LODxa | **21.69** | `PhoVietObstacleBatches.cs`, `PhoVietRiver.cs`, `DayNightLighting.cs`, `Editor/PhoVietPerformanceUpgrade.cs`, `AgentScripts/ProfileRuntimeCost.cs`, `ProfileStreetDetail.cs`, `VerifyPerformanceSafety.cs`, `VerifyPerformanceLighting.cs` |
| Kiểm tra theo loại thay đổi | 14, 18.2 | `AgentScripts/Verify*.cs`, `*.cjs` |
| Build WebGL, phát hành Pages/data parts | 15.2–15.4, 21.66 | `Editor/PhoVietWebBuild.cs`, `AgentScripts/ConfigureWebRelease.cs`, `PublishWebArtifact.py`, `.github/workflows/pages.yml` |

Bản đồ đầy đủ mọi file: `huongdan.md` mục 4. Các file `PhoViet*.cs` là phần của cùng `partial class PhoVietGame`.

## 6. Quy trình mỗi yêu cầu

1. Đọc file này, `git status --short`, `git log --oneline -5`. Nếu `git log` có commit mới hơn bản ghi ở mục 1 → đọc thêm mục cuối cùng (21.x mới nhất) của `huongdan.md` và `git show --stat` các commit đó.
2. Chỉ đọc các mục/file liên quan theo bảng 5. Hỏi lại nếu chưa chắc ≥90%.
3. Làm, chạy kiểm tra phù hợp (18.2), xem ảnh nếu đổi hình. Việc dài: ghi “Tiến độ việc đang làm” ở mục 3 sau mỗi mốc, để phiên khác (tài khoản Claude khác hoặc Codex) làm tiếp nếu phiên này bị dừng giữa chừng.
4. Thêm mục `21.x` mới vào `huongdan.md` (hành vi, file, tham số, kiểm tra, trạng thái) và **cập nhật mục 1–3 của file này**.
5. Khi được yêu cầu build/phát hành: theo 15.2; xong thì ghi version/commit/run/cache vào mục 1 file này và `huongdan.md`.
