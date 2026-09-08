Shader "Skybreak/Finish" {Properties{_MainTex("Image",2D)="white"{}}
SubShader {Cull Off ZWrite Off ZTest Always
CGINCLUDE
#include "UnityCG.cginc"
sampler2D _MainTex,_Bloom;float4 _MainTex_TexelSize,_Blur;float _Intensity,_Flash,_Damage,_Exposure,_MobileAA;float4 _Ripple;
float4 threshold(v2f_img i):SV_Target{
 float3 c=tex2D(_MainTex,i.uv).rgb;
 if(_MobileAA>.5){float2 p=_MainTex_TexelSize.xy;c=(tex2D(_MainTex,i.uv+p).rgb+tex2D(_MainTex,i.uv-p).rgb+tex2D(_MainTex,i.uv+float2(p.x,-p.y)).rgb+tex2D(_MainTex,i.uv+float2(-p.x,p.y)).rgb)*.25;}
 return float4(max(0,c-.8),1);
}
float4 blur(v2f_img i):SV_Target{float2 d=_Blur.xy;return tex2D(_MainTex,i.uv)*.227027+(tex2D(_MainTex,i.uv+d*1.384615)+tex2D(_MainTex,i.uv-d*1.384615))*.316216+(tex2D(_MainTex,i.uv+d*3.230769)+tex2D(_MainTex,i.uv-d*3.230769))*.07027;}
float3 sceneColor(float2 uv){
 float3 center=tex2D(_MainTex,uv).rgb;if(_MobileAA<.5)return center;
 // Directional edge filtering is part of the existing final pass. It uses
 // only this frame, so rotation and explosions cannot leave temporal ghosts.
 float2 px=abs(_MainTex_TexelSize.xy);float3 luma=float3(.299,.587,.114);
 float nw=dot(tex2D(_MainTex,uv+px*float2(-1,1)).rgb,luma),ne=dot(tex2D(_MainTex,uv+px).rgb,luma);
 float sw=dot(tex2D(_MainTex,uv-px).rgb,luma),se=dot(tex2D(_MainTex,uv+px*float2(1,-1)).rgb,luma),mid=dot(center,luma);
 float low=min(mid,min(min(nw,ne),min(sw,se))),high=max(mid,max(max(nw,ne),max(sw,se)));
 if(high-low<max(.035,high*.12))return center;
 float2 along=float2(-((nw+ne)-(sw+se)),(nw+sw)-(ne+se));
 along=clamp(along/(min(abs(along.x),abs(along.y))+max((nw+ne+sw+se)*.03125,.0078125)),-4,4)*px;
 float3 inner=(tex2D(_MainTex,uv-along/6).rgb+tex2D(_MainTex,uv+along/6).rgb)*.5;
 float3 outer=inner*.5+(tex2D(_MainTex,uv-along*.5).rgb+tex2D(_MainTex,uv+along*.5).rgb)*.25;
 float light=dot(outer,luma);return light<low||light>high?inner:outer;
}
float4 finish(v2f_img i):SV_Target{float2 delta=i.uv-_Ripple.xy;float dist=length(delta*float2(1.7778,1));float band=exp(-abs(dist-_Ripple.z*.85)*65)*_Ripple.w;float2 sampleUV=i.uv+normalize(delta+float2(.00001,0))*band*.009;float3 c=sceneColor(sampleUV)+tex2D(_Bloom,sampleUV).rgb*_Intensity;
 // Filmic highlight rolloff preserves metal and explosion detail without
 // adding another texture sample or blur pass.
 c=max(0,c*_Exposure);c=saturate((c*(2.51*c+.03))/(c*(2.43*c+.59)+.14));
 float2 uv=i.uv*2-1;float vig=1-dot(uv,uv)*.09;c*=vig;float edge=saturate((dot(uv,uv)-.3)*.65)*_Damage;c=lerp(c,float3(.52,.035,.02),edge*.65);c=lerp(c,float3(.65,.9,1),saturate(_Flash));return float4(c,1);}
ENDCG
Pass {CGPROGRAM
#pragma vertex vert_img
#pragma fragment threshold
ENDCG}
Pass {CGPROGRAM
#pragma vertex vert_img
#pragma fragment blur
ENDCG}
Pass {CGPROGRAM
#pragma vertex vert_img
#pragma fragment finish
ENDCG}
}}
