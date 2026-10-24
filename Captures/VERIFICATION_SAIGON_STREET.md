# Phố Sài Gòn theo ảnh mẫu — 24/10/2026 (Claude Code)

Ảnh mẫu: `ArtSource/Reference/Street/saigon_street_reference.webp`. Ảnh so sánh: `saigon-street-comparison.png`
(mẫu / trước / sau, góc trục đường và góc chơi). Ảnh trước `saigon-street-before-*.png`, sau `saigon-street-after-*.png`.

## Thay đổi

- Nguồn mới `ArtSource/Blender/pho_viet_saigon_streets.py`, dựng trong Blender MCP (scene riêng `PV_SaigonStreets`),
  lưu `ArtSource/Blender/PhoViet_SaigonStreets.blend`. Không ghi vào `PhoViet_Streets.blend` hay các blend cục bộ khác.
- Xuất đè đúng 16 FBX cũ (12 shop, 3 cây, vỉa hè) cùng tên đối tượng/mesh → giữ GUID/fileID, mọi 35 khối phố trên
  5 road rows tự cập nhật, không dựng lại road rows.
- Nhà ống 3–5 tầng kem/vàng/be, ban công lan can sắt, máy lạnh, mái ngói/sân thượng, cây ban công, hoa giấy, cờ,
  biển dọc, nhà cao tầng phía sau; tầng trệt sáng đèn, mái hiên sọc có riềm, biển lớn chữ đậm; vỉa hè caro, bó vỉa vàng,
  chậu cây lớn, thùng rác, bàn ghế, ghế nhựa đỏ, bảng đen thực đơn, kệ hàng, tủ nước đỏ không logo, dù.
- 12 tên shop mới trong `VietnamStreet.Shops`; 490 đối tượng `Blender • …` được đổi tên theo chỉ số model.
- Palette 128 px lưới 8×8 (64 màu), Point/không mip.
- Hộp vật cản đọc từ JSON cạnh FBX: 735 nhóm / 7.955 hộp (trước 4.575). Mặt tiền vẫn |x| = 9,3 m.

## Ngân sách tam giác (LOD0 / LOD1 / LOD2)

| Asset | Trước | Sau |
| --- | --- | --- |
| 12 shop, tổng LOD0 | 229.458 | 81.592 (4.309–8.703 mỗi shop) |
| Cây | 30.076 / 12.316 / 7.132 | 21.026 / 10.190 / 4.670 |
| Vỉa hè | 15.500 / 7.440 / 2.766 | 1.258 / 474 / 156 |

## Kiểm tra Unity (Play Mode, scene đã lưu, art cuối)

13/14 suite đạt, **1.907 điều kiện**: StreetArt 323, DistanceMode 31, QuickKick 62, JunctionKick 216, SmoothTurns 468,
Boost 35, CrashOutcome 51, Gestures 39, CrashUpgrade 97, CrashPoliceNavigation 214, JunctionRoutes 92, Gameplay 23,
MeshyCarRoofs 256. JSON: `saigon-street-Verify*-results.json`, tổng hợp `saigon-street-verification-summary.json`.

- `VerifyStreetArt` được mở rộng (186 → 323 điều kiện): metadata/generator, palette 8×8, ngân sách tam giác ≤ bản cũ,
  số tam giác Unity khớp Blender, tên shop khớp chữ biển, số hộp vật cản khớp JSON, giới hạn kích thước LOD 45 m
  (nhà cao tầng phía sau), jobs khóa.
- `VerifyJunctionRoutes` đọc tên shop từ `VietnamStreet.Shops[0]` thay vì chuỗi cố định cũ.
- **VerifyJump không đạt**: “Upright medium box/drowsy trucks now have eligible roofs” — kiểm tra mái xe tải theo kích
  thước bộ ô tô cũ; suite này chưa chạy lại kể từ bộ ô tô Meshy 1.33 (đã thay bằng VerifyMeshyCarRoofs, đạt 256).
  Không liên quan phố; chưa sửa.

## Chưa làm

Chưa build WebGL, chưa commit/push/Pages. Chưa đo FPS điện thoại thật. Dây điện, xe đậu, người vỉa hè, xe đẩy giữ
nguyên bộ cũ (đèn đường, đèn tín hiệu và bầu trời đã làm lại ở lần hai bên dưới).

## Lần hai: cột đèn đường, cột tín hiệu, mây (24/10/2026)

- `build_saigon_fixtures()` xuất `Art/Lighting/PV_LEDStreetLamp.fbx` (cột cong, 778/378/228 tam giác) và
  `Art/Streets/PV_TrafficSignal.fbx` (cần vươn + hộp treo, 358/304/158). `AgentScripts/InstallSaigonStreetFixtures.cs`
  gỡ 5.824 tam giác cột tín hiệu cũ khỏi mesh gộp, gắn 140 cột mới, dời 420 bóng tín hiệu runtime và 70 tâm quang học.
