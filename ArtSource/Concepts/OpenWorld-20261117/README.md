# Bản đồ mở Phố Việt — dựng lại toàn bộ cảnh quan (21.238)

Yêu cầu người dùng 17/11/2026: xóa toàn bộ cảnh quan cũ, dựng lại nhà cửa, mặt đường, cầu, sông và mọi chi tiết theo 3 ảnh tham chiếu bằng Blender MCP. Giữ model phương tiện, máy bay, xe tăng, khủng long. Bản đồ là **thế giới mở**, ô tô người chơi lái tự do như game nhập vai bắn súng.

Lựa chọn đã chốt (hỏi đáp 17/11/2026):

| Câu hỏi | Chọn |
| --- | --- |
| Bố cục | Bản đồ mở, xe lái tự do khắp bản đồ (bỏ khung ô phố cuộn 80 m) |
| Asset cảnh quan cũ | Ngắt khỏi game/bản build và chuyển vào thư mục lưu trữ ngoài build, khôi phục được |
| Mức chi tiết | Chi tiết tối đa, chấp nhận nặng hơn trên điện thoại |
| Cách làm | Từng đợt, mỗi đợt có ảnh để duyệt |

## File

- `ref-1-cau-vom.webp`, `ref-2-cho-ben-thanh.webp`, `ref-3-pho-ven-song.webp`: ảnh gốc người dùng gửi.
- `layout.json`: nguồn số liệu duy nhất cho Blender và Unity (đường, sông, cầu, công trình, khu, bảng hiệu, điểm xuất phát). Đơn vị mét, x = đông, z = bắc.
- `build_map_plan.cjs` → `map-plan.svg`: sơ đồ nhìn từ trên. Chạy `node ArtSource/Concepts/OpenWorld-20261117/build_map_plan.cjs`.

## Bố cục v1

- Bản đồ 1,3 × 1,3 km. Sông hình chữ L ôm khu trung tâm: nhánh dọc phía đông, nhánh ngang phía bắc.
- Đường chính 4 làn × 3,5 m (lòng đường 14 m, giống bề rộng hiện tại), vạch đôi vàng giữa, vỉa hè 6 m. Đường nhánh 2 làn, vỉa hè 4 m. Mạng lưới khép kín, không có ngõ cụt.
- Ảnh 3 → đường Tôn Đức Thắng (R2) chạy lên bắc, nhà phố bên trái, sông bên phải. Điểm xuất phát.
- Ảnh 2 → Đại lộ Lê Lợi (R1) chạy sang đông, chợ Bến Thành bên trái, bùng binh trước chợ.
- Ảnh 1 → đường Bạch Đằng (R3) chạy sang đông, sông bên trái, phố bên phải, Cầu vòm Ánh Dương hai vòm trắng phía trước qua bờ đông.
- Tháp Tài Chính dáng búp sen ở (160, −60) (người dùng chọn khu tài chính): thấy phía trước ở ảnh 2 và ảnh 3, lệch phải ngoài khung ở ảnh 1.

## Các đợt

1. Sơ đồ bản đồ — đã duyệt (21.238).
2. Blender: đoạn mẫu Tôn Đức Thắng theo ảnh 3 — đã duyệt (21.239–21.246).
3. Blender: Lê Lợi + chợ Bến Thành (ảnh 2) — đã duyệt (21.240–21.244).
4. Blender: Bạch Đằng + Cầu vòm Ánh Dương (ảnh 1) — đã duyệt (21.247).
5. Blender: phủ toàn bản đồ — xong, chờ xem (21.248, `PhoViet_OW_Map.blend`, `map-tong-hop-dot5.jpg`).
6. Unity: scene/bản đồ mới, lái tự do trong toạ độ thế giới, va chạm, LOD/culling; ngắt và lưu trữ cảnh quan cũ.
7. Unity: chuyển giao thông, CSGT, khủng long, xe tăng, trực thăng, rơi sông sang bản đồ mở; kiểm tra; build khi người dùng yêu cầu.

Lưu ý kiến trúc: gameplay hiện tại cuộn mặt đường dưới xe (`PhoVietRoutes.cs`, `VietnamStreet.cs`, `PhoVietRiver.cs`, `RoadScrollSpeed` trong `PhoVietFreeDrive.cs`). Bản đồ mở cần viết lại lõi lái xe và cách sinh giao thông/hiểm họa ở đợt 6–7.
