# Phố Việt 1.2.0 — Đạp xe, dây chuyền, truy đuổi

Ngày: 17/10/2026. Unity 6000.5.6f1.

- Biên dịch mã gameplay: 0 lỗi, 0 cảnh báo.
- `VerifyGameplay.cs`: 23/23 đạt.
- `VerifyCrossTraffic.cs`: 28/28 đạt, đã cập nhật mốc tăng độ khó 1.2.0.
- `VerifyPursuit.cs`: 39/39 đạt. Tổng cộng **90/90**.
- Nhịp độ khó 3x bản gốc, bằng 1,5x bản 1.1: gia tốc 0,36 m/s mỗi giây, tốc độ tối đa đường thường sau khoảng 36,1 giây, xe đôi sau 4 giây.
- Đã thử cú đạp trái/phải, chọn đúng xe song song cùng chiều, bỏ qua xe hơi/xe ngược chiều/xe quá xa, hồi chiêu, hoạt ảnh chân đúng phía.
- Xe máy đổ làm xe máy khác đổ; ô tô đâm vào xe ngã bị hất lên khỏi mặt đất. Kiểm tra hướng lan truyền và không đếm lặp xe đã ngã.
- Double tap: hai tap gần nhau trong 0,32 giây kích hoạt; tap đơn, tap cách xa, tap trên nút điều khiển không kích hoạt. Đã thử handler của mobile trong Editor; chưa thử trên thiết bị điện thoại thật.
- Input desktop thực tế bằng Computer Use: nhấn Space trong Game View làm số cú đạp từ 0 lên 1 và IsChased=true.
- Mô tô CSGT tiếp cận nhanh, sau đó tốc độ người chơi hơn 0,65 m/s khi đường trống. Tốc độ vẫn tăng trong truy đuổi; cú đạp tiếp không đặt lại khoảng cách.
- Va chạm trong Update thật làm khoảng cách CSGT giảm từ 6,38 m xuống 2,08 m, sau đó chuyển sang Jailed và bật buồng giam. Đã kiểm tra nút chơi lại khôi phục phố, trạng thái xe, camera và tắt truy đuổi.
- Đã xem ảnh ngang/dọc của truy đuổi và buồng giam: áo tù sọc đen trắng, nhân vật ngồi trên bệ, song sắt, đồng phục CSGT vàng cát, mũ, cầu vai, mô tô trắng và đèn ưu tiên.
- Kỷ lục đang có của người chơi được giữ nguyên.

Các script kiểm tra nằm trong AgentScripts, không đưa vào bản game. Các cảnh mới và mesh/vật liệu đã lưu trong scene/tài sản của project.

Windows 1.2.0: build **Succeeded**, 0 lỗi, khoảng 12,9 giây. Một cảnh báo cho biết Pipeline điều khiển từ xa không bật trong Player. Kiểm tra khởi động bằng `-batchmode -nographics` tiếp tục chạy, không có lỗi runtime trong log; hình ảnh và điều khiển được kiểm tra trong Editor. Ảnh `20-chain-reaction.png` ghi va chạm dây chuyền với ô tô ở độ cao 1,10 m; `19-jail-final.png` ghi cảnh nhà tù cuối.
