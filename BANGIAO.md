# BÀN GIAO NHANH — Phố Việt

> File này là **điểm bắt đầu cho mọi phiên Codex/Claude Code**. Đọc file này + `git status` + `git log --oneline -5` là đủ để nhận yêu cầu mới. **Không đọc lại toàn bộ `huongdan.md` (≈385 KB) hay toàn bộ mã nguồn**; chỉ mở mục/file liên quan theo bảng tra cứu bên dưới.
>
> Ai vừa sửa/thêm chức năng, build hoặc phát hành **phải cập nhật file này trong cùng công việc** (mục 1–3 và 6), song song với ghi chi tiết vào `huongdan.md`.

Cập nhật lần cuối: **27/10/2026 — Codex**

## 1. Trạng thái hiện tại

| Mục | Giá trị |
| --- | --- |
| Bản đã phát hành | **1.39.0** — commit `17aa0b2`; Pages run `40268861830` success; cache `770899e40e23f8e2`; **26 kiểm tra live đạt/0lỗi runtime** |
| Bản chơi | <https://denispham1107.github.io/taixecongnghe/> (thư mục `docs/` trên `main`) |
| Repo | <https://github.com/denispham1107/taixecongnghe>, nhánh `main` |
| Unity | 6000.5.6f1, Built-in RP, Legacy Input, IMGUI (`OnGUI`); scene `Assets/PhoViet/Scenes/PhoViet.unity` |
| Chế độ chơi | Chỉ thi quãng đường/kỷ lục. **Công việc/khách/GPS/Coin HUD đang khóa** (`RideJobsReleased=false`, scene `jobsEnabled=false`) |
| Tài liệu bàn giao | `BANGIAO.md`/`AGENTS.md`/`CLAUDE.md` đã commit riêng lên `main` (21.56) |
| Người chơi trong source/scene | **Blue Petshop Supercar** xanh, **đã thu nhỏ15%** thay người áo đỏ+xe máy; Q/chạm hai lần áp sát+húc ngang, Space/vuốt lên nhảy4jet trắng, W/vuốt xuống boost2bô (21.64–21.65). **Web1.39.0 vẫn hero xe máy xanh–đen** |
| Điều khiển/nhảy trong source | **21.65:** scale0,85 (1,761×0,915×3,74m); đổi làn theo quãng đường22,95m/làn, nhanh tỷ lệ Speed/W (17m/s~1,35s;30~0,765s;67,5~0,34s), gồm W giữa chừng; nhảy theo tốc độ cất cánh21.60 giữ nguyên |
| Thay đổi chưa build/chưa commit | **21.65 xe nhỏ15%/đổi làn nhanh theo Speed**; **21.64 Blue Supercar player/4jet/2bô**; **21.63 sửa mọi xe văng sông rơi/chìm và CSGT rơi thì hết đuổi**; **21.62 khủng long Emerald Tyrant** đã có trong source/scene (10s/ngược chiều/đuôi4làn/nhảy né), chưa có trên web; các art/.blend/Captures/Wave khác giữ cục bộ, không reset/stage toàn bộ |

## 2. Việc vừa làm gần nhất (mới nhất ở trên)

- **21.65, chưa phát hành:** Supercar nhỏ15%, đồng bộ collider/axle/đèn/jet; đổi làn nhanh tỷ lệ Speed, W giữa cú cũng tăng nhịp. **5suite/634 điều kiện đạt**, đã lưu/reload scene giữ đúng scale0,85; ảnh dọc đã xem. Unity Edit Mode/scene sạch; chi tiết huongdan21.65.

- **21.64, chưa phát hành:** Blue Petshop Supercar player xanh, 3LOD28k/12,3k/4,5k; chuyển làn1,35–1,60s/yaw collider đúng hướng; nhảy4jet trắng, W2ống bô. Q=húc như nhánh ô tô đã chốt. **8suite/1.602 điều kiện đạt**, ảnh dọc/ngang/FX/đêm đã xem; Unity Edit Mode/scene sạch; NPC/CSGT/jobs giữ nguyên. Chi tiết huongdan21.64.

