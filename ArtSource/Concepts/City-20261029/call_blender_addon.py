"""Local fallback to the installed Blender MCP add-on's documented JSON socket.
Used only after the Codex MCP process reports Transport closed; no configuration changes.
Protocol checked against installed blender_mcp.py (_handle_client/execute_command).
"""
import argparse,json,socket
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--script');p.add_argument('--inspect',action='store_true');args=p.parse_args()
if args.script:
    s=Path(args.script).resolve()
    allowed=Path(__file__).resolve().parent
    if s.parent!=allowed or s.suffix!='.py':raise RuntimeError('Only this concept directory scripts may be run')
    code='exec(compile(open('+repr(str(s))+',encoding="utf-8").read(),'+repr(s.name)+',"exec"))'
    cmd={'type':'execute_code','params':{'code':code}}
else:cmd={'type':'get_addon_info','params':{}}
cmd['user_prompt']='Tôi muốn dựng lại tất cả nhà cửa trong game với phong cách y chang như hình (cây cối vẫn giữ nguyên không cần thay đổi),bạn hãy sử dụng blender mcp dựng lại tất cả nhà cửa giống như vậy nhé.Dựng xong để cho tôi xem,khi nào tôi duyệt mới cho vào game nhé'
with socket.create_connection(('127.0.0.1',9876),timeout=8) as client:
    client.settimeout(45);client.sendall(json.dumps(cmd,ensure_ascii=False).encode('utf8'));data=b''
    while True:
        chunk=client.recv(65536)
        if not chunk:raise RuntimeError('Blender closed connection before response')
        data+=chunk
        try:response=json.loads(data.decode('utf8'));break
        except (json.JSONDecodeError,UnicodeDecodeError):pass
print(json.dumps(response,ensure_ascii=False))
if response.get('status')!='success':raise SystemExit(1)
