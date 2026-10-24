# PHỐ VIỆT — Tài liệu tính năng, kiến trúc và bàn giao cho Claude Code

Cập nhật toàn diện: **24/10/2026**, múi giờ **Asia/Bangkok**; kế thừa tài liệu lập 18–19/10. Đã đọc toàn bộ bản cũ, đối chiếu mã nguồn/scene, lịch sử Git và báo cáo các bản 1.15.0–1.34.0. Các kết quả kiểm tra bên dưới là bằng chứng đã lưu của từng bản, không phải tuyên bố chạy lại toàn bộ chỉ vì sửa tài liệu.

> **Trạng thái hiện tại: 1.36.0 (xe tăng Meshy “Iron Sentinel” trong giao thông, trên nền 1.35.0: đổi làn kiểu đánh lái, nhà cao tầng kính trong suốt, zoom dọc +10%, trên nền phố Sài Gòn 1.34.0; giữ xe Meshy/độ nét mobile), chỉ thi quãng đường/kỷ lục. Toàn bộ công việc đưa đón/trả khách, GPS, khách và Coin trên HUD đang tắt. Chỉ mở lại khi người dùng yêu cầu rõ ràng. Không tự bật cờ công việc khi nâng cấp model, sửa lỗi hoặc build.** Chi tiết phát hành 21.32.
>
> **Bộ ô tô Meshy1.33.0 (được giữ trong1.34.0):** đã thay 11 xe bốn bánh bằng 4 mẫu Meshy, 8 màu sơn chọn lại mỗi lần xuất hiện; 3.002 điều kiện Unity và 28 kiểm tra nguồn đạt. **Đã build/push/phát hành 1.33.0**, qua 77 kiểm tra browser local + 29 kiểm tra bundle + 26 kiểm tra live, 0 lỗi runtime; Pages run `40021075002` thành công. Chi tiết 21.17–21.18.
>
> **1.35.0 (21.28–21.32, Claude Code):** zoom màn hình dọc gần hơn 10% (PortraitWorldScale .5775), nhà cao tầng phía sau có vách kính trong suốt/văn phòng sáng đèn, đổi làn kiểu đánh lái và lấy lại thăng bằng từ từ; sửa test tải Meshy (21.30). Build `build_8f58593f4444`, cache `d06d08624dba54fc`; **đã phát hành**: commit `b8c7b9f`, Pages run `40078793874`, 26 kiểm tra live đạt, 0 lỗi runtime (21.32).
>
> **1.36.0 (21.33–21.34, Claude Code):** thêm **xe tăng Meshy “Iron Sentinel”** vào giao thông: xuất hiện ngẫu nhiên ở 5 ô xe con (~1/3 số lần), một màu gốc, chậm hơn 60%, va chạm không đổi hướng/không dừng — mọi phương tiện khác (kể cả người chơi, CSGT) văng ra. 24 suite / 9.521 điều kiện Unity đạt; build `build_fa643a0c8cd3`, cache `2ad4c813b6952999`; 29 bundle + 77 browser local đạt, 0 lỗi runtime. **Đã phát hành**: commit `c49059c`, Pages run `40090621801` thành công, 26 kiểm tra live đạt, 0 lỗi runtime (21.34).
>
> **Phố Sài Gòn — đã phát hành1.34.0:** giữ toàn bộ cập nhật Claude21.19–21.26: 12shop/kính/nội thất/biển tủ, cây/chậu/vỉa hè/đồ vật, đèn cong/tín hiệu và mây, bỏ người đứng và xe đẩy cũ. Hồi quy mới1.642điều kiện Unity,29bundle,77browser local và26live đạt;0lỗi runtime. Pages run`40069532577` thành công, cache`28a1dc1e64a78083`. VerifyJump đã dùng chiều cao Meshy; lỗi CSGT chập chờn lịch sử chưa tái hiện ở hai lần chạy mới. Chi tiết21.27.
>
> **Độ nét mobile 1.32.1:** đã phát hành và xác minh 40 kiểm tra live, không lỗi runtime. DPR tối đa 3 / 3,2 triệu pixel, hai vùng texture 2K đầy đủ, người chơi giữ LOD0; chi tiết 21.15.
>
> **Người/xe Meshy (23/10/2026):** đã phát hành WebGL **1.32.0**: người Golden Star Stance có rig, hero áo đỏ sao vàng ngực/lưng, NPC nhiều màu áo, vai tự nhiên và ngón nắm v4. Xe hero trắng–đen, CSGT giữ nguyên cả người và xe. Pages run `39948530264` thành công, cache `96d0c318c468dcab`; 26kiểm tra live đạt. Xem21.9–21.14. Xe Meshy1.31.0 và các nguồn Wave ở21.5–21.8 là lịch sử.
>
> **Quy tắc bắt buộc từ 22/10/2026:** mỗi lần thay đổi, sửa lỗi hoặc thêm chức năng phải cập nhật đầy đủ `huongdan.md` trong cùng công việc. Ghi hành vi mới, cách triển khai/file liên quan, tham số, quyết định thay thế, kiểm tra và trạng thái commit/build/phát hành; cập nhật mục cũ bị thay thế. Đây là tài liệu dùng chung cho Codex và Claude Code.

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
| Phiên bản game đã phát hành | **1.36.0** — xe tăng Meshy trong giao thông, trên nền 1.35.0 (đổi làn đánh lái, nhà cao tầng kính, zoom dọc); chỉ thi quãng đường |
| Commit source + WebGL hiện hành | `c49059c6dc83ef970c758c6e2147b48890feb260` |
| Bằng chứng phát hành hiện hành | `Captures/release-1.36-*.json`, `VERIFICATION_RELEASE_1_36.md`; bản trước `release-1.35-*.json` |
| Cache WebGL hiện hành | `2ad4c813b6952999`; build UTC `2026-10-24T11:18:50.2444606Z` |
| Pages đã xác minh | Run `40090621801`, completed/success đúng source SHA; 26 kiểm tra live đạt (21.34) |
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
- **Xe máy (yêu cầu mới 23/10/2026):** dùng đúng mô hình đã nhận từ Meshy cho xe người chơi, xe máy thường hai chiều, quái xế và xe đậu. Xe chính **trắng–đen**, bốn phối màu còn lại **xanh ngọc, xanh dương, vàng, cam**. Giữ nguyên mô tô CSGT hiện tại theo xác nhận riêng của người dùng. Động tác giữ nguyên; người lái ngoài CSGT được thay bằng Golden Star Stance có rig (21.9). Giữ hình học và texture gốc xe Meshy (kể cả gương/chi tiết nhận diện vốn có), chỉ đổi vùng sơn; yêu cầu không gương/không logo của bản Wave cũ không áp vào mẫu mới này.
- **Mọi lần sửa/thêm chức năng đều phải ghi vào tài liệu này**, kể cả thay đổi chưa build hoặc chưa phát hành; ghi rõ trạng thái chưa hoàn tất thay vì bỏ qua.
- **Phố Sài Gòn (24/10):** cảnh quan nhà phố hai bên đường dựng theo ảnh mẫu `ArtSource/Reference/Street/saigon_street_reference.webp`, giữ gameplay (mặt tiền |x| = 9,3 m) và ngân sách WebGL; chậu cây được đưa lại theo ảnh mẫu. Đã được người dùng yêu cầu build/phát hành ngày24/10 và đã phát hành1.34.0; bằng chứng ở21.27. Xem 21.19.
- **Ô tô Meshy (24/10):** người dùng đã chọn dùng cả thể thao/bán tải/tải bạt/container, giữ tỷ lệ; hai mẫu tải được chọn ngẫu nhiên ở tải thường/ngủ gật, mọi xe chọn một trong tám màu sơn khi spawn. Kính/lốp/kim loại giữ màu riêng. Đã phát hành WebGL1.33.0. Xem 21.17–21.18.
- **Xe tăng Meshy (24/10, 21.33):** người dùng gửi mẫu “Iron Sentinel”; yêu cầu: xuất hiện ngẫu nhiên như ô tô con, chỉ một màu gốc (không đổi màu), va chạm với mọi phương tiện thì xe tăng không đổi hướng/không dừng, còn phương tiện kia văng ra; luôn chậm hơn ô tô khác 60%.

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
- Scene mới dùng nhân vật **Golden Star Stance có rig**: tóc đen, áo đỏ sao vàng ở ngực và chính giữa lưng, quần jeans, giày trắng; không đội mũ theo mẫu đã nhận. Đi xe Meshy trắng–đen. Các tài xế ngoài CSGT dùng cùng người, đổi áo và bỏ sao. Bộ người mới đã có trên web1.32.0 (21.14).
- Đường có 4 làn, vạch giữa đường vàng đứt nét, vạch trắng phân làn, vạch biên, vạch qua đường và giao lộ.
- Hai làn bên trái chủ yếu là xe ngược chiều; hai làn bên phải chủ yếu cùng chiều. Xe ngược chiều có thể lấn qua phía người chơi.
- Xe máy, sedan, hatchback, taxi, xe giao hàng, xe tải thường và xe tải ngủ gật cùng xuất hiện; từ 21.33 ô sedan/hatchback/taxi có thể ra **xe tăng** (~1/3 lần, chạy 40% tốc độ, không bị đẩy khi va chạm).
- Các lượt chơi dùng seed mới, không lặp nguyên xi mẫu xe/làn/tốc độ/lịch di chuyển của lượt trước.
- Quãng đường và tốc độ được hiển thị; có kỷ lục lưu cục bộ, số xe đã né, thông báo tình huống và truy đuổi.
- Không có mạng dự phòng: va chạm văng/té xong kết thúc lượt nếu không bị đuổi; nếu đang bị đuổi thì nằm chờ CSGT đến bắt và chuyển sang buồng giam. Góc/ngả/văng phụ thuộc tiếp xúc, không luôn đổ phải tại chỗ.
- 10 giây chạy đầu giảm một nửa mật độ/số xe (làm tròn lên), rồi trở về bình thường; pause không ăn thời gian này.
- Ngày–đêm lặp 60 giây, đèn xe/đường tự bật theo trời tối; sương sớm tan hết trong 6 giây đầu mỗi lượt.
- Hiện chỉ thi số mét và kỷ lục cục bộ; không có bảng xếp hạng online, không hiện menu việc/GPS/Coin.
- Có bắt đầu, tạm dừng/tiếp tục, chơi lại, về màn hình đầu và bật/tắt âm thanh.