- **21.63, chưa phát hành:** mọi xe va chạm văng sông hủy recovery/course, rơi/chìm cả khi đang bay hoặc đã GameOver; tra mọi nhánh/đổi khung sau cua. CSGT chìm thì hết truy đuổi; người chơi đã té thì hết lượt. **8 suite/1.166 điều kiện đạt**, ảnh xác nhận chìm. Unity Edit Mode/scene sạch, jobs khóa; chi tiết huongdan21.63.

- **21.62, chưa phát hành:** Emerald Tyrant rộng4,65m/cao6,081m, rig/Walk/TailSweep bằng BlenderMCP, lịch10s/ngược chiều3m/s; đuôi thật quét4làn, nhảy né, hất cả xe tăng/CSGT. Sửa CSGT tìm đường tới người chơi bị hất sang đường ngang. **69 kiểm tra +96 cuộc gặp +681 hồi quy =846 đạt**; chi tiết huongdan21.62.

- **1.39.0 đã phát hành (21.61):** commit17aa0b2, Pages40268861830 thành công; build0lỗi/3warning,29bundle +37web/offline/touch +20fullscreen +26live đạt,0lỗi runtime. Unity Edit Mode/scene sạch, jobs khóa.

1. **21.60, đã có trên web1.39.0:** đổi làn0,72 s và phục hồi nghiêng chậm; nhảy thấp25% ở tốc độ đầu, tăng đều theo tốc độ lúc cất cánh/W; W giữa không trung giữ cú đang bay. **6 suite/541 điều kiện Unity đạt**, xem trace/ảnh; chi tiết huongdan21.60.
2. **21.58, đã có trên web1.39.0:** khôi phục đúng hero áo đỏ sao vàng/ngón nắm v4 và xe máy Meshy, sơn xanh dương–đen; Q trở lại đạp, nhảy/boost/đèn/khói cũ. NPC28/CSGT giữ nguyên. **4 suite / 229 điều kiện Unity đạt**, xem ảnh trước/sau; người/sao và phần đen trong atlas không đổi. Thay lựa chọn player Petshop21.57.
3. **Đã có trên web1.39.0:** đổi làn mượt như cua (21.55); khu sông/cầu từ ~10–12 ms còn ~0,6 ms/khung trong Editor (21.54).
4. **1.38.0:** mặt sông gương, cầu dài 50,4 m giữa hai giao lộ, căn hộ kính 6–9 tầng ven sông sáng đèn đêm, sửa nhà lơ lửng trên sông, sửa test CSGT chập chờn (21.45–21.53).
5. **1.37.0:** cầu vượt sông 2 làn giữa, rơi sông = kết thúc; xe tăng mỗi 9 giây chạy ngược chiều bắn đạn vào 1/4 làn phía trước (1,0 s → tối thiểu 0,4 s theo tốc độ), bán kính nổ 2,2 m; ánh sáng sương sớm D, mũi tên báo xe sau D (21.35–21.44).
6. **1.36.0:** xe tăng Meshy “Iron Sentinel” (21.33–21.34). **1.35.0:** đổi làn kiểu đánh lái, nhà kính, zoom dọc +10% (21.28–21.32). **1.34.0:** phố Sài Gòn (21.19–21.27).

## 3. Đang dở / cần người dùng quyết

> **Tiến độ việc đang làm**: Phát hành1.40.0/21.66 · Build0lỗi/3warning;9scene/34repo+29site/7parts/37web+20fullscreen+5offline/27staged bytechecks đều đạt; cache10d5883fe6b0325f · Đang commit/push source+parts đúng phạm vi, kế chờPages đúngSHA/live · Unity Edit Mode/scene sạch; Blender không thao tác · Codex27/10/2026



> Mẫu: `Yêu cầu: … · Đã xong: … · Bước kế tiếp: … · File đang sửa: … · Unity/Blender: … · Cập nhật: <ngày giờ>, <agent>`

