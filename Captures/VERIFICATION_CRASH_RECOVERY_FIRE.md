# Phố Việt 1.27.0 — xe hồi phục, va chạm cảnh vật và lửa khói Blender

## Các lựa chọn đã xác nhận

- Xe giao thông bị văng chờ ba giây tính từ lần va chạm gần nhất, tự dựng, nhập làn gần nhất đúng chiều cũ rồi chạy tiếp. Animation dựng xe dài 0,65 giây; nếu hai làn đều kín thì chờ chỗ trống để nhập làn.
- Xe đang ôm cua tiếp tục hoàn thành hướng rẽ đã chọn trước khi bị tông.
- Quái xế/xe tải ngủ gật đã đâm nhà và cháy vẫn nằm yên tại vị trí tai nạn trong thế giới, chỉ bị thu hồi khi ra khỏi vùng hiển thị như trước.
- CSGT giữ cơ chế dựng xe 1,2 giây hiện có, không chuyển sang ba giây.
- Người chơi giữ ba mạng, mỗi chuỗi tai nạn mất một mạng, thời gian hồi phục cũ, tự quay về hướng hành trình trước va chạm và tiếp tục công việc. CSGT bắt vẫn kết thúc ngay.
- Đâm nhà, cây hoặc vật dụng làm người chơi và xe bị văng theo tốc độ/góc va chạm. Cảnh vật đứng yên.

## Triển khai

- `PhoVietTrafficRecovery.cs`: lưu hướng chạy, tốc độ giao thông, hệ trục đường và đích rẽ trước xung lực đầu tiên. Mỗi lần va chạm thật tiếp theo đặt lại thời gian chờ. Sau ba giây, chọn làn còn trống trong đúng chiều, dựng xe mượt và chạy tiếp. Kiểm tra chỗ trống dùng kích thước xe theo góc quay, bao gồm xe đang nằm ngang.
- Xe đang rẽ nối một đường cong từ vị trí bị văng đến đích rẽ cũ, không quay ngẫu nhiên theo góc xoay lúc ngã. Hướng và đường cong lưu lại được biến đổi cùng cảnh khi người chơi rẽ hoặc tọa độ cuộn/rebase.
- `PhoVietGame.cs`: cập nhật trạng thái hồi phục trước chuyển động giao thông thông thường; giữ xe trong pool đủ lâu để hồi phục, thay vì xóa ngay khi bị tụt phía sau camera. Va chạm trong lúc dựng xe vẫn có xung lực và đặt lại đồng hồ.
- `PhoVietCollisions.cs`, `PhoVietActions.cs`: ghi hướng trước cả va chạm vật lý lẫn cú đạp; không dùng góc quay đã văng làm hướng phục hồi. Xe thường đụng mép nhà bật lại, không trở thành xác xe cháy cố định.
- `BikeVisualAnimator.cs`: dùng action Recover hiện có cho xe máy giao thông. Khôi phục tư thế nền của các xương trước khi lấy mẫu animation, không để action hồi phục còn ảnh hưởng tư thế Ride sau khi dựng xong.
- `PhoVietRiderCrash.cs`: giữ hướng hành trình đã lưu, xóa spin còn lại khi dựng xong; dùng vật cản tĩnh có khối lượng vô hạn cho xung lực người chơi. Luật ba mạng và hồi phục/nhường xe trước đây được giữ.
- `StreetObstacleSet.cs`, `PhoVietObstacleUpgrade.cs`: 735 nhóm với 4.575 hộp va chạm nhẹ, theo đúng nhà/cây/vật dụng trên cả năm dãy đường. Có nhà, bảng hiệu, mái hiên, thân cây/bồn gốc, bàn ghế, quầy thức ăn, kệ tạp hóa, tủ sửa xe/lốp, xe đẩy, xe đậu và cột đèn/điện. Không cần collider mesh nhiều tam giác hoặc bật đọc mesh trên mobile.
- `PhoVietSceneryContacts.cs`: quét vị trí cũ–mới trong hệ tọa độ của từng nhóm cảnh, kể cả nhóm đã xoay/đường nhánh. So sánh với thời điểm va xe để dùng va chạm xảy ra trước. Kiểm tra chiều cao để nhảy qua được vật thấp; cho phép lái ra khỏi phần đang chạm khi vừa dựng xe. Các va chạm phụ lúc đang văng không trừ nhiều mạng trong cùng một chuỗi tai nạn.