### 2.2. Điều khiển

| Thao tác | Desktop | Điện thoại/tablet |
| --- | --- | --- |
| Bắt đầu | Nút “LÊN XE THÔI”, Enter hoặc Space | Chạm nút bắt đầu |
| Đổi làn/chọn rẽ | ←/→ hoặc A/D; rẽ theo nửa đường trong vùng hợp lệ | Vuốt trái/phải |
| Đạp xe máy | **Q** | Chạm hai lần gần nhau trong vùng chơi |
| Ngóc bánh/nhảy | **SpaceBar** | Vuốt từ dưới lên |
| Tăng tốc | **W** | Vuốt từ trên xuống |
| Tạm dừng/tiếp tục | P, Esc hoặc nút tạm dừng | Nút tạm dừng/tiếp tục |
| Chơi lại | R, Space hoặc nút chơi lại | Chạm nút chơi lại |
| Chọn công việc | **Đang ẩn/tắt** | **Đang ẩn/tắt** |

**Đổi làn kiểu đánh lái (21.29):** A/D/←/→ hoặc vuốt ngang làm xe đi theo đường cong sang làn kế (tăng tốc ngang 150 m/s², hãm êm 60 m/s²; một làn ~0,3 s, 90% làn sau ~0,26 s), đầu xe và người lái quay theo hướng đi (tối đa 26°) và nghiêng theo hướng đó, rồi **vừa chạy tới vừa trả lái, dựng xe từ từ ~0,6 s** (21.31); không còn trượt ngang thẳng đuột. Hộp va chạm giữ hướng đường. Không có ga/phanh thủ công. Tốc độ tăng theo thời gian, có nhảy và tăng tốc chủ động. Tăng tốc chỉ áp dụng cho xe người chơi. Trong cua vẫn giữ tốc độ như đường thẳng. Vuốt thực hiện ngay tại lúc đủ ngưỡng; một lần chạm tối đa một lệnh, giữ/nhấc tay không lặp lệnh. Bỏ các nút trái/phải và ô nhắc double tap dưới màn hình dọc.

### 2.3. Giao thông gây bất ngờ

- Xe cùng chiều có thể phanh gấp hoặc giảm tốc mạnh, có đèn phanh, rồi tăng tốc trở lại.
- Xe chạy song song có thể bất ngờ đổi làn. Chuyển làn được nội suy liên tục; xe lớn đổi làn chậm hơn xe máy.
- Xe thường ngược chiều cũng đổi làn, có thể vượt vạch vàng sang làn người chơi rồi quay lại; không đổi thành xe cùng chiều chỉ vì đổi làn.
- Xe băng ngang ở giao lộ đi từ cả hai phía; số lượng, tốc độ và tỷ lệ xe lớn tăng theo độ khó. Có xe đi thẳng và có xe rẽ vào đường chính.
- Xe chạy dọc đường cũng có thể rẽ trái/phải qua đường nhánh bằng quỹ đạo cong.
- Xe có thể va chạm và bị hất văng. Sau **3 giây từ lần va chạm mới nhất**, xe giao thông tự dựng (hoạt ảnh khoảng 0,65 s), nhập làn trống gần nhất đúng chiều cũ và chạy tiếp; nếu đang rẽ thì hoàn thành đích rẽ đã chọn. CSGT giữ 1,2 s. Ngoại lệ xe nguy hiểm đâm nhà cháy nằm lại.
- Xe thường vượt từ dưới màn hình lên có mũi tên đỏ viền vàng ở đúng làn ít nhất 2 giây trước khi xuất hiện; không áp dụng cảnh báo này cho xe từ phía trên hoặc xe băng ngang.
- Sau khi người chơi rẽ xong, có đợt xe ngẫu nhiên từ phía sau vượt lên và từ hướng ngược chiều đi tới.
- Đèn giao thông hiện là cảnh vật có đổi màu; game chưa dùng đèn để buộc tất cả xe dừng/nhường theo luật giao thông.

### 2.4. Quái xế và xe tải ngủ gật

