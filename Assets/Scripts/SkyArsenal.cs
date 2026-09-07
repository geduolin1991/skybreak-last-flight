using UnityEngine;
using System.Collections.Generic;
namespace Skybreak {
public partial class SkyGame {
 public int WeaponMode;
 double musicScheduledUntil;
 float railGlow;LineRenderer chargeHalo;
 readonly List<RailTrace> railTraces=new List<RailTrace>();
 class RailTrace {public LineRenderer core,halo;public float age,width;}
 readonly string[] roles={"高速追击型","重装爆破型","远距狙击型"};
 readonly string[,] weaponTitles={{"双联速射炮","蜂群追击弹"},{"破片霰射炮","聚爆榴弹炮"},{"蓄能轨道炮","速射穿甲炮"}};
 string WeaponTitle=>weaponTitles[Ship,WeaponMode];
 Color ShipColor=>Ship==0?Art.Cyan:Ship==1?new Color(1,.63f,.23f):new Color(.69f,.63f,1);
 float BaseCadence=>Ship==0?(WeaponMode==0?.105f:.19f):Ship==1?(WeaponMode==0?.34f:.6f):(WeaponMode==0?.68f:.29f);
 float ShotCadence=>BaseCadence/(1+upgradeFire*.12f)*(Overdrive>0?.62f:1)/(Ship==1&&SkillTime>0?1.3f:1);
 void SwitchWeaponMode(){WeaponMode=1-WeaponMode;fireClock=Mathf.Min(fireClock,ShotCadence);Sound("Click",.5f);Toast(ShipNames[Ship]+" · "+WeaponTitle,1.4f);}
 void AddShipExhaust(Transform root){float x=Ship==1?1.68f:Ship==2?.64f:.61f;float z=Ship==2?-1.8f:-1.55f;for(int i=-1;i<=1;i+=2)Art.Exhaust(root,new Vector3(i*x,.14f,z),ShipColor,Ship==1?.7f:.5f);}
 void RefreshShowcase(){if(showcase)Destroy(showcase);showcase=Art.Model(new[]{"Kestrel","Manta","Needle"}[Ship],null);showcase.transform.position=new Vector3(13,3,3.8f);showcase.transform.localScale=Vector3.one*(Ship==1?1.85f:Ship==2?2.05f:2.2f);AddShipExhaust(showcase.transform);}
 void ResetArmament(){foreach(var r in railTraces){if(r.core)Destroy(r.core.gameObject);if(r.halo)Destroy(r.halo.gameObject);}railTraces.Clear();railGlow=0;}
 void TickArmament(float dt,bool focused,bool firing){
  if(Ship==2&&Player){if(!chargeHalo){chargeHalo=Art.Ring(Player.transform,.3f,ShipColor,.03f);chargeHalo.transform.localPosition=new Vector3(0,.18f,1.95f);}float charge=1-Mathf.Clamp01(fireClock/ShotCadence);chargeHalo.gameObject.SetActive(firing);chargeHalo.transform.localScale=Vector3.one*(.4f+charge*.7f+railGlow*.5f);chargeHalo.startWidth=chargeHalo.endWidth=.018f+charge*.04f;}

  for(int i=railTraces.Count-1;i>=0;i--){var r=railTraces[i];r.age+=dt;float k=1-r.age/.18f;if(k<=0){Destroy(r.core.gameObject);Destroy(r.halo.gameObject);railTraces.RemoveAt(i);continue;}r.core.startWidth=r.core.endWidth=r.width*k;r.halo.startWidth=r.halo.endWidth=r.width*(1.8f+k);r.halo.startColor=r.halo.endColor=new Color(.47f,.4f,1,k*.45f);}
  railGlow=Mathf.MoveTowards(railGlow,0,dt*6);
  if(!firing)return;fireClock-=dt;if(fireClock<=0){Shoot(focused);fireClock=ShotCadence;}
  missileClock-=dt;
  if(missileClock<=0&&Enemies.Count>0){missileClock=(Ship==1?2.1f:Ship==0?1.7f:2.8f)/(1+hunters*.35f);
   if(Ship==1||hunters>0||Ship==0)for(int i=-1;i<=1;i+=2){int n=Ship==1?2:1;for(int j=0;j<n;j++)AddBullet(PlayerPos+new Vector3(i*(Ship==1?1.25f:.7f),0,-j*.35f),new Vector3(i*(5+j*3),0,14-j*2),true,Ship==1?3:0,(Ship==1?15:6)*(1+hunters*.6f),.15f,true,kind:Ship==1?4:2);}}
 }
 void Shoot(bool focused){
  float dmg=DamageBoost*(Overdrive>0?1.4f:1)*(Ship==1&&SkillTime>0?1.35f:1)*(focused?1+focusLevel*.35f:1);
  if(critLevel>0&&Random.value<Mathf.Min(.6f,.25f+(critLevel-1)*.1f))dmg*=2;
  if(Ship==0){if(WeaponMode==0){for(int i=-1;i<=1;i+=2){AddBullet(PlayerPos+new Vector3(i*.6f,0,.95f),new Vector3(i*(focused?-.7f:1.6f),0,46),true,0,6*dmg,.1f,kind:1);if(upgradeFire>0)AddBullet(PlayerPos+new Vector3(i*.9f,0,.65f),new Vector3(i*(focused?1:5),0,40),true,0,2.3f*dmg,.085f,kind:1);}}
   else for(int i=-1;i<=1;i++)AddBullet(PlayerPos+new Vector3(i*.58f,0,.7f),new Vector3(i*7,0,32),true,0,5.4f*dmg,.1f,true,kind:2);
  }else if(Ship==1){int n=WeaponMode==0?7:3;for(int i=0;i<n;i++){float a=(i-(n-1)*.5f)*(focused?.055f:WeaponMode==0?.135f:.18f);AddBullet(PlayerPos+new Vector3((i-(n-1)*.5f)*.23f,0,.72f),new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*(WeaponMode==0?31:24),true,3,(WeaponMode==0?5.7f:18)*dmg,WeaponMode==0?.14f:.2f,kind:WeaponMode==0?3:4);}}
  else FireRail((WeaponMode==0?80:31)*dmg,focused);
  if(wingmen>0)for(int i=-1;i<=1;i+=2)AddBullet(PlayerPos+new Vector3(i*1.4f,0,.4f),Vector3.forward*38,true,Ship==1?3:0,4*wingmen*dmg,.1f,kind:Ship==1?3:1);
  OnWeaponFired();
 }
 void FireRail(float damage,bool focused){Vector3 start=PlayerPos+Vector3.forward*1.35f;Vector3 end=start+Vector3.forward*36;float width=WeaponMode==0?.23f:.1f;
  var trace=new RailTrace{width=width,core=Art.Line(null,"Rail white core",new[]{start,end},new Color(.84f,.95f,1),width),halo=Art.Line(null,"Rail ion afterimage",new[]{start,end},new Color(.47f,.4f,1,.45f),width*3)};railTraces.Add(trace);railGlow=1;
  foreach(var e in Enemies.ToArray()){float radius=e.boss?2.65f:e.kind>=2?1.35f:e.kind==1?.75f:.67f;if(e.pos.z>=start.z-radius&&SegmentDistance(e.pos,start,end)<radius+(focused?.25f:.38f)){Burst(e.pos,10,2,.65f);DamageEnemy(e,damage);}}
  if(WeaponMode==0){Shockwave(start,new Color(.65f,.8f,1),1.3f,.22f);shake=Mathf.Max(shake,.19f);}
 }
 void WeaponImpact(Round b,Hostile direct){
  if(b.kind==4){Shockwave(b.pos,new Color(1,.7f,.28f),2.25f,.27f);Burst(b.pos,15,1,.7f);foreach(var e in Enemies){if(e==direct||(e.pos-b.pos).sqrMagnitude>6.25f)continue;float d=b.damage*.6f;if(pendingDamage.ContainsKey(e))pendingDamage[e]+=d;else pendingDamage[e]=d;}if(impactGate<=0){Sound("MissileBurst",.34f);impactGate=.07f;}}
  else Burst(b.pos,b.kind==3?7:4,b.kind==3?3:0,b.kind==3?.5f:.3f);
 }
 void DrawWeaponRound(Round b){var dir=b.velocity.sqrMagnitude>.01f?b.velocity.normalized:Vector3.forward;var rotation=Quaternion.LookRotation(dir);Vector3 scale=b.kind==3?new Vector3(.2f,.12f,.37f):b.kind==4?new Vector3(.2f,.2f,.75f):new Vector3(.1f,.1f,b.kind==1?.9f:.48f);
  draws[b.style].Add(Matrix4x4.TRS(b.pos,rotation,scale));
  if(b.kind==4){draws[3].Add(Matrix4x4.TRS(b.pos-dir*.53f,rotation,new Vector3(.12f,.12f,.52f)));for(int j=1;j<=3;j++)draws[4].Add(Matrix4x4.TRS(b.pos-dir*(.5f+j*.24f),Cam.transform.rotation,Vector3.one*(.12f+j*.035f)));}
  else if(b.kind==2)for(int j=1;j<=3;j++)draws[0].Add(Matrix4x4.TRS(b.pos-dir*(j*.27f),rotation,new Vector3(.06f/j,.06f/j,.32f)));
 }
 void DrawShipSelector(){Color col=ShipColor;Panel(1015,535,520,299);SmallTag("SELECT YOUR AIRFRAME",1036,550,218,col);Label("0"+(Ship+1)+" / 03",1424,550,91,26,17,paper,TextAnchor.MiddleRight);
  Label(ShipNames[Ship],1036,584,160,48,34,paper);Label(roles[Ship],1230,594,282,29,18,col,TextAnchor.MiddleRight);Label(ShipCodes[Ship],1038,634,365,25,15,muted);
  Label(weaponTitles[Ship,0]+" / "+weaponTitles[Ship,1],1038,670,471,29,19,col);
  Label(new[]{"高速连射 · 自动追击  /  机动 ★★★★","宽幅清场 · 导弹溅射  /  装甲 ★★★★★","蓄能贯穿 · 一线歼灭  /  精度 ★★★★★"}[Ship],1038,706,475,28,14,paper);
  if(ShipArrow("← 上一架",1036,753,230,59,col))SelectShip(Ship-1);if(ShipArrow("下一架 →",1283,753,230,59,col))SelectShip(Ship+1);
  for(int i=0;i<3;i++){float x=1015+i*177;var r=new Rect(x,481,166,42);bool hover=r.Contains(Event.current.mousePosition);Color c=i==Ship?col:hover?new Color(.3f,.46f,.54f):new Color(.1f,.2f,.27f);Rect(x,481,166,42,c);Label("0"+(i+1)+"  "+ShipNames[i],x,481,166,42,18,i==Ship?ink:paper,TextAnchor.MiddleCenter);bool old=GUI.enabled;GUI.enabled=!qaRunning;if(GUI.Button(r,GUIContent.none,GUIStyle.none))SelectShip(i);GUI.enabled=old;}
 }
 bool ShipArrow(string title,float x,float y,float w,float h,Color c){var r=new Rect(x,y,w,h);bool hover=r.Contains(Event.current.mousePosition);Rect(x,y,w,h,hover?Color.Lerp(c,Color.white,.35f):Color.Lerp(c,ink,.62f));Rect(x,y,w,2,c);Rect(x,y+h-2,w,2,c);Rect(x,y,2,h,c);Rect(x+w-2,y,2,h,c);Label(title,x,y,w,h,23,paper,TextAnchor.MiddleCenter);bool old=GUI.enabled;GUI.enabled=!qaRunning;bool hit=GUI.Button(r,GUIContent.none,GUIStyle.none);GUI.enabled=old;return hit;}
}
}
