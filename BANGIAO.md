# BÀN GIAO NHANH — Phố Việt

> File này là **điểm bắt đầu cho mọi phiên Codex/Claude Code**. Đọc file này + `git status` + `git log --oneline -5` là đủ để nhận yêu cầu mới. **Không đọc lại toàn bộ `huongdan.md` (≈500 KB) hay toàn bộ mã nguồn**; chỉ mở mục/file liên quan theo bảng tra cứu bên dưới.
>
> Ai vừa sửa/thêm chức năng, build hoặc phát hành **phải cập nhật file này trong cùng công việc** (mục 1–3 và 6), song song với ghi chi tiết vào `huongdan.md`.

Cập nhật lần cuối: **10/11/2026 — Codex**

## 1. Trạng thái hiện tại

| Mục | Giá trị |
| --- | --- |
| Bản đã phát hành | **1.68.0** — WebGL `95ffa4d`, Pages `42731990683` thành công đúng SHA, cache `5ff1a2b82a648276`. Gồm 21.179–21.180. **34 kiểm tra live và 5 kiểm tra bay trên mobile giả lập đạt, 0 lỗi runtime.** |
| Meshy Bridge | **21.176:** đã khôi phục 0.2.5 trong Unity Phố Việt. Menu **Meshy → Bridge** mở, cổng `127.0.0.1:5326` đã kiểm tra, plugin cục bộ chưa commit. |
| Bản chơi | <https://denispham1107.github.io/taixecongnghe/> (thư mục `docs/` trên `main`) |
| Icon cài đặt | **21.167, web 1.64.0:** dùng mẫu A gốc «Phố lên đèn» với cờ nhỏ, có favicon, Apple Touch Icon và bản maskable. |
| Repo | <https://github.com/denispham1107/taixecongnghe>, nhánh `main` |
| Unity | 6000.5.6f1, Built-in RP, Legacy Input, IMGUI (`OnGUI`); scene `Assets/PhoViet/Scenes/PhoViet.unity` |
| Simulator iPhone | **21.71:** đã thêm **Apple iPhone 14 Pro / 14 Pro Max**, Dynamic Island + Safe Area dọc/ngang; đang chọn Pro Max ngang, Safe Area bật. Asset chỉ trong Editor, chưa commit/push |
| Chế độ chơi | **Web1.53.0/21.118:** reset quãngvăng trước dựngđường để chơi lại rẽđúng/khônglệchnhà. **21.115:** điểm/kỷlục dừngngaylúcva chạm, không cộngquãngvăng; physics giữ; truy đuổi CSGT tạm khóa trong source21.155. Chỉ thi quãng đường/kỷ lục. **Công việc/khách/GPS/Coin HUD đang khóa** (`RideJobsReleased=false`, scene `jobsEnabled=false`) |
| Tài liệu bàn giao | `BANGIAO.md`/`AGENTS.md`/`CLAUDE.md` đã commit riêng lên `main` (21.56) |
| Người chơi trong source/scene | **Blue Petshop Supercar** xanh, thu nhỏ 15%. Q/chạm hai lần móc xích trong 4 m khi chưa bay, lộn khi bay (21.155, web 1.61.0). Space/vuốt lên nhảy với bốn luồng khói trắng. W/F/vuốt xuống bắn tên lửa. Boost tạm khóa và giữ mã hiệu ứng hai ống bô. |
| Bay cao và máy bay hai tầng cánh | **21.177, đã có trên WebGL 1.67.0:** giữ Bay lên tối đa 2,2× (9,96 m), tiêu hao 10–40 mỗi 0,5 s. Crimson Skywing sinh 2/4/6 chiếc theo độ cao, dùng tên lửa như trực thăng. |
| Bay và nhảy | **21.169, web 1.65.0:** nút Bay và nhảy rồi giữ cùng đỉnh **4,52832 m** từ mặt đỡ. Đỉnh nhảy tăng 10%, cố định theo tốc độ. Lệnh giữ lúc đang rơi sẽ nâng lên đỉnh. |
| NPC xe máy trong source21.67 | **Urban Rider mới:31actor(17traffic+14parked)+56baked nhánh**, rig19xương/3LOD42k/18,5k/6,7k, cổ+tay+thân đồngbộ; **đã có trên web1.41.0**. PlayerSupercar/CSGT giữ nguyên |
| Tối ưu mobile | **21.136/21.138 đã có trên web 1.57.1:** mỗi mức có plan riêng + bộ tự giảm trên cảm ứng; desktop **Rất cao (source21.144)** giữ baseline trừ cascades WebGL; Rấtcao không tựgiảm. Layer ẩn sớm gồm đèn tín hiệu và đèn đường từ source21.147 |
| Cài đặt đồ họa trong source | **21.144 đã có trên web1.59.0:** Rất cao(cũCao, cốđịnh)/Cao(mới,tựgiảm)/Trung bình/Thấp. IDlưu0/3/1/2 theo thứtựUI, không đổi pref cũ. DesktopRấtcao fullbaseline/mobileRấtcao planCao cũ; không governor/learneddrop/đổiHz. Cao có LOD0,6, tầm150m và phản chiếu448px. **21.147 có trên web 1.60.0:** giữ mật độ điểm ảnh và MSAA của Rất cao ở cả bốn mức để chữ biển rõ. |
| Trang chủ HTML | **21.168, web 1.64.0:** ba JPEG 2778 × 1284 của người dùng theo thứ tự gửi, khớp SHA, toàn khung và đã gỡ hai ảnh cũ. |
| Xuất phát trong source | **21.150:** bắt đầu và chơi lại từ 0 km/h, tăng khoảng 22 → 43 → 65 km/h ở giây 1–3. Xe và quãng đường dùng tốc độ thực. Đã có trên web 1.61.0. |
| Xe mặt đất lúc mở màn trong source | **21.151:** các xe có mặt lúc bắt đầu tăng dần từ đứng yên đến nhịp riêng trong khoảng 3,01 giây. Áp dụng cho các nhóm giao thông, xe đặc biệt và nhánh CSGT nếu được mở lại. Đã có trên web 1.61.0. |
| Bánh xe | **21.164, web 1.64.0:** bánh lăn theo vận tốc thực, giữ quán tính rồi ngừng sau va chạm. Xe đua và bốn mẫu ô tô Meshy có chi tiết trên mâm. |
| Phản hồi đổi làn trong source | **21.151:** xe người chơi đổi làn nhanh thêm 15% theo cả hai hướng, hệ số 1,53065 × 1,15, đã có trên web 1.61.0.  **21.81 đã có trên web1.46.0:** gain đầu quỹ đạo car + yawattack0,05s; nhận phím/vuốt ngay, giữ nhịp hoàn tất.779kiểm tra đạt/4ảnh đã xem; chi tiết huongdan21.81 |
| Đợt bổ sung sau rẽ và mỗi400m trong source | **21.114 đã có trên web1.53.0:**2bike sau cua sinhngay/tạt làn/vịtrígần giữ; rear thường/quái xế/tải ngủ gật báo0,6s. Rear làncùngchiềugầnnhất; oncoming sinh sauDino/Tank ởphíaplayerhơn; Tank70%/junction−24m/bắn0,4s/lịch400m-9s/Dino10s giữ |
| Pickup tên lửa mẫu C đã duyệt | **21.131 đã có trên web1.55.0:** Hex Ammo nền lục giác xanh/viền vàng/mũi đỏ;3LOD1482/1014/592tri,pool70.735Unity đạt/5ảnh đã xem; chỉ pickup đường, HUD/nút/đạn bay giữ. Approvaltrue/C |
| Nút tên lửa/âm lượng/ảnh trang chủ mới | **21.134 đã có trên web 1.56.0:** nút góc trái dưới, master 1,56×/+3,86249dB, `poster-home-flight.png` dưới ảnh cũ |
| Tên lửa địch và âm lượng tổng | **21.180, web 1.68.0:** tên lửa xe tăng/trực thăng/Skywing 36 m/s, thẻ kết thúc ghi đúng nguồn. Master +30% ở 21.132 vẫn có trên web. |
| Vệt lửa/khói tên lửa trong source | **21.129 đã có trên web1.55.0:** plume vàng-cam theo đường bay/6,5m/0,14s/32điểm, khói32hạt/0,6s rõ hơn; chung52player+heli/pause-scroll-rebase-pool.722Unity đạt/5ảnh đã xem; UnityEdit/scene sạch |
| Tên lửa người chơi | **21.179, web 1.68.0:** hồi 0,25 giây. Kho 5/5, nhặt +1; bắn thẳng khi thiếu mục tiêu. Dưới đất và khi bay giữ các loại mục tiêu theo 21.169/21.173/21.177. |
| Va chạm phương tiện trong source | **21.179, web 1.68.0:** mảnh 3D và tia lửa tại tiếp xúc, tăng theo lực tông. Áp dụng xe–xe, người chơi–máy bay, máy bay–máy bay và máy bay–xe dưới đường. Giữ lửa/khói 21.127 và ngoại lệ cháy xe tăng. |
| Trực thăng | **21.180, web 1.68.0:** bay 18,4 m/s ngay khi xuất hiện. Theo 21.174, có 1/2/3 chiếc ở mốc 0/200/400 m cộng mỗi lần rẽ xong 1 chiếc. Nhịp tên lửa giảm 0,1 giây/lượt rẽ, sàn 0,25 giây. |
| Khủng long | **21.173, web 1.66.0:** thanh máu 3 nấc trên đầu. Tên lửa bắn từ mặt đất có thể hạ khủng long. Mỗi phát trúng thêm một điểm lửa/khói. Phát thứ ba làm khủng long ngã; xác cháy và vẫn gây va chạm. |
| Bộ icon HUD trong source | **21.154:** B Arcade đã duyệt. Bỏ hai cánh nhô ở Bay/Hạ, áp dụng cả bốn nút. Số đạn động, bố cục và vùng chạm giữ theo 21.152. Đã có trên web 1.61.0. |
| Ga và Phanh trong source | **21.152:** Ga trên, Phanh dưới ở góc phải, bấm một lần giữ chế độ. Phanh giảm đến 0 và giữ xe, Ga tăng dần trở lại. Xe khác có tốc độ riêng. Bay và Phóng tên lửa lớn thêm 15% ở màn hình ngang. Đã có trên web 1.61.0. |
| Nút bay / icon trên không | **Source21.145:**6iconđất+7iconbay mỗi tile (+4mỗiloại), pool210+245/35tile. Bayvẫnchỉhiệnicontrênkhôngkhiengaged; nút bay trên nút tên lửa; vuốt lên 8,5%, góc rộng, nhảy hồi 1,6 giây + đệm 0,55 giây |
| Năng lượng / bay / lộn | **21.148 có trên web 1.60.0:** bắt đầu và tối đa 180. Biểu tượng xanh dưới đất hồi 15, biểu tượng cam trên không hồi 20. Nhảy tốn 5, bay tốn 10 mỗi 0,5 giây. |
| Bộ nhà mới đang chờ duyệt | **21.86:**19mẫu Blender/6ảnh/board tại`ArtSource/Concepts/City-20261029/`; `approval.json=false`, **chưa tích hợp Unity/web**, cây giữ mesh hiện tại. Cầu21.85 vẫn chờ chọn riêng |
| Trang trí cầu hiện tại trong source | **21.87 đã sửa:** bỏ12chậuhoa/cầu,8cờ dùng16tayđỡngangxanhcóđaikẹp,đủ3LOD;45Blender+27assetUnity+101sceneđạt/6ảnhđãxem. **Đã có trên web1.48.0**,khôngápdụngmẫucầu21.85 |
| Đèn giao thông/điều hòa/biển thuê nhà | Signal2mặt/ACxámtrắng đã có trên web1.48.0. Web1.49.0 có24biển cỡ21.94. **Source21.97 thêm14biển đúng tầng theo ảnh →38biển/3LOD**, giữ cỡ/màu/vị trí24biển cũ; đã có trên web1.50.0 |
| Chất liệu tường trong source | **21.98:** người dùng chọn vân rõ hơn21.97; relief0,42/scale0,65/contrast0,28/smoothness0,015, giữ màu/kính/kim loại/sông. UV mask25FBX/144mesh, texture1K nén/mipmap, không thêm renderer. Chi tiết21.97–21.98; đã có trên web1.50.0 |
| Cờ ban công trong source | **21.88 đã sửa:** bỏ thanhđen cạnh cờ ở8biếnthểnhà/24LOD,giữcờ-sao/hoa/lan can;64Blenderđạt/24mesh+510refsUnitygiữnguyên,4ảnhđãxem. **Đã có trên web1.48.0** |
| Góc đèn pha trong source | **21.108 đã có trên web1.53.0:** khôi phục đèn gốc1.52, chỉmởgócngang1,8×/spot≈90°;bỏreference0,65/mirror21.107,giữhướng/cườngđộ/tầm32m/nguồnđèn.435kiểmđạt/ảnhgốc-vàmởgóccùngframe |
| Mưa giông/sét trong source | **21.179, web 1.68.0:** tiếng nổ và rền 2,2 giây theo vị trí từng tia. **21.109 đã có trên web1.53.0:** mỗi đợt giữ tia cũ +3tia đồng thời/3làn khác nhau trong tile hiện tại, báo0,8s;763kiểm đạt/4ảnh thật. **Web1.52.0 còn21.105:**15s quang/3s tăng/9s giông/3s tan; xe trúng cháy/văng/nằm lại, player hết lượt, Tank/Dino miễn, CSGT hết đuổi |
| Hố đạn và chỉnh lái trong source | **21.103 đã có trên web1.51.0:** hố R2,024m/lòng−0,40m/nứt/mảnh văng; bánh chạm kéo lệch tới khi nhấn/vuốt ngược, nhảy tránh được. **1.090 kiểm tra đạt/11 ảnh Unity**, không đổi hình ảnh ngoài hố. Xem21.103 |
| Tàu và đèn phụ trong source | **21.102 đã có trên web1.51.0:** tàu đánh cá Meshy140chiếc,3LOD/texture2K; đènnhỏ chungtínhiệu2mặt vớiđènlớn;3346kiểmđạt |
| Thay đổi cục bộ ngoài phát hành | Simulator21.71/Packages, Art/Wave/.blend và Captures lịch sử giữ cục bộ, không reset/stage toàn bộ. **21.70/21.72–21.74 đã có trên web1.43.0**; **21.76 kéo điểm xuất phát và21.77 sửa Safari/bộ nhớ đã lên1.44.0**; xem mục2 |

## 2. Việc vừa làm gần nhất (mới nhất ở trên)

- **21.181 đã phát hành WebGL 1.68.0:** ETC2/EAC + DXT, 399,105 giây. 220 trước + 220 sau build, 604 SHA, 48/39 gói, 603 bố cục, 22 Chrome cục bộ và 42 Git index. Commit `95ffa4d`, Pages `42731990683`, cache `5ff1a2b82a648276`. 34 kiểm tra live + 5 kiểm tra mobile giả lập đạt.

- **21.180:** trực thăng +15%, tên lửa địch −10%, thẻ kết thúc ghi xe tăng/trực thăng/máy bay hai tầng cánh. 106 kiểm tra Play Mode đạt. Có trên web 1.68.0.

- **21.179:** va chạm phương tiện có mảnh văng theo lực tông, tên lửa người chơi hồi 0,25 giây và sét có tiếng nổ/rền. 866 kiểm tra Play Mode đạt, đã xem ảnh Unity. Có trên web 1.68.0.

