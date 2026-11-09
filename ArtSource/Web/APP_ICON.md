# Phố Việt — icon Phố lên đèn

Nguồn chỉnh sửa là `pho-viet-app-icon.svg`, trùng byte với mẫu A gốc đã duyệt
`ArtSource/Concepts/PWA-Icons-20261109/a-pho-len-den.svg`. Hình gồm xe đua xanh,
hai dãy nhà phố và hai cờ đỏ sao vàng nhỏ. SVG dùng hình khối vector và màu
chuyển sắc nội bộ.

Chạy `node AgentScripts/GenerateWebIcons.cjs` với Sharp để xuất PNG 192/512 px,
maskable 512 px và Apple Touch Icon 180 px nền kín. Manifest và HTML dùng đường
dẫn `*-pho-len-den-*` mới. Bản SVG và PNG tên chung cũng được xuất từ cùng
nguồn. Phiên bản maskable dùng đúng khung tranh đã duyệt, với thân xe nằm trong
vùng an toàn tròn.

Khi cập nhật web, đi theo quy trình 15.2 trong `huongdan.md`. Bộ icon được đưa vào
gói cùng manifest, HTML và danh sách cache offline.
