# Phố Việt 1.56.0 (05/11/2026)

Claude Code build và phát hành. Chi tiết: `huongdan.md` 21.134–21.135.

## Thay đổi so với 1.55.0

- Nút bắn tên lửa chuyển sang góc trái dưới; thanh gợi ý phím dời sang phải nút.
- Âm lượng toàn game to thêm 20%: master 1,3× → 1,56× (+3,86249 dB).
- Trang chủ thêm ảnh `poster-home-flight.png` ngay dưới ảnh cũ: xe đang bay, 2 trực thăng, tên lửa sắp tới xe.
- Công việc đưa đón vẫn khóa.

## Kiểm tra

- Build `Succeeded`, 2 gói mobile/desktop, 0 lỗi, 1 warning PipelineNoRuntimeConfig. Cache `03fc6346d88758ed`.
- 183 preflight mobile, 183 postflight desktop; 577 file nguồn không đổi sau build; 44 bundle repo; 34 bundle site; 36 kiểm tra index.
- Trình duyệt 540 đạt, 0 lỗi JS/Unity: landing 459, web/PWA 37, fullscreen 20, năng lượng 13, tên lửa 11.
- Unity: PlayerMissiles 262, Audio 67, Scene 115. Đo âm trong Editor 1,5695× (mong đợi 1,56 ±0,03).

## Phát hành

- Source/artifact `ecb0b02`, push main.
- Pages run 41992250686 completed/success đúng SHA: https://github.com/denispham1107/taixecongnghe/actions/runs/41992250686
- Live: version 1.56.0 / cache `03fc6346d88758ed`; 28 kiểm tra đạt, 0 lỗi runtime (`release156-live-results.json`, `release156-pages.json`).
- Bài tên lửa/âm lượng chạy trên chính trang live: 11 đạt, 0 lỗi (`release156-live-missiles-checks.json`).

## Chưa làm

- Chưa nghe thử trên loa/điện thoại thật: 1,56× có thể méo khi nhiều tiếng to chồng nhau.
- Chưa thử Safari/Android máy thật.
- `VerifyHudTap.cs` còn tọa độ HUD cũ; `VerifyPortraitHud.cs` cần Game View dọc, chưa chạy; `VerifyPlayerMissileScene2128.cs` ghi cứng version 1.54.0.
