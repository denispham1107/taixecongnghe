# Phố Việt 1.30.4 — Áp sát nhanh, tăng tốc chỉ dành cho người chơi

## Hai lỗi đã tái hiện trên 1.30.3

`boost-kick-before-results.json` ghi nhận xe máy khác tăng từ **15,8072 m/s lên 37,0824 m/s** ngay khi người chơi bật W. Nhánh chạy song song lấy `Speed - 1.2` làm tốc độ đích; xe vượt sau và xe nguy hiểm cũng có các nhánh lấy `Speed + ...`.

Cùng phép tái hiện đó: xe máy chạy 28 m/s cách trước 2 m ở làn bên cạnh không được chọn để đạp. Bộ áp sát cũ chủ yếu giảm tốc để chạy cạnh mục tiêu, không có nhịp bắt kịp mục tiêu nhanh.

Sau sửa, `boost-kick-after-results.json` xác nhận xe đối chứng giữ **12 → 12 m/s** khi W bật; mục tiêu 28 m/s được chọn và đạp trúng qua kiểm tra bàn chân, không gọi ép KnockVehicle.

## Áp sát và đạp

- `PhoVietKickAssist`: tính vận tốc tiếp cận từ vận tốc mục tiêu và vị trí dự kiến của bàn chân, tăng tốc nhanh khi cần bắt kịp. Giới hạn tốc độ nhịp áp sát theo mức tối đa 2,25 lần tốc độ hành trình cơ sở; không nhân chồng thêm 2,25 lần lên W đang hoạt động.
- Tốc độ áp sát ngang tối đa **32 m/s** thay cho 16 m/s. Clip KickLeft/KickRight dùng cùng nội dung Blender sẵn có nhưng phát trong **0,30 giây** thay cho 0,55 giây. Pose hiển thị và quét giày vẫn dùng chung đồng hồ.
- Căn lại tốc độ để giữ được điểm chạm khi đã ở cạnh mục tiêu, thay vì để xe chạy thoát trong lúc chờ vung chân. Quét và dự đoán giao thông dùng tốc độ nhịp áp sát sắp áp dụng; kiểm tra lại trước khi tăng tốc để hủy khi đường bị chắn.
- Tiếp tục cập nhật áp sát trước lúc đạp trúng nếu mục tiêu dịch ngang. Nếu mục tiêu tạt làm lối áp sát không còn an toàn, hủy và trở về làn; không ép trúng một xe đã ngoài tầm chân.
- Hồi tốc theo bước có giới hạn sau cú đạp, tự trở về làn ban đầu. Đổi làn, nhảy, W, dừng dịch vụ và va chạm vẫn có thể hủy như trước. Giới hạn giảm tốc đón/trả khách được áp lại sau tính nhịp áp sát.
- Vẫn chỉ hất xe khi giày chạm vùng thân xe. Giữ phạm vi gần cũ, gồm tối đa 16 m phía trước cho xe ngược chiều và kiểm tra đủ thời gian tiếp cận; không kéo mục tiêu về gần, không dịch chuyển tức thời, không miễn va chạm.

`VerifyQuickKick`: ở hai phía trái/phải và bước 0,02/0,05 giây, xe máy chạy 28 m/s ở trước 2 m bị đạp trúng sau **0,15–0,18 giây**; xe ngược chiều ở trước 12 m bị đạp trúng sau **0,25–0,26 giây**. Đây là thời gian mô phỏng, không phải phép đo trên điện thoại vật lý.

## Tách tốc độ các phương tiện khỏi W/nhịp áp sát

- Thêm `TrafficPaceSpeed` từ đồng hồ độ khó bình thường, độc lập với W, nhảy và nhịp áp sát.
- `PhoVietTraffic`: bỏ tốc độ chạy song song lấy từ tốc độ người chơi. Xe thường chạy theo `baseTrafficSpeed`, còn phanh và đổi làn vẫn hoạt động. Xe vượt sau dùng tham chiếu giao thông cơ sở.
- `PhoVietReckless` và `PhoVietExitTraffic`: tốc độ spawn, vượt sau và chạy lạng lách lấy tham chiếu cơ sở, không tăng theo W.
- `PhoVietActions`/`PhoVietRoutePolice`: khởi đầu truy đuổi và theo đường rẽ không thừa hưởng tốc độ tăng tạm thời của người chơi.
- `PhoVietJobTraffic`: xe phía trước không bị tăng tốc để giữ khoảng cách với người chơi. Trong giai đoạn bảo vệ đón/trả/nhận chuyến, giới hạn tốc độ người chơi dựa trên khoảng trống phía trước; xe khác vẫn né/phanh theo các quy tắc nhường đã có.
- Vẫn trừ `Speed` khỏi vị trí các xe khi cuộn thế giới: đó là chuyển động tương đối của camera, không phải tốc độ thực của NPC. Bỏ phép trừ này sẽ khiến toàn bộ đường và xe trôi sai khi người chơi tăng tốc.
- W giữ **+125% trong 4 giây**, hồi chiêu **2 giây**; cử chỉ vuốt xuống và Q/chạm hai lần giữ nguyên.

