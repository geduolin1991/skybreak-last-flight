using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace Skybreak {
public partial class SkyGame {
 IEnumerator VerifyCompleteCampaign112(Action<bool,string> check,string output){
  qaRunning=true;LoadCampaignStory();check(storyLibrary.sequences.Length==11,"Campaign contains prologue, chapter bridges, three endings and three pilot codas");
  foreach(var sequence in storyLibrary.sequences){
   bool valid=sequence.beats.Length>0;
   foreach(var beat in sequence.beats)valid&=beat.world>=0&&beat.world<3&&beat.speaker>=0&&beat.speaker<3&&beat.body.Length>15&&voiceById.ContainsKey(beat.voice);
   check(valid,"Authored story sequence is complete: "+sequence.id);
  }
  Ship=0;BeginRun();StartStory("before_0");int hull=Hull;float clock=StageTime;
  yield return new WaitForSecondsRealtime(.3f);Launch();
  check(StoryActive&&State==FlightState.Briefing&&StageTime==clock&&Hull==hull&&!Player.activeSelf,"Story keeps mission time and hull unchanged and blocks accidental launch");
  var snapshot=MobileStatus();check(snapshot.storyActive&&snapshot.storyBody.Length>15&&snapshot.storySpeaker==StorySpeaker&&snapshot.storyCount==3,"Mobile story snapshot includes full prose and the current portrait actor together");
  ScreenCapture.CaptureScreenshot(Path.Combine(output,"112-prologue.png"));yield return new WaitForSecondsRealtime(.15f);
  WebMobileMode("1");WebMobileAction("storyNext");check(StoryIndex==1,"Touch next advances exactly one story beat");WebMobileAction("storyAuto");check(storyAuto,"Touch autoplay is available without holding a button");
  WebMobileAction("storySkip");check(!StoryActive&&State==FlightState.Briefing&&Player.activeSelf,"Skipping presentation returns to the playable briefing");WebMobileMode("0");Launch();
  foreach(int ending in new[]{0,1,2}){
   State=FlightState.Victory;for(int i=0;i<3;i++){missionResults[i]=1;chapterNodeMasks[i]=7;chapterNodes[i]=3;rescuedHull[i]=100;}
   if(ending==1){missionResults[0]=-1;rescuedHull[2]=0;}
   if(ending==2){missionResults[2]=-1;chapterNodes[2]=2;chapterNodeMasks[2]=3;}
   check(CampaignEnding==ending,"Ending "+ending+" follows actual civilian and uplink outcomes");
   if(ending==1)check(CampaignReport.Contains("580 人")&&CampaignReport.Contains("2 / 3 艘"),"Outcome report counts surviving passengers instead of claiming a perfect rescue");
   PlayEpilogueScore();StartStory("ending_"+ending,true);
   check(StoryCount==4&&StoryActive,"Ending "+ending+" includes the selected pilot's personal coda");
   int pages=0;while(StoryActive&&pages<8){
    var beat=storyBeats[StoryIndex];check(World.Stage==beat.world,"Ending "+ending+" shot "+pages+" presents its corresponding map");
    if(pages==0){yield return new WaitForSecondsRealtime(.2f);ScreenCapture.CaptureScreenshot(Path.Combine(output,"112-ending-"+ending+".png"));yield return new WaitForSecondsRealtime(.1f);}
    AdvanceStory();pages++;
   }
   check(pages==4&&!StoryActive&&State==FlightState.Victory,"Ending "+ending+" completes and returns to results");
  }
  Ship=0;BeginRun();Launch();AutoFire=false;spawnClock=999;missionStarted=missionResolved=true;ClearBattle();
  var wreck=SpawnEnemy(1,new Vector3(0,1,4),0);var visual=wreck.go;DamageEnemy(wreck,99999);
  check(ActiveWrecks==1&&visual.activeSelf&&!Enemies.Contains(wreck),"Destroyed aircraft stays visible during its fall with no active hostile collision");
  Vector3 before=visual.transform.position;TickWrecks(.2f);check(visual.transform.position.y<before.y,"A destroyed aircraft falls below the battle plane");
  for(int i=0;i<20;i++){var e=SpawnEnemy(0,new Vector3(i%3,1,5),0);DamageEnemy(e,99999);}check(ActiveWrecks<=9,"Burst destruction remains within nine reusable wreck slots");
  TickWrecks(2);check(ActiveWrecks==0,"All finished wreck visuals return to their pools");
  Stage=1;BeginStage();Launch();AutoFire=false;spawnClock=999;missionStarted=missionResolved=true;ClearBattle();
  check(World.CampaignMovingParts>=3&&World.Civilians.Count==3&&World.MovingEvacuationVehicles==0,"City includes three district trains and three waiting evacuation buses");
  World.SetDistrictPower(0);yield return new WaitForSeconds(.3f);
  check(World.MovingEvacuationVehicles==1,"Restoring one district releases only its evacuation bus");
  float storm=World.StormAmount;World.SetDistrictPower(1);World.SetDistrictPower(2);yield return new WaitForSeconds(1.2f);
  check(World.StormAmount<storm-.1f&&World.MovingEvacuationVehicles==3,"Restored city visibly calms its storm and releases all three evacuation vehicles");
  ScreenCapture.CaptureScreenshot(Path.Combine(output,"112-city-restored.png"));yield return new WaitForSeconds(.15f);
  Stage=2;BeginStage();Launch();AutoFire=false;spawnClock=999;missionStarted=missionResolved=true;ClearBattle();
  check(World.CampaignMovingParts>=5,"Orbit retains Blender-authored telescope and solar-wing pivots");
  foreach(var name in new[]{"HarborSignal112","CivicHospital112","ElevatedRail112","MetroCar112","OrbitalObservatory112","SolarSail112"})check(Resources.Load<GameObject>("Models/"+name)!=null,"Campaign scenery is a prepared model: "+name);
  SpawnBoss();Boss.pos=new Vector3(0,1,8);Boss.age=12;bossSpawned=true;yield return new WaitForSeconds(1.5f);
  check(music.clip==Clip("MusicSeraph")&&music.isPlaying&&!outgoingMusic.isPlaying,"Boss entrance crossfades to its dedicated score and retires the old cue");
  foreach(var node in Enemies.ToArray())if(node.owner==Boss)DamageEnemy(node,99999);
  Boss.coreExpose=0;bossShots=1;bossShotClock=0;bossTell=bossPhaseBreak=0;Boss.attackClock=5;TickBossEncounter(Boss,2,.01f);
  check(Boss.coreExpose>1.7f,"Finishing an unshielded boss volley opens a readable counterattack window");
  AddBeam(PlayerPos.x);AddBullet(PlayerPos+Vector3.forward,Vector3.back,false,1);DamageEnemy(Boss,99999);
  check(Beams.Count==0&&Bullets.TrueForAll(b=>b.friendly),"Boss defeat clears residual hostile beams and projectiles before final dialogue");
  int protectedHull=Hull;for(int i=0;i<25;i++){Tick(.05f,false,false);HitPlayer();}
  check(Hull==protectedHull&&State==FlightState.Playing,"Final boss transmission stays safe from late damage throughout the exit delay");
  ClearBattle();check(ActiveWrecks==0,"Leaving a battle releases every retained wreck visual");
  foreach(string name in new[]{"MusicLeviathan","MusicTempest","MusicSeraph","MusicDawn","MusicAftermath","AmbienceSea","AmbienceCity","AmbienceOrbit","AmbienceHangar","ObjectiveChime"})check(Clip(name)&&Clip(name).length>1,"Campaign audio loads: "+name);
  SetupHangar();yield return new WaitForSeconds(1.5f);var cue=music.clip;float playhead=music.time;SelectShip(1);SelectShip(2);yield return new WaitForSeconds(.2f);
  check(music.clip==cue&&music.time>playhead,"Campaign audio changes do not restart music when pilots are selected");
  Ship=0;BeginRun();Launch();AutoFire=false;
 }
}
}
