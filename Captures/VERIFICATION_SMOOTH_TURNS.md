# Rẽ trái/phải liền mạch, giữ tốc độ — 1.28.1

## Thay đổi

- `PhoVietGame.TickRide`: bỏ giới hạn 8/10 m/s trong cua và giới hạn tốc độ theo quãng đường còn lại ở khung hình cuối. Dùng cùng tốc độ hành trình, tăng tốc và phần tốc độ tiến khi đang nhảy cho cả đường thẳng và cua. Các quy tắc dừng đón/trả khách và nhường xe khi nhận việc vẫn áp dụng.
- `PhoVietRoutes.BeginOrContinueTurn`: chia đúng phần quãng đường đi thẳng trước vạch bắt đầu cua và phần đã đi vào cua trong cùng một khung hình. Khi ra cua, giữ phần quãng đường còn dư và đi tiếp theo tiếp tuyến của đường đích trước khi đổi hệ tọa độ; không hãm xe về điểm cuối cung.
- Cua đã chọn từ trước dùng đường Bézier bậc năm, tiếp tuyến khớp hướng hai con đường và độ cong về 0 tại hai đầu. Bảng độ dài cung 128 đoạn giữ vận tốc theo quãng đường thay vì theo tham số đường cong. Rẽ muộn trong giao lộ vẫn dùng đường bậc ba nằm trong mặt đường đã kiểm tra, cũng chạy bằng bảng độ dài cung và giữ phần dư cuối khung hình.
- Giữ phép biến đổi chung cho xe, đường, camera, xe giao thông và CSGT khi sang đường mới. Không đổi số thứ tự làn, địa chỉ khách, tên phố hay luật va chạm. Camera theo độ cao nhảy nhất quán trước/trong/sau cua; tốc độ bám vị trí/góc quay trong và ngay sau cua tăng theo vận tốc để xe boost không chạy khỏi khung hình. Tách bước cập nhật camera nhận delta thời gian để kiểm tra đúng tốc độ khung hình.
- Lưu sẵn các TextMesh bảng tên đường của từng hàng phố, không quét lại toàn bộ cây đối tượng lúc chuyển đường; chỉ gán chữ khi tên thực sự thay đổi. Đây là cache runtime không serialize vào scene.
- Xóa độ nghiêng đổi làn còn dư (`lean=0`) tại điểm kết thúc cua. Nếu chỉ đặt rotation thẳng mà không xóa biến này, khung hình chạy thẳng tiếp theo sẽ nghiêng xe lại rồi mới từ từ về 0, gây thêm một nhịp lắc.

## Kiểm tra

- `VerifySmoothTurns`: tốc độ và quãng đường từng khung hình ở bốn làn, rẽ sớm và hai vị trí rẽ muộn, tốc độ đầu lượt/tối đa/boost, bước thời gian 1/60 và 0,05 giây. Đo chuyển động so với một tile đường cố định xuyên qua lúc rebase; kiểm tra phần dư 0,73 m và phép chiếu camera/xe trước và sau chuyển hệ tọa độ.
- Kiểm tra camera có giữ xe trong tầm nhìn qua cua và sau khi ra cua ở tốc độ boost tối đa, cả hai bước thời gian; nhảy cùng boost, pause, va chạm trong cua và CSGT theo hành trình.
- 32 lượt bộ kiểm tra Unity, 3.079 kiểm tra đạt: 468 kiểm tra mới chạy cả ngang và dọc, cùng hồi quy đường phố, công việc/GPS, nhảy/boost, xe giao thông, cảnh sát, tai nạn, cử chỉ và art. Kiểm tra camera theo delta thực phát hiện trường hợp rẽ muộn khi boost làm xe lọt khỏi mép hình; sau khi sửa tốc độ bám camera, tất cả trường hợp đều đạt. Kiểm tra thêm góc xe ở khung hình đầu tiên sau cua đã tái hiện cú lắc trước khi đặt lại `lean`, và đạt sau sửa ở toàn bộ 72 tổ hợp mỗi hướng màn hình.
- Đã xem ảnh thực tế trong cua và vừa ra cua ở cả ngang/dọc. Tổng 12 ảnh `smooth-turns-{wide,portrait}-{0,3}-{curve,exit,straight}.png`. Unity trở về Edit Mode, Game View ngang; công việc bật, 55 Coin và kỷ lục 1.775 m được giữ nguyên.
- 69 kiểm tra trình duyệt đạt trên build cache `1b24495d356eb5f4`: chọn công việc/HUD bằng input thật 12, WebGL/PWA/offline 37, toàn màn hình và xoay iPhone/iPad mô phỏng 20. Game WebAssembly thật khởi động được khi ngắt mạng; không có lỗi JavaScript hoặc Unity runtime. Đã xem thêm ảnh HUD/GPS dọc của bản WebGL mới. Bản build cuối bổ sung duy nhất việc xóa độ nghiêng dư khi ra cua, đã kiểm tra lại bằng bộ native liên quan; kiểm tra bản công khai cuối được lưu riêng.
- Kết quả các bộ Unity, ảnh thực tế và kiểm tra web được lưu bằng tiền tố `smooth-turns-`. Kiểm tra trình duyệt mô phỏng không thay thế phép thử trên iPhone/iPad vật lý.

## Phát hành

- Bản build đầu 1.28.1 thành công (`build_b392dc84dd58`), 272,983 giây, 0 lỗi, cache `1b24495d356eb5f4`. Ba cảnh báo cũ không chặn build: Pipeline không có runtime config cho công cụ Editor, `dragging` và `hudTouchChoice` không dùng trong nhánh WebGL nhận cử chỉ bằng DOM.
- Bản đầu `bcb52e60dff5183063d88d68a63d37b38105ef60` đã push và GitHub Pages thành công: https://github.com/denispham1107/taixecongnghe/actions/runs/39580901914. 26 kiểm tra bản công khai đầu đạt, lưu ở `smooth-turns-initial-live-results.json`.
- Bản build cuối có sửa độ nghiêng dư thành công (`build_8035926eaf3c`), 181,579 giây, 0 lỗi, ba cảnh báo cũ như trên. Phiên bản 1.28.1, cache `d0d0e5b1e9e91d91`; đủ 20 tài nguyên precache.
- Bản cuối `212b3344236c66585f8490eeeee828026402a676` đã push. GitHub Pages thành công: https://github.com/denispham1107/taixecongnghe/actions/runs/39582390761.
- 26 kiểm tra bản công khai cuối đạt (`smooth-turns-live-results.json`): phiên bản/cache khớp, tất cả tài nguyên công khai khớp build cuối, Unity WebAssembly thật khởi động/nhận input, không có lỗi JavaScript hoặc Unity runtime.
- Đã xác minh Unity ở Edit Mode, không còn server thử trên cổng 8765. Giữ nguyên các file Blender và `huongdan.md` đang sửa sẵn, không đưa chúng vào commit của công việc này.
- Trang chơi: https://denispham1107.github.io/taixecongnghe/. Đóng hẳn rồi mở lại ứng dụng PWA để nhận bản mới; không cần xóa dữ liệu Coin/kỷ lục.
