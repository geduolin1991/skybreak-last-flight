"""Keep the current browser build and two recent rollback builds within Pages size.

Only generated Build/vMAJOR.MINOR.PATCH directories are eligible. Published
source, tagged downloads, and Git history are preserved. Unknown files stay.
"""
from pathlib import Path
import re, shutil

def prune_versions(output, keep=3):
    output=Path(output).resolve()
    assert keep>=2, 'Retain a rollback browser build'
    assert (output/'index.html').is_file(), 'Expected a generated player'
    root=output/'Build'
    versions=[]
    for path in root.iterdir():
        match=re.fullmatch(r'v(\d+)\.(\d+)\.(\d+)',path.name)
        if match and path.is_dir() and not path.is_symlink():
            versions.append((tuple(map(int,match.groups())),path))
    versions.sort(reverse=True)
    current=(output/'index.html').read_text()
    removed=[]
    for _,path in versions[keep:]:
        assert path.resolve().parent==root.resolve()
        assert ('Build/'+path.name+'/') not in current, 'Never remove the active player'
        shutil.rmtree(path);removed.append(path.name)
    return removed

if __name__=='__main__':
    import argparse
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('output');args=parser.parse_args()
    print('Retired generated browser builds:',prune_versions(args.output))
