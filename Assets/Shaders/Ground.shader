Shader "Skybreak/Ground" {
Properties { _Travel("Travel",Float)=0 _Storm("Weather intensity",Range(0,1))=1 _Power("District power",Vector)=(0,0,0,0) }
SubShader { Tags {"RenderType"="Opaque"} LOD 250
CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
struct Input {float3 worldPos;};float _Travel,_Storm;float4 _Power;
float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
float band(float x,float halfWidth){return 1-smoothstep(halfWidth,halfWidth+max(.015,fwidth(x)),abs(x));}
float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
void surf(Input IN,inout SurfaceOutputStandard o){
 float2 p=IN.worldPos.xz+float2(0,_Travel);float2 q=float2(p.x,p.y+35);
 float street=band(p.x,3.5)+band(abs(p.x)-12,1.4);float cross=band(frac((q.y-5.5)/14+.5)*14-7,1.4);float road=saturate(street+cross);
 float grain=(hash(floor(p*30))-.5)*.016*(1-saturate(length(fwidth(p))*12));float slab=(hash(floor(p*1.3))-.5)*.022;
 float2 grid=abs(frac(p*1.3)-.5);float seam=smoothstep(.472,.5,max(grid.x,grid.y));
 float3 concrete=float3(.14,.18,.20)+slab-seam*.027;float3 asphalt=float3(.043,.061,.074)+grain;
 float lane=band(abs(p.x)-.11,.025)*step(.3,frac(q.y/2.7));float edge=band(abs(p.x)-3.28,.04);float stripe=lane*.58+edge*.35;
 float gutter=band(abs(p.x)-3.48,.05)*step(.3,frac(q.y*3));
 // Ten 14 m sectors wrap together; three power districts do not form a 126 m loop.
 float sector=fmod(floor((q.y+7)/14)+1000,10);float district=fmod(floor(sector/3),3);float power=district<1?_Power.x:district<2?_Power.y:_Power.z;
 float light=band(abs(p.x)-3.36,.018)*step(.75,frac(q.y/3));
 float puddle=smoothstep(.47,.70,noise(p*float2(.9,.24)))*road;
 float reflection=exp(-abs(abs(p.x)-3.0)*1.7)*pow(saturate(sin(q.y*2.3+noise(p*.6)*2)),4)*puddle*power;
 float ripple=sin(p.x*17+p.y*11-_Time.y*4)*.015*puddle*_Storm;
 o.Albedo=(lerp(concrete,asphalt,road)+float3(.6,.62,.50)*stripe-gutter*.02)*lerp(1,.76,puddle);
 o.Normal=normalize(float3(ripple,-ripple*.7,1));o.Metallic=.13;o.Smoothness=lerp(lerp(.34,.60,road),.90,puddle);
 o.Occlusion=1-seam*.18;o.Emission=float3(.15,.48,.67)*light*power*1.7+float3(.60,.25,.08)*reflection*.25;
}
ENDCG
} Fallback "Standard"
}