## Kiểm tra Unity

`VerifyBoostTrafficIsolation` so sánh từng frame giữa lượt có/không W: xe máy, xe hơi, xe van, xe tải thường, xe vượt sau, xe ngược chiều, xe đã tách tuyến, xe cắt ngang, quái xế/xe tải ngủ gật đang vượt sau/đang lạng lách/đang ngược chiều. So sánh cả trường speed lẫn chuyển động thế giới sau khi loại cuộn camera; kiểm tra cả xe nguy hiểm vừa xuất hiện và CSGT vừa được gọi. Quỹ đạo được giữ giống nhau giữa hai lượt để không nhầm ngẫu nhiên đổi hướng với lỗi tăng tốc.

Các bộ kiểm tra và số điều kiện đạt (số assertion, không phải số kịch bản độc lập):

| Bộ kiểm tra | Điều kiện đạt |
|---|---:|
| VerifyBikeArt | 156 |
| VerifyBoost | 39 |
| VerifyBoostTrafficIsolation | 478 |
| VerifyCrashOutcome | 51 |
| VerifyCrashPoliceNavigation | 214 |
| VerifyGestures | 39 |
| VerifyKickContact | 221 |
| VerifyOrdinaryRearWarnings | 137 |
| VerifyPursuit | 39 |
| VerifyQuickKick | 62 |
| VerifyRearRandomPolice | 60 |
| VerifyReckless | 120 |
| VerifyRiderContacts | 45 |
| VerifyServiceApproach | 152 |
| VerifyServiceRearYield | 117 |
| VerifySmoothTurns | 468 |
| VerifyTrafficDynamics | 82 |

Tổng **2.480 điều kiện Unity đạt**. Sáu ảnh `fast-kick-{right-same,left-oncoming}-{before,approach,contact}.png` chụp trên frame Editor riêng; ảnh contact lấy ở lần tiếp xúc giày đầu tiên. Các ca xe tạt vào đường áp sát xác nhận hủy an toàn nếu không còn tiếp cận được, thay vì gây lực từ xa.

## Build và phát hành

WebGL **1.30.4** build thành công (`build_aa4f17ed29f0`, 241,285 giây), **0 lỗi**; ba cảnh báo sẵn có về Pipeline RuntimeConfig và hai biến dragging/hudTouchChoice chưa đọc. Cache `196acfd579d1e4e9` (`fast-build-summary.json`).

**26 kiểm tra gói web/WASM thật đạt** (`fast-local-results.json`): tài nguyên khớp build, đúng phiên bản/cache, WASM khởi tạo và thao tác bắt đầu/tạm dừng không gây lỗi runtime.

Dữ liệu native sau kiểm thử: **105 Coin**, kỷ lục **1.775 m**; Unity đã về Edit Mode (`fast-release-state.json`). Các file Blender và huongdan.md đang chỉnh được giữ riêng ngoài bản sửa.

**25 kiểm tra cử chỉ trên WASM thật đạt** (`fast-web-gestures-results.json`): mobile dọc 412×839 và ngang 839×412 đều gửi lệnh trước khi nhấc tay, không nhân đôi lệnh lúc giữ/thả, phân biệt tap/rung nhẹ, hủy khi xoay màn hình và nhận cử chỉ mới ngay. Đây là giả lập Chrome/SwiftShader, không phải phép đo GPU hay cảm ứng trên thiết bị vật lý.

Tổng trước phát hành: **2.531 điều kiện đạt** = 2.480 Unity + 26 gói/WASM + 25 cử chỉ web. `git diff --check` đạt.

## Xác minh phát hành công khai

Người dùng đã cho phép push và phát hành **1.30.4**. Commit nguồn/build `e96fd6e43a36196ca4bd32f6872493f131f5d261` đã được push lên main. [GitHub Pages run 39757627284](https://github.com/denispham1107/taixecongnghe/actions/runs/39757627284) hoàn tất thành công.

[Game công khai](https://denispham1107.github.io/taixecongnghe/) trả về phiên bản **1.30.4**, cache **196acfd579d1e4e9**. **26 kiểm tra bản công khai đạt** (`fast-live-results.json`): tài nguyên khớp build đã kiểm thử, WASM thật khởi động, bắt đầu/tạm dừng hoạt động và không ghi nhận lỗi JavaScript/Unity runtime. Kiểm tra trình duyệt dùng Chrome/SwiftShader; không phải phép đo trên điện thoại vật lý. Bằng chứng triển khai và thời điểm xác minh nằm trong `fast-pages-release.json`.
