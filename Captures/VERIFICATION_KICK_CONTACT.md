# Phố Việt 1.30.3 — Áp sát và đạp bằng tiếp xúc bàn chân

## Lỗi đã tái hiện

`TryKick` cũ chọn xe cách ngang tới 3,65 m rồi gọi `KnockVehicle` ngay khi bấm. Animation chân dài khoảng 0,74 m chỉ phát sau đó. Vì vậy xe mục tiêu đã bị hất dù chân chưa tới. `kick-contact-before-results.json` ghi nhận kiểm tra thất bại trên bản cũ: xe bị đổ, tăng KickCount và gọi CSGT ngay lúc bấm.

## Hành vi mới

- Q trên desktop và chạm hai lần trên mobile vẫn dùng chung `TryKick`.
- Chọn một xe máy đang chạy, cùng chiều hoặc ngược chiều, ở phía có thể áp sát. Xe ô tô, tải, xe đã đổ và giao thông đang cắt ngang không phải mục tiêu.
- Xe ngược chiều được xét sớm theo vận tốc tương đối, tối đa 16 m phía trước; từ chối mục tiêu đã quá sát để còn đủ thời gian áp sát và vung chân. Xe nguy hiểm được ưu tiên trong những mục tiêu hợp lệ.
- Thực hiện Approach → Swing → Return. Khi Approach, chân còn trên xe; xe người chơi dịch sát về đúng bên. Với xe cùng chiều, tốc độ được điều chỉnh nhẹ để chạy cạnh mục tiêu. Chỉ Swing mới dùng KickLeft/KickRight, kéo dài 0,55 giây.
- Lực đạp chỉ xuất hiện một lần khi giày trong animation chạm vùng thân xe. Hộp tiếp xúc hẹp ở phần thân xe, loại khoảng trống quanh tay lái. Các điểm gót/mũi giày được lấy từ clip Blender hiện có; không dựng animation mới trong Blender ở bản này.
- Cùng một đồng hồ điều khiển pose hiển thị và kiểm tra tiếp xúc. Chia nhỏ phép quét theo tối đa khoảng 8 ms để xe ngược chiều chạy nhanh không xuyên qua khoảng kiểm tra giữa hai frame.
- Sau động tác, tự trở về làn ban đầu bằng chuyển động liên tục. Người chơi đổi làn để ngắt động tác; nhảy/tăng tốc, va chạm, bị bắt hoặc bắt đầu đón/trả khách cũng hủy động tác đang thực hiện. Pause đóng băng và restart xóa trạng thái.
- Kiểm tra đường áp sát bằng hình bao và vận tốc của các phương tiện, cùng vật cản cảnh quan. Bị chắn thì hủy và chờ lối về làn thông; không cấp miễn va chạm. Nếu thực sự bị xe khác chạm khi đang áp sát, luật té/thua hoặc chờ CSGT vẫn áp dụng.
- Mục tiêu biến mất hoặc được tái sử dụng trong pool không thể nhận lực đạp nhầm. Mục tiêu chạy thoát sẽ thành cú hụt, không có lực từ xa.
- Chỉ đạp trúng mới tăng KickCount. Đạp trúng xe thường gọi CSGT; tự vệ với quái xế vẫn không gọi CSGT. Đạp hụt/hủy không bị truy đuổi.

## Tích hợp mã

- `Assets/PhoViet/Scripts/PhoVietKickAssist.cs`: chọn mục tiêu, dự đoán thời điểm, kiểm tra lối áp sát, điều chỉnh tốc độ, dịch xe, đồng hồ Swing, quét giày, tạo lực và trở về làn.
- `BikeVisualAnimator.cs`: cache 65 mẫu cho gót/mũi mỗi chân từ KickLeft/KickRight, nội suy theo pose gameplay. Bỏ đồng hồ kick độc lập để hình ảnh và lực không lệch nhau.
- `PhoVietGame.cs`: chèn chuyển động hỗ trợ trước cập nhật giao thông, kiểm tra va chạm thân xe trước xử lý tiếp xúc giày. CurrentLane giữ làn gốc trong động tác. `spawnGeneration` nhận biết lần spawn của mục tiêu.
- `PhoVietActions.cs`, `PhoVietTraffic.cs`: điểm vào TryKick và reset/pool. Các file Jump/Boost/Jobs/RiderCrash/Routes có hook hủy khi hành vi khác tiếp quản.
- `PhoVietCollisions.cs`: bề rộng xe máy ngã phụ thuộc góc nghiêng thật. Ghi lại bề rộng đầu frame và giải phép quét với bán kính thay đổi trong frame; tránh nở hình bao hồi tố gây đụng giả ngay sau cú đạp. Va chạm thật vẫn có hiệu lực, kể cả lúc bị truy đuổi.

## Kiểm tra Unity

