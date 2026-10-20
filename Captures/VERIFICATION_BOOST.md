# Phố Việt 1.24.0 — tăng tốc E / vuốt xuống

## Quy tắc người dùng đã chốt

- E trên máy tính; vuốt từ trên xuống trong vùng chơi trên mobile.
- Tốc độ mong muốn được nhân 1,75 trong bốn giây; không cộng dồn hoặc kéo dài khi bấm liên tục.
- Hồi chiêu ba giây bắt đầu sau khi kết thúc tăng tốc.
- Té, bị CSGT bắt hoặc dừng đón/trả khách sẽ hủy ngay; muốn dùng tiếp phải chờ hồi chiêu.
- Hồi chiêu chạy trong thời gian té/hồi phục/đón/trả; tạm dừng hoặc chọn công việc thì đóng băng thời gian mô phỏng.
- Các giới hạn giảm tốc/nhường khi nhận việc và dừng đúng điểm hẹn vẫn ưu tiên an toàn. Có thể dùng tăng tốc cùng động tác nhảy. Va chạm vẫn mất một mạng; bị CSGT bắt kết thúc ngay.

## Triển khai

- `PhoVietBoost.cs`: trạng thái, timer, chống cộng dồn, nhận diện vuốt xuống, reset/cancel. `TryBoost()` chỉ cho phép ở trạng thái Riding và không đón/trả khách.
- `PhoVietGame.cs`: nhận E; ưu tiên phân loại vuốt dọc trước vuốt ngang, phân biệt với Q/vuốt lên. Nhân tốc độ mong muốn sau phần nhảy, trước giới hạn đón/trả/nhường xe. Dự đoán thời gian tới giao lộ xét cả thời gian boost còn lại và thời điểm trở về tốc độ thường, để lịch xe băng ngang không dùng sai tốc độ.
- `PhoVietRiderCrash.cs`, `PhoVietJobs.cs`, `PhoVietActions.cs`: hủy tăng tốc ngay tại trigger té, bắt giữ, boarding hoặc alighting.
- `ArtSource/Blender/pho_viet_boost_smoke.py`: chạy bằng Blender MCP; dựng khối thể tích có biên lượn, mật độ noise 4D, vật liệu khói trắng, camera và hai nguồn sáng; render bốn khung RGBA bằng Cycles rồi ghép atlas 768×192. Nguồn chỉnh sửa ở `PhoViet_BoostSmoke.blend`; không sửa các file Blender của xe/khách/phố/nhảy có thay đổi từ trước.
- `Resources/Boost/PV_WhiteBoostSmoke.png`: flipbook được bake từ volume thực trong Blender, có alpha mềm. `ConfigureBoostArt.cs` giữ alpha, wrap Clamp, filter Bilinear, không nén và không mipmap để tránh viền/lẫn các ô.
- `BoostSmokeVisual.cs`: hai lớp dùng chung flipbook: luồng hẹp, liên tục sát pô và khói nở/loãng ở sau. Gắn vào visual xe thật nên bám cả ngóc bánh/nhảy; mô phỏng trong hệ tọa độ đường để khói đã phụt không xoay theo xe khi đổi làn. Tối đa 128 hạt cho cả hai lớp; dùng shader trong game, không có mô phỏng chất lưu nặng trên WebGL.
- Khói pause/resume theo game, ngừng phát khi hết boost, xóa khi té/đón/trả/reset. Khi rebase đường sau cua, xóa vệt cũ và tiếp tục phát trong hệ tọa độ mới, không làm mất thời lượng boost.
- Hướng dẫn trong game và PWA đã bổ sung E và vuốt xuống. Thông báo tăng tốc dùng ô thông báo hiện có.

## Kiểm tra

- 39 kiểm tra boost riêng đã đạt: tốc độ, bốn giây, hồi chiêu ba giây, chống cộng dồn, vuốt, pause, khói thật, phối hợp nhảy, công việc, pickup/dropoff, va chạm quét ở tốc độ cao, truy đuổi/bắt giữ, rebase và restart.
- Đã xem ảnh nguồn Blender và ba ảnh Unity: `boost-exhaust-close.png`, `boost-exhaust-game.png`, `boost-jump.png`; khói trắng phụt liên tục từ đúng phía sau pô, nở rồi loãng dần.
- Tổng 26 bộ kiểm tra Unity, 1.895 kiểm tra đều đạt: 39 boost mới và 1.856 kiểm tra hồi quy (bao gồm 44 kiểm tra nhảy).

## Phát hành

- WebGL 1.24.0 build `build_58ee7501960c` thành công trong 205,442 giây, 0 lỗi. Hai cảnh báo cũ: Pipeline chỉ dùng trong Editor và trường `hudTouchChoice` không dùng ở nhánh WebGL.
- Cache phát hành: `c18c89f5fba61b82`.
- 81 kiểm tra web đạt: 6 smoke tăng tốc E/vuốt xuống, 6 smoke nhảy Q/vuốt lên, 12 UI/công việc/chuột/touch/xoay, 37 WebGL/PWA/offline, 20 fullscreen/safe area iPhone/iPad. Không có lỗi JavaScript hoặc Unity runtime.
- Đã xem ảnh WebGL mobile: vuốt xuống kích hoạt tốc độ 108 km/h và vệt khói trắng thật phía sau xe. Đây là kiểm tra profile trình duyệt, không phải đo hiệu năng trên iPhone vật lý.
- Commit phát hành: `a6dab40e885b45cb69b8c94ba061fe20923ab13c`.
- GitHub Pages triển khai thành công đúng commit: https://github.com/denispham1107/taixecongnghe/actions/runs/39419860687
- 26 kiểm tra live đạt: phiên bản/cache đúng, tất cả binary khớp bản đã thử, WebAssembly khởi động và nhận điều khiển, không có lỗi JavaScript/Unity runtime. Bằng chứng: `boost-live-results.json`.
- Trang chơi: https://denispham1107.github.io/taixecongnghe/
- Unity kết thúc ở Edit Mode, Game View ngang, công việc bật; dữ liệu hiện tại đọc lại là 55 Coin và kỷ lục 1.775 m. Không đặt lại ví về số dư cũ. Server thử nội bộ đã dừng.
- Các file Blender xe/khách/phố/nhảy đã có thay đổi từ trước, bản sao lưu Motorcycle và `huongdan.md` được giữ riêng khỏi commit tính năng này.