- **21.178 đã phát hành WebGL 1.67.0:** ETC2/EAC và DXT, 172,159 giây. 214/220 kiểm tra Unity trước/sau, 603 bố cục, 22 Chrome cục bộ và 42 Git index đạt. Pages `42685816470`, commit `18dddeb`, cache `d7ae63b6ac6d824e`. 34 kiểm tra công khai và 5 kiểm tra bay khổ dọc đạt.

- **21.177:** bay cao, tiêu hao năng lượng và máy bay Crimson Skywing đã kiểm tra xong trong Unity. 619 điều kiện Unity và 10 chuỗi thao tác JavaScript đạt. Trở về Edit Mode. Đã bỏ bản xem trước khỏi scene đang mở. Tính năng đã có trên web 1.67.0, xem 21.178.

- **21.176:** khôi phục Meshy Bridge chính thức, biên dịch Editor và kiểm tra HTTP 200/403. Unity Phố Việt đang mở Bridge để nhận mô hình. Xem mục 13.3 và 21.176.

- **21.175 đã phát hành WebGL 1.66.0:** ETC2/DXT trong 449,321 giây. 208 kiểm tra Unity trước/sau, 603 bố cục, 18 Chrome cục bộ và 34 live đạt. Pages `42664763165`, commit `6fcae72`.
- **21.174:** mỗi lượt rẽ hoàn tất cộng 1 trực thăng, giảm 0,1 giây giữa các phát tên lửa địch tới sàn 0,25 giây. 322 kiểm tra Play Mode đạt. Có trên web 1.66.0.
- **21.173:** thêm thanh máu, tên lửa mặt đất và xác khủng long cháy gây va chạm. 1.295 kiểm tra Play Mode đạt và đã xem 3 ảnh Game View. Có trên web 1.66.0.
- **21.172 đã phát hành WebGL 1.65.0:** ETC2/DXT trong 490,455 giây. 207 kiểm tra Unity trước/sau, 603 bố cục, 18 Chrome cục bộ và 34 live đạt. Pages `42629982323`, commit `b7f7f92`.
- **21.171:** đổi mốc giữ bay sang 0/200/400 m cho 1/2/3 chiếc. 128 kiểm tra Unity Play Mode đạt. Có trên web 1.65.0.
- **21.170:** bỏ việc chờ đợt sau rẽ mới sinh trực thăng thường. Giữ bay sinh ngay, tối đa 3 chiếc tính cả đợt sau rẽ. Mốc 400/800 m của bước này đã được thay ở 21.171 trước khi phát hành.
- **21.169:** đồng bộ nút Bay và giữ bay ở đỉnh nhảy mới +10%. Tên lửa từ trên không hạ được xe tăng. 205 kiểm tra Unity Play Mode đạt. Có trên web 1.65.0.
- **21.168 đã phát hành WebGL 1.64.0:** đèn phanh, ba ảnh trang chủ, icon A và bánh xe. Build 467,457 giây. 17 kiểm tra đèn phanh, 603 bố cục, 18 WebGL cục bộ và 34 live đạt.
- **21.167:** dùng nguyên SVG mẫu A gốc đã duyệt. Xuất bộ PNG 192/512/maskable/Apple 180 với URL `*-pho-len-den-*`. 17 kiểm tra Sharp/Chrome đạt. Có trên web 1.64.0.
- **21.166:** dựng ba biến thể mẫu A với cờ đỏ sao vàng lớn hơn. Đã xem bản 32 px, 48 px và mặt nạ tròn. Web vẫn 1.63.0.
- **21.165:** thiết kế 5 icon cài đặt Phố Việt theo xe đua xanh, đường phố, xe bay, trực thăng/tên lửa và nhặt năng lượng. Đã xem bảng render và thử bố cục Chrome ở 4 chiều rộng. Đây là mẫu duyệt, web vẫn 1.63.0.
- **21.164:** nối tốc độ thực với trục bánh của xe người chơi và xe giao thông. Bánh giảm quay sau va chạm, lăn lại khi NPC chạy tiếp. 113 kiểm tra Play Mode đạt. Có trên web 1.64.0.
- **21.163 đã phát hành WebGL 1.63.0:** ETC2/DXT 360,644 giây, 204 trước + 204 sau build, 585 SHA giữ nguyên, 43 kiểm gói Git + 34 gói Pages + 35 index. Chrome local 18, bố cục 459 và live 29 đạt. Pages `42508754502` đúng SHA `062e06f`.

- **21.162 đã có trên web 1.63.0:** canvas mở dọc theo viewport được hiệu chỉnh trên iPhone; xe tăng/trực thăng của đợt sau rẽ bắn ngay phát tên lửa đầu khi xe người chơi đang bay và ở trong tầm.

- **21.161 đã có trên web 1.63.0:** trực thăng sau rẽ xuất hiện sau khủng long 0,5 giây. 35 kiểm tra Play Mode đạt, gồm cả hai hướng rẽ và tạm dừng/chơi lại.

- **21.160 đã phát hành WebGL 1.62.0:** ETC2/DXT 389,628 giây, 203 kiểm tra trước + 203 sau, 585 SHA giữ nguyên, 43 kiểm tra gói repo + 34 gói Pages + 35 Git index. Chrome local 16, bố cục 459 và live 29 đạt. Pages `42503724591` đúng SHA `a5e3afb`.

- **21.159 đã có trên web 1.62.0:** xe tăng bắn tên lửa mỗi giây khi xe người chơi đang bay, chuyển về pháo 0,4 giây khi dưới đất. Tên lửa dùng pool và hiệu ứng chung với trực thăng. 20 kiểm tra Play Mode đạt.

- **21.158 đã có trên web 1.62.0:** trực thăng xuất hiện trên đầu khủng long ngay khi rẽ xong và đợt giao lộ tiếp theo được sinh, kể cả khi xe ở dưới đất. Giới hạn 1 chiếc đến hết 500 m, tối đa 2 sau mốc. Biên dịch sạch, 68 kiểm tra Play Mode đạt.

- **21.157 đã có trên web 1.62.0:** gạch caro hai bên hiện ở cả 3 LOD, engine riêng +30%, có tiếng phóng và vòng âm bay cho xe người chơi/trực thăng. Kiểm Play Mode 2.335 điều kiện trên 560 nhóm vỉa hè và vòng đời audio; đã xem hai ảnh.

- **21.156 đã phát hành WebGL 1.61.0:** ETC2/DXT build 417,176 giây, 200 preflight + 200 postflight, 589 SHA giữ nguyên, 43 repo + 34 site + 35 index, 16 Chrome + 459 bố cục. Pages `42404611482` đúng SHA `ed35ea9`, 29 live đạt.

- **21.155 đã lên web 1.61.0:** thay húc bằng dây xích Blender MCP, chọn một xe máy gần nhất trong bán kính 4 m, kéo ngã và tách ra khỏi xe người chơi. Q/chạm hai lần vẫn lộn ngang khi bay. Tạm khóa CSGT. Biên dịch 0 lỗi/cảnh báo; 201 kiểm Play Mode đạt, đã xem ảnh.

- **21.154 đã lên web 1.61.0:** B Arcade đã duyệt và áp dụng, bỏ hai cánh nhô ở Bay/Hạ. Atlas 1024×512, 17 kiểm mới + 987 kiểm hồi quy trên 24 bố cục. Đã xem Game View dọc/ngang.

- **21.153:** dựng ba bộ tham khảo A Buồng lái, B Arcade và C Kính neon. Người dùng đã chọn B ở 21.154. 12 icon vector và hai ảnh so sánh tại `ArtSource/Concepts/DriveControls-20261108/`. Chỉ bản xem trước.

- **21.152 đã lên web 1.61.0:** hai nút Ga/Phanh giữ chế độ, tốc độ NPC độc lập, Bay/Tên lửa ngang +15%. 1.017 kiểm tra và 24 cấu hình bố cục, đã xem hai ảnh Game View. Unity Edit Mode, cảnh sạch.

- **21.151 đã lên web 1.61.0:** thêm tăng tốc mở màn cho xe mặt đất và tăng 15% tốc độ đổi làn xe người chơi. 841 kiểm tra mới, 1.790 kiểm tra tốc độ người chơi và 66 kiểm tra điểm xuất phát đạt. Unity Edit Mode, cảnh sạch.

- **21.150 hoàn tất trong Unity:** xuất phát/chơi lại từ 0 km/h rồi tăng dần. Dự báo xe cắt ngang bám nhịp tăng tốc. 1.790 kiểm tra xuất phát và 66 kiểm tra điểm xuất phát đạt. Chưa build hoặc phát hành.

- **21.149 đã phát hành 1.60.0:** `a1d3a7f`/Pages `42347958136`/cache `09eaa621cf166826`. ETC2/DXT build 343,431 giây, 0 lỗi. 194 preflight + 194 postflight, 585 SHA giữ nguyên, 43 kiểm tra repo + 34 site + 35 index, 13 browser local và 29 live đạt. Đã xem HUD 180/180 trên web và xác nhận vào chơi/tạm dừng.

- **21.148 có trên web 1.60.0:** năng lượng 180/180, sét cam trên không +20 và sét xanh dưới đất +15. 338 kiểm tra Unity đạt và đã xem ảnh Game View.

- **21.147 có trên web 1.60.0:** giữ mật độ điểm ảnh và MSAA của Rất cao cho bảng hiệu và “CHO THUÊ NHÀ” khi đổi mức hoặc tự giảm. 238 kiểm tra Unity đạt, hai cặp ảnh Rất cao/Thấp đã xem.

- **21.146 đã phát hành1.59.0:** commit `7e64e1d`/Pages`42294743068`success/cache`c6699b769f95f932`; gộp21.142–145. Build2variant378,570s/0lỗi/1warningPipeline;193pre+193post/584SHAgiữ/43repo+34site+35index/575browserlocal/29liveđạt/0runtime. UnityEdit/scene sạch/jobsfalse/boostfalse/server8786đãdừng. Kết quả và2docs/README lưu riêng sau commit artifact.

*Bốn mục21.142–145 bên dưới ghi trạng thái tại lúc commit source; toàn bộ nay đã có trên web1.59.0.*

- **21.145 hoàn tất source/chưa phát hành (Codex):** thêm4NLmỗiloại→6đất+7bay/phố-cầu/lànrandom/cầuđấtgiữ2làngiữa. W/F/vuốtxuống/nútbắn dướiđất homingtankgầnnhất/1hit; tankdừngchạy-bắn/cháylửa-khói2,8x/xácsolidkhônghồi. Baybắntrực thănggiữ;5/5/chờ0,4s. **1.215Unityđạt**,3ảnhngày-đêm-bayđãxem. RootparticlesLocal trướcđâybỏparentScale; tankdùngHierarchy, reuseactor khác trảmodegốc. UnityEdit/scene sạch/jobsfalse/boostfalse; commitlocal+2docs,chưa build/push/web(1.58.0).

- **21.144 hoàn tất source/chưa phát hành (Codex):**4mức Rấtcao/Cao/Trungbình/Thấp; Rấtcao giữ Cao cũ và không tựgiảm, Cao mới giữa2plan/tựgiảmdesktop-mobile. GiữID0/1/2, CaoID3; governor theo rank/không đổi lựa chọn.4hàng/modalresponsive/phím1–4/lưuprefs. **233đạt** (59mới+112settings+62mobileperf/34khungroadcull lệch tốiđa1pixel).8ảnhđãchụp/4modalđãxem. UnityEdit/scene sạch/jobsfalse/boostfalse; source/tests/2docs lưucommitlocal,chưa build/push/web(1.58.0).

- **21.143 hoàn tất source/chưa phát hành (Codex):** bay tối thiểu+20%→3,45799m so mặt đỡ, nhảy4,11665m giữ; Space/vuốt lần2 nâng lên cả lúc đang rơi; nútbay/icon/trực thăng đồngbộ. Khođạn5/5, pickup+1/cap5. **805Unityđạt** (172altitude+264missile+94flight+53input+222enemymissile),2ảnhGameviewdọcngangđãxem/HUD5/5. UnityEdit/scene sạch/jobsfalse/boostfalse; lưucommitlocalcùng2docs,chưabuild/push/web(1.58.0).

- **21.142 hoàn tất source/chưa phát hành (Codex):** W/F/vuốt xuống bắn tên lửa **chỉ khi bay**, giữ nút/kho3/3/cooldown0,4s; `BoostReleased=false`, không mở lại nếu chưa được yêu cầu. Cả icon đất/trên không xác nhận **+15**, cap150 (mã vốn15; bay vẫn tốn10/0,5s). **447 kiểm tra đạt** (51 mới+94flight+262missile+40gesture). Sửa fixture flight loại pickup ngẫu nhiên khỏi kiểm trừNL; gesture theo mapping mới. UnityEdit/scene sạch/jobsfalse; source+tests+2docs lưu commit cục bộ, chưa build/push/web (web1.58.0).

- **21.141 đã phát hành 1.58.0 (Claude Code):** commit `6fc558b`/Pages `42092459874` success/cache `5b4a43086927aac5`; live 28 + tên lửa 11 + nút bay 9 + vuốt 22 đạt/0 runtime; 540 browser local. Gồm 21.140.

- **21.140 đã có trên web 1.58.0 (Claude Code):** màn hình ngang từng bỏ qua vuốt ở dải dưới, có góc chết 37–52°, ngón nghỉ chặn ngón vuốt; đã sửa, chạm thật qua shell cũ 12/22 → mới 22/22. Thêm nút bay trên nút tên lửa và 3 icon năng lượng mỗi tile ở độ cao bay. `VerifyFlight2140` 94 đạt.

- **21.139 đã phát hành 1.57.1 (Claude Code):** commit `fcd8fa0`/Pages `42074745472` success/cache `9ff81f1d0cf14557`; 28 live + 11 tên lửa live/0 runtime; 540 browser local đạt. Gồm sửa vùng xanh 21.138.

- **21.138 đã có trên web 1.57.1 (Claude Code):** bản 1.57.0 ẩn nhầm mặt đường ở xa, lộ nền xanh nhạt sau giao lộ. Đã bỏ `Geometry_*` khỏi nhóm early-cull, thêm kiểm tra chống tái phát; ảnh 3 mức đã xem.

- **21.137 đã phát hành 1.57.0 (Claude Code):** commit `0845f08`/Pages `42044425442` success/cache `d612d524b1a39e3a`; 28 live + 11 tên lửa live/0 runtime; 540 browser local đạt. Gồm gói 21.136.

- **21.136 đã có từ web 1.57.0 (Claude Code):** tối ưu giật lag mobile. Giả lập DPR3+CPU×4 FPS 1.56→mới có tự giảm: Cao 4,4→38,8; Trung bình 5→57,2; Thấp 6,9→56,1; draw call Thấp 886→179. Gói mobile 116,6→70,3MB. Gồm `GraphicsPlan`, bộ tự giảm, tắt tile ngoài tầm, một nguồn sáng hướng, bỏ cascades, texture mobile 1K, tạo sẵn tài nguyên/shader, rAF. Chi tiết 21.136.

- **21.135 đã phát hành 1.56.0 (Claude Code):** commit `ecb0b02`/Pages `41992250686` success/cache `03fc6346d88758ed`; 28 live + 11 tên lửa live/0 runtime; 540 browser local đạt. Gồm gói 21.134.

- **21.134 đã có trên web 1.56.0:** nút tên lửa chuyển sang góc trái dưới (thanh gợi ý dời sang phải nút); `MasterGain` 1,3→1,56/+3,86249dB, đo thật 1,5695×; trang chủ có **2 ảnh**, ảnh mới xe bay + 2 trực thăng + tên lửa sắp tới xe ngay dưới ảnh cũ. Đạt: PlayerMissiles 262, Audio 67, Scene 115, Release155Scene 182, Landing 459.

