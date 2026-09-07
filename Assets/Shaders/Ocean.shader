Shader "Skybreak/Ocean" {
Properties {_Deep("Deep",Color)=(.02,.12,.17,1) _Shallow("Shallow",Color)=(.05,.35,.4,1) _Travel("Travel",Float)=0}
SubShader {Tags {"RenderType"="Opaque"} LOD 200
Pass {CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct app {float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct vf{float4 pos:SV_POSITION;float3 world:TEXCOORD0;};float4 _Deep,_Shallow;float _Travel;
vf vert(app v){vf o;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.pos=UnityObjectToClipPos(v.vertex);return o;}
float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
float wave(float2 p){return sin(p.x*.8+p.y*.3)*.34+sin(p.y*1.8-p.x*.22)*.19+noise(p*2.8)*.2;}
float4 frag(vf i):SV_Target {float2 p=i.world.xz+float2(_Time.y*.7,_Travel);float w=wave(p*.65);float dx=wave((p+float2(.08,0))*.65)-w;float dz=wave((p+float2(0,.08))*.65)-w;float3 n=normalize(float3(-dx*8,1,-dz*8));float3 v=normalize(_WorldSpaceCameraPos-i.world);float fres=pow(1-saturate(dot(v,n)),3);float spec=pow(saturate(dot(reflect(normalize(float3(.4,-.9,.5)),n),v)),80);float foam=pow(saturate(noise(p*.4)*1.08),12);float3 c=lerp(_Deep.rgb,_Shallow.rgb,saturate(w+.35)) + float3(.09,.22,.26)*fres+spec*float3(.65,.86,1)*.65+foam*.025;return float4(c,1);}
ENDCG}}
}
