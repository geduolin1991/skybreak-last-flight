Shader "Skybreak/Projectile" {
 Properties{_Color("Color",Color)=(1,1,1,1)}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Blend One One ZWrite Off Cull Back
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "UnityCG.cginc"
 struct app{float4 vertex:POSITION;float3 normal:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct vf{float4 pos:SV_POSITION;float3 normal:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
 float4 _Color;
 vf vert(app v){vf o;UNITY_SETUP_INSTANCE_ID(v);UNITY_TRANSFER_INSTANCE_ID(v,o);o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);return o;}
 float4 frag(vf i):SV_Target {float light=.75+.25*saturate(dot(normalize(i.normal),normalize(float3(-.3,1,-.4))));return float4(_Color.rgb*light*2.1,1);}
 ENDCG }
 }
}
