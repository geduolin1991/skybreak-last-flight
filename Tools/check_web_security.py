"""Verify generated security policy, asset integrity, idempotence and path boundaries."""
from pathlib import Path
import re
import shutil
import tempfile
from harden_web import harden, sri

def verify(output):
    root = Path(output).resolve()
    text = (root / 'index.html').read_text()
    match = re.search(r'<meta http-equiv="Content-Security-Policy" content="([^"]+)">', text)
    assert match and text.index(match.group(0)) < text.index('<script'), 'CSP must precede scripts'
    policy = dict((parts[0], parts[1:]) for clause in match[1].split(';') if (parts := clause.split()))
    for name in ['default-src', 'frame-src', 'object-src', 'base-uri', 'form-action']:
        assert policy[name] == ["'none'"], name
    assert "'unsafe-eval'" not in policy['script-src'] and "'unsafe-inline'" not in policy['script-src']
    assert "'wasm-unsafe-eval'" in policy['script-src']
    assert policy['connect-src'] == ["'self'", 'blob:']
    assert 'name="referrer" content="no-referrer"' in text
    for attrs, body in re.findall(r'<script\b([^>]*)>(.*?)</script>', text, re.S):
        if not re.search(r'\bsrc\s*=', attrs) and body.strip():
            assert "'" + sri(body.encode()) + "'" in policy['script-src'], 'Unapproved inline script'
    for tag in re.findall(r'<(?:script|link)\b[^>]*>', text):
        if tag.startswith('<link') and 'rel="stylesheet"' not in tag:
            continue
        src = re.search(r'\b(?:src|href)="([^"]+)"', tag)
        if not src:
            continue
        path = (root / src[1]).resolve()
        assert path.is_relative_to(root)
        assert 'integrity="' + sri(path.read_bytes()) + '"' in tag, 'Asset checksum mismatch: ' + src[1]
    return True

def self_test():
    with tempfile.TemporaryDirectory(prefix='skybreak-security-') as folder:
        p = Path(folder)
        (p / 'a.js').write_text('window.example=true;')
        (p / 'a.css').write_text('body{color:white}')
        template = '<html><head><meta charset="utf-8"><link rel="stylesheet" href="a.css"></head><body><script src="a.js"></script><script>const a=1;</script></body></html>'
        (p / 'index.html').write_text(template)
        harden(p);verify(p)
        before = (p / 'index.html').read_bytes();harden(p)
        assert before == (p / 'index.html').read_bytes(), 'Must be idempotent'
        (p / 'a.js').write_text('window.example=false;')
        try:
            verify(p)
        except AssertionError:
            pass
        else:
            raise AssertionError('Tampered script was accepted')
        (p / 'index.html').write_text(template.replace('src="a.js"', 'src="../outside.js"'))
        try:
            harden(p)
        except ValueError:
            pass
        else:
            raise AssertionError('Out-of-directory script was accepted')
    # Exercise the real template and versioning pipeline, not only the helper.
    template_dir = Path(__file__).resolve().parents[1] / 'Assets/WebGLTemplates/Skybreak'
    with tempfile.TemporaryDirectory(prefix='skybreak-template-security-') as folder:
        p = Path(folder)
        for source in template_dir.iterdir():
            if source.is_file() and source.suffix != '.meta':
                shutil.copy2(source, p / source.name)
        text = (p / 'index.html').read_text()
        substitutions = {
            'LOADER_FILENAME': 'Web.loader.js', 'DATA_FILENAME': 'Web.data.unityweb',
            'FRAMEWORK_FILENAME': 'Web.framework.js.unityweb', 'CODE_FILENAME': 'Web.wasm.unityweb',
            'JSON.stringify(COMPANY_NAME)': '"Skybreak Studio"',
            'JSON.stringify(PRODUCT_NAME)': '"SKYBREAK"',
            'JSON.stringify(PRODUCT_VERSION)': '"1.8.0"',
        }
        for key, value in substitutions.items():
            text = text.replace('{{{ ' + key + ' }}}', value)
        assert '{{{' not in text
        (p / 'index.html').write_text(text)
        (p / 'Build').mkdir()
        for name in ['Web.loader.js', 'Web.data.unityweb', 'Web.framework.js.unityweb', 'Web.wasm.unityweb']:
            (p / 'Build' / name).write_text('security-test-fixture')
        from version_web_assets import version_assets
        version_assets(p)
        verify(p)

if __name__ == '__main__':
    import argparse
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('output', nargs='?', type=Path)
    args = parser.parse_args()
    self_test()
    if args.output:
        verify(args.output)
    print('PASS CSP, inline hashes, asset integrity, tamper rejection and path boundaries')
