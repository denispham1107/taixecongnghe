# Phố Việt 1.20.0 — HUD ngang gọn và điểm hẹn 3D

## Phạm vi được xác nhận

- Gộp thông tin quãng đường, Coin, mạng, chỉ đường và bản đồ thành một thanh ngang gọn, nền xanh đậm trong suốt giống HUD dọc. Kỷ lục, tạm dừng và bật/tắt âm thanh nằm trong cùng thanh.
- Thông báo chữ vàng nằm trong một dải nhỏ bên dưới. HUD dọc và góc nhìn rộng hơn 25% của bản 1.19 được giữ lại.
- Mẫu mũi tên người dùng chọn: khối nổi có bề dày, cạnh vát, mặt xanh ngọc sáng, viền sáng, quầng xanh và nhấp nhô nhẹ. Giữ nhãn ĐÓN KHÁCH / TRẢ KHÁCH.

## Triển khai

- `PhoVietLandscapeHud.cs`: thanh 1000 × 76 đơn vị, tỉ lệ thích ứng theo cả chiều rộng và chiều cao; dùng nền 64% opacity chung với HUD dọc. Lưu/khôi phục ma trận IMGUI để không ảnh hưởng menu chọn việc và các màn hình khác.
- `PhoVietPortraitHud.cs`: thông báo ngang cao 26 đơn vị, nền 70% opacity; vùng chạm HUD không gây đổi làn. `PhoVietGame.cs` chọn đúng bố cục theo hướng màn hình.
- Kiểm tra WebGL phát hiện nút tạm dừng chưa nhận ổn định touch trực tiếp. Web shell theo dõi pointer bắt đầu/kết thúc (chuột, chạm và bút) và gửi tọa độ chuẩn hóa qua `ApplyHudTap`; Unity quy đổi bằng vùng safe area hiện tại. Nhờ đó tap ngắn không bị mất khi không có khung hình Unity trong thời gian chạm. Giới hạn khoảng kéo, bỏ qua chọn việc và chặn sự kiện chuột tổng hợp sau touch để không bật/tắt hai lần. Các nút HUD web chỉ nhận một đường pointer để không bị xử lý lại bởi IMGUI; native giữ GUI mouse và Input.GetTouch. Áp dụng cho tạm dừng hai hướng và âm thanh ngang.
- `WaypointMesh.cs`: mesh khối kín có mặt trước, mặt sau, thành bên và mặt vát; 36 tam giác mỗi khối. Vòng tròn định vị dưới đất có 96 tam giác.
- `PhoVietJobMarker.cs`: khối xanh ngọc, viền bạc hà, viền tối tương phản, vệt sáng trên mặt, quầng sáng nhịp và vòng định vị; có nghiêng nhẹ để nhìn rõ độ dày. Không dùng ảnh phẳng thay cho khối 3D.
- `PhoVietWaypointSolid.shader`: đổ sáng theo pháp tuyến, tạo mặt sáng/tối và phản sáng nhẹ; không cần thêm đèn động hay hậu kỳ. Shader trong Resources được đưa vào WebGL.
- Điểm hẹn vẫn lấy địa chỉ thật của mạng phố, kiểm tra tầm nhìn camera và gắn đúng phía lề; điều hướng, quy tắc Coin, ba mạng và công việc không thay đổi.

## Kiểm tra Unity

- 1,535 kiểm tra đạt trong 22 bộ kiểm tra: gameplay, giao lộ, xe và va chạm, truy đuổi, ba mạng, công việc/GPS, art, safe area và HUD dọc.
- Bộ wayfinding có 57 kiểm tra, gồm độ dày mesh, pháp tuyến trước/sau, mặt vát, ngân sách tam giác, vị trí hai lề, đón/trả và cập nhật sau khi rẽ.
- Sau sửa xử lý touch, chạy lại 160 kiểm tra HUD dọc, gameplay và công việc; đều đạt. Thêm 7 kiểm tra cầu nối HUD: tạm dừng/tiếp tục, bỏ qua vùng chơi và tọa độ sai, bật âm thanh và không đóng danh sách chọn việc.
- Đã xem ảnh HUD dọc/ngang ở trạng thái đón, trả, không có việc và số lớn. Ảnh cận `waypoint-3d-detail-wide.png` thể hiện mặt vát, độ dày và các lớp viền.
- Các kiểm tra trình duyệt là mô phỏng thiết bị; không phải đo hiệu năng trên iPhone vật lý.

## Phát hành

- WebGL 1.20.0: build `build_c66deaad3530` thành công trong 173.298 giây, 0 lỗi.
- Hai cảnh báo: Pipeline runtime không được cấu hình (chỉ dùng Pipeline trong Editor); trường theo dõi native touch không dùng trong nhánh WebGL vì web đã dùng pointer bridge. Không có lỗi shader hay lỗi runtime được bỏ qua.
- Cache phát hành: `176343a50a90fd24`, Unity 6000.5.6f1.
- 69 kiểm tra trình duyệt đạt: 12 kiểm tra nhận chuyến/HUD bằng chuột và touch thật qua CDP, 37 kiểm tra WebGL/PWA/offline/điều khiển, 20 kiểm tra full viewport và safe area iPhone/iPad ở cả hai hướng.
- Đã xem ảnh `waypoint-3d-desktop-gps.png`, `waypoint-3d-phone-gps.png` và `waypoint-3d-phone-landscape.png` của bản build cuối. Nút pause ngang và dọc đều có kiểm tra chuyển trạng thái bằng hình ảnh, không chỉ gửi lệnh giả lập tới C#.
- Đang triển khai GitHub Pages; SHA, workflow và kết quả kiểm tra live sẽ được ghi sau khi hoàn tất.

Các tệp Blender đã thay đổi từ trước, bản sao lưu Blender và `huongdan.md` ngoài phạm vi được giữ nguyên và không đưa vào commit này.
