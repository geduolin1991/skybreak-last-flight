"""Reproduce original score and SFX; see Docs/音乐与战机更新1.3.md for renderer setup."""
from pathlib import Path
import subprocess,sys,shutil
folder=Path(__file__).resolve().parent
for name in ['build_legacy_effects.py','compose_score.py']:
 subprocess.run([sys.executable,str(folder/name)],check=True)
node=shutil.which('node')
if not node:raise SystemExit('Node.js is required for sampled instrument rendering.')
subprocess.run([node,str(folder/'render_score.mjs')],check=True)
subprocess.run([sys.executable,str(folder/'master_score.py')],check=True)
subprocess.run([sys.executable,str(folder/'build_nova_audio.py')],check=True)
