# Kiểm tra bản Phố Việt đầu tiên

Ngày: 17/10/2026. Unity 6000.5.6f1, Pipeline 0.8.0-exp.1.

- Biên dịch C#: 0 lỗi, 0 cảnh báo.
- `AgentScripts/VerifyGameplay.cs`: **23/23** kiểm tra đạt trong Play Mode trên mã cuối.
- Đã kiểm tra đổi làn và giới hạn đường; chuyển làn mượt; bắt đầu, tạm dừng, tiếp tục và đặt lại lượt.
- Đã kiểm tra xe cùng chiều, ngược chiều, va chạm ô tô/xe máy, và không va chạm giả ở làn bên cạnh.
- Mô phỏng 120 giây: quãng đường tăng, tốc độ tăng đến giới hạn, 7 đoạn phố tái sử dụng không hở đoạn.
- Scene có đầy đủ 7 tên đường và các nhóm quán ăn / tạp hóa / bách hóa / sửa xe; 28 cột đèn giao thông; font tiếng Việt kèm giấy phép.
- Kiểm tra mesh sao: 11 đỉnh gồm tâm, đúng 5 đỉnh ngoài và 5 đỉnh lõm, 10 tam giác.
- Thử UI thực tế: bấm nút LÊN XE THÔI bắt đầu game; phím phải chuyển sang làn 3 tại x=4.65; phím P tạm dừng.
- Khi tạm dừng, số khung hình tăng từ 8316 đến 9790 nhưng quãng đường giữ nguyên 740.31024 m.
- Va chạm qua vòng Update thật: chuyển sang GameOver và xe nghiêng -78 độ (Euler z=282).
- Đã xem ảnh giao diện ngang 1440×900, dọc 540×960 và màn hình thua. Khung dọc được kiểm tra trong Game View; chưa thử trên điện thoại thật.
- Build Windows x64: **Succeeded**, 0 lỗi, 134.960.968 byte, khoảng 76,7 giây. Một cảnh báo cho biết Pipeline điều khiển từ xa không bật trong bản Player; đây là cấu hình phát hành hiện tại.
- Bản Windows đã được khởi động kiểm tra bằng `-batchmode -nographics`, tiến trình tiếp tục chạy và log không có lỗi runtime. Kiểm tra hình ảnh và điều khiển được thực hiện trong Editor, không phải trong lượt chạy headless này.

Ảnh bằng ScreenCapture của Unity gồm giao diện OnGUI. Ảnh `01-ready.png` là camera render ban đầu để chẩn đoán, trước khi sửa tỷ lệ chữ. Bản cuối dùng `06-portrait-final.png`, `07-gameover.png` và `08-final-ready.png`.

Các kịch bản kiểm tra nằm ngoài Assets để không đi vào bản game. Không có lỗi game trong console; một lỗi timeout MCP lúc dựng cảnh được đối chiếu với scene và tài sản đã lưu thành công.
