# Phố Việt 1.1.0 — Xe băng ngang giao lộ

Ngày kiểm tra: 17/10/2026. Unity 6000.5.6f1.

- Biên dịch: 0 lỗi, 0 cảnh báo.
- `AgentScripts/VerifyGameplay.cs`: 23/23 kiểm tra hồi quy đạt. Các kiểm tra riêng chuyển động dọc tắt lịch xe ngang để kiểm tra độc lập.
- `AgentScripts/VerifyCrossTraffic.cs`: 28/28 kiểm tra mới đạt.
- Xe ngang dùng đúng mẫu xe máy / ô tô, quay 90 hoặc 270 độ, xuất phát từ hai phía của đường nhánh.
- Xe giữ đúng tọa độ giao lộ khi thế giới cuộn; mỗi giao lộ chỉ lập lịch một lần. Số xe tăng theo độ khó: 1, 2, 3; hướng đầu tiên luân phiên giữa các giao lộ và các đợt đông có cả hai phía.
- Lịch nhả xe tính theo thời gian đến giao lộ và gia tốc người chơi. Xe trở về pool khi ra khỏi đường; đặt lại lượt xóa trạng thái xe ngang.
- Va chạm quét cả X và Z tương đối với tài xế, hoán đổi chiều dài/rộng cho xe quay ngang. Đã kiểm tra xe máy/ô tô đâm từ cả hai phía, tốc độ đủ vượt qua người chơi giữa hai khung hình, và các trường hợp đi gần nhưng không va chạm.
- Nhịp độ khó gấp đôi: 30 giây đạt 24,2 m/s; khoảng 54,2 giây đạt giới hạn 30 m/s; đợt xe đôi từ giây thứ 6; mật độ tăng theo cùng nhịp thời gian 2x.
- Thử trong Update thật: xe ngang không đổi vị trí và quãng đường không đổi khi tạm dừng; va chạm ngang chuyển sang Falling / GameOver và tài xế nghiêng -78 độ.
- Kỷ lục hiện có của người chơi được giữ nguyên khi chạy kiểm thử.
- Đã xem ảnh xe ngang tại giao lộ do bộ sinh tạo tự động: `09-cross-intersection.png`, `10-cross-road.png`. Ảnh `11-cross-crash.png` ghi màn hình thua sau va chạm ngang thật.
- Windows x64 1.1.0: build **Succeeded**, 0 lỗi, khoảng 34,4 giây. Cảnh báo duy nhất: Pipeline điều khiển từ xa không bật trong bản Player.
