# Phố Việt 1.29.0 — ngày/đêm và màn hình mở đầu

## Nội dung

- Bỏ ánh nắng vàng chiều; dùng nắng trắng, bầu trời xanh sáng, phản xạ và vật liệu kiến trúc trung tính. Giữ màu biển hiệu, cờ và cây xanh.
- Mỗi chu kỳ 60 giây: ngày → chạng vạng → đêm trong 30 giây, rồi đêm → bình minh → ngày trong 30 giây. Nội suy cosine liên tục; tạm dừng/chọn việc dừng đồng hồ. Lượt mới bắt đầu vào ban ngày.
- Đèn trước, đèn hậu của xe người chơi, xe giao thông và CSGT bật dần theo độ tối. Đèn đường LED có thấu kính, quầng sáng và vùng chiếu xuống đường. Đèn bám bộ phận xe đang được animation; tắt khi xe trong pool không hoạt động.
- Giữ hướng nắng, trăng và bầu trời khi hệ tọa độ phố chuyển sau một cú rẽ. Cảnh tù dùng ánh sáng riêng để nhân vật vẫn nhìn rõ.
- Thẻ mở đầu trắng trong, chữ xanh đậm, điểm nhấn/nút đỏ, tiêu đề và mô tả gọn hơn. Hỗ trợ nút bắt đầu qua cầu nối touch của WebGL; giữ keyboard Enter.

## Blender MCP và Unity

Blender MCP được dùng trực tiếp để dựng nguồn mới `ArtSource/Blender/PhoViet_DayNight.blend` bằng `pho_viet_day_night.py`. Nguồn có mô hình đèn LED ba LOD, studio ánh sáng có keyframe ngày/đêm, kết xuất vùng chiếu đèn pha/đèn đường và hồ sơ màu. Quầng sáng quang học mềm được tạo bằng Blender image API. Không ghi đè các file Blender xe/khách/phố đang được người dùng chỉnh.

- `PhoVietLightingUpgrade.cs`: nhập FBX/textures, thay đèn cũ trên 35 khối phố bằng 70 bộ LED, gắn 30 bộ đèn xe, lắp material/skybox, lưu scene.
- `DayNightLighting.cs` / `PhoVietDayNight.cs`: đồng hồ gameplay, màu nắng/trăng/ambient/fog/sky, emission và đèn tự động. Vật liệu phát sáng được clone trong Play Mode để không sửa asset khi chạy.
- `DayNightSky.shader`: trời ngày/đêm, mặt trời/trăng, sao nhẹ. `OpticalLight.shader`: vùng chiếu và quầng sáng mềm, có độ sâu che khuất và sương mù.
- Tối đa 6 nguồn sáng thực ở gần người chơi, ngân sách 4 đèn per-pixel; ưu tiên đèn pha người chơi. Các nguồn xa dùng vùng chiếu đã dựng trong Blender; đèn đường có 3 LOD. Đây là kết hợp ánh sáng thực và profile quang học tối ưu cho WebGL/mobile.
- `PhoVietWelcome.cs` cùng `PhoVietWebRuntime.cs`: bố cục nhỏ gọn và hit test đúng nút mới trên mobile.

## Kiểm tra đã chạy

- `VerifyDayNight`: 273 điều kiện ở 1440×900 và 273 điều kiện ở 540×960: mốc 0/30/60, nhiều chu kỳ, pause/chọn việc, toàn bộ fixture, LOD, emission, pooling, bốn làn rẽ/rebase, reset, kích thước/nút mở đầu.
- 14 bộ hồi quy: xe máy, ô tô, phố, rẽ mượt, công việc, GPS, kết quả va chạm, HUD touch, cử chỉ, tăng tốc, nhảy, nhảy nóc xe tải, truy đuổi, cảnh báo sau/đón trả khách: 1.615 điều kiện.
- HUD dọc 21, camera mobile 8, safe area 9. Tổng native ban đầu: 2.199 điều kiện.
- Kiểm tra bổ sung cảnh tù tái hiện vệt sáng tách rời còn bật sau chuyển cảnh. Đã tắt toàn bộ optics khi vào tù; `VerifyJailLighting` 7 điều kiện đạt. Chạy lại `VerifyDayNight` 273 và `VerifyCrashOutcome` 51 sau sửa đều đạt. Tổng các điều kiện trong bộ chính và bổ sung: 2.206 (các lượt chạy lại được lưu riêng).
- Đã xem ảnh Unity ban ngày/đêm và màn hình mở đầu ở desktop và mobile dọc. Ảnh nguồn studio Blender là kiểm tra nguồn quang học, không thay cho ảnh game.

## Web và phát hành

- Build đầu `build_f9481fdb7f52` thành công trong 394,780 giây, 0 lỗi, cache `dd7d0a81a7633aa5`. Bốn cảnh báo không chặn build; cảnh báo API reflection texture cũ đã được sửa trong bản cuối, ba cảnh báo còn lại thuộc cấu hình công cụ Editor và hai trường input không dùng trong nhánh WebGL.
- 77 điều kiện web đạt trên cache đầu: hình ảnh ngày/đêm và mở đầu thực tế 8; chọn việc/HUD bằng chuột và chạm 12; WebGL/PWA/offline 37; toàn màn hình iPhone/iPad dọc/ngang mô phỏng 20. Đã xem ảnh WebGL desktop, mobile dọc và mobile ngang.
- Một lượt đầu bị timeout 30 giây khi Playwright chờ hai frame ổn định của nút “Cách chơi” trên software renderer. Không có lỗi runtime; tăng thời gian chờ lên 90 giây và chạy lại đủ bộ 37 điều kiện đã đạt. Lưu chẩn đoán riêng, không bỏ qua kiểm tra input.
- Build cuối `build_f2c1ab83fda6` thành công trong 274,508 giây, 0 lỗi, 3 cảnh báo cũ không chặn build. Phiên bản 1.29.0, cache `ca54198ddf4a4c9b`. Bản cuối bổ sung việc tắt optics trong cảnh tù và dùng API reflection texture hiện hành; các bộ native liên quan đã chạy lại đạt như trên. Bộ web 77 điều kiện chạy trên cache đầu có cùng giao diện và chu kỳ ánh sáng; kết quả kiểm tra riêng bản công khai cuối được lưu sau phát hành.

Các profile trình duyệt mobile kiểm tra bố cục và touch emulation; chưa phải kiểm tra trực tiếp Safari trên iPhone/iPad thật.
