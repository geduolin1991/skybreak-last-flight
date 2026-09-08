Shader "Skybreak/Ocean" {
Properties {
 _Deep("Channel blue",Color)=(.14,.30,.44,1)
 _Shallow("Sunlit shallows",Color)=(.17,.48,.51,1)
 _WaveMap("Periodic wave spectrum",2D)="gray"{}
 _Travel("Travel",Float)=0
 _Wake0("Rescue wake 0",Vector)=(0,0,0,0)
 _Wake1("Rescue wake 1",Vector)=(0,0,0,0)
 _Wake2("Rescue wake 2",Vector)=(0,0,0,0)
}
SubShader {
 Tags {"RenderType"="Opaque" "Queue"="Geometry"}
 Pass {
  Tags {"LightMode"="ForwardBase"}
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
  #pragma multi_compile_fog
  #pragma target 3.0
  #include "UnityCG.cginc"
  #include "Lighting.cginc"
  #include "AutoLight.cginc"
  sampler2D _WaveMap;
  float4 _Deep,_Shallow,_Wake0,_Wake1,_Wake2,_Shore[8];
  float _Travel;
  struct v2f {float4 pos:SV_POSITION;float3 world:TEXCOORD0;SHADOW_COORDS(1) UNITY_FOG_COORDS(2)};
  v2f vert(appdata_base v) {
   v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;
   TRANSFER_SHADOW(o);UNITY_TRANSFER_FOG(o,o.pos);return o;
  }
  float coastDistance(float2 p) {
   float nearest=100;
   [unroll] for(int i=0;i<8;i++) {
    float4 shore=_Shore[i];float2 d=abs(p-shore.xy);
    float2 q=d-float2(shore.z,abs(shore.w))+.48;
    float wall=length(max(q,0))+min(max(q.x,q.y),0)-.48;
    float island=(length(d/max(float2(shore.z,-shore.w),.1))-1)*min(shore.z,-shore.w);
    nearest=min(nearest,shore.w<0?island:wall);
   }
   return nearest;
  }
  // Bow shoulders, widening Kelvin arms and propeller turbulence all live on
  // the water plane. No glowing line renderers or transparent mesh overdraw.
  float shipWake(float2 p,float4 ship,float grain) {
   float2 d=p-ship.xy;float tail=-d.y-.48;
   float len=lerp(5.8,9.4,ship.w),life=saturate(tail/.45)*saturate(1-tail/len);
   float spread=.22+max(0,tail)*.19;
   float ripple=sin(tail*8.1-_Time.y*4.2)*.012*saturate(tail);
   float edge=abs(abs(d.x)-spread-ripple);
   float aa=max(.022,fwidth(d.x)*.85),width=.027+max(0,tail)*.012;
   float arms=(1-smoothstep(width,width+aa,edge))*life;
   float propeller=exp(-d.x*d.x/max(.018,.028+max(tail,0)*.08))*life;
   float bow=abs(length(d*float2(1, .55))-.32);
   float bowFoam=(1-smoothstep(.04,.14,bow))*saturate(d.y*2+.5)*saturate(1-d.y);
   return saturate((arms*(.16+grain*.42)+propeller*grain*.25+bowFoam*.24)*ship.z);
  }
  fixed4 frag(v2f i):SV_Target {
   float t=_Time.y;float2 p=i.world.xz+float2(0,_Travel);
   float4 large=tex2D(_WaveMap,p*.029+float2(t*.0025,-t*.0018));
   float2 cross=float2(p.x*.8-p.y*.6,p.x*.6+p.y*.8);
   float4 small=tex2D(_WaveMap,cross*.075+float2(-t*.0036,t*.003));
   float4 micro=tex2D(_WaveMap,p*.21+large.rg*.08+float2(t*.004,0));
   float broad=tex2D(_WaveMap,p*.0035+float2(t*.00025,0)).a;
   float2 slope=(large.rg-.5)*.48+(small.rg-.5)*.26+(micro.rg-.5)*.10;
   float3 n=normalize(float3(-slope.x,1,-slope.y));
   float3 view=unity_OrthoParams.w>.5?normalize(UNITY_MATRIX_V[2].xyz):normalize(_WorldSpaceCameraPos-i.world);
   float3 light=normalize(_WorldSpaceLightPos0.xyz),halfway=normalize(view+light);
   float shore=coastDistance(i.world.xz);
   float shoal=exp(-max(0,shore)*.25)*(.66+large.a*.2);
   float3 water=lerp(_Deep.rgb,_Shallow.rgb,shoal);
   water*=lerp(.93,1.06,broad)*(.98+large.b*.04);
   float cap=saturate(1-abs(micro.b-.68)*28)*smoothstep(.66,.82,small.b);
   water+=float3(.008,.018,.024)*cap;
   // Broad, bounded reflections survive phone downsampling without glitter.
   float sparkle=pow(saturate(dot(n,halfway)),180)*.047;
   float sheen=pow(saturate(dot(n,normalize(float3(-.12,1,-.18)))),22);
   float shadow=SHADOW_ATTENUATION(i);
   water*=lerp(.68,1,shadow);
   water+=float3(.004,.008,.012)*sheen+float3(1,.88,.7)*sparkle*shadow;
   float fresnel=.025+.42*pow(1-saturate(dot(n,view)),5);
   water=lerp(water,float3(.19,.30,.44)*( .8+large.b*.2),fresnel);
   // Foam follows authored quay/islet footprints and breaks into patches.
   float grain=saturate(micro.b*.72+small.b*.5-.15);
   float edgeFoam=exp(-abs(shore-.08)*10)*(.14+grain*.42);
   float breakerPhase=shore*2.2-t*1.25+large.b*2;
   float breaker=pow(saturate(sin(breakerPhase)),9)*exp(-max(0,shore)*.72)*smoothstep(.15,.7,shore)*grain*.19;
   float foam=saturate(edgeFoam+breaker+shipWake(i.world.xz,_Wake0,grain)+shipWake(i.world.xz,_Wake1,grain)+shipWake(i.world.xz,_Wake2,grain));
   water=lerp(water,float3(.42,.57,.59),foam*.8);
   fixed4 result=fixed4(water,1);UNITY_APPLY_FOG(i.fogCoord,result);return result;
  }
  ENDCG
 }
}
Fallback "Diffuse"
}