- Có xe máy quái xế (hiện dùng thân xe Meshy màu cam, người lái Golden Star Stance áo xám đậm, không sao (scene mới; web 1.31.0 còn mũ fullface cam); trước đó thân Wave rồi côn tay kiểu Exciter) và xe mang biển **XE TẢI NGỦ GẬT**.
- Có cả cùng chiều lẫn ngược chiều, lấn làn và lạng lách qua phía người chơi.
- Xe cùng chiều xuất hiện từ ngoài camera phía sau. Mỗi đợt chỉ một xe nguy hiểm trên một làn cùng chiều được chọn ngẫu nhiên, báo mũi tên đỏ–vàng đúng làn trước 2 giây. Nếu còn xe nguy hiểm ngược chiều/chạy từ sau đang di chuyển thì đợi nó va chạm; khi đủ điều kiện lại phải báo đủ 2 giây. Quy tắc còn hiệu lực qua rẽ/rebase.
- Có thể húc và đẩy xe khác, gây rối giao thông và phản ứng dây chuyền.
- Có thể đâm nhà bên trái/phải, nằm lại với hiệu ứng lửa/khói. Phần đường giao nhau trống không bị coi là vách nhà.
- Người chơi có thể đạp ngã xe máy quái xế ở cả hai chiều để tự vệ; không thể dùng cú đạp để đạp trực tiếp ô tô hoặc xe tải.

### 2.5. Đạp xe, dây chuyền, CSGT và buồng giam

- Q/double tap chọn **một** xe máy hợp lệ gần nhất/ưu tiên quái xế, cùng chiều, ngược chiều, cắt ngang hoặc đang rẽ gần giao lộ. Tài xế tăng tốc bắt kịp, áp sát đúng bên rồi mới vung đúng chân; **chỉ hất khi giày chạm thân xe thật**, không tác động từ xa. Đạp xong trở về làn gốc; bị chắn thì hủy, không xuyên vật cản.
- Xe bị đạp ngã theo hướng ra xa người chơi. Xe ngã có thể kéo theo xe máy khác; xe lớn va vào đống xe cũng nhận phản lực theo mô hình vật lý.
- Có hồi chiêu đạp; không đạp khi tạm dừng, đang đón/trả khách hoặc đã thua.
- Đạp trúng xe máy thường lần đầu gọi mô tô CSGT từ phía sau rất nhanh. CSGT có đồng phục vàng cát, mũ, chi tiết cầu vai, đèn ưu tiên và còi.
- Người chơi khi đường thông thoáng có ưu thế tốc độ nhỏ sau khi CSGT áp sát; truy đuổi vẫn tăng nhịp theo thời gian.
- CSGT dự đoán xe, chọn lối né và phanh khi bị chặn; không được đi xuyên xe đang chạy, xe đứng yên hoặc xe ngã.
- Nếu CSGT vẫn va chạm, mô tô ngã **1,2 giây**, không bắt người chơi trong lúc nằm; dựng xe rồi tiếp tục từ vị trí thực tế.
- Đang bị truy đuổi mà chạm thật vẫn văng/té, không miễn va chạm. Người chơi nằm chờ; CSGT tìm đường tới vị trí bắt quanh người đã té, kể cả khi phải vòng cụm xe hoặc đi tạm xa trước rồi quay lại. Không tự dựng xe người chơi hoặc kết thúc bằng timer.
- Khi CSGT thực sự tới trong khoảng bắt và đã đứng dậy, chuyển `Arrested` → `Jailed`; nếu người chơi đang té phải đợi té xong. Nhân vật mặc đồ sọc trong buồng giam, có thông báo “CSGT ĐÃ ĐƯA BẠN VÀO TÙ”.

### 2.6. Rẽ đường và cảnh quan Việt Nam

- Sơn 16 cụm mũi tên ở mỗi giao lộ: ngoài trái rẽ trái; giữa trái thẳng+rẽ trái; giữa phải thẳng+rẽ phải; ngoài phải rẽ phải. Tất cả cùng màu sơn trắng, nhánh nối liền thân mũi tên, áp dụng cả bốn hướng.
- Mũi tên cùng màu sơn với vạch qua đường, được đặt cách vạch để không chồng lên nhau.
- Có thể chọn rẽ trong vùng mũi tên trước giao lộ, hoặc rẽ muộn khi đang ở lòng giao lộ và sát đúng phía lề.
- Xe ôm cua 90 độ, camera chuyển mượt rồi trở về góc nhìn sau xe; vẫn giữ quãng đường, độ khó, giao thông và truy đuổi.
- **Phố Sài Gòn theo ảnh mẫu (24/10/2026, scene/web1.34.0 đã phát hành —21.19–21.27):** nhà ống 3–5 tầng màu kem/vàng/be/hồng đào/xanh nhạt, cửa kính khung trắng, ban công lan can sắt đen, cục nóng điều hòa, mái ngói đỏ hoặc sân thượng có bồn nước, cây ban công, hoa giấy rủ, cờ đỏ sao vàng, biển dọc NHA KHOA/NHÀ NGHỈ/GIẶT SẤY và nhà cao tầng phía sau tạo chân trời. Tầng trệt là cửa hàng có **vách kính trong suốt khung tối/gỗ, cửa kính hai cánh có tay nắm**, nhìn thấy lòng tiệm sâu 3,4 m sáng đèn với kệ hàng/quầy/bàn ghế/bảng thực đơn (21.22; tiệm sửa xe để mở cửa cuốn), mái hiên sọc có riềm, biển lớn; vỉa hè caro kem–đất nung, bó vỉa vàng; đồ vật vỉa hè dựng chi tiết theo ảnh mẫu (21.23): chậu vuông cây lá to bản, thùng rác xanh có nắp/bánh, bàn gỗ nhỏ có khay chai tương ớt/ống đũa, ghế nhựa đỏ chân loe, bàn ghế gỗ tựa nan, bảng đen chữ A khung gỗ, tủ kính bày tô/rau + nồi nước dùng, tủ nước đỏ cửa kính thấy chai (không logo), kệ sắt bày chai, két nhựa, dù có riềm. Cây đường theo ảnh mẫu cây riêng: thân xám mảnh, cành chẻ, tán nhiều cụm lá xanh tươi (21.24). Cột đèn đường cong cao ~8,7 m màu xám, cột đèn tín hiệu cần vươn ra lòng đường với hộp đèn treo; trời ban ngày xanh đậm có mây tích trôi chậm (21.20). Người đứng vỉa hè **đã xóa** (21.21), xe đẩy bánh mì cũ **đã xóa** (21.26). Dây điện và xe đậu vẫn là bộ cũ.
- 12 mẫu cửa hàng: PHỞ BÒ - GÀ, TIỆM TRÀ, BÁNH MÌ, TẠP HÓA, THUỐC TÂY, SỬA XE, CÀ PHÊ SÀI GÒN, CƠM TẤM, BÚN BÒ HUẾ, HỦ TIẾU NAM VANG, SINH TỐ, TIỆM BÁNH (`VietnamStreet.Shops`, trùng chữ trên biển và tên đối tượng `Blender • …`).
- Có 48 tên đường Việt Nam trong `RouteNames`, gồm các tên người dùng chỉ định và các tên bổ sung; cùng một trục phố giữ cùng tên trong lượt hiện tại.
- Biển hiệu đã dựng song song, sát mặt tiền; chữ không xuyên qua nhà bên cạnh. Biển dọc chỉ nằm trong bề rộng tiệm. Mặt tiền vẫn ở |x| = 9,3 m nên luật đâm nhà, làn và vật cản giữ nguyên.

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

