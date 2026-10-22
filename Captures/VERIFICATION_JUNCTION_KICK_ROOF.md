# Phố Việt 1.30.5 — Đạp xe ở giao lộ và chạy trên nóc xe

## Đạp tại giao lộ

Mã cũ loại thẳng `crossing` và loại xe không gần song song/ngược song song với hướng tài xế. Vì vậy xe máy cắt ngang, ôm cua hoặc ở cạnh người chơi đang rẽ có thể không được chọn dù đang rất gần.

`PhoVietKickAssist.cs` hiện cho phép chọn mọi xe máy còn hoạt động trong phạm vi tiếp cận an toàn, gồm xe cắt ngang, đang rẽ, cùng chiều và ngược chiều. Mỗi lần Q/chạm hai lần vẫn chọn một mục tiêu gần có thể áp sát; vẫn phải có tiếp xúc giày thật, không hất các xe khác từ xa.

- Khoảng áp sát tính theo hướng xoay và kích thước xe mục tiêu. Điểm ngắm phía trước tính theo vùng thân xe mà chân vươn tới được.
- Lối áp sát dùng phép quét khối có hướng xoay; hộp bao theo trục thế giới trước đây chặn nhầm hai xe song song khi cả hai đang chéo ở giao lộ.
- Dự đoán cả vận tốc ngang của mục tiêu, phần tiến lên của cung rẽ và khác biệt quãng đường khi xe đang chạy lệch khỏi tâm cung để áp sát.
- Xe lấy thăng bằng trước khi vung chân. Độ nghiêng khi rẽ trái trước đây kéo giày phải vào trong khoảng 30 cm, làm hụt dù khoảng cách tâm xe đã đúng.
- Trong động tác đạp, `TickRide` chia thời gian thành bước tối đa 10 ms; tổng thời gian và quãng đường vẫn được cộng đủ. Điều này giữ điều khiển áp sát ổn định ở khung hình mobile chậm. Giày vẫn được kiểm tra theo animation Blender hiện có.
- Sau tiếp xúc, xe bắt đầu trở về làn trong nhịp thu chân; nếu đường về bị chắn thì điều chỉnh tốc độ theo khoảng trống. Dự đoán cả độ rộng xe mục tiêu đang ngã. Lực đạp đẩy mục tiêu ra phía xa tài xế, kể cả mục tiêu đang tạt về phía tài xế trước va chạm.
- `PhoVietRoutes.cs` đổi hệ tọa độ cho offset, điểm và hướng tham chiếu của cú đạp khi kết thúc rẽ; không xóa động tác giữa chừng. Làn gốc và tiến trình đường mới được giữ.
- `PhoVietCollisions.cs` tính độ rộng của xe máy bị đạp theo tiến trình té, không lấy độ nghiêng lái có sẵn trước cú đạp làm vùng thân xe đã ngã ngay lập tức. Đây là nguyên nhân của một va chạm giả với xe đang tạt làn.

`VerifyJunctionKick` kiểm tra 48 tình huống ở bước khung hình 20/50 ms: tài xế đi thẳng/rẽ trái/rẽ phải, mục tiêu hai bên, bốn hướng tương đối. Có 40 tiếp xúc giày thật; 8 mục tiêu đổi đường hoặc thoát cửa sổ tiếp xúc được bỏ qua và tài xế trở về an toàn, không gây lực từ xa. Có tiếp xúc thật cho từng hướng mục tiêu trong cả ba trạng thái đi thẳng/trái/phải. Ca cuối cung rẽ kiểm tra động tác còn hoạt động qua thời điểm đổi hệ tọa độ. Ô tô chắn đường áp sát vẫn ngăn cú đạp. Bộ kiểm tra còn có hai ca `SpawnCrossTraffic`/`TickIntersectionVehicle` thực, thay vì chỉ di chuyển mục tiêu bằng vận tốc tuyến đã tách.

## Đáp nóc và chạy trên nóc xe

Các kiểm tra nhảy cũ qua ở những thời điểm đáp mẫu, nhưng chưa bao phủ đầy đủ chuyển động tương đối cao và việc tiếp đất/rời mép xảy ra giữa hai khung hình. Mặt nóc trước đây lấy tại điểm cuối khung hình trong khi kiểm tra va chạm dùng thời điểm sớm hơn; tính mặt nóc chỉ theo tâm xe máy cũng có thể hạ xe vào kính/capo khi bánh sau còn trên nóc cao.

`PhoVietJump.cs` xử lý:

- Tính mặt nóc từ vị trí và góc xe mục tiêu tại cùng thời điểm với xe máy. Ô tô lấy mặt đỡ của cả hai bánh; không hạ xe sớm khi bánh sau còn trên nóc cao.
- Quét đoạn chuyển động giữa khung hình để tìm lần đáp nóc đầu tiên, kể cả khi trong cùng một khung hình xe máy đã đáp rồi đi qua mép nóc. Chỉ khảo sát xe trong vùng quét gần để hạn chế chi phí trên mobile.
- Phần thời gian sau tiếp xúc dùng để chạy tiếp trên nóc. Khi vượt mép, xe bắt đầu rơi theo trọng lực; từ thùng tải cao xuống cabin thấp cũng là một nhịp rơi, không dịch xuống tức thời.
- Kiểm tra va chạm dùng cùng bề mặt đang chuyển động và chỉ xét đoạn còn chồng nhau theo phương ngang. Xe được đỡ trên nóc không bị tính tông vào hộp bao bảo thủ của xe bốn bánh.
- Vẫn va chạm nếu nhảy không đủ cao, đâm vào thân xe/vách thùng cao hơn, hoặc xe đỡ đã nghiêng đổ. Không dùng miễn va chạm để che lỗi tiếp đất.

