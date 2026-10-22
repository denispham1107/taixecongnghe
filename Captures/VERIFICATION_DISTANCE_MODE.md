# Phố Việt 1.30.6 — tạm ẩn công việc, chỉ thi quãng đường

## Yêu cầu đang có hiệu lực

Ngày 22/10/2026, người dùng yêu cầu tạm ẩn toàn bộ công việc đưa đón/trả
khách. Chỉ mở lại khi người dùng yêu cầu rõ ràng. Đây không phải xóa tính năng.

## Triển khai

- `PhoVietJobs.cs`: khóa phát hành `RideJobsReleased=false`, cờ scene
  `jobsEnabled=false`, điều kiện chung `JobsAvailable`. Cấu hình scene cũ bật
  công việc cũng không thể vượt khóa phát hành.
- Không sinh danh sách việc, không nhận việc qua API, không tự tạm dừng,
  không giảm tốc/dừng để đón trả, không phát giọng dẫn đường. Các điểm vào
  `BeginService`, `TickService`, `SpeakNavigation` cũng kiểm tra khóa.
- Marker điểm hẹn, UI chọn việc và bản đồ đều kiểm tra khóa; hai model hành
  khách được ẩn khi reset. Không tạo tuyến GPS khi chơi bình thường.
- Tắt nhường đường dành riêng cho công việc. Giao thông, quái xế, CSGT,
  nhảy, tăng tốc, đạp xe và va chạm tiếp tục dùng luật hiện tại.
- `PhoVietDistanceHud.cs`: HUD trong nhẹ gọn hơn, giữ tên đường, quãng đường,
  tốc độ, kỷ lục và nút tạm dừng. Bản ngang giữ nút âm thanh. Ẩn Coin và GPS;
  cập nhật vùng chạm HUD, vùng vuốt và vị trí thông báo cho bố cục mới.
- Giữ nguyên phố ô bàn cờ/giao lộ để rẽ và giữ tên phố ổn định.
- Giữ nguyên số dư Coin đã lưu. Không xóa PlayerPrefs và không có thưởng mới.
- Công cụ nâng cấp khách cũng tuân thủ khóa; ghi chú yêu cầu tại README và
  ngay cạnh khóa mã nguồn để tránh vô tình bật lại trong các lần sửa sau.

Khi được yêu cầu mở lại: bật `RideJobsReleased`, cờ scene `jobsEnabled`, rồi
kiểm tra lại công việc, GPS, hoạt ảnh khách và nhường đường. Mã/model/âm thanh
được giữ nguyên. Không có nút tự mở lại trong giao diện người chơi.

## Kiểm tra Unity

8 bộ kiểm tra, **946 điều kiện đạt**:

| Bộ kiểm tra | Số điều kiện |
|---|---:|
| VerifyDistanceMode | 31 |
| VerifyQuickKick | 62 |
| VerifyJunctionKick | 216 |
| VerifySmoothTurns | 468 |
| VerifyJump | 44 |
| VerifyBoost | 35 |
| VerifyCrashOutcome | 51 |
| VerifyGestures | 39 |

`VerifyDistanceMode` chạy hai lượt mô phỏng 60 giây ở cả cờ scene tắt/bật,
đi qua giao lộ, kiểm tra không phát sinh việc/tự dừng, không mất Coin, vẫn
tăng mét, lưu kỷ lục và chơi lại. Kiểm tra thêm việc gọi API nhận việc và
hoàn thành dịch vụ cũ không thể tạo thưởng hoặc phát giọng GPS.

VerifyBoost/VerifyGestures giữ nhánh kiểm tra dịch vụ cũ khi tính năng được
phát hành; ở chế độ hiện tại kiểm tra cuộc gọi công việc không cản tăng tốc
hoặc cử chỉ. Không chạy các bộ kiểm tra đón/trả đang bị khóa, không tuyên bố
chúng đã được kiểm tra trong bản này.

Đã xem trực tiếp ba ảnh HUD từ Unity:

- `distance-hud-portrait.png` — 540×960.
- `distance-hud-mobile-landscape.png` — 960×540.
- `distance-hud-desktop.png` — 1440×900.

Không còn vùng Coin/GPS/công việc; mét, kỷ lục và điều khiển không chồng nhau.
Ảnh native và hồ sơ trình duyệt giả lập không thay thế kiểm tra máy iOS thật.

## Build và phát hành

Build WebGL **Succeeded**, 0 lỗi, 3 cảnh báo đã có từ bản trước (Pipeline
runtime config và hai trường không dùng trong WebGL). Phiên bản **1.30.6**,
cache **c713cfa4fa73614d**, build UTC `2026-10-22T11:49:17.4262969Z`.

Đã đồng bộ toàn bộ build sang `docs`, bỏ các binary cũ và kiểm tra qua HTTP
local bằng `VerifyPublishedGame.cjs`: **26/26 điều kiện đạt**. Bao gồm đối chiếu
chính xác các tài nguyên đã build, metadata/cache, khởi động WebAssembly thật,
thao tác bắt đầu/tạm dừng, không có lỗi JavaScript/Unity runtime. Kết quả tại
`distance-local-results.json`, ảnh `browser-distance-local.png`.

Sau kiểm tra: Unity ở Edit Mode, `timeScale=1`, Coin vẫn **105**, kỷ lục vẫn
**1.775 m** (`distance-native-final-state.json`).

Đã push commit `23941b11c82116d45185a51672d554dfac07413d` lên `main` và phát
hành thành công sau khi người dùng yêu cầu tiếp tục công việc.

- GitHub Actions run **39797882879**: completed/success, đúng SHA nguồn.
- Trang công khai: https://denispham1107.github.io/taixecongnghe/
- Kiểm tra trực tiếp bản công khai bằng `VerifyPublishedGame.cjs`: **26/26 đạt**,
  đúng version/cache và toàn bộ tài nguyên, WebAssembly khởi động được, không có
  lỗi JavaScript/Unity runtime.
- Bằng chứng: `distance-live-results.json`, `distance-pages-release.json`,
  `browser-distance-live.png`.

Người dùng cài PWA nên đóng toàn bộ tab/cửa sổ game rồi mở lại để service worker
chuyển sang bộ tài nguyên mới đã tải đủ.
