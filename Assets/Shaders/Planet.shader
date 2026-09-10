Shader "Skybreak/Planet" {
Properties {_Color("Ocean",Color)=(.018,.055,.10,1)}
SubShader {Tags {"RenderType"="Opaque"} Pass {CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#include "UnityCG.cginc"
struct app{float4 vertex:POSITION;float3 normal:NORMAL;};
struct vf{float4 pos:SV_POSITION;float3 n:TEXCOORD0;float3 local:TEXCOORD1;float3 world:TEXCOORD2;};float4 _Color;
vf vert(app v){vf o;o.pos=UnityObjectToClipPos(v.vertex);o.n=UnityObjectToWorldNormal(v.normal);o.local=v.vertex.xyz;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
float ns(float3 p){float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+1),f.x),f.y),f.z);}
float fbm(float3 p){float n=0,w=.55;for(int j=0;j<4;j++){n+=ns(p)*w;p=p*2.02+float3(17.4,11.8,3.5);w*=.43;}return n;}
float4 frag(vf i):SV_Target{
 float3 p=normalize(i.local),n=normalize(i.n),v=normalize(_WorldSpaceCameraPos-i.world);
 float daylight=dot(n,normalize(float3(-.65,.62,-.43)));float day=smoothstep(-.24,.50,daylight);
 float warp=fbm(p*3.2);float terrain=fbm(p*5.1+warp*1.8);float land=smoothstep(.46,.57,terrain);
 float3 landColor=lerp(float3(.07,.105,.105),float3(.17,.17,.13),smoothstep(.34,.62,fbm(p*10)));
 float ice=smoothstep(.84,.99,abs(p.y)+warp*.08);landColor=lerp(landColor,float3(.27,.34,.38),ice);
 float3 surface=lerp(_Color.rgb,landColor,land);
 // Broad cloud systems retain shape without foreground-like granular contrast.
 float3 wind=p*13+float3(_Time.y*.0012,p.y*p.y*1.4,warp*1.8);
 float cloud=smoothstep(.37,.66,fbm(wind)+sin(p.y*19+warp*4)*.035);
 surface=lerp(surface,float3(.31,.39,.44),cloud*.45);
 float limb=pow(1-saturate(dot(n,v)),2.8);float haze=.24+limb*.50;
 surface=lerp(surface,float3(.085,.17,.26),haze);
 float3 c=surface*(.055+day*.66);float night=1-day;
 float cities=smoothstep(.65,.76,ns(p*112))*land*(1-cloud);c+=cities*night*float3(.025,.018,.008);
 c+=float3(.035,.10,.17)*limb*smoothstep(-.35,.3,daylight);return float4(c*.55,1);
}
ENDCG
}}
}
