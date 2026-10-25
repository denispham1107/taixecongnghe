# BÀN GIAO NHANH — Phố Việt

> File này là **điểm bắt đầu cho mọi phiên Codex/Claude Code**. Đọc file này + `git status` + `git log --oneline -5` là đủ để nhận yêu cầu mới. **Không đọc lại toàn bộ `huongdan.md` (≈385 KB) hay toàn bộ mã nguồn**; chỉ mở mục/file liên quan theo bảng tra cứu bên dưới.
>
> Ai vừa sửa/thêm chức năng, build hoặc phát hành **phải cập nhật file này trong cùng công việc** (mục 1–3 và 6), song song với ghi chi tiết vào `huongdan.md`.

Cập nhật lần cuối: **26/10/2026 — Claude Code**

## 1. Trạng thái hiện tại

| Mục | Giá trị |
| --- | --- |
| Bản đã phát hành | **1.38.0** — commit `bec6ce1`, ghi nhận `452243c`; Pages run `40180769646`; cache `1ffc9c93f6c6d874` |
| Bản chơi | <https://denispham1107.github.io/taixecongnghe/> (thư mục `docs/` trên `main`) |
| Repo | <https://github.com/denispham1107/taixecongnghe>, nhánh `main` |
| Unity | 6000.5.6f1, Built-in RP, Legacy Input, IMGUI (`OnGUI`); scene `Assets/PhoViet/Scenes/PhoViet.unity` |
| Chế độ chơi | Chỉ thi quãng đường/kỷ lục. **Công việc/khách/GPS/Coin HUD đang khóa** (`RideJobsReleased=false`, scene `jobsEnabled=false`) |
| Tài liệu bàn giao | `BANGIAO.md`/`AGENTS.md`/`CLAUDE.md` đã commit riêng lên `main` (21.56) |
| Thay đổi chưa build/chưa commit | huongdan 21.54 (giảm tải sông/cầu: LOD căn hộ, nhiễu nướng sẵn, nước đục khi cần, gương LOD1+) và 21.55 (đổi làn theo đường cong bậc 5). Nằm trong working tree, cùng nhiều `.blend`/FBX/Captures sửa cục bộ |

## 2. Việc vừa làm gần nhất (mới nhất ở trên)

1. **Sau 1.38.0, chưa phát hành:** đổi làn mượt như cua (21.55); khu sông/cầu từ ~10–12 ms còn ~0,6 ms/khung trong Editor (21.54).
2. **1.38.0:** mặt sông gương, cầu dài 50,4 m giữa hai giao lộ, căn hộ kính 6–9 tầng ven sông sáng đèn đêm, sửa nhà lơ lửng trên sông, sửa test CSGT chập chờn (21.45–21.53).
3. **1.37.0:** cầu vượt sông 2 làn giữa, rơi sông = kết thúc; xe tăng mỗi 9 giây chạy ngược chiều bắn đạn vào 1/4 làn phía trước (1,0 s → tối thiểu 0,4 s theo tốc độ), bán kính nổ 2,2 m; ánh sáng sương sớm D, mũi tên báo xe sau D (21.35–21.44).
4. **1.36.0:** xe tăng Meshy “Iron Sentinel” (21.33–21.34). **1.35.0:** đổi làn kiểu đánh lái, nhà kính, zoom dọc +10% (21.28–21.32). **1.34.0:** phố Sài Gòn (21.19–21.27).

## 3. Đang dở / cần người dùng quyết

- 21.54–21.55 chưa build/commit/phát hành. Chỉ build/phát hành khi người dùng yêu cầu.
- Các file Wave cũ chưa theo dõi (`UpgradeBikesWave.cs`, `PhoViet_Motorcycles_Wave.blend`…) là lịch sử; **không xóa/commit/dùng** nếu chưa được yêu cầu.

## 4. Luật đã chốt — không tự đổi

