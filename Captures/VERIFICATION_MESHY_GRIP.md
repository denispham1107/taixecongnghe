# Sửa tư thế vai và nắm tay lái

23/10/2026. Đã cài/lưu trong Unity, chưa commit/build/push/phát hành.

Áp dụng5mẫu ngoài CSGT/32actor/56xe đậu nhánh. Vai1.365m, đầu hạ.065m, torsoend1.415m, khuỷu(±.305,1.195,.055), cổ tay(±.395,1.261,.230). Các ngón ôm quanh grip(±.395,1.237,.315), ngón cái đối hướng, bàn tay mirror đúng hai bên. Source/17bone/clip/xe/công việc tắt giữ nguyên.

| Suite | Đạt |
| --- | --- |
| meshy-grip-VerifyBikeArt-results | 156 |
| meshy-grip-VerifyJump-results | 44 |
| meshy-grip-VerifyJunctionKick-results | 216 |
| meshy-grip-VerifyMeshyLighting-results | 16 |
| meshy-grip-VerifyMeshyRiders-results | 356 |
| meshy-grip-VerifyNightJumpLighting-results | 88 |
| meshy-grip-VerifyQuickKick-results | 62 |

Tổng 938 điều kiện/7suite đạt. `git diff --check` đạt.

Ảnh chính: meshy-grip-shoulders-front.png và meshy-grip-grip-close.png; đã xem ảnh Unity cuối. Kết quả install/captures/preflight và summary cùng tiền tố meshy-grip. Không suy diễn chỉ từ assertion gameplay rằng mô hình đã đẹp; ảnh cận được dùng để sửa thêm vị trí bàn tay trước chốt.

Giới hạn: chưa build WebGL hay thử trên điện thoại thật cho bản này; bản công khai1.31.0 chưa đổi.