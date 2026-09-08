using System;
using UnityEngine;

namespace Skybreak {
public partial class SkyGame {
 void VerifyPacing19(Action<bool,string> check){
  Ship=0;Difficulty=0;BeginRun();Launch();AutoFire=false;ClearBattle();
  StageTime=0;SpawnWave(0);
  check(Enemies.Count==3&&Enemies.TrueForAll(e=>e.kind==0),"Opening uses a readable three-drone formation before specialist enemies");
  int kills=Kills,score=Score;var retreat=Enemies[0];
  StageTime=48;TickEncounterDirector(0);int count=Enemies.Count;
  TickEncounterSpawns(3);
  check(EncounterRecovery&&Enemies.Count==count&&Enemies.TrueForAll(e=>e.retreating),"Recovery recalls active aircraft and suspends new waves");
  check(Supplies.Count==2&&comboClock>=9,"Recovery offers two real pickups and preserves the combo between fights");
  Vector3 before=retreat.pos;TickWithdrawal(retreat,.5f);
  check(retreat.pos.z>before.z&&Kills==kills&&Score==score,"Withdrawals visibly leave the route without granting fake kills");
  ClearBattle();StageTime=54;TickEncounterDirector(0);
  for(int i=0;i<20;i++){spawnClock=0;TickEncounterSpawns(.05f);}
  check(ActiveAirThreats<=AirThreatBudget,"Authored formations respect the active aircraft budget under repeated spawn requests");
  StageTime=70;TickEncounterDirector(0);
  check(Boss==null&&warningClock==5&&CurrentEncounterSection==5,"First boss has a full five-second warning after the final wave");
  StageTime=75;TickEncounterSpawns(.02f);var firstBoss=Boss;
  check(Boss!=null&&bossArrived&&BossArrivalTime==75,"First boss enters at 75 combat seconds");
  TickEncounterSpawns(.02f);check(Boss==firstBoss,"Boss arrival is idempotent and cannot spawn duplicate bosses");
  DamageEnemy(Boss,100000);TickEncounterSpawns(.02f);
  check(Boss==null&&bossArrived,"A defeated boss never respawns while its debrief is playing");
  for(int stage=1;stage<3;stage++){
   Stage=stage;BeginStage();Launch();AutoFire=false;ClearBattle();
   StageTime=BossArrivalTime+30;TickEncounterSpawns(.02f);
   check(Boss!=null,"Chapter "+stage+" boss cannot be missed when a frame passes its arrival threshold");
  }
  Stage=1;BeginStage();Launch();AutoFire=false;ClearBattle();StageTime=70;missionNodes=0;corridorClock=0;spawnClock=999;
  PlayerPos.x=4;TickEncounterSpawns(.05f);int darkBeams=Beams.Count;
  foreach(var beam in Beams)if(beam.go)Destroy(beam.go);Beams.Clear();missionNodes=3;corridorClock=0;TickEncounterSpawns(.05f);
  check(darkBeams==2&&Beams.Count==1&&corridorClock==12,"Restored city power reduces the actual storm corridor from paired beams to a slower single beam");
  Stage=2;BeginStage();Launch();ClearBattle();StageTime=80;World.SetUplink(1,true);corridorClock=0;spawnClock=999;TickEncounterSpawns(.05f);
  check(Beams.Count==0,"Recognized orbital signals disable environmental blockade beams");
  check(World.StarDrawCalls==1,"180 distant orbital stars share a single static mesh draw");
  BeginRun();Launch();AutoFire=false;missionResults[0]=1;Stage=1;BeginStage();Launch();StageTime=4;TickMission(0);
  check(Supplies.Exists(p=>p.kind==3),"A rescued coast convoy sends a usable support beacon into the next chapter");
  missionResults[1]=1;Stage=2;BeginStage();Launch();Energy=10;StageTime=4;TickMission(0);
  check(Energy==45,"Restored city navigation supplies 35 reactor energy for the orbital chapter");
  TickMission(0);check(Energy==45,"Cross-chapter aid is awarded only once");
  BeginRun();Launch();StageTime=4;TickMission(0);check(missionResults[0]==0&&missionResults[1]==0,"Restart clears previous civilian outcomes and their aid");
  StageTime=30;float attackMusic=EncounterMusic;StageTime=48;
  check(attackMusic>.5f&&EncounterMusic<.05f,"Counterattack and recovery have clearly different synchronized music intensity");
  check(Shader.Find("Skybreak/Ocean").isSupported&&Shader.Find("Skybreak/Ground").isSupported&&Shader.Find("Skybreak/Finish").isSupported,"Revised ocean, wet ground and finishing shaders are supported by the player");
  foreach(string model in new[]{"CoastalBreakwater","CivicPlaza","OrbitalTruss"}){
   var asset=Resources.Load<GameObject>("Models/"+model);int vertices=0;if(asset)foreach(var mesh in asset.GetComponentsInChildren<MeshFilter>())if(mesh.sharedMesh)vertices+=mesh.sharedMesh.vertexCount;
   check(vertices>500,"New Blender environment asset has detailed imported geometry: "+model+" / "+vertices+" vertices");
  }
  check(World.SectorRenderersAfter<World.SectorRenderersBefore,"Detailed environments retain material-batched scenery");
  BeginRun();Launch();AutoFire=false;
 }
}
}
