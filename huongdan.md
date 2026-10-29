# PHỐ VIỆT — Tài liệu tính năng, kiến trúc và bàn giao cho Claude Code

Cập nhật toàn diện: **24/10/2026**, múi giờ **Asia/Bangkok**; kế thừa tài liệu lập 18–19/10. Đã đọc toàn bộ bản cũ, đối chiếu mã nguồn/scene, lịch sử Git và báo cáo các bản 1.15.0–1.34.0. Các kết quả kiểm tra bên dưới là bằng chứng đã lưu của từng bản, không phải tuyên bố chạy lại toàn bộ chỉ vì sửa tài liệu.

> **Trạng thái hiện tại: 1.45.0 đã phát hành** — commit `9dc3aa5`, Pages `40571941077` success, cache `c0f81245589b65f2`; **27 kiểm tra live đạt/0 lỗi runtime** (21.79). Bánh răng trên màn hình chờ mở đồ họa Cao/Trung bình/Thấp, lưu lựa chọn; Cao giữ chất lượng nền tảng hiện tại. Player Blue Petshop Supercar, NPC Urban Rider, chỉ thi quãng đường; công việc/GPS/khách/Coin HUD vẫn khóa.
>
> **Urban Rider21.67 đã phát hành1.41.0 (21.68):** Urban Rider (người + xe nguyên khối Meshy) thay31xe máy dân sự có người và56mesh xe đậu nhánh; giữ người/mô tôCSGT và playerSupercar. Cổ xe, tay lái, bánh trước, bàn tay/thân trên đồng bộ; đổi làn bậc5, nghiêng/về cân bằng có giảm chấn. 7suite/1.493điều kiện Unity đạt; xem21.67.
>
> **Bộ ô tô Meshy1.33.0 (được giữ trong1.34.0):** đã thay 11 xe bốn bánh bằng 4 mẫu Meshy, 8 màu sơn chọn lại mỗi lần xuất hiện; 3.002 điều kiện Unity và 28 kiểm tra nguồn đạt. **Đã build/push/phát hành 1.33.0**, qua 77 kiểm tra browser local + 29 kiểm tra bundle + 26 kiểm tra live, 0 lỗi runtime; Pages run `40021075002` thành công. Chi tiết 21.17–21.18.
>
> **1.35.0 (21.28–21.32, Claude Code):** zoom màn hình dọc gần hơn 10% (PortraitWorldScale .5775), nhà cao tầng phía sau có vách kính trong suốt/văn phòng sáng đèn, đổi làn kiểu đánh lái và lấy lại thăng bằng từ từ; sửa test tải Meshy (21.30). Build `build_8f58593f4444`, cache `d06d08624dba54fc`; **đã phát hành**: commit `b8c7b9f`, Pages run `40078793874`, 26 kiểm tra live đạt, 0 lỗi runtime (21.32).
>
> **1.36.0 (21.33–21.34, Claude Code):** thêm **xe tăng Meshy “Iron Sentinel”** vào giao thông: xuất hiện ngẫu nhiên ở 5 ô xe con (~1/3 số lần), một màu gốc, chậm hơn 60%, va chạm không đổi hướng/không dừng — mọi phương tiện khác (kể cả người chơi, CSGT) văng ra. 24 suite / 9.521 điều kiện Unity đạt; build `build_fa643a0c8cd3`, cache `2ad4c813b6952999`; 29 bundle + 77 browser local đạt, 0 lỗi runtime. **Đã phát hành**: commit `c49059c`, Pages run `40090621801` thành công, 26 kiểm tra live đạt, 0 lỗi runtime (21.34).
>
> **1.37.0 (21.35–21.44, Claude Code):** **xe tăng mỗi 9 giây chạy ngược chiều, bắn đạn pháo vào 1 trong 4 làn phía trước — mỗi 1,0 s ở tốc độ khởi đầu, càng nhanh càng dày tới tối thiểu 0,4 s (21.39), có vòng cảnh báo đỏ dưới đất; trúng hoặc trong bán kính nổ 2,2 m là văng ra và kết thúc; xe khác trong bán kính cũng văng (21.38).** Ngoài ra thêm **cầu vượt sông** theo ảnh mẫu (Blender MCP): khối phố phía trước có 50% thành cầu, không quá 2 khối phố liền nhau nên đi một lúc là gặp sông; cầu chỉ 2 làn giữa, kè đá, dây leo, lan can trắng, cờ, đèn, thuyền neo (bản1.37 có chậu hoa trên cầu; source21.87 đã bỏ và thay giá cờ hai thanh xanh, chưa phát hành web). Ra ngoài 2 làn cầu trên mặt sông là rơi xuống sông và **chìm dần xuống đáy** (bắn nước, bọt khí, vòng gợn sóng; không còn đứng hình lơ lửng sau thẻ kết thúc — 21.36); mặt nước sâu **5 m** dưới mặt đường (21.37; hình xe/người không còn bị nâng lên mặt đường khi ở dưới sông), trong, nhấp nhô, có bóng cầu/kè/trụ và bọt sát kè; người chơi rơi = kết thúc chuyến; xe thường tự nhập làn cầu, xe lạng lách/bị văng ra ngoài thì rơi. Thêm: ánh sáng sáng sớm có sương kiểu **D** (nắng vàng nhạt, sương xám bạc — 21.41), mũi tên báo xe phía sau kiểu **D** (ba mũi tên chạy lên — 21.42), cây dời vào khe giữa hai mái che và mặt tiền tiệm nhiều màu (21.43). 28 suite / 9.762 điều kiện Unity đạt; build `build_9c295f769ef8`, cache `5018a747f9b8d80c`; 29 bundle + 77 browser local đạt, 0 lỗi runtime. **Đã phát hành**: commit `f8d1be0`, Pages run `40124389275` thành công, 26 kiểm tra live đạt, 0 lỗi runtime; chơi thật bản live thấy cầu sông, rơi sông và đạn xe tăng nổ (21.44).
>
> **1.38.0 (21.45–21.53, Claude Code):** mặt sông kiểu gương theo ảnh mẫu (phản chiếu thật trời/mây/kè/cầu/nhà, gợn rung, thân nước xanh đen, sát kè ngả bùn) và cầu dài gấp đôi **50,4 m** chạy suốt giữa hai giao lộ; hai bờ ngắn lát vỉa hè + quảng trường ven sông; sửa nền đường thừa cạnh hai đầu cầu và ẩn nhà đường ngang đứng trên mặt sông (21.47); dựng dãy nhà lấp kín hai bờ, bỏ lan can dài ở góc ngã tư (21.48), nay là căn hộ cao cấp 6–9 tầng có ban công lan can kính và vách kính trong suốt hướng sông (21.49), ~70% phòng sáng đèn ban đêm (21.51); sửa nhà lơ lửng trên sông khi té/rơi sông (21.52). 28 suite / 9.770 điều kiện đạt (21.52); gương tốn ~2,8 ms/khung trong Editor. Build `build_13392e2af78d`, cache `1ffc9c93f6c6d874`; 29 bundle + 77 browser local đạt, 0 lỗi runtime. **Đã phát hành**: commit `bec6ce1`, Pages run `40180769646` thành công, 26 kiểm tra live đạt, 0 lỗi runtime (21.53).
>
> **Sau 1.38.0 — chưa build/phát hành (21.54–21.55, Claude Code):** đổi làn theo đường cong bậc 5 mượt như cua giao lộ (độ giật ngang ~5 lần nhỏ hơn, đầu xe quay êm, 21.55); khu sông/cầu nhẹ hơn mà không đổi thiết kế/hiệu ứng: căn hộ tách LOD từng khối, bóng đổ bằng bản LOD thô, gợn nước đọc nhiễu nướng sẵn, lớp nước đục chỉ vẽ khi có vật chìm, gương dùng LOD1+. Trong Editor, cả khối cầu từ ~10–12 ms/khung còn ~0,6 ms.
>
> **Phố Sài Gòn — đã phát hành1.34.0:** giữ toàn bộ cập nhật Claude21.19–21.26: 12shop/kính/nội thất/biển tủ, cây/chậu/vỉa hè/đồ vật, đèn cong/tín hiệu và mây, bỏ người đứng và xe đẩy cũ. Hồi quy mới1.642điều kiện Unity,29bundle,77browser local và26live đạt;0lỗi runtime. Pages run`40069532577` thành công, cache`28a1dc1e64a78083`. VerifyJump đã dùng chiều cao Meshy; lỗi CSGT chập chờn lịch sử chưa tái hiện ở hai lần chạy mới. Chi tiết21.27.
>
> **Độ nét mobile 1.32.1:** đã phát hành và xác minh 40 kiểm tra live, không lỗi runtime. DPR tối đa 3 / 3,2 triệu pixel, hai vùng texture 2K đầy đủ, người chơi giữ LOD0; chi tiết 21.15.
>
> **Người/xe Meshy (23/10/2026):** đã phát hành WebGL **1.32.0**: người Golden Star Stance có rig, hero áo đỏ sao vàng ngực/lưng, NPC nhiều màu áo, vai tự nhiên và ngón nắm v4. Xe hero trắng–đen, CSGT giữ nguyên cả người và xe. Pages run `39948530264` thành công, cache `96d0c318c468dcab`; 26kiểm tra live đạt. Xem21.9–21.14. Xe Meshy1.31.0 và các nguồn Wave ở21.5–21.8 là lịch sử.
>
> **Bàn giao nhanh (26/10/2026, 21.56):** mỗi phiên Codex/Claude Code bắt đầu bằng `BANGIAO.md` (ngắn) + Git, chỉ mở mục liên quan của tài liệu này. Mỗi lần sửa/thêm chức năng/build/phát hành phải cập nhật cả `BANGIAO.md`.
>
> **Quy tắc bắt buộc từ 22/10/2026:** mỗi lần thay đổi, sửa lỗi hoặc thêm chức năng phải cập nhật đầy đủ `huongdan.md` trong cùng công việc. Ghi hành vi mới, cách triển khai/file liên quan, tham số, quyết định thay thế, kiểm tra và trạng thái commit/build/phát hành; cập nhật mục cũ bị thay thế. Đây là tài liệu dùng chung cho Codex và Claude Code.

> **Source/scene21.62–21.65 đã có trên web1.40.0 (21.66):** player Blue Petshop Supercar scale0,85, lane theo Speed/W,4jet/2bô, Q/chạm hai lần húc; khủng long và sửa xe chìm sông. Jobs khóa; NPC/CSGT giữ nguyên. Source/asset/binary đã commit/push, Pages/live đã xác minh.

## 1. Đọc trước khi tiếp tục dự án

**Phố Việt — Chuyến xe không ngừng** là game Unity 3D lái xe máy và né giao thông vô tận, sau đó được mở rộng thành game tài xế nhận chuyến, đón/trả khách và kiếm Coin. Nhịp chơi ban đầu lấy cảm hứng từ thể loại endless runner như Subway Surfers; nhân vật, xe và cảnh quan trong repo được dựng riêng cho bối cảnh Việt Nam.

| Thông tin | Giá trị tại thời điểm bàn giao |
| --- | --- |
| Thư mục đang làm việc | `C:\Users\HP\Documents\GameUnity\Taixecongnghe` |
| GitHub | <https://github.com/denispham1107/taixecongnghe> |
| Nhánh | `main` |
| Bản chơi công khai | <https://denispham1107.github.io/taixecongnghe/> |
| Unity Editor | **6000.5.6f1**, cần module Web Build Support để build WebGL |
| Scene chính | `Assets/PhoViet/Scenes/PhoViet.unity` |
| Phiên bản game đã phát hành | **1.38.0** — sông gương, cầu dài 50,4 m, căn hộ kính ven sông sáng đèn đêm, sửa nhà lơ lửng/CSGT; trên nền 1.37.0 (cầu sông, xe tăng bắn đạn, ánh sáng/mũi tên D); chỉ thi quãng đường |
| Commit source + WebGL hiện hành | `bec6ce1` (Release mirror river, long bridge and riverside apartments in WebGL 1.38.0) |
| Bằng chứng phát hành hiện hành | `Captures/release-1.38-*.json`, `VERIFICATION_RELEASE_1_38.md`; bản trước `release-1.37-*.json` |
| Cache WebGL hiện hành | `1ffc9c93f6c6d874`; build UTC `2026-10-25T09:11:28.4593563Z` |
| Pages đã xác minh | Run `40180769646`, completed/success đúng source SHA; 26 kiểm tra live đạt (21.53) |
| Render pipeline | Built-in Render Pipeline; `m_CustomRenderPipeline` đang là `fileID: 0` |
| Input | Legacy Input Manager, `activeInputHandler: 0`; dùng `UnityEngine.Input` |
| Giao diện trong game | IMGUI, `OnGUI`, `GUIStyle`, `GUI.matrix`; không phải Canvas/uGUI |
| Vật lý giao thông | Mã chuyển động, va chạm quét và xung lực tự viết |
| Điều hướng công việc | Đồ thị phố dạng ô bàn cờ trong game, không gọi Google Maps/GPS thật |

### 1.1. Quy tắc làm việc người dùng đã xác nhận

- **Nếu không thực sự chắc chắn từ 90% trở lên về ý người dùng, yêu cầu hoặc phương án định làm, phải hỏi lại bằng các lựa chọn cụ thể. Chỉ làm phần chưa chắc chắn sau khi người dùng chọn và xác nhận. Không tự chọn thay hoặc coi im lặng là đồng ý.** Có thể tiếp tục kiểm tra và làm những phần đã rõ, không phụ thuộc câu trả lời.
- Giữ phong cách **bán hiện thực vui nhộn**: xe có cấu tạo gần xe thật, người có tỷ lệ tương đối tự nhiên, nét mặt/động tác vui nhộn, màu sắc tươi; tối ưu cho điện thoại.
- **Không còn ba mạng.** Va chạm thật làm văng/té theo hướng và vận tốc tương đối. Không bị truy đuổi: té xong kết thúc lượt. Đang bị CSGT đuổi: nằm lại chờ CSGT thật sự tới bắt rồi vào tù; không tự dựng xe, không GameOver ngay, không timer bắt từ xa. Luật này thay thế quy tắc thua ngay khi bị đuổi của bản cũ.
- Đạp quái xế để tự vệ **không làm xuất hiện CSGT**. Đạp xe máy thường thì có truy đuổi. Nếu đã bị truy đuổi từ trước, đạp quái xế không tự xóa cuộc truy đuổi.
- Hai làn trái được chọn rẽ trái, hai làn phải được chọn rẽ phải; sau cua giữ số thứ tự làn cũ, gồm cả làn giữa. Đi thẳng/đang cua dùng cùng tốc độ mong muốn, không còn hãm về 8/10 m/s.
- Trong lòng giao lộ, sát lề trái được rẽ trái, sát lề phải được rẽ phải, kể cả chưa chọn rẽ từ trước.
- **Công việc đang tắt từ 1.30.6 cho tới yêu cầu mở lại.** Coin đã lưu được giữ nguyên, hiện không hiện trên HUD và không phát sinh thưởng. Khi tính năng được cho phép mở lại: bắt đầu 0 nếu chưa có số dư, chỉ cộng sau khi hoàn tất trả khách, giữ sau thua/đóng game.
- Cơ chế được giữ trong mã nhưng **đang tắt**: chưa có việc thì tạm dừng và hiện 3 đề xuất, chọn xong chạy tiếp. Không được để menu này xuất hiện trong bản hiện tại.
- Cơ chế dịch vụ được giữ nhưng đang tắt: đúng lề mới tự dừng đón/trả; xe khác nhường từ khi bắt đầu giảm tốc cách điểm hẹn 22 m, suốt hoạt ảnh khách và 2 giây thực sự chạy lại sau cả đón lẫn trả. Nhận việc có cửa sổ nhường riêng 3 giây.
- Mạng phố hiện tại là **ô bàn cờ, tên/vị trí đường cố định trong từng lượt**; quay lại đúng đường phải thấy tên cũ. Lượt mới có thể sinh lại cách gán tên.
- Giọng chỉ đường đã chọn: nữ miền Nam, “Quẹo trái”, “Quẹo phải”, “Đi thẳng”; báo trước khoảng 4 giây, nhắc trước 2 giây, tránh “Đi thẳng” ngay trước rẽ. Hiện không phát vì công việc đang tắt.
- Phím hiện tại: **Q đạp**, **Space nhảy**, **W tăng tốc +125% trong 4 giây, hồi 2 giây**. Mobile: chạm hai lần đạp, vuốt lên nhảy, xuống tăng tốc; vuốt nhận ngay khi đủ ngưỡng, không chờ nhấc tay.
- **Người chơi hiện tại (source21.64):** Blue Petshop Supercar xanh giữ chữ/sơn Meshy, thay riêng người áo đỏ+xe máy. Hero xe máy xanh–đen21.58 là bản trên web1.39.0 và được giữ asset lịch sử. Từsource21.67, NPC xe máy có người dùng Urban Rider nhiều màu xe/áo không sao; cả người/mô tô CSGT giữ nguyên.
- **Mọi lần sửa/thêm chức năng đều phải ghi vào tài liệu này**, kể cả thay đổi chưa build hoặc chưa phát hành; ghi rõ trạng thái chưa hoàn tất thay vì bỏ qua.
- **Phố Sài Gòn (24/10):** cảnh quan nhà phố hai bên đường dựng theo ảnh mẫu `ArtSource/Reference/Street/saigon_street_reference.webp`, giữ gameplay (mặt tiền |x| = 9,3 m) và ngân sách WebGL; chậu cây được đưa lại theo ảnh mẫu. Đã được người dùng yêu cầu build/phát hành ngày24/10 và đã phát hành1.34.0; bằng chứng ở21.27. Xem 21.19.
- **Ô tô Meshy (24/10):** người dùng đã chọn dùng cả thể thao/bán tải/tải bạt/container, giữ tỷ lệ; hai mẫu tải được chọn ngẫu nhiên ở tải thường/ngủ gật, mọi xe chọn một trong tám màu sơn khi spawn. Kính/lốp/kim loại giữ màu riêng. Đã phát hành WebGL1.33.0. Xem 21.17–21.18.
- **Xe tăng Meshy (24/10, 21.33; kiểu xuất hiện ngẫu nhiên ~1/3 ô xe con đã thay bằng lịch 9 giây ở 21.38):** người dùng gửi mẫu “Iron Sentinel”; yêu cầu: xuất hiện ngẫu nhiên như ô tô con, chỉ một màu gốc (không đổi màu), va chạm với mọi phương tiện thì xe tăng không đổi hướng/không dừng, còn phương tiện kia văng ra; luôn chậm hơn ô tô khác 60%.
- **Xe tăng bắn (25/10, 21.38):** cứ 9 giây một xe tăng (bỏ xe tăng ngẫu nhiên ở ô xe con); chỉ xe tăng ngược chiều bắn, mỗi 1 giây một phát rơi ngẫu nhiên vào 1 trong 4 làn ở đoạn đường người chơi sắp tới; có cảnh báo dưới đất; trúng/nổ trong bán kính → văng ra, kết thúc; xe khác trong bán kính cũng văng; hiệu ứng lửa đầu nòng, đạn bay, nổ, cảnh báo dựng bằng Blender MCP.
- **Cầu vượt sông (24/10, 21.35):** người dùng gửi ảnh mẫu cầu 2 làn qua sông có thuyền, yêu cầu dựng bằng Blender MCP, liền mạch với phố cũ, “đi một lúc là phải gặp cầu và sông” (tỷ lệ 50% là để diễn đạt ý này), cầu chỉ đúng 2 làn; người chơi hoặc phương tiện không đi đúng 2 làn cầu thì rơi xuống sông, người chơi rơi là kết thúc; xe lạng lách ra ngoài phạm vi cầu cũng rơi. **Đã chốt:** cầu nối 2 làn giữa (làn 1 ngược chiều + làn 2 cùng chiều, vạch vàng giữa); xe thường tự nhập làn cầu trước khi lên cầu; cầu dài một khối (mặt sông ~25 m) để giữ giao lộ 80 m.

### 1.2. Những yêu cầu cũ đã được thay thế

| Nội dung từng tồn tại | Quy tắc hiện tại cần giữ |
| --- | --- |
| Quái xế/xe tải chỉ xuất hiện mỗi 20 giây hoặc 15–20 giây | Đã bỏ lịch riêng; tham gia các đợt sinh giao thông bình thường, ở cả hai chiều |
| Cứ rẽ là gán một tên đường chưa từng dùng | Đã thay bằng mạng phố cố định theo tọa độ để GPS có thể quay lại cùng địa điểm |
| Va chạm khi bị đuổi làm chậm/chạy tiếp hoặc GameOver ngay | Hiện văng/té rồi nằm chờ CSGT đến bắt thật; Arrested → Jailed |
| Ba mạng/tự dựng xe người chơi | Đã bỏ từ 1.28.0; không tự dựng lại sau té |
| Space đạp, Q nhảy, E tăng tốc +75%/hồi 3 s | Q đạp, Space nhảy, W +125%/4 s/hồi 2 s |
| Hai làn giữa chỉ được đi thẳng; cua giảm 8/10 m/s | Làn giữa trái/phải rẽ đúng phía; cua giữ tốc độ như đường thẳng |
| Bật công việc trong scene trước mọi bản phát hành | Sai với trạng thái hiện tại: RideJobsReleased=false và jobsEnabled=false |
| Xe tay ga/xe số v2, rồi bản Wave/WaveAlpha | Scene hiện dùng xe Meshy đã nhập, xe chính trắng–đen; giữ tên semantic `PV_HeroScooter`… và riêng xe CSGT cũ. Xem 21.7 |
| Icon minh họa raster hoặc PV cách điệu | SVG xe máy chính diện, sao vàng, đường; PNG chỉ là bản xuất cho hệ điều hành |
| Chậu cây trang trí ở ban công/vỉa hè đã xóa (1.13) | Yêu cầu phố Sài Gòn 24/10 theo ảnh mẫu đưa lại chậu cây lớn vỉa hè, cây ban công và hoa giấy rủ (21.19) |
| 12 shop QUÁN PHỞ/HỦ TIẾU/MÌ GÕ… mặt tiền màu đậm, biển nhỏ | 12 shop Sài Gòn: PHỞ BÒ - GÀ, TIỆM TRÀ, BÁNH MÌ, TẠP HÓA… biển lớn chữ trắng/đỏ, nhà kem/vàng/be (21.19) |
| Tán lá ghép từ những khối cầu | Đã dựng lại bằng lá có hình học riêng và nhiều LOD |
| Mái hiên một mặt hoặc thiếu dải vải khi giảm chi tiết | Đã sửa thành vải có độ dày, giữ nguyên các dải ở cả 3 LOD |

`README.md`, báo cáo cũ và Git history có thể mô tả luật tại thời điểm cũ (ba mạng, phím cũ, việc đang bật, va chạm thua ngay khi bị đuổi). Không dùng chúng để ghi đè trạng thái hiện tại. Các mục 1/2/7/8/19–21 của tài liệu này và source hiện hành là mốc bàn giao; yêu cầu mới trực tiếp của người dùng vẫn được ưu tiên.

## 2. Toàn bộ tính năng người chơi đang có

### 2.1. Bối cảnh, nhân vật và vòng chơi

- Góc nhìn thứ ba từ phía sau và hơi cao, camera nhìn dọc con đường; xe tự tiến.
- **Source/scene21.65:** người chơi là xe đua Blue Petshop Supercar, kích thước85% so với21.64; bỏ visual người áo đỏ/xe máy dưới root player. Q/chạm hai lần áp sát+húc ngang; nhảy/W giữ luật cũ và thêm4luồng khói nâng/2luồng khói bô. Từ21.67 NPC xe máy có người đã đổi Urban Rider; CSGT không đổi. Web1.40.0 vẫn dùng NPC cũ và xe đua nhỏ15% như source21.65.
- Đường có 4 làn, vạch giữa đường vàng đứt nét, vạch trắng phân làn, vạch biên, vạch qua đường và giao lộ.
- Hai làn bên trái chủ yếu là xe ngược chiều; hai làn bên phải chủ yếu cùng chiều. Xe ngược chiều có thể lấn qua phía người chơi.
- Xe máy, sedan, hatchback, taxi, xe giao hàng, xe tải thường và xe tải ngủ gật cùng xuất hiện; từ 21.33 ô sedan/hatchback/taxi có thể ra **xe tăng** (~1/3 lần, chạy 40% tốc độ, không bị đẩy khi va chạm).
- Các lượt chơi dùng seed mới, không lặp nguyên xi mẫu xe/làn/tốc độ/lịch di chuyển của lượt trước.
- Quãng đường và tốc độ được hiển thị; có kỷ lục lưu cục bộ, số xe đã né, thông báo tình huống và truy đuổi.
- Không có mạng dự phòng: va chạm văng/té xong kết thúc lượt nếu không bị đuổi; nếu đang bị đuổi thì nằm chờ CSGT đến bắt và chuyển sang buồng giam. Góc/ngả/văng phụ thuộc tiếp xúc, không luôn đổ phải tại chỗ.
- 10 giây chạy đầu giảm một nửa mật độ/số xe (làm tròn lên), rồi trở về bình thường; pause không ăn thời gian này.
- Ngày–đêm lặp 60 giây, đèn xe/đường tự bật theo trời tối; sương sớm tan hết trong 6 giây đầu mỗi lượt.
- Hiện chỉ thi số mét và kỷ lục cục bộ; không có bảng xếp hạng online, không hiện menu việc/GPS/Coin.
- Có bắt đầu, tạm dừng/tiếp tục, chơi lại, về màn hình đầu và bật/tắt âm thanh. Menu Pause có **ĐI TIẾP** và **VỀ ĐẦU PHỐ**; menu kết thúc có **THỬ MỘT CHUYẾN NỮA** và **VỀ ĐẦU PHỐ**. Hai nút chính không còn dấu mũi tên phía sau (source21.90, chưa phát hành).

### 2.2. Điều khiển

| Thao tác | Desktop | Điện thoại/tablet |
| --- | --- | --- |
| Bắt đầu | Nút “LÊN XE THÔI”, Enter hoặc Space | Chạm nút bắt đầu |
| Đổi làn/chọn rẽ | ←/→ hoặc A/D; rẽ theo nửa đường trong vùng hợp lệ | Vuốt trái/phải |
| Áp sát rồi đạp xe máy | **Q** | Chạm hai lần gần nhau trong vùng chơi |
| Nhảy | **SpaceBar** | Vuốt từ dưới lên |
| Tăng tốc | **W** | Vuốt từ trên xuống |
| Tạm dừng/tiếp tục | P, Esc hoặc nút tạm dừng | Nút tạm dừng/tiếp tục |
| Chơi lại | R, Space hoặc nút chơi lại | Chạm nút chơi lại |
| Chọn công việc | **Đang ẩn/tắt** | **Đang ẩn/tắt** |

**Đổi làn hiện hành (21.60, thay nhịp nhanh21.55):** A/D/←/→ hoặc vuốt ngang nhận lệnh ngay nhưng chuyển một làn theo đường cong bậc5 trong0,72 s (90% sau~0,55 s, gần tâm~0,68 s). Bẻ đầu xe0,16 s; trả lái0,45 s; giữ nghiêng ngắn0,18 s rồi hồi phục nghiêng bằng SmoothDamp0,45 s. Vẫn chạy tới trong suốt nhịp, đầu xe gần thẳng sau~1,45 s và độ nghiêng xuống<2° sau~2 s ở tốc độ17 m/s. Chuyển tiếp/reverse giữ vị trí/vận tốc/gia tốc liên tục. Hộp va chạm vẫn theo đường, không tự giảm tốc chạy tới. Mỗi lần vuốt một lệnh; nhảy/tăng tốc/Q và quy tắc rẽ giao lộ giữ nguyên.

- **Source21.81, chưa phát hành:** xe đua có nhịp khởi đầu đổi làn phản hồi ngay từ khung hình nhận lệnh, giữ quãng/thời gian hoàn tất theo tốc độ và nhịp trả lái cũ. Input phím/vuốt nhận ngay; xem số đo và công thức ở21.81.

### 2.3. Giao thông gây bất ngờ

- Xe cùng chiều có thể phanh gấp hoặc giảm tốc mạnh, có đèn phanh, rồi tăng tốc trở lại.
- Xe chạy song song có thể bất ngờ đổi làn. Từ21.67 xe máy Urban Rider đổi làn theo quỹ đạo bậc5 và thời lượng theo tốc độ riêng (0,65–1,6s), cổ/ghi đông/bánh trước/tay/nghiêng thân đồng bộ; các ô tô và CSGT giữ cơ chế cũ.
- Xe thường ngược chiều cũng đổi làn, có thể vượt vạch vàng sang làn người chơi rồi quay lại; không đổi thành xe cùng chiều chỉ vì đổi làn.
- Xe băng ngang ở giao lộ đi từ cả hai phía; số lượng, tốc độ và tỷ lệ xe lớn tăng theo độ khó. Có xe đi thẳng và có xe rẽ vào đường chính.
- Xe chạy dọc đường cũng có thể rẽ trái/phải qua đường nhánh bằng quỹ đạo cong.
- Xe có thể va chạm và bị hất văng. Sau **3 giây từ lần va chạm mới nhất**, xe giao thông tự dựng (hoạt ảnh tối thiểu 0,65 s, kéo dài khi xe dài phải xoay/trượt xa — 21.50), nhập làn trống gần nhất đúng chiều cũ ở nửa đường xe đang đứng và chạy tiếp; nếu đang rẽ thì hoàn thành đích rẽ đã chọn. CSGT giữ 1,2 s. Ngoại lệ xe nguy hiểm đâm nhà cháy nằm lại.
- Xe thường vượt từ dưới màn hình lên có mũi tên đỏ viền vàng ở đúng làn ít nhất 2 giây trước khi xuất hiện; không áp dụng cảnh báo này cho xe từ phía trên hoặc xe băng ngang.
- Sau khi người chơi rẽ xong, có đợt xe ngẫu nhiên từ phía sau vượt lên và từ hướng ngược chiều đi tới.
- Đèn giao thông hiện là cảnh vật có đổi màu; game chưa dùng đèn để buộc tất cả xe dừng/nhường theo luật giao thông.

### 2.4. Quái xế và xe tải ngủ gật

- Quái xế source21.67 dùng Urban Rider xe đỏ/áo xanh rêu không sao, có biển nguy hiểm; bản1.40.0 trước đó dùng xe Meshy cam/người Golden Star Stance cũ. Xe tải mang biển **XE TẢI NGỦ GẬT** giữ mẫu tải hiện hành.
- Có cả cùng chiều lẫn ngược chiều, lấn làn và lạng lách qua phía người chơi.
- Xe cùng chiều xuất hiện từ ngoài camera phía sau. Mỗi đợt chỉ một xe nguy hiểm trên một làn cùng chiều được chọn ngẫu nhiên, báo mũi tên đỏ–vàng đúng làn trước 2 giây. Nếu còn xe nguy hiểm ngược chiều/chạy từ sau đang di chuyển thì đợi nó va chạm; khi đủ điều kiện lại phải báo đủ 2 giây. Quy tắc còn hiệu lực qua rẽ/rebase.
- Có thể húc và đẩy xe khác, gây rối giao thông và phản ứng dây chuyền.
- Có thể đâm nhà bên trái/phải, nằm lại với hiệu ứng lửa/khói. Phần đường giao nhau trống không bị coi là vách nhà.
- Người chơi có thể đạp ngã xe máy quái xế ở cả hai chiều để tự vệ; không thể dùng cú đạp để đạp trực tiếp ô tô hoặc xe tải.

### 2.5. Húc ngang/đạp xe, dây chuyền, CSGT và buồng giam

- Q/double tap chọn **một** xe máy hợp lệ gần nhất/ưu tiên quái xế, cùng chiều, ngược chiều, cắt ngang hoặc đang rẽ gần giao lộ. **Source21.64:** player ô tô dùng nhánh áp sát rồi húc ngang21.57, chỉ hất khi thân xe tiếp xúc thật, rồi về làn gốc; bị chắn thì hủy. Web1.39.0/player xe máy21.58 vẫn là vung chân/chạm giày. Luật tự vệ/gọi CSGT giữ nguyên.
- Xe bị đạp ngã theo hướng ra xa người chơi. Xe ngã có thể kéo theo xe máy khác; xe lớn va vào đống xe cũng nhận phản lực theo mô hình vật lý.
- Có hồi chiêu đạp; không đạp khi tạm dừng, đang đón/trả khách hoặc đã thua.
- Đạp trúng xe máy thường lần đầu gọi mô tô CSGT từ phía sau rất nhanh. CSGT có đồng phục vàng cát, mũ, chi tiết cầu vai, đèn ưu tiên và còi.
- Người chơi khi đường thông thoáng có ưu thế tốc độ nhỏ sau khi CSGT áp sát; truy đuổi vẫn tăng nhịp theo thời gian.
- CSGT dự đoán xe, chọn lối né và phanh khi bị chặn; không được đi xuyên xe đang chạy, xe đứng yên hoặc xe ngã.
- Nếu CSGT vẫn va chạm trên đường, mô tô ngã **1,2 giây**, không bắt người chơi trong lúc nằm; dựng xe rồi tiếp tục từ vị trí thực tế. **Ngoại lệ21.63:** CSGT văng xuống sông thì rơi/chìm và kết thúc truy đuổi; người chơi còn chạy thì tiếp tục, đã té thì kết thúc lượt sau khi té xong, không chờ bắt và không vào tù.
- Đang bị truy đuổi mà chạm thật vẫn văng/té, không miễn va chạm. Người chơi nằm chờ; CSGT tìm đường tới vị trí bắt quanh người đã té, kể cả khi phải vòng cụm xe hoặc đi tạm xa trước rồi quay lại. Không tự dựng xe người chơi hoặc kết thúc bằng timer.
- Khi CSGT thực sự tới trong khoảng bắt và đã đứng dậy, chuyển `Arrested` → `Jailed`; nếu người chơi đang té phải đợi té xong. Nhân vật mặc đồ sọc trong buồng giam, có thông báo “CSGT ĐÃ ĐƯA BẠN VÀO TÙ”.

### 2.6. Rẽ đường và cảnh quan Việt Nam

- Sơn 16 cụm mũi tên ở mỗi giao lộ: ngoài trái rẽ trái; giữa trái thẳng+rẽ trái; giữa phải thẳng+rẽ phải; ngoài phải rẽ phải. Tất cả cùng màu sơn trắng, nhánh nối liền thân mũi tên, áp dụng cả bốn hướng.
- Mũi tên cùng màu sơn với vạch qua đường, được đặt cách vạch để không chồng lên nhau.
- Có thể chọn rẽ trong vùng mũi tên trước giao lộ, hoặc rẽ muộn khi đang ở lòng giao lộ và sát đúng phía lề.
- Xe ôm cua 90 độ, camera chuyển mượt rồi trở về góc nhìn sau xe; vẫn giữ quãng đường, độ khó, giao thông và truy đuổi.
- **Phố Sài Gòn theo ảnh mẫu (24/10/2026, scene/web1.34.0 đã phát hành —21.19–21.27):** nhà ống 3–5 tầng màu kem/vàng/be/hồng đào/xanh nhạt, cửa kính khung trắng, ban công lan can sắt đen, cục nóng điều hòa, mái ngói đỏ hoặc sân thượng có bồn nước, cây ban công, hoa giấy rủ, cờ đỏ sao vàng (source21.88 đã bỏ thanh đen nhô cạnh cờ ban công), biển dọc NHA KHOA/NHÀ NGHỈ/GIẶT SẤY và nhà cao tầng phía sau tạo chân trời. Tầng trệt là cửa hàng có **vách kính trong suốt khung tối/gỗ, cửa kính hai cánh có tay nắm**, nhìn thấy lòng tiệm sâu 3,4 m sáng đèn với kệ hàng/quầy/bàn ghế/bảng thực đơn (21.22; tiệm sửa xe để mở cửa cuốn), mái hiên sọc có riềm, biển lớn; vỉa hè caro kem–đất nung, bó vỉa vàng; đồ vật vỉa hè dựng chi tiết theo ảnh mẫu (21.23): chậu vuông cây lá to bản, thùng rác xanh có nắp/bánh, bàn gỗ nhỏ có khay chai tương ớt/ống đũa, ghế nhựa đỏ chân loe, bàn ghế gỗ tựa nan, bảng đen chữ A khung gỗ, tủ kính bày tô/rau + nồi nước dùng, tủ nước đỏ cửa kính thấy chai (không logo), kệ sắt bày chai, két nhựa, dù có riềm. Cây đường theo ảnh mẫu cây riêng: thân xám mảnh, cành chẻ, tán nhiều cụm lá xanh tươi (21.24); từ 21.43 đứng ở khe giữa hai mái hiên (z 8,2/33,4/50,2) và mặt tiền mỗi tiệm một màu (chỉ một tiệm vàng). Cột đèn đường cong cao ~8,7 m màu xám, cột đèn tín hiệu cần vươn ra lòng đường với hộp đèn treo; trời ban ngày xanh đậm có mây tích trôi chậm (21.20). Người đứng vỉa hè **đã xóa** (21.21), xe đẩy bánh mì cũ **đã xóa** (21.26). Dây điện và xe đậu vẫn là bộ cũ.
- 12 mẫu mặt tiền gốc: PHỞ BÒ - GÀ, TIỆM TRÀ, BÁNH MÌ, TẠP HÓA, THUỐC TÂY, SỬA XE, CÀ PHÊ SÀI GÒN, CƠM TẤM, BÚN BÒ HUẾ, HỦ TIẾU NAM VANG, SINH TỐ, TIỆM BÁNH (`VietnamStreet.Shops`). **Source21.72:** thêm12biến thể cùng mặt tiền, biển chính **Cù Lao Petshop nền xanh/chữ trắng**, chiếm147/490cửa hàngphố; tên Hierarchy scene cũ giữ semantic mẫu nhà, phải đếm thương hiệu theo mesh path `_Petshop.fbx`, không suy từ tên GameObject. **Source21.73:** biển thêm dòng nhỏ “Lưu Trú - Spa Pet”; Petshop dùng nội/ngoại thất riêng, bỏ bàn ghế/tủ/quầy cũ, có9chó/mèo/thỏ cả trong và ngoài. **Source21.74:** 12mẫu tiệm thường dùng12màu riêng, không dùngxanh; mọi cặp biển kềkhácmàu, Petshop xanh cốđịnh, chi tiết21.74. Chưa build web.
- Có 48 tên đường Việt Nam trong `RouteNames`, gồm các tên người dùng chỉ định và các tên bổ sung; cùng một trục phố giữ cùng tên trong lượt hiện tại.
- Biển hiệu đã dựng song song, sát mặt tiền; chữ không xuyên qua nhà bên cạnh. Biển dọc chỉ nằm trong bề rộng tiệm. Mặt tiền vẫn ở |x| = 9,3 m nên luật đâm nhà, làn và vật cản giữ nguyên.
- **Cầu vượt sông (21.35; dài 50,4 m và nước gương từ 21.46):** khối phố có thể thay đoạn giữa hai giao lộ bằng sông chảy ngang (**z 4,8–55,2** trong khối từ 21.46, trước đó 16,8–42; kéo dài ±95 m), mặt nước gương phản chiếu thật (21.45–21.46), kè đá xám xếp khối có dây leo rủ, lối dạo lát gạch + lan can sắt + chậu cây, cầu tàu gỗ và 4 thuyền gỗ mái xanh/xanh dương chở hàng nhấp nhô. Cầu 2 làn có vạch vàng giữa, vạch trắng mép, gờ bó + lan can trắng con tiện; **source21.87 đã bỏ toàn bộ hộp hoa trên cầu**, giữ cây/chậu ở bờ sông, 4 vị trí đèn LED mỗi bên kèm cờ đỏ sao vàng cố định bằng hai tay đỡ ngang xanh đậm có đai kẹp, không còn thanh chéo qua cờ; trụ đá dưới nước. Bản web1.47.0 vẫn là trang trí cũ, chưa phát hành21.87. Hai đầu cầu giữ nhà/vỉa hè/giao lộ nên liền mạch; làn ngoài kết thúc ở mép kè có vạch chéo thu hẹp, gờ vàng–đen và biển “CẦU HẸP 2 LÀN”.

### 2.7. Công việc, hành khách, Coin và GPS — đang tắt

**Toàn bộ mục này mô tả cơ chế được giữ trong mã để mở lại sau; không xuất hiện trong bản 1.30.6.** Khóa `RideJobsReleased=false`, scene `jobsEnabled=false`, runtime dùng `JobsAvailable`. Không bật lại chỉ để chạy kiểm tra cũ.

- Sau khoảng 2 giây đang chạy mà chưa có việc, game hiện 3 chuyến và tạm dừng để chọn. Sau trả khách cũng có khoảng chờ ngắn trước bộ việc mới.
- Mỗi chuyến có điểm đón, điểm trả, quãng đường tới đón, quãng đường chở, mức thưởng và giới tính khách ngẫu nhiên.
- Điểm hẹn là địa chỉ trên cạnh đường của mạng phố, gắn với vị trí cửa hàng; không dời mục tiêu sang vị trí mới khi người chơi đi sai.
- 3 vị trí đón khác nhau; mức thưởng khác nhau, tối thiểu 20 Coin, tăng theo tổng quãng đường đón + chở.
- GPS có ô bản đồ, đường đi, điểm đích, hướng người chơi, khoảng cách, hướng rẽ tiếp theo, tên đường, phía lề cần vào và mức thưởng.
- Khi rẽ: GPS xem trước đường thoát ngay frame bắt đầu cua, khoảng cách/vị trí/hướng cập nhật liên tục; tọa độ phố chính thức chỉ đổi một lần lúc rebase. Đi sai/lỡ điểm dừng vẫn tính lại tới đúng địa chỉ cũ.
- Điểm đón/trả đang thực hiện có mũi tên khối xanh ngọc, bề dày/cạnh vát, viền sáng/tối, quầng nhịp và vòng dưới đất; nhãn ĐÓN KHÁCH/TRẢ KHÁCH. Chỉ hiện mục tiêu giai đoạn hiện tại khi trong tầm camera, kể cả đường ngang/đang rẽ. Hiện marker bị khóa cùng công việc.
- Giọng nữ có sẵn trong build/offline; dùng thời gian tới giao lộ theo tốc độ thật: báo khoảng 4 s, nhắc khoảng 2 s; không phát “Đi thẳng” nếu sắp tới lệnh rẽ trong cửa sổ đó. Không phát giọng khi tính năng tắt.
- Khi người chơi vào đúng lề gần điểm hẹn, xe tự giảm tốc/dừng. Khách nam hoặc nữ bước ra, lên sau xe, đi cùng rồi xuống ở điểm trả; chỉ một khách xuất hiện cho chuyến đó.
- Khi bật lại dịch vụ: nhường từ lúc giảm tốc trong vùng 22 m đúng đường/lề, suốt Boarding/Alighting và **2 giây thực sự di chuyển sau cả đón lẫn trả**. Cửa sổ **3 giây sau nhận việc** riêng biệt. Đổi làn nếu trống, không trống thì phanh/nhường; tình huống ngược chiều kín có thể cho người chơi giảm/dừng. Pause/đứng phục vụ không ăn hai giây xuất phát.
- Nhường dùng dự đoán/phanh/quét thân xe trước và sau va chạm xe–xe, không tắt collision. Tai nạn thật vẫn dùng luật té/kết thúc hoặc chờ CSGT hiện tại. Bản đang tắt công việc không chạy bảo vệ riêng này.
- Số dư có dạng `Coin: 0`, `Coin: 50`, `Coin: 1,000`; chỉ trả thưởng sau khi khách xuống xong. Không trả trước lúc nhận việc/đón khách; thua giữa chuyến không được thưởng.

### 2.8. WebGL, cài ứng dụng và offline

- Desktop chơi trực tiếp trên web; Chrome/Edge hỗ trợ cài PWA nếu môi trường trình duyệt đáp ứng.
- Android cài ứng dụng web từ Chrome/nút cài; chưa phải bản APK/AAB gốc.
- iPhone/iPad mở bằng Safari rồi **Thêm vào Màn hình chính** để chạy dạng ứng dụng web; chưa phải ứng dụng App Store/IPA.
- Cần mạng tải lần đầu. Service worker tải đủ shell + tài nguyên Unity mới báo sẵn sàng offline.
- Hỗ trợ màn hình ngang/dọc; cảnh game phủ khung nhìn trên điện thoại/tablet, HUD/nút bấm tránh tai thỏ/thanh vuốt.
- Nút menu nổi ba gạch đã ẩn trên mobile. Desktop còn điều khiển shell; có hướng dẫn, tải/lỗi/thử lại, trợ giúp cài và trạng thái offline.
- Icon hiện tại là **vector xe máy chính diện + sao vàng + con đường**, đỏ–vàng–xanh đậm; SVG nguồn không chứa ảnh nhúng. PNG 192/512/maskable/Apple 180 chỉ là các bản xuất tương thích nền tảng.
- Rời ứng dụng, ẩn tab hoặc mở trợ giúp khi đang chạy sẽ tạm dừng game.

### 2.9. Nhảy và chạy trên nóc

Space/vuốt lên nhảy tới; source21.64 là ô tô phụt khói trắng xuống từ4góc, hơi ngẩng rồi lấy cân bằng khi đáp (xe máy trên web1.39.0 vẫn ngóc bánh trước); hồi2 s, chỉ nhảy từ đường/nóc đang đỡ, không nhảy thêm trên không. **Source21.60:** độ cao theo tốc độ thực lúc cất cánh:17 m/s~3,63 m (giảm25%),30 m/s~4,84 m, W38,25 m/s~5,61 m,67,5 m/s~8,34 m; tăng đều theo tốc độ. W kích hoạt sau khi đã bay chỉ áp dụng độ cao cho cú sau; không bơm thêm vận tốc Y trong cú hiện tại. Độ cao là so với mặt đỡ lúc phóng. Có thể vượt xe máy nằm và đáp/chạy trên nóc bốn bánh nếu đủ cao; nhảy ở tốc độ thấp có thể không vượt được nóc tải cao. Xe tải đứng thẳng còn là mặt đỡ, xe nghiêng/lật là vật cản. Rời mép rơi thật; tiếp đất theo vận tốc, hấp thụ lực rồi cân bằng; đâm thân/vách vẫn té.

### 2.10. Tăng tốc

W/vuốt xuống tăng **125% = nhân 2,25** tốc độ mong muốn trong 4 s; không cộng dồn, hồi 2 s sau hết/hủy. Chỉ xe người chơi tăng tốc; mọi xe khác giữ tốc độ nền/hành vi của chính nó. Dùng chung với nhảy được. Té/bị bắt/dừng dịch vụ hủy boost. Khói phản lực trắng mềm từ dữ liệu volume Blender bám ống xả và pose xe, không bay rời khi ngóc bánh; pause/rebase được xử lý riêng.

### 2.11. Ánh sáng, màu phố và sương sớm

Ban ngày nắng trắng, trời xanh sáng; nhà/vỉa hè/cây vẫn dùng palette rực rỡ, không trắng bệt. Mỗi 30 s chuyển dần ngày → đêm, 30 s tiếp theo đêm → ngày; lặp 60 s và pause dừng đồng hồ. Đèn trước/hậu cả xe người chơi/NPC/CSGT và 70 đèn đường bật dần theo độ tối. Đèn pha theo đúng độ cao/hướng xe lúc nhảy, đáp và đứng trên nóc. Sương phủ có chiều sâu trong 6 s đầu mỗi lượt, tan dần và tắt hẳn; không xuất hiện lại mỗi vòng bình minh.

### 2.12. Khủng long Emerald Tyrant — đã có trên web1.40.0; đợt sau cua21.80 chưa phát hành

- Nhận model Meshy mới, rộng **4,65 m = 1,5 làn** ở tư thế gốc; giữ tỷ lệ, cao6,081 m/dài8,676 m. Cứ10giây chơi xuất hiện phía trước, **đi ngược chiều**, tốc độ đi riêng3m/s; W của người chơi không tăng tốc khủng long.
- Bước luân phiên, lấy đà/hạ thân/cúi đầu rồi xoay hông quất đuôi thấp qua cả4làn, có thông báo “QUẤT ĐUÔI · NHẢY LÊN!”. Phải nhảy đủ cao để qua đuôi; đuôi và thân/chân là các vùng va chạm vật lý riêng. Nhà/cây đứng yên; xe máy/ô tô/tải/quái xế/tải ngủ gật/**xe tăng/CSGT** đều có thể bị hất.
- Té thường: kết thúc sau ngã. Té khi bị đuổi: nằm chờ CSGT đến bắt rồi vào tù. NPC giữ hồi phục3s, CSGT1,2s. Timer/pose dừng khi pause, reset khi chơi lại; khủng long đang có tiếp tục đi lúc người chơi té để không chắn luồng bắt. Chi tiết asset/code/kiểm tra ở21.62. Đã có trên web từ1.40.0/21.66.
- **Source21.80, chưa build/phát hành:** sau mỗi lần rẽ hoàn tất có thêm1khủng long ở giao lộ kế tiếp phía trước;2giây sau khủng long thực sự xuất hiện sẽ có thêm1xe tăng ở giao lộ còn ở phía trước lúc đó. Lịch định kỳ10s/9s vẫn chạy độc lập. Nếu vị trí bị chắn/pool đầy, chờ và thử lại; không tạo chồng mô hình. Chi tiết21.80.

## 3. Cấu trúc dự án và những gì cần gửi cho Claude Code

```text
Taixecongnghe/
├─ huongdan.md                         # Tài liệu bàn giao này
├─ README.md                           # Giới thiệu và lịch sử tính năng
├─ Assets/
│  ├─ PhoViet/
│  │  ├─ Scenes/PhoViet.unity           # Scene đã tích hợp đầy đủ
│  │  ├─ Scripts/                      # Gameplay, GUI, vật lý, animation và các shader
│  │  ├─ Editor/                       # Builder, importer, upgrader, build web
│  │  ├─ Art/{Bikes,Cars,Streets,Passengers}/
│  │  ├─ Resources/                    # Pose nhảy, waypoint, khói boost/lửa và shader
│  │  ├─ Audio/Navigation/             # 3 câu chỉ đường MP3
│  │  ├─ Fonts/                       # Be Vietnam Pro và giấy phép OFL
│  │  └─ Generated/                   # Mesh/material đã persist, không phải rác
│  └─ WebGLTemplates/PhoVietPWA/        # Source HTML/CSS/JS/PWA
├─ ArtSource/Blender/                  # .blend và script Python tái tạo model
├─ AgentScripts/                      # Công cụ kiểm tra/chụp/nâng cấp chạy qua CLI
├─ Captures/                          # Báo cáo, JSON kiểm tra, ảnh bằng chứng
├─ Packages/                          # Package manifest và lock
├─ ProjectSettings/                   # Cấu hình Unity, scene build, input, version
├─ docs/                              # WebGL đã build, GitHub Pages phát hành từ đây
├─ .github/workflows/pages.yml         # Deploy artifact docs, không build Unity trên CI
└─ Builds/                            # Build cục bộ, bị gitignore
```

Khi chuyển dự án, giữ **Assets + toàn bộ `.meta`**, ArtSource, AgentScripts, Captures, Packages, ProjectSettings, docs và Git history nếu có. `Assets/PhoViet/Generated` có tài nguyên scene đang tham chiếu nên phải giữ. Không cần gửi `Library`, `Temp`, `Obj`, `Logs`, `UserSettings`, `node_modules` hoặc `.tools`; Unity/công cụ sẽ tạo lại khi cần.

Trước lần cập nhật tài liệu 22/10, có sẵn **5 nguồn Blender sửa cục bộ**: Cars, Jump, Motorcycles, Passengers, Streets; ba file chưa track là `PhoViet_Anhsangngaydem.blend`, `PhoViet_Motorcycles.backup-20261019.blend`, `Phoviet_Suongmu.blend`. Giữ nguyên, không tự reset/ghi đè/stage chúng. `huongdan.md` cũng chưa được track trước lần cập nhật này và có bổ sung xe máy v2 của Claude Code: đã đọc/giữ phần đó rồi đối chiếu, không thay bằng bản hướng dẫn cũ trong Git. Phiên xe máy Wave 23/10 thêm nguồn mới `PhoViet_Motorcycles_Wave.blend` + `pho_viet_wave.py` và không ghi vào `PhoViet_Motorcycles.blend` (theo lựa chọn “lưu file mới” của người dùng). Phiên phố Sài Gòn 24/10 thêm nguồn mới `PhoViet_SaigonStreets.blend` + `pho_viet_saigon_streets.py`, ảnh mẫu `ArtSource/Reference/Street/`, không ghi vào `PhoViet_Streets.blend`. Snapshot không thay cho `git status` lúc bắt đầu phiên mới. Sửa `.blend` không đồng nghĩa đã xuất FBX hoặc cập nhật build.

## 4. Bản đồ mã nguồn

Tất cả đường dẫn file trong các bảng dưới đây tính từ gốc repo. Các file `PhoViet*.cs` runtime chủ yếu là các phần của **cùng một `partial class PhoVietGame : MonoBehaviour`**; không gắn mỗi file thành một component riêng.

| File trong `Assets/PhoViet/Scripts/` | Trách nhiệm chính |
| --- | --- |
| `PhoVietGame.cs` | Dữ liệu xe/pool, Awake/Reset/Update/TickRide, spawn thường/ngang, điều khiển, camera, âm cơ bản, HUD/màn hình đầu/thua |
| `PhoVietActions.cs` | Điểm vào TryKick, double tap, cú ngã/bộ đếm, truy đuổi, bắt giữ và buồng giam |
| `PhoVietKickAssist.cs` | Chọn/dự đoán mục tiêu, tăng tốc áp sát, Swing/Return, quét giày từ animation, giữ động tác qua rẽ |
| `PhoVietJump.cs`, `BikeJumpPose.cs` | Quỹ đạo nhảy, mặt nóc chuyển động, đáp/rời mép, pose Blender |
| `PhoVietBoost.cs`, `BoostSmokeVisual.cs` | W/vuốt xuống, timer +125%/4/2, khói trắng Blender |
| `PhoVietRiderCrash.cs` | Xung lực/văng/ngã người chơi, kết thúc sau té hoặc nằm chờ CSGT |
| `PhoVietTrafficRecovery.cs` | Lưu hướng/đích rẽ trước tai nạn; NPC dựng sau 3 giây |
| `PhoVietSceneryContacts.cs`, `StreetObstacleSet.cs` | Quét va chạm nhà/cây/vật dụng theo hộp nhẹ, có xét cao và hệ đường xoay |
| `PhoVietCrashPoliceNavigation.cs` | A* và điểm tiếp cận để CSGT bắt người chơi đã té |
| `PhoVietRearWarning.cs` | Mũi tên đúng làn báo xe thường/nguy hiểm vượt sau |
| `PhoVietCollisions.cs` | Va chạm người chơi, swept SAT xe–xe, khối lượng, xung lực, trượt/xoay sau va chạm |
| `PhoVietTraffic.cs` | Khởi tạo lại tài xế, phanh/đèn phanh, tạt làn, xe băng ngang và rẽ vào đường chính |
| `PhoVietRoadDrivers.cs` | Xe dọc đường quyết định và thực hiện rẽ sang đường nhánh |
| `PhoVietReckless.cs` | Quái xế/xe tải ngủ gật hai chiều, vào từ phía sau, lạng, đâm nhà, khói/lửa, tái sử dụng xe |
| `PhoVietPolice.cs` | Dự đoán đường đi, chọn đường né/phanh, va chạm CSGT, ngã 1,2 giây và phục hồi |
| `PhoVietRoutePolice.cs` | CSGT theo vết quỹ đạo khi người chơi ôm cua/đổi khung đường |
| `PhoVietRoutes.cs` | Pool 5 dãy đường, mũi tên/vùng chọn rẽ, cung cua, rẽ muộn, rebase hệ tọa độ, tên đường và route trail |
| `PhoVietExitTraffic.cs` | Đợt xe từ phía sau/ngược chiều sau mỗi lần rẽ |
| `CityNavigation.cs` | `RideAddress`, `CityRoute`, gán tên phố cố định và tìm đường trên đồ thị có hướng |
| `PhoVietJobs.cs` | Sinh/chọn chuyến, chuyển giai đoạn, Coin, GPS, âm chỉ đường, dừng đón/trả, nhường xe phía sau |
| `PhoVietJobGui.cs` | Thẻ công việc/hit-test/bản đồ; kiểm tra khóa phát hành |
| `PhoVietJobTraffic.cs`, `PhoVietJobMarker.cs` | Nhường lúc nhận việc/tiếp cận/đón trả và điểm hẹn 3D, hiện bị khóa cùng jobs |
| `PhoVietPortraitHud.cs`, `PhoVietLandscapeHud.cs`, `PhoVietDistanceHud.cs` | Thanh trong gọn dọc/ngang; bố cục chỉ mét/tốc độ/kỷ lục khi jobs tắt |
| `PhoVietGestures.cs` | Nhận đủ ngưỡng ngay khi move, bridge pointer WebGL, một chạm một lệnh |
| `PhoVietWelcome.cs` | Thẻ trắng trong mở đầu nhỏ, nút bắt đầu/bánh răng/hit-test |
| `PhoVietGraphicsSettings.cs` | Cửa sổ đồ họa3mức, lưu lựa chọn, snapshot cấu hình theo nền tảng, icon bánh răng |
| `PhoVietDayNight.cs`, `DayNightLighting.cs`, `MorningFog.cs` | Chu kỳ ngày–đêm, pool đèn và sương volume sáu giây |
| `WreckFireVisual.cs`, `WaypointMesh.cs` | Flipbook lửa/khói và mesh mũi tên khối vát |
| `PhoVietWebRuntime.cs` | Safe area, cầu nối web pause/insets, ký hiệu GUI WebGL, chất lượng đồ họa trình duyệt |
| `UrbanRiderMotion.cs` / `UrbanRider.shader` | Cổ/ghi đông/bánh trước, IK tay, nghiêng/về cân bằng và mặt nạ màu/đèn riêng cho xe máy dân sự21.67 |
| `BikeVisualAnimator.cs` | Lấy mẫu hoạt ảnh xe máy/người lái, bánh xe, đạp, ngã, dựng xe, tiếp đất hình ảnh |
| `CarVisualAnimator.cs` | Chạy/phanh/va chạm xe bốn bánh, quay/đánh lái bánh, nhún thân, phản ứng cản/gương |
| `PassengerVisualAnimator.cs` | Lấy mẫu Idle/Walk/Board/Sit của khách |
| `StreetArt.cs` | Helper tạo hình học/material/text/sao và các model ban đầu/fallback/factory |
| `VietnamStreet.cs` | Module phố 80 m, nền đường, giao lộ, cửa hàng/cây/vỉa hè, tín hiệu, bảng tên và vật dụng phố |
| `JunctionStreetArt.cs` | Mesh 16 mũi tên giao lộ, vị trí cách vạch qua đường |
| `PursuitArt.cs` | Model mô tô CSGT ban đầu và cảnh buồng giam |
| `RecklessArt.cs` | Model sự kiện ban đầu, biển nguy hiểm và particle khói/lửa |

| File trong `Assets/PhoViet/Editor/` | Cách dùng/vai trò |
| --- | --- |
| `PhoVietBuilder.cs` | Dựng scene gốc, gộp mesh/persist tài nguyên, các menu nâng cấp lịch sử |
| `PhoVietBikeImport.cs` | Cấu hình FBX xe máy và 7 clip Legacy |
| `PhoVietBikeUpgrade.cs` | Thay visual xe máy/người lái, giữ root gameplay và các đối tượng ngữ nghĩa |
| `PhoVietCarUpgrade.cs` | Importer + thay visual 11 xe bốn bánh, giữ root/kích thước vật lý |
| `PhoVietRouteUpgrade.cs` | Cài 5 road rows, mũi tên, world text, bake xe đậu ở nhánh |
| `PhoVietStreetUpgrade.cs` | Factories/importer cảnh quan Blender, thay các dãy phố trong scene |
| `PhoVietPassengerUpgrade.cs` | Import 2 khách/4 clip/3 LOD, material, gắn tham chiếu khách và 3 clip chỉ đường |
| `PhoVietWebBuild.cs` | Cấu hình WebGL, build, hậu xử lý offline/cache/version và refresh shell |
| `PhoVietLightingUpgrade.cs` | Cài đèn/sky/profile/material, giữ palette phố rực rỡ |
| `PhoVietObstacleUpgrade.cs` | Tạo hộp vật cản nhẹ theo scene/model hiện có; shop/cây đọc `obstacles` trong JSON phố (`StreetArtMetadata`) |

## 5. Scene, trạng thái và vòng cập nhật

### 5.1. Scene đã lưu sẵn là điểm khởi đầu

Scene chính đã chứa root **`PHỐ VIỆT • Chuyến xe không ngừng`**, component `PhoVietGame`, camera/audio, tài xế, pool xe, CSGT, buồng giam, road rows và hai khách cùng các tham chiếu font/material/clip. Tên root này còn được JavaScript dùng trong `SendMessage`; đổi tên phải sửa cả web shell và công cụ kiểm tra liên quan.

Mở scene rồi Play là cách chạy bình thường. Không cần dựng lại scene mỗi lần mở project.

**Lưu ý builder:** `PhoVietBuilder.Build` tạo scene mới, thay thế scene đang mở, cài bộ xe và đường nhưng hiện **chưa gọi `PhoVietPassengerUpgrade.UpgradeOpenScene()`**. Vì vậy không coi menu “Dựng lại scene chính” là lệnh khôi phục đầy đủ 1.14 chỉ bằng một bước. Các menu nâng cấp cũ cũng còn gán số version lịch sử. Nếu thật sự cần tái tạo, sao lưu/kiểm tra diff, đọc chuỗi builder hiện tại, bổ sung bước khách/âm chỉ đường, cấu hình web lại và kiểm tra đầy đủ trước khi lưu/phát hành.

### 5.2. Trạng thái game

```text
Ready → StartRide → Riding ↔ Paused
Riding → va chạm → Falling (văng/trượt theo vật lý)
Falling + không bị đuổi lúc té → té xong → GameOver
Falling + đang bị đuổi lúc té → nằm chờ CSGT tới → Arrested → Jailed
Riding + CSGT bắt được → Arrested → Jailed
GameOver/Jailed → RestartRide → ResetRide + StartRide

Luồng được giữ nhưng đang khóa:
Riding chưa có việc → ShowJobOffers → Paused + JobOffersOpen
Paused + JobOffersOpen → AcceptJob → Riding + Pickup
```

Không còn Recovering hoặc LivesRemaining cho người chơi. `CrashDuringPursuit` chốt trạng thái đuổi lúc bị tông, `RiderFallComplete` đánh dấu đã té xong. CSGT/NPC vẫn có phục hồi riêng.

`IsChased`, `IsTurning` và `WorkPhase` là các trạng thái bổ sung; không nên gộp tất cả vào một enum hoặc bỏ các kiểm tra hiện có mà chưa đánh giá ảnh hưởng.

Pause chủ yếu được triển khai bằng `State`, không phải luôn đặt `Time.timeScale = 0`. `Update` vẫn chạy để đọc nút/menu, xử lý chạm chọn việc và một số phần giao diện/cảnh vật. Timer gameplay mới phải được gắn với đúng trạng thái; không giả định mọi `Time.time` hoặc `Update` sẽ tự đứng lại khi `State == Paused`.

### 5.3. Thứ tự quan trọng

`Awake` cấu hình world text, âm thanh, CSGT và `ResetRide`. Reset tạo seed mới, xóa chuyến chưa hoàn thành/khách, đặt lại thành phố và đường, hành động/truy đuổi, xe nguy hiểm, giao thông, pool và camera; số dư Coin/kỷ lục đọc từ dữ liệu lưu. **Từ21.76, điểm xuất phát lấy từ vị trí xe/model được kéo và Save trong Scene**, không ép về vị trí cũ. Hệ đường cuộn vẫn dùng player local z=0 bằng cách dịch khung runtime đến điểm đó, đồng bộ pha phố/giao lộ/camera; chi tiết21.76.

`Update` đọc bàn phím, swipe và chạy `TickRide` khi đang `Riding`, sau đó gọi `UpdateWork` kể cả khi menu việc đang pause. Ngoài ra xử lý chuyển cảnh ngã/bị bắt, thông báo, camera, tín hiệu và âm thanh.

Thứ tự trong `TickRide(dt)` hiện là:

1. Khi đang hỗ trợ đạp, chia dt thành các bước tối đa 10 ms, vẫn cộng đủ thời gian/quãng đường.
2. `AdvanceDaylight`, `TickWorkMotion` (thoát ngay khi jobs tắt), timer hành động, `TickJump`, `TickBoost`.
3. Cập nhật độ khó/cruise; cộng tốc độ bay +3 m/s, nhân W 2,25, giới hạn dịch vụ nếu có, `AssistedKickSpeed`, rồi `JobAcceptanceSpeedLimit`. **Không giới hạn riêng tốc độ cua.**
4. Cộng Distance, scroll; nhập làn/độ nghiêng, `BeginOrContinueTurn` tiêu thụ đúng quãng đường, giữ độ cao nhảy, `TickAssistedKickMotion`.
5. Lưu pose/vận tốc trước frame cho người chơi; `TickTrafficMotion` xử lý các loại xe, hồi phục, nhường và va chạm xe–xe.
6. `ResolveJumpSurface` xác định nóc/đáp/rời mép, cập nhật hai hàng chờ cảnh báo xe vượt sau.
7. `ResolveRiderContacts`: kiểm tra thân xe/cảnh vật với độ cao và thời điểm chạm; nếu đã té thì thoát.
8. `TickAssistedKickContact`: chỉ bây giờ giải tiếp xúc giày, tránh đạp bỏ qua va chạm thân xe trước đó.
9. Trừ timer xuất phát dịch vụ chỉ khi thực chạy, thu hồi xe, `TickChase`, pose đạp.
10. `EndRouteFrame` thực hiện rebase nếu hết cua; khi không đang cua mới tick đợt xe sau cua, lịch giao lộ và sinh xe thường (tốc độ đợt nhân TrafficDensity).

Nhánh `Falling` chạy vật lý té và điều hướng CSGT riêng, không khôi phục điều khiển lái cho người chơi.

Thứ tự này giải thích vì sao chỉ sửa hình ảnh hoặc chỉ chặn một nhánh chuyển động không đủ để sửa lỗi xuyên xe.

## 6. Tọa độ, pool và độ khó

### 6.1. Hai hệ tọa độ khác nhau

- **Hệ hiển thị Unity cục bộ:** sau đoạn đường thẳng, xe người chơi gần `z = 0`, đường nằm theo `+Z`, `X` là bề ngang, `Y` là độ cao. Thế giới và xe khác chuyển động tương đối với người chơi.
- **Hệ thành phố logic:** `Vector2Int`/`Vector2` trong `CityNavigation`; hai thành phần tương ứng mặt phẳng phố, hướng `N/E/S/W`. Không phải trực tiếp `Transform.position` của scene.
- Vận tốc đường thực của xe dọc `v.speed`; thay đổi vị trí tương đối thường là `(v.speed - Speed) * dt`. Xe ngược chiều có tốc độ âm.
- Khi tính xung lực, dùng vận tốc đường thực (`BodyVelocity`/`motionVelocity`), không nhầm tốc độ scroll thành tốc độ vật lý.
- `TrafficPaceSpeed = min(17 + DifficultyTime*0.12, 30)` là tham chiếu NPC độc lập W, nhảy, tốc độ áp sát và chaseBoost của người chơi. Giữ phép trừ `Speed` khi tính vị trí tương đối; đó không phải tăng ga NPC.

Thông số nền:

| Thông số | Giá trị |
| --- | --- |
| `LaneX` | `[-4.65, -1.55, 1.55, 4.65]` |
| Module phố `VietnamStreet.Length` | 80 m |
| Tâm ngã tư trong module nguồn | z = 70 m |
| Bề rộng mặt đường/đường giao nhau | 14 m |
| `DifficultyMultiplier` | 3 |
| Tốc độ khởi hành/giới hạn đường thường | 17 / 30 m/s |
| Tốc độ cua thường/cua muộn | giữ cùng công thức đường thẳng, không còn cap 10/8 |
| W | nhân 2,25 trong 4 s; hồi 2 s sau hết/hủy |
| Nhảy | vận tốc lên 14,6 m/s; trọng lực 22 m/s²; windup 0,24 s; hồi 2 s |
| Khung mô phỏng từ Update | `min(Time.deltaTime, 0.05)` |

### 6.2. Ngẫu nhiên và tái sử dụng xe

`ResetRide` tạo `RunSeed` mới bằng `Guid.NewGuid().GetHashCode()` và tránh trùng seed lượt ngay trước. `rng` điều khiển giao thông; việc/địa chỉ dùng RNG tách từ seed để giảm phụ thuộc giữa lựa chọn công việc và giao thông.

Pool hiện có **28 xe giao thông**: 26 xe thường và 2 slot `eventOnly` cho quái xế/xe tải ngủ gật. Có 17 xe máy giao thông và 11 xe bốn bánh. Tài xế/CSGT không chiếm slot giao thông. Khách cũng là actor riêng.

`SpawnTraffic` dùng đối tượng đang rảnh, lựa chọn ngẫu nhiên; `SpawnCrossTraffic` lọc silhouette xe máy/xe lớn rồi chọn ngẫu nhiên. Các lần spawn reset cờ phanh, đổi làn, ngã, FX, route và contact cooldown qua `InitializeDriver`. Xe được `RetireVehicle` đưa về pool khi ra khỏi vùng, đi hết đường nhánh hoặc xác xe đã đủ lâu.

Không dời xe đang hoạt động để “lấy chỗ” cho xe mới. Các spawn đặc biệt/đợt sau cua kiểm tra chồng lấn với xe khác và CSGT; pool đầy thì thử lại hoặc bỏ lượt spawn đó.

### 6.3. Độ khó hiện tại

```csharp
DifficultyTime = timeRiding * 3f;
Difficulty01 = Mathf.Clamp01(DifficultyTime / 110f);
cruiseSpeed = Mathf.Min(17 + DifficultyTime * .12f, 30)
              + (IsChased ? chaseBoost : 0);
```

Vì vậy trên đoạn thẳng không bị dừng/cua, tốc độ tăng khoảng **0,36 m/s mỗi giây chơi**, đạt 30 m/s sau khoảng **36,1 giây**. Đây là kết quả tăng nhịp ban đầu gấp đôi rồi tăng thêm 50%; không nhân đôi/nhân 1,5 lại mỗi lần chỉnh tính năng khác.

- Mỗi đợt thường có 1 xe; khi `DifficultyTime > 12` (hơn khoảng 4 giây chạy) có thêm xe thứ 2, lệch làn/vị trí.
- Khoảng sinh đợt thường: `Lerp(2.5, 1.35, Difficulty01) * random(0.8, 1.2)`.
- Mỗi giao lộ được đặt lịch 2, 3 hoặc 5 xe ngang theo mốc `DifficultyTime < 18`, `< 60`, hoặc cao hơn; tỷ lệ xe lớn/tốc độ ngang cũng tăng.
- Trong **10 giây Riding đầu**, `TrafficDensity=0.5`: số mở đầu/xe ngang/đợt sau rẽ dùng `ceil(count*0.5)`, đồng hồ spawn thường chạy nửa tốc độ; xe nguy hiểm dùng chung đợt đó. Sau 10 s density=1.
- Đứng phục vụ khách (khi được bật lại) không tăng timeRiding; pause không chạy tick đường/độ khó. Mốc này tách với sương sáu giây và chu kỳ ngày–đêm.
- Các va chạm và xe lấn làn có thể làm đường bị chặn dù một đợt spawn không tự sinh kín cả bốn làn; không cam kết luôn tồn tại đường tránh an toàn ở mọi thời điểm.

## 7. Giao thông, va chạm và luật thua

### 7.1. Hành vi xe

`TrafficVehicle` lưu model root, kích thước nửa thân, tốc độ/làn, vị trí trước, vận tốc thực, trạng thái phanh/đổi làn/rẽ, trạng thái ngã/xung lực, xe sự kiện, FX và thông tin đường cũ. Thêm trường mới phải xem cả spawn, reset, retire và rebase.

`BeginSuddenBrake` chỉ áp dụng xe thường cùng chiều còn hoạt động; phanh khoảng 0,95–1,6 giây, mục tiêu gần 10% tốc độ nền với giới hạn tối thiểu, rồi tăng lại. Xe máy/ô tô/xe tải dùng chung hệ thống, đèn phanh gắn theo vị trí đuôi model.

`BeginSuddenLaneChange` từ21.67 dùng riêng cho Urban Rider đường cong quintic u³(10−15u+6u²), vận tốc ngang giải tích, thời gian Clamp(12/max(|speed|,6),0,65,1,6)s; bánh/cổ/thân do UrbanRiderMotion điều khiển theo độ cong thực. Ô tô/xe tải và xe máy cũ không có component này giữ SmoothStep0,7/0,9/0,52s. Xe thường cùng chiều đổi giữa làn 2/3; xe ngược chiều có thể bước sang các làn 0–3, kể cả lấn sang người chơi. Hành vi được kích hoạt theo cooldown và tình huống xe song song, không phải một animation dịch chuyển tức thì.

Xe ngang có khoảng 75% lựa chọn rẽ khi `trafficSurprises` bật; dùng cung tròn rồi chuyển về xe dọc, cập nhật chiều/làn/tốc độ. Xe dọc tới giao lộ có xác suất chọn rẽ riêng khoảng 68%, thực hiện qua `TryBeginRoadTurn`/`TickRoadTurn`; hết cua chuyển sang `routeDetached` để tiếp tục chuyển động ở nhánh/khung đường cũ.

### 7.2. Xe nguy hiểm

`MaybeSpawnReckless` vẫn dùng đợt xe thường. Hướng ngược chiều sinh xa phía trước; hướng vượt sau đi qua `QueueRearReckless` → cảnh báo → spawn. Mỗi đợt sau chỉ một xe và một làn 2/3. `MovingOncomingDanger`/`MovingRearDanger` chặn xe vượt sau mới khi xe nguy hiểm tương ứng vẫn đang chạy (chưa knocked/wallCrashed); chặn countdown trong cua/dừng dịch vụ, khi mở lại yêu cầu đủ 2 s. Không reset quy tắc để sinh bất ngờ ngay sau cua.

Xe vượt sau sinh ngoài camera đúng làn đã báo; khoảng spawn bị chắn thì giữ làn/chờ, không chọn làn chưa cảnh báo. Tốc độ sinh/vượt/lạng dùng TrafficPaceSpeed và tham số NPC, không bám W của người chơi. Sau khi tới gần/vượt thì rời enteringFromBehind để lạng. `SpawnReckless` mức thấp phục vụ test/preview không thay cho hàng chờ gameplay. Xe ngược chiều vẫn sinh phía trước; hướng/nhịp lạng và lần đâm nhà được ngẫu nhiên hóa.

`CheckBuildingImpact` xét biên mặt tiền khoảng `|x| + footprint >= 9.3`, đồng thời loại vùng ngã tư theo chu kỳ module 80 m. Xe đâm nhà chuyển thành xác xe, dừng chuyển động, có FX khói/lửa; model bốn bánh nhận hoạt ảnh va chạm tương ứng.

### 7.3. Vật lý xe–xe

Đây là mô hình gameplay tự viết, **không phải các xe đều dùng Rigidbody/WheelCollider/PhysX để quyết định di chuyển**. Không thêm một hệ Rigidbody thứ hai lên cùng root rồi để hai hệ cùng điều khiển transform.

- `VehicleContact`: swept separating-axis test trên 4 trục thân xe, kiểm tra khoảng thời gian các hình chiếu thật sự cùng chồng lên nhau; có xét chiều cao để xe đang văng trên cao không tự va giả với xe dưới đất.
- Kích thước thân đổi theo yaw; xe máy nằm có bề ngang chiếm chỗ lớn hơn lúc đứng.
- Khối lượng quy ước: xe máy kèm người **220 kg**, ô tô **1.350 kg**, xe giao hàng **2.200 kg**, xe tải **4.500 kg**. `VehicleMass` còn dựa vào kích thước/tên model nên đổi tên xe tùy tiện có thể ảnh hưởng phân loại.
- **Xe tăng (21.33):** `TrafficVehicle.armored` coi như khối lượng vô hạn trong `ResolveVehicleCollisions` (`invA/invB=0`, không `ActivateCollisionBody`, không lùi vị trí swept/tách thân), trong va chạm người chơi (`ApplyRiderImpulse`) và CSGT (`PhoVietPolice`): giữ tốc độ, làn và hướng; xe kia nhận toàn bộ xung lực và văng. `VehicleMass` không đổi cho các phép khác.
- Xung lực dựa vào vận tốc tương đối, hướng pháp tuyến, khối lượng, điểm tác động và mô men quán tính. Hệ số nảy đang 0,18, ma sát va chạm 0,28.
- Va lệch tâm tạo yaw/roll/pitch; có trọng lực, ma sát mặt đường và giảm xoay. Xe tải không bị xe máy bắn văng như cùng khối lượng.
- Swept hit đưa xe về thời điểm chạm thay vì để xuyên nhau hết frame; có tách thân khi chồng lấn và cooldown cặp va chạm.
- Trạng thái `knockFromPlayer` truyền ghi nhận dây chuyền từ cú đạp; các tai nạn tự phát không tự gọi CSGT.

Không phải mô phỏng phá hủy/biến dạng vỏ xe liên tục. Hoạt ảnh cản/gương phản ứng theo hướng lực là phần hình ảnh riêng.

### 7.4. Va chạm người chơi — hợp đồng bắt buộc

`ResolveRiderContacts` quét vị trí tương đối, có yaw/chiều cao/thời điểm tiếp xúc; so cả xe và cảnh vật để chọn va chạm xảy ra trước. Tiếp xúc thân thật gọi `BeginRiderImpact`, chốt hướng/vận tốc/điểm tông và chuyển Falling; xe/người bị văng bởi xung lực, trọng lực, ma sát và spin. Va chạm nhanh/lệch tâm thay đổi độ xa và hướng té. Kết thúc chỉ sau khi té/trượt xong: không bị đuổi thì GameOver; đang bị đuổi thì nằm chờ bắt, không tự dựng xe. Không có ba mạng.

Runtime còn các trường grace lịch sử nhưng **khi IsChased=true không được bỏ qua contact**. Cú đạp mới không dựa vào miễn va chạm để áp sát; đạp trúng phải kiểm tra giày và thân tách biệt. Đáp nóc hợp lệ được giải trước va chạm thân, dùng cùng thời điểm/hình bao cao để không coi đang đứng trên nóc là đâm vào xe. Không thêm miễn va chạm cho cả cú nhảy hoặc cả dịch vụ.

`YieldRearForService` là nhường/phanh vật lý khi JobsAvailable=true, không bỏ ResolveRiderContacts. Hiện bị khóa cùng dịch vụ; chi tiết khoảng 22 m, hoạt ảnh và 2 s chạy lại ở mục 10.4.

### 7.5. Hồi phục NPC và va chạm cảnh vật

`PhoVietTrafficRecovery` ghi hướng/tốc độ/làn/hệ trục/đích rẽ trước xung lực đầu tiên. Va chạm mới đặt lại age. Sau 3 s, tìm làn trống đúng chiều, dựng ≥0,65 s (thời gian = max(0,65; 1,5×(quãng trượt + góc xoay×nửa chiều dài)/10 m/s), để điểm xa nhất của thân không quét nhanh hơn 10 m/s) rồi đi tiếp; chỉ lấy hệ trục phố ngang khi xe thật sự băng ngang hoặc tâm xe nằm trong bốn làn của phố ngang (≤6,2 m từ tâm ngã tư), hướng chạy gần vuông góc với đường thì chọn chiều làn theo nửa đường xe đang đứng (21.50); đường rẽ lưu nối từ vị trí ngã tới đích cũ. Pose, vận tốc và đường phục hồi phải scroll/rebase cùng thế giới. Giữ slot đủ lâu để xe hồi phục; bị tông lúc đang dựng cũng phải nhận lực và reset timer. CSGT giữ cơ chế 1,2 s, không dùng hệ 3 s. Xe nguy hiểm wallCrashed nằm cháy không hồi phục.

`PhoVietObstacleUpgrade` tạo **735 nhóm / 8.050 hộp** nhẹ (phố Sài Gòn 24/10 sau 21.26, bỏ hộp “Xe đẩy bán hàng”; 21.23: 8.120; bản 21.19 5.515–7.955, trước nữa 4.575) theo nhà, biển, mái hiên, ban công, quầy, bàn ghế, chậu cây, thùng rác, tủ nước, kệ, dù, bảng thực đơn, cây, xe đậu và cột trên 5 road rows. Hộp shop/cây đọc từ JSON Blender. `StreetObstacleSet` lưu hộp trong local của cụm; `PhoVietSceneryContacts` quét trong đúng hệ trục kể cả row đã xoay và xét cao để nhảy qua vật thấp. Cảnh vật đứng yên, chỉ xe/người văng; không gắn mesh collider/PhysX thứ hai cho toàn cảnh. Đây là số lịch sử; **source21.73** sau thay Petshop có770nhóm/7.659hộp; **source21.74 hiện có770nhóm/7.670hộp** do xếp lại các gian Petshop để biển xanh không kề nhau. Các hộp đồ cũ bị bỏ chỉ tạiPetshop, tiệm thường được phục hồi đúng mẫu. Cần tái kiểm sau dựng lại scene.

### 7.5. Khủng long (21.62)

`PhoVietDinosaurs.cs` điều khiển một pool riêng, không thêm Rigidbody/WheelCollider hoặc dùng tốc độ W làm tốc độ NPC. Collider đuôi lấy6điểm từ chính xương đang deform skin; quét các capsule qua vị trí trước/hiện tại ở bước<=1/120s, kèm khoảng hở Y và footprint xe. Mỗi xe chỉ nhận một xung lực trong một cú quất. Riêng cú hất khủng long được phép tác động cả xe tăng; xe tăng vẫn miễn bị xe thường đẩy như cũ. Không gán công lao cú hất cho người chơi/không gọi CSGT mới vì khủng long đánh NPC.

## 8. Húc ngang/đạp, CSGT và cảnh bị bắt trong code

**Source21.64 hiện tại:** `IsPlayerCar=true`, `PlayerCarVisual` dùng Blue Supercar và `TryKick` chuyển sang áp sát/húc bằng tiếp xúc thân xe như21.57; không cần giày/clip chân. Player xe máy21.58 là bản web1.39.0. Không chạy lại installer Petshop cũ; dùng `PhoVietSupercarUpgrade` cho xe mới.

### 8.1. Chọn mục tiêu và áp sát (1.30.3–1.30.5)

`TryKick` đi vào `BeginAssistedKick` trong `PhoVietKickAssist.cs`; từ chối khi không Riding, service stopped, đang bay/đứng nóc/đang đạp hoặc cooldown. Chọn xe active, chưa ngã/cháy, halfWidth<=0,7 và cùng khoảng cao. Từ 1.30.5 **không loại crossing/đang rẽ**: áp dụng cả gần giao lộ khi người chơi đi thẳng, rẽ trái hoặc rẽ phải. Mỗi lần chọn một mục tiêu hợp lệ, không đạp diện rộng.

Khoảng ngang lựa chọn 0,75–3,65 m là vùng có thể **tiến tới**, không phải tầm chân để hất ngay. Cùng chiều gần song song xét khoảng dọc ±2 m; ngược chiều/cắt hướng xét 0–16 m phía trước và đủ thời gian tiếp cận theo vận tốc tương đối. Ưu tiên quái xế trong mục tiêu an toàn. Dự đoán vị trí thân mục tiêu, yaw, vận tốc ngang và tiếp tuyến cung cua, có `KickTravelRatio` bù tiến độ khi đang lệch khỏi tâm quỹ đạo.

Luồng **Approach → Swing → Return**. `AssistedKickSpeed` tăng tốc bắt kịp theo vận tốc dự đoán (bước tăng có giới hạn); cap dựa trên tối đa 2,25×TrafficPaceSpeed, không nhân chồng W. Tốc độ ngang tối đa 32 m/s, Swing 0,30 s, cooldown khởi tạo 0,8 s. Căn lại tốc độ trong Swing để xe không chạy thoát trước nhịp vung. CurrentLane giữ làn gốc; áp sát là offset riêng trong hệ hướng tài xế.

### 8.2. Chạm giày, va chạm và trở về làn

`BikeVisualAnimator` cache 65 mẫu gót/mũi cho mỗi chân từ clip KickLeft/KickRight Blender hiện có. `KickSwingPose` cấp cùng tiến trình Swing cho pose hiển thị và phép quét; không phát animation và lực bằng hai timer độc lập. Vùng thân xe hẹp loại khoảng rỗng quanh tay lái; giày có bán kính 0,08 m. Chỉ khi giày tới vùng thân trong cửa sổ Swing mới `KnockVehicle`, tăng KickCount và gọi CSGT nếu là xe thường. Hụt/hủy không có lực, không truy đuổi. Không tạo mới animation Blender cho ba bản sửa này.

`TickRide` chia tối đa 10 ms khi hỗ trợ đạp; phép quét giày có bước nhỏ để không bỏ lỡ xe ngược chiều. Lối áp sát/quay về dùng hộp có hướng, dự đoán xe và cảnh vật, xét mục tiêu đang ngã nở hình bao dần. Thân xe được kiểm tra trước giày; không miễn va chạm. Nếu đường chắn hoặc mục tiêu đổi đường quá xa thì hủy, giữ/chờ lối về làn, không teleport/ép trúng. `spawnGeneration` ngăn hất nhầm slot pool đã được tái sử dụng.

Trở về bắt đầu trong nhịp thu chân. MoveLane/nhảy/W/va chạm/bị bắt/dịch vụ có thể ngắt; pause giữ timer, reset xóa. Khi hết cua `RebaseAssistedKick` biến đổi offset/pose/điểm tiếp xúc, không xóa động tác giữa chừng. Xe ngã dùng độ rộng theo tiến trình té; độ nghiêng lái trước cú đạp không được nở hình bao hồi tố gây va chạm giả.

### 8.3. Truy đuổi khi đang chạy

Đạp trúng xe thường lần đầu `StartChase`; quái xế tự vệ không gọi mới/xóa cuộc đuổi đang có. CSGT bắt đầu khoảng 38 m sau; chaseBoost nền tăng 0,28 m/s/s tới 12 m/s. Tham chiếu NPC/CSGT không tự nhân theo W/áp sát. `ChoosePolicePath`/`PolicePathClear` dự đoán giao thông, chọn lối/phanh; `ResolvePoliceContact` vẫn xử lý xung lực và ngã 1,2 s. Cua dùng routeTrail/SampleRoute với kiểm tra quét; không nhảy qua xe để về sau người chơi.

### 8.4. Khi người chơi đã té trong cuộc đuổi (1.30.2)

Bổ sung21.62: cú hất có thể đưa người chơi vào nhánh ngang dù không bấm rẽ. `CrashPoliceRoad` nhận cả mặt đường ngang thực ở pha giao lộ70±6m, |x|<=40m; planner mở miền ngang khi điểm đầu/đích ở đó. Tránh CSGT đứng ở |x|~8,4m không tới được người chơi đã văng ra nhánh ngang. Không đi xuyên mặt tiền; giữ tránh các xe đang chắn.


`CrashDuringPursuit` chốt lúc va chạm. Sau khi té hoàn tất, người chơi nằm lại; không tự lái/nhảy/boost/đạp hoặc chạy tiếp. `PhoVietCrashPoliceNavigation.cs` ưu tiên đường thẳng nếu thoáng, nếu chắn lập **A* lưới 1,2 m**, lưu waypoint và có thể chạy tạm xa để vòng ra khỏi túi xe. Mỗi bước kiểm tra thân có yaw bằng quét SAT hai hộp xoay thật (không dùng AABB của xe xoay), đệm 0,08 m chỉ cho xe đang chạy/xoay/đứng dậy (xe đứng yên dùng hình thật), sau va chạm CSGT được đẩy ra cách thân xe 0,1 m (> đệm) để không tự khóa mọi hướng đi (21.50); vật cản và dự đoán xe di chuyển; cập nhật đường khi vật cản/mục tiêu thay đổi, scroll và rebase waypoint đúng hệ.

Mục tiêu bắt là điểm có thể đứng quanh người té, không cố đi vào tâm đang nằm trong xe. Hỗ trợ hai nhánh mặt đường khi té giữa cua. Không có đường thì chờ tính lại, không xuyên/teleport/bắt từ xa bằng timer. CSGT té thì đợi dựng 1,2 s. Chỉ khi đã đứng dậy, người chơi té xong và khoảng cách thực <=2,2 m mới `Arrest` → `TickArrest` khoảng 1,1 s → `Jailed`. Jail là đối tượng riêng cùng scene; camera và ánh sáng riêng, có nút chơi lại.

## 9. Đường vô tận, giao lộ và phép rebase

### 9.1. Pool hình học

`RoadRow` gồm một root và 7 module 80 m. Có **5 rows**, tổng 35 module đã chuẩn bị: đường chính, nhánh chuẩn bị và các phần đường cũ cần giữ khi rẽ. `blocks` trỏ tới 7 module của đường hiện tại; không liên tục Instantiate cả thành phố trong runtime.

`ScrollRoutes` trượt đường ngược hướng xe; module qua phía sau được cộng lại 7 × 80 m để tái sử dụng. Giữ đồng bộ đường nhánh và `routeTrail` trong cùng phép scroll.

**Khối cầu (21.35):** mỗi khối có thêm `RiverBlock` + con "Cầu vượt sông • Blender" (tắt sẵn). `AssignRiverRow` (reset: 2 khối đầu là phố; nhánh giao lộ: khối đầu là phố) và `AssignRecycledRiverBlock` (khi khối được cuộn +7×80 m, xét khối ngay sau trong phạm vi 6 m) chọn cầu theo luật: sau cầu luôn là phố, đã 2 khối phố liền nhau thì bắt buộc cầu, còn lại 50%. Dùng `riverRng` riêng (hạt `RunSeed^0x5bd1e995`) để không đổi chuỗi ngẫu nhiên của giao thông. `riversEnabled=false` tắt hẳn (kiểm tra cũ dùng cờ này).

**Xe văng xuống sông (21.63):** mọi nhóm xe giao thông, gồm xe máy/quái xế/4 mẫu ô tô/tải ngủ gật/xe tăng, hủy hồi phục trên đường khi ra ngoài mặt cầu trên vùng nước, tiếp tục rơi theo vận tốc va chạm, bắn nước rồi chìm. Tra cứu mọi dãy đường đang hiện, kể cả đường cũ/nhánh đã xoay; ưu tiên mặt cầu thật nếu các hệ đường chồng nhau. `TickRiverAftermath` tiếp tục cả quỹ đạo hất văng chưa tới nước và quá trình chìm sau thẻ kết thúc. CSGT dùng cùng luồng chìm, hết truy đuổi theo chốt ở2.5; quay khung sau cua phải đổi cả vận tốc/góc nghỉ của xe đang chìm. Xe va chạm trên đất vẫn hồi phục3s, CSGT trên đất vẫn1,2s; xác cháy trên đất giữ nguyên.

### 9.2. Chọn rẽ và hình mũi tên

- `CanTurnFromLane`: lane 0/1 rẽ trái, 2/3 rẽ phải. `SelectJunctionTurn` nhận chọn trước tâm 12–30 m khi xe đã gần tâm làn (<0,6 m).
- Lệnh rẽ đã chọn được giữ; chuyển ra làn khác hủy lệnh. Ngoài vùng rẽ, thao tác tiếp tục là đổi làn.
- Trong lòng giao lộ, NextJunctionDistance từ -7 đến +7 m: x<=-0,65 được rẽ trái, x>=+0,65 được rẽ phải. Chọn lane 0/1 hoặc 3/2 theo vị trí thực (mốc ±3,1), giữ số thứ tự sau cua.
- Lệnh đã chọn trước bắt đầu tự cua khi gần khoảng **11,65 m** tới tâm và vẫn còn đúng làn.
- Mũi tên là mesh riêng `PV_JunctionArrows16`, đặt ở độ cao nhỏ trên mặt đường. Vị trí trên hai nhánh được đẩy ra xa hơn để không đè lên vạch ngang. Không gộp mất mesh này khi chạy combiner.

### 9.3. Cua và đổi khung tọa độ

Cua đặt trước dùng Bézier bậc năm, tiếp tuyến và độ cong về 0 ở hai đầu nối đường thẳng; rẽ muộn dùng Bézier bậc ba nằm trong hợp mặt đường. Bảng độ dài cung **128 đoạn/129 mốc** đổi quãng đường sang tham số. Tick chia đúng phần đi thẳng trước vạch vào cua và phần qua cua ngay frame đầu; frame cuối giữ quãng dư tiến theo tiếp tuyến đường đích trước rebase. Không ép Speed về 8/10 hoặc hãm về điểm cuối. Camera theo delta thực, tăng bám theo tốc độ khi đang/vừa cua và theo độ cao nhảy. Cache TextMesh tên phố, chỉ đổi chữ khi cần; kết thúc cua xóa lean dư để không lắc một nhịp ở frame thẳng kế tiếp.

`FinishJunctionTurn` không chỉ xoay camera. Nó tính phép quay ngược 90 độ và gốc mới, áp dụng cho:

- Các row đường và module đang giữ.
- Vị trí/hướng xe, vị trí trước, vận tốc ngã/vận tốc chuyển động, góc crash.
- Các vector bắt đầu/hướng cua của NPC.
- FX đang nằm ở đường cũ và offset của FX.
- CSGT, góc ngã CSGT, camera và toàn bộ điểm/hướng trong trail.

Sau đó chọn row mới thành đường chính, đưa tài xế về `z = 0`, giữ đúng `LaneX[CurrentLane]`, cập nhật heading/node thành phố và chuẩn bị giao lộ kế tiếp. Xe đang ở đường cũ tiếp tục theo `routeDetached`/`routeVelocity` thay vì bỗng quay theo đường mới.

**Nếu thêm một đối tượng gắn với thế giới hoặc một vector vận tốc mới, phải quyết định nó có cần scroll/rebase không.** Chỉ đổi vị trí mà quên vị trí trước/vận tốc sẽ tạo xuyên xe, xung lực sai hoặc FX bay khỏi xe.

### 9.4. Xe xuất hiện sau cua

`QueueExitTraffic` chọn 3–5 xe sau và 3–5 ngược chiều ở mật độ đầy, đầu lượt áp TrafficCount làm tròn lên. Xe sau đi qua hàng chờ riêng từng lane 2/3, **báo đủ 2 s đúng lane**; countdown không tiêu trong cua/dừng dịch vụ, pause đóng băng, restart giải phóng xe chờ. Lane đang bị chiếm giữ cảnh báo/thử lại, không nhảy sang lane chưa báo. Model được reserve, spawn khác không được lấy slot đó. Thời hạn lập đợt 12 s; chỉ tăng số đã sinh khi tạo được xe, không tăng mật độ chỉ vì thêm cảnh báo. Xe thường và nguy hiểm cùng lane dùng một icon đỏ viền vàng, các chỉ báo nằm ngoài mép màn hình được giãn theo thứ tự để không chồng. Xe từ phía trước/ngang không dùng cảnh báo sau này. Tốc độ NPC lấy TrafficPaceSpeed, không lấy W hiện tại.

## 10. Công việc, Coin, GPS và hành khách trong code — giữ lại nhưng khóa

**1.30.6:** toàn bộ luồng mô tả tại đây hiện bị `JobsAvailable=false` chặn. Đây là tài liệu để bảo trì/mở lại sau yêu cầu người dùng, không phải tính năng đang xuất hiện. Chi tiết khóa ở mục 21.

### 10.1. Dữ liệu và vòng đời

`RideJob` chứa `pickup`, `dropoff`, `reward`, `pickupMetres`, `tripMetres`, `female`. `RideAddress` gồm:

| Trường | Ý nghĩa |
| --- | --- |
| `node` | Đầu chuẩn của cạnh đường trên lưới |
| `axis` | 0 = cạnh theo Bắc, 1 = cạnh theo Đông |
| `offset` | Khoảng cách dọc cạnh 80 m |
| `curb` | Phía lề cố định theo trục chuẩn, giá trị -1/+1 |
| `Point` | Tọa độ logic điểm hẹn |
| `CurbDirection` | Vector từ đường ra phía lề; khi đi ngược hướng, trái/phải theo người lái sẽ đổi đúng |

```text
None → hiện 3 việc / pause → AcceptJob → Pickup
Pickup → đúng lề tại điểm đón → Boarding
Boarding hoàn tất → Delivery + 2 giây thực chạy nhường phía sau
Delivery → đúng lề tại điểm trả → Alighting
Alighting hoàn tất → cộng Coin một lần + 2 giây xuất phát → None → bộ việc tiếp theo
Thua giữa chuyến → dừng phục vụ, không cộng thưởng
ResetRide → hủy chuyến/khách chưa hoàn thành
```

`RideJobsReleased=false` là khóa phát hành; `jobsEnabled=false` là cờ scene; `JobsAvailable` kết hợp cả hai. Bản hiện tại **phải tắt**, không bật lại trước build. Khi được cho phép mở lại: JobOffersOpen chặn pause thường mở nhầm menu; ReadJobTouch chạy từ UpdateWork cả khi menu đang Paused. Chỉ gán jobsEnabled=true không vượt được khóa phát hành.

### 10.2. Sinh việc và tính thưởng

`ShowJobOffers` tạo ba đề xuất ở các phạm vi gần/vừa/xa quanh vị trí logic; cố có một điểm đón tương đối gần phía trước nếu phù hợp. Các điểm được chọn ở offset cửa hàng **14; 22,4; 30,8; 39,2; 47,6; 56; 64,4 m** của cạnh đường. Offset này có tính phần dịch giữa tâm giao lộ logic và vị trí shop `4 + 8.4*n` trong module nguồn.

Thử sinh lại để tránh hai điểm đón quá gần nhau và để hành trình chở không quá ngắn, có giới hạn số lần thử. Quãng đường tính bằng route thật trên lưới, gồm đoạn tới đón và đoạn chở khách, không chỉ khoảng cách thẳng giữa hai điểm.

```text
baseReward = 20 + 5 * floor(max(0, pickupDistance + tripDistance - 160) / 80)
```

Sau đó sắp các đề xuất theo tổng quãng đường tăng dần và đảm bảo mỗi mức thưởng sau lớn hơn mức trước ít nhất 5 Coin. Mức đã báo khi nhận là mức trả khi hoàn thành; đi vòng không tự làm tăng tiền của cùng chuyến.

### 10.3. Tên phố cố định và tìm đường

`CityNavigation` dùng các hướng `N, E, S, W`, node cách 80 m. Tên gắn vào trục `V:x` hoặc `H:y`, lưu dictionary trong lượt; `V:0` là Trần Hưng Đạo. Tên mới lấy từ `RouteNames`, hết danh sách thì dùng hậu tố nối dài.

Vị trí logic tài xế:

```text
NavigationPosition = navNext * 80 - Directions[navHeading] * NextJunctionDistance
```

`Plan` ưu tiên đi trực tiếp nếu đích đang ở phía trước trên cùng cạnh hiện tại. Nếu không, tìm đường kiểu Dijkstra trên trạng thái **(node, hướng đang đi)**, mỗi bước được đi thẳng/trái/phải, không quay đầu 180 độ tại chỗ. Tính cả phần cạnh còn lại và phần cạnh tới điểm đích, rồi truy ngược cha để tạo `CityRoute.points`, `distance`, `nextTurn`.

`PassCityJunction`, `TurnCityJunction`, `CityJunctionPrepared` đồng bộ khi đi thẳng hoặc kết thúc cua/rebase. Rẽ sai hướng hoặc bỏ qua lệnh rẽ gọi `ReplanJob` và tăng `Reroutes`. Lỡ chạy qua điểm đón/trả quá khoảng 4 m cũng tính lại vòng quay về. Địa chỉ khách không thay đổi.

Hình học phố chỉ render bằng pool; lưới logic có thể mở rộng theo chuyến. Không nhầm hệ GPS này với NavMesh của Unity, bản đồ thật, tọa độ địa lý hoặc API dịch vụ bên ngoài.

### 10.4. Dừng và hoạt ảnh khách

`WorkSpeedLimit` bắt đầu giảm tốc trong khoảng 22 m nếu đang đúng trục và đúng làn lề. `TickWorkMotion` chỉ bắt đầu phục vụ khi đúng phía lề, ở gần điểm hẹn theo chiều dọc và vị trí x thực đã gần tâm làn. Chỉ đổi biến `CurrentLane` mà xe chưa chạy tới lề thì chưa đủ.

Đón khách kéo dài khoảng **3,6 s**: 2,3 s đi từ mặt tiền ra cạnh xe, rồi 1,3 s chuyển tư thế/lên xe. Trả khách khoảng **3,2 s**: 0,9 s rời yên rồi 2,3 s đi về mặt tiền. Đây là đường đi/tween được script điều khiển, chưa phải AI đi bộ/NavMesh né mọi đồ vật cửa tiệm.

Khi ngồi, khách được parent vào `rider`, local position **`(0, 0.16, -0.9)`**, dùng clip `Sit`; model FBX con được xoay 180 độ lúc tích hợp để hướng mặt đúng chiều. Hai model có bộ xương riêng, chỉ một root khách được bật cho một chuyến.

`TryServiceApproach` dùng chung điều kiện với WorkSpeedLimit: đúng địa chỉ/đường/lề, khoảng tiếp cận 22 m. `ServiceRearProtected` bao phủ tiếp cận + Boarding/Alighting + 2 s thực sự chạy lại sau cả đón và trả; timer trừ ở cuối bước xe/va chạm, không hụt ngay frame kết thúc animation. `TrafficYieldActive` còn có cửa sổ nhận việc riêng 3 s.

Xe thường/nguy hiểm/ngang/đang rẽ/bị hất văng đều đổi lane trống hoặc phanh; mỗi bước kiểm lại lane. `ServiceTrafficFootprint` gộp hình bao yaw cũ/mới, cả thân xe dài. `YieldRearForService` chặn toàn đoạn quét chuyển động tương đối và giữ yaw an toàn khi phanh, không chỉ sửa Z cuối frame. Giữ khoảng hở theo đầu xe (khoảng 2,6 m tới tâm tài xế khi thẳng), kiểm lại sau va chạm dây chuyền. Rider pose trước frame phải cập nhật trước AI để không bị quét sai. `HoldCrossTrafficForService`/phanh khẩn cấp giữ xe cắt ngang trước điểm dừng. Không tắt collision hoặc cho xe trước tăng tốc theo W để né. Tất cả đều bị JobsAvailable khóa trong 1.30.6.

### 10.5. Lưu tiền và giọng chỉ đường

- Coin: khóa PlayerPrefs **`PhoViet.Coins.v1`**, mặc định 0.
- Kỷ lục: khóa **`PhoViet.BestMetres.v1`**.
- Chỉ nhánh hoàn tất `Alighting` mới cộng `ActiveJob.reward`, gọi `PlayerPrefs.Save`, xóa chuyến và khách; cộng có chặn tràn `int`.
- `FormatCoins` dùng `InvariantCulture` và `N0`, đảm bảo dấu phẩy đơn vị nghìn dù ngôn ngữ hệ điều hành khác.
- Lượt/chuyến đang làm không được lưu để khôi phục sau đóng game; số dư và kỷ lục mới là dữ liệu lưu cục bộ hiện tại.
- Đây không phải ví online/chống gian lận; xóa dữ liệu trình duyệt/đổi origin/profile có thể mất hoặc thay dữ liệu lưu.

Ba file `left.mp3`, `right.mp3`, `straight.mp3` trong `Assets/PhoViet/Audio/Navigation` đã tạo với giọng nữ `vi-VN-HoaiMyNeural` bằng edge-tts 7.3.1, được gắn vào scene. Runtime không cần mạng/TTS key. Xem `README.md` trong chính thư mục âm thanh để tái tạo; không commit `.tools` đã cài tạm.

`UpdateNavigationVoice` tính giây còn lại bằng khoảng cách/tốc độ hiện tại: stage đầu <=4 s, nhắc <=2 s, khóa node/hướng/lệnh/phase chống lặp và không xếp hàng lời cũ. `TurnSoonAfterStraight` bỏ Đi thẳng nếu sắp có rẽ trong 4 s; tới trực tiếp điểm hẹn quá gần cũng bỏ. `SpeakNavigation` dùng AudioSource riêng có mute/pause và kiểm tra JobsAvailable trước cả tạo source. `NavigationRoute`/`NavigationMapPosition`/`NavigationMapDirection`/`NavigationRemainingDistance` dùng đường preview ngay khi commit cú rẽ; thành phố logic chỉ tiến node một lần lúc FinishJunctionTurn, tránh chậm >1 s hoặc tiến node hai lần.

## 11. Model, rig, animation và quy trình Blender → Unity

### 11.1. Những bộ model và nguồn dựng

| Bộ | Model/source | Kỹ thuật |
| --- | --- | --- |
| Xe đua người chơi Blue Supercar21.64 | `Assets/PhoViet/Art/PlayerCar/PV_BlueSupercar.fbx`, nguồn `PhoViet_BlueSupercar.blend` | Meshy xanh giữ chữ/logo; asset2,071×1,076×4,4m; player21.65 scale0,85, 15xương, 3LOD28k/12,3k/4,5k, 7clip, PBR2K; 4socket lift +2exhaust |
| Xe người chơi Petshop — lưu trữ21.57, không còn dùng từ21.58 | `Assets/PhoViet/Art/PlayerCar/PV_PetshopPlayer.fbx`, nguồn `PhoViet_PetshopPlayer.blend` | Bán tải xanh Meshy 3,8 m, 15 xương, 3 LOD 14.690/6.462/2.349 tam giác, 7 clip Legacy, PBR 2K, giữ chữ Petshop; thay riêng player |
| Xe máy + người lái **Urban Rider (27/10/2026, scene/web1.41.0,21.67)** | `Art/UrbanRider/PV_UrbanRider.fbx`; 17traffic +14parked +56baked nhánh, giữ tên role cũ; CSGT/player không thay | Model ngồi/nắm tay lái nguyên bản, rig19xương, 3LOD42.000/18.480/6.720tri,7clip; 4màu xe/áo đang dùng, PBR2K chung và mask chọn vùng; cổ+tay+thân đồng bộ bằng runtime |
| Xe máy + người lái **Meshy (23/10/2026, lịch sử/web1.40.0)** | HeroScooter, CityScooter, Underbone, SportBike, Racer dùng cùng hình xe đã nhập từ Meshy và màu khác nhau; TrafficPolice vẫn mô tô tuần tra v2 | Scene/web1.32.0: người Golden Star Stance có rig, 17 xương semantic/7 clip/3 LOD, 42.939/11k/5k tam giác. Atlas2048×4096, mỗi vùng người/xe2K đầy đủ; surface/normal/emission chung; người CSGT không đổi. Xem21.9. Bản web1.31.0 dùng người cũ (21.7) |
| Xe máy + người lái v2 (19/10/2026, **đã thay thế trừ CSGT**) | Tay ga trắng/tay ga nữ/xe số/côn tay/quái xế/CSGT | `Captures/VERIFICATION_BIKES_V2.md`; nguồn `pho_viet_motorcycles.py` vẫn là thư viện người lái/rig/xuất cho bản Wave |
| Xe bốn bánh — scene/web1.33.0 (24/10/2026) | 4 mẫu Meshy: thể thao, bán tải, tải bạt, đầu kéo container; giữ 6 tên role Sedan/Hatchback/Taxi/DeliveryVan/BoxTruck/DrowsyTruck; **từ 21.33 thêm `PV_MeshyTank` (xe tăng) làm mẫu thứ hai ở 5 ô xe con** | 15 xương hoặc 23 ở container 4 trục; 7 clip, 3 LOD, một material/mẫu; màu sơn ngẫu nhiên mỗi lần spawn (xe tăng giữ một màu gốc), hai mẫu tải được chọn lại ở cả tải thường/ngủ gật (21.17) |
| Phố Sài Gòn —scene/web1.34.0 (24/10/2026) | 12 shop, 3 cây, 1 module vỉa hè, cùng tên FBX cũ | Mesh tĩnh 3 LOD dựng theo mức chi tiết (không decimate), palette 128 px lưới 8×8 = 64 màu; chữ/mái hiên giữ ở mọi LOD; JSON kèm hộp vật cản. LOD0 shop 9.025–26.848 (21.28; shop có nhà cao tầng 21.180–26.848), cây 29.026 (21.24), vỉa hè 1.258 tam giác (21.19); shop có kính có submesh thứ hai dùng `PV_StreetGlass.mat` |
| Khách | PassengerMale, PassengerFemale | 14 xương/mẫu, 4 clip, 3 LOD, một palette material/mẫu |

Xe máy có vỏ cong, lốp/mâm/phanh/giảm xóc/động cơ/pô, gương/đèn/biển số; người ngoài CSGT dùng khuôn mặt/tóc/trang phục Meshy mới, không mũ; hero có sao ngực và chính giữa lưng. Mô tô CSGT có kính/túi hông và nhận diện riêng. Xe lớn có hốc bánh, kính, gương/gạt mưa, lưới tản nhiệt, thùng hàng, gân/cửa/bản lề/khóa.

**Source21.67:** NPC xe máy ngoàiCSGT đã thay Urban Rider; mô tả người Golden Star Stance/HeroScooter ở trên là asset lịch sử/web1.40.0. Người chơi có `PlayerCarVisual` riêng Blue Supercar, không còn `BikeVisualAnimator` trên root player. Player vàCSGT giữ nguyên so với21.65; có31animatorUrbanRider (17giao thông+14đậu) và1CSGT. Số animator xe máy giảm1 so với mô tả33 ở21.58 (không tính các hình xe máy đậu bake tĩnh ở nhánh). Có11 `CarVisualAnimator` giao thông và1 `PlayerCarVisual` của người chơi; không dùng bộ mẫu NPC làm player.

Tam giác xe máy dân sự hiện hành21.67: **42.000/18.480/6.720**; metadata `Art/UrbanRider/PV_UrbanRider.json`. Ví dụ lịch sử/web1.40.0: HeroScooter với người rigged **42.939 / khoảng11.000 / khoảng5.000**; metadata `Art/Bikes/MeshyRiders/*.json`. Bản web1.31.0:30.423/10.999/4.999 trong thư mục Meshy cũ. Ô tô Meshy từ scene/web1.33.0: Sport **16.488/7.254/2.638**, Pickup **20.575/9.053/3.291**, Covered **13.481/5.931/2.156**, Container **7.582/3.335/1.213**; bộ cũ ở web1.32.2 DrowsyTruck **23.503/10.340/3.760**. Khách nam **4.688/2.338/1.091**, khách nữ **5.084/2.541/1.160** (đang khóa). Bản Wave cũ có 35.604/16.020/5.200 và không còn được scene dùng trừ rider/rig kế thừa.

### 11.2. Source chỉnh sửa và điểm vào tái tạo

| File trong `ArtSource/Blender/` | Script | Hàm tái tạo |
| --- | --- | --- |
| **`PhoViet_UrbanRider.blend`** (người+xe dân sự hiện hành21.67) | `integrate_urban_rider.py` + `prepare_urban_rider_maps.py` | Chạy script quaBlenderMCP trong scenePV_UrbanRider riêng; cache decimate ở.tools hoặc tự importnguồn; xuấtrig/LOD/clip/JSON, PythonPillow tạo masks; cài `InstallUrbanRider.Main`. Giữ các nguồn cũ/CSGT/player |
| **`PhoViet_MeshyRiders.blend`** (người lịch sử/web1.40.0) | `integrate_meshy_riders.py` + `prepare_meshy_rider_atlas.py` | Blender --phase prepare → Python atlas → Blender --phase build; dùng rig/weights nguồn mới, xuất MeshyRiders; giữ CSGT |
| **`PhoViet_MeshyMotorcycles.blend`** (nguồn xe1.31.0, 23/10) | **`prepare_meshy_bike_atlas.py` + `integrate_meshy_bikes.py`** | Tạo atlas bằng Python rồi ghép hình học FBX Meshy với người lái/rig cũ trong Blender, xuất 5 FBX mới vào `Art/Bikes/Meshy/`; không xuất CSGT, không ghi đè Wave/nguồn Meshy. File blend chứa cả 5 rig đã xuất |
| `PhoViet_Motorcycles_Wave.blend` (nguồn cũ giữ lại) | `pho_viet_wave.py` / `pho_viet_motorcycles.py` | Chỉ dùng khi cần bảo trì nguồn cũ; không chạy để thay thế lại bản Meshy hiện hành |
| `PhoViet_Motorcycles.blend` (v2, giữ nguyên, có sửa cục bộ chưa commit có từ trước) | `pho_viet_motorcycles.py` (`pho_viet_bikes.py` chỉ còn là helper cho script khác; `rebuild_all()` cũ đưa xe về v1) | `build_motorcycles()` dựng lại **cả 6 mẫu v2, sẽ ghi đè FBX Wave** — chỉ dùng khi muốn quay về v2 hoặc để xuất lại riêng CSGT bằng `build_motorcycles(['PV_TrafficPolice'])` |
| **`PhoViet_MeshyCars.blend`** (scene mới) | `integrate_meshy_cars.py` + `prepare_meshy_car_textures.py` | Blender nền xuất 4 rig/LOD/7 clip + profile nóc; Python tạo mask sơn/đèn, giữ maps nguồn; Install received Meshy cars |
| **`PhoViet_MeshyTank.blend`** (xe tăng, 21.33) | **`integrate_meshy_tank.py`** | Blender nền `blender -b --factory-startup -P ArtSource/Blender/integrate_meshy_tank.py`: tỉ lệ dài 7 m, rig 15 xương (2 trục bánh ảo không gắn mặt), 7 clip, 3 LOD, profile nóc; chép maps 2K, mặt nạ sơn/emission đen; rồi `AgentScripts/InstallMeshyTank.cs` |
| **`PhoViet_RiverBridge.blend`** (cầu vượt sông, 21.35) | **`pho_viet_river_bridge.py`** | Chạy trong Blender MCP: exec file rồi `build_river_bridge()`; exec `pho_viet_saigon_streets.py` để dùng lớp `Geo`/bảng màu `PV_StreetAtlas`. Xuất `Assets/PhoViet/Art/River/PV_RiverBridge.fbx` (3 LOD ~45k/15k/2,3k tam giác; vật liệu theo thứ tự Unity ghi trong JSON `slots`) và `PV_RiverBoat0/1.fbx`; ghi nguồn bằng `bpy.data.libraries.write` (không ghi đè file phố đang mở). Sau đó `AgentScripts/InstallRiverBridge.cs` |
| `PhoViet_Cars.blend` (nguồn cũ, có chỉnh cục bộ cần giữ) | `pho_viet_cars.py` | `build_all()` chỉ tái tạo bộ cũ; không dùng để thay bộ Meshy mới |
| **`PhoViet_SaigonStreets.blend`** (phố hiện hành, 24/10) | **`pho_viet_saigon_streets.py`** | `build_saigon_streets()` trong Blender MCP: scene riêng `PV_SaigonStreets`, xuất đè 16 FBX/JSON + palette + catalogue rồi lưu blend mới; sau đó chạy `AgentScripts/InstallSaigonStreets.cs` |
| **`PhoViet_PetshopSigns.blend`** (source21.72–21.74) | **`pho_viet_petshop_signs.py`** + `pho_viet_river_bridge.py` + `pho_viet_petshop_layout.py` | Chạy **Blender background riêng**; xuất12shopgốc+12variantPetshop và riverbase+1part bổ sung; không ghi palette/bridge/boats hoặc fileBlenderđangmở; pet geometry từlibrary đã dựng bằng BlenderMCP (`pho_viet_pet_animals.py` → `PhoViet_PetAnimals.blend`, `Art/Pets/PV_PetGeometry.json`). Sau đó `AgentScripts/InstallPetshopSigns.cs` → `InstallPetshopSigns.Main`; không rebuild road rows |
| `PhoViet_Streets.blend` (nguồn cũ, có sửa cục bộ cần giữ) | `pho_viet_streets.py` | `build_streets()` — **sẽ ghi đè 16 FBX và palette 4 hàng của bộ Sài Gòn**; chỉ dùng khi muốn quay lại phố cũ |
| `PhoViet_Passengers.blend` | `pho_viet_passengers.py` | `build_passengers()` |
| `PhoViet_Jump.blend` | `pho_viet_jump.py` | Chạy script trên rig HeroScooter; xuất action + Resources/PhoVietJump.json |
| `PhoViet_BoostSmoke.blend` | `pho_viet_boost_smoke.py` | Chạy script; volume Cycles → atlas RGBA 4 frame |
| `PhoViet_WreckFire.blend` | `pho_viet_wreck_simulation.py` | `bake_simulation_atlases()` trong namespace riêng |
| `PhoViet_DayNight.blend` | `pho_viet_day_night.py` | `build()` — **lưu ý:** cũng xuất `PV_LEDStreetLamp.fbx`; chạy lại sẽ đưa đèn đường thẳng cũ về và lệch tâm quang học (2,35; 8,48). Đèn hiện hành do `pho_viet_saigon_streets.py::build_saigon_fixtures()` xuất (21.20) |
| `PhoViet_MorningFog.blend` | `pho_viet_morning_fog.py` | `build()` |

Các script gọi helper mesh/atlas/rig/FBX từ `pho_viet_bikes.py`. Biến `ROOT` hiện gắn đường dẫn máy Windows ở đầu script; chuyển máy phải cập nhật đường dẫn phù hợp **ở các script liên quan**, kiểm tra đường xuất trước khi chạy. Đơn vị tác giả là mét, có helper đổi trục Blender/Unity; không sửa scale/axis tùy ý để chữa một model lệch mà chưa kiểm tra cả import, clip và xương.

Blender MCP đã được dùng để chạy Python trong Blender thật, tạo hình học, rig, action, LOD, material và xuất FBX; không phải chỉ tạo ảnh minh họa. Phiên làm trước dùng Blender 5.2 LTS; khi chuyển môi trường kiểm tra lại version/add-on. Không tự xóa toàn scene của người dùng: chỉ thay đối tượng/collection do các script sở hữu.

Ví dụ nội dung gửi vào công cụ execute Python của Blender MCP, sau khi đã xác nhận đúng file/mô hình đang mở:

```python
source = r"C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Blender/pho_viet_passengers.py"
exec(compile(open(source, encoding="utf-8").read(), source, "exec"), globals())
build_passengers()
```

Lệnh rebuild có ghi đè tài nguyên do script quản lý. Lưu thay đổi thủ công cần giữ trước khi chạy. Sửa `.blend` nhưng chưa export FBX thì Unity chưa nhận hình mới; sửa script Python nhưng chưa chạy cũng chưa làm đổi model đang dùng.

### 11.3. Clip và phân chia visual/gameplay

| Bộ | Tên clip dùng trong Unity |
| --- | --- |
| Xe máy | `Idle`, `Ride`, `KickLeft`, `KickRight`, `FallLeft`, `FallRight`, `Recover` |
| Xe lớn | `Idle`, `Drive`, `Brake`, `ImpactFront`, `ImpactRear`, `ImpactLeft`, `ImpactRight` |
| Khách | `Idle`, `Walk`, `Board`, `Sit` |
| Nhảy người chơi | `JumpTakeoff`, `JumpAir`, `LandBoth`, `LandFront`, `LandRear`; `JumpDemo` dùng xem thử Blender |

Các importer dùng **Legacy Animation**, không phải Animator Controller/Mecanim. Với xe Meshy từ 1.32.2, phần mâm/hub quay còn đường bao lốp và các mặt chắn bùn dính trong mẫu AI giữ rigid theo Frame/Steering, tránh mảnh vỏ quay sai (21.16). Controller script chọn clip, đặt normalized time, speed = 0 rồi `Animation.Sample` để khớp trạng thái gameplay. `BikeVisualAnimator`/`CarVisualAnimator` cập nhật visual trong `LateUpdate`; bánh được quay theo vận tốc thật, bánh trước xe lớn đánh lái theo chuyển hướng. Khi pause, pose/bánh được giữ.

Root vật lý/pool là đối tượng ổn định. FBX/skin nằm ở model con. Animator chỉ thay bone/visual; không tự điều khiển root hay thay kích thước va chạm theo mesh mới. Hướng va chạm từ physics được chuyển sang hệ local của visual để chọn clip cản/gương đúng phía. Cú ngã/tiếp đất có hiệu chỉnh visual để không xuyên mặt đường; luật thua vẫn thuộc gameplay.

Importer/upgrader giữ semantic objects như biển nguy hiểm, đèn ưu tiên và các tham chiếu cần thiết. Cần giữ tên xương mà code đang tìm, ví dụ `WheelFront`, `WheelRear`, các xương chân xe máy; xe lớn có `Body`, `BumperFront`, `BumperRear`, `WheelFL/FR/RL/RR`, `SteerFL/FR/RL/RR`. Đổi tên xương/clip phải sửa và kiểm tra importer/controller/tests cùng lúc.

### 11.4. Phố, mái hiên, biển hiệu và cây

**Source21.72 (chưa web):** bộ16FBX gốc vẫn giữ; thêm12variantPetshop3LOD và2bộmesh căn hộ (base+1part biến thể), xem21.72. Biển Petshop nền xanh/chữ trắng chiếm đúng30%cửa hàng; nhàven sông có tên ở cả mặtđường/mặtsông và cả3LOD. **Source21.73:** thêm phụđề, thú cưng và nội thất Petshop riêng, cập nhật obstacle tương ứng (xem21.73). Sau khi tạo lại cảnh bằng công cụ cũ phải chạy `InstallPetshopSigns.Main`; không chạy lại installer lịch sử chỉ để sửa bảng hiệu.

`PhoVietStreetUpgrade.ConfigureFactories` nối `ShopFactory`, `TreeFactory`, `PavementFactory` của `VietnamStreet` sang FBX Blender. Khi các factory này hoạt động, hình học shop/cây/vỉa hè fallback cũ không phải bộ cảnh quan đang render.

**Bộ hiện hành là phố Sài Gòn 24/10/2026** (`pho_viet_saigon_streets.py`, chi tiết 21.19). Bộ 1.12–1.13 (`pho_viet_streets.py`) là lịch sử.

- Shop đặt theo mặt tiền quay vào đường; chữ tiếng Việt (Be Vietnam Pro, làm đậm bằng offset, chữ nghiêng “Sài Gòn”) dựng vào hình học biển 7,36 × 1,36 m ở y 3,4–4,8 m. Biển dọc vuông góc chỉ nằm trong bề rộng tiệm, không lấn nhà bên.
- Nhà rộng 8,36 m theo trục phố (khe 4 cm giữa hai tiệm), sâu 7,8 m, tường trước z = −3,9 → mặt tiền |x| = 9,3 m như cũ. Tầng trệt 4,9 m, mỗi tầng trên 3 m; 2–4 tầng trên.
- **Mặt tiền tầng trệt (21.22):** vách kính trên mặt tường z = −3,9 (vật liệu trong suốt `PV_StreetGlass`), khung đứng/ngang, chân vách, dầm trên 3,16 m, dầm ngang 2,62 m; cửa hai cánh rộng 1,32 m tại x = 0,65 (tạp hóa 0,45, thuốc/bánh 0,4) có tay nắm inox và chữ MỞ CỬA; đèn tường hai bên cột. Lòng tiệm sâu 3,4 m (tường trong z = −0,5), sàn gạch caro, đèn thả, nội thất theo loại tiệm.
- Mái hiên 14 dải vải có độ dày, dốc từ 3,22 m xuống 2,8 m, nhô tới z = −5,4, có riềm lượn; luôn giữ ở cả 3 LOD.
- **Chậu cây được đưa lại theo yêu cầu 24/10** (ảnh mẫu): chậu lớn vỉa hè, chậu ban công, bồn hoa cửa sổ, hoa giấy rủ. Hoa ở tầng 1 rủ ngắn để không che chữ biển.
- **Ba cây theo ảnh mẫu `ArtSource/Reference/Street/tree_reference.webp` (21.24):** thân xám mảnh thẳng hơi nghiêng cao ~3,7–4 m, 5 cành chính xám tỏa lên từ đỉnh thân, 40 cụm lá ở đầu cành con trên vỏ tán gồ ghề (rx 1,68, ry 1,6, rz 1,8), có khe hở giữa cụm; lá bầu dục hai mặt (rộng 0,5 chiều dài, chỗ rộng nhất ở 48%), xanh tươi ngả vàng sáng ở ngọn/ngoài, đậm ở trong/dưới. Lá **3.520 / 1.500 / 660** ở ba LOD, không dùng khối cầu. Thay cho tán bầu dục 21.19–21.21.
- Vỉa hè gạch caro 0,6 m kem `EBD7AE`–đất nung `B97B58`, bó vỉa vàng/xám xen kẽ, nắp cống. Palette `PV_StreetPalette.png` 128 px nay là **lưới 8×8 ô 16 px (64 màu)**, UV = ((ô%8+.5)/8, (ô//8+.5)/8); vẫn Point/không mip. Cài lại ánh sáng chỉ nạp lại PNG này, không đổi màu.
- Hộp vật cản shop/cây đọc từ JSON cạnh FBX (`obstacles`), không còn hardcode trong `PhoVietObstacleUpgrade`.
- Các bản sao chia sẻ mesh/material; tổng scene có 35 module và 1.260 nhóm LOD cảnh quan theo bộ kiểm tra hiện tại. Điều này không thay cho đo FPS trên điện thoại thật.

World text cho bảng tên đường dùng `WorldStreetText.mat` và shader **`PhoViet/World Street Text`**, có depth test/fog để không vẽ xuyên nhà. HUD vẫn dùng font Be Vietnam Pro; sao HUD được vẽ bằng texture 5 cánh tự tạo để không phụ thuộc ký tự font trên WebGL. Khói boost dùng texture volume bake, lửa/khói wreck dùng atlas mô phỏng; có shader riêng WreckFlame/SoftParticle, xem mục 20. Waypoint có Solid shader vát khối; DayNightSky/OpticalLight/MorningFog phụ trách trời, vùng đèn và sương.

### 11.5. Tích hợp lại art đúng cách

Thoát Play Mode trước khi lưu nâng cấp scene. Các menu thường dùng:

- `Phố Việt/Art/Configure Blender bike imports`.
- **Hiện hành21.67: chạy `AgentScripts/InstallUrbanRider.cs` → `InstallUrbanRider.Main` trong Edit Mode.** Chỉ thay31xe máy ngoàiCSGT, tạo3mesh bake riêngUrbanRider cho56bản sao nhánh, giữ playerSupercar/CSGT/pool/tênrole, nối lại17bộ đèn, lưu scene. Không chạy lại installer cũ ở dưới cho cảnh hiện tại.
- **Lịch sử/web1.40.0:** menu `Phố Việt/Art/Install received Meshy motorcycles (keep police)`, hoặc `AgentScripts/UpgradeBikesMeshy.cs` → `UpgradeBikesMeshy.Main`. Trình cài mới thay đúng 32 actor ngoài CSGT, bake lại 3 mesh xe đậu nhánh giữ GUID và đổi vật liệu trên 56 bản sao, dựng lại neo đèn/nozzle, làm mới emission users rồi lưu scene. Menu chung `Replace all motorcycles with Blender models` và `UpgradeBikesWave` không phải quy trình cài hiện hành: chúng có thể thay cả CSGT hoặc làm mất neo đèn. `ModelPath` ưu tiên `Bikes/MeshyRiders/`, sau đó `Bikes/Meshy/`, cuối cùng asset cũ nếu chưa có. Menu Install received Meshy riders (keep police) cũng dùng chung trình cài.
- `Phố Việt/Art/Replace all four-wheel vehicles with Blender models`.
- `Phố Việt/Routes/Install connected junction roads`.
- `Phố Việt/Art/Install Blender streets` — **dựng lại toàn bộ 5 road rows**, có thể làm mất các lớp cài sau (đèn LED, vật cản…); không dùng cho thay art phố. Thay art phố hiện hành bằng `AgentScripts/InstallSaigonStreets.cs` → `InstallSaigonStreets.Main`: reimport 16 FBX cùng GUID, đổi tên shop theo `VietnamStreet.Shops`, tính lại bounds 1.260 LODGroup, chạy lại `PhoVietObstacleUpgrade.Install` rồi lưu scene.
- Khách: chạy `AgentScripts/UpgradePassengers.cs` → `UpgradePassengers.Main` hoặc gọi `PhoVietPassengerUpgrade.UpgradeOpenScene()` trong editor script.

Không chạy toàn bộ các menu trên theo thói quen cho một thay đổi nhỏ. Có menu tái tạo cả dãy đường/model con; đọc phạm vi trước, giữ GUID `.meta`, xem diff scene và kiểm tra references. `PhoVietBuilder.Combine` đã có ngoại lệ cho skeleton, LOD, đèn/mũi tên và phần chuyển động; không gộp toàn bộ skinned mesh thành mesh tĩnh chỉ để giảm draw call.

Unity có thể chưa cập nhật GPU skin trong đúng frame vừa `Animation.Sample`. Khi chụp pose, chuẩn bị pose rồi chờ một frame editor thật trước khi render/chụp; hoặc dùng `SkinnedMeshRenderer.BakeMesh` để xác nhận biến dạng. Đã từng cần cách này để tránh ảnh đứng sai pose dù bone/clip đã đúng.

## 12. GUI, camera, cảm ứng và fullscreen

`OnGUI` dùng `GameGuiArea` và GUI.matrix: menu ngang theo thiết kế 1440×900, dọc rộng 480. HUD ngang có scale riêng `min(area.width/1280, area.height/420)`. Hit-test phải dùng đúng ma trận HUD, đổi touch dưới-trái sang GUI trên-trái, trừ safe area và chia scale; không dùng ma trận menu cho nút HUD.

- **Hiện tại jobs tắt:** `PhoVietDistanceHud` vẽ dọc cao 76 ở y=10, thông báo y=92 cao 28; ngang rộng 640 cao 76 y=18, thông báo y=100 cao 26. Có đường/mét/tốc độ/kỷ lục/pause, ngang thêm âm thanh. Không Coin, mạng, GPS hoặc ô Chưa có chuyến. Nền thanh alpha 0,64; thông báo 0,70, chữ vàng. Ellipsis đo theo font để tên dài không chồng.
- Bố cục được giữ cho khi jobs bật: dọc 96 ở y=10, map 80×80, notice y=112; ngang 1000×76 chứa tất cả, map 60×60. Không còn hai thẻ HUD/GPS riêng cỡ lớn. Dọc bỏ cả ba ô điều khiển dưới cùng. Bản đồ vẫn dùng GUI.BeginGroup để clip các nét và vẽ các tuyến ngang/dọc; không xoay GUI.matrix làm map tràn ô.
- `HudButtonAt`, `ApplyHudTap` và `GameplayPointerAllowed` dùng kích thước thanh theo JobsAvailable; thu hẹp HUD cũng thu hẹp vùng chặn cử chỉ. Marker job không đổi vị trí thông báo xuống giữa cảnh trong layout mới.
- Notice chỉ một dải xanh đậm bo góc/chữ vàng; bỏ các dải route, ngược chiều, reckless, phanh, truy đuổi xanh/đỏ riêng. Mũi tên báo xe sau vẫn có chức năng riêng tại mép dưới.
- Web HUD dùng pointer bridge ApplyHudTap; bỏ IMGUI compatibility-mouse xử lý lần hai. Native giữ mouse/GetTouch. Pause/âm thanh nhận đúng một lần, cả tap ngắn giữa hai frame.
- `PhoVietWelcome` dùng thẻ trắng trong alpha 0,93, viền/bóng nhẹ, tiêu đề xanh đậm và đỏ; kích thước dọc 432×258, ngang 360×276 trong tọa độ GUI. WelcomeStartRect dùng chung vẽ/hit-test; không còn thẻ vàng lớn cũ. Từ21.78, icon đường góc phải được thay bằng nút bánh răng mở cài đặt đồ họa Cao/Trung bình/Thấp; mặc định Cao và lưu lựa chọn. Chi tiết21.78–21.79.

**Cử chỉ 1.27.1:** app.js bắt pointerdown/move/up/cancel trực tiếp, pointer capture; khoảng vuốt theo cạnh ngắn CSS canvas, không theo chiều rộng hay DPR. Ngang >0,075 cạnh ngắn; dọc >0,12 và rõ dọc (>1,3 lần ngang). Nhận ngay khi move đủ ngưỡng, một pointer một lệnh; WebGL tắt polling Input touch/mouse trùng. `ApplyBrowserGesture` nhận phase/id/action/x/y/at, kiểm finite/bounds và timestamp gốc. Begin từ HUD/paused không thể hóa gameplay sau đổi State. Cancel khi blur/visibility/rotation/restart/pause/mất capture hoặc canvas resize. Double tap dùng timestamp gốc nên hai tap chung frame vẫn phân biệt; cửa sổ thực xem RegisterTap, không nới để chiều software-renderer.

**Camera dọc:** từ 24/10/2026 sau 1.34.0 (21.28, scene/source, **chưa build web**) `PortraitWorldScale=.75*.70*1.10=.5775`, công thức `2*atan(tan(65°/2)/.5775)` ≈95,62° — zoom gần hơn 10% so với 1.27.2–1.34.0 (.525, ≈101,017°). Lịch sử: giảm kích thước chiếu 25% ở 1.19, thêm 30% ở 1.27.2; game vẫn phủ kín màn hình, không scale nhỏ canvas. Cả bốn làn được giữ trong nhìn; camera bám độ cao khi nhảy, góc/camera cua theo tốc độ và delta thực. Camera jail riêng.

**Icon vector:** nguồn `ArtSource/Web/pho-viet-app-icon.svg`, không ảnh nhúng/font ngoài. `AgentScripts/GenerateWebIcons.cjs` + sharp xuất PNG 192/512, maskable 512, Apple opaque 180; icon-moto-* URLs dùng trong manifest/favicon/Apple, tên legacy cũng giữ cùng artwork. Mẫu PV monogram không phát hành; icon minh họa raster 1.18 ban đầu đã thay. iOS có thể giữ icon cũ của mục Home Screen đã cài, không đồng nghĩa gameplay còn cũ.

### 12.1. Phân chia trách nhiệm web/Unity

| Lớp | Xử lý |
| --- | --- |
| `Assets/WebGLTemplates/PhoVietPWA/index.html` | Canvas, loader config, viewport/meta, UI web, hướng dẫn |
| `style.css` | Khung nhìn động, immersive canvas, toolbar/menu nổi, safe-area probe, bố cục trang |
| `app.js` | Khởi tạo Unity, fullscreen/install, resize/orientation/visibility, gửi insets/pause, trạng thái offline |
| `PhoVietWebRuntime.cs` | Nhận insets chuẩn hóa, tính vùng HUD, pause từ web, ký tự GUI, chất lượng đồ họa mobile |

`app.js` dùng `visualViewport`, `innerWidth/innerHeight`, CSS env safe-area và các lần sync sau xoay máy. Chế độ immersive đặt vùng game phủ viewport; chỉ HUD/nút nhận inset. iOS standalone có xử lý riêng khi `visualViewport.height` trừ safe area. Không ép canvas theo kích thước màn hình vật lý vì iPad split view/cửa sổ có thể nhỏ hơn màn hình.

DPR bản sửa1.32.1: theo mật độ thật tối đa **3 trên mobile/touch**, giới hạn **3,2 triệu pixel**; desktop tối đa2/8,3triệu pixel. Tính lại sau xoay/fullscreen theo diện tích canvas, không dùng scale CSS để làm nét giả. Bản1.32.0 trước đây chặn1,5 trên pointer coarse. Inset gửi theo tỷ lệ viewport, tránh sai khi canvas render ở DPR khác CSS. Trong WebGL player tắt HDR; mobile giảm shadow distance còn 45, bóng thấp, không cascade, MSAA4 để giảm răng cưa (bản1.32.0 dùng2).

Cầu nối JavaScript đang gọi tên root chính qua `SendMessage`:

```javascript
window.phoVietInstance.SendMessage(
  'PHỐ VIỆT • Chuyến xe không ngừng',
  'PauseForBrowser'
);
```

`ApplyBrowserInsets` nhận JSON left/top/right/bottom chuẩn hóa và có kiểm tra giá trị. Các hàm bridge có `[Preserve]` khi cần. Đừng bỏ bridge vì trong Editor chưa thấy tác dụng; lỗi safe area/fullscreen thường chỉ lộ trên web.

Fullscreen native cần thao tác người dùng và tùy hỗ trợ trình duyệt. iOS Safari quản lý thanh trình duyệt/status bar; “cảnh phủ viewport” không đồng nghĩa code có thể ép mọi phiên bản iOS ẩn toàn bộ thanh hệ điều hành. Người dùng đã cho biết các ảnh lỗi lề cũ chụp từ biểu tượng Home Screen trên iPhone 13 Pro Max; **chưa có phiên bản iOS chính xác** trong thông tin bàn giao.

## 13. Kết nối Unity CLI/MCP và Blender MCP

### 13.1. Unity

Package dự án: `com.unity.pipeline` **0.8.0-exp.1** trong `Packages/manifest.json`. Unity CLI trên máy lúc lập tài liệu: **1.0.0-beta.13**. Lần kiểm tra read-only cho tài liệu thấy một editor `ready`, project đúng thư mục, cổng **7800**; cổng/PID là thông tin phiên, phải kiểm tra lại khi mở máy khác.

```powershell
unity --help
unity status --json
unity list --json
unity recompile --timeout 60 --json
unity command editor_play --json
unity command editor_stop --json
```

MCP stdio server có thể chạy bằng:

```powershell
unity mcp --project-path 'C:/Users/HP/Documents/GameUnity/Taixecongnghe'
```

Để biết cú pháp cấu hình agent hiện được CLI hỗ trợ:

```powershell
unity mcp --help
unity mcp configure --list
```

CLI phân biệt **`claude` = Claude Desktop** và **`claude-code` = Claude Code CLI**. Không cấu hình nhầm client chỉ vì cả hai có chữ Claude. Lệnh cấu hình là thao tác thay đổi môi trường người dùng; tài liệu này chỉ mô tả, không tự cài/cấu hình Claude Code. Nếu Claude Code chưa có MCP thì vẫn có thể gọi Unity CLI từ shell sau khi được cài và kết nối đúng project. Không giả định cấu hình MCP của Codex sẽ tự chuyển sang Claude Code.

Kiểm tra nhanh không thay đổi scene:

```powershell
unity command eval 'var g=UnityEngine.Object.FindAnyObjectByType<PhoViet.PhoVietGame>(); return new {state=g.State.ToString(),g.Coins,g.BestDistance,g.jobsEnabled,g.JobsAvailable};' --json
```

Chạy editor script có entry point static:

```powershell
unity command run_script --file AgentScripts/VerifyDistanceMode.cs --entry VerifyDistanceMode.Main --timeout_ms 120000 --timeout 130 --json
```

JSON có nhiều tầng: thành công của lệnh transport không có nghĩa script kiểm tra đã đạt. Với `run_script`, đọc **`data.result.success`**, `data.result.errorDetails` và kết quả thực bên trong **`data.result.result`**. Nếu terminal trả session đang chạy, chờ xong rồi mới chạy lệnh Unity phụ thuộc tiếp theo.

`eval` phù hợp truy vấn ngắn; những việc import, SaveAssets, nâng cấp scene hoặc kiểm tra dài dùng `run_script` với timeout rõ ràng. Không chạy nhiều mutation Unity đồng thời trên cùng editor. Không mở thêm editor batchmode vào cùng project đang bị editor khác khóa.

Build 1.30.6 có 0 lỗi/3 cảnh báo đã biết: Pipeline thiếu RuntimePipelineConfig (công cụ Editor), dragging và hudTouchChoice không được đọc trong nhánh WebGL dùng DOM bridge. Không coi mọi warning mới là cùng loại. CLI/MCP là công cụ phát triển, không cần runtime plugin hoạt động trong game web.

### 13.2. Blender MCP

Claude Code cần có kết nối Blender MCP riêng tới add-on đang bật trong Blender, hoặc được người dùng thiết lập; repo không tự cấp kết nối MCP cho agent khác. Kiểm tra trạng thái/scene trước khi chạy Python; xem lại model bằng viewport/render sau khi dựng.

Ở phiên dựng trước, một số yêu cầu handshake metadata add-on có thể không tương thích phiên bản tool dù thực thi Python vẫn hoạt động. Xác nhận bằng thao tác đọc scene thực tế; không tự nâng cấp/cài lại add-on hoặc xóa scene để “chữa kết nối”. Cần hỏi người dùng nếu phương án thay đổi môi trường chưa chắc chắn.

Python chạy trong Blender, không phải Python ngoài shell có sẵn `bpy`. File export Unity là FBX đã lưu trong Assets; không phụ thuộc việc Unity phải mở `.blend` bằng Blender khi import. Giữ nguồn `.blend` ở ArtSource để tránh phụ thuộc Blender auto-import trên máy người nhận.

### 13.3. Cầu nối Meshy → Unity (Meshy Bridge, 23/10/2026)

Mục đích: bấm **Cầu DCC → Gửi đến Unity** trên meshy.ai để mô hình (ví dụ xe máy dựng bằng Meshy) tự tải vào editor đang mở.

**Đã cài:**
- **Plugin chính thức** `meshy-for-unity-0.2.4.unitypackage` (28.279 byte), lấy từ link trên trang `https://www.meshy.ai/integrations/unity` (`https://cdn.meshy.ai/meshy-for-unity/meshy-for-unity-0.2.4.unitypackage`).
  - Chỉ chép thư mục `Assets/ai.meshy/` cùng `.meta` gốc để giữ GUID. Không chép `Assets/TutorialInfo/` (script readme mẫu của template Unity đi kèm gói).
  - Assembly chỉ dùng trong Editor: `MeshyAssembly`. Không vào build WebGL.
- **Bản vá phạm vi lắng nghe** ở `Assets/ai.meshy/Editor/Script/MeshyBridgeWindow.cs`: `TcpListener(IPAddress.Loopback, 5326)` thay cho `IPAddress.Any` (hai bản vá Origin/preflight bổ sung được ghi bên dưới).
  - Lý do: bản gốc nghe cả mạng LAN, và mọi request `POST /import` đều được tải về và import, vì kiểm tra Origin chỉ ảnh hưởng header CORS.
  - Trình duyệt trên cùng máy vẫn gửi được. Người dùng đã đồng ý bản vá này.
  - Khi nâng cấp plugin phải vá lại.
- **Hai bản vá thêm ngày 23/10**, sau khi Meshy vẫn báo lỗi gửi mà nhật ký Bridge không ghi request nào từ trình duyệt:
  - (a) thêm origin `https://meshy.ai` (không có www) vào `allowedOrigins`;
  - (b) preflight `OPTIONS` trả thêm `Access-Control-Allow-Private-Network: true`, header Chrome đòi khi trang web công khai gọi tới localhost.
  - Đã kiểm tra bằng curl giả lập preflight của Chrome: 200, đủ header.
- **Gói `com.unity.cloud.gltfast` 6.20.0** trong `Packages/manifest.json`, để nhập được file `.glb` mà Meshy hay gửi. Người dùng đã đồng ý cài.

**Cách dùng:**
1. Trong Unity chọn menu **Meshy → Bridge**. Cửa sổ mở là server tự chạy; nút hiện **Bridge ON**, còn nếu hiện **Run Bridge** thì bấm vào.
   - Đóng cửa sổ là server dừng. Sau domain reload (biên dịch lại, vào Play Mode), cửa sổ còn mở sẽ tự chạy lại server.
2. Kiểm tra nhanh: `curl -H "Origin: https://www.meshy.ai" http://127.0.0.1:5326/status` phải trả `{"status":"ok","dcc":"unity",...}`. Đã kiểm tra đạt ngày 23/10 trên Unity 6000.5.6f1.
3. Trên meshy.ai:
   - Tài khoản cần gói **Pro trở lên**.
   - Tài liệu Meshy hiện hành **không còn** công tắc "Unity DCC Bridge" trong Settings/Preferences (đó là hướng dẫn cũ). Chỉ cần mở Bridge trong Unity.
   - Chọn mô hình → **Cầu DCC → Gửi đến Unity**. Nếu Chrome hỏi quyền truy cập mạng cục bộ/thiết bị trong mạng, chọn Cho phép.
   - Lỗi "Không thể gửi mô hình đến unity…" có thể do một trong các nguyên nhân sau:
     - cửa sổ Bridge chưa mở;
     - cổng 5326 bị ứng dụng khác chiếm;
     - tài khoản Meshy không phải gói Pro;
     - tiện ích chặn quảng cáo hoặc bảo mật chặn request tới `127.0.0.1`;
     - Chrome chặn quyền **truy cập mạng cục bộ** của meshy.ai. Chrome 142 trở lên bắt buộc xin quyền này cho cả loopback. Bấm biểu tượng bên trái thanh địa chỉ → bật "Truy cập mạng cục bộ", hoặc vào "Cài đặt trang web" → dòng "mạng cục bộ" → Cho phép, rồi tải lại trang.
   - Chẩn đoán đúng thứ tự: kiểm tra Bridge ON và cổng 5326 trước, rồi GET /status, sau đó mới xem Console có request từ trình duyệt. Không có dòng Received request **chưa đủ để kết luận trình duyệt chặn**: server có thể chưa chạy vì cửa sổ Bridge chưa mở trong dự án hiện tại. Bridge/layout của Diablo25D không tự bật Bridge cho Phố Việt.
4. Mô hình được lưu vào `Assets/MeshyImports/<tên>_<thời gian>…`. Plugin **tự đặt một bản vào scene đang mở** ở gốc tọa độ, đặt chạm Y=0 nếu bật "Stand on Ground", và đánh dấu scene bẩn.

**Lưu ý với Phố Việt:**
- Đừng lưu `PhoViet.unity` với bản thả tự động này.
- Muốn dùng mô hình Meshy cho xe trong game, phải tích hợp theo hợp đồng xe:
  - 17 xương, 7 clip, 3 LOD;
  - neo đèn pha/đèn hậu (mục 20.3) và vị trí khói boost (mục 19.3);
  - chạy lại các bộ kiểm tra xe.
- Mô hình Meshy thô là mesh tĩnh có texture riêng, chưa có rig của game.

#### 13.3.1. Chẩn đoán lại sau đổi dự án — 23/10/2026 (Codex)

Người dùng xác nhận gửi mô hình từ cùng Meshy vào Diablo25D được nhưng Phố Việt báo không kết nối Unity. Kiểm tra thực tế trên editor Phố Việt cho thấy:

- Đúng scene `Assets/PhoViet/Scenes/PhoViet.unity`, Edit Mode, không compiling; scene không bẩn trong bộ nhớ.
- Assembly `MeshyAssembly` và type `MeshyBridgeWindow` **đã được nạp**, không thiếu plugin/không phải lỗi compile hiện tại.
- Trước xử lý: `IsRunning=false`, không có instance cửa sổ Bridge, không có listener trên TCP 5326. `OnEnable` mới gọi StartServer; `OnDisable` dừng server. Plugin được cài không đồng nghĩa server tự chạy mỗi lần mở bất kỳ dự án.
- Cách xử lý: thực thi menu `Meshy/Bridge` ngay trong editor Phố Việt. Sau đó `IsRunning=true`, một cửa sổ Bridge mở; `127.0.0.1:5326` thuộc đúng tiến trình Unity của dự án. Không cần vào Play Mode.
- Bốn kiểm tra HTTP đạt: GET `/status` và OPTIONS `/import` cho cả `https://www.meshy.ai` và `https://meshy.ai`; đều HTTP 200, status `ok`/dcc `unity`, CORS trả đúng origin và preflight có `Access-Control-Allow-Private-Network: true`.
- Giữ cửa sổ Bridge mở, trạng thái **Bridge ON**, rồi gửi lại từ Meshy. Đóng cửa sổ sẽ dừng; nếu đổi dự án phải kiểm tra lại menu/cửa sổ của dự án vừa mở. Chỉ một Bridge được chiếm cổng 5326 trên máy; khi dùng editor khác thì dừng Bridge cũ trước.

Bằng chứng: `Captures/meshy-bridge-20261023-diagnostic.json`. Đây là kiểm tra kết nối/status/preflight, **không phải tuyên bố đã gửi hoặc import mô hình Meshy của người dùng**. Không sửa mã plugin/Packages/gameplay, không import/thả model, không lưu scene hoặc build/push/phát hành trong lần xử lý này. Giữ nguyên toàn bộ thay đổi Wave/model/scene/Packages và phần tài liệu Claude Code đang làm. Web công khai vẫn bản 1.30.6.

## 14. Kiểm tra và bằng chứng đã có

Các `AgentScripts/Verify*.cs` là script kiểm tra điều khiển trực tiếp scene, nhiều script dùng reflection và mô phỏng các tick. Chúng **không phải toàn bộ đều là NUnit Test Runner**; chỉ chạy `unity test` sẽ không tự thay thế hết hệ kiểm tra này.

**Bằng chứng khóa công việc1.30.6 (lịch sử):** 8 suite/946 điều kiện Unity đạt, 26 local WebGL và 26 live WebGL đạt; xem `Captures/VERIFICATION_DISTANCE_MODE.md` và `distance-*-results.json`. Đã xem ba ảnh HUD native dọc/mobile ngang/desktop; Coin 105 và record 1.775 m được khôi phục trong phiên kiểm tra đó (không phải số dư mặc định người chơi).

| Suite 1.30.6 | Điều kiện |
| --- | ---: |
| VerifyDistanceMode | 31 |
| VerifyQuickKick | 62 |
| VerifyJunctionKick | 216 |
| VerifySmoothTurns | 468 |
| VerifyJump | 44 |
| VerifyBoost | 35 |
| VerifyCrashOutcome | 51 |
| VerifyGestures | 39 |

1.30.5 trước khi jobs tắt đã chạy 21 suite/5.121 điều kiện, gồm VerifyRoofRiding 2.304 điều kiện từ 576 cú nhảy thực, VerifyTruckJump 69, BoostTrafficIsolation 478, CrashPoliceNavigation 214, các suite dịch vụ/va chạm/cử chỉ/art. Không cộng dồn số các bản để gọi là số test độc lập.

**Bằng chứng lịch sử xe máy Wave 23/10 (đã được Meshy thay thế, chưa từng phát hành):** 25 suite / 5.033 điều kiện đạt sau lần xuất cuối (gồm cả bản dựng lại đầu xe chi tiết; VerifyWaveBikes đo tới bề mặt tam giác), gồm suite mới `VerifyWaveBikes` 111 (metadata Wave, neo đèn trên xương mới, emission users, đèn đêm/pool, kính đèn và đầu pô có hình học thật tại neo, khói boost ở đầu pô, phối màu) cùng BikeArt 156, RoofRiding 2.304, SmoothTurns 468, BoostTrafficIsolation 478, JunctionKick 216, CrashPoliceNavigation 214… Xem `Captures/VERIFICATION_WAVE_BIKES.md` và `wave-bikes-*-results.json`. `VerifyKickContact`/`VerifyDayNight` dừng ở đoạn bắt buộc mở công việc (bị khóa) nên **không** tính là đạt.

**Xe máy Meshy 23/10 (đã phát hành WebGL 1.31.0):** **15 suite / 4,572 điều kiện đạt**: MeshyBikes 340, MeshyLighting 16, BikeArt 156, Jump 44, TruckJump 69, RoofRiding 2.304, QuickKick 62, JunctionKick 216, Boost 35, BoostTrafficIsolation 478, CrashOutcome 51, CrashPoliceNavigation 214, SmoothTurns 468, NightJumpLighting 88, DistanceMode 31. Xem `Captures/VERIFICATION_MESHY_BIKES.md` và `meshy-bikes-verification-summary.json`. Có ảnh Unity ngày/đêm đã kiểm tra; WebGL 1.31.0 đã kiểm tra riêng ở mục 21.8, chưa đo điện thoại thật.

**Phố Sài Gòn — phát hành1.34.0:** hồi quy mới10suite/**1.642điều kiện** đạt; thêm29bundle/77local/26live,0lỗi runtime. VerifyJump44đã bỏ giả định nóc tải cũ>3m; MeshyCarRoofs256giữ64quỹ đạo nhảy thật. CrashPoliceNavigation214đạt trước và sau chuỗi suite; lần lặp không cộng trùng, lỗi chập chờn lịch sử chưa tái hiện và runtime CSGT không đổi. `Captures/VERIFICATION_SAIGON_WEB_RELEASE.md`, proof`saigon-release-*`. Các lần kiểm tra Claude ban đầu1.907điều kiện và bản cuối2.017ở21.26 là lịch sử; không cộng dồn thành test mới.

**Lịch sử 1.14:** 1.335 assertion trong 17 suite; bảng bên dưới là snapshot lúc đó, không phải kết quả mới hoặc hướng dẫn bật công việc.

| Suite | Assertion đã ghi nhận | Phạm vi |
| --- | ---: | --- |
| `VerifyGameplay` | 23 | Vòng chơi, điều khiển, recycle, phố/ngôi sao |
| `VerifyCrossTraffic` | 28 | Xe ngang, lịch giao lộ, tốc độ/độ khó |
| `VerifyPursuit` | 39 | Đạp, dây chuyền, tự vệ, truy đuổi/bị bắt |
| `VerifyReckless` | 120 | Quái xế/xe tải, hai chiều, lạng, va nhà/FX |
| `VerifyTrafficDynamics` | 82 | Phanh, làn, rẽ, va chạm vật lý |
| `VerifyRearRandomPolice` | 60 | Vào từ sau, seed/random, CSGT né/ngã |
| `VerifyRiderContacts` | 45 | Quét va chạm mọi loại xe, luật thua khi truy đuổi |
| `VerifyBikeArt` | 156 | Rig/clip/LOD/bánh/đạp/ngã và giữ root; từ v2 kỳ vọng `SkinQuality.Bone2` |
| `VerifyCarArt` | 204 | 11 xe lớn, hướng impact, bánh/phanh, pool/FX |
| `VerifyMobileCamera` | 8 | Camera dọc đủ 4 làn, có/không truy đuổi |
| `VerifySafeArea` | 9 | Inset HUD, toàn cảnh, vùng touch hợp lệ |
| `VerifyJunctionRoutes` | 92 | Rows, mũi tên, cua, rebase, camera, CSGT |
| `VerifyJunctionRefinements` | 105 | Mũi tên không chồng, rẽ muộn, xe sau cua |
| `VerifyRoadDrivers` | 45 | NPC rẽ và xe ngược chiều lấn làn |
| `VerifyStreetArt` | 186 (323 từ 24/10) | Phố Blender, chiều mặt tiền/biển, LOD/cảnh quan; từ 24/10 thêm metadata/ngân sách/tên shop/vật cản JSON |
| `VerifyJobs` | 116 | 100 tình huống route và chuyến thật đón/trả/tiền/lưu |
| `VerifyJobEdges` | 17 | Sai đường giữ đích, đúng lề, khách/LOD, nhường xe, timer/pause/cua và thua |

**Chú ý từ 1.30.6:** scene/jobs release gate phải tắt. Các suite jobs/JobEdges/ServiceApproach/ServiceRearYield và browser JobWeb cũ đòi công việc bật sẽ không phù hợp khi chạy nguyên trạng. Không tự mở lại để làm test pass. Chỉ sau khi được người dùng yêu cầu mở lại mới bật hai cờ và chạy hồi quy dịch vụ. VerifyBoost/VerifyGestures đã có nhánh theo RideJobsReleased: khi tắt kiểm tra lời gọi công việc không cản boost/swipe, khi bật giữ kỳ vọng dịch vụ. Các suite va chạm cũ phải đọc kỳ vọng hiện tại, không dùng số mạng hoặc cấm đạp crossing cũ.

`VerifyMobileCamera` yêu cầu Game View dọc thật. Dùng `GameViewSetup.Portrait` rồi đợi cập nhật kích thước trước khi gọi; `GameViewSetup.Wide` để trả về ngang. Lỗi “Requires portrait Play Mode” là thiếu điều kiện thử, không phải bằng chứng camera/game đã hỏng.

Các bài test thay PlayerPrefs/timescale phải bảo toàn và khôi phục giá trị trước thử. Không xóa toàn PlayerPrefs của người dùng để đưa Coin/kỷ lục về 0 cho đẹp ảnh.

### 14.1. Kiểm tra trình duyệt

| Script | Kết quả bản 1.14 | Nội dung |
| --- | --- | --- |
| `AgentScripts/VerifyWeb.cjs` | 36 kiểm tra | HTTP, manifest/icons, cache, khởi động WASM, keyboard/touch, xoay máy, offline, lỗi runtime |
| `AgentScripts/VerifyJobWeb.cjs` | 8 kiểm tra | Click/chạm nút việc thật trên canvas, đổi từ menu sang GPS, layout và xoay |
| `AgentScripts/VerifyPublishedGame.cjs` | 26 live ở bản hiện hành; 22 ở 1.14 | So version/cache và nội dung tài nguyên deploy với docs, WASM/start/pause/lỗi |
| `AgentScripts/VerifyFullscreen.cjs`, `VerifyFullscreenGame.cjs` | Có trong repo | Kiểm tra layout/fullscreen riêng; xem script và báo cáo tương ứng trước dùng |

Các script browser dùng Node + Playwright; một số dùng `sharp`, Chrome channel và software WebGL flags. Máy mới cần cài dependency/browser phù hợp; `node_modules` không nằm trong repo. Script có thể dùng version kỳ vọng hardcode; khi tăng version, cập nhật đúng các chỗ kiểm tra tương ứng chứ không xóa kiểm tra.

Trong automation, touch quá nhanh có thể đi qua giữa hai frame Unity software-rendered. Các smoke đã dùng touchStart → giữ ngắn → touchEnd để mô phỏng chạm thực. Không thay test chạm bằng `SendMessage('AcceptJob')` rồi tuyên bố nút cảm ứng đã hoạt động.

### 14.2. Báo cáo và ảnh cần xem

- [Báo cáo 1.14 công việc/GPS/Coin](Captures/VERIFICATION_JOBS.md), các `jobs-*-results.json`, `jobs-web-results.json`, `jobs-web-ui-results.json`, `jobs-live-results.json`.
- [Xe máy và người lái Blender](Captures/VERIFICATION_BLENDER_ART.md).
- [Xe bốn bánh Blender](Captures/VERIFICATION_CAR_ART.md).
- [Rẽ và mạng đường hình học](Captures/VERIFICATION_JUNCTION_ROUTES.md), [sửa giao lộ](Captures/VERIFICATION_JUNCTION_REFINEMENTS.md).
- [Phố Sài Gòn 24/10](Captures/VERIFICATION_SAIGON_STREET.md), ảnh `Captures/saigon-street-comparison.png`.
- [Cảnh quan Blender](Captures/VERIFICATION_STREET_UPGRADE.md), [mái hiên/lá cây/chậu cây](Captures/VERIFICATION_FABRIC_LEAVES.md).
- [Full màn hình](Captures/VERIFICATION_FULLSCREEN.md), [Web/PWA](Captures/VERIFICATION_WEB_PWA.md).
- [Luật va chạm người chơi](Captures/VERIFICATION_RIDER_CONTACT.md), [thua khi truy đuổi](Captures/VERIFICATION_PURSUIT_LOSS.md).
- Các báo cáo `VERIFICATION_CROSS_TRAFFIC`, `TRAFFIC`, `RECKLESS`, `DROWSY`, `ONCOMING`, `REAR_RANDOM_POLICE`, `PURSUIT` ghi các bước triển khai trước đó.
- Ảnh lịch sử khi công việc còn bật: [3 công việc desktop](Captures/jobs-web-desktop-offers.png), [GPS điện thoại](Captures/jobs-web-phone-gps.png), [khách nam](Captures/passenger-male-seated.png), [khách nữ](Captures/passenger-female-seated.png).

Các kiểm tra điện thoại/iOS bằng browser profile là **mô phỏng layout/cảm ứng**, chưa phải thử trên phần cứng iPhone/iPad/Android thật. Chưa xác nhận FPS, nhiệt máy, mọi phiên bản iOS, âm lượng/gesture audio và thanh hệ thống trên mọi thiết bị. Không tuyên bố đã test máy thật chỉ từ tên profile Playwright.

## 15. Build WebGL và phát hành GitHub Pages

### 15.1. Cấu hình đang dùng

`PhoVietWebBuild.Configure()` đặt:

- Template `PROJECT:PhoVietPWA`.
- Gzip + decompression fallback, tên file theo hash.
- Một luồng, `threadsSupport = false`, vì Pages không cấp các isolation header cần cho shared-memory threads.
- `wasm2023 = false` để giữ dải tương thích Safari của dự án.
- Bộ nhớ khởi đầu128MiB, trần512MiB từ1.44.0/21.77; tănggeometric10%, mỗi lần tốiđa32MiB. Dữ liệu đã nénGPU theo thiết bị; trần1024MiB/heap550,69MiB của1.43.0 chỉ là lịch sử, không áp dụng lại.
- `dataCaching = false`, vì service worker phiên bản hóa chịu trách nhiệm cache thay vì hai cache độc lập.
- Cấu hình phiên bản build hiện1.47.0; `runInBackground` cấu hình build khác với ý định editor, còn web có bridge pause khi mất visibility.

Hậu xử lý build tạo `precache.json`, `build-info.json`, `sw.js` đã thay mã cache và `.nojekyll`. Cache là 16 ký tự hex từ SHA-256 nội dung tài nguyên, không gán tay để làm giả một lần build mới.

### 15.2. Quy trình phát hành

1. Kiểm tra Git/scene đúng, lưu thay đổi cần giữ và thoát Play Mode trước chỉnh scene/build.
2. Chạy kiểm tra phù hợp thay đổi, đọc lỗi compile/scene/runtime, xem hình nếu thay art/GUI.
3. Khi tăng version, đồng bộ ít nhất `PhoVietWebBuild.cs`, `AgentScripts/ConfigureWebRelease.cs`, kỳ vọng trong `VerifyWeb.cjs`/`VerifyFullscreenGame.cjs`, README và `PlayerSettings.bundleVersion` qua configure.
4. Dùng menu **Phố Việt/Build Web PWA (mobile + desktop)** hoặc `PhoVietWebBuild.Build()` để build lần lượt ETC2/EAC vàDXT, kiểm mã chung sau giải nén giống hệt rồi gộp2data. Không dùng một lệnh build đơn để phát hành từ1.44.0. Từ21.79 dùng `AgentScripts/BuildWebRelease.cs` entryMain đểqueue đủ2variant và đọcreportthànhcông; mỗi lượtprunehashcũtheogeneratedindex, importGPUphảiổnđịnh trướcbuild. EntryResume/Desktop chỉdùngphụchồicókiểmtra như21.79. Chờ thành công thật; không import/mutation/Play song song với build. Global Generic/NoOverride, chỉ cấu hình texture dependency bằng `PhoVietWebMemorySettings`. Giữ pixel/mip/filter/sRGB/geometry.
5. Sao chép toàn bộ output cuối sang `docs`, dọn hash cũ đúng phạm vi; chạy `python AgentScripts/PublishWebArtifact.py pack --source docs` (21.66). Dữ liệu được chia32MiB, file nguyên vẹn vẫn local/gitignore. Chạy `materialize --source docs --output _site` vào thư mục mới/rỗng để ghép/kiểm SHA/cache chính xác như Pages. Không stage file data nguyên vẹn >100MB.
6. Chạy server HTTP và kiểm tra local/offline/touch/layout trên artifact `_site` đã ghép sẽ publish; không test một shell khác với binary.
7. `git diff --check`, xem staged files, commit source/asset/scene và docs đã kiểm tra; không kéo các chỉnh sửa khác chưa liên quan vào commit.
8. Push `main`, chờ đúng workflow Pages cho đúng SHA thành công.
9. Kiểm tra live bằng `VerifyPublishedGame.cjs`; version/cache và nội dung tài nguyên phải khớp artifact local.
10. Lưu báo cáo và kết quả; trả Unity về trạng thái dễ chơi, khôi phục dữ liệu người dùng và dừng server kiểm tra cục bộ.

**Trước commit tính năng/sửa lỗi:** cập nhật `huongdan.md` cùng source, bao gồm trạng thái đang chờ phát hành. Khi publish xong cập nhật bằng chứng SHA/run/cache/live; không để tài liệu nói chưa phát hành mãi. Không rebuild Unity cho thay đổi tài liệu đơn thuần.

Ví dụ CLI đã dùng:

```powershell
unity command run_script --file AgentScripts/ConfigureWebRelease.cs --entry ConfigureWebRelease.Main --timeout_ms 60000 --timeout 70 --json
# Build đủ hai texture variants bằng menu Phố Việt/Build Web PWA (mobile + desktop)
unity command build_status --json
```

`build_status` có thể chứa báo cáo rất dài. Khi chỉ cần trạng thái, chọn các trường `status`, `buildId`, `result`, `totalErrors`, `totalWarnings`, `buildTimeMs`. Kết quả 1.14 cuối: `build_754073c7acfe`, Succeeded, **`buildTimeMs = 247744` (tương đương 247,744 giây)**, 0 lỗi, 1 warning Pipeline đã nêu.

Hoặc dùng Unity batchmode ngoài một editor đang khóa cùng project:

```powershell
& 'C:/Program Files/Unity/Hub/Editor/6000.5.6f1/Editor/Unity.exe' -batchmode -quit -projectPath 'C:/Users/HP/Documents/GameUnity/Taixecongnghe' -executeMethod PhoViet.Editor.PhoVietWebBuild.Build -logFile 'Builds/webgl-build.log'
```

Tạo thư mục log/output nếu chưa có. Thời gian build phụ thuộc thay đổi và cache; không cam kết mọi lần đều mất đúng thời gian báo cáo.

Ví dụ copy và dọn **chỉ file trong `docs/Build` đã xác minh đường dẫn**, không xóa cả workspace:

```powershell
Copy-Item -Path 'Builds/WebGL/*' -Destination docs -Recurse -Force
$publishBuild = (Resolve-Path 'docs/Build').Path
foreach ($file in Get-ChildItem -LiteralPath $publishBuild -File) {
    if (-not (Test-Path -LiteralPath (Join-Path 'Builds/WebGL/Build' $file.Name))) {
        if ($file.DirectoryName -ne $publishBuild) { throw 'Unexpected build path' }
        Remove-Item -LiteralPath $file.FullName
    }
}
```

Nếu chỉ sửa CSS/JS/manifest, `PhoVietWebBuild.RefreshWebShell()` có thể cập nhật shell từ một build tốt rồi tính cache lại. Từ21.83, hàm copy CSS/JS/manifest, icon và2ảnhposter-morning; thay toàn bộHTML từtemplate nhưng giữ nguyên khốiPHO_VIET_BUILD đã sinh (gồm2dataURL/version). Kiểm còn tokenUnity thì dừng; bỏposter.png cũ ởoutput và tínhcachelại. Tài nguyên mới ngoài danh sách phải bổ sung rõ ràng. Dùngrun_script với timeoutđủdài vìhash190MB cóthểvượt5giâyeval. **Sửa C#/FBX/scene/âm thanh phải build lại**, không dùng refresh shell để bỏ qua WASM/data cũ.

### 15.3. Chạy kiểm tra web

Từ21.66, trước khi chạy server trên checkout mới, chạy `python AgentScripts/PublishWebArtifact.py materialize --source docs --output _site` vào thư mục mới/rỗng để khôi phục data. Chạy server ở một terminal, các lệnh Node ở terminal khác. Máy người nhận tự chọn Python/Node đã cài; không phụ thuộc đường runtime riêng của Codex.

```powershell
python -m http.server 8765 --bind 127.0.0.1 --directory .
```

```powershell
node AgentScripts/VerifyWeb.cjs http://127.0.0.1:8765/_site/ handoff
node AgentScripts/VerifyPublishedGame.cjs https://denispham1107.github.io/taixecongnghe/ handoff-live
```

Nếu Playwright/sharp không ở `node_modules` thông thường, cấu hình `NODE_PATH` tới thư mục package đã cài. Dùng cùng môi trường mạng cho server localhost và Chrome; đã có trường hợp shell sandbox mở server nhưng Chrome ngoài sandbox không truy cập được. Không sửa code game để chữa lỗi kết nối localhost của môi trường kiểm tra.

Các file `Captures/browser-*.json/png` bị gitignore. Muốn lưu bằng chứng phát hành vào Git, copy kết quả đã kiểm tra sang tên có nghĩa như `jobs-web-results.json`; không chỉ báo “pass” mà không lưu bằng chứng khi cần bàn giao.

### 15.4. GitHub/PWA

Workflow `.github/workflows/pages.yml` chạy khi push `main` có thay đổi `docs/**`, scriptPublishWebArtifact hoặc chính workflow. Từ21.66 nó ghép dữ liệu từ parts/kiểmSHA/cache vào `_site`, kiểm artifact, upload `_site`, deploy bằng GitHub Pages. Nó **không biên dịch Unity từ source**. Chỉ push C# mà không build/copy docs thì người chơi trên web vẫn chạy bản cũ.

`AgentScripts/GitHubProject.ps1` có mode `Inspect`, `EnablePages`, `Status`; dùng Git Credential Manager trong process. Không in token, không đưa credential vào tài liệu/repo, không chạy `EnablePages` lại khi chỉ cần đọc trạng thái.

Service worker dùng cache theo scope và hash build. Từ1.44.0, install chỉ cache shell nhỏ; runtime được stream vào cache qua yêu cầu tải thật của Unity, không cache.addAll cả game ở landing. `precache.json.dataVariants` liệt kê2data; sẵn sàng offline khi đủ shell+loader/framework/WASM+đúng1data. Không tải variant không dùng; không đọc ArrayBuffer cảgói chỉ để cache. Worker restart đọc manifest từ cache, kiểm trạng thái offline bằng message. Lỗi tải có thể thử lại, không tuyên bố offline trước khi đủ tài nguyên. Không `skipWaiting` cưỡng bức thay binary giữa lượt; bản cũ có thể còn được dùng đến khi các tab/app cũ đóng. Sau khi activate mới dọn cache phiên bản cũ cùng scope.

Nếu người dùng vẫn thấy bản trước, kiểm tra `build-info.json`, trạng thái worker và việc đóng/mở lại ứng dụng; không khuyên xóa toàn dữ liệu ngay vì có thể xóa Coin/kỷ lục. Browser có thể thu hồi cache khi thiếu dung lượng, nên offline không phải bảo đảm dữ liệu tồn tại vĩnh viễn.

## 16. Các mốc công việc đã làm

| Mốc | Công việc đã triển khai | Chỗ tra thêm |
| --- | --- | --- |
| Nền game | Scene phố Việt, tài xế đỏ sao vàng, xe máy/ô tô hai chiều, lane input, camera sau, va chạm/thua, âm động cơ, HUD và kỷ lục | `VERIFICATION.md`, source gốc/builder |
| Giao lộ/nhịp độ khó | Xe ngang hai phía, tăng tốc/tần suất theo thời gian, tăng nhịp gấp đôi rồi thêm 50% | `VERIFICATION_CROSS_TRAFFIC.md` |
| Đạp và truy đuổi ban đầu (phím đã đổi) | Space lúc đó/double tap, ngã theo hướng, domino, CSGT, boost truy đuổi và buồng giam | `VERIFICATION_PURSUIT.md` |
| Quái xế/xe tải | Xe lạng/húc, ngược/cùng chiều, tự vệ không gọi CSGT, cháy/khói khi đâm nhà; bỏ timer 20 s | `VERIFICATION_RECKLESS/DROWSY/ONCOMING.md` |
| Giao thông bất ngờ | Phanh, đèn phanh, tạt làn, xe ngang rẽ, physics theo khối lượng/xung lực | `VERIFICATION_TRAFFIC.md` |
| Spawn/random/CSGT | Xe nguy hiểm vào từ sau camera; đổi seed/mẫu/lịch mỗi lượt; CSGT né, phanh, ngã 1,2 s | `VERIFICATION_REAR_RANDOM_POLICE.md` |
| Web/PWA | Đưa source lên GitHub, build WebGL, Pages, install desktop/Android/iOS, offline, trình tải/hướng dẫn | `VERIFICATION_WEB_PWA.md` |
| Fullscreen mobile | Sửa lề/crop, immersive viewport, safe-area HUD, camera dọc và xoay máy | `VERIFICATION_FULLSCREEN.md` |
| Luật truy đuổi trước 1.28 (đã thay thế) | Lúc đó va xe đang bị đuổi thua ngay; hiện phải té xong nằm chờ CSGT bắt thật | `VERIFICATION_PURSUIT_LOSS.md` |
| 1.8 | Dựng lại toàn bộ bộ xe máy/người lái bằng Blender MCP, rig/7 clip/3 LOD, giữ gameplay roots | commit `0a61dce` |
| 1.9 | Dựng lại 6 mẫu xe bốn bánh và thay 11 actor, rig/va chạm/bánh/LOD | commit `812dcb9` |
| 1.10 | 16 mũi tên, đường nhánh thật, rẽ 90 độ, camera/rebase, CSGT theo quỹ đạo | commit `97e4051` |
| 1.11 | Dời mũi tên khỏi crosswalk, rẽ muộn trong giao lộ và đợt 6–10 xe sau cua | commit `21e2e1c` |
| 1.12 | NPC dọc rẽ, xe thường ngược chiều lấn làn, dựng lại shop/cây/vỉa hè bằng Blender và sửa biển hiệu | commit `1a8cd82` |
| 1.13 | Sửa vải mái hiên mọi LOD, xóa chậu cây, dựng lá cây riêng | commit `954f72f` |
| 1.14 | Job/Coin/GPS/grid tên phố/giọng nữ/khách nam nữ/đón trả/nhường xe/touch chọn việc | commit `1cd602d`, báo cáo `92c64f2` |
| 1.15.0 — xe máy v2 của Claude Code | 6 mẫu xe/người, tay ga trắng, skin 2 xương; đã build/push/Pages, không còn trạng thái chỉ source 1.14 | `cb95f85` art, `6c5b8df` build; `VERIFICATION_BIKES_V2_WEB.md` |
| 1.16.0 | Ẩn hamburger mobile, một notice vàng/xanh, nửa mật độ 10 s đầu, nhường 3 s sau nhận việc, GPS báo 4/2 s | `33c58e0`; `VERIFICATION_MOBILE_REFINEMENTS.md` |
| 1.17.0 (lịch sử) | Vật lý té theo góc/tốc độ và ba mạng; nhánh ba mạng/hồi phục người chơi sau đó đã bỏ ở 1.28 | `d7067a3`; `VERIFICATION_RIDER_LIVES.md` |
| 1.18.0 | Điểm hẹn trong tầm nhìn, GPS đổi route ngay khi cua; ban đầu icon raster | `cd73911`; `VERIFICATION_WAYFINDING.md` |
| 1.18.0, revision vector | Thay raster bằng SVG xe máy chính diện + sao + đường, xuất cả bộ icon/PWA | `4c5ba84`; `VERIFICATION_VECTOR_LOGO.md` |
| 1.19.0 | HUD dọc gộp, trong nhẹ; bỏ ba ô dưới; FOV cho vật thể nhỏ 25% | `c5cb133`; `VERIFICATION_COMPACT_PORTRAIT_HUD.md` |
| 1.20.0 | HUD ngang gộp, mũi tên waypoint khối vát và pointer bridge HUD | `3253dd1`, `b6cab56`; `VERIFICATION_LANDSCAPE_HUD_3D_WAYPOINT.md` |
| 1.21.0 | Hàng chờ một xe nguy hiểm vượt sau, báo 2 s đúng lane, chờ xe nguy hiểm đang chạy va chạm; nhường lúc trả khách | `be3feaa`; `VERIFICATION_REAR_WARNING_SERVICE.md` |
| 1.22.0 | Cảnh báo 2 s cho cả xe thường vượt sau, reserve slot từng lane và không chồng icon | `d540734`; `VERIFICATION_ORDINARY_REAR_WARNINGS.md` |
| 1.23.0 | Animation nhảy Blender, quỹ đạo/tiếp đất/chạy nóc xe nhỏ; phím lúc đó Q, độ cao thấp hơn hiện tại | `3d4c80e`; `VERIFICATION_JUMP.md` |
| 1.24.0 (tham số cũ) | Boost E +75%/4 s/hồi 3 s và volume khói trắng Blender; phím/tham số đã đổi ở 1.25 | `a6dab40`; `VERIFICATION_BOOST.md` |
| 1.25.0 | Space nhảy, Q đạp, W +125%/4 s/hồi 2 s; hai làn giữa thêm nhánh sơn và được rẽ/giữ lane | `bf03269`; `VERIFICATION_INNER_TURNS_CONTROLS.md` |
| 1.26.0 | Nhảy cao 4,844 m, chạy nóc mọi xe tải đứng thẳng, rơi thật từ thùng xuống cabin | `09af0ba`; `VERIFICATION_TRUCK_JUMP.md` |
| 1.27.0 | NPC tự dựng sau 3 s, đúng hướng/đích rẽ cũ; vật cản tĩnh; lửa/khói Mantaflow bake | `61ff435`; `VERIFICATION_CRASH_RECOVERY_FIRE.md` |
| 1.27.1 | Ngưỡng vuốt theo cạnh ngắn, nhận tại pointermove, không chờ release, chống mất/trùng lệnh/rotation | `1d4a9c6`; `VERIFICATION_MOBILE_GESTURES.md` |
| 1.27.2 | Camera dọc xa thêm 30% so với 1.27.1, world scale 0,525 | `93d2500`; `VERIFICATION_PORTRAIT_ZOOM30.md` |
| 1.28.0 | Bỏ ba mạng/tự dựng người chơi; té thường hết lượt, té khi bị đuổi nằm chờ bắt vào tù | `22a50e3`; `VERIFICATION_CRASH_OUTCOME.md` |
| 1.28.1 | Cua bậc năm/arc length, giữ tốc độ và quãng dư, camera bám nhanh khi boost; xóa lean dư | `bcb52e6`, bản cuối `212b334`; `VERIFICATION_SMOOTH_TURNS.md` |
| 1.29.0 | Nắng trắng/ngày đêm 30+30 s, đèn xe/LED/sky Blender, thẻ mở đầu trắng nhỏ | `8f4be7a`; `VERIFICATION_DAY_NIGHT.md` |
| 1.29.1 | Giữ nắng trắng nhưng trả palette nhà/vỉa hè/cây rực rỡ, bảo vệ khi cài lại ánh sáng | `10f71ad`; `VERIFICATION_STREET_COLORS.md` |
| 1.29.2 | Tăng đèn đường, dành 2 nguồn sáng gần; đèn pha theo pose nhảy bằng world-to-light | `19fdea8`; `VERIFICATION_NIGHT_JUMP_LIGHTING.md` |
| 1.29.3 | Nhường toàn bộ dịch vụ và 2 s thực chạy sau cả đón/trả, không hụt frame chuyển phase | `bec8d73`; `VERIFICATION_SERVICE_REAR_YIELD.md` |
| 1.30.0 | Sương volume Blender sáu giây đầu/tan hết, theo thế giới/độ sâu và tắt chi phí sau đó | `4013767`; `VERIFICATION_MORNING_FOG.md` |
| 1.30.1 | Nhường ngay lúc giảm tốc 22 m, quét cả thân/yaw cũ mới và xe bị đẩy, không chỉ chỉnh Z cuối | `1bd65b2`; `VERIFICATION_SERVICE_APPROACH.md` |
| 1.30.2 | A* tìm đường tới điểm bắt quanh người té, vòng túi xe, không bị kẹt vô hạn do steering cục bộ | `e8691d2`; `VERIFICATION_CRASH_POLICE_NAVIGATION.md` |
| 1.30.3 | Approach/Swing/Return, giày chạm mới hất, đúng chân, trở về lane và hủy nếu chắn | `a2ee4e1`; `VERIFICATION_KICK_CONTACT.md` |
| 1.30.4 | Áp sát tăng tốc nhanh, ngang 32 m/s/Swing 0,30 s; TrafficPaceSpeed tách mọi NPC khỏi W | `e96fd6e`; `VERIFICATION_FAST_KICK_BOOST_ISOLATION.md` |
| 1.30.5 | Đạp crossing/xe rẽ ở giao lộ khi đi thẳng/trái/phải; quét đáp nóc xe chuyển động, hai bánh và rời mép giữa frame | `539ca1e`; `VERIFICATION_JUNCTION_KICK_ROOF.md` |
| 1.30.6 hiện hành | Khóa toàn bộ công việc/khách/GPS/Coin HUD, chỉ thi mét/kỷ lục, giữ tiền và mã để mở lại theo lệnh | `23941b1`, proof `98c3ed3`; `VERIFICATION_DISTANCE_MODE.md` |
| Xe máy Wave (23/10, chưa build/phát hành) | Mọi xe máy trừ CSGT thành xe số kiểu Wave theo ảnh mẫu, xe chính đúng màu ảnh, xe khác đổi màu, không logo/tem/gương; neo lại đèn và khói boost | `VERIFICATION_WAVE_BIKES.md`; commit xem mục 21.5 |

Không duy trì mỗi phiên bản như một nhánh gameplay độc lập. Bản 1.30.6 kế thừa art/cơ chế hiện hành và tạm khóa jobs; ba mạng/phím cũ/cua hãm tốc/va xe đang bị đuổi GameOver ngay là lịch sử đã thay thế.

## 17. Giới hạn hiện tại và điểm dễ làm sai khi phát triển tiếp

1. **Nguồn/bản build khác nhau:** sửa Assets không tự cập nhật docs hay Pages. Kiểm thử source pass chưa đồng nghĩa đã phát hành.
2. **Scene có tham chiếu serialize:** giữ `.meta`, Generated, imported model/material/clip; không xóa rồi tạo mới tùy tiện khiến GUID đổi.
3. **Play Mode không phải scene đã lưu:** thay Inspector/lưu asset cần đúng Edit Mode; đừng chỉ sửa instance trong Play rồi báo đã lưu.
4. **Builder lịch sử chưa tự cài đủ jobs:** đọc mục 5.1; không dùng nó để vô tình đưa game về state trước 1.14 hoặc reset version.
5. **Root và visual:** không đổi root vật lý/pool khi chỉ nâng cấp art, không để animation root motion chạy song song với code di chuyển.
6. **Rebase:** bất kỳ vị trí cũ, vận tốc, hiệu ứng hay actor mới gắn thế giới đều phải xem logic scroll/cua. Địa chỉ GPS logic không nên bị xoay trực tiếp theo transform hiển thị.
7. **CSGT:** không xuyên xe/teleport/bắt từ xa. Người chơi té khi bị đuổi nằm chờ cảnh sát tới bắt; không GameOver ngay, không tự dựng. Dùng navigation A* đã sửa, không quay lại steering cục bộ dễ mắc kẹt.
8. **Công việc đang khóa:** không tự bật lại. Mã nhường đã bao trùm tiếp cận/khách lên xuống/2 s chạy lại; không tắt collision hoặc xóa traffic để che lỗi nếu được mở lại sau này.
9. **Pause:** đừng chặn toàn bộ Update đến mức không nhận được chạm chọn việc/tiếp tục; timer phải gắn đúng State/WorkPhase.
10. **Tên phố:** `PickStreet`/`visitedStreets` cũ vẫn có trong file Routes, nhưng `PrepareBranches` hiện dùng `GridJunctionStreet`; không nối lại `PickStreet` làm GPS mất địa chỉ cố định.
11. **Pots/fallback:** code fallback hình học ban đầu có thể còn chi tiết khác FBX hiện tại. Muốn thay cảnh đang chơi, kiểm tra factory/scene thực; không suy luận chỉ từ hàm fallback.
12. **Blender → Unity:** kiểm tra axis, normals, scale, FBX take/clip, xương trái/phải trong world space, LOD, material và pose sau một frame. Không “sửa” bằng scale âm mà bỏ qua animation/collision.
13. **Cảnh phố và khách:** phố là modular lặp lại; tên/toạ độ logic cố định trong lượt không có nghĩa đã lưu một bản đồ thế giới độc nhất với từng đồ vật qua mọi lượt. Khách đi bằng script, không có AI đông người đi bộ.
14. **Vật lý:** đang là mô hình gameplay đơn giản hóa, không phải giả lập giao thông/biến dạng hoàn chỉnh; chưa có bảo đảm giải được mọi đống xe chặn đường.
15. **Kinh tế:** mã nhận Coin/lưu số dư còn giữ nhưng hiện khóa cùng jobs. Chưa có cửa hàng tiêu Coin/nâng cấp/thanh toán/tài khoản/ví máy chủ/đồng bộ đa thiết bị.
16. **Khôi phục lượt:** không lưu chuyến đang chạy, thành phố/seed và toàn traffic để resume sau đóng game; chưa có save-game toàn cảnh.
17. **Nền tảng:** hiện ưu tiên WebGL/PWA; không tuyên bố APK/IPA/native app đã build. Bản Windows trong Builds nếu còn chỉ là output cục bộ, không mặc định là bản mới nhất.
18. **Chất lượng thiết bị thật:** chưa xác nhận mọi iOS/iPadOS, FPS, nhiệt, RAM hoặc thời lượng pin; cần thử phần cứng thực khi có yêu cầu cụ thể.
20. **Thay visual xe máy:** dùng `PhoVietMeshyBikeUpgrade`/`UpgradeBikesMeshy`, đọc neo từ metadata Meshy, cập nhật mesh/material xe đậu nhánh và kiểm tra `VerifyMeshyBikes`. Đèn là con của Steering/Frame; khói boost dùng socket `PV_MeshyExhaust`. Không dùng `UpgradeBikesWave` để cài bản hiện tại.
19. **MCP không tự đi theo repo:** cấu hình agent, Unity CLI/Blender add-on, Node/Python và credential thuộc máy người dùng; không sao chép cấu hình cá nhân chứa bí mật để bàn giao.

## 18. Quy trình Claude Code tiếp tục làm cùng Codex

### 18.1. Bắt đầu một phiên làm việc

1. Đọc `BANGIAO.md` (bàn giao nhanh, 21.56), yêu cầu mới nhất của người dùng và `git status --short`/`git log --oneline -5`. Chỉ mở các mục của tài liệu này liên quan đến yêu cầu theo bảng tra cứu trong `BANGIAO.md`; không đọc lại toàn bộ tài liệu/mã nguồn mỗi phiên.
2. Xác định scope và những gì chưa chắc chắn; nếu dưới 90%, hỏi phương án trước khi làm phần đó.
3. Mở đúng scene, kiểm tra Unity CLI/MCP đang trỏ đúng project; không dùng port/PID cũ như hằng số.
4. Xác định source file + scene/asset bị ảnh hưởng, kiểm tra trạng thái đang Play/Edit và các chỉnh sửa chưa commit.
5. Nếu nhiều agent/công cụ cùng làm, phân công file rõ ràng và báo cho người dùng; không cùng lúc sửa scene hoặc chạy mutation vào cùng Unity/Blender. Không tự spawn/nhắn một agent khác nếu chưa được cho phép theo môi trường làm việc.
6. Thực hiện thay đổi nhỏ đúng phạm vi, giữ các hợp đồng gameplay nêu trên. Đọc lại Git diff trước khi sửa cùng partial class hoặc shared scene; thống nhất người phụ trách import/build/scene với người dùng nếu hai công cụ đang cùng chạy.
7. Chạy kiểm tra phù hợp, xem ảnh/render nếu có thay hình, rồi mới build/deploy khi phạm vi đã được người dùng cho phép.
8. **Bắt buộc cập nhật huongdan.md và `BANGIAO.md` trong cùng lần sửa/thêm chức năng**, gồm sửa mục cũ và thêm chi tiết mới: yêu cầu/quyết định, hành vi, file/hàm/tham số/asset/scene, kiểm tra/bằng chứng, build/version/cache/SHA/run/live nếu đã có; nếu chưa xong/chưa publish phải nói rõ. Không đợi người dùng nhắc, không chỉ ghi ở README/report riêng.
9. Báo rõ thay gì, vì sao, kiểm tra/source/commit/push/Pages và hạn chế. Nếu người cùng làm đã sửa huongdan.md, đọc lại rồi gộp phần bổ sung, không ghi đè nguyên file từ bản cũ.

### 18.2. Gợi ý chọn nhóm kiểm tra theo thay đổi

| Thay đổi | Nhóm nên ưu tiên |
| --- | --- |
| Va chạm/movement | Gameplay, RiderContacts, TrafficDynamics, CrossTraffic, Pursuit, RearRandomPolice |
| Rẽ/rebase | SmoothTurns, JunctionKick, RoofRiding, RoadDrivers, RiderContacts, CrashPoliceNavigation; Jobs chỉ khi được cho phép mở lại |
| Jobs đang tắt | VerifyDistanceMode, Boost/Gestures nhánh khóa; kiểm tra không sinh menu/GPS/khách và giữ Coin |
| Job/GPS/Coin sau yêu cầu mở lại | Jobs, JobEdges, ServiceApproach, ServiceRearYield + JobWeb; kiểm tra lại cờ và layout |
| Đạp/boost/nhảy/nóc | QuickKick, KickContact, JunctionKick, BoostTrafficIsolation, Boost, Jump, TruckJump, RoofRiding |
| Té/CSGT/hồi phục NPC | CrashOutcome, CrashPoliceNavigation, CrashUpgrade, TrafficDynamics |
| Ngày–đêm/đèn/sương | DayNight, StreetColors, NightJumpLighting, MorningFog + ảnh theo mốc thời gian |
| Xe máy/xe lớn | BikeArt/CarArt + collision/pursuit + ảnh pose/collision thật |
| Cảnh quan/mái hiên/cây | StreetArt, JunctionRoutes, hình LOD/mặt trên dưới, kiểm tra browser mobile |
| HUD/mobile/fullscreen | Gestures, VerifyDistanceMode, camera/safe area và ảnh dọc/ngang; JobWeb cũ không dùng khi khóa jobs |
| PWA/build/deploy | VerifyWeb trên docs, trạng thái đúng workflow SHA và VerifyPublishedGame |

Không cần rebuild/deploy WebGL chỉ vì sửa chính tài liệu bàn giao. Ngược lại, thay gameplay/art mà muốn cập nhật bản người chơi phải đi hết chuỗi source → build → docs → deploy → live check.

### 18.3. Nội dung có thể gửi kèm cho Claude Code

Từ 26/10/2026 (21.56) **không cần gửi prompt dài này nữa**: `AGENTS.md` (Codex tự đọc) và `CLAUDE.md` (Claude Code tự đọc) yêu cầu đọc `BANGIAO.md` + Git đầu mỗi phiên; người dùng chỉ cần gõ yêu cầu mới. Đoạn dưới giữ làm lịch sử (trạng thái 1.34.0, đã cũ):

> Hãy đọc toàn bộ `huongdan.md` tại gốc dự án và `git status` trước khi sửa. Game Phố Việt đã phát hành WebGL/PWA **1.34.0** với cảnh phố Sài Gòn/kính/nội thất/cây/đèn/mây mới của Claude ở21.19–21.27, giữ 4 mẫu ô tô Meshy và 8 màu sơn ngẫu nhiên mỗi lần xuất hiện ở 21.17–21.18, đuôi xe ổn định ở 21.16, giữ độ nét mobile ở 21.15, người/xe máy Meshy nhiều màu, xe chính trắng–đen, CSGT giữ riêng: hiện **chỉ thi quãng đường/kỷ lục, công việc/GPS/khách/Coin HUD đang khóa**, chỉ mở lại khi tôi yêu cầu. Q đạp có áp sát/chạm giày thật, Space nhảy và chạy nóc, W +125%/4 s/hồi 2 s; rẽ giữ tốc độ và đúng số làn; mobile vuốt nhận ngay đủ ngưỡng. Không có ba mạng: té thường kết thúc sau té, té đang bị CSGT đuổi thì nằm chờ cảnh sát tới bắt rồi vào tù. Không ghi đè file Blender cục bộ, không dựng lại scene toàn bộ theo thói quen. Nếu chưa chắc 90% hãy đưa phương án và chờ tôi xác nhận phần đó. Phân chia file/công cụ rõ ràng khi làm song song với Codex; không cùng mutation Unity/Blender. **Mỗi lần sửa/thêm chức năng phải cập nhật đầy đủ huongdan.md ngay trong cùng công việc**, ghi hành vi/triển khai/tham số/kiểm tra và trạng thái source/build/Pages. Đọc lại tài liệu mới nhất trước khi lưu để không mất phần người khác bổ sung.


## 19. Nhảy, nóc xe và boost — tham chiếu triển khai hiện hành

### 19.1. Dữ liệu animation từ Blender

**Từ21.58 đã dùng lại BikeJumpPose/BikeVisualAnimator cho người chơi xe máy xanh–đen. Phần ô tô21.57 dưới đây chỉ là lịch sử.**

**Player ô tô ở source 21.57:** `PlayerCarVisual` dùng 7 clip khung/nhún/va chạm từ FBX Petshop; pitch cất cánh/đáp là lớp pose có damping theo `RiderJumpClip/Pose`, không áp rig người hay clip đá chân vào ô tô. Quỹ đạo vật lý nhảy giữ nguyên. `BikeJumpPose` bên dưới giữ để hỗ trợ các scene xe máy cũ.

`pho_viet_jump.py` chạy trong Blender thật trên `PV_HeroScooter_Rig`, tạo năm action JumpTakeoff/JumpAir/LandBoth/LandFront/LandRear, mỗi pha 61 mẫu, cùng JumpDemo để xem quỹ đạo đầy đủ. Lưu nguồn `PhoViet_Jump.blend` riêng, xuất bảng pose `Assets/PhoViet/Resources/PhoVietJump.json`. Không cần thay sáu FBX/17 xương/bảy clip Legacy cũ chỉ để thêm nhảy.

`BikeJumpPose` đọc/nội suy bảng; `BikeVisualAnimator` phối pose, co gối/tay/chuyển trọng tâm, xoay thân quanh điểm bánh sau, hấp thụ khi đáp. Chuyển góc bằng damping, giữ điểm thấp nhất trên mặt đỡ; nếu khách được mở lại thì `PoseJumpPassenger` bám cả lift/pitch. Visual không tự điều khiển quỹ đạo root.

### 19.2. Vật lý và mặt đỡ

`PhoVietJump` (source21.60) giữ windup0,24 s, gravity22 m/s², cooldown2 s. Tham chiếu đỉnh cũ H0=14,6²/(2×22)=4,844545 m; độ cao mới H(v)=H0×(0,75+max(0,v−17)/52), v tính bằng m/s. `jumpLaunchVelocity=sqrt(2×22×H(v))` được chốt ở lúc windup kết thúc; `RiderJumpTakeoffSpeed`/`RiderJumpApex` là telemetry. Tốc độ lấy sau cruise/W/giới hạn dịch vụ, trước bonus di chuyển khi bay+3 m/s; W ngay trong windup được tính, W giữa không trung không sửa vận tốc Y hay đỉnh đã chốt. TickJump chỉ tích phân phần dt còn lại sau windup trong frame cất cánh; pose JumpAir chuẩn hóa bằng vận tốc phóng riêng của cú đó để chuyển động mượt ở mọi độ cao. Hồi2 s và điều kiện từ đường/nóc, không double jump, pause/rebase vẫn giữ nguyên.

Scene/web1.33.0 dùng `MeshyCarVariants`: kích thước/hình bao và 49 mẫu mặt nóc lấy từ giao tia xuống tam giác thật, đổi đồng thời khi chọn mẫu tải. `JumpRoofHeight` và `RoofSurfaceAt` đọc profile này; không dùng chiều cao xe tải cũ cho container mới. Fallback cho bộ cũ/web 1.32.2: sedan/taxi 1,60 m, hatchback 1,69 m, DeliveryVan 1,91 m, thùng tải 3,175 m/cabin 2,71 m. Góc pitch/roll vượt 12° không còn mặt đỡ. Xe tải đã va chạm nhưng còn đứng thẳng vẫn có nóc; xe nhỏ knocked/wallCrashed bị loại. Model inactive/pool bị thu hồi cũng không đỡ người chơi. Chi tiết và kiểm tra mẫu mới ở 21.17.

Sửa 1.30.5 đồng bộ mặt nóc với **vị trí/yaw của xe tại đúng thời điểm va chạm**, không lấy mặt cuối frame cho va chạm đầu frame. Ô tô xét cả bánh trước và sau, không hạ root vào kính/capo khi bánh sau còn trên nóc cao. Xe tải giữ điểm tựa bánh sau trên thùng tới khi qua mép; sau đó rơi xuống cabin, không snap xuống.

`ResolveJumpSurface` quét toàn đoạn để tìm đáp lần đầu, kể cả đáp rồi qua mép trong một frame tốc độ cao. Dùng phần thời gian dư sau chạm để chạy tiếp trên mặt đỡ/rời mép. Chỉ khảo sát xe gần vùng quét để giới hạn chi phí. Collision Y/XZ dùng cùng contact/support, tránh hộp bảo thủ coi đang được đỡ là đâm xe. Đâm vách thùng từ cabin thấp, nhảy muộn/thiếu cao hoặc xe lật vẫn té thật.

Tiếp đất chọn LandBoth/Front/Rear theo vận tốc xuống và chuyển động tương đối; chân/root theo mặt đỡ, cân bằng rồi tiếp tục. Bắt đầu té hủy trạng thái nhảy nhưng chuyển vận tốc Y hiện có sang vật lý văng. Rebase khi cua chỉ đổi mặt phẳng, giữ độ cao/vận tốc đứng; camera theo lift nhẹ và giữ xe trong khung.

### 19.3. W và khói phản lực

`PhoVietBoost`: BoostMultiplier=2.25, BoostDuration=4, BoostRecovery=2. TryBoost chỉ Riding, không ServiceStopped/cooldown/đang boost; không stack hoặc kéo dài khi spam. `CancelBoost` hủy ngay, đặt hồi 2 s nếu đang boost, xóa khói. TickBoost đóng băng khi pause, xử lý phần thời gian dư lúc boost kết thúc để cooldown không lệch frame. Va chạm/bị bắt/dịch vụ hủy; reset lượt xóa tất cả. W có thể chạy cùng nhảy và qua rẽ.

`pho_viet_boost_smoke.py` dựng volume khói trắng có mật độ noise 4D trong Blender, render Cycles bốn frame RGBA, atlas 768×192 tại `Resources/Boost/PV_WhiteBoostSmoke.png`. Import Clamp/Bilinear, alpha, không mip/nén làm lem ô. Lịch sử21.57 (đã ngưng dùng từ21.58) gắn khói người chơi vào `PlayerCarVisual.exhaustMount` trên Frame tại `(0,68; 0,36; -1,85)`; đèn/khói bám cùng visual khi nhảy/đáp/va chạm. Fallback xe máy: nguồn khói gắn vào socket `PV_MeshyExhaust` trên xương Frame, vị trí metadata Meshy `(.23,.40,-1.0)` trong hệ xe. `BoostSmokeVisual` tự tìm socket, localPosition zero; fallback `(.214,.45,-1.02)` chỉ dùng cho scene Wave cũ không có socket. `BoostSmokeVisual` có luồng hẹp sát pô và lớp khói nở sau, tối đa 128 hạt, mô phỏng theo hệ đường. Nguồn phát bám Frame/pose thực khi ngóc bánh/nhảy; vệt đã phụt không quay theo đổi làn. Pause/resume cùng game; sau rebase xóa vệt cũ rồi phát lại mà không mất thời lượng W.

### 19.4. Không tăng ga NPC theo W

Nguyên nhân cũ: nhánh pacing dùng `Speed-1.2`, xe vượt/reckless dùng `Speed+...`, khiến W hoặc áp sát kéo tốc độ mọi xe lên. Sửa 1.30.4 tách **TrafficPaceSpeed** theo độ khó cơ sở; xe thường dùng baseTrafficSpeed, phanh/đổi làn giữ hành vi riêng. Spawn/vượt/lạng trong PhoVietReckless/PhoVietExitTraffic, khởi đầu/theo đường rẽ CSGT và nhường JobTraffic không lấy boost tạm của người chơi.

Vẫn cần `(v.speed - Speed)*dt` để mô phỏng tọa độ tương đối với tài xế: việc xe trên màn hình lùi nhanh hơn khi người chơi W không có nghĩa xe đó tăng ga. Không bỏ phép trừ scroll để “giữ tốc độ NPC”. Kiểm tra bằng tốc độ thực/độ dịch chuyển trong hệ đường và so một lượt đối chứng cùng seed, không chỉ nhìn khoảng cách camera.

### 19.5. Bằng chứng và giới hạn kiểm tra

- `VerifyQuickKick`: xe cùng chiều 28 m/s cách trước 2 m được bắt kịp/chạm giày trong khoảng 0,15–0,18 s; xe ngược 12 m khoảng 0,25–0,26 s (thời gian mô phỏng).
- `VerifyJunctionKick`: các hướng mục tiêu thẳng/ngược/cắt ngang/đang rẽ, người chơi đi thẳng/trái/phải, hai bên và bước 20/50 ms; có trường hợp hủy an toàn khi thoát cửa sổ, không ép mọi trường hợp phải đạp trúng. Có bộ điều khiển crossing thật và động tác qua thời điểm rebase.
- `VerifyRoofRiding`: cú nhảy thật từ mặt đường, sáu pool xe, dt 16/33/50/100 ms, có/không W, tốc độ mục tiêu -12/0/7/28 m/s và ba offset; 2.304 điều kiện/576 cú nhảy đạt ở 1.30.5–1.32. Từ 21.30 (sau 1.34.0) ghim từng mẫu Meshy của pool tải (thùng bạt và container) trước/sau `SpawnTraffic`, lấy độ cao chạm từ profile nóc mẫu đó: **768 cú nhảy / 3.072 điều kiện**, gồm 8 ca nhảy muộn (container 28 m/s offset +0,5 không W) phải té thật. Có ảnh chụp lúc tiếp xúc/được đỡ thật, không ép gọi KnockVehicle hoặc đặt người đứng sẵn trên nóc.
- `VerifyTruckJump` từ 21.30 chạy trên mẫu Meshy đã ghim của cả hai pool tải (125 điều kiện); bản 69 điều kiện cũ gắn hình học tải Blender (nóc 3,175 m, cabin 2,71 m) đã lỗi thời từ 1.33.
- `VerifyJump`, `VerifyTruckJump`, `VerifyBoostTrafficIsolation`, `VerifyBoost` kiểm tra riêng quỹ đạo/xe lạng/xe lật/rời thùng/cooldown/khói và tốc độ NPC. Xem JSON/report từng bản, không suy rộng smoke input WebGL thành đã đo mọi trường hợp vật lý trên iPhone thật.

## 20. Ngày–đêm, đèn, màu phố, sương và lửa khói

### 20.1. Nguồn và ranh giới Blender/Unity

Blender dùng dựng model/volume/keyframe/profile/texture đã bake, không chạy mô phỏng nặng Blender trong trình duyệt. Unity đọc dữ liệu và áp dụng trong Built-in Render Pipeline. Không cần chuyển sang URP/HDRP hay thêm bloom toàn cảnh để các hiệu ứng hiện tại hoạt động.

| Nguồn | Tích hợp runtime/editor |
| --- | --- |
| `pho_viet_day_night.py` → `PhoViet_DayNight.blend`, FBX/textures/profile trong Art/Lighting | PhoVietLightingUpgrade; DayNightLighting/PhoVietDayNight; DayNightSky/OpticalLight shaders |
| `pho_viet_morning_fog.py` → `PhoViet_MorningFog.blend`, Art/Fog/PV_MorningFogDensity.png và PV_MorningFog.json | MorningFog trên camera; MorningFog.shader; AgentScripts/InstallMorningFog.cs |
| `pho_viet_wreck_simulation.py` → `PhoViet_WreckFire.blend`/JSON, Resources/Wreck atlases | RecklessArt/WreckFireVisual, WreckFlame.shader; AgentScripts/InstallCrashUpgrade.cs |
| `pho_viet_boost_smoke.py` → `PhoViet_BoostSmoke.blend`, Resources/Boost atlas | BoostSmokeVisual, AgentScripts/ConfigureBoostArt.cs |

Nguồn `.blend` thủ công khác tên (`PhoViet_Anhsangngaydem.blend`, `Phoviet_Suongmu.blend`) đang chưa track không mặc định là nguồn đã xuất vào build. Đừng thay chúng bằng các file canonical hoặc tự ghi đè khi chạy script.

### 20.2. Chu kỳ và màu ban ngày

`DayNightLighting` có ElapsedTime tăng chỉ trong Riding, CycleTime=Repeat(ElapsedTime,60); NightLevel=0.5-0.5*cos(CycleTime*pi/30). Ngày ở 0/60, đêm ở 30; chuyển liên tục, không đổi cảnh đột ngột. Pause/menu việc khi bật không tăng; reset khởi tạo ngày. Đồng hồ khác timeRiding của độ khó và không bị reset về 0 mỗi chu kỳ. Cảnh jail dùng ánh sáng riêng.

Trời/nắng/trăng/ambient/fog/reflection và emission nội suy theo profile. Từ 24/10 `DayNightSky.shader` thêm mây tích fBm 4 tầng (`_CloudCover` .5, `_CloudScale` 1,15, `_CloudSpeed` .006, bám `_SkyFrame` nên không xoay khi rẽ, có dự phòng khi `_SkyFrame` chưa gán ở Edit Mode) và màu `_SkyBlue` (.33,.6,.93; trộn .78) làm trời cao ban ngày xanh đậm như ảnh mẫu; đêm mây chuyển xám xanh tối, không ảnh hưởng sao/trăng. Solar frame đổi cùng rebase để hướng nguồn sáng ngoài trời không giật khi phố đổi hệ tọa độ. Runtime clone material emission/sky để Play Mode không ghi thay asset.

1.29.1 khôi phục **PV_StreetPalette.png** cho PV_StreetAtlas.mat, các mặt tiền vàng/xanh ngọc/hồng/lam, gạch/vỉa hè/bó vỉa và nền đất xanh 92A68A; giữ nắng trắng và vạch sơn trắng. PhoVietLightingUpgrade/RestoreColorfulStreetPaint và script Blender không xuất lại palette kiến trúc bạc nhạt. Đừng “giữ tông trắng” bằng cách tẩy toàn bộ vật liệu phố. Từ phố Sài Gòn 24/10 palette này do `pho_viet_saigon_streets.py` xuất, lưới 8×8 = 64 màu (21.19).

### 20.3. Ngân sách đèn và đèn pha bám nhảy

Rig hiện có 70 bộ LED trên 35 module và 30 bộ đèn xe gồm người chơi/NPC/CSGT. Đèn trước/hậu, lens/halo và cookie sáng dần theo độ tối, tắt khi root xe inactive. Xa dùng optics bake; gần tối đa **6 đèn thực**, quality tối đa 4 per-pixel. Ưu tiên đèn người chơi và **dành 2 trong 6 cho hai cột gần nhất**, không để xe đông chiếm hết ánh sáng phố.

Đèn đường gần đã tăng intensity 1,65→3,6, range 17→20 m, alpha vùng chiếu 0,33→0,44. Từ 24/10 bầu đèn cong cao hơn: tâm quang học `(2,35; 8,48; 0)` trong hệ `Đèn LED • ±1` (trước `(1,45; 6,64)`), vệt sáng mặt đường ở x = ±5,25 (trước ±6,15); `PhoVietLightingUpgrade` đã dùng giá trị mới, vật liệu `PV_StreetLEDMetal` xám `8C959C`. Đây là cài đặt bản hiện hành, không nhân thêm mỗi lần cài lại. Đèn LED ba LOD, có glow/profile tạo bằng Blender.

Neo đèn xe máy Meshy (23/10): đọc `anchors` trong `Bikes/Meshy/PV_*.json`, đèn pha trên Steering tại khoảng `(0,1.25,.573413)`, đèn hậu trên Frame `(0,.995,-.830376)`. `PhoVietMeshyBikeUpgrade` dựng lại các neo sau thay visual; `PhoVietLightingUpgrade.Bike` cũng đọc metadata khi cài lại đèn. CSGT giữ neo cũ `(0,1.13,.68)`/`(0,.805,-1.112)`. Không còn nhánh “sport dùng Frame”. DayNightLighting có DefaultExecutionOrder(200), cập nhật sau pose LateUpdate của xe. Lấy vị trí/trục từ head gắn Frame/Steering, gồm độ cao nhảy, pitch ngóc/đáp và nóc xe; độ chúc dùng local-down. OpticalLight shader dùng ma trận world→head mỗi frame để chiếu cookie Blender lên mặt đường. Cookie chuẩn author đèn cao 1,1 m, miền x[-16,16], z[-4,28]; khoảng chiếu chặn 32 m, giảm theo góc/khoảng cách. Khi đầu ngóc, vùng gần rời đường; hạ bánh thì vùng sáng quay gần đúng hướng. Không nâng một tấm tam giác sáng lơ lửng hoặc để ánh sáng cố định dưới chân người bay. Spotlight thật chiếu hình học xe/nóc/vật thể; receiver xa/nhỏ tối ưu overdraw, chỉ mở rộng khi nguồn cao/nghiêng. MaterialPropertyBlock chỉ bật chế độ project mới trên vệt đèn pha, không làm sai UV halo/đèn phố.

### 20.4. Sương sớm sáu giây

Blender tạo volume noise/fBm loang, giảm theo cao, animation tan frame 1–181 ở 30 fps. Xuất density tuyến tính 32³ trong atlas **272×136**, 32 lát có padding (8×4, tile34); profile density 0,065, heightFalloff 0,24, distance 100 m, 61 mẫu fade. Không sRGB/mip/nén/rescale NPOT làm lệch dữ liệu. Textures đã pack trong nguồn Blender để gửi dự án.

MorningFog trên camera đọc depth và tích phân **6 mẫu/pixel** dọc tia nhìn; truyền sáng theo bề dày sương, có gió/jitter nhẹ. Đây là sương theo sâu, không overlay trắng toàn màn hình/billboard tròn; HUD IMGUI vẽ sau nên không bị phủ. Trường theo hệ phố, scroll/rebase qua rẽ; không dính màn hình.

Sương dùng **ElapsedTime**, không CycleTime: tan từ giây 0 đến hết giây 6 của mỗi lượt; không quay lại vào bình minh vòng sau. Pause đóng băng; trong service Riding nếu được mở lại vẫn tan. Tới giây 6 tắt component và phần depth mà nó sở hữu, tránh tốn render phần còn lại. Không làm mất yêu cầu depth của thành phần khác; jail Hide sương. Cài lại lighting toàn bộ phải chạy InstallMorningFog sau đó để giữ tham chiếu camera/profile.

### 20.5. Xe cháy khi đâm tường

Hiệu ứng mới dùng **Mantaflow gas combustion** Blender: lưới 96, cache 48 frame, xoáy/nhiên liệu/khói, render Cycles 128 mẫu. Bake `PV_WreckFlame.png`/`PV_WreckSmoke.png` trong Resources/Wreck, mỗi atlas RGBA 768×384 có 8 frame. Common alpha crop cho khói giữ tâm/kích thước nhất quán. Cache tái tạo Temp/WreckGasCache96 không phải runtime hoặc dữ liệu phải commit.

WreckFireVisual/RecklessArt có tối đa 48 hạt lửa / 40 hạt khói mỗi wreck, lửa emission và đèn nhấp nháy, khói đen bốc/tản theo nhiễu, nguồn ở động cơ theo yaw xe. Bỏ capsule/sphere giả ngọn lửa. Scene lưu đủ material/texture, installer dùng lại material đã có. Chỉ quái xế/tải ngủ gật wallCrashed nằm lại cháy, không dựng sau 3 s; vẫn scroll như vật trong thế giới và thu hồi khi ngoài vùng, không “đóng đinh” vào một tọa độ màn hình.

## 21. Khóa công việc 1.30.6 và hợp đồng duy trì tài liệu

### 21.1. Một khóa phát hành, giữ mã để phục hồi

```csharp
// Assets/PhoViet/Scripts/PhoVietJobs.cs
public static readonly bool RideJobsReleased=false;
public bool jobsEnabled=false;
public bool JobsAvailable=>RideJobsReleased&&jobsEnabled;
```

Scene lưu jobsEnabled:0. PhoVietPassengerUpgrade gán theo RideJobsReleased nên nâng cấp model khách không vô tình bật tính năng. Không thêm UI tùy chọn mở lại cho người chơi hoặc đổi khóa trong build thường. **Chỉ sau yêu cầu trực tiếp mở lại của người dùng** mới đổi release gate và scene, kiểm tra đầy đủ các luồng/timer/UI cũ cùng thay đổi mới.

Guards dùng JobsAvailable tại ShowJobOffers/AcceptJob/UpdateWork/TickWorkMotion/ReplanJob/preview route, service approach/stopped/protected, BeginService/TickService, voice, JobOffersGui/DrawJobMap, marker và traffic yielding. ResetCityJobs vẫn cần tạo city/navNext/heading/seed/tên phố và ẩn khách/clear route/stop voice, **không bỏ cả hàm vì jobs đang tắt**. Thành phố/road turning hoạt động độc lập.

Không còn menu 3 việc/tự pause/GPS/nhãn điểm hẹn/giọng dẫn đường/khách lên xuống/giảm tốc phục vụ/nhường riêng công việc. Không cộng/xóa Coin: chỉ load số dư cũ, ẩn trên HUD. Game bình thường vẫn quái xế/CSGT/va chạm/nhảy/boost/đạp/rẽ/ngày đêm/sương, tính mét và kỷ lục như cũ. Không có leaderboard máy chủ.

VerifyDistanceMode có hai lượt mô phỏng 60 s với cờ jobsEnabled false/true, chứng minh cờ scene cũ không vượt release gate; không tự dừng/chọn việc, vẫn đi giao lộ/tăng mét/kỷ lục/chơi lại. Còn kiểm API nhận việc/stale service không cộng thưởng/tạo voice. 8 suite / 946 điều kiện Unity và 26 local / 26 live đã đạt; không gọi các suite dịch vụ bị khóa là pass. Đã xem ảnh distance-hud-portrait/mobile-landscape/desktop và WebGL pause.

### 21.2. Mốc phát hành đã đối chiếu

- Mốc lịch sử 1.30.6 (đã thay bằng 1.31.0 tại 21.8), source/artifact commit `23941b11c82116d45185a51672d554dfac07413d`.
- Build `build_6c2bf1bad844` Succeeded, 374.734 ms (374,734 giây), 0 lỗi / 3 warning đã biết. Thời gian build là snapshot, không phải dự đoán lần sau.
- Cache `c713cfa4fa73614d`, build UTC `2026-10-22T11:49:17.4262969Z`.
- Pages run [39797882879](https://github.com/denispham1107/taixecongnghe/actions/runs/39797882879) completed/success đúng SHA.
- 26 live checks đạt: version/cache/tài nguyên khớp docs, khởi động WASM thật, input start/pause, 0 runtime error. Proof commit `98c3ed3`; `Captures/distance-pages-release.json`, `distance-live-results.json` và `VERIFICATION_DISTANCE_MODE.md`.
- Tài liệu cập nhật 22/10 không thay đổi C#/art/scene/build/docs, không cần rebuild hoặc phát hành bản game mới. Không tự nâng version chỉ để tài liệu mang ngày mới.

### 21.3. Mỗi lần sửa/thêm chức năng phải cập nhật huongdan.md

Đây là yêu cầu trực tiếp của người dùng, áp dụng các lần làm tiếp, không phải lời khuyên tùy chọn. Ghi trong **chính file này**, không chỉ README, báo cáo Captures, chat hoặc bộ nhớ cá nhân. Cần bao gồm:

1. Yêu cầu/quyết định đã xác nhận; tính năng nào đang bật/tắt, thay thế luật/phím/tham số nào.
2. Hành vi người chơi và edge case quan trọng, desktop/mobile/dọc/ngang nếu liên quan.
3. File/hàm/field, scene/asset/shader/animation/Blender nguồn → export → runtime, tham số và tương tác với các hệ khác.
4. Kiểm tra đã chạy và bằng chứng thật, phần chưa kiểm/chưa xong/giới hạn; không tự tăng số test hoặc đánh đồng profile với máy thật.
5. Version/commit/build/cache/push/Pages run/live nếu đã có; nếu chưa publish ghi rõ chờ, sau publish cập nhật kết quả.
6. Sửa cả đoạn cũ mâu thuẫn; lịch sử giữ nhưng gắn nhãn “đã thay thế”, không để hai quy tắc cùng được gọi hiện tại.

Đọc lại phiên bản file mới nhất trước khi lưu/gộp, giữ bổ sung của Claude Code/Codex. Nếu làm song song, thống nhất phạm vi file/branch với người dùng; tránh cùng sửa file partial, scene, Generated hoặc docs build. Chỉ một bên thực hiện mutation Unity/Blender/import/build tại một thời điểm. Không tự gửi lệnh/message sang task khác nếu chưa được cho phép. Trước commit, kiểm diff không lẫn nguồn Blender/local ngoài scope; mỗi phần việc có báo cáo/bằng chứng dễ bàn giao, không force push để bỏ mất thay đổi bên còn lại.

Hướng dẫn duy trì này cũng được ghi ở AGENTS.md tại gốc repo và trong bộ nhớ Codex theo yêu cầu. Khi chuyển máy/agent, **repo và tài liệu hiện hành** mới là nội dung cần gửi; không giả định bộ nhớ riêng hoặc MCP client của Codex sẽ tự có ở Claude Code.

### 21.4. Lần cập nhật tài liệu ngày 22/10/2026

- Đọc hết 889 dòng của bản bàn giao cũ, giữ phần xe máy v2 Claude Code đã bổ sung; sửa các mô tả hết hiệu lực thay vì chỉ nối nhật ký mới ở cuối.
- Đối chiếu lịch sử Git, mã/scene/config, metadata art và báo cáo từ 1.15 đến 1.30.6. Bổ sung cơ chế, tham số, bản đồ mã, Blender → Unity, bằng chứng và quy tắc phối hợp.
- 12 kiểm tra tài liệu đạt: cấu trúc mục/code fence, link file nội bộ, mốc version/cache/SHA, các cơ chế mới và loại mô tả bật jobs/phím cũ khỏi hướng dẫn hiện hành. Kết quả tại `Captures/huongdan-doc-checks.json`.
- Chỉ thay tài liệu và hướng dẫn dự án; không thay gameplay/model/scene/WebGL, không chạy lại toàn bộ kiểm tra game hoặc build chỉ cho lần cập nhật tài liệu này.

### 21.5. Xe máy kiểu Wave theo ảnh người dùng (23/10/2026, Claude Code) — đã được Meshy thay thế ở 21.7

- **Yêu cầu:** dựng lại tất cả xe máy giống ảnh mẫu (xe số kiểu Wave độ) bằng Blender MCP. **Đã chốt:** trừ CSGT; không logo/tem; xe chính y màu ảnh, xe khác đổi màu; lưu nguồn file mới.
- **Nguồn:** `ArtSource/Blender/pho_viet_wave.py` → `PhoViet_Motorcycles_Wave.blend`, gọi `build_wave_motorcycles()`. Script exec `pho_viet_motorcycles.py` để dùng người lái/rig/clip/LOD/atlas/xuất của v2, rồi xuất đè `Assets/PhoViet/Art/Bikes/PV_{HeroScooter,CityScooter,Underbone,SportBike,Racer}.fbx/.json/_Palette.png`. `PV_TrafficPolice` không đổi.
- **Thân xe (bản chi tiết, dựng lại theo góp ý người dùng 23/10):**
  - **Đầu xe:** hộp mỏng, mặt trên gần phẳng, vách thẳng sắc cạnh, dựng bằng `slab_solid` từ đường viền nhìn từ trên. Mũi nhọn chính là kính đèn pha hình khiên đáy chữ V; cánh mỏng dần ra hai đầu tay lái; dải xi-nhan cam chạy dọc hông trước; vách sau và hộp đồng hồ màu đen. Mũi dày khoảng 0,12 m, cánh khoảng 0,04–0,05 m. Bản trước là khối loft tròn mập, người dùng đã bác bỏ.
  - **Mặt nạ:** xanh–đen, thuôn chữ V có gờ giữa (`OutlineSolid`), phần trên lẩn vào cằm dưới đèn.
  - **Yếm bạc:** tấm vỏ cong có độ dày (`side_panel`) hình tam giác ngược, rộng dưới đầu xe, mũi nhọn chĩa xuống trước máy; mép trước ôm vào mặt nạ, mép sau có nẹp crôm, luôn nằm trước đầu gối người lái. Yếm trong đen nối kín hai tấm bạc.
  - **Chắn bùn trước:** rộng bằng lốp, có gờ giữa. Chắn bùn sau là tấm đen rộng, không còn vây chẻ đôi biển số. Hai lỗi này do hướng mặt cắt sweep: với `up_hint=X` trên cung tròn, bán kính thứ nhất là hướng xuyên tâm, bán kính thứ hai là bề ngang.
  - Phần còn lại giữ như cũ: ốp sườn sơn vuốt lên đuôi; yên đen, tay dắt crôm, phuộc sau lò xo vàng; pô crôm phải; vành nan crôm 36 nan, đĩa sóng; không gương.
  - Xe chính: đầu `1A2130`, thân `EEF0F1`, yếm `C6CCD2`.
  - Xe khác: đỏ, xanh dương, đen, cam–đen.
  - Mỗi mẫu 17 xương/7 clip/3 LOD (LOD2 = 5.200).
- **Unity:**
  - `AgentScripts/UpgradeBikesWave.cs` cài model, xe đậu, bake nhánh, **neo lại đèn** và `emissionUsers`, rồi lưu scene.
  - `PhoVietLightingUpgrade.Bike` dùng vị trí đèn Wave.
  - `BoostSmokeVisual` đặt nozzle ở đầu pô Wave.
  - `VerifyWaveBikes.cs` và `CaptureBikesWave.cs` là công cụ mới.
  - Công việc vẫn khóa.
- **Kiểm tra:** xem mục 14 và `Captures/VERIFICATION_WAVE_BIKES.md`; ảnh `wave-blender-*.png`, `wave-bikes-*.png`.
- **Trạng thái:** source/asset/scene đã lưu trong working tree; **chưa commit, chưa build WebGL, chưa push/Pages**. Bản chơi công khai vẫn 1.30.6 với xe v2. Khi phát hành phải tăng version theo quy trình 15.2 và cập nhật mục này.
- **Không đụng:** các `.blend` cục bộ có sẵn (Cars, Jump, Motorcycles, Passengers, Streets) và file chưa track (`PhoViet_Anhsangngaydem.blend`, `Phoviet_Suongmu.blend`, backup).


### 21.6. Dựng lại xe máy theo ảnh Wave đỏ (23/10/2026, Claude Code — nguồn thử nghiệm cũ)

- **Yêu cầu người dùng:** bản Wave ở 21.5 "hoàn toàn không giống" 5 ảnh mới (Wave đỏ: ngang trái, trước, sau, 3/4 trước trái, 3/4 sau phải). Người dùng muốn dựng lại từ đầu trong file mới, không dựa vào thiết kế cũ.
  - Đã chốt: **xe chính màu trắng** (giữ yêu cầu cũ), dáng y ảnh.
  - Chỉ dựng lại **xe**; giữ người lái áo đỏ sao vàng cùng rig/clip.
  - **Gửi ảnh render cho người dùng duyệt trước khi đưa vào Unity.**
- **Ảnh mẫu:** `ArtSource/Reference/WaveAlpha/wave_{side_left,front,rear,front34_left,rear34_right}.png`.
  - Tỉ lệ ảnh ngang: trục trước tại px (217,742), mặt đất v=958, 1 px = 1,557 mm, khớp hai trục bánh game `(0,.34,±)`. Ảnh trước: tâm u=722, đất v=1030, 1 px = 1,463 mm.
  - Phát hiện quan trọng: đầu đèn thật nằm ở z≈0,56. Bản 21.5 đặt đèn ở z 0,80, lệch ra trước khoảng 25 cm. Tay nắm thật nằm ở khoảng z 0,11–0,20, x ±0,28–0,42.
- **Nguồn mới:** `ArtSource/Blender/pho_viet_wave_alpha.py`, viết mới hoàn toàn.
  - Mỗi tấm vỏ là trường khoảng cách có dấu (SDF) dựng từ đường viền dò trên ảnh, gồm các hàm `P()`/`PF()` và các bảng `LEG`, `COWL`, `REAR_BODY`, `SEAT`, …
  - Phép giao tạo cạnh bo: `inter_round`. Chi tiết dán dạng da mỏng có kiểm tra hướng nhìn: `decal`.
  - Lưới hóa qua OpenVDB → Volume to Mesh, rồi decimate theo từng phần. Bánh căm, phuộc, giảm xóc, gương, tay lái là capsule/khối xoay.
  - Mỗi file `.vdb` tạm có tên duy nhất, vì Blender lưu cache lưới theo đường dẫn file. Lỗi này từng làm các lần sửa đầu không có tác dụng.
  - Hàm `build_body(colors)` dựng xe; `review(dir)` render ảnh ngang/trước/sau/3/4 trùng khung ảnh mẫu ra `Captures/wa-*.png`.
- **Trạng thái:**
  - Đã dựng và đang tinh chỉnh trong Blender: khoảng 41k tam giác cho xe khi chưa giảm.
  - Còn phải làm: ghép người lái/rig/LOD/atlas và xuất FBX, neo đèn và nozzle mới, cập nhật `VerifyWaveBikes`, cài vào Unity, chạy hồi quy.
  - **Chưa gửi bản duyệt, chưa xuất FBX, chưa đổi asset game, chưa commit/build.**
  - File `.blend` mới sẽ là `PhoViet_Motorcycles_WaveAlpha.blend` khi lưu.
- Ngày 23/10 người dùng đã chọn mô hình Meshy đã gửi để thay toàn bộ xe máy trừ CSGT (mục 21.7). Không tiếp tục tự cài bản WaveAlpha vào scene; giữ nguyên nguồn/ảnh thử nghiệm để tránh mất công việc Claude Code.


### 21.7. Thay xe bằng mô hình Meshy đã nhận (23/10/2026, Codex)

- **Xác nhận nhận thành công:** `Assets/MeshyImports/Red Honda Scooter Front View_20261023_023822/Meshy_AI_Red_Honda_Scooter_Fro_1022193819_texture.fbx`, một mesh tĩnh **15.739 tam giác**, kèm `meshy_basecolor.png`, `meshy_normal.png`, `meshy_metallic_smoothness.png`. `Captures/meshy-bike-source-inspection.json` là kết quả kiểm tra Unity. Không sửa các file nguồn này.
- **Quyết định mới:** dùng đúng xe Meshy cho toàn bộ xe máy ngoài CSGT; hero trắng–đen, City xanh ngọc, Underbone xanh dương, Sport vàng, Racer cam. Tên semantic giữ để không phá tham chiếu; không có nghĩa còn dùng hình Wave/côn tay cũ. Người dùng đã xác nhận riêng **giữ mô tô CSGT hiện tại**. Công việc vẫn khóa cả code và scene.
- **Tạo tài sản:**
  - `ArtSource/Blender/prepare_meshy_bike_atlas.py`: Pillow/NumPy đổi riêng vùng đỏ trong basecolor sang màu mới, giữ đen/cao su/yên/kim loại và texture gốc. Atlas 2048²: phần xe chiếm 1920 hàng trên, bảng màu người lái 128 hàng dưới. UV xe `v=.0625+.9375*v`, UV người `v=.0625*v`. Surface map tương ứng, normal chung có dải người màu trung tính.
  - `ArtSource/Blender/integrate_meshy_bikes.py`: Blender chạy nền riêng, không điều khiển/chỉnh Blender người dùng đang mở. Chuẩn hóa chiều dài cơ sở 1,51 m, tâm bánh `(0,.34,.78)` / `(0,.34,-.73)`, nhận bánh từ cung lốp thấp để tránh nhầm lốc máy. Phân riêng Frame/Steering/hai bánh bằng trọng số cứng theo mặt, tránh kéo giãn các tam giác giữa bánh và thân. Giữ hình người/17 xương/7 action cũ; căn phần cuối cẳng tay vào tay lái mới, không đổi chân/quỹ đạo giày đạp.
  - Tạo `Meshy_Emission.png` từ UV các mặt đèn thật trong không gian xe: 128 mặt pha, 66 mặt hậu; pha trắng/hậu đỏ, phần thân/người đen. Standard emission được `DayNightLighting` điều khiển cùng danh sách vật liệu mới. Không dùng mask 128px Wave trên UV Meshy.
  - Tạo 3 LOD mỗi mẫu: LOD0 khoảng 28,3–30,4k, LOD1 khoảng 11k, LOD2 khoảng 5k tam giác. Một material/skin, chất lượng Bone2. Xuất mới trong `Assets/PhoViet/Art/Bikes/Meshy/`, **không ghi đè** FBX/palette Wave cũ. File `.blend` độc lập `PhoViet_MeshyMotorcycles.blend` chứa 5 rig; script/JSON cho phép tái tạo. LOD2 vẫn dưới 6k, không thay ngân sách đèn/gameplay.
- **Tích hợp Unity:**
  - `PhoVietBikeUpgrade.ModelPath` ưu tiên `Bikes/Meshy/{asset}.fbx`, fallback đường cũ (CSGT không có bản Meshy). Atlas mới import Bilinear/Clamp/mipmap/max2048; surface/normal tuyến tính, màu sRGB, Standard shader dùng metallic/smoothness và normal. Palette cũ vẫn Point/128/không mip.
  - `Assets/PhoViet/Editor/PhoVietMeshyBikeUpgrade.cs`: sao lưu scene trước cài vào `.tools/PhoViet-before-meshy*.unity`; giữ root physics/pool và role, thay **32 actor gồm 14 xe đậu nguồn**; bake lại 3 `Generated/RouteParked_*.asset` giữ GUID và cập nhật material cho **56 xe đậu nhánh**. Không thay visual/clip/material/đèn CSGT, có đối chiếu trước–sau khi cài.
  - Neo pha/hậu theo metadata trên Steering/Frame; điểm pha khoảng `(0,1.25,.573413)`, hậu `(0,.995,-.830376)`. Neo `PV_MeshyExhaust` khoảng `(.23,.40,-1.0)` trên Frame để boost đi theo nhảy/ngã. `BoostSmokeVisual` tìm socket này, giữ fallback Wave cho scene cũ. Cài lại đèn cũng đọc metadata; không hardcode tọa độ Wave cho Meshy.
  - Làm mới `DayNightLighting.emissionUsers`, giữ đủ 30 binding đèn xe và không có head/halo null. Giữ xe bốn bánh, người lái CSGT, gesture/Q/Space/W, va chạm, công việc đang tắt và dữ liệu Coin/kỷ lục.
- **Kiểm tra cuối:** **15 suite / 4,572 điều kiện đạt**, có thêm `VerifyMeshyLighting` (16) cho mask/đèn/nozzle; ảnh Unity ngày/đêm đã xem. `AgentScripts/VerifyMeshyBikes.cs` kiểm provenance/5 màu/atlas/normal/surface, 33 actor/3 LOD/17 xương/7 clip, tất cả xe đậu nhánh/material, CSGT cũ và neo đèn. Bộ hồi quy và kết quả cuối xem `Captures/VERIFICATION_MESHY_BIKES.md`, `Captures/meshy-bikes-Verify*-results.json`. `CaptureBikesMeshy.cs` chụp hero trước/sau, màu giao thông, quái xế, CSGT, ban ngày/đêm trong Unity.
- **Trạng thái phát hành:** **đã build/commit/push/phát hành WebGL 1.31.0 và xác minh live** theo yêu cầu trực tiếp ngày 23/10. Source/artifact `6a0266098bf8fd80cbd6e7b05896ddf04079c3f9`, Pages run `39851261543`, cache `4f9dbd6172a2b7df`; 26 live checks đạt, 0 lỗi runtime. Xem 21.8. Không cần hỏi lại quyền cho cùng lần phát hành đã được cấp. Giữ nguyên mọi file Wave/WaveAlpha và thay đổi cục bộ có sẵn của Claude Code; không gộp chúng vào một commit rộng ngoài phạm vi.


### 21.8. Build WebGL và phát hành bộ xe Meshy 1.31.0 (23/10/2026)

- **Ủy quyền:** người dùng yêu cầu trực tiếp “Build WebGL và phát hành lên web”; gồm build, kiểm tra, commit, push main và phát hành GitHub Pages. Không hỏi lại phê duyệt cho cùng lần phát hành này.
- **Cấu hình:** nâng `PhoVietWebBuild.Configure`, `ConfigureWebRelease`, `PlayerSettings.bundleVersion` và các kỳ vọng version trong VerifyWeb/FullscreenGame/CrashPoliceWeb/MorningFogWeb lên **1.31.0**. Giữ Gzip fallback, WASM một luồng, bộ nhớ 128–512 MB, PWA/cache theo hash nội dung và toàn bộ cấu hình mobile. README thêm bộ xe mới; không bật công việc.
- **Preflight Unity:** `AgentScripts/VerifyMeshyRelease.cs` kiểm đúng scene, 33 bike actors, chỉ một CSGT nguồn cũ, không thiếu script, mọi xe ngoài CSGT trỏ Meshy FBX, đèn đầy đủ, jobs=false. Kết quả `Captures/meshy-release-preflight.json`; native trước phát hành đã đạt 15 suite / 4.572 điều kiện (21.7).
- **Build cuối:** `build_1e00ac19ccba` **Succeeded**, 30,275 ms, 0 lỗi / 1 warning Pipeline thiếu RuntimePipelineConfig đã biết (công cụ Editor không chạy trong player). Build đầu `build_ec0562990b16` cũng thành công nhưng được thay bằng bản cuối sau khi phát hiện một xe mẫu Meshy rời ngoài game root. `VerifyMeshyRelease` giữ xe mẫu trong Editor, gán `EditorOnly`, lưu scene rồi build lại; báo cáo packedAssets bản cuối xác nhận **0 asset MeshyImports gốc** bị đưa vào runtime. Đây là loại bản xem mẫu khỏi build, không xóa file/xe mẫu của người dùng.
  - Output cuối đã copy sang `docs`, chỉ dọn các hash cũ trong `docs/Build` sau khi kiểm tra đường dẫn tuyệt đối. Cache **`4f9dbd6172a2b7df`**, build UTC `2026-10-22T20:34:46.3473513Z`, tài nguyên khoảng 52,45 MB; data gzip khoảng 45,21 MB.
  - `Captures/meshy-release-build-summary.json`, `meshy-release-preflight.json`, `meshy-release-bundle-checks.json`: 28 kiểm tra static bundle gồm gzip/WASM/data header, SHA cache, offline list, không file quá 100 MB, không hash cũ.
- **Kiểm tra local cuối:** **83 kiểm tra trình duyệt đạt** = VerifyWeb/PWA/offline 37, so tài nguyên + WASM thật 26, fullscreen iPhone/iPad dọc-ngang 20; **0 lỗi runtime**. Đã xem ảnh desktop/portrait/landscape/offline của đúng cache `4f9dbd6172a2b7df`. Bằng chứng `Captures/meshy-release-web-results.json`, `meshy-local-artifact-results.json`, `meshy-release-fullscreen-results.json`, `meshy-release-web-*.png`. Đây là giả lập browser, chưa thử phần cứng điện thoại thật.
- **Push/Pages/live đã hoàn tất:** commit **`6a0266098bf8fd80cbd6e7b05896ddf04079c3f9`** đã push main, [Pages run 39851261543](https://github.com/denispham1107/taixecongnghe/actions/runs/39851261543) completed/success đúng SHA. **26 live checks đạt**, version/cache và toàn bộ tài nguyên trên [https://denispham1107.github.io/taixecongnghe/](https://denispham1107.github.io/taixecongnghe/) khớp docs đã kiểm tra; WASM thật/start/pause hoạt động, **0 lỗi runtime**. Bằng chứng `Captures/meshy-pages-release.json`, `meshy-live-results.json`, `meshy-live-game.png`, `VERIFICATION_MESHY_WEB_RELEASE.md`.
- **PWA:** nếu ứng dụng/tab cũ còn chạy, đóng hết rồi mở lại để worker mới kích hoạt; không xóa Coin/kỷ lục hay bắt người dùng xóa toàn bộ dữ liệu. Giữ nguyên giới hạn: browser profile giả lập, chưa đo thiết bị vật lý. Báo cáo phát hành được commit/push riêng sau source/artifact; không rebuild cho tài liệu đơn thuần.
- **Giới hạn phạm vi Git:** commit bộ Meshy mới, scene/mesh xe đậu, mã tích hợp/đèn/boost, cấu hình phiên bản, tài liệu và docs build tương ứng. Giữ các file Wave/WaveAlpha, .blend cũ, Meshy Bridge/Packages cục bộ chưa liên quan ngoài staging; không reset/xóa công việc Claude Code. Các FBX Meshy cuối đã chứa người lái/rig nên build không phụ thuộc việc xuất lại Wave.

### 21.9. Người Meshy có rig Golden Star Stance (23/10/2026)

- Người dùng thay yêu cầu: bỏ mẫu Vietnam Star Streetwear chưa rig và bản ghép thử; dùng mẫu mới có rig cho mọi người cưỡi xe ngoài CSGT. **Giữ nguyên cả người CSGT và mô tô tuần tra** theo xác nhận riêng, không tự thay đồng phục.
- Nguồn hiện hành `Assets/MeshyImports/Golden Star Stance_20261023_150430/Meshy_AI_Golden_Star_Stance_biped_Character_output.fbx`. Bản `_150426` là cùng mẫu kèm animation Running, không dùng clip chạy bộ để lái xe. Inspection Blender ghi 27.200 tam giác/29.791 vertex (vertex split do UV), rig smartrig gồm các chi/ngón tay; số trên giao diện Meshy khác số importer sau bỏ mặt suy biến. Bằng chứng `Captures/meshy-rigged-source-inspection.json`.
- `integrate_meshy_riders.py`: dùng bind geometry và trọng số da của rig nguồn, gom về 13 xương người semantic + 4 xương xe. Chuyển bind T-pose sang tư thế lái, căn cổ tay/ngón vào tay nắm, giữ 7 clip Idle/Ride/KickLeft/KickRight/FallLeft/FallRight/Recover của game. Runtime tham chiếu trực tiếp AnimationClip đã kiểm tra trong `Bikes/Meshy/{asset}.fbx` thay vì clip re-bake từ FBX mới, tránh biến đổi vòng lặp Euler/quaternion qua hai lần xuất. Do đó thư mục Meshy cũ là dependency clip/nguồn xe; không xóa cả thư mục này vì tưởng còn người cũ đang hiển thị. Không dùng cách đo khoảng cách capsule để rig lại người mới. Giữ nguyên 15.739 tam giác xe Meshy, đèn/nozzle và mọi root gameplay. Blender chạy nền độc lập, không phải phiên MCP trực tiếp.
- Xuất 5 FBX mới vào `Assets/PhoViet/Art/Bikes/MeshyRiders/` và nguồn `ArtSource/Blender/PhoViet_MeshyRiders.blend`. LOD0 42.939 tam giác; LOD1 khoảng 11k; LOD2 khoảng 5k. 17 xương, 7 clip, Bone2, một material mỗi skin. Nguồn rig gốc giữ riêng để tái tạo; không phá xe CSGT.
- `prepare_meshy_rider_atlas.py` + `MeshyRider_ShirtMask.png`: atlas2048×4096 từ bản sửa1.32.1 (1.32.0:2048²), UV xe ở nửa trên v[.5,1], người ở nửa dưới v[0,.5]. Hero giữ áo đỏ/sao ngực texture gốc, thêm sao lưng ở21.13; City xanh, Underbone kem, Sport tím, Racer xám. Mask lấy theo weights Torso/Pelvis/UpperArm và nới biên UV 9px để không sót mảng đỏ ở vai. Chỉ đổi vùng áo, xóa sao trên áo NPC bằng mask/inpaint; 11.250 pixel mask trước resize. Normal/surface/emission dùng chung 5 mẫu; vùng người emission đen.
- `PhoVietBikeUpgrade.ModelPath` ưu tiên thư mục mới, `IsMeshyPath` hỗ trợ cả hai thế hệ. AtlasMaterial nạp shared Meshy_Surface; `PhoVietLightingUpgrade` tìm mask trong cùng thư mục. `PhoVietMeshyBikeUpgrade.Install` thay 32 actor (14 xe đậu nguồn), bake 3 mesh Generated giữ GUID cho 56 xe đậu nhánh; loại vật liệu thế hệ trước khỏi emissionMaterials, gắn lại18 bộ đèn và kiểm CSGT trước/sau. Backup scene trong `.tools/PhoViet-before-meshy-rider*.unity`.
- `CleanupMeshyRiders.cs` đã xóa root và toàn bộ folder import **Vietnam Star Streetwear_20261023_104420** chưa rig bằng AssetDatabase.DeleteAsset sau kiểm không còn skin dùng. Năm asset thử chưa rig trong MeshyRiders được xuất đè bằng mẫu có rig mới; scene không dùng lại chúng. Ba root nguồn Meshy khác giữ để tham chiếu Editor, gán EditorOnly và tắt active để không có người T-pose/Running đứng lẫn trong Play Mode. Các nguồn lịch sử xe/CSGT/clip đã commit và file cục bộ Claude không bị xóa.
- Ảnh kiểm tra Unity: `Captures/meshy-riders-hero-front.png`, `hero-rear`, `racer`, `police`, `game`, `night`, `night-hero`, `pose-KickLeft/KickRight/FallLeft/FallRight`; đã chụp lại sau sửa mask áo và dùng clip gốc. **10 suite /3.308 điều kiện đạt** (art, đạp nhanh/giao lộ, nhảy/nóc xe, boost, đèn, chế độ quãng đường), cùng5 kiểm tra màu atlas. Bộ kiểm tra và kết quả cuối xem `Captures/VERIFICATION_MESHY_RIDERS.md`; không dùng bằng chứng của bản chưa rig thay cho mẫu có rig.
- Trạng thái: **đã cài/lưu scene và phát hành WebGL1.32.0** (21.14). Công việc vẫn khóa cả code lẫn scene, dữ liệu Coin/kỷ lục giữ nguyên. Chỉ nâng phiên bản/build khi có yêu cầu phát hành tiếp theo.

### 21.10. Sửa vai và bàn tay nắm tay lái v2 (23/10/2026; ngón tay được chỉnh tiếp ở21.11)

- Người dùng phản hồi ảnh21.9: vai nhún cao, cẳng tay gác lên tay lái, bàn tay chưa nắm tay nắm. Sửa trên cùng mẫu Golden Star Stance, không thay mặt/trang phục/xe, vẫn giữ CSGT và jobsfalse.
- `integrate_meshy_riders.py::targets/bind_person`: hạ vị trí vai từ1.492 xuống1.365m, khuỷu đặt `(±.305,1.195,.055)`, cổ tay `(±.395,1.261,.230)`; đầu hạ6,5cm để cổ không kéo dài sau hạ vai. Đỉnh torso/đáy cổ tới `(0,1.415,-.115)`; không nâng vai theo Head của rig cũ nữa.
- Giữ nhãn group nguồn từng vertex để nhận diện Thumb. Bốn ngón uốn theo tiết diện quanh tay nắm tại khoảng`(±.395,1.237,.315)`; offset độ dày đi theo pháp tuyến vòng cong để tránh cả bàn tay bẹt. Ngón cái có cung đối hướng ôm từ sau xuống dưới, không dùng cùng đường cong với bốn ngón. Lòng tay đi từ cổ tay phía sau đến mặt trên tay nắm; cẳng tay kết thúc trước tay nắm.
- Áp dụng cả5FBX/3LOD, rebake xe đậu và giữ7clip gốc/17xương semantic. Không thay tốc độ/gameplay/input. `CaptureMeshyRiders.cs` bổ sung ảnh cận bàn tay và chính diện vai để đánh giá hình ảnh; các assertion gameplay cũ không thay thế việc xem ảnh.
- **Đã lưu scene và kiểm tra:** 7 suite /938 điều kiện đạt (MeshyRiders356, BikeArt156, QuickKick62, JunctionKick216, Jump44, NightJumpLighting88, MeshyLighting16). `Captures/meshy-grip-verification-summary.json` và từng `meshy-grip-Verify*-results.json`. Installer thay32 actor/56xe đậu nhánh, CSGT nguyên trạng,18neođèn đủ, jobsfalse. Preflight không thiếu script/đèn. Đã xem ảnh Unity cận tay, chính diện vai và các pose đạp; ảnh cuối `meshy-grip-grip-close.png`, `meshy-grip-shoulders-front.png`, `meshy-grip-hero-front.png`, `meshy-grip-pose-*.png`.
- **Đã commit/build/push/phát hành1.32.0** (21.14); web hiện1.32.0. Các con số3.308 của21.9 là kiểm tra bản trước; kết quả938 ở đây được chạy lại trên bản sửa vai/tay cuối. Không tuyên bố đã kiểm thiết bị điện thoại thật.

### 21.11. Cuộn sát các đốt ngón quanh tay nắm v3 (23/10/2026; chiều dài ngón được sửa ở21.12)

- Người dùng chỉ ra ảnh cận v2 vẫn còn ngón thả xuống; các kết quả kiểm tra gameplay ở21.10 không chứng minh tư thế nắm đã đạt. Giữ vai/cổ tay và toàn bộ mẫu, chỉ sửa trường biến dạng bàn tay.
- `integrate_meshy_riders.py::bind_person`: thay chuỗi nội suy hở bằng cung tròn quanh trục tay nắm. Tâm tiết diệnY/Z`(1.237,.315)`, bán kính đường giữa`.033m`; đốt xa theo chiều dài nguồn`t=.08→.20m` cuộn tới`1.65π`(297°), đưa đầu ngón vòng về sau tay nắm. Độ dày theo pháp tuyến chặn±.007m để mặt trong ôm cao su, tránh ngón bẹt hoặc xuyên sâu. Phần lòng tay nối mềm từ cổ tay tới điểm đầu cung.
- Ngón cái dùng cung đối hướng từ−90°đến−234°, đi quanh mặt dưới và kẹp đối diện bốn ngón. Giữ mirror chiều rộng hai tay, weights nguồn/17xương/7clip/LOD, màu áo/xe và CSGT. Metadata`ridingPose=relaxed-shoulders-closed-grip-v3`.
- Xuất lại cả5FBX và nguồn blend, cài/bake các actor/xe đậu như21.9. `CaptureMeshyRiders.cs` thêm ảnh cận tay trái bên cạnh ảnh cận tay phải, đánh giá bằng ảnh thật trong Unity.
- **Đã lưu scene và kiểm tra:** VerifyMeshyRiders356 + VerifyBikeArt156 = **512 điều kiện đạt** trên v3, gồm cả clip đạp trái/phải/ngã. Không tính lại938 của bản v2 như kiểm tra mới. `Captures/meshy-finger-roll-summary.json`, `meshy-finger-roll-Verify*-results.json`, installer/preflight đều success;32actor/56xe đậu nhánh đã thay, CSGT nguyên trạng,18neođèn đủ, không thiếu script và jobsfalse. Đã xem ảnh cận cả hai tay trong Unity: `Captures/meshy-finger-roll-grip-close.png` (phải), `meshy-finger-roll-grip-left-close.png` (trái). `git diff --check` đạt.
- **Đã commit/build/push/phát hành1.32.0** (21.14); web hiện1.32.0, chưa kiểm phần cứng mobile cho bản sửa ngón này.

### 21.12. Rút ngắn ngón tay, giữ tư thế nắm v4 (23/10/2026)

- Người dùng xác nhận tư thế nắm v3 đã đúng nhưng ngón cả hai tay quá dài. Chỉ rút đoạn cuộn của các đốt xa, giữ nguyên lòng tay/cổ tay/vai và vị trí/tâm/bán kính tay nắm; không scale toàn bàn tay.
- `integrate_meshy_riders.py`: `FINGER_ARC_SCALE=.80`, giảm20% chiều dài cung. Bốn ngón từ tối đa297°còn237,6°; ngón cái từ144°còn115,2°, vẫn cuộn đối hướng. Tâm/radius.033m và thickness±.007m giữ nguyên. Các bản thử32% làm dáng nắm bị hở hoặc kéo lòng tay nên không dùng; bản cuối giữ gốc ngón và kích thước lòng tay của v3. Metadata`ridingPose=relaxed-shoulders-closed-grip-v4`, `fingerArcScale=.8`.
- Xuất lại5FBX/3LOD/blend và cài/rebake32actor/56xe đậu nhánh; CSGT và jobsfalse giữ nguyên. **Đã lưu scene** và xem ảnh cận cả hai tay trên bản v4 cuối: `Captures/meshy-short-fingers-grip-close.png`, `meshy-short-fingers-grip-left-close.png`. VerifyMeshyRiders356 + VerifyBikeArt156 = **512 điều kiện đạt chạy lại cho v4**. Installer/captures/preflight success, CSGT nguyên trạng, không thiếu script/đèn, jobsfalse; `git diff --check` đạt. Bằng chứng `meshy-short-fingers-summary.json` và các JSON cùng tiền tố. **Đã commit/build/push/phát hành1.32.0** (21.14), web hiện1.32.0; chưa thử phần cứng điện thoại thật.

### 21.13. Sao vàng giữa lưng áo nhân vật chính (23/10/2026)

- Thêm đúng một ngôi sao vàng5cánh vào chính giữa lưng áo đỏ của hero, không thêm cho NPC. Giữ sao ngực và tư thế nắm/ngón v4.
- `ArtSource/Blender/bake_hero_back_star.py`: chạy Blender nền, đọc hình học/UV source Golden Star Stance rồi chiếu polygon10đỉnh lên mặt lưng; tâm đứng`(X=0,Y=1.18m)`, R=.125m, r/R=.381966. Chỉ mặtZ<−.02 được nhận, tránh in xuyên qua ngực. Xuất`ArtSource/Blender/MeshyHero_BackStarMask.png`2048², số pixel mask gốc ghi tại`meshy-back-star-mask.json`; không sửa FBX/texture import nguồn.
- `prepare_meshy_rider_atlas.py`: dùng mask riêng cho Hero, viền mềm.45pixel nguồn, vàngRGB(1,.82,.09) theo độ sáng vải gốc để giữ nếp áo. Bản hero palette atlas hiện hành được cập nhật; shader/normal/emission và geometry/rig/clip giữ nguyên. Cả3LOD dùng cùng atlas nên đều có sao. Muốn tái tạo: Blender bake_hero_back_star.py trước, rồi Python prepare_meshy_rider_atlas.py. Metadata source.json ghiheroBackStarAdded=true/heroFrontTexturePreserved=true; không còn khẳng định toàn texture Hero giữ nguyên.
- Đã đối chiếu hash: mọi palette NPC và3sharedmap giữ nguyên; nửa atlas xe của hero cũng nguyên. Kiểm tra`Captures/meshy-back-star-texture-checks.json`. Đã xem ảnh Unity trước/sau và góc chơi, sao5cánh rõ và ở giữa lưng: `Captures/meshy-back-star-hero-rear.png`, `meshy-back-star-game.png`; sao ngực vẫn giữ. Captures/preflight success, scene33actor/1CSGT, không thiếu script/đèn, jobsfalse. Đây là đổi texture; kiểm hash/ảnh và preflight, không chạy lại toàn bộ suite gameplay hoặc xuất lại FBX. **Đã commit/build/push/phát hành1.32.0** (21.14), web hiện1.32.0.

### 21.14. Phát hành bộ nhân vật 1.32.0 (23/10/2026)

- Người dùng yêu cầu trực tiếp “build và phát hành lên web”; đã ủy quyền build, kiểm tra, commit/push main và deploy Pages. Không hỏi lại quyền cùng lần phát hành.
- Gồm Golden Star Stance có rig,5màu áo, vai/cổ/tay nắm/ngón v4 và sao giữa lưng hero (21.9–21.13); CSGT nguyên trạng, công việc khóa, dữ liệu Coin/kỷ lục giữ nguyên.
- Cấu hình/PlayerSettings và kỳ vọng web được nâng1.32.0, giữ toàn bộ PWA/mobile/DPR/gzip/WASM một luồng. **15suite/4.588điều kiện native đạt**. Build`build_636fb2e3e84d`Succeeded, 646,526ms,0lỗi/1warning Pipeline thiếu RuntimePipelineConfig đã biết. Tổng khoảng47,15MB; báo cáo packedAssets xác nhận0asset nguồn MeshyImports vào runtime. Output cuối đã copydocs, dọn4hash cũ đúng thư mụcdocs/Build.
- Cache`96d0c318c468dcab`, UTC`2026-10-23T11:33:20.5868417Z`. `AgentScripts/VerifyWebBundle.py` đạt29kiểm tra bundle/SHA/header gzip/WASM/data/4fileBuild/giới hạn100MB. Local browser **83kiểm tra đạt**: PWA/offline37, khớp tài nguyên+WASM26, fullscreen iPhone/iPad dọc-ngang20,0lỗi runtime. Đã xem ảnh desktop và mobile dọc; ảnh cuối `Captures/riders-release-desktop.png`, `riders-release-portrait.png`, `riders-release-landscape.png`, `riders-release-offline.png`. Đây là giả lập browser, chưa kiểm phần cứng điện thoại thật.
- **Đã commit/push/phát hành và xác minh live**: source/artifact`c50614413f412f9f70150412e651494a4c2400b8`, [Pages run39948530264](https://github.com/denispham1107/taixecongnghe/actions/runs/39948530264) completed/success đúng SHA. **26kiểm tra live đạt**,0lỗi runtime; version1.32.0/cache`96d0c318c468dcab`và từng tài nguyên trùng artifact local (binary từngbyte, text chuẩn hóaCRLF). WASM thật khởi tạo/start/pause hoạt động. Đã xem ảnh live`Captures/riders-live-game.png`. Bằng chứng`riders-pages-release.json`, `riders-live-results.json`, `VERIFICATION_RIDERS_WEB_RELEASE.md`.
- Nếu PWA còn giữ bản cũ, đóng các tab/app Phố Việt rồi mở lại để worker mới kích hoạt; giữ nguyên dữ liệu người chơi, không yêu cầu xóa Coin/kỷ lục. Báo cáo được commit/push riêng sau source/artifact; không rebuild chỉ cho tài liệu.
- Giữ ngoài staging các file Wave/WaveAlpha,5blend cục bộ cũ, Bridge plugin/Packages chưa liên quan. Commit model mới/source rig/texture/scene/Generated/mã cài và bản WebGL tương ứng. Không reset công việc Claude.


### 21.15. Sửa hình ảnh mobile nhòe — 1.32.1 (23/10/2026)

- Người dùng yêu cầu sửa chất lượng mobile. Cấu hình cũ giới hạn DPR 1,5; atlas 2048² ép mỗi nguồn 2K còn 2048×1024. Người chơi cũng có thể xuống LOD xa sau mở rộng góc nhìn.
- `app.js::gamePixelRatio`: mật độ gốc tối đa 3 trên touch/iOS, framebuffer tối đa 3.200.000 pixel; desktop tối đa 2 / 8.300.000 pixel. Cập nhật `Module.devicePixelRatio` theo diện tích canvas sau resize/xoay/fullscreen. Không đổi camera, zoom, HUD hay cử chỉ.
- `prepare_meshy_rider_atlas.py`: atlas chữ nhật **2048×4096**, hai tile **2048² đầy đủ** từ nguồn xe/người, không kéo từ atlas cũ. Giữ UV hai nửa, FBX/rig/collision và một material mỗi skin. Giữ màu xe/áo, sao ngực/lưng hero, NPC không sao; normal/surface/emission dùng chung. Tổng texel tăng gấp đôi, không gấp bốn như atlas 4096².
- `PhoVietBikeUpgrade.ImportTexture`: riêng MeshyRiders dùng max 4096, Trilinear/aniso 4, mipmaps và CompressedHQ/quality 100. `UpgradeMobileClarity.cs` cập nhật material/texture tại chỗ, giữ GUID; không cài lại actor/scene/CSGT.
- `BikeVisualAnimator.Initialize`: chỉ Player khóa LOD0, 42.939 tam giác nguồn; Unity nhập 42.917 sau bỏ mặt suy biến. NPC/xe đậu vẫn có ba LOD. `PhoVietWebRuntime` dùng MSAA 4 thay 2 trên mobile; bóng thấp giữ để bù tải. Công việc vẫn khóa, dữ liệu Coin/kỷ lục và logic chơi không đổi. Nguồn Blender/FBX không cần xuất lại cho thay atlas/render này.
- **Kiểm tra native:** 4 suite / **643 điều kiện**: MeshyRiders 356, BikeArt 156, NightJumpLighting 88, MobileClarity 43. Thêm **14 kiểm tra pixel nguồn/atlas/sao**, **433 kiểm tra công thức DPR**. Native audit xác nhận đủ 8 texture 2048×4096/13 mip, không giữ bản CPU readable. Thống kê bộ nhớ Editor không phải đo GPU điện thoại. Phép kiểm ban đầu đòi đúng số tam giác Blender đã được sửa để tính việc Unity bỏ mặt suy biến; không thay mesh để làm test qua.
- **Build:** `build_2d0ffed36b84` Succeeded, **293.373 ms**, 0 lỗi / 3 warning đã biết: Pipeline thiếu config runtime và hai field touch cũ không đọc ở nhánh DOM. Artifact **57.336.107 byte**, cache **`45680c1d5a472f7c`**, UTC `2026-10-23T15:20:09.2340891Z`; **29 kiểm tra bundle** đạt.
- **Browser local:** **77 kiểm tra** đạt = DPR thật 14 + PWA/offline 37 + tài nguyên/WASM 26, 0 lỗi runtime. iPhone CSS 390×844 / DPR3: cũ 585×1266, mới **1170×2532**; ngang **2532×1170**. Tablet CSS1024×768 → 2066×1549; xoay lại phục hồi đúng 1170×2532. Đã xem ảnh cảnh chơi không bị thẻ pause che nhân vật: `mobile-clarity-after-portrait.png`, `mobile-clarity-after-landscape.png`. `VerifyMobileClarity.cjs` chạy DPR3 thật trong browser; các bộ layout cũ dùng DPR1 không thay thế kiểm tra này. **Chưa thử thiết bị vật lý/FPS/nhiệt độ.**
- **Đã commit/push/phát hành và xác minh live:** source/artifact **`557d540b5fbb9bc0ca5eb3da2a2fb8ef519f0e95`**, [Pages run 39987470876](https://github.com/denispham1107/taixecongnghe/actions/runs/39987470876) completed/success đúng SHA. **40 kiểm tra live đạt** (26 khớp tài nguyên/WASM + 14 độ nét DPR3 dọc/ngang/tablet/xoay lại), **0 lỗi runtime**. Version 1.32.1/cache `45680c1d5a472f7c` khớp docs đã kiểm tra; binary từng byte, text chuẩn hóa CRLF.
- **Bàn giao:** `Captures/mobile-clarity-verification-summary.json`, `mobile-clarity-pages-release.json`, các JSON `mobile-clarity-*`, `VERIFICATION_MOBILE_CLARITY.md`. Native suite/ảnh local và ảnh live dùng cùng artifact. Nếu PWA còn dùng bản cũ, đóng hết tab/app rồi mở lại cho worker mới kích hoạt; không xóa dữ liệu người chơi. Unity đã về Edit Mode, server kiểm tra docs đã dừng. Báo cáo phát hành commit/push riêng, không rebuild tài liệu. Mọi thay đổi Wave/Blender/Packages có sẵn vẫn ngoài staging. Chưa thử phần cứng điện thoại thật.



### 21.16. Sửa đuôi/bánh xe nhấp nháy — 1.32.2 (24/10/2026)

- Người dùng báo phần đuôi xe chớp khi di chuyển ở mobile dọc/ngang. Đã tái hiện ở 8 góc quay: mặt chắn bùn và các mảng vỏ gần lốp bị gắn nhầm `WheelRear`, tạo mảnh đen ló ra khi quay. Phép chọn cũ dựa vào tâm mặt trong vùng bánh, không xét hết các đỉnh; mẫu AI còn có vùng lốp/chắn bùn dính mặt. Không phải đèn phanh hay thiếu DPR.
- `ArtSource/Blender/repair_meshy_wheels.py`: chạy Blender nền độc lập, giữ mọi vị trí/UV/normal góc/mặt và tên mesh, chỉ sửa skin weights đủ 5 mẫu ngoài CSGT / cả 3 LOD. Chỉ phần mâm/hub nằm trọn trong bán kính **.245 m** mới quay; rộng tối đa **.09 m trước / .065 m sau**. Lốp/đường bao/chắn bùn cố định theo Frame ở sau, Steering ở trước; vẫn đi cùng nhún/đánh lái/nhảy/ngã. Không xóa mesh, không hạ texture hay vô hiệu hóa góc quay WheelAngle. Xử lý cả mặt có trọng số trộn sau decimate để không còn mảng vỏ bị kéo theo một phần.
- Cách thử chỉ chặn ngoài bán kính .344 m vẫn nhận sót các mặt dính nên đã bỏ; bản cuối ổn định đường bao lốp, phần mâm vẫn quay. Metadata `wheelBinding=stable-rim-only-v1`. `integrate_meshy_riders.py` gọi cùng helper cho LOD0 và sau decimate LOD1/2 để tái tạo không quay lại lỗi. Nguồn `PhoViet_MeshyRiders.blend` lưu lại cả 5 rig tương ứng. Không sửa nguồn MeshyImports, clip runtime cũ, Wave hoặc bộ CSGT.
- `AgentScripts/CaptureRearWheel.cs`: lấy mesh đã BakeMesh và render proxy riêng cho từng góc, tránh GPU skin cache dùng lại hình trong cùng frame. Bằng chứng `rear-wheel-before-sheet.png` / `rear-wheel-after-sheet.png`: đã xem đủ 8 góc, bản cuối hết các mảng đen bắn ra. `VerifyWheelStability.cs` đã kiểm tra bánh trước/sau, 5 mẫu, 3 LOD qua nhiều góc; vừa đo mâm quay thật vừa đo đường bao đứng yên, không chỉ kiểm số lượng bone.
- Giữ toàn bộ sửa độ nét 1.32.1: atlas2048×4096, hai vùng2K, DPR3/3,2triệu pixel, LOD0 player/MSAA4. Công việc vẫn khóa, Coin/kỷ lục và mọi logic điều khiển/va chạm giữ nguyên. Xe đậu bake không phải xuất lại vì dáng rest/UV không đổi; scene không được dựng lại.
- **Kiểm tra native:** 8 suite / **1.022 điều kiện đạt**: MeshyRiders356, WheelStability242, BikeArt156, QuickKick62, Jump44, NightJumpLighting88, MobileClarity43, DistanceMode31. WheelStability đo đủ30 trường hợp (5 mẫu ×3LOD ×2bánh), mỗi trường hợp6góc; sai lệch đường bao lớn nhất0m, mâm vẫn chuyển động. Phép đo BakeMesh dùng scale để đúng với NPC/xe đậu scale.86; chạy trong phiên Play Mode sạch sau khi Editor recompile. Bằng chứng `rear-wheel-verification-summary.json` và các JSON cùng tiền tố.
- **Giữ dáng:** so ảnh góc quay0 trước/sau cùng camera cho silhouetteIoU=1,0; chênh lệch pixel trung bình0,00103/255, từ sai số xuất/normal rất nhỏ. Toàn bộ mặt vẫn giữ, không giảm chi tiết hoặc xóa phần đuôi để che lỗi.
- **Bảo toàn clip nguồn:** FBX import vào Blender tạo Actions nhưng không tạo lại NLA strips. Helper tái dựng đủ bảy strip trước export để không mất các take Idle/Ride/KickLeft/KickRight/FallLeft/FallRight/Recover. `AgentScripts/RefreshWheelAssets.cs` reimport đồng bộ, kiểm tra đủ bảy clip Legacy ở từng FBX và chuẩn hóa tên/loop. Đã xác nhận cả năm asset; sau đó chạy lại toàn bộ 1.022 điều kiện native và chụp ảnh trên đúng bộ FBX cuối. Clip runtime đã kiểm định vẫn giữ nguyên.
- **Build cuối:** `build_692689eb6e6a` Succeeded, **29.500 ms**, 0 lỗi / 1 warning Pipeline thiếu config runtime đã biết. Output 1.32.2/cache **`441262423309bccc`**, UTC `2026-10-23T19:03:08.0950663Z`, tổng output 57.353.820 byte; **29 kiểm tra bundle đạt**, tài nguyên precache 57.352.271 byte. Các build thử trước đó đã loại, không publish. Bản cuối đã xác nhận Edit Mode/preflight 1.32.2 trước build.
- **Browser local trên artifact cuối:** 40 kiểm tra đạt = DPR3 dọc/ngang/tablet/xoay lại 14 + đối chiếu từng tài nguyên/khởi tạo WASM/start/pause 26; không lỗi runtime. Ảnh `rear-wheel-mobile-portrait.png`, `rear-wheel-mobile-landscape.png`; report `rear-wheel-mobile-results.json`, `rear-wheel-local-results.json`. Bộ PWA/offline 37 đã chạy ở build thử trước khi bổ sung lại NLA nguồn; không cộng vào 40 kiểm tra cuối này.
- **Đã commit/push/phát hành:** source/artifact `8f06a86e3d015e601ca05339757f8b0d5ece6120`, [Pages run 40010075408](https://github.com/denispham1107/taixecongnghe/actions/runs/40010075408) completed/success đúng SHA. Bản công khai trả version 1.32.2/cache `441262423309bccc`; **26 kiểm tra live đạt, 0 lỗi runtime**: từng tài nguyên khớp artifact local (binary từng byte, text chuẩn hóa CRLF), WASM khởi tạo/start/pause thành công. Bằng chứng `rear-wheel-live-results.json` và `rear-wheel-pages-release.json`.
- **Bàn giao:** `Captures/VERIFICATION_REAR_WHEEL.md`, `rear-wheel-pages-release.json` và các JSON/ảnh cùng tiền tố. Unity về Edit Mode, server kiểm tra docs đã dừng. Giữ nguyên công việc Wave/Bridge/Packages và các nguồn Blender khác ngoài commit. Báo cáo phát hành commit/push riêng, không rebuild chỉ vì tài liệu. Chưa thử thiết bị điện thoại vật lý/FPS/nhiệt độ. Đóng hết tab/app rồi mở lại nếu PWA còn bản cũ; không xóa dữ liệu người chơi.


---

### 21.17. Nhận và tích hợp bốn mẫu ô tô Meshy (24/10/2026, scene đã lưu; phát hành xem 21.18)

- Đã nhận đủ bốn FBX và maps PBR 2048² trong `Assets/MeshyImports/`: `Covered cargo truck_20261024_022230` (13.481 tam giác), `Meshy_Model_20261024_022256` (thể thao, 16.488), `...022318` (bán tải, 20.575), `...022335` (đầu kéo container, 7.582). Giữ nguyên toàn bộ nguồn; kiểm nhận bằng Blender độc lập và JSON `meshy-cars-source-inspection.json`.
- Người dùng chọn **dùng cả bốn, giữ tỷ lệ và kích thước phù hợp**: thể thao thay sedan/hatchback/taxi, bán tải thay xe giao hàng; hai mẫu tải được chọn ngẫu nhiên mỗi lần spawn của tải thường và tải ngủ gật. Giữ 11 root/pool và sáu tên role để không đổi phân loại hành vi; `ModelAsset` biểu thị hình thực đang dùng. Không dùng tên role để suy ra còn hình xe cũ.
- `integrate_meshy_cars.py` chạy Blender nền, xuất vào `Assets/PhoViet/Art/Cars/Meshy/`, lưu nguồn mới `PhoViet_MeshyCars.blend`; không ghi đè file Cars/Blender cục bộ có trước. Hướng +Z, dài 4,45 / 4,2 / 6,4 / 12,8 m, scale đồng nhất giữ tỷ lệ nguồn. Fit các trục bánh từ lốp; rim quay riêng, đường bao AI giữ rigid để không chớp như lỗi xe máy. Mẫu thường 15 xương/4 bánh, container 23 xương/8 bánh. Bảy clip Legacy, ba LOD; LOD2 đều dưới 4k. Profile nóc lấy raycast trên mặt tam giác, tránh bịa lỗ trên tấm nóc container ít đỉnh.
- `prepare_meshy_car_textures.py` giữ nguyên ba map nguồn 2K cho mỗi mẫu, tạo mask chỉ vùng sơn và emission vùng đèn. `PhoVietMeshyCarPaint.shader` dùng Standard lighting/normal/metallic/smoothness, đổi sơn bằng `_PaintColor` qua MaterialPropertyBlock; không nhân màu cả lốp/kính/kim loại. Tám màu, chọn lại lúc xuất hiện, dùng chung vật liệu/texture giữa các slot, không tạo material mới mỗi spawn. Mask đã được chỉnh sau xem ảnh để bỏ lem kính xe tải; vùng bánh dùng nội suy tọa độ từng texel trên tam giác, tránh vừa nhuộm lốp vừa để sót mảng màu gốc ở hốc bánh. Hai nguồn tải không có texel đỏ trong vùng đèn hậu đã chọn: đèn hậu vẫn có hai halo quang học và đèn phanh động; không tô đỏ cả thùng xe để giả emission.
- `MeshyCarVariants.cs` đổi visual/animation/bánh, kích thước va chạm, profile nóc, neo đèn và đèn phanh cùng lúc. `InitializeDriver` dùng cho spawn thường/ngược/ngang/nguy hiểm và tái sử dụng. Nhánh reckless và sau rẽ chuẩn bị mẫu trước khi kiểm chỗ trống để dùng đúng chiều dài container; nếu chỗ bị chắn, giữ mẫu đã chuẩn bị tới lần thử sau. Lần khôi phục sau tai nạn không đổi màu/mẫu giữa lúc xe đang chạy.
- `PhoVietMeshyCarUpgrade.Install` thay đúng 11 actor, giữ root, CSGT/người chơi/xe máy, ghép lại 30 binding đèn và emission users. Preview Bridge được giữ ngoài runtime bằng EditorOnly/inactive. Sao lưu cả scene đĩa và scene bẩn vào `.tools` trước thay. Installer/menu xe lớn cũ chuyển sang installer mới khi asset Meshy có mặt; cài lại lighting cũng giữ material emission/neo mới. `PhoVietJump`/`PhoVietCollisions` dùng profile/kích thước mẫu đang chọn; vẫn va chạm thật khi thiếu độ cao, xe nghiêng/lật không thành mặt đỡ.
- **Kiểm tra hoàn tất:** 11 suite / **3.002 điều kiện đạt**: MeshyCars 1.175, MeshyCarRoofs 256 (64 cú nhảy thật), TrafficDynamics 82, CrashUpgrade 97, BoostTrafficIsolation 478, QuickKick 62, CrashOutcome 51, CrashPoliceNavigation 214, NightJumpLighting 88, DistanceMode 31, SmoothTurns 468. Thêm **28 kiểm tra nguồn**: đủ mọi tam giác gốc LOD0, scale đồng nhất giữ tỷ lệ, không quá bề rộng làn và hash ba map 2K trùng nguồn. `VerifyTrafficDynamics` được chỉnh vị trí fixture theo tổng chiều dài hai thân xe thay khoảng 4,7 m cứng của xe cũ; không sửa vật lý để ép test qua.
- **Bằng chứng:** `Captures/meshy-cars-verification-summary.json`, từng `meshy-cars-Verify*.json`, source-preservation/texture/export/install JSON và `VERIFICATION_MESHY_CARS.md`. Đã xem ảnh Unity cận cả bốn mẫu với ba màu mỗi mẫu: `meshy-cars-PV_Meshy*-paint*.png`. Mặt nạ màu là bước tinh chỉnh cuối; sau đó chụp lại 12 ảnh và chạy lại MeshyCars 1.175 trên scene cuối. Restore pose Idle trước khi gắn lại neo đèn để không giữ offset từ cản xe bị cong khi tái sử dụng.
- **Trạng thái:** scene đã cài và lưu, Unity về Edit Mode; source/asset/tài liệu được commit riêng (SHA xem Git log). **Đã build/push/phát hành WebGL 1.33.0 và xác minh live ở 21.18**; `docs` và bản công khai cùng cache `f730cee9fbafa12d`. Công việc vẫn `RideJobsReleased=false`, `jobsEnabled=false`; dữ liệu Coin/kỷ lục, DPR/độ nét mobile và bản sửa đuôi xe máy được giữ. WebGL được kiểm ở 21.18; chưa thử điện thoại vật lý/FPS cho bộ xe này. Giữ nguyên ngoài commit mọi Wave/Bridge/Packages và các nguồn Blender cũ có sẵn.

### 21.18. Build/phát hành bộ ô tô Meshy — 1.33.0 (24/10/2026, đã phát hành/xác minh live)

- Người dùng yêu cầu trực tiếp “build và phát hành web”, cho phép build, kiểm tra, commit/push main và GitHub Pages trong cùng công việc; không cần hỏi lại quyền phát hành.
- Nâng cấu hình/PlayerSettings và kỳ vọng các kiểm tra web lên 1.33.0. Source ô tô ở commit `ac45183`; kế thừa 3.002 điều kiện Unity + 28 kiểm tra nguồn ở 21.17, không gọi đó là các suite đã chạy lại cho lần build này.
- Preflight kiểm đúng scene, jobs khóa, 33 xe máy/CSGT nguyên trạng, 11 root ô tô Meshy, đủ mẫu/LOD/clip/material/neo đèn. Giữ PWA, DPR3/3,2triệu pixel và bản sửa bánh xe. Không đưa các thay đổi Wave/Bridge/Packages có sẵn vào staging.
- **Build cuối:** `build_6bd4eaf57194` Succeeded, 6.238 ms (incremental sau build đầy đủ 329.081 ms), 0 lỗi/1 warning Pipeline đã biết; 86.476.927 byte. Build đầy đủ có cảnh báo mã editor chưa compile nên đã chờ ready/reconfigure rồi build lại; không phát hành lần thử. Hai lần dùng cùng tài nguyên/cache `f730cee9fbafa12d`; báo cáo packedAssets của lần đầy đủ xác nhận 0 nguồn MeshyImports vào runtime và đủ 4 material/texture ô tô; lần incremental không lặp inventory. UTC cuối `2026-10-23T20:24:30.7102647Z`. Bằng chứng `meshy-cars-release-build-summary.json`.
- Đã copy output cuối sang docs và dọn đúng 4 file hash cũ; **29 kiểm tra bundle đạt**. Kiểm tra browser DPR3 đạt 14 điều kiện (1170×2532 dọc, 2532×1170 ngang, 2066×1549 tablet, xoay lại đúng), 0 lỗi runtime; đã xem hai ảnh cảnh chơi với xe tải Meshy màu xanh và xe thể thao phía xa. **Browser local hoàn tất 77 điều kiện** = DPR3 14 + PWA/offline/cảm ứng 37 + đối chiếu tài nguyên/WASM 26; 0 lỗi runtime. `meshy-cars-{mobile,pwa,local}-results.json`, ảnh dọc/ngang và desktop/offline. `git diff --check` đạt. Đã commit/push và kiểm tra live như dưới đây. Chưa kiểm điện thoại vật lý/FPS/nhiệt độ.

- **Phát hành hoàn tất:** source/artifact `9f5c5210500e169fcda5418d976d1841307e6fb9`, [Pages run40021075002](https://github.com/denispham1107/taixecongnghe/actions/runs/40021075002) completed/success đúng SHA. **26 kiểm tra live đạt, 0 lỗi runtime**: version/cache khớp, từng tài nguyên trùng artifact đã thử (binary từng byte, text chuẩn hóaCRLF), WASM thật khởi tạo/start/pause thành công. Đã xem ảnh live `meshy-cars-live-game.png`. Bằng chứng `meshy-cars-live-results.json`, `meshy-cars-pages-release.json`, `VERIFICATION_MESHY_CARS_WEB.md`.
- **Bàn giao cuối:** Unity ready/Edit Mode, server docs kiểm tra đã dừng; dữ liệu người dùng/cờ công việc giữ nguyên. Các chỉnh sửa Wave/Bridge/Packages và Blender cũ vẫn ngoài commit. Báo cáo phát hành commit/push riêng sau artifact, không rebuild chỉ vì tài liệu. Nếu PWA giữ bản cũ, đóng hết tab/app Phố Việt rồi mở lại để worker mới kích hoạt; không xóa Coin/kỷ lục. Chưa thử phần cứng điện thoại/FPS/nhiệt độ.

### 21.19. Phố Sài Gòn theo ảnh mẫu (24/10/2026, Claude Code — đã phát hành1.34.0 ở21.27)

- **Yêu cầu:** dựng lại toàn bộ cảnh quan nhà phố hai bên đường giống ảnh mẫu Sài Gòn 3D stylized bằng Blender MCP, đưa vào Unity; giữ gameplay và ngân sách WebGL; gửi ảnh so sánh cho người dùng duyệt; không commit/build/push khi chưa được đồng ý; yêu cầu phát hành tiếp theo đã cho phép và hoàn tất ở21.27. Ảnh mẫu đã chép vào `ArtSource/Reference/Street/saigon_street_reference.webp`.
- **Quyết định đã áp dụng (cần người dùng xác nhận khi duyệt ảnh):** (1) giữ mặt tiền ở |x| = 9,3 m nên vỉa hè hẹp hơn ảnh mẫu, đổi lại không đổi luật đâm nhà/làn/vật cản; (2) đưa lại chậu cây/cây ban công/hoa giấy vì ảnh mẫu có, thay quy tắc “đã xóa chậu cây” của 1.13; (3) 12 tên shop mới theo ảnh mẫu: PHỞ BÒ - GÀ, TIỆM TRÀ, BÁNH MÌ, TẠP HÓA, THUỐC TÂY, SỬA XE, CÀ PHÊ SÀI GÒN, CƠM TẤM, BÚN BÒ HUẾ, HỦ TIẾU NAM VANG, SINH TỐ, TIỆM BÁNH; (4) tủ nước đỏ không logo thương hiệu; (5) đèn đường LED, đèn giao thông, dây điện, xe đậu, người vỉa hè, xe đẩy giữ bộ cũ vì gắn với hệ đèn/danh sách `trafficLights`.
- **Nguồn Blender:** `ArtSource/Blender/pho_viet_saigon_streets.py` → `build_saigon_streets()` trong Blender đang mở qua MCP. Script tạo scene riêng `PV_SaigonStreets`, dựng hình học bằng lớp `Geo` (hộp 12 tam giác, lá hai mặt 8 tam giác, chữ từ font Be Vietnam Pro), sửa chiều mặt theo gợi ý pháp tuyến (vì `B()` là phép đối xứng), xuất đè `Assets/PhoViet/Art/Streets/PV_Street{Shop00..11,Tree0..2,Pavement}.fbx/.json`, `PV_StreetPalette.png`, `catalogue.json`, rồi lưu `ArtSource/Blender/PhoViet_SaigonStreets.blend`. LOD dựng theo mức chi tiết (0/1/2), không decimate; chữ và 14 dải mái hiên có ở mọi LOD. Lúc bắt đầu Blender trống, không có collection `WA_Bike`; không xóa dữ liệu khác. `PhoViet_Streets.blend`/`pho_viet_streets.py` cũ giữ nguyên (chạy lại sẽ đè bộ mới).
- **Kích thước/tham số:** nhà rộng 8,36 m, sâu 7,8 m, tầng trệt 4,9 m + 2–4 tầng × 3 m; biển 7,36 × 1,36 m ở y 3,4–4,8; mái hiên 3,22 → 2,8 m, nhô tới z −5,4; ban công sâu 1 m tới z −4,9; nhà cao tầng phía sau ở shop 1/4/7/10, cao 24–36 m. Cây: tán bầu dục 1,25 × 1,95 × 2,45 m, lá 2.600/1.250/560. Vỉa hè gạch caro 0,6 m. Palette 64 màu lưới 8×8.
- **Ngân sách:** tổng LOD0 12 shop 81.592 (trước 229.458), mỗi shop 4.309–8.703; cây 21.026/10.190/4.670 (trước 30.076/12.316/7.132); vỉa hè 1.258/474/156 (trước 15.500/7.440/2.766). Vẫn một material `PV_StreetAtlas`, một texture 128 px.
- **Unity:** `AgentScripts/InstallSaigonStreets.cs` → `InstallSaigonStreets.Main` (Edit Mode): reimport FBX/JSON/palette, đổi tên 490 đối tượng `Blender • …` theo chỉ số model, `RecalculateBounds` 1.260 LODGroup, chạy `PhoVietObstacleUpgrade.Install` rồi lưu scene. `VietnamStreet.Shops`/`Subs` đổi theo tên mới. `PhoVietObstacleUpgrade` đọc hộp vật cản shop/cây từ JSON qua lớp `StreetArtMetadata` (Editor assembly, vì JsonUtility trong assembly tạm `run_script` không đọc được mảng lớp lồng); 735 nhóm / **7.955 hộp**, giữ loại cũ (Nhà cửa, Bàn ăn, Ghế nhựa, Quầy thức ăn, Kệ tạp hóa, Tủ dụng cụ sửa xe, Mái hiên, Thân cây…) và thêm Bảng hiệu, Ban công, Chậu cây lớn, Thùng rác, Tủ nước, Bảng thực đơn, Cán dù, Ghế gỗ. Không đổi C# gameplay. `RideJobsReleased=false`, `jobsEnabled=false`.
- **Kiểm tra:** 13/14 suite / 1.907 điều kiện đạt (mục 14). `VerifyStreetArt` mở rộng lên 323 điều kiện; giới hạn kích thước LOD nâng 25 → 45 m vì nhà cao tầng phía sau (vẫn bắt lỗi scale cm). `VerifyJunctionRoutes` đọc tên shop từ `VietnamStreet.Shops[0]`. VerifyJump lỗi có sẵn (mái xe tải, bộ ô tô 1.33) — chưa sửa. Sau Play Mode, `PV_Racer.mat`/`PV_TrafficPolice.mat` không bị đổi. Bằng chứng `Captures/VERIFICATION_SAIGON_STREET.md`, `saigon-street-verification-summary.json`, `saigon-street-Verify*-results.json`; ảnh `saigon-street-comparison.png`, `saigon-street-{before,after}-{game,axis,facade-left,facade-right,sidewalk}.png` (chụp bằng `AgentScripts/CaptureSaigonStreet.cs`, không đổi scene).
- **Trạng thái:** scene đã lưu, Unity ở Edit Mode. **Đã commit/build/push/phát hành1.34.0 theo yêu cầu tiếp theo, bằng chứng ở21.27**. Chưa đo FPS trên điện thoại thật. Lần phát hành1.34.0 đã tăng version theo15.2 và cập nhật trạng thái. Không stage các blend cục bộ (Cars, Jump, Motorcycles, Passengers, Streets) và file Wave/Bridge/Packages ngoài phạm vi.

### 21.20. Cột đèn đường, cột đèn tín hiệu và mây theo ảnh mẫu (24/10/2026, Claude Code — đã phát hành1.34.0 ở21.27)

- **Yêu cầu tiếp theo của người dùng:** cột đèn đường và cột đèn xanh đỏ còn thô, chưa giống ảnh; thêm mây phía xa trên trời giống ảnh.
- **Nguồn Blender:** `pho_viet_saigon_streets.py::build_saigon_fixtures()` (được `build_saigon_streets()` gọi; lưu trong `PhoViet_SaigonStreets.blend`):
  - `street_lamp`: cột côn xám cao 7,55 m, cần cổ ngỗng cung 1,05 m rồi vươn tới bầu đèn dẹt, kính trắng phía dưới; tâm quang học `LAMP_REACH=2,35`, `LAMP_HEIGHT=8,48`. Xuất đè `Assets/PhoViet/Art/Lighting/PV_LEDStreetLamp.fbx` cùng tên gốc LOD0 + hai con `_LOD1/_LOD2`, 3 slot vật liệu metal/silver/lens để giữ prefab instance, LODGroup và emission kính của hệ ngày–đêm. 778/378/228 tam giác (đèn cũ nặng hơn nhiều). Metadata `PV_LEDStreetLamp.json`.
  - `traffic_signal`: cột xám, cần vươn 3,35 m ở y 7,6 có thanh giằng, hộp đèn treo đối xứng trước/sau (tâm `SIGNAL_REACH=3`, `SIGNAL_HEAD_Y=6,82`, cao 6,1–7,5 m nên trên đỉnh nhảy của người lái ~6,4 m), hộp phụ trên thân cột và hộp điện. `Assets/PhoViet/Art/Streets/PV_TrafficSignal.fbx` (atlas phố, tên không bắt đầu bằng PV_Street để không lẫn vào kiểm tra 1.260 nhóm phố). 358/304/158 tam giác.
- **Unity:** `AgentScripts/InstallSaigonStreetFixtures.cs` (Edit Mode, lưu scene): gỡ **5.824 tam giác** cột/cần/hộp tín hiệu cũ khỏi 7 mesh gộp `Geometry • …` (vùng |x| 5,8–7,85, y 0,08–5,12, z 61/78 ± 0,5), lưu thành `Assets/PhoViet/Generated/PV_NoOldSignals_<guid>.asset`; gắn `Blender • cột đèn tín hiệu` (LOD 3 mức) vào **140** nhóm `Đèn giao thông` (xoay 180° ở bên phải); dời `Hộp đèn` tới (±3; 6,82) nên **420 bóng "Tín hiệu"** runtime vẫn đổi màu như cũ; dời **70** `Tâm quang học LED` và vệt sáng mặt đường; đặt lại xoay gốc đèn theo FBX và tính lại bounds. Không thêm vật cản: cột tín hiệu ở vỉa hè, cần/hộp đèn trên cao nên luật va chạm không đổi.
- **Mây:** sửa `Assets/PhoViet/Scripts/DayNightSky.shader` như mô tả ở 20.2. Chi phí: 4 tầng value noise chỉ trên pixel bầu trời.
- **Kiểm tra (art cuối, Play Mode):** VerifyStreetArt **330** (thêm 7 điều kiện: 140 cột tín hiệu có 3 LOD, bóng tín hiệu dưới cần mới và hướng ra đường, 420 renderer tín hiệu vẫn thuộc `Hộp đèn`, mesh gộp không còn cột cũ, 70 tâm quang học đúng vị trí, hình đèn chạm tâm quang học, shader mây không lỗi), NightJumpLighting 88, MeshyLighting 16, CrashUpgrade 97, JunctionRoutes 92, Gameplay 23, DistanceMode 31, SmoothTurns 468, CrashOutcome 51, QuickKick 62, JunctionKick 216, Boost 35, Gestures 39, MeshyCarRoofs 256 = **14 suite / 1.804 điều kiện đạt**. CrashPoliceNavigation lỗi một lần trong lượt chạy chung (“Unbounded police movement in Moving traffic…”, giao thông theo seed ngẫu nhiên) rồi **đạt 214 ở cả hai lần chạy lại riêng**; thay đổi lần này không đụng vật cản/logic CSGT — ghi là kiểm tra chập chờn cần theo dõi. VerifyJump vẫn lỗi có sẵn như 21.19. Ảnh Play Mode ngày/đêm `Captures/saigon-street-day-*.png`, `saigon-street-night-*.png` (`CaptureSaigonStreet.DayNight`), ảnh so sánh `saigon-street-comparison.png` đã cập nhật.
- **Trạng thái:** scene đã lưu, Unity Edit Mode, jobs khóa. **Đã commit/build/push/phát hành1.34.0, bằng chứng ở21.27** (trạng thái chờ của lần dựng đã kết thúc).

### 21.21. Tán cây kín lá và xóa người đứng vỉa hè (24/10/2026, Claude Code — đã phát hành1.34.0 ở21.27)

- **Yêu cầu:** tán cây có khoảng trống không lá ở giữa nhìn rất kỳ; xóa toàn bộ hình người đứng trên vỉa hè hai bên đường.
- **Nguyên nhân tán rỗng:** bản 21.19 xếp lá ở vỏ tán với trục lá hướng ra ngoài theo bán kính, nên nhìn thẳng vào giữa tán chỉ thấy mép lá mỏng, lộ lõi rỗng. **Sửa** `pho_viet_saigon_streets.py::tree`: `Geo.leaf` nhận thêm `face` để mặt lá quay theo pháp tuyến tán (trục lá theo tiếp tuyến, hơi rủ xuống); 75% lá ở vỏ (bán kính 0,8–1,0), 25% ở lớp trong (0,4–0,8) màu xanh đậm; lá dài 0,38–0,56 m. Số lá 3.000/1.450/680, tam giác 24.226/11.790/5.630 (LOD0 vẫn dưới mức 30.076 của bộ cây 1.13). Xuất lại qua Blender MCP, cài bằng `InstallSaigonStreets`.
- **Xóa người:** người đứng vỉa hè (`StreetArt.Person`) đã bị gộp vào mesh `Geometry • …` của từng khối. `AgentScripts/RemoveSidewalkPeople.cs` gỡ **49.560 tam giác** nằm trọn trong hộp |x| = 8,15 ± 0,6, y 0,2–2,1, z = 15/33/51 ± 0,6 khỏi 42 mesh gộp, lưu `Assets/PhoViet/Generated/PV_NoPeople_<guid>.asset`, rồi lưu scene. `VietnamStreet.Build` bỏ lời gọi `Person` nên dựng lại phố cũng không sinh lại người. Người không có hộp vật cản nên gameplay không đổi.
- **Chưa xóa (chờ người dùng chọn):** xe máy đậu trên vỉa hè (`Xe máy đậu trên vỉa hè`, 1 xe/bên/khối và bản tĩnh ở nhánh) vẫn có **người lái ngồi trên xe** vì dùng chung FBX xe + người Meshy. Đây không phải người đứng nên chưa tự xóa.
- **Kiểm tra (Play Mode, art cuối):** **15 suite / 2.019 điều kiện đạt**: StreetArt 331 (thêm kiểm tra không còn người đứng), CrashUpgrade 97, Gameplay 23, JunctionRoutes 92, DistanceMode 31, NightJumpLighting 88, CrashOutcome 51, CrashPoliceNavigation 214, SmoothTurns 468, QuickKick 62, JunctionKick 216, Boost 35, Gestures 39, MeshyCarRoofs 256, MeshyLighting 16. Ảnh `Captures/saigon-street-after-tree.png`, `saigon-street-after-people-removed.png`, ảnh ngày/đêm và `saigon-street-comparison.png` đã chụp lại.
- **Trạng thái:** scene đã lưu, Unity Edit Mode, jobs khóa. **Đã commit/build/push/phát hành1.34.0, bằng chứng ở21.27** (trạng thái chờ của lần dựng đã kết thúc).

### 21.22. Vách kính, cửa ra vào và nội thất tầng trệt (24/10/2026, Claude Code — đã phát hành1.34.0 ở21.27)

- **Yêu cầu:** mặt tiền tầng trệt trông như tường kín, không có cửa; phải giống ảnh mẫu: vách kính trong suốt nhìn thấy bên trong cửa hàng và có cửa đi vào.
- **Nguyên nhân:** bản 21.19 chỉ có lòng tiệm nông 2,4 m với tường trong màu vàng phẳng, không có kính/khung/cửa nên từ đường nhìn như bức tường.
- **Blender (`pho_viet_saigon_streets.py`):**
  - `glass_front`: ba tấm kính (hai bên + ô cửa) trên mặt tường z = −3,9, gắn tag `Glass` nên `to_mesh` tách sang slot vật liệu thứ hai `PV_StreetGlass`; khung tối (`darkFrame`) hoặc gỗ (cà phê/trà/bánh), chân vách 0,38 m, đố đứng mỗi ~1,6 m, dầm 3,16/2,62 m; cửa hai cánh 1,32 m có đố giữa, ray dưới, hai tay nắm inox, chữ “MỞ CỬA” (LOD0/1). Vị trí cửa tránh quầy/bàn ngoài vỉa hè (`DOOR_X`).
  - `interior`: lòng tiệm sâu `DEPTH = 3,4 m`, sàn gạch caro, 3–6 đèn thả; cà phê có quầy gỗ, máy pha, kệ ly, bảng thực đơn, hai bàn gỗ bốn ghế, chậu cây; phở/cơm/bún/hủ tiếu/bánh mì có bếp inox, nồi, hai bảng thực đơn, bàn inox ghế nhựa đỏ (bánh mì thêm ổ bánh); trà/sinh tố có quầy ly màu, kệ, bảng menu màu biển; tạp hóa/thuốc/bánh có kệ tường cao 2,3 m năm tầng, kệ hai vách, kệ giữa/tủ lạnh (tạp hóa), quầy chữ thập xanh (thuốc), tủ bánh kính (bánh). Tiệm sửa xe không có kính: cửa cuốn kéo lên, vách dụng cụ, bàn đỏ, lốp.
  - Đèn tường hai bên cột như ảnh mẫu. Kệ trong tiệm cũ (z = −1,7) được thay; khối nhà phía sau dời theo chiều sâu mới.
  - Tam giác LOD0 shop 5.453–10.613 (trước 4.309–8.703), LOD1 3.417–6.273, LOD2 1.641–2.913; vẫn dưới mức 27.879 của bộ shop cũ.
- **Unity:** `PhoVietStreetUpgrade.Glass()` tạo/cấu hình `Assets/PhoViet/Art/Streets/PV_StreetGlass.mat` (Standard Transparent premultiply, màu (.78,.9,.97, α .2), smoothness .92, ZWrite 0, queue 3000); `PhoVietStreetUpgrade.Materials(r)` trả `[PV_StreetAtlas, PV_StreetGlass]` cho mesh 2 submesh. `InstallSaigonStreets` gán lại vật liệu cho **1.350 renderer** shop có kính (450 shop × 3 LOD; 40 tiệm sửa xe giữ một vật liệu), tính lại bounds rồi lưu scene. Vật cản không đổi (kính nằm trên mặt tường, trong hộp “Nhà cửa”).
- **Kiểm tra (Play Mode):** **15 suite / 2.020 điều kiện đạt**: StreetArt 332 (thêm kiểm tra submesh kính + PV_StreetGlass trong suốt, sửa xe không kính), CrashUpgrade 97, Gameplay 23, JunctionRoutes 92, DistanceMode 31, NightJumpLighting 88, CrashOutcome 51, CrashPoliceNavigation 214, SmoothTurns 468, QuickKick 62, JunctionKick 216, Boost 35, Gestures 39, MeshyCarRoofs 256, MeshyLighting 16. Ảnh `Captures/saigon-street-after-storefront.png`, `saigon-street-after-storefront-left.png`, `saigon-street-day-*`, `saigon-street-night-*` và `saigon-street-comparison.png`.
- **Giới hạn:** ban đêm lòng tiệm tối theo ánh sáng cảnh (chưa có emission/đèn trong tiệm); kính không phản chiếu môi trường thật ngoài reflection probe chung.
- **Trạng thái:** scene đã lưu, Unity Edit Mode, jobs khóa. **Đã commit/build/push/phát hành1.34.0, bằng chứng ở21.27** (trạng thái chờ của lần dựng đã kết thúc).

### 21.23. Đồ vật vỉa hè và nội thất chi tiết theo ảnh mẫu (24/10/2026, Claude Code — đã phát hành1.34.0 ở21.27)

- **Yêu cầu:** dựng lại thật chi tiết đồ vật/nội thất trên vỉa hè và bên trong các quán, quan sát kỹ ảnh mẫu.
- **Thư viện đồ vật mới trong `pho_viet_saigon_streets.py`** (đều dựng theo mức chi tiết LOD 0/1/2):
  - `stool`: ghế nhựa đỏ mặt 0,28 m, váy quanh mặt, 4 chân loe, vành giằng dưới (LOD0). `pho_table`: bàn gỗ nhỏ có diềm, 4 chân, `condiments` (khay, chai tương ớt/xì dầu/chanh có nắp, ống đũa inox, hộp giấy), 2 chén.
  - `cafe_table` + `cafe_chair`: bàn gỗ có ly/tách; ghế gỗ có 2 trụ lưng và 3 nan ngang quay ra ngoài bàn.
  - `a_frame`: bảng đen chữ A khung gỗ, mặt bảng nghiêng, chữ thực đơn đặt trên mặt nghiêng (`PHỞ`/`CÀ PHÊ` + giá).
  - `planter`: chậu vuông cao trắng/xám có viền, cây lá to bản kiểu môn/lan ý 20/11/6 lá, lá trong cao, lá ngoài ngả ra (thay chậu lá nhọn kiểu lô hội).
  - `food_cabinet`: tủ kính trên thân inox có cửa và bánh xe, kính trước/hai bên/trên dùng `PV_StreetGlass`, hai tầng tô có món (rau, bún, thịt) hoặc ổ bánh mì; nồi nước dùng inox có bếp, nắp, vá (phở/cơm/bún/hủ tiếu).
  - `drink_fridge`: tủ đỏ cửa kính, 4 tầng chai nhiều màu nhìn qua kính, băng đầu trắng không logo, tay nắm. `outdoor_shelf`: kệ sắt 4 tầng chai/hộp. `crates`: két vàng/đỏ có lỗ, chai trên cùng.
  - `umbrella`: dù 12 múi có riềm rủ, gọng và đế nặng. `trash_bin`: thùng xanh thân loe, nắp, tay cầm, bánh xe. `display_case` (thuốc/bánh), `toolbox`, `tyre_stack` (lốp hình xuyến) cho tiệm sửa xe; thêm `Geo.torus`.
- **Bố trí (`sidewalk`):** cà phê hai bộ bàn + 4 ghế, chậu cây nhỏ, bảng chữ A; phở/cơm/bún/hủ tiếu/bánh mì: tủ kính + nồi, 2 bàn gỗ × 4 ghế nhựa đỏ, bảng chữ A (phở); trà/sinh tố: dù vàng–trắng, quầy ly màu, bàn trắng + ghế đỏ; tạp hóa: tủ nước, kệ sắt, két; thuốc/bánh: tủ kính trưng bày; sửa xe: lốp, tủ đồ nghề. Lối vào cửa để trống. Trong tiệm: bàn ghế dùng cùng mẫu (cà phê ghế tựa, quán ăn bàn gỗ + ghế nhựa đỏ), hũ cà phê trên kệ, chồng tô, rổ rau, đèn lồng đỏ (quán ăn).
- **Vật cản:** 735 nhóm / **8.120 hộp** (thêm nồi nước dùng, bảng chữ A, tủ/kệ/két mới). Hộp ghế nhựa giữ 0,42 × 0,46 × 0,42 m như bộ đã kiểm định: thử thu nhỏ còn 0,36 m làm `VerifyCrashUpgrade` (“Rider can steer away … Ghế nhựa”) báo lỗi vì điểm thử nằm sau tâm hộp, nên đã trả lại kích thước cũ thay vì sửa test.
- **Ngân sách:** LOD0 shop 6.309–13.338 tam giác, LOD1 3.833–7.698, LOD2 1.713–3.196; vẫn dưới 27.879 của bộ shop 1.13.
- **Kiểm tra (Play Mode, art cuối):** StreetArt 332, CrashUpgrade 97, Gameplay 23, JunctionRoutes 92, DistanceMode 31, NightJumpLighting 88, CrashOutcome 51, SmoothTurns 468, QuickKick 62, JunctionKick 216, Boost 35, Gestures 39, MeshyCarRoofs 256, MeshyLighting 16 đạt trong lượt chạy chung; **CrashPoliceNavigation** báo “Unbounded police movement in Moving traffic…” trong lượt chạy chung (lần thứ hai trong ngày) rồi **đạt 214 ở hai lần chạy lại riêng**. Kịch bản dùng giao thông ngẫu nhiên; không đổi mã CSGT/vật cản liên quan — cần theo dõi/làm ổn định test riêng (nguyên nhân thật và bản sửa: 21.50). Tổng 15 suite / 2.020 điều kiện đạt khi tính lần chạy lại.
- **Ảnh:** `Captures/saigon-street-shops-sheet.png` (cận 12 loại tiệm, `CaptureSaigonStreet.Shops`), `saigon-street-shop-00..11.png`, `saigon-street-after-*`, `saigon-street-day-*`, `saigon-street-night-*`, `saigon-street-comparison.png`.
- **Trạng thái:** scene đã lưu, Unity Edit Mode, jobs khóa. **Đã commit/build/push/phát hành1.34.0, bằng chứng ở21.27** (trạng thái chờ của lần dựng đã kết thúc). Người ngồi trên xe máy đậu vỉa hè vẫn giữ (chờ người dùng chọn, 21.21).

### 21.24. Dựng lại cây xanh theo ảnh mẫu cây (24/10/2026, Claude Code — đã phát hành1.34.0 ở21.27)

- **Yêu cầu:** cây trong game quá xấu; dựng lại giống ảnh cây mẫu (mọi chi tiết và tông màu). Ảnh đã chép vào `ArtSource/Reference/Street/tree_reference.webp`.
- **Quan sát ảnh mẫu:** thân xám mảnh thẳng, hơi nghiêng, chiếm ~40% chiều cao; cành xám chia lên từ đỉnh thân; tán tròn không đều rộng hơn cao, gồm nhiều cụm lá dày có khe hở lộ cành/bóng; lá bầu dục to bản; xanh tươi ngả vàng (sáng ở ngọn và mặt ngoài, đậm ở trong/dưới).
- **Blender `pho_viet_saigon_streets.py::tree`:** thân côn r 0,10→0,065 m tới ~3,7–4 m (lệch nhẹ theo seed), 5 cành chính r 0,055→0,032 m tới nút trong tán, cành con nối nút gần nhất tới 40/30/20 cụm lá (bán kính cụm 0,36–0,55 m, vỏ tán 0,6–1,05). Mỗi cụm 88/50/33 lá xếp chùm quanh tâm cụm, mặt lá quay ra ngoài và hơi lên; lá dài 0,21–0,30 m (LOD xa phóng 1,3/1,6). `Geo.leaf` thêm tham số `wpos` (vị trí chỗ rộng nhất) để lá bầu dục thay vì hình thoi nhọn. Màu theo độ cao/khoảng cách tới tâm tán; palette đổi `leaf1 4C8E2A`, `leaf2 5FA834`, `leaf3 78BE42`, `leaf4 9CD25C`, `leaf5 2F5F1F`, `trunk 8E8B86` (xám). Các cây/chậu/hoa khác dùng chung các màu lá này nên cũng tươi hơn.
- **Ngân sách:** 29.026 / 12.414 / 5.446 tam giác (dưới mức 30.076 của bộ cây 1.13); lá 3.520 / 1.500 / 660. Vật cản thân/bồn giữ nguyên.
- **Unity:** cài bằng `InstallSaigonStreets` (reimport, bounds, vật cản). Ảnh so sánh `Captures/saigon-street-tree-comparison.png` (ảnh mẫu bên trái, cây trong Unity bên phải, `CaptureSaigonStreet.Tree`), `saigon-street-tree-closeup.png`; ảnh ngày/đêm và so sánh phố đã chụp lại.
- **Kiểm tra (Play Mode):** StreetArt 332, CrashUpgrade 97, Gameplay 23, JunctionRoutes 92, DistanceMode 31, NightJumpLighting 88, CrashOutcome 51, SmoothTurns 468, QuickKick 62, JunctionKick 216, Boost 35, Gestures 39, MeshyCarRoofs 256, MeshyLighting 16 đạt. **CrashPoliceNavigation** lại lỗi “Unbounded police movement in Moving traffic…” trong lượt chạy dài 15 suite (lần thứ ba trong ngày) nhưng **đạt 214 ở 5 lần chạy riêng/ngay sau CrashOutcome**; không phải hình học cây. (Giả thuyết “gắn với chuỗi suite dài” sau này được chứng minh là sai: lỗi xảy ra cả khi chạy riêng, do xe tải hồi phục trượt 15–20 m và CSGT bị khóa cạnh xe — đã sửa ở 21.50.)
- **Trạng thái:** scene đã lưu, Unity Edit Mode, jobs khóa. **Đã commit/build/push/phát hành1.34.0, bằng chứng ở21.27** (trạng thái chờ của lần dựng đã kết thúc).

### 21.25. Bảng tên tiệm dưới tủ kính và cây chậu kiểu cây mẫu (24/10/2026, Claude Code — đã phát hành1.34.0 ở21.27)

- **Yêu cầu:** (1) nửa dưới các tủ kính (thân tủ, không phải phần kính) hiện tên tiệm như biển hiệu, cùng tông màu, tên hai nơi phải đồng nhất; ví dụ tủ Cơm Tấm nền vàng chữ đỏ “CƠM TẤM / SƯỜN - BÌ - CHẢ”, căn giữa chuẩn, chữ không tràn. (2) Chậu cây hai bên vỉa hè và trên ban công có thân và lá giống cây mẫu 21.24.
- **`name_panel(g, spec, x, y, z, w, h)`** trong `pho_viet_saigon_streets.py`: viền (màu viền biển: trắng, hoặc màu chữ nếu biển nền trắng) + nền `spec['board']`; dòng 1 `spec['main']` màu `ink`, dòng 2 lấy đúng dòng phụ của biển (`side` → `script` → `sub`, cùng màu/nghiêng như biển). Cả hai dòng dùng cùng nguồn chữ với biển hiệu nên không thể lệch tên. Chữ căn giữa ngang (align CENTER) và dọc (dòng 1 ở +0,14h, dòng 2 ở −0,24h; một dòng thì giữa bảng), co theo `maxwidth = w − 0,18/0,22 m` nên không tràn. Gắn vào mặt trước thân tủ của `food_cabinet` (phở, bánh mì, cơm tấm, bún bò, hủ tiếu; 1,42 × 0,62 m) và `display_case` (thuốc tây, tiệm bánh; 1,32 × 0,56 m); bỏ hai tấm cửa inox cũ ở mặt trước tủ đồ ăn. Ảnh kiểm tra nhìn thẳng `Captures/saigon-street-cabinet-panels.png` (`CaptureSaigonStreet.Cabinets`).
- **`mini_tree(g, base, height, d, seed)`**: cây nhỏ cùng kiểu cây đường 21.24 (thân xám côn, cành chẻ tới 8/5/3 cụm lá, mỗi cụm 24/16/12 lá bầu dục `wpos=.48`, màu sáng ngọn/ngoài, đậm trong/dưới). `planter` vỉa hè dùng cây cao 1,3 m × tỉ lệ chậu (hộp vật cản chậu nâng lên 1,9 m cao); chậu ban công dùng cây 0,95 m với 5/3/0 cụm (LOD2 chỉ còn chậu). Chậu sân thượng và bồn hoa cửa sổ giữ như cũ.
- **Ngân sách:** LOD0 shop 9.025–19.278 tam giác (trước 6.309–13.338), LOD1 4.933–10.358, LOD2 1.965–3.835; vẫn dưới 27.879 của bộ shop 1.13. Vật cản 735 nhóm / 8.120 hộp.
- **Kiểm tra (Play Mode):** **15 suite / 2.020 điều kiện đạt**: StreetArt 332, CrashUpgrade 97, Gameplay 23, JunctionRoutes 92, DistanceMode 31, NightJumpLighting 88, CrashOutcome 51, SmoothTurns 468, QuickKick 62, JunctionKick 216, Boost 35, Gestures 39, MeshyCarRoofs 256, MeshyLighting 16 (lượt chung) + CrashPoliceNavigation 214 (chạy riêng, xem 21.24). Ảnh `saigon-street-cabinets-sheet.png`, `saigon-street-cabinet-panels.png`, `saigon-street-after-facade-left.png`, ngày/đêm và so sánh phố chụp lại.
- **Lưu ý (đã xử lý ở 21.26):** xe đẩy “BÁNH MÌ” cũ của `VietnamStreet.Cart` đứng trước tủ Bún Bò Huế; đã xóa.
- **Trạng thái:** scene đã lưu, Unity Edit Mode, jobs khóa. **Đã commit/build/push/phát hành1.34.0, bằng chứng ở21.27** (trạng thái chờ của lần dựng đã kết thúc).

### 21.26. Xóa xe đẩy bánh mì cũ trước tủ Bún Bò Huế (24/10/2026, Claude Code — đã phát hành1.34.0 ở21.27)

- **Yêu cầu:** dời hoặc xóa xe đẩy bánh mì cũ để không che tủ kính Bún Bò Huế. **Đã chọn xóa** vì phố đã có tiệm BÁNH MÌ riêng và xe đẩy mang tên khác tiệm phía sau.
- **Scene:** `AgentScripts/RemoveBreadCart.cs` gỡ tam giác nằm trọn trong hộp |x| 7,3–9,8, y 0,25–2,7, z = 43 ± 1,2 khỏi mesh gộp `Geometry • …` (lần đầu 2.772 tam giác/21 mesh, lần hai nới hộp gỡ thêm 140 tam giác viền biển còn sót), lưu `Assets/PhoViet/Generated/PV_NoCart_<guid>.asset`; xóa 70 nhóm `Xe bánh mì` (chữ biển); chạy lại `PhoVietObstacleUpgrade.Install` rồi lưu scene. Kiểm tra lại bằng quét tam giác quanh vị trí cũ: không còn hình học.
- **Mã:** `VietnamStreet.Build` không gọi xe đẩy nữa, bỏ hàm `Cart`; `PhoVietObstacleUpgrade` bỏ hộp “Xe đẩy bán hàng” (735 nhóm / **8.050 hộp**). `VerifyCrashUpgrade` bỏ “Xe đẩy bán hàng” khỏi danh sách loại vật cản cần thử (vật đó không còn tồn tại; 97 → 93 điều kiện). `VerifyStreetArt` thêm điều kiện không còn xe đẩy/hộp vật cản xe đẩy (332 → 333).
- **Kiểm tra (Play Mode):** StreetArt 333, CrashUpgrade 93, Gameplay 23, JunctionRoutes 92, DistanceMode 31, NightJumpLighting 88, CrashOutcome 51, SmoothTurns 468, QuickKick 62, JunctionKick 216, Boost 35, Gestures 39, MeshyCarRoofs 256, MeshyLighting 16 (lượt chung) + CrashPoliceNavigation 214 (chạy riêng) = **15 suite / 2.017 điều kiện đạt**. Ảnh `Captures/saigon-street-bunbo-cart-removed.png`, `saigon-street-cabinet-panels.png`, `saigon-street-shop-*.png`, ngày/đêm, so sánh phố.
- **Ghi chú hình ảnh:** tấm sáng trắng thấy ở góc chụp chéo cạnh tủ Bún Bò là vách kính tiệm Cơm Tấm bên cạnh phản chiếu bầu trời khi nhìn xiên (vật liệu `PV_StreetGlass` smoothness .92), không phải phần sót của xe đẩy (đã xác nhận bằng bắn tia).
- **Trạng thái:** scene đã lưu, Unity Edit Mode, jobs khóa. **Đã commit/build/push/phát hành1.34.0, bằng chứng ở21.27** (trạng thái chờ của lần dựng đã kết thúc).

### 21.27. Đọc/kiểm tra thay đổi Claude và phát hành phố Sài Gòn — 1.34.0 (24/10/2026, đã phát hành/xác minh live)

- Người dùng yêu cầu trực tiếp đọc lại toàn bộ các cập nhật Claude Code rồi build/phát hành web; đây là ủy quyền phát hành hiện tại, thay cho trạng thái chờ duyệt/build ở 21.19–21.26. Đã đọc toàn bộ huongdan.md, diff mã/scene/asset, script Blender 1.084 dòng và các installer/công cụ gỡ người/xe đẩy/chụp ảnh.
- Phạm vi: 12 mặt tiền/kính/nội thất/biển tủ, 3 cây/chậu/vỉa hè, cột đèn cong/tín hiệu, mây; 735 nhóm/8.050 hộp, 1.260 LOD, 1.350 renderer kính và 420 bóng tín hiệu. Giữ người ngồi xe đậu, gameplay, 4 ô tô Meshy, độ nét mobile và jobs khóa.
- `VerifyJump` bỏ kỳ vọng hình học xe tải cũ “nóc >3m”; kiểm nóc hợp lệ và nằm trong chiều cao mẫu Meshy hiện chọn. Giữ mọi kiểm tra nhảy thật/va chạm; `VerifyMeshyCarRoofs` kiểm thêm 64 quỹ đạo thật. Không nâng hình xe hoặc đổi vật lý để ép test qua. `VerifySaigonRelease.cs` kiểm scene đã lưu/dependency/đủ cảnh phố trong Edit Mode.
- **Hồi quy mới đã đạt 10 suite / 1.642 điều kiện**: StreetArt333, CrashUpgrade93, JunctionRoutes92, SmoothTurns468, NightJumpLighting88, MeshyCarRoofs256, DistanceMode31, Jump44, Gameplay23, CrashPoliceNavigation214. CSGT đạt thêm214 ở lần chạy sau chuỗi suite (không cộng trùng vào1.642); chưa tái hiện lỗi chập chờn của21.23–21.24, không sửa runtime CSGT và không tuyên bố đã chữa lỗi đó. Bằng chứng `saigon-release-Verify*-results.json`, `saigon-release-police-after-suites.json`, `saigon-release-verification-summary.json`. Nâng cấu hình/PlayerSettings/kỳ vọng web1.34.0, **Build WebGL đã thành công**: `build_7f1d3b13c181`, 244.124ms, 88.100.101byte, 0 lỗi/3warning đã biết (Pipeline config, dragging, hudTouchChoice). Cache `28a1dc1e64a78083`, UTC `2026-10-24T05:24:29.4681080Z`; packedAssets đủ vật liệu kính/palette/mesh Generated mới, 0 nguồn MeshyImports vào runtime. Output cuối đã copydocs, loại hash cũ không còn dùng và giữ đúng4file Build, **29 kiểm tra bundle đạt**. Browser đạt14kiểm tra DPR3 và37PWA/offline/cảm ứng,0lỗi runtime; đã xem ảnh dọc/ngang với cảnh phố mới. **Browser local hoàn tất77điều kiện** (14DPR3 +37PWA/offline/cảm ứng +26tài nguyên/WASM),0lỗi runtime. Đã xem ảnhWebGL dọc/ngang đúng cảnh phố mới; `git diff --check` đạt. **Đã commit/push/phát hành và xác minh live như dưới đây**. Giữ ngoài staging nguồn Wave/Blender cũ/Bridge/Packages. Chưa thử thiết bị điện thoại vật lý/FPS/nhiệt độ.

- **Phát hành hoàn tất:** source/artifact `de04b2954376c63af3920629f390834494ba0334`, [Pages run40069532577](https://github.com/denispham1107/taixecongnghe/actions/runs/40069532577) completed/success đúng SHA. **26kiểm tra live đạt,0lỗi runtime**: version/cache và từng tài nguyên khớp artifact đã kiểm (binary từng byte/text chuẩn hóaCRLF), WASM thật khởi tạo/start/pause thành công. Proof`saigon-release-pages.json`, `saigon-release-live-results.json`, ảnh`saigon-release-live-game.png` và `VERIFICATION_SAIGON_WEB_RELEASE.md`.
- **Bàn giao:** 341file source/asset/Generated/ảnh/báo cáo/WebGL của phạm vi phố đã commit/push; đủ77mesh Generated mới và `.meta`, không kéo Wave/5blend cũ/Bridge/Packages vào commit. Đối chiếu SHA256 của203file scene/art trước/sau kiểm tra không đổi. Unity ready/Edit Mode, server docs đã dừng. Khóa index Git cũ không có tiến trình/handle dùng đã được xác minh rồi dọn trước staging; không xóa công việc nguồn. Báo cáo phát hành commit/push riêng, không rebuild cho tài liệu. Nếu PWA giữ bản cũ, đóng hết tab/app rồi mở lại; không xóa Coin/kỷ lục. Chưa thử điện thoại vật lý/FPS/nhiệt độ.

### 21.28. Zoom màn hình dọc gần hơn 10% và dựng lại nhà cao tầng phía sau (24/10/2026, Claude Code — scene đã lưu, chưa build; sau 1.34.0)

- **Yêu cầu:** (1) chơi mobile màn hình dọc thì zoom lại gần hơn 10%; (2) dựng lại tất cả nhà cao tầng phía sau thật chi tiết, mặt chính diện có ô cửa kính lớn trong suốt.
- **Camera dọc:** `PhoVietPortraitHud.PortraitWorldScale` .525 → **.5775** (×1,10): vật thể chiếu to hơn 10%, FOV dọc ≈101,02° → **≈95,62°**. Không đổi camera ngang/desktop, HUD hay cử chỉ. `VerifyPortraitHud` cập nhật kỳ vọng tỉ lệ chiếu 70% → **77%** của bản 1.27.1; đồng thời sửa điều kiện cũ “chạm hai lần trong vùng dưới hất xe ngay” (sai từ cơ chế áp sát 1.30.3; suite này chưa chạy lại từ 1.29) thành “chạm hai lần bắt đầu cú đạp áp sát” (`KickAssistActive`), cùng hợp đồng VerifyQuickKick. `VerifyMobileCamera` (Game View dọc 540×960) xác nhận vẫn thấy đủ 4 làn, có/không truy đuổi. Ảnh `Captures/saigon-street-portrait-zoom.png` (`CaptureSaigonStreet.Portrait`).
- **Nhà cao tầng (`pho_viet_saigon_streets.py::back_tower`, sau shop 1/4/7/10):** 8–11 tầng × 3,2 m, khối 7,2 × 7 m giữ vị trí cũ sau dãy phố. Mặt chính diện quay ra đường: mỗi tầng một vách kính lớn trong suốt (`PV_StreetGlass`) chia ô bằng đố (1,2 m; kiểu lam đứng 1,8 m sâu 0,5 m), đố ngang, dải sàn; sau kính là văn phòng sâu 2 m: sàn, trần trắng có máng đèn, vách sau đổi màu theo tầng, bàn + màn hình, cây cảnh (LOD0). Ba kiểu theo seed: tường kính đố tối; khung bê tông + ban công lan can kính tay vịn inox; lam đứng thép. Hông nhà: gờ tầng, 3 cửa sổ lớn/tầng có khung, đố, bậu. Mái: lan can, đỉnh mặt tiền, phòng kỹ thuật có cửa, bồn nước, cục lạnh, ăng-ten. Cột góc mặt tiền.
- **Ngân sách:** shop có nhà cao tầng LOD0 21.180–26.848 tam giác (dưới ngưỡng kiểm tra 27.879), LOD1 12.548–14.424, LOD2 3.761–4.259. FBX các shop khác được xuất lại (nội dung hình học không đổi).
- **Kiểm tra:** Play Mode ngang: StreetArt 333, CrashUpgrade 93, Gameplay 23, JunctionRoutes 92, DistanceMode 31, NightJumpLighting 88, CrashOutcome 51, SmoothTurns 468, QuickKick 62, JunctionKick 216, Boost 35, Gestures 39, MeshyCarRoofs 256, MeshyLighting 16 + CrashPoliceNavigation 214 (chạy riêng); Play Mode dọc: PortraitHud 21, MobileCamera 8 = **17 suite / 2.046 điều kiện đạt**. Ảnh `saigon-street-towers-high.png`, `saigon-street-tower-front.png`, ngày/đêm và so sánh phố.
- **Trạng thái:** scene đã lưu, Unity Edit Mode (Game View trả về ngang), jobs khóa. **Chưa commit/build/push**; web công khai vẫn 1.34.0, zoom dọc và nhà cao tầng mới chỉ lên web sau khi build/phát hành.

### 21.29. Đổi làn kiểu đánh lái thay cho trượt ngang (24/10/2026, Claude Code — source, chưa build; sau 1.34.0)

- **Yêu cầu:** nhấn A/D (←/→) hoặc vuốt trái/phải trên mobile thì xe và nhân vật đang trượt ngang sang làn khác, không hợp lý; sửa hướng di chuyển cho đúng logic rẽ trái/phải.
- **Lỗi cũ (`PhoVietGame.TickRide`):** `x=MoveTowards(x, LaneX, 15·dt)` — vận tốc ngang bật ngay 15 m/s rồi dừng gắt, đầu xe luôn thẳng (đi ngang như cua); `lean=(baseX−x)·−35` còn **nghiêng ngược chiều** (sang phải lại nghiêng trái, trái quy ước cua giao lộ `PhoVietRoutes` z>0 = nghiêng trái).
- **Mới (tham số hãm/trả lái/nghiêng đã đổi ở 21.31):** `laneVelocity` tăng/giảm với gia tốc `LaneAccel=150 m/s²`, trần `LaneSpeed=15 m/s`, vận tốc mong muốn `sign·min(15, √(2·a·|còn lại|))` nên vào làn êm, không vượt quá làn đích (chặn vượt). Một làn 3,1 m mất ~0,25–0,27 s (trước ~0,21 s) — giữ khả năng né. Góc lái `steerYaw = atan2(laneVelocity, max(Speed,8))` giới hạn ±26°, làm mượt `1−e^(−18dt)`; nghiêng `−atan(Speed·ψ̇/g)` giới hạn ±24°, làm mượt `1−e^(−12dt)` → đầu đổi làn nghiêng vào phía rẽ, cuối nghiêng ngược nhẹ để dựng thẳng. Đang đạp áp sát (`KickAssistActive`) hoặc cua giao lộ thì góc lái về 0 để giữ khớp mẫu giày/cua.
- **Phạm vi hình/va chạm:** góc lái chỉ xoay phần hình (`BikeVisualAnimator` nhân `Euler(0, RiderSteerYaw, 0)` vào `visual.localRotation`, kể cả khi nhảy); gốc `rider` (hộp va chạm `riderBody`, vật cản cảnh, nóc xe) giữ hướng đường, chỉ có độ nghiêng như trước nhưng đúng chiều. Khi va chạm, `BeginRiderImpact` chuyển góc lái hiện tại vào hướng thật của cú ngã (không bật về thẳng). Kết thúc cua/rebase (`PhoVietRoutes`) và `ResetRide` đặt lại `laneVelocity/steerYaw`. Không xoay tay lái vì tay người lái gắn xương tay, xoay riêng tay lái sẽ làm tay rời tay nắm.
- **Kiểm tra mới `AgentScripts/VerifyLaneSteering.cs` (87 điều kiện):** dt 1/60, 1/30, 0,05 × trái/phải × tốc độ 17/30: tới làn ≤0,42 s, chỉ tiến về làn đích và không vượt, đỉnh góc lái cùng chiều 6–26°, nghiêng đầu tiên vào phía rẽ, dựng thẳng sau khi xong, hộp va chạm không xoay; phần hình xoay đúng bằng góc lái quanh trục đứng; đổi ý giữa chừng quay về làn cũ không giật. Ảnh `Captures/lane-steering-left.png`, `lane-steering-right.png`, `lane-steering-closeup.png`.
- **Hồi quy (Play Mode):** LaneSteering 87, Gameplay 23, DistanceMode 31, QuickKick 62, JunctionKick 216, SmoothTurns 468, Jump 44, Boost 35, CrashOutcome 51, Gestures 39, CrashUpgrade 93, JunctionRoutes 92, MeshyCarRoofs 256, NightJumpLighting 88, StreetArt 333, RiderContacts 45, TrafficDynamics 82, MeshyRiders 356, CrashPoliceNavigation 214 (chạy riêng); dọc: PortraitHud 21, MobileCamera 8 = **21 suite / 2.644 điều kiện đạt**.
- **Lỗi cũ không do thay đổi này:** `VerifyRoofRiding` 2.292/2.304 — 12 ca xe tải BoxTruck/DrowsyTruck 28 m/s offset 0,5 bị té; suite đạt 2.304 ở 1.32 nhưng chưa chạy lại từ khi đổi ô tô Meshy 1.33 (mẫu tải chọn ngẫu nhiên lúc spawn, đã thay bằng `VerifyMeshyCarRoofs` 256 đạt). Trong suite này người chơi không đổi làn nên đường đi trùng hệt mã cũ. **Đã chẩn đoán và sửa test ở 21.30** (giả định hình học cũ của test, không phải lỗi vật lý nhảy).
- **Trạng thái:** chỉ đổi mã (`PhoVietGame.cs`, `PhoVietRoutes.cs`, `PhoVietRiderCrash.cs`, `BikeVisualAnimator.cs`) + suite mới; scene không đổi. **Chưa commit/build/push**; web vẫn 1.34.0.

### 21.30. Sửa VerifyRoofRiding/VerifyTruckJump cho tải Meshy (24/10/2026, Claude Code — chỉ sửa test, chưa commit/build; sau 1.34.0)

- **Tái hiện:** Play Mode, `VerifyRoofRiding` cũ cho 2.294/2.304 (lần trước 2.292). Số ca hỏng **thay đổi giữa các lần chạy**: pool `PV_BoxTruck`/`PV_DrowsyTruck` chọn ngẫu nhiên thùng bạt `PV_MeshyCovered` (6,4 m) hoặc đầu kéo container `PV_MeshyContainer` (12,8 m) cả lúc `RestartRide` (`ResetTrafficDynamics` → `InitializeDriver`) lẫn lúc `SpawnTraffic`. `VerifyTruckJump` cũ dừng ngay ở “exposes its Blender cargo roof” (đòi nóc 3,175 m).
- **Gốc lỗi (chẩn đoán ghim 2×2 mẫu trước/sau spawn, 4 dt, 3 offset):** ca hỏng phụ thuộc **chỉ** vào mẫu sau spawn = container + offset +0,5 + 28 m/s; mẫu dùng để tính `contactDistance` không quyết định (chỉ lệch thời điểm vài ms). Theo vết từng tick: test đặt **tâm** xe dưới người lúc chạm nóc; với xe dài 12,8 m vượt từ sau (tương đối ~11 m/s trước khi bay, ~8 m/s khi bay) thì mép đầu xe (nửa dài 6,4 + 0,91 m đuôi xe máy) tới người ở t≈0,30–0,40 s sau khi bấm (windup 0,24 s), lúc người mới cao ~1,1–1,4 m < mũi cabin 1,59 m → bị tông đuôi thật, tức một **cú nhảy muộn**. Thùng bạt 6,4 m tới ở ~0,6 s khi người đã cao ~3,9 m nên luôn đáp được; tải Blender cũ cũng ngắn nên bản 1.32 đạt. Hành vi game đúng luật 19.2 (nhảy muộn/thiếu cao vẫn té thật); **không sửa `PhoVietJump.cs`**.
- **Sửa `AgentScripts/VerifyRoofRiding.cs`:** với pool có `MeshyCarVariants`, lặp từng mẫu và `Select` mẫu đó trước khi đo quỹ đạo tự do và ngay sau `SpawnTraffic`; độ cao chạm lấy `SupportHeight(-.73,.90)` của profile mẫu (container ~2,50 m, không phải `size.y` 2,63 m). Ghi quỹ đạo tự do thật (thời gian, độ cao, quãng đường) ở lượt đo; mỗi ca xác định **nhảy muộn** độc lập với lượt thử: quét giữa các frame (20 bước nội suy như kiểm tra va chạm của game) trong pha lên, nếu thân xe (|z cục bộ| ≤ nửa dài + 0,91) đã chạm đường bay khi người còn thấp hơn `SupportHeight` tại đó quá 0,04 m. Ca thường giữ nguyên ba kiểm tra đáp nóc/không té/được đỡ; ca muộn đòi: không hề được nóc đỡ, `Falling` thật, và va chạm xảy ra trong pha lên trước đỉnh. Nhãn ghi `climbMargin` để thấy biên. Kết quả **768 cú nhảy / 3.072 điều kiện đạt**, 8 ca muộn (container 28 m/s +0,5 không W ở cả 4 dt và hai pool), biên ca muộn −0,15…−0,18 m, ca đáp gần nhất +0,55 m — tách rõ, chạy lại hai lần cùng kết quả. Có W thì người nhanh hơn nên offset +0,5 vẫn đáp.
- **Sửa `AgentScripts/VerifyTruckJump.cs`:** lặp hai pool × hai mẫu đã ghim; thời điểm spawn tính từ quỹ đạo tự do thật xuống nóc mẫu đó (thay `25,1 − v·1,29` và `47,4` cứng); “mount” đặt người ở `SupportHeight` thật (thay 3,2 m); kiểm `JumpRoofHeight` = `size.y` mẫu và profile đúng mẫu; tìm điểm cabin thấp nhất ở nửa trước (thùng bạt 2,20 m tại z 2,3; container 2,13 m tại z 5,65) và kiểm cabin thấp hơn nóc hàng ≥0,3 m trước khi thử “từ cabin thấp lao vào vách hàng phải va chạm”. Đoạn xe ngủ gật lạng: thùng bạt có bậc thùng→cabin nên phải rơi vật lý; container là dốc mái gió liên tục nên được theo mặt, và cả hai không được tụt quá 0,12 m trong một bước 10 ms (đo: 0,005 / 0,084 m). Giữ kiểm tra camera đỉnh 4,845/8,045 m (8,045 cao hơn nóc Meshy thực nên chặt hơn). **125 điều kiện đạt.**
- **Hồi quy (Play Mode, ngang):** RoofRiding 3.072, MeshyCarRoofs 256, Jump 44, TruckJump 125 = **4 suite / 3.497 điều kiện đạt**. Bằng chứng `Captures/roof-meshy-Verify{RoofRiding,MeshyCarRoofs,Jump,TruckJump}-results.json` (JSON thô của `run_script`, đọc `data.result.result`). Sau Play Mode `PV_Racer.mat`/`PV_TrafficPolice.mat` không đổi, scene không bẩn, Unity về Edit Mode; không đụng scene/asset/các `.blend`.
- **Ghi chú vật lý chưa sửa (không thuộc phạm vi):** ngưỡng bám mặt nóc 0,12 m/frame của `ResolveJumpSurface` phụ thuộc dt — dốc mái gió container ở tốc độ tương đối cao có thể được “bám” ở dt nhỏ nhưng rơi ở dt lớn. Chưa thấy lỗi gameplay, chỉ ghi để theo dõi.
- **Trạng thái:** chỉ đổi hai file test + JSON bằng chứng + tài liệu. `RideJobsReleased=false`, `jobsEnabled=false` giữ nguyên. **Chưa commit/build/push**; web vẫn 1.34.0.

### 21.31. Lấy lại thăng bằng từ từ sau khi đổi làn (24/10/2026, Claude Code — source, chưa build; sau 1.34.0)

- **Yêu cầu:** hướng sang trái/phải đã đúng, nhưng xe lấy lại thăng bằng quá nhanh, trông giả; phải vừa chạy tới vừa lấy lại thăng bằng từ từ.
- **Nguyên nhân:** 21.29 hãm vận tốc ngang 150 m/s² và trả góc lái với hệ số 18/s, nên tới làn là đầu xe/độ nghiêng về thẳng gần như tức thì; độ nghiêng tính theo tốc độ quay đầu còn tạo nghiêng ngược lớn khi trả lái.
- **Sửa `PhoVietGame.TickRide`:** vận tốc mong muốn `sign·min(15, √(2·LaneBrake·|còn lại|))` với `LaneBrake=60 m/s²` (tăng tốc vẫn `LaneAccel=150`); góc lái bẻ vào nhanh (hệ số 18/s) nhưng **trả lái chậm** (`SteerReturn=4,5/s`); độ nghiêng đi theo góc lái `lean=−clamp(0,85·steerYaw, ±24°)` làm mượt 9/s — sang phải nghiêng phải, trả lái thì dựng lên cùng nhịp; bỏ nghiêng theo tốc độ quay đầu.
- **Số đo (VerifyLaneSteering, dt 1/60–0,05, tốc độ 17/30):** 90% làn sau 0,25–0,27 s, vào hẳn tâm làn 0,30–0,32 s; đỉnh góc lái 22–26°; từ đỉnh tới <2° mất 0,55–0,65 s; tốc độ trả lái tối đa 81–106°/s. Ảnh chuỗi `Captures/lane-steering-recovery.png` (0,10/0,25/0,45/0,65/0,90 s).
- **Kiểm tra:** `VerifyLaneSteering` nay **123 điều kiện** (thêm: 90% làn ≤0,36 s, vào tâm ≤1,2 s, trả lái ≥0,3 s, tốc độ trả lái ≤125°/s). `VerifyGameplay` nới thời gian chờ đổi 2 làn 0,5 → 0,7 s và 3 làn 0,8 → 1,0 s (kiểu mới 0,59/0,8 s; kiểu trượt cũ 0,41/0,62 s). Hồi quy đạt: LaneSteering 123, Gameplay 23, DistanceMode 31, QuickKick 62, JunctionKick 216, SmoothTurns 468, Jump 44, Boost 35, CrashOutcome 51, Gestures 39, CrashUpgrade 93, JunctionRoutes 92, MeshyCarRoofs 256, NightJumpLighting 88, RiderContacts 45, TrafficDynamics 82, MeshyRiders 356, RoofRiding 3.072, TruckJump 125 (đã sửa ở 21.30), CrashPoliceNavigation 214 (chạy riêng), PortraitHud 21, MobileCamera 8 = **22 suite / 5.544 điều kiện**.
- **Ảnh hưởng gameplay:** mỗi lần đổi làn chậm hơn kiểu trượt cũ ~0,1 s (1 làn) đến ~0,18 s (2–3 làn) vì có pha tăng tốc/hãm êm; hộp va chạm vẫn đi theo vị trí thật, không xoay.
- **Trạng thái:** chỉ đổi mã `PhoVietGame.cs` + test; scene không đổi. **Chưa commit/build/push**; web vẫn 1.34.0.

### 21.32. Build/phát hành 1.35.0 — đổi làn đánh lái, nhà cao tầng kính, zoom dọc (24/10/2026, Claude Code)

- **Ủy quyền:** người dùng yêu cầu trực tiếp “Đổi làn, nhà cao tầng và zoom dọc đã ổn, hãy commit, build WebGL và phát hành lên web” — gồm commit, build, push main và GitHub Pages.
- **Phạm vi:** 21.28 (zoom dọc, nhà cao tầng), 21.29 + 21.31 (đổi làn đánh lái, lấy lại thăng bằng từ từ), 21.30 (sửa test RoofRiding/TruckJump cho tải Meshy). Công việc vẫn khóa (`RideJobsReleased=false`, `jobsEnabled=false`); không kéo 5 `.blend` cục bộ cũ, Wave/WaveAlpha, Meshy Bridge (`Assets/ai.meshy`), `Packages/*` hay ảnh/JSON lịch sử chưa track vào commit.
- **Version:** 1.34.0 → **1.35.0** ở `PhoVietWebBuild.cs`, `ConfigureWebRelease.cs`, `PlayerSettings.bundleVersion`, kỳ vọng `VerifyWeb.cjs`, `VerifyFullscreenGame.cjs`, `VerifyCrashPoliceWeb.cjs`, `VerifyMorningFogWeb.cjs`, `VerifyWebBundle.py`; README thêm mục 1.35.0.
- **Hồi quy native trước build:** 22 suite / 5.544 điều kiện (21.31) trên đúng mã/scene phát hành. Preflight `VerifySaigonRelease` (Edit Mode): scene đã lưu, jobs khóa, 735 nhóm/8.050 hộp vật cản, 1.260 LOD phố, 1.350 renderer kính, 420 bóng tín hiệu, không thiếu dependency → `Captures/release-1.35-preflight.json`.
- **Build:** `build_8f58593f4444` **Succeeded**, 250.356 ms, 0 lỗi / 3 cảnh báo đã biết (Pipeline thiếu RuntimePipelineConfig; `dragging`, `hudTouchChoice` không đọc ở nhánh DOM). Cache **`d06d08624dba54fc`**, UTC `2026-10-24T08:23:22.2948967Z`, tài nguyên precache 88.417.715 byte (data 81.173.476 byte, dưới giới hạn 100 MB). Đã chép sang `docs`, dọn đúng 3 file hash cũ trong `docs/Build` sau kiểm tra đường dẫn; còn đúng 4 file Build.
- **Kiểm tra artifact local:** 29 kiểm tra bundle (`release-1.35-bundle-checks.json`; script ghi đè `saigon-release-bundle-checks.json` đã được khôi phục về bằng chứng 1.34.0). Trình duyệt local **77 điều kiện, 0 lỗi runtime**: DPR3 dọc/ngang/tablet/xoay lại 14 (`release-1.35-mobile-results.json`, đã xem ảnh dọc với zoom mới/nhà cao tầng), đối chiếu tài nguyên + WASM 26 (`release-1.35-local-results.json`), PWA/offline/cảm ứng 37 (`browser-release-1.35-pwa.json`, bị gitignore). Playwright dùng qua `NODE_PATH` tới bộ phụ thuộc Node có sẵn trên máy; không cài thêm. Giả lập trình duyệt, chưa thử điện thoại vật lý/FPS.
- **Phát hành hoàn tất:** source/artifact **`b8c7b9f5d69da959a4f55ae65f98cc8b574fda19`** đã push main, [Pages run 40078793874](https://github.com/denispham1107/taixecongnghe/actions/runs/40078793874) completed/success đúng SHA. Bản công khai trả version 1.35.0 / cache `d06d08624dba54fc`; **26 kiểm tra live đạt, 0 lỗi runtime** (từng tài nguyên khớp artifact đã kiểm, binary từng byte/text chuẩn hóa CRLF; WASM thật khởi tạo/start/pause). Bằng chứng `Captures/release-1.35-live-results.json`, `release-1.35-pages.json`, ảnh `release-1.35-live-game.png`, `VERIFICATION_RELEASE_1_35.md`. Trạng thái Pages đọc qua GitHub API công khai (công cụ `.tools/GetMeshyPagesRun.ps1` không lấy được credential trong phiên này; không in hay lưu token).
- **Bàn giao:** Unity Edit Mode, server docs kiểm tra đã dừng, jobs khóa, dữ liệu Coin/kỷ lục không đổi. Báo cáo phát hành commit/push riêng, không rebuild cho tài liệu. Nếu PWA còn bản cũ, đóng hết tab/app Phố Việt rồi mở lại; không xóa dữ liệu. Chưa thử điện thoại vật lý/FPS/nhiệt độ.

### 21.33. Xe tăng Meshy “Iron Sentinel” trong giao thông (24/10/2026, Claude Code — phát hành 1.36.0 ở 21.34)

- **Nhận mô hình:** Meshy Bridge đã nhập `Assets/MeshyImports/Iron Sentinel_20261024_170520/` (FBX 14.153 tam giác, `meshy_basecolor/normal/metallic_smoothness.png` 2K) và tự thả một bản vào scene; bản thả được trình cài gắn `EditorOnly` + tắt, không vào build. Không sửa file nguồn.
- **Yêu cầu:** xe tăng xuất hiện ngẫu nhiên như ô tô con; chỉ một màu gốc; va chạm với mọi phương tiện thì xe tăng không đổi hướng/không dừng, phương tiện kia văng ra; luôn chậm hơn các ô tô khác 60%.
- **Mô hình (`ArtSource/Blender/integrate_meshy_tank.py` → `Assets/PhoViet/Art/Cars/Meshy/PV_MeshyTank.*`, nguồn `ArtSource/Blender/PhoViet_MeshyTank.blend`):** nòng pháo nguồn hướng −X → +Z (hướng chạy); scale đồng nhất dài **7,0 m** gồm nòng (thân ~5,1 m, nòng nhô 1,9 m), rộng **2,51 m** (vừa làn 3,1 m), cao 2,10 m. Giữ mọi mặt/UV/pháp tuyến nguồn; xích/gầm gắn `Frame`, thân trên `Body`. Rig 15 xương chuẩn ô tô Meshy; hai trục bánh là xương ảo không gắn mặt (xe tăng không có bánh quay), `wheelBinding=virtual-axles-no-geometry`. 7 clip Legacy, 3 LOD **14.153 / 6.227 / 2.264** tam giác. Profile nóc 49 mẫu bằng giao tia (phía trước là nòng pháo ~1,40 m, tháp pháo ~1,6–1,8 m). Maps nguồn chép nguyên; `PaintMask`/`Emission` đen hoàn toàn → không đổi sơn, không vùng đèn phát sáng. Metadata `armored=true`, `speedScale=0.4`, `fixedPaint=true`.
- **Unity:** `PhoVietMeshyCarUpgrade.Models` thêm `PV_MeshyTank`; ô Sedan/Hatchback/Taxi (5 ô) có biến thể `[PV_MeshySport, PV_MeshyTank]`, trọng số xe tăng `TankWeight=0.5` → ~1/3 số lần ô xe con xuất hiện. `AgentScripts/InstallMeshyTank.cs` chạy lại `Install()` (giữ 11 root/pool, 30 binding đèn, CSGT nguyên trạng, jobs khóa) rồi lưu scene. `MeshyCarVariants`: `Variant.weight/armored/fixedPaint/speedScale`, chọn theo trọng số, `PrepareForSpawn(…, allowArmored)`; `Select` gán `TrafficVehicle.armored/speedScale` và dùng màu trắng (không nhuộm) cho mẫu `fixedPaint`.
- **Tốc độ:** `InitializeDriver` nhân `speedScale` vào `speed/baseTrafficSpeed/crossSpeed` ngay khi xuất hiện (cùng chiều 7–12 → 2,8–4,8 m/s, ngược chiều −10…−17 → −4…−6,8 m/s, băng ngang và đoạn rẽ dùng `crossSpeed` đã nhân). Xe vượt lên từ phía sau (`SpawnExitTraffic` rear) không bao giờ chọn xe tăng vì xe chậm không thể vượt.
- **Va chạm:** xem 7.3 — `armored` = khối lượng vô hạn với xe–xe, người chơi và CSGT; xe tăng không `knocked`, không đổi làn/hướng/tốc độ; người chơi đâm xe tăng thì té như thường; CSGT đâm xe tăng thì ngã 1,2 s.
- **Kiểm tra mới `AgentScripts/VerifyMeshyTank.cs` (~1.1k điều kiện, số thay đổi theo mẫu ngẫu nhiên):** 5 ô có mẫu xe tăng đủ LOD/vật liệu/kích thước; tỉ lệ xuất hiện **31,2%** (3.000 lần, kỳ vọng 1/3 ±3%); không bao giờ chọn khi vượt từ sau; 8 chỉ số sơn đều giữ màu gốc; tốc độ trung bình xe tăng/ô tô **0,403**; băng ngang đúng 40%; ngược chiều đúng khoảng; 4 kịch bản va chạm (ô tô tông đuôi xe tăng, xe tăng tông đuôi ô tô, xe máy tông xe tăng, xe tăng gặp ô tô ngược chiều): xe tăng không bao giờ dừng/chậm/lệch làn/đổi hướng, xe kia văng (Δv 8,9–16,9 m/s); người chơi tông xe tăng thì té, xe tăng không bị đẩy. Ảnh `Captures/meshy-tank-sheet.png` (cận trên phố, bên hông, góc chơi, khoảnh khắc xe thể thao tông đuôi bị nảy) — `AgentScripts/CaptureMeshyTank.cs`.
- **Sửa test do ô xe con nay có thể mang hình xe tăng 7 m:** `VerifyMeshyCars` kỳ vọng 2 mẫu ở ô xe con; `VerifyTrafficDynamics` phân loại ô theo vai trò (có mẫu Covered = xe tải) và dùng ô tô thường (bỏ lượt ra xe tăng) cho các bài vật lý xe–xe/băng ngang; `VerifyCrashPoliceNavigation` chọn đúng ô xe tải cho chướng ngại (trước đây có thể nhặt ô đang là xe tăng → CSGT đâm không đẩy được, ngã liên tục); `VerifyRoofRiding` thử mẫu xe tăng ở tốc độ thật (×0,4) và chấp nhận va chạm thật khi đã đáp lên nòng/sàn trước rồi gặp tháp pháo cao hơn ≥0,2 m (luật “đang trên nóc gặp phần cao hơn thì đâm”, 9 ca bước thô 0,05/0,1 s) — `stepCases` trong JSON. Không đổi vật lý để ép test qua.
- **Hồi quy (Play Mode, một phiên chạy liên tục):** MeshyTank 1.129, MeshyCars 1.266, MeshyCarRoofs 256, TrafficDynamics 82, CrashUpgrade 93, RiderContacts 45, CrashOutcome 51, Gameplay 23, DistanceMode 31, QuickKick 62, JunctionKick 216, SmoothTurns 468, Jump 44, TruckJump 125, RoofRiding 4.206, Boost 35, BoostTrafficIsolation 478, NightJumpLighting 88, LaneSteering 123, StreetArt 333, JunctionRoutes 92, RoadDrivers 45, CrashPoliceNavigation 214 (cũng đạt 3/3 lần chạy riêng), MeshyLighting 16 = **24 suite / 9.521 điều kiện đạt**. Bằng chứng `Captures/meshy-tank-verification-summary.json`, `meshy-tank-Verify*-results.json`, `meshy-tank-install.json`, `meshy-tank-export.json`.
- **Trạng thái:** scene đã lưu, Unity Edit Mode, jobs khóa (`RideJobsReleased=false`, `jobsEnabled=false`). Build/phát hành **1.36.0** ở 21.34. Chưa thử điện thoại thật.

### 21.34. Build/phát hành 1.36.0 — xe tăng Meshy (24/10/2026, Claude Code)

- **Ủy quyền:** người dùng yêu cầu trực tiếp “Build webGL và phát hành lên web nhé” sau khi nhận báo cáo xe tăng (21.33) — gồm commit, build, push main và GitHub Pages.
- **Phạm vi:** 21.33 (xe tăng Meshy, va chạm khối lượng vô hạn, tốc độ ×0,4, sửa 4 test). Commit gồm cả nguồn Meshy đã nhận `Assets/MeshyImports/Iron Sentinel_20261024_170520/` (như các mẫu ô tô Meshy trước, để chạy lại `integrate_meshy_tank.py`) và `PhoViet_MeshyTank.blend`. Công việc vẫn khóa (`RideJobsReleased=false`, `jobsEnabled=false`). Không kéo vào commit: 5 `.blend` cục bộ cũ, các thay đổi FBX/JSON/Palette xe máy Wave cũ trong `Art/Bikes/` (từ 23/10, scene dùng xe Meshy ở `Art/Bikes/Meshy*`), Wave/WaveAlpha, Meshy Bridge (`Assets/ai.meshy`), `Packages/*`, ảnh/JSON lịch sử đã đổi do chạy lại test.
- **Version:** 1.35.0 → **1.36.0** ở `PhoVietWebBuild.cs`, `ConfigureWebRelease.cs`, `PlayerSettings.bundleVersion`, kỳ vọng `VerifyWeb.cjs`, `VerifyFullscreenGame.cjs`, `VerifyCrashPoliceWeb.cjs`, `VerifyMorningFogWeb.cjs`, `VerifyWebBundle.py`; README thêm mục 1.36.0. `VerifyWebBundle.py` giờ ghi `Captures/release-1.36-bundle-checks.json` (không ghi đè bằng chứng 1.34.0 nữa).
- **Hồi quy native trước build:** 24 suite / 9.521 điều kiện (21.33) trên đúng mã/scene phát hành. Preflight `VerifySaigonRelease` (Edit Mode): scene đã lưu, version 1.36.0, jobs khóa, 735 nhóm/8.050 hộp vật cản, 1.260 LOD phố, 1.350 renderer kính, 420 bóng tín hiệu → `Captures/release-1.36-preflight.json`.
- **Build:** `build_fa643a0c8cd3` **Succeeded**, 244.218 ms, 0 lỗi / 4 cảnh báo: 3 cảnh báo đã biết (Pipeline thiếu RuntimePipelineConfig; `dragging`, `hudTouchChoice` không đọc) + “uncompiled code changes” do sửa chuỗi version trong `PhoVietWebBuild.cs` (Editor) ngay trước build — `bundleVersion`/`build-info.json` đã đúng 1.36.0 qua `ConfigureWebRelease`, mã game runtime không đổi so với bản đã test. Cache **`2ad4c813b6952999`**, UTC `2026-10-24T11:18:50.2444606Z`, tài nguyên precache 96.312.131 byte (data 89.067.822 byte, tăng ~7,9 MB do xe tăng; dưới giới hạn 100 MB/file). Đã chép sang `docs`, dọn đúng 3 file hash cũ trong `docs/Build` sau kiểm tra đường dẫn; còn đúng 4 file Build.
- **Kiểm tra artifact local:** 29 kiểm tra bundle (`release-1.36-bundle-checks.json`). Trình duyệt local **77 điều kiện, 0 lỗi runtime**: DPR3 dọc/ngang/tablet/xoay lại 14 (`release-1.36-mobile-results.json`, ảnh `release-1.36-mobile-portrait.png`/`-landscape.png`), đối chiếu tài nguyên + WASM 26 (`release-1.36-local-results.json`), PWA/offline/cảm ứng 37 (`browser-release-1.36-pwa.json`, bị gitignore). Playwright qua `NODE_PATH` tới bộ phụ thuộc Node có sẵn trên máy. Giả lập trình duyệt, chưa thử điện thoại vật lý/FPS.
- **Phát hành hoàn tất:** source/artifact **`c49059c6dc83ef970c758c6e2147b48890feb260`** đã push main, [Pages run 40090621801](https://github.com/denispham1107/taixecongnghe/actions/runs/40090621801) completed/success đúng SHA. Bản công khai trả version 1.36.0 / cache `2ad4c813b6952999`; **26 kiểm tra live đạt, 0 lỗi runtime** (tài nguyên khớp artifact đã kiểm; WASM thật khởi tạo/start/pause). Bằng chứng `Captures/release-1.36-live-results.json`, `release-1.36-pages.json`, ảnh `release-1.36-live-game.png`, `VERIFICATION_RELEASE_1_36.md`. Trạng thái Pages đọc qua GitHub API công khai (không in hay lưu token).
- **Bàn giao:** Unity Edit Mode, server docs kiểm tra đã dừng, jobs khóa, dữ liệu Coin/kỷ lục không đổi. Báo cáo phát hành commit/push riêng, không rebuild cho tài liệu. Nếu PWA còn bản cũ, đóng hết tab/app Phố Việt rồi mở lại; không xóa dữ liệu. Chưa thử điện thoại vật lý/FPS/nhiệt độ.
- **Kiểm tra lại khi người dùng báo “chưa thấy xe tăng” (24/10, sau phát hành):** bản công khai trả `build-info.json` 1.36.0 / cache `2ad4c813b6952999`, data trên Pages đúng 89.067.822 byte và chứa `PV_MeshyTank`; scene có 5 biến thể xe tăng (`weight 0.5`, `speedScale 0.4`, `armored 1`). Chơi thật bản live bằng Chrome có GPU (Playwright, 150 khung hình/≈4,6 phút, tự chơi lại khi té): **thấy xe tăng 2 lần** — `Captures/release-1.36-live-tank-junction.png` (xe tăng ở làn giữa trước người chơi) và `release-1.36-live-tank.png` (người chơi tông xe tăng, xe tăng không bị đẩy, người chơi té). Đo lượt chơi tự nhiên trong Editor bằng `AgentScripts/ProbeTankGameplay.cs` (600 s game, không tắt giao thông bất ngờ, tự chơi lại 123 lần): 910 xe xuất hiện, 137 ở ô xe con, **42 xe tăng (~1 xe/14 s, lần đầu sau 16,7 s)**, xe tăng hiển thị 3.859 khung. Kết luận: bản web đã có xe tăng; nếu máy người dùng không thấy, khả năng cao PWA/tab cũ còn giữ bản 1.35.0 (service worker không cưỡng bức thay binary giữa lượt) hoặc lượt chơi quá ngắn. Hướng dẫn: đóng hết tab/app Phố Việt (kể cả ứng dụng đã cài), mở lại 1–2 lần; mở `…/taixecongnghe/build-info.json` để xem version. Không cần build lại.

### 21.35. Cầu vượt sông theo ảnh mẫu (24/10/2026, Claude Code — phát hành 1.37.0 ở 21.44)

- **Yêu cầu:** quan sát ảnh mẫu (`ArtSource/Reference/River/river_bridge_reference.png`: cầu 2 làn vạch vàng giữa, lan can trắng có chậu hoa hồng, cột đèn cong treo cờ đỏ sao vàng, sông xanh ngọc, kè đá có dây leo, thuyền gỗ mái che chở hàng, phố hai bên) và dùng Blender MCP dựng thêm cầu/sông/thuyền; liền mạch với phố cũ; đi một lúc là phải gặp cầu (người dùng giải thích “50%” chỉ để diễn đạt ý này); cầu đúng 2 làn; người chơi hoặc phương tiện không đi đúng 2 làn cầu thì rơi xuống sông; người chơi rơi là kết thúc; xe lạng lách ra ngoài phạm vi cầu cũng rơi. Người dùng chọn: **2 làn giữa** và **xe thường tự nhập làn cầu**.
- **Bố cục khối cầu (tọa độ khối 80 m, `RiverBlock`):** *(lịch sử — từ 21.46 mặt nước z 4,8–55,2, hai bờ ngắn lát lại, xem 21.46)* mặt nước z **16,8–42,0** (đúng mép tấm vỉa hè 8,4 m), cao −2,3 m (**đã hạ xuống −3,2 m, đáy −7,4 m ở 21.36**), kéo dài x ±95 m (sương che phần xa, đầu sông có vách đá). Hai bờ z 0–16,8 và 42–63 giữ nguyên nhà phố/vỉa hè/cây/biển/đèn tín hiệu và giao lộ z 63–77 → liền mạch; nhà ở hai bờ thành nhà ven sông, nhà của đường ngang ở giao lộ phía trước thành dãy nhà bờ bên kia. Bề rộng sông bị giới hạn bởi chiều sâu nhà của đường ngang (tới 25,8 m tính từ mặt tiền) nên mặt nước ~25 m.
- **Mô hình Blender (`pho_viet_river_bridge.py`, nguồn `PhoViet_RiverBridge.blend`, dựng trong Blender MCP đang mở, scene riêng `PV_RiverBridge`):** mặt nước + mặt nạ stencil; kè đá xếp khối so le 6 hàng, vệt ẩm sát nước, gờ đá; 24 cụm dây leo rủ; lối dạo lát gạch |x| 26–95 có lan can sắt (lan can kè chỉ từ |x| 7,15 trở ra — **làn ngoài của đường không có lan can** để xe đi thẳng rơi được); chậu cây kiểu cây mẫu; cầu: mặt nhựa đường 2 làn |x| ≤ 3,1, vạch trắng mép, vạch vàng đứt cùng nhịp phố, gờ bó trắng 3,1–3,4, lan can trắng 3,4–3,7 cột vuông 2,1 m + con tiện, 8 hộp hoa giấy hồng, 4 cờ trên cột đèn, dầm, 2 trụ đá + mố; vạch chéo thu hẹp làn ngoài ở hai bờ, gờ vàng–đen mép kè, 2 biển tam giác “CẦU HẸP 2 LÀN”; 4 cầu tàu gỗ. Thuyền `PV_RiverBoat0` (9 m, mái vòm xanh lá, ca bin, cờ) và `PV_RiverBoat1` (6,4 m, mái xanh dương), hàng hóa nhiều màu. LOD 45.474/15.008/2.312 tam giác (thuyền 496/304/196 và 420/228/120). Unity tách submesh theo thứ tự vật liệu xuất hiện lần đầu, nên JSON ghi `slots` theo thứ tự đó (lần đầu gán theo slot Blender làm atlas nhận nhầm vật liệu mặt nạ — đã sửa).
- **Unity — cài đặt (`PhoVietRiverUpgrade.Install`, menu “Phố Việt/Art/Install river bridge blocks”, chạy qua `AgentScripts/InstallRiverBridge.cs`):** cả **35 khối** của 5 dãy đường: thêm `RiverBlock` và con “Cầu vượt sông • Blender” (FBX xoay −90° X vì mesh còn trục Z lên; LODGroup .12/.04/.002), 4 thuyền `RiverBoat` (nhấp nhô/lắc nhẹ), 4 đèn LED phố (`PV_LEDStreetLamp`, x ±3,55, z 22/36,8) đăng ký vào `DayNightLighting.streets` (thêm 140) và `emissionUsers`. Mỗi khối ghi `streetOnly` (17 đối tượng có z nằm trên lòng sông: 6 nhà, 6 tấm vỉa hè, 2 cây, 2 xe máy đậu, nhóm đèn phố) và thay 6 mesh gộp `Geometry • …` bằng bản **cắt bỏ dải z 16,8–42** (Sutherland–Hodgman, giữ pháp tuyến/UV/tangent; 42 mesh asset trong `Art/River/Clipped/`). Vật liệu: atlas phố, `PV_RiverWater` (shader `PhoViet/River Water`), `PV_RiverGroundMask` (`PhoViet/River Ground Mask`), nhựa đường/vạch trắng/vạch vàng lấy đúng vật liệu mesh gộp của phố. “Nền thành phố” đổi sang `PV_CityGround` (`PhoViet/City Ground`, Standard + stencil NotEqual 1) — mặt nạ ở hàng đợi Geometry−1 ghi stencil trên lòng sông nên nền không che nước. Cài lặp lại an toàn (trả mesh gốc/hiện đối tượng trước khi tính lại). Kết quả `Captures/river-bridge-install.json`.
- **Shader nước (bản đầu, đã thay ở 21.36):** sóng sin nhiều hướng tính theo tọa độ khối (sóng đi cùng khối khi phố cuộn), màu sâu/nông, phản chiếu trời theo Fresnel từ cubemap phản chiếu, lấp lánh nắng, sương; không nhận bóng (độ sâu ở lòng sông là của nền thành phố).
- **Luật xuất hiện (`PhoVietRiver.cs`):** xem 9.1. Khối xuất phát và khối kế sau luôn là phố; sau cầu luôn là phố; tối đa 2 khối phố liền nhau thì bắt buộc cầu; còn lại 50% → gặp cầu sau tối đa ~240 m (đo 250 m kể cả sai số lấy mẫu). Nhánh đường ngang: khối đầu là phố.
- **Rơi xuống sông — người chơi (cách chìm đã sửa ở 21.36):** `CheckRiderRiver` (sau va chạm trong `TickRide`): đang chạy, không cua, không đang nhảy, tâm xe ở **|x| > 3,35** trên dải mặt nước → `BeginRiverFall`: hất nhẹ ra ngoài (+1,5 m/s lên), lật nghiêng, tiếng va chạm, lưu kỷ lục, thông báo “RƠI XUỐNG SÔNG · CHUYẾN ĐI KẾT THÚC”; `TickRiderRiver`: rơi tự do tới mặt nước, bọt nước, chìm chậm, sau 1,3 s **GameOver** (kể cả đang bị CSGT đuổi). Thẻ kết thúc ghi “Rơi xuống sông!”. Đi thẳng ở làn 0/3 thì rơi ở mép kè (đo ở 97,1 m với khối cầu tại 80 m); đổi làn ra ngoài trên cầu rơi sau 0,15 s; té (va chạm) rồi trượt ra khỏi mặt cầu cũng rơi (thay vì chặn ở mặt tiền nhà). Nhảy (Space) qua mép chỉ rơi khi tiếp đất ngoài mặt cầu.
- **Rơi xuống sông — xe (cách chìm đã sửa ở 21.36):** sau mỗi bước di chuyển, xe (không băng ngang/không trên đường cũ/không rẽ/không đâm nhà) có tâm ngoài mặt cầu trên mặt nước → `riverFalling`: giữ vận tốc thật, rơi, lật, bọt nước, chìm; không còn va chạm (`VehicleContact` trả false), không làm nóc nhảy, thu hồi sau 3,5 s. Xe thường **tự nhập làn cầu** (`MergeForBridge`: làn 0→1, 3→2; cùng chiều trong 38 m trước mặt nước, ngược chiều trong 58 m), không “tạt làn” ra làn ngoài khi gần cầu, và không bao giờ xuất hiện ở làn ngoài trên/sát mặt nước (`KeepSpawnOnBridge`). Quái xế lạng lách, xe bị tông/văng ra ngoài thì rơi. Xe đâm nhà không xét trên dải sông.
- **Giới hạn đã biết:** CSGT không có luật rơi sông (đi theo vệt xe người chơi trên mặt cầu); lối đi tránh vật cản của CSGT sau va chạm hiếm khi có thể vượt ra ngoài mặt cầu. Mặt nước rộng ~25 m (hẹp hơn ảnh mẫu) do giữ giao lộ 80 m. Chưa thử điện thoại thật/FPS (khối cầu LOD0 45k tam giác, chỉ 1–2 khối hiện cùng lúc).
- **Kiểm tra mới `AgentScripts/VerifyRiverBridge.cs` (130 điều kiện):** cài đặt (35 khối, mesh cắt không còn tam giác trên lòng sông, nền stencil, vật liệu, 4 thuyền, 140 đèn); 25 lượt chạy thẳng ~35 s: khối đầu là phố, không 2 cầu liền nhau, không quá 2 khối phố liền nhau (trừ khối xuất phát), nhánh giao lộ bắt đầu bằng phố, **qua 90 cầu an toàn ở làn 2, khoảng cách lớn nhất 250 m**; vị trí trên/ngoài mặt cầu; người chơi đổi làn ra ngoài trên cầu → rơi (0,15 s), xuống dưới mặt nước, GameOver; đi thẳng làn 3 và làn 0 → rơi ở mép kè, GameOver; làn 1 và 2 qua an toàn; té trượt khỏi cầu → rơi, GameOver; xe cùng chiều làn 3 và ngược chiều làn 0 tự nhập làn cầu, qua cầu không rơi; xe xuất hiện ở làn ngoài trên mặt nước được đặt vào làn cầu; chặn tạt làn ra ngoài gần cầu; xe tạt khỏi cầu và xe bị văng ngang rơi, có bọt nước, bị thu hồi; xe đang rơi không va chạm. Ảnh: `Captures/river-bridge-sheet.png` (ảnh mẫu, tiến tới cầu, trên cầu, nhìn ngang sông, thuyền/lan can, ban đêm), từng ảnh `river-bridge-*.png` từ `AgentScripts/CaptureRiverBridge.cs`.
- **Hồi quy:** các suite cũ chạy với `riversEnabled=false` (trình chạy đặt cờ rồi `ResetRide` ngay khi vào Play Mode) vì chúng đổi làn/đi thẳng theo kịch bản không tính tới sông; suite cầu bật lại cờ. Kết quả: **27 suite / 9.655 điều kiện đạt** — MeshyTank 1.104, MeshyCars 1.266, MeshyCarRoofs 256, TrafficDynamics 82, CrashUpgrade 93, RiderContacts 45, CrashOutcome 51, Gameplay 23, DistanceMode 31, QuickKick 62, JunctionKick 216, SmoothTurns 468, Jump 44, TruckJump 125, RoofRiding 4.206, Boost 35, BoostTrafficIsolation 478, NightJumpLighting 88, LaneSteering 123, StreetArt 333, JunctionRoutes 92, RoadDrivers 45, CrashPoliceNavigation 214, MeshyLighting 16, PortraitHud 21, MobileCamera 8 và **RiverBridge 130 (đạt 5 lần liên tiếp sau khi sửa test)**. Sửa trong lúc kiểm tra: (1) vật liệu FBX lệch thứ tự (xem trên); (2) khối tái sử dụng không tìm thấy khối liền sau khi lệch hơn 1 m trong cùng khung cuộn → nới 6 m; (3) xe làn ngoài xuất hiện quá sát mặt sông không kịp nhập làn → đặt thẳng vào làn cầu nếu cách mặt nước dưới 14 m (cùng chiều)/30 m (ngược chiều); (4) test: đếm cầu theo vị trí tuyệt đối (khối tái sử dụng), dùng ô tô thường (xe tăng quá chậm, bị người chơi vượt trước cầu). Bằng chứng `Captures/river-bridge-verification-summary.json`, `river-bridge-Verify*-results.json`, `river-bridge-install.json`.
- **Trạng thái:** scene đã lưu, jobs khóa (`RideJobsReleased=false`, `jobsEnabled=false`). Đã build/phát hành trong **1.37.0** (21.44); lúc viết mục này web còn 1.36.0. Blender MCP đang mở `PhoViet_SaigonStreets.blend` có thêm scene `PV_RiverBridge` trong bộ nhớ (nguồn cầu đã ghi riêng); không lưu đè file phố.

### 21.36. Sửa rơi sông lơ lửng, chìm dần và dựng lại mặt nước (24/10/2026, Claude Code — chưa sửa hết: lỗi hình bị nâng lên mặt đường và độ sâu −5 m ở 21.37)

- **Phản hồi người dùng (kèm 2 ảnh chơi thử trong Unity):** người và xe rơi xuống sông nằm lơ lửng giữa không trung/trên mặt nước; sông phải thấp hơn mặt đất và cầu; khi rơi phải chìm dần xuống sông sâu kèm hiệu ứng mặt nước, không nổi lềnh bềnh; dựng lại mặt nước có sóng nhấp nhô, đổ bóng như sông thật.
- **Nguyên nhân lơ lửng:** khi chuyến kết thúc (`GameOver`) game ngừng cập nhật nên xe đang rơi và người đang chìm bị đứng hình giữa chừng; ngoài ra người chơi rơi giữ gần hết đà chạy (13 m/s tới trước, tới ~10 m/s ngang) nên lao xa rồi trôi qua mép kè vào trong đất bờ, và sau khi bắn nước chỉ lún tới −4,2 m với tốc độ 0,55 m/s.
- **Sửa:** `Update` gọi `TickRiverAftermath` khi `GameOver`: người/xe đang rơi hoặc chìm tiếp tục (phố đứng yên) tới đáy rồi xe được thu hồi. Người chơi lật qua lan can với đà ngang 1,6–3,4 m/s, đà tới ×0,45; xe giới hạn đà ngang ±4 m/s. `KeepInRiver` giữ vật đang rơi/chìm trong lòng sông của đúng khối đã rơi (kè hai bờ chặn, đà dọc về 0). Sau khi bắn nước: vận tốc ngang tắt dần, chìm đều **0,75 m/s (người/xe máy), 1,15 m/s (ô tô)** tới đáy −7,4 m; thẻ kết thúc hiện sau 1,6 s kể từ lúc chạm nước, việc chìm tiếp diễn dưới thẻ. Xe được thu hồi khi đã chạm đáy (>3 s) hoặc sau 9 s. Camera khi rơi sông ở lại phía trên mặt cầu (x giới hạn ±2,2, cao 6,4 m) nhìn xuống chỗ chìm — trước đó camera đi theo ra ngoài và lọt vào nhà ven sông.
- **Sông sâu hơn (Blender, `pho_viet_river_bridge.py`):** mặt nước −3,2 m (kè cao 3,2 m trên mặt nước), kè xuống tới −7,6 m (phần dưới nước đá sẫm phủ rêu), **lòng sông −7,4 m**, 4 lớp “nước đục” trong suốt ở −3,9/−4,7/−5,6/−6,6 m (vẽ từ sâu lên, shader `PhoViet/River Murk`, theo sáng môi trường ngày–đêm) → vật chìm càng sâu càng mờ hẳn. Mặt nước là **lưới mịn 152×20 ô (~1,25 m)** ở LOD0 (50×6, 10×2 ở LOD1/2) cho sóng dịch đỉnh. LOD cầu 51.534/15.616/2.352 tam giác.
- **Shader mặt nước mới (`PhoViet/River Water`, trong suốt, hàng đợi Transparent):** 4 sóng chính hướng khác nhau (bước sóng 7,4/4,1/2,5/1,5 m, vận tốc pha nước sâu ω=√(g·k), biên độ tổng ~0,19 m với `_WaveHeight` 1,35) dịch đỉnh thật; gợn nhỏ dạng nhiễu đa tỉ lệ cho pháp tuyến, mờ dần theo khoảng cách (hết sọc); màu xanh ngọc loang sáng–tối, ánh sáng xuyên đỉnh sóng; phản chiếu trời theo Fresnel bằng màu môi trường ngày–đêm (không dùng cubemap trắng); lấp lánh nắng; **bọt trắng sát chân kè và quanh trụ cầu** đập theo sóng; độ trong 0,78 → thấy trụ/thuyền/vật đang chìm dưới nước. **Bóng đổ** của mặt cầu, 2 trụ và hai bờ kè lên mặt nước tính giải tích theo hướng nắng/trăng hiện tại (5 mẫu làm mềm) vì vật liệu trong suốt không nhận shadow map built-in. Mesh cầu giữ trục Blender (Z lên, renderer xoay −90° X) nên shader đổi trục `ToTile/FromTile` — lần đầu tính sóng theo trục sai nên ra sọc dài; thử `bakeAxisConversion` của Unity cho ra trục sai khác nên giữ cách xoay cũ. Trình cài đặt lại màu/thông số vật liệu mỗi lần (`_ShallowColor` .2/.68/.74, `_DeepColor` .04/.33/.45, sóng 1,35, độ trong .78, bóng .62).
- **Hiệu ứng mặt nước khi chìm (`PhoVietRiver.cs`):** mỗi khối cầu có 3 hệ hạt riêng (mô phỏng trong hệ khối nên trôi cùng phố, vẽ sau mặt nước): bọt nước bắn khi chạm nước (~48 hạt × cỡ), **vòng gợn sóng** nằm ngang nở 0,6→5 lần trong 1,7 s, phát mỗi 0,38 s khi vật còn trong 2,6 m dưới mặt nước, **bọt khí sủi** mỗi 0,12 s khi còn trong 4,2 m. Kết cấu chấm/vòng tạo bằng code (không thêm asset). Đếm `RiverRipples/RiverBubbles` cho kiểm tra.
- **Kiểm tra:** `VerifyRiverBridge` thêm: vật liệu nước trong suốt + lớp nước đục; sau thẻ kết thúc người chơi tiếp tục chìm tới đáy (đo y lúc hiện thẻ → sau 8 s); có gợn sóng và bọt khí; xe rơi chìm sâu hơn 2,5 m dưới mặt nước; xe đang ở giữa không trung khi chuyến kết thúc vẫn rơi tiếp và chìm. Ảnh `Captures/river-sink-sheet.png` (mặt nước, bắn nước, chìm dần có bọt khí/gợn sóng, gợn lan rộng, chìm hẳn, nước trong quanh trụ, thuyền/kè, ban đêm) từ `AgentScripts/CaptureRiverSink.cs` + `CaptureRiverBridge.cs`. Chạy lại cả chuỗi sau sửa: **27 suite / 9.688 điều kiện đạt** (RiverBridge 135, MeshyTank 1.132, RoofRiding 4.206, CrashPoliceNavigation 214, PortraitHud 21, MobileCamera 8…; suite cũ với `riversEnabled=false`) — `Captures/river-bridge-verification-summary.json`.
- **Lưu ý cho người làm tiếp:** Unity đang mở bởi người dùng có thể ở Play Mode; trình cài (`InstallRiverBridge`) chỉ chạy ở Edit Mode (lần đầu bị từ chối vì người dùng đang chơi thử — đã chờ tới khi thoát Play). Không cần build lại cho tài liệu.
- **Trạng thái:** scene đã lưu, jobs khóa. Đã build/phát hành trong **1.37.0** (21.44); lúc viết mục này web còn 1.36.0.

### 21.37. Sửa thật lỗi “lơ lửng” và hạ mặt sông sâu 5 m (24/10/2026, Claude Code — phát hành 1.37.0 ở 21.44)

- **Phản hồi người dùng (ảnh chơi thật trong Unity sau 21.36):** người và xe vẫn nằm lơ lửng phía trên, mặt nước vẫn ngang mặt đường và cầu; mặt nước phải nằm sâu hơn hẳn đường và cầu.
- **Nguyên nhân thật (đo trong chính lượt chơi của người dùng):** gốc người chơi đã chìm tới đáy (y −6,69) nhưng **hình xe + người lái nằm ở y ≈ 0,74**: `BikeVisualAnimator` (dòng “Physics owns the outer vehicle roll…”) khi đang ngã nâng phần hình lên sao cho điểm thấp nhất không dưới mặt đường; `CarVisualAnimator` làm tương tự với xe bị tông. Ở 21.36 các kiểm tra/ảnh chụp chạy bằng tick thủ công với `Time.timeScale=0`, `LateUpdate` của animator không chạy nên không lộ lỗi — **bài học: phải kiểm tra bằng Play Mode thời gian thực** (đã làm ở dưới).
- **Sửa:** `PhoVietGame.InRiverWater(transform)` (ngoài mặt cầu trên lòng sông); `BikeVisualAnimator` bỏ nâng hình khi người chơi `RiderInRiver` hoặc xe `riverFalling`/đang ở trên lòng sông; `CarVisualAnimator` bỏ nâng khi `riverFalling` hoặc ở trên lòng sông. Trên mặt cầu vẫn giữ nâng hình như cũ.
- **Sông sâu hơn:** mặt nước **−5,0 m** (kè cao 5 m trên mặt nước, 8 hàng đá), kè xuống −9,8 m, **đáy −9,5 m**, lớp nước đục −5,7/−6,6/−7,6/−8,7 m, dây leo rủ dài 2–3,4 m; `RiverBlock.WaterY=-5`, `BedY=-9,5`; shader nước có `_WaterY` (trình cài đặt theo JSON), trụ che bóng xuống −12 m. LOD cầu 52.754/15.856/2.352 tam giác.
- **Camera khi rơi sông:** đứng phía trên mép nước (không lùi sau kè gần — trước đó người rơi sát kè bị chính bờ kè che), cao 8,2 m, nhìn thẳng xuống chỗ chìm (`RiverCameraMinZ`: mép nước bờ gần của khúc sông đã rơi − 0,5 m).
- **Kiểm chứng thời gian thực (Play Mode thật, `Time.timeScale=1`, Update/LateUpdate chạy như khi chơi):** cho người chơi chạy thẳng làn ngoài tới mép kè + một xe cùng rơi; chụp liên tiếp → người chơi rơi, bắn nước, chìm tới −8,8 m (đáy + 0,7), **điểm cao nhất của hình xe/người −6,18 m < mặt nước −5,0 m**, trạng thái GameOver. Ảnh `Captures/realtime-river-1..4.png` (kịch bản trong `huongdan.md` này: vào Play, eval đặt khối cầu ở z 80, làn 0, chụp bằng `rideCamera.Render`).
- **Kiểm tra tự động thêm:** `VerifyRiverBridge` gọi `LateUpdate` của `BikeVisualAnimator` sau khi chìm và kiểm tra **hình người/xe nằm dưới mặt nước** (đo −6,18) — 136 điều kiện. Ảnh tổng hợp `Captures/river-sink-sheet.png` cập nhật (mặt nước sâu 5 m, bắn nước, chìm dần, gợn sóng, chìm hẳn, trụ cầu, kè cao, ban đêm). Chạy lại cả chuỗi: **27 suite / 9.698 điều kiện đạt** (RiverBridge 136, MeshyTank 1.141, RoofRiding 4.206, PortraitHud 21, MobileCamera 8…; suite cũ với `riversEnabled=false`) — `Captures/river-bridge-verification-summary.json`.
- **Trạng thái:** scene đã lưu, jobs khóa. Đã build/phát hành trong **1.37.0** (21.44); lúc viết mục này web còn 1.36.0. Mục 21.35–21.36 giữ làm lịch sử; độ sâu/camera hiện hành theo mục này.

### 21.38. Xe tăng mỗi 9 giây và bắn đạn pháo có cảnh báo (25/10/2026, Claude Code — phát hành 1.37.0 ở 21.44)

- **Yêu cầu:** xe tăng xuất hiện nhiều hơn, cứ 9 giây một xe; xe tăng bắn đạn bay ra rơi ngẫu nhiên xuống 1 trong 4 làn, mỗi phát cách nhau 1 giây; dưới đất có dự báo chỗ đạn rơi để người chơi tránh; trúng đạn hoặc ở trong bán kính nổ thì văng ra và kết thúc game; dựng hiệu ứng lửa đầu nòng, đạn bay, vụ nổ và cảnh báo dưới đất bằng Blender MCP. **Người dùng chọn:** đúng mỗi 9 giây 1 xe và bỏ xe tăng ngẫu nhiên; chỉ xe tăng ngược chiều bắn (nòng chĩa về người chơi); đạn rơi ở đoạn đường phía trước người chơi sắp tới, vụ nổ làm văng cả xe khác.
- **Hiệu ứng Blender (`ArtSource/Blender/pho_viet_tank_fx.py`, scene `PV_TankFx`, nguồn `PhoViet_TankFx.blend`; render EEVEE nền trong suốt, camera trực giao):** `PV_TankShell.fbx` đạn pháo dài 0,92 m (thân thép, vai thuôn, đầu nhọn, đai đồng, 4 cánh đuôi, đuôi vạch sáng; 228/164/84 tam giác; Unity phóng 1,8× để thấy đường bay); `PV_TankFx_Muzzle.png` 4 khung 2×2 lửa đầu nòng (9 tia lửa nón tỏa sao, lõi vàng, lửa cam loang nhiễu, khói); `PV_TankFx_Explosion.png` 8 khung 4×2 vụ nổ (lõi vàng trắng, cầu lửa cam xoáy, cụm lửa/khói cuộn, mảnh vụn văng, kết bằng khói đen); `PV_TankFx_Warning.png` vòng cảnh báo đỏ nhìn từ trên (vòng ngoài, vòng đứt, 4 mũi tên chỉ vào tâm, tâm ngắm trắng). Bản render đầu bị cháy trắng/cắt mép khung → giảm cường độ phát sáng và cỡ.
- **Unity — cài (`PhoVietTankFxUpgrade.Install`, menu “Phố Việt/Art/Install tank shell effects”, `AgentScripts/InstallTankFx.cs`):** nhập FBX/kết cấu (alpha trong suốt, clamp), tạo `PV_TankSpark.png` (chấm mềm), vật liệu `PV_TankMuzzle/PV_TankExplosion/PV_TankSpark` (shader `PhoViet/Soft Particle`, hàng đợi 3100) và `PV_TankWarning` (shader mới `PhoViet/Tank Warning`: vòng Blender + đĩa đỏ đầy dần từ tâm theo `_Progress`, nhấp nháy nhanh dần); gán vào `PhoVietGame` (`tankShellPrefab`, `tankShellMaterial` = atlas phố, các vật liệu) và lưu scene với `randomTanks=false`, `tankBarrageEnabled=true`.
- **Lịch xe tăng (`PhoVietTankFire.cs`):** `TickTankSchedule`: mỗi **9 s** chạy (không tính lúc cua/dừng dịch vụ) gọi `SpawnScheduledTank` — chọn ô xe có mẫu xe tăng, `MeshyCarVariants.PrepareVariant` chọn trước mẫu xe tăng rồi `SpawnTraffic` ở làn ngược chiều 0 hoặc 1 (còn trống 14 m), z 122–130; không có ô trống thì thử lại sau 0,25 s. Thông báo “XE TĂNG PHÍA TRƯỚC · NÉ ĐẠN PHÁO”. Xe tăng ngẫu nhiên ở ô xe con (21.33) tắt bằng `randomTanks=false` (InitializeDriver và xe ra sau cua đều không chọn xe tăng). Sửa kèm: `SelectForSpawn` khi mẫu đã chọn trước giờ gán lại cờ `armored/speedScale` (trước đây xe chọn trước bị mất cờ xe tăng).
- **Bắn:** xe tăng ngược chiều, không bị tông/rơi sông/đi đường cũ, ở z 24–115 và đủ xa để đạn rơi đúng chỗ (đầu nòng − 5 m ≥ tốc độ × 1,6 s) bắn mỗi **1,0 s** (`tankFireTimer`; từ 21.39 khoảng này giảm theo tốc độ người chơi, tối thiểu 0,4 s), làn rơi ngẫu nhiên 0–3. Đầu nòng = điểm (0; 1,42; 3,62) của mô hình xe tăng. Điểm rơi = làn đã chọn, **phía trước người chơi một đoạn tốc độ × 1,6 s** (thời gian bay) — đứng yên trên mặt đường nên trôi về phía người chơi đúng lúc đạn chạm đất. Đạn bay theo cung parabol (đỉnh 3,2 m + 6% khoảng cách), mũi theo hướng bay, vệt lửa + khói đuôi; lửa đầu nòng + 5 cụm khói phụt theo nòng; tiếng pháo/tiếng nổ tổng hợp bằng code.
- **Cảnh báo dưới đất:** hiện ngay khi bắn tại điểm rơi (trên mặt nước nếu điểm rơi ở lòng sông), đường kính = 2 × bán kính nổ, thu nhỏ dần 1,25→1 và đĩa đỏ đầy từ tâm theo tiến độ bay, nhấp nháy nhanh dần.
- **Nổ:** khi hết thời gian bay (hoặc đạn chạm người chơi giữa đường bay — trúng trực tiếp) → cầu lửa flipbook 7,5 m, 26 tia lửa/mảnh vụn, 4 cụm khói đen, tiếng nổ. **Bán kính 2,2 m** (làn cách nhau 3,1 m nên chỉ làn bị bắn nguy hiểm). Người chơi trong bán kính (không nhảy cao ≥ 2,2 m) → `BlastRider`: va chạm hất lên 7,5 m/s, ra xa 8 m/s, xoay mạnh, luôn kết thúc chuyến kể cả khi đang bị CSGT đuổi; thông báo/thẻ kết thúc “TRÚNG ĐẠN XE TĂNG · CHUYẾN ĐI KẾT THÚC” / “Trúng đạn xe tăng!”. Xe khác (trừ xe tăng) trong bán kính + nửa thân → văng ra (`ActivateCollisionBody`, Δv 11 m/s + bật lên). Đạn rơi xuống sông (làn ngoài trên cầu) → bắn nước sông, không nổ.
- **Thế giới cuộn/cua/chơi lại:** đạn, điểm rơi, cảnh báo và hạt hiệu ứng trôi cùng phố (`ScrollTankFire` trong `ScrollRoutes`; hạt mô phỏng trong một mốc “Hiệu ứng đạn xe tăng • Blender” cuộn theo phố); đạn đang bay vẫn bay tới và nổ khi người chơi đang té hoặc đã kết thúc; bắt đầu cua giao lộ và chơi lại thì hủy đạn/cảnh báo đang bay.
- **Kiểm tra mới `AgentScripts/VerifyTankBarrage.cs` (54 điều kiện):** tài sản/vật liệu Blender đã cài; scene lưu `randomTanks: 0`, `tankBarrageEnabled: 1`; 300 lần xuất hiện ở ô xe con không còn xe tăng, xe ra sau cua không chọn xe tăng; 40 s chạy: xe tăng xuất hiện đúng **9,00/18,00/27,00/36,00 s**, mỗi xe ngược chiều, làn 0/1, chậm 60%, ở xa; mỗi xe bắn **đúng 1,00 s/phát**, đạn rơi đủ 4 làn (lần đo `031222013`), cảnh báo hiện đúng làn ngay khi bắn, điểm rơi đúng chỗ người chơi sẽ tới sau 1,6 s, chỉ xe ngược chiều bắn; đạn vào làn người chơi → văng ra sau 1,57 s, bay lên > 1,2 m, GameOver; nổ làn bên cạnh → an toàn; đổi làn sau khi thấy cảnh báo → né được; ô tô trong bán kính bị hất, xe tăng không; nổ xa 40 m không ảnh hưởng; đạn rơi xuống sông bắn nước; chơi lại xóa đạn đang bay. `VerifyMeshyTank` (tỉ lệ/tốc độ/va chạm xe tăng) vẫn chạy với `randomTanks=true` do trình chạy đặt.
- **Ảnh:** `Captures/tank-fire-sheet.png` (lửa đầu nòng + đạn rời nòng, vòng cảnh báo hiện ngay khi bắn, nổ làn bên an toàn, khói + cảnh báo phát tiếp, đạn nhắm đúng làn, trúng → nổ và văng, kết thúc; khung tấm hiệu ứng Blender), từng ảnh `tank-fire-*.png` từ `AgentScripts/CaptureTankFire.cs`. **Chơi thời gian thực** (Play Mode thật, `realtime-tank-*.png`): xe tăng xuất hiện, bắn 2 phát, người chơi không né bị trúng, văng ra, GameOver.
- **Trình chạy suite cũ:** đặt `riversEnabled=false`, `tankBarrageEnabled=false`, `randomTanks=true` rồi `ResetRide` (giữ đúng điều kiện các suite đã được kiểm chứng ở 1.36); suite cầu/xe tăng bắn tự bật cờ của mình. Cả chuỗi: **28 suite / 9.732 điều kiện đạt** (TankBarrage 54, RiverBridge 136, MeshyTank 1.121, RoofRiding 4.206, CrashPoliceNavigation 214, PortraitHud 21, MobileCamera 8…) — `Captures/tank-fire-verification-summary.json`.
- **Trạng thái:** scene đã lưu, jobs khóa. Đã build/phát hành trong **1.37.0** (21.44); lúc viết mục này web còn 1.36.0 (chưa có cầu/sông/xe tăng bắn).

### 21.39. Xe tăng bắn dày hơn khi người chơi chạy nhanh (25/10/2026, Claude Code — phát hành 1.37.0 ở 21.44)

- **Yêu cầu:** tốc độ người chơi càng tăng thì khoảng thời gian giữa các lần bắn của xe tăng càng giảm, giảm tới 0,4 giây thì dừng.
- **Triển khai (`PhoVietTankFire.cs`):** `TankFireIntervalAt(speed)` = **1,0 s ở 17 m/s** (tốc độ khởi đầu), giảm tuyến tính tới **0,4 s ở 30 m/s** (tốc độ chạy tối đa), nhanh hơn nữa (tăng tốc W, nhảy) vẫn **0,4 s** (`TankFireIntervalMin`); chậm hơn 17 m/s giữ 1,0 s. Mỗi lần bắn đặt `tankFireTimer` theo `Speed` hiện tại; khi người chơi tăng tốc giữa hai phát, bộ đếm bị kẹp xuống khoảng mới ngay (không chờ hết khoảng cũ dài hơn). Ví dụ 23,5 m/s → 0,7 s. Lịch 9 giây, điểm rơi, cảnh báo, bán kính nổ giữ nguyên 21.38 (thời gian bay vẫn 1,6 s nên ở tốc độ cao có thể 3–4 vòng cảnh báo cùng lúc).
- **Kiểm tra:** `VerifyTankBarrage` 69 điều kiện — ánh xạ 17→1,0 s, 23,5→0,7 s, 30→0,4 s, 45/60→0,4 s, 8→1,0 s, giảm đơn điệu; trong 40 s chơi các khoảng đo `0,87/0,83/0,70/0,70/0,57/0,53/0,43/0,43 s` khớp khoảng tính theo tốc độ lúc bắn (sai số < 0,05 s, bước mô phỏng 1/30 s), luôn trong [0,4; 1,0]; ở tốc độ tối đa 30 m/s một xe tăng bắn cách **0,43 s** (0,4 s làm tròn lên bước 1/30 s). Chạy lại RiverBridge 136, MeshyTank 1.141, Gameplay 23, CrashOutcome 51 — đạt (các suite cũ khác chạy với `tankBarrageEnabled=false` nên không chịu ảnh hưởng của thay đổi này).
- **Trạng thái:** scene không đổi (chỉ mã), jobs khóa. Đã build/phát hành trong **1.37.0** (21.44); lúc viết mục này web còn 1.36.0.

### 21.40. Lỗi “không có xe tăng 9 giây, xe tăng không bắn” khi người dùng chơi thử (25/10/2026, Claude Code)

- **Người dùng báo:** vào chơi thử (Unity Editor) không thấy xe tăng ngược chiều mỗi 9 giây; xe tăng xuất hiện thì không bắn, chỉ chạy.
- **Nguyên nhân (không phải mã xe tăng):** trình chạy suite cũ của Claude Code đặt `riversEnabled=false`, `tankBarrageEnabled=false`, `randomTanks=true` rồi `ResetRide` bằng `unity command eval` ngay sau khi vào Play Mode. Một lần Unity chưa kịp vào Play Mode (đang nạp lại mã) nên lệnh đó chạy ở **Edit Mode** và đổi luôn đối tượng `PhoVietGame` của scene đang mở (không đánh dấu dirty, file scene trên đĩa vẫn đúng: `riversEnabled: 1`, `randomTanks: 0`, `tankBarrageEnabled: 1`). Khi người dùng bấm Play, game chạy với cờ sai: xe tăng ngẫu nhiên kiểu 21.33 (không bắn), không có lịch 9 giây, cũng không có cầu sông.
- **Sửa:** mở lại scene từ đĩa (`EditorSceneManager.OpenScene`) → cờ trở lại đúng, scene không dirty; các trình chạy kiểm tra/chụp ảnh của Claude Code (`run_legacy`, `run_portrait_river`, `realtime_*`) giờ chỉ đổi cờ khi `Application.isPlaying` (Edit Mode trả về ngay, không đụng scene). **Người làm tiếp: không bao giờ chỉnh cờ gameplay của `PhoVietGame` bằng eval nếu chưa chắc đang ở Play Mode;** nếu nghi scene đang mở bị đổi, so cờ trong file `PhoViet.unity` rồi mở lại scene.
- **Kiểm chứng thời gian thực (Play Mode thật, chỉ bấm bắt đầu, miễn va chạm xe để không té trước 9 s; đạn vẫn gây chết):** `barrage=True rivers=True random=False`; sau 9 s chạy có 1 xe tăng ngược chiều (z 115, tốc độ −4 m/s), bắn 3 phát, 3 vụ nổ, người chơi không né bị trúng → `KilledByShell`, GameOver. Ảnh `Captures/realtime-tank-natural.png` (xe tăng/xe trên cầu; vụ nổ + vòng cảnh báo phát tiếp theo), `realtime-natural-*.png`.
- **Trạng thái:** không đổi mã game; scene đã mở lại đúng bản lưu. Đã build/phát hành trong **1.37.0** (21.44); lúc viết mục này web còn 1.36.0 (bản web cũ vẫn là xe tăng ngẫu nhiên không bắn — đúng như 1.36.0).

### 21.41. Kiểu ánh sáng buổi sáng có sương (25/10/2026, Claude Code — người dùng chọn **D**, phát hành 1.37.0 ở 21.44)

- **Yêu cầu:** ánh sáng lúc sáng có sương mù quá trắng làm cả màn hình ám trắng; tạo vài kiểu ánh sáng để người dùng duyệt, vẫn giữ sương mù.
- **Nguyên nhân ám trắng:** đầu lượt (`DayNightLighting` chu kỳ 0, `NightLevel` 0) dùng nắng trắng `FFFFFF` cường độ 1,12, ánh sáng môi trường gần trắng (`E4F1FF/D9E4EF/ADB7C2`), và sương thể tích (`MorningFog`, 6 s đầu) trộn màu `E1ECF1` gần trắng lên ảnh (`source·T + fogColor·(1−T)`).
- **Triển khai:** `MorningLook.All` (`Assets/PhoViet/Scripts/MorningLooks.cs`) — 4 kiểu: **A · Bình minh vàng ấm** (nắng `FFC98E` 1,0 xiên 24°, môi trường xanh–be ấm, sương đào `E8C9A6` ×0,85); **B · Sương xanh dịu** (nắng `F1F0EA` 0,92, môi trường xanh xám trầm, sương `A9BCC8` ×0,9); **C · Sương mỏng trong trẻo** (nắng `FFF6E6` 0,98, môi trường dịu hơn gốc, sương `CCDAE2` ×0,55); **D · Nắng sớm vàng nhạt, sương xám bạc** (nắng `FFE3B5` 0,95, môi trường xám ấm, sương `B9BDBF` ×0,8). `DayNightLighting.morningLook` (−1 = gốc) chọn kiểu; `ApplyMorningLook` pha màu nắng/cường độ/góc nắng, môi trường 3 tầng, màu chân trời/thiên đỉnh trời, màu sương tuyến tính theo **độ đậm sương** (`MorningFog.Strength`) — đầu lượt đúng kiểu đã chọn, sương tan thì về ánh sáng ngày cũ; `MorningFog.SetLook` đổi màu/độ đậm sương thể tích. Sương mù vẫn giữ nguyên (lưới mật độ Blender, 6 s, mờ dần).
- **Ảnh để duyệt:** `Captures/morning-looks-sheet.png` (gốc + A–D, mỗi kiểu lúc sương dày 0,6 s và lúc sương đang tan 3,5 s, cùng góc chơi/cùng xe) từ `AgentScripts/CaptureMorningLooks.cs`; từng ảnh `morning-look-*.png`.
- **Người dùng chọn kiểu D · Nắng sớm vàng nhạt, sương xám bạc:** scene lưu `DayNightLighting.morningLook: 3`. Kiểm tra Play Mode: đầu lượt độ đậm sương 1,00, nắng `FFE3B5` (1,000/0,890/0,710) cường độ 0,95, môi trường trời `A2AFBE`; ảnh `Captures/morning-look-d-play.png` — hết ám trắng, sương xám bạc ở xa, màu phố rõ. Các kiểu A–C giữ trong mã để chọn lại (đổi `morningLook` 0/1/2, −1 = gốc).
- **Trạng thái:** scene đã lưu (jobs khóa, cầu/xe tăng bắn bật). Đã build/phát hành trong **1.37.0** (21.44); lúc viết mục này web còn 1.36.0.

### 21.42. Mẫu mũi tên báo “xe phía sau sắp chạy lên” (25/10/2026, Claude Code — người dùng chọn **D**, phát hành 1.37.0 ở 21.44)

- **Yêu cầu:** thiết kế lại các mũi tên hướng lên ở đáy màn hình báo có xe chạy lên từ phía sau (hiện quá xấu, không hợp phong cách game); làm vài mẫu để chọn trước.
- **Mẫu cũ (`PhoVietRearWarning.cs`):** khung vuông tối + quầng đỏ nhấp nháy + mũi tên đỏ pixel thô viền vàng + vạch vàng dưới (`CreateRearWarningArrow`).
- **Mẫu mới (`PhoVietRearWarningStyles.cs`, vẽ bằng hàm khoảng cách có khử răng cưa, mipmap, bảng màu HUD Mực 203C43/Kem F6F9FE/Đỏ D83833/Teal 177E80/Vàng FFD35A):** **A · Huy hiệu tròn đỏ** (đĩa đỏ chuyển sắc, viền kem, hai mũi tên kép kem, bóng đổ, quầng đỏ nhấp nháy, nhịp phóng to nhẹ); **B · Biển báo tam giác** (tam giác vàng bo góc viền đỏ như biển báo giao thông, mũi tên mực đậm, nhấp nhô); **C · Thẻ kính teal** (thẻ bo góc teal như bảng HUD, viền sáng mảnh, mũi tên kem nét tròn, chấm đỏ góc, chữ “XE SAU”); **D · Ba mũi tên chạy lên** (ba chevron đỏ viền kem sáng lần lượt từ dưới lên như xe lao tới, quầng đỏ mềm). `rearWarningStyle` (−1 = cũ, 0–3 = A–D) chọn mẫu; vị trí/thời điểm hiện (làn 2/3, cảnh báo xe sau thường và quái xế) không đổi. `previewRearWarnings` chỉ dùng khi chụp ảnh duyệt.
- **Ảnh duyệt:** `Captures/rear-warning-styles-sheet.png` (toàn cảnh + phóng to vùng biểu tượng cho mẫu cũ và A–D), chụp màn hình Game view thật (HUD IMGUI) trong Play Mode; từng ảnh `rear-warning-style*.png`.
- **Người dùng chọn mẫu D · Ba mũi tên chạy lên:** scene lưu `rearWarningStyle: 3`, `previewRearWarnings: 0`. Kiểm tra Play Mode: kiểu đang dùng = 3, ảnh `Captures/rear-warning-d-play.png` (ba chevron đỏ viền kem ở đáy màn hình trên làn 2/3). Mẫu A–C và mẫu cũ giữ trong mã để chọn lại (`rearWarningStyle` 0/1/2, −1 = cũ).
- **Trạng thái:** scene đã lưu (cầu/xe tăng bắn bật, ánh sáng sáng sớm D, jobs khóa). Đã build/phát hành trong **1.37.0** (21.44); lúc viết mục này web còn 1.36.0.

### 21.43. Dời cây ra khỏi mái che và tô mặt tiền nhiều màu (25/10/2026, Claude Code — phát hành 1.37.0 ở 21.44)

- **Yêu cầu:** cây xanh đâm xuyên qua mái che → dời cây sang vị trí người dùng đánh dấu (khe giữa hai mái hiên của hai tiệm cạnh nhau); nếu chỗ mới có đồ vật thì dời đồ vật cho hợp lý, không đè nhau; mặt tiền nhà/tiệm hai bên đường chủ yếu tông vàng → tô lại nhiều màu cho khỏi đơn điệu.
- **Cây:** trước đây cây ở z 10/30/50 m của khối (x ±8,2), rơi giữa mái hiên của tiệm (mái rộng ±3,68 m quanh tâm tiệm 4+8,4n, cao 2,8–3,22 m, vươn ra 1,5 m). Dời tới **ranh giới hai tiệm z 8,2 / 33,4 / 50,2 m** (khe hở ~1 m giữa hai mái) — `AgentScripts/MoveStreetTrees.cs` dời 210 cây trong 35 khối, giữ mẫu cây; `VietnamStreet.TreeZ` + `PhoVietStreetUpgrade.TreeFactory` (mẫu 1/0/2 theo thứ tự) cập nhật để dựng lại ra đúng chỗ. Kiểm tra va chạm thân cây (bán kính 0,35 m) với mọi vật cản mặt đất của tiệm (bàn ghế, tủ, bảng chữ A, chậu, quầy…): **không có chồng lấn** nên không cần dời đồ vật. `AgentScripts/CheckTreeAwnings.cs` đếm đỉnh lưới cây nằm trong khối mái hiên: vị trí cũ 132 đỉnh (cây z 10 và 30), **vị trí mới 0**. Cây z 33,4 vẫn nằm trên lòng sông nên khối cầu ẩn nó (chạy lại `InstallRiverBridge`, vẫn 17 đối tượng ẩn/khối).
- **Màu mặt tiền (`pho_viet_saigon_streets.py`, Blender MCP):** đổi giá trị 7 ô màu mặt tiền trong bảng màu atlas (`cream` trắng ngà F3EADB, `butter` vàng F5D06B, `beige` hồng phấn EDB3BC, `peach` cam san hô F2A487, `mint` xanh bạc hà A9DCC4, `sky` xanh da trời A7CBEA, `sand` tím oải hương C8B7E6; `stone` giữ) và gán lại: PHỞ trắng ngà, TIỆM TRÀ bạc hà, BÁNH MÌ san hô, TẠP HÓA xanh trời, THUỐC TÂY hồng, SỬA XE xám, CÀ PHÊ oải hương, CƠM TẤM bạc hà, BÚN BÒ HUẾ xanh trời, HỦ TIẾU vàng (tiệm vàng duy nhất), SINH TỐ oải hương, TIỆM BÁNH san hô — hai tiệm liền nhau không trùng màu. Các chỗ khác từng mượn các ô màu này (sàn trong tiệm, thức ăn, tủ kính, cột ghế) đổi sang màu khác để giữ sắc cũ. Nhà cao tầng phía sau dùng `sky/mint` nên cũng tươi hơn. Mở `PhoViet_SaigonStreets.blend` và chạy `build_saigon_streets()` đầy đủ (lưu lại nguồn); các FBX không đổi hình (cây, vỉa hè, đèn, tín hiệu) được trả về bản Git để tránh thay đổi nhị phân thừa; chỉ 12 FBX tiệm + `PV_StreetPalette.png` đổi.
- **Kiểm tra:** ảnh `Captures/street-trees-colors-sheet.png` (cây ở khe mái che hai bên, góc chơi và nhìn rộng mặt tiền nhiều màu) từ `AgentScripts/CaptureStreetColors.cs`; suite StreetArt 333, RiverBridge 136, JunctionRoutes 92, Gameplay 23, CrashOutcome 51, CrashUpgrade 93, RiderContacts 45, NightJumpLighting 88, TankBarrage 69 — đạt.
- **Duyệt:** 25/10/2026 người dùng xác nhận màu mặt tiền và vị trí cây “ổn rồi” — giữ nguyên, không cần chỉnh thêm.
- **Trạng thái:** scene đã lưu (jobs khóa). Đã build/phát hành trong **1.37.0** (21.44); lúc viết mục này web còn 1.36.0.

### 21.44. Build/phát hành 1.37.0 — cầu sông, xe tăng bắn đạn, ánh sáng/mũi tên D, cây và mặt tiền (25/10/2026, Claude Code)

- **Ủy quyền:** người dùng yêu cầu trực tiếp “Build WebGL và phát hành lên web nhé” sau khi duyệt màu mặt tiền và vị trí cây (21.43) — gồm commit, build, push main và GitHub Pages.
- **Phạm vi:** 21.35–21.43: cầu vượt sông + rơi/chìm (21.35–21.37), xe tăng mỗi 9 giây bắn đạn có cảnh báo và nhịp bắn theo tốc độ (21.38–21.39), sửa runner 21.40, ánh sáng sương sáng sớm D (21.41), mũi tên báo xe sau D (21.42), cây và mặt tiền (21.43). Công việc vẫn khóa (`RideJobsReleased=false`, `jobsEnabled=false`). Cờ scene phát hành: `riversEnabled=1`, `tankBarrageEnabled=1`, `randomTanks=0`, `morningLook=3`, `rearWarningStyle=3`, `previewRearWarnings=0`. Không kéo vào commit: 5 `.blend` cục bộ cũ (Cars, Jump, Motorcycles, Passengers, Streets), FBX/JSON/Palette xe máy cũ trong `Art/Bikes/` (từ 23/10), Wave/WaveAlpha, `Assets/ai.meshy`, `Packages/*`, `PhoViet_Anhsangngaydem.blend`/`Phoviet_Suongmu.blend` của người dùng, ảnh/JSON lịch sử đổi do chạy lại test.
- **Version:** 1.36.0 → **1.37.0** ở `PhoVietWebBuild.cs`, `ConfigureWebRelease.cs`, `PlayerSettings.bundleVersion`, kỳ vọng `VerifyWeb.cjs`, `VerifyFullscreenGame.cjs`, `VerifyCrashPoliceWeb.cjs`, `VerifyMorningFogWeb.cjs`, `VerifyWebBundle.py` (ghi `Captures/release-1.37-bundle-checks.json`); README thêm mục 1.37.0.
- **Hồi quy native trước build (Play Mode, đúng mã/scene phát hành): 28 suite / 9.762 điều kiện đạt.** Cờ mặc định scene: VerifyRiverBridge 136, VerifyTankBarrage 69. Cờ cũ (tắt sông/xe tăng bắn, xe tăng ngẫu nhiên — runner chỉ đổi cờ khi `Application.isPlaying`): MeshyTank 1.136, MeshyCars 1.266, MeshyCarRoofs 256, TrafficDynamics 82, CrashUpgrade 93, RiderContacts 45, CrashOutcome 51, Gameplay 23, DistanceMode 31, QuickKick 62, JunctionKick 216, SmoothTurns 468, Jump 44, TruckJump 125, RoofRiding 4.206, Boost 35, BoostTrafficIsolation 478, NightJumpLighting 88, LaneSteering 123, StreetArt 333, JunctionRoutes 92, RoadDrivers 45, CrashPoliceNavigation 214, MeshyLighting 16. Dọc: PortraitHud 21, MobileCamera 8. Kết quả ở `Captures/saigon-street-VerifyRiverBridge/TankBarrage-results.json`, `Captures/river-bridge-Verify*-results.json`.
- **Sửa test trong lúc phát hành:** lần chạy đầu VerifyRiverBridge trượt ở “Riding straight in a bridge lane crosses every river safely (run 0)”: test chạy 35 s với cờ scene nên xe tăng bắn (21.38) có thể nã trúng người chơi giữa đường thử. `AgentScripts/VerifyRiverBridge.cs` giờ tắt `tankBarrageEnabled` khi thử cầu và trả lại cờ cũ trong `finally`; chạy lại 2 lần liền, đạt 136/136 cả hai. Không đổi mã game.
- **Preflight** `VerifySaigonRelease` (Edit Mode): scene đã lưu, version 1.37.0, jobs khóa, 735 nhóm/8.050 hộp vật cản, 1.260 LOD phố, 1.350 renderer kính, 420 bóng tín hiệu → `Captures/release-1.37-preflight.json`.
- **Build:** `build_9c295f769ef8` **Succeeded**, 293.753 ms, 0 lỗi / 3 cảnh báo đã biết (Pipeline thiếu RuntimePipelineConfig; `dragging`, `hudTouchChoice` không đọc). Cache **`5018a747f9b8d80c`**, UTC `2026-10-24T19:00:32.4293158Z`, tài nguyên precache 98.188.592 byte (data 90.911.241 byte, tăng ~1,8 MB do cầu/sông/FX xe tăng; dưới giới hạn 100 MB/file). Đã chép sang `docs`, dọn đúng 4 file hash cũ trong `docs/Build` sau kiểm tra đường dẫn; còn đúng 4 file Build.
- **Kiểm tra artifact local:** 29 kiểm tra bundle (`release-1.37-bundle-checks.json`). Trình duyệt local **77 điều kiện, 0 lỗi runtime**: DPR3 dọc/ngang/tablet/xoay lại 14 (`release-1.37-mobile-results.json`), đối chiếu tài nguyên + WASM 26 (`release-1.37-local-results.json`), PWA/offline/cảm ứng 37 (`browser-release-1.37-pwa.json`, bị gitignore). Playwright qua `NODE_PATH` tới bộ phụ thuộc Node có sẵn trên máy. Giả lập trình duyệt, chưa thử điện thoại vật lý/FPS.
- **Phát hành:** source/artifact `f8d1be0a7039aae6a52afab3d733d8736aa6f9f3`, push main. Pages run **40124389275** completed/success đúng SHA (`Captures/release-1.37-pages.json`). Live: version 1.37.0 / cache `5018a747f9b8d80c`; `VerifyPublishedGame` **26 kiểm tra đạt, 0 lỗi runtime** (`release-1.37-live-results.json`, ảnh `release-1.37-live-game.png`).
- **Chơi thật bản live (Chrome GPU, 0 lỗi trang):** bot đổi làn ngẫu nhiên chạy tới 264 m, qua cầu sông thấy mặt nước, kè, lan can và một đạn xe tăng nổ cạnh người chơi (`release-1.37-live-bridge-blast.png`), xe máy rơi xuống sông (`release-1.37-live-river-fall.png`). Gọi thêm `SpawnScheduledTank` qua `SendMessage` ngay đầu lượt để quay cảnh bắn: vòng cảnh báo đỏ dưới đất (`release-1.37-live-tank-warning.png`), nổ khói và thẻ “Trúng đạn xe tăng!” (`release-1.37-live-tank-hit.png`). Nhịp 9 giây tự nhiên đã đo trong Unity thời gian thực (21.40); trên web người chơi bot thường thua trước 9 giây nên không dùng làm phép đo nhịp.
- **Chưa làm:** chưa thử điện thoại vật lý/FPS.

### 21.45. Mặt sông kiểu gương theo ảnh mẫu — bản xem trước (25/10/2026, Claude Code — người dùng duyệt, đưa vào game ở 21.46)

- **Yêu cầu:** người dùng thấy mặt sông dưới cầu “còn giả tạo quá”, gửi ảnh mẫu (`ArtSource/Reference/River/river_water_reference.jpg`, sông dưới cầu thép xanh) và yêu cầu dựng lại mặt nước giống ảnh, **chụp gửi xác nhận trước rồi mới đưa vào game**.
- **Quan sát ảnh mẫu:** mặt nước là gương — phản chiếu trời xanh, mây trắng sáng, bờ/cây tối và cầu; gợn nhỏ làm hình phản chiếu rung và hơi kéo dọc, mềm; thân nước xanh đen sâu, sát bờ ngả bùn ô liu; cầu đổ bóng làm tối mặt nước; ánh nắng lấp lánh rất ít; nước lặng, **không có mảng bọt trắng**. Nước cũ (21.37) chỉ là màu xanh ngọc đồng đều, phản chiếu màu trời giả, nhiều bọt sát kè — không phản chiếu gì của cảnh.
- **Cách làm (bản xem trước):**
  - `Assets/PhoViet/Scripts/RiverReflection.cs` (mới): camera gương lật qua mặt nước (ma trận phản chiếu + mặt phẳng cắt xiên ở mực nước), render vào `_RiverReflectionTex` mỗi khung khi có khối cầu trong tầm nhìn. Độ phân giải 42% màn hình, tối đa 768 px; tắt bóng đổ/đèn điểm, LOD bias ×0,5, xa tối đa 150 m để nhẹ cho WebGL. Không có cầu trong tầm nhìn thì không render.
  - `Assets/PhoViet/Scripts/RiverWaterMirror.shader` (mới, “PhoViet/River Water Mirror”): giữ lưới sóng/bóng giải tích của nước cũ nhưng sóng êm hơn (cao ×0,35, tốc ×0,7). Phản chiếu thật lệch theo gợn (ngang nhẹ, dọc ×1,6), làm mềm 5 mẫu nhòe dọc, tô xanh (0,68/0,79/0,88). Thân nước xanh đen `(0,015;0,075;0,12)`, sát kè 2,2 m ngả bùn. Độ phản chiếu 0,5→0,94 theo Fresnel. Chỉ còn một đường nước vỗ mảnh sát kè/trụ, lấp lánh nắng nhỏ, độ đục 0,93. Bỏ qua chính nó khi đang render ảnh phản chiếu (`_RiverReflecting`).
  - `CityGround.shader`: thêm `clip` khi `_RiverReflecting=1`. Lý do: tấm nền thành phố (có mặt dưới) bị vẽ vào ảnh phản chiếu, che hết trời giữa lòng sông. Ngoài lúc render phản chiếu thì không đổi gì.
- **Chưa đưa vào game:** scene và `PV_RiverWater.mat` không đổi, chưa gắn `RiverReflection` vào camera. `AgentScripts/CaptureRiverMirror.cs` chỉ thay vật liệu và gắn camera gương tạm thời trong Play Mode, chụp trước/sau cùng khung hình rồi trả lại như cũ.
- **Ảnh duyệt:** `Captures/river-mirror-sheet.png` (ảnh mẫu + 5 cặp trước/sau: trên cầu góc chơi, từ bờ, nhìn dọc sông, từ mặt cầu, trên cầu ban đêm); ảnh lẻ `Captures/river-mirror-before/after-*.png`; ảnh gỡ lỗi texture phản chiếu `river-mirror-debug-*.png`.
- **Khi người dùng duyệt:**
  - Đổi shader của `PV_RiverWater.mat` sang River Water Mirror, gắn `RiverReflection` (game = PhoVietGame) lên camera chơi và lưu scene.
  - Thêm kiểm tra vào VerifyRiverBridge, chạy lại hồi quy.
  - Đo chi phí khung hình trên bản web; nếu nặng thì giảm `resolution`/`farClip`.
- **Trạng thái:** người dùng duyệt (“Đưa vào game”) và đã cài ở 21.46. Đã build/phát hành trong **1.38.0** (21.53); lúc viết mục này web còn 1.37.0.

### 21.46. Đưa mặt sông gương vào game và cầu dài gấp đôi 50,4 m (25/10/2026, Claude Code — scene đã lưu; phát hành 1.38.0 ở 21.53)

- **Yêu cầu:** “Đưa vào game đồng thời cho chiều dài cây cầu dài hơn gấp đôi nhé” (sau khi xem ảnh duyệt 21.45).
  - Người dùng chọn “Gấp đôi, giữ ngã tư”. Lý do phải chọn: mỗi khối 80 m đều có giao lộ ở z 70, vạch mũi tên của nó trải 55,4–84,6, nên khoảng trống thẳng giữa hai giao lộ chỉ khoảng 50,8 m.
  - Hai phương án bị loại: bỏ giao lộ ở khối có cầu, hoặc chỉ dài gấp rưỡi.
- **Bố cục mới (tọa độ khối):**
  - Mặt sông **z 4,8–55,2 = 50,4 m** (bản cũ 16,8–42 = 25,2 m). Sông bắt đầu ngay sau vạch mũi tên của giao lộ trước (kết thúc 4,6) và dừng sát vạch mũi tên giao lộ sau (bắt đầu 55,4).
  - Vạch dừng ở 56, đèn tín hiệu ở 60,6/77,6, biển tên đường và tấm vỉa hè 58,8–63 giữ nguyên.
  - Hai trụ cầu ở **21,6 và 38,4** chia 3 nhịp 16,8 m.
  - **8 đèn LED cầu** mỗi khối (z 10 / 23,4 / 36,6 / 50 × hai bên), 6 hộp hoa trên lan can mỗi bên.
  - Vạch vàng đứt chạy suốt cầu (tâm 6 + 4k). Vẫn 4 thuyền neo.
  - Bỏ vạch chéo thu hẹp làn vì không còn chỗ. Giữ gờ vàng–đen ở mép kè hai làn ngoài.
  - Biển “CẦU HẸP 2 LÀN” dời lên vỉa hè hai bờ (z 2,9 và 57,2).
  - Đi thẳng ở làn ngoài ra khỏi giao lộ là rơi ngay ở 4,8, nên người chơi phải vào 2 làn giữa từ trong giao lộ. Xe thường tự nhập làn trong 45 m trước cầu như cũ.
- **Hai bờ ngắn** (0–4,8 và 55,2–58,8):
  - Các tấm vỉa hè 8,4 m và tiệm chồng lên đoạn này đều bị ẩn. Hàm mới `bank_land()` trong `pho_viet_river_bridge.py` lát lại *(quảng trường/ghế đá/lối dạo đã thay bằng dãy nhà ống ven sông ở 21.48)*:
    - vỉa hè caro kem–đất nung đúng lưới gạch 0,6 m và độ cao của `PV_StreetPavement`, có gờ bó vỉa;
    - quảng trường ven sông |x| 11,2–26 lát bàn cờ kem–xám 1,2 m, mỗi bờ mỗi bên 2 chậu cây và 1 ghế đá quay ra sông.
  - Mỗi khối cầu ẩn 37 vật (vỉa hè, 14 tiệm, 6 cây, xe đậu, chiếu sáng phố). Đèn tín hiệu, vạch mũi tên, biển tên đường giữ nguyên.
- **Blender MCP:** chạy lại `build_river_bridge()` trong `PhoViet_RiverBridge.blend` (đã ghi lại nguồn).
  - LOD tam giác: **73.060 / 21.130 / 3.662** (bản 21.37: 52.754 / 15.856 / 2.352).
  - Lưới mặt nước LOD0 152×40 ô. Thứ tự slot vật liệu không đổi.
  - Hai thuyền không đổi hình nên trả về bản Git để tránh thay đổi nhị phân thừa.
- **Unity:**
  - `RiverBlock.Near/Far = 4,8/55,2`. Installer kiểm tra hai số này khớp JSON, nếu lệch thì dừng.
  - `PhoVietRiverUpgrade`:
    - Đổi `PV_RiverWater.mat` sang shader **PhoViet/River Water Mirror** và đặt lại mọi thông số như bản đã duyệt 21.45.
    - `_Banks` lấy theo JSON, `_Piers` lấy từ JSON mới `piers`.
    - Gắn `RiverReflection` lên camera chơi (resolution .42, maxSize 768, farClip 120). `PhoVietGame.BuildWorld` cũng gắn khi dựng lại thế giới.
    - Đặt tên **layer 8 = `PV_RiverReflect`** trong TagManager. Gán lớp này cho mô hình cầu (kè, cầu, thuyền, đèn cầu) và mọi tòa nhà “Blender • …”: 5.145 transform.
  - Camera gương chỉ vẽ lớp `PV_RiverReflect` cùng bầu trời. Không vẽ vỉa hè, cây, đèn tín hiệu, mặt đường, xe: các thứ này gần như không thấy từ mặt nước mà tốn draw call.
  - Culling của camera gương dùng phối cảnh thường giới hạn farClip. Ma trận xiên làm méo mặt phẳng xa nên trước đó không loại được gì.
  - Các tham số còn lại: `interval` (mặc định 1, render mỗi khung), `noShadows/lowerLod/noPixelLights` (mặc định bật), `allLayers` (chỉ để so sánh).
  - Không có lớp nào khác dùng layer, vì game không dùng Physics/LayerMask.
  - Cài lại 35 khối: 280 đèn cầu, 140 thuyền, 70 mesh cắt.
- **Hiệu năng (Editor, Play Mode, vSync tắt, 280 khung mỗi pha, có sông trong khung):**
  - Trung vị khung hình **51,9 ms khi tắt gương, 54,7 ms khi bật**, tức gương tốn khoảng **2,8 ms (~5%)**. Đo bằng `AgentScripts/ProbeMirrorFrames.cs`, kết quả ở `Captures/river-mirror-frames.json`.
  - Phép đo `camera.Render()` lặp từng lần cho kết quả 18–41 ms dao động vô lý, nên đã bỏ.
  - Chưa đo trên điện thoại/WebGL thật.
- **Kiểm tra:**
  - `VerifyRiverBridge` thêm các điều kiện:
    - sông 50,4 m nằm giữa hai vạch mũi tên giao lộ;
    - `_Banks/_Piers` của nước khớp sông dài;
    - camera chơi có `RiverReflection`;
    - có sông trong khung thì render ảnh phản chiếu, không có sông thì không render (tắt cầu ở mọi dãy đường, kể cả đường nhánh);
    - 280 đèn cầu.
  - Các vị trí test đổi theo sông mới (83/137 ngoài sông).
  - Xe cùng chiều nhập làn sinh ở 50 m với tốc độ 14 m/s. Xe máy 7 m/s bị người chơi vượt trước khi tới cầu nên lần chạy đầu trượt; đây là lỗi test, không phải lỗi game.
  - `CaptureRiverBridge` thêm góc toàn cảnh, bờ gần, bờ xa, và tắt xe tăng bắn khi chụp. `CaptureRiverMirror` chỉ dùng cho bản xem trước, giờ sẽ dừng nếu scene đã có nước gương.
  - Ảnh `Captures/river-bridge-long-sheet.png` gồm 8 góc: toàn cảnh, tới ngã tư trước cầu, giữa cầu, nhìn dọc sông, bờ gần, bờ xa, thuyền/kè, ban đêm.
  - Hồi quy sau khi cài (Play Mode): **28 suite / 9.751 điều kiện đạt**. Gồm VerifyRiverBridge 142 (chạy riêng 3 lần liền đều 142), TankBarrage 69, RoofRiding 4.206, MeshyCars 1.266, MeshyTank 1.119, StreetArt 333, JunctionRoutes 92, NightJumpLighting 88, PortraitHud 21, MobileCamera 8. Sau khi chạy, scene ở Edit Mode, không bẩn.
- **Trạng thái:** scene đã lưu (jobs khóa). **Đã build/phát hành trong **1.38.0** (21.53); lúc viết mục này web còn 1.37.0.

### 21.47. Sửa nền đường thừa và nhà lơ lửng ở hai đầu cầu dài (25/10/2026, Claude Code — scene đã lưu; phát hành 1.38.0 ở 21.53)

- **Báo lỗi (ảnh chơi trong Unity):**
  - Dư 4 mảng nền đường (khung đỏ): mảng xám có vạch trắng ở hai làn ngoài, kéo từ mép kè ra mặt sông ở cả hai đầu cầu.
  - Nhà lơ lửng trên mặt sông ở đầu cầu và bên kia cầu (khung xanh). Người dùng cho phép kéo sát nền hoặc ẩn/xóa tùy chọn.
- **Nguyên nhân 1 (nền đường thừa):** mesh gộp đã cắt đúng (đếm tam giác: 0 tam giác trên mặt sông).
  - Khi cài lại, `PhoVietRiverUpgrade.ClipRiver` ghi vào asset có sẵn bằng `EditorUtility.CopySerialized`. Dữ liệu lưu đã đổi, nhưng bản mesh trên GPU của Editor đang mở vẫn là bản cắt cũ 16,8–42. Vì vậy trong Play Mode đoạn 4,8–16,8 và 42–55,2 còn hiện mặt đường làn ngoài.
  - Đã xác định bằng cách lần lượt tắt từng nhóm renderer: tắt mesh gộp “Geometry •” thì mất mảng xám; mô hình cầu không có mặt nào ở đó.
  - **Sửa:** `ClipRiver` ghi thẳng vào mesh có sẵn qua API Mesh (`Clear`, `SetVertices`/`SetTriangles`…, `UploadMeshData(false)`), rồi cài lại 35 khối. Bản build đọc dữ liệu lưu nên không bị lỗi này, nhưng Editor của người dùng thì có.
- **Nguyên nhân 2 (nhà lơ lửng):** dãy nhà của đường ngang ở giao lộ trước/sau (tiệm của các dãy đường nhánh, sâu tới 25,8 m tính từ mặt tiền) từng nằm ngoài sông 16,8–42. Với sông 4,8–55,2, chúng đứng một phần trên mặt sông.
  - **Sửa — ẩn khi đứng trên sông:** `PhoVietRiver.HideBuildingsOverRiver()` chạy đầu mỗi `TickRide`.
    - Mặt bằng mỗi tòa “Blender • …” của mọi khối được tính một lần trong hệ khối, từ bounds mesh, nên dùng được cả khi đang ẩn.
    - Tòa nào của khối **khác** chồng lên dải mặt sông (|x| < 95, z 5,1–54,9) của một khối cầu đang bật thì ẩn; hết chồng thì hiện lại.
    - Không bật lại tòa mà chính khối của nó đang là cầu đã ẩn (streetOnly).
    - Chỉ xét các cặp khối trong vòng 260 m.
    - `ResetRivers` trả lại mọi tòa đã ẩn. `BuildingsHiddenOverRiver` đếm số tòa đang ẩn.
- **Kiểm tra:**
  - `VerifyRiverBridge` **145 điều kiện** đạt, thêm: chạy tự nhiên tới cầu dài; nhà đường ngang trên sông được ẩn, không tòa nào khác đứng trên mặt nước; chơi lại thì hiện lại đủ. `VerifyStreetArt` 333 đạt.
  - Ảnh `Captures/river-ends-fix-sheet.png`: trước/sau ở đầu cầu, tới ngã tư, cuối cầu và nhìn từ trên.
  - Script gỡ lỗi `AgentScripts/ProbeRiverPatch.cs` (đếm tam giác mesh gộp trên mặt sông) và `AgentScripts/CaptureRiverEnds.cs` (chạy tự nhiên tới cầu, chụp hai đầu).
  - Hồi quy đầy đủ sau sửa (Play Mode): **28 suite / 9.782 điều kiện đạt**. Gồm VerifyRiverBridge 145 (khi chạy tự nhiên tới cầu ẩn 64 tòa đường ngang, 0 tòa còn đứng trên mặt sông), TankBarrage 72, RoofRiding 4.206, MeshyTank 1.144, StreetArt 333, PortraitHud 21, MobileCamera 8. Sau khi chạy, Editor ở Edit Mode, scene không bẩn.
- **Trạng thái:** scene đã lưu (jobs khóa). **Đã build/phát hành trong **1.38.0** (21.53); lúc viết mục này web còn 1.37.0.

### 21.48. Dãy nhà ống ven sông lấp đất trống, bỏ hàng rào dài ở góc ngã tư (25/10/2026, Claude Code — scene đã lưu; phát hành 1.38.0 ở 21.53)

- **Báo lỗi (ảnh chơi trong Unity):**
  - Khung xanh: hàng rào bên trái hiển thị như chắn ngã tư nhưng xe vẫn quẹo trái được.
  - Khung đỏ: đất trống không nhà cửa ở góc ngã tư và hai bờ sông, nhìn từ xa không thấy nhà.
  - Người dùng yêu cầu dựng thêm nhà vào các vùng này và không được để nhà lòi ra/lơ lửng trên sông.
- **Nguyên nhân:** sau 21.47, dãy nhà của đường ngang đứng trên sông bị ẩn. Giữa vỉa hè đường ngang và kè chỉ còn dải rộng **3,6 m** (z 1,2–4,8 ở bờ gần, 55,2–58,8 ở bờ xa, đo trên khối cầu khi chạy tự nhiên). Dải này chỉ có quảng trường lát gạch và lan can sắt dài từ |x| 7,15 tới 95. Vì vậy góc ngã tư trống trơn, lan can nhìn như chắn ngang lối rẽ.
- **Sửa (Blender MCP, `pho_viet_river_bridge.py`):**
  - **Dãy nhà ống ven sông `riverside_row()`/`house()`** *(thay bằng khối căn hộ cao cấp ở 21.49)*:
    - Nhà sâu 3 m: bờ gần z 1,5–4,5, mặt tiền quay ra đường ngang (−z); bờ xa z 55,5–58,5, mặt tiền +z. Lưng nhà quay ra sông.
    - Chạy từ |x| 11,4 (sau vỉa hè đường chính) tới 95 (hết sông). Mỗi bên đường chính rộng ngẫu nhiên 4,2–7 m có hạt giống, tổng cộng **61 nhà mỗi khối cầu**.
    - 2–5 tầng (trệt 3,6 m, tầng trên 3 m), màu mặt tiền từ 10 màu bảng phố.
    - Tầng trệt: 60% mặt kính cửa hàng khung trắng có dải đèn, còn lại cửa cuốn xám. Có hộp cuốn, biển hiệu chữ Việt (TẠP HÓA, NHÀ TRỌ, CƠM BÌNH DÂN, SỬA ĐIỆN, TIỆM VÀNG…). 45% nhà có mái hiên sọc.
    - Tầng trên: cửa sổ khung trắng, ban công lan can sắt xen kẽ có chậu hoa, cục nóng.
    - Lưng nhà: cửa sổ mỗi tầng, cục nóng sâu 0,26 m.
    - Mái ngói hai dốc hoặc sân thượng có tường chắn và bồn nước inox.
  - Không phần nào lấn mặt nước. Đã kiểm trong Blender: đỉnh nhà xa nhất 4,76 (kè 4,8) và 55,24 (kè 55,2).
  - `bank_land()` chỉ còn vỉa hè caro của đường chính (|x| 7–11,2), nền lát dưới dãy nhà và **một đoạn lan can ngắn** ở góc đường chính (|x| 7,2–11,4) cạnh mố cầu. Đã bỏ quảng trường, ghế đá, chậu cây và lối dạo/lan can dài (`promenade` không còn dùng).
  - Dãy nhà là một phần mô hình cầu nên chỉ hiện khi khối là cầu, nằm trên lớp phản chiếu (soi bóng xuống sông).
  - LOD tam giác **101.712 / 33.504 / 4.386** (21.46: 73.060 / 21.130 / 3.662).
- **Vật cản:** JSON mô hình có thêm `obstacles` (61 hộp “Nhà ven sông”). `PhoVietRiverUpgrade` gắn `StreetObstacleSet` lên “Cầu vượt sông • Blender” của từng khối: 2.135 hộp. Vật cản chỉ có hiệu lực khi khối là cầu, vì `SceneryContact` bỏ qua đối tượng không hoạt động.
- **Kiểm tra:**
  - `VerifyRiverBridge` thêm 3 điều kiện:
    - mỗi khối cầu có dãy nhà ven sông và vật cản (≥ 50);
    - mọi nhà nằm trên bờ ngắn, cách vỉa hè đường chính, không chồng mặt sông;
    - tổng bề rộng nhà phủ kín cả hai bờ từ vỉa hè tới hết sông.
  - Ảnh `Captures/river-houses-sheet.png`: ngã tư trước/sau cầu, đường ngang ven sông, trên cầu, toàn cảnh, nhìn từ trên. Script chụp `AgentScripts/CaptureRiverJunction.cs` chạy tự nhiên tới cầu, chụp và đo mép đường ngang trong hệ khối cầu.
  - Sửa trong lúc kiểm tra:
    - Dãy nhà ban đầu dừng khi còn dưới 3 m, để hở tới 3 m ở cuối sông. Giờ nhà cuối dãy nhận phần dư, mỗi bờ phủ đúng 167,2 m (2 × 83,6).
    - `VerifyCrashUpgrade` giờ đếm riêng 735 bộ vật cản phố và 35 bộ của dãy nhà ven sông.
  - Lần chạy đầu `VerifyCrashPoliceNavigation` trượt một lần (“Unbounded police movement in Moving traffic changes the planned route”), chạy lại 5/6 lần đạt. Đây là lỗi CSGT chập chờn đã ghi từ trước. Suite này chạy với sông tắt, nên không có cầu, vật cản dãy nhà hay ẩn nhà nào hoạt động. (Nguyên nhân thật và bản sửa: 21.50.)
  - Hồi quy đầy đủ cuối (Play Mode): **28 suite / 9.753 điều kiện đạt**. Gồm VerifyRiverBridge 148, CrashUpgrade 93, CrashPoliceNavigation 214, TankBarrage 69, RoofRiding 4.206, MeshyTank 1.115, StreetArt 333. Sau khi chạy, Editor ở Edit Mode, scene không bẩn.
- **Trạng thái:** scene đã lưu (jobs khóa). **Đã build/phát hành trong **1.38.0** (21.53); lúc viết mục này web còn 1.37.0.

### 21.49. Căn hộ cao cấp kính và ban công ở hai bờ sông (25/10/2026, Claude Code — scene đã lưu; phát hành 1.38.0 ở 21.53)

- **Yêu cầu:** (ảnh chơi, thẻ “Trúng đạn xe tăng!” trên cầu) nhà bên kia cầu quá sơ sài, đơn điệu. Thiết kế lại thành nhà cao tầng giống thật, kiểu căn hộ chung cư cao cấp có ban công và cửa kính trong suốt.
- **Thiết kế (Blender MCP, `pho_viet_river_bridge.py`):** `house()` đổi thành `apartment_block()`; `riverside_row()` xếp khối rộng 10–16,8 m. Khối cuối nhận phần dư để mỗi bờ vẫn phủ đúng |x| 11,4–95, tổng **24 khối mỗi khối cầu**. Vẫn sâu 3 m, mặt bằng như 21.48, nên không phần nào lấn mặt nước: đỉnh xa nhất 4,758 và 55,243, kè 4,8 và 55,2.
  - **Chiều cao:** 6–9 tầng (trệt 3,6 m, tầng trên 3 m), vỏ trắng/đá/kem/xanh nhạt.
  - **Mặt hướng sông (thấy từ cầu):**
    - Mỗi tầng có gờ sàn ở lưng nhà và lan can kính trong suốt kèm tay vịn inox.
    - **Ban công lõm 1 m** để không nhô ra sông.
    - **Vách kính trong suốt sàn–trần** chia ô bằng đố, theo 3 kiểu: đố tối, lam đứng, khung trắng.
    - Sau kính là phòng sâu 1,25 m: trần có đèn, vách sau 6 màu ấm, sofa, cây cảnh; ngoài ban công có chậu cây.
    - Mái là sân thượng vườn có lan can kính và phòng kỹ thuật.
  - **Source21.91 (chưa phát hành):** cục nóng đã nâng thành mẫu xám trắng có quạt/lưới/khe/giá; phía sông mỗi tòa một bảng CHO THUÊ NHÀ, tầng cao thấp khác nhau1–7, đủ3LOD. Giữ toàn bộ kiến trúc/vật cản/kính/phòng/biển tiệm hiện tại; xem21.91.
  - **Mặt đường ngang:** 2–3 gian cửa hàng tầng trệt (kính có dải đèn hoặc cửa cuốn, biển chữ như TẠP HÓA, SPA, NGÂN HÀNG, MINI MART, BẤT ĐỘNG SẢN…, mái hiên). Các tầng trên có cửa sổ khung trắng, ban công sắt xen kẽ, cục nóng.
  - **Kính trong suốt:** mặt tag `Glass` dùng submesh vật liệu `PV_StreetGlass` của phố (cùng kính cửa hàng và tòa văn phòng 21.27). `to_mesh` của script phố thêm slot này; `river_mesh` nối các slot sông sau nó.
    - Thứ tự slot Unity mới: PV_StreetAtlas, PV_RiverMurk, PV_RiverWater, PV_RiverGroundMask, **PV_StreetGlass**, PV_RiverAsphalt, PV_RiverLineWhite, PV_RiverLineYellow.
    - `PhoVietRiverUpgrade` ánh xạ PV_StreetGlass tới `Assets/PhoViet/Art/Streets/PV_StreetGlass.mat`; nếu thiếu vật liệu thì dừng.
  - LOD tam giác **161.094 / 64.188 / 15.014** (21.48: 101.712 / 33.504 / 4.386). Vật cản 24 hộp “Nhà ven sông” mỗi khối, tổng 840.
- **Kiểm tra:**
  - `VerifyRiverBridge`: mỗi khối cầu có ≥ 16 khối căn hộ cao ≥ 18 m có vật cản; renderer cầu có vật liệu kính `PV_StreetGlass`. Các điều kiện 21.48 (không lấn sông, phủ kín hai bờ) vẫn giữ.
  - `CaptureRiverBridge` thêm 3 góc: từ cầu nhìn sang căn hộ, cận cảnh từ trên mặt sông, ban đêm. Góc toàn cảnh nâng lên 40 m vì nhà cao.
  - Ảnh `Captures/river-apartments-sheet.png`.
  - Hồi quy đầy đủ (Play Mode): **27/28 suite đạt** (VerifyRiverBridge 149, TankBarrage 69, MeshyTank 1.133, RoofRiding 4.206, StreetArt 333, CrashUpgrade 93, PortraitHud 21, MobileCamera 8…).
  - `VerifyCrashPoliceNavigation` trượt ở “Unbounded police movement in Moving traffic changes the planned route”; chạy lại 3 lần: 2 đạt (214), 1 trượt cùng chỗ.
  - Đây là lỗi CSGT chập chờn đã ghi từ trước (21.27, 21.48), không liên quan thay đổi này: suite chạy với sông tắt, logic CSGT không dùng vật cản cảnh (`SceneryContact` chỉ dùng cho người chơi/đạp).
  - Từ 21.48 tần suất trượt có vẻ cao hơn (3 lần/9 lần chạy, trước đó 0/4) nên cần điều tra riêng — đã điều tra và sửa ở 21.50 (không liên quan cầu/sông).
  - Sau khi chạy, Editor ở Edit Mode, scene không bẩn.
- **Đèn phòng ban đêm:** đã làm ở 21.51.
- **Trạng thái:** scene đã lưu (jobs khóa). **Đã build/phát hành trong **1.38.0** (21.53); lúc viết mục này web còn 1.37.0.

### 21.50. Sửa lỗi chập chờn “Unbounded police movement” của VerifyCrashPoliceNavigation (25/10/2026, Claude Code — source; phát hành 1.38.0 ở 21.53)

- **Triệu chứng:** ca “Moving traffic changes the planned route” trượt khoảng 1/3–1/6 số lần chạy: xe CSGT dịch quá `max(12, PoliceSpeed+2)·dt + 0,2` m trong một tick. Lỗi xảy ra cả khi chạy suite riêng, nên giả thuyết “trạng thái tích lũy giữa các suite” (21.24) là sai.
- **Không liên quan cầu/sông (21.48–21.49):** suite chạy với `riversEnabled=false`. `HideBuildingsOverRiver()` mỗi tick chỉ bật/tắt hiển thị nhà ven sông, không dùng `rng`, không đụng xe hay CSGT, và CSGT không dùng vật cản cảnh. Chênh lệch 0/4 → 3/9 là dao động ngẫu nhiên của số lần chạy nhỏ.
- **Nguồn ngẫu nhiên:** mỗi `RestartRide()` gieo `RunSeed` mới. Ô xe tải bốc ngẫu nhiên Covered (6,4 m) hay Container (12,8 m) cho từng xe trong ca, và chuỗi va chạm CSGT ↔ xe tải ↔ xe tải phụ thuộc tổ hợp đó.
- **Nguyên nhân 1 (lỗi game, gốc của “Unbounded”):** `RememberTrafficCourse` coi mọi xe có `routeDetached` bị hất chủ yếu theo trục x là xe băng ngang.
  - Xe được gán hệ trục của **ngã tư gần nhất**, dù ngã tư cách 15–20 m (xe đang nằm giữa đoạn thẳng).
  - Sau 3 s, pha đứng dậy nội suy 0,65 s tới làn của phố ngang đó, tức xe trượt **15–20 m trong 0,65 s** (~35–46 m/s, đo được `slide=20.5`, đích z≈−11,6), xuyên qua CSGT đang ngã. `ResolvePoliceContact` đẩy CSGT 0,46–0,50 m/tick.
  - Trong game, mọi xe đều `routeDetached` sau khi người chơi rẽ, nên xe bị hất ngang sau khi rẽ cũng có thể trượt như vậy.
- **Sửa (`PhoVietTrafficRecovery.cs`):**
  - Chỉ dùng hệ trục phố ngang khi `v.crossing`, hoặc tâm xe cách tâm ngã tư ≤ `|LaneX[3]|+1,55` = 6,2 m (trong bốn làn phố ngang).
  - Hướng chạy gần vuông góc với hệ trục thì dấu của nó không cho biết chiều làn. Khi đó chọn chiều theo nửa đường xe đang đứng, thay vì có thể kéo sang làn ngược chiều bên kia (đo được trượt 9,6 m).
  - Thời gian đứng dậy `StandDuration = max(0,65; standTime)`, với `standTime = 1,5·(quãng trượt + |Δyaw|·max(nửa dài, nửa rộng)) / StandUpSweepSpeed` (10 m/s; 1,5 là đỉnh đạo hàm SmoothStep). Xe máy/xe con nhích vào làn vẫn 0,65 s; container xoay ~90° mất ~3,3 s thay vì quét đầu xe ước tính ~23 m/s. `StandUpProgress` (hoạt ảnh Recover của xe máy) chia theo `StandDuration`.
- **Nguyên nhân 2 (lỗi game, lộ ra khi xe tải không còn trượt đi):** CSGT kẹt cạnh xe tải đứng yên suốt 35 s (`reaches fallen rider… gap≈11,7`).
  - `RoutePolicePathClear` dùng AABB của thân xe đã xoay cộng đệm 0,08 m. Container nằm chéo 43° có AABB ~10,6×11 m phủ cả mặt đường trống. Ngay cả xe Covered lệch 4° cũng khiến CSGT cách thân thật 2–7 cm bị coi là nằm trong hộp.
  - Phép quét từ điểm xuất phát chồng lấn chặn **mọi** hướng (đầu dò 16 hướng đều chặn, A* rỗng).
  - Va chạm chỉ đẩy CSGT ra 2,5 cm (nhỏ hơn đệm), còn va chạm giữa lúc quét đặt CSGT sát thân (khe 0).
- **Sửa (`PhoVietRoutePolice.cs`, `PhoVietPolice.cs`):**
  - Quét SAT 4 trục giữa hộp CSGT xoay theo hướng bước đi và hộp xe xoay thật (cùng toán `ContactHalfSpace` của `VehicleContact`; xe máy ngã dùng nửa rộng ≥1,05 như `VehicleFootprint`).
  - Đệm `PolicePathPadding=0,08` chỉ áp cho xe có vận tốc, quay hoặc đang đứng dậy (để bù sai số ngoại suy 0,15 s); xe đứng yên dùng hình thật.
  - Trong chế độ té/cua (`RiderCrashActive||routePolice`), sau va chạm CSGT được đẩy ra thêm `PolicePathPadding+0,02` theo pháp tuyến (đuổi đường thẳng giữ 2,5 cm cũ).
- **Đã thử và bỏ:**
  - Bắt xe tải chờ khi CSGT trong vùng quét đứng dậy: gây khóa chết, vì xe tải chờ CSGT còn CSGT bị hai xe tải chắn lối.
  - Cho phép “bước thoát” khi xuất phát trong hộp, theo độ sâu AABB/trục nông nhất: làm hỏng ca túi ba xe tải 12–20/20 lần.
- **Không đổi test:** `AgentScripts/VerifyCrashPoliceNavigation.cs` giữ nguyên (214 điều kiện, giới hạn bước như cũ). Script chẩn đoán chỉ nằm ở scratchpad phiên (bản sao suite có ghi trạng thái xe/hồi phục ở bước lỗi, đầu dò 16 hướng/A*), không đưa vào repo.
- **Kiểm tra (Play Mode, trình chạy cũ: chỉ khi `Application.isPlaying` mới đặt `riversEnabled=false; tankBarrageEnabled=false; randomTanks=true; ResetRide()`):**
  - Trước sửa: bản sao suite có ghi chép trượt 1/6 và 1/8 lượt; 5/8 lượt có xe đứng dậy trượt 15,5–20,5 m.
  - Sau sửa: bản sao có ghi chép **20/20** lượt đạt, không còn lần đứng dậy nào trượt >1,6 m. File test thật **10/10 lượt liên tiếp đạt 214**.
  - Hồi quy cùng trình chạy, sau bản sửa cuối: **19 suite / 7.747 điều kiện đạt**. Gồm CrashPoliceNavigation 214, CrashUpgrade 93, CrashOutcome 51, RiderContacts 45, TrafficDynamics 82, BikeArt 156 (hoạt ảnh Recover), RearRandomPolice 60, Pursuit 39, JunctionKick 216, QuickKick 62, RoadDrivers 45, SmoothTurns 468, JunctionRoutes 92, Gameplay 23, DistanceMode 31, Reckless 120, BoostTrafficIsolation 478, MeshyCars 1.266, RoofRiding 4.206. Không chạy RiverBridge/TankBarrage (một phiên khác đang sửa file cầu/sông).
  - Sau khi chạy, Editor ở Edit Mode; `PV_Racer.mat`/`PV_TrafficPolice.mat` không bị đổi.
- **Trạng thái:** chỉ sửa mã nguồn 3 file (`PhoVietTrafficRecovery.cs`, `PhoVietRoutePolice.cs`, `PhoVietPolice.cs`). Không đổi scene/asset; jobs vẫn khóa (`RideJobsReleased=false`, `jobsEnabled=false`). **Đã build/phát hành trong **1.38.0** (21.53); lúc viết mục này web còn 1.37.0.

### 21.51. Phòng căn hộ sau kính sáng đèn ban đêm (25/10/2026, Claude Code — scene đã lưu; phát hành 1.38.0 ở 21.53)

- **Yêu cầu:** “Cho các phòng căn hộ sau kính sáng đèn vào ban đêm nhé” (tiếp 21.49).
- **Shader mới `Assets/PhoViet/Scripts/ApartmentRoom.shader` (“PhoViet/Apartment Room”):** Standard bề mặt dùng bảng màu phố `PV_StreetPalette.png`, nên ban ngày giống hệt atlas.
  - Emission = màu bảng phố × `_Warm` (1; 0,86; 0,66) × `_Glow` 1,15 × global **`_PVNightLights`**. `DayNightLighting.ApplyEnvironment` đã đặt global này mỗi khung bằng `LampLevel`: cùng nhịp bật đèn đường, sáng dần lúc chạng vạng, tắt lúc sáng.
  - Không thêm danh sách vật liệu hay mã runtime nào.
- **Blender MCP (`pho_viet_river_bridge.py`):** tag mới `Room` → submesh `PV_RiverRoomLight`.
  - Mỗi tầng căn hộ bật đèn theo hàm băm `(k·7 + |cx|·13 + |zf|·7) % 10 < 7`, tức **~70% phòng sáng**. Hàm băm giống nhau ở mọi LOD.
  - Phòng sáng dùng vách tông ấm `LIT_INTERIORS` (interiorWall, interior, cream, woodLight, peach, trim) và máng đèn trần `lamp` có tag `Room`. Phòng tắt dùng vách thường.
  - Dải đèn cửa hàng tầng trệt **không** gắn tag: nó chỉ có ở LOD0, nên gắn sẽ làm lệch thứ tự slot giữa các LOD (export kiểm tra thứ tự giống nhau).
  - Thứ tự slot mới: PV_StreetAtlas, PV_RiverMurk, PV_RiverWater, PV_RiverGroundMask, PV_StreetGlass, **PV_RiverRoomLight**, PV_RiverAsphalt, PV_RiverLineWhite, PV_RiverLineYellow. LOD tam giác 161.418 / 64.188 / 15.014.
- **Unity:** `PhoVietRiverUpgrade` tạo/cập nhật `Assets/PhoViet/Art/River/PV_RiverRoomLight.mat` (Apartment Room + PV_StreetPalette, Glow 1,15, Warm, Smoothness 0,2); nếu thiếu bảng màu thì dừng. Đã cài lại 35 khối, scene đã lưu.
  - Phòng sáng nằm trên lớp phản chiếu (`PV_RiverReflect`) nên ánh đèn soi bóng xuống mặt sông.
- **Kiểm tra:**
  - `VerifyRiverBridge` thêm 2 điều kiện: mọi LOD cầu có vật liệu Apartment Room có bảng màu; `_PVNightLights` > 0,95 lúc nửa đêm của chu kỳ và < 0,05 lúc đầu ngày.
  - `CaptureRiverBridge` thêm góc đêm cận cảnh và chạng vạng (30% chu kỳ).
  - Ảnh `Captures/river-apartments-night-sheet.png`: ngày, chạng vạng, đêm, đêm cận cảnh, góc chơi trên cầu ban đêm, ngày cận cảnh.
  - Hồi quy đầy đủ (Play Mode, gồm cả sửa CSGT 21.50): **28 suite / 9.751 điều kiện đạt**. Gồm VerifyRiverBridge 151, TankBarrage 69, CrashPoliceNavigation 214, RoofRiding 4.206, MeshyTank 1.111, StreetArt 333, NightJumpLighting 88, PortraitHud 21, MobileCamera 8. Sau khi chạy, Editor ở Edit Mode, scene không bẩn.
- **Trạng thái:** scene đã lưu (jobs khóa). **Đã build/phát hành trong **1.38.0** (21.53); lúc viết mục này web còn 1.37.0.

### 21.52. Nhà đường ngang hiện lơ lửng trên sông khi té/rơi sông (25/10/2026, Claude Code — scene không đổi; phát hành 1.38.0 ở 21.53)

- **Báo lỗi (ảnh chơi trong Unity, thẻ “RƠI XUỐNG SÔNG”):** khi người chơi va chạm, nhà ở bên kia cầu “chuyển cảnh” thành nhà bay lơ lửng trên mặt nước. Người dùng yêu cầu kiểm tra lại toàn bộ.
- **Nguyên nhân:** `HideBuildingsOverRiver()` (21.47) chỉ chạy ở đầu `TickRide`, tức chỉ khi đang chạy xe. Nhưng khi té (`TickRiderCrash`) và khi rơi/chìm sông (`SinkRider`, cả sau thẻ kết thúc qua `TickRiverAftermath`), phố **vẫn cuộn** qua `ScrollRoutes`:
  - khối bị đẩy ra sau được đưa lên phía trước và có thể thành khối cầu mới (`AssignRecycledRiverBlock` → `SetBridge` bật lại nhà/vỉa hè);
  - các dãy đường nhánh dịch theo.
  - Vì vậy trạng thái ẩn/hiện nhà đứng yên theo vị trí cũ.
  - `PrepareBranches` (đặt hai dãy đường ngang của giao lộ kế tiếp, cả lúc reset/rẽ) cũng không gọi lại.
- **Sửa:** gọi `HideBuildingsOverRiver()` ở cuối `ScrollRoutes` (mọi đường cuộn phố: chạy, té, chìm sông, sau thẻ kết thúc) và cuối `PrepareBranches` (`PhoVietRoutes.cs`). Lời gọi ở đầu `TickRide` giữ nguyên. Không đổi scene/asset.
- **Kiểm tra (`VerifyRiverBridge`, 153 điều kiện):** hàm `floating()` đếm tòa “Blender • …” đang hiện của khối khác đứng trên mặt nước của **bất kỳ** khối cầu đang bật nào, kèm tên ví dụ khi trượt. Kiểm tra ở các thời điểm:
  - chạy tự nhiên tới cầu;
  - té ngay trước cầu và trượt 14 m/s, kiểm từng khung suốt pha té + 3 s sau thẻ kết thúc;
  - đi làn ngoài rơi xuống sông rồi chìm 8 s, kiểm mỗi 5–10 khung;
  - sau khi chơi lại (màn hình sẵn sàng).
  - Lần chạy đầu ngay sau khi Unity biên dịch lại trượt một lần ở kiểm tra “sau khi chơi lại”. 6 lần chạy sau và probe 12 lượt (chơi lại / rơi sông rồi chơi lại) đều 0 nhà lơ lửng; chưa tái hiện được, đã thêm thông tin chẩn đoán vào thông báo.
  - Không chạy được bản mã cũ để tái hiện, vì vào Play Mode là Unity biên dịch bản mới.
- **Ảnh:** `AgentScripts/CaptureRiverCrash.cs` chạy tự nhiên tới cầu, chụp rơi sông ở đầu cầu, ở cuối cầu (gần dãy căn hộ bờ xa như ảnh người dùng) và té trên đường trước cầu → `Captures/river-crash-sheet.png`.
- **Hồi quy đầy đủ (Play Mode): 28 suite / 9.770 điều kiện đạt.** Gồm VerifyRiverBridge 153, TankBarrage 69, CrashPoliceNavigation 214, CrashOutcome 51, RiderContacts 45, RoofRiding 4.206, StreetArt 333. Sau khi chạy, Editor ở Edit Mode, scene không bẩn.
- **Trạng thái:** Đã build/phát hành trong **1.38.0** (21.53).

### 21.53. Build/phát hành 1.38.0 — sông gương, cầu dài, căn hộ kính, sửa nhà lơ lửng và CSGT (25/10/2026, Claude Code)

- **Ủy quyền:** người dùng yêu cầu trực tiếp “Build WebGL và phát hành lên web nhé” sau khi nhận báo cáo 21.52, gồm commit, build, push main và GitHub Pages.
- **Phạm vi:**
  - 21.45–21.46: mặt sông gương và cầu dài 50,4 m.
  - 21.47: nền đường thừa, nhà đường ngang trên sông.
  - 21.48–21.49: dãy nhà rồi căn hộ kính ven sông.
  - 21.50: sửa CSGT, do phiên phụ “Điều tra lỗi chập chờn VerifyCrashPoliceNavigation” làm, đã đọc lại diff.
  - 21.51: phòng sáng đèn đêm.
  - 21.52: nhà lơ lửng khi té/rơi sông.
  - Công việc vẫn khóa (`RideJobsReleased=false`, `jobsEnabled=false`). Cờ scene: `riversEnabled=1`, `tankBarrageEnabled=1`, `randomTanks=0`, `morningLook=3`, `rearWarningStyle=3`. Layer 8 `PV_RiverReflect` (TagManager) đi kèm.
  - Không kéo vào commit: 5 `.blend` cục bộ cũ, `Art/Bikes/` cũ, Wave/WaveAlpha, `Assets/ai.meshy`, `Packages/*`, blend ánh sáng/sương của người dùng, ảnh/JSON lịch sử đổi do chạy lại test.
- **Version:** 1.37.0 → **1.38.0** ở `PhoVietWebBuild.cs`, `ConfigureWebRelease.cs`, `PlayerSettings.bundleVersion`, kỳ vọng `VerifyWeb.cjs`, `VerifyFullscreenGame.cjs`, `VerifyCrashPoliceWeb.cjs`, `VerifyMorningFogWeb.cjs`, `VerifyWebBundle.py` (ghi `release-1.38-bundle-checks.json`). README thêm mục 1.38.0.
- **Hồi quy native trên đúng mã phát hành:** 28 suite / 9.770 điều kiện (21.52, gồm sửa CSGT 21.50). Sau đó chỉ đổi chuỗi version và preflight.
- **Preflight `VerifySaigonRelease`:** sửa để đếm riêng 35 bộ vật cản dãy căn hộ ven sông (trước đó đòi đúng 735 bộ nên trượt). Kết quả: scene đã lưu, version 1.38.0, jobs khóa, 735 bộ / 8.050 hộp phố + 35 bộ ven sông, 1.260 LOD phố, 1.350 renderer kính, 420 tín hiệu → `release-1.38-preflight.json`.
- **Build:**
  - `build_13392e2af78d` **Succeeded**, 310.288 ms, 0 lỗi / 3 cảnh báo đã biết.
  - Cache **`1ffc9c93f6c6d874`**, UTC `2026-10-25T09:11:28.4593563Z`.
  - Precache 98.876.670 byte; data 91.584.962 byte, tăng ~0,7 MB, **còn ~8,4 MB tới giới hạn 100 MB/file của GitHub**.
  - Đã chép sang `docs`, dọn đúng 4 file hash cũ.
- **Kiểm tra artifact local:** 29 kiểm tra bundle. Trình duyệt local **77 điều kiện, 0 lỗi runtime**: DPR3 dọc/ngang/tablet/xoay lại 14 (`release-1.38-mobile-results.json`), tài nguyên + WASM 26 (`release-1.38-local-results.json`), PWA/offline/cảm ứng 37 (`browser-release-1.38-pwa.json`, bị gitignore). Giả lập trình duyệt, chưa thử điện thoại vật lý/FPS (gương phản chiếu chưa đo trên thiết bị thật).
- **Phát hành:**
  - Source/artifact `bec6ce1`, push main.
  - Pages run **40180769646** completed/success đúng SHA (`Captures/release-1.38-pages.json`).
  - Live: version 1.38.0 / cache `1ffc9c93f6c6d874`; `VerifyPublishedGame` **26 kiểm tra đạt, 0 lỗi runtime** (`release-1.38-live-results.json`).
- **Chơi thật bản live (Chrome GPU, 3 lượt, 0 lỗi trang):** thấy cầu dài giữa hai ngã tư, mặt sông gương phản chiếu dãy căn hộ kính bờ xa, biển CẦU HẸP và gờ vàng–đen ở mép kè (`release-1.38-live-bridge.png`, `release-1.38-live-bridge-2.png`). Bot không lái nên té ở 56–80 m, ngay giao lộ trước cầu đầu tiên.
- **Chưa làm:** chưa thử điện thoại vật lý/FPS; chưa đo chi phí gương phản chiếu trên thiết bị thật.

### 21.54. Giảm tải khu sông/cầu mà không đổi thiết kế và hiệu ứng (25/10/2026, Claude Code — scene đã lưu, chưa build)

- **Yêu cầu:** “Mặt nước hiện giờ quá nặng, có cách nào giảm nhẹ bớt không nhưng không được làm thay đổi thiết kế và hiệu ứng.”
- **Cách đo (`AgentScripts/ProbeWaterCost.cs`):** Play Mode, vSync tắt, người chơi giữa cầu dài; các biến thể chạy **xen kẽ 4 vòng × 60 khung** (bỏ 15 khung đầu mỗi lần), lấy trung vị; ghi `Captures/river-water-cost.json`, ms/khung và chênh với hiện tại.
  - Đo kiểu một lượt liên tiếp (21.54 ban đầu) bị trôi 5–18 ms giữa đầu và cuối lượt, nên chỉ dùng số xen kẽ.
  - Game view 1440×900 trong Editor. Số tuyệt đối gồm cả chi phí Editor, chỉ dùng để so tương đối.
- **Trước khi tối ưu** (`Captures/river-water-cost-before.json`): hiện tại 50,7 ms. Bỏ từng phần thì giảm:

  | Phần bỏ đi | Giảm (ms) |
  | --- | --- |
  | Mặt cầu + mặt nước | −10,5 |
  | Ẩn căn hộ | −7,6 |
  | Shader mặt nước thành màu phẳng | −6,3 |
  | Mặt cầu/nước không đổ bóng | −5,1 |
  | Căn hộ không đổ bóng | −5,1 |
  | Tắt gương phản chiếu | −4,1 |
  | Shader nước đục thành màu phẳng | −3,9 |

- **Tối ưu (không đổi hình):**
  1. **Căn hộ tách LOD từng khối (Blender MCP, `pho_viet_river_bridge.py`):**
     - `riverside_specs()`, `apartment_parts()` và `export_parts()` xuất `Assets/PhoViet/Art/River/PV_RiverApartments.fbx`: 24 phần `PV_RiverApt00…23`, mỗi phần là empty + `_LOD0/1/2`, đỉnh ở hệ khối phố, slot Atlas/StreetGlass/RiverRoomLight.
     - `PV_RiverBridge` không còn căn hộ: LOD 52.736 / 13.782 / 3.654. Vật cản căn hộ vẫn ở JSON cầu.
     - Trước đây cả khối cầu 190 m là **một LODGroup**: khi đứng gần cầu, mọi tòa (kể cả tòa cách 90 m hay sau lưng) vẽ LOD0, tức 108.682 tam giác căn hộ.
     - `PhoVietRiverUpgrade` đặt “Căn hộ ven sông • Blender” dưới cầu, mỗi khối một LODGroup ngưỡng `ApartmentLods` .3 / .08 / .006 (khối ~25 m: LOD0 trong ~80 m).
     - Xóa LODGroup gốc mà trình nhập FBX tự thêm (bao cả 24 khối).
  2. **Bóng đổ bằng bản thô:** `SetupLods(..., proxyLevel)` thêm “Bóng • …” là bản sao `ShadowsOnly` của LOD2 (căn hộ) / LOD1 (mặt cầu–nước), gắn vào các mức LOD chi tiết hơn; các LOD chi tiết tắt đổ bóng.
     - Khối bao giống nhau nên bóng trên đường, vỉa hè và mặt nước (bóng giải tích của shader nước không đổi) trông như cũ, nhưng mỗi tầng shadow map vẽ ít tam giác hơn nhiều.
  3. **Nhiễu nướng sẵn cho gợn nước:** `Assets/PhoViet/Scripts/RiverNoise.cs` tạo lúc chạy texture 256×256 lặp liền mạch (lưới 64 ô, 4 texel/ô): R = nhiễu giá trị nội suy smoothstep như hàm `Noise` cũ, G/B = đạo hàm.
     - `RiverWaterMirror.shader` đọc `_RiverNoise` thay cho 4 phép băm + nội suy mỗi lần gọi `Noise`.
     - Gợn lấy độ dốc trực tiếp từ đạo hàm (`RippleSlope`) thay cho 3 lần tính `Ripple` để sai phân: từ 27 phép băm/điểm ảnh còn 3 lần đọc texture.
     - Màu loang thân nước và đường nước vỗ cũng đọc texture.
  4. **Lớp nước đục chỉ vẽ khi có vật dưới nước:** `RiverMurk.shader` bỏ đỉnh ra ngoài khung cắt khi `_RiverMurkOn` = 0.
     - `PhoVietRiver.UpdateRiverMurk()` bật khi người chơi rơi sông hoặc có xe đang rơi/chìm; gọi mỗi Update, khi bắt đầu rơi và trong `TickRiverAftermath`.
     - Lúc không có gì dưới nước, 4 lớp nằm sau mặt nước đục ≥ 93% nên đóng góp < 1% màu; khi có vật chìm, hiệu ứng mờ dần theo độ sâu giữ nguyên.
  5. **Ảnh phản chiếu dùng LOD1+** (`RiverReflection.maxLod` = 1): ảnh gương 42% độ phân giải đã làm nhòe, nên LOD0 không thêm chi tiết thấy được.
  - Không đổi màu, vật liệu, thông số shader, thiết kế nhà hay cầu.
- **Sau khi tối ưu** (`river-water-cost.json`, cùng góc):

  | Phần bỏ đi | Trước (ms) | Sau (ms) |
  | --- | --- | --- |
  | Mặt cầu + nước | 10,5 | 4,3 |
  | Shader mặt nước | 6,3 | 1,7 |
  | Nước đục | 3,9 | 0,3 |
  | Bóng mặt cầu | 5,1 | 2,1 |
  | Bóng căn hộ | 5,1 | 2,0 |
  | Gương | 4,1 | 2,1 |
  | Căn hộ | 7,6 | 6,6 |

  - **Ẩn cả khối cầu chỉ còn chênh +0,6 ms** (gần như không tốn so với phố thường), so với ~10–12 ms trước đó.
  - Số tuyệt đối của lượt sau (53,2) cao hơn lượt trước (50,7) do máy bận khác nhau giữa hai lượt; chênh tương đối mới là số đáng tin.
- **Hình ảnh:** `Captures/river-perf-sheet.png`, trước (`Captures/perf-before/`) và sau cùng 5 góc.
  - Chênh điểm ảnh trung bình 0,8–8,7/255; phần lớn do mây và gợn sóng chạy theo thời gian (ảnh chụp ở thời điểm khác nhau).
  - Gợn sóng nhỏ trông mịn hơn một chút, vì đạo hàm từ texture 4 texel/ô thay cho sai phân bước 0,11 m. Hình mặt tiền, phản chiếu và bóng giống nhau.
- **Kiểm tra (`VerifyRiverBridge` 160 điều kiện):**
  - 24 LODGroup căn hộ, 72 renderer + 24 proxy bóng (LOD2 ShadowsOnly, LOD chi tiết không đổ bóng);
  - proxy bóng LOD1 của cầu;
  - texture `_RiverNoise` 256 lặp; gương LOD1+;
  - nước đục bật khi người chơi chìm và tắt khi không có gì dưới nước;
  - cùng các điều kiện cũ (kính, đèn phòng, phản chiếu chỉ khi có sông).
  - Hồi quy đầy đủ (Play Mode): **28 suite / 9.791 điều kiện đạt**. Gồm VerifyRiverBridge 160, TankBarrage 72, CrashPoliceNavigation 214, RoofRiding 4.206, StreetArt 333, NightJumpLighting 88, PortraitHud 21, MobileCamera 8. Sau khi chạy, Editor ở Edit Mode, scene không bẩn.
- **Chưa làm:** chưa đo trên điện thoại/WebGL thật.
- **Trạng thái:** scene đã lưu (jobs khóa). **Chưa commit/build/push**; web vẫn 1.38.0.

### 21.55. Đổi làn mượt như cua giao lộ (25/10/2026, Claude Code — source, chưa build)

- **Yêu cầu:** nhấn trái/phải (A/D, ←/→) hoặc vuốt trái/phải trên mobile, nhân vật đổi làn chưa mượt như khi rẽ ở giao lộ; điều chỉnh hiệu ứng di chuyển cho thật mượt.
- **Nguyên nhân (đo bằng mô phỏng và trong Unity, dt 1/60, tốc độ 17):**
  - 21.29/21.31 đẩy xe ngang với gia tốc **bật ngay 150 m/s²**, rồi hãm đều 60 m/s², rồi dừng. Gia tốc ngang nhảy bậc 3 lần: đầu, giữa và lúc chạm làn.
  - Độ giật tối đa **14.085 m/s³**, gia tốc tối đa 249 m/s².
  - Góc đầu xe đi qua bộ lọc bậc một hệ số 18/s nên tốc độ quay nhảy ngay **279°/s** ở khung đầu tiên.
  - Cua giao lộ (`PhoVietRoutes.BeginOrContinueTurn`) thì đi theo đường cong bậc 5, độ cong bằng 0 ở chỗ nối, nên không có bước nhảy.
- **Sửa (`PhoVietGame.cs`):**
  - **Đường cong đổi làn bậc 5** (`AdvanceLaneCurve`, `LaneCurveValue`):
    - Từ trạng thái hiện tại (vị trí, vận tốc, gia tốc ngang) tới tâm làn đích với vận tốc = gia tốc = 0, theo dạng minimum-jerk.
    - Hệ số: c1 = v·T, c2 = a·T²/2, A = D−c1−c2, B = −c1−2c2, C = −2c2; c3 = 10A−4B+C/2, c4 = −15A+7B−C, c5 = 6A−3B+C/2.
    - Thời gian `T = 0,36·√(|D|/3,1)`, kẹp 0,18–0,62 s: một làn 0,36 s, hai làn ~0,51 s.
    - Đang lao về phía đích thì `T ≤ 1,6·|D|/|v|` để không vượt quá làn.
    - Đổi làn đích giữa chừng (bấm liên tiếp, đổi ý) lập đường cong mới từ trạng thái hiện tại, nên vị trí, vận tốc và gia tốc vẫn liên tục.
    - Nếu vị trí bị dời khỏi đường cong quá 2 cm (va chạm, đạp áp sát, quay về làn sau cua) cũng lập lại.
  - **Góc đầu xe** = tiếp tuyến đường đi `atan2(v_ngang, max(Speed, 8))`, bão hòa mềm `26·tanh(góc/26)` thay cho cắt cứng ±26°.
    - Làm mượt bậc hai `SmoothDamp`: bẻ vào 0,09 s, trả lái 0,24 s. Vẫn lấy lại thăng bằng từ từ như 21.31.
  - Độ nghiêng = −0,85 × góc đầu xe (±24°), `SmoothDamp` 0,12 s.
  - `ClearLaneMotion()` đặt lại trạng thái ở `ResetRide`, khi té (`PhoVietRiderCrash`), rơi sông (`PhoVietRiver`) và sau cua (`PhoVietRoutes`).
  - Bỏ `LaneSpeed/LaneAccel/LaneBrake/SteerReturn`.
  - Hộp va chạm vẫn đi theo vị trí thật, không xoay; góc lái chỉ xoay phần hình như 21.29.
- **Kết quả (trong Unity, dt 1/60, khớp mô phỏng):**

  | | Cũ | Mới |
  | --- | --- | --- |
  | Độ giật ngang (m/s³) | 14.085 | **2.800** (~5 lần êm hơn) |
  | Gia tốc ngang tối đa (m/s²) | 249 | **137** |
  | Tốc độ quay đầu xe (°/s) | 279 | **155** (124 ở tốc độ 30) |
  | Gia tốc quay đầu xe (°/s²) | 3.747 | **2.417** |
  | 90% làn (s) | 0,27 | **0,28** (vẫn né kịp) |
  | Vào hẳn tâm làn (s) | 0,32 | **0,35** |
  | Đỉnh góc đầu xe (°) | 25,6 | **22,6** (18,8 ở tốc độ 30) |
  | Từ đỉnh tới thẳng (s) | 0,63 | **0,50** |

  - Biểu đồ so sánh: `Captures/lane-smooth-curves.png`.
  - Mô phỏng chọn thông số: `lanesim.py`/`lanesim2.py` trong thư mục tạm của phiên, không lưu repo.
- **Kiểm tra:**
  - `VerifyLaneSteering` **131 điều kiện** (thêm 8): ở dt 1/60, gia tốc ngang ≤ 160 m/s² và độ giật ≤ 4.000 m/s³; đầu xe quay ≤ 190°/s và ≤ 3.000°/s². Bản cũ trượt cả hai.
  - Các điều kiện cũ vẫn đạt: 90% làn ≤ 0,36 s, không vượt, trả lái ≥ 0,3 s và ≤ 125°/s, nghiêng đúng phía, dựng thẳng, đổi ý không giật. Bước tối đa mỗi khung khi đổi ý nới 0,26 → 0,34 m vì đỉnh vận tốc ngang ~16 m/s.
  - `VerifyGameplay` 23 đạt (đổi 2/3 làn trong thời hạn cũ).
  - Hồi quy đầy đủ (Play Mode): **28 suite / 9.815 điều kiện đạt**. Gồm LaneSteering 131, Gameplay 23, SmoothTurns 468, QuickKick 62, JunctionKick 216, RiderContacts 45, CrashOutcome 51, RoofRiding 4.206, RiverBridge 160, TankBarrage 69, CrashPoliceNavigation 214, PortraitHud 21, MobileCamera 8. Sau khi chạy, Editor ở Edit Mode, scene không bẩn.
- **Ghi chú môi trường:** trong phiên này, ghi đè `Captures/river-bridge-*-results.json` và `saigon-street-*-results.json` bị “Permission denied” lúc chạy. Kết quả hồi quy lần này ghi vào thư mục tạm của phiên, không phải repo.
- **Trạng thái:** chỉ đổi mã (`PhoVietGame.cs`, `PhoVietRiderCrash.cs`, `PhoVietRiver.cs`, `PhoVietRoutes.cs`) và test; scene không đổi. **Chưa commit/build/push**; web vẫn 1.38.0.

### 21.56. File bàn giao nhanh `BANGIAO.md` để Codex/Claude Code nhận việc không cần prompt lại (26/10/2026, Claude Code — chỉ tài liệu)

- **Yêu cầu:** sau mỗi lần Claude Code làm/phát hành chức năng, người dùng chỉ cần qua Codex gõ yêu cầu mới; Codex phải tự biết trạng thái mới nhất, không cần dán lại prompt “đọc toàn bộ code và huongdan.md” (lần trước Codex mất ~37 phút đọc lại tài liệu ≈385 KB).
- **Cách làm:**
  - Thêm `BANGIAO.md` ở gốc: trạng thái/bản phát hành hiện hành, việc vừa làm, việc đang dở, luật đã chốt, bảng “sửa gì → đọc mục nào/file nào”, quy trình mỗi yêu cầu. Giữ ngắn (≈100 dòng).
  - `AGENTS.md` (Codex tự đọc khi mở dự án): đầu mỗi phiên đọc `BANGIAO.md` + `git status --short` + `git log --oneline -5`; không đọc lại toàn bộ `huongdan.md`/mã nguồn, chỉ mở mục/file liên quan; nếu Git có commit mới hơn bản ghi trong `BANGIAO.md` thì đọc thêm mục 21.x mới nhất. Thêm quy tắc bắt buộc cập nhật `BANGIAO.md` cùng `huongdan.md` mỗi lần sửa/thêm chức năng/build/phát hành.
  - Thêm `CLAUDE.md` chỉ chứa `@AGENTS.md` để Claude Code nạp cùng bộ quy tắc.
- **Thay thế:** prompt mẫu cũ ở 18.3 (ghi 1.34.0) đã lỗi thời; 18.1 và 18.3 nay trỏ về `BANGIAO.md`.
- **Kiểm tra:** chỉ tài liệu; không cần build. **Đã commit riêng 4 file** (`AGENTS.md`, `CLAUDE.md`, `BANGIAO.md`, `huongdan.md`) và push `main`; mã/scene/asset của 21.54–21.55 vẫn chưa commit, web vẫn 1.38.0.

### 21.57. Xe người chơi Cù Lao Petshop Truck và húc ngang (26/10/2026, Codex — chưa phát hành, đã thay thế ở21.58)

- **Yêu cầu/chốt:** đã nhận xe Meshy `Cù Lao Petshop Truck_20261026_012447`; thay toàn bộ người áo đỏ + xe máy **của người chơi** bằng đúng bán tải xanh, giữ màu/chữ gốc. Người dùng chọn giữ nhảy/tăng tốc và đổi đạp thành áp sát rồi húc ngang. Không thay NPC hoặc CSGT; jobs tiếp tục khóa.
- **Nguồn/asset:** FBX nhận `Meshy_AI_Cù_Lao_Petshop_Truck_1025182443_texture.fbx` (GUID `8109923099eec0c46a30971f31fbbf6d`), 14.690 tam giác. `ArtSource/Blender/integrate_petshop_player.py` chỉ dựng xe mới bằng thư viện tích hợp Meshy, lưu riêng `PhoViet_PetshopPlayer.blend`, xuất `Assets/PhoViet/Art/PlayerCar/PV_PetshopPlayer.*`. Không sửa source Meshy. Kích thước rộng/cao/dài **1,889312 × 1,949565 × 3,8 m**, 15 xương semantic, 3 LOD **14.690 / 6.462 / 2.349**, 7 clip Idle/Drive/Brake/ImpactFront/Rear/Left/Right; bánh quay phần mâm trong, không quay cả vè/lốp AI dính liền (tránh chớp đuôi cũ). Texture PBR 2K giữ nguyên, mip/trilinear/aniso4, Uncompressed; `prepare_petshop_lamps.py` tạo mask emission theo UV/đúng vị trí đèn, giữ màu/chữ sơn.
- **Cài vào scene:** `Editor/PhoVietPlayerCarUpgrade.cs`, gọi bằng `AgentScripts/InstallPetshopPlayer.cs`. Trước khi sửa lưu scene đang bẩn vào `.tools/PhoViet-before-petshop-player-memory.unity` (SaveScene asCopy), bảo toàn phần Claude 21.54–21.55 và import vừa nhận. Chỉ xóa visual con/`BikeVisualAnimator` cũ dưới `game.rider`, giữ outer root/vòng chơi; thêm `PlayerCarVisual`, bỏ hai chân Q cũ, chuyển preview đúng FBX Petshop sang EditorOnly/inactive. So sánh cấu hình 28 actor giao thông và CSGT trước/sau: giữ nguyên. Installer chạy lại chỉ bảo đảm cặp đèn, không thay NPC.
- **Visual xe:** `PlayerCarVisual.cs`: nhún thân từ clip Blender, bốn mâm quay theo tốc độ, hai bánh trước đánh lái; góc đầu xe theo `RiderSteerYaw`. Nhảy giữ windup 0,24 s/phóng 14,6/gravity22/cooldown2; nghiêng thân nhẹ theo nhịp cất cánh, vận tốc bay, đáp trước/sau/cả hai và damping 0,085 s; không dùng chân/người cũ. Nghiêng đổi làn chỉ 0,18×yaw, tối đa5°; cua nghiêng5°. Không nâng hình trở lại mặt đường khi rơi sông. Giữ camera/FOV dọc hiện có.
- **Va chạm/nhảy:** `PhoVietPlayerVehicle.cs` cấp kích thước player dùng chung (fallback xe máy cũ); `ResetRiderImpact` dùng halfWidth **0,944656**, halfLength **1,9**, khối lượng theo nhóm ô tô thay vì cố định220 kg; `PhoVietSceneryContacts` xét đúng kích thước/cao xe. Khi cua, chia phép quét theo góc quay <=3° để không dùng góc cuối frame cho cả đoạn đi. `PhoVietJump` lấy trục trước/sau1,2105/1,1643 m, vùng ngang theo bề rộng ô tô, giữ support đến khi thân sau rời hẳn mép nóc; tránh báo đâm vào mép khi rời nóc xe nhanh hơn. Nhảy quá muộn/thiếu cao vẫn va chạm thật, không thêm miễn va chạm.
- **Q/double tap = húc:** giữ API `TryKick`/`KickCount` cho input/mobile/tự vệ cũ. `PhoVietKickAssist` chọn một xe máy, tăng tốc dự đoán tương đối như cũ; gap bằng nửa rộng xe người chơi + hình chiếu thân mục tiêu +0,10 m. Chờ mục tiêu ngang cạnh xe rồi dịch vào0,15 m. `TryResolvePlayerRam` trong phép quét SAT xác nhận tiếp xúc thật, đúng bên, Swing/mục tiêu/spawnGeneration; không cần giày/clip chân, không hất từ xa. Chấp nhận tiếp xúc xiên ở giao lộ, loại đầu/đuôi xe. Húc thành công hất mục tiêu ra đúng bên rồi trở về làn gốc; ô tô/xe tải/cảnh vật chắn đường thì hủy. Đổi làn/nhảy/W có thể ngắt. Húc quái xế tự vệ không gọi mới CSGT; húc xe thường bị truy đuổi. Các va chạm khác giữ luật té/kết thúc/chờ bắt hiện tại.
- **Đèn/khói/HUD:** gắn head/halo trước-sau lên bumper tương ứng của rig, hai đèn trước + hai đèn sau, emission theo ngày–đêm; quầng sáng vẫn dùng pool đang có. Socket khói `(0,68; 0,36; -1,85)` trên Frame; `BoostSmokeVisual` ưu tiên socket xe mới. W vẫn2,25× trong4 s, hồi2 s, không nhân tốc độ NPC. HUD/màn đầu/buồng giam dùng HÚC thay ĐẠP khi player là ô tô.
- **Tinh chỉnh sau thử:** bước mô phỏng khi ô tô đang cua/húc tối đa **5 ms** (fallback xe máy vẫn10 ms khi đạp). Khi tách khỏi xe vừa húc, cho phép chuyển động tách đơn điệu và bỏ lần chạm đầu bước chỉ khi cuối bước đã hết chồng lấn, đúng mục tiêu/spawnGeneration; không miễn các va chạm mới. Recoil10 cm theo pháp tuyến tiếp xúc thay vì chỉ đẩy ngang, tránh mắc ở tiếp xúc xiên. Xe hơi bắt đầu cua đã chọn ở **9 m** trước tâm giao lộ (xe máy cũ11,65 m) để góc sau không quệt chậu cây ở làn sát lề; cua muộn dùng tay nắm đầu0,5 m. `LastRamContact` chỉ là chẩn đoán tiếp xúc, không hiện lên HUD.
- **Kiểm tra hoàn tất trong Unity:** **5 suite / 1.044 điều kiện đạt**: `VerifyPetshopPlayer`64 (model/kích thước, 12 trường hợp húc ở dt16/33/50 ms, hủy khi bị chắn, nhảy/boost/khói/đèn, va chạm thường kết thúc, CSGT đến bắt và vào tù); `VerifyBoostTrafficIsolation`478; `VerifyJunctionKick`216; `VerifyMeshyCarRoofs`254/64 tình huống (62 đáp/chạy/rời nóc; 2 trường hợp container từ sau tới quá sớm khi xe mới cao<0,7 m phải té thật); `VerifyPetshopTurns`32 (4 làn × boost/thường ×60/20 FPS, giữ tốc độ và làn sau cua, không chạm chậu cây khi chọn rẽ trước). Compile không lỗi/cảnh báo. `git diff --check` đạt. Báo cáo `Captures/petshop-player-checks.json`, `petshop-turn-checks.json`, `petshop-*-regression.json`; ảnh `petshop-player-{front,rear,game-landscape,game-portrait,night-jump}.png` đã xem. Ảnh dọc là preview RenderTexture dùng cùng FOV/camera dọc; chưa phải kiểm thử trên điện thoại thật.
- **Giới hạn kiểm tra cần giữ rõ:** suite `VerifySmoothTurns` cũ không đạt trường hợp ép cua cực muộn tại khoảng -6,5 m sau tâm giao lộ khi W67,5 m/s: ô tô rộng/dài hơn thực sự chạm chậu cây lớn. Không bỏ luật va chạm cảnh vật hoặc thêm miễn va chạm để ép trường hợp này đạt. Bằng chứng quay xe mới dùng `VerifyPetshopTurns` ở điểm chọn rẽ bình thường; chưa tuyên bố toàn bộ suite xe máy cũ đạt trên player ô tô. Chưa đo FPS/độ nét trên điện thoại hoặc build WebGL mới.
- **Sự cố xuất asset đã xử lý:** lần chạy đầu gọi nhầm main của thư viện cũ, chạm hai FBX NPC Sport/Pickup; đã khôi phục đúng hai file từ HEAD (hai file sạch trước lần chạy), xác nhận `git status` thư mục Cars/Meshy sạch. Script mới chặn main thư viện, chỉ xuất Petshop. Giữ nguyên các `.blend`/FBX xe máy và thay đổi Claude vốn có trong working tree.
- **Trạng thái:** source/scene đã cài; **chưa build WebGL/commit/push/phát hành**, web và cache vẫn1.38.0 (`bec6ce1`, `1ffc9c93f6c6d874`). Bản local đồng thời chứa21.54–21.55 của Claude, không được reset hoặc bỏ khi build sau. Chưa thử trên điện thoại thật/bundle WebGL mới.

### 21.58. Khôi phục người áo đỏ sao vàng và xe máy xanh dương–đen (26/10/2026, Codex — scene/asset, chưa phát hành)

- **Yêu cầu mới thay thế21.57:** người chơi quay lại đúng Golden Star Stance áo đỏ sao vàng cùng xe máy cũ, xe đổi phối xanh dương–đen. Không dùng xe hơi/húc ngang cho player nữa. Q/chạm hai lần trở lại áp sát rồi đạp; Space/vuốt lên nhảy, W/vuốt xuống +125%/4 s/hồi2 s giữ nguyên.
- **Khôi phục có chọn lọc:** `AgentScripts/RestoreBlueBlackPlayer.cs` lấy riêng hierarchy người chơi từ bản sao ngay trước Petshop `.tools/PhoViet-before-petshop-player-memory.unity`; mở additive tạm, nhân bản riêng player vào scene hiện tại, nối lại `rider`, hai chân, animator/clip/LOD và các nguồn đèn, rồi đóng/xóa scene tạm. Không khôi phục toàn bộ scene hoặc reset working tree. Trước thao tác lưu `.tools/PhoViet-before-restore-blue-bike-memory.unity`; giữ nguyên thay đổi Claude21.54–21.55. So sánh cấu hình28 NPC cùng CSGT trước/sau đều nguyên trạng. Mô hình xe hơi không còn trên player; asset/code Petshop được giữ lưu trữ, không tự chạy lại installer.
- **Art/màu:** `ArtSource/Blender/prepare_hero_blue_black.py` đọc atlas hero đang dùng, lấy mask sơn đỏ gốc từ xe Meshy, tô xanh RGB tỷ lệ `(0.035,0.30,0.90)` theo độ sáng nguồn; giữ nguyên phần đen, cơ khí, người và sao ngực/lưng. Xuất riêng `Art/Bikes/MeshyRiders/PV_HeroScooter_BlueBlack_Palette.png` **2048×4096** và material `PV_HeroScooter_BlueBlack.mat`; giữ texture/material cũ cho NPC và bản lưu. Nửa atlas người so sánh pixel bằng tuyệt đối; pixel ngoài mask sơn xe cũng không đổi. Sao/UV/tư thế tay/ngón hiện có giữ nguyên. Import sao chép bằng `ReadTextureSettings/SetTextureSettings` và chép riêng maxTextureSize4096, CompressedHQ, compressionQuality cùng cấu hình WebGL/Standalone/iPhone/Android; đã xác minh texture nạp thực sự **2048×4096**. Không dùng EditorJsonUtility để chép importer vì cách đó đã để ảnh mới ở mặc định1024×2048 trong lần kiểm đầu (đã sửa). Normal/surface/emission dùng chung như cũ.
- **Gameplay/binding:** `BikeVisualAnimator` rolePlayer, assetPV_HeroScooter, 3 LOD; halfWidth0,33/halfLength0,91, khối lượng/đường cua/nhảy trở lại nhánh xe máy (`IsPlayerCar=false`). Đèn trước/hậu trở lại hai nguồn đúng rig xe máy; bỏ hai quầng sáng bổ sung riêng cho Petshop. Khói W tự dùng lại socketPV_MeshyExhaust. Các nhánh ô tô trong runtime không hoạt động; luật té/chờ CSGT/rơi sông, dữ liệu Coin/kỷ lục, cảnh phố và jobs khóa giữ nguyên.
- **Kiểm tra lần này:** **4 suite / 229 điều kiện Unity đạt**: `VerifyQuickKick`62, `VerifyJump`44, `VerifyBoost`35, `VerifyNightJumpLighting`88. Texture kiểm tra1.223.071 pixel sơn, người/sao và chi tiết đen không đổi. `CaptureBlueBlackHero` xác nhận player không còn ô tô, animator chân/chạm giày hợp lệ, 3skin cùng material mới, 2nguồn halo, jobs khóa; đã xem ảnh trước/sau `Captures/hero-blue-black-{front,rear}.png`. Kết quả mới lưu `hero-blue-black-*-checks.json`; không ghi đè kết quả phát hành trước. Chưa thử điện thoại thật/WebGL mới.
- **Trạng thái bàn giao:** scene đã lưu, Editor trở về Edit Mode; **chưa build/commit/push/phát hành** theo phạm vi yêu cầu lần này. Web vẫn1.38.0/cache1ffc9c93f6c6d874 và còn xe trắng–đen. Source hiện tại21.58 thay thế lựa chọn playerPetshop/húc ở21.57; không xem các test/ghi chú chỉ dành cho ô tô là trạng thái gameplay hiện hành.

### 21.59. Ghi tiến độ giữa chừng để chuyển tài khoản Claude không mất ngữ cảnh (26/10/2026, Claude Code — chỉ tài liệu)

- **Yêu cầu:** người dùng có hai tài khoản Claude Pro trên cùng máy/cùng thư mục; khi tài khoản 1 hết lượt dùng thì đăng nhập tài khoản 2 làm tiếp mà không phải viết lại prompt.
- **Cơ sở:** `CLAUDE.md`/`AGENTS.md`/`BANGIAO.md`/`huongdan.md` nằm trong repo nên tài khoản nào cũng đọc. Bộ nhớ tự động của Claude Code và lịch sử hội thoại nằm trong `C:\Users\HP\.claude\` của máy, không gắn với tài khoản đăng nhập.
- **Lỗ hổng đã vá:** trước đây `BANGIAO.md` chỉ được cập nhật khi xong việc, nên nếu hết lượt giữa chừng thì phiên sau không biết việc dở. Nay `AGENTS.md` bắt buộc ghi “Tiến độ việc đang làm” ở mục 3 của `BANGIAO.md` sau mỗi mốc của việc dài (yêu cầu, bước đã xong, bước kế tiếp, file đang sửa, trạng thái Unity/Blender) và xóa khi xong; mục 6 của `BANGIAO.md` có thêm bước này.
- **Kiểm tra:** chỉ tài liệu; không cần build. Chưa commit (các file tài liệu đang chứa cả ghi chú 21.57–21.58 chưa commit của Codex).

### 21.60. Đổi làn chậm, dựng xe từ từ và nhảy theo tốc độ (26/10/2026, Codex — source, chưa build)

- **Yêu cầu/chốt:** chuyển làn trái/phải còn quá nhanh và khựng do dựng xe quá sớm; làm chuyển hướng từ từ, giữ nhịp đi theo hướng đó rồi hồi phục cân bằng chậm. Nhảy ở tốc độ đầu giảm25%; càng nhanh càng cao, gồm tốc độ W. Người dùng đã chọn đường tăng đều với mốc17 m/s~3,63 m,30 m/s~4,84 m và W~138 km/h~5,62 m. **Chốt độ cao theo tốc độ lúc cất cánh**; bấm W khi đã bay chỉ làm cú nhảy kế tiếp cao hơn.
- **Đổi làn — PhoVietGame.cs:** giữ đường cong bậc5 C² và tái lập từ vị trí/vận tốc/gia tốc hiện tại; LaneTime0,36→**0,72 s**, biên thời gian0,32–1,20 s theo căn quãng đường. SteerIn0,09→**0,16**, SteerOut0,24→**0,45**. Nghiêng vào LeanIn0,20; giữ hướng nghiêng LeanHold0,18 khi bắt đầu trả lái, rồi LeanOut0,45 thay bộ lọc nghiêng0,12 s. Đổi hướng mới hủy giữ nghiêng; ClearLaneMotion xóa trạng thái giữ để không lẫn sau té/cua/reset. Không thay input/ngưỡng vuốt, tốc độ tiến, kích thước va chạm hoặc thao tác rẽ.
- **Kết quả thực (17 m/s,60 FPS):**90% quãng làn0,55 s; gần tâm0,683 s; đỉnh yaw17,87° tại0,567 s; yaw<2° tại1,45 s. Khi tới làn còn nghiêng12,81°, tới~2,0 s mới còn<2° — không dựng thẳng ở khoảnh khắc chạm làn. Gia tốc ngang tối đa34,47 m/s², jerk407,69 m/s³, tốc độ trả lái tối đa28,71°/s. So với21.55, giảm mạnh chuyển động ngang/độ giật và kéo dài phục hồi. Xem chuỗi ảnh `lane-steering-seq-0..6.png` ở0,2/0,4/0,7/1,0/1,3/1,7/2,2 s và trace `lane-gradual-checks.json`.
- **Nhảy — PhoVietJump.cs:** `JumpHeightAtSpeed(v)=4,844545×(0,75+max(0,v−17)/52)`; tăng vận tốc phóng theo căn bậc hai độ cao, không nhân thẳng vận tốc Y thêm25%. Mốc17→3,6334 m;30→4,8445 m;38,25→5,6132 m;67,5→8,3382 m. Đỉnh tính tương đối mặt đỡ, gravity22/cooldown2/windup0,24 giữ nguyên. Runtime khởi đầu vẫn tăng tốc nền nhẹ trong windup nên thực đo cất cánh17,09 m/s đạt3,6418 m, đúng cùng công thức. Pose dùng `jumpLaunchVelocity` riêng, không hằng14,6 cho mọi cú.
- **Thứ tự cập nhật:** TickBoost và tính Speed (cruise, W, giới hạn hiện có) trước TickJump; khi cất cánh lưu tốc độ/đỉnh/vận tốc Y, sau đó mới áp dụng bonus bay+3 m/s cho di chuyển. Frame hết windup chỉ tích phân phần thời gian bay thật còn lại, tránh thêm trọn dt làm sai quỹ đạo giữa20/30/60 FPS. Tăng tốc khi đang bay không thay dữ liệu phóng; lần nhảy sau đọc tốc độ mới. Không thêm miễn va chạm hoặc cho nhảy lần hai trên không. Nóc tải cao vẫn cần đủ tốc độ/độ cao, nhảy thiếu thì té thật.
- **Kiểm tra đã chạy: 6 suite / 541 điều kiện Unity đạt**, compile không lỗi/cảnh báo: `VerifyLaneSteering`155 (slower timing, giới hạn giật, hướng đúng, tới làn còn bank và hồi phục tiếp>=0,55 s, đảo hướng liên tục, dt1/60–1/20); **mới** `VerifySpeedJump`157 (18 tổ hợp3 tốc độ×W/tắt×3dt, đỉnh thực so với công thức sai<1,5 cm, monotonic pose, W trong windup/W giữa không trung/cú sau); `VerifyJump`44 (cập nhật mốc thấp3,60–3,70 m), `VerifyBoost`35, `VerifyQuickKick`62, `VerifyNightJumpLighting`88. Fixture đèn nhảy cao đặt tốc độ30 m/s thay giả định cú đầu luôn cao4,84 m; các kiểm tra đèn bám vị trí/góc thật giữ nguyên.
- **Bằng chứng:** `Captures/lane-gradual-checks.json` (trace x/yaw/roll và số đo), `speed-jump-checks.json`, `gradual-speed-{lane,jump,boost,kick,night-lighting}-checks.json`. Đã xem ảnh ở0,7 và1,3 s: xe vẫn nghiêng lúc vừa tới làn và dựng dần. Không thay model/texture hay cảnh phố, hero xanh–đen21.58 và thay đổi Claude21.54–21.55/ghi chú21.59 vẫn giữ.
- **Trạng thái:** mã/test/tài liệu đã lưu; Editor về Edit Mode. **Chưa build WebGL/commit/push/phát hành**; web vẫn1.38.0/cache1ffc9c93f6c6d874. Chưa đo cảm giác thao tác/FPS trên điện thoại thật hoặc bundle WebGL mới. `BANGIAO.md` đã cập nhật cùng lúc, giữ phần quy tắc tiến độ Claude vừa thêm ở21.59.

### 21.61. Build/phát hành WebGL 1.39.0 (26/10/2026, Codex — đã phát hành và kiểm tra live)

- **Ủy quyền:** người dùng yêu cầu trực tiếp “Hãy build và phát hành web”; bao gồm commit/push main và Pages theo15.2, không cần hỏi lại.
- **Nội dung:** đưa21.54–21.55 và21.58–21.60 lên web: tối ưu sông/cầu của Claude, hero áo đỏ sao vàng/xe máy xanh dương–đen, đổi làn0,72s với dựng xe chậm, độ cao nhảy theo tốc độ lúc cất cánh (17m/s~3,63m;30m/s~4,84m; W tính tại cất cánh). Jobs vẫn khóafalse. Player ô tô/húc21.57 ngưng dùng; giữ các lớp C# hỗ trợ vì mã hiện tại còn tham chiếu, không chạy installer hay thay hero. Không đưa lịch sử Wave hay các .blend khác đang sửa cục bộ vào commit.
- **Version:** đồng bộ `PhoVietWebBuild.cs`, `ConfigureWebRelease.cs`, `VerifyWeb.cjs`, `VerifyFullscreenGame.cjs`, `VerifyWebBundle.py`, README và PlayerSettings.bundleVersion=1.39.0. Scene Edit Mode đã lưu, không missing script, rider là xe máy, jobsEnabled/RideJobsReleased=false; tất cả scene dependency có trên đĩa. Raw Petshop root vẫn EditorOnly, không vào Player.
- **Build:** Unity build `build_95d39d4c9def` Succeeded,340716ms,0lỗi/3warning (Pipeline không có RuntimePipelineConfig dành cho Player,2CS0414 biến `dragging`/`hudTouchChoice` chưa dùng). Không chỉnh gameplay để dọn warning. `Captures/release139-build-report.json`. Fingerprint source trước/sau build không đổi.
- **Artifact:** copy output nguyên vẹn vào docs; chỉ dọn các hash cũ đã xác minh trong docs/Build. Cache **770899e40e23f8e2**, tổng tài nguyên102562520byte; data95262789byte <100MB. **29 kiểm tra bundle đạt**: chữ ký WASM/data gzip thật, cache SHA đúng, offline list đủ/không trùng, đúng4file build, không quá giới hạn GitHub. `Captures/release-1.39-bundle-checks.json`.
- **Kiểm tra gameplay trước build:**6suite/541 điều kiện Unity tại21.60 đã đạt;4suite/229 điều kiện phục hồi xe xanh–đen tại21.58 đã đạt.
- **Web local đạt:** VerifyWeb37 kiểm tra (desktop/start/pause/phím, cảm ứng, xoay, PWA installability, toàn bộ cache và chạy WASM khi offline); VerifyFullscreenGame20 kiểm tra (iPhone/iPad dọc/ngang và safe area),0lỗi JavaScript/Unity runtime. Đã xem ảnh mobile: hero đỏ sao vàng/xe xanh–đen. Bằng chứng `Captures/release139-local-results.json`, `fullscreen-game-release139-local-results.json`, ảnh `release139-phone.png`/`release139-desktop.png`.
- **Git:** commit `17aa0b2` đã push origin/main thành công; giữ nguyên các file art/Blender và lịch sử Wave cục bộ không liên quan.
- **Phát hành hoàn tất:** GitHub Pages run `40268861830` success đúng SHA `17aa0b23576a6e02e6153653e1cb4c0d0f3bae21`. `VerifyPublishedGame.cjs` **26 kiểm tra live đạt/0lỗi runtime**: version/cache đúng, mọi tài nguyên remote khớp local, WASM thật khởi tạo, Start/Pause hoạt động. Bằng chứng `Captures/release139-pages-results.json`, `release139-live-results.json`, `release139-source-fingerprint.json`. Link: https://denispham1107.github.io/taixecongnghe/.
- **Bàn giao cuối:** Unity Edit Mode, scene không dirty, người chơi vẫn xe máy, jobs=false, version1.39.0. Không rebuild cho lần ghi kết quả tài liệu; commit ghi nhận sau release không đổi docs/binary. Giữ các thay đổi cục bộ không liên quan. Chưa kiểm tra điện thoại Safari thật.

### 21.62. Khủng long Emerald Tyrant đi ngược chiều và quất đuôi (26/10/2026, Codex — đã phát hành1.40.0 ở21.66)

- **Yêu cầu/chốt:** model vừa gửi từ Meshy; người dùng chọn **xuất hiện phía trước/ngược chiều**, cứ10s; rộng1,5làn và cao theo tỷ lệ; đuôi quét đủ4làn/có lấy đà, người chơi phải nhảy đủ cao. Hất cả xe tăng/CSGT, nhà/cây không di chuyển. Áp dụng luật té/kết thúc hoặc chờ CSGT bắt hiện có, không thêm mạng/miễn va chạm.
- **Nguồn đã nhận:** `Assets/MeshyImports/Emerald Tyrant_20261026_221137/Meshy_AI_Emerald_Tyrant_20261025_texture.fbx`, texture màu/normal gốc. Unity báo11.177vertex, một MeshRenderer, **không rig/clip**, scene dirty do import mới; đã giữ phần import và chỉ gắn tag EditorOnly cho bản tham chiếu rời, không xóa model của người dùng. `Captures/dinosaur-source-data.json`/`dinosaur-source-inspection.json`.
- **Blender MCP:** mở instance mới vì Blender chưa chạy, không ghi đè các .blend cũ; dựng bằng `ArtSource/Blender/pho_viet_dinosaur.py`, lưu `PhoViet_Dinosaur.blend`. Giữ geometry/UV/màu Meshy, scale đều theo bề ngang4,65m; cao6,0811m/dài8,6758m. Rig21xương (Root/TailTip là helper không deform), da dùng bone heat mịn, fallback cho phần răng/móng rời; IK hai chân, chống trượt chân theo tốc độ3m/s.
- **Animation thật:** Walk1,4s vòng kín; TailSweep2,8s bake60FPS: lấy đà0–0,85s, quét0,85–1,55s, theo đà/dựng lại1,55–2,8s. Hạ thân1m, cúi cổ/giữ đầu tự nhiên và xoay hông rồi vung đuôi; đầu/cổ không chắn cú nhảy chuẩn ở làn giữa. Có blend vào/ra0,2s. Đuôi quét bằng mesh/xương thật, không phải đánh vô hình toàn đường. Đã xem ảnh Blender và Unity, sửa da hông/chân, pose đầu và khoảng hở nhảy trong lúc hoàn thiện.
- **Asset/installer:** `Assets/PhoViet/Art/Dinosaur/PV_EmeraldTyrant.{fbx,json,prefab}` (~1,35MB FBX), `PV_EmeraldSkin.mat`; giữ basecolor và normal map nguồn2048, Standard/Glossiness0,18/BumpScale0,65. `Editor/PhoVietDinosaurUpgrade.cs` + `AgentScripts/InstallDinosaur.cs` chỉ thêm prefab/cờ vào scene hiện tại, không chạy builder lịch sử. ModelImporter Legacy, không nén animation, không tối ưu mất xương. `DinosaurVisual.cs` sample clip theo clock gameplay, state.speed=0 chống animation tự trôi khi pause; bound skin đủ chứa cú quất để không bị culling giữa chừng.
- **Lịch/di chuyển:** `dinosaursEnabled=true`, `dinosaurPrefab` đã gắn, interval10s chơi, phía trước z=max(110,Speed×2,5+20), hướng180°/tốc độ riêng3m/s. Nếu deadline đến giữa cua, timer vẫn ghi nợ và xuất hiện ở frame thẳng đầu tiên; không reset lại10s. Pool tối đa4, retire khi quá sau65m/quá xa240m/lệch nhánh100m; reset/tắt/vào tù ẩn pool. Di chuyển tách cuộn đường, không dùng boost làm tốc độ khủng long. Bắt đầu lấy đà theo tốc độ tiếp cận và thời điểm đuôi thật tới làn người chơi; cooldown cú quất3,4s, không bắt đầu cú mới khi đã đi qua người chơi.
- **Va chạm:** `PhoVietDinosaurs.cs` dùng capsule theo6joint Tail0…TailTip, bán kính0,72→0,27m, bước1/120s và swept sampling<=0,18m giữa các pose. Nội suy vị trí trước/sau của xe và người chơi, xét chiều cao thật; nhảy đủ cao thì tránh. Thân/chân có volume riêng khớp hình. Giữ luồng riêng cho xác cháy cố định/xe đang chìm, không kéo chúng ra khỏi trạng thái cũ. Mỗi đối tượng một lần/cú, impulse ngang14m/s và hất xe lên5m/s, ghi nhớ đường cũ/hồi phục3s; cả armored bị hất nhưng miễn bị xe thường đẩy vẫn giữ. Người chơi dùng BeginRiderImpact rồi xung lực/độ xoay cú hất; CrashDuringPursuit không bị ép false. CSGT hất/trượt/nhảy theo vận tốc tích phân, hồi1,2s. Telemetry: DinosaursSpawned/DinosaurTailHits/HitByDinosaur/DinosaurCountdown.
- **Điểm tích hợp:** Game ResetRide/TickRide; Routes ScrollRoutes/FinishJunctionTurn (đổi khung cả root/lịch sử joint, giữ hướng đường cũ); RiderCrash TickDinosaurs(dt,false) để khủng long đang có đi tiếp nhưng không sinh mới; Police TickDinosaurPolicePush; Actions HideDinosaurs khi vào tù. Không đổi điều khiển Space/vuốt lên/W/Q hoặc jobs.
- **Sửa lỗi tiếp cận CSGT phát hiện qua test thật:** cú hất có thể đưa người chơi đến x≈−16,85 trên đường ngang lúc chưa rẽ; planner cũ chỉ chấp nhận đường chính±8,4 nên lượt bị treo. `PhoVietCrashPoliceNavigation.cs` nhận đúng mặt đường ngang ở giao lộ70±6m, |x|<=40, mở miền tìm đường ngang khi cần; giữ kiểm tra vật cản/né xe và điều kiện phải CSGT tới thật mới bắt. Test cú hất→settle→Arrested→Jailed đã đạt khi khủng long còn trong cảnh.
- **Kiểm tra cuối: 8 nhóm / 846 điều kiện hoặc tình huống đạt**, compile0lỗi/0warning. `VerifyDinosaurs`69: kích thước/rig/skin hữu hạn, loop/windup/đuôi4làn và khoảng hở Y, lịch10s ở20/30/60FPS, tốc độ tách W, pause/rebase/reset, hit một lần, xe máy/xe hơi/tải ngủ gật/xe tăng/CSGT, té thường kết thúc/té khi đuổi vào tù, ẩn dino khỏi cảnh tù. `ProbeDinosaurEncounter`96tình huống thật =3dt×2tốc độ nền17/30×W bật/tắt×4làn×nhảy/không: giữ dưới đất bị đuôi hất; nhảy đúng nhịp vẫn chạy tiếp. Hồi quy cô lập dino trong fixture cũ: LaneSteering155, SpeedJump157, Boost35, CrashOutcome51, CrashPoliceNavigation214, TankBarrage69; tất cả đạt, flags/prefs khôi phục.
- **Bằng chứng:** `Captures/dinosaur-checks.json`, `dinosaur-encounter-probe.json`, `dinosaur-regression-*.json`, `dinosaur-install.json`, `dinosaur-preview.json`, `dinosaur-final-scene.json`; ảnh `dinosaur-{walk,sweep}.png` (Blender), `dinosaur-unity-{approach,sweep}.png`. Sau kiểm tra Unity về Edit Mode, scene đã lưu; jobsEnabled=false/RideJobsReleased=false, hero xe xanh–đen giữ nguyên.
- **Trạng thái hiện hành:** nội dung đã được build/commit/push và phát hành trong **1.40.0/2c118c3/Pages40396554304/cache10d5883fe6b0325f**, cùng cập nhật21.62–21.65. Kiểm tra WebGL/local/live ở21.66 đã đạt; chưa kiểm Safari/FPS trên điện thoại thật. Unity Edit Mode/scene sạch, jobs khóa. Giữ mọi art/Wave/.blend khác không liên quan; BANGIAO.md đã đồng bộ.

### 21.63. Sửa toàn bộ phương tiện văng xuống sông bị treo trên nước (27/10/2026, Codex — đã phát hành1.40.0 ở21.66)

- **Yêu cầu/chốt:** xe tự va chạm hoặc va chạm người chơi rồi văng xuống sông phải rơi và chìm như người chơi. Áp dụng xe máy, quái xế, cả4 mẫu ô tô/bán tải/tải/container, tải ngủ gật, xe tăng và CSGT. Người dùng chốt thêm: **CSGT rơi sông thì kết thúc truy đuổi; nếu người chơi đã té thì kết thúc lượt**, không tiếp tục nằm chờ bắt. Cảnh vật và tàu/thuyền không đổi; jobs vẫn khóa.
- **Nguyên nhân đã tái hiện:** `RiverCandidate` loại mọi xe có `recovery`, trong khi `ActivateCollisionBody` thực tế tạo recovery ngay khi tông; đồng thời loại xe crossing/routeDetached/roadTurning và chỉ nhận Y<0,6m, nên xe văng còn trên không bị bỏ qua. Sau GameOver, chỉ xe đã được gắn riverFalling mới được cập nhật, còn xe đang bay về phía sông bị đứng hình. Tra cứu cũ chỉ theo7 blocks đường hiện tại, bỏ sót sông ở nhánh/đường cũ còn hiện sau cua. CSGT chưa có luồng chìm riêng.
- **Sửa runtime:** `PhoVietRiver.cs` nhận xe va chạm đang bay/hồi phục/đang rẽ/cắt ngang, chuyển sang falling trước phục hồi trên đất, dùng knockVelocity thực (không ép Y lên), hủy recovery/course/changingLane/brake và tắt lửa/khói khi vào nước. `PhoVietGame.cs` kiểm tra trước luồng motion, sau motion và sau giải va chạm/tách xe; không có quyền miễn rơi cho xe đang hồi phục. Xác cháy rơi vào nước cũng chìm; xác cháy còn trên đất giữ nguyên. Dùng kiểm tra `!=null` theo Unity cho ParticleSystem để tránh gọi Stop trên wrapper đã bị Destroy.
- **Sau thẻ kết thúc:** `TickRiverAftermath` kiểm tra mọi xe active, tiếp tục `TickKnockedVehicle` cho xe đang văng còn trên mặt cầu rồi chuyển sang rơi/chìm khi qua mép. Vận tốc dọc/ngang giảm trong nước, ô tô chìm1,15m/s, xe máy0,75m/s; chạm nước có splash/ripple/bubble. Sau khi chạm nước, góc xe tiến dần về góc nghỉ65°/82° với65°/s để container dài không tiếp tục quay đầu trồi ra khỏi nước; retire sau9s hoặc tới đáy sau3s. Giữ lớp nước đục và các miễn bám mặt đường cho model đang chìm.
- **Nhánh/cua:** `RiverZoneInTiles` xét mọi roadRows đang active, trong phạm vi nước thật±95m/Near–Far; ưu tiên mặt cầu thực đỡ xe nếu các hệ đường chồng nhau. `PhoVietRoutes.cs` đổi khung riverVelocity/riverSpin/riverRestRotation của NPC và CSGT, giữ quỹ đạo khi người chơi rẽ. Không thay xác suất cầu hay vật liệu/asset sông.
- **CSGT:** thêm PoliceInRiver/PoliceRiverAge và Begin/Check/TickPoliceRiver. `PhoVietPolice.cs` kiểm tra sông ngay sau chuyển động/va chạm trước khi có thể bắt; tắt IsChased/routePolice/CrashDuringPursuit, planner và còi. Chìm vẫn chạy sau khi hết truy đuổi (`PhoVietGame`, `PhoVietRiderCrash`, `TickRiderRiver`, aftermath). Người chơi đã té hoàn tất té rồi GameOver theo luồng hiện có. `BikeVisualAnimator` giữ pose té, không chuyển Recover1,2s khi đang chìm; `PhoVietActions.StartChase` reset body nếu gọi cuộc đuổi mới trong khi xe cũ đang chìm. CSGT ngã trên đất vẫn dựng sau1,2s.
- **Kiểm tra: 8 suite / 1.166 điều kiện đạt**, compile0lỗi/0warning. Mới `AgentScripts/VerifyRiverCrashSinking.cs`: **446** điều kiện, 8 nhóm xe×3dt(20/30/60FPS)×3luồng(recovery, detached/course flags, GameOver); dùng ActivateCollisionBody thật thay chỉ tự gắn knocked. Kiểm rơi ngay khi còn bay, hủy phục hồi, chạm/chìm sâu, không bật lại Y=0, đếm một lần; thêm va chạm NPC–NPC/NPC–người chơi thật, nhánh xoay, cầu chồng nước, xe bay khỏi cầu sau endcard, hồi phục trên đất và hai kết quả CSGT. Trước sửa đã có6 kiểm tra thất bại/6 đạt ở `Captures/river-crash-sinking-red.json`.
- **Hồi quy:** RiverBridge160, CrashUpgrade93, CrashOutcome51, CrashPoliceNavigation214, RiderContacts45, NightJumpLighting88, Dinosaurs69 đều đạt. `VerifyCrashUpgrade` cô lập riversEnabled=false trong fixture và khôi phục sau test: bộ này kiểm hồi phục **trên đất**, trước đó xe chạy tiếp12s có thể tới cầu thật; không sửa gameplay để miễn rơi. Các trường hợp sông thật được bao phủ bằng446 kiểm tra mới. Kết quả `Captures/river-sink-regression-*.json` và `river-crash-sinking-checks.json`.
- **Hình ảnh:** `AgentScripts/CaptureRiverTrafficSink.cs` chụp xe thể thao/xe máy/container khi rơi0,6s, chìm3,5s và6,5s trong GameOver; `Captures/river-npc-sink-{0.6,3.5,6.5}.png` và `river-npc-sink-visuals.json`. Đã xem ảnh rơi và sau chìm; mốc3,5s Y ô tô/container≈−7,34m, xe máy≈−6,51m, không còn treo trên nước. Bộ chụp cô lập và khôi phục scene/flags; không dùng hình làm bằng chứng chạy trên điện thoại thật.
- **Trạng thái hiện hành:** nội dung đã được build/commit/push và phát hành trong **1.40.0/2c118c3/Pages40396554304/cache10d5883fe6b0325f**, cùng cập nhật21.62–21.65. Kiểm tra WebGL/local/live ở21.66 đã đạt; chưa kiểm Safari/FPS trên điện thoại thật. Unity Edit Mode/scene sạch, jobs khóa. Giữ mọi art/Wave/.blend khác không liên quan; BANGIAO.md đã đồng bộ.

### 21.64. Blue Petshop Supercar người chơi, chuyển làn ô tô và khói nâng4góc/boost2bô (27/10/2026, Codex — đã phát hành1.40.0 ở21.66; kích thước/nhịp đổi làn được thay ở21.65)

- **Yêu cầu:** nhận model xe đua xanh mới gửi từ Meshy, thay toàn bộ người áo đỏ sao vàng + xe máy xanh dương của player; đổi làn trái/phải mượt như ô tô; Space/vuốt lên có khói trắng từ4góc phun xuống, W/vuốt xuống có2luồng khói từ hai bô. Chỉ thay player, không thay giao thông/CSGT/cảnh phố. Nhảy theo tốc độ21.60/W4s+hồi2s giữ nguyên; Q/chạm hai lần dùng nhánh ô tô áp sát/húc đã được người dùng chọn ở21.57. Jobs tiếp tục khóa.
- **Nguồn đã nhận:** `Assets/MeshyImports/Blue Petshop Supercar_20261027_004421/Meshy_AI_Blue_Petshop_Supercar_20261026_texture.fbx`, 507.941vertex/1.016.646tam giác, một mesh/UV, ~48,75MB. Đã kiểm đúng xe/logo bằng Blender MCP và ảnh `Captures/supercar-source-front.png`; không sửa/xóa nguồn Meshy. Model raw trong scene được ẩn và gắn EditorOnly, không để1triệu tam giác tham gia bản chơi.
- **Blender/asset riêng:** `ArtSource/Blender/integrate_supercar_player.py` dùng thư viện xe ở chế độ library, chỉ xuất `Assets/PhoViet/Art/PlayerCar/PV_BlueSupercar.*`, không gọi main/xuất lại xe NPC. Decimate mesh copy xuống **28.000/12.319/4.479tri** (3LOD), giữ UV/chữ/màu xanh, PBR2K gốc/mip/trilinear/aniso4/uncompressed. FBX game~2,50MB; rig15xương/7clip Legacy Idle/Drive/Brake/Impact*. Kích thước asset gốc rộng/cao/dài **2,071266×1,076452×4,4m**; source21.65 scale player0,85 còn1,760576×0,914984×3,74m, asset FBX giữ nguyên. Bánh AI dính vè chỉ cho mâm trong quay; phép khớp vòng tròn ban đầu nhận sai vè, đã đo lại ảnh chiếu cạnh và đặt tâm trước z1,20/sau−1,32, y/r0,315m. `PhoViet_BlueSupercar.blend` riêng có texture đóng gói và6helper đầu phun, không ghi đè .blend cũ. Metadata có kích thước/LOD/trục bánh/vị trí đèn và nozzles.
- **Installer:** `Editor/PhoVietSupercarUpgrade.cs` / `AgentScripts/InstallSupercarPlayer.cs` lưu scene copy trước sửa, chỉ thay visual/component dưới `g.rider`, bỏ chân/rig xe máy player. Gắn PlayerCarVisual và CarJumpJetVisual; so sánh serialized cấu hình28NPC và CSGT trước/sau: giữ nguyên. Root player/luật crash/CSGT/điểm/Coin/jobs giữ. Đèn trước2nguồn/đèn hậu2nguồn và emission mask UV riêng theo vị trí lens mới; nguồn head(0;0,46;2,04), tail(±0,73;0,62;−2,10). Đèn theo rig khi nhảy/đánh lái.
- **Chuyển làn:** `PhoVietGame.AdvanceLaneCurve` dùng đường cong bậc5 liên tục vị trí/vận tốc/gia tốc hiện tại; bản21.64 ban đầu dùng một làn **1,35→1,60s** theo tốc độ17→67,5m/s. **Đã thay ở21.65:** tiến độ tăng tỷ lệ Speed, một làn tương đương22,95m đi tới (1,35s ở17m/s;0,34s ở67,5m/s); giữ quỹ đạo và nhiều làn theo căn khoảng cách. Giới hạn0,60–2,60 là giây tham chiếu17m/s, không cố định thời gian thực. Đổi ý giữa chừng giữ vận tốc/gia tốc thay vì giật lại. Yaw theo tiếp tuyến vận tốc ngang/dọc, lọc0,08s vào/0,13s trả lái; root vật lý quay cùng hình, không nhân yaw lần hai trong visual hoặc lúc crash/rơi sông. Thân nghiêng nhẹ **ra ngoài** theo gia tốc ngang0,18°, tối đa2,5°, damping0,22s; không bank xe máy/giữ nghiêng cũ. `PlayerCarVisual` bánh trước đánh lái theo atan(wheelbase×gia tốc ngang/Speed²), trả dần; mâm quay theo tốc độ/radius. Hướng va chạm cũ/mới và quét cảnh vật có nội suy góc. Không giảm cruise/W khi đổi làn/cua.
- **Cập nhật source21.89:** các mốc22,95m/1,35s/0,34s trên là lịch sử21.65; hiện pace xe đua nhân thêm1,15, thời gian chia1,15 và full curve19,9565m. Chi tiết/xác nhận399 điều kiện ở21.89.
- **Nhảy4động cơ:** `CarJumpJetVisual.cs` gắn4socket dưới gầm `(±0,77;0,17;1,35)` và `(±0,77;0,17;−1,45)`, phụt dọc xuống theo góc xe. Mỗi đầu2lớp particle (lõi ngắn đặc + khói mềm tản), tái dùng flipbook khói thể tích4frame đã dựng Blender `Resources/Boost/PV_WhiteBoostSmoke`. Không có mô phỏng fluid runtime. Bật từ windup tới bay/đáp, theo pose pitch nhỏ−3→4°/damping0,085s, dừng rồi tan sau đáp; pause đóng băng, reset/crash/rebase xóa trails. Hạt thả ra thuộc hệ đường, socket vẫn theo rig. Nhảy/đáp/chạy nóc vẫn dùng luật thật, front/rear axle lấy từ model mới thay số đo pickup cũ (`PhoVietPlayerVehicle.cs`).
- **Boost2ống bô:** `BoostSmokeVisual.cs` hỗ trợ danh sách exhaustMounts, fallback1socket cho xe máy/cũ. Player mới dùng `(±0,73;0,20;−2,13)` trên bumperRear; cả hai có lõi và lớp tản trắng, theo rig. Emitting/ParticleCount/Clear/Simulate/Pause/Rebase áp dụng toàn bộ, không bỏ sót bô thứ hai. W vẫn+125%/4s/hồi2s và không đổi tốc độ NPC; tổng hệ particle lift+boost có giới hạn số hạt cho mobile.
- **Kiểm tra cuối: 8suite / 1.602 điều kiện Unity đạt**, compile0lỗi/0warning, diff whitespace đạt. `VerifySupercarMotionFx`90:12tổ hợp trái/phải×W/tắt×20/30/60FPS, quỹ đạo đơn điệu/không snap/giữ tốc độ/thời gian đúng/yaw và collision cùng hướng/roll nhỏ, đổi ý giữa chừng; bốn jet có hạt/hướng xuống/theo xe, pause/đáp/reset/crash, hai bô cùng phát. `VerifySupercarPlayer`76: footprint/LOD,12húc thật và hủy khi bị chắn, nhảy/boost/đèn theo xe, crash thường kết thúc/CSGT tới thật vào tù. Test húc cho đường cong mới hoàn tất3cm cuối trong0,8s rồi bắt buộc tâm làn sai<1mm, thay kiểm kiểu snap ngay khi offset húc hết. Không sửa gameplay/miễn va chạm để ép test đạt.
- **Hồi quy khác:** SupercarTurns32 (4làn×W/tắt×20/60FPS, giữ tốc độ/vào đúng làn/không cạ chậu khi rẽ đã chọn trước); BoostTrafficIsolation478; MeshyCarRoofs254; RiverCrashSinking446 (gồmCSGT); Dinosaurs69; SpeedJump157. Fixture cô lập hệ ngẫu nhiên liên quan và khôi phục flags/prefs sau chạy. Báo cáo `Captures/supercar-verification-summary.json`, `supercar-motion-fx-checks.json`, `supercar-player-checks.json`, `supercar-regression-*.json`.
- **Ảnh đã xem:** `AgentScripts/CaptureSupercar.cs` → `Captures/supercar-{front,rear,portrait,landscape,lane-change,four-lift-jets,dual-boost,night-lights}.png`; đã xem dọc,2bô,4jet và đèn đêm. Ảnh RenderTexture theo camera dọc/ngang, không gồm IMGUI và không phải kiểm thử trên điện thoại thật.
- **Trạng thái hiện hành:** nội dung đã được build/commit/push và phát hành trong **1.40.0/2c118c3/Pages40396554304/cache10d5883fe6b0325f**, cùng cập nhật21.62–21.65. Kiểm tra WebGL/local/live ở21.66 đã đạt; chưa kiểm Safari/FPS trên điện thoại thật. Unity Edit Mode/scene sạch, jobs khóa. Giữ mọi art/Wave/.blend khác không liên quan; BANGIAO.md đã đồng bộ.

### 21.65. Thu nhỏ xe đua15% và chuyển làn nhanh tỷ lệ tốc độ (27/10/2026, Codex — đã phát hành1.40.0 ở21.66)

- **Yêu cầu:** player Blue Supercar nhỏ hơn15%; xe chạy càng nhanh thì đổi làn càng nhanh tỷ lệ thuận, kể cả tăng tốc W; giữ quỹ đạo/đánh lái/trả lái mượt. Không thay NPC/CSGT, luật nhảy/húc/boost, camera hay jobs.
- **Scale đồng bộ:** `PhoVietSupercarUpgrade.PlayerScale=0,85`, `ApplyPlayerScale` đặt visual.localScale tuyệt đối0,85 (không nhân lặp), size **1,760576×0,914984×3,740000m**, halfWidth0,880288/halfLength1,87; axle trước1,02/sau1,122m, bán kính bánh0,26775m. Vùng va chạm/clearance/cảnh vật/nóc xe lấy thông số PlayerCarVisual đã scale, không giữ hitbox cũ. Bốn jet/hai bô và socket đèn đều nằm trong rig nên đi theo scale; halo cũng giảm15%. Head nguồn bên ở `(±0,6205;0,391;1,734)`, bô `(±0,6205;0,17;−1,8105)`, jet x±0,6545/y0,1445. Texture/UV/FBX/3LOD và Blender source vẫn kích thước gốc, không xuất lại asset.
- **Lưu scene:** installer xử lý cả xe đã cài và lần cài mới; dùng `PrefabUtility.RecordPrefabInstancePropertyModifications` cho scale của FBX instance, save scene. Đã chạy installer lại và **mở lại scene từ đĩa** xác nhận scale0,85/kích thước đúng, không thu nhỏ lần hai. `PlayerCarVisual` khi nâng hình xe té khỏi mặt đất chia local box cho ModelScale trước TransformPoint để không nhân scale hai lần.
- **Chuyển làn:** `PhoVietGame.AdvanceLaneCurve` không còn tăng thời gian khi chạy nhanh. Giữ đường cong bậc5; clock tăng **dt×max(Speed,1)/17** mỗi bước, kể cả khi W bắt đầu/hết giữa đường cong. Một làn có1,35 giây tham chiếu ở17m/s, tương đương **22,95m đi tới**; thời gian danh nghĩa17→1,35s,30→0,765s,38,25→0,60s,67,5→0,34s. Nhiều làn vẫn nhân căn khoảng cách; T được giới hạn trong đơn vị tham chiếu cũ0,60–2,60s, không chặn tốc độ đổi làn ở tốc độ cao. Vận tốc đạo hàm nhân pace, gia tốc hình học nhân pace²; khi lập lại vì đổi hướng, hệ số đầu chia pace/pace² để giữ vận tốc/gia tốc đang có. Điều khiển xe máy cũ pace=1 nên không đổi.
- **Mốc lịch sử21.65:** công thức/timing trên đã được tăng nhịp thêm15% ở source21.89 (`pace=max(Speed,1)/17*1.15f`); giữ các kết quả kiểm tra21.65 để đối chiếu, xem21.89 cho timing hiện tại.
- **Cảm giác lái:** yaw vẫn theo vận tốc ngang/chạy tới; hình và collider cùng hướng; bánh trước đánh/trả lái theo độ cong, thân nhún nhẹ/cap2,5° như21.64. Thay đổi tốc độ chỉ tiến nhanh/chậm hơn trên quỹ đạo hiện có, không dời xe tức thì hay lập lại từ đầu. W giữa không trung vẫn chỉ cộng độ cao cho cú nhảy sau theo21.60, không đổi quỹ đạo nhảy hiện tại.
- **Kiểm tra cuối: 5suite / 634 điều kiện Unity đạt**, compile0lỗi/0warning; diff whitespace đạt. Mới `VerifySupercarScalePace`**182**: scale model/hitbox/axle/radius/socket;36tổ hợp trái/phải×3tốc độ17/24/30×W/tắt×20/30/60FPS, thời gian giảm đúng tỷ lệ và cùng quãng đường tiến, không snap/overshoot, W tăng nhịp2,25 lần, W giữa làn và đổi ý khi chạy nhanh. Socket đèn đo ở clip Idle frame0 để không lẫn nhún thân Drive vào phép đo vị trí cố định.
- **Số đo:** ở60FPS, ngưỡng tới tâm làn sai<1mm: tốc độ17m/s hoàn tất~1,30s/22,42m;30m/s~0,75s/22,50m; W ở17m/s~0,583s; W ở30m/s~0,333s. Khác thời gian danh nghĩa do ngưỡng1mm và lấy mẫu frame. W kích hoạt sau~0,3s khi đang chuyển làn hoàn tất tổng~0,77s; không chờ cú đổi làn sau.
- **Hồi quy:** `VerifySupercarMotionFx`90 (cập nhật kỳ vọng3,74m/axle và cùng quãng đường, giữ mọi kiểm tra nhảy4jet/boost2bô/pause/crash); `VerifySupercarPlayer`76 (footprint nhỏ, húc/va chạm/CSGT, nhảy/đèn); `VerifySupercarTurns`32; `VerifyMeshyCarRoofs`254. Kết quả `Captures/scale-pace-summary.json`, `supercar-scale-pace-checks.json`, `scale-pace-Verify*.json`; `scale-pace-persistence.json` chứng minh giữ scale sau reload. Flags/prefs khôi phục sau test.
- **Ảnh:** `CaptureSupercar` chụp lại xe nhỏ; đã xem ảnh dọc mới. Bản bằng chứng riêng `Captures/scale-pace-supercar-{portrait,landscape,lane-change,four-lift-jets,dual-boost,front,rear,night-lights}.png`; RenderTexture preview, không gồm IMGUI/không phải đo trên điện thoại thật.
- **Trạng thái hiện hành:** nội dung đã được build/commit/push và phát hành trong **1.40.0/2c118c3/Pages40396554304/cache10d5883fe6b0325f**, cùng cập nhật21.62–21.65. Kiểm tra WebGL/local/live ở21.66 đã đạt; chưa kiểm Safari/FPS trên điện thoại thật. Unity Edit Mode/scene sạch, jobs khóa. Giữ mọi art/Wave/.blend khác không liên quan; BANGIAO.md đã đồng bộ.

### 21.66. Build/phát hành WebGL1.40.0 — Supercar, khủng long và chìm sông (27/10/2026, Codex — đã phát hành và xác minh live)

- **Ủy quyền:** người dùng yêu cầu “Hãy build và phát hành web”; gồm commit/push main/Pages theo15.2, không hỏi lại. Đưa21.62–21.65 lên web: Emerald Tyrant, mọi xe văng sông chìm/CSGT rơi kết thúc truy đuổi, Blue Supercar player scale0,85/đổi làn theo tốc độ, nhảy4jet và boost2bô. Jobs vẫn khóa. Giữ các art/Wave/.blend khác không liên quan ngoài commit.
- **Version/preflight:** đồng bộ PhoVietWebBuild/ConfigureWebRelease/VerifyWeb/VerifyFullscreenGame/VerifyWebBundle/README và ProjectSettings=1.40.0. Scene đúng player3,74m/scale0,85,28NPC+CSGT giữ nguyên, dino/rivers bật, jobs=false; **9 kiểm tra scene đạt**, không missing script,401dependency tồn tại. 2.776 file source/asset/config có fingerprint trước/sau build **không đổi**; các FBX/texture xe máy cũ đang sửa không nằm trong scene dependency.
- **Build thật:** `build_80cb7c1cfac1` Succeeded,336447ms,**0lỗi/3warning**: Pipeline thiếuRuntimePipelineConfig cho Player,2CS0414 dragging/hudTouchChoice chưa dùng (như bản trước). Không sửa gameplay để dọn warning. WASM6.230.115byte, data119.573.248byte; cache **10d5883fe6b0325f**, tổng runtime126.884.683byte. `Captures/release140-build-report.json`, `release140-scene-checks.json`, `release140-source-fingerprint.json`.
- **Dữ liệu vượt giới hạn file GitHub:** bổ sung `AgentScripts/PublishWebArtifact.py`: pack data thành4phần32MiB/32MiB/32MiB/18.909.952byte, mỗi phần và file gốc có SHA256 trong `docs/web-data-parts.json`. Gitignore data nguyên vẹn local, chỉ commit parts/manifest; `_site` cũng gitignore. WorkflowPages ghép lại vào `_site`, xác minh hash từng phần/hash cuối/cache trước upload; runtime/loader/SW vẫn nhận đúng file gốc, không đổi chất lượng asset. Pack/reassemble không rebuild C# và không giả cache. Script không ghi đè output không rỗng, chặn path ra ngoài artifact. Update15.2/15.3/15.4 và README cho quy trình này.
- **Kiểm tra đóng gói:** `VerifyPublishParts.py` **7 kiểm tra đạt**: ghép khi không có local original, hash/data/cache nguyên vẹn, site không lẫn helpers, từ chối part hỏng/mất/path traversal/output có dữ liệu. Artifact local `_site` được ghép theo cùng lệnhCI, **29 kiểm tra runtime bundle đạt**; repo docs+parts **34 kiểm tra đạt**. `release140-parts-checks.json`, `release140-site-bundle-checks.json`, `release-1.40-bundle-checks.json`. Cập nhật VerifyWebBundle hỗ trợ repo parts và site đã ghép; shell/binary/precache/hash cùng phiên bản.
- **Gameplay đã kiểm trước build:**21.65 có5suite/634 điều kiện Unity đạt;21.64 có8suite/1.602 điều kiện,21.63 có8suite/1.166,21.62 có846 điều kiện/cuộc gặp. Không chạy lại tất cả khi không có thay đổi gameplay mới.
- **Web local:** VerifyWeb **37 kiểm tra** desktop/khởi tạo WASM/offline/touch/manifest/installability, VerifyFullscreenGame **20 kiểm tra** iPhone/iPad dọc/ngang đều đạt,0lỗi runtime. Đã xem ảnh Android đang lái Supercar. Sau chuẩn hóa cache cuối, kiểm offline thêm **5 điều kiện** (tải đủ cache, shell offline, WASM offline, input W/Space/P,0lỗi). `release140-local-results.json`, `fullscreen-game-release140-local-results.json`, `release140-final-offline-results.json`.
- **Artifact trong commit:** đã phát hiện SVG có xuống dòngCRLF khác GitLF; `PhoVietWebBuild.FinalizeWebOutput` chuẩn hóa mọi web textLF trước hash, `.gitattributes` thêmSVG LF. Chỉ refresh shell sau build, không biên dịch lại/đổi WASM/data. Cache cuối10d5883fe6b0325f, tổng126.884.682byte. **27 đối chiếu byte staged/runtime/parts/SW/manifest đạt**, tương đương output Pages; manifestPython cũng ghiLF. Đã bỏ2dấu cách cuối trườngYAML rỗng prefabDino, giá trị không đổi; báo cáo fingerprint ghi riêng thao tác định dạng sau build. Không sửa model/gameplay cho việc đóng gói.
- **Git/phát hành:** commit **2c118c3769221e9f9c2218dec5dc26ecb560589d** đã push origin/main và xác minh SHA từ xa. GitHub chấp nhận sourceDinosaur.blend79,07MiB, có cảnh báo khuyến nghị50MiB; không cản push. Pages run **40396554304 success đúngSHA**, gồm bước ghép parts/kiểm cache/upload/deploy thành công. `Captures/release140-push-results.json`, `release140-pages-results.json`. Đã xử lý khóaGit rỗng bị bỏ lại bằng kiểm không cóGit + mở file độc quyền, giữ khóa cũ trong.tools; không xóa index hay thay đổi ngoài phạm vi.

- **Live cuối:** `VerifyPublishedGame.cjs` **26 kiểm tra đạt/0lỗi runtime**, version1.40.0/cache10d5883fe6b0325f; toàn bộ tài nguyên remote (gồm data119.573.248byte đã ghép) khớp bản local, Unity WebAssembly thật khởi tạo, Start/Pause hoạt động. `Captures/release140-live-results.json`. Link: https://denispham1107.github.io/taixecongnghe/.
- **Bàn giao hoàn tất:** hai tài liệu ghi nhận cùng version/SHA/run/cache, giữ source/gameplay/binary đã kiểm; chỉ commit báo cáo/tài liệu sau release, không rebuild hoặc kích hoạt lại jobs. Unity Edit Mode, scene sạch; đã dừng server localhost. Art/Wave/.blend khác không liên quan vẫn ở cục bộ. Thử mobile là Chrome mô phỏng dọc/ngang, chưa phải Safari/thiết bị thật. Với tab/PWA cũ, đóng các tab game rồi mở lại để service worker mới kích hoạt, không xóa dữ liệu Coin/kỷ lục.


### 21.67. Urban Rider nguyên khối, đánh lái/đổi làn/rẽ mượt — đã phát hành1.41.0 qua21.68 (27/10/2026, Codex)

- **Yêu cầu/chốt:** đã nhận `Assets/MeshyImports/Urban Rider_20261027_035549/Meshy_AI_Urban_Rider_20261026_texture.fbx`; thay mọi xe máy có người **trừ cả người và mô tôCSGT** theo trả lời trực tiếp của người dùng. Player hiện là Blue Supercar nên không thuộc phạm vi thay. Không bật jobs. Áo/xe NPC nhiều màu, không thêm sao vàng.
- **Nguồn thật:** FBX gốc120.667.852byte, một mesh2.664.028tri/1.331.108vertex, khôngrig. Giữ assetnguồn nguyên vẹn, bỏ đúng một raw preview ra khỏiscene (backupscene ở.tools); không xóa file người dùng. File này >100MiB, **không stage/push mù quáng**; game chỉ tham chiếu asset tối ưu4MB. Nguồn `.blend` riêng9,6MB, không sửa các file Blender cũ.
- **BlenderMCP:** `ArtSource/Blender/integrate_urban_rider.py` tạo scene riêngPV_UrbanRider_Work, giữ tỷ lệ/ngồi/nắm ghi đông sẵn có, chuẩn cao2,15m/rộng1,101m/dài2,267m. Decimate3LOD **42.000/18.480/6.720tri**, rig19xương Frame/Steering/2Wheel/Pelvis/Torso/Head/12xươngtaychân. Steering là cha của bánh trước và hai bàn tay; hai cánh tay giảiIK2đoạn, thân trên xoay65% góc ghi đông. 7clipLegacy Idle/Ride/KickLeft/KickRight/FallLeft/FallRight/Recover; Idle/Ride thởnhẹ, ngã/dựng kết hợp rootvật lý. KickLeft/Right giữ slot tương thích, không dùng để đổi kỹ năng player làô tô. Không tạo thêm người/xe cũ chồng dưới modelmới.
- **Skinning/hình:** mọi vertextrùng tại đường ghép cổtay/vai/cổxe được thống nhất tốiđa2weights để không hở mép khi đánh lái; không kéo dài ngón hay sửa tư thế nắm sẵn. Chỉ phần mâm trong quay, đườngbao lốp/dè/độngcơ AI dính nhau giữ ổn định. Trụcquay được chốt ở bindpose trong hệparent để không quay sai khi cổxe xoay. Bánh trướcR0,38156m, sauR0,32169m; axle trước(0;0,38728;0,69155), sau(0;0,32481;−0,64155). ColliderNPC halfWidth0,33/halfLength1,18016m; tênrole/khốilượng220kg/phânloại/speed/spawn/luật va chạm giữ nguyên.
- **Màu/đèn:** `prepare_urban_rider_maps.py`, `UrbanRider.shader`; giữPalette/Normal/Surface2K gốc, dùngRegions R=áo/G=sơn vàEmission riêng. UVmask padding5px chống viềntrắng kheUV/mipmap; thu vùng pha đúng cụm đèn, không làm cả yếm phát sáng. Vậtliệu4role đang dùng: City trắng/trắng, Underbone áo xanhđậm/xe xanhngọc, Sport áo nâu/xe vàng, Racer áo xanhrêu/xe đỏ; màu da/tóc/quầnjeans/giày/chi tiết đen/chrome giữnguồn. Mips/trilinear/aniso4, mapschung.
- **Installer:** `Assets/PhoViet/Editor/PhoVietUrbanRiderUpgrade.cs`, entry `AgentScripts/InstallUrbanRider.cs`. Đã thay **31actor=17traffic (gồm quái xế)+14parked**, tạo3`PV_UrbanParked_<role>.asset` và gắn vào**56mesh tĩnh** đườngnhánh. Cả3LOD/7clip19bone đãthamchiếu đúng; không sửa `Generated/RouteParked_*` cũ. Giữ rootpool/sign/brake và giá trị semantic; nối lại17bộđèn (pha dướiSteering, hậu dướiFrame), bôsocket; cậpnhật emissionusers và bỏ tham chiếu atlas xe dân sự cũ không còn renderer dùng (tránh mang cả hai bộ texture vào WebGL). KiểmtraJSON/visualCSGT vàPlayerCarVisual trước/sau đều không đổi, jobsfalse. Backupscene `.tools/PhoViet-before-urban-rider.unity`.
- **Chuyển động:** `PhoVietTraffic.cs` riêng UrbanRider dùng quintic C2 `u³(10−15u+6u²)` và đạo hàm vận tốc ngang `30u²(1−u)²*dx/duration`; duration=Clamp(12/max(|speed|,6),0,65,1,6)s. Cùng/ngượcchiều đều áp dụng, không lấy tốcđộWplayer làm nhịpNPC. `UrbanRiderMotion.cs` lấy yawrate/tốcđộ di chuyển thực để suy độcong, cổxe=atan(wheelbase*curvature) giới hạn25°, nghiêng=−atan(v²curvature/g) giới hạn23°; SmoothDamp cổ0,14s/0,25s ra, thân0,20s/0,30s ra. Cánh tay theo tayláithật, thântrên xoay65% góclái; không nhảtay độtngột khi cua. Hai hệroadturn/crossing dùng cùng component; không đổi luật/quỹđạo/giới hạn tốcđộ rẽNPC cũ. `PhoVietRoutes.cs` bù historyyaw qua phép rebase90° nên không phát sinh cú đánh lái giả. `BikeVisualAnimator.cs` sampleclip trướcIK/steering rồi quaymâm, nâng hình theo tiếp xúc lốp khi nghiêng, không nâng xe rơi sông; skip springs ởdt0 để không tạoNaN, pause giữpose. Các actor không cóUrbanRiderMotion giữnhánh cũ.
- **Kiểmtra:** compile0lỗi/0warning. **7suite/1.493điều kiện Unity đạt**: VerifyUrbanRider**286** (31actor/56baked/LOD/rig/material/shader, 36ca đổi làn 8/20/40m/s ×16/33/50ms ×2chiều ×2bên,8ca rẽ dọc/băngngang, IK, pause, dt0,rebase); VerifySupercarPlayer76; VerifyBoostTrafficIsolation478; VerifyRiverCrashSinking446; VerifyCrashUpgrade93; VerifyRoadDrivers45; VerifyDinosaurs69. Maxsai lệch endpointtay trong cua gắt~7,3mm, các case đườngdọc/đổi làn gần0. `VerifyCrashUpgrade` so sánh đúng xương dướivisual (bỏ riêngrootvisual vì nay có nâng tiếp xúc bánh~9mm), không nới ngưỡng bone0,1°/1mm. Báocáo `Captures/urban-rider-checks.json`, `urban-regression-*.json`, install/captures/export/mapsJSON.
- **Ảnh đã xem trongUnity:** front/side/rear/steering/grip/corner, từng4màu, game landscape, night. Đã sửa cổtay hở, viềntrắng UV và vùngđèn lan vào yếm theoảnh; model/hands/phamàu hiệnđúng. Chưa kiểmSafari/thiếtbịthật, chưa buildWebGL bảnnày. Nguồnasset tốiưu/sceneđãlưu, jobs khóa; đã thoátPlay vềEditMode. `InspectUrbanRiderScene` xác minh sceneDirty=false,31Urban/1CSGT/56baked,0missing scripts,0phụthuộcrawFBX và0FBXxe máy dân sự cũ trong dependencies (`Captures/urban-rider-saved-scene.json`).
- **Trạng thái mới nhất:** toàn bộ21.67 đã commit/push/build/phát hành **1.41.0**, commit648b163/Pages40420198461/cachec9db437e230d3824,26livechecks đạt (21.68). Nguồn/asset/meta/scene/code/tài liệu đi cùng release; giữ các art/Wave/.blend/Captures không liên quan. Không chạy installer MeshyRiders/Wave cũ vì sẽ thay ngược lại mẫu mới.

### 21.68. Build và phát hành WebGL1.41.0 — hoàn tất (27/10/2026, Codex)

- Người dùng yêu cầu trực tiếp build và phát hành web sau khi hoàn tất Urban Rider21.67. Đồng bộ version1.41.0 ở PhoVietWebBuild/ConfigureWebRelease/ProjectSettings/VerifyWeb/VerifyFullscreenGame/VerifyWebBundle/README; giữ toàn bộ cấu hình PWA/DPR/camera và jobs khóa. Không chạy lại installer hoặc thay gameplay khi build.
- Preflight14kiểm tra scene đạt: Supercar85%/4jet/2bô,31UrbanRider+56baked,CSGT gốc,28pool, dino/sông bật, jobsfalse,391dependencies đều tồn tại/khôngmissing script/không rawUrbanFBX hay model dân sự cũ. 7suite/1.493điều kiện gameplay21.67 đã đạt trước build. Cầu nối Unity lúc đầu chưa nạp; kích hoạt editor và chờdomain nạp xong, không cài lại/nâng package/mở editor trùng.
- Build **build_2bfab4572e28 Succeeded**,284.363ms,0lỗi/3warning đã biết: Pipeline thiếuRuntimePipelineConfig (công cụEditor), CS0414 dragging/hudTouchChoice chỉ dùng ngoàiWebGL. Không có warning mới. Source/config **2.816file fingerprint không đổi** trong build. PackedAssets xác nhận25entryUrban (gồm rig/mesh/map/script), không chứa rawFBX120,7MB hoặc model dân sự cũ. `Captures/release141-build-report.json`, `release141-source-fingerprint.json`, `release141-scene-checks.json`.
- Artifact cache **c9db437e230d3824**,version1.41.0,116.986.641byte precache. Đã copy toàn bộBuilds/WebGL→docs, dọn đúng hash cũ trongdocs/Build, pack4phần32MiB theo21.66; materialize ra.tools/site141 đúng cáchCI. **34kiểm tra repo bundle và29kiểm tra site ghép đạt**. `release141-repo-bundle-checks.json`, `release141-site-bundle-checks.json`. Toàn bộ file runtime/caches được giữ đúng byte của build tốt.
- **Web local:** VerifyWeb **37kiểm tra đạt**, gồm WASM,keyboard/touch/doubletap,rotate,PWA/installability và offline khởi động game thật; VerifyFullscreenGame **20kiểm tra đạt** ở iPhone/iPad dọc/ngang cósafeinsets.0lỗi runtime. ẢnhAndroid/menu/game và mobile ngang đã xem. Báo cáo `Captures/release141-local-results.json`, `fullscreen-game-release141-local-results.json`. Đây là Chrome mô phỏng thiết bị, chưa phải Safari/điệnthoại thật.
- **Git artifact:** **31đối chiếu đạt**: bytes runtime trong index,4parts ghép/hash/cache và SW/manifest chính xác như bản local; mọi scene dependency dự án đã tracked/staged, không rawUrbanFBX. `Captures/release141-staged-artifact-checks.json`. Git diff --check sạch sau chuẩn hóa newline cuối2AgentScript (ngoàiAssets, không ảnhhưởng build). KhóaGitindex rỗng bị bỏ lại đã được xác minh không có Git/độcquyền file và giữ lại trong.tools, không xóa index.
- **Git/Pages:** commit **648b16326d635c264649d7178af85b47c4b0a2ac** đã push origin/main, xác minh remoteSHA khớp. Pages run **40420198461 success đúng SHA**: checkout, ghép/kiểm parts, validate, upload và deploy đều thành công. Không có rawMeshyFBX120,7MB trong commit, không kéo art/Wave/.blend cũ ngoài phạm vi. `Captures/release141-push-results.json`, `release141-pages-results.json`.
- **Live cuối:** **26kiểm tra đạt/0lỗi runtime**,version1.41.0/cachec9db437e230d3824. Toàn bộ tài nguyên trên https://denispham1107.github.io/taixecongnghe/ khớp local, gồm data109.672.744byte ghép từ4parts; UnityWebAssembly thật khởi tạo, Enter/Pause hoạt động. Đã xem ảnh live. `Captures/release141-live-results.json`.
- **Bàn giao:** Unity EditMode, không compiling/sceneDirty, scenePhoViet chuẩn/version1.41.0/jobsfalse; `Captures/release141-final-editor.json`. Serverlocalhost8765 đã dừng. Hai tài liệu cập nhật cùngversion/SHA/run/cache và commit bằng chứng riêng sau release; không rebuild hoặc sửaartifact đã kiểm. Với tab/PWA cũ, đóng tất cả tab/app game rồi mở lại để worker mới kích hoạt; không xóa dữ liệu Coin/kỷlục. Chưa thửSafari/điệnthoại thật.

**Nguồn kiểm chứng chính:** mã C# và cấu hình hiện tại trong repo, `ArtSource/Blender`, metadata asset, Git history, các báo cáo `Captures/VERIFICATION_*.md` và JSON kết quả tương ứng. Nếu dự án tiếp tục thay đổi, cập nhật lại tài liệu này theo hành vi thực tế; đừng coi một con số/version/port của ngày bàn giao là thông tin luôn đúng về sau.


### 21.69. Tối ưu lag desktop/mobile, giữ độ nét và cảnh gần — đã phát hành1.42.0 (27/10/2026, Codex)

- **Yêu cầu:** giảm giật/lag cả desktop/mobile, không làm môi trường xấu hoặc bể/nhòe hình. Không đổi camera, DPR3/cap3,2Mpixel, texture2K, AA4, global lodBias2, shadowDistance100, chất lượng phản chiếu nước; không giảm số xe, bỏ vật cản hoặc đổi luật chơi. Jobs vẫn khóa.
- **CPU:** `PhoVietRiver.cs` cache bố cục tương đối35tile theo tile chính; common scroll không đổi giao cắt nhà/sông nên bỏ full sweep lặp. Invalid khi recycle/đổi khung cua/active/scale/rotation/SetBridge; `RiverBlock.LayoutRevision` tăng cả khi gán lại cùng loại vì SetBridge có thể bật lại streetOnly. Chỉ xét tile active (tile ẩn được xét lại trước khi hiện); stamp giữ pose lần tính thật, không nuốt sai lệch nhỏ tích lũy. `RiverVisibilityRefreshes` phục vụ kiểm chứng.
- **Va chạm cảnh:** `PhoVietObstacleBatches.cs` gom770 StreetObstacleSet thành35tile, aggregate bounds bảo thủ; loại tile xa trước khi chạy nguyên narrowphase/swept rotation trong `PhoVietSceneryContacts.cs`. Rebuild nếu source/array obstacle đổi. Không đổi collision boxes/normal/contact time/quy tắc nhảy tránh vật cản.
- **Đèn:** `DayNightLighting.AdvanceSimulation` chỉ tính đúng đồng hồ/pha mỗi substep; `PhoVietDayNight` dùng hàm này; ApplyEnvironment+350street lamps thực hiện ở LateUpdate sau pose cuối. Public Advance/SetCycleTime vẫn cập nhật ngay cho preview/test; UpdateLamps flush pending. Giữ6real lights/4pixel lights, nguồn/projector theo xe khi nhảy, toàn bộ màu/cường độ/chu kỳ/sương/đèn. Đây là cập nhật cơ chế20/21.41; số nguồn350 hiện hành thay số70 lịch sử trước cầu.
- **GPU:** `Editor/PhoVietPerformanceUpgrade.cs` chỉ chỉnh700group shop/tree3LOD trong scene: LOD0 transition0,45, LOD1 0,175, LOD2 cuối0,006 giữ nguyên, thay0,18/0,07 lịch sử11.4/21.19. `PhoVietStreetUpgrade.Place` đồng bộ để lần tạo mới không quay lại ngưỡng cũ (không chạy rebuild phố). Toàn bộ mesh/material/texture/asset dependencies giữ nguyên; scene save có vài sai số float localReferencePoint dưới0,1mm do Unity SetLODs. Không bật GPU instancing hàng loạt vì phép thử chậm hơn; không tắt bóng/phản chiếu hoặc hạ resolution.
- **Đo:** cùng Unity6000.5.6f1/D3D11/IntelUHD620; median5round delegate: HideBuildingsOverRiver0,85604→0,037491ms (~95,6% giảm), scenery sweep0,364536→0,239033ms (~34,4%), TickRide(dt0)1,688213→0,325617ms (~80,7%). dt0 chỉ đo overhead, không phải FPS thực; GC counter Mono trả0 không đủ để khẳng định không allocation. `ProfileStreetDetail` A/B luân phiên4round cùng frozen scene1100×700/AA4, Camera.Render+ReadPixels1pixel:61,1263→52,7958ms (~13,6% giảm), không phải FPS gameplay. Captures/runtime-cost-before/after.json, street-lod-controlled.json.
- **Ảnh:** original/optimized cùng frame1100×700: mean channel diff0,2618/255;0,7614%pixel chênh>10;45%phần dưới ảnh **MAE0** (cảnh gần giữ nguyên pixel), biển/mái hiên/màu/bóng vẫn giữ. `Captures/perf-street-original.png`, perf-street-optimized.png, performance-image-comparison.json.
- **Kiểmtra Unity:** compile0;10suite/**3.810điều kiện đạt**: PerformanceSafety1326 (1200exact contact so code1.41,120cache/full sweep,common scroll,clock/quality), PerformanceLighting717 (601pha, fog/pause,5substep equivalence,20jump/projector), RiverBridge160, RiverCrashSinking446, BoostTrafficIsolation478, SupercarMotionFx90, CrashUpgrade93, SupercarScalePace182, SupercarTurns32, UrbanRider286. RiverBridge fixture nay tắt/khôi phục dinosaur khi thử cầu độc lập và ngưỡng rơi theo nhịp đổi làn ô tô1,5s (xe máy0,6s như cũ); không sửa gameplay. Các test DayNight/MorningFog/NightJumpLighting lịch sử còn giả định70đèn/playerxe máy/jobsmở nên không dùng làm chứng nhận hiện hành; PerformanceLighting thay phần liên quan, jobs không mở để ép test. Báo cáo perf-regression-*.json/performance-safety-checks.json/performance-lighting-checks.json.
- **Build:**16preflight scene đạt,391dependencies, source2820file fingerprint trước build. Unity build_1bc4dfad9085 thànhcông313,691s/116.993.884byte/0lỗi,3warning cũ (Pipeline không bật trongPlayer,2field khôngdùng). Version1.42.0/cache d7e9bf2a636e37f7; `Captures/release142-build-result.json`.
- **Web đã kiểm:**2820file source/config không đổi sau build; đónggói docs/4parts32MiB và materialize.tools/site142;34repo+29site+31stagedbytechecks đạt.37web/offline/desktop/mobiletouch đạt/0runtime, ảnh dọc/ngang đã xem. Fullscreen20+DPR3 14 đạt: framebuffer1170×2532 dọc/2532×1170 ngang, tablet2066×1549 theo cap3,2Mpixel, xoay lại khôi phụcDPR3; không overflow/0runtime. Đã xem ảnh dọc/ngangDPR3. Tổng71browserchecks; hiện vẫn chưa thử Safari/điện thoại thật. Đã phát hành và kiểm tra live như dưới.
- **Phát hành hoàn tất:** commit **be0c6ffc77319cd7adc1e935194f9df3b93cc6b3** đã push main; **Pages run40445290840 success**, version**1.42.0**/cache**d7e9bf2a636e37f7**. **26livechecks đạt/0runtime**: toàn bộ runtime20files+worker/precache khớp byte build local, data109.671.867byte ghép4parts; khởi tạo UnityWebAssembly thật, start/pause hoạt động. Đã xem ảnh live. `Captures/release142-live-results.json`, release142-pages-status.json.
- **Bàn giao:** source/scene/code/assetmeta/cấu hình/QA/2tài liệu nằm trong release; commit bằng chứng và tài liệu riêng sau deploy (không đổi artifact đã kiểm). UnityEditMode/version1.42.0/khôngcompile/scenekhôngdirty/jobsfalse; serverlocalhost8765 đã dừng, không dùng Blender. Giữ art/Wave/.blend/Captures cục bộ ngoài phạm vi. Chưa thử Safari/điện thoại thật, không hứa60FPS mọi máy. Đóng tất cả tab/PWA game rồi mở lại để worker mới kích hoạt, không xóa Coin/kỷlục.


### 21.70. Tăng âm lượng động cơ người chơi70% — source đã xong, chưa build web (27/10/2026, Codex) — **đã lên web1.43.0/21.75**

- Người dùng yêu cầu tăng70% tiếng động cơ xe đua người chơi cả chạy thường và tăng tốc. `Assets/PhoViet/Scripts/PhoVietGame.cs`, `CreateSound`: `engine.volume=.08f*1.7f` = **0,136**, trước0,08. Hai chế độ cùng dùngAudioSource engine, boost chỉ làm đổi pitch theoSpeed nên hệ số1,7 áp dụng đồng đều; không cộng thêm lần hai khi boost. Không tăng master volume/âm va chạm0,3/còiCSGT0,12/chỉđường0,9, không đổi pitch/clip/mute/pause/gameplay/đồhọa.
- Kiểmtra: Unity recompile **0lỗi/0warning**; PlayMode đọc AudioSource thực: normalVolume**0,136**, boostVolume**0,136**, multiplier~**1,70000017** (sai sốfloat), TryBoost=true/correct=true. Lần eval đầu timeout trong lúc khởi tạo scene; chạy lại sau scene sẵn sàng đạt. Không thêm test suite cho thay đổi một tham số. Đã ResetRide/thoátPlayMode, UnityEditMode; jobs vẫn khóa.
- Trạng thái: chỉnh source và cập nhật cả hai tài liệu trong cùng công việc; chưa bump version/build/push/phát hành web. Web công khai vẫn **1.42.0/be0c6ff/Pages40445290840/cache d7e9bf2a636e37f7** với âm lượng cũ; muốn lên web phải build lại C# theo15.2, không refresh shell. Giữ mọi sửa đổi Art/Wave/Captures khác ngoài phạm vi.

### 21.71. Thêm iPhone 14 Pro / Pro Max vào Unity Device Simulator (27/10/2026, Codex)

- **Yêu cầu/hành vi:** thêm đúng hai mẫu để thử bố cục Dynamic Island khác tai thỏ iPhone13. Menu Simulator đã có **Apple iPhone 14 Pro** và **Apple iPhone 14 Pro Max**; chọn mẫu, dùng Rotate và Safe Area. Giữ gói Device Simulator Devices1.0.1 hiện có, không sửa manifest/lock (hai file đã có thay đổi trước yêu cầu).
- **Asset/triển khai:** `Assets/PhoViet/Editor/DeviceSimulator/` chứa `iPhone 14 Pro.device`, `iPhone 14 Pro Max.device`, `Overlays/iPhone 14 Pro.png`, `Overlays/iPhone 14 Pro Max.png`, meta Unity, `README.md` và giấy phép nguồn `LICENSE.md`. Asset nằm trong Editor để không đưa vào WebGL. Nguồn cấu hình/ảnh: https://github.com/irksome-dev/com.unity.device-simulator-Apple-iPhone-14-and-15-models tại revision **97636faf84d93d97a955dfaec33edb95680a8955**, phát triển từ Opodlinok; chỉ lấy hai mẫu đã yêu cầu. Đối chiếu schema Unity6000.5 và độ phân giải Apple (support.apple.com/en-us/111849, /111846). Không chạy mã từ nguồn tải xuống.
- **Tham số:** Pro1179×2556, Max1290×2796, DPI460, model iPhone15,2/iPhone15,3, OS iOS17.0. Safe Area4hướng: dọc thường `(0,102,1179,2277)` / `(0,102,1290,2517)`; dọc ngược y177; ngang trái/phải `(177,63,2202,1116)` / `(177,63,2442,1227)`. PNG Pro1339×2696 / Max1450×2936; căn border trái/phải80, trên/dưới70pixel cả hai (sửa border upstream không khớp kích thước thật). Chỉ giữ model/OS trong systemInfo, bỏ RAM/GPU/capabilities sao chép không đại diện máy thật. Overlay readable/alpha, không nén/mipmap/NPOT resize, max4096 để giữ hình/vùng chặn chạm. Dynamic Island là hình tĩnh, không giả lập hoạt ảnh iOS hoặc hiệu năng phần cứng/Safari.
- **Kiểm tra:** Unity đã import hai Device Info Asset (ScriptedImporter13973) và meta/overlay; menu xuất hiện cả hai. Đã xem trực tiếp **4trạng thái: Pro dọc/ngang, Pro Max dọc/ngang**, Dynamic Island và khung vàng Safe Area hiển thị đúng. Kiểm JSON2mẫu/8hướng nằm trong màn hình; kích thước overlay trừ border khớp chính xác màn hình; alpha tâm màn hình0/đảo255 đạt, kết quả `Captures/iphone14-simulator-checks.json`. Không thêm suite gameplay cho asset Editor. Unity CLI báo Pipeline server chưa chạy; dùng kỹ năng computer-use để kiểm tra trên Unity đang mở, không cài lại Pipeline hoặc mở Editor thứ hai.
- **Bàn giao/trạng thái:** Unity Edit Mode, đang chọn **Apple iPhone 14 Pro Max ngang**, Safe Area bật để người dùng thử ngay. Không sửa/lưu scene hoặc mã gameplay, không bật jobs (`RideJobsReleased=false`, `jobsEnabled=false`), không dùng Blender. Đã cập nhật BANGIAO/huongdan cùng công việc. Chưa commit/push, không bump/build/phát hành vì đây là công cụ thử Editor; web vẫn1.42.0 và việc21.70 vẫn chờ build riêng. Giữ các sửa đổi ngoài phạm vi (gồm PhoVietWelcome.cs có sẵn khi bắt đầu).


### 21.72. Cù Lao Petshop30%, bổ sung tên nhà ven sông (27/10/2026, Codex) — **đã lên web1.43.0/21.75**

- **Yêu cầu/hành vi:** biển chính **“Cù Lao Petshop” nền xanh dương, chữ trắng**, chiếm30% cửa hàng trong bộ khu phố. Các nhà ven sông có tên cả mặt đường và mặt kính hướng sông để nhìn thấy khi qua cầu; hai mặt cùng tên, đủ chữ ở **LOD0/1/2**. Nguyên nhân bảng trống cũ: `shop_unit` chỉ dựng chữ khi d==0; mặt hướng sông trước không có bảng. Giữ22tên đa dạng cho các cửa hàng ven sông còn lại (TẠP HÓA/XÔI/ỐC/CÀ PHÊ/NHÀ THUỐC…).
- **Tỷ lệ/bố trí:** 35tile×14=**490cửa hàng phố,147Petshop**; mỗi bên mỗi tile có2biển,7tile thêm1biển để đủ30%, rải seedổn định72026. Mỗi khối sông66gian: base19Petshop,28/35tile dùng1part thay thế thêm1gian (20Petshop),7tile còn19 → **693/2.310gian=30%**. Mỗi gian2mặt nên **1.386/4.620biển ven sông=30%**; cộng490biển chính phố thì1.533/5.110biển chính=30%. Tính theo pool cảnh đã author, không phải tung ngẫu nhiên lại mỗi frame; nhà/cầu streaming kế thừa mẫu tile hiện hành.
- **Blender/source:** `ArtSource/Blender/pho_viet_petshop_signs.py` chạy Blender5.2 **background riêng**, không dùng/sửa Blender GUI đang mở hoặc ghi các.blend cũ. Tạo12asset `Assets/PhoViet/Art/Streets/PV_StreetShop00..11_Petshop.{fbx,json}`: chỉ đổi biển chính, giữ mặt tiền/tầng/mái hiên/nội thất/va chạm của12kiểu gốc, font BeVietnamPro-SemiBold dựng geometry. Xanh dùng ôpalette `blue=2B66B0`, chữ `white=FBF8F0`, không tạo texture/material riêng. Nguồn mới `PhoViet_PetshopSigns.blend`.
- **River:** `pho_viet_river_bridge.py` giữ thứ tự random kiến trúc cũ; chỉ chọn brand bằng danh sách key riêng, thêm chữ tất cảLOD và bảng hậu. Bảng hậu ở y3,35m, rộng w−0,4/cao0,72m, nhô0,12m, chữ0,14m khỏi lưng nhà; nằm trong khoảng bờ0,30m, không lấn nước/che phần kính chính. Xuất lại **chỉ** `PV_RiverApartments.fbx/json` cùng GUID/names/slots; thêm `PV_RiverPetshopExtra.fbx/json` cho riêngPV_RiverApt02 (tên mesh `_PetshopExtra` tránh Blender thêm.001). `PV_PetshopSigns.json` lưu66key/tham số/catalô. Không xuất lại bridge/boat/palette/cây/vỉa hè.
- **Unity:** `Editor/PhoVietPetshopSignsUpgrade.cs`, entry `AgentScripts/InstallPetshopSigns.cs`; backupscene `.tools/PhoViet-before-petshop-signs.unity`. Kiểm đủasset trước sửa, chỉ thaysharedMesh của renderer sẵn có, ghiPrefab override/savescene; bóngLOD2 căn hộ dùng đúng biến thể. `PhoVietStreetUpgrade.ConfigureFactories` nhận variant theo seed3/10 cho lần tạo mới; sau quy trình dựng lại phố/cầu phải chạy installer này để cân bằng chính xác toànpool. Không rebuildscene, không tạoTextMesh/component Update/signrenderer mới; giữ **10.983renderer cảnh quan**,3LOD/các ngưỡng .45/.175 của21.69, mọi collider/chiều nhà tại mốc21.72 (**21.73 đã thay collider nội/ngoại thất Petshop**), vật liệu/AA4/shadow100/DPR/texture/phảnchiếu/ngàyđêm/các phương tiện. Chữ ban đêm chịu ánh sáng hiện hành như biển khác, không tự thêm đèn hoặc glow toàn phố.
- **Kiểmtra:** compile0lỗi/0warning; **VerifyPetshopSigns682 điều kiện đạt**, đọc MeshUtility.AcquireReadOnlyMeshData nên không bậtRead/Write: chữ/UV trắng/nềnxanh trong36LODshop,36boundsfacade so với gốc;66gian×3LOD cóglyph thật cả2mặt,3LODextra cóglyphtrắng;147/490 và693/2310 đúng; meshLOD/bóng cùngvariant; renderer/quality/jobs giữ. Lúc kiểm tra ban đầu đã sửa phép đổi trục X FBX (rawUnityX=authorX) thay vì bỏ kiểm tra màu. Installer chạy lại **scene byte-identical**, không sinh thêm objects. **PerformanceSafety1326+RiverBridge160 đạt** sau bản cuối; tổng **2.168điều kiện**. Đã xem9ảnh ngày/đêm, phố, mặtđường/mặtsông, LOD1/2 vàgamewide. Captures/petshop-sign-{checks,install,export,idempotence,visuals}.json +petshop-sign-regression-*.json/PNG.
- **Bàn giao:** UnityEditMode/sceneđãlưu/jobsfalse, Blenderbackgroundđãthoát; nguồn/assets/meta/scene/QA/cả2tài liệu cùng công việc, chỉ commit file thuộc21.72. **Chưa bump/build/push/phát hành web**; web vẫn1.42.0/be0c6ff/Pages40445290840/cached7e9bf2a636e37f7. Âmđộngcơ21.70 cũng vẫn chờ build. Giữ Simulator21.71/Packages/ArtWave/Captures và các thay đổi ngoài phạm vi; không stage/reset toàn bộ. TestVerifyStreetArt lịch sử còn giả định16asset được thay cho phạm vi này bằngVerifyPetshopSigns; không giảm đồhọa để ép test.


### 21.73. Petshop có phụ đề, chó/mèo/thỏ bằng Blender MCP; bỏ nội thất quán ăn (27/10/2026, Codex) — **đã lên web1.43.0/21.75**

- **Yêu cầu/hành vi:** biển xanh dương/chữ trắng “Cù Lao Petshop” có dòng nhỏ **“Lưu Trú - Spa Pet”** ngay dưới tên chính, ở cả phố thường và hai mặt nhà ven sông, đủ3LOD. Petshop không còn bàn/ghế, quầy thức ăn, tủ kính trưng bày/tủ nước, thực đơn, biển nghề phụ cũ; vẫn giữ kính mặt tiền để thấy thú cưng bên trong. Cửa hàng khác và tầng trên giữ nguyên chức năng. Tỷ lệ21.72 vẫn **147/490shop phố,693/2310gian river =30%**.
- **Blender MCP thực tế:** kiểm addon/scene trước; dựng và xem chó nâu tai cụp, mèo cam tai nhọn/đuôi cong, thỏ trắng tai hồng trong scene/collection riêng **PV_PetAnimals_Work**, giữ scene người dùng. Nguồn `ArtSource/Blender/pho_viet_pet_animals.py`, library `PhoViet_PetAnimals.blend`; xuất `Assets/PhoViet/Art/Pets/PV_PetDog`, `PV_PetCat`, `PV_PetRabbit` (FBX/JSON/meta) và `PV_PetGeometry.json` chứa hình học/màu/normal3LOD để ghép vào facade. Số triangleLOD0/1/2: **dog956/520/396,cat838/422/338,rabbit864/432/320**. Dùng palette phố chung; thân smooth qua tagPetSmooth, tai giữ cạnh; không tạo texture hoặc vật liệu riêng cho mỗi con. Không dùng hình chó mèo phẳng.
- **Bố trí:** `pho_viet_petshop_layout.py` ghép đúng geometry từ libraryMCP, scale/yaw/đặt chân sát sàn. Mỗi Petshop phố **9con** (5trong+4ngoài; mỗi loài có cả hai khu vực), có thảm ngủ/gói đồ thú cưng/bát ăn nhỏ. Mỗi gian Petshop river **9con** (3ngoài,3trong cửa mặt đường,3trong phòng kính nhìn sông), đủ chó/mèo/thỏ tại mỗi khu vực; không đặt ngoài mép bờ sông hẹp. **7.560instance đã ghép trong pool840cửa hàng**, không phải tất cả đang hiển thị cùng lúc. Đây là thú cưng trang trí tĩnh; không thêm AI/Update/collider cho từng con.
- **Generator:** `pho_viet_petshop_signs.py` dùng interior/sidewalkPetshop riêng, bỏ blade nghề khác, xuất lại12variantPetshop. `pho_viet_river_bridge.py`: mở riêng phần lõi tầng trệt của gian Petshop, mặt kính dùng slotGlass thực, bỏ sofa tầng trệt nếu giao gian Petshop; giữ RNG cho tầng trên/cửa hàng khác. Thêm phụđề ở cả2mặt; cập nhật riverbase+1partExtra, metadata pets theo part/LOD. `pho_viet_saigon_streets.py` chỉ thêm nhận tagPetSmooth trong to_mesh, không đổi shading hình học khác. Thú dựng qua liveMCP, facade export bằng Blenderbackground riêng; không ghi đè fileBlender hiện mở/palette/cầu/thuyền.
- **Unity/va chạm:** `Editor/PhoVietPetshopSignsUpgrade.cs` cập nhật metadataobstacles của147Petshop, tìm parent cả khi inactive (`GetComponentInParent<StreetObstacleSet>(true)`), bỏ ghostcollider đồ đã xóa. Scene hiện **770set/7.659hộp**, giảm1.231hộp; assert mọi obstacle set không thuộcPetshop không đổi. Giữ renderer cảnh quan **10.983**, LOD.45/.175/.006,3LOD, texture/DPR/AA4/shadow100/phảnchiếu/ngàyđêm và phương tiện. Scene backup `.tools/PhoViet-before-petshop-pets.unity`; installer chỉ cập nhật mesh/set có sẵn, không rebuildscene. Sau dựng lại phố dùng đúng `InstallPetshopSigns.Main`; không chạy installerphố/cầu lịch sử để sửa riêng Petshop.
- **Kiểm tra bản cuối:** `VerifyPetshopSigns` **682**, mới `VerifyPetshopDetails` **1.106** (mẫu vertex thực từ MCP trong mọiLOD, đủloài/vị trí, collider và tỷlệ), `VerifyPerformanceSafety` **1.326**, `VerifyRiverBridge` **160** → **3.274điều kiện đạt**. Test bảo toàn phần nhà so sánh vertex từcao8,1m (cao hơn biển nghề cũ đã chủ đích bỏ), không ép bound phụ kiện cũ. Đã xem9ảnh Unity phố/river2mặt/ngàyđêm/LOD1/2/gamewide; `CapturePetshopSigns` căn máy thấp hơn để thấy thú. Installer chạy lặp sceneSHA256 **D1C8AF5BC6ABFF16507E54E3CA01BE4E8C28DA7EC2CB0D9775DAC071197AC026** không đổi. Unity EditMode, scene đãlưu/khôngdirty,0missingScript,jobsfalse/RideJobsReleasedfalse; liveBlender giữ scene riêng các thú, backgroundexport đãthoát. QA: `Captures/pet-animals-blender.json`, `petshop-details-{checks,idempotence,final-editor}.json`, `petshop-details-regression-Verify*.json`, bộ `petshop-sign-*` hiện hành.
- **Bàn giao/phát hành:** source/assets/meta/scene/QA cùng `huongdan.md`/`BANGIAO.md` trong commit của công việc21.73; giữ nguyên Simulator21.71/Packages/ArtWave và Captures ngoài phạm vi. **Chưa bump/build/push/phát hành web**; public vẫn **1.42.0/be0c6ff/Pages40445290840/cached7e9bf2a636e37f7**. Khi người dùng yêu cầu phát hành, cần buildC#/data đầy đủ gồm âmđộngcơ21.70+bảnghiệu21.72+Petshop21.73, không chỉ refreshwebshell.


### 21.74. Bảng hiệu tiệm kề nhau khác màu, dành xanh dương cho Petshop (27/10/2026, Codex) — **đã lên web1.43.0/21.75**

- **Hành vi:** sửa trường hợp TIỆM BÁNH và Cù Lao Petshop cùng biển xanh như ảnh người dùng. 12mẫu tiệm thường có12màu riêng, biển tên chính/biển dọc/bảng tên quầy cùng phối màu của mẫu; chữ trắng hoặc đậm theo nền. Màu xanh dương `#2B66B0` dành cố định cho Cù Lao Petshop (không dùng cả navy/awnBlue cho nền tên tiệm thường). Tất cả biển kề nhau trong từng dãy phố hoặc ven sông đều khác màu. Petshop giữ phụđề/9thúcưng21.73 và đúng30% (147/490phố,693/2310gian river).

| Mẫu tiệm thường | Màu bảng trong atlas hiện có |
| --- | --- |
| PHỞ BÒ - GÀ | `white` / `#FBF8F0` |
| TIỆM TRÀ | `green` / `#1E6A4A` |
| BÁNH MÌ | `orange` / `#F08A24` |
| TẠP HÓA | `red` / `#D6312B` |
| THUỐC TÂY | `purple` / `#9B4BA0` |
| SỬA XE | `chalk` / `#2A2E2B` |
| CÀ PHÊ SÀI GÒN | `woodDark` / `#5C3A22` |
| CƠM TẤM | `yellow` / `#FFD23F` |
| BÚN BÒ HUẾ | `greenDark` / `#134A33` |
| HỦ TIẾU NAM VANG | `peach` / `#F2A487` |
| SINH TỐ | `pink` / `#E24C8E` |
| TIỆM BÁNH | `ironGrey` / `#4A4F55` |

- **Generator/màu:** `pho_viet_saigon_streets.py` thêm `SHOP_BOARDS`, gán board/ink/subink cho12spec, đồng bộ màu biển dọc; không đổi RGB/ô UV của atlas128px để tránh đổi màu cảnh/xe/lá/thú. `pho_viet_petshop_signs.py` xuất có chọn lọc12shopgốc+12variantPetshop và riverbase/extra vào source riêng `PhoViet_PetshopSigns.blend`, không ghi đè `PhoViet_SaigonStreets.blend` cục bộ. Metadata thêmboard/ink, báo cáo `PV_PetshopSigns.json` thêm bảngUV/hex và kếtquảshopgốc. River dùng chuỗi12màu theo thứ tự gian đã sắp bằng tọa độ; cả2mặt/cả3LOD cùngmàu. Giữ chính xác RNGdraw7lựa chọn cũ trước khi override màu, nên không vô tình đổi tên cửa hàng/cửa cuốn/mái hiên/tầng trên chỉ vì số màu tăng. `shop_signs` trongJSON từngpart mô tả từng gian để kiểm UV thực.
- **Tránh xanh liền nhau:** `petshop_order` ưu tiên20gian không chung cạnh, rồi mới ghi các gian còn lại; base19,extra20 giữ cách cân bằng28/35tile. ExtraPart hiện là `PV_RiverApt09`; luôn lấy từJSON, không hardcode chỉ số cũ02. Installer chọn tổ hợp2hoặc3Petshop không kề nhau trong mỗi nửa dãy7shop, ưu tiên gần thứ tựshuffle cũ; vẫn tổng147. `BrandedSeed` cho tạo mới dùng vị trí0/3/6 trong chu kỳ10, sau khi dựng lại vẫn phải chạy installer cân bằngtoànpool.
- **Unity/scene:** `PhoVietPetshopSignsUpgrade.cs` cập nhật đủ490bộobstacle shop theo metadata của mesh hiện dùng (cần phục hồi đồ tiệm thường khi gian không còn làPetshop); các set ngoài shop giữ nguyên. Khi extraPart đổi, đi qua toàn bộ24part căn hộ để phục hồi phầnextra cũ vềbase và gán phần mới, gồm3LOD+shadowproxy; xử lýmeshnull sauFBX đổi tên, loại tiền tố “Bóng •” khi tra tênfallback. Backup `.tools/PhoViet-before-sign-colours.unity`. Scene cuối770set/**7.670hộp**,10.983renderer. Không rebuildscene/khôngthêmUpdate/renderobject/material/texture riêng, giữDPR/AA4/LOD/bóng/phảnchiếu/ánhsáng/xe/côngviệcđangkhóa.
- **Kiểm tra bản cuối:** `VerifySignColours` mới **12.849điều kiện**: đọc UV thực mặt bảng từngFBX, màu biển dọc, hai mặt river trong mọiLOD, kiểmtoànpool **420cặp phố+2.170cặp river=2.590cặp kề** khácmàu, đúngtỷlệ/cácmesh+shadowkhôngmissing. Hồi quy `VerifyPetshopSigns`682 + `VerifyPetshopDetails`1.111 + `VerifyPerformanceSafety`1.326 + `VerifyRiverBridge`160 → **16.128điều kiện đạt**. So sánhJSON12shopgốc vớiHEAD21.73: sốtriangle3LOD và toàn bộobstacle metadata không đổi. Compile0lỗi/0warning; đã xem ảnh ngày/đêm/river2mặt/LODxa và ảnh trực tiếp cặp **TIỆM BÁNH–Petshop** (`petshop-sign-street-neighbours.png`); tổng10ảnh. Installer chạy lặp byte-identical; UnityEditMode/scenesạch/0missingScript/jobsfalse. LiveBlender giữ nguyên scene người dùng, các background export đãthoát.
- **QA/bàn giao:** `AgentScripts/VerifySignColours.cs`, `Captures/sign-colours-{checks,idempotence,final-editor,geometry-preserved}.json`, `sign-colours-regression-Verify*.json`, các `petshop-sign-*`/`petshop-details-checks.json` hiện phản ánh21.74. Lưu source/assets/scene/QA cùng2tài liệu; giữ Simulator/Packages/ArtWave/Captures ngoài phạm vi. **Chưa bump/build/push/phát hành**; bản web ghi nhận vẫn1.42.0. Lần phát hành tiếp cần build đầy đủ gồm21.70 và21.72–21.74, không chỉ refreshshell.


### 21.75. WebGL1.43.0 — đã phát hành (27/10/2026, Codex)

- **Ủy quyền/phạm vi:** người dùng yêu cầu trực tiếp build và phát hành, gồm commit/push main/Pages. Bản này đưa lên web âmđộngcơ+70%21.70, Petshop30%21.72, phụđề/thúcưng/nội thất21.73, màu biển khôngtrùng21.74 (source399980f). Giữ luậtchơi, jobsfalse/RideJobsReleasedfalse, độnét/mesh/texture/DPR/AA4/LOD/bóng/phảnchiếu; khôngchạyinstaller hoặc dùngBlender khi build. Simulator21.71/Packages/ArtWave/Captures ngoài phạm vi giữ cục bộ.
- **Version/preflight:** đồng bộ1.43.0 trong PhoVietWebBuild/ConfigureWebRelease/VerifyWeb/VerifyFullscreenGame/VerifyWebBundle/README/ProjectSettings. `VerifyRelease143Scene`20điều kiệnđạt/404dependencies, không artFBX/texture/material/scene ngoài commit nằmtrongdeps. Fingerprint2.905file source/config trước/sau bản cuối không đổi; không rebuildcảnh.
- **Sửa lỗi phát hiện khi kiểm web:** bảnđầu build_02a1a252b3fe build thànhcôngnhưngbrowser báoOOM/memoryaccess ở trần512MB, UI cònkhung nhưng thiếu chữ; khôngpháthành bảnlỗi. Tăng `PlayerSettings.WebGL.maximumMemorySize`512→1024MB trongConfigure, giữinitial128MB/growthgeometric20%/cap96MB. Đây làtrần choheap tăng theo nhu cầu, khôngcấp sẵn1GB và khônggiảmđồhọa. `VerifyRelease143Memory.cjs` chạydesktop+phone23s saukhởitạo, nhảy/boost/pausetớiUI, 6mẫu heap ổn định **577.437.696byte≈550,69MiB**,0OOM/0runtime; ảnh giao diện đầyđủ. Bằngchứng banđầu: release143-initial-memory-failure.json/initial-build-result.json.
- **Build cuối:** `build_c44154138dd2` Succeeded/**321,196s/0lỗi/1warning** PipelinekhôngbậttrongPlayer (nhưcũ). Payload **129.817.344byte**, cache **1b0d45bcf76b698e**, WebGLgzipfallback/singlethread/Safari-compatibleWASM giữ nguyên. Data **122.496.891byte** chia4phần32MiB, manifestSHA; cácphần/ghép lại/cache chínhxác. `PublishWebArtifact.py pack`→docs, `materialize`→`.tools/site143`; bản512MB giữriêng `.tools/site143-initial512`.
- **Kiểm tra trước push:** **34repo bundle +29site ghép +37browser desktop/offline/touch/rotate/installability +20fullscreen iPhone/iPad/desktop** đạt,0lỗi runtime. Đãxem ảnhdesktop/mobile dọc/ngang, chữUI vàthúcưng/biểnhiệu; giữDPR/khungđầyđủ. Đãkiểm6mẫu heap riêng nhưtrên. **31kiểm stagedbytes/SHA/cache đạt** trướccommit, khôngstage data nguyênvẹn>100MB. Bộkiểm16.128điềukiện nguồn21.74 đãđạttrước khi build; không tựnhận là đãđoSafari/điệnthoạithật.
- **Phát hành hoàn tất:** commit **7b19e84556672ec8d90114e6cf90274ad31ba2bc** đãpushmain, **Pages40487519674 success**, version**1.43.0**/cache**1b0d45bcf76b698e**. `VerifyPublishedGame.cjs` **26livechecks đạt/0runtime**: mọi runtimefile/worker/precache khớpbyte artifactđãkiểm, UnityWebAssembly thật khởitạo/start/pauseđược; đãxemảnhlive. 21.70/21.72–21.74 không còn chờpháthành. Khiworker cũcònhoạtđộng, đónghết các tab/PWA game rồi mởlại, khôngxóaCoin/kỷlục.
- **QA/bàn giao:** `release143-{build-result,scene-checks,source-fingerprint,repo-bundle-checks,site-bundle-checks,local-web-results,memory-checks,staged-artifact-checks,pages-status,live-results,final-editor}.json`, `fullscreen-game-release143-local-results.json`, ảnhdesktop/mobile/live; mãkiểm `VerifyRelease143Scene.cs`/Memory.cjs. Commitbằngchứng/2docs sau deploy khôngđổi artifact. Server8765 đãdừng; không dùngBlender trongcôngviệc, cácscene/asset ngoài phạmvi khôngbịreset.
- **Lịch sử21.75, đã được làm rõ và xử lý ở21.76:** người dùng xác nhận đây là thao tác tự kéo điểm xuất phát và đã Save. Hiện scene đã chuyển độ lệch sang gốc xe, giữ đúng vị trí thật; ghi nhận lúc hoàn tất21.75: UnityEditMode/khôngcompile/jobsfalse, nhưng liveEditor có **1chỉnhsửa chưa lưu** so với sceneđĩa: prefabmodelBlueSupercar.m_LocalPosition.z0→4,39 (toàn bộ1.088.218dòng chỉ1dòngkhác; lưu bảncopy `.tools/release143-postbuild-probe.unity`). Không xác định do thao tácEditor/previewnào; không tựsave/reset đểtránh mất sửađổi. Sceneđĩa/sourcefingerprint/artifact phát hànhkhôngđổi. Đãghi ởBANGIAOmục3 đểagent sau kiểmtratrước khi mutation. Tácvụdiffchẩnđoán đãdừng đúngPID của phiên, khôngđụng tiếntrìnhkhác.

### 21.76. Tự kéo thả và lưu điểm xuất phát xe đua trong Unity (27/10/2026, Codex — đã lênweb1.44.0/21.77)

- **Yêu cầu đã xác nhận:** người dùng muốn tự kéo thả xe để chọn điểm xuất phát thật. Vị trí model Z=4,39 là chỉnh sửa có chủ ý đã Save, không phải thay đổi lạ sau build. Trước sửa, `ResetRide` ép gốc xe về (LaneX[2],0,0), `PlayerCarVisual.Advance` ép skin local0 mỗi khung nên vị trí model lưu ở Edit bị xóa khi Play; kéo model lúc Play cũng bị kéo lại. Ảnh mới đã chọn đúng Move, nguyên nhân chính là mã ghi đè.
- **Cách dùng:** Edit Mode → **Phố Việt / Điểm xuất phát / Chọn xe để kéo vị trí** (chọn đúng gốc xe, bật Move và focus) → kéo bằng gizmo → **Ctrl+S**. Click xe trong Scene cũng ưu tiên gốc nhờ `SelectionBase`. Khi Play còn **Ready/màn hình chờ trước khi bắt đầu chạy**, kéo gốc xe hoặc root model đều đổi điểm xuất phát; thoát Play sẽ chuyển vị trí này về Edit Mode, Scene đánh dấu bẩn, **Ctrl+S** để lưu. Không mở kéo tự do khi đang đua/té/rẽ: đó là Transform do gameplay điều khiển, không phải lúc đặt điểm xuất phát. Không tự Save mọi thay đổi khác trong Play.
- **Runtime:** mới `Scripts/PhoVietPlayerStart.cs` thuộc partial `PhoVietGame`. Lần reset đầu chụp vị trí xe thực trong hệ game, nhận cả offset FBX cũ và chuyển hình về gốc vật lý; giữ điểm xuất phát trong suốt lượt/reset. Mỗi reset khôi phục vị trí gốc game rồi dịch khung dọc đường tới Z đã chọn, player dùng localz0 để giữ quy ước vật lý/xe/giao lộ cũ; không cộng dồn offset. Chọn làn gần X ban đầu nhưng giữ nguyên X thật khi Ready; bắt đầu chạy thì đổi làn mượt như cũ. Y khác0 giữ đúng ở Ready, khi chạy dùng rơi/tiếp đất trên mặt đường/mái xe thật, không đổi cao độ mặt đất. `PhoVietGame.cs` dùng Restore/Place thay tọa độ cố định; `PhoVietRoutes.cs` đồng bộ pha khối, đường ngang, roadOffset và lịch giao lộ; `PhoVietJobs.cs` chỉ đồng bộ chỉ số navNext theo phố xuất phát, **RideJobsReleased=false/jobsEnabled=false vẫn khóa**.
- **Editor:** mới `Editor/PhoVietPlayerStartEditor.cs`: Undo.postprocessModifications chỉ nhận chỉnh vị trí Transform của gốc xe hoặc root skin, chuyển hình và collision root cùng nhau, ghi prefab modifications và đánh dấu Scene dirty. Khi Play Ready dùng API SetPlayerStartFromEditor, cập nhật SessionState theo scene; EnteredEditMode áp lại điểm vừa kéo, không mất khi thoát Play/domain reload thông thường. `PlayerCarVisual.cs` thêm SelectionBase; hoạt ảnh/thông số xe, collider/mesh/texture/LOD/bóng/nhảy/húc/boost giữ nguyên.
- **Scene đã lưu:** `Assets/PhoViet/Scenes/PhoViet.unity`: gốc `Tài xế • áo đỏ sao vàng` (tên lịch sử, nay là xe đua) world/local **(1,55;0;4,39)**; `Blue Supercar • xe người chơi` local **(0;0;0)**. So với HEAD chỉ1dòng thay đổi vị trí gốc; offset model cũ được chuyển sang gốc mà vị trí nhìn thấy không đổi. Đã Save và mở lại file từ đĩa, rồi Play/reset xác nhận cùng tọa độ. Nguồn/artifact web1.43.0 trước đó không bị thay đổi.
- **Kiểm tra:** **686 điều kiện đạt**: `VerifyPlayerStart`66 (xyz, khởi động/lặp reset, không cộng dồn, cầu thật với spawn4,39/145, sáu lượt rẽ trái/phải tại4,39/145/−24); `VerifyPlayerStartAuthoring`13 (kéo skin sáu hướng bằng Undo pipeline, gốc/skin đồng bộ và Scene dirty); `VerifySupercarScalePace`182, `VerifySupercarPlayer`76, `VerifySupercarTurns`32, `VerifySpeedJump`157; `VerifyRiverBridge`160. Lần đầu bộ RiverBridge trượt do fixture tìm tile chính xác tại80 trong khi spawn4,39 làm tile ở75,61; đã sửa **fixture** để lưu/đặt start chuẩn0 rồi restore trong finally, chạy lại160đạt; kiểm cầu với spawn tùy chỉnh nằm riêng trong66điều kiện, không nới điều kiện/đổi luật gameplay.
- **Thử trực tiếp:** đã kéo gizmo thật bằng chuột trong Play Ready: X1,55→2,97 và giữ nguyên sau nhiều khung; sau thử trả lại đúng điểm người dùng `(1,55;0;4,39)`. Đã nhìn Game/Scene, Save→reload→Play→Reset không đổi vị trí, skin/collider trùng và jobs khóa. Bằng chứng `Captures/player-start-{checks,authoring-checks,gizmo-play,save-reload-play}.json`; script kiểm ở `AgentScripts/VerifyPlayerStart.cs` và `VerifyPlayerStartAuthoring.cs`, cập nhật fixture `VerifyRiverBridge.cs`. Unity đã compile, không có lỗi; đã bỏ các API obsolete mới phát sinh trong helper Editor. Diff whitespace của file sửa đạt; asset/meta được import.
- **Trạng thái bàn giao:** Unity Edit Mode/scene sạch, xe ở điểm người dùng đã đặt; Blender không dùng, không dựng lại cảnh/asset hoặc reset thay đổi Art/Wave/Packages/Welcome ngoài phạm vi. Ởmốc21.76 chỉsource/scene cục bộ; sauđó đãbuild/commit/push/pháthành cùng1.44.0/21.77, giữ điểm người dùng đặt mới Z=1,92. Hai tài liệu đã cập nhật, không còn ghi chú chưa rõ nguồn gốc về modelZ4,39.


### 21.77. Sửa tải/thoát game Safari iPhone13 Pro Max — WebGL1.44.0 (27/10/2026, Codex — đãpháthành vàkiểmlive đạt)

- **Yêu cầu/thiết bị:** người dùng gặp Safari báo “Đã có sự cố xảy ra liên tục” trước vào game và giữa lượt chơi, iPhone13 Pro Max; chưa biết iOS. Đây là crash tiến trình trang, ảnh không phải thông báo giới hạn kho GitHub. Không kết luận chỉ do kích thước thư mục dự án; đo dữ liệu tải/heap/GPU/cache thực tế.
- **Bằng chứng gốc:**1.43.0 tải129.817.344byte, WASM heap577.437.696byte=550,69MiB. Báo cáo build: Texture2D276,48MiB/Mesh170,61MiB (bảnmớiTexture2D144,30MiB, giảm47,8%, Meshgiữ170,61MiB);9maps xe/người RGB(A)32 chiếm~169MiB, DXT còn lại không phù hợp iPhone. WebData giải nén147,37MiB nhưng serializedassets448MiB. Worker cũ cache.addAll runtime ở landing có thể chồng tảiUnity. Không xóa model, giảm độ phân giải, DPR, mipmap, AA4, bóng100m hay khoảng dựng cảnh.
- **Texture theo GPU:** mới `Editor/PhoVietWebMemorySettings.cs` chỉ sửa44texture dependency diện tích>=65.536px; mobileETC2_RGB4/ETC2_RGBA8, mask2kênh UrbanRegions EAC_RG; desktopDXT1/DXT5. Giữ nguồn/source,2K và các thông số lọc/sRGB/mip cũ. Import đồng bộ/WriteImportSettingsIfDirty để không còn metadata stale. Giữ globalGeneric/NoOverride, không ép reimport asset lịch sử. ChromeIntelUHD620 thực không cóETC2, nên cần2data; app probeWebGL2+extensionETC rồi nhả context, chỉ tải variant hỗ trợ. CóGPUETC dùngmobile, còn lạiDXT.
- **Heap/build:** `PhoVietWebBuild`1.44.0 initial128MiB/max512MiB/growth10%/cap32MiB. Build() tự build2format, so mã chung sau giải nén (gzip header có thể khác), gộpdata rồi refresh shell/hash; không build lại scene. `MergeWebTextureVariants.py` cho lần phát hành này xác nhận WASM giải nén giốnghệt, framework chỉ khác2hằng growth đãchốt ởlầnmobile trước; dùng frameworkdesktop mới. Mobilebuild4160ea1791f4 thànhcông531.970ms/0lỗi, desktop3b0eaa6f9df2 thànhcông308.860ms/0lỗi; warning Pipeline thiếuRuntimePipelineConfig đãbiết. Dữ liệu mobile85.386.369byte, desktop100.774.556byte; mỗi máy tải92.707.789 hoặc108.095.976byte, không tải cả2. Cachedb471371e548dd93; tổngsite193.482.345byte không phải lượt tải1máy.
- **PWA/cache:** `app.js` đợi SW điều khiển trước tảiUnity với timeout, đăng kýSW ngay; `sw.js` chỉinstall shell nhỏ, cache runtime streamtheorequest, trạng thái offline xác nhận đủ mãchung+1data. Ghi manifest vào cache cho workerrestart, event.waitUntil đăng ký đồng bộ, không tạo lại cache cũ sauactivate. KhôngskipWaiting giữa lượt. Coin/kỷlục/localStorage giữ nguyên.
- **Xe kéo thả21.76:** gồm trong binary mới; người dùng sau đó tự lưu gốc xe `(1,55;0;1,92)`, là vị trí mới nhất thay4,39 của kiểmtra21.76. Giữ đúng sceneđĩa/không reset. `RideJobsReleased=false`, scenejobsfalse,10.983renderer/7.670box giữ nguyên.
- **Kiểm tra đang có:**99preflight mỗi cấu hình (texture/resolution/AA4/bóng/scene/spawn/jobs/0missing);33bundle gộp đạt. Cachefixture15điều kiện đạt: không preloadruntime, tải hỏng/thử lại, không trùngtải, chỉ1variant, mởoffline sau workerrestart, cậpnhật giữtabcũ/dọncachecũ. Kiểmtra async Playwright dùng poll evaluate (waitForFunctionasync trong môi trường này không chờ đúng). Scripts `AuditWebMemory.cs`, `VerifyRelease144Scene.cs`, `ConfigureDesktopWebMemory.cs`, `VerifyWebCache.cjs`, `VerifyWebMemory.cjs`, `VerifyWebBundle.py` và báo cáo `Captures/release144-*`, `web-memory-*`, `web-cache-memory-checks.json`.
- **Đã đo browser:** desktopGPUthậtDXT vàprofileiPhone13ProMax/SwiftshaderETC đều peak417.202.176byte=397,875MiB (giảm27,75%so577.437.696byte), không tăng qua lượt lặp;2phút trởlên mỗi profile, cónhảy/boost/húc/xoayDPR3/pause/mởoffline sauworkerrestart,0runtime/0crash. 18điềukiệnmemory đạt; đây không phải toànRAMSafari/GPU hoặcFPSthiếtbịthật. Scene99preflighttrướcbuild/101cuối cóAA4/shadow100/growth. Bundle42repo/33materialized,2datachia7parts32MiB ghépSHA/cache đạt. Đãxemảnhphone/desktop. 37web/touch/offline +20fullscreen +34index-bytechecks đạt, `git diff --cached --check` sạch. `VerifyStagedWeb.py` đối chiếu blob/parts trongGitindex với artifactđãkiểm. Đãcommit **b0a31f870bdeaba04b3b4d1378f1f4da01bc523c**, pushmain; **Pages40535422845success**. **27livechecksđạt/0runtime** (`Captures/release144-live-results.json`), mọi tài nguyênbao gồm2data/WASM/shell/SW khớpbyte artifactđãkiểm, gamekhởitạo/start/pauseđược; đãxemảnhlive. Chưa thửSafari trên iPhonevật lý; profileChromium/Swiftshader không thaythế máy thật. Bảncũ trướcpháthành là1.43.0; hiệnlive1.44.0.

- **Trạng thái sau triển khai:** Pages40535422845success/commitb0a31f8/cache db471371e548dd93. **27livechecksđạt/0runtime**: version/cache vàmọi file khớpbyte, WASMkhởitạo/start/pauseđược; ảnhliveđãxem. Khôngcòn phần sửa lỗi/pháthành chờxửlý; cần người dùngthửlại trên iPhone13ProMaxthật đểxácnhận triệu chứng hết trên máy đó. Editor xácnhậnplayFalse/dirtyFalse/start(1,55;0;1,92)/jobsFalse/NoOverride; reportrelease144-final-editor.json. Đãdừng đúng4server kiểmtra8767/8768/8769/8770, giữcác thayđổiArt/Wave/Packages/Simulator/Welcome ngoài phạm vi.


### 21.78. Bánh răng và cài đặt đồ họa trên màn hình chờ (28/10/2026, Codex — đã lên web1.45.0 ở21.79)

- **Yêu cầu/hành vi:** thay icon con đường được khoanh trên thẻ “LÁI QUA PHỐ QUEN” bằng bánh răng xanh đậm. Bấm mở cửa sổ trắng trong “CÀI ĐẶT ĐỒ HỌA”, có3lựa chọn **Cao / Trung bình / Thấp**, đánh dấu mức hiện tại; thay đổi áp dụng ngay, bấm **XONG**, × hoặc bên ngoài để đóng. Chỉ mở ở Ready; áp dụng desktop và mobile dọc/ngang, Safe Area. Không thêm nút vào HUD đang chạy.
- **Mặc định/lưu:** `PlayerPrefs["PhoViet.GraphicsQuality.v1"]` lưu0/1/2; chưa có key hoặc dữ liệu sai → Cao. Không thay Coin/kỷ lục. `ResetRide` đóng modal nhưng giữ mức đã chọn. Cao là **snapshot chất lượng thật của nền tảng**, không hardcode lại mọi thiết bị về desktop. Native/Editor lấy sau Awake setup; WebGL lấy sau `ConfigureBrowserGraphics` đã áp cấu hình mobile cũ (AA4/bóng45m/Low/cascade0), vì vậy chọn Cao giữ đúng bản đang chơi trước đây. Đổi nhiều lần không nhân dồn hệ số. Thoát Play khôi phục cấu hình Editor, không lưu giảm chất lượng vào ProjectSettings.
- **Preset cụ thể (so với snapshot Cao):**

| Thông số | Cao | Trung bình | Thấp |
| --- | --- | --- | --- |
| MSAA | Gốc | min(gốc,2) | 0 |
| Bóng | Gốc | HardOnly nếu gốc có bóng, xa×0,65 | Tắt, xa0 |
| Độ phân giải bóng/cascade | Gốc | Hạ1mức, cascade0 | Hạ1mức, cascade0 (bóng đã tắt) |
| lodBias | Gốc | ×0,75 | ×0,5 |
| Phản chiếu sông scale/kích thước | Gốc | scale×0,75; trần min(gốc,512px) | scale×0,5; trần min(gốc,384px) |
| Phản chiếu chu kỳ/far | Gốc | interval≥2; far≤100m | interval≥3; far≤75m |

- **Độ nét/phạm vi:** giữ DPR3/cap3,2Mpixel, texture nguồn/mipmap/filter/GPUvariant, camera/FOV/tầm nhìn chính, geometry và số cảnh vật/xe, lighting/ngày đêm và gameplay. Chỉ Medium/Low người chơi chủ động chọn mới giảm các chi phí trên; các cam kết chất lượng mặc định ở21.69/21.77 vẫn là Cao. Heap128/512MiB/growth10%/cap32MiB, cơ chế nénETC/DXT và cache web không đổi. Không tuyên bố đo FPS trên điện thoại thật.
- **Triển khai:** partial mới `Assets/PhoViet/Scripts/PhoVietGraphicsSettings.cs` +meta, cấu hình/prefs/modal/hit-test/icon8răng96px procedural có anti-alias (không phụ thuộc ký tự font). `PhoVietWelcome.cs` bỏ motif đường, nút44×44logic/icon24×24. Modal432×440dọc/560×440ngang trong tọa độ GUI, thẻ3lựa chọn64cao, Safe Area dùng cùng vùng GUI. `PhoVietGame.cs` khởi tạo/đóng khi reset/cleanup, vẽ modal cuối, khóa GUI nền và StartRide, chặn Enter/Space/P/Esc xuyên modal; `PhoVietWebRuntime.cs` snapshot sau setup chất lượng trình duyệt. `WelcomeTap` xử lý modal trước nút bắt đầu; sử dụng cầu nối `ApplyHudTap` có sẵn, không tạo double-click WebGL. Esc/P/Enter đóng, phím1/2/3 chọn; chuột/chạm đều dùng cùng rect.
- **Kiểm tra:** compile Unity0lỗi/0warning. `VerifyGraphicsSettings.cs` **85điều kiện đạt**:6lần đổi preset, khôi phục đúng Cao, lưu/reload/fallback, tham số thật, không đổi texture/camera/sốrenderer/jobs,4kích thước, các packet `ApplyHudTap` thật với/có Safe Area, khóaStart và không tap xuyên, reset và chỉReady. `VerifyGestures` **39đạt**; `VerifyDistanceMode` **31đạt**; tổng **155**. Fixture distance cũ chưa tắt khủng long/xe tăng/sông mới, nên test60giây bị dừng ởframe428 do hazard; bổ sung lưu/tắt/khôi phục3cờ trong test để cô lập jobs, không sửa gameplay hay bỏ kiểm tra crash/kỷ lục. `Captures/graphics-settings-checks.json`, `graphics-settings-regression.json` lưu kết quả.
- **Ảnh/thao tác thật:** `CaptureGraphicsSettings.cs` chụp6ảnh540×960,960×540,1440×900 (`graphics-settings-*-gear/window.png`), đã xem cả cửa sổ dọc/ngang/desktop không cắt chữ. Computer Use bấm gear→Medium→XONG đúng; đọc lại Medium/AA2/bóng65m, thoátPlay→EditorAA4/bóng100/LOD2, Play mới tựnạpMedium; Enter đóng ởReady không start. Sau thử trảHigh. CLI eval đầu tiên ngay sau vàoPlay bị timeout5s, đọc lại sau khởi tạo đạt; đây là lỗi probe, không lỗi compile/game. Chưa build WebGL hoặc thử menu trên Safari vật lý; packet browser đã kiểm trong Unity.
- **Bàn giao ở mốc source21.78 (sau đó đã phát hành ở21.79):** Unity Edit Mode/khôngcompile/scene sạch; gốc xe đã lưu `(1,55;0;1,92)` giữ nguyên, không save/rebuild/importScene. Jobsfalse/RideJobsReleasedfalse. Không dùng Blender; cửa sổ Unity về kích thước ban đầu. Đã cập nhật BANGIAO cùng công việc, chỉ commit cục bộ các file21.78; ở mốc này chưa bumpversion/build/push/phát hành; sau đó đã lên web1.45.0/21.79. Giữ nguyên Art/Wave/Packages/Simulator/Captures lịch sử ngoài phạm vi.


### 21.79. Build/phát hành cài đặt đồ họa — WebGL1.45.0 (28/10/2026, Codex — đã phát hành và kiểm live đạt)

- Người dùng yêu cầu trực tiếp “build và phát hành web” sau21.78/commitb66bb6c. Được phép build/commit/pushmain/Pages và kiểm tra live trong cùng công việc; không cần hỏi lại.
- Đồng bộ1.45.0 trong `PhoVietWebBuild.cs`, `ConfigureWebRelease.cs`, kỳ vọng `VerifyWeb.cjs`/`VerifyFullscreenGame.cjs`/`VerifyWebBundle.py`, README vàPlayerSettings. Giữ ETC2/EACmobile +DXTdesktop, heap128/512MiB/growth10%/cap32MiB, texture/DPR, defaultCao, jobsfalse và sceneđãlưu. Không đưaArt/Wave/Packages/Simulator/Captures lịch sử vào commit.
- **Mốc trước build:**101điều kiện `VerifyRelease145Scene.cs` đạt (scene sạch, spawn1,55/0/1,92, geometry/collider/texture/bộnhớ/chấtlượng/jobs/missing scripts); source21.78 đã155kiểmtra. Unity cần focus/Ctrl+R đểPipeline7800 chạy; Configure texture vượt timeoutCLI120s nhưngEditor tiếp tục và hoàn tất, preflight sauđó đạt. Không chạy lại mutation khi import còn chạy.
- **Mốc build ban đầu (đã hoàn tất bên dưới):** `BuildWebRelease.cs` dùngEditor.delayCall để chạy `PhoVietWebBuild.Build()` đủ2variant và báo kết quả thật tại `Captures/release145-build-report.json`. Không coi lệnhqueued làthànhcông. Đangchuẩnbị `VerifyGraphicsWeb.cjs`: mởWASMthật desktop/mobile, click/touchgear/options, kiểmradio hiệnra bằngpixelảnh, tải lại giữMedium, xoayngang/dọc giữLow, chặnstartxuyênmodal. Bảnđanglive vẫn1.44.0 cho tới khiPages/live xácminh.

- **Sửa pipeline phát hiện trong lần build này:** Unity giữ lại filedata1.44.0 trong output làm `Single()` dừng sau mobile thànhcông. `PhoVietWebBuild.BuildSingle` nay chỉ giữ4URLruntime lấy từgeneratedindex trong đúng `Builds/WebGL/Build`, xác minh path/tên file trước khi xóa filecũ, rồi tạo lại cache; `CompleteTextureVariants` tách riêng để phục hồi sau mobile. Không xóa cả thư mục dự án, không sửascene. Ngoài ra importer cache làm4map desktop cònETC dù vòngApply đầu báođãđổi; `PhoVietWebMemorySettings.Apply` đọc lại tốiđa3pass, SaveAssets/refresh vàvalidate toànbộ địnhdạng trướcbuild. Đãbuildlạiriêngdesktop sau101preflightổnđịnh, final101preflightđạt và cácmetaGPUtrởđúngbaselineDXT.
- **Artifact cuối:** mobileETC/EAC85.387.558byte, desktopDXT100.775.498byte; code/frameworkgiảinénkhớp; cache **c0f81245589b65f2**, version1.45.0. Mỗi máy tải92.713.771hoặc108.101.711byte, không tải2data. Buildmobile453.679ms/0lỗi/3warning, desktopcuối120.801ms/0lỗi/1warning. Desktopthửđầu chưaổnđịnh đãloại, khôngpháthành. `PublishWebArtifact.py`7parts32MiB, **42repo/33materializedchecks đạt**, giữ216file runtime/scene/template/settings khôngđổi trongbuild. Bảnkiểmtra từ `.tools/site145` ghép đúngnhưCI; server127.0.0.1:8771.
- **Kiểm tra đồ họa trên WASM cuối:** `VerifyGraphicsWeb.cjs` **26điều kiện đạt/0runtime**, ChromeGPU desktop +profileiPhone dọc/ngangDPR1: click/touchgear,3mức, APIStart/Spacekhôngxuyênmodal,Done/Enterkhôngstart, reloadgiữMedium, xoayngang/dọcgiữLow, trảHigh. Lấy trạngthái radio từpixelrenderthật, khôngdebugAPI hay gánfakeUI.3ảnhđãxem, chữ/nút khôngcắt. Đây làgiảlậpbố cục/touch, khôngphảiiPhonethật. Bàichạythử trêncandidatecũ bằngSwiftShader đãdừng, khôngtínhlàthànhcông; chỉ26checks bảncuốiđượcbáocáo.

- **Kiểm tra cuối trước phát hành:**101finalscene +42repo/33site +26graphicsbrowser +37web/offline/touch +20fullscreen/SafeArea +34indexbytechecks đạt,0lỗi runtime. `VerifyStagedWeb.py` đối chiếu blob/7parts trongGit vớiartifactđãkiểm vàcache, không chỉsofileworkingtree.3ảnhsettingsdọc/ngang/desktop đãxem. UnityEdit/khôngcompile/scene sạch/start(1,55;0;1,92)/AA4/bóng100/jobsfalse, reportrelease145-final-editor.json. Gitlock tạm thời làmstageđầukhôngchạy; khi lockhết đãstage lại và34bytechecks đạt. Ở mốc này đang chuẩn bị commit/push; kết quả phát hành hoàn tất bên dưới.

- **Phát hành hoàn tất:** đã commit/pushmain **9dc3aa50bd89513838e39f95ed5fe64db03ed05b**; **Pages40571941077success**, version **1.45.0**, cache **c0f81245589b65f2**. `VerifyPublishedGame.cjs` **27livechecks đạt/0runtime**: mọi runtimefile, cả2data/WASM, shell/SW/precache khớpbyte artifact đãkiểm; UnityWebAssembly trênURLchínhthức khởi tạo và nhận start/pause được. `Captures/release145-live-results.json`, `release145-pages.json`, `release145-live.png`; đã xem ảnh. Hai tài liệu đã cập nhật trạng thái cuối. Không còn việc21.78/21.79 chờ build/phát hành.
- **Kết thúc phiên:** UnityEdit/khôngcompile/scenesạch/start(1,55;0;1,92)/AA4/bóng100/jobsfalse; server kiểmtra8771/PID26640 đãdừng đúngprocess. Giữ mọi thay đổi Art/Wave/Packages/Simulator ngoài phạm vi. Các phép kiểmtra mobile là profileChromium/touch, chưa thaythế Safari/Android vật lý. Bản trước khi phát hành là1.44.0, hiệnweb1.45.0.


### 21.80. Thêm khủng long rồi xe tăng sau mỗi lần rẽ (28/10/2026, Codex — đã phát hành1.46.0/21.82)

- **Yêu cầu đã chốt:** sau khi player quẹo trái/phải xong, thêm1khủng long ngay ở **giao lộ kế tiếp phía trước trên đường mới**, rồi thêm1xe tăng **sau2giây**. Giữ lịch định kỳ khủng long10giây và xe tăng9giây. Đợt bổ sung có thứ tự riêng; xe đang có và các đợt định kỳ tiếp tục hoạt động, không xóa/đặt lại bộ đếm của chúng.
- **Điểm kích hoạt:** `FinishJunctionTurn` gọi `QueueTurnHazards()` đúng sau rebase90độ, chọn đường mới và `PrepareBranches`. Không kích hoạt khi chỉ đổi làn hoặc đi thẳng qua giao lộ. Partial mới `PhoVietTurnHazards.cs` +meta giữ một wave riêng cho mỗi lần cua; nhiều lần rẽ liên tiếp không xóa lịch của lần trước. Khủng long dùng vị trí `NextJunctionDistance` trong hệ trục mới, hướng180độ đi ngược chiều; không dùng khoảng sinh định kỳ110m. Xe tăng tính lại giao lộ phía trước ở lúc sinh; nếu người chơi chạy nhanh đã vượt giao lộ đầu thì chọn giao lộ kế tiếp cách80m, không sinh sau lưng.
- **Thời gian/vòng đời:** `TurnTankDelay=2` giây gameplay kể từ khủng long của wave thực sự sinh thành công. Tạm dừng không chạy đồng hồ; khi đang cua tiếp có thể đếm thời gian nhưng chờ hết cua mới sinh. Hết chỗ/pool thì giữ yêu cầu và thử lại, không sinh xe tăng trước khủng long và không rút ngắn khoảng2giây do chờ pool. Retry/reset xóa toàn bộ wave; đang té/GameOver/bị bắt không tạo thêm. Giữ pool khủng long tốiđa4 và pool xe hiện hành; các cờ dinosaursEnabled/tankBarrageEnabled tiếp tục được tôn trọng.
- **Vị trí an toàn:** chỉ xe tăng bổ sung được kiểm footprint cả người chơi/xe khác/thân khủng long (rộng4,65m, nửa dài4,35m) với khoảng đệm0,5m ngang/2m dọc trước khi sinh. Tính đúng làn cuối mà `KeepSpawnOnBridge` sẽ chuyển vào khi gần cầu, không chỉ làn ngoài được yêu cầu. Khi bị chắn, không đổi variant/prepared của bất kỳ xe thường đang chờ nào; đọc trước kích thước variant xe tăng rồi mới PrepareVariant khi có chỗ. Giữ hướng ngược chiều và hoạt động bắn/đạn/cảnh báo hiện hành.
- **Các file:** `PhoVietDinosaurs.cs` tách private `SpawnDinosaurAt(z)` còn public `SpawnDinosaur()` giữ nguyên khoảng sinh định kỳ; `PhoVietTankFire.cs` tách private `SpawnTankAt(float? junctionZ)` còn public `SpawnScheduledTank()` giữ122–130m; `PhoVietGame.cs` reset/tick wave chỉ khi đang chơi; `PhoVietRoutes.cs` gắn hook hoàn tất rẽ. Không sửa scene/asset/animation/Blender, camera, tốc độ cua, điều khiển, chất lượng đồ họa hoặc jobs.
- **Kiểm tra:** compile0lỗi/0warning. `VerifyTurnHazards.cs` **219điều kiện đạt**:48lần rẽ thực (3seed×4làn×2tốcđộ thường/W×2dt16,7/50ms), đúng vị trí sau rebase,1khủng long rồi1xe tăng, giữ timer định kỳ;2wave liên tiếp độc lập; pause/reset/falling; pool Dino/xe đầy; vị trí bị chắn và không làm xe thường thành xe tăng; vị trí thật sau tự nhập làn cầu (probe bằngSpawnTraffic). `VerifyDinosaurs`69 + `VerifyTankBarrage`69 + `VerifySupercarTurns`32 đạt; **tổng389**. Fixture TankBarrage được bổ sung lưu/tắt/khôi phục cờ khủng long để phép đo9giây độc lập với hazard mới, không thay gameplay. Các báo cáo Captures lịch sử được sao lưu và khôi phục; kết quả lần này nằm trong `turn-hazards-checks.json`/`turn-hazards-regression.json`.
- **Ảnh:** `CaptureTurnHazards.cs` chạy cua phải thật, chụp bằng camera game1280×720 ngay sau cua và sau khi xe tăng sinh; người chơi thực hiện cú nhảy bình thường trong cảnh thứ2. Đã xem2ảnh `turn-hazards-dinosaur.png`/`turn-hazards-tank.png`. Trường hợp chụp xe tăng sinh ở1,99999881s, khủng longz25,775 vàxe tăngz31,788 (ở sau khủng long, cả hai phía trước player). `turn-hazards-visuals.json` lưu số đo.
- **Các lỗi probe/fixture đã xử lý:** kiểm tra pool đầy ban đầu còn vị trí xe cũ làm chắn đường; fixture đưa xe chiếm pool ra xa để tách giới hạn pool khỏi vật cản. Sau recompile trong Play, Unity còn tham chiếu AudioSource đã hủy qua domain reload; thoát/vào Play sạch rồi chạy toàn bộ lần cuối đạt, không sửa âm thanh/jobs để né lỗi kiểm tra. Không tính các lần chạy thử lỗi vào số đạt.
- **Bàn giao:** Unity Edit Mode/scene sạch, gốc xe `(1,55;0;1,92)` giữ nguyên, dinosaurs/tankBarrage=true, jobsfalse/RideJobsReleased=false. Chỉ commit cục bộ source/test/bằng chứng và hai tài liệu. **Đã build/push/phát hành1.46.0 trong21.82; trạng thái sourcecục bộ trước đây đã được thay thế.** Các sửa đổi Art/Wave/Packages/Simulator và Captures khác được giữ nguyên.


### 21.81. Sửa cảm giác trễ trước khi xe đua đổi làn (28/10/2026, Codex — đã phát hành1.46.0/21.82)

- **Yêu cầu:** phím trái/phải và vuốt trái/phải phải phản hồi ngay, giữ hiệu ứng đổi làn đang mượt. Không tăng tốc toàn bộ quá trình hay làm xe nhảy sang làn mới.
- **Nguyên nhân đã kiểm tra:** `Update` đọc `GetKeyDown` rồi gọi `MoveLane` trước `TickRide`; native gọi hành động ngay khi vuốt đủ ngưỡng; `app.js` gửi `ApplyBrowserGesture` trực tiếp từpointermove, không có debounce/timer hay chờpointerup. Đường cong từ trạng thái nghỉ có cả vận tốc/gia tốc ngang bằng0, nên đầu thao tác dịch chuyển rất nhỏ; bộ lọc yaw làm thân xe biểu hiện chậm thêm. Không sửa JS hoặc giảm ngưỡng vuốt7,5% cạnh ngắn, vẫn1chạm/1lệnh và chặn cử chỉ bắt đầu trênHUD.
- **Triển khai duy nhất ở gameplay `PhoVietGame.cs`:** thêm `laneResponseGain` vào bộ lấy mẫu đường cong. Xe ô tô xuất phát từ nghỉ dùng **Hermite5 cũ + 3d×u²×(1−u)^4**, với `d` là khoảng cách tới làn đích và `u` là tiến độ chuẩn hóa. Thành phần bổ sung có gia tốc đầu hữu hạn6d×pace²/T² (T là duration cơ sở), vị trí và vận tốc đầu vẫn liên tục, bằng0 tới đạo hàm bậc3 ở cuối; nhịp vào làn/trả lái cũ được giữ. `LaneCurveValue` tính đúng cả vị trí/vận tốc/gia tốc; khi đổi hướng giữa chừng giữ các đạo hàm đang có và không thêm gain mới. `ClearLaneMotion` xóa gain khi reset/cua/va chạm.
- **Hướng thân xe:** car yaw attack **0,08→0,05giây**; return0,13giây giữ nguyên. Lọc nghiêng thân0,22giây, giới hạn roll2,5độ, bộ lọc bánh lái và hướng va chạm theo thân vẫn giữ. NPC/xe máy không dùng thành phần đáp ứng mới. Rẽ giao lộ vẫn theo quỹ đạo/điểm vào hiện hành; không ép cua sớm qua vỉa hè.
- **Tốc độ/thời gian:** giữ duration cơ sở1,35giây và pace=Speed/17 cho xe đua, tức khoảng22,95m/làn khi tính hết đường cong; W giữa chừng tiếp tục tăng nhịp theo tốc độ thật. Không thay tăng tốc/nhảy/húc/camera/độ phân giải/texture/mô hình/đồ họa hoặc các luật va chạm. Ở phép đo kết thúc cách tâm làn<1mm, quãng chạy60FPS vẫn **22,423702m** như trước.
- **Mốc lịch sử21.81:** các số timing/quãng đường trên là baseline đã phát hành; source21.89 tăng pace thêm15% (full curve19,9565m), giữ gain phản hồi đầu và damping21.81. Xem21.89 cho đo trước/sau.
- **Số đo trước/sau (17m/s,60FPS, cùng đường xử lý `MoveLane`):**

| Mốc | Trước | Sau |
| --- | --- | --- |
| Dịch ngang khung đầu | 0,05746mm | 1,40893mm |
| Yaw thân khung đầu | 0,002288° | 0,082272° |
| Góc bánh lái khung đầu | 0,12076° | 0,98849° |
| Dịch ngang sau50ms | 1,495mm | 12,493mm |
| Dịch ngang sau100ms | 11,304mm | 48,934mm |
| Đạt1cm dịch ngang | 100ms | 50ms |
| Bước yaw lớn nhất60FPS | 0,49892° | 0,48977° |

- Đây là đo mô phỏng trong Unity, không phải độ trễ phần cứng/browser từ ngón tay/phím tới màn hình. Xe bắt đầu thay đổi vị trí/hướng/bánh ở khung xử lý đầu, không tuyên bố thiết bị có độ trễ tuyệt đối0ms. Dữ liệu `lane-response-before.json`, `lane-response-comparison.json`.
- **Kiểm tra cuối:** `VerifyLaneResponse.cs` **217đạt**:36tổ hợp (MoveLane chung với bàn phím/nativeContinueSwipe/webpacket × trái/phải × thường/W × dt16,7/33,3/50ms), nhận trước nhấc tay, khôngteleport, phản hồi vị trí/yaw/bánh ngay khung đầu, đường đi đơn điệu, bước vị trí/yaw giới hạn, quãng/thời gian và trả lái. Hồi quy **182ScalePace +90MotionFX +32cua +39gestures +219đợtDino/Tank sau cua = tổng779đạt**. Các suite cũ không nới tiêu chí. Phương án thử đầu có thay đổi nhỏ đoạn cuối làm2kiểm tra thời gian60FPS thất bại; đã thay bằng gain triệt tiêu ở cuối như trên rồi chạy lại tất cả đạt. Compile0lỗi/0warning.
- **Hình ảnh:** `CaptureLaneResponse.cs` chụp camera game ở50/150/450/1600ms, đã xem4ảnh `lane-response-*ms.png`: xe tiến sang làn, hướng thân và tư thế ổn định khi hoàn tất, không giật/nhảy vị trí. Báo cáo tổng `lane-response-regression.json`; các Captures lịch sử được sao lưu/khôi phục sau hồi quy.
- **Bàn giao:** Unity Edit Mode/scene sạch/start(1,55;0;1,92)/jobsfalse, version vẫn1.45.0. Cập nhật cả BANGIAO và tài liệu này, commit cục bộ riêng source/test/bằng chứng. **21.81 và21.80 đã build/push/phát hành1.46.0/21.82.** Giữ nguyên các thay đổi Art/Wave/Packages/Simulator ngoài phạm vi; không dùng Blender hoặc chỉnh scene/asset.


### 21.82. Build và phát hành1.46.0 (28/10/2026, Codex — đã phát hành và kiểm live)

- Người dùng yêu cầu build/phát hành. Gồm toàn bộ21.80 (Dino rồiTank sau cua) và21.81 (phản hồi đầu đổi làn); baselineHEADf7bbce3, web cũ1.45.0. Đã cập nhật pinversion/helper146; đang cấu hình và kiểm scene trước build hai biến thể GPU. Giữ jobs khóa, vị trí đã lưu(1,55;0;1,92), chất lượngDPR/texture/AA/bóng và heap128/512MiB. Không dựng lại scene hoặc importAssetBlender.
- Chưa có kết quả build/bundle/browser/commit/push/Pages cho1.46.0; sẽ ghi kết quả thật sau từng mốc. Các fileArt/Wave/Packages/Simulator/Captures lịch sử không liên quan được giữ nguyên ngoài release.

- **Mốc trước build:** compile0lỗi/0warning, `VerifyRelease146Scene`101đạt; scene sạch/404dependency/10.983renderer/7.670obstacle/start1,92/jobsfalse/AA4/shadow100m và GPUformat/độphângiải đúng. ConfigureCLI hết timeout120s trong lúc44texture đang nén nhưng Editor vẫn chạy; không gọi trùng. Chờ cấu hình thật hoàn tất rồi kiểm101đạt mới queuebuild bằngBuildWebRelease.Main; queued chưa phải thành công. Dấu vân tay194file source/scene/template lưu ở`release146-source-fingerprint.json`.

- **Mốc build mobile:** Succeeded461,795s/0lỗi/3warning. Lần chuyểnDXT đầu dừng vì Unity báokhôngghiđược`PV_MeshyTank_Palette.png.meta` (khôngphảifileReadOnly). Giữreportlỗi ở`release146-first-build-report.json`, xác nhậnmobileBuildReport thànhcông rồi gọientryResume; lầnnàyimportđạt vàdesktopđangbuild. Không bỏkiểmGPU hoặc giảmđồhọa để láchlỗi; chưa tínhlầnlỗi làbuildthànhcông.

- **Build/đóng gói đã thành công:** mobile461,795s/0lỗi/3warning; desktop108,640s/0lỗi/1warning, Resume120,871s gồmmerge. MãWASM/framework giải nén giống nhau giữa2variant; cache cuối`7b8dfa6dc4f391bd`. `VerifyRelease146Scene`final101đạt, fingerprint194file nguồn/scene/template không đổi. `publish146.ps1`pack32MiBparts/materialize`.tools/site146`,42repo+33sitechecksđạt. Tổng193.494.172B; mỗi máy chỉ tải1data: mobile92.717.923B, desktop108.106.217B. Đang kiểmbrowserlocal, chưacommit/push/Pages.

- **Browser/Git mốc1:** `VerifyGraphicsWeb`26đạt (radio thậtbằngpixel, chuột/touch, lưuqua reload, xoaydọc/ngang); `VerifyWeb`37đạt (desktop, Pixel7, hướng dẫn iOS, offlinekhởichạyWASMthật, vuốt/chạmhailần/pause, canvasfullsaurotation);0lỗiJS/Unity. Đã xemảnhgraphicsdesktop/phone2hướng vàgameplaytouch/landscape. `VerifyStagedWeb`34đạt: Gitblob/parts/cache khớp chínhgóiđãtest. Đang chạyfullscreeniPhone/iPad; chưapush/pháthành. Finalbuildwarning1làPipelinekhôngcóRuntimePipelineConfig nênMCPkhôngchạytrongPlayer, khôngphảilỗigame; lưu`release146-build-warnings.json`.

- **Kiểm tra local hoàn tất:** `VerifyFullscreenGame`20đạt, iPhone/iPad dọc/ngang vớiSafeArea,0runtime. Đã xemảnh iPhone ngang/iPad dọc. Tổnglocal:101finalscene+42repo+33materialized+26settings+37web/offline/touch+20fullscreen+34index; kiểm194file nguồn giữ nguyên. Hồ sơ lưu`Captures/release146-*`. Đangcommit/push và xácminhPages; chỉtestbrowsergiảlập, chưatestSafari/Androidmáythật, khôngtuyênbốđộtrễphầncứng0ms hoặcFPSmáythật.

- **Đãpush:** commit`6ae2f3f`lênmain gồmfeature35aa737/f7bbce3vàartifact1.46.0 đãtest; khôngstageArt/Wave/Simulator/Packagesngoàiphạmvi. ChờworkflowPagesđúngSHA vàlivecheck, chưađánhdấuphat_hanhthànhcông.

- **Kết quả cuối đã phát hành:** releasecommit`6ae2f3ffe7e08c6e9209e6bd6d5140d502cb959b`, Pagesrun`40662116798`success, cache`7b8dfa6dc4f391bd`, URL`https://denispham1107.github.io/taixecongnghe/`. `VerifyPublishedGame`**27đạt/0lỗiJS/Unity**: version/cache vàmọitệp(chứa2data/WASM/framework/shell/icon) bytekhớplocal, gameWASMthật khởiđộng/Start/Pauseđược. Đãxemảnh`release146-live.png`; báo cáo`release146-live-results.json`và`release146-pages.json`. Cácmốcđangchờởtrênlànhậtkýthựchiện, khôngphảiviệcchưaxong.
- **Bàn giao cuối:** UnityEdit/khôngcompile/khôngbuild/scenesạch, start`(1,55;0;1,92)`, version1.46.0, jobsfalse/RideJobsReleased=false. DừngđúngserverPython8771/site146. GiữArt/Wave/.blend/Packages/Simulator/Captureslịchsửngoàiphạmvi; khôngreset/commitmùquáng. CậpnhậtcảBANGIAO/huongdan/READMEvàđẩybằngchứngpháthànhtrongcommitbàngiaotiếptheo (khôngđổiartifact, khôngcầnbuildlại).


### 21.83. Thiết kế lại trang chủ và ảnh buổi sáng có sương (28/10/2026, Codex — đã phát hành1.47.0/21.84)

- **Phạm vi:** trang HTML trước khi tải Unity, không phải bảngWelcome IMGUI tronggame. Người dùng yêu cầu phong cách xe đua hiện tại, ảnhgame mới có sương buổi sáng, không tràn/chồngdesktop/mobile2hướng. Đã được build/phát hành1.47.0 theo yêu cầu tiếp theo, xem21.84.
- **Nguyên nhân:** CSSlanding cũ đặtfooterabsolute lênhai cột; welcomeheight100%/nội dungvượtchiềucao vàmediaqueryngang khônggiải quyết hết. Nội dung vàposter vẫn nhânvậtáođỏ/xe máy, guide cònnóiđạp/mấtmạng/đónkhách.
- **Thiết kế:** nềntrắngxanhsáng, chữxanhđậm/điểmnhấnxanhdương hợpBlueSupercar, CTAđỏ. Title “Phố quen. Chuyến đi mới.”; mô tảxeđua, điều khiểnQhúc/Space/W/P vàvuốt. Headerflexwrap khi cóthêminstall, main2cộtdesktop/ngangđủrộng,1cộtmobile; bodygridautoheader/minmaxgame, welcome tựcuộn. Intro/photo/figcaption/footer đềutrongflow, khôngpositionabsoluteđènhau; nhãncảnhnằmdướiảnh, khôngchecảngxe. HìnhảnhkhôngthaythếbằngảnhAI. Safeinsetsbaoquanhheader/content/footer/dialog; dialogcuộntrongviewport, đónghelp trảfocusvềnúthelptrênlanding.
- **Ảnhchụpthật:** `CaptureLandingMorning.cs` chạyPlay từscenesaved, tạmđóngbăngthờigian/cleartraffic vàđặt3NPCthuộcpoolhiệncó, dùngrideCameraRenderkhôngHUD. Chụpmốcdaylight1,5s, MorningFog.Strength0,84375 (profile6giâyhiệnhành), xeđua/nhàcửa/thúcưng/xehiệncó. XuấtJPEG93%, `poster-morning.jpg`1920×1280/732.102B và`poster-morning-portrait.jpg`1280×900/399.810B (khungảnhthẻmobile), picturemediachọnđúngảnh. Camera/flags/time/ResetRideđượckhôiphụctrongfinally, khôngsave/rebuildscene. Đãxemảnhnguồnvàảnhlayout. LầnprobeđầulỗiserializeVector3normalizedvònglặp, đổi sangx/y/zscalar rồi chụplạiđạt; khôngtínhlầnlỗi.
- **File:** `Assets/WebGLTemplates/PhoVietPWA/index.html`, `style.css`, `app.js` (chỉfocushelp),2JPEG+meta; xóaposter.png+meta cũ. `PhoVietWebBuild.RefreshWebShell` giờlấyHTMLtemplateđầyđủvàgiữkhốiruntime sinhtrước,copy2JPEGvàbỏposteroutputcũ,khôngthaygameplay/WASM. Helper`RefreshLandingPreview.cs`3kiểmtrađạt/5runtimeURLs giữđúng. Lệnh evalđầuvượt5s khi bămartifact nhưngthựctếhoàntất; helperrun_script120s đãkiểmthànhcông, khôngbuildgame đểláchfrontend.
- **Kiểmtra layout:** `VerifyLanding.cjs`**315đạt**,12kíchthước320×568/390×844/430×932/844×390/568×320/932×430/768×1024/1024×768/1280×600/1440×900/1920×1080/640×450 (tươngđươngkhungzoomhẹp). Mỗikhungkèmnormal/loadingdài/retry/offlinestatusdài, kiểmheader/CTA/intro/photo/label/footer khôngchồng, khôngoverflowngang, cuộntớiCTAdễdàng,ảnhmớiđủ1280px, safearea vàhelpdialog/Escape/focustrảvề. Fixturecuối dùnginnerTexttrên2node nênlỗistrictlocator; sửaallInnerTextsrồichạylạitrọnsuite315đạt. KhôngđổiCSSđểnékiểmtra.
- **Bằngchứnglocal:** `landing-morning-source.json`, `landing-refresh-checks.json`, `landing-artifact-checks.json`, `landing-layout-checks.json`, ảnhlanding-desktop/phone/phone-landscape/small-phone. Artifactpreviewở`Builds/WebGL`,cache`6b003537ca1b174d`; cả5runtimefiles bytekhớpdocs1.46.0,2ảnhmớitổng1.131.912B, khôngcònposter.png trongprecache. Khôngsửaartifact`docs/`hoặcwebđangpháthành.
- **Nhật ký mốc đầu (đã hoàn tất ở cuối mục):** WebGL/offline/touch/fullscreenhồiquy trênpreview, xácnhậnEditorcuối vàsourcecommit. Khôngbumpversion/fullbuild/push/Pages trongyêucầuthiếtkếnày.

- **Mốc37web:** `VerifyWeb.cjs`trênpreviewđạt37/0JS/Unityerror: landing/installguide, WASMthật, bànphím/vuốt/chạmhailần/pause, lưuvàkhởiđộngoffline, canvasphủkínsaunhấpxoay. Copybằngchứng`landing-web-checks.json`. FinalEditorEdit/scenesạch/start(1,55;0;1,92)/jobsfalse/version1.46.0. Đangchạynốt20fullscreen.

- **Fixturefullscreen:** scriptcũđốichiếumặcđịnh`docs/`(bảnđãpháthànhcũ), nênlầnđầuđúnglàbáokhácindex. Thêmtham sốthứ4`artifactRoot`mặcđịnhvẫndocs, chạyvới`Builds/WebGL`chođúngpreview; vẫnđốichiếubyteđủ6files/giữnguyên20tiêuchí, khôngcopyđèdocsđểláchtest.

- **Kết quả cuối:** `VerifyFullscreenGame`20đạt/0runtime,iPhone/iPad2hướnggiữcanvasfull/insets/khôngheaderthườngtrực. Tổng**315layout+37web/offline/touch+20fullscreen=372browserchecksđạt**; helperrefresh3đạt,ảnhmớiđúnggame/fog,5runtimefilebytegiữnguyên. KhôngtuyênbốđãtestmáySafari/Androidthật. Bằngchứng`landing-fullscreen-checks.json`, `landing-web-checks.json`, `landing-layout-checks.json`và4ảnhlayoutđãxem.
- **Bàn giao:** UnityEdit/scenesạch/start(1,55;0;1,92)/jobsfalse/RideJobsReleased=false/version1.46.0;serverPython8783đãdừng. Source/test/ảnhmới/meta/2tàiliệulưucommitcụcbộriêng; khôngstageArt/Wave/Packages/Simulator/Captureskhác. **Trạngtháilocaltrướckhiđượcyêucầupháthànhđãđượcthaythế: fullbuild/push/Pages1.47.0trong21.84**. PreviewBuilds/WebGL/cache6b003537ca1b174dchỉđểreview, khôngtuyênbốbảnmớipháthành.


### 21.84. Build và phát hành1.47.0 với trang chủ mới (28/10/2026, Codex — đã phát hành và kiểm live)

- Người dùng yêu cầu build/phát hành21.83. BaselineHEADcb71f1d, web1.46.0/21.82. Bumpversionhelper147vàVerifyRelease147Scene, đangconfigure/preflightvàbuildmobile+desktop. Giữ2texturevariant/AA4/DPR/độnéthình/heap128–512MiB/start(1,55;0;1,92)/jobsfalse. KhôngstagecácthayđổiArt/Wave/Packages/Simulatorngoàiphạmvi. Chưa có kếtquảbuild/commit/push/Pagescho1.47.0; cậpnhậttheomốcởdưới.

- **Preflight:** compile0lỗi/0warning, ConfigureWebRelease.Maintimeout600s hoàn tấtđúng1.47.0,101scenechecksđạt (Edit/scenesạch/start1,92/jobsfalse/AA4/bóng/2Ktexture/128–512MiB/growth10%cap32MiB/404deps). Fingerprint196filesource/scene/template ởrelease147-source-fingerprint.json. QueueBuildWebRelease.Mainđủ2variant;queuedkhôngtínhthànhcông. Thêmtham sốprefixbằngchứngchoVerifyLanding đểkhôngghilạicapture21.83, khôngđổitiêuchí315checks.

- **Mốcbuild:** hàngđợidelayCallchỉbắtđầusaukhikhôiphụccửasổUnity;toolactivatebáotimeoutnhưngreportđãchuyểnqueued→building vàlogchạythật, khôngqueue2lần. MobileđãBuildSuccessful/output1.47.0, đangimportDXT/builddesktop;chưacôngbốbuildhoàntất.

- **Build vàbundle đãđạt:** BuildWebRelease.Maincompleted/Succeeded1.47.0,total296,451s,lastdesktop101,956s/0lỗi/1warning. Đãbuildđủmobile/desktopvàkiểmWASM/frameworkgiảinénkhớp nhau.101finalsceneđạt;196filenguồn/scene/templatebytekhớpbaselinecb71f1d. pack32MiBparts/materialize`.tools/site147`đạt43repo+34sitechecks,cache`d26fbf3bf46d9ea7`; tổng194.008.957B,mỗi máy1data:mobile93.233.017B/desktop108.620.973B. Đangkiểmbrowser trênartifactghépđúngPages;chưapush.

- **Landingbảnfullbuild:** VerifyLanding315đạttrênsite147đãghép,12khungnormal/loading/retry/cutout/help/focus; đãxemảnhdesktopvàphonedọc. Bằngchứng`release147-landing-*`. Cảnhbáocuối1làPipelinekhôngcóRuntimePipelineConfignênMCPkhôngchạytrongPlayer, khôngphảilỗigame; lưu`release147-build-warnings.json`.

- **Gitkiểmbyte:** stageđúngsource/helper/pinversion/docs/newrelease147evidence, `VerifyStagedWeb`35đạt: cácblob/parts/hash/cachekhớp góiđãtest; khôngstageArt/Packages/Simulator. DesktopWASM/keyboard/pausevàkhởiđộngofflineđãđạt, đangchạycuốiphone/fullscreen; chưa tínhsuitechưaxonglàpass.

- **Weblocal:** VerifyWeb37đạt/0runtime, gồmWASMthật/offline/touch/rotation/PWAinstall; lưurelease147-web-checks.json, đãxemlandingdesktop/phonedọc/ngang. VerifyPublishedGameđượcbổsungchụplandingtrướckhivàogameđểlưubằngchứngtrangchủwebthật, khôngđổicáctiêuchíbyte/WASM/runtime. Đangchạynốt20fullscreen.

- **Localhoàntất:** 315landing+37web/offline/touch+20fullscreeniPhone/iPad2hướng=372browserchecksđạt/0JS/Unityerror;101finalscene+43repo+34site+35Gitindexđạt. Đãgiữnguyên196filenguồn/cảnh/template. Nốtchiềungang/dọcdùngSafeArea; ảnhtrangchủmớiđãxem. Chuẩnbịcommit/push vàxácminhPages/live, chưađánhdấuwebđãcậpnhật.

- **Đãpush:** releasecommit`ec06629`lênmain, gồmcb71f1d/21.83vàdocs1.47.0đãtest. UnityEdit/khôngcompile/khôngbuild/scenesạch/version1.47.0/start1,92/jobsfalse;server8771site147đãdừng. ChờPagesđúngSHAvàlivechecktrướckhiđánhdấupháthành.

- **Kết quả cuối:** releasecommit`ec066294a2f4627bb33e27fdf4dc3cc7d9c69d06`, Pagesrun`40689965684`success, cache`d26fbf3bf46d9ea7`, URL`https://denispham1107.github.io/taixecongnghe/`. `VerifyPublishedGame`**28đạt/0JS/Unityruntimeerror**: version/cache/2data/WASM/framework/shell/icon/2ảnhmớibytekhớplocal,gameWASMthậtStart/Pauseđược. Đãxemảnh`release147-live-landing.png`(trangchủmới) và`release147-live.png`(gameđãchạy). Lưu`release147-live-results.json`/`release147-pages.json`. Cácmốcchờởtrênlànhậtkýtrongquátrình, khôngcònviệcchưaxong.
- **Bàn giao cuối:** UnityEdit/khôngcompile/khôngbuild/scenesạch/start(1,55;0;1,92)/jobsfalse/RideJobsReleased=false/version1.47.0;server8771site147đãdừng. Tài liệuBANGIAO/huongdan/READMEvàevidencelivecommit/pushbàngiaotiếptheo, khôngđổiartifact/khôngcầnbuildlại. GiữArt/Wave/.blend/Packages/Simulator/Captureslịchsửkhôngliênquan. Trangchủ21.83khôngcònchờpháthành; khôngtuyênbốđãtestSafari/Androidmáythật.


### 21.85. Ba mẫu cầu tham khảo dựng bằng Blender MCP (28–29/10/2026, Codex — đã xong mẫu, chờ người dùng chọn và xác nhận)

- **Yêu cầu trực tiếp:** chỉ dựng lại mỗi cây cầu, làm vài mẫu để người dùng tham khảo; **chỉ sau khi người dùng chọn và xác nhận mới thay vào game**. Không sửa đường/sông/nhà/cây/xe/gameplay, không import vàoUnity hoặc build/phát hành trong giai đoạnchọnmẫu.
- ĐãđọcRiverBlock/generatorcầucũ: Near4,8/Far55,2→nhịp50,4m; mặtđường2làngiữa rộng6,2m, deck7,4m, méptínhrơisông3,35m. Cácmẫugiữkíchthướcmặtđườngđểsosánh, khôngchỉnhconstants/collider.
- BlenderMCPkếtnốiđược5.2LTS, addonprotocol4cũnhưngexecute/lookfallbackhoạtđộng; khôngtựcàiđè/khởiđộnglạiaddon. Kiểmtratrước: filechưalưu, Scene0objects/khôngrender. Tạo3scenePV85_A/B/Criêng, giữScenegốctrống. SavecopyfilemớingoàiAssets: `ArtSource/Concepts/Bridge-20261028/PhoViet_Bridge_Concepts.blend`; khôngghimđèfile.blendcũvàgiữbpy.data.filepathgốctrống.
- **MẫuA–SôngViệt:** bê tông/đá sáng,3vòmnhịp16,8m, lan canđồngcóhọatiếthìnhcánhsen/hìnhthoi,trụbo,đènlồnggọn. **B–VòmXanh:**2vòmthépxanhvới dâytreo/thanhneo/bảnmãvàbulông,khôngcógiằngngangchắntâmlòngđường. **C–CánhBuồm:**2thápngoàimépđường,cápvănghìnhquạt,dầmhộpxanhđậm,khônggiằngngangquađường. Đềucódeck/dầmdọc/diaphragm,khecogiãn,vạch2làn,gờbó/lan can/đèn/cờnhỏvàchậuhoa. Chỉlàmconceptvisual,chưacóLOD/collider/merge/xuấtUnity.
- **Preview:** mỗi mẫu cócollectionBRIDGE_ONLY vàcollectionPRESENTATION_NOT_FOR_GAME(nước/bờtrơn/đènstudio/camera), đểngườidùngxemhìnhkhối. Khôngthaythếmôitrườnggame. Cameraoverviewvàgócnhìnđầucầuđượcdùngnhưnhautrong3mẫu. RenderđầuAđãxemquaMCPlook: góccũchưanổivòm; đãhạgóccamera/thuortho66→57,thêmspandrelwallđúng3vòmvàsửafacewindingmeshkhối. Đangrender6ảnh1600×1000(A/B/C×day/street)thựcbằngEEVEEquaMCP.
- Scriptnguồn`build_bridge_concepts.py`,refineidempotent`refine_bridge_concepts.py`,queue`render_bridge_concepts.py`,metadata`concepts.json`,tiếnđộ`render-progress.json`,trangsosánh`bridge-comparison.html`cùngthưmục. BảovệgamebằngSHAfile`protected-game-fingerprint.json`trướcđụngviệc; sẽđốichiếulạisaukhixong. Chưaimport/build/commit/push/pháthành,webvẫn1.47.0.

- **Hoàn tất render:**6ảnhEEVEE1600×1000(A/B/C×day/street)thựcbằngBlenderMCP,queue226,15s. ĐãxemA-day/A-street/B-day/B-street/C-day/C-street vàboardtổnghợp. Board`bridge-comparison.html`vàPNG1800×1250hiện3cộtrõA/B/C,6ảnhkhôngthiếu,khôngoverflow;kiểmChromePlaywrightđạt3card/6ảnhđủ1600px. Đây làghépgiaodiệntrìnhbàycácảnhBlender, khôngảnhAI.
- **KiểmBlendercuối:**3mẫuroad6,2m/span50,4m;Scenegốc0object,filepathgốcvẫntrống/savecopyriêng. BasegeometryA803mesh/13.268tri,B707mesh/11.276tri,C643mesh/7.820tri;chưaappliedbevel/LOD/merge,khôngcoiđâylàbudgetruntime. Cầu/ngữcảnhnằmcáccollectionriêng. Fileblendđãlưucopylạivớicameraoverview,sceneA/rootAđượcchọn.
- **Giữgame:** đối chiếuSHA**2993file**Assets/ProjectSettings/Packages/docs với trước tác vụ,**0filethayđổi**,kểcảgiữnguyêncácthayđổicụcbộđãcó. KhôngđụngUnity/assetFBX/generatorcầucũ,khôngbuild/push/Pages,webvẫn1.47.0. Báocáo`blender-verification.json`, `comparison-checks.json`, `game-unchanged-checks.json`.
- **Bàn giao chờ lựa chọn:** `selection.json`selected=null,confirmed=false,installed_in_unity=false; chỉkhingườidùngchọn**vàxácnhận**mớilàmtiếpphầntíchhợp,sửa**duynhấtcâycầu**. ĐãcậpnhậtcảBANGIAO/huongdanvàREADMEbộmẫu. Lưudesign/script/render/bằngchứngtrongcommitcụcbộriêng,khôngpush/pháthành; cácfile.blendcũcủangườidùngkhôngbịghimđè.

### 21.86. Bộ nhà phố hiện đại theo ảnh mới — Blender MCP, chờ duyệt (29/10/2026, Codex)

- **Yêu cầu:** dựng lại tất cả nhóm nhà theo ảnh người dùng gửi (khung xám–xanh đậm, cửa kính/nội thất sáng ấm, biển hiệu nhiều màu, hiên sọc và billboard thú cưng), **giữ cây cối**. Chỉ cho xem mẫu; **chưa được nhập vào game trước khi người dùng duyệt**. Đây là tác vụ độc lập với lựa chọn cầu21.85 vẫn đang chờ.
- Đã kiểm tra generator nhà hiện tại:12cửa hàng rộng8,36m/sâu7,8m; Petshop là nhóm riêng; nhà ven sông có hai mặt,6–9tầng và sâu3m. Không chạy installer cũ hoặc dựng lại scene Unity. Ghi SHA3104file dướiAssets/ProjectSettings/Packages/docs để đối chiếu sau tác vụ, bao gồm cả các thay đổi cục bộ đã có.
- Dựng thật qua Blender MCP5.2LTS trong3scene `PV86_Street_Preview`, `PV86_Building_Catalogue`, `PV86_Riverside_Preview`, giữScene ban đầu0object/filepathtrống. Nguồn/script/render tách tại `ArtSource/Concepts/City-20261029/`; savecopy `PhoViet_NewCity_Approval.blend`, không ghi đè.blend cũ. Addonprotocol4 vẫn execute được; không tự cài lại. SkyenumNISHITA đã đổi ở5.2 nên dùng nềntrời xanh/đènSun+Area, không gán enum cũ.
- **Thư viện19mẫu:**13cửa hàng (12nhóm tiệm+Petshop),2nhà góc có returnfacade,4căn hộ hai mặt6–9tầng trên tầng trệt. Sàn/viềntrắng, tườngnavy/slate/xám, kínhxanh/ôphòngsángấm, lan cankimloại, khe mái/điềuhòa, biển nổi+biểnđứng, billboardcat/paw geometrythật,14sọc/hiên. Petshopgiữnềnxanh/chữtrắng/phụđề“Lưu Trú - Spa Pet”, không bàn ăn/quầy kính ngoài cửa; dùng lại meshchó/mèo/thỏ có sẵn. Cây lấy nguyên meshLOD0 từPhoViet_SaigonStreets.blend, không sửa nguồn/cây runtime.
- Script `build_city_study.py` gộp hình học theo vậtliệu cho từngmẫu, có bevelcạnh; base19mẫu103.723tri sau biến thểbillboard, **chưa phải budget runtime/chưa LOD/collider/atlas tối ưu**. Ngữ cảnhđường/vỉahè/đèn/cây/xeđua/tọađộ đặt chỉ để xem mẫu, khôngápdụnggame. `study.json` lưu collection/kíchthước/camera; `approval.json` approvedfalse/installedfalse.
- **Hoàn tất bộ mẫu, chờ duyệt:** đãdựng/savecopy/render6ảnhngang/dọc/Petshop/cậnmặtđứng/catalogue/ven sông bằng `render_city_study.py`. Đãxemcả6ảnhcuối vàboard; chưa nhậpUnity/build/push/pháthành, webvẫn1.47.0. Lưubộmẫu/tàiliệutrongcommitdesigncụcbộriêng,khônggộpcácArt/Wave/Packages/capturecũđangdirty.
- Đãxem2ảnhnháp vàảnhwidecao; sửa kínhalpha0,16→0,035, thêmđènấm170W/cửa hàng, biển/billboard3kiểu(cat/paw/chevron), hạfillmặttiền16k→3,5kW/giảmhighlightkính vàfitfonttheochiềucao(khôngphóngtochữđểlấpbiển). Hạcamerađểthấyđường/xeđua gốc, thêmnhàtiềncảnh cheđầudãytrống. `refine_city_study.py`idempotentđượcbuildergọicuối, chưaghiUnity.
- Lượtrenderđầu6ảnhhoànthành820,77s nhưngMCPprocessbáoTransportclosed, requestrefinechưachạy. Blenderthậtcònmở/ảnhvàblendđãlưu. Đãđọcgiao thứcJSONtrongaddonđangcài`blender_mcp.py`, kếtnốiquaAPIlocalhost9876củachínhBlenderMCPbằng`call_blender_addon.py`(chỉchophépchạyscripttrongfolderconcept); khôngcàilại/sửaconfig/restartBlender. Đãápdụngrefinevàsavecopy. **42kiểmBlenderđạt**(3scene/19mẫu/fontkhôngtràn/câysharedmesh/9Petshoptrên30nhà/texturepack/Scenegốc0/filepathtrống). Bộ6ảnhcuối32EEVEEsampleshoànthành**306,06s**, đủresolution1920×1080,1080×1600,2ảnh1600×1200,2100×1500,1920×1080. `render-progress.json`completed/0remaining; fileblendđãlưulạivớicameraStreet_Wide.
- **Bằng chứng và trình bày:** `verify_blender_study.py`→`blender-verification.json`42/0; `check_preview.py`→`preview-checks.json`34/0, gồmđủ6PNGđúngkíchthước vàđối chiếuSHA**3104filegame/0đổi/0thêm**(`game-unchanged-checks.json`). `capture_preview.cjs`dùngChromeđangcài→`viewer-checks.json`:1440×900/390×844/844×390đềukhôngtrànngang,6card/7ảnhgồmảnhthamkhảođềutảiđủ; screenshotHTMLboard1800px(`city-approval-board.png`)khôngcắtảnhherocar. Playwrightbundledbrowserpathtrongsandboxkhôngcó,đãdùngchannelChromehiệnhữukhôngcài/download. `preview.html`cho xemđủ6ảnh vàreference; ảnhlàBlenderthật, khôngảnhgameđãsửahoặcAI.
- **Bàn giao:** người dùngchưaduyệt, `approval.json`approvedfalse/installed_in_unityfalse. Chỉsau duyệtmớilàmchuỗitíchhợp**nhà**vớiLOD/merge/atlas/collider/giữgiớihạnđường–sông, kiểmhiệunăng mobile; câygiữnguyên, cầu21.85chờchọnđộclập. Khôngtựchạygenerator/installerphốtoàncảnhcũ. BANGIAO/READMEvàbảngtracứuđãcậpnhậtcùngtácvụ.

### 21.87. Bỏ chậu hoa trên cầu hiện tại, giá cờ hai thanh xanh (29/10/2026, Codex — đã sửa Unity/source, chưa build/phát hành)

- **Yêu cầu trực tiếp:** bỏ tất cả chậu hoa trên cây cầu hiện tại; bỏ thanh đen xuyên chéo lá cờ, thay hai giá đỡ xanh ở mép trên–dưới theo ảnh vẽ. Đây là chỉnh sửa được yêu cầu cho cầu đang dùng, **không phải duyệt/tích hợp mẫu cầu21.85 hoặc nhà21.86**. Hai bộ mẫu đó vẫn giữ trạng thái chờ.
- **Generator:** `ArtSource/Blender/pho_viet_river_bridge.py` bỏ vòng gọi12`flower_box` trên lan can, đủ3LOD; giữ nguyên cây/chậu ở lối dạo bờ sông. `lamp_flag` giữ cờ0,95×0,64m/tâmcao4,6m và sao vàng hai mặt; thay rodchéo bằng2tayngangtròn radius0,024m ởy4,245/4,955m, màu`awnGreen`#2E7D55. Đai xanh quanh cột radius0,105m/cao0,09m, chụpđầuthanh0,065m;LOD0thêm bulôngthép. Hai tay nằm ngoài mặt vải, không xuyên sao;4vịtrícờ/mỗibên→8cờ/16tayđỡ.
- **Xuất riêng:** `patch_bridge_fixtures.py` dùngBlender5.2background/factory-startup riêng vì MCPclientTransportclosed vàaddon9876khôngmở. Không tự mở/cài/restart/sửaMCP, khôngđụngBlenderlivehoặccác.blendcụcbộ; nguồn mới `PhoViet_BridgeFixtures.blend`. Chỉxuất `Assets/PhoViet/Art/River/PV_RiverBridge.fbx/json`, giữhierarchy/meshname/slots7vậtliệu. **Không chạy `build_river_bridge()` hoặc Unityriverinstaller**, không táitạo nhà/sông/đèn/collider/scene. Helperkiểmvới `.tools/bridge-fixtures-before*`; checkoutmới fallback`git show92c4348`đểđọcbaselinecũ, khôngcheckout/reset.
- **45kiểmBlender:** baselinegenerator khớp sốtriđãcài; nướckè/lốidạo/tường/metadataobstacle giữnguyên; deck/lan can/trụkhớp khi bỏriêngchậu/giácờ; vịtrícờ/sao2mặtgiữnguyên,2tayngangxanhngoàivải ở3LOD×2bên. TrianglesLOD0/1/2 **52736/13782/3654→50328/13454/3854**;LOD2tăng200tri để giữđủđai+2tayđỡ, khôngthêmrenderer/texture/material/collider. JSONthêm`bridge_fixtures`count/style/revision21.87, cácmetadatađangdùngkhôngđổi.
- **Unity:** `VerifyBridgeFixtures.cs` lưu snapshottrước tại`Captures/bridge-fixtures-before-unity.json`, kiểm **27điềukiện** sauimport:3meshGUID/localID,140meshrefvàmaterialbindings,35khối,10.983renderer,7.670obstacle,scene/jobs/startgiữnguyên. Boundschỉchophéplệch<5cmdođaikẹpởđỉnhcờ. EditorchưarefreshFBXngay nên gọi`Import()`đểForceSynchronousImportđúng1asset; khôngchỉnh importer/readability. Hai lỗiởprobeđãsửa: Vector3JSONvònglặp→mảngsố, Single-vs-Double→canonicalJSONroundtrip; xácnhậnvịtríthậtchưatừngđổi, khôngcoilàlỗigame.
- **101kiểmscene cuối đạt** bằngbảncopy`VerifyRelease147Scene`chỉđổitênclass/đườngreport (`.tools/VerifyBridgeFixturesScene.cs`), khôngghiđèbằngchứngreleasecũ. Giữplayer(1,55;0;1,92),scale0,85/jobsfalse/đồhọa/heap/GPUtexture/dependencies404;0missing,EditMode/scenesạch. **Tổng173điềukiện**45+27+101đạt, khôngphảikiểmWebGL hoặcđoFPS.
- **ẢnhthậtUnity:** `CaptureBridgeFixtures.cs` chỉPlayMode, bật1khốicầu/chặnNPCvàForceLODđểchụp6ảnh1600×1000: `bridge-fixtures-overview.png`, `bridge-fixtures-flag-left-lod0/1/2.png`, `bridge-fixtures-flag-right.png`, `bridge-fixtures-night.png`; đãxemcả6. Hai mặt sao/cờ/giá xanh rõ, khôngchậuhoa ởgần/xa, đènđêm/phảnchiếu vẫnhiện. `finally`khôiphụcflags/camera/timescale/reset rồiđãthoátPlay, khôngSaveScene.
- **Giới hạn tác động:** `bridge-fixtures-scope-checks.json`đốichiếuSHA3104fileAssets/ProjectSettings/Packages/docs; **chỉ2fileđổi:FBX+JSONcầu**, khôngfilethêm. Scene/.meta/cây/nhà/đèn/material/gameplay/jobs/config/docswebbytegiữnguyên. BáocáoBlender/Unity/scene/visualsở`Captures/bridge-fixtures-*.json`;cảBANGIAOvàphầnmôtảcầucũđãcậpnhật. Commitcụcbộriêngsource/asset/ảnh/check/tàiliệu; **chưabuild/push/pháthành**,webvẫn1.47.0vẫntrangtrícũ.

### 21.88. Bỏ thanh đen bên cạnh cờ ban công nhà phố (29/10/2026, Codex — đã sửa Unity/source, chưa build/phát hành)

- **Yêu cầu:** xóa đúng thanh màu đen cạnh lá cờ đỏ sao vàng trong ảnh ban công, không bỏ lá cờ/ngôi sao/lan can/hoa. Đây là thanh ởnhàphố,không phải giá cờ cầu vừa sửa21.87.
- `pho_viet_saigon_streets.py`→`flag(g,x,y)` bỏduynhấtlệnh`g.cyl((x,y,WALL-.05),(x,y+.6,WALL-1.3),.025,'iron',n=5)`, giữ nguyên cube vải vàhai saovàng. Mẫu0/3/6/9 vàcả4biếnthểPetshopđều gọi hàm này; đủ3LOD. Không thêm thanh/giá cờ thay thế khi người dùng chỉ yêu cầu xóa.
- Đãxuất riêng8FBX/JSONqua`remove_balcony_flag_poles.py` trongBlenderbackground/factory-startup; khôngchạybuild phố/river/installer, khôngghimđè.blendcụcbộ. Source mới`PhoViet_BalconyFlags.blend`; baseline3104filevà24mesh/510meshrefUnityđãlưu tại`.tools/balcony-flags-before/`và`Captures/balcony-flags-before-unity.json`. ScriptfallbackbaselineGit`b620b78`nếukhôngcócache; **64kiểmBlenderđạt**:countsbaselinekhớp8assetđangcài,24LODchỉmất10vertex/16tri của1cyl, mọi geometrycòn lại/collider/pets/nội thất/biểnhiệu/metadata JSONgiữnguyên(trừtriangles). Báocáo`balcony-flags-blender-checks.json`.
- **Unity:**`BalconyFlagCleanup.cs`Baseline/Import dùngrun_script,ForceSynchronousImportđúng8FBX.24meshGUID/localID/submeshvà510meshref/materialbindingsgiữnguyên;mọimeshgiảm16triđúngBlender,renderer/obstacle/scene/start/jobsđốichiếutrướcsaukhớp. `balcony-flags-unity-checks.json`thànhcông. KhôngthêmGameObject/material/texture/collidertrongsceneđãlưu.
- **4ảnh1280×900:**`balcony-flags-close-lod0/1/2.png`,`balcony-flags-balcony.png`đãxem; cờ/sao hai mặtvẫnđủ,khôngcònthanhnhôđượckhoanh. Gócchụpđầu bịsaitọađộXdoFBXUnitymirrored;đãsửahelper, sauđócâyđườngchecờnênchụpbảnsaotạmđúngLODGroupmôhìnhnhàtrongPlayModeđưa lên100mđểtáchcảnh, khôngdờixóahayẩn câythật. Ảnhlàkiểm môhìnhUnitytáchriêng,khôngphải gameplaynhàbay. Bảnsaohủytrongfinally,camera/timescale/daycyclekhôiphục;đãthoátPlay,`balcony-flags-final-editor.json`xácnhậnScene sạch/start(1,55;0;1,92)/jobsfalse/temporaryPreviewfalse. Envelopeảnhở`balcony-flags-visuals.json`.
- **Giới hạn tác động:**SHA3104fileAssets/ProjectSettings/Packages/docs(`balcony-flags-scope-checks.json`)chỉ8FBX+8JSONnhàđổi,khôngfilethêm;scene/.meta/cây/lan can/hoa/cầu21.87/river/config/docswebgiữbytecũ. Chỉsửageneratorhàmcờvàassetcóliênquan;catalogue/registryexporttổnghợp lịch sửkhôngtáitạovìkhôngchạyinstaller/fullexporter. BANGIAOvàphầnmôtảcảnhphốđãcậpnhật;commitcụcbộriêngsource/asset/ảnh/report/tàiliệu, **chưabuild/push/pháthành**,webvẫn1.47.0. Mẫucầu21.85vànhà21.86tiếptụcchờduyệt.

### 21.89. Xe đua chuyển làn nhanh hơn15% (29/10/2026, Codex — đã kiểm Unity, chưa build/phát hành)

- **Yêu cầu:** mỗi lần người chơi đổi làn trái/phải, tăng tốc độ chuyển làn thêm15%. Giữ chuyển động mượt, phản hồi nhập lệnh ngay và tỷ lệ theo tốc độ xe/W như đã chốt.
- `PhoVietGame.cs`→`AdvanceLaneCurve`:pacexeđua`max(Speed,1)/17`→`max(Speed,1)/17*1.15f`;nhánhxe máyvẫn1. Khôngđổiđườngcongbậc5/gainphảnhồiđầu21.81/dampingtrảlái hoặc tốcđộchạy thẳng/boost/nhảy/húc/NPC. Phím và vuốt đều đi chung đường xử lý này. Tăngrate×1,15tươngứngthờigiantừnglànchia1,15(khôngphảinhân0,85):fullcurve22,95m→19,9565m;ở17m/s1,35→1,1739s,30m/s0,765→0,6652s,67,5m/s0,34→0,2957s. W giữa chừng vẫn cập nhật pace mỗi frame;đổiýgiữvậntốc/gia tốccũ.
- **Kiểm tra:** dùng hai suite hiện có `AgentScripts/VerifyLaneResponse.cs` và `VerifySupercarScalePace.cs`, chỉ cập nhật expectation quãng đường/thời gian theo rate mới; chạy bản copy `.tools/VerifyLanePaceResponse.cs`/`VerifyLanePaceScale.cs` với đường report mới để giữ bằng chứng cũ. Compile **0 lỗi/0 warning**; **217+182=399 điều kiện đạt**, không failure. Bao phủ phím/Unity swipe/browser gesture chung handler, trái/phải, W bật/tắt/giữa chừng, tốc độ17/24/30m/s, timestep1/60,1/30,0,05s, đổi ý giữ vận tốc/gia tốc; giữ các ngưỡng mượt và thời gian trả lái. Đây là kiểm tra Unity mô phỏng điều khiển, chưa phải kiểm WebGL hoặc điện thoại thật.
- **Đo trước/sau60FPS:** cùng input phải/khôngW, đạt ngưỡng cách tâm làn<1mm sau22,4237022→19,5148678m (tỷ lệ1,149057 do lượng tử frame); dịch chuyển frame đầu1,409→1,861mm. Mốc1cm vẫn50ms theo bước lấy mẫu, không kết luận giảm thêm độ trễ nhận input. Full curve danh nghĩa chia đúng1,15. Bằng chứng `Captures/lane-pace-before.json`, `lane-pace-response-checks.json`, `lane-pace-speed-checks.json`, `lane-pace-comparison.json`, `lane-pace-recompile.json`, `lane-pace-final-editor.json`.
- **Trạng thái:** đã thoát Play; Unity Edit Mode/scene sạch, điểm xuất phát(1,55;0;1,92), jobsfalse. Commit cục bộ riêng source/suite/báo cáo và hai tài liệu; giữ các thay đổi Art/Wave/Packages/Simulator/Captures không thuộc yêu cầu. **Chưa build/push/phát hành**, web1.47.0 vẫn timing cũ;21.87–21.88 cũng chưa phát hành. Không đổi các mẫu cầu/nhà21.85–21.86 đang chờ duyệt.


### 21.90. Menu Pause thêm Về đầu phố, bỏ mũi tên nút chính (29/10/2026, Codex — chưa build/phát hành)

- **Yêu cầu/triển khai:** `Assets/PhoViet/Scripts/PhoVietGame.cs` → `EndCard`: nhánh paused thêm nút **VỀ ĐẦU PHỐ** gọi `ResetRide()` như nút ở màn hình kết thúc, ở y+285, cao34, cùng bề rộng/padding và kiểu chữ/màu nút phụ cũ. Nút **ĐI TIẾP** giữ y+215/cao58 và `TogglePause()`. Giữ card cao340: hai nút cách12, nút phụ còn21 đơn vị cách đáy nên không cần tăng diện tích card. Bỏ suffix `   →` ở **ĐI TIẾP** và **THỬ MỘT CHUYẾN NỮA**; retry vẫn gọi `RestartRide()`. Áp dụng cùng đường vẽ cho desktop và mobile dọc/ngang; không đổi Pause HUD, menu khác hoặc gameplay.
- **Kiểm tra:** Unity compile **0 lỗi/0 warning** (`Captures/pause-menu-recompile.json`). Đã xem đủ **6ảnh** `Captures/pause-menu-{portrait,landscape,desktop}-{paused,gameover}.png`, kích thước540×960/960×540/1440×900, hai nút rõ và không tràn/chồng. Helper chụp tạm `.tools/CapturePauseMenus.cs` dựa trên helper đồ họa hiện có, khôi phục size/timeScale/quality/preferences khi xong; `pause-menu-visuals.json` success/6frames. Đây là xem giao diện Unity, chưa phải build WebGL/điện thoại thật.
- **Hành vi:** gọi các handler hiện có trong Play Mode xác nhận Riding→Paused→Riding và Paused→ResetRide→Ready, Distance=0/Speed=0, giữ BestDistance, jobsfalse (`pause-menu-actions.json`). Không đổi thân handler ResetRide/TogglePause/RestartRide. Đã thoát Play, scene sạch/start(1,55;0;1,92)/jobsfalse (`pause-menu-final-editor.json`).
- **Bàn giao/phát hành:** cập nhật mô tả2.1 và `BANGIAO.md`; commit cục bộ riêng3file source/tài liệu và các ảnh/report mới. Không stage thay đổi Art/Wave/Packages/Simulator hoặc Captures lịch sử. **Chưa build/push/phát hành web**, live vẫn1.47.0;21.87–21.89 cũng chưa lên web, mẫu nhà/cầu chưa duyệt vẫn giữ riêng.


### 21.91. Đèn giao thông chi tiết, cục nóng xám trắng và bảng thuê nhà ven sông (29/10/2026, Codex — source đã xong, chưa build/phát hành)

- **Quyết định người dùng:** dùng Blender MCP dựng lại đèn giao thông/cục nóng; AC tông xám trắng dịu. Dãy ven sông **mỗi tòa một bảng “CHO THUÊ NHÀ” tại tầng cao thấp khác nhau như ảnh**, không gắn mọi tầng. Chỉ nâng chi tiết bộ kiến trúc đang dùng, không áp dụng19mẫu nhà21.86 hoặc3mẫu cầu21.85 chưa duyệt; giữ cây/cầu21.87/cờ ban công21.88.
- **Nguồn/triển khai:** thêm `ArtSource/Blender/pho_viet_fixture_details.py` (hàm geometry không tự sửa scene/export) và `upgrade_fixture_details.py` (xuất riêng26asset). `pho_viet_saigon_streets.py` nạp helper vào namespace, `ac()`→`condenser`, `traffic_signal()`→`detailed_traffic_signal`. `pho_viet_river_bridge.py` thay cube AC mặt đường bằng condenser và thêm `rental_sign()` sau phần tầng mặt sông; metadata phần tòa thêm `rental_signs`. Các generator không tự chạy main/installer. Lưu mới `ArtSource/Blender/PhoViet_FixtureDetails.blend` (~26,35MB), không ghi đè các .blend cục bộ cũ.
- **Đèn:** chân đế vát/neo bulông, cột thuôn/cổ cong nối cần ngang, giằng kim loại, hộp điện, housing vát góc/backplate viền xám và chụp che nắng cong mở phía trước. Giữ chính xác `head_center=[3,6.82,0]`, ba tâm runtime y+0,43/0/−0,43 và z−0,23, bán kính0,15; không đưa bóng cố định vào FBX hay đổi chu kỳ/màu logic. Collider metadata cũ giữ nguyên. `PV_TrafficSignal` LOD0/1/2: **1540/1060/676tri** (cũ358/304/158), vẫn dùng atlas/vật liệu cũ.
- **Điều hòa:** vỏ bo vát0,75×0,55×0,42m, dùng `chrome=D7DDE0`/`steel=A9B0B3` trong atlas cũ thay `white=FBF8F0`; mặt quạt lõm tối,5cánh sau lưới vòng/nan, tâm quạt, khe gió bên, nắp bảo dưỡng/ốc, giá chữL và đệm cao su/ống ởLOD0. LOD1 giản lược nan/khe/ống, LOD2 giữ khối/quạt/khe tối và màu xám trắng. Mesh được ghép vào24shop (12thường+12Petshop) và AC mặt đường của căn hộ; không tạo renderer/material/texture riêng hay animation quạt gây nhấp nháy. Tọa độ AC/bố cục nhà giữ nguyên; AC river scale0,93 tương đương kích thước cũ.
- **Bảng thuê:**24part `PV_RiverApt00–23`, mỗi part1bảng/3LOD cùng vị trí, seed độc lập từ tọa độ nên không tiêu thụ RNG bố cục nhà. Các tầng `[7,4,1,3,4,5,5,4,4,5,4,6,3,4,6,4,4,3,5,1,6,6,6,7]` tính từ tầng trệt0. Nềnnavy, chữtrắng font BeVietnamPro hiện có, viền kim loại mảnh và2ốc; cao0,88m/rộng min(0,68×bề ngang tòa,9m), ở khoảng giữa phần kính tầng được chọn, phía sông, không đổi cửa sổ/lan can. Không phát sáng riêng ban đêm; nhận ánh sáng cảnh hiện có. Metadata lưu label/floor/center/width/height/facing trong JSON từng part, chung atlas và renderer hiện tại.
- **Blender MCP:** tool tích hợp phiên báo `Transport closed` và không có Blender đang chạy; mở Blender5.2 riêng PID2336 với add-on hiện có/port9876, gọi thật MCP STDIO bằng `.tools/blender_fixture_mcp_client.py` tới server đã cài trong cache. Đã gọi get_addon_status/get_scene_info trước dựng; addon protocol4 có execute_code tương thích, không nâng/chỉnh config của Claude/Codex. Sau6shop đầu, auto-review chặn bước tiếp do MCP ghi log upload telemetry; đã gọi `disable_telemetry` thành công, client dùng cờ chính thức `BLENDER_MCP_DISABLE_TELEMETRY=1`, các bước còn lại không có upload ảnh/code/trajectory; server vẫn gửi2request `telemetry_events` thống kê sử dụng tối thiểu như thông báo của tool disable_telemetry. Không yêu cầu người dùng cho phép gửi dữ liệu. `fixture-details-privacy.json` ghi trạng thái tắt. Không coi transport lỗi hay telemetry rejection là lỗi game.
- **Đối chiếu Blender175đạt:** baseline tại `.tools/fixture-details-before` (fallback Git `438733a` cho generator/JSON). Mọi baseline LOD count khớp asset đã cài; thay AC mới về AC cũ phải tái tạo toàn bộ faces/vertex/color/tag/meta của từng shop; tương tự bỏ rental và trả ACcube cũ phải tái tạo đầy đủ24tòa/3LOD. Metadata collision/pets/chữ biển tiệm giữ nguyên, mỗi part đúng1rental/LOD và tầng lệch nhau. Báo cáo `Captures/fixture-details-blender-checks.json`, AC vị trí mẫu `fixture-details-positions.json`.
- **Unity:** `AgentScripts/FixtureDetailsImport.cs` Baseline/Import/Final không gọi builder. Import đúng26FBX, tải lại scene sạch từ đĩa để resolve renderer refs rồi so snapshot: **147mesh GUID/localID/submesh,5138mesh refs/materialbindings**, `10983renderer/7670obstacle` giữ nguyên; **2779LODGroup/0missing renderer,420bóng runtime**. Một lần export thử signal ẩn LOD1/2 trước export khiến thiếu2mesh; đã sửa thứ tự ẩn sau export, xuất lại đủ3LOD. Các link LOD còn null tạm trong scene memory đã hết sau reload scene đã lưu; không save trạng thái tạm. Kiểm cuối `fixture-details-unity-checks.json` và `fixture-details-final-editor.json`: Edit Mode/scene sạch/start(1,55;0;1,92)/jobsfalse/0temporary preview.
- **Ảnh:** `AgentScripts/CaptureFixtureDetails.cs` chụp11PNG1280×900 `Captures/fixture-details-{signal-lod0/1/2,signal-pole,ac-lod0/1/2,ac-facade,river-day,rental-close,rental-night}.png`, đã xem. Signal/AC dùng bản sao Play tạm trên cao để tránh cây che, hủy trongfinally; river là tile thật bật bridge tạm trongPlay rồi ResetRide. Lần chụp đầu sai dấuX camera (không phải asset lỗi), đã sửa theo trục mesh import và chụp lại; ảnh LODsignal cuối kiểm sau reload, khác nhau đúng mức chi tiết. Camera/daycycle/timeScale khôi phục, đã thoát Play. Không tuyên bố đã kiểm điện thoại thật/WebGL hay FPS không đổi; hình học chi tiết hơn nhưng vẫn3LOD, không thêm renderer/vật liệu/texture.
- **Scope:** SHA **3104file** Assets/ProjectSettings/Packages/docs so trước/sau chỉ **52file** dự kiến đổi (26FBX+26JSON),0unexpected/added/missing (`fixture-details-scope-checks.json`). Giữ byte scene/.meta, cây/cầu/thuyền/nước/xe/gameplay/config/web.4Python source parse đạt, script Unity kiểm/chụp compile không lỗi, `git diff --check` đạt. Commit cục bộ riêng source/assets/bằng chứng/tài liệu; **chưa build/push/phát hành**, live vẫn1.47.0. Blender riêng đang mở file nguồn này (ẩn), MCP9876 nhàn; các thay đổi source21.87–21.90 cũng chưa lên web.