- **21.133 đã phát hành 1.55.0 (Codex build, Claude Code kiểm tra cuối/phát hành, không rebuild):** commit `86b26ec`/Pages `41983779125` success/cache `7a2f7e521dd62b06`; 27 live + 11 tên lửa live/0 runtime; 468 browser local. Sửa 2 assertion sai trong `VerifyMissileReleaseWeb` (gain theo từng nguồn 0,136×1,3; nút trong suốt lộ vạch đường), không đổi game. Chi tiết 21.133.

- **21.132 hoàn tất source/chưa phát hành:** đạn trực thăng40m/s (−20%), âm lượngtổng1,3× bằng1mastervolume+2,2788668dB,routeđủengine/fx/siren/rain/5rotor/voice, khôngnhânlặp/giữmute-pause-pitch.1016Unityđạt+RMSđo1,30528×; so3201baseline chỉ6runtime đổi,khôngđổiscene/assetđồhọa. UnityEdit/scenesạch/jobsfalse; lưucommitlocal+2docs,chưa build/push/web(vẫn1.54.0).

- **21.131 C đã tích hợp source/chưa phát hành:** userchọnC; geometry Blender3LOD1482/1014/592tri/70pool, hộp lục giác xanh/viền vàng/mũi đỏ, chỉ pickup đường.735Unityđạt/5ảnh ngày-đêm-dọc-ngang-cận đã xem; so3199baseline chỉprefab+3mesh đổi. Installer mới cập nhật GPUbuffer giữGUID; HUD/nút/đạn bay/gameplay/scene/môi trường giữ. UnityEdit/scenesạch/jobsfalse/Blendergốc; lưucommitlocal+2docs,chưa build/push/web(vẫn1.54.0).

- **21.130 concept xong, sau đó C được duyệt/tích hợp21.131:** dựng3pickup tên lửa bằng BlenderMCP,2ảnh studio/mặt đường đã xem; nguồn/script/approvalfalse tại `ArtSource/Concepts/MissilePickup-20261105/`. So3199file runtime/web SHA giữ nguyên, Blender về scene gốc8object; chưa tích hợp/build/push/web. Lưucommitlocal cùng2docs. Chỉ làm tiếp mẫu được chọn, không đổi HUD/nút/đạn bay21.129.

- **21.129 hoàn tất source, chưa phát hành:** lửa vàng-cam có turbulence bám sau đuôi/đường bay, khói trắng xám rõ hơn;32điểm/6,5m/0,14s +32hạt/0,6s, chung52missile.722Unity (65FX+261player+222heli+174scene) đạt,5ảnh ngày/đêm/dọc/cận/2chiều đã xem. Giữ gameplay/atlas/môi trường; pause-scroll-rebase-pool đúng. UnityEdit/scenesạch/jobsfalse; lưucommitlocal+2docs,chưa build/push/web(vẫn1.54.0). Chi tiết21.129.

- **21.128 hoàn tất source, chưa phát hành:** kho3/3,nhặt+1/2icon khác làn mỗi phố-cầu,nút tròn/F khi giữ bay,chờ0,4s; tự khóa trực thăng gần nhất phía trước/homing,không mục tiêu vẫn bắn thẳng. Trúng máy bay cháy/khói/rơi; pool12/pickup70/3LOD868-504-252/HUD256vector; nút>=56CSSpx,tap không thành roll.1278Unity+436browser+3refreshđạt/5ảnh đã xem. UnityEdit/scenesạch/jobsfalse/server8787dừng; lưucommitlocal+2docs,chưa build/push/web(vẫn1.54.0). Chi tiết21.128.

- **21.127 hoàn tất source, chưa phát hành:** lửa/khói tại từng mặt va chạm player–NPC/NPC–NPC/CSGT/húc ngang; tông tiếp mặt khác thêm điểm mới, Tank miễn (kể cả tông xác sát tường). Người dùng chọn xe hồi phục vẫn chạy kèm lửa/khói; giữ3s/CSGT1,2s/score/roof/pursuit. Pool Mantaflow riêng/neo local/gộp0,65m/reclaim generation-sông-reset;802Unity đạt/4ảnh ngày-đêm-dọc-hồi phục đã xem. UnityEdit/scene sạch/jobsfalse; lưu commit local +2docs; chưa build/push/web(vẫn1.54.0). Chi tiết21.127.

- **21.126 hoàn tất source, chưa phát hành:** tên lửa BlenderMCP/3LOD/pool40/v50m/s tại21.126 (hiện40m/s theo21.132),1→0,8→0,6→0,5s mỗi400m; ngắm lúc bắn/bay thẳng. Đổi làn/hạ/lộn né đạn; đâm thân máy bay vẫn rơi cháy cả2, kết thúc kể cảCSGT.1028Unity+387layout+3refresh đạt;7ảnhUnity+Blender đã xem, giữscene/modelmôi trường/texture. UnityEdit/jobsfalse; lưucommitlocal cùng2docs, chưa build/push/web(vẫn1.54.0). Chi tiết21.126.

- **21.125 hoàn tất source, chưa phát hành:** tăng trực thăng2→3→4→5 theo mốc vượt400m, cả4làn/hai chiếc đầu khác làn/so le12m/pool5.286fleet+131jump+46rẽthật+158scene=621Unityđạt;4ảnh2/5chiếc dọc/ngang đã xem. Giữ mesh/texture/rotor/audio/va chạm/nhảy70%. UnityEdit/scene sạch/jobsfalse; lưucommitlocal cùng2docs, chưa build/push/web (vẫn1.54.0). Chi tiết21.125.

- **21.124 hoàn tất source, chưa phát hành:** trực thăng Meshy một chiếc/lần, ngược chiều ngang tầm xe khi bay, hai rotor/âm3D/collision; nhảy bật ngay, vuốt lần2 sớm chờ70% đỉnh mới giữ. **700Unity+387layout+3refresh đạt**, đủ5ảnh đã xem;3LOD14k/7,3k/3,4k/texture2K/pool1. UnityEdit/scene sạch/jobsfalse; lưu commit cục bộ cùng2docs, chưa build/push/web (vẫn1.54.0). Chi tiết21.124; kiểm mobile máy thật/WebGL mới chưa chạy.

- **21.123 đã phát hành1.54.0:**commitee18d91/Pages41645004184success/cache6147ef260e9458a8;27liveđạt/0runtime/toànbộbyteskhớp. GồmC/nănglượng150/nhảy−5/nhặt+15/bay−10mỗi0,5s/Q-doubletaplộn360. Mobile443,229s/desktop112,467s/0lỗibuild;phụchồiimportDXTriêng/Resume.143preflightmỗibộ,530sourceSHAgiữ/3152scopechỉbuilder-versionđổi,42repo+33site+34index+464browserđạt. UnityEdit/scenesạch/jobsfalse/server8784đãdừng;giữArt/Wave/Packages/Simulatorngoàicommit. ChưathửSafari/Androidmáythật.

- **21.122 hoàn tất source:** C_Blender3LOD1868/864/336tri/HUD256;pool70icon/35tile/2lànkhácnhau,cầuchỉlàngiữa.150NL/nhảy−5/nhặt+15/bay−10/0,5s;Space/vuốtlênlần3hạ,hếtNLtựhạ;Q/doubletapbaylộn360°0,9s.224gameplay+46rẽ/recycle+4590hồiquy+138scene=4998Unity,387layout12viewport+3refreshđạt/5ảnhđãxem. UnityEdit/scenesạch/jobsfalse/đồhọagốcgiữ;code/asset/2docs lưucommitlocal sauedb9b67. **Chưa build/push/web**, previewHTMLdùngWASM1.53.0cũ;chưathửmobilemáythật.

- **21.121 mẫu icon năng lượng đã xong, chờ duyệt:**3mẫuA/B/CdựngBlenderMCPthật/boardđãxem,nguồn+script+approvalfalse;khôngsửaUnity/gameplay/web. Toànbộyêucầunănglượng/bayđãghinhậntạihuongdan21.121/approval.json. Chưa build/push/pháthành cho mẫu;webvẫn1.53.0.

- **21.120 đã phát hành1.53.0:** commitaf55729/Pages41559294306success/cache055a51cf4c16d704;27liveđạt/0runtime/toànbộbyteskhớp.2variant609,309s/0lỗi/1warningDevMCP;138pre+138postscene,524SHA/2974scopegiữ,42repo+33site+31staged+444browserđạt. Gồm21.108–21.119/1PNGngườidùng/điểmva chạm/resetgiao lộ. UnityEdit/scenesạch/server8784đãdừng;giữArt/Wave/Packages/Simulatorngoàicommit. Chưa thửSafari/Androidmáythật.

- **21.119 hoàn tất source, chưa phát hành:** bỏ2ảnhhomecũ, chỉdùng1PNGngườidùnggửi→poster-home.png1920×1080/nguyênbyteSHA.387layout12viewport+3refreshđạt/0JSerror/ảnhbrowserđãxem; dọnposter cũ trongoutput/cache, giữnguyênruntime/scene/sửa21.118. Compile0lỗi-0warning/UnityEdit;commitlocal+2docs,chưabuild/push/web(1.52.0).

- **21.118 hoàn tất source, chưa phát hành:** ResetRide xóa quãngvăng trước ResetCityJobs/Routes; sửa mốcrẽlệch/nhàkhungnhánh sai saucrash→retry. Tái hiện49,136m→0m;phím865+vuốt889+điểm62+smooth468+start66+CSGT214+sông446+scene20=3.030đạt,ảnhbefore/afterđãxem/0compileerror-warning. Chỉ1runtime2dòng, giữkhóađiểm/đồhọa/ảnhc4. UnityEdit/sạch;commitlocal+2docs,chưabuild/push/web(1.52.0).

- **21.117 đã khôi phục c4a5c93 theo người dùng:** hoàn tác d5180ac, ảnh2/script/caption trở lại21.115 (Tankkhaihoả/flashnòng); giữ ảnh1/luật khóađiểm. Câyindexkhớptarget trước2docs,3fileđốichiếuđạt/3refreshgiữruntimeURL; UnityEdit. Không reset dirtyArt/Packages/Captures; lưucommitlocalhoàntác, chưabuild/push/web(1.52.0). Không áp lại21.116.

- **21.115 hoàn tất source, chưa phát hành:** ảnh1Petshop hai bênđủchữ/ảnh2ngàyTankbắn+Dinochínhdiện, chụpUnitythật/16:9contain. Điểmmét/kỷlục khóa từva chạm, quãngvăng vẫncuộnworld riêng; giữphysics/chờCSGT/sông/sét.868Unity+387layout12viewport+3refresh=1.258đạt/0compileerror-warning; ảnhđãxem/UnityEdit/sạch/jobsfalse. Lưucommitlocal+2docs;chưabuild/push/web(1.52.0).

- **21.114 hoàn tất source, chưa phát hành:** hai bike sau cua1,2×Speed kể cảW/sinhngaykhiđủchỗ; rear thường/quái xế/tải ngủ gật báo0,6s thay2s, giữlàn/chờchỗ/pause/rẽ. 81pair+120timing+20scene=221đạt/compile0lỗi-0warning, UnityEdit/sạch/jobsfalse. Chỉ3partial runtime, cập nhật verifier/report/2docs và lưucommitlocal. Chưa build/push/web(vẫn1.52.0).

- **21.113 hoàn tất source, chưa phát hành:** chỉgiảm TurnBikeSpeedMultiplier2→1,5; hai xe vẫn bámSpeed/W, sinhngay/gap/làn/thứtựTank-Dino của21.112 giữ.81suitepair+20scene=101đạt/compile0lỗi-0warning, UnityEdit/sạch. Runtime chỉ1hằngsố đổi; cập nhật assertion verifier và2docs, lưucommitlocal. Chưa build/push/web(vẫn1.52.0). Chi tiết21.113.

- **21.112 hoàn tất source, chưa phát hành:** bỏtimer0,5s/arrow riêng,2bike sinh trong lời gọi hếtcua nếu đủ chỗ; luôn2×Speed kểcảW. Reargapmax8m/footprint+3/Speed×0,35s, ưu tiênlàncùngchiềugầnplayernhất/thửgầntrướcrồixahơn; oncoming chờDino/Tank sinhxong rồi ở khoảngtrống phía player, khôngđuổiđâmTank.81mới+63aim+87barrage+221turn+60distance+478boost+61immunity+20scene=1.071đạt/compile0lỗi-0warning;3ảnhthậtđãxem.2970filechỉTurnBikes/Routesđổi,scene/Art/ánh sáng/sét/pháo giữ. UnityEdit/sạch;commitlocal+2docs, chưabuild/push/web(vẫn1.52.0). Chi tiết21.112.

- **21.111 hoàn tất source, chưa phát hành:** điểm70% dời12m về gần player (junction−24m), Tank bắn0,4s ngay từ đầu. Mỗi cua thêm1UrbanBike ngược chiều +1bike vượt từ sau/báo0,5s; speed luôn±1,5×Speed kể cảW, tạt giữa2làn đúng chiều bằng animation cũ.71mới+63aim+87barrage+221turn+60distance+478boost+61immunity+20scene=1.061đạt,compile0lỗi/0warning;6ảnh thật đã xem.2968file:4runtime đổi/thêm partial+meta,scene/Art/texture/lighting/sét giữ. UnityEdit/sạch; lưucommitlocal+2docs, chưa build/push/web (1.52.0). Chi tiết21.111.

- **21.110 hoàn tất source, chưa phát hành:** Tank sinh sau cua mỗi phát70% neo LaneX[3]/junction−12m,30% chọn cũ; Tank9s/400m giữ. Tag theo generation/RNG riêng/neo cuộn cùng đường; giữ lịch/tầm/flight/đạn-nổ-hố.63aim+221turn+60distance+69barrage+61miễnDino+20scene=494đạt;2.000phát đo70,7%,3ảnh Unity đã xem. SHA2968file chỉ2runtime đổi; scene/đồ họa/lighting21.108/sét21.109 giữ. UnityEdit/sạch, lưucommitlocal cùng2docs; chưa build/push/web, web1.52.0. Chi tiết21.110.

- **21.109 hoàn tất source, chưa phát hành:** mỗi đợt sét thêm3tia trên3làn khác nhau ở khu phố hiện tại, giữ tia gốc/nhịp cũ/báo0,8s; theo road tile qua rẽ/scroll. Pool4bolt/4warning, không thêm Light/rain.636local+107storm+20scene=763đạt,4ảnh Unity ngang/dọc đã xem. SHA2968file chỉ2runtime đổi; scene/asset/lighting21.108 giữ. UnityEdit/scenesạch/jobsfalse; lưucommitlocal, chưa build/push/web (vẫn1.52.0). Chi tiết21.109.

- **21.108 đã sửa source, chưa phát hành:** quayvềđèn gốc/chỉtănggócmở,khôngđổiopticalheight/falloffhaythêmđèn. Shadergốcchỉthêm3chỗUVngang,width1,8;spot58→≈90°tươngứng.14pixel+305binding+88nhảyđêm+8legacy+20scene=435đạt;5ảnhUnity gồmoriginal/aftercùngframe1920×1280,dọc,rẽ,nhảy. So2968filechỉ2sourceđổi;UnityEdit/scenesạch/start1,92/jobsfalse. Lưucommitlocal;webvẫn1.52.0. Chi tiết21.108.

