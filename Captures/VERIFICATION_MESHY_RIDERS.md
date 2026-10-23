# Kiểm tra người Meshy có rig — Golden Star Stance

Ngày 23/10/2026. Scene native đã lưu. đã commit/build/push/phát hành1.32.0 (xem VERIFICATION_RIDERS_WEB_RELEASE.md); web vẫn1.31.0.

Dùng chính FBX Character_output có rig từ folder Golden Star Stance_20261023_150430. Bộ người chưa rig trước đó đã bị xóa khỏi Assets/scene. 32 actor ngoài CSGT và56 xe đậu nhánh được thay; người và xe CSGT giữ nguyên. Jobs=false.

| Suite | Điều kiện đạt |
| --- | --- |
| VerifyBikeArt | 156 |
| VerifyBoost | 35 |
| VerifyDistanceMode | 31 |
| VerifyJump | 44 |
| VerifyJunctionKick | 216 |
| VerifyMeshyLighting | 16 |
| VerifyMeshyRiders | 356 |
| VerifyNightJumpLighting | 88 |
| VerifyQuickKick | 62 |
| VerifyRoofRiding | 2304 |

**Tổng: 3308 điều kiện /10 suite đạt.**

5 kiểm tra màu atlas thêm: hero348.150 pixel đỏ ở vùng áo, các NPC chỉ8–52 pixel viền sau resize; không còn mảng áo đỏ/sao. Bằng chứng meshy-riders-colors.json.

Preflight kiểm đúng scene,33 actor/1 CSGT, không thiếu script/đèn, jobsfalse. Cleanup xóa1 root người cũ, folder cũ và tắt3 preview nguồn ngoài game; không xóa nguồn xe/clip lịch sử. Installer đối chiếu policeUnchanged=true,14xe đậu nguồn,56xe đậu nhánh,18neođèn.

Đã xem ảnh Unity hero trước/sau, Racer sau đổi áo, pose đạp và ảnh Blender bind; ảnh ngày/đêm/CSGT tại meshy-riders-*.png. Runtime dùng clip gốc Meshy bike để tránh sai rotation sau FBX re-bake. Mô hình người mới có17xương semantic/7clip/3LOD, bản nguồn rig vẫn giữ để tái tạo.

Giới hạn: kiểm trong Unity Editor; chưa đo FPS trên điện thoại thật. WebGL1.32.0 đã kiểm local và live. Không thay đổi gameplay hay mở công việc.
Cập nhật: bộ nhân vật và sao lưng đã lên web1.32.0, Pages39948530264,26kiểm tra live đạt. Các giới hạn kiểm phần cứng vẫn giữ.
