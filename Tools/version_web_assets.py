"""Keep old browser caches from mixing different Unity releases."""
from pathlib import Path
import re,shutil

def version_assets(output:Path):
    index=output/'index.html'
    html=index.read_text()
    match=re.search(r"productVersion\s*:\s*['\"]([0-9.]+)['\"]",html)
    if not match:
        match=re.search(r'电脑浏览器试玩 · v([0-9.]+)',html)
    if not match:raise RuntimeError('Built page has no release version')
    version=match.group(1)
    build=output/'Build';destination=build/('v'+version)
    destination.mkdir(exist_ok=True)
    for file in list(build.iterdir()):
        if file.is_file():shutil.move(str(file),str(destination/file.name))
    html=html.replace('Build/Web.','Build/v'+version+'/Web.')
    index.write_text(html)
    assert (destination/'Web.loader.js').is_file()
    assert ('Build/v'+version+'/Web.data.unityweb') in html
    from harden_web import harden
    harden(output)
    return version

if __name__=='__main__':
    import argparse
    parser=argparse.ArgumentParser();parser.add_argument('output',type=Path);args=parser.parse_args()
    print('Versioned browser assets:',version_assets(args.output))
