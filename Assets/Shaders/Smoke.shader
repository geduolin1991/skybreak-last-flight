Shader "Skybreak/Smoke" {
SubShader{Tags {"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off
Pass{CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_instancing
#include "UnityCG.cginc"
struct app{float4 vertex:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};struct vf{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
vf vert(app v){vf o;UNITY_SETUP_INSTANCE_ID(v);o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
float4 frag(vf i):SV_Target{float2 uv=i.uv*2-1;float r=length(uv);float ripple=sin(uv.x*13+_Time.y*3)*sin(uv.y*17)*.08;float a=pow(saturate(1-r+ripple),2)*.52;return float4(.022,.027,.034,a);}
ENDCG}}
}
