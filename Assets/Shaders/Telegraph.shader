Shader "Skybreak/Telegraph" {
 SubShader {
 Tags {"Queue"="Transparent-10" "RenderType"="Transparent"}
 Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct app {float4 vertex:POSITION;fixed4 color:COLOR;};
 struct vf {float4 pos:SV_POSITION;fixed4 color:COLOR;};
 vf vert(app v){vf o;o.pos=UnityObjectToClipPos(v.vertex);o.color=v.color;return o;}
 fixed4 frag(vf i):SV_Target{return i.color;}
 ENDCG}
 }
}
