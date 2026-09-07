Shader "Skybreak/Ocean" {
Properties {_Deep("Deep",Color)=(.02,.12,.17,1) _Shallow("Shallow",Color)=(.05,.35,.4,1) _Travel("Travel",Float)=0}
SubShader {Tags {"RenderType"="Opaque"} LOD 200
Pass {CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#include "UnityCG.cginc"
struct app{float4 vertex:POSITION;};struct vf{float4 pos:SV_POSITION;float3 world:TEXCOORD0;};float4 _Deep,_Shallow;float _Travel;
vf vert(app v){vf o;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.pos=UnityObjectToClipPos(v.vertex);return o;}
float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
float4 frag(vf i):SV_Target {
 float2 p=i.world.xz+float2(_Time.y*.21,_Travel);float t=_Time.y;
 float drift=noise(p*.31+float2(t*.035,0));float a=dot(p,float2(.8,.45))*2.9+t*1.1+drift*3.2,b=dot(p,float2(-.37,1))*5.2-t*1.4-noise(p*.47)*4,c=dot(p,float2(.8,1))*11.8+t*1.7+drift*7;
 float3 n=normalize(float3(-cos(a)*.15+cos(b)*.09+cos(c)*.025,1,-cos(a)*.085-cos(b)*.15+cos(c)*.03));
 float3 v=normalize(_WorldSpaceCameraPos-i.world);float fres=pow(1-saturate(dot(v,n)),4);float spec=pow(saturate(dot(reflect(normalize(float3(.4,-.9,.5)),n),v)),125);
 float swell=noise(p*.11);float crest=pow(saturate(sin(a)*.5+sin(b)*.25+.18),8)*smoothstep(.35,.75,noise(p*.6));
 float current=sin(dot(p,float2(.1,1))*1.5+noise(p*.13)*3)*.015;
 float3 col=lerp(_Deep.rgb,_Shallow.rgb,.22+swell*.25)+float3(.07,.19,.22)*fres+spec*float3(.7,.83,.92)*.11+crest*float3(.05,.11,.12)+current+sin(a)*.004+sin(b)*.003+sin(c)*.0018;
 return float4(col,1);
}
ENDCG}}
}
