# 21.372 — chạy nền: "C:/Program Files/Blender Foundation/Blender 5.2/blender.exe" -b --factory-startup --python run_cards_hd.py -- [main|featured|petshop]
import sys
OW = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Blender/OpenWorld'
argv = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
ns = {'OW_DIR': OW, 'MODE': 'render_cards_hd', 'PART': argv[0] if argv else None}
p = OW + '/build_interiors.py'
exec(compile(open(p, encoding='utf-8').read(), p, 'exec'), ns)
