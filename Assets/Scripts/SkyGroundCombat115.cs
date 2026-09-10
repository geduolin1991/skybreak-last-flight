using UnityEngine;
namespace Skybreak {
public partial class SkyGame {
 GroundUnit115 SpawnGround115(int kind,Vector3 p){
  string model=kind==1?"SiegeWalker115":kind==2?"MortarBattery115":kind==3||kind==5?"GroundRelay115":kind==4?(GroundArea==3?"FurnaceWalker115":"BastionCrawler115"):"SiegeCrawler115";
  var go=Art.Model(model,groundRoot);float size=kind==4?.92f:kind==0?.62f:kind==1?.63f:kind==2?.75f:1;
  go.transform.position=new Vector3(p.x,0,p.z);go.transform.localScale=Vector3.one*size;go.transform.rotation=Quaternion.Euler(0,180,0);
  float hp=kind==4?(GroundArea==1?1950:2700):kind==3||kind==5?165:kind==0?150:kind==1?110:130;
  hp*=Difficulty==0?.85f:Difficulty==2?1.22f:1;
  var e=new GroundUnit115{go=go,p=p,home=p,kind=kind,hp=hp,max=hp,fire=2+(groundUnits.Count%3)*.65f,heading=180,radius=kind==4?2.1f:kind==0?1.0f:kind==1?.85f:.72f,renderers=go.GetComponentsInChildren<Renderer>()};
  foreach(var t in go.GetComponentsInChildren<Transform>())if(t.name.StartsWith("Motion_Turret"))e.turret=t;
  e.line=Art.Line(go.transform,"Ground firing solution",new[]{p,p},Art.Orange,.04f);e.line.useWorldSpace=true;e.line.enabled=false;
  groundUnits.Add(e);return e;
 }
 GroundUnit115 GroundTarget115(){GroundUnit115 best=null;float score=float.MaxValue;foreach(var e in groundUnits){float d=(e.p-PlayerPos).sqrMagnitude;if(e.kind==4&&GroundNodeCount>0)d+=1800;Vector3 end=GroundRayEnd115(PlayerPos,e.p,false);if((end-e.p).sqrMagnitude>.05f)d+=800;if(d<score){score=d;best=e;}}return best;}
 void GroundFire115(){
  bool tank=GroundTank;float cadence=tank?(Ship==0?.18f:Ship==1?.72f:.83f):(Ship==0?.16f:Ship==1?.25f:.30f);
  groundFire=cadence;groundShots++;float damage=(tank?(Ship==0?11:Ship==1?104:126):(Ship==0?9:Ship==1?27:31))*groundPower;
  Vector3 right=Vector3.Cross(Vector3.up,groundAim),start=PlayerPos+groundAim*(tank?1.28f:.72f);
  int shots=Ship==0?2:1;for(int i=0;i<shots;i++){Vector3 p=start+right*(i-(shots-1)*.5f)*.33f;AddBullet(p,groundAim*(tank&&Ship==1?22:39),true,tank&&Ship==1?3:Ship==2?2:0,damage,tank?.16f:.10f,false,pierce:tank&&Ship==2?3:0,kind:tank&&Ship==1?4:0);}
  if(groundRig)groundRig.Recoil();weaponKick=tank?.45f:.12f;Sound(tank?(Ship==1?"FrameSiege":Ship==2?"ShotLance":"ShotPulse"):(Ship==1?"ShotScatter":"ShotPulse"),tank?.32f:.16f);
  FrameRay(start,start+groundAim*.65f,Ship==1?Art.Orange:ShipColor,tank?.15f:.055f,.08f);
 }
 // Swept slab intersection in the gameplay plane. The same cover extents are
 // used for movement, line of sight, projectiles and the visible barrier meshes.
 static bool GroundSegmentBox115(Vector3 a,Vector3 b,Vector3 center,Vector2 half,out float enter){
  float lo=0,hi=1;for(int axis=0;axis<2;axis++){float origin=axis==0?a.x:a.z,delta=axis==0?b.x-a.x:b.z-a.z,c=axis==0?center.x:center.z,h=axis==0?half.x:half.y;
   if(Mathf.Abs(delta)<.00001f){if(origin<c-h||origin>c+h){enter=0;return false;}continue;}
   float x=(c-h-origin)/delta,y=(c+h-origin)/delta;if(x>y){float swap=x;x=y;y=swap;}lo=Mathf.Max(lo,x);hi=Mathf.Min(hi,y);if(lo>hi){enter=0;return false;}}
  enter=lo;return true;
 }
 Vector3 GroundRayEnd115(Vector3 a,Vector3 b,bool damage,float amount=0){
  float closest=1;GroundCover115 hit=null;foreach(var c in groundCovers)if(c.hp>0&&GroundSegmentBox115(a,b,c.p,c.half,out float t)&&t<closest){closest=t;hit=c;}
  if(hit!=null&&damage)DamageCover115(hit,amount);return Vector3.Lerp(a,b,closest);
 }
 void DamageCover115(GroundCover115 c,float damage){if(c.hp<=0)return;c.hp-=damage;groundCoverHits++;if(c.hp<=0){c.go.transform.localScale=new Vector3(1,.15f,1);Burst(c.p+Vector3.up*.3f,15,1,.55f);Sound("CrashGround114",.23f);}}
 void TickGroundUnits115(float dt){
  foreach(var e in groundUnits){e.age+=dt;e.flash=Mathf.Max(0,e.flash-dt);
   if(e.kind==0||e.kind==1){Vector3 d=PlayerPos-e.p;float wanted=e.kind==1?4.3f:7.0f;Vector3 move=d.magnitude>wanted?d.normalized:e.kind==1?Vector3.Cross(d.normalized,Vector3.up)*.65f:Vector3.zero;
    Vector3 next=e.p+move*dt*(e.kind==1?2.4f:1.25f);next.x=Mathf.Clamp(next.x,-9,9);next.z=Mathf.Clamp(next.z,-8.8f,9.5f);
    if(!GroundMoveBlocked115(new Vector3(next.x,1,e.p.z),false))e.p.x=next.x;if(!GroundMoveBlocked115(new Vector3(e.p.x,1,next.z),false))e.p.z=next.z;
    if(d.magnitude<1.35f)GroundHurt115(e.p);
   }
   if(e.kind==4){e.p.x=Mathf.Sin(e.age*.32f)*(GroundArea==3?2.3f:1.35f);e.p.z=7+Mathf.Sin(e.age*.18f)*.45f;}
   e.go.transform.position=new Vector3(e.p.x,0,e.p.z);
   Vector3 aim=(PlayerPos-e.p).normalized;float heading=Mathf.Atan2(aim.x,aim.z)*Mathf.Rad2Deg;e.heading=Mathf.LerpAngle(e.heading,heading,dt*3);
   if(e.kind==0||e.kind==1)e.go.transform.rotation=Quaternion.Euler(0,e.heading,0);
   if(e.turret)e.turret.rotation=Quaternion.Euler(0,heading+180,0);
   if(e.kind!=3){if(e.tell>0){e.tell-=dt;e.line.enabled=true;e.line.SetPosition(0,e.p);e.line.SetPosition(1,e.aim);if(e.tell<=0){FireGroundEnemy115(e);e.line.enabled=false;e.fire=e.kind==4?4.2f:e.kind==2?4.7f:3.5f;}}
    else {e.fire-=dt;e.line.enabled=false;if(e.fire<=0){e.tell=e.kind==4?1.1f:.9f;e.aim=PlayerPos;}}}
   bool exposed=e.kind==4&&GroundNodeCount==0&&e.fire>2.6f&&e.tell<=0;
   foreach(var r in e.renderers){if(!r)continue;block.Clear();if(e.flash>0)block.SetColor("_EmissionColor",Art.Orange*e.flash*3);else if(exposed)block.SetColor("_EmissionColor",Art.Cyan*.32f);else if(e.kind==3||e.kind==5)block.SetColor("_EmissionColor",Art.Orange*.12f);else block.SetColor("_EmissionColor",Color.black);r.SetPropertyBlock(block);}
  }
 }
 void FireGroundEnemy115(GroundUnit115 e){
  if(e.kind==2){GroundRing115(e.aim,2.1f,1.3f,true,true);Sound("Warning",.15f);return;}
  Vector3 direction=(e.aim-e.p).normalized;int count=e.kind==4?(e.hp/e.max<.5f?7:5):e.kind==1?3:e.kind==5?2:1;
  for(int i=0;i<count;i++){float a=(i-(count-1)*.5f)*(e.kind==4?10:8);AddBullet(e.p+direction*.8f,Quaternion.Euler(0,a,0)*direction*(e.kind==4?7:8.5f)*DifficultySpeed,false,e.kind==4?2:1,1,.18f);}
  if(e.kind==4){GroundRing115(e.aim,2.4f,1.35f,true,true);if(e.hp/e.max<.5f)GroundRing115(new Vector3(-e.aim.x,1,e.aim.z+2.4f),1.8f,1.75f,true,true);Sound("FrameSiege",.30f);}
 }
 void TickGroundRounds115(float dt){
  for(int i=Bullets.Count-1;i>=0;i--){var b=Bullets[i];b.prev=b.pos;b.pos+=b.velocity*dt;b.age+=dt;b.life-=dt;bool dead=b.life<=0||Mathf.Abs(b.pos.x)>18||Mathf.Abs(b.pos.z)>20;
   if(!dead){Vector3 end=GroundRayEnd115(b.prev,b.pos,true,b.friendly?b.damage*.75f:18);if((end-b.pos).sqrMagnitude>.00001f){b.pos=end;dead=true;Burst(end,3,3,.23f);}}
   if(!dead&&b.friendly){for(int n=groundUnits.Count-1;n>=0;n--){var e=groundUnits[n];if(SegmentDistance(e.p,b.prev,b.pos)<e.radius+b.radius){DamageGround115(e,b.damage);
     if(GroundTank&&Ship==1){foreach(var other in groundUnits.ToArray())if(other!=e&&(other.p-e.p).sqrMagnitude<5.8f)DamageGround115(other,b.damage*.42f);GroundRing115(e.p,1.7f,.22f,false,false);}
     // Piercing rounds move past the full target diameter before another hit.
     if(b.pierce>0){b.pierce--;b.pos+=b.velocity.normalized*(e.radius*2+.3f);}else dead=true;break;}}}
   if(!dead&&!b.friendly&&SegmentDistance(PlayerPos,b.prev,b.pos)<.42f+b.radius){GroundHurt115(b.prev);dead=true;}
   if(State==FlightState.Defeat){Bullets.Clear();return;}
   if(dead)Bullets.RemoveAt(i);else Bullets[i]=b;
  }
  if(groundPurgeRounds){groundPurgeRounds=false;CancelBullets(false);}
 }
 float GroundDamageScale115(GroundUnit115 e){if(e.kind!=4&&e.kind!=0)return 1;if(e.kind==4&&GroundNodeCount>0)return .08f;bool open=e.kind==4&&e.fire>2.6f&&e.tell<=0;if(open)return 1.7f;
  Vector3 front=new Vector3(Mathf.Sin(e.heading*Mathf.Deg2Rad),0,Mathf.Cos(e.heading*Mathf.Deg2Rad));float facing=Vector3.Dot((PlayerPos-e.p).normalized,front);return facing>.60f?.65f:1.15f;
 }
 void DamageGround115(GroundUnit115 e,float damage){
  if(e.dead)return;e.hp-=damage*GroundDamageScale115(e);e.flash=.12f;Burst(e.p,2,3,.2f);if(e.hp>0)return;e.dead=true;groundUnits.Remove(e);Kills++;Combo++;MaxCombo=Mathf.Max(MaxCombo,Combo);comboClock=5;Score+=(e.kind==4?10000:e.kind==3||e.kind==5?800:300)*(1+Mathf.Min(5,Combo/8));
  SurfaceExplosion(new Vector3(e.p.x,.02f,e.p.z),CrashSurface.Ground,e.kind==4);Burst(e.p,e.kind==4?34:14,1,e.kind==4?1.3f:.65f);GroundRing115(e.p,e.kind==4?3.5f:1.7f,.42f,false,false);Sound(e.kind==4?"BossBreak":"CrashGround114",e.kind==4?.7f:.33f);e.go.SetActive(false);
  if(e.kind==4){groundPurgeRounds=true;foreach(var mark in groundMarks){mark.age=99;mark.ring.enabled=false;}GroundRadio115(GroundArea==1?"守门人的核心停了。它收到的名单是伪造的……有人在地面上改写了战争。":"冥炉不再发送识别码。把密钥带走，让空中编队听见这里发生的事。",(Ship+2)%3);}
 }
 void GroundRing115(Vector3 pos,float radius,float duration,bool hostile,bool blast){foreach(var m in groundMarks)if(m.age>=m.duration){m.age=0;m.duration=duration;m.p=pos;m.radius=radius;m.hostile=hostile;m.blast=blast;m.ring.enabled=true;m.ring.startColor=m.ring.endColor=hostile?Art.Orange:Art.Cyan;m.ring.transform.position=new Vector3(pos.x,.075f,pos.z);m.ring.transform.localScale=Vector3.one*radius;return;}}
 void TickGroundMarkers115(float dt){foreach(var m in groundMarks){if(m.age>=m.duration)continue;m.age+=dt;float f=m.age/m.duration;m.ring.startWidth=m.ring.endWidth=m.hostile?.04f+f*.08f:.11f*(1-f);m.ring.startColor=m.ring.endColor=m.hostile?new Color(1,.30f,.06f,.65f+f*.35f):new Color(.19f,.8f,1,1-f);
  if(m.age>=m.duration){m.ring.enabled=false;if(m.blast){Burst(m.p,25,1,m.radius*.5f);Sound("CrashGround114",.26f);if(m.hostile){if(Vector3.Distance(PlayerPos,m.p)<m.radius)GroundHurt115(m.p+Vector3.forward*.01f);}else foreach(var e in groundUnits.ToArray())if(Vector3.Distance(e.p,m.p)<m.radius+e.radius)DamageGround115(e,Ship==1?180:140);
   foreach(var c in groundCovers)if(c.hp>0&&Vector3.Distance(c.p,m.p)<m.radius+1)DamageCover115(c,80);}}
 }}
}
}