`VerifyKickContact`: **301 điều kiện đạt**, gồm phải/trái, cùng chiều/ngược chiều, làn ngược chiều thật, bước mô phỏng 0,02/0,05/0,1 giây; không gây lực ngay lúc bấm, đúng chân/đúng pose, khoảng giày ≤ 0,08 m tới vùng thân xe, tự về làn, không teleport; hủy bởi điều khiển/vật cản/xe tái sử dụng/dừng dịch vụ; tự vệ không gọi CSGT; tăng tốc sau cú đạp không bị giữ giới hạn gia tốc.

Mỗi dòng bên dưới là số điều kiện/assertion, không phải số kịch bản độc lập. Các fixture cũ đã được đổi để chờ tiếp xúc thật. Fixture chỉ cần trạng thái truy đuổi gọi StartChase trực tiếp, không giả định TryKick tạo lực ngay.

| Bộ kiểm tra | Điều kiện đạt |
|---|---:|
| VerifyBikeArt | 156 |
| VerifyBoost | 39 |
| VerifyCrashOutcome | 51 |
| VerifyCrashPoliceNavigation | 214 |
| VerifyCrashUpgrade | 97 |
| VerifyGestures | 39 |
| VerifyJump | 44 |
| VerifyOrdinaryRearWarnings | 137 |
| VerifyPursuit | 39 |
| VerifyRearRandomPolice | 60 |
| VerifyReckless | 120 |
| VerifyRiderContacts | 45 |
| VerifyServiceApproach | 152 |
| VerifyServiceRearYield | 117 |
| VerifySmoothTurns | 468 |
| VerifyTrafficDynamics | 82 |
| VerifyTruckJump | 69 |

Tổng: **2.230 điều kiện Unity đạt** (301 chuyên biệt + 1.929 hồi quy).

## Bằng chứng hình ảnh

Sáu ảnh `kick-{right-same,left-oncoming}-{before,approach,contact}.png` chụp trên các frame Editor riêng để mesh da phản ánh đúng pose. Có hai chuỗi: đạp phải xe cùng chiều và đạp trái xe ngược chiều. Ảnh contact được chụp ở lần tiếp xúc vật lý đầu tiên, không ép gọi KnockVehicle.

## Build và phát hành

WebGL **1.30.3** build thành công (`build_b931e177b5a1`, 229,744 giây), **0 lỗi**. Ba cảnh báo sẵn có: Pipeline RuntimeConfig và hai biến dragging/hudTouchChoice chưa được đọc. Cache: `e7abee28429a3fd1`. Kết quả gọn lưu tại `kick-build-summary.json`.

**26 kiểm tra gói web đạt**: mọi tài nguyên trong precache khớp build đã kiểm tra, đúng phiên bản/cache, Unity WASM thật khởi tạo và thao tác bắt đầu/tạm dừng không gây lỗi runtime (`kick-local-results.json`).

Dữ liệu native được xác nhận sau kiểm thử: **105 Coin**, kỷ lục **1.775 m**; Unity trở về Edit Mode (`kick-release-state.json`). Các file Blender và huongdan.md người dùng đang chỉnh không thuộc bản sửa này.

**25 kiểm tra cử chỉ trên WASM thật đạt** (`kick-web-gestures-results.json`): trên khổ dọc 412×839 và ngang 839×412, vuốt được gửi trước khi nhấc tay, không lặp thao tác khi giữ/thả, rung nhẹ vẫn là tap, hủy/xoay màn hình xóa cử chỉ cũ và chạm mới hoạt động ngay. Đây là giả lập Chrome/SwiftShader, không thay cho thử trên iPhone/Android vật lý. Native `VerifyGestures` xác nhận hai tap cùng frame bắt đầu áp sát, không hất xe ngay.

Tổng trước phát hành: **2.281 điều kiện đạt** = 2.230 Unity + 26 gói/WASM + 25 cử chỉ web. `git diff --check` đạt. Gói web và mã nguồn đã được người dùng xác nhận cho phép push/phát hành phiên bản mới.

## Phát hành đã xác minh

- Người dùng đã xác nhận cho phép push/phát hành bản **1.30.3**. Commit nguồn và gói web `a2ee4e10be6b926a0cb76e6ca147e01274eca130` đã được push lên main.
- GitHub Pages [run 39713599187](https://github.com/denispham1107/taixecongnghe/actions/runs/39713599187) hoàn tất thành công cho đúng commit trên (`kick-pages-release.json`).
- [Game công khai](https://denispham1107.github.io/taixecongnghe/) trả về **1.30.3**, cache `e7abee28429a3fd1`, khớp gói đã kiểm tra.
- **26 kiểm tra trên bản công khai đạt**: toàn bộ tài nguyên khớp build, Unity WASM khởi tạo, thao tác bắt đầu/tạm dừng không gây lỗi runtime (`kick-live-results.json`). Tổng **2.281 điều kiện trước phát hành + 26 điều kiện trên bản công khai**.
- Bản PWA đang mở sẽ thông báo có bản mới; đóng và mở lại để nhận bộ nhớ đệm mới theo cơ chế cập nhật sẵn có của game.
