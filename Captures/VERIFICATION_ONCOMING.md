# Phố Việt 1.6.0 — Bổ sung xe nguy hiểm ngược chiều

Ngày: 18/10/2026 (Asia/Bangkok). Unity 6000.5.6f1.

- Giữ xe nguy hiểm cùng chiều ở làn bên; bổ sung quái xế và xe tải ngủ gật từ hai làn ngược chiều. Luồng sinh xe thường chọn ngẫu nhiên cả hai hướng, không có lịch 20 giây / 15–20 giây.
- Xe ngược chiều có vận tốc âm trên trục đường, thân xe quay đúng hướng đối diện người chơi; biên độ lạng đủ lớn để vượt vạch giữa đường sang các làn phải. Xe vẫn có thể húc xe máy/ô tô và lao vào nhà bên trái hoặc phải.
- Đã kiểm tra chuyển động tự lao vào cả hai dãy nhà cho cả xe máy và xe tải ngược chiều; cả bốn tình huống đều bật khói/lửa. Va vào xe nguy hiểm gây thua; đạp quái xế ngược chiều ở cả hai phía vẫn tự vệ không gọi CSGT.
- Biển tên quái xế và XE TẢI NGỦ GẬT có hai mặt, đọc được khi chạy đối diện. HUD cảnh báo rõ ngược chiều lấn làn. Xem ảnh 30-oncoming-invades.png và 31-oncoming-house-fire.png; cảnh chụp được đặt có kiểm soát.
- Biên dịch: 0 lỗi, 0 cảnh báo. VerifyGameplay 23, VerifyCrossTraffic 28, VerifyPursuit 39, VerifyReckless 116, VerifyTrafficDynamics 82: **288/288** kiểm tra đạt. Kết quả trong tests-v1.6.json.
- Bài tích hợp 90 giây giữ đủ phanh gấp, tạt làn, xe rẽ và va chạm theo khối lượng. Không đổi cơ chế tự vệ/truy đuổi hoặc dữ liệu kỷ lục.

Windows cuối: build_11f9a67ae6df, Succeeded, 0 lỗi, 1 cảnh báo dự kiến Pipeline điều khiển từ xa không bật trong Player; 10,59 giây. Chạy khởi động 5 giây với -batchmode -nographics không thoát sớm và không có exception/runtime error trong windows-v1.6-smoke.log. Hình ảnh được kiểm tra trong Editor.

Bàn giao: Unity Play Mode ở màn hình Ready, Time.timeScale=1, recklessTrafficEnabled=true, phiên bản 1.6.0, kỷ lục 1775 m giữ nguyên. Bản chạy Builds/Windows/PhoViet.exe kèm thư mục dữ liệu/DLL.
