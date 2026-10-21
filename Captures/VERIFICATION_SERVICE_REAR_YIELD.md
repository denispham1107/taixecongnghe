# Phố Việt 1.29.3 — nhường đường khi đón/trả khách

## Lỗi và cách sửa

- Trước đây bộ điều khiển né xe chỉ chạy khi khách đang lên/xuống hoặc trong ba giây nhận việc. Sau khi trả khách không có cửa sổ nhường đường; sau đón khách chỉ còn bộ chặn xe phía sau, không tiếp tục điều khiển né chủ động. Đây là khoảng hở khi chuyển từ dừng phục vụ khách sang chạy lại.
- `PhoVietJobTraffic.TrafficYieldActive` tiếp tục chạy trong toàn bộ `ServiceRearProtected`. Cùng một bộ điều khiển chọn làn trống/phanh nhường được dùng cho xe thường, quái xế và xe tải ngủ gật, bao gồm xe đã chạy từ phía sau lên trước lúc dừng.
- `PhoVietJobs.TickService` kích hoạt đúng hai giây nhường đường sau cả đón lẫn trả khách, kể cả sau khi `ActiveJob` được xóa vì hoàn thành chuyến. Bảo vệ không bị đứt ở khung hình hoàn tất animation khách.
- Đồng hồ hai giây chuyển sang cuối bước xử lý di chuyển trong `TickRide`, sau khi xe cộ và va chạm đã cập nhật. Chỉ giảm khi người chơi thực sự chạy, không giảm khi đang phục vụ khách/đứng yên/tạm dừng.
- Giữ kiểm tra va chạm thật; không thêm miễn va chạm hay sửa luật thua/CSGT. Không đổi model, vật liệu hoặc các file Blender người dùng đang sửa.

## Kiểm tra

- `VerifyServiceRearYield`: 117 điều kiện đạt. Vòng cập nhật thật `TickRide` được chạy xuyên suốt boarding/alighting và lúc xe chạy lại; bao gồm năm loại xe, hai bên lề, bước thời gian 0,02/0,1 giây, xe đã ở sát điểm dừng chạy 50 m/s, làn bên cạnh bị kín, xe mới xuất hiện sau khi khách đã lên/xuống và việc hết cửa sổ hai giây.
- `VerifyJobEdges`: 17; `VerifyJobs`: 116; `VerifyRideRefinements`: 50; `VerifyRearWarningsService`: 140; `VerifyOrdinaryRearWarnings`: 137. Tổng cộng **577 điều kiện đạt**. Toàn chuyến vẫn trả thưởng đúng, cảnh báo xe phía sau vẫn đúng làn/hai giây, va chạm thật vẫn kích hoạt té/ngã.
- Kiểm tra mới đã thất bại trên bản trước khi sửa ở điều kiện thời gian sau đón khách; sau sửa toàn bộ kiểm tra đạt.
- Build WebGL `build_0e32b60114e0` thành công trong 230,549 giây, 0 lỗi, 3 cảnh báo cũ (Pipeline không bật trong Player, hai trường input chỉ dùng ngoài WebGL). Phiên bản 1.29.3, cache `ca5defa4375b1ca7`.
- 26 kiểm tra bản WebGL trên localhost đạt: tài nguyên đúng, WebAssembly thật khởi động và nhận thao tác bắt đầu/tạm dừng, không có lỗi JavaScript/Unity runtime. Kết quả lưu tại `service-rear-web-results.json`. Các trường hợp nhường đường được kiểm tra trong Unity bằng toàn bộ vòng `TickRide`; chưa kiểm tra trực tiếp trên iPhone/iPad.
- Unity trở về Edit Mode, giữ 55 Coin và kỷ lục 1.775 m. Máy chủ localhost đã dừng.

## Phát hành

- Người dùng xác nhận “Cho phép push và phát hành bản 1.29.3”. Commit nguồn/build `bec8d73323b45ea6549af5b006e8708f5265651f` đã push lên main.
- Ban đầu chưa thấy đợt Pages tự động trong danh sách; đã xác nhận workflow còn active và main đúng commit được duyệt, sau đó kích hoạt `workflow_dispatch` của workflow Pages có sẵn. Đợt https://github.com/denispham1107/taixecongnghe/actions/runs/39613250335 đã thành công. Lần kiểm tra tiếp tục cũng ghi nhận đợt https://github.com/denispham1107/taixecongnghe/actions/runs/39613396507 thành công với cùng commit `bec8d733`.
- **26 kiểm tra trang công khai đạt**: phiên bản 1.29.3/cache `ca5defa4375b1ca7` khớp, toàn bộ tài nguyên khớp bản đã thử, WebAssembly thật khởi động/nhận input bắt đầu và tạm dừng, không có lỗi JavaScript/Unity runtime. Kết quả lưu tại `service-rear-live-results.json`; trạng thái Pages tại `service-rear-pages-status.txt`.
- Trang chơi: https://denispham1107.github.io/taixecongnghe/. Đóng hẳn rồi mở lại PWA/web app để nhận bản 1.29.3.
