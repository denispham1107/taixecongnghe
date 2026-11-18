# Chạy trong Blender qua Blender MCP (execute_blender_code):
#   p='C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Blender/OpenWorld/run_in_blender.py'
#   ns={'TARGET':'build_ref3_segment.py'}; exec(open(p,encoding='utf-8').read(), ns); print(ns['REPORT'])
# Gom stdout và lỗi thành chuỗi REPORT (traceback.print_exc ghi ra stderr nên MCP không thấy).
import sys, io, os, traceback
OW_DIR = 'C:/Users/HP/Documents/GameUnity/Taixecongnghe/ArtSource/Blender/OpenWorld'
_buf = io.StringIO(); _old = sys.stdout; sys.stdout = _buf
_ns = {'OW_DIR': OW_DIR}
if 'PARTS' in globals(): _ns['PARTS'] = PARTS
_err = ''
try:
    _p = os.path.join(OW_DIR, globals().get('TARGET', 'build_ref3_segment.py'))
    exec(compile(open(_p, encoding='utf-8').read(), _p, 'exec'), _ns)
    if 'purge' in _ns: _ns['purge']()
except BaseException:
    _err = traceback.format_exc()
finally:
    sys.stdout = _old
REPORT = _buf.getvalue()[-2000:] + ('\nLỖI:\n' + _err if _err else '\nOK')
