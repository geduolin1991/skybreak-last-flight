Shader "Skybreak/Ocean" {
Properties {
 _Deep("Deep",Color)=(.018,.095,.14,1) _Shallow("Shallow",Color)=(.045,.29,.32,1)
 _Travel("Travel",Float)=0
 _Wake0("Rescue wake 0",Vector)=(0,0,0,0) _Wake1("Rescue wake 1",Vector)=(0,0,0,0) _Wake2("Rescue wake 2",Vector)=(0,0,0,0)
}
SubShader {Tags {"RenderType"="Opaque"} LOD 250
CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
struct Input {float3 worldPos;float3 viewDir;};
float4 _Deep,_Shallow,_Wake0,_Wake1,_Wake2;float _Travel;
float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
float wake(float2 p,float4 boat){
 float2 d=p-boat.xy;float tail=-d.y;float length=lerp(4.8,8,boat.w);
 float spread=.3+tail*.15;float aa=max(.035,fwidth(d.x)*1.5);
 float edge=1-smoothstep(.07,.07+aa,abs(abs(d.x)-spread));
 float fade=saturate(tail/.6)*saturate(1-tail/length)*step(0,tail);
 return edge*fade*boat.z*(.65+.35*sin(tail*12-_Time.y*5));
}
void surf(Input IN,inout SurfaceOutputStandard o){
 float2 p=IN.worldPos.xz+float2(_Time.y*.13,_Travel);float t=_Time.y;
 float drift=noise(p*.18);float a=dot(p,float2(.8,.45))*2.3+t*1.1+drift*5;
 float b=dot(p,float2(-.37,1))*4.2-t*1.4-noise(p*.43)*5;float c=dot(p,float2(.8,1))*10.8+t*1.7+drift*7;
 float detail=1-smoothstep(.9,2.8,fwidth(c));
 float sx=cos(a)*.075-cos(b)*.07+cos(c)*.022*detail;
 float sz=cos(a)*.043+cos(b)*.12+cos(c)*.022*detail;
 o.Normal=normalize(float3(-sx,-sz,1));
 float swell=noise(p*.085);float current=sin(p.y*.6+drift*4)*.022;
 float clouds=lerp(.82,1,noise(p*.035+float2(t*.025,0)));
 float foam=saturate(wake(IN.worldPos.xz,_Wake0)+wake(IN.worldPos.xz,_Wake1)+wake(IN.worldPos.xz,_Wake2));
 o.Albedo=lerp(_Deep.rgb,_Shallow.rgb,.28+swell*.35+current)*clouds;
 o.Albedo=lerp(o.Albedo,float3(.31,.48,.48),foam*.52);
 o.Metallic=.18;o.Smoothness=lerp(.86,.48,foam);o.Occlusion=1;
 // Preserve readable water under aircraft and harbor shadows.
 o.Emission=o.Albedo*.11;
}
ENDCG
} Fallback "Standard"
}