Space/vuốt lên ngóc bánh trước rồi nhảy tiến tới; hồi 2 s từ lúc bắt đầu, chỉ nhảy từ đường hoặc nóc đang đỡ, không nhảy thêm trên không. Đỉnh đo từ mặt đường khoảng 4,844 m; có thể vượt xe máy nằm và đáp/chạy trên sáu mẫu xe bốn bánh, gồm BoxTruck/DrowsyTruck đứng thẳng. Xe tải lạng lách hoặc đã va chạm nhưng vẫn đứng thẳng còn là mặt đỡ; xe nghiêng/lật là vật cản. Xe nhanh hơn chạy qua mép rồi rơi xuống; từ thùng xuống cabin thấp cũng rơi thật. Tiếp bằng hai bánh/trước/sau theo vận tốc, hấp thụ lực rồi cân bằng. Nhảy hụt đâm thân/vách vẫn gây té như thường.

### 2.10. Tăng tốc

W/vuốt xuống tăng **125% = nhân 2,25** tốc độ mong muốn trong 4 s; không cộng dồn, hồi 2 s sau hết/hủy. Chỉ xe người chơi tăng tốc; mọi xe khác giữ tốc độ nền/hành vi của chính nó. Dùng chung với nhảy được. Té/bị bắt/dừng dịch vụ hủy boost. Khói phản lực trắng mềm từ dữ liệu volume Blender bám ống xả và pose xe, không bay rời khi ngóc bánh; pause/rebase được xử lý riêng.

### 2.11. Ánh sáng, màu phố và sương sớm

Ban ngày nắng trắng, trời xanh sáng; nhà/vỉa hè/cây vẫn dùng palette rực rỡ, không trắng bệt. Mỗi 30 s chuyển dần ngày → đêm, 30 s tiếp theo đêm → ngày; lặp 60 s và pause dừng đồng hồ. Đèn trước/hậu cả xe người chơi/NPC/CSGT và 70 đèn đường bật dần theo độ tối. Đèn pha theo đúng độ cao/hướng xe lúc nhảy, đáp và đứng trên nóc. Sương phủ có chiều sâu trong 6 s đầu mỗi lượt, tan dần và tắt hẳn; không xuất hiện lại mỗi vòng bình minh.

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
| `PhoVietWelcome.cs` | Thẻ trắng trong mở đầu nhỏ, nút bắt đầu/hit-test |
| `PhoVietDayNight.cs`, `DayNightLighting.cs`, `MorningFog.cs` | Chu kỳ ngày–đêm, pool đèn và sương volume sáu giây |
| `WreckFireVisual.cs`, `WaypointMesh.cs` | Flipbook lửa/khói và mesh mũi tên khối vát |
| `PhoVietWebRuntime.cs` | Safe area, cầu nối web pause/insets, ký hiệu GUI WebGL, chất lượng đồ họa trình duyệt |
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

`Awake` cấu hình world text, âm thanh, CSGT và `ResetRide`. Reset tạo seed mới, xóa chuyến chưa hoàn thành/khách, đặt lại thành phố và đường, hành động/truy đuổi, xe nguy hiểm, giao thông, pool và camera; số dư Coin/kỷ lục đọc từ dữ liệu lưu.

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

`BeginSuddenLaneChange` dùng SmoothStep: xe máy khoảng 0,52 s, ô tô 0,7 s, xe tải 0,9 s. Xe thường cùng chiều đổi giữa làn 2/3; xe ngược chiều có thể bước sang các làn 0–3, kể cả lấn sang người chơi. Hành vi được kích hoạt theo cooldown và tình huống xe song song, không phải một animation dịch chuyển tức thì.

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

`PhoVietTrafficRecovery` ghi hướng/tốc độ/làn/hệ trục/đích rẽ trước xung lực đầu tiên. Va chạm mới đặt lại age. Sau 3 s, tìm làn trống đúng chiều, dựng 0,65 s rồi đi tiếp; đường rẽ lưu nối từ vị trí ngã tới đích cũ. Pose, vận tốc và đường phục hồi phải scroll/rebase cùng thế giới. Giữ slot đủ lâu để xe hồi phục; bị tông lúc đang dựng cũng phải nhận lực và reset timer. CSGT giữ cơ chế 1,2 s, không dùng hệ 3 s. Xe nguy hiểm wallCrashed nằm cháy không hồi phục.

`PhoVietObstacleUpgrade` tạo **735 nhóm / 8.050 hộp** nhẹ (phố Sài Gòn 24/10 sau 21.26, bỏ hộp “Xe đẩy bán hàng”; 21.23: 8.120; bản 21.19 5.515–7.955, trước nữa 4.575) theo nhà, biển, mái hiên, ban công, quầy, bàn ghế, chậu cây, thùng rác, tủ nước, kệ, dù, bảng thực đơn, cây, xe đậu và cột trên 5 road rows. Hộp shop/cây đọc từ JSON Blender. `StreetObstacleSet` lưu hộp trong local của cụm; `PhoVietSceneryContacts` quét trong đúng hệ trục kể cả row đã xoay và xét cao để nhảy qua vật thấp. Cảnh vật đứng yên, chỉ xe/người văng; không gắn mesh collider/PhysX thứ hai cho toàn cảnh. Đây là số đã kiểm tra của cảnh hiện hành, cần tái kiểm sau dựng lại scene.

## 8. Đạp, CSGT và cảnh bị bắt trong code

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

`CrashDuringPursuit` chốt lúc va chạm. Sau khi té hoàn tất, người chơi nằm lại; không tự lái/nhảy/boost/đạp hoặc chạy tiếp. `PhoVietCrashPoliceNavigation.cs` ưu tiên đường thẳng nếu thoáng, nếu chắn lập **A* lưới 1,2 m**, lưu waypoint và có thể chạy tạm xa để vòng ra khỏi túi xe. Mỗi bước kiểm tra thân có yaw, vật cản và dự đoán xe di chuyển; cập nhật đường khi vật cản/mục tiêu thay đổi, scroll và rebase waypoint đúng hệ.

Mục tiêu bắt là điểm có thể đứng quanh người té, không cố đi vào tâm đang nằm trong xe. Hỗ trợ hai nhánh mặt đường khi té giữa cua. Không có đường thì chờ tính lại, không xuyên/teleport/bắt từ xa bằng timer. CSGT té thì đợi dựng 1,2 s. Chỉ khi đã đứng dậy, người chơi té xong và khoảng cách thực <=2,2 m mới `Arrest` → `TickArrest` khoảng 1,1 s → `Jailed`. Jail là đối tượng riêng cùng scene; camera và ánh sáng riêng, có nút chơi lại.

## 9. Đường vô tận, giao lộ và phép rebase

### 9.1. Pool hình học

`RoadRow` gồm một root và 7 module 80 m. Có **5 rows**, tổng 35 module đã chuẩn bị: đường chính, nhánh chuẩn bị và các phần đường cũ cần giữ khi rẽ. `blocks` trỏ tới 7 module của đường hiện tại; không liên tục Instantiate cả thành phố trong runtime.

