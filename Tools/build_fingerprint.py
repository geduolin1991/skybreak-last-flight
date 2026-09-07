"""Match SkyBuildProvenance's canonical build-input digest without Unity."""
from pathlib import Path
import hashlib

def fingerprint(root):
    root = Path(root)
    files = [p for p in (root/'Assets').rglob('*')
             if p.is_file() and not p.relative_to(root).as_posix().startswith('Assets/Resources/SkyBuildIdentity.txt')]
    files += [root/'Packages'/p for p in ('manifest.json', 'packages-lock.json')]
    files += [root/'ProjectSettings'/p for p in (
        'ProjectSettings.asset','GraphicsSettings.asset','QualitySettings.asset',
        'TagManager.asset','InputManager.asset','ProjectVersion.txt','TimeManager.asset')]
    result = hashlib.sha256()
    for p in sorted((p for p in files if p.is_file()), key=lambda p:p.relative_to(root).as_posix()):
        result.update(p.relative_to(root).as_posix().encode()+b'\0')
        result.update(hashlib.sha256(p.read_bytes()).digest())
    return result.hexdigest()

if __name__ == '__main__':
    print(fingerprint(Path(__file__).resolve().parents[1]))
