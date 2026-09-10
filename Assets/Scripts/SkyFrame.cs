using UnityEngine;
using System.Collections.Generic;
namespace Skybreak {
// Only the visual assemblies move; the flight corridor and hit core never change.
public class SkyFrame:MonoBehaviour {
 sealed class Joint {public Transform part;public Vector3 position;public Quaternion rotation;public string name;public float side;}
 readonly List<Joint> joints=new List<Joint>();float clock,kick;int variant;
 public int JointCount=>joints.Count;
 public void Initialize(int ship){variant=ship;foreach(var t in GetComponentsInChildren<Transform>())if(t.name.StartsWith("Motion_"))joints.Add(new Joint{part=t,position=t.localPosition,rotation=t.localRotation,name=t.name,side=t.name.Contains("_L")?-1:1});}
 public void Recoil(){kick=1;}
 public void Tick(float dt,float deployed,Vector2 movement){
  clock+=dt;kick=Mathf.MoveTowards(kick,0,dt*(variant==1?4:7));float fold=1-Mathf.SmoothStep(0,1,deployed);
  foreach(var j in joints){
   Vector3 offset=Vector3.zero,angles=Vector3.zero;
   if(j.name.Contains("Wing")){angles=new Vector3(0,j.side*(fold*62+movement.x*3),j.side*(fold*26+Mathf.Sin(clock*1.8f)*2));offset.x=-j.position.x*fold*.62f;}
   else if(j.name.Contains("Petal")){int n=j.name[j.name.Length-1]-'0';offset=new Vector3(j.side*(.12f+Mathf.Sin(clock*1.5f+n)*.055f)*(1-fold)-j.position.x*fold*.58f,Mathf.Sin(clock*1.2f+n)*.045f*(1-fold),0);angles.y=j.side*(fold*75+Mathf.Sin(clock*.8f+n)*4);}
   else if(j.name.Contains("Halo")){angles.y=clock*12;offset.y=-fold*.45f;}
   else if(j.name.Contains("Cannon")){offset.y=-kick*.20f;angles.x=fold*48;}
   else if(j.name.Contains("Rack")){angles.y=j.side*fold*76;offset.x=-j.side*fold*.45f;}
   else if(j.name.Contains("Arm")){angles=new Vector3(fold*42+kick*(variant==1?9:4),0,j.side*(fold*32+movement.x*2));offset.x=-j.position.x*fold*.28f;}
   else if(j.name.Contains("Leg")){angles=new Vector3(fold*-38+movement.y*3,0,j.side*(5+movement.x*1.5f)*(1-fold));}
   j.part.localPosition=j.position+offset;j.part.localRotation=j.rotation*Quaternion.Euler(angles);
  }
 }
}
public partial class SkyGame {
 GameObject frameForm,airForm;SkyFrame frameRig;float frameBlend,frameStrikeClock,frameShieldGate;int frameGuard;
 public static readonly string[] FrameNames={"岚刃","赤垒","霜环"};
 public static readonly string[] FrameRoles={"翼刃拦截 · 高速追击","重炮齐射 · 正面拦弹","棱镜锁定 · 贯穿狙击"};
 public string FrameName=>FrameNames[Ship];
 public string FrameRole=>FrameRoles[Ship];
 public string FrameWeapon=>Ship==0?(WeaponMode==0?"翼刃协奏 / 双联追击":"收束翼刃 / 蜂群追猎"):Ship==1?(WeaponMode==0?"四联攻城炮 / 迎击盾 "+frameGuard:"集中聚爆 / 迎击盾 "+frameGuard):(WeaponMode==0?"霜环棱镜 / 多目标轨道炮":"零点收束 / 快速贯穿");
 public float FrameDeployment=>frameBlend;
 public int FrameGuard=>frameGuard;
 public int FrameJoints=>frameRig?frameRig.JointCount:0;
 public int FrameStrokes {get{int n=0;foreach(var s in frameStrokes)if(s.age<s.duration)n++;return n;}}
 public float FrameMobility=>Overdrive>0?(Ship==0?1.18f:Ship==1?.91f:1.05f):1;
 float FrameCadence=>Ship==0?(WeaponMode==0?.13f:.21f):Ship==1?(WeaponMode==0?.64f:.78f):(WeaponMode==0?.80f:.43f);
 sealed class FrameStroke {public LineRenderer line;public float age=99,duration,width;}
 readonly List<FrameStroke> frameStrokes=new List<FrameStroke>();
 readonly List<Hostile> frameTargets=new List<Hostile>(40);
 GameObject frameFxRoot;readonly Vector3[] frameArc=new Vector3[23];
 void PrepareFrame(){
  airForm=Player.transform.childCount>0?Player.transform.GetChild(0).gameObject:null;
  frameForm=Art.Model(new[]{"AstraFrame","CrimsonFrame","OracleFrame"}[Ship],Player.transform);
  frameForm.transform.localPosition=new Vector3(0,.10f,-.10f);
  // Face the armour toward the elevated camera; the head and long gun point up-screen.
  frameForm.transform.localRotation=Quaternion.Euler(-58,180,0);frameForm.transform.localScale=Vector3.one*(Ship==1?1.0f:.96f);
  frameRig=frameForm.AddComponent<SkyFrame>();frameRig.Initialize(Ship);frameForm.SetActive(false);frameBlend=0;
  if(!frameFxRoot){frameFxRoot=new GameObject("Frame weapon strokes / fixed pool");for(int i=0;i<12;i++){var line=Art.Line(frameFxRoot.transform,"Frame stroke "+i,new Vector3[2],Art.Cyan,.05f);line.useWorldSpace=true;line.enabled=false;frameStrokes.Add(new FrameStroke{line=line});}}
 }
 void BeginFrame(){frameGuard=Ship==1?8:0;frameStrikeClock=frameShieldGate=0;fireClock=0;Toast(FrameName+"展开 · "+FrameRole,3);Shockwave(PlayerPos,ShipColor,3.2f,.42f);Sound("FrameDeploy"+Ship,.65f);}
 void ResetFrame(){Overdrive=frameBlend=0;frameGuard=0;if(frameForm)frameForm.SetActive(false);if(airForm){airForm.SetActive(true);airForm.transform.localScale=Vector3.one;}foreach(var s in frameStrokes){s.age=99;if(s.line)s.line.enabled=false;}}
 void AnimateFrame(float dt){
  frameStrikeClock-=dt;frameShieldGate=Mathf.Max(0,frameShieldGate-dt);
  foreach(var stroke in frameStrokes){if(stroke.age>=stroke.duration)continue;stroke.age+=dt;float fade=Mathf.Clamp01(1-stroke.age/stroke.duration);stroke.line.startWidth=stroke.line.endWidth=stroke.width*fade;stroke.line.enabled=fade>0;}
  if(!frameForm)return;bool active=Overdrive>0;frameBlend=Mathf.MoveTowards(frameBlend,active?1:0,dt*3.2f);
  frameForm.SetActive(frameBlend>.08f);if(airForm){airForm.SetActive(frameBlend<.72f);airForm.transform.localScale=Vector3.one*(1-frameBlend*.12f);}
  if(frameRig&&frameForm.activeSelf)frameRig.Tick(dt,frameBlend,moveLast);
  frameForm.transform.localRotation=Quaternion.Euler(-58-moveLast.y*2,180+moveLast.x*3,0);
 }
 FrameStroke RentFrameStroke(Color color,float width,float duration,int points){foreach(var s in frameStrokes)if(s.age>=s.duration){s.age=0;s.duration=duration;s.width=width;s.line.sharedMaterial=Art.GlowMat(color);s.line.positionCount=points;s.line.startWidth=s.line.endWidth=width;s.line.enabled=true;return s;}return null;}
 void FrameRay(Vector3 a,Vector3 b,Color color,float width=.08f,float duration=.22f){var s=RentFrameStroke(color,width,duration,2);if(s==null)return;s.line.SetPosition(0,a);s.line.SetPosition(1,b);}
 void FrameSweep(bool focus){
  float radius=focus?6.6f:5.2f,angle=focus?34:65;
  var stroke=RentFrameStroke(ShipColor,.13f,.26f,frameArc.Length);for(int i=0;i<frameArc.Length;i++){float a=Mathf.Lerp(-angle,angle,i/(float)(frameArc.Length-1))*Mathf.Deg2Rad;frameArc[i]=PlayerPos+new Vector3(Mathf.Sin(a)*radius,.10f,Mathf.Cos(a)*radius+.4f);if(stroke!=null)stroke.line.SetPosition(i,frameArc[i]);}
  frameTargets.Clear();frameTargets.AddRange(Enemies);foreach(var e in frameTargets){Vector3 d=e.pos-PlayerPos;if(d.z>.1f&&d.z<radius+EnemyRadius(e)&&Mathf.Abs(d.x)<(focus?2.4f:4.6f))DamageEnemy(e,38*DamageBoost*RouteDamage);}
  for(int i=Bullets.Count-1;i>=0;i--){var b=Bullets[i];Vector3 d=b.pos-PlayerPos;if(!b.friendly&&d.z>.5f&&d.z<3.7f&&Mathf.Abs(d.x)<(focus?1.4f:2.6f))Bullets.RemoveAt(i);}
  Sound("FrameSlash",.30f);
 }
 void FramePierce(Vector3 from,Vector3 to,float damage,float width){
  FrameRay(from,to,ShipColor,width,.26f);frameTargets.Clear();frameTargets.AddRange(Enemies);
  foreach(var e in frameTargets)if(SegmentDistance(e.pos,from,to)<EnemyRadius(e)+width*.5f){DamageEnemy(e,damage);Burst(e.pos,3,2,.35f);}
 }
 void FireFrame(bool focused,float damage){
  if(frameRig)frameRig.Recoil();bool narrow=focused||WeaponMode==1;
  if(Ship==0){
   for(int side=-1;side<=1;side+=2)AddBullet(PlayerPos+new Vector3(side*1.1f,0,1),new Vector3(side*(narrow?1:6),0,44),true,0,13*damage,.11f,WeaponMode==1,pierce:1,kind:WeaponMode==1?2:1);
   if(frameStrikeClock<=0){FrameSweep(narrow);frameStrikeClock=.75f;}
  }else if(Ship==1){
   for(int i=0;i<4;i++){float x=(i-1.5f)*.64f;AddBullet(PlayerPos+new Vector3(x,0,1.35f),new Vector3(x*(narrow?.35f:3.8f),0,29),true,3,(narrow?43:36)*damage,.21f,kind:4);}
   FrameRay(PlayerPos+new Vector3(-1,0,.8f),PlayerPos+new Vector3(-1,0,2.7f),ShipColor,.22f,.12f);FrameRay(PlayerPos+new Vector3(1,0,.8f),PlayerPos+new Vector3(1,0,2.7f),ShipColor,.22f,.12f);
   shake=Mathf.Max(shake,.16f);Sound("FrameSiege",.50f);
  }else {
   Hostile left=null,right=null;float dl=float.MaxValue,dr=float.MaxValue;
   foreach(var e in Enemies){if(e.pos.z<PlayerPos.z)continue;float l=(e.pos-(PlayerPos+Vector3.left*4)).sqrMagnitude*(e.pos.x<PlayerPos.x-.35f?.4f:1);if(l<dl){dl=l;left=e;}}
   foreach(var e in Enemies){if(e==left||e.pos.z<PlayerPos.z)continue;float r=(e.pos-(PlayerPos+Vector3.right*4)).sqrMagnitude;if(r<dr){dr=r;right=e;}}if(right==null)right=left;
   FramePierce(PlayerPos+new Vector3(.72f,0,1),PlayerPos+new Vector3(.72f,0,35),(WeaponMode==0?90:70)*damage,narrow?.16f:.23f);
   if(WeaponMode==0){for(int side=-1;side<=1;side+=2){var target=side<0?left:right;Vector3 start=PlayerPos+new Vector3(side*1.9f,0,.6f);Vector3 end=target!=null?target.pos+(target.pos-start).normalized*2:start+new Vector3(side*2,0,25);FramePierce(start,end,39*damage,.07f);}}
   Sound("FramePrism",.43f);
  }
  weaponKick=Ship==1?.85f:.38f;
 }
 bool FrameIntercept(Round bullet){
  if(Overdrive<=0||Ship!=1||frameGuard<=0||bullet.friendly)return false;Vector3 d=bullet.pos-PlayerPos;
  if(d.z<.55f||d.z>2.5f||Mathf.Abs(d.x)>2.0f)return false;frameGuard--;
  if(frameShieldGate<=0){frameShieldGate=.09f;FrameRay(PlayerPos+new Vector3(-1.9f,0,1.4f),PlayerPos+new Vector3(1.9f,0,1.4f),ShipColor,.13f,.24f);Sound("ImpactArmor",.32f);}Burst(bullet.pos,3,3,.3f);return true;
 }
}
}
