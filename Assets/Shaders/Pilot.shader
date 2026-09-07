Shader "Skybreak/Pilot" {
Properties {
 _MainTex("Portrait",2D)="white"{}
 _BlinkTex("Closed eyes",2D)="white"{}
 _Blink("Blink",Float)=0
 _Life("Life",Float)=0
 _Eyes("Eye region",Vector)=(.42,.84,.23,.085)
 _Fade("Edge fade",Float)=1
 _UVRect("Crop",Vector)=(0,0,1,1)
 _Pivot("Waist pivot and image aspect",Vector)=(.45,.36,.666667,0)
 _ChestLeft("Left chest region",Vector)=(.4,.56,.16,.10)
 _ChestRight("Right chest region",Vector)=(.57,.56,.14,.10)
 _Motion("Lean, lateral sway, breathing, strength",Vector)=(0,0,0,1)
 _Pose("Turn, forward bend, inhale, hair inertia",Vector)=(0,0,0,0)
 _Secondary("Left and right secondary displacement",Vector)=(0,0,0,0)
}
SubShader {
 Tags {"Queue"="Overlay"}
 Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off ZTest Always
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct app {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
 struct vf {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
 sampler2D _MainTex,_BlinkTex;
 float _Blink,_Life,_Fade;
 float4 _Eyes,_UVRect,_Pivot,_ChestLeft,_ChestRight,_Motion,_Pose,_Secondary;
 vf vert(app v) {vf o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
 float region(float2 uv,float4 zone) {
  float2 d=(uv-zone.xy)/zone.zw;
  return 1-smoothstep(.18,1,dot(d,d));
 }
 float4 frag(vf i):SV_Target {
  float2 uv=_UVRect.xy+i.uv*_UVRect.zw;
  // Inverse waist rotation keeps the head coherent and the lower body anchored.
  // Use image aspect space so a portrait never stretches as it leans.
  float anchor=smoothstep(.14,.73,uv.y);
  float angle=-_Motion.x*anchor*_Motion.w;
  float sn=sin(angle),cs=cos(angle);
  float2 p=(uv-_Pivot.xy)*float2(_Pivot.z,1);
  p=float2(cs*p.x-sn*p.y,sn*p.x+cs*p.y);
  uv=_Pivot.xy+p/float2(_Pivot.z,1);
  uv-=_Motion.yz*anchor*_Motion.w;

  // Broad shoulder perspective and head translation suggest a turn/lean without
  // trying to reveal an unseen side of the source illustration. A rigid head
  // weight keeps the eyes and mouth together while the torso changes pose.
  float upper=smoothstep(.30,.72,uv.y);
  float head=smoothstep(.69,.77,uv.y);
  float yaw=_Pose.x*_Motion.w,bend=_Pose.y*_Motion.w;
  float spine=lerp((_ChestLeft.x+_ChestRight.x)*.5,_Eyes.x,head);
  float width=1+upper*(bend*.047+_Pose.z*.007*_Motion.w)*(1-head*.85);
  width*=1-(1-cos(yaw))*upper*(1-head*.65);
  uv.x=spine+(uv.x-spine)/width-yaw*.044*upper;
  uv.y+=bend*(.018*upper+.014*head);
  uv.x-=bend*.008*head;

  // Feathered, normalized weights prevent doubled displacement at the seam.
  float left=region(uv,_ChestLeft),right=region(uv,_ChestRight);
  float coverage=max(left,right),weight=max(.0001,left+right);
  uv-=(_Secondary.xy*left+_Secondary.zw*right)/weight*coverage*_Motion.w;

  float hair=smoothstep(.53,.87,uv.x)*smoothstep(.28,.62,uv.y);
  hair*=1-coverage;
  uv.x+=(sin(_Life*1.65+uv.y*4)*.007+_Pose.w*.36)*hair*_Motion.w;
  uv.y+=sin(_Life*1.45+uv.x*5)*.0020*hair*_Motion.w;

  // Expressions sample the same deformed coordinates as the open portrait.
  float4 a=tex2D(_MainTex,uv),b=tex2D(_BlinkTex,uv);
  float2 eye=abs((uv-_Eyes.xy)/(_Eyes.zw*.5));
  float mask=(1-smoothstep(.72,1,eye.x))*(1-smoothstep(.65,1,eye.y));
  a=lerp(a,b,mask*_Blink);
  float edge=smoothstep(0,.15,i.uv.x)*smoothstep(0,.15,1-i.uv.x)*smoothstep(0,.12,i.uv.y);
  a.a*=lerp(1,edge,_Fade);
  return a;
 }
 ENDCG}
}
}
