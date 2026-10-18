# Xe máy và người lái phiên bản 2 (gần như thật) — 19/10/2026

Dựng lại cả 6 mẫu xe máy có người lái bằng Blender MCP (Blender 5.2 LTS) rồi cài vào scene Unity đang mở. Tại thời điểm commit `cb95f85`, WebGL/GitHub Pages chưa build lại và còn ở 1.14.0. Bước build/phát hành 1.15.0 tiếp theo được ghi riêng tại [VERIFICATION_BIKES_V2_WEB.md](VERIFICATION_BIKES_V2_WEB.md).

## Phương án người dùng đã chọn

- Tự dựng bằng Python trong Blender, không dùng AI Hyper3D.
- Phong cách gần như thật: tỷ lệ người thật, xe giống xe Việt Nam, không gắn logo hãng.
- Xe của nhân vật chính là tay ga phổ thông màu trắng (kiểu Vision/Lead). Tài xế vẫn mặc áo đỏ có sao vàng 5 cánh ở ngực và lưng.
- Sao lưu `PhoViet_Motorcycles.blend` cũ thành `ArtSource/Blender/PhoViet_Motorcycles.backup-20261019.blend` rồi ghi đè.

## Sáu mẫu

| Asset | Xe | Người lái | Tam giác LOD0 / LOD1 / LOD2 |
| --- | --- | --- | --- |
| `PV_HeroScooter` | Tay ga trắng, bánh mâm 5 chấu kép, phanh đĩa, ống xả phải, giảm xóc trái | Áo phông đỏ sao vàng trước/sau, quần jeans, giày thể thao, mũ nửa đầu trắng có lưỡi trai | 31.818 / 14.318 / 5.408 |
| `PV_CityScooter` | Tay ga xanh ngọc | Nữ, áo chống nắng có mũ trùm, khẩu trang, tóc buộc, dép, mũ hồng | 30.104 / 13.546 / 5.116 |
| `PV_Underbone` | Xe số kiểu Wave, bánh nan hoa, máy lộ, giảm xóc đôi | Áo polo be, quần kaki, dép, mũ xanh rêu | 25.946 / 11.674 / 4.410 |
| `PV_SportBike` | Côn tay kiểu Exciter/Winner vàng–đen, đèn đôi, ốp bình xăng | Áo khoác đen, jeans, mũ fullface đen | 25.442 / 11.448 / 4.324 |
| `PV_Racer` (quái xế) | Côn tay xanh GP, phuộc vàng | Áo phông đen, găng tay, mũ fullface xanh kính đen, cúi người 32° | 25.746 / 11.584 / 4.376 |
| `PV_TrafficPolice` | Mô tô CSGT trắng–xanh, kính chắn gió, thùng hai bên, thùng sau chữ C.S.G.T, cột đèn ưu tiên | Đồng phục vàng cát tay dài, cầu vai đỏ, thắt lưng, găng trắng, ủng đen, mũ trắng có huy hiệu | 26.212 / 11.794 / 4.456 |

## Kỹ thuật

