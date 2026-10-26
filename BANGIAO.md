# BÀN GIAO NHANH — Phố Việt

> File này là **điểm bắt đầu cho mọi phiên Codex/Claude Code**. Đọc file này + `git status` + `git log --oneline -5` là đủ để nhận yêu cầu mới. **Không đọc lại toàn bộ `huongdan.md` (≈385 KB) hay toàn bộ mã nguồn**; chỉ mở mục/file liên quan theo bảng tra cứu bên dưới.
>
> Ai vừa sửa/thêm chức năng, build hoặc phát hành **phải cập nhật file này trong cùng công việc** (mục 1–3 và 6), song song với ghi chi tiết vào `huongdan.md`.

Cập nhật lần cuối: **26/10/2026 — Codex**

## 1. Trạng thái hiện tại

| Mục | Giá trị |
| --- | --- |
| Bản đã phát hành | **1.39.0** — commit `17aa0b2`; Pages run `40268861830` success; cache `770899e40e23f8e2`; **26 kiểm tra live đạt/0lỗi runtime** |
| Bản chơi | <https://denispham1107.github.io/taixecongnghe/> (thư mục `docs/` trên `main`) |
| Repo | <https://github.com/denispham1107/taixecongnghe>, nhánh `main` |
| Unity | 6000.5.6f1, Built-in RP, Legacy Input, IMGUI (`OnGUI`); scene `Assets/PhoViet/Scenes/PhoViet.unity` |
| Chế độ chơi | Chỉ thi quãng đường/kỷ lục. **Công việc/khách/GPS/Coin HUD đang khóa** (`RideJobsReleased=false`, scene `jobsEnabled=false`) |
| Tài liệu bàn giao | `BANGIAO.md`/`AGENTS.md`/`CLAUDE.md` đã commit riêng lên `main` (21.56) |
| Người chơi trong source/scene | **Người áo đỏ sao vàng ngực/lưng trên xe máy xanh dương–đen**; Q/chạm hai lần áp sát rồi đạp, Space/vuốt lên nhảy, W/vuốt xuống boost (21.58). Đã có trên web1.39.0 |
| Điều khiển/nhảy trong source | **21.60:** chuyển làn0,72 s, trả lái/dựng xe tới~2 s; nhảy theo tốc độ lúc cất cánh (17 m/s~3,63 m,30 m/s~4,84 m), W có tính; W giữa không trung chỉ tăng độ cao cú sau |
| Thay đổi chưa build/chưa commit | Các thay đổi21.54–21.55,21.58–21.60 đã phát hành trong1.39.0; giữ các art/.blend/Captures cục bộ và lịch sử Wave không liên quan, không reset/commit tự động |

## 2. Việc vừa làm gần nhất (mới nhất ở trên)

- **1.39.0 đã phát hành (21.61):** commit17aa0b2, Pages40268861830 thành công; build0lỗi/3warning,29bundle +37web/offline/touch +20fullscreen +26live đạt,0lỗi runtime. Unity Edit Mode/scene sạch, jobs khóa.