- **21.107 bản thử đã bị thay bởi21.108:** người dùng đính chính muốn mở1góc sáng rộngđốixứng,khôngthêm2tia. DayNightLighting/OpticalLight dùng1matrix/1samplecookie,carwidth2/reference0,65/fill78°,bikegiữnguyên/pool6.14pixel+304binding+88nightjump+717lighting+107storm+20scene=1.250đạt;6ảnhtrước/sau/ngang/dọc/rẽ/nhảyđãxem. So2.968filechỉ2sourceđổi,scene/asset/pixeltexturesgiữ. UnityEdit/scene sạch/start1,92/jobsfalse;chưabuild/pháthành,webvẫn1.52.0. Chi tiết21.107.

- **21.106 đã phát hành1.52.0:** mưa giông/sét21.105,release`5da12c7`/Pages`41080719656`success/cache`52d4e08424a12b5d`;29liveđạt/0runtime,đối chiếu toàn bộbytes/2data. Build2variant681,183s/0lỗi/1warningDevMCP;138preflight/521SHA giữ,44repo+35site+36staged+450browser đạt. Khônghạđồhọa;UnityEdit/start1,92/jobsfalse/serverđãdừng. Chưa kiểmFPS/Safari/Androidmáythật. Chi tiết21.106.

- **21.105 đã phát hành1.52.0:** mưa tăng dần/sky uám/đườngướt/đèn tự bật, sét nhánh báo0,8s vàcháy/khóivăngtheohướngcũ. Xe cháy khônghồiphục3s, Tank/Dino miễn; playerhếtlượt kểcảđuổi; CSGTtrúnghếtđuổi/khôngkẹtchờbắt. **107sét+717ánh sáng+51crash+82traffic+468turn+177jump+446river+81crater+20scene=2.149đạt**,7ảnhthậtđãxem. Compile0lỗi; scenechỉ5fieldconfig mới,geometry/pixeltexture/ProjectSettingsgiữ. UnityEdit/scenesạch/start1,92/jobsfalse. Chi tiết21.105.

- **21.104 đã phát hành1.51.0:** tàu đánh cá140chiếc/đèn phụ2mặt + hốđạn2,024m/nứt/mảnhvăng/lệchphảichỉnhlái. Commit`690a30e`, Pages`41048787064`success/cache`47008e2b65b2d929`;**29liveđạt/0runtime** và đối chiếutoànbộbytes/2data. Build2variant760,371s/0lỗi/1warningDevMCP;129preflight/515SHAgiữ,44repo+35site+36staged+387landing+37web+20fullscreen đạt. Không hạđồhọa;UnityEdit/start1,92/jobsfalse/serverđãdừng. Chi tiết21.104.

- **21.103 đã phát hành1.51.0:** đạn rơi đất tạo hố2,024m sâu0,40m, viền nứt/18mảnh nhựa đường. Dò4bánh swept20/30/60FPS; lệch giữ đến khi đánh lái ngược, một thao tác chỉnh hướng rồi thao tác tiếp đổi làn; nhảy tránh được/không tự gây thua. Pool64/3meshchung, theo road tile qua scroll/rẽ, không hết hạn theo giây. **1.090 kiểm đạt**,11ảnh thật đã xem; vùng ngoài hố pixel không đổi. UnityEdit/scenesạch/start1,92/jobsfalse. Đã phát hành cùng21.102 ở21.104. Chi tiết21.103.

- **21.102 đã phát hành1.51.0:** thay140tàu neo bằng Vietnamese Fishing Boat mới,9m/tỷlệnguồn/texture2K/3LOD39.998–19.998–9.998tri; giữvịtrí/hướng/bob/reflection. Đènnhỏ đỏ/vàng/xanh sángđồngbộđènlớn cả2mặt, dùngchung420renderer/6mesh, khôngthêmđèn/drawcall.3346kiểmđạt/10ảnhUnityđãxem; scopeScene0ngoàiphạmvi/anchor140tàugiữ. Chi tiếthuongdan21.102.

- **21.100–21.101 đã phát hành1.50.0:** commit`d7da47e`/Pages`41020101134`success/cache`4405f16bdef633ec`. Lane thêm10%→1,53065; nhảy cố định4,11665m (+3% mức đầu), đèn căn giữa theo xe; trang chủ cóảnhđêm dướiảnhsáng. Gồm21.96–21.99 miễnDino/38biển/vữa/lịchTank.840gameplay+113finalscene,505fileSHAgiữ;44repo+35site+36staged+387landing+37web+20fullscreen+29liveđạt/0runtime. Build2variant583,555s/0lỗi/1warningDevMCP; đối chiếu toàn bộ tài nguyênPages và khởiđộngWASM thật. GiữDPR/texture/AA4/bóng/start1,92/jobsfalse; chưa thử Safari/Android máy thật.

- **21.99 đã có trên web1.50.0:** Dino+Tank cùng khung hình sau mỗi lượt rẽ trái/phải; thêm1Tank ngược chiều mỗi mốc vượt400m. Lịch9s không bị tiêu/reset; pool/làn chắn giữ yêu cầu, pause/té không sinh mới, retry xóa sạch. **221turn+60distance+69pháo+61miễnDino+111final=522đạt**, compile0lỗi/0warning;48lượt rẽ4làn×W×20/60FPS và4lượt chạy thật>805m đạt. SHA3114file chỉ2partial runtime đổi; scene/đồhọa/vậtliệu/web giữ nguyên. UnityEdit/scenesạch/start1,92/jobsfalse;2ảnh sau rẽ đã xem. Chi tiết huongdan21.99.

- **21.98 đã có trên web1.50.0:** người dùng muốn vân tường rõ hơn trước khi phát hành. Chỉ tăng cỡ/độ nổi/loang của vữa và giảm bóng trong2material + default2shader; không đổi texture/geometry/UV. **942kiểm vật liệu/mesh+109finalscene đạt**, ảnh cận cùng camera có tương phản hạt×2,25, độ sáng trung bình chỉ−0,39%/giữ tông. SHA3114file chỉ4file vật liệu/shader dự kiến đổi,0ngoài phạm vi. 6ảnh Unity ngày/đêm đã chụp, đã xem cận tường phố/căn hộ và góc chơi. UnityEdit/scenesạch/start1,92/jobsfalse. Chi tiết huongdan21.98.

- **21.97 đã có trên web1.50.0:** thêm14biển CHO THUÊ NHÀ tại8tòa/tầng khoanhđỏ, tổng38biển/3LOD, giữ24biển cũ. Tường phố/căn hộ/bên trong phòng sơn lì với vân vữa nhẹ, giữ bảng màu/kính/kim loại/sông. **241Blender+940vật liệu/mesh+109finalscene đạt**,144mesh/4718refs/2779LOD0missing;6ảnh ngày/đêm đã xem. SHA3106file cũ chỉ5file dự kiến đổi, thêm shader/include/importer/map1K; không thêm renderer/vật cản. UnityEdit/scenesạch/start1,92/jobsfalse; nguồn riêng`PhoViet_RentalWalls2197.blend`. Chi tiết huongdan21.97.

- **21.96 đã có trên web1.50.0:** người dùng xác nhận xe tăng miễn nhiễm thân/chân/đuôi khủng long, tiếp tục chạy và bắn. `ResolveDinosaurHits` bỏqua`v.armored` trước dò/xung lực, không đụng speed/route/timer/pháo; xe khác/CSGT giữ như cũ. **61kiểm thực+69Dino+69Tank=199đạt**,0lỗi/warningcompile;6case body/tail×20/30/60FPS đều chạy>30m/bắn2phát sau1,5s. SHA3106file chỉ1filegame đổi;scene/asset/webgiữnguyên. Chi tiếthuongdan21.96.

- **21.95 đã phát hành1.49.0:** commit`faa345d`/Pages`40927522323`success/cache`e37aaa3a16abef99`. Gồm21.94 bảngthuê/chữ+20%,lane+10%→1,3915,đỉnh/v0nhảy+10%/fall22. Build2variant530,736s/0lỗi/1warningPipeline;107preflight/500filebytegiữ,43repo+34site+35staged+315landing+37web+20fullscreen+28liveđạt/0runtime. Đãđốichiếu toàn bộtài nguyênPages/khởiđộngWASMthật;khônghạđồhọa. Chi tiếthuongdan21.95.

- **21.94 đã có trên web1.49.0:** bảngthuê/chữ thêm20% (BlenderMCP riêng,24part/3LOD); lane thêm10%→1,3915; nhảy cao/vận tốc bật+10%, rơi22m/s² theo lựa chọn người dùng. **193Blender+920Unity đạt**,0lỗi/warningcompile,3ảnhbảngngày/đêmđãxem;72mesh/3248refs/2779LOD0missing. SHA3106filechỉ4filegameđổidựkiến;start1,92/jobsfalse/scene sạch. Nguồn`PhoViet_Rental2194.blend`riêng, khôngđụng .blend người dùng; xemhuongdan21.94.

- **21.93 đã phát hành1.48.0:** commit`a424235`/Pages`40905048845`success/cache`72d4794dd7d8a0a4`. Gồm21.87–21.92: trangtrícầu/cờ, menuPause, signal2mặt,ACxámtrắng,biểnthuê+20%,lane pace1,265. Build2variant/636,698s/0lỗi/1warningPipeline;102preflight/255source byte-stable;43repo+34site+35Gitstaged+315landing+37web+20fullscreen+28live đạt/0runtime. Đã đối chiếu toàn bộ tài nguyên trênPages và khởi độngWASM/thao tácPause thật. Chưa kiểm máy thật; giữDPR/texture/AA4/bóng/jobsfalse/start1,92. Chi tiết huongdan21.93.


- **21.92 đã xong:** signal2mặt đối xứng cả housing/visor/đèn đổi màu (gộp lens2mặt dùng chung1mesh/renderer), 24bảng thuê rộng/chữ+20%, chuyển làn thêm10% (1,265cumulative). **201Blender+505Unity đạt**,75mesh/3668refs/2779LOD0missing,10ảnh đã chụp/xem chi tiết. SHA3104file chỉ4assetFBX/JSON+PhoVietGame đổi, thêmpartialSignalFaces.cs/meta; scene/cầu/cây/AC/web giữ byte cũ. Nguồn mới`PhoViet_SignalRental2192.blend`, không ghi đè .blend21.91 đang có sửa cục bộ. Edit/scenesạch/start1,92/jobsfalse. Commit cục bộ kèm2tài liệu; **đã có trên web1.48.0**, xem21.93. Chi tiết huongdan21.92.

- **21.91 nâng chi tiết cảnh đã xong:** Blender MCP dựng/xuất riêng26asset (24shop thường/Petshop+signal+river apartments),3LOD/chung vật liệu, không thêm renderer; đèn giữ socket/420bóng runtime, AC xám trắng, mỗi tòa ven sông1biển CHO THUÊ NHÀ ở tầng cao thấp khác nhau theo người dùng chọn. **175kiểm Blender đạt;147mesh/5138refs và10983renderer/7670obstacle giữ nguyên;2779LOD/0missing**,11ảnh Unity ngày/đêm/LOD đã xem. SHA3104file chỉ52FBX/JSON dự kiến đổi, scene/cầu/cây/gameplay giữ byte cũ. Nguồn`PhoViet_FixtureDetails.blend`;Unity Edit/scene sạch/start1,92/jobsfalse. Đã commit cục bộ kèm tài liệu/bằng chứng, **đã có trên web1.48.0**, xem21.93. Chi tiết huongdan21.91.

- **21.90 menu Pause/kết thúc đã sửa:** Pause thêm **VỀ ĐẦU PHỐ** gọi ResetRide như khi thua; bỏ mũi tên sau **ĐI TIẾP** và **THỬ MỘT CHUYẾN NỮA**. Compile0lỗi/0warning; 6ảnh dọc/ngang/desktop đã xem, không chồng; kiểm Pause→Riding và Pause→Ready/0m/0speed/giữ kỷ lục đạt. Edit/scene sạch/start1,92/jobsfalse. Commit cục bộ cùng tài liệu/bằng chứng; **đã có trên web1.48.0**, xem21.93. Chi tiết huongdan21.90.

- **21.89 đổi làn xe đua nhanh thêm15% đã xong:** `AdvanceLaneCurve` nhân pace car ×1,15, giữ đường cong/phản hồi đầu/steering damping và W giữa chừng. **399 điều kiện Unity đạt** (217 input/motion +182 scale/pace), compile0lỗi/0warning. Đo60FPS quãng tới làn22,4237→19,5149m, tỷ lệ1,1491 do lượng tử frame. Edit/scenesạch/start1,92/jobsfalse; đã commit cục bộ cùng tài liệu và báo cáo, **đã có trên web1.48.0**, xem21.93. Chi tiết huongdan21.89.

- **21.88 bỏ thanhđen cờban công đãxong:** chỉxóa1cylở`flag()`;4tiệm0/3/6/9+4Petshop/3LODđãexport/import.64kiểmBlenderđạt,24mesh/510refs,vậtliệu/scene/obstaclegiữnguyên;4ảnhmôhìnhUnityđãxem. SHA3104filechỉđổi8FBX+8JSONnhà,cầu21.87vàcâykhôngđổi. Nguồn`PhoViet_BalconyFlags.blend`,khônginstaller; đãpháthành1.48.0/21.93;Edit/scenesạch/start1,92/jobsfalse. Chi tiếthuongdan21.88.

- **21.87 trang trí cầu hiện tại đã sửa:** bỏchậuhoa/cầu,thaythanhchéoqua8cờbằng2tayđỡngangxanh/1cờcóđaikẹp vàbulôngLOD0;3LOD,140meshref/vậtliệugiữnguyên.45Blender+27Unityasset+101sceneđạt,6ảnhday/night/2mặt/LODđãxem. SHA3104filechỉđổiFBX+JSONcầu,scene/đèn/nhà/cây/config/docswebkhôngđổi. Lưu`PhoViet_BridgeFixtures.blend`riêng,khônginstaller; đãpháthành1.48.0/21.93;web1.48.0. Chi tiếthuongdan21.87.

- **21.86 bộ nhà Blender đã dựng xong, chờ duyệt:**19mẫu(13tiệm+2góc+4căn hộ),6ảnh thật vàboard/previewHTML đã xem; 42Blender+34filechecks/3viewportviewerđạt,3104filegameSHAkhôngđổi. Cây/pets/xeđua dùng nguồn hiện tại, chưaLOD/collider/Unityimport/build/push/pháthành. Fileblend/render/script ở`ArtSource/Concepts/City-20261029/`; chỉsau người dùngduyệtmớitíchhợpnhà. Chi tiếthuongdan21.86.

- **21.85 dựng mẫu cầu đã xong, chờ chọn:** 3sceneBlenderA/B/C,6render1600×1000vàboard1800×1250 đãxem; kếtcấu/lan can/đènchi tiết. Chỉthamkhảo,tạingoàiAssets/khônggameimport/build/push/pháthành;2993fileAssets/config/docsđốichiếuSHAkhôngđổi. File/ảnhởArtSource/Concepts/Bridge-20261028,selectionnull/false; người dùng phải xácnhậntrướckhithaycầu. Chi tiếthuongdan21.85.

- **1.47.0/21.84 đã phát hành:** commit`ec06629`, Pages`40689965684`success, cache`d26fbf3bf46d9ea7`; **28liveđạt/0runtime**. Trangchủ21.83/2ảnhsươngđãcótrênweb; ảnhlandingvàgamewebthậtđãxem.315landing+37web/offline/touch+20fullscreen,101finalscene+43repo+34site+35Gitbytesđạt;196filenguồn/scene/templategiữnguyên. Build296,451s/0lỗi/1warningPipeline; UnityEdit/scenesạch/start1,92/jobsfalse,server8771đãdừng. Chi tiếthuongdan21.84.

