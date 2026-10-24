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

- Đang commit/push main; SHA, Pages run và kiểm tra live ghi bổ sung sau khi xác minh.
- Chưa thử điện thoại vật lý/FPS.
