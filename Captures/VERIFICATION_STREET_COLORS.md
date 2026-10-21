# Phố Việt 1.29.1 — khôi phục màu cảnh quan

- Giữ nguyên ánh sáng trắng ban ngày, bầu trời, fog, chu kỳ 30 giây mỗi chiều và toàn bộ hệ đèn của 1.29.0.
- Gắn lại `PV_StreetPalette.png` gốc cho `PV_StreetAtlas.mat`: mặt tiền vàng, xanh ngọc, hồng, xanh lam; chi tiết cửa, mái hiên và gạch lát giữ màu vật liệu đã dựng. Không dùng bảng màu kiến trúc bạc của bản trước.
- Khôi phục nền đất xanh `92A68A` và màu gạch/bó vỉa gốc; giữ vạch sơn đường trắng trung tính.
- Sửa `PhoVietLightingUpgrade` để những lần cài lại đèn sau này cũng giữ màu phố. Lệnh riêng `RestoreColorfulStreetPaint` chỉ lưu vật liệu được đổi, không dựng lại scene/model hay chỉnh vật liệu xe. Script Blender ngày–đêm ngừng xuất bảng màu nhà nhạt, dùng bảng màu phố gốc.
- Không sửa các file Blender xe/khách/phố đang có thay đổi sẵn của người dùng.

## Kiểm tra

- `VerifyStreetArt`: 186 điều kiện đạt. `VerifyDayNight`: 273 điều kiện đạt, gồm đồng hồ, pause, emission và ánh sáng khi rẽ. Tổng 459.
- Đã xem ảnh Unity buổi sáng và ban đêm ở desktop, cùng ảnh mobile dọc. Sáu ảnh được lưu dưới tiền tố `street-colors-native-`.
- Build `build_bd2671ad33bb` thành công trong 46,713 giây, 0 lỗi; một cảnh báo cũ rằng công cụ Pipeline Editor không bật trong Player. Phiên bản 1.29.1, cache `7da072f7b7c355cd`.
- 8 kiểm tra WebGL thật đạt: ngày/đêm, trở lại ban ngày, desktop/mobile không tràn màn hình, mobile ngang và không có lỗi shader/JavaScript/Unity runtime. Đã xem ảnh WebGL và xác nhận màu mặt tiền/vỉa hè khôi phục đúng. Mobile dùng mô phỏng trình duyệt.
- Đang push và xác minh GitHub Pages.
