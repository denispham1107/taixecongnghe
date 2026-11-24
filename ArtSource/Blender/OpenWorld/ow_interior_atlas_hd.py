"""21.372 — ghép atlas ảnh nội thất sau kính độ phân giải cao (Python ngoài Blender, cần Pillow).

Người dùng: «khi chơi trên mobile dù đã để mức đồ họa Rất cao nhưng hình ảnh mặt tiền cửa hàng vẫn rất mờ».
Gói điện thoại không có nội thất 3D (Editor/PhoVietMobileSceneStrip.cs) → sau kính chỉ là tấm ảnh chụp sẵn. Atlas cũ ô 192 × 144
(1920 × 864), bản web còn 1024 px → ~102 × 77 px cho cả mặt tiền một căn. Bản này:
  OW_NoiThat_AnhXa.png         54 ô (3 bề ngang × 18 loại) lưới 7 cột × 8 hàng, ô 584 × 438 → 4088 × 3504
                               (UV FBX cũ lưới 10 × 6 được Unity đổi sang lưới này: PhoVietInteriorMeshPostprocessor.RemapCardUv)
  OW_NoiThat_NoiBat_AnhXa.png  14 nhà nổi bật, lưới 5 × 3 giữ nguyên (UV không đổi), ô 816 × 560 → 4080 × 1680
  OW_NoiThat_PetShop_AnhXa.png 3 bề ngang, lưới 3 × 1 giữ nguyên, ô 1168 × 876 → 3504 × 876
Lật gương từng ô (MIRROR): tấm ảnh xa trong FBX (card_object / featured_card_object / petshop_card_object của build_interiors.py)
gán u0 ở mép y lớn — mà người đứng ngoài đường nhìn vào thì y lớn ở bên PHẢI → ảnh hiện ngược trái/phải trong game từ 21.272
(chữ «Bún Bò Huế» trên quầy đọc ngược, cầu thang sai bên). Lật ảnh trong atlas thay vì đổi UV để mọi FBX cũ dùng đúng luôn.
Unity nhập với nPOTScale ToNearest (→ 4096², 4096×2048, 4096×1024) để nén ETC2/DXT được — để None thì WebGL lưu RGB24 không nén.
Ảnh nguồn: render 1168 px bằng build_interiors.py MODE 'render_cards_hd' (run_cards_hd.py), thu nhỏ LANCZOS vào ô.
Chạy: python ArtSource/Blender/OpenWorld/ow_interior_atlas_hd.py
"""
import glob, os
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..'))
CON = os.path.join(ROOT, 'ArtSource', 'Concepts')
LIB = os.path.join(ROOT, 'Assets', 'PhoViet', 'OpenWorld', 'Library', 'NoiThat')

MIRROR = True

JOBS = [  # (thư mục ảnh, số ảnh, cột, hàng, ô, tệp ra)
    (os.path.join(CON, 'OpenWorld-20261117', 'NoiThat-21272', 'AnhXaHD'), 54, 7, 8, (584, 438), 'OW_NoiThat_AnhXa.png'),
    (os.path.join(CON, 'OpenWorld-20261117', 'NoiThat-21272', 'AnhXaNoiBatHD'), 14, 5, 3, (816, 560), 'OW_NoiThat_NoiBat_AnhXa.png'),
    (os.path.join(CON, 'OpenWorld-20261117', 'Petshop-21285', 'AnhXaHD'), 3, 3, 1, (1168, 876), 'OW_NoiThat_PetShop_AnhXa.png'),
]


def build(src, count, cols, rows, cell, name):
    files = {int(os.path.basename(f)[:3]): f for f in glob.glob(os.path.join(src, '*.png'))}
    missing = [i for i in range(count) if i not in files]
    if missing: raise SystemExit('%s: thiếu ảnh %s' % (src, missing))
    at = Image.new('RGB', (cell[0] * cols, cell[1] * rows), (40, 40, 40))
    for i in range(count):
        im = Image.open(files[i]).convert('RGB').resize(cell, Image.LANCZOS)
        if MIRROR: im = im.transpose(Image.FLIP_LEFT_RIGHT)
        at.paste(im, ((i % cols) * cell[0], (i // cols) * cell[1]))
    out = os.path.join(LIB, name); at.save(out, optimize=True)
    print(name, at.size, '%.1f MB' % (os.path.getsize(out) / 1048576))


if __name__ == '__main__':
    for j in JOBS: build(*j)