1. **21.60, đã có trên web1.39.0:** đổi làn0,72 s và phục hồi nghiêng chậm; nhảy thấp25% ở tốc độ đầu, tăng đều theo tốc độ lúc cất cánh/W; W giữa không trung giữ cú đang bay. **6 suite/541 điều kiện Unity đạt**, xem trace/ảnh; chi tiết huongdan21.60.
2. **21.58, đã có trên web1.39.0:** khôi phục đúng hero áo đỏ sao vàng/ngón nắm v4 và xe máy Meshy, sơn xanh dương–đen; Q trở lại đạp, nhảy/boost/đèn/khói cũ. NPC28/CSGT giữ nguyên. **4 suite / 229 điều kiện Unity đạt**, xem ảnh trước/sau; người/sao và phần đen trong atlas không đổi. Thay lựa chọn player Petshop21.57.
3. **Đã có trên web1.39.0:** đổi làn mượt như cua (21.55); khu sông/cầu từ ~10–12 ms còn ~0,6 ms/khung trong Editor (21.54).
4. **1.38.0:** mặt sông gương, cầu dài 50,4 m giữa hai giao lộ, căn hộ kính 6–9 tầng ven sông sáng đèn đêm, sửa nhà lơ lửng trên sông, sửa test CSGT chập chờn (21.45–21.53).
5. **1.37.0:** cầu vượt sông 2 làn giữa, rơi sông = kết thúc; xe tăng mỗi 9 giây chạy ngược chiều bắn đạn vào 1/4 làn phía trước (1,0 s → tối thiểu 0,4 s theo tốc độ), bán kính nổ 2,2 m; ánh sáng sương sớm D, mũi tên báo xe sau D (21.35–21.44).
6. **1.36.0:** xe tăng Meshy “Iron Sentinel” (21.33–21.34). **1.35.0:** đổi làn kiểu đánh lái, nhà kính, zoom dọc +10% (21.28–21.32). **1.34.0:** phố Sài Gòn (21.19–21.27).

## 3. Đang dở / cần người dùng quyết

> **Tiến độ việc đang làm** (cập nhật ở mỗi mốc của việc dài; xóa khi xong): _không có việc đang làm dở._
> Mẫu: `Yêu cầu: … · Đã xong: … · Bước kế tiếp: … · File đang sửa: … · Unity/Blender: … · Cập nhật: <ngày giờ>, <agent>`

- 1.39.0 đã build/commit/push/phát hành và kiểm tra live xong; không có việc phát hành dở.
- Petshop/húc21.57 đã ngưng dùng từ21.58; giữ asset/code lưu trữ, **không tự chạy lại installer Petshop**. Các test/giới hạn cua cực muộn riêng cho ô tô21.57 không mô tả player hiện tại. Đã kiểm tra WebGL mobile mô phỏng dọc/ngang; chưa thử Safari trên điện thoại thật.
- Các file Wave cũ chưa theo dõi (`UpgradeBikesWave.cs`, `PhoViet_Motorcycles_Wave.blend`…) là lịch sử; **không xóa/commit/dùng** nếu chưa được yêu cầu.

## 4. Luật đã chốt — không tự đổi

- Hỏi lại bằng lựa chọn cụ thể nếu chưa chắc ≥90% ý người dùng.
- Không ba mạng. Té thường → hết lượt. Té khi đang bị CSGT đuổi → nằm chờ CSGT tới bắt thật → tù (không GameOver ngay, không tự dựng xe). Rơi sông/trúng đạn xe tăng → hết lượt, kể cả khi bị đuổi.
- Đạp quái xế tự vệ không gọi mới CSGT; đạp xe thường thì bị truy đuổi.
- Phím: **Q đạp, Space nhảy, W tăng tốc +125%/4 s/hồi 2 s**; mobile: chạm hai lần đạp, vuốt lên nhảy, vuốt xuống tăng tốc, nhận ngay khi đủ ngưỡng.
- Hai làn trái rẽ trái, hai làn phải rẽ phải, giữ số làn sau cua, cua không giảm tốc.
- Source21.58: hero Golden Star Stance áo đỏ sao vàng ngực/lưng, xe máy **xanh dương–đen**. Đã có trên web1.39.0. NPC xe máy nhiều màu/áo khác, CSGT giữ nguyên cả người và xe; ô tô NPC4 mẫu×8 màu, xe tăng màu gốc/chậm60%/không bị đẩy.
- Phong cách bán hiện thực vui nhộn, tối ưu điện thoại/WebGL.
- Không bật lại công việc khi nâng cấp/sửa lỗi/build. Giữ dữ liệu Coin và mã jobs.
- Không ghi đè/reset file Blender cục bộ, không dựng lại cả scene bằng builder lịch sử.

## 5. Tra cứu nhanh: sửa gì → đọc gì