- `DayNightSky.shader`: mây tích fBm + trời cao xanh đậm ban ngày; đêm mây xám tối.
- 14 suite / 1.804 điều kiện đạt (StreetArt 330 gồm 7 kiểm tra mới, NightJumpLighting 88, MeshyLighting 16, …).
  CrashPoliceNavigation lỗi 1 lần trong lượt chạy chung, đạt 214 ở 2 lần chạy lại riêng (giao thông seed ngẫu nhiên).
- Ảnh Play Mode: `saigon-street-day-*.png`, `saigon-street-night-*.png`; so sánh `saigon-street-comparison.png`.

## Lần ba: tán cây kín lá, xóa người đứng vỉa hè (24/10/2026)

- Tán cây: mặt lá quay ra ngoài, 25% lá lớp trong; 3.000/1.450/680 lá, 24.226/11.790/5.630 tam giác.
- `AgentScripts/RemoveSidewalkPeople.cs`: gỡ 49.560 tam giác người đứng khỏi 42 mesh gộp; `VietnamStreet.Build` không gọi `Person` nữa.
- Xe máy đậu vỉa hè vẫn có người lái ngồi (chờ người dùng chọn).
- 15 suite / 2.019 điều kiện đạt (StreetArt 331, CrashPoliceNavigation 214, …). Ảnh `saigon-street-after-tree.png`,
  `saigon-street-after-people-removed.png`.

## Lần bốn: vách kính, cửa ra vào, nội thất tầng trệt (24/10/2026)

- `glass_front` + `interior` trong `pho_viet_saigon_streets.py`: kính trên submesh riêng (`PV_StreetGlass.mat`, Standard Transparent),
  cửa hai cánh có tay nắm, lòng tiệm sâu 3,4 m có kệ/quầy/bàn ghế/bảng thực đơn theo loại tiệm; tiệm sửa xe mở cửa cuốn.
- 1.350 renderer shop nhận `[PV_StreetAtlas, PV_StreetGlass]`; shop LOD0 5.453–10.613 tam giác.
- 15 suite / 2.020 điều kiện đạt (StreetArt 332). Ảnh `saigon-street-after-storefront*.png`.

## Lần năm: đồ vật vỉa hè và nội thất chi tiết (24/10/2026)

- Thư viện đồ vật mới (ghế nhựa đỏ, bàn gỗ + khay gia vị, bàn ghế cà phê tựa nan, bảng chữ A, chậu cây lá to, tủ kính đồ ăn + nồi,
  tủ nước cửa kính, kệ sắt, két, dù riềm, thùng rác, tủ trưng bày, tủ đồ nghề, lốp) và bố trí theo loại tiệm, cả trong tiệm.
- Vật cản 735 nhóm / 8.120 hộp; shop LOD0 6.309–13.338 tam giác.
- 15 suite / 2.020 điều kiện đạt; CrashPoliceNavigation lỗi chập chờn 1 lần trong lượt chung, đạt 214 ở 2 lần chạy riêng.
- Ảnh `saigon-street-shops-sheet.png` (12 loại tiệm).

## Lần sáu: cây theo ảnh mẫu cây (24/10/2026)

- `tree()` mới: thân xám mảnh, 5 cành chính, 40 cụm lá bầu dục xanh tươi; 29.026/12.414/5.446 tam giác, 3.520/1.500/660 lá.
- Ảnh `saigon-street-tree-comparison.png` (mẫu | Unity), `saigon-street-tree-closeup.png`.
- 14 suite đạt trong lượt chung; CrashPoliceNavigation lỗi trong lượt dài nhưng đạt 214 ở 5 lần chạy riêng — cần làm ổn định riêng.

## Lần bảy: bảng tên dưới tủ kính, cây chậu kiểu cây mẫu (24/10/2026)

- `name_panel` dùng đúng chữ/màu biển hiệu, căn giữa, co chữ theo bề rộng; trên 5 tủ đồ ăn và 2 tủ trưng bày.
- `mini_tree` cho chậu vỉa hè và ban công. Shop LOD0 9.025–19.278 tam giác.
- 15 suite / 2.020 điều kiện đạt. Ảnh `saigon-street-cabinet-panels.png`, `saigon-street-cabinets-sheet.png`.

## Lần tám: xóa xe đẩy bánh mì cũ (24/10/2026)

- `AgentScripts/RemoveBreadCart.cs`: gỡ 2.912 tam giác xe đẩy khỏi mesh gộp, xóa 70 nhóm chữ, bỏ hộp vật cản (8.050 hộp).
- 15 suite / 2.017 điều kiện đạt (StreetArt 333, CrashUpgrade 93 sau khi bỏ loại vật cản không còn tồn tại). Ảnh `saigon-street-bunbo-cart-removed.png`.
