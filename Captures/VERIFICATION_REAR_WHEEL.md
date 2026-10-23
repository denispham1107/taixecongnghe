# Phố Việt 1.32.2 — sửa đuôi xe chớp khi bánh quay

Ngày 24/10/2026, Asia/Bangkok. Tái hiện từ ảnh người dùng trên mobile dọc/ngang.

## Nguyên nhân và thay đổi

Mặt chắn bùn/vỏ gần lốp trong mesh AI bị gắn vào bone bánh vì thuật toán cũ chọn theo tâm mặt. Khi quay, các mặt dài ló ra ngoài lốp và tạo chớp. Sửa skin weights của cả năm mẫu MeshyRiders, cả ba LOD; giữ nguyên mặt, tọa độ, UV, normal và texture. Chỉ mâm/hub nằm trọn vùng bán kính 0,245 m quay; đường bao lốp/vỏ giữ theo Frame hoặc Steering. Không tắt WheelAngle hoặc hạ độ nét.

Nguồn tái tạo: `repair_meshy_wheels.py`, được dùng trong `integrate_meshy_riders.py`; nguồn Blender và năm FBX/metadata đã cập nhật. Export dựng lại bảy NLA strips từ Actions trước khi ghi FBX. `RefreshWheelAssets.cs` xác nhận đủ bảy clip Legacy mỗi asset. Không thay người/xe CSGT, scene, gameplay hoặc khóa công việc.

## Bằng chứng cuối

- Native: tám suite, **1.022 điều kiện đạt**; xem `rear-wheel-verification-summary.json` và từng JSON suite.
- WheelStability: 30 trường hợp (5 mẫu × 3 LOD × 2 bánh), sáu góc quay; mâm chuyển động, đường bao có độ lệch lớn nhất **0 m**.
- Ảnh BakeMesh ở tám góc trước/sau: `rear-wheel-before-sheet.png`, `rear-wheel-after-sheet.png`; đã kiểm bằng mắt. Silhouette tại góc 0 giống nhau (IoU 1,0); chênh lệch pixel trung bình 0,001031/255.
- Source audit: cả năm FBX có Idle, Ride, KickLeft, KickRight, FallLeft, FallRight, Recover. Toàn bộ native suite và ảnh đã chạy lại sau lần export cuối này.
- Build `build_692689eb6e6a`: Succeeded, 29.500 ms, không lỗi; một warning Pipeline thiếu config runtime đã biết, không ảnh hưởng player.
- Version **1.32.2**, cache **441262423309bccc**, UTC **2026-10-23T19:03:08.0950663Z**. Tổng output 57.353.820 byte; precache 57.352.271 byte. **29 kiểm tra bundle đạt**.
- Giữ DPR tối đa 3 / 3,2 triệu pixel, atlas 2048×4096 / hai tile 2K, player LOD0 và MSAA4 của 1.32.1.

Browser local trên artifact cuối: **40 kiểm tra đạt** = 14 kiểm tra DPR3 dọc/ngang/tablet/xoay lại + 26 kiểm tra tài nguyên/WASM/start/pause; không lỗi runtime. Các JSON `rear-wheel-mobile-results.json`, `rear-wheel-local-results.json` và ảnh mobile ghi bằng chứng. Bộ PWA/offline 37 đã kiểm trên build thử, không cộng vào kết quả artifact cuối.

Đã commit/push và phát hành: source/artifact `8f06a86e3d015e601ca05339757f8b0d5ece6120`, [Pages run 40010075408](https://github.com/denispham1107/taixecongnghe/actions/runs/40010075408) completed/success đúng SHA. **26 kiểm tra live đạt**, không lỗi runtime; binary khớp từng byte và text chuẩn hóa CRLF, WASM/start/pause hoạt động. Bằng chứng `rear-wheel-pages-release.json`, `rear-wheel-live-results.json`. Unity ở Edit Mode; server docs đã dừng. Nếu PWA giữ bản cũ, đóng hết tab/app rồi mở lại, không xóa dữ liệu người chơi.

Đây là kiểm tra Unity/Chrome giả lập; chưa thử điện thoại vật lý hoặc đo FPS/nhiệt độ.

Các file Wave/Bridge/Packages và nguồn Blender khác có sẵn trong working tree không thuộc thay đổi này; giữ nguyên ngoài commit.
