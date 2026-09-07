Shader "Skybreak/Planet" {
Properties{_Color("Ocean",Color)=(.01,.08,.19,1)}
SubShader{Tags {"RenderType"="Opaque"}
Pass{CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct app {float4 vertex:POSITION;float3 normal:NORMAL;};struct vf{float4 pos:SV_POSITION;float3 n:TEXCOORD0;float3 local:TEXCOORD1;float3 world:TEXCOORD2;};float4 _Color;
vf vert(app v){vf o;o.pos=UnityObjectToClipPos(v.vertex);o.n=UnityObjectToWorldNormal(v.normal);o.local=v.vertex.xyz;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
float ns(float3 p){float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+1),f.x),f.y),f.z);}
float4 frag(vf i):SV_Target{float3 p=i.local;float land=ns(p*11)*.55+ns(p*24)*.28+ns(p*50)*.17;float cloud=ns(p*27+float3(_Time.y*.002,0,0))*.65+ns(p*64)*.35;float3 c=lerp(_Color.rgb,float3(.035,.15,.17),smoothstep(.49,.55,land));c=lerp(c,float3(.3,.49,.57),smoothstep(.59,.69,cloud)*.8);float light=saturate(dot(normalize(i.n),normalize(float3(-.4,.7,-.5))))*.8+.12;float rim=pow(1-saturate(dot(normalize(i.n),normalize(_WorldSpaceCameraPos-i.world))),3);return float4(c*light+float3(.025,.2,.45)*rim,1);}
ENDCG}}
}
