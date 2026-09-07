using UnityEngine;
using System;
using System.Collections;
using System.IO;
namespace Skybreak {
public partial class SkyGame {
 IEnumerator VerifyLivingCampaign(Action<bool,string> check,string output){
  for(int i=0;i<3;i++)selectedRoutes[i]=0;Ship=0;Difficulty=0;BeginRun();Launch();AutoFire=false;invuln=999;StageTime=4;TickMission(0);
  check(World.Civilians.Count==3&&ConvoySurvivors==3,"Three separate rescue ships are present in the playable coast");
  check(World.Civilians[0].wakeLeft&&World.Civilians[0].go.GetComponentsInChildren<MeshFilter>().Length>3,"Rescue ships use authored geometry and independent wake trails");
  var first=World.Civilians[0];var second=World.Civilians[1];World.DamageCivilian(0,50);
  check(first.health==50&&second.health==100&&first.smoke,"Bomb damage affects the targeted ship and starts engine smoke");
  var bomber=SpawnEnemy(4,new Vector3(first.station.x,1,8),0);bomber.missionIndex=0;TickSpecialEnemy(bomber,.02f);
  check(bomber.attackClock==1&&bomber.fire>=3.4f,"Convoy bomber visibly locks before releasing its payload");
  bomber.fire=0;TickSpecialEnemy(bomber,.02f);var payload=Enemies.Find(e=>e.kind==11);
  check(payload!=null&&payload.missionIndex==0,"Bomber releases a shootable payload assigned to the actual rescue ship");
  float hp=first.health;if(payload!=null)DamageEnemy(payload,1000);
  check(first.health==hp&&!Enemies.Exists(e=>e.kind==11),"Intercepting a payload prevents damage to the convoy");
  var impact=SpawnEnemy(11,new Vector3(0,1,6),0);impact.missionIndex=1;impact.fire=0;TickSpecialEnemy(impact,.02f);
  check(second.health<100&&!Enemies.Contains(impact),"Unintercepted payload resolves exactly once against its civilian target");
  stageBanner=dialogClock=toastClock=0;yield return new WaitForSeconds(.3f);ScreenCapture.CaptureScreenshot(Path.Combine(output,"16-coast-convoy.png"));yield return new WaitForSeconds(.2f);
  MissionBossDefeated();Vector3 before=first.station;World.TickLivingMission(.5f);
  check(sideObjectiveComplete&&rescueSignals==1&&first.station.z>before.z,"Surviving convoy physically departs after its route is cleared");
  ClearBattle();Stage=1;BeginStage();Launch();AutoFire=false;invuln=999;StageTime=4;TickMission(0);
  check(missionTargets.Count==3&&World.PoweredDistricts==0,"City starts with three live jammers and an unpowered grid");
  var target=missionTargets[0];Vector3 visible=Cam.WorldToViewportPoint(target.pos+target.visualOffset);Vector3 collision=Cam.WorldToViewportPoint(target.pos);
  check(Vector2.Distance(new Vector2(visible.x,visible.y),new Vector2(collision.x,collision.y))<.002f,"Ground target art and air-plane shot collision align on screen");
  DamageEnemy(target,10000);check(missionNodes==1&&World.PoweredDistricts==1&&!sideObjectiveComplete,"First jammer powers exactly one district without faking mission completion");
  foreach(var e in missionTargets.ToArray())DamageEnemy(e,10000);
  check(missionNodes==3&&World.PoweredDistricts==3&&sideObjectiveComplete,"All three city jammers must be destroyed to restore the full grid");
  stageBanner=dialogClock=toastClock=0;yield return new WaitForSeconds(2);ScreenCapture.CaptureScreenshot(Path.Combine(output,"16-city-power-restored.png"));yield return new WaitForSeconds(.3f);
  ClearBattle();Stage=2;BeginStage();Launch();AutoFire=false;invuln=999;StageTime=4;TickMission(0);foreach(var e in missionTargets.ToArray())DamageEnemy(e,10000);
  check(World.UplinkActive&&!World.UplinkComplete&&!sideObjectiveComplete,"Opening orbital nodes starts a real upload instead of claiming instant success");
  TickMission(7.8f);check(!World.UplinkComplete,"Orbital upload requires its full protection window");TickMission(.21f);
  check(World.UplinkComplete&&sideObjectiveComplete&&World.Civilians[0].departed,"Completed upload connects the dock and releases its visible shuttles");
  stageBanner=dialogClock=toastClock=0;yield return new WaitForSeconds(.2f);ScreenCapture.CaptureScreenshot(Path.Combine(output,"16-orbit-uplink.png"));yield return new WaitForSeconds(.2f);
  for(int ship=0;ship<3;ship++){
   Ship=ship;BeginRun();Launch();AutoFire=false;invuln=999;ClearBattle();SpawnSupply(PlayerPos,3);UpdateSupplies(.01f);
   check(squadron.Count==2&&squadron.TrueForAll(w=>w.pilot!=Ship)&&squadron[0].pilot!=squadron[1].pilot,"Ship "+ship+" support pickup summons exactly the other two pilots");
   var dummy=SpawnEnemy(2,new Vector3(0,1,8),0);dummy.hp=dummy.maxHp=100000;
   foreach(var w in squadron){w.age=2;w.go.transform.position=PlayerPos+new Vector3(w.side*2.5f,.18f,-.6f);w.fire=0;}
   Bullets.Clear();TickSquadron(.05f);check(Bullets.Exists(b=>b.friendly)||railTraces.Count>0,"Ship "+ship+" wingmen contribute actual combat fire");
   float time=supportRemaining;var go=squadron[0].go;Vector3 position=go.transform.position;Pause();yield return new WaitForSecondsRealtime(.2f);
   check(supportRemaining==time&&go.transform.position==position,"Ship "+ship+" support duration and movement freeze while paused");Pause();
   CallSquadron();check(squadron.Count==2&&supportRemaining>time&&supportRemaining<=38,"Repeated beacon extends support without duplicating pilots");
   supportRemaining=.01f;TickSquadron(.02f);check(supportDeparting&&squadron.Count==2,"Expired support enters a visible departure phase");TickSquadron(2.5f);check(squadron.Count==0,"Departed wingmen are removed after leaving formation");
  }
  Ship=0;BeginRun();Launch();AutoFire=false;invuln=999;ClearBattle();
  var ward=SpawnEnemy(5,new Vector3(0,1,5),0);var protectedEnemy=SpawnEnemy(2,new Vector3(1,1,5),0);float protectedHp=protectedEnemy.hp;DamageEnemy(protectedEnemy,10);float shielded=protectedHp-protectedEnemy.hp;DamageEnemy(ward,1000);protectedHp=protectedEnemy.hp;DamageEnemy(protectedEnemy,10);
  check(shielded<5&&protectedHp-protectedEnemy.hp>9,"Destroying the shield drone removes protection from nearby enemies");
  ClearBattle();var lancer=SpawnEnemy(6,new Vector3(0,1,8),0);lancer.fire=.01f;TickSpecialEnemy(lancer,.01f);check(lancer.aimReady&&Bullets.Count==0,"Rail lancer provides a locked telegraph before firing");lancer.fire=0;TickSpecialEnemy(lancer,.01f);check(Bullets.FindAll(b=>!b.friendly&&b.velocity.magnitude>8).Count==4,"Rail lancer releases a distinct fast four-round burst");
  ClearBattle();var tender=SpawnEnemy(7,new Vector3(0,1,8),0);tender.fire=0;TickSpecialEnemy(tender,.01f);check(Enemies.Exists(e=>e.kind==10),"Mine tender deploys separate destructible mines");
  ClearBattle();var carrier=SpawnEnemy(8,new Vector3(0,1,8),0);carrier.fire=0;TickSpecialEnemy(carrier,.01f);check(Enemies.FindAll(e=>e.kind==0).Count==2,"Drone carrier launches two independently simulated aircraft");
  ClearBattle();int contactHull=Hull;invuln=0;var rammer=SpawnEnemy(4,PlayerPos,0);rammer.phase=0;TickSpecialEnemy(rammer,.01f);check(Hull==contactHull-1,"New attack aircraft retain the same player contact damage rules");
  for(int ship=0;ship<3;ship++)for(int route=0;route<3;route++){
   Ship=ship;selectedRoutes[ship]=route;BeginRun();Launch();AutoFire=false;ClearBattle();invuln=0;float baseCadence=ShotCadence;GainRouteExperience(85);
   check(RouteTier==3,"Ship "+ship+" route "+route+" progresses through three specialization tiers");
   var foe=SpawnEnemy(2,new Vector3(0,1,2),0);foe.hp=foe.maxHp=10000;Vector3 original=PlayerPos;UsePilotSkill();
   bool effect=ship==0?(route==0?Bullets.Exists(b=>b.homing&&b.pierce==2):route==1?squadron.Count==2:PlayerPos!=original):ship==1?(route==0?foe.hp<10000:route==1?routeShield>=6:SkillTime==7&&ShotCadence<baseCadence):route==0?foe.hp<10000:route==1?SkillTime>6:PlayerPos!=original;
   check(effect,"Ship "+ship+" route "+route+" changes live skill behavior");
  }
  Ship=0;selectedRoutes[0]=1;BeginRun();Launch();AutoFire=false;ClearBattle();int hull=Hull;invuln=0;HitPlayer();check(Hull==hull&&routeShield==0,"Escort regeneration absorbs one actual incoming hit");invuln=0;HitPlayer();check(Hull==hull-1,"A depleted escort shield does not grant unlimited protection");
  for(int i=0;i<3;i++)selectedRoutes[i]=0;Ship=0;BeginRun();Launch();AutoFire=false;ClearBattle();
  // Exercise queue invalidation against a real target, without persisting QA preferences.
  bool enabled=VoiceEnabled,canPlay=voiceCanPlay;VoiceEnabled=true;voiceCanPlay=true;qaRunning=false;voiceSawState=true;voiceLastState=State;voiceLastShip=Ship;voiceLastRadio=RadioText;
  var contextTarget=SpawnEnemy(5,new Vector3(0,1,8),0);MissionSay("ward_contact",85,()=>Enemies.Contains(contextTarget),false);check(voiceQueue.Count==1,"A live threat queues its matching actor and event dialogue");
  Enemies.Remove(contextTarget);Destroy(contextTarget.go);UpdateVoices(0);check(voiceQueue.Count==0&&voiceCurrent==null,"Destroying a threat cancels its now-obsolete queued warning");
  qaRunning=true;VoiceEnabled=enabled;voiceCanPlay=canPlay;ClearVoices();CallSquadron();SetupHangar();check(squadron.Count==0&&Enemies.Count==0,"Returning to hangar clears wingmen, hostile actors and combat context");
 }
}
}
