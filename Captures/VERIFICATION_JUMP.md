# Phố Việt 1.23.0 — nhảy xe và chạy trên nóc xe

## Hành vi đã xác nhận với người dùng

- Q trên máy tính, vuốt lên trong vùng chơi trên điện thoại.
- Độ cao cố định; hồi chiêu hai giây kể từ lúc bắt đầu ngóc bánh. Không nhảy lần hai trên không. Có thể nhảy từ mặt đường hoặc một nóc xe đang đỡ người chơi.
- Vượt xe máy đã ngã; đáp và chạy trên Sedan, Hatchback, Taxi và DeliveryVan (xe giao hàng nhỏ hiện có). BoxTruck và DrowsyTruck là xe tải thùng cỡ trung, không phải nóc xe nhỏ có thể chạy lên.
- Tiếp bằng bánh trước/bánh sau/hai bánh dựa trên tốc độ rơi và vận tốc tương đối; sau đó hấp thụ lực, lấy lại cân bằng.
- Va vào thân xe vẫn trừ một mạng và dùng hoạt cảnh té/văng hiện có. Ba mạng, công việc và quy tắc bị CSGT bắt kết thúc ngay được giữ nguyên.

## Code và dữ liệu Blender

- `ArtSource/Blender/pho_viet_jump.py`: chạy bằng Blender MCP trên rig HeroScooter hiện có. Tạo năm action thật có keyframe (JumpTakeoff, JumpAir, LandBoth, LandFront, LandRear), mỗi pha 61 mẫu; thêm JumpDemo có quỹ đạo tiến tới và lên/xuống để xem/chỉnh trong Blender. Lưu riêng `PhoViet_Jump.blend`, không ghi đè bốn nguồn Blender đang có thay đổi từ trước.
- `Resources/PhoVietJump.json`: bảng pose được sinh từ cùng các keyframe. Dùng dữ liệu nhỏ này thay vì thay cả sáu FBX; cấu trúc LOD, skin và bảy clip chạy/đạp/té hiện có vẫn được giữ.
- `BikeJumpPose.cs` và `BikeVisualAnimator.cs`: nội suy pose theo pha vật lý, co gối/khuỷu tay, chuyển trọng tâm, xoay thân xe quanh bánh sau, giữ điểm thấp nhất trên mặt đỡ. SmoothDamp chuyển góc nghiêng giữa các pha. Khách ngồi sau đi theo góc xe và độ cao.
- `PhoVietJump.cs`: ngóc bánh 0,24 giây, vận tốc lên 12,4 m/s, gia tốc rơi 22 m/s², độ cao đo được khoảng 3,494 m, thêm 3 m/s tiến tới khi bay. Khi đáp thì chốt độ cao mặt đỡ; tốc độ xe và làn vẫn hoạt động nên xe nhanh hơn chạy qua nóc và rời mép. Mặt đỡ hết hiệu lực nếu xe bị đụng/ngã hoặc rời khỏi vị trí đỡ.
- Mặt nóc dùng bao hình đơn giản theo kích thước của các model hiện có, kèm độ dốc nắp capo/cốp; đây là mô phỏng tối ưu cho WebGL/mobile, không dùng mesh collider hoặc rigidbody cho từng bộ phận.
- `PhoVietCollisions.cs`: SAT quét theo thời gian ở XZ và khoảng cao Y; kiểm tra chân xe vượt qua đúng chiều cao vật cản. Không bật miễn va chạm cho toàn bộ cú nhảy. Đáp lên mặt đỡ hợp lệ được xử lý trước kiểm tra thân xe.
- `PhoVietGame.cs`: nhận Q và nhận vuốt lên trước vuốt ngang; giữ quyền ưu tiên HUD và double tap. Tick vật lý nhảy trước chuyển động, xác định mặt đỡ sau chuyển động giao thông. Reset khi chơi lại.
- `PhoVietRoutes.cs`: rebase đường phố chỉ theo mặt phẳng, giữ độ cao người chơi khi đổi đường lúc đang bay.
- `PhoVietRiderCrash.cs`: bỏ trạng thái nhảy khi bắt đầu té nhưng truyền vận tốc Y vào quá trình văng hiện có.
- `PhoVietJobs.cs`: không bắt đầu đón/trả khi người chơi còn bay/đang cân bằng trên nóc; khách đang ngồi vẫn bám xe. Hướng dẫn Q/vuốt lên có trong game và trang web.

## Kiểm tra Unity

- 25 bộ kiểm tra, tổng 1.856 kiểm tra đều đạt.
- Trong đó 44 kiểm tra riêng của `VerifyJump.cs`: chuẩn bị/ngóc bánh, độ cao, hồi chiêu, chặn nhảy trên không, vuốt, pause/reset, vượt xe ngã bằng quỹ đạo thật, nhảy từ đường lên ô tô đang chạy, đáp đủ bốn loại nóc nhỏ, chạy xuống đường, mất mặt đỡ, ba kiểu tiếp đất, rebase khi đang bay, va chạm mất mạng trong/ngoài truy đuổi, bắt giữ.
- 24 bộ hồi quy trước đó: giao thông, va chạm, truy đuổi, ba mạng, giao lộ, công việc/GPS, khách, model, cảnh quan, cảnh báo phía sau, HUD, camera và safe area dọc/ngang.
- Đã xem trực tiếp các keyframe trong Blender MCP và các ảnh Unity `jump-wheelie.png`, `jump-air.png`, `jump-roof.png`, `jump-landing.png`.

## Phát hành

- WebGL 1.23.0: build `build_df3040937803` thành công trong 231,092 giây, 0 lỗi; cache `8dc2e2cea59236fa`.
- Hai cảnh báo cũ: Pipeline không có runtime config (chỉ cần Editor) và trường `hudTouchChoice` không dùng trong nhánh WebGL.
- 75 kiểm tra trình duyệt đạt: 6 smoke mới với Q và vuốt lên qua CDP trên WASM thật; 12 kiểm tra UI/công việc/chuột/touch/xoay; 37 kiểm tra WebGL/PWA/offline; 20 kiểm tra full viewport và safe area iPhone/iPad dọc/ngang. Không có lỗi JavaScript hoặc Unity runtime.
- Profile điện thoại trong trình duyệt dùng để kiểm tra bố cục/điều khiển, không phải phép đo hiệu năng trên thiết bị iPhone vật lý.
- Commit và kết quả GitHub Pages/live sẽ được bổ sung sau khi push.
