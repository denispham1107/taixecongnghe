# Sửa phản hồi vuốt mobile dọc/ngang — 1.27.1

## Nguyên nhân đã xác minh từ code

- Bản 1.27.0 dùng `Screen.width * .075` cho đổi làn và `Screen.width * .12` cho nhảy/tăng tốc. Với viewport 412×839, ngưỡng tương đương khoảng 31/49 CSS pixel; khi xoay thành 839×412 lại thành 63/101 pixel. Cùng một khoảng vuốt sẽ vượt ngưỡng ở dọc nhưng không đủ ở ngang.
- `ReadSwipe()` chỉ nhận lệnh ở `TouchPhase.Ended`/mouse-up: luôn phải chờ nhấc tay.
- Unity đọc touch theo frame. Cử chỉ rất nhanh có thể bắt đầu và kết thúc giữa các frame; bridge DOM trước đây chỉ xử lý nút HUD, chưa xử lý vuốt gameplay.
- Đây là các vấn đề đường input xác minh được; không dùng kết quả giả lập trên PC để kết luận FPS/GPU của iPhone thật.

## Phương án người dùng đã chọn

Dùng cùng khoảng vuốt cho dọc/ngang, nhận ngay khi đủ ngưỡng, mỗi lần chạm một lệnh. Trái/phải vẫn đổi làn hoặc chọn rẽ; lên nhảy, xuống tăng tốc; chạm hai lần để đạp. Không đổi cơ chế di chuyển, animation, tốc độ, cooldown hoặc chất lượng đồ họa.

## Triển khai

- `Assets/WebGLTemplates/PhoVietPWA/app.js`: nhận pointer down/move/up/cancel trực tiếp từ trình duyệt, pointer capture giữ thao tác khi ngón tay đi ra mép canvas. Ngưỡng tính theo cạnh ngắn CSS canvas, không phụ thuộc DPR. Gửi lệnh ngay tại move vượt ngưỡng, đánh dấu đã dùng để không lặp khi giữ tay/nhấc tay. Pointer phụ không điều khiển gesture đang có.
- Gửi tọa độ chuẩn hóa cùng thời gian sự kiện gốc qua `ApplyBrowserGesture`. Lưu begin trước, không cho một gesture bắt đầu từ HUD/menu/paused/job picker biến thành lệnh gameplay khi trạng thái đổi.
- `PhoVietGestures.cs`: dùng chung bộ phân loại và bộ chặn một-lệnh cho runtime. Native/Editor cũng nhận trong lúc đang vuốt. WebGL tắt đường polling touch/mouse cũ để không nhận trùng sự kiện chuột tương thích.
- `PhoVietGame.cs`/`PhoVietJobs.cs`: hủy thao tác đang giữ khi restart, tạm dừng/tiếp tục hoặc mở chọn việc. Shell hủy khi pointer cancel, mất capture, blur, ẩn trang hoặc xoay máy; thay đổi kích thước canvas giữa thao tác cũng hủy.
- Double tap dùng timestamp từ trình duyệt, nên hai lần chạm được xử lý trong cùng một frame Unity vẫn giữ đúng khoảng thời gian. HUD vẫn dùng `ApplyHudTap`; mỗi nút được xử lý một lần.
- Ngưỡng ngang .075 cạnh ngắn; ngưỡng dọc .12 cạnh ngắn và phải rõ hướng dọc (lớn hơn ngang 1,3 lần). Một cú vuốt không tự lặp thành nhiều lần đổi làn.

## Kiểm tra

- 320 kiểm tra Unity đạt: VerifyGestures 39 ở ngang + 39 ở dọc; HUD ngang 7; HUD dọc 21; safe area 9; boost 39; jump 44; rẽ từ làn giữa 57; ba mạng 65.
- VerifyGestures kiểm tra hành động game thật trước release, chống lặp, sai pointer/cancel, HUD, tạm dừng/resume/restart, chọn việc và hai tap trong cùng frame. Các ngưỡng được kiểm tra cho điện thoại/tablet dọc/ngang.
- 25 kiểm tra shell qua pointer/touch của Chromium đạt: bốn hướng ở cả hai khổ gửi lệnh trước finger-up, không lặp khi giữ/release, jitter vẫn là tap, cancel/rotation không sinh lệnh cũ, gesture mới hoạt động ngay sau rotation. Mock chỉ dùng để cô lập shell; không được xem là kiểm tra engine thật.
- Test engine thật dùng timestamp input cụ thể cho cú chạm 40 ms: software rendering có thể làm từng round trip CDP chậm hơn giới hạn tap 240 ms. Không thay đổi giới hạn tap của game để chiều theo tốc độ máy chạy test.
- 119 kiểm tra trình duyệt đạt: shell cô lập 25; cử chỉ với engine WebGL thật 25; công việc/HUD qua input thật 12; WebGL/PWA/offline 37; iPhone/iPad dọc/ngang toàn màn hình 20. Trong engine thật, mỗi hướng đều gửi lệnh trước finger-up và không gửi hai lần. Nút pause hoạt động bằng touch cả trước/sau rotation; game WebAssembly chạy khi tắt mạng. Không có lỗi runtime.
- Chưa kiểm tra trực tiếp trên iPhone/iPad vật lý. Cần người dùng đóng hẳn rồi mở lại PWA sau phát hành để kiểm tra cảm giác trên máy thật; không xóa dữ liệu Coin/kỷ lục.

## Phát hành

- WebGL 1.27.1 build thành công: `build_aa064c7ed889`, 193,197 giây, 0 lỗi, cache `88961a68f468460d`. Ba cảnh báo không chặn build: Pipeline không có runtime config (công cụ Editor), hai biến native `dragging`/`hudTouchChoice` không được đọc ở nhánh WebGL vốn dùng DOM bridge.
- Đã đọc lại Unity ở Edit Mode, Game View ngang, công việc bật; giữ nguyên 55 Coin và kỷ lục 1.775 m. Các file Blender sửa sẵn và `huongdan.md` không thuộc bản sửa này.
- Đã commit/push bản phát hành `1d4a9c6a242329de5771f75ab8684df8d1f1a5d9`. GitHub Pages triển khai thành công: https://github.com/denispham1107/taixecongnghe/actions/runs/39523247146.
- 26 kiểm tra trang công khai đạt: version/cache khớp, tài nguyên khớp byte với bản kiểm tra, Unity WebAssembly thật khởi động và nhận input bắt đầu/tạm dừng; không lỗi runtime. Kết quả `gesture-live-results.json`.
- Trang chơi https://denispham1107.github.io/taixecongnghe/. Đã dừng server HTTP kiểm tra nội bộ. Đóng hẳn rồi mở lại PWA để cập nhật; không xóa dữ liệu.
