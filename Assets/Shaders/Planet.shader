Shader "Skybreak/Planet" {
Properties{_Color("Ocean",Color)=(.012,.066,.13,1)}
SubShader{Tags {"RenderType"="Opaque"}
Pass{CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#include "UnityCG.cginc"
struct app{float4 vertex:POSITION;float3 normal:NORMAL;};struct vf{float4 pos:SV_POSITION;float3 n:TEXCOORD0;float3 local:TEXCOORD1;float3 world:TEXCOORD2;};float4 _Color;
vf vert(app v){vf o;o.pos=UnityObjectToClipPos(v.vertex);o.n=UnityObjectToWorldNormal(v.normal);o.local=v.vertex.xyz;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
float ns(float3 p){float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+1),f.x),f.y),f.z);}
float fbm(float3 p){float n=0,w=.52;for(int j=0;j<5;j++){n+=ns(p)*w;p=p*2.03+float3(17.4,11.8,3.5);w*=.48;}return n;}
float4 frag(vf i):SV_Target{
 float3 p=normalize(i.local),n=normalize(i.n),v=normalize(_WorldSpaceCameraPos-i.world);float3 lightDir=normalize(float3(-.4,.7,-.5));float daylight=dot(n,lightDir);
 float warp=fbm(p*3.6);float terrain=fbm(p*5.7+warp*2.5);float land=smoothstep(.475,.505,terrain);float mountain=pow(saturate((fbm(p*78)-.25)*1.8),3);
 float coast=smoothstep(.46,.493,terrain)*(1-land);float3 landColor=lerp(float3(.048,.11,.075),float3(.25,.24,.15),smoothstep(.35,.67,fbm(p*19)));
 landColor+=mountain*.075;float ice=smoothstep(.83,.97,abs(p.y)+fbm(p*17)*.1);landColor=lerp(landColor,float3(.50,.61,.64),ice);
 float3 c=lerp(_Color.rgb+coast*float3(.025,.1,.105),landColor,land);
 float3 wind=p*24+float3(_Time.y*.005,p.y*p.y*2,0);float streak=sin(p.y*27+warp*7)*.06;float cloud=fbm(wind+float3(warp*3,0,0))+streak;
 float wisps=fbm(wind*3.6);float density=smoothstep(.47,.70,cloud+wisps*.10);float shadow=smoothstep(.45,.69,fbm(wind+float3(.1,.13,.1)))*.17;c*=1-shadow;
 c=lerp(c,float3(.57,.66,.68),density*.91);float night=1-smoothstep(-.15,.12,daylight);float cities=step(.78,ns(p*370))*step(.52,terrain)*step(terrain,.60);c=c*(saturate(daylight)*.83+.045)+cities*night*float3(.38,.21,.07);
 float rim=pow(1-saturate(dot(n,v)),4);c+=float3(.03,.17,.30)*rim*saturate(daylight+.35);
 return float4(c,1);
}
ENDCG}}
}
