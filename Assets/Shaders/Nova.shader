Shader "Skybreak/Nova" {
 Properties{_Color("Plasma",Color)=(.1,.7,1,1) _Progress("Expansion",Range(0,1))=0}
 SubShader{Tags{"Queue"="Transparent+10" "RenderType"="Transparent"} Blend SrcAlpha One ZWrite Off Cull Off
 Pass{CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct app{float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct vf{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};float4 _Color;float _Progress;
 vf vert(app v){vf o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv*2-1;return o;}
 float4 frag(vf i):SV_Target{float r=length(i.uv),a=atan2(i.uv.y,i.uv.x);float p=saturate(_Progress);float front=.045+.95*(1-pow(1-p,2));float noise=sin(a*31+r*67-p*18)*sin(a*13-r*41+p*9);float band=exp(-abs(r-front+noise*.012)*105);float wake=exp(-abs(r-front*.82)*55)*.37;float rays=pow(saturate(sin(a*21+p*5)),22)*exp(-abs(r-front*.63)*6)*.26;float core=exp(-r*35)*pow(1-p,3)*2;float plasma=(band+wake+rays+core)*pow(1-p,.7);float3 color=lerp(_Color.rgb,float3(.83,.96,1),saturate(band*.6+core));return float4(color*2.1,plasma*saturate((1-r)*25));}
 ENDCG}
 }
}
