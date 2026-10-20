# Phố Việt 1.26.0 — nhảy lên mọi nóc xe tải

## Yêu cầu đã xác nhận

- Tăng độ cao nhảy để tài xế cùng xe máy đáp lên nóc mọi loại xe tải, chạy trên nóc rồi tiếp đất.
- Mọi xe tải còn đứng thẳng đều có nóc sử dụng được, kể cả xe tải ngủ gật đang lạng lách hoặc xe đã va chạm nhưng còn đứng thẳng. Xe lật/nghiêng đổ vẫn là vật cản.
- Giữ SpaceBar nhảy trên desktop và vuốt lên trên mobile, hồi chiêu và các luật va chạm/ba mạng hiện có.

## Triển khai

- `PhoVietJump.cs`: vận tốc phóng tăng từ 12,4 lên 14,6 m/s, trọng lực vẫn 22 m/s². Đỉnh nhảy từ mặt đường đo được 4,844 m, trước đây khoảng 3,494 m. Thời gian bay dài hơn theo quỹ đạo, không dịch chuyển tức thì.
- Bỏ giới hạn nóc chỉ dành cho xe nhỏ. Xe tải thùng và xe tải ngủ gật có bề mặt thùng 3,175 m và cabin 2,71 m theo model Blender hiện có. Phần nóc tại vị trí bánh sau được giữ làm điểm tựa cho đến khi bánh ra khỏi mép.
- Khi rời thùng cao sang cabin thấp hoặc rời hẳn nóc, xe rơi theo trọng lực và dùng lại lựa chọn tiếp đất bánh trước/bánh sau/hai bánh. Không kéo xe xuống mặt thấp tức thì.
- Nóc đứng thẳng được phân biệt bằng góc nghiêng/pitch tối đa 12 độ; trạng thái đã va chạm không tự loại bỏ nóc xe tải nếu vẫn đứng thẳng. Khi xe đỡ bị nghiêng hoặc bị thu hồi khỏi pool, người chơi mất điểm tựa và rơi.
- Không tắt kiểm tra va chạm: nhảy quá muộn/không đủ cao vẫn đâm thân xe, mất một mạng và bị văng. Tăng tốc vẫn có thể kết hợp với nhảy.
- Pha `JumpAir` của animation Blender được lấy theo vận tốc thẳng đứng thay cho đồng hồ bay cố định 1,12 giây, để phù hợp cả quỹ đạo cao hơn và rơi từ nóc. Dùng lại năm action hiện có, không ghi đè các file Blender đang thay đổi từ trước.
- `PhoVietGame.cs`: camera đi lên nhẹ theo độ cao người chơi, giữ xe trong khung khi nhảy từ đường hoặc từ nóc xe tải.

## Kiểm tra

- `VerifyJump.cs`: 44 kiểm tra cũ cập nhật đỉnh nhảy và khoảng lấy đà theo quỹ đạo mới; vẫn kiểm tra xe ngã, bốn loại xe con/van, cú nhảy thực tế, tiếp đất, ba mạng và bắt giữ.
- `VerifyTruckJump.cs`: kiểm tra cú nhảy thực tế lên cả hai loại xe tải khi cùng chiều, đứng yên và ngược chiều; chạy khỏi nóc và đáp đường; nhảy tiếp từ nóc; xe tải ngủ gật lạng lách; xe đỡ bị lật/thu hồi; va thân xe; kết hợp tăng tốc ở bước thời gian 0,05 giây; tầm nhìn camera thường/truy đuổi trên bốn làn, cả đỉnh nhảy từ đường và từ nóc.
- 69 kiểm tra xe tải đều đạt ở từng khung ngang 1440×900 và dọc 540×960. Điểm tựa bánh sau được đổi theo hướng tương đối giữa hai xe, nên xe tải ngược chiều không dùng nhầm mép nóc phía trước. Chạy từ cabin thấp vào vách thùng cao gây va chạm, không tự kéo lên nóc.
- Tổng cộng 29 lượt bộ kiểm tra Unity, 2.090 kiểm tra đạt. Các bộ nhảy, xe tải, tăng tốc và va chạm người chơi được chạy lại sau tinh chỉnh điểm tựa cuối cùng.
- Đã xem ảnh `truck-jump-apex.png`, `truck-jump-roof.png`, `truck-jump-cab.png`, `truck-jump-road.png`: xe vượt nóc thùng, bánh đáp lên mặt nóc, rơi qua cabin và trở về đường.

## Phát hành

- WebGL 1.26.0: build `build_c86493759c6b` thành công trong 202,244 giây; 0 lỗi, cache `4b93e751d2cd718e`.
- Hai cảnh báo cũ: Pipeline không có runtime config (công cụ chỉ dùng trong Editor), và trường HUD touch native không dùng trong nhánh WebGL.
- 81 kiểm tra web đều đạt: 6 SpaceBar/vuốt lên, 6 W/vuốt xuống, 12 công việc/UI/chuột/touch/xoay, 37 WebGL/PWA/offline, 20 fullscreen/safe area theo profile iPhone/iPad dọc/ngang. Không có lỗi JavaScript hoặc Unity runtime.
- Profile điện thoại trong trình duyệt kiểm tra bố cục/điều khiển, không phải phép đo hiệu năng trên thiết bị vật lý.
- Unity kết thúc ở Edit Mode, Game View ngang; công việc bật, dữ liệu đọc lại vẫn 55 Coin và kỷ lục 1.775 m.
- Commit phát hành: `09af0babe5aaf07a97380b847b0daffafcf39242`.
- GitHub Pages triển khai thành công đúng commit: https://github.com/denispham1107/taixecongnghe/actions/runs/39465334366
- 26 kiểm tra live đều đạt: phiên bản/cache và từng tài nguyên khớp bản đã thử, WebAssembly thật khởi động, nhận thao tác bắt đầu/tạm dừng, không có lỗi JavaScript hoặc Unity runtime. Bằng chứng: `truck-jump-live-results.json`.
- Trang chơi: https://denispham1107.github.io/taixecongnghe/
- Server thử nội bộ đã dừng. Các file Blender thay đổi từ trước, bản sao lưu Motorcycle và `huongdan.md` được giữ riêng khỏi commit.
