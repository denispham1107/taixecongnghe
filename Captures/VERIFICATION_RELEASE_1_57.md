# Phố Việt 1.57.0 (05/11/2026)

Claude Code build và phát hành. Chi tiết: `huongdan.md` 21.136–21.137.

## Thay đổi so với 1.56.0

- Tối ưu giật lag mobile: mỗi mức đồ họa có kế hoạch riêng (`GraphicsPlan`), thiết bị cảm ứng tự giảm khi máy không theo kịp.
- Tắt khu phố ngoài tầm nhìn, chỉ một nguồn sáng hướng trên cảm ứng, WebGL bỏ cascaded shadow.
- Tải sẵn tài nguyên và shader, main loop theo requestAnimationFrame, HUD bỏ lượt layout và cache chữ.
- Gói mobile dùng texture 1K (trừ sơn xe người chơi): 116,6 → 70,3 MB.
- `?perf=1` hiện FPS trên máy thật.

## Đánh đổi trên điện thoại

- Độ nét tối đa 2x/1,6x/1,25x (trước 3x), tầm nhìn ngắn hơn và mờ dần vào sương.
- Hiệu ứng sương volume 6 giây đầu chỉ còn trên desktop.
- Trung bình: nhà và cây không đổ bóng. Thấp: không bóng, không phản chiếu.

## Đo so sánh (không phải điện thoại)

844×390 DPR3, Intel UHD620, 5 lượt × 9 giây, trên bản build thử cùng mã nguồn game với bản phát hành. FPS trung bình 1.56 → 1.57:

| Điều kiện | Cao | Trung bình | Thấp |
|---|---|---|---|
| CPU×2, cố định | 10,5 → 22,8 | 13,4 → 38,9 | 19 → 49,7 |
| CPU×4, cố định | 4,4 → 12,3 | 5 → 18,9 | 6,9 → 23,4 |
| CPU×4, có tự giảm | 4,4 → 38,8 | 5 → 57,2 | 6,9 → 56,1 |

Draw call WebGL/khung: Thấp 886 → 179, Trung bình → 196, Cao 977 → 317. Báo cáo: `perf2136-bench-*.json`, `perf2136-webcpu-*.json`.

## Kiểm tra

- Build `Succeeded`, 2 gói mobile/desktop, 0 lỗi, 1 warning PipelineNoRuntimeConfig. Cache `d612d524b1a39e3a`.
- 187 preflight mobile, 187 postflight desktop; 580 file nguồn không đổi sau build; 43 bundle repo; 34 bundle site; 35 kiểm tra index.
- Trình duyệt 540 đạt, 0 lỗi JS/Unity: landing 459, web/PWA 37, fullscreen 20, năng lượng 13, tên lửa 11.
- Unity: VerifyMobilePerf2136 56 cùng 23 bộ hồi quy (xem `huongdan.md` 21.136).

## Phát hành

- Source/artifact `0845f08`, push main.
- Pages run 42044425442 completed/success đúng SHA: https://github.com/denispham1107/taixecongnghe/actions/runs/42044425442
- Live: version 1.57.0 / cache `d612d524b1a39e3a`; 28 kiểm tra đạt, 0 lỗi runtime (`release157-live-results.json`, `release157-pages.json`).
- Bài tên lửa chạy trên chính trang live: 11 đạt, 0 lỗi (`release157-live-missiles-checks.json`).

## Chưa làm

- Chưa thử trên điện thoại thật/Safari; thời gian tải thêm do warmup shader trên máy thật chưa đo (laptop ANGLE/D3D11: 5–17 giây).
- Test cũ lỗi sẵn chưa sửa: `VerifyPerformanceLighting`, `VerifyRiverBridge`, `VerifyRiverCrashSinking`, `VerifyDistanceTanks2199`, `VerifyEnergy2122`, `VerifyEnergyRoutes2122`.
- `VerifyWebMemory.cjs` còn kiểm điện thoại giữ DPR>2,5; `VerifyHudTap.cs`, `VerifyPlayerMissileScene2128.cs` như ghi ở 1.56.
