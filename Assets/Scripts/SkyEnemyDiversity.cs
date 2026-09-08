using UnityEngine;
namespace Skybreak {
public partial class SkyGame {
 static readonly string[] EnemyModels={"Drone","Interceptor","Gunship","CargoShuttle","DiveBomber","WardDrone","RailLancer","MineTender","DroneCarrier","GridPylon","ShieldEmitter","SupplyBeacon"};
 static readonly float[] EnemySizes={.8f,.7f,.95f,.67f,.67f,.64f,.65f,.64f,.76f,.58f,.23f,.28f};
 static readonly float[] EnemyHealth={12,26,80,125,95,82,66,110,220,260,18,14};
 float EnemyRadius(Hostile e)=>e.boss?2.65f:e.kind==9?1.04f:e.kind==8?1.6f:e.kind==10?.45f:e.kind==11?.37f:e.kind==3?1.25f:e.kind==2?1.35f:e.kind>=4?.88f:e.kind==1?.75f:.67f;
 void ConfigureEnemy(Hostile e){
  if(e.kind==9){float depth=Stage==2?6:9.8f;e.visualOffset=new Vector3(0,-depth,depth*.57735027f);e.go.transform.localScale=Vector3.one*.45f;e.fire=3.4f;e.marker=Art.Ring(e.go.transform,1.3f,new Color(1,.3f,.1f,.33f),.045f);e.marker.transform.localPosition=Vector3.up*.15f;}
  if(e.kind==5){e.marker=Art.Ring(e.go.transform,4.3f,new Color(.64f,.26f,1,.33f),.045f);}
  if(e.kind==10){e.fire=3.7f;e.marker=Art.Ring(e.go.transform,2.5f,new Color(1,.36f,.1f,.4f),.07f);}
  if(e.kind==11){e.fire=3.1f;e.marker=Art.Ring(null,.7f,Art.Orange,.035f);}
 }
 float WardDamageScale(Hostile e){if(e.boss||e.owner!=null||e.kind>=9||e.kind==5)return 1;foreach(var ward in Enemies)if(ward.kind==5&&(ward.pos-e.pos).sqrMagnitude<8.2f){if(e.flash<=0)Shockwave(e.pos,Art.Violet,1,.15f);return .4f;}return 1;}
 bool TickSpecialEnemy(Hostile e,float dt){
  if(e.kind<4)return false;
  if(e.kind==9){
   if(e.fire<=0){for(int i=-1;i<=1;i++){Vector3 v=(PlayerPos-e.pos).normalized;AddBullet(e.pos,Quaternion.Euler(0,i*22,0)*v*3.7f*DifficultySpeed,false,2,1,.16f);}e.fire=3.7f;}
  }else if(e.kind==11){
   if(e.missionIndex>=0&&e.missionIndex<World.Civilians.Count){var c=World.Civilians[e.missionIndex];Vector3 target=c.station;target=World.CivilianAirTarget(c);e.pos=Vector3.Lerp(e.origin,target,Mathf.Clamp01(e.age/3.1f));e.visualOffset=Vector3.up*Mathf.Sin(Mathf.Clamp01(e.age/3.1f)*Mathf.PI)*1.8f;if(e.marker){e.marker.transform.position=target;e.marker.transform.localScale=Vector3.one*(.7f+e.fire*.18f);}}
   if(e.fire<=0){ConvoyImpact(e);RemoveSpecialEnemy(e);return true;}
  }else if(e.kind==10){e.pos.z-=dt*.7f;if(e.marker)e.marker.transform.localScale=Vector3.one*(.8f+Mathf.Sin(e.age*8)*.08f);if(e.fire<=0){for(int i=0;i<10;i++){float a=i*Mathf.PI/5;AddBullet(e.pos,new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*4.4f*DifficultySpeed,false,1);}Burst(e.pos,12,1);RemoveSpecialEnemy(e);return true;}}
  else {
   float speed=e.kind==4?2.6f:e.kind==8?1.8f:e.kind==6?2.6f:2.1f;
   if(e.kind==4&&e.missionIndex>=0&&e.pos.z<=7.5f&&e.age<12)speed=.15f;
   e.pos.z-=dt*speed;e.pos.x=e.origin.x+Mathf.Sin(e.age*.8f+e.phase)*(e.kind==4?.4f:e.kind==6?.65f:1.2f);
   if(e.kind==4&&e.missionIndex>=0){
    if(e.pos.z<=9&&e.attackClock==0){e.attackClock=1;e.fire=3.5f;MissionSay("convoy_bomb_lock",94,()=>Enemies.Contains(e),false);}
    if(e.attackClock>0&&e.fire<=0&&e.attackClock<3){e.attackClock++;var bomb=SpawnEnemy(11,e.pos,0);bomb.missionIndex=e.missionIndex;e.fire=4.5f;}
   }else if(e.kind==5){if(e.age>3)MissionSay("ward_contact",86,()=>Enemies.Contains(e));if(e.fire<=0&&e.pos.z<12){for(int i=0;i<8;i++){float a=e.age*.2f+i*Mathf.PI/4;AddBullet(e.pos,new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*3.2f*DifficultySpeed,false,2);}e.fire=3;}}
   else if(e.kind==6){UpdateWeaponTell(e);if(e.aimReady&&e.fire<.4f)MissionSay("rail_lock",92,()=>Enemies.Contains(e)&&e.aimReady);if(e.fire<=0&&e.pos.z<12&&e.pos.z>PlayerPos.z+1){Vector3 dir=(e.lockedAim-e.pos).normalized;for(int i=0;i<4;i++)AddBullet(e.pos-dir*i*.75f,dir*13*DifficultySpeed,false,3,1,.14f);FinishWeaponTell(e);e.fire=3.1f;}}
   else if(e.kind==7){if(e.fire<=0&&e.pos.z<13){if(CountEnemyKind(10)<8){var mine=SpawnEnemy(10,e.pos+Vector3.back*.6f,0);mine.fire=4.1f;}e.fire=2.6f;}}
   else if(e.kind==8){if(e.fire<=0&&e.pos.z<13){if(Enemies.Count<36)for(int s=-1;s<=1;s+=2)SpawnEnemy(0,e.pos+new Vector3(s*1.4f,0,-1),6);e.fire=5;MissionSay("carrier_launch",84,()=>Enemies.Contains(e));}}
   else if(e.fire<=0&&e.pos.z<13){EnemyAttack(e);e.fire=2.5f;}
   if(e.pos.z<-15){RemoveSpecialEnemy(e);return true;}
  }
  if(e.go)RenderEnemyFeedback(e);if(e.kind>=4&&e.kind<=8&&invuln<=0&&(e.pos-PlayerPos).sqrMagnitude<1.3f)HitPlayer();return true;
 }
 int CountEnemyKind(int kind){int n=0;foreach(var e in Enemies)if(e.kind==kind)n++;return n;}
 void RemoveSpecialEnemy(Hostile e){if(e.marker&&e.marker.transform.parent==null)Destroy(e.marker.gameObject);ReleaseEnemyVisual(e);Enemies.Remove(e);}
 void SpawnDiverseWave(int wave){
  int type=wave%9;float x=Mathf.Sin(wave*2.7f)*6;
  if(type==1){SpawnEnemy(Stage==0?4:6,new Vector3(x,1,17),1);SpawnEnemy(1,new Vector3(-x*.65f,1,19),4);}
  else if(type==3){SpawnEnemy(5,new Vector3(x*.6f,1,18),0);for(int s=-1;s<=1;s+=2)SpawnEnemy(1,new Vector3(x*.6f+s*2.1f,1,19),4);}
  else if(type==5){SpawnEnemy(Stage==2?8:7,new Vector3(x,1,18),0);}
  else if(type==7){SpawnEnemy(6,new Vector3(-6,1,17),4);SpawnEnemy(Stage==0?7:8,new Vector3(5,1,20),0);}
  else SpawnClassicChapterWave(wave);
 }
}
}
