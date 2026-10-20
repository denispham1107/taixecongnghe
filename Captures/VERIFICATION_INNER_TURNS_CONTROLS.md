# Phố Việt 1.25.0 — đổi phím, tăng tốc 125%, làn giữa được rẽ

## Yêu cầu và lựa chọn đã xác nhận

- Desktop: SpaceBar nhảy, Q đạp xe khác, W tăng tốc. SpaceBar vẫn bắt đầu/chơi lại ở màn hình tương ứng, không tự nhảy ngay khi bắt đầu.
- Tăng tốc +125% tương đương nhân 2,25, giữ bốn giây; hồi chiêu hai giây sau kết thúc/hủy. Các luật hủy, nhường xe, ba mạng và bắt giữ vẫn như trước.
- Mobile giữ vuốt lên nhảy, vuốt xuống tăng tốc, chạm hai lần đạp xe.
- Hai làn trái được rẽ trái, hai làn phải được rẽ phải. Hai làn giữa vẫn đi thẳng nếu không chọn rẽ.
- Người dùng xác nhận giữ nguyên số thứ tự làn sau cua, kể cả khi rẽ từ làn giữa.

## Triển khai

- `PhoVietGame.cs`: đổi ba phím gameplay; tách với SpaceBar bắt đầu/chơi lại bằng điều kiện `wasRiding` hiện có.
- `PhoVietBoost.cs`: `BoostMultiplier=2.25f`, `BoostDuration=4`, `BoostRecovery=2`; cập nhật thông báo +125%.
- HUD ngang, hướng dẫn trong game/PWA và nhắc tự vệ trước quái xế đều dùng phím mới. Không thay đổi cử chỉ mobile.
- `JunctionStreetArt.cs`: thêm nhánh rẽ ở phần dưới thân mũi tên đi thẳng của hai làn giữa; dùng đúng vật liệu sơn trắng hiện có, không dùng màu xanh/đỏ của hình minh họa. Áp dụng cả bốn hướng của mọi giao lộ. Các nhánh nằm trong làn và giữ khoảng cách với vạch qua đường.
- `RefreshJunctionPaint.cs`: cập nhật bảy mesh sơn đã lưu được dùng chung bởi các nhánh phố. Mesh tăng từ 192 lên 264 vertex/index. Dùng setter của Mesh và UploadMeshData để renderer trong Editor cập nhật ngay; CopySerialized đơn thuần ban đầu giữ lại buffer vẽ cũ dù dữ liệu CPU đã đổi.
- `PhoVietRoutes.cs`: hàm `CanTurnFromLane` quy định hướng hợp lệ theo nửa đường. Trong vùng chọn rẽ, một thao tác đúng hướng xếp lệnh rẽ; trong lòng giao lộ có thể rẽ ngay. Hướng ngược với mũi tên vẫn đổi làn. Khi vào vùng rẽ muộn, chọn làn gần vị trí xe thực tế trong nửa đường phù hợp.
- Bán kính và điểm cuối đường cong được tính theo `turnSide * LaneX[CurrentLane]`, thay cho offset 4,65 cố định; vì vậy làn giữa nối đúng vị trí làn tương ứng trên đường mới. Camera, rebase, lịch xe sau cua và công việc/GPS dùng cơ chế cũ.
- Các test có kỳ vọng “làn giữa không được rẽ” được cập nhật theo yêu cầu mới, không bỏ kiểm tra va chạm hoặc tính liên tục của đường.

## Kiểm tra

- `VerifyInnerTurns.cs`: 57 kiểm tra mới về mũi tên đủ bốn hướng, nhánh nằm trong làn, rẽ sớm/muộn từ hai làn giữa, đường cong nằm trên mặt đường, giữ làn và đổi tên đường, đi thẳng khi không chọn rẽ, đổi làn khi bấm hướng đối diện.
- Đã xem ảnh thực tế `inner-turns-paint.png`, `inner-turns-lane-1-curve.png`, `inner-turns-lane-2-exit.png`; mũi tên nối liền và xe vào đúng đường/làn.
- 27 bộ kiểm tra Unity, tổng 1.952 kiểm tra đều đạt. Bao gồm các bộ nhảy, tăng tốc mới 2,25/hồi chiêu hai giây, va chạm/quét tốc độ cao, ba mạng, truy đuổi, giao lộ, giao thông, công việc/GPS, cảnh quan, HUD và safe area dọc/ngang.

## Phát hành

- WebGL 1.25.0: build `build_b07884968582` thành công trong 176,477 giây, 0 lỗi; cache `83be3171de979f44`.
- Hai cảnh báo cũ: Pipeline không có runtime config (chỉ dùng Editor), và trường HUD touch native không dùng ở nhánh WebGL.
- 81 kiểm tra web đạt: 6 W/vuốt xuống, 6 SpaceBar/vuốt lên, 12 UI/công việc/chuột/touch/xoay, 37 WebGL/PWA/offline (bao gồm Q), 20 fullscreen/safe area iPhone/iPad. Không có lỗi JavaScript hoặc Unity runtime.
- Profile điện thoại trong trình duyệt kiểm tra bố cục/điều khiển, không phải phép đo hiệu năng trên thiết bị vật lý.
- Commit phát hành: `bf032695fbf3b166cf15dce09fcd16cbf584530f`.
- GitHub Pages triển khai thành công đúng commit: https://github.com/denispham1107/taixecongnghe/actions/runs/39443915283
- 26 kiểm tra live đạt: phiên bản/cache đúng, binary khớp bản đã thử, WebAssembly khởi động và nhận điều khiển, không có lỗi JavaScript/Unity runtime. Bằng chứng: `inner-turns-live-results.json`.
- Trang chơi: https://denispham1107.github.io/taixecongnghe/
- Unity kết thúc ở Edit Mode, Game View ngang, công việc bật; dữ liệu đọc lại vẫn là 55 Coin và kỷ lục 1.775 m. Server thử nội bộ đã dừng.
- Các file Blender có thay đổi từ trước, bản sao lưu Motorcycle và `huongdan.md` được giữ riêng khỏi commit.
