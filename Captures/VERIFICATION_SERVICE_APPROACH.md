# Phố Việt 1.30.1 — nhường người chơi trong toàn bộ giai đoạn đón/trả khách

## Lỗi đã tái hiện

- Bản 1.30.0 chỉ bật `ServiceRearProtected` ở Boarding/Alighting và hai giây xuất phát. `WorkSpeedLimit` đã hạ tốc độ từ khoảng 22 m trước điểm hẹn nhưng khoảng này chưa yêu cầu giao thông nhường. Test hành trình thật với xe máy phía sau đã gây Falling ở frame thứ 12, vẫn trong Pickup, trước khi khách bắt đầu lên xe.
- Sau khi mở rộng khoảng nhường, test tiếp tục phát hiện xe tải ngủ gật xoay thân dài khi phanh/đổi làn. Chỉ kiểm tra tâm xe hoặc chiều dài chưa xoay không đủ; góc xoay cuối frame còn có thể làm phép kiểm tra swept contact thấy va chạm ngay đầu frame.
- Chỉ đẩy vị trí cuối của xe về phía sau không đủ khi xe cắt chéo: đoạn chuyển động vẫn có thể quét qua người chơi. Đã tái hiện ca này tại frame đầu xuất phát sau Boarding.

## Sửa trong runtime

- `PhoVietJobs.TryServiceApproach` dùng chung điều kiện cho giảm tốc và nhường xe: đúng đường/đúng lề của điểm đón hoặc trả đang thực hiện, trong vùng tiếp cận 22 m. Phạm vi nhường bao gồm tiếp cận, khách lên/xuống và hai giây thực sự di chuyển sau đó. Điểm hẹn ở xa/sai làn không bật bảo vệ phục vụ khách.
- Xe giao thông đổi sang làn trống hoặc phanh khi đường bị kín; làn né được kiểm tra lại mỗi bước vì xe khác có thể vừa chiếm chỗ.
- `ServiceTrafficFootprint` lấy kích thước phủ mặt đường theo góc xoay hiện tại và trước đó, bao gồm xe dài/xe đang bị hất văng. Dùng cùng phép kiểm tra chuyển động tương đối với người chơi đang nhập lề.
- `YieldRearForService` chặn cả quỹ đạo chuyển động trước khoảng hở an toàn, thay vì chỉ sửa tọa độ Z cuối. Góc xoay có thể quét thân xe dài qua người chơi được giữ ở hướng an toàn trước đó trong bước phanh. Giữ khoảng hở theo thân xe, tương ứng khoảng 2,6 m từ đầu xe sau đến tâm người chơi khi đi thẳng.
- Cập nhật vị trí/góc trước đó của thân người chơi trước khi chạy AI giao thông. Kiểm tra nhường/phanh lặp lại sau giải quyết va chạm giữa các xe để chặn xe bị đẩy tới từ phía sau.
- Áp dụng cho xe máy, ô tô, xe tải thường, quái xế, xe tải ngủ gật, xe đang cắt làn/từ đường rẽ tới và xe bị hất văng. Không tắt phát hiện va chạm; một va chạm thật vẫn gây té/kết thúc theo luật hiện tại.

## Kiểm tra

- `VerifyServiceApproach`: **152** điều kiện đạt. Ma trận 2 điểm đón/trả × 2 lề × 5 loại xe × 3 bước thời gian 0,02/0,05/0,1 giây chạy trọn đoạn giảm tốc → phục vụ khách → 1,9 giây xuất phát; thêm làn bị kín, cắt làn, route detached, xe bị hất văng, người chơi đang nhập lề, va chạm dây chuyền thực tế, và kiểm tra không dùng miễn va chạm.
- Hồi quy đạt: `VerifyJobEdges` 17, `VerifyServiceRearYield` 117, `VerifyRearWarningsService` 140, `VerifyOrdinaryRearWarnings` 137, `VerifyRideRefinements` 50, `VerifyJobs` 116, `VerifyRiderContacts` 45, `VerifyTrafficDynamics` 82, `VerifyRoadDrivers` 45, `VerifyCrashOutcome` 51. Tổng **952 điều kiện Unity**.
- Sáu ảnh `service-approach-{pickup,dropoff}-{approach,stop,departure}.png` chụp Game View thật, đều ở Riding. Xe tải từ phía sau đã đổi sang làn trống và vượt qua người chơi trong thời gian phục vụ khách. Ảnh trả khách có actor khách thực hiện xuống xe. Capture phục hồi ví/kỷ lục sau khi chụp.
- WebGL **1.30.1** build thành công (`build_5dc3338ab194`, 187,007 giây), **0 lỗi**. Ba cảnh báo sẵn có về Pipeline RuntimeConfig và hai biến chưa được đọc, không phát sinh lỗi biên dịch mới. Cache `a9544bfce916de62`.
- **26 kiểm tra gói web cục bộ đạt**: toàn bộ tài nguyên khớp build, WASM thật khởi tạo được, bắt đầu/tạm dừng hoạt động và không có lỗi JavaScript/Unity (`service-approach-local-results.json`).
- **20 kiểm tra WebGL mobile đạt** trên giả lập iPhone/iPad, cả dọc và ngang: canvas phủ màn hình, vùng safe area, ẩn toolbar/menu theo bố cục và không có lỗi runtime (`fullscreen-game-service-approach-results.json`). Đây là giả lập Chrome theo thiết bị, không phải thử trực tiếp trên iPhone vật lý. Tổng **998 điều kiện Unity + WebGL**.
- Unity cuối kiểm tra ở Edit Mode, công việc được bật; ví **75 Coin** và kỷ lục **1.775 m** được giữ nguyên.
- Chưa push/phát hành bản 1.30.1; xác nhận phát hành gần nhất của người dùng áp dụng bản 1.30.0.

## Các file chính

- `Assets/PhoViet/Scripts/PhoVietJobs.cs`
- `Assets/PhoViet/Scripts/PhoVietJobTraffic.cs`
- `Assets/PhoViet/Scripts/PhoVietGame.cs`
- `AgentScripts/VerifyServiceApproach.cs`, `AgentScripts/CaptureServiceApproach.cs`

Các file Blender và `huongdan.md` người dùng đang sửa được giữ nguyên, không đưa vào commit bản sửa này.
