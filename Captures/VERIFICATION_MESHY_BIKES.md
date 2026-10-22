# Kiểm tra thay xe máy Meshy — 23/10/2026

Đã nhận đúng FBX người dùng gửi qua Meshy Bridge và thay mọi xe máy ngoài CSGT trong scene Unity. Hero trắng–đen, City xanh ngọc, Underbone xanh dương, Sport vàng, Racer cam. Người dùng đã xác nhận giữ mô tô CSGT.

## Phạm vi đã lưu

- 32 actor được thay, gồm 14 xe đậu nguồn; 56 xe đậu nhánh cập nhật mesh LOD1/vật liệu; 3 mesh bake giữ GUID.
- CSGT trước/sau cài không đổi visual/component; đủ 30 bộ đèn xe, không thiếu head/halo. Đèn mặt kính phát sáng từ mask UV Meshy: 128 mặt pha, 66 mặt hậu; 7.332 pixel pha trắng, 12.445 pixel hậu đỏ. Không làm cả thân trắng phát sáng.
- 17 xương, 7 clip Legacy, 3 LOD/mẫu, một atlas vật liệu; LOD2 khoảng 5k tam giác. Đèn/khói gắn trên Steering/Frame theo metadata.
- Giữ RideJobsReleased=false / jobsEnabled=false, Coin/kỷ lục, root/pool vật lý và điều khiển Q/Space/W.
- Nguồn Meshy và nguồn Wave/WaveAlpha cũ được giữ. Chạy Blender nền riêng, không sửa Blender đang mở của người dùng.

## Kiểm tra Unity

**15 suite / 4,572 điều kiện đạt**, không cộng kết quả lịch sử của bản Wave vào tổng này.

| Suite | Điều kiện |
| --- | ---: |
| VerifyMeshyBikes | 340 |
| VerifyMeshyLighting | 16 |
| VerifyBikeArt | 156 |
| VerifyJump | 44 |
| VerifyTruckJump | 69 |
| VerifyRoofRiding | 2,304 |
| VerifyQuickKick | 62 |
| VerifyJunctionKick | 216 |
| VerifyBoost | 35 |
| VerifyBoostTrafficIsolation | 478 |
| VerifyCrashOutcome | 51 |
| VerifyCrashPoliceNavigation | 214 |
| VerifySmoothTurns | 468 |
| VerifyNightJumpLighting | 88 |
| VerifyDistanceMode | 31 |

`VerifyRoofRiding` bao gồm 576 cú nhảy mô phỏng thật với các chiều/góc/tốc độ/mặt nóc; `VerifyBikeArt` kiểm clip/rig/đạp hai chân/quay bánh/ngã/phục hồi pool. `VerifyNightJumpLighting` kiểm nguồn pha/cookie/đèn thật đi theo nhảy và đáp. `VerifyBoostTrafficIsolation` kiểm xe khác giữ tốc độ. Suite mới kiểm mọi nguồn model, màu, vật liệu PBR, đèn đêm, socket và xe đậu nhánh.

Một lần chạy đầu `VerifyMeshyLighting` thiếu khởi tạo khói vốn chỉ tạo khi tăng tốc; đã sửa setup của test và chạy lại đạt 16 điều kiện. Lỗi này ở setup kiểm tra, không thay đổi gameplay. Không dùng kết quả cũ đó làm bằng chứng đạt.

Ảnh cuối đã xem trực tiếp: `meshy-bikes-hero-front.png`, `meshy-bikes-hero-rear.png`, `meshy-bikes-game.png`, `meshy-bikes-racer.png`, `meshy-bikes-police.png`, `meshy-bikes-night-hero.png`, `meshy-bikes-night.png`. Chụp bởi `AgentScripts/CaptureBikesMeshy.cs` trong Play Mode. Kết quả chi tiết JSON cùng tiền tố `meshy-bikes-`.

## Giới hạn và phát hành

- Đã biên dịch C# Unity thành công, lưu scene/asset và thoát Play Mode. Chưa đo FPS trên điện thoại vật lý; kiểm tra này chạy trong Unity Editor.
- Theo yêu cầu phát hành tiếp theo, **đã build WebGL 1.31.0 thành công** (`build_1e00ac19ccba`, 0 lỗi/1 warning), cache `4f9dbd6172a2b7df`; local browser 83 kiểm tra và static bundle 28 đạt. Xe mẫu rời được giữ EditorOnly, không đóng gói vào game. **Đang chờ commit/push/Pages/live**, xem `huongdan.md` 21.8 và `meshy-release-local-summary.json`.
- Working tree có các chỉnh sửa Wave/Blender/Packages/plugin của người dùng và Claude Code từ trước; giữ nguyên, không tự stage/reset hoặc đưa vào một commit rộng.
- Tái tạo: Python chạy `prepare_meshy_bike_atlas.py`, Blender nền chạy `integrate_meshy_bikes.py`, Unity chạy menu **Phố Việt/Art/Install received Meshy motorcycles (keep police)**; không chạy `UpgradeBikesWave` để thay ngược mẫu hiện hành.
