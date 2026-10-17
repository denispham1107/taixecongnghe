# Phố Việt 1.5.0 — Xe tải ngủ gật và quái xế ở làn bên

Ngày: 18/10/2026 (Asia/Bangkok). Unity 6000.5.6f1.

- Đã xóa trường nextRecklessAt, thuộc tính NextRecklessAt và lịch TickRecklessSchedule. Xe nguy hiểm tham gia các đợt sinh xe thường, xác suất 75% tăng tới 90% theo độ khó khi có mẫu xe đang rảnh. Không còn chờ 20 giây hay chu kỳ 15–20 giây.
- Xe xuất phát ở làn kề bên làn hiện tại của người chơi, chạy cùng chiều, lạng qua làn người chơi. Đã kiểm tra đủ bốn làn, hướng lạng, hướng thân xe và kích thước va chạm của xe máy/xe tải.
- Xe tải nguy hiểm có tên "Xe tải ngủ gật", biển "XE TẢI NGỦ GẬT" và cảnh báo cùng tên trên HUD. Quái xế có biển riêng. Xe tải thường vẫn có thể phân biệt với xe ngủ gật.
- Không sinh xe nguy hiểm chồng lên xe khác; không dịch chuyển một xe đang hoạt động để tạo lần xuất hiện mới. Hai loại có thể cùng hiện diện. Xe có thể húc giao thông khác hoặc lao vào nhà hai bên; khói/lửa và va chạm theo khối lượng vẫn hoạt động.
- Giữ nguyên đạp xe ngược chiều, tự vệ trước quái xế không gọi CSGT, truy đuổi sau khi đạp xe thường, phanh gấp, tạt làn và xe rẽ giao lộ.
- Biên dịch: 0 lỗi, 0 cảnh báo. VerifyGameplay 23, VerifyCrossTraffic 28, VerifyPursuit 39, VerifyReckless 92, VerifyTrafficDynamics 82: **264/264** kiểm tra đạt. Có bài chạy tích hợp 90 giây bật đủ giao thông mới.
- Đã xem ảnh desktop 29-drowsy-truck.png để xác nhận biển tên và cảnh báo trong góc nhìn chơi. Chưa thử trên điện thoại thật.

Kết quả kiểm thử tóm tắt: tests-v1.5.json. Các báo cáo phiên bản trước là lịch sử, cơ chế 20 giây đã được thay thế ở bản này.

Windows: build_93225bc2ca6f, Succeeded, 0 lỗi, 1 cảnh báo dự kiến về Pipeline điều khiển từ xa không bật trong Player; 11,83 giây. Khởi động 5 giây với -batchmode -nographics: tiến trình tiếp tục chạy, không thấy exception/runtime error trong windows-v1.5-smoke.log. Hình ảnh được kiểm tra trong Editor.

Bàn giao: Unity Play Mode ở màn hình Ready, Time.timeScale=1, recklessTrafficEnabled=true, phiên bản 1.5.0. Tên model xe tải là "Xe tải ngủ gật". Kỷ lục 1775 m được giữ nguyên. Bản chạy Builds/Windows/PhoViet.exe kèm thư mục dữ liệu/DLL.
