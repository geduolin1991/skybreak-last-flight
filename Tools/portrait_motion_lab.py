"""Independent WebGL art preview. Does not launch or validate Unity.

The small C# math subset and fragment shader are translated from source rather
than maintaining a second animation implementation. Unity compilation and
native-player verification remain separate release requirements.
"""
from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler
from pathlib import Path
import re
import json
import hashlib

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Build" / "PortraitMotionLab"
OUT.mkdir(parents=True, exist_ok=True)


def balanced(text, start):
    level = 0
    for i in range(start, len(text)):
        if text[i] == "{":
            level += 1
        elif text[i] == "}":
            level -= 1
            if level == 0:
                return text[start + 1:i], i + 1
    raise ValueError("Unbalanced source")


def translate_rig(src):
    # Only the pure scalar math / material-uniform subset used by this rig.
    profiles = src.split("static readonly Profile[] profiles={", 1)[1].split("\n };", 1)[0]
    functions = []
    for match in re.finditer(r"(?:public )?static (?:void|float) (\w+)\(([^)]*)\)\s*\{", src):
        body, _ = balanced(src, match.end() - 1)
        args = re.sub(r"\b(?:float|int|bool|Material) ", "", match[2])
        # C# vector scaling has no JavaScript array operator equivalent.
        body = body.replace("new Vector4(Mathf.Clamp(x", "Scale(new Vector4(Mathf.Clamp(x")
        body = body.replace(")*p.softness);", "),p.softness));")
        body = re.sub(r"\b(?:float|var) ", "let ", body)
        functions.append(f"function {match[1]}({args}){{{body}\n}}")
    code = """
function Vector4(x,y,z,w){return [x,y,z,w];}
Vector4.zero=[0,0,0,0];
function Scale(v,s){return v.map(x=>x*s);}
function Profile(eyes,pivot,left,right,tempo,sway,softness){
 Object.assign(this,{eyes,pivot,left,right,tempo,sway,softness});
}
const Mathf={PI:Math.PI,Sin:Math.sin,Exp:Math.exp,Sqrt:Math.sqrt,Atan2:Math.atan2,
 Max:Math.max,Clamp:(x,a,b)=>Math.max(a,Math.min(b,x)),
 Clamp01:x=>Math.max(0,Math.min(1,x)),Repeat:(x,n)=>x-Math.floor(x/n)*n};
""" + "const profiles=[" + profiles + "];\n" + "\n".join(functions)
    code = re.sub(r"(?<=\d)f\b", "", code)
    return code + "\nfunction sample(pilot,clock,age=clock,face=false,fade=true){\n" + \
        "const uniforms={};const m={SetVector:(k,v)=>uniforms[k]=v,SetFloat:(k,v)=>uniforms[k]=v};\n" + \
        "Configure(m,pilot);Animate(m,pilot,clock,age,face,fade);return uniforms; }\n"


def translate_shader(src):
    region, _ = balanced(src, src.index("{", src.index("float region(")))
    frag, _ = balanced(src, src.index("{", src.index("float4 frag(")))
    text = """precision highp float;
varying vec2 vUV;
uniform sampler2D _MainTex,_BlinkTex;
uniform float _Blink,_Life,_Fade;
uniform vec4 _Eyes,_UVRect,_Pivot,_ChestLeft,_ChestRight,_Motion,_Pose,_Secondary;
float region(vec2 uv,vec4 zone){""" + region + "}\nvoid main(){" + frag + "}"
    for old, new in {"float2":"vec2", "float4":"vec4", "i.uv":"vUV",
                     "tex2D":"texture2D", "lerp(":"mix(", "return a;":"gl_FragColor=a;"}.items():
        text = text.replace(old, new)
    # GLSL ES overloads do not implicitly promote integer literals to floats.
    text = re.sub(r"(?<![\w.])(\d+)(?![\w.])", r"\1.0", text)
    return text


def generate():
    rig = (ROOT / "Assets/Scripts/SkyPortraitRig.cs").read_text()
    if "SampleBody(" in rig:
        raise SystemExit("The 1.2 natural-motion rig uses vector pose history beyond this historical scalar translator. Use the native --sky-portrait-preview capture; do not use old WebGL output to validate current animation.")
    shader = (ROOT / "Assets/Shaders/Pilot.shader").read_text()
    (OUT / "rig.js").write_text(translate_rig(rig))
    (OUT / "fragment.glsl").write_text(translate_shader(shader))
    (OUT / "source-info.json").write_text(json.dumps({
        "preview": "Independent WebGL translation; not a Unity build or native validation",
        "rig_sha256": hashlib.sha256(rig.encode()).hexdigest(),
        "shader_sha256": hashlib.sha256(shader.encode()).hexdigest(),
    }, indent=2))
    (OUT / "index.html").write_text((ROOT / "Tools/portrait_motion_lab.html").read_text())


class Handler(SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(ROOT), **kwargs)

    def do_POST(self):
        match = re.fullmatch(r"/__portrait_frame/(\d{4})\.jpg", self.path)
        length = int(self.headers.get("Content-Length", "0"))
        if not match or not 0 < length <= 4_000_000:
            self.send_error(400)
            return
        data = self.rfile.read(length)
        if not data.startswith(b"\xff\xd8"):
            self.send_error(415)
            return
        frames = OUT / "Frames"
        frames.mkdir(exist_ok=True)
        (frames / (match[1] + ".jpg")).write_bytes(data)
        self.send_response(204)
        self.end_headers()

    def log_message(self, fmt, *args):
        if self.command != "POST":
            super().log_message(fmt, *args)


if __name__ == "__main__":
    generate()
    print("Independent portrait preview: http://127.0.0.1:8768/Build/PortraitMotionLab/", flush=True)
    ThreadingHTTPServer(("127.0.0.1", 8768), Handler).serve_forever()