`ScrollRoutes` trượt đường ngược hướng xe; module qua phía sau được cộng lại 7 × 80 m để tái sử dụng. Giữ đồng bộ đường nhánh và `routeTrail` trong cùng phép scroll.

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
| Xe máy + người lái **Meshy (23/10/2026)** | HeroScooter, CityScooter, Underbone, SportBike, Racer dùng cùng hình xe đã nhập từ Meshy và màu khác nhau; TrafficPolice vẫn mô tô tuần tra v2 | Scene/web1.32.0: người Golden Star Stance có rig, 17 xương semantic/7 clip/3 LOD, 42.939/11k/5k tam giác. Atlas2048×4096, mỗi vùng người/xe2K đầy đủ; surface/normal/emission chung; người CSGT không đổi. Xem21.9. Bản web1.31.0 dùng người cũ (21.7) |
| Xe máy + người lái v2 (19/10/2026, **đã thay thế trừ CSGT**) | Tay ga trắng/tay ga nữ/xe số/côn tay/quái xế/CSGT | `Captures/VERIFICATION_BIKES_V2.md`; nguồn `pho_viet_motorcycles.py` vẫn là thư viện người lái/rig/xuất cho bản Wave |
| Xe bốn bánh — scene/web1.33.0 (24/10/2026) | 4 mẫu Meshy: thể thao, bán tải, tải bạt, đầu kéo container; giữ 6 tên role Sedan/Hatchback/Taxi/DeliveryVan/BoxTruck/DrowsyTruck; **từ 21.33 thêm `PV_MeshyTank` (xe tăng) làm mẫu thứ hai ở 5 ô xe con** | 15 xương hoặc 23 ở container 4 trục; 7 clip, 3 LOD, một material/mẫu; màu sơn ngẫu nhiên mỗi lần spawn (xe tăng giữ một màu gốc), hai mẫu tải được chọn lại ở cả tải thường/ngủ gật (21.17) |
| Phố Sài Gòn —scene/web1.34.0 (24/10/2026) | 12 shop, 3 cây, 1 module vỉa hè, cùng tên FBX cũ | Mesh tĩnh 3 LOD dựng theo mức chi tiết (không decimate), palette 128 px lưới 8×8 = 64 màu; chữ/mái hiên giữ ở mọi LOD; JSON kèm hộp vật cản. LOD0 shop 9.025–26.848 (21.28; shop có nhà cao tầng 21.180–26.848), cây 29.026 (21.24), vỉa hè 1.258 tam giác (21.19); shop có kính có submesh thứ hai dùng `PV_StreetGlass.mat` |
| Khách | PassengerMale, PassengerFemale | 14 xương/mẫu, 4 clip, 3 LOD, một palette material/mẫu |

Xe máy có vỏ cong, lốp/mâm/phanh/giảm xóc/động cơ/pô, gương/đèn/biển số; người ngoài CSGT dùng khuôn mặt/tóc/trang phục Meshy mới, không mũ; hero có sao ngực và chính giữa lưng. Mô tô CSGT có kính/túi hông và nhận diện riêng. Xe lớn có hốc bánh, kính, gương/gạt mưa, lưới tản nhiệt, thùng hàng, gân/cửa/bản lề/khóa.

Trong scene có **33 `BikeVisualAnimator`**: 19 actor gameplay tính cả người chơi và CSGT, 14 xe đậu có animator ở dãy nguồn. Xe đậu nhân bản ở các nhánh được bake tĩnh để tránh nhân toàn bộ skeleton/animator; không hiểu 33 là tổng mọi hình xe máy được render trên cả 35 module. Có **11 `CarVisualAnimator`** trên các xe bốn bánh giao thông.

Tam giác ví dụ hiện hành: HeroScooter với người rigged **42.939 / khoảng11.000 / khoảng5.000**; metadata `Art/Bikes/MeshyRiders/*.json`. Bản web1.31.0:30.423/10.999/4.999 trong thư mục Meshy cũ. Ô tô Meshy từ scene/web1.33.0: Sport **16.488/7.254/2.638**, Pickup **20.575/9.053/3.291**, Covered **13.481/5.931/2.156**, Container **7.582/3.335/1.213**; bộ cũ ở web1.32.2 DrowsyTruck **23.503/10.340/3.760**. Khách nam **4.688/2.338/1.091**, khách nữ **5.084/2.541/1.160** (đang khóa). Bản Wave cũ có 35.604/16.020/5.200 và không còn được scene dùng trừ rider/rig kế thừa.

### 11.2. Source chỉnh sửa và điểm vào tái tạo

| File trong `ArtSource/Blender/` | Script | Hàm tái tạo |
| --- | --- | --- |
| **`PhoViet_MeshyRiders.blend`** (người hiện hành) | `integrate_meshy_riders.py` + `prepare_meshy_rider_atlas.py` | Blender --phase prepare → Python atlas → Blender --phase build; dùng rig/weights nguồn mới, xuất MeshyRiders; giữ CSGT |
| **`PhoViet_MeshyMotorcycles.blend`** (nguồn xe1.31.0, 23/10) | **`prepare_meshy_bike_atlas.py` + `integrate_meshy_bikes.py`** | Tạo atlas bằng Python rồi ghép hình học FBX Meshy với người lái/rig cũ trong Blender, xuất 5 FBX mới vào `Art/Bikes/Meshy/`; không xuất CSGT, không ghi đè Wave/nguồn Meshy. File blend chứa cả 5 rig đã xuất |
| `PhoViet_Motorcycles_Wave.blend` (nguồn cũ giữ lại) | `pho_viet_wave.py` / `pho_viet_motorcycles.py` | Chỉ dùng khi cần bảo trì nguồn cũ; không chạy để thay thế lại bản Meshy hiện hành |
| `PhoViet_Motorcycles.blend` (v2, giữ nguyên, có sửa cục bộ chưa commit có từ trước) | `pho_viet_motorcycles.py` (`pho_viet_bikes.py` chỉ còn là helper cho script khác; `rebuild_all()` cũ đưa xe về v1) | `build_motorcycles()` dựng lại **cả 6 mẫu v2, sẽ ghi đè FBX Wave** — chỉ dùng khi muốn quay về v2 hoặc để xuất lại riêng CSGT bằng `build_motorcycles(['PV_TrafficPolice'])` |
| **`PhoViet_MeshyCars.blend`** (scene mới) | `integrate_meshy_cars.py` + `prepare_meshy_car_textures.py` | Blender nền xuất 4 rig/LOD/7 clip + profile nóc; Python tạo mask sơn/đèn, giữ maps nguồn; Install received Meshy cars |
| **`PhoViet_MeshyTank.blend`** (xe tăng, 21.33) | **`integrate_meshy_tank.py`** | Blender nền `blender -b --factory-startup -P ArtSource/Blender/integrate_meshy_tank.py`: tỉ lệ dài 7 m, rig 15 xương (2 trục bánh ảo không gắn mặt), 7 clip, 3 LOD, profile nóc; chép maps 2K, mặt nạ sơn/emission đen; rồi `AgentScripts/InstallMeshyTank.cs` |
| `PhoViet_Cars.blend` (nguồn cũ, có chỉnh cục bộ cần giữ) | `pho_viet_cars.py` | `build_all()` chỉ tái tạo bộ cũ; không dùng để thay bộ Meshy mới |
| **`PhoViet_SaigonStreets.blend`** (phố hiện hành, 24/10) | **`pho_viet_saigon_streets.py`** | `build_saigon_streets()` trong Blender MCP: scene riêng `PV_SaigonStreets`, xuất đè 16 FBX/JSON + palette + catalogue rồi lưu blend mới; sau đó chạy `AgentScripts/InstallSaigonStreets.cs` |
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
- **Dùng menu `Phố Việt/Art/Install received Meshy motorcycles (keep police)`**, hoặc `AgentScripts/UpgradeBikesMeshy.cs` → `UpgradeBikesMeshy.Main`. Trình cài mới thay đúng 32 actor ngoài CSGT, bake lại 3 mesh xe đậu nhánh giữ GUID và đổi vật liệu trên 56 bản sao, dựng lại neo đèn/nozzle, làm mới emission users rồi lưu scene. Menu chung `Replace all motorcycles with Blender models` và `UpgradeBikesWave` không phải quy trình cài hiện hành: chúng có thể thay cả CSGT hoặc làm mất neo đèn. `ModelPath` ưu tiên `Bikes/MeshyRiders/`, sau đó `Bikes/Meshy/`, cuối cùng asset cũ nếu chưa có. Menu Install received Meshy riders (keep police) cũng dùng chung trình cài.
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
- `PhoVietWelcome` dùng thẻ trắng trong alpha 0,93, viền/bóng nhẹ, tiêu đề xanh đậm và đỏ; kích thước dọc 432×258, ngang 360×276 trong tọa độ GUI. WelcomeStartRect dùng chung vẽ/hit-test; không còn thẻ vàng lớn cũ.

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
- Bộ nhớ khởi đầu 128 MB, tối đa 512 MB.
- `dataCaching = false`, vì service worker phiên bản hóa chịu trách nhiệm cache thay vì hai cache độc lập.
- Cấu hình phiên bản build hiện 1.34.0; `runInBackground` cấu hình build khác với ý định editor, còn web có bridge pause khi mất visibility.

