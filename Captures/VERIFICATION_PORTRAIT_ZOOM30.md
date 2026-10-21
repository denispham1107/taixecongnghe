# Góc nhìn dọc xa hơn 30% — 1.27.2

- Yêu cầu: zoom ra thêm 30% so với bản hiện tại trên mobile dọc; game vẫn phủ kín màn hình.
- `PortraitWorldScale` đổi từ 0,75 thành 0,75 × 0,70 = 0,525. Nhân vật/xe có kích thước chiếu bằng 70% so với 1.27.1. FOV dọc đổi từ 80,69095° thành 101,01716° theo công thức phối cảnh `tan(FOV/2)`.
- Chỉ đổi nhánh camera portrait. Nhánh camera ngang, vị trí camera, HUD/GPS, khoảng vuốt, điều khiển, tốc độ và luật chơi giữ nguyên.
- Đã render và xem trực tiếp hai ảnh cùng cảnh, cùng vị trí camera: `portrait-zoom30-before.png` và `portrait-zoom30-after.png`; thấy rộng hai bên hơn và xe nhỏ hơn.
- `VerifyPortraitHud` được cập nhật để đo tỷ lệ chiếu 70% so với FOV bản 1.27.1, ở cả bốn làn và khi CSGT truy đuổi.
- 38 kiểm tra Unity đạt: PortraitHud 21, MobileCamera 8, SafeArea 9. Kiểm tra cả bốn làn và khi bị CSGT đuổi. Lần chạy đầu bị guard từ chối vì vừa bật Play Mode, kích thước Game View chưa cập nhật; chờ frame portrait ổn định rồi chạy lại, toàn bộ đạt.
- Build WebGL `build_7a0588f273d2` thành công, 169,147 giây, 0 lỗi; cache `c3f610804495fe43`. Ba cảnh báo không chặn build giữ nguyên từ 1.27.1: Pipeline không có runtime config và hai biến native không đọc trong WebGL.
- Unity được trả về Edit Mode, Game View ngang; công việc bật, giữ nguyên 55 Coin và kỷ lục 1.775 m.
- 20 kiểm tra WebGL đạt: version/tài nguyên đúng bản build; iPhone/iPad dọc/ngang phủ kín viewport, không có header hoặc hamburger che game; không lỗi JavaScript/Unity runtime.
- Đã push bản phát hành `93d2500233e69cb059730ee17e6bede1f8287a00`. GitHub Pages triển khai thành công: https://github.com/denispham1107/taixecongnghe/actions/runs/39525905208.
- 26 kiểm tra trang công khai đạt: version/cache và tài nguyên khớp bản build, WebAssembly thật khởi động/nhận input, không lỗi runtime. Kết quả `portrait-zoom30-live-results.json`.
- Trang chơi https://denispham1107.github.io/taixecongnghe/. Đã dừng HTTP server nội bộ. Đóng hẳn rồi mở lại PWA để nhận 1.27.2, không xóa dữ liệu. Chưa kiểm tra trên iPhone vật lý.
