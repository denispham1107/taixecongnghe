# Phố Việt 1.57.1 (06/11/2026)

Claude Code build và phát hành. Chi tiết: `huongdan.md` 21.138–21.139.

## Thay đổi so với 1.57.0

- Sửa vùng xanh nhạt phía trước sau giao lộ: 1.57.0 ẩn sớm cả mesh mặt đường `Geometry_*`, lộ nền bên dưới. Nay chỉ đèn tín hiệu, đèn đường và biển chữ được ẩn sớm.

## Kiểm tra

- Build `Succeeded`, 2 gói, 0 lỗi, 1 warning PipelineNoRuntimeConfig. Cache `9ff81f1d0cf14557`.
- 188 preflight, 188 postflight; 580 file nguồn không đổi sau build; 43 bundle repo; 34 bundle site; 35 kiểm tra index.
- Trình duyệt 540 đạt, 0 lỗi JS/Unity: landing 459, web/PWA 37, fullscreen 20, năng lượng 13, tên lửa 11.
- Unity: VerifyMobilePerf2136 62 (bài mới đã báo lỗi đúng trên mã 1.57.0), GraphicsSettings 89, PerformanceSafety 1326.
- Ảnh WASM thật: trước `road2138-before-ngang-trung.png`; sau `release1571-road-*.png`; trên trang live `release1571-live-road-*.png`, đủ 3 mức, ngang và dọc.

## Đo FPS (không phải điện thoại), 1.57.0 → 1.57.1, Cao/Trung bình/Thấp

- CPU×2 cố định: 22,8/38,9/49,7 → 23,2/36,7/49,4.
- CPU×4 cố định: 12,3/18,9/23,4 → 12,2/16,7/20,9.
- CPU×4 có tự giảm: 38,8/57,2/56,1 → lần 1: 34,2/43,1/42,3; lần 2: 26,2/43,4/36,8.

Bản sửa tốn thêm vì đường và các mesh `Geometry_*` không còn bị ẩn sớm; vẫn cao hơn nhiều so với 1.56 (4,4/5/6,9 ở CPU×4).

## Phát hành

- Source/artifact `fcd8fa0`, push main.
- Pages run 42074745472 completed/success đúng SHA: https://github.com/denispham1107/taixecongnghe/actions/runs/42074745472
- Live: version 1.57.1 / cache `9ff81f1d0cf14557`; 28 kiểm tra đạt, 0 lỗi runtime (`release1571-live-results.json`, `release1571-pages.json`).
- Bài tên lửa trên trang live: 11 đạt, 0 lỗi (`release1571-live-missiles-checks.json`).

## Chưa làm

- Chưa thử trên điện thoại thật do Claude; chờ người dùng xác nhận và gửi FPS qua `?perf=1`.
- Có thể lấy lại một phần FPS bằng cách chỉ ẩn sớm các `Geometry_*` nhỏ đã kiểm từng loại.
- Test cũ lỗi sẵn: như `VERIFICATION_RELEASE_1_57.md`.