- **Script nguồn:** `ArtSource/Blender/pho_viet_motorcycles.py`, gọi `build_motorcycles()`. `pho_viet_bikes.py` cũ giữ nguyên vì script ô tô, phố và khách vẫn `exec` nó làm thư viện.
- **Thân xe:** các panel loft theo mặt cắt siêu elip, nội suy Catmull-Rom giữa các mặt cắt. Bánh xe dựng bằng revolve, ống dùng sweep.
- **Thân người:** một bề mặt liền khối. Các khối giải phẫu và quần áo đặt chồng lên nhau, rồi voxel remesh 6,5 mm, làm mượt và giảm còn khoảng 6.200 tam giác. Màu được gán theo khối gần nhất. Vạt áo, tay áo và cổ áo được cắt theo mặt phẳng nên viền thẳng.
- **Trọng số skin:** tính theo đoạn xương giải phẫu, tối đa 2 xương mỗi đỉnh. Unity dùng `SkinQuality.Bone2` (trước là Bone1).
- **Đầu:** có hình học mũi, cằm, gò má, môi, mắt, lông mày, tai.
- **Mũ:** nửa đầu có lưỡi trai và quai cằm, fullface có kính, mũ CSGT có huy hiệu.
- **Bàn tay:** remesh riêng, các ngón cuộn quanh tay nắm.
- **Hợp đồng Unity giữ nguyên:** 17 xương cùng tên, xoay nghỉ đồng nhất; tâm bánh `(0,.34,-.73)/(0,.34,.78)`; 7 clip Legacy; 3 LOD skinned; atlas palette 128 px; một material.
- **Ánh kim (metallic) giữ thấp:** scene không có reflection probe, nên vật liệu metallic cao sẽ hiện đen.
- **Mảng hở dựng thành tấm mỏng kín:** sao, kính, khẩu trang, lưỡi trai, chữ biển số, để tính lại normal không làm Unity cắt mất mặt.

## Cài vào Unity

```powershell
unity command run_script --file AgentScripts/UpgradeBikesV2.cs --entry UpgradeBikesV2.Main --timeout_ms 240000 --timeout 250 --json
```

Script này gọi `PhoVietBikeUpgrade.UpgradeOpenScene()` (19 actor gameplay), rồi tạo lại 14 xe đậu vỉa hè ở dãy đường chính, vì upgrader gốc bỏ qua các xe đã chuyển đổi. Sau đó nó bake lại 3 mesh xe đậu tĩnh `Assets/PhoViet/Generated/RouteParked_*.asset` từ LOD1 (giữ nguyên GUID) và lưu scene.

## Kiểm tra (Play Mode, scene đã lưu)

Các suite cơ chế chạy với `jobsEnabled = false` chỉ trong Play Mode; suite công việc chạy với `jobsEnabled = true`. Scene lưu vẫn `jobsEnabled = true`. JSON nằm ở `Captures/bikes-v2-<Suite>-results.json`.

| Suite | Assertion |
| --- | ---: |
| VerifyBikeArt (đã đổi kỳ vọng sang Bone2) | 156 |
| VerifyGameplay | 23 |
| VerifyPursuit | 39 |
| VerifyRiderContacts | 45 |
| VerifyReckless | 120 |
| VerifyRearRandomPolice | 60 |
| VerifyTrafficDynamics | 82 |
| VerifyCrossTraffic | 28 |
| VerifyJunctionRoutes | 92 |
| VerifyJunctionRefinements | 105 |
| VerifyRoadDrivers | 45 |
| VerifySafeArea | 9 |
| VerifyCarArt | 204 |
| VerifyStreetArt | 186 |
| VerifyJobs | 116 |
| VerifyJobEdges | 17 |
| **Tổng** | **1.327** |

Chưa chạy `VerifyMobileCamera` (cần Game View dọc; camera không đổi) và các kiểm tra trình duyệt (chưa build WebGL).

## Ảnh

- `bikes-v2-lineup.png`: 6 mẫu render trong Blender.
- `bikes-v2-game.png`: camera game thật.
- `bikes-v2-hero-rear.png` / `bikes-v2-hero-front.png`: nhân vật chính.
- `bikes-v2-chase.png`, `bikes-v2-police.png`, `bikes-v2-racer.png`: CSGT và quái xế trong Play Mode.

## Hạn chế còn lại

- Bên hông, chỗ vạt áo gặp đùi, còn vài răng cưa nhỏ. Lưng và vạt sau đã thẳng.
- Màu vẫn là palette phẳng 32 ô, không có texture vải/da chi tiết.
- LOD0 khoảng 25–32k tam giác (bản cũ 23,6k). Chưa đo FPS trên điện thoại thật.
- Xe đậu ở các dãy phố nhánh là mesh tĩnh bake từ LOD1, người lái ngồi nguyên tư thế.
