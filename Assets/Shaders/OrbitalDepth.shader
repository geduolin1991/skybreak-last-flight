Shader "Skybreak/OrbitalDepth" {
Properties{_Color("Tint",Color)=(.11,.26,.43,1) _Shell("Atmosphere shell",Float)=0}
SubShader{Tags{"Queue"="Transparent-20" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Back
Pass{CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct app{float4 vertex:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;};struct vf{float4 pos:SV_POSITION;float3 n:TEXCOORD0;float3 view:TEXCOORD1;float2 uv:TEXCOORD2;};float4 _Color;float _Shell;
vf vert(app v){vf o;o.pos=UnityObjectToClipPos(v.vertex);float3 w=mul(unity_ObjectToWorld,v.vertex).xyz;o.n=UnityObjectToWorldNormal(v.normal);o.view=_WorldSpaceCameraPos-w;o.uv=v.uv;return o;}
float4 frag(vf i):SV_Target{
 if(_Shell>.5){float facing=saturate(dot(normalize(i.n),normalize(i.view)));float rim=pow(1-facing,4.5);float fade=smoothstep(0,.13,facing);float sun=smoothstep(-.4,.5,dot(normalize(i.n),normalize(float3(-.65,.62,-.43))));return float4(_Color.rgb,rim*fade*sun*.48);}
 float2 p=i.uv*2-1;float band=exp(-pow((p.y+p.x*.32+.08)*2.4,2));float structure=.72+.14*sin(p.x*6+p.y*4)+.10*sin(p.y*13-p.x*4);float fade=smoothstep(1,.35,length(p*.75));return float4(_Color.rgb,band*structure*fade*.20);
}
ENDCG
}}
}