`VerifyRoofRiding` chạy **576 cú nhảy thật từ mặt đường**, gồm Sedan, Hatchback, Taxi, DeliveryVan, BoxTruck và DrowsyTruck; bước khung hình 16/33/50/100 ms, có/không W, xe mục tiêu -12/0/7/28 m/s và ba vị trí đáp lệch nhau. Mọi ca đều chạm nóc, chạy tiếp bình thường và kết thúc ở mặt đường hoặc còn được nóc xe đỡ. Tổng **2.304 điều kiện đạt**. Các kiểm tra `VerifyJump` và `VerifyTruckJump` cũ cũng đạt, gồm bật lại từ nóc, xe tải lạng lách, xe đỡ bị lật/ngừng hoạt động, rơi từ thùng xuống cabin và đâm vào vách cao khi đang ở cabin thấp.

## Hồi quy và ảnh

Kết quả từng nhóm ở `junction-Verify*-results.json`; tổng hợp ở `junction-native-summary.json`: **21 nhóm, 5.121 điều kiện đạt** (assertion, không phải 5.121 kịch bản độc lập). `VerifyJunctionKick` đạt 216 điều kiện, gồm hai ca bộ điều khiển cắt ngang thật bổ sung. Có kiểm tra lại CSGT, kết thúc lượt do va chạm, giao thông va chạm, nhường xe khi đón/trả khách, tăng tốc chỉ dành cho người chơi, cử chỉ và tốc độ khi rẽ.

Fixture phanh/đổi làn của `VerifyTrafficDynamics` được đánh dấu đã quyết định đường rẽ để không ngẫu nhiên chuyển sang kiểm tra rẽ ở một giao lộ tái sử dụng trong vòng lặp kiểm tra đổi làn. Các assert về đổi làn giữ nguyên; các kiểm tra rẽ và chạy giao thông dài vẫn riêng biệt.

Ảnh chụp từ frame tiếp xúc/được đỡ thật, không gọi ép hất xe hoặc gán đứng trên nóc:

- `junction-kick-turn-left.png`, `junction-kick-turn-right.png`: chân chạm thân xe khi đang rẽ.
- `junction-ride-sedan-roof.png`, `junction-ride-truck-roof.png`: nhảy thật rồi được mặt nóc đỡ.
- `junction-roof-captures.json`: hoàn tất 4 ảnh; đã xem cả 4 ảnh.

Phím/cử chỉ giữ nguyên: Q/chạm hai lần để đạp, Space/vuốt lên để nhảy, W/vuốt xuống để tăng tốc. Xe thường vẫn gọi CSGT khi bị đạp trúng; tự vệ trước quái xế giữ luật hiện có.

## Build và bàn giao

WebGL **1.30.5** build thành công: `build_244b633caaed`, **234,803 giây**, **0 lỗi**, ba cảnh báo sẵn có về Pipeline RuntimeConfig và hai biến chưa đọc (`dragging`, `hudTouchChoice`). Cache **80915edfb1105b45**. Kết quả ở `junction-build-summary.json`; toàn bộ gói build được đồng bộ sang `docs`.

Kiểm tra trình duyệt trên máy cục bộ với WASM thật:

- **26 kiểm tra gói/WASM đạt** (`junction-local-results.json`): tài nguyên khớp build, đúng phiên bản/cache, khởi tạo và bắt đầu/tạm dừng không gây lỗi runtime.
- **25 kiểm tra cử chỉ đạt** (`junction-web-gestures-results.json`): mobile dọc 412×839 và ngang 839×412 nhận cả bốn hướng trước khi nhấc tay; giữ/thả không nhân đôi lệnh, rung tay nhỏ vẫn là tap, hủy khi xoay và nhận lại cử chỉ mới.
- **6 kiểm tra đầu vào/runtime đạt** (`junction-web-jump-results.json`): tải WASM desktop/mobile, gửi SpaceBar/vuốt lên và không ghi nhận lỗi runtime. Các ca này là smoke đầu vào; kiểm chứng quỹ đạo đáp và chạy trên nóc nằm ở các kiểm tra Unity phía trên.

Trình duyệt là Chrome/SwiftShader giả lập, không phải phép đo GPU/cảm ứng trên iPhone hoặc Android vật lý. Tổng trước phát hành **5.178 điều kiện đạt** = 5.121 Unity + 26 gói/WASM + 25 cử chỉ + 6 đầu vào nhảy (`junction-verification-summary.json`). `git diff --check` đạt.

Unity đã về Edit Mode, phiên bản 1.30.5; dữ liệu native giữ **105 Coin** và kỷ lục **1.775 m** (`junction-release-state.json`). Máy chủ kiểm thử cục bộ đã dừng.

Chưa push/phát hành. Các file Blender và `huongdan.md` đang sửa từ trước được giữ ngoài phạm vi commit bản sửa này.