Hậu xử lý build tạo `precache.json`, `build-info.json`, `sw.js` đã thay mã cache và `.nojekyll`. Cache là 16 ký tự hex từ SHA-256 nội dung tài nguyên, không gán tay để làm giả một lần build mới.

### 15.2. Quy trình phát hành

1. Kiểm tra Git/scene đúng, lưu thay đổi cần giữ và thoát Play Mode trước chỉnh scene/build.
2. Chạy kiểm tra phù hợp thay đổi, đọc lỗi compile/scene/runtime, xem hình nếu thay art/GUI.
3. Khi tăng version, đồng bộ ít nhất `PhoVietWebBuild.cs`, `AgentScripts/ConfigureWebRelease.cs`, kỳ vọng trong `VerifyWeb.cjs`/`VerifyFullscreenGame.cjs`, README và `PlayerSettings.bundleVersion` qua configure.
4. Cấu hình rồi build WebGL từ Unity đang mở hoặc batchmode độc lập; chờ kết quả thành công thật, không coi trạng thái `queued` là đã build xong.
5. Sao chép toàn bộ output cuối sang `docs`, xóa các file hash build cũ không còn trong output; giữ shell và binary cùng phiên bản.
6. Chạy server HTTP và kiểm tra local/offline/touch/layout trên chính thư mục `docs` sẽ publish.
7. `git diff --check`, xem staged files, commit source/asset/scene và docs đã kiểm tra; không kéo các chỉnh sửa khác chưa liên quan vào commit.
8. Push `main`, chờ đúng workflow Pages cho đúng SHA thành công.
9. Kiểm tra live bằng `VerifyPublishedGame.cjs`; version/cache và nội dung tài nguyên phải khớp artifact local.
10. Lưu báo cáo và kết quả; trả Unity về trạng thái dễ chơi, khôi phục dữ liệu người dùng và dừng server kiểm tra cục bộ.

**Trước commit tính năng/sửa lỗi:** cập nhật `huongdan.md` cùng source, bao gồm trạng thái đang chờ phát hành. Khi publish xong cập nhật bằng chứng SHA/run/cache/live; không để tài liệu nói chưa phát hành mãi. Không rebuild Unity cho thay đổi tài liệu đơn thuần.

Ví dụ CLI đã dùng:

