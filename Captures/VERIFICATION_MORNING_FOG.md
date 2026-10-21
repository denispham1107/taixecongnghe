# Phố Việt 1.30.0 — sương mù buổi sớm trong sáu giây đầu

## Dựng trong Blender MCP

- Đã kết nối Blender MCP và dựng scene riêng `PV • Six second morning mist`, khối thể tích tham gia tán xạ ánh sáng, mật độ fBm có các vùng loang và giảm dần theo độ cao. Scene xem thử dùng nhà/cây Phố Việt, ánh sáng ban ngày và animation tan sương từ frame 1 đến 181 ở 30 fps.
- Xuất trường mật độ **32×32×32** thành atlas **272×136**, gồm 32 lát có padding và nối tuần hoàn ở hai phương ngang. File `Assets/PhoViet/Art/Fog/PV_MorningFogDensity.png` là dữ liệu tuyến tính, không phải ảnh phủ lên màn hình.
- Xuất `PV_MorningFog.json`: thời lượng sáu giây, mật độ 0,065, giảm mật độ theo độ cao 0,24, khoảng tích phân tối đa 100 m và 61 mẫu đường cong tan sương mượt. Gió nhẹ dịch trường mật độ trong lúc chơi.
- Nguồn: `ArtSource/Blender/pho_viet_morning_fog.py`, `ArtSource/Blender/PhoViet_MorningFog.blend`. Texture được pack vào file Blender để có thể chuyển dự án sang máy khác. Đã xem ảnh render đầu/cuối bằng Blender MCP; ảnh 0/3/6 giây lưu dưới `morning-fog-blender-`.
- Không ghi đè các file Blender xe/khách/phố/ngày đêm người dùng đang sửa.

## Tích hợp Unity

- `MorningFog` gắn vào camera, đọc profile/atlas Blender. `MorningFog.shader` tích phân mật độ dọc tia nhìn, lấy độ sâu cảnh thật, giảm mật độ theo độ cao và dùng truyền sáng theo độ dày sương. Cảnh xa bị che nhiều hơn cảnh gần; nhà và xe nằm trong sương, không dùng các tấm billboard trắng. HUD vẽ sau camera nên chữ/nút không bị hiệu ứng phủ lên.
- Sáu mẫu tích phân trên mỗi pixel; atlas nhỏ, không mip/không nén/không sRGB và giữ nguyên kích thước NPOT để không lệch lát. Jitter nhẹ giảm banding và hạt nhiễu.
- `DayNightLighting.ElapsedTime` giữ tổng thời gian gameplay riêng với `CycleTime` tuần hoàn 60 giây. Sương chỉ có trong sáu giây đầu mỗi lượt, không quay lại khi chu kỳ đêm chuyển sang ngày. Reset lượt mới khởi tạo lại sương; pause/chọn việc đóng băng thời gian; đón/trả khách vẫn là gameplay đang chạy nên sương tiếp tục tan.
- Trường mật độ bám theo tọa độ phố khi cuộn và được chuyển hệ tọa độ cùng các cảnh rẽ. Không bám vào màn hình hoặc giật về vị trí cũ khi camera đổi hướng. Cảnh trong tù tắt sương.
- Đúng giây thứ sáu, component hiệu ứng và yêu cầu depth bổ sung tắt, tránh tốn chi phí render trong phần còn lại của lượt. Giữ ánh sáng, bảng màu nhà, chu kỳ ngày–đêm và lớp haze xa vốn có.
- Scene `Assets/PhoViet/Scenes/PhoViet.unity` đã gắn đủ tham chiếu. Lệnh cài lại: `unity command run_script --file AgentScripts/InstallMorningFog.cs --entry InstallMorningFog.Main --json`; chạy lại lệnh này nếu dựng lại toàn bộ rig bằng `PhoVietLightingUpgrade`.

## Kiểm tra

- `VerifyMorningFog`: **87** điều kiện đạt, gồm mật độ giảm đơn điệu 0–6 giây, tắt đúng thời điểm, không lặp ở giây 60/120/600, pause/chọn việc/phục vụ khách, reset/cảnh tù, cuộn đường/rẽ 90° và so sánh pixel render thật. Pixel khi hết sáu giây khớp ảnh tắt hiệu ứng; sương tác động cảnh xa nhiều hơn cảnh gần.
- Hồi quy: `VerifyDayNight` **273**, `VerifyJailLighting` **7**, `VerifyNightJumpLighting` **88**, `VerifyServiceRearYield` **117**. Tổng **572 điều kiện Unity/render đạt**.
- Đã xem ảnh cuối ở khổ ngang 1440×900 và dọc 540×960; 14 ảnh ở giây 0/1,5/3/4,5/6/30/60 lưu dưới `morning-fog-native-`. Có bốn ảnh render riêng dùng so sánh pixel.
- WebGL **1.30.0** build thành công (`build_d2fb18f40280`, 20,878 giây ở lần build cuối), **0 lỗi**. Một cảnh báo Pipeline RuntimeConfig vốn có; game không dùng runtime Pipeline trong Player.
- **13 kiểm tra WebGL sương đạt** trên Chrome desktop 1440×900, giả lập điện thoại dọc và ngang. Đã tải WASM thật, so sánh ảnh 0/6 giây, đối chiếu cùng ánh sáng 0/60 giây, kiểm tra chiều sâu, màu shader và bố cục canvas; không có lỗi JavaScript/Unity/shader. Đây là giả lập trình duyệt, chưa phải kiểm tra trên iPhone vật lý.
- Ảnh WebGL lưu dưới `morning-fog-web-`; số đo và kết quả đầy đủ ở `morning-fog-web-results.json`. Cache gói build: `ea3fb90268515ad6`.
- **26 kiểm tra gói web cục bộ đạt**: tất cả tài nguyên khớp build, WASM khởi tạo, bắt đầu/tạm dừng chạy được và không có lỗi runtime (`morning-fog-local-results.json`).
- Unity cuối kiểm tra ở Edit Mode; dữ liệu người dùng giữ nguyên **75 Coin**, kỷ lục **1.775 m**, công việc được bật.

## Phát hành

- Bản hoàn chỉnh **1.30.0** đã build cục bộ; chưa push/phát hành bản này. Quyền phát hành lần trước chỉ áp dụng 1.29.3, cần người dùng xác nhận cho bản mới.
