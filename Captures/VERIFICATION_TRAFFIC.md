# Phố Việt 1.4.0 — Phanh gấp, tạt làn, xe rẽ và va chạm

Ngày kiểm tra: 18/10/2026 (Asia/Bangkok). Unity 6000.5.6f1.

- Biên dịch C#: không lỗi, không cảnh báo.
- VerifyGameplay 23/23, VerifyCrossTraffic 28/28, VerifyPursuit 39/39, VerifyReckless 87/87, VerifyTrafficDynamics 82/82. Tổng **259/259**. Kết quả tóm tắt trong tests-v1.4.json.
- Cả xe máy, ô tô và xe tải cùng chiều tự phanh gấp, sáng đèn phanh và tăng tốc lại. Kiểm tra tốc độ không đổi thành chạy lùi khi phanh, trạng thái đèn và pause/resume.
- Cả ba loại xe có thể tạt làn lúc chạy cạnh người chơi; hướng thân xe đổi theo chuyển động. Thử người chơi giữ nguyên làn khi xe máy thực sự chạy song song tạt vào: gây thua. Xe tải có thời gian đổi làn lâu hơn xe máy.
- Khoảng 75% xe giao lộ chọn rẽ, còn lại đi thẳng. Đã thử đủ 8 tổ hợp xe máy/ô tô, hai hướng xuất phát và hai hướng rẽ. Cung rẽ liên tục, chờ đúng thời gian xuất phát, nhập vào đúng chiều làn.
- Xe chưa bị đạp vẫn va chạm lẫn nhau. Kiểm tra đối đầu, tông đuôi xe phanh gấp, đâm ngang, va chạm lệch tâm, xe tải húc xe máy, xe chạy nhanh xuyên qua nhau trong một khung hình và xe bay phía trên không gây va chạm giả.
- Mô hình va chạm gameplay đơn giản hóa: khối lượng xe máy 220 kg, ô tô 1.350 kg, xe giao hàng 2.200 kg, xe tải cỡ trung 4.500 kg. Xung lực pháp tuyến, ma sát tiếp tuyến, mô men quán tính, độ nảy 0,18; thân xe quay và trượt, giảm xoay và giảm vận tốc do ma sát, có trọng lực và va chạm tường nhà.
- Kiểm tra bảo toàn động lượng ngang; năng lượng chuyển động giảm sau va chạm; tổng năng lượng ngang và xoay yaw không tăng trong va chạm lệch tâm. Xe máy không tạo lực hất xe tải lên trời. Hai xe chạm nhau không liên tục nhận thêm lực mỗi frame.
- Xe bị đạp vẫn gây dây chuyền; phản lực ô tô giờ tính theo khối lượng và vận tốc thay cho lực hất cố định. Credit chỉ truyền từ va chạm do người chơi; giao thông tự gây tai nạn không gọi CSGT. Đạp quái xế vẫn là tự vệ không gọi CSGT; đạp xe thường vẫn truy đuổi và có cảnh nhà tù.
- Chạy tích hợp 90 giây bằng bước 0,02 giây, bật đủ tính năng mới và miễn va chạm người chơi để quan sát liên tục: có hơn 5 lần phanh, tạt làn, rẽ và va chạm; hơn 2.000 m đường, ít nhất 4 sự kiện quái xế, không có tọa độ/vận tốc NaN hay Infinity; pool vẫn có xe đang chạy.
- Xe tái sử dụng sau khi đâm nhà xóa trạng thái khóa tường, bị đạp, vật lý, đèn phanh và các timer. Xác nhận xe xuất hiện lại di chuyển bình thường, không tính nhầm hành động người chơi.
- Pool hiện có 28 xe: 26 xe giao thông thường (gồm 2 xe tải cỡ trung) và 2 xe sự kiện riêng.

Đã xem ảnh kiểm tra desktop 1440×900 và mobile 540×960: 25-sudden-braking.png, 26-intersection-turns.png, 27-traffic-collision.png, 28-cut-in-mobile.png. Các cảnh chụp được đặt có kiểm soát. Chưa thử trên điện thoại thật.

Windows cuối: build_b383849902bc, Succeeded, 0 lỗi, 1 cảnh báo dự kiến về Pipeline điều khiển từ xa không bật trong Player; khoảng 7,47 giây, 136.484.204 byte. Assembly của bản build có các hàm BeginSuddenBrake, TickIntersectionVehicle và ResolveVehicleCollisions. Khởi động 5 giây bằng -batchmode -nographics: tiến trình tiếp tục chạy, không thấy exception/runtime error trong windows-v1.4-smoke.log. Kiểm tra hình ảnh thực hiện trong Editor.

Bàn giao: Unity Play Mode, màn hình Ready, Time.timeScale=1, trafficSurprises=true, phiên bản 1.4.0, pool 28 xe, sự kiện quái xế đầu tiên giây 20. Kỷ lục 1775 m giữ nguyên. Bản chạy Builds/Windows/PhoViet.exe kèm thư mục dữ liệu và DLL.
