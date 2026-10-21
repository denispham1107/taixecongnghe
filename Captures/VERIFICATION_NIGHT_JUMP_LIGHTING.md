# Phố Việt 1.29.2 — đèn đường và đèn pha khi nhảy

## Thay đổi

- Đèn đường gần người chơi tăng cường độ từ 1,65 lên 3,6 và tầm chiếu từ 17 lên 20 m. Vùng sáng từ profile Blender tăng alpha từ 0,33 lên 0,44; mặt LED và quầng sáng rõ hơn. Giữ màu sáng trắng và các thông số bầu trời, ánh sáng môi trường, màu nhà cửa, chu kỳ ngày–đêm.
- Dành hai trong sáu nguồn sáng thật cho hai cột đèn gần nhất. Xe cộ đông không còn chiếm hết nguồn sáng của cột đèn. Đèn người chơi vẫn được ưu tiên; giữ giới hạn bốn đèn per-pixel và sáu đèn tổng cộng.
- `DayNightLighting` chạy sau cập nhật animation xe. Đèn pha thật lấy vị trí và trục hướng từ đèn gắn vào Frame/Steering, bao gồm độ cao nhảy, ngóc đầu, pitch lúc đáp và chạy trên nóc xe. Độ chúc đèn dùng trục xuống cục bộ của đèn thay vì trục xuống thế giới.
- `OpticalLight.shader` chiếu lại cookie Blender bằng ma trận thế giới → đèn ở từng khung hình. Profile gốc được dựng với đèn cao 1,1 m, vùng cookie x=[-16,16], z=[-4,28]. Khi xe ngóc đầu, ánh sáng rời vùng đường gần; khi xe hạ bánh, vùng sáng dịch về gần theo góc đèn. Cường độ giảm theo khoảng cách/góc tới, giới hạn 32 m. Vùng nhận sáng nằm trên mặt đường; không nâng một tấm sáng phẳng bay theo xe. Đèn spotlight thật tiếp tục chiếu lên hình học xe/nóc xe/đồ vật.
- Receiver xe đang đi bình thường giữ kích thước nhỏ để hạn chế overdraw trên điện thoại; chỉ mở rộng khi nguồn sáng cao/nghiêng. Chế độ shader mới chỉ bật qua MaterialPropertyBlock cho vệt đèn pha, không thay cách lấy UV của quầng đèn/đèn đường.
- Không sửa model hoặc các file Blender đang có thay đổi sẵn của người dùng.

## Kiểm tra đã thực hiện

- `VerifyNightJumpLighting`: 88 điều kiện đạt, kiểm tra đèn đường trong cảnh có tám xe, nguồn sáng/ma trận projector qua 180 bước nhảy, độ cao và hướng đèn, đáp xuống, chạy trên nóc xe tải và tắt đèn ban ngày.
- `VerifyDayNight`: 273; `VerifyJailLighting`: 7; `VerifyJump`: 44; `VerifyTruckJump`: 69 điều kiện đạt. Tổng hiện tại: 481 điều kiện.
- Đã xem ảnh Unity ngang và dọc ở các pha trên đường, ngóc đầu, đang bay, đỉnh nhảy, đáp và chạy trên nóc xe tải. 12 ảnh lưu dưới `night-jump-native-`.
- `VerifyHeadlightProjectionRender`: 8 điều kiện đạt bằng cách render shader thật lên RenderTexture. Vệt sáng khi ngóc đầu rời mặt đường; lúc chúc đầu thì tiến gần xe; nguồn cao trên nóc xe làm đổi vị trí/cường độ; xoay hướng 90° giữ đúng profile; ban ngày tắt hoàn toàn. Tổng cộng 489 điều kiện Unity/render đạt.
- Build WebGL `build_3795329b77de` thành công trong 261,809 giây, 0 lỗi, 3 cảnh báo cũ (Pipeline không bật trong Player, hai trường input chỉ dùng ngoài WebGL). Phiên bản 1.29.2, cache `124df654195facbf`.
- 8 kiểm tra WebGL thật đạt trên desktop và mobile mô phỏng: ngày/đêm, trở lại buổi sáng, không tràn canvas ở dọc/ngang và không có lỗi shader/JavaScript/Unity runtime. Đã xem ảnh đêm của bản WebGL. Kết quả `night-jump-web-visual-results.json`; chưa kiểm tra trên thiết bị iOS vật lý.

## Phát hành

- Phiên bản: 1.29.2. Trạng thái push/Pages sẽ được bổ sung sau khi triển khai.