- **21.62–21.65 chưa build/commit/push/phát hành**; source có khủng long/sửa xe chìm/Blue Supercar player/scale85% và lane theo tốc độ. Web1.39.0 chưa có bốn thay đổi này; chỉ phát hành khi người dùng yêu cầu.
- Bán tải/installer Petshop21.57 đã ngưng dùng từ21.58; không tự cài lại. **Nhánh húc được tái dùng cho Blue Supercar21.64–21.65**. Test/giới hạn cua cực muộn của pickup cũ không đại diện player hiện tại. Chưa thử Safari trên điện thoại thật.
- Các file Wave cũ chưa theo dõi (`UpgradeBikesWave.cs`, `PhoViet_Motorcycles_Wave.blend`…) là lịch sử; **không xóa/commit/dùng** nếu chưa được yêu cầu.

## 4. Luật đã chốt — không tự đổi

- Hỏi lại bằng lựa chọn cụ thể nếu chưa chắc ≥90% ý người dùng.
- Không ba mạng. Té thường → hết lượt. Té khi đang bị CSGT đuổi → nằm chờ CSGT tới bắt thật → tù (không GameOver ngay, không tự dựng xe). Rơi sông/trúng đạn xe tăng → hết lượt, kể cả khi bị đuổi. **CSGT rơi sông → hết truy đuổi; người chơi đã té thì hết lượt**, còn đang chạy thì tiếp tục (21.63).
- Húc/đạp quái xế tự vệ không gọi mới CSGT; húc/đạp xe thường thì bị truy đuổi.
- Phím: **Q húc (web1.39.0 đạp), Space nhảy, W tăng tốc +125%/4 s/hồi 2 s**; mobile: chạm hai lần húc (web cũ đạp), vuốt lên nhảy, vuốt xuống tăng tốc, nhận ngay khi đủ ngưỡng.
- Hai làn trái rẽ trái, hai làn phải rẽ phải, giữ số làn sau cua, cua không giảm tốc.
- **Source21.65: player Blue Petshop Supercar** xanh scale0,85, Q=húc thân xe; hero xe máy xanh–đen21.58 vẫn là bản web1.39.0. NPC xe máy nhiều màu/áo khác, CSGT giữ cả người+xe; NPC4mẫu ô tô×8màu, xe tăng giữ nguyên.
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
| Model Blender/Meshy → Unity | 11, 13, 21.7, 21.17 | `Editor/PhoVietMeshy*Upgrade.cs`, `ArtSource/Blender/` |
| Công việc/Coin (đang khóa) | 10, 21.1 | `PhoVietJobs.cs`, `PhoVietJobGui.cs` |
| Kiểm tra theo loại thay đổi | 14, 18.2 | `AgentScripts/Verify*.cs`, `*.cjs` |
| Build WebGL, phát hành Pages/data parts | 15.2–15.4, 21.66 | `Editor/PhoVietWebBuild.cs`, `AgentScripts/ConfigureWebRelease.cs`, `PublishWebArtifact.py`, `.github/workflows/pages.yml` |

Bản đồ đầy đủ mọi file: `huongdan.md` mục 4. Các file `PhoViet*.cs` là phần của cùng `partial class PhoVietGame`.

## 6. Quy trình mỗi yêu cầu

1. Đọc file này, `git status --short`, `git log --oneline -5`. Nếu `git log` có commit mới hơn bản ghi ở mục 1 → đọc thêm mục cuối cùng (21.x mới nhất) của `huongdan.md` và `git show --stat` các commit đó.
2. Chỉ đọc các mục/file liên quan theo bảng 5. Hỏi lại nếu chưa chắc ≥90%.
3. Làm, chạy kiểm tra phù hợp (18.2), xem ảnh nếu đổi hình. Việc dài: ghi “Tiến độ việc đang làm” ở mục 3 sau mỗi mốc, để phiên khác (tài khoản Claude khác hoặc Codex) làm tiếp nếu phiên này bị dừng giữa chừng.
4. Thêm mục `21.x` mới vào `huongdan.md` (hành vi, file, tham số, kiểm tra, trạng thái) và **cập nhật mục 1–3 của file này**.
5. Khi được yêu cầu build/phát hành: theo 15.2; xong thì ghi version/commit/run/cache vào mục 1 file này và `huongdan.md`.
