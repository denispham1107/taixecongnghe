# Phố Việt 1.22.0 — cảnh báo xe thường vượt từ phía sau

## Phạm vi được xác nhận

Chỉ cảnh báo xe thường đi từ dưới màn hình lên, trước hai giây ở đúng làn, dùng lại mũi tên đỏ viền vàng. Xe từ phía trên và xe băng ngang giữ hành vi hiện tại.

## Triển khai

- `PhoVietExitTraffic.cs`: giữ một xe chờ riêng cho mỗi làn cùng chiều, cùng đồng hồ cảnh báo hai giây. Số xe xuất hiện chỉ tăng khi xe thực sự được tạo. Nếu khoảng trống phía sau bị xe hoặc CSGT chiếm, giữ xe và mũi tên tại làn đã báo, thử lại khi an toàn.
- `PhoVietGame.cs`: cập nhật countdown trong thời gian chạy; các đợt xe thường và xe giao lộ không được dùng nhầm model đang dành cho một cảnh báo phía sau.
- `PhoVietRearWarning.cs`: vẽ chung một biểu tượng trên mỗi làn có xe thường hoặc xe nguy hiểm sắp vượt lên, tránh chồng hai biểu tượng trên cùng làn. Vẫn bám vị trí làn qua camera và safe area, dùng lại màu đỏ, cạnh vàng, nền tối và quầng nhấp nháy.
- Khi camera theo tài xế ở sát lề trái, cả hai làn cùng chiều có thể nằm ngoài mép phải màn hình. Các chỉ báo được giãn cách tại mép đó, giữ thứ tự trái/phải và không vẽ chồng lên nhau.
- Sau cua, tạo hàng chờ trên đường mới với đầy đủ hai giây. Trong lúc rẽ hoặc đón/trả khách, không tiêu thụ countdown; yêu cầu lại đủ hai giây khi tiếp tục. Pause đóng băng countdown; restart hoặc tắt exit traffic giải phóng các xe chờ.
- Giữ số xe dự kiến của đợt sau cua; tăng thời hạn lập đợt từ 7 lên 12 giây để các xe có đủ thời gian báo hai giây trước khi vào đường. Đây không phải tăng mật độ xe.

## Kiểm tra

- 137 kiểm tra mới (gồm 16 kiểm tra giãn cách chỉ báo ở bốn làn và hai camera): xe máy, ô tô và xe tải thường; đủ hai giây; vị trí chính xác; giữ model chờ không bị đợt khác lấy; hai làn độc lập; không chồng hàng chờ; pause, rẽ, khách xuống xe, restart; không chuyển sang làn khác khi điểm xuất hiện bị chiếm; không báo mũi tên dưới cho xe ngược chiều.
- Chạy lại 137 kiểm tra của bộ cảnh báo ở màn hình dọc; đều đạt. Các kiểm tra sau giao lộ được cập nhật để xe phía sau phải cảnh báo trước thay vì xuất hiện ngay.
- 1,812 kiểm tra đạt trong 24 bộ kiểm tra Unity, bao gồm toàn bộ hồi quy giao thông, CSGT, va chạm/ba mạng, rẽ, công việc/GPS, art, camera, safe area và HUD.
- Đã xem sáu ảnh: một làn/hai làn/sát lề trái trên màn hình dọc và ngang. Mũi tên đúng vị trí, không chiếm HUD phía trên.
- Sửa phép đo test lạng lách cũ để tính cả điểm đầu của quỹ đạo; trước đây test có thể bỏ sót điểm đầu nằm đúng làn người chơi. Không thay đổi chuyển động quái xế.

## Phát hành

- WebGL 1.22.0 cuối cùng: build `build_7c94ce9e0885` thành công trong 166.965 giây, 0 lỗi; cache `66c809085e9bc16a`.
- Hai cảnh báo cũ: Pipeline chỉ dùng trong Editor, và trường native HUD touch không dùng trong nhánh WebGL. Không có lỗi shader/build.
- 69 kiểm tra web trên bản cuối đều đạt: 12 kiểm tra UI/chuột/touch/xoay, 37 kiểm tra WebGL/PWA/offline/điều khiển, 20 kiểm tra full viewport/safe area iPhone/iPad dọc/ngang. Không có lỗi JavaScript/Unity runtime.
- Commit phát hành: `d540734e92997843294ab3b3f60bd8e60b61f713`.
- GitHub Pages triển khai thành công: https://github.com/denispham1107/taixecongnghe/actions/runs/39354100972
- 26 kiểm tra live đạt: đúng phiên bản/cache, toàn bộ tài nguyên khớp bản kiểm tra, WebAssembly khởi động và nhận điều khiển, không có lỗi JavaScript/Unity runtime.
- Trang chơi: https://denispham1107.github.io/taixecongnghe/
- Unity kết thúc ở Edit Mode, Game View ngang; công việc bật, Coin thử nghiệm 0, kỷ lục 1,775 m. Server nội bộ đã dừng.
- Kiểm tra thiết bị bằng profile trình duyệt là mô phỏng bố cục, không phải đo hiệu năng trên iPhone vật lý.

Các tệp Blender và `huongdan.md` có thay đổi từ trước được giữ riêng.
