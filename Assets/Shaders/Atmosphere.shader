Shader "Skybreak/Atmosphere" {
 Properties {_Tint("Mist tint",Color)=(.26,.49,.54,.12) _Travel("Travel",Float)=0}
 SubShader {
 Tags {"Queue"="Transparent-20" "RenderType"="Transparent"}
 Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct app {float4 vertex:POSITION;};
 struct vf {float4 pos:SV_POSITION;float3 world:TEXCOORD0;};
 float4 _Tint;float _Travel;
 vf vert(app v){vf o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);
  return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 float4 frag(vf i):SV_Target {
  float2 p=(i.world.xz+float2(_Travel*.13,_Travel*.58))*.055;
  float n=noise(p)*.62+noise(p*2.03+17.7)*.27+noise(p*4.17)*.11;
  float density=smoothstep(.38,.78,n);
  return float4(_Tint.rgb,_Tint.a*density);
 }
 ENDCG}
 }
}
