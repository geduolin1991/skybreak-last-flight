"""Apply CSP and integrity checks to a rendered Unity page without changing game binaries."""
from pathlib import Path
import base64
import hashlib
import re

def sri(data):
    return 'sha256-' + base64.b64encode(hashlib.sha256(data).digest()).decode('ascii')

def harden(output):
    output = Path(output).resolve()
    index = output / 'index.html'
    html = index.read_text()
    if '{{{' in html:
        raise ValueError('Harden the Unity-rendered page, after versioning its assets.')
    if '<meta charset="utf-8">' not in html:
        raise ValueError('Expected the UTF-8 declaration before installing the policy.')
    # Recompute from final content on every build; repeated calls are idempotent.
    html = re.sub(r'\s*<meta\b[^>]*(?:http-equiv="Content-Security-Policy"|name="referrer")[^>]*>', '', html)
    hashes = [sri(body.encode()) for attrs, body in re.findall(r'<script\b([^>]*)>(.*?)</script>', html, re.S)
              if not re.search(r'\bsrc\s*=', attrs) and body.strip()]
    policy = "; ".join([
        "default-src 'none'",
        "script-src 'self' blob: 'wasm-unsafe-eval' " + ' '.join("'" + h + "'" for h in hashes),
        "style-src 'self' 'unsafe-inline'",
        "img-src 'self' data: blob:", "font-src 'self'", "media-src 'self' blob:",
        "connect-src 'self' blob:", "worker-src 'self' blob:",
        "frame-src 'none'", "object-src 'none'", "base-uri 'none'", "form-action 'none'",
    ])
    meta = '\n <meta http-equiv="Content-Security-Policy" content="' + policy + '">\n <meta name="referrer" content="no-referrer">'
    html = html.replace('<meta charset="utf-8">', '<meta charset="utf-8">' + meta, 1)
    def protect(match):
        tag = match.group(0)
        path_match = re.search(r'\b(?:src|href)="([^"]+)"', tag)
        if not path_match or (tag.startswith('<link') and 'rel="stylesheet"' not in tag):
            return tag
        name = path_match.group(1)
        file = (output / name).resolve()
        if not file.is_relative_to(output) or not file.is_file():
            raise ValueError('Unexpected script/stylesheet path: ' + name)
        tag = re.sub(r'\s+(?:integrity|crossorigin)="[^"]*"', '', tag)
        return tag[:-1] + ' integrity="' + sri(file.read_bytes()) + '" crossorigin="anonymous">'
    html = re.sub(r'<(?:script|link)\b[^>]*>', protect, html)
    index.write_text(html)
    return policy

if __name__ == '__main__':
    import argparse
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('output', type=Path)
    print(harden(parser.parse_args().output))
