# Phố Việt 1.3.0 — Quái xế, xe tải và tự vệ

Ngày kiểm tra: 17/10/2026. Unity 6000.5.6f1, scene PhoViet.

- Biên dịch C#: 0 lỗi, 0 cảnh báo.
- VerifyGameplay: 23/23; VerifyCrossTraffic: 28/28; VerifyPursuit: 39/39; VerifyReckless: 81/81. Tổng 171/171 kiểm tra đạt.
- Xe máy thường ngược chiều đạp được ở cả phía trái/phải trong tầm. Xe tải và ô tô không phải mục tiêu đạp.
- Quái xế trong tầm được ưu tiên khi tự vệ. Đạp quái xế không gọi CSGT; một cuộc truy đuổi có trước do đạp xe thường vẫn tiếp tục và không bị đặt lại khoảng cách.
- Sự kiện đầu tiên ở giây 20, kiểm tra 40 lần lặp với khoảng cách ngẫu nhiên 15–20 giây; tạm dừng không chạy lịch sự kiện.
- Xe thể thao kiểu Exciter và xe tải cỡ trung chạy từ làn ngược chiều, có hướng xe theo chuyển động lạng lách và đi qua vạch giữa đường. Hai mẫu dùng pool riêng; đủ 24 xe thường vẫn được cấp phát.
- Cả hai xe sự kiện có thể húc xe máy và ô tô sang hai bên; va chạm tự phát không cộng hành động cho người chơi hoặc gọi CSGT.
- Kiểm tra chuyển động tự lao vào nhà ở cả hai bên cho cả hai loại xe, không chỉ đặt xe trực tiếp tại tường. Khoảng giao lộ trống không bị coi là tường.
- Va chạm người chơi với xe sự kiện gây thua. Xác xe ở lại đúng vị trí cạnh nhà khi phố cuộn. Khói/lửa dừng khi pause, xóa sạch khi thu hồi xe và khi chơi lại.
- Đã xem ảnh desktop của xe quái xế, xe tải húc xe và xe cháy. Hiệu ứng có khói đen, hạt lửa và ngọn lửa cách điệu; vị trí phát nằm ngoài thân xe để nhìn được từ camera sau lưng.
- Đã xem khung hình dọc 540×960 của cú đạp tự vệ: thông báo không gọi CSGT, nút đổi làn và double tap. Double tap được kiểm thử bằng handler trong Editor, chưa thử trên điện thoại thật.

Ảnh cuối: 21-burning-truck.png, 22-truck-rams-traffic.png, 23-racing-boy.png, 24-self-defense-mobile.png. Các ảnh là tình huống được đặt có kiểm soát để kiểm tra hình ảnh.

Windows: build_b546a81df639, Succeeded, 0 lỗi, 1 cảnh báo dự kiến Pipeline điều khiển từ xa không bật trong Player; khoảng 12,4 giây, 136.463.584 byte. Assembly trong bản build có SpawnReckless. Khởi động bản mới trong 5 giây với -batchmode -nographics: tiếp tục chạy, không thấy exception/runtime error trong windows-v1.3-smoke.log. Kiểm tra hình ảnh thực hiện trong Editor.

Trạng thái bàn giao: Unity Play Mode ở màn hình Ready, Time.timeScale=1, bản 1.3.0, 26 xe trong pool, sự kiện kế tiếp giây 20. Kỷ lục 1775 m được giữ nguyên. Bản chạy: Builds/Windows/PhoViet.exe cùng thư mục dữ liệu/DLL đi kèm.
