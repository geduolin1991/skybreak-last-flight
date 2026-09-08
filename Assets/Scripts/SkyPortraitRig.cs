using UnityEngine;

namespace Skybreak {
// Analytic, unscaled motion is independent of GUI repaint counts and frame rate.
public static class SkyPortraitRig {
 public static Rect FitRect(Rect bounds,float aspect) {
  float width=Mathf.Min(bounds.width,bounds.height*aspect),height=width/aspect;
  return new Rect(bounds.center.x-width*.5f,bounds.center.y-height*.5f,width,height);
 }
 struct Profile {
  public Vector4 eyes,pivot,left,right;
  public float tempo,sway,softness;
  public Profile(Vector4 eyes,Vector4 pivot,Vector4 left,Vector4 right,float tempo,float sway,float softness) {
   this.eyes=eyes;this.pivot=pivot;this.left=left;this.right=right;
   this.tempo=tempo;this.sway=sway;this.softness=softness;
  }
 }
 static readonly Profile[] profiles={
  new Profile(new Vector4(.417f,.839f,.205f,.074f),new Vector4(.405f,.37f,2f/3,0),
   new Vector4(.297f,.609f,.158f,.116f),new Vector4(.447f,.607f,.168f,.117f),1f,.013f,.94f),
  new Profile(new Vector4(.425f,.825f,.205f,.083f),new Vector4(.548f,.37f,2f/3,0),
   new Vector4(.531f,.565f,.139f,.119f),new Vector4(.678f,.553f,.115f,.111f),1.04f,.015f,1f),
  new Profile(new Vector4(.355f,.829f,.205f,.078f),new Vector4(.463f,.35f,2f/3,0),
   new Vector4(.407f,.561f,.133f,.122f),new Vector4(.582f,.552f,.120f,.117f),.97f,.012f,.92f)
 };
 public static void Configure(Material material,int pilot) {
  var p=profiles[pilot];
  material.SetVector("_Eyes",p.eyes);material.SetVector("_Pivot",p.pivot);
  material.SetVector("_ChestLeft",p.left);material.SetVector("_ChestRight",p.right);
 }
 // Quintic easing has no sudden velocity or acceleration at a pose boundary.
 static float Ease(float value) {
  float x=Mathf.Clamp01(value);return x*x*x*(x*(x*6-15)+10);
 }
 static float Gesture(float time,float start,float arrive,float leave,float end) {
  return Ease((time-start)/(arrive-start))*(1-Ease((time-leave)/(end-leave)));
 }
 // Both the body and its soft follow-through sample the same continuous pose.
 // A weighted delay approximates a strongly damped response with no independent
 // oscillator or artificial impulse when selecting a portrait or breathing out.
 static Vector4 SampleBody(Profile p,int pilot,float t,out Vector4 pose) {
  float cycle=Mathf.Repeat(t,16f),sway=t*Mathf.PI/3.3f,breath=t*Mathf.PI/1.8f;
  float direction=pilot==1?-1:1;
  float turn=direction*Gesture(cycle,2.8f,3.9f,4.35f,5.55f);
  float bend=Gesture(cycle,9.1f,10.35f,11.1f,12.65f)*.62f;
  float lean=(Mathf.Sin(sway)+.12f*Mathf.Sin(sway*.5f+.4f))*p.sway+turn*.009f-bend*.006f;
  float lateral=Mathf.Sin(sway-.35f)*.0021f+turn*.0045f;
  float rise=Mathf.Sin(breath)*.0022f+Mathf.Sin(sway-.3f)*.0003f;
  pose=new Vector4(turn*.14f,bend,Mathf.Sin(breath)*.45f,0);
  return new Vector4(lean,lateral,rise,0);
 }
 public static void Animate(Material material,int pilot,float clock,float selectionAge,bool face,bool fade) {
  var p=profiles[pilot];float t=clock*p.tempo+pilot*1.75f;
  Vector4 body=SampleBody(p,pilot,t,out var pose);
  Vector4 lag=SampleBody(p,pilot,t-.10f,out var a)*.62f
   +SampleBody(p,pilot,t-.22f,out var b)*.28f
   +SampleBody(p,pilot,t-.36f,out var c)*.10f;
  Vector4 lagPose=a*.62f+b*.28f+c*.10f;
  float x=(lag.y-body.y)+(lag.x-body.x)*.36f+(lagPose.x-pose.x)*.044f;
  float y=(lag.z-body.z)-(lagPose.y-pose.y)*.026f;
  float differential=(lag.x-body.x)*.035f;
  // Follow-through stays small relative to the torso's actual displacement.
  // As the body settles the delay naturally vanishes within 0.36 seconds.
  body.w=face?.12f:1;pose.w=(lagPose.x-pose.x)*.16f;
  material.SetVector("_Motion",body);
  material.SetVector("_Pose",face?Vector4.zero:pose);
  material.SetVector("_Secondary",face?Vector4.zero:
   new Vector4(Mathf.Clamp(x,-.006f,.006f),Mathf.Clamp(y+differential,-.009f,.009f),
    Mathf.Clamp(x,-.006f,.006f),Mathf.Clamp(y-differential,-.009f,.009f))*p.softness);
  float phase=t%5.3f;
  float blink=phase<.17f?Mathf.Sin(phase/.17f*Mathf.PI):
   phase>.32f&&phase<.45f?Mathf.Sin((phase-.32f)/.13f*Mathf.PI):0;
  material.SetFloat("_Blink",blink);material.SetFloat("_Life",t);
  material.SetFloat("_Fade",fade?1:0);
  material.SetVector("_UVRect",face?new Vector4(pilot==2?.04f:.12f,.68f,.62f,.28f):new Vector4(0,0,1,1));
 }
}
}
