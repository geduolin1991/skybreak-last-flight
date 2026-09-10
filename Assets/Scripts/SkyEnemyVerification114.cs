using UnityEngine;
using System;
using System.Collections;
using System.IO;
namespace Skybreak {public partial class SkyGame {
 IEnumerator VerifyEnemy114(Action<bool,string> check,string output){
  Ship=0;BeginRun();Launch();ClearBattle();AutoFire=false;spawnClock=999;missionStarted=missionResolved=true;invuln=999;stageBanner=dialogClock=toastClock=0;
  int[] modelBudget={4000,5000,14000,9000,6000,3000,8000,8000,9000};
  for(int k=0;k<9;k++){var e=SpawnEnemy(k,new Vector3((k%3-1)*5.2f,1,11-(k/3)*5.3f),0);e.fire=999;int triangles=0;foreach(var f in e.go.GetComponentsInChildren<MeshFilter>())triangles+=f.sharedMesh.triangles.Length/3;check(triangles>=1000&&triangles<=modelBudget[k]&&e.renderers.Length<=12,"Enemy "+k+" has imported foundry detail within the combat renderer budget");}
  yield return new WaitForSeconds(.12f);ScreenCapture.CaptureScreenshot(Path.Combine(output,"114-enemy-fleet.png"));yield return new WaitForSeconds(.10f);ClearBattle();
  var heavy=SpawnEnemy(2,new Vector3(0,1,4),0);heavy.fire=999;float hp=heavy.hp;DamageEnemy(heavy,20);float protectedHit=hp-heavy.hp;EnemyAttack(heavy);TickEnemyArmor(heavy,.2f);float openedHp=heavy.hp;DamageEnemy(heavy,20);float exposedHit=openedHp-heavy.hp;
  check(heavy.maxHp>6*EnemyHealth[1]&&exposedHit>protectedHit*2,"Heavy gunship survives sustained fire but takes more than double damage during its counter window");
  check(heavy.ventBlend>.8f&&heavy.go.GetComponent<SkyEnemyVisual>().vents.Length==2,"Heavy attack physically opens both authored radiator shutters");
  stageBanner=dialogClock=toastClock=0;yield return new WaitForSeconds(.10f);ScreenCapture.CaptureScreenshot(Path.Combine(output,"114-armor-open.png"));yield return new WaitForSeconds(.10f);
  heavy.fire=0;int beforeVolley=armorOpenings;for(int tick=0;tick<40&&armorOpenings==beforeVolley;tick++)UpdateEnemies(.05f);bool volleyFired=armorOpenings>beforeVolley;for(int tick=0;tick<40;tick++)UpdateEnemies(.05f);check(volleyFired&&heavy.ventOpen==0&&heavy.ventBlend==0&&heavy.fire>.5f,"Heavy gunship closes its cooling shutters before the next live volley");
  heavy.hp=heavy.maxHp*.3f;TickEnemyArmor(heavy,.1f);check(heavy.armorBroken&&EnemyArmorScale(heavy)>1,"Low hull permanently breaks heavy armor instead of restoring protection");
  ClearBattle();var carrier=SpawnEnemy(8,new Vector3(0,1,8),0);carrier.fire=0;TickSpecialEnemy(carrier,.01f);check(carrier.ventOpen>2&&CountEnemyKind(0)==2,"Carrier drone launch creates two threats and a real cooling vulnerability");TickEnemyArmor(carrier,.2f);bool opposite=true;foreach(var vent in carrier.go.GetComponent<SkyEnemyVisual>().vents){float angle=Mathf.DeltaAngle(0,vent.localEulerAngles.z);opposite&=vent.name.StartsWith("Motion_Vent_L")?angle<-45:angle>45;}check(opposite,"Carrier radiator shutters open in opposite directions despite imported name suffixes");ClearBattle();
  for(int kind=0;kind<4;kind++)SpawnSupply(new Vector3((kind-1.5f)*4.0f,1,3),kind);
  yield return new WaitForSeconds(.15f);check(Supplies.TrueForAll(s=>s.go.GetComponentsInChildren<MeshFilter>().Length>=4),"All four pickups use authored equipment meshes rather than primitive cores");ScreenCapture.CaptureScreenshot(Path.Combine(output,"114-supplies.png"));yield return new WaitForSeconds(.1f);
  foreach(var s in Supplies.ToArray()){Hull=MaxHull-1;Bombs=2;Energy=20;var beforePos=PlayerPos;PlayerPos=s.pos;int kind=s.kind;UpdateSupplies(.001f);check(kind==0?Hull==MaxHull:kind==1?Bombs==3:kind==2?Energy==45:WingmanCount==2,"Equipment pickup "+kind+" retains its real gameplay reward");PlayerPos=beforePos;}
  ClearBattle();PlayerPos=new Vector3(0,1,-8);Player.transform.position=PlayerPos;
  for(int stage=0;stage<3;stage++){
   Stage=stage;BeginStage();Launch();ClearBattle();AutoFire=false;spawnClock=999;missionStarted=missionResolved=true;invuln=999;stageBanner=dialogClock=toastClock=0;ClearVoices();
   int before=stage==0?waterCrashes:stage==1?groundCrashes:vacuumBreakups;var e=SpawnEnemy(1,new Vector3(-2,1,5),0);var go=e.go;int kills=Kills;DamageEnemy(e,10000);
   check(!Enemies.Contains(e)&&go.activeSelf&&Kills==kills+1,"Chapter "+stage+" removes wreck collision and pays the kill reward immediately");
   float deadline=Time.realtimeSinceStartup+3;while(go&&go.activeSelf&&go.transform.position.y>-.3f&&Time.realtimeSinceStartup<deadline)yield return null;check(go&&go.activeSelf&&go.transform.position.y<0,"Chapter "+stage+" wreck visibly falls below the flight plane");ScreenCapture.CaptureScreenshot(Path.Combine(output,"114-fall-"+stage+".png"));deadline=Time.realtimeSinceStartup+3;while(ActiveWrecks>0&&Time.realtimeSinceStartup<deadline)yield return null;yield return new WaitForSeconds(.20f);
   int after=stage==0?waterCrashes:stage==1?groundCrashes:vacuumBreakups;check(after==before+1,"Chapter "+stage+" resolves exactly one appropriate surface or vacuum impact");ScreenCapture.CaptureScreenshot(Path.Combine(output,"114-impact-"+stage+".png"));yield return new WaitForSeconds(.12f);
   if(stage<2){SurfaceImpact live=null;foreach(var impact in surfaceImpacts)if(impact.active)live=impact;check(live!=null&&live.root.position.y<-9&&live.plume.particleCount>0,"Impact "+stage+" has real particles on the background surface");if(live!=null){float z=live.root.position.z,travel=World.SceneryTravel;yield return new WaitForSeconds(.15f);check(Mathf.Abs(live.root.position.z-z+World.SceneryTravel-travel)<.3f,"Impact "+stage+" scrolls with the ground instead of sliding over it");}}
   check(Kills==kills+1,"Delayed impact "+stage+" cannot award the same kill twice");ClearBattle();
  }
  Stage=0;BeginStage();Launch();ClearBattle();missionStarted=missionResolved=true;spawnClock=999;AutoFire=false;invuln=999;
  int oldKills=Kills;Kills=2;int air=airBreakups;var drone=SpawnEnemy(0,new Vector3(2,1,5),0);DamageEnemy(drone,9999);check(airBreakups==air+1,"Some light aircraft break up in the air");yield return new WaitForSeconds(.27f);check(ActiveWrecks==0,"Air breakup retires its model promptly");Kills=oldKills;ClearBattle();
  int allocations=crashEffectsAllocated;for(int i=0;i<36;i++){var e=SpawnEnemy(2,new Vector3((i%7)-3,1,4),0);DamageEnemy(e,9999);}check(ActiveWrecks<=9,"Mass destruction keeps the fixed wreck budget");
  yield return new WaitForSeconds(1.3f);check(ActiveSurfaceImpacts<=10&&crashEffectsAllocated==allocations,"Mass impacts reuse the bounded surface-effect pool");
  Pause();float age=0;foreach(var s in surfaceImpacts)if(s.active){age=s.age;break;}yield return new WaitForSecondsRealtime(.15f);float pausedAge=0;foreach(var s in surfaceImpacts)if(s.active){pausedAge=s.age;break;}check(age==pausedAge,"Pause freezes delayed surface presentation");Pause();ClearBattle();check(ActiveSurfaceImpacts==0&&ActiveWrecks==0,"Changing chapter clears wrecks and all ground effects");
  foreach(var clip in new[]{"CrashWater114","CrashGround114","ArmorBreak114"})check(Clip(clip)!=null,"Destruction sound is packaged: "+clip);
  File.WriteAllText(Path.Combine(output,"114-destruction.json"),"{\"water\":"+waterCrashes+",\"ground\":"+groundCrashes+",\"vacuum\":"+vacuumBreakups+",\"air\":"+airBreakups+",\"pool\":"+crashEffectsAllocated+",\"armorOpenings\":"+armorOpenings+",\"armorBreaks\":"+armorBreaks+"}");
  Ship=0;BeginRun();Launch();AutoFire=false;
 }
}}