- Hỏi lại bằng lựa chọn cụ thể nếu chưa chắc ≥90% ý người dùng.
- Không ba mạng. Té thường → hết lượt. Té khi đang bị CSGT đuổi → nằm chờ CSGT tới bắt thật → tù (không GameOver ngay, không tự dựng xe). Rơi sông/trúng đạn xe tăng → hết lượt, kể cả khi bị đuổi.
- Đạp quái xế tự vệ không gọi CSGT; đạp xe máy thường thì bị truy đuổi.
- Phím: **Q đạp, Space nhảy, W tăng tốc +125%/4 s/hồi 2 s**; mobile: chạm hai lần đạp, vuốt lên nhảy, vuốt xuống tăng tốc, nhận ngay khi đủ ngưỡng.
- Hai làn trái rẽ trái, hai làn phải rẽ phải, giữ số làn sau cua, cua không giảm tốc.
- Xe máy Meshy: xe chính trắng–đen; xe khác xanh ngọc/xanh dương/vàng/cam; **giữ mô tô CSGT cũ**. Ô tô Meshy 4 mẫu × 8 màu. Xe tăng một màu gốc, chậm hơn 60%, không bị đẩy.
- Phong cách bán hiện thực vui nhộn, tối ưu điện thoại/WebGL.
- Không bật lại công việc khi nâng cấp/sửa lỗi/build. Giữ dữ liệu Coin và mã jobs.
- Không ghi đè/reset file Blender cục bộ, không dựng lại cả scene bằng builder lịch sử.

## 5. Tra cứu nhanh: sửa gì → đọc gì

| Muốn đụng tới | Mục `huongdan.md` | File chính |
| --- | --- | --- |
| Điều khiển, đổi làn, camera, HUD | 2.2, 12, 21.29/21.55 | `PhoVietGame.cs`, `PhoVietGestures.cs`, `PhoViet*Hud.cs` |
| Giao thông, va chạm, văng/té | 7, 21.33 | `PhoVietTraffic.cs`, `PhoVietCollisions.cs`, `PhoVietRiderCrash.cs` |
| Quái xế, xe tải ngủ gật | 2.4, 7.2 | `PhoVietReckless.cs` |
| Xe tăng, đạn pháo | 21.38–21.40 | `PhoVietTankFire.cs`, `Editor/PhoVietTankFxUpgrade.cs` |
| Đạp, CSGT, bị bắt | 8 | `PhoVietActions.cs`, `PhoVietKickAssist.cs`, `PhoVietPolice.cs`, `PhoVietCrashPoliceNavigation.cs` |
| Nhảy, nóc xe, tăng tốc | 19 | `PhoVietJump.cs`, `PhoVietBoost.cs` |
| Giao lộ, rẽ, rebase tọa độ | 6.1, 9 | `PhoVietRoutes.cs`, `PhoVietRoadDrivers.cs`, `PhoVietExitTraffic.cs` |
| Sông, cầu, căn hộ ven sông | 21.35–21.37, 21.45–21.54 | `PhoVietRiver.cs`, `RiverBlock.cs`, `RiverReflection.cs`, `River*.shader`, `Editor/PhoVietRiverUpgrade.cs` |
| Cảnh phố, nhà, cây | 11.4, 21.19–21.28, 21.43 | `VietnamStreet.cs`, `StreetArt.cs`, `Editor/PhoVietStreetUpgrade.cs` |
| Ngày–đêm, đèn, sương | 20, 21.41 | `PhoVietDayNight.cs`, `DayNightLighting.cs`, `MorningFog.cs`, `MorningLooks.cs` |
| Model Blender/Meshy → Unity | 11, 13, 21.7, 21.17 | `Editor/PhoVietMeshy*Upgrade.cs`, `ArtSource/Blender/` |
| Công việc/Coin (đang khóa) | 10, 21.1 | `PhoVietJobs.cs`, `PhoVietJobGui.cs` |
| Kiểm tra theo loại thay đổi | 14, 18.2 | `AgentScripts/Verify*.cs`, `*.cjs` |
| Build WebGL, phát hành Pages | 15.2 | `Editor/PhoVietWebBuild.cs`, `AgentScripts/ConfigureWebRelease.cs` |

Bản đồ đầy đủ mọi file: `huongdan.md` mục 4. Các file `PhoViet*.cs` là phần của cùng `partial class PhoVietGame`.

## 6. Quy trình mỗi yêu cầu

1. Đọc file này, `git status --short`, `git log --oneline -5`. Nếu `git log` có commit mới hơn bản ghi ở mục 1 → đọc thêm mục cuối cùng (21.x mới nhất) của `huongdan.md` và `git show --stat` các commit đó.
2. Chỉ đọc các mục/file liên quan theo bảng 5. Hỏi lại nếu chưa chắc ≥90%.
3. Làm, chạy kiểm tra phù hợp (18.2), xem ảnh nếu đổi hình.
4. Thêm mục `21.x` mới vào `huongdan.md` (hành vi, file, tham số, kiểm tra, trạng thái) và **cập nhật mục 1–3 của file này**.
5. Khi được yêu cầu build/phát hành: theo 15.2; xong thì ghi version/commit/run/cache vào mục 1 file này và `huongdan.md`.
