# Phố Việt 1.36.0 — xe tăng Meshy trên phố (24/10/2026)

## Thay đổi so với 1.35.0

- Xe tăng Meshy "Iron Sentinel" do người dùng gửi xuất hiện ngẫu nhiên ở 5 ô ô tô con (trọng số 0,5 → ~1/3 số lần), dài 7 m, rộng 2,51 m. `huongdan.md` 21.33.
- Một màu gốc duy nhất (mặt nạ sơn đen, không nhuộm).
- Luôn chạy 40% tốc độ ô tô khác (chậm hơn 60%), cả cùng chiều, ngược chiều, băng ngang/rẽ; không bao giờ là xe vượt lên từ phía sau.
- Va chạm: xe tăng là khối lượng vô hạn — không dừng, không chậm, không đổi làn/hướng; ô tô, xe máy, người chơi và CSGT va vào đều nhận toàn bộ xung lực và văng ra.
- Sửa 4 test cho ô xe con có thể mang hình xe tăng (MeshyCars, TrafficDynamics, CrashPoliceNavigation, RoofRiding). Công việc vẫn khóa.

## Kiểm tra

- Native (Play Mode): 24 suite / 9.521 điều kiện đạt, gồm VerifyMeshyTank 1.129, MeshyCars 1.266, RoofRiding 4.206, CrashPoliceNavigation 214 (`meshy-tank-verification-summary.json`, `meshy-tank-Verify*-results.json`). Ảnh `meshy-tank-sheet.png`.
- Preflight Edit Mode: `release-1.36-preflight.json`.
- Build `build_fa643a0c8cd3` Succeeded, 244.218 ms, 0 lỗi / 4 cảnh báo (3 đã biết + "uncompiled code changes" do sửa chuỗi version Editor ngay trước build; version đã đúng qua ConfigureWebRelease). Cache `2ad4c813b6952999`.
- 29 kiểm tra bundle: `release-1.36-bundle-checks.json`.
- Trình duyệt local 77 điều kiện, 0 lỗi runtime: DPR3 14 (`release-1.36-mobile-results.json`, ảnh `release-1.36-mobile-portrait.png`/`-landscape.png`), tài nguyên + WASM 26 (`release-1.36-local-results.json`), PWA/offline/cảm ứng 37.

## Phát hành

- Source/artifact `c49059c6dc83ef970c758c6e2147b48890feb260`, push main.
- Pages run 40090621801 completed/success đúng SHA: https://github.com/denispham1107/taixecongnghe/actions/runs/40090621801
- Live: version 1.36.0 / cache `2ad4c813b6952999`; 26 kiểm tra đạt, 0 lỗi runtime (`release-1.36-live-results.json`, `release-1.36-pages.json`, ảnh `release-1.36-live-game.png`).
- Kiểm tra lại sau phát hành: chơi thật bản live bằng Chrome GPU 150 khung hình, thấy xe tăng 2 lần (`release-1.36-live-tank-junction.png`, `release-1.36-live-tank.png` — người chơi tông xe tăng thì té, xe tăng không bị đẩy). Editor `ProbeTankGameplay`: 42 xe tăng/600 s chơi tự nhiên (~1 xe/14 s).
- Chưa thử điện thoại vật lý/FPS.