| Muốn đụng tới | Mục `huongdan.md` | File chính |
| --- | --- | --- |
| Điều khiển, đổi làn, camera, HUD | 2.2, 12, 21.55/21.60 | `PhoVietGame.cs`, `PhoVietGestures.cs`, `PhoViet*Hud.cs` |
| Giao thông, va chạm, văng/té | 7, 21.33 | `PhoVietTraffic.cs`, `PhoVietCollisions.cs`, `PhoVietRiderCrash.cs` |
| Quái xế, xe tải ngủ gật | 2.4, 7.2 | `PhoVietReckless.cs` |
| Xe tăng, đạn pháo | 21.38–21.40 | `PhoVietTankFire.cs`, `Editor/PhoVietTankFxUpgrade.cs` |
| Đạp, CSGT, bị bắt | 8 | `PhoVietActions.cs`, `PhoVietKickAssist.cs`, `PhoVietPolice.cs`, `PhoVietCrashPoliceNavigation.cs` |
| Nhảy, nóc xe, tăng tốc | 19, 21.60 | `PhoVietJump.cs`, `PhoVietBoost.cs` |
| Giao lộ, rẽ, rebase tọa độ | 6.1, 9 | `PhoVietRoutes.cs`, `PhoVietRoadDrivers.cs`, `PhoVietExitTraffic.cs` |
| Sông, cầu, căn hộ ven sông | 21.35–21.37, 21.45–21.54 | `PhoVietRiver.cs`, `RiverBlock.cs`, `RiverReflection.cs`, `River*.shader`, `Editor/PhoVietRiverUpgrade.cs` |
| Cảnh phố, nhà, cây | 11.4, 21.19–21.28, 21.43 | `VietnamStreet.cs`, `StreetArt.cs`, `Editor/PhoVietStreetUpgrade.cs` |
| Ngày–đêm, đèn, sương | 20, 21.41 | `PhoVietDayNight.cs`, `DayNightLighting.cs`, `MorningFog.cs`, `MorningLooks.cs` |
| Người chơi xe máy xanh–đen | 21.58, 21.9–21.16, 8, 19 | `RestoreBlueBlackPlayer.cs`, `prepare_hero_blue_black.py`, `BikeVisualAnimator.cs` |
| Model Blender/Meshy → Unity | 11, 13, 21.7, 21.17 | `Editor/PhoVietMeshy*Upgrade.cs`, `ArtSource/Blender/` |
| Công việc/Coin (đang khóa) | 10, 21.1 | `PhoVietJobs.cs`, `PhoVietJobGui.cs` |
| Kiểm tra theo loại thay đổi | 14, 18.2 | `AgentScripts/Verify*.cs`, `*.cjs` |
| Build WebGL, phát hành Pages | 15.2, 21.61 | `Editor/PhoVietWebBuild.cs`, `AgentScripts/ConfigureWebRelease.cs` |

Bản đồ đầy đủ mọi file: `huongdan.md` mục 4. Các file `PhoViet*.cs` là phần của cùng `partial class PhoVietGame`.

## 6. Quy trình mỗi yêu cầu

1. Đọc file này, `git status --short`, `git log --oneline -5`. Nếu `git log` có commit mới hơn bản ghi ở mục 1 → đọc thêm mục cuối cùng (21.x mới nhất) của `huongdan.md` và `git show --stat` các commit đó.
2. Chỉ đọc các mục/file liên quan theo bảng 5. Hỏi lại nếu chưa chắc ≥90%.
3. Làm, chạy kiểm tra phù hợp (18.2), xem ảnh nếu đổi hình. Việc dài: ghi “Tiến độ việc đang làm” ở mục 3 sau mỗi mốc, để phiên khác (tài khoản Claude khác hoặc Codex) làm tiếp nếu phiên này bị dừng giữa chừng.
4. Thêm mục `21.x` mới vào `huongdan.md` (hành vi, file, tham số, kiểm tra, trạng thái) và **cập nhật mục 1–3 của file này**.
5. Khi được yêu cầu build/phát hành: theo 15.2; xong thì ghi version/commit/run/cache vào mục 1 file này và `huongdan.md`.