- **21.83 trang chủ mới đã có trên web1.47.0:** nền trắng–xanh, tiêu đềgọn/xeđua/Qhúc,2ảnhgame mới sángsương; footer/nhãnảnh trongflow, headerwrap vàdialogsafearea. **315layout+37web/offline/touch+20fullscreenđạt/0runtime**, ảnhdesktop/phone2hướngđãxem. Chỉfrontend/editorhelper,5runtimefilesbytekhớp1.46.0. Đãfullbuild/push/Pages1.47.0/21.84,28liveđạt. Xemhuongdan21.83.

- **1.46.0/21.82 đã phát hành:** commit`6ae2f3f`, Pages`40662116798`success, cache`7b8dfa6dc4f391bd`; **27liveđạt/0runtime**, ảnhwebthậtđãxem. Bao gồm21.80Dino→Tanksaurẽ và21.81phảnhồiđầulàn.101finalscene+42repo+33site+26graphics+37web/offline/touch+20fullscreen+34indexđạt,194filesource/scene/templategiữnguyên. Mobile461,795s/0lỗi/3warning, desktop108,640s/0lỗi/1warning; lỗighimetaTankPalette tạmthờiđãtiếptụcResumethànhcông. UnityEdit/scenesạch/start1,92/jobsfalse; server8771đãdừng. Chi tiếthuongdan21.82.

- **21.81 đã có trên web1.46.0:** input vốn nhận ngay; thêm gain đầu quỹ đạo car, triệt tiêu cuối để giữ nhịp hoàn tất, yawattack0,08→0,05s/return0,13s giữ.60FPS: frame đầu0,057→1,409mm,1cm100→50ms, quãng tới làn22,4237m khớp cũ. **779kiểm tra đạt**,4ảnh đã xem. UnityEdit/scenesạch/start1,92/jobsfalse. Đã build/phát hành cùng21.80 trong1.46.0/21.82. Chi tiết huongdan21.81.

- **21.80 đã có trên web1.46.0:** mỗi lần rẽ hoàn tất thêmDino ở giao lộ kế tiếp, Tank sau2giây; giữ lịch định kỳ10s/9s. Wave độc lập cho từng cua, pause/reset/pool/vị trí bị chắn xử lý đủ; Tank tính cả vị trí sau nhập làn cầu. **219kiểmtra mới+170hồi quy=389đạt**,2ảnhcameraGame đãxem. UnityEdit/scenesạch/start1,92/jobsfalse. Đã phát hành1.46.0/21.82. Chi tiết huongdan21.80.

- **1.45.0/21.79 đã phát hành:** commit `9dc3aa5`, Pages `40571941077` success, cache `c0f81245589b65f2`; **27 livechecks đạt/0 runtime**. Cài đặt Cao/Trung bình/Thấp21.78 đã có trên web. 101finalscene+42repo/33site+26settingsbrowser+37web/offline/touch+20fullscreen+34Gitbytes đạt; ảnh dọc/ngang/desktop/live đã xem. Sửa output hash cũ và ổn định importGPU trước build; giữ texture/DPR/heap128/512MiB, scene/start1,92/jobsfalse. Unity Edit, server8771 đã dừng. Chi tiết huongdan21.79.

- **21.78 cài đặt đồ họa đã lên web1.45.0:** icon đườngWelcome→bánh răng, modal3mứcCao/Trung bình/Thấp, áp dụng/lưu ngay; Cao khôi phục snapshot từng nền tảng. Medium/Low giảmAA/bóng/LODxa/gương, giữDPR/texture/camera/cảnh vật.85graphics+39gesture+31distance=155đạt,6ảnhdọc/ngang/desktop vàchuột/phím/reloadPlay đãxem. UnityEdit/scenesạch/start(1,55;0;1,92)/jobsfalse/High. **Đãpháthành1.45.0/21.79**,27liveđạt/0runtime. Chi tiết huongdan21.78.

- **1.44.0/21.77 đã triển khaiPages:** commitb0a31f8/run40535422845success/cachedb471371e548dd93. TextureGPU44maps ETC2/EACchođiệnthoại, DXTchodesktop, mỗi máy chỉ1data; gói mobile129,8→92,7MB, heap550,69→397,875MiB. Bỏpreloadbuildởlanding/tảitrùng, giữDPR3/2K/AA4/bóng/cảnh. 101finalpreflight+18memory+15cachefixture+42repo/33site+37web+20fullscreen+34indexchecksđạt/0runtime; 27livechecksđạt/0runtime. UnityEdit/scenesạch/gốcxe1,92/jobsfalse, serverskiểmtrađãdừng. Chi tiết huongdan21.77.

- **21.76 đặt điểm xuất phát xe bằng kéo thả:** Edit Mode kéo xe/model → gốc xe và collider cùng đi → Ctrl+S; Play Ready cũng kéo được và thoát Play giữ vị trí về Edit để Save. Menu **Phố Việt → Điểm xuất phát → Chọn xe để kéo vị trí**. Đã giữ đúng vị trí người dùng (1,55;0;4,39), không tự trả về0; camera/đường ngang/cầu/nhảy đồng bộ. **686 điều kiện đạt + kéo gizmo thật + save/reload/Play/reset**; scene sạch/jobsfalse. Đã có trênweb1.44.0/21.77; người dùng sauđó tựđặtZ=1,92 vàbảnpháthành giữgiátrịmới này. Chi tiết huongdan21.76.

- **1.43.0 đã phát hành (21.75):** commit7b19e84/Pages40487519674 success/cache1b0d45bcf76b698e; gồmâmđộngcơ+70%,Petshop/thúcưng/phụđề/phốkhácmàu. Đã sửaOOM512MB:initial128/max1024MB, đo550,69MiB. Build321,196s/0lỗi/1warning;20preflight+34repo+29site+37web+20fullscreen+31staged+26live đạt/0runtime. Server8765 đãdừng; UnityEditMode/khôngcompile/jobsfalse; thayđổiEditor chưa lưuxem3. Chi tiết huongdan21.75.

- **21.74 đã có trên web1.43.0:** 12mẫu tiệm dùng12màu riêng, xanh dương cốđịnh choPetshop; **2.590cặp biển kề phố/river đều khácmàu**, biển dọc/2mặt/3LOD đồngbộ. XếpPetshop không kề nhau, vẫn147/490 và693/2310=30%; phụđề/thú giữ nguyên. 10.983renderer/770set/7.670box, khônghạđồhọa. **16.128điều kiện đạt**,10ảnh gồmTIỆMBÁNH–Petshop đãxem; installer byte-identical, EditMode/scenesạch/jobsfalse. Source cuối cùng dựa trên c5d873c; chi tiết huongdan21.74. Đã phát hành1.43.0/21.75.

- **21.73 đã có trên web1.43.0:** Petshop thêm **“Lưu Trú - Spa Pet”**, bỏ bàn ghế/quầy/tủ trưng bày, mỗi cửa hàng **9chó/mèo/thỏ** trong+ngoài. Dựng thật bằng **Blender MCP**,3LOD956/838/864tri tốiđa mỗi loài; ghép vào facade, giữ10.983renderer/30%/đồhọa. Bỏ1.231ghostbox trong147Petshop, scene770set/7.659box. **3.274điều kiện đạt**,9ảnh Unityđãxem; installer lặp byte-identical, EditMode/scenesạch/0missing/jobsfalse. Đã phát hành1.43.0/21.75. Chi tiết huongdan21.73.

- **21.72 đã có trên web1.43.0:** Cù Lao Petshop nềnxanh/chữtrắng **30%**:147/490shop phố,693/2310gian river; river cóbiển **cả mặtđường/mặtsông, đủ3LOD** (1386/4620biển).12variantgiữfacade+riverbase/1partbổsung; cùngmaterial, không thêmrenderer (10983), collider giữ ởmốc21.72, đã thay đồ cũ ở21.73; LOD/độnétgiữ nguyên. **682asset+1326performance+160river=2168điềukiện đạt**,9ảnh đãxem, installer repeatbyte-identical. UnityEditMode/scenesạch/jobsfalse; Blenderbackground riêng đãthoát. Đã phát hành1.43.0/21.75. Chi tiết huongdan21.72.

- **21.71 Simulator đã dùng được:** hai cấu hình iPhone 14 Pro1179×2556 / Pro Max1290×2796, overlay Dynamic Island, Safe Area đủ4hướng tại `Assets/PhoViet/Editor/DeviceSimulator/`. Unity đã import/menu có cả hai; xem cả hai dọc/ngang+khungSafeArea đạt; kiểm JSON/kích thước/alpha2mẫu/8hướng đạt (`Captures/iphone14-simulator-checks.json`). Giữ package1.0.1, không đổi gameplay/scene/web; chưa commit/push. Chi tiết huongdan21.71.

- **21.70 đã có trên web1.43.0:** tiếng động cơ player cả cruise/boost tăng70%, AudioSource0,08→**0,136** (chungnguồn, không nhân boost lần2). ChỉPhoVietGame.CreateSound; âm khác/pitch/gameplay giữ nguyên. Compile0lỗi/0warning; PlayMode đọcnormal/boost đều0,136 đạt; đãthoátPlay. Đã build/phát hành1.43.0/21.75. Chi tiết huongdan21.70.

- **1.42.0 đã phát hành (21.69):** cache bố cục sông, broadphase va chạm theo tile, gom ánh sáng sau physics;700group shop/tree dùng LOD xa sớm hơn, giữ DPR/texture/AA4/bóng/phảnchiếu. **3.810Unity+16preflight+34repo+29site+31staged+71browser+DPR3+26live đạt**,0runtime. CPUriver0,856→0,037ms, controlledrender61,13→52,80ms (không phải FPS gameplay); ảnh gần45% dướiMAE0. Commitbe0c6ff/Pages40445290840/cached7e9bf2a636e37f7. UnityEditMode/scenesạch/jobsfalse; server đã dừng. Chi tiết huongdan21.69.

- **1.41.0 đã phát hành (21.68):** commit648b163/Pages40420198461 thành công/cachec9db437e230d3824; build0lỗi/3warning đã biết;14preflight+34repo+29site+37web/offline/touch+20fullscreen+31Gitbytechecks+26live đạt,0lỗi runtime. Urban Rider31actor+56baked/cổ xe/tay lái đã có trên web. Unity Edit Mode/scene sạch/jobs khóa, server kiểmtra đã dừng.

