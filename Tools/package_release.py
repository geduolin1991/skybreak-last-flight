"""Package the validated macOS app and an editable, cache-free Unity project."""
from pathlib import Path
import shutil, subprocess, tempfile, zipfile, hashlib, json, struct
from build_fingerprint import fingerprint

def mark_utf8_names(path):
    # ditto writes UTF-8 names without the language flag. Preserve compressed
    # payloads, resource forks and permissions while making names portable.
    data=bytearray(path.read_bytes())
    with zipfile.ZipFile(path,metadata_encoding='utf-8') as archive:
        cursor=archive.start_dir
        for entry in archive.infolist():
            assert data[cursor:cursor+4]==b'PK\x01\x02'
            name_len,extra_len,comment_len=struct.unpack_from('<HHH',data,cursor+28)
            assert data[cursor+46:cursor+46+name_len].decode('utf-8')==entry.filename
            local=entry.header_offset
            assert data[local:local+4]==b'PK\x03\x04'
            for offset in [cursor+8,local+6]:
                flags=struct.unpack_from('<H',data,offset)[0]
                struct.pack_into('<H',data,offset,flags|0x800)
            cursor+=46+name_len+extra_len+comment_len
    path.write_bytes(data)

root=Path(__file__).resolve().parents[1]
releases=root/'Releases'
releases.mkdir(exist_ok=True)
identity_path=root/'Build/build-identity.txt'
assert identity_path.exists(),'No source-matched build yet. Build and run native QA before packaging; the existing release is preserved.'
identity=identity_path.read_text().strip()
assert identity==fingerprint(root),'Source has changed since the playable build; build and validate again.'
for kind in ['runtime','campaign']:
    assert (root/f'Build/QA/{kind}-build-identity.txt').read_text().strip()==identity, f'{kind} QA belongs to a different build.'
assert (root/'Build/QA/PortraitMotion/portrait-build-identity.txt').read_text().strip()==identity, 'Portrait capture belongs to a different build.'
assert 'Succeeded' in (root/'Build/build-report.txt').read_text()
qa=(root/'Build/QA/runtime-results.txt').read_text()
assert 'FAIL' not in qa and qa.count('PASS ')>=166
assert 'PASS Natural campaign reaches ending: Victory' in (root/'Build/QA/campaign-results.txt').read_text()

for ship in [1,2]:
    folder=root/f'Build/QA/Ship{ship}'
    assert (folder/'campaign-build-identity.txt').read_text().strip()==identity
    assert 'PASS Natural campaign reaches ending: Victory' in (folder/'campaign-results.txt').read_text()
built_app=root/(root/'Build/build-app-path.txt').read_text().strip()
assert built_app.is_dir(), 'Recorded build app is missing.'

with tempfile.TemporaryDirectory(prefix='skybreak-package-') as tmp:
    staging=Path(tmp)/'SKYBREAK'
    staging.mkdir()
    (staging/'Build').mkdir()
    subprocess.run(['/usr/bin/ditto',str(built_app),str(staging/'Build/SKYBREAK.app')],check=True)
    subprocess.run(['/usr/bin/codesign','--force','--deep','--sign','-',str(staging/'Build/SKYBREAK.app')],check=True)
    subprocess.run(['/usr/bin/codesign','--verify','--deep','--strict',str(staging/'Build/SKYBREAK.app')],check=True)
    for name in ['README_开始试玩.md','开始游戏.command']:
        shutil.copy2(root/name,staging/name)
    shutil.copytree(root/'Docs',staging/'Docs')
    shutil.copytree(root/'Build/QA',staging/'Build/QA',ignore=shutil.ignore_patterns('Frames'))
    shutil.copytree(root/'Build/AudioPreview',staging/'Build/AudioPreview')
    shutil.copy2(root/'Build/build-report.txt',staging/'Build/build-report.txt')
    shutil.copy2(root/'Build/build-identity.txt',staging/'Build/build-identity.txt')
    subprocess.run(['/usr/bin/ditto','-c','-k','--sequesterRsrc','--keepParent',str(staging),str(releases/'SKYBREAK-macOS.zip')],check=True)
    mark_utf8_names(releases/'SKYBREAK-macOS.zip')

with zipfile.ZipFile(releases/'SKYBREAK-Unity-Source.zip','w',zipfile.ZIP_DEFLATED,compresslevel=6) as archive:
    for folder in ['Assets','Packages','ProjectSettings','Tools','Docs']:
        for p in sorted((root/folder).rglob('*')):
            if p.is_file() and p.suffix not in ['.blend1','.pyc'] and p.name!='extend_game.py':
                archive.write(p,Path('SkybreakUnity')/p.relative_to(root))
    for name in ['Build/QA/runtime-results.txt','Build/QA/campaign-results.txt','Build/build-identity.txt','Build/QA/runtime-build-identity.txt','Build/QA/campaign-build-identity.txt','Build/QA/PortraitMotion/portrait-build-identity.txt','Build/QA/PortraitMotion/capture-report.txt','Build/QA/PortraitMotion/visual-review.md','Build/build-report.txt']:
        archive.write(root/name,Path('SkybreakUnity')/name)
    archive.writestr('SkybreakUnity/源码工程说明.txt','这是可编辑 Unity 源码包，不含预编译游戏。使用 Unity 6000.6.0f1 打开工程，在 Skybreak 菜单构建 macOS 游戏。直接试玩请使用另一个 SKYBREAK-macOS.zip。\n')
    for ship in [1,2]:
        for name in ['campaign-results.txt','campaign-build-identity.txt']:
            p=root/f'Build/QA/Ship{ship}'/name
            archive.write(p,Path('SkybreakUnity')/p.relative_to(root))
    for name in ['README_开始试玩.md','在Unity中打开.command','.gitignore']:
        archive.write(root/name,Path('SkybreakUnity')/name)

manifest={}
for name in ['SKYBREAK-macOS.zip','SKYBREAK-Unity-Source.zip']:
    p=releases/name
    manifest[name]={'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()}
(releases/'checksums.json').write_text(json.dumps(manifest,indent=2)+'\n')
print(json.dumps(manifest,indent=2))
