Shader "Skybreak/ProjectileRim" {
 SubShader {
 Tags {"Queue"="Transparent-5" "RenderType"="Transparent"}
 Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Back
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "UnityCG.cginc"
 struct app {float4 vertex:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct vf {float4 pos:SV_POSITION;};
 vf vert(app v){vf o;UNITY_SETUP_INSTANCE_ID(v);o.pos=UnityObjectToClipPos(v.vertex);return o;}
 float4 frag(vf i):SV_Target{return float4(.006,.013,.025,.9);}
 ENDCG}
 }
}
