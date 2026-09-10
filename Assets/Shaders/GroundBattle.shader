Shader "Skybreak/GroundBattle" {
Properties{_Color("Concrete",Color)=(.25,.26,.25,1) _Night("Night",Float)=0}
SubShader{Tags{"RenderType"="Opaque"} CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
struct Input{float3 worldPos;};fixed4 _Color;float _Night;
float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
void surf(Input i,inout SurfaceOutputStandard o){float2 p=i.worldPos.xz;float2 cell=floor(p/3);float2 edge=abs(frac(p/3)-.5);float seam=smoothstep(.486,.495,max(edge.x,edge.y));float grain=hash(floor(p*34));float worn=hash(cell)*.08;
 float lane=1-smoothstep(3.25,3.4,abs(p.x));float stripe=(1-smoothstep(.028,.055,abs(abs(p.x)-3.05)))*(step(.45,frac(p.y*.25)));
 float drains=step(.90,frac(p.y*3))*step(3.55,abs(p.x))*step(abs(p.x),3.78);
 float3 c=lerp(_Color.rgb*(.92+worn),float3(.15,.18,.19),lane);c*=1-seam*.23;c+=float3(.006,.008,.008)*(grain-.5);c=lerp(c,float3(.66,.59,.33),stripe*.7);c*=1-drains*.5;
 o.Albedo=c;o.Metallic=.12;o.Smoothness=lerp(.28,.48,_Night);o.Occlusion=1-seam*.22;}
ENDCG}Fallback "Standard"
}