## Hiệu ứng Blender MCP

- Đã thực hiện qua Blender MCP, Blender 5.2.0 LTS. Dùng Mantaflow gas combustion: nhiên liệu liên tục, độ xoáy và sinh khói; lưới 96, cache 48 frame. Render thể tích bằng Cycles, 128 mẫu.
- `ArtSource/Blender/PhoViet_WreckFire.blend`, `PhoViet_WreckFire.json` và `pho_viet_wreck_simulation.py` là nguồn chỉnh sửa/tái tạo. Cache mô phỏng tái tạo tại `Temp/WreckGasCache96`, không phải phụ thuộc runtime và không đưa cache tạm lên Git.
- Xuất hai atlas RGBA 768×384, tám frame/atlas: `Resources/Wreck/PV_WreckFlame.png` và `PV_WreckSmoke.png`. Khói dùng chung vùng cắt alpha qua cả tám frame để căn tâm hạt mà không gây nhảy kích thước giữa các frame.
- `RecklessArt.cs`, `WreckFlame.shader`, `WreckFireVisual.cs`: lửa phát sáng, ánh sáng nhấp nháy, khói đen nổi lên và tản theo nhiễu. Điểm phát nằm ở phần động cơ theo hướng xe. Bỏ hoàn toàn capsule/sphere làm ngọn lửa và texture chấm tròn cũ. Tối đa 48 hạt lửa và 40 hạt khói mỗi xác xe.
- Scene đã lưu đầy đủ material và texture. `InstallCrashUpgrade.cs` nhập PNG đúng alpha và cập nhật hai hiệu ứng trong scene; chạy lại dùng lại material đã lưu. Không ghi đè các file Blender có thay đổi từ trước của người dùng.
- Tái tạo qua Blender MCP: chạy file Python trong một namespace riêng, sau đó gọi `bake_simulation_atlases()` trong namespace đó. Khi chạy như script chính của Blender, bước xuất atlas tự chạy. Khi chuyển dự án sang máy/thư mục khác cần đổi biến ROOT trong script.

## Kiểm tra

- `VerifyCrashUpgrade.cs`: 97 kiểm tra về mốc ba giây, va chạm lặp/ngắt dựng xe, đúng chiều/làn, làn bị chắn, xe bị tụt sau camera, tiếp tục hai kiểu rẽ, rebase, ngoại lệ xe cháy, atlas/hạt, va chạm và nhảy qua cảnh vật, thoát vùng chạm, tốc độ/xung lực, tư thế xương sau dựng xe và bốn làn rẽ thật.
- Các test GPS dựng giao lộ giả bằng junctionAt nhưng không di chuyển mesh đường được cô lập khỏi cảnh vật; các test giao lộ thật và test mới vẫn bật kiểm tra cảnh vật. Không tắt va chạm để che lỗi trong gameplay.
- Đã xem ảnh thực tế `crash-upgrade-truck-fire.png`, `crash-upgrade-bike-fire.png`, `crash-upgrade-fallen.png`, `crash-upgrade-standing.png`, `crash-upgrade-resumed.png`.
- 30 lượt bộ kiểm tra Unity, tổng 2.187 kiểm tra đạt. Bao gồm 97 kiểm tra mới và các bộ cũ về ba mạng, CSGT, chuỗi va chạm, rẽ, công việc/GPS, nhảy/tăng tốc, cảnh quan, HUD và camera/safe area dọc/ngang. Các phần ảnh hưởng được chạy lại sau tinh chỉnh cuối.

## Phát hành

- Build WebGL `build_69573e3f6da9` thành công, 253,159 giây, 0 lỗi. Phiên bản 1.27.0, cache `927440463c898c5a`.
- Hai cảnh báo cũ: Pipeline không có RuntimePipelineConfig (công cụ chỉ dùng trong Editor), và trường HUD touch native không dùng trong nhánh WebGL.
- Đã đọc lại trạng thái Unity: Edit Mode, Game View ngang, công việc bật; vẫn 55 Coin và kỷ lục 1.775 m.
- 81 kiểm tra trình duyệt đạt: nhảy 6, tăng tốc 6, công việc 12, WebGL/PWA 37 và toàn màn hình iPhone/iPad dọc/ngang 20. Không có lỗi JavaScript/Unity runtime; bản WebAssembly thật cũng khởi động được từ cache khi offline.
- Đang triển khai lên GitHub Pages và đối chiếu bản công khai.
