Shader "Skybreak/GlowSprite" {
Properties{_MainTex("Glow",2D)="white"{} _Color("Tint",Color)=(1,1,1,1)}
SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha One ZWrite Off Cull Off
Pass {CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct app{float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};struct vf{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};sampler2D _MainTex;float4 _Color;
vf vert(app v){vf o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
float4 frag(vf i):SV_Target{float a=tex2D(_MainTex,i.uv).a;return float4(i.color.rgb*2.2,a*i.color.a);}
ENDCG}}}
