# Phố Việt 1.30.2 — CSGT tìm đường đến bắt người chơi đã té

## Lỗi và bằng chứng tái hiện

- `TickCrashPolice` trước đây thử một quạt hướng theo tia tới người chơi, nhìn trước khoảng 3,2 m. Cách này không giữ một đường vòng: khi cần chạy xa người chơi trước để thoát một cụm xe rồi quay lại, nó có thể đổi hướng qua lại mãi.
- `crash-police-before-results.json` ghi nhận ca thất bại trên code 1.30.1: ba xe tải đứng yên tạo túi chữ U, CSGT ở phía trước người chơi. Sau 35 giây mô phỏng, CSGT vẫn cách đúng **10 m**, chưa ngã và chưa bắt; game vẫn Falling. Vẫn tồn tại đường đi vòng qua đầu hàng xe.
- Các xe chặn trong test đứng yên thật, không dùng xác xe tự bị thu hồi sau vài giây; vì vậy test buộc CSGT phải tìm đường vòng.

## Sửa

- Thay quạt hướng cục bộ bằng đường tiếp cận được lưu trong `PhoVietCrashPoliceNavigation`: ưu tiên đoạn thẳng thật sự thông suốt; khi bị chắn dùng A* trên lưới 1,2 m. Đường vòng có thể tạm đi xa người chơi rồi quay lại, tránh dao động ở một chỗ.
- Mỗi đoạn dùng hình bao xe theo hướng di chuyển và dự đoán giao thông. Bước di chuyển thật kiểm tra lại trước khi chạy, giữ giới hạn theo tốc độ × delta time; vẫn xử lý va chạm CSGT bình thường.
- Theo dõi các waypoint qua chuyển động cuộn đường. Tính lại khi người chơi bị văng sang vị trí khác hoặc đoạn đường bị xe khác chiếm. Nếu không có đường thì CSGT chờ rồi kiểm tra lại, không xuyên xe/nhà và không tự nhảy đến người chơi.
- Khi người té nằm sát/trong xe đã tông, tìm điểm đứng bắt có thể tiếp cận quanh người đó, thay vì cố chạy vào tâm vị trí đang bị xe chiếm. Kiểm tra các điểm liên tục quanh bán kính tiếp cận để không bỏ sót khoảng trống do làm tròn ô lưới.
- Té giữa giao lộ vẫn cho CSGT đi trên hai nhánh đường của hệ tọa độ đang rẽ, tránh ép toàn bộ đường đi về nhánh thẳng cũ.
- Người chơi vẫn nằm lại, không tự dựng xe; CSGT vẫn giữ thời gian dựng xe **1,2 giây**. Chỉ bắt khi người chơi đã ngã xong và CSGT thật sự đến trong khoảng bắt **2,2 m**. Sau đó đi qua Arrested → Jailed như cũ, có nút chơi lại. Không có timer tự xử thua/tự bắt từ xa.
- Cache đường và thống kê lập đường được xóa khi bắt đầu một lần té mới hoặc khởi động lượt mới; pause đóng băng chuyển động.

## Kiểm tra

- `VerifyCrashPoliceNavigation`: **214 điều kiện đạt**, gồm trước/sau người chơi, ba vị trí làn/lề, bước thời gian 0,02/0,05/0,1 giây, đường trống, xe chắn ngang, túi ba xe tải, người té bị xe che vị trí, CSGT đang té, xe di chuyển đổi lộ trình, pause/resume/restart, đón/trả khách và té giữa góc rẽ trái/phải 30°/60° thật. Kiểm tra đến Arrested và Jailed, không chỉ thấy CSGT bắt đầu di chuyển.
- Hồi quy đạt: `VerifyCrashOutcome` **51**, `VerifyRearRandomPolice` **60**, `VerifyPursuit` **39**, `VerifyRiderContacts` **45**, `VerifyServiceApproach` **152**, `VerifyServiceRearYield` **117**, `VerifyJailLighting` **7**. Tổng **685 điều kiện Unity**.
- Bốn ảnh `crash-police-capture-{waiting,detour,arrested,jail}.png` ghi lại quá trình trên Game View; bước cuối xác nhận giao diện ĐÃ BỊ BẮT và nút CHƠI LẠI xuất hiện.
- WebGL **1.30.2** build thành công (`build_7a12c4387fcf`, 187,867 giây), **0 lỗi**, ba cảnh báo sẵn có về Pipeline RuntimeConfig và hai biến chưa được đọc. Cache `f8a17176175c5001`.
- **8 kiểm tra luồng trên WASM thật đạt**: desktop 1024×640, giả lập điện thoại dọc 412×839 và ngang 844×390. Mỗi bố cục khởi động lượt, bắt đầu truy đuổi, cho nhân vật té, rồi để game tự chạy đến cảnh ĐÃ BỊ BẮT/CHƠI LẠI; không gọi ép Arrest hay Jailed. Có ảnh `crash-police-web-*-jail.png`, xác nhận bằng vùng giao diện và xem ảnh, không có lỗi JavaScript/Unity (`crash-police-web-results.json`). Các profile mobile là giả lập Chrome/SwiftShader, chưa phải kiểm tra trên iPhone vật lý; thời gian wall-clock của trình duyệt phần mềm không phải thời gian gameplay.
- **26 kiểm tra gói web đạt**: toàn bộ tài nguyên khớp build, cache/phiên bản đúng, Unity WebAssembly khởi tạo, bắt đầu/tạm dừng hoạt động và không có lỗi runtime (`crash-police-local-results.json`). Tổng **719 điều kiện Unity + WebGL đạt**.
- Dữ liệu người dùng được giữ nguyên: **105 Coin**, kỷ lục **1.775 m**; Unity ở Edit Mode sau build.

## File

- Runtime: `Assets/PhoViet/Scripts/PhoVietCrashPoliceNavigation.cs`, `Assets/PhoViet/Scripts/PhoVietRiderCrash.cs`.
- Test/capture: `AgentScripts/VerifyCrashPoliceNavigation.cs`, `AgentScripts/CaptureCrashPolice.cs`, `AgentScripts/VerifyCrashPoliceWeb.cjs`.
- Không thay các file Blender hay `huongdan.md` người dùng đang chỉnh.

## Phát hành

- Người dùng đã xác nhận cho phép push/phát hành **1.30.2**. Commit nguồn và gói web `e8691d2bda3bfb4300ed379012032dfb3bed3dc3` đã được push lên `main`.
- GitHub Pages [run 39663697829](https://github.com/denispham1107/taixecongnghe/actions/runs/39663697829) hoàn tất thành công cho đúng commit trên (`crash-police-pages-release.json`).
- [Bản công khai](https://denispham1107.github.io/taixecongnghe/) trả về **1.30.2**, cache `f8a17176175c5001`, khớp gói đã kiểm tra.
- **26 kiểm tra trên web công khai đạt**: toàn bộ tài nguyên khớp build, WASM khởi tạo, bắt đầu/tạm dừng hoạt động và không có lỗi runtime (`crash-police-live-results.json`). Tổng **719 điều kiện trước phát hành + 26 điều kiện trên bản công khai**.
