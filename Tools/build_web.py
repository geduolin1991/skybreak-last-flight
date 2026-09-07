#!/usr/bin/env python3
"""Build the complete browser game in a disposable copy, preserving Mac settings."""
import argparse
from pathlib import Path
import shutil
import subprocess

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--unity", default="/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity")
args = parser.parse_args()
workspace = ROOT / "Build" / "WebWorkspace"
workspace.mkdir(parents=True, exist_ok=True)
for name in ("Assets", "Packages", "ProjectSettings"):
    target = workspace / name
    if target.exists():
        shutil.rmtree(target)
    shutil.copytree(ROOT / name, target)
log = ROOT / "Build" / "web-build.log"
subprocess.run([args.unity, "-batchmode", "-nographics", "-quit", "-projectPath", str(workspace),
                "-buildTarget", "WebGL", "-executeMethod", "SkyWebBuild.Build", "-logFile", str(log)], check=True)
output = workspace / "Build" / "Web"
assert (output / "index.html").is_file(), "Unity did not produce a web player"
from version_web_assets import version_assets
version_assets(output)
print(f"Browser build: {output}\nBuild log: {log}")