```powershell
unity command run_script --file AgentScripts/ConfigureWebRelease.cs --entry ConfigureWebRelease.Main --timeout_ms 60000 --timeout 70 --json
unity command build --target WebGL --outputPath Builds/WebGL --confirm true --json
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

Nếu chỉ sửa CSS/JS/manifest, `PhoVietWebBuild.RefreshWebShell()` có thể cập nhật shell từ một build tốt rồi tính cache lại. Hàm này hiện copy một danh sách file cụ thể; sửa `index.html` hoặc tài nguyên ngoài danh sách phải đọc/điều chỉnh đúng quy trình. **Sửa C#/FBX/scene/âm thanh phải build lại**, không dùng refresh shell để bỏ qua WASM/data cũ.

### 15.3. Chạy kiểm tra web

Chạy server ở một terminal, các lệnh Node ở terminal khác. Máy người nhận tự chọn Python/Node đã cài; không phụ thuộc đường runtime riêng của Codex.

```powershell
python -m http.server 8765 --bind 127.0.0.1 --directory .
```

```powershell
node AgentScripts/VerifyWeb.cjs http://127.0.0.1:8765/docs/ handoff
node AgentScripts/VerifyPublishedGame.cjs https://denispham1107.github.io/taixecongnghe/ handoff-live
```

Nếu Playwright/sharp không ở `node_modules` thông thường, cấu hình `NODE_PATH` tới thư mục package đã cài. Dùng cùng môi trường mạng cho server localhost và Chrome; đã có trường hợp shell sandbox mở server nhưng Chrome ngoài sandbox không truy cập được. Không sửa code game để chữa lỗi kết nối localhost của môi trường kiểm tra.

Các file `Captures/browser-*.json/png` bị gitignore. Muốn lưu bằng chứng phát hành vào Git, copy kết quả đã kiểm tra sang tên có nghĩa như `jobs-web-results.json`; không chỉ báo “pass” mà không lưu bằng chứng khi cần bàn giao.

### 15.4. GitHub/PWA

Workflow `.github/workflows/pages.yml` chạy khi push `main` có thay đổi `docs/**` hoặc chính workflow. Nó kiểm tra artifact, upload `docs`, deploy bằng GitHub Pages. Nó **không biên dịch Unity từ source**. Chỉ push C# mà không build/copy docs thì người chơi trên web vẫn chạy bản cũ.

`AgentScripts/GitHubProject.ps1` có mode `Inspect`, `EnablePages`, `Status`; dùng Git Credential Manager trong process. Không in token, không đưa credential vào tài liệu/repo, không chạy `EnablePages` lại khi chỉ cần đọc trạng thái.

Service worker dùng cache theo scope và hash build. Cài cache mới chỉ thành công khi tải đủ tài nguyên, lỗi thì xóa cache chưa hoàn chỉnh. Không `skipWaiting` cưỡng bức thay binary giữa lượt; bản cũ có thể còn được dùng đến khi các tab/app cũ đóng. Sau khi activate mới dọn cache phiên bản cũ cùng scope.

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

1. Đọc tài liệu này, yêu cầu mới nhất của người dùng và `git status --short`/branch/remote. Đọc `AGENTS.md` hoặc hướng dẫn dự án nếu có thêm về sau.
2. Xác định scope và những gì chưa chắc chắn; nếu dưới 90%, hỏi phương án trước khi làm phần đó.
3. Mở đúng scene, kiểm tra Unity CLI/MCP đang trỏ đúng project; không dùng port/PID cũ như hằng số.
4. Xác định source file + scene/asset bị ảnh hưởng, kiểm tra trạng thái đang Play/Edit và các chỉnh sửa chưa commit.
5. Nếu nhiều agent/công cụ cùng làm, phân công file rõ ràng và báo cho người dùng; không cùng lúc sửa scene hoặc chạy mutation vào cùng Unity/Blender. Không tự spawn/nhắn một agent khác nếu chưa được cho phép theo môi trường làm việc.
6. Thực hiện thay đổi nhỏ đúng phạm vi, giữ các hợp đồng gameplay nêu trên. Đọc lại Git diff trước khi sửa cùng partial class hoặc shared scene; thống nhất người phụ trách import/build/scene với người dùng nếu hai công cụ đang cùng chạy.
7. Chạy kiểm tra phù hợp, xem ảnh/render nếu có thay hình, rồi mới build/deploy khi phạm vi đã được người dùng cho phép.
8. **Bắt buộc cập nhật huongdan.md trong cùng lần sửa/thêm chức năng**, gồm sửa mục cũ và thêm chi tiết mới: yêu cầu/quyết định, hành vi, file/hàm/tham số/asset/scene, kiểm tra/bằng chứng, build/version/cache/SHA/run/live nếu đã có; nếu chưa xong/chưa publish phải nói rõ. Không đợi người dùng nhắc, không chỉ ghi ở README/report riêng.
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

> Hãy đọc toàn bộ `huongdan.md` tại gốc dự án và `git status` trước khi sửa. Game Phố Việt đã phát hành WebGL/PWA **1.34.0** với cảnh phố Sài Gòn/kính/nội thất/cây/đèn/mây mới của Claude ở21.19–21.27, giữ 4 mẫu ô tô Meshy và 8 màu sơn ngẫu nhiên mỗi lần xuất hiện ở 21.17–21.18, đuôi xe ổn định ở 21.16, giữ độ nét mobile ở 21.15, người/xe máy Meshy nhiều màu, xe chính trắng–đen, CSGT giữ riêng: hiện **chỉ thi quãng đường/kỷ lục, công việc/GPS/khách/Coin HUD đang khóa**, chỉ mở lại khi tôi yêu cầu. Q đạp có áp sát/chạm giày thật, Space nhảy và chạy nóc, W +125%/4 s/hồi 2 s; rẽ giữ tốc độ và đúng số làn; mobile vuốt nhận ngay đủ ngưỡng. Không có ba mạng: té thường kết thúc sau té, té đang bị CSGT đuổi thì nằm chờ cảnh sát tới bắt rồi vào tù. Không ghi đè file Blender cục bộ, không dựng lại scene toàn bộ theo thói quen. Nếu chưa chắc 90% hãy đưa phương án và chờ tôi xác nhận phần đó. Phân chia file/công cụ rõ ràng khi làm song song với Codex; không cùng mutation Unity/Blender. **Mỗi lần sửa/thêm chức năng phải cập nhật đầy đủ huongdan.md ngay trong cùng công việc**, ghi hành vi/triển khai/tham số/kiểm tra và trạng thái source/build/Pages. Đọc lại tài liệu mới nhất trước khi lưu để không mất phần người khác bổ sung.


## 19. Nhảy, nóc xe và boost — tham chiếu triển khai hiện hành

### 19.1. Dữ liệu animation từ Blender

`pho_viet_jump.py` chạy trong Blender thật trên `PV_HeroScooter_Rig`, tạo năm action JumpTakeoff/JumpAir/LandBoth/LandFront/LandRear, mỗi pha 61 mẫu, cùng JumpDemo để xem quỹ đạo đầy đủ. Lưu nguồn `PhoViet_Jump.blend` riêng, xuất bảng pose `Assets/PhoViet/Resources/PhoVietJump.json`. Không cần thay sáu FBX/17 xương/bảy clip Legacy cũ chỉ để thêm nhảy.

`BikeJumpPose` đọc/nội suy bảng; `BikeVisualAnimator` phối pose, co gối/tay/chuyển trọng tâm, xoay thân quanh điểm bánh sau, hấp thụ khi đáp. Chuyển góc bằng damping, giữ điểm thấp nhất trên mặt đỡ; nếu khách được mở lại thì `PoseJumpPassenger` bám cả lift/pitch. Visual không tự điều khiển quỹ đạo root.

### 19.2. Vật lý và mặt đỡ

`PhoVietJump` quản lý windup 0,24 s, phóng 14,6 m/s, gravity 22 m/s², hồi 2 s từ lúc bắt đầu. Đỉnh thực đo từ mặt đường khoảng 4,844 m. Lúc bay cộng 3 m/s tiến trước W; vẫn steer và có thể boost. Không nhảy lần hai giữa không trung; chỉ nhảy từ đường hoặc roofVehicle đang đỡ, hết pha tiếp đất/cooldown. JumpAir lấy theo vận tốc Y để khớp các độ cao rơi khác nhau, không dùng đồng hồ bay cố định cũ.

Scene/web1.33.0 dùng `MeshyCarVariants`: kích thước/hình bao và 49 mẫu mặt nóc lấy từ giao tia xuống tam giác thật, đổi đồng thời khi chọn mẫu tải. `JumpRoofHeight` và `RoofSurfaceAt` đọc profile này; không dùng chiều cao xe tải cũ cho container mới. Fallback cho bộ cũ/web 1.32.2: sedan/taxi 1,60 m, hatchback 1,69 m, DeliveryVan 1,91 m, thùng tải 3,175 m/cabin 2,71 m. Góc pitch/roll vượt 12° không còn mặt đỡ. Xe tải đã va chạm nhưng còn đứng thẳng vẫn có nóc; xe nhỏ knocked/wallCrashed bị loại. Model inactive/pool bị thu hồi cũng không đỡ người chơi. Chi tiết và kiểm tra mẫu mới ở 21.17.

Sửa 1.30.5 đồng bộ mặt nóc với **vị trí/yaw của xe tại đúng thời điểm va chạm**, không lấy mặt cuối frame cho va chạm đầu frame. Ô tô xét cả bánh trước và sau, không hạ root vào kính/capo khi bánh sau còn trên nóc cao. Xe tải giữ điểm tựa bánh sau trên thùng tới khi qua mép; sau đó rơi xuống cabin, không snap xuống.

`ResolveJumpSurface` quét toàn đoạn để tìm đáp lần đầu, kể cả đáp rồi qua mép trong một frame tốc độ cao. Dùng phần thời gian dư sau chạm để chạy tiếp trên mặt đỡ/rời mép. Chỉ khảo sát xe gần vùng quét để giới hạn chi phí. Collision Y/XZ dùng cùng contact/support, tránh hộp bảo thủ coi đang được đỡ là đâm xe. Đâm vách thùng từ cabin thấp, nhảy muộn/thiếu cao hoặc xe lật vẫn té thật.

Tiếp đất chọn LandBoth/Front/Rear theo vận tốc xuống và chuyển động tương đối; chân/root theo mặt đỡ, cân bằng rồi tiếp tục. Bắt đầu té hủy trạng thái nhảy nhưng chuyển vận tốc Y hiện có sang vật lý văng. Rebase khi cua chỉ đổi mặt phẳng, giữ độ cao/vận tốc đứng; camera theo lift nhẹ và giữ xe trong khung.

### 19.3. W và khói phản lực

`PhoVietBoost`: BoostMultiplier=2.25, BoostDuration=4, BoostRecovery=2. TryBoost chỉ Riding, không ServiceStopped/cooldown/đang boost; không stack hoặc kéo dài khi spam. `CancelBoost` hủy ngay, đặt hồi 2 s nếu đang boost, xóa khói. TickBoost đóng băng khi pause, xử lý phần thời gian dư lúc boost kết thúc để cooldown không lệch frame. Va chạm/bị bắt/dịch vụ hủy; reset lượt xóa tất cả. W có thể chạy cùng nhảy và qua rẽ.

`pho_viet_boost_smoke.py` dựng volume khói trắng có mật độ noise 4D trong Blender, render Cycles bốn frame RGBA, atlas 768×192 tại `Resources/Boost/PV_WhiteBoostSmoke.png`. Import Clamp/Bilinear, alpha, không mip/nén làm lem ô. Nguồn khói hiện gắn vào socket `PV_MeshyExhaust` trên xương Frame, vị trí metadata Meshy `(.23,.40,-1.0)` trong hệ xe. `BoostSmokeVisual` tự tìm socket, localPosition zero; fallback `(.214,.45,-1.02)` chỉ dùng cho scene Wave cũ không có socket. `BoostSmokeVisual` có luồng hẹp sát pô và lớp khói nở sau, tối đa 128 hạt, mô phỏng theo hệ đường. Nguồn phát bám Frame/pose thực khi ngóc bánh/nhảy; vệt đã phụt không quay theo đổi làn. Pause/resume cùng game; sau rebase xóa vệt cũ rồi phát lại mà không mất thời lượng W.

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
- **Kiểm tra (Play Mode, art cuối):** StreetArt 332, CrashUpgrade 97, Gameplay 23, JunctionRoutes 92, DistanceMode 31, NightJumpLighting 88, CrashOutcome 51, SmoothTurns 468, QuickKick 62, JunctionKick 216, Boost 35, Gestures 39, MeshyCarRoofs 256, MeshyLighting 16 đạt trong lượt chạy chung; **CrashPoliceNavigation** báo “Unbounded police movement in Moving traffic…” trong lượt chạy chung (lần thứ hai trong ngày) rồi **đạt 214 ở hai lần chạy lại riêng**. Kịch bản dùng giao thông ngẫu nhiên; không đổi mã CSGT/vật cản liên quan — cần theo dõi/làm ổn định test riêng. Tổng 15 suite / 2.020 điều kiện đạt khi tính lần chạy lại.
- **Ảnh:** `Captures/saigon-street-shops-sheet.png` (cận 12 loại tiệm, `CaptureSaigonStreet.Shops`), `saigon-street-shop-00..11.png`, `saigon-street-after-*`, `saigon-street-day-*`, `saigon-street-night-*`, `saigon-street-comparison.png`.
- **Trạng thái:** scene đã lưu, Unity Edit Mode, jobs khóa. **Đã commit/build/push/phát hành1.34.0, bằng chứng ở21.27** (trạng thái chờ của lần dựng đã kết thúc). Người ngồi trên xe máy đậu vỉa hè vẫn giữ (chờ người dùng chọn, 21.21).

### 21.24. Dựng lại cây xanh theo ảnh mẫu cây (24/10/2026, Claude Code — đã phát hành1.34.0 ở21.27)

- **Yêu cầu:** cây trong game quá xấu; dựng lại giống ảnh cây mẫu (mọi chi tiết và tông màu). Ảnh đã chép vào `ArtSource/Reference/Street/tree_reference.webp`.
- **Quan sát ảnh mẫu:** thân xám mảnh thẳng, hơi nghiêng, chiếm ~40% chiều cao; cành xám chia lên từ đỉnh thân; tán tròn không đều rộng hơn cao, gồm nhiều cụm lá dày có khe hở lộ cành/bóng; lá bầu dục to bản; xanh tươi ngả vàng (sáng ở ngọn và mặt ngoài, đậm ở trong/dưới).
- **Blender `pho_viet_saigon_streets.py::tree`:** thân côn r 0,10→0,065 m tới ~3,7–4 m (lệch nhẹ theo seed), 5 cành chính r 0,055→0,032 m tới nút trong tán, cành con nối nút gần nhất tới 40/30/20 cụm lá (bán kính cụm 0,36–0,55 m, vỏ tán 0,6–1,05). Mỗi cụm 88/50/33 lá xếp chùm quanh tâm cụm, mặt lá quay ra ngoài và hơi lên; lá dài 0,21–0,30 m (LOD xa phóng 1,3/1,6). `Geo.leaf` thêm tham số `wpos` (vị trí chỗ rộng nhất) để lá bầu dục thay vì hình thoi nhọn. Màu theo độ cao/khoảng cách tới tâm tán; palette đổi `leaf1 4C8E2A`, `leaf2 5FA834`, `leaf3 78BE42`, `leaf4 9CD25C`, `leaf5 2F5F1F`, `trunk 8E8B86` (xám). Các cây/chậu/hoa khác dùng chung các màu lá này nên cũng tươi hơn.
- **Ngân sách:** 29.026 / 12.414 / 5.446 tam giác (dưới mức 30.076 của bộ cây 1.13); lá 3.520 / 1.500 / 660. Vật cản thân/bồn giữ nguyên.
- **Unity:** cài bằng `InstallSaigonStreets` (reimport, bounds, vật cản). Ảnh so sánh `Captures/saigon-street-tree-comparison.png` (ảnh mẫu bên trái, cây trong Unity bên phải, `CaptureSaigonStreet.Tree`), `saigon-street-tree-closeup.png`; ảnh ngày/đêm và so sánh phố đã chụp lại.
- **Kiểm tra (Play Mode):** StreetArt 332, CrashUpgrade 97, Gameplay 23, JunctionRoutes 92, DistanceMode 31, NightJumpLighting 88, CrashOutcome 51, SmoothTurns 468, QuickKick 62, JunctionKick 216, Boost 35, Gestures 39, MeshyCarRoofs 256, MeshyLighting 16 đạt. **CrashPoliceNavigation** lại lỗi “Unbounded police movement in Moving traffic…” trong lượt chạy dài 15 suite (lần thứ ba trong ngày) nhưng **đạt 214 ở 5 lần chạy riêng/ngay sau CrashOutcome**; lỗi gắn với chuỗi suite dài trong một phiên Play Mode, không phải hình học cây. Cần một việc riêng để làm ổn định kiểm tra này (xem trạng thái tích lũy giữa các suite, PoliceSpeed/dt).
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

**Nguồn kiểm chứng chính:** mã C# và cấu hình hiện tại trong repo, `ArtSource/Blender`, metadata asset, Git history, các báo cáo `Captures/VERIFICATION_*.md` và JSON kết quả tương ứng. Nếu dự án tiếp tục thay đổi, cập nhật lại tài liệu này theo hành vi thực tế; đừng coi một con số/version/port của ngày bàn giao là thông tin luôn đúng về sau.
