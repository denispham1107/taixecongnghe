# Phố Việt 1.58.0 (06/11/2026)

Claude Code build và phát hành. Chi tiết: `huongdan.md` 21.140–21.141.

## Thay đổi so với 1.57.1

- Vuốt lên nhạy hơn, nhất là màn hình ngang: bỏ dải cấm ở đáy màn hình ngang, không còn góc vuốt "chết" 37–52°, quãng vuốt nhảy ngắn hơn (8,5%), nhận flick nhanh, ngón mới nhất thắng khi giữ hai tay, giữ lệnh nhảy 0,55 giây sau khi đáp, hồi nhảy 2 → 1,6 giây.
- Nút bay tròn ở góc trái, trên nút tên lửa: ấn để lên độ cao bay, ấn lần nữa để đáp.
- Khi bay có 3 icon năng lượng mỗi 80m đường, ngẫu nhiên 4 làn, ngang tầm xe.
- Giữ khu phố sau camera trong tầm bóng đổ để không mất bóng trên tường.

## Kiểm tra

- Build `Succeeded`, 2 gói, 0 lỗi, 1 warning PipelineNoRuntimeConfig. Cache `5b4a43086927aac5`.
- 192 preflight, 192 postflight; 583 file nguồn không đổi sau build; 43 bundle repo; 34 bundle site; 35 kiểm tra index.
- Trình duyệt 540 đạt, 0 lỗi JS/Unity: landing 459, web/PWA 37, fullscreen 20, năng lượng 13, tên lửa 11.
- Vuốt chạm thật trên WASM 1.58.0: 22/22 (shell 1.57.1 chỉ 12/22). Nút bay: 9/9 ngang và dọc.
- Unity: VerifyFlight2140 94, cùng 18 bộ hồi quy (xem `huongdan.md` 21.140).

## Phát hành

- Source/artifact `6fc558b`, push main.
- Pages run 42092459874 completed/success đúng SHA: https://github.com/denispham1107/taixecongnghe/actions/runs/42092459874
- Live: version 1.58.0 / cache `5b4a43086927aac5`; 28 kiểm tra đạt, 0 lỗi runtime.
- Trên trang live: tên lửa 11/11, nút bay 9/9, vuốt 22/22, 0 lỗi.

## Chưa làm

- Chưa thử trên điện thoại thật/Safari thật.
- FPS CPU×2 cố định Trung bình/Thấp thấp hơn 7–12% so với 1.57.1 trong một lần đo, chưa tách được do nhiễu hay do icon/nút mới.
- Mật độ 3 icon trên không mỗi tile là tự chọn, chờ người dùng góp ý.
- `VerifyJump` lỗi sẵn từ trước, chưa sửa.
