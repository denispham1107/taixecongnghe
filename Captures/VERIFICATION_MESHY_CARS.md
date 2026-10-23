# Bộ ô tô Meshy — 24/10/2026

Đã nhận đủ bốn mẫu và thay **11 xe bốn bánh** trong scene Phố Việt. Người dùng xác nhận dùng cả xe thể thao, bán tải, tải phủ bạt và đầu kéo container, giữ tỷ lệ và chỉnh kích thước phù hợp. Thể thao dùng cho sedan/hatchback/taxi; bán tải dùng cho giao hàng; cả hai mẫu tải được chọn ngẫu nhiên ở nhóm tải thường và tải ngủ gật.

Mỗi lần spawn/tái sử dụng chọn lại một trong tám màu. Mặt nạ riêng giữ kính/lốp/kim loại; màu qua MaterialPropertyBlock, không cấp phát material mỗi lần xuất hiện. Bộ xe có ba LOD, bảy clip Legacy; container 23 xương/8 bánh, các mẫu khác 15 xương/4 bánh. Nóc lấy 49 mẫu từ raycast lên tam giác thật. Kích thước va chạm và neo đèn đổi đồng thời với mẫu tải; pose cản được reset trước khi gắn lại đèn khi tái sử dụng.

| Mẫu | Rộng × cao × dài (m) | Tam giác LOD0 / LOD1 / LOD2 |
| --- | --- | --- |
| Thể thao | 2,055 × 1,382 × 4,45 | 16.488 / 7.254 / 2.638 |
| Bán tải | 2,467 × 2,354 × 4,2 | 20.575 / 9.053 / 3.291 |
| Tải bạt | 2,634 × 2,848 × 6,4 | 13.481 / 5.931 / 2.156 |
| Container | 2,523 × 2,628 × 12,8 | 7.582 / 3.335 / 1.213 |

**11 suite / 3.002 điều kiện Unity đạt**, xem `meshy-cars-verification-summary.json`: MeshyCars 1.175, MeshyCarRoofs 256, TrafficDynamics 82, CrashUpgrade 97, BoostTrafficIsolation 478, QuickKick 62, CrashOutcome 51, CrashPoliceNavigation 214, NightJumpLighting 88, DistanceMode 31, SmoothTurns 468. Có 64 cú nhảy thật lên bốn mẫu, hai chiều, có/không boost và dt 16/50 ms.

**28 kiểm tra nguồn đạt**: giữ toàn bộ tam giác gốc LOD0, scale đồng nhất, bề rộng vừa làn, ba map 2K mỗi mẫu khớp hash nguồn. Source/normal/surface maps không bị chỉnh; các mask được tạo riêng. `VerifyTrafficDynamics` dùng tiếp xúc theo chiều dài thân thật thay khoảng cách fixture cũ 4,7 m. Không thay vật lý để làm test qua.

Đã kiểm bằng mắt 12 ảnh Unity `meshy-cars-PV_Meshy*-paint*.png`. Mặt nạ cuối xử lý theo từng texel trên tam giác ở vùng bánh để không lem màu vào lốp hoặc để sót mảng màu gốc trên hốc bánh. Sau tinh chỉnh màu đã chụp lại và chạy lại MeshyCars 1.175. Hai nguồn tải không có texel đèn hậu đỏ trong vùng chọn; giữ halo hậu và đèn phanh động, không tạo mask phát sáng cho cả thùng.

Nguồn mới: `ArtSource/Blender/PhoViet_MeshyCars.blend`, `integrate_meshy_cars.py`, `prepare_meshy_car_textures.py`; output `Assets/PhoViet/Art/Cars/Meshy/`. Cài bằng menu **Phố Việt/Art/Install received Meshy cars** hoặc `UpgradeCarsMeshy.Main`. Không ghi đè nguồn Cars/Blender cũ có thay đổi cục bộ. Bản mẫu Bridge giữ EditorOnly/inactive; nguồn import được bảo toàn.

Scene đã lưu; CSGT/xe máy/nhân vật, 30 binding đèn, khóa công việc và dữ liệu người chơi giữ nguyên. Unity trả Edit Mode. Chưa build/push/phát hành bộ này; web/`docs` vẫn 1.32.2. Chưa thử WebGL hoặc thiết bị di động vật lý/FPS/nhiệt độ cho bộ ô tô mới.
