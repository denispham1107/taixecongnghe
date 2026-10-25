# Phố Việt 1.38.0 — sông gương, cầu dài gấp đôi, căn hộ kính ven sông (25/10/2026)

## Thay đổi so với 1.37.0

- Mặt sông kiểu gương theo ảnh mẫu (`huongdan.md` 21.45–21.46):
  - Camera gương phản chiếu thật trời, mây, kè, cầu và nhà.
  - Gợn nước làm rung hình phản chiếu; nước xanh đen, sát kè ngả màu bùn.
  - Chi phí khoảng 2,8 ms/khung trong Editor.
- Cầu dài gấp đôi, 50,4 m, chạy suốt giữa hai ngã tư và giữ mọi ngã tư (21.46).
- Sửa ở hai đầu cầu:
  - Nền đường thừa trên mặt sông (21.47).
  - Ẩn nhà đường ngang đứng trên sông (21.47); sửa lại để vẫn ẩn khi té/rơi sông (21.52).
  - Bỏ hàng rào dài ở góc ngã tư (21.48).
- Hai bờ sông:
  - Dãy căn hộ cao cấp 6–9 tầng: ban công lan can kính, vách kính trong suốt sàn–trần, phòng nhìn thấy bên trong (21.48–21.49).
  - Khoảng 70% phòng sáng đèn ấm ban đêm (21.51).
  - Tầng trệt mặt đường ngang là cửa hàng.
  - Nhà không lấn mặt nước.
- Sửa lỗi CSGT trượt xuyên xe tải / kẹt cạnh xe tải (21.50).
- Công việc vẫn khóa.

## Kiểm tra

- Native (Play Mode): 28 suite / 9.770 điều kiện đạt trên đúng mã phát hành, gồm VerifyRiverBridge 153 và CrashPoliceNavigation 214 (21.52).
- Preflight Edit Mode `release-1.38-preflight.json`:
  - 735 bộ / 8.050 hộp vật cản phố + 35 bộ dãy căn hộ ven sông;
  - 1.260 LOD phố, 1.350 renderer kính, 420 tín hiệu.
- Build `build_13392e2af78d`: Succeeded, 310.288 ms, 0 lỗi, 3 cảnh báo đã biết.
  - Cache `1ffc9c93f6c6d874`.
  - Data 91,6 MB, còn ~8,4 MB tới giới hạn 100 MB/file của GitHub.
- 29 kiểm tra bundle: `release-1.38-bundle-checks.json`.
- Trình duyệt local: 77 điều kiện, 0 lỗi runtime.
  - DPR3: 14 (`release-1.38-mobile-results.json`, ảnh `release-1.38-mobile-portrait.png` và `-landscape.png`).
  - Tài nguyên + WASM: 26 (`release-1.38-local-results.json`).
  - PWA/offline/cảm ứng: 37.

## Phát hành

- Source/artifact `bec6ce1`, push main.
- Pages run 40180769646 completed/success đúng SHA: https://github.com/denispham1107/taixecongnghe/actions/runs/40180769646
- Live: version 1.38.0 / cache `1ffc9c93f6c6d874`; 26 kiểm tra đạt, 0 lỗi runtime (`release-1.38-live-results.json`, `release-1.38-pages.json`).
- Chơi thật bản live bằng Chrome GPU (3 lượt, 0 lỗi trang): thấy cầu dài, mặt sông gương phản chiếu dãy căn hộ kính bờ xa (`release-1.38-live-bridge.png`, `release-1.38-live-bridge-2.png`).
- Chưa thử điện thoại vật lý/FPS; chưa đo chi phí gương trên thiết bị thật.
