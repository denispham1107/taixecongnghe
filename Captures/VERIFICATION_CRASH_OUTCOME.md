# Bỏ ba mạng; tai nạn kết thúc chuyến hoặc chờ CSGT — 1.28.0

## Quy tắc đã xác nhận

- Không có bộ đếm ba mạng và không hiển thị mạng trên HUD dọc/ngang.
- Va chạm khi không bị truy đuổi: giữ chuyển động văng/té theo va chạm, kết thúc chuyến sau khi trượt và té xong. Không tự dựng xe.
- Va chạm khi đang bị CSGT đuổi: người chơi nằm lại, không game over và không tự dựng xe. Nếu CSGT đang té/bị cản, chờ cảnh sát dựng xe/né vật cản rồi đến bắt. Chuyển sang cảnh tù sau khi bị bắt.
- CSGT vẫn hồi phục trong 1,2 giây; xe giao thông khác vẫn giữ luật dựng xe sau ba giây và ngoại lệ xe cháy tại tường.

## Code

- `PhoVietRiderCrash.cs`: bỏ `LivesRemaining`, trạng thái hồi phục và toàn bộ đường tự dựng/nhập làn/chạy tiếp. `CrashDuringPursuit` lưu tình trạng truy đuổi lúc va chạm; `RiderFallComplete` ghi nhận hoàn tất té. Vẫn dùng xung lực, ma sát, va chạm phụ và hướng/vận tốc thực. Sau khi nằm yên 0,7 giây và hoạt cảnh té đã tiến triển, chuyển GameOver nếu tai nạn thường; nếu có truy đuổi thì tiếp tục Falling để chờ.
- `PhoVietActions.cs`: cảnh sát đến gần chỉ bắt người đang té sau khi hoạt cảnh té đã hoàn tất; bắt người còn đang chạy vẫn như cũ. Cảnh tù thêm dòng “CSGT ĐÃ ĐƯA BẠN VÀO TÙ”.
- `TickCrashPolice`: thêm các hướng thăm dò vòng xe đang dừng, giữ hướng vòng ưu tiên; kiểm tra đường quét trước mỗi bước, không dịch chuyển xuyên xe. Nếu chưa có đường thì dừng/chờ. Cơ chế ngã và dựng xe 1,2 giây giữ nguyên.
- `BikeVisualAnimator.cs`: bỏ animation dựng xe của người chơi; giữ tư thế té khi hết chuyến hoặc đang bị bắt sau tai nạn. Animation hồi phục của NPC/CSGT vẫn còn.
- Bỏ enum Recovering của người chơi, timer nhường xe sau hồi phục và phần rebase chỉ dùng để chạy tiếp sau tai nạn. Các timer nhường xe khi nhận việc/đón/trả khách vẫn giữ.
- `PhoVietPortraitHud.cs`, `PhoVietLandscapeHud.cs`, màn đầu/kết thúc và hướng dẫn PWA: không còn số mạng hoặc nội dung ba mạng. Thông báo chờ: “ĐÃ TÉ · CHỜ CSGT ĐẾN BẮT”. Giữ góc nhìn dọc đã nới và cử chỉ tức thời từ các bản trước.

## Kiểm tra

- 31 lượt bộ kiểm tra Unity, 2.194 kiểm tra đạt. `VerifyCrashOutcome` mới có 51 kiểm tra, chạy cả ngang/dọc: lần va chạm đầu kết thúc sau té, lực/hướng va chạm, giữ tư thế té, pause, không điều khiển khi nằm chờ, CSGT té 1,2 giây, vòng xe dừng không xuyên/teleport, bắt sau khi té hoàn tất, tai nạn đang ôm cua, chuyển tù, chơi lại và không trả Coin cho chuyến chưa xong.
- Các bộ cũ được đổi từ assert số mạng sang trạng thái thực (Riding/Falling/GameOver). `VerifyRiderLives` và capture hồi phục người chơi được thay bằng các bộ theo luật mới.
- Một fixture cũ của VerifyJump lấy xe tải không hoạt động từ pool nhưng còn góc đổ của kiểm tra trước. Đã đặt về tư thế đứng thẳng đúng tên test trước khi kiểm tra độ cao nóc; không sửa luật nóc/va chạm của runtime để làm test đạt.
- Đã xem ảnh HUD, chờ CSGT và cảnh tù ngang, cùng màn kết thúc dọc. Capture mới dùng xe băng ngang tông thật để tạo tư thế té; tám ảnh trong `crash-outcome-native-{wide,portrait}-*.png`.
- 69 kiểm tra trình duyệt đạt: công việc/HUD qua input thật 12, WebGL/PWA/offline 37, toàn màn hình iPhone/iPad dọc/ngang 20. Thêm 10 bước tải/chụp engine WebGL thật đạt, không lỗi runtime; đã xem ảnh desktop GameOver, mobile HUD không còn mạng và mobile vào tù. Các ảnh WebGL xác nhận trực quan kết quả riêng, không dùng số bước chụp thay cho assert trạng thái trong bộ native.
- Chưa kiểm tra trên thiết bị iOS vật lý.

## Phát hành

- WebGL 1.28.0 build thành công (`build_87eb3ca57d12`), 211,148 giây, 0 lỗi, cache `f38472ef1f17d268`. Ba cảnh báo không chặn build giữ nguyên: Pipeline không có runtime config (công cụ Editor), các biến native dragging/hudTouchChoice không dùng trong nhánh WebGL nhận chạm bằng DOM.
- Đã xác minh lại sau khi tiếp tục phiên làm việc: bản build và toàn bộ kết quả kiểm tra vẫn còn nguyên; Unity ở Edit Mode, công việc bật, giữ 55 Coin và kỷ lục 1.775 m.
- Lần push đầu lỗi HTTP 408; đã kiểm tra remote SHA vẫn là bản cũ trước khi thử lại. Đẩy lại bằng HTTP/1.1 và postBuffer lớn thành công; không force push, không build lại hoặc ghi đè model Blender đang sửa sẵn.
- Bản phát hành `22a50e30396f6810d0b16e49ca1176a3fa2199a3`. GitHub Pages thành công: https://github.com/denispham1107/taixecongnghe/actions/runs/39572194447.
- 26 kiểm tra bản công khai đạt: phiên bản/cache và toàn bộ tài nguyên khớp bản đã kiểm tra, Unity WebAssembly thật khởi động/nhận input, không lỗi JavaScript/Unity runtime. Kết quả `crash-outcome-live-results.json`.
- Trang chơi: https://denispham1107.github.io/taixecongnghe/. Không còn server kiểm tra nội bộ trên cổng 8765. Đóng hẳn rồi mở lại PWA để nhận bản 1.28.0; không cần xóa dữ liệu Coin/kỷ lục.