- **21.67 đã có trên web1.41.0:** Urban Rider thay31xe máy có người ngoàiCSGT+56baked nhánh; cổ/ghiđông/bánhtrước/tay/IK/thân đồngbộ, đổi lànquintic theo tốcđộNPC, nghiêng/về cân bằng giảmchấn. 7suite/**1.493điều kiện đạt**, ảnh4màu/grip/cua/đêm đãxem; map2K cópadding, đèn không sánglan yếm. Jobs khóa; giữSupercar/CSGT. File/parameter/checks chi tiết huongdan21.67.

- **1.40.0 đã phát hành (21.66):** commit2c118c3/Pages40396554304/cache10d5883fe6b0325f;0lỗi build/3warning. 34repo bundle+29site+7parts,37web/offline/touch+20fullscreen+5offline cuối+27staged bytes+26live đều đạt,0lỗi runtime. Data chia4phần32MiB/ghép SHA trênPages; source/cảnh/gameplay giữ đúng21.62–21.65. Unity Edit Mode/scene sạch, jobs khóa.

- **21.65, đã có trong1.40.0:** Supercar nhỏ15%, đồng bộ collider/axle/đèn/jet; đổi làn nhanh tỷ lệ Speed, W giữa cú cũng tăng nhịp. **5suite/634 điều kiện đạt**, đã lưu/reload scene giữ đúng scale0,85; ảnh dọc đã xem. Unity Edit Mode/scene sạch; chi tiết huongdan21.65.

- **21.64, đã có trong1.40.0:** Blue Petshop Supercar player xanh, 3LOD28k/12,3k/4,5k; yaw collider đúng hướng (nhịp lane đã thay ở21.65); nhảy4jet trắng, W2ống bô. Q=húc như nhánh ô tô đã chốt. **8suite/1.602 điều kiện đạt**, ảnh dọc/ngang/FX/đêm đã xem; Unity Edit Mode/scene sạch; NPC/CSGT/jobs giữ nguyên. Chi tiết huongdan21.64.

- **21.63, đã có trong1.40.0:** mọi xe va chạm văng sông hủy recovery/course, rơi/chìm cả khi đang bay hoặc đã GameOver; tra mọi nhánh/đổi khung sau cua. CSGT chìm thì hết truy đuổi; người chơi đã té thì hết lượt. **8 suite/1.166 điều kiện đạt**, ảnh xác nhận chìm. Unity Edit Mode/scene sạch, jobs khóa; chi tiết huongdan21.63.

- **21.62, đã có trong1.40.0:** Emerald Tyrant rộng4,65m/cao6,081m, rig/Walk/TailSweep bằng BlenderMCP, lịch10s/ngược chiều3m/s; đuôi thật quét4làn, nhảy né, hất cả xe tăng/CSGT ở bản cũ; source21.96 đã miễn xe tăng theo yêu cầu mới, chưa phát hành. Sửa CSGT tìm đường tới người chơi bị hất sang đường ngang. **69 kiểm tra +96 cuộc gặp +681 hồi quy =846 đạt**; chi tiết huongdan21.62.

- **1.39.0 đã phát hành (21.61):** commit17aa0b2, Pages40268861830 thành công; build0lỗi/3warning,29bundle +37web/offline/touch +20fullscreen +26live đạt,0lỗi runtime. Unity Edit Mode/scene sạch, jobs khóa.

1. **21.60, đã có trên web1.39.0:** đổi làn0,72 s và phục hồi nghiêng chậm; nhảy thấp25% ở tốc độ đầu, tăng đều theo tốc độ lúc cất cánh/W; W giữa không trung giữ cú đang bay. **6 suite/541 điều kiện Unity đạt**, xem trace/ảnh; chi tiết huongdan21.60.
2. **21.58, đã có trên web1.39.0:** khôi phục đúng hero áo đỏ sao vàng/ngón nắm v4 và xe máy Meshy, sơn xanh dương–đen; Q trở lại đạp, nhảy/boost/đèn/khói cũ. NPC28/CSGT giữ nguyên. **4 suite / 229 điều kiện Unity đạt**, xem ảnh trước/sau; người/sao và phần đen trong atlas không đổi. Thay lựa chọn player Petshop21.57.
3. **Đã có trên web1.39.0:** đổi làn mượt như cua (21.55); khu sông/cầu từ ~10–12 ms còn ~0,6 ms/khung trong Editor (21.54).
4. **1.38.0:** mặt sông gương, cầu dài 50,4 m giữa hai giao lộ, căn hộ kính 6–9 tầng ven sông sáng đèn đêm, sửa nhà lơ lửng trên sông, sửa test CSGT chập chờn (21.45–21.53).
5. **1.37.0:** cầu vượt sông 2 làn giữa, rơi sông = kết thúc; xe tăng mỗi 9 giây chạy ngược chiều bắn đạn vào 1/4 làn phía trước (1,0 s → tối thiểu 0,4 s theo tốc độ), bán kính nổ 2,2 m; ánh sáng sương sớm D, mũi tên báo xe sau D (21.35–21.44).
6. **1.36.0:** xe tăng Meshy “Iron Sentinel” (21.33–21.34). **1.35.0:** đổi làn kiểu đánh lái, nhà kính, zoom dọc +10% (21.28–21.32). **1.34.0:** phố Sài Gòn (21.19–21.27).

## 3. Đang dở / cần người dùng quyết










- **21.163 hoàn tất.** Unity Edit Mode, scene sạch, máy chủ thử nghiệm đã dừng. Ca mở dọc được kiểm trên Chrome giả lập iPhone, chưa thử iPhone thật.

- **21.160 hoàn tất.** Unity Edit Mode, scene sạch, máy chủ local đã dừng. Mobile đã kiểm tra bằng trình giả lập Chrome.

- **21.156 hoàn tất.** Web 1.61.0 đã xác minh live. Unity Edit Mode, scene sạch, server 8788 đã dừng. Mobile đã kiểm giả lập, chưa thử thiết bị thật.

- **21.149 hoàn tất.** Unity Edit Mode, scene sạch, jobs và boost khóa, server 8787 đã dừng. Mobile đã kiểm bằng giả lập DPR3, chưa đo FPS trên máy thật. Giữ nguyên các thay đổi cục bộ ngoài phạm vi.

- **21.141 hoàn tất:** 1.58.0 đã phát hành/xác minh live; không còn bước build/push/Pages/live đang dở. Unity Edit/scene sạch/jobs false, server 8785 đã dừng. **Chờ người dùng thử trên điện thoại:** vuốt màn hình ngang, nút bay, mật độ icon trên không (`AirPickupsPerTile`=3, tự chọn). Test cũ lỗi sẵn: `VerifyJump`, `VerifyPerformanceLighting`, `VerifyRiverBridge`, `VerifyRiverCrashSinking`, `VerifyDistanceTanks2199`, `VerifyEnergy2122`, `VerifyEnergyRoutes2122`. Giữ dirty Art/Wave/Packages/Simulator/`PV_MissileHud.png.meta`/`PV_MissileSmoke.png.meta`/Captures lịch sử ngoài commit.

- **21.135 hoàn tất:** 1.56.0 đã phát hành/xác minh live; không còn bước build/push/Pages/live đang dở. Unity Edit/scene sạch/jobs false, server 8785 đã dừng. Chưa nghe thử loa/điện thoại thật (1,56× có thể méo khi nhiều tiếng to chồng nhau; nếu rè thì giảm `MasterGain` rồi chạy `InstallAudio2132`). Chưa thử Safari/Android máy thật. Verifier cũ cần sửa nếu dùng lại: `VerifyHudTap.cs` (tọa độ HUD cũ), `VerifyPlayerMissileScene2128.cs` (ghi cứng 1.54.0). Giữ dirty Art/Wave/Packages/Simulator/`PV_MissileHud.png.meta`/`PV_MissileSmoke.png.meta` ngoài commit.

- **21.133 hoàn tất:** 1.55.0 đã phát hành/xác minh live; không còn bước build/push/Pages/live đang dở. Unity Edit/scene sạch/jobs false, server 8785 đã dừng. Chưa thử Safari/Android máy thật. Giữ dirty Art/Wave/Packages/Simulator/`PV_MissileHud.png.meta` ngoài commit.

- **21.132 hoàn tất:** không còn bước sửa/đo âm đang chạy. UnityEdit/scene sạch; master lưu1,3×. Chưa có yêu cầu build/phát hành lượt này; web1.54.0 giữ.


- **21.131 hoàn tất:** không còn bước tích hợp/kiểm hình mẫu C đang dở; approvaltrue/C. UnityEdit/scene sạch/Blendergốc. Chưa có yêu cầu build/phát hành lượt này; web1.54.0 giữ.




- **21.129 đã hoàn tất source/kiểm tra:** không còn bước sửa hay kiểm hình đang dở. Chưa có yêu cầu build/phát hành lượt này; web vẫn1.54.0. UnityEdit/scene sạch, dirty ngoài phạm vi giữ nguyên.






- **21.123 hoàn tất:**1.54.0đãpushcommitee18d91/Pages41645004184success/cache6147ef260e9458a8/27liveđạt. Khôngcònbướcbuild/push/Pages/liveđangdở. UnityEdit/scenesạch/server8784đãdừng;chưathửSafari/Androidmáythật. GiữdirtyArt/Wave/Packages/Simulatorngoàicommit.

- **21.122 đã phát hành cùng1.54.0/21.123.** Nănglượng/bay/lộnCđãkiểmUnityvàWebGL,khôngcònbước triểnkhai đangdở;nguồnBlenderCởfile riêng.

- **21.120 hoàn tất:**1.53.0đãpush/Pages41559294306success/27liveđạt. Không còn bướcbuild/push/Pages/liveđangdở. UnityEdit/scenesạch/serverđãdừng;chưa kiểmthiếtbịthật. Chi tiết21.120. Giữdirtyngoàiphạmvi.

- **21.119 hoàn tất:** trangchủchỉ1ảnhPNGgameTaixe người dùnggửi/SHAkhớp nguồn,387layout12viewport+3refreshđạt/ảnhđãxem/UnityEdit. Source/ảnh/report/2docs lưucommitlocal; không còn bước sửa/kiểm đang dở; chưa build/push/web(vẫn1.52.0).

- **21.118 hoàn tất:** sửa thứtựreset quãngvăng trước tạođường; offset49,136→0m/rẽtrái-phảiđúng/đườngkhôngnhàchắn.3.030đạt/2ảnhđãxem/UnityEdit/scenesạch, source+tests+report+2docs lưucommitlocal. Không còn bước triển khai/kiểm đang dở; chưa build/push/web(vẫn1.52.0). Backupkiểmthử .tools/route2118-Recovery giữ cục bộ, khôngstage.

- **21.117 hoàn tất:** nội dung đã vềc4a5c93 bằngcommit hoàntác mới, chỉthêm nhật ký bàn giao/bằngchứng. UnityEdit/ready, previewcục bộđãđồngbộ; không còn bước hoàn tác đang dở. Chưa build/push/web(vẫn1.52.0).

- **21.115 hoàn tất:**868Unity+387layout+3refresh=1.258đạt/compile0lỗi-0warning; ảnhđãxem, UnityEdit/scenesạch. Source/ảnh/verifier/report/2docs lưucommitlocal; không còn bước triển khai/kiểm đang dở. Chưa build/push/web(vẫn1.52.0); preview chỉrefreshshell vớiWASMcũ.

- **21.114 hoàn tất:**221kiểmđạt/compile0lỗi-0warning/UnityEdit/scene sạch; source+verifier+report+2docs lưucommitlocal. Không còn bước triển khai/kiểm đang dở; chưa build/push/web(vẫn1.52.0).

- **21.113 hoàn tất:**101kiểmđạt/UnityEdit/scene sạch, source+2docs lưucommitlocal. Không còn bước triển khai/kiểm đang dở; chưa build/push/web(vẫn1.52.0).

- **21.112 hoàn tất:**1.071kiểm đạt/3ảnhthật/compile0lỗi-0warning;UnityEdit/scene sạch. Source+2docs lưucommitlocal; không còn bước triển khai/kiểm đang dở, chưa build/push/web(1.52.0). Verify/CaptureTurnBikes2111 chỉ là fixture lịch sử0,5s/1,5×; dùng bản2112 cho source hiện tại.

- **21.111 hoàn tất:**1.061kiểm đạt/6ảnh thật,compile0lỗi/0warning; UnityEdit/scene sạch. Source và hai tài liệu lưucommitlocal, không còn bước triển khai/kiểm đang dở; chưa build/push/web, web vẫn1.52.0.

- **21.110 hoàn tất:**494kiểm đạt/3ảnh thật, compile0lỗi/0warning; UnityEdit/scene sạch. Source và tài liệu lưucommitlocal; chưa build/push/web (vẫn1.52.0). Không còn bước triển khai/kiểm tra đang dở.

- **21.109 đã hoàn tất:**763kiểm đạt/4ảnh thật; UnityEdit/scene sạch. Source lưucommitlocal cùng2docs; chưa build/push/phát hành, web vẫn1.52.0. Không còn bước triển khai/kiểm tra đang dở.

- **21.108 hoàn tất source/kiểm tra:**435đạt,ảnhgốc-vàmởgóccùngframeđãxem;UnityEdit/scenesạch/start1,92/jobsfalse. Lưucommitlocal cùng2docs;chưabuild/push/pháthành,webvẫn1.52.0.21.107đãbịthay,khôngdùngthamsốảnhcũ. GiữdirtyArt/Wave/Packages/Simulatorngoàiphạmvi.


- **21.105–21.106 đã phát hành1.52.0**, không còn sửa/build/push/Pages/live đang dở. UnityEdit/scene sạch/start1,92/jobsfalse;server8774/browser đãđóng. GiữdirtyArt/Wave/Packages/Simulator/lịch sử ngoài commit. Chưa kiểmSafari/Androidmáythật;chi tiết21.106.


- **21.102–21.104 đã phát hành1.51.0**, không còn bước build/push/live đang dở. Release690a30e/Pages41048787064success/cache47008e2b65b2d929,29liveđạt/0runtime. UnityEdit/scene sạch/start1,92/jobsfalse; server8773vàbrowserkiểmđãđóng. CậpnhậtREADME/huongdan/BANGIAO/bằngchứnglive trongcommitdocsriêng, khôngrebuild. Chưa kiểmSafari/Androidmáythật. DirtyArt/Wave/Packages/Simulator/lịch sử vẫn ngoài commit.



- **21.96–21.101 đã có trên web1.50.0**, không còn bước sửa/build/push/live đang dở. UnityEdit/scenesạch/start1,92/jobsfalse; GPUdesktop, server kiểmtra8771/8772 đã dừng. DirtyArt/Wave/Packages/Simulator/.blend/Captures lịch sử giữ cục bộ ngoài commit; chưa kiểm điệnthoạithật.

- **21.87–21.93 đã phát hành1.48.0**, không còn bước build/push/live đang dở. Server kiểmtra8771 đã dừng; Unity giữEdit/scenesạch/start1,92/jobsfalse. Thayđổi Art/Wave/Packages/Simulator/.blend/Captures lịch sử vẫn giữ cục bộ ngoài commit. NguồnBlender21.91 và21.92 ởphiên riêng, khôngghiđè`PhoViet_FixtureDetails.blend` đangcó sửa người dùng; không còntácvụ dựng/import đang chạy.

- **21.86 chờ người dùng duyệt bộ nhà:** đãxem6rendercuối/board, file`PhoViet_NewCity_Approval.blend`, tranggallery`preview.html`, bảng`city-approval-board.png`;19mẫu/3scene, approvalfalse/installedfalse. **Chưađượcduyệt→khôngtíchhợpUnity/web.** Nguồn/render chỉthamkhảođãlưuGit,khôngđónggóivàogame. Câygiữnguồnhiện tại; sau duyệt phải làmLOD/gộpmesh/atlas/collider/kiểmhiệunăng. BlenderđangStreet_Preview/cameraStreet_Wide,renderđãxong306,06s;Scene gốc0/filepathtrống. MCPclientTransportclosednhưngBlender/addoncònmở; helper`call_blender_addon.py`gọiAPIaddon9876đãkiểm, khôngcàiđè/restart. 42Blender+34file/3viewportđạt,3104filegamegiữnguyên;tạithờiđiểmdựngweb1.47.0. Cầu21.85vẫnchờchọnriêng.

- **21.85 chờ người dùng chọn và xác nhận cầu A/B/C:** đã dựng thật BlenderMCP3mẫu/6ảnh, board`ArtSource/Concepts/Bridge-20261028/bridge-comparison.png`vàfile`PhoViet_Bridge_Concepts.blend`. A bê tông3vòm/đồng;Bvòmthépxanh;Cdâyvăng.50,4m/road6,2m/deck7,4m, chưaLOD/collider/merge. **Không tự tíchhợp hoặc đổiUnity trước xácnhận**;selection.jsonvẫnnull/false.2993filegamebytegiữnguyên;tạithờiđiểmdựngweb1.47.0. BlenderđangsceneA/rootA,cácfile.blendcũkhôngđổi. Chi tiếthuongdan21.85.

- **21.83–21.84 đã lênweb1.47.0**, khôngcònchờbuild/pháthành. Giữthayđổicụcbộkháckhôngliênquan; chưađoSafari/Androidmáythật.



- **21.80–21.82 đã lênweb1.46.0**, khôngcònchờbuild/pháthành. Bảnmớiđượckiểm27live/0runtime; chưađođộtrễ/FPS trênmáythật. Giữcácthayđổicụcbộkháckhôngliênquan.











> Mẫu: `Yêu cầu: … · Đã xong: … · Bước kế tiếp: … · File đang sửa: … · Unity/Blender: … · Cập nhật: <ngày giờ>, <agent>`

- **21.78/21.79 đã phát hành1.45.0:** không còn phần cài đặt đồ họa chờ build. Hai lỗi pipeline outputcũ/importer đã sửa; dùng builder mới và GPUformat ổn định. Server8771 đã dừng; Unity Edit/scene sạch/jobs khóa.
- **21.70/21.72–21.74 đã phát hành1.43.0:** không còn tính năng này chờ build. Khi sửaPetshop dùng generator/installer hiện hành, không chạy công cụ dựng phố/cầu cũ. Cấu hình1024MB là lịch sử1.43.0; hiện dùng128/512MiB+GPUcompression/growth10%/cap32MiB theo21.77. Khôngpháthành lại rawartifactlỗiOOM ở21.75.
- **21.69 đã phát hành1.42.0:** không còn phần tối ưu chờ build; không tự hạ DPR/texture/bóng để tăng FPS. Phép đo là Editor/camera readback và browser giả lập; chưa đo Safari/điện thoại thật. Các probe shadow/instancing chỉ thử tạm, không áp dụng.
- **21.67 đã phát hành1.41.0:** asset/meta+scene/code/2tài liệu nằm trong commit648b163. Khi bảo trì dùng đúng bộ Urban Rider hiện hành. RawMeshyFBX120,7MB giữ cục bộ, không stage/push mù quáng; dependencies không còn raw FBX/FBX dân sự cũ, không thiếu script; game dùng FBX4MB. Không chạy lạiMeshyRiders/Wave installer cũ.
- Không còn chức năng21.62–21.65 chờ phát hành: **đã có trên web1.40.0**. Dữ liệu119,6MB lưu trongGit dạng4parts; Pages ghép/kiểmSHA trước upload. Trên checkout mới, materialize ra_site trước khi chạy server local (15.2–15.4/21.66).
- Bán tải/installer Petshop21.57 đã ngưng dùng từ21.58; không tự cài lại. **Nhánh húc được tái dùng cho Blue Supercar21.64–21.65**. Test/giới hạn cua cực muộn của pickup cũ không đại diện player hiện tại. Chưa thử Safari trên điện thoại thật.
- Các file Wave cũ chưa theo dõi (`UpgradeBikesWave.cs`, `PhoViet_Motorcycles_Wave.blend`…) là lịch sử; **không xóa/commit/dùng** nếu chưa được yêu cầu.

## 4. Luật đã chốt — không tự đổi

- **Source21.145:**6NLđất+7NLbay/phố-cầu, randomlàn; cầuđất2làngiữa, bay4làn. Source21.148 đổi lên 180, đất +15 và không +20. W/F/vuốtxuống/nútbắn cả đất(nhắm tank)/bay(nhắm trực thăng), 5/5/chờ0,4s. Tankbịđạnplayer1phát dừng/cháylớn/khônghồi/xácactive-armoredgâyva chạm; tankcònnguyên miễnDino/sét nhưcũ. Đạnpháođãbắnvẫn tiếp tục bay.

- **Source21.144:** Rấtcao giữ nguyên Cao cũ/không tựgiảm cả desktop-mobile; Cao mới giữaRấtcao-Trungbình/tựgiảm. Giữ mức đãchọn riêng mức hiệu dụng. IDpref0=Rấtcao,3=Cao,1=Trungbình,2=Thấp; không đổi lại thứtựenum hoặc tựmở adaptive choRấtcao.

- **21.145 đã có trên web1.59.0:** player5/5tên lửa,nhặt+1/tốiđa5,2icon/phố-cầu. W/F/vuốt xuống/nút bắn dưới đất tự nhắm tank gần nhất, khi bay nhắm trực thăng; chờ0,4s/không mục tiêu bắn thẳng.1hit tankdừng/cháylớn/xácgâyva chạm. Giữ đạn địch/roll/khođầygiữpickup.

- **Source21.127:** va chạm xe mặt đất làm cả hai cháy tại chỗ tông; tông lại chỗ khác thêm điểm cháy. Tank không cháy. Xe thường vẫn dựng/chạy lại3s, CSGT1,2s, **vẫn chạy kèm lửa/khói**, chỉ dọn khi retire/generation/sông/retry. Giữ physics/score/chờ CSGT; lửa do sét/không chiến giữ luật riêng.

- **Source21.109 (web1.52.0 còn21.105):** quang15s→tăngmưa3s→giông9s→tan3s/lặp30s, pause dừng. Mỗi đợt giữ tia cũ và thêm đồng thời3tia ở3làn ngẫu nhiên khác nhau trong tile hiện tại; cả4báo0,8s, điểm đã báo không retarget. Nhảy vẫn trúng; xe trúng cháy/văngtheohướng cũ/nằm lại. Playertrúngluônhếtlượt sauvăng,kểcảđangđuổi. Tank/Dino miễn. CSGTtrúng→hếtđuổi; playerđãtéchờbắt thìhếtlượt. Vi phạmmới vềsaumới cólầnđuổimới, khônghồi phụcCSGTcháyđểtiếptụclầnđuổicũ.

- **Source21.103:** hố đạn giữ tới khi ra xa/chơi lại; bánh chạm làm xe lệch liên tục tới khi nhấn/vuốt NGƯỢC hướng lệch. Thao tác đó chỉ chỉnh lái, không nhảy làn tức thì; thao tác sau đổi làn bình thường. Nhảy qua tránh hiệu ứng; hố không tự gây thua, va vào xe/cảnh vật/sông vẫn theo luật hiện tại.

- **Source21.99/21.114:** sau mỗi lượt rẽ, Dino+Tank ở giao lộ kế tiếp không chờ2s; Tank sau rẽ có70% điểm rơi làn ngoài phải,24m trước tâm giao lộ/30% cách cũ. MọiTank bắn0,4s/flight1,6s, giữ lịch9s/mốc vượt400m. Thêm mỗi cua1bike ngược chiều và1bike từ sau, sinhngay/khôngcảnhbáoriêng, **21.152:** tốc độ riêng ±1,2× nhịp giao thông cơ sở, độc lập Ga/Phanh người chơi, tạt2làn đúngchiều/tránh tạt ra sông. Rear gầnđuôi ởlàncùngchiềugầnnhất; oncoming được sinh sauDino/Tank, ở khoảngtrống giữa chúng vàplayer để khôngđuổiđâmTank. Pool/vị trí kín giữyêucầu/thửngaykhitrống; resetxóa/pause-tékhôngsinh. Xe thường khác vẫn tốcđộ riêng; cảnh báo rear thường/nguyhiểm0,6s (21.114), hai bike riêng vẫn khôngcảnhbáo.

- **Source21.96:** xe tăng không bị thân/chân/đuôi khủng long hất, vẫn chạy/bắn theo quy tắc pháo hiện có; các phương tiện khác/player/CSGT giữ luật cũ. Đã có trênweb1.50.0, thay quyết định hất xe tăng ở21.62.

- Hỏi lại bằng lựa chọn cụ thể nếu chưa chắc ≥90% ý người dùng.
- Luật CSGT lịch sử khi nhánh truy đuổi được mở: không ba mạng. Té thường → hết lượt. Té khi đang bị CSGT đuổi → nằm chờ CSGT tới bắt thật → tù (không GameOver ngay, không tự dựng xe). Rơi sông/trúng đạn xe tăng → hết lượt, kể cả khi bị đuổi. **CSGT rơi sông → hết truy đuổi; người chơi đã té thì hết lượt**, còn đang chạy thì tiếp tục (21.63).
- **Source21.155:** truy đuổi CSGT tạm khóa ở `PolicePursuitReleased=false`. Va chạm thật vẫn theo luật kết thúc hiện tại. Cơ chế CSGT cũ giữ để có thể mở lại theo yêu cầu.
- Từ 21.140: vuốt ở đâu cũng được trừ thanh HUD và hai nút; không được dựng lại dải cấm dưới màn hình ngang.
- Nhảy cố định **4,11665m**, không tăng theo Speed/W (21.100/web1.50.0); lane pace1,53065 vẫn theo tốc độ.
- **Source21.155:** Q móc xích/lộn, Space nhảy/bay/hạ, **W/F bắn tên lửa cả đất/bay theo21.145**; mobile chạm hai lần móc xích/lộn, vuốt lên nhảy/bay/hạ, **vuốt xuống bắn cả đất/bay theo21.145**; nút tròn vẫn bắn. Nhận khi đủ ngưỡng, một cú vuốt một lệnh. **Boost tạm khóa (`BoostReleased=false`)**, chỉ mở lại khi người dùng yêu cầu. Source21.148 có đất xanh +15, không cam +20, tối đa 180 và bay −10/0,5s.
- Hai làn trái rẽ trái, hai làn phải rẽ phải, giữ số làn sau cua, cua không giảm tốc.
- **Source21.65: player Blue Petshop Supercar** xanh scale0,85; đã có trên web1.40.0; hero xe máy xanh–đen21.58 là lịch sử. NPC xe máy source21.67 dùngUrbanRider nhiều màu xe/áo, CSGT giữ cả người+xe; NPC4mẫu ô tô×8màu, xe tăng giữ nguyên.
- Phong cách bán hiện thực vui nhộn, tối ưu điện thoại/WebGL. **Từ 21.136 trên cảm ứng ưu tiên mượt hơn độ nét:** không quay lại DPR3/AA4 nếu người dùng không yêu cầu.
- Không bật lại công việc khi nâng cấp/sửa lỗi/build. Giữ dữ liệu Coin và mã jobs.
- Không ghi đè/reset file Blender cục bộ, không dựng lại cả scene bằng builder lịch sử.

## 5. Tra cứu nhanh: sửa gì → đọc gì

| Muốn đụng tới | Mục `huongdan.md` | File chính |
| --- | --- | --- |
| Thiết kế icon cài đặt Phố Việt | **21.167**, 21.165–21.166, 15.2 | `ArtSource/Web/pho-viet-app-icon.svg`, `AgentScripts/GenerateWebIcons.cjs`, `ArtSource/Concepts/PWA-Icons-20261109/`, template PWA `manifest.webmanifest`, `index.html`, `icons/` |
| Bánh xe lăn và giảm quay sau va chạm | **21.164** | `WheelRolling.cs`, `CarWheelDetail.cs`, `PlayerCarVisual.cs`, `CarVisualAnimator.cs`, `BikeVisualAnimator.cs`, `PhoVietCollisions.cs` |
| Player tên lửa3/3/pickup/HUD/F/homing/shot-down | **21.128** | `PhoVietPlayerMissiles.cs`, `PhoVietMissilePickups.cs`, `PhoVietPlayerMissileHud.cs`, `PhoVietHelicopter.cs`, HUD/energy/river/web runtime; `Resources/PlayerMissile`, `ArtSource/UI/player_missile_icon.*`; verifier/capture `PlayerMissile*2128`, reports `player2128-*` |
| Mảnh vỡ va chạm, nhịp tên lửa người chơi, tiếng sét | **21.179** | `PhoVietVehicleImpactFx.cs`, `PhoVietAircraftContacts.cs`, `PhoVietContactFire.cs`, `PhoVietPlayerMissiles.cs`, `ThunderstormVisual.cs`; `AgentScripts/VerifyVehicleImpact21179.cs` |
| Tốc độ trực thăng/tên lửa địch và tên phương tiện trên thẻ kết thúc | **21.180** | `PhoVietHelicopter.cs`, `PhoVietHelicopterMissiles.cs`, `PhoVietTankFire.cs`, `PhoVietGame.cs`; `AgentScripts/VerifyMissileSource21180.cs` |
| Cháy/khói đa điểm va chạm mặt đất; xe hồi phục vẫn cháy | **21.127** | `PhoVietContactFire.cs`, `ContactFireVisual.cs`, `PhoVietCollisions.cs`, `PhoVietRiderCrash.cs`, `PhoVietPolice.cs`, `PhoVietPlayerVehicle.cs`, `PhoVietThunderstorm.cs`; verifier/report/capture `ContactFire2127`/`contact2127-*` |
| Đội trực thăng theo quãng đường/lượt rẽ và nhịp tên lửa địch | **21.174**, 21.171/21.161/21.162/21.126/21.132 | `PhoVietHelicopter.cs`, `PhoVietHelicopterMissiles.cs`, `PhoVietTankFire.cs`, `PhoVietTurnHazards.cs`, `PhoVietRoutes.cs`, `ThunderstormVisual.cs`; prefab trong `Resources/Helicopter` |
| Welcome/bánh răng/cài đặt đồ họa3mức/lưu lựa chọn | **21.78**,12 | `PhoVietGraphicsSettings.cs`, `PhoVietWelcome.cs`, `PhoVietGame.cs`, `PhoVietWebRuntime.cs`, `AgentScripts/VerifyGraphicsSettings.cs`, `CaptureGraphicsSettings.cs` |
| Nút Đạp Ga/Đạp Phanh, đèn hậu và kích thước nút Bay/Tên lửa | **21.168**, 21.152 | `PhoVietDrivePedals.cs`, `DayNightLighting.cs`, `OpticalLight.shader`, `PhoVietGame.cs`, `PhoVietPlayerMissileHud.cs`, `PhoVietPortraitHud.cs` |
| Dây xích/khóa CSGT | **21.155**, 2.2, 2.5 | `PhoVietChain.cs`, `PhoVietActions.cs`, `Resources/Chain/PV_Chain.json`, `ArtSource/Blender/Chain/` |
| Bộ icon HUD B Arcade | **21.153–21.154** | `ArtSource/Concepts/DriveControls-20261108/`, `arcade-da-duyet.svg`, `approval.json`, `Resources/PV_ArcadeControls.png`, `PhoVietDrivePedals.cs`, `PhoVietFlightButton.cs`, `PhoVietPlayerMissileHud.cs` |
| Tốc độ xuất phát, xe cắt ngang và đổi làn | **21.150–21.151** | `PhoVietGame.cs`, `PhoVietTraffic.cs`, `PhoVietRoadDrivers.cs`, `PhoVietReckless.cs`, `PhoVietPolice.cs`, `PhoVietRoutePolice.cs`; kiểm tra cục bộ `.tools/VerifyGroundLaunch2151.cs` |
| Điểm xuất phát xe, kéo thả Edit/Play Ready | **21.76**,6.1 | `PhoVietPlayerStart.cs`, `Editor/PhoVietPlayerStartEditor.cs`, `PhoVietGame.cs`, `PhoVietRoutes.cs`, `AgentScripts/VerifyPlayerStart*.cs` |
| Menu Pause/kết thúc/về đầu phố | **21.90**, 2.1 | `PhoVietGame.cs` → `EndCard`, `TogglePause`, `ResetRide` |
| Điều khiển, đổi làn, camera, HUD | 2.2, 12, **21.94**,21.92/21.89/21.81/21.55/21.60/21.65 | `PhoVietGame.cs`, `PhoVietGestures.cs`, `PhoViet*Hud.cs`, `AgentScripts/VerifyLaneResponse.cs`, `VerifySupercarScalePace.cs`, `CaptureLaneResponse.cs` |
| Mưa giông/sét/báo trước/xe cháy vĩnh viễn | **21.109**,21.105,2.14 | `PhoVietThunderstorm.cs`, `ThunderstormVisual.cs`, `ThunderstormFX.shader`, `DayNightLighting.cs`, `DayNightSky.shader`, `TankCraterRoad.shader`, `PhoVietTrafficRecovery.cs`; `AgentScripts/ConfigureThunderstorm.cs`, `VerifyThunderstorm.cs`, `CaptureThunderstorm.cs`, `VerifyStormScene.cs`, `VerifyLocalLightning.cs`, `CaptureLocalLightning.cs`, `RunStormRegressions.ps1` |
| Giao thông, va chạm, văng/té/khóađiểm | **21.115**,7,21.33 | `PhoVietRiderCrash.cs`→Distance/TravelDistance, `PhoVietRiver.cs`, `PhoVietRoutes.cs`, `PhoVietRoutePolice.cs`, `PhoVietTraffic.cs`, `PhoVietTrafficRecovery.cs`, `PhoVietRoadDrivers.cs`, `MorningFog.cs`; `AgentScripts/VerifyCrashDistance.cs` |
| Xe vượt từ sau/cảnh báo0,6s, quái xế/tải ngủ gật | **21.114**,2.3–2.4,7.2 | `PhoVietExitTraffic.cs`, `PhoVietReckless.cs`, `AgentScripts/VerifyRearWarningTiming.cs` |
| Dino+Tank sau cua,2bike1,2× sinhngay/vịtrígần/tạt làn,Tank400m | **21.114**,21.113,21.112/21.111/21.99 | `PhoVietTurnHazards.cs`, `PhoVietTurnBikes.cs`, `PhoVietExitTraffic.cs`, `PhoVietTraffic.cs`, `PhoVietRoutes.cs`; `AgentScripts/VerifyTurnBikes2112.cs`, `CaptureTurnBikes2112.cs`, `VerifyTurnHazards.cs`, `VerifyDistanceTanks2199.cs` |
| Khủng long, thanh máu, tên lửa, xác cháy, quất đuôi | **21.173**, 2.12/7.5/21.96/21.62 | `PhoVietDinosaurs.cs`, `DinosaurVisual.cs`, `PhoVietPlayerMissiles.cs`, `ThunderstormVisual.cs`, `PhoVietGame.cs`, `Editor/PhoVietDinosaurUpgrade.cs`, `pho_viet_dinosaur.py` |
| Hố đạn/nứt/mảnh văng/lệch hướng và counter-steer | **21.103**,2.13 | `PhoVietTankCraters.cs`, `TankCraterGeometry.cs`, `TankCraterMask.cginc`, `TankCraterRoad.shader`, `TankCraterSurface.shader`, `CityGround.shader`, `PlayerCarVisual.cs`; `AgentScripts/VerifyTankCraters.cs`, `VerifyCraterTurns.cs`, `VerifyTankCraterGeometry.cs`, `CaptureTankCraters.cs`, `VerifyCraterRender.py` |
| Xe tăng, tên lửa phòng không và pháo 0,4 giây | **21.159**, 21.111/21.110/21.103 | `PhoVietTankFire.cs`, `PhoVietHelicopterMissiles.cs`, `PhoVietHelicopter.cs`, `PhoVietTurnHazards.cs`, `AgentScripts/VerifyTurnTankAim.cs`, `VerifyTankBarrage.cs` |
| Đạp, CSGT, bị bắt | 8 | `PhoVietActions.cs`, `PhoVietKickAssist.cs`, `PhoVietPolice.cs`, `PhoVietCrashPoliceNavigation.cs` |
| Nhảy, nóc xe, tăng tốc | 19, 21.60 | `PhoVietJump.cs`, `PhoVietBoost.cs` |
| Giao lộ/rẽ/nhà chắn sau crash→retry/reset/rebase | **21.118**,6.1,9 | `PhoVietGame.cs`→ResetRide (Distance/crashTravelDistance trước ResetRoutes), `PhoVietRoutes.cs`, `PhoVietRoadDrivers.cs`, `PhoVietExitTraffic.cs`; `AgentScripts/VerifyRouteRestarts2118.cs` |
| Chọn mẫu cầu mới (chưa tích hợp) | **21.85** | `ArtSource/Concepts/Bridge-20261028/README.md`, `bridge-comparison.png/html`, `PhoViet_Bridge_Concepts.blend`, `selection.json`; chờ người dùng xác nhận |
| Trang trí cầu hiện tại/chậuhoa/giá cờ | **21.87** | `ArtSource/Blender/pho_viet_river_bridge.py`, `patch_bridge_fixtures.py`, `PhoViet_BridgeFixtures.blend`, `Art/River/PV_RiverBridge.fbx/json`, `AgentScripts/VerifyBridgeFixtures.cs`, `CaptureBridgeFixtures.cs`; không chạy installer sông/cầu cũ |
| Đèn giao thông, điều hòa xám trắng, bảng thuê nhà ven sông | **21.102**,21.94/21.92/21.91 | `pho_viet_fixture_details.py`, `upgrade_fixture_details.py`, `pho_viet_saigon_streets.py`, `pho_viet_river_bridge.py`, `AgentScripts/FixtureDetailsImport.cs`, `CaptureFixtureDetails.cs`;21.92 thêm`upgrade_signal_rental.py`, `PhoVietSignalFaces.cs`, `SignalRentalUpdate.cs`, `VerifySignalFaces.cs`, `CaptureSignalRental.cs`; chỉ export/import signal+river, không installer |
| Thanh đen cạnh cờ ban công nhà phố | **21.88** | `pho_viet_saigon_streets.py`→`flag()`, `remove_balcony_flag_poles.py`, `PhoViet_BalconyFlags.blend`, `AgentScripts/BalconyFlagCleanup.cs`, `Captures/balcony-flags-*`; chỉ8mẫu0/3/6/9 thường+Petshop |
| Duyệt bộ nhà phố mới (chưa tích hợp) | **21.86** | `ArtSource/Concepts/City-20261029/README.md`, `preview.html`, `city-approval-board.png`, `PhoViet_NewCity_Approval.blend`, `approval.json`; chỉ sau duyệt mới thay nhà, giữ cây |
| Tàu đánh cá Meshy / tín hiệu phụ2mặt | **21.102** | `Editor/PhoVietFishingBoatUpgrade.cs`, `Art/River/FishingBoat/`, `ArtSource/Blender/prepare_fishing_boat.py`, `PhoViet_FishingBoat.blend`, `PhoVietSignalFaces.cs`, `AgentScripts/VerifyBoatSignal.cs`, `CaptureBoatSignal.cs`; không chạy lại installer sông/nhà |
| Sông, cầu, căn hộ ven sông, xe/CSGT rơi chìm | 21.35–21.37, 21.45–21.54, 21.63 | `PhoVietRiver.cs`, `RiverBlock.cs`, `RiverReflection.cs`, `River*.shader`, `Editor/PhoVietRiverUpgrade.cs`, `AgentScripts/VerifyRiverCrashSinking.cs` |
| Cảnh phố, nhà, cây | 11.4, 21.19–21.28, 21.43 | `VietnamStreet.cs`, `StreetArt.cs`, `Editor/PhoVietStreetUpgrade.cs` |
| Tường sơn lì/vân vữa rõ hơn, 14biển thuê mới | **21.98/21.97**,21.91/21.94 | `Editor/PhoVietWallFinish.cs`, `StreetWall.shader`, `WallStucco.cginc`, `ApartmentRoom.shader`, `PV_WallStuccoDetail.png`, `pho_viet_fixture_details.py`, `upgrade_rental_walls_2197.py`, `AgentScripts/TuneWall2198.cs`, `VerifyRentalWalls*.cs` |
| Ngày–đêm, đèn, sương | **21.108**,21.100,20,21.41 | `PhoVietDayNight.cs`, `DayNightLighting.cs`, `MorningFog.cs`, `MorningLooks.cs`; đèn player lệch: `PhoVietSupercarUpgrade.CenterHeadProjector`, `VerifyHeadlightAngle.cs`, `VerifyHeadlightAngleRender.cs`, `CaptureHeadlightAngle.cs` |
| Xe đua người chơi/nhảy cố định/khói4góc+2bô/chuyển làn | **21.100**,21.64–21.65,19 | `PhoVietGame.AdvanceLaneCurve`, `PhoVietJump.JumpHeightAtSpeed`, `PlayerCarVisual.cs`, `PhoVietSupercarUpgrade.cs`, `CarJumpJetVisual.cs`, `BoostSmokeVisual.cs`, `VerifySpeedJump.cs`, `VerifySupercarScalePace.cs` |
| Khôi phục player xe máy xanh–đen (lịch sử) | 21.58, 21.9–21.16 | `RestoreBlueBlackPlayer.cs`, `prepare_hero_blue_black.py`, `BikeVisualAnimator.cs` |
| Xe máy dân sự Urban Rider/cổ xe/tay nắm/đổi làn | **21.67**,7.1,11 | `UrbanRiderMotion.cs`, `BikeVisualAnimator.cs`, `UrbanRider.shader`, `PhoVietTraffic.cs`, `PhoVietRoutes.cs`, `Editor/PhoVietUrbanRiderUpgrade.cs`, `integrate_urban_rider.py`, `prepare_urban_rider_maps.py`, `AgentScripts/InstallUrbanRider.cs`, `VerifyUrbanRider.cs` |
| Model Blender/Meshy → Unity | 11, 13, 21.7, 21.17 | `Editor/PhoVietMeshy*Upgrade.cs`, `ArtSource/Blender/` |
| Công việc/Coin (đang khóa) | 10, 21.1 | `PhoVietJobs.cs`, `PhoVietJobGui.cs` |
| Màu bảng không trùng, Petshop30%, phụđề/thú cưng/nội thất | **21.72–21.74**,11.4 | `pho_viet_petshop_signs.py`, `pho_viet_river_bridge.py`, `Editor/PhoVietPetshopSignsUpgrade.cs`, `AgentScripts/InstallPetshopSigns.cs`, `VerifyPetshopSigns.cs`, `VerifyPetshopDetails.cs`, `VerifySignColours.cs`, `pho_viet_petshop_layout.py`, `pho_viet_pet_animals.py`, `Art/Pets/PV_PetGeometry.json` |
| Gạch caro vỉa hè, tiếng động cơ và tên lửa | **21.157** | `ArtSource/Blender/pho_viet_saigon_streets.py`, `Art/Streets/PV_StreetPavement.fbx`, `PhoVietGame.cs`, `PhoVietMissileAudio.cs`, `Resources/Audio/PV_Rocket*.wav` |
| Âm thanh tổng / master +30% / engine cruise-boost | **21.70,21.132** | `Scripts/PhoVietAudio.cs`, `Resources/Audio/PV_Master.mixer`, `Editor/PhoVietAudioUpgrade.cs`, `AgentScripts/InstallAudio2132.cs`, `VerifyAudioMissile2132.cs`, `MeasureMasterGain2132.cs`; `PhoVietGame.cs` (`CreateSound`, `engine`, cuối `Update`) |
| Simulator iPhone 14 Pro/Pro Max/Dynamic Island/Safe Area | **21.71** | `Assets/PhoViet/Editor/DeviceSimulator/` (`README.md`, hai `.device`, `Overlays/`, `LICENSE.md`); `Captures/iphone14-simulator-checks.json` |
| Giật lag mobile/mức đồ họa/bộ tự giảm/tile culling/warmup | **21.147,21.144,21.136** | `PhoVietGraphicsSettings.cs`, `PhoVietTileCulling.cs`, `Plugins/WebGL/PhoVietWeb.jslib`, `app.js`, `DayNightLighting.cs`, `MorningFog.cs`, `Editor/PhoVietWebMemorySettings.cs`, `Resources/PV_ShaderWarmup.shadervariants`; `AgentScripts/VerifyGraphics2144.cs`, `CaptureGraphics2144.cs`, `BenchMobile2136.cjs`, `ProfileWebCpu2136.cjs`, `VerifyMobilePerf2136.cs`, `SampleFrames2136.cs`, `AbFrozen2136.cs`, `GcHotspots2136.cs`, `TrackShaderVariants2136.cs`, `CaptureLevels2136.cs` |
| Hiệu năng/lag/cache sông/va chạm/đèn/LODxa | **21.69** | `PhoVietObstacleBatches.cs`, `PhoVietRiver.cs`, `DayNightLighting.cs`, `Editor/PhoVietPerformanceUpgrade.cs`, `AgentScripts/ProfileRuntimeCost.cs`, `ProfileStreetDetail.cs`, `VerifyPerformanceSafety.cs`, `VerifyPerformanceLighting.cs` |
| Kiểm tra theo loại thay đổi | 14, 18.2 | `AgentScripts/Verify*.cs`, `*.cjs` |
| Trang chủ HTML / 3 ảnh / responsive | **21.168**, 21.134, 15.2 | Template PWA `index.html`, `style.css`, `poster-home-jump.jpg`, `poster-home-missile.jpg`, `poster-home-encounter.jpg`; `Editor/PhoVietWebBuild.cs` → RefreshWebShell; `AgentScripts/RefreshLandingPreview.cs`, `VerifyLanding.cjs` |
| Mở game dọc sát mép, phát đầu phòng không sau rẽ | **21.162**, 21.161 | `Assets/WebGLTemplates/PhoVietPWA/app.js`, `style.css`, `PhoVietTurnHazards.cs`, `PhoVietTankFire.cs`, `PhoVietHelicopter.cs`, `PhoVietHelicopterMissiles.cs` |
| Pickup tên lửa C đã duyệt | **21.130–21.131** | `Editor/PhoVietMissilePickupUpgrade.cs`, `AgentScripts/InstallPlayerMissile2128.cs`, `VerifyMissilePickupC2131.cs`, `CaptureMissilePickupC2131.cs`, `Resources/PlayerMissile/PV_MissilePickup*`; `ArtSource/Concepts/MissilePickup-20261105/` (`approval.json`, `build_concepts.py`, hai `.blend`, hai PNG, `verification.json`) |
| Vệt lửa/khói tên lửa | **21.129** | `Scripts/MissileFlightVisual.cs`, `MissileTrail.shader`, `Editor/PhoVietMissileTrailSetup.cs`, `Resources/Helicopter/PV_HelicopterMissile.prefab`; `AgentScripts/UpgradeMissileTrail2129.cs`, `VerifyMissileTrail2129.cs`, `CaptureMissileTrail2129.cs` |
| Vuốt/nhảy/nút bay/icon năng lượng trên không | **21.140** | `PhoVietGestures.cs`, `PhoVietJump.cs`, `PhoVietFlightButton.cs`, `PhoVietEnergy.cs`, `PhoVietPortraitHud.cs`→GameplayPointerAllowed, `app.js`→swipeAction; `Resources/PlayerFlight/`, `ArtSource/UI/flight_button_icon.py`; `AgentScripts/VerifyFlight2140.cs`, `VerifySwipeShell2140.cjs`, `CaptureFlight2140.cs`, `InstallFlight2140.cs` |
| Mapping W/vuốt xuống bắn, boost tạm khóa, nhặt+15 | **21.142** | `PhoVietGame.cs`, `PhoVietGestures.cs`, `PhoVietBoost.cs`, `PhoVietEnergy.cs`, `PhoVietLandscapeHud.cs`, HTML template; `AgentScripts/VerifyMissileInput2142.cs`, `VerifyGestures.cs`, `VerifyFlight2140.cs` |
| Năng lượng6/7 / bắn tank dướiđất / xáccháy | **21.145** | `PhoVietPlayerTankMissiles.cs`, `PhoVietPlayerMissiles.cs`, `PhoVietTankFire.cs`, `PhoVietEnergy.cs`, `ThunderstormVisual.cs`, Traffic/Actions/Recovery; `AgentScripts/VerifyTankMissiles2145.cs`, `CaptureTank2145.cs` |
| Bay cao, giữ nút Bay và máy bay Crimson Skywing | **21.177** | `PhoVietSkywing.cs`, `SkywingVisual.cs`, `PhoVietFlightButton.cs`, `PhoVietEnergy.cs`, `PhoVietGestures.cs`, `PhoVietHelicopter*.cs`, template WebGL `app.js`, `Resources/Skywing`, `AgentScripts/VerifySkywing21177.cs` |
| Đỉnh nhảy và giữ bay / bắn xe tăng từ trên không | **21.169**, lịch sử 21.143/21.145 | `PhoVietJump.cs`, `PhoVietEnergy.cs` → FlightHeightAboveSupport/RequestEnergyHover, `PhoVietFlightButton.cs`, `PhoVietPlayerMissiles.cs`, `PhoVietPlayerTankMissiles.cs` |
| Năng lượng / icon C / giữ cao độ / lộn360 | **21.148,21.121–21.123** | `Scripts/PhoVietEnergy.cs`, `PhoVietJump.cs`, `PlayerCarVisual.cs`, `PhoVietActions.cs`, `PhoVietRiver.cs`, `PhoVietDistanceHud.cs`; `Resources/Energy/`, `Art/Energy/`, `EnergyPickup.shader`; `AgentScripts/VerifyEnergyWeb.cjs`, `VerifyRelease154Scene.cs`, `AgentScripts/VerifyEnergy2122.cs`, `VerifyEnergyRoutes2122.cs`, `InstallEnergy2122.cs`, `CaptureEnergy2122.cs`; `ArtSource/Concepts/Energy-20261103/approval.json` |
| WebGL/Safari/OOM/textureGPU/cache/offline/Pages | **21.106**,15.2–15.4,21.77/21.66 | `Editor/PhoVietWebBuild.cs`, `AgentScripts/BuildWebRelease.cs`, `VerifyRelease159Scene.cs`, `VerifyStormWeb.cjs`, `ConfigureWebRelease.cs`, `PublishWebArtifact.py`, `PhoVietWebMemorySettings.cs`, `VerifyWebMemory.cjs`, `VerifyWebCache.cjs`, `VerifyStagedWeb.py`, `.github/workflows/pages.yml` |

Bản đồ đầy đủ mọi file: `huongdan.md` mục 4. Các file `PhoViet*.cs` là phần của cùng `partial class PhoVietGame`.


## 6. Quy trình mỗi yêu cầu

1. Đọc file này, `git status --short`, `git log --oneline -5`. Nếu `git log` có commit mới hơn bản ghi ở mục 1 → đọc thêm mục cuối cùng (21.x mới nhất) của `huongdan.md` và `git show --stat` các commit đó.
2. Chỉ đọc các mục/file liên quan theo bảng 5. Hỏi lại nếu chưa chắc ≥90%.
3. Làm, chạy kiểm tra phù hợp (18.2), xem ảnh nếu đổi hình. Việc dài: ghi “Tiến độ việc đang làm” ở mục 3 sau mỗi mốc, để phiên khác (tài khoản Claude khác hoặc Codex) làm tiếp nếu phiên này bị dừng giữa chừng.
4. Thêm mục `21.x` mới vào `huongdan.md` (hành vi, file, tham số, kiểm tra, trạng thái) và **cập nhật mục 1–3 của file này**.
5. Khi được yêu cầu build/phát hành: theo 15.2; xong thì ghi version/commit/run/cache vào mục 1 file này và `huongdan.md`.
