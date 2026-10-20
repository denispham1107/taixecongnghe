# Phố Việt 1.21.0 — cảnh báo xe vượt sau và nhường khi trả khách

## Yêu cầu đã xác nhận

- Mỗi đợt từ phía dưới chỉ có một quái xế hoặc xe tải ngủ gật, chọn ngẫu nhiên một trong hai làn cùng chiều. Xe vẫn được lạng lách sau khi đã vượt lên.
- Nếu xe nguy hiểm ngược chiều đang chạy, chờ nó va chạm xe khác hoặc nhà cửa rồi mới bắt đầu cảnh báo hai giây cho xe vượt sau. Không reset/bỏ qua quy tắc khi rẽ trái hoặc phải.
- Chỉ hiển thị mũi tên đỏ viền vàng, nhấp nháy ở đúng làn sắp có xe vượt lên; không báo đồng thời hai làn.
- Trong suốt lúc khách xuống xe, các xe khác chủ động tránh hoặc nhường tài xế đang dừng.

## Mã nguồn

- `PhoVietReckless.cs`: hàng chờ riêng lưu loại xe, làn và thời gian cảnh báo. Chặn một đợt phía sau mới khi còn xe phía sau đang chạy; chặn countdown/spawn khi có xe nguy hiểm ngược chiều, đang rẽ hoặc đang phục vụ khách. Nếu bị chặn, yêu cầu đủ hai giây mới khi tiếp tục. Điểm xuất hiện nằm ngoài camera, trên chính làn đã báo; giữ nguyên làn khi khoảng trống xuất hiện chưa đủ an toàn. Pool và kiểm tra chồng xe/CSGT vẫn được dùng.
- `PhoVietRearWarning.cs`: vẽ mũi tên nhỏ màu đỏ với cạnh vàng, vệt sáng và quầng nhịp. Chiếu tọa độ làn qua camera xuống mép dưới trong vùng an toàn màn hình, nên bám làn trên cả màn hình ngang/dọc và camera đang ổn định sau cua.
- `PhoVietJobTraffic.cs`: cơ chế đổi làn/nhường hoạt động trong toàn bộ Boarding/Alighting, không phụ thuộc ba giây sau nhận việc. Xe băng ngang hoặc đang theo đường rẽ dừng nhường nếu quỹ đạo đi vào vị trí tài xế. Giới hạn dịch chuyển bằng kiểm tra quét trước tài xế đang dừng là phanh khẩn cấp, không bỏ kiểm tra va chạm hay cho xe đi xuyên qua.
- `PhoVietJobs.cs`: xóa lựa chọn làn nhường cũ khi bắt đầu đón/trả. `PhoVietGame.cs`: cập nhật countdown trong thời gian chạy và gọi cảnh báo ở HUD; kiểm tra phanh trước/sau tương tác giữa các xe.
- `SpawnReckless` là hàm tạo vật thể mức thấp dùng bởi công cụ preview/test; vòng chơi thật gọi qua `MaybeSpawnReckless` → hàng chờ/cảnh báo → tạo xe. Các lệnh preview không đại diện cho bộ hẹn giờ gameplay.

## Kiểm tra

- 140 kiểm tra mới trong `VerifyRearWarningsService`: đủ hai giây, đúng làn, ngẫu nhiên hai làn, không chồng đợt phía sau; đợi va chạm thực với xe/nhà; rẽ thật cả hai hướng; pause/restart; xe máy, ô tô, xe tải từ hai phía tại lề trái/phải khi khách xuống; xe băng ngang nhường.
- Các kiểm tra cũ về làn kế bên được cập nhật theo yêu cầu làn cùng chiều ngẫu nhiên; kiểm tra không chồng xe CSGT vẫn xét đầy đủ cả khoảng cách ngang và dọc.
- Tổng cộng 1,675 kiểm tra đạt trong 23 bộ kiểm tra Unity, gồm gameplay, va chạm, CSGT, ba mạng, công việc/GPS, đường rẽ, art, camera, safe area và HUD.
- Đã xem đủ bốn ảnh cảnh báo: hai làn cùng chiều trên màn hình dọc và ngang. Chỉ một mũi tên hiện ở một thời điểm, có viền vàng và nền tối tạo tương phản; không chiếm khu vực HUD phía trên.

## Phát hành

- WebGL 1.21.0 build `build_ab0840d58d46` thành công trong 206.556 giây, 0 lỗi; cache `f3c419729535151d`.
- Hai cảnh báo cũ: Pipeline chỉ được dùng trong Editor, và trường native HUD touch không dùng ở nhánh WebGL. Không có lỗi shader hoặc lỗi build.
- 69 kiểm tra web đạt: 12 kiểm tra UI/chuột/touch/xoay, 37 kiểm tra WebGL/PWA/offline/điều khiển, 20 kiểm tra full viewport/safe area iPhone/iPad dọc/ngang. Không có lỗi JavaScript/Unity runtime.
- Các profile iPhone/iPad là mô phỏng bố cục, không thay thế kiểm tra hiệu năng Safari trên máy thật.
- GitHub Pages: đang triển khai; SHA, workflow và kiểm tra live sẽ ghi khi hoàn tất.

Các tệp Blender và `huongdan.md` có thay đổi từ trước được giữ riêng.
