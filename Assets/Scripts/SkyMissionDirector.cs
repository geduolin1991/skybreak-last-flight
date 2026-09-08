using UnityEngine;
using System;
using System.Collections.Generic;
namespace Skybreak {
public partial class SkyGame {
 readonly List<Hostile> missionTargets=new List<Hostile>();
 readonly HashSet<string> missionSpoken=new HashSet<string>();
 float missionSupplyClock,uplinkClock;int missionNodes,missionNodesSpawned,convoyAttackIndex;bool missionStarted,missionResolved;
 static readonly float[] convoyAttackTimes={12,24,56};
 readonly int[] missionResults=new int[3];
 public int MissionNodes=>missionNodes;
 public int ConvoySurvivors {get{int n=0;if(World)foreach(var c in World.Civilians)if(c.health>0)n++;return n;}}
 public float ConvoyIntegrity {get{float sum=0;if(World)foreach(var c in World.Civilians)sum+=c.health;return sum/300f;}}
 public string MissionStatus=>Stage==0?"撤离船队  "+ConvoySurvivors+" / 3  ·  "+Mathf.CeilToInt(ConvoyIntegrity*100)+"%":Stage==1?"城市电网  "+missionNodes+" / 3":"轨道接点  "+missionNodes+" / 3"+(World.UplinkActive?"  ·  "+Mathf.CeilToInt(World.UplinkProgress*100)+"%":"");
 void ResetCampaignMission(){for(int i=0;i<3;i++)missionResults[i]=0;}
 void BeginMission(){missionTargets.Clear();missionSpoken.Clear();missionNodes=missionNodesSpawned=convoyAttackIndex=0;missionStarted=missionResolved=false;missionSupplyClock=21;uplinkClock=0;World.BeginLivingMission();}
 void MissionSay(string id,int priority=85,Func<bool> valid=null,bool once=true){
  if(valid!=null&&!valid())return;if(!voiceById.TryGetValue(id,out var entry))return;
  if(once&&!missionSpoken.Add(id))return;
  pilotSpeaker=entry.pilot;RadioName=PilotNames[entry.pilot]+" / 实时通讯";RadioText=entry.text;voiceLastRadio=RadioText;dialogClock=Mathf.Clamp(entry.text.Length*.14f,3,7);
  int stage=Stage;Func<bool> context=()=>Stage==stage&&State==FlightState.Playing&&(valid==null||valid());
  float delay=stageEndClock>0?Mathf.Max(0,commanderRadioClock):0;
  if(stageEndClock>0&&!qaRunning)stageEndClock=Mathf.Max(stageEndClock,delay+VoiceDuration(entry)+.5f);
  QueueVoice(entry,priority,delay,true);var request=voiceQueue.Find(r=>r.entry.id==id);if(request!=null){request.valid=context;request.expires=Time.unscaledTime+delay+8;}
 }
 void TickMission(float dt){
  World.TickLivingMission(dt);TickSquadron(dt);TickSpecialization(dt);
  if(stageEndClock>0)return;
  if(!missionStarted&&StageTime>3){missionStarted=true;
   MissionSay("world_intro_"+Stage,88);
   if(Stage==1&&missionResults[0]>0){SpawnSupply(PlayerPos+Vector3.forward*6,3);Toast("船队回礼 · 支援信标已送达",3);}
   if(Stage==2&&missionResults[1]>0){Energy=Mathf.Min(100,Energy+35);Toast("城市导航站已接通 · 反应堆 +35",3);}
  }
  if(!missionStarted)return;
  // Reveal each district/contact at a different beat, giving its result time
  // to register before the next target arrives. Unfinished targets persist.
  if(Stage>0)while(missionNodesSpawned<3&&StageTime>=(missionNodesSpawned==0?3:chapterBeats[Stage][missionNodesSpawned])){
    int i=missionNodesSpawned++;
    var e=SpawnEnemy(9,new Vector3((i-1)*6,1,Stage==1?5+i%2*3:7+i%2*2),0);e.missionIndex=i;e.hp=e.maxHp=(Difficulty==0?210:280)*(1+Stage*.15f);missionTargets.Add(e);
    if(i>0)Toast(Stage==1?"发现下一座干扰塔 · 街区 "+(i+1):"锁定新的封锁接点 · "+(i+1)+" / 3",3);
  }
  missionSupplyClock-=dt;if(missionSupplyClock<=0&&StageTime<BossWarningTime-8&&!EncounterRecovery){missionSupplyClock=36;SpawnSupply(new Vector3(Mathf.Clamp(PlayerPos.x,-7,7),1,7),3);MissionSay("support_beacon",64,()=>Supplies.Exists(p=>p.kind==3),false);}
  if(Stage==0&&StageTime<BossWarningTime&&convoyAttackIndex<convoyAttackTimes.Length&&StageTime>=convoyAttackTimes[convoyAttackIndex]){
   int target=convoyAttackIndex++;if(ConvoySurvivors>0){if(World.Civilians[target].health<=0)for(int i=0;i<3;i++)if(World.Civilians[i].health>0){target=i;break;}
    var e=SpawnEnemy(4,new Vector3(World.Civilians[target].station.x,1,17),0);e.missionIndex=target;MissionSay("convoy_bomber",93,()=>Enemies.Contains(e)&&!e.retreating,false);
   }}
  if(Stage==2&&missionNodes==3&&!missionResolved){uplinkClock+=dt;World.SetUplink(uplinkClock/8);if(uplinkClock>=8){World.SetUplink(1,true);ResolveMission(true);World.DepartCivilians();MissionSay("orbit_connected",94);}}
 }
 void OnMissionTargetDestroyed(Hostile e){
  if(e.kind==4&&e.missionIndex>=0)MissionSay("convoy_intercept",71,()=>Stage==0&&ConvoySurvivors>0,false);
  if(e.kind==9&&!missionResolved){missionTargets.Remove(e);missionNodes++;World.SetDistrictPower(e.missionIndex);Score+=1800;SpawnSupply(e.pos,2);Shockwave(e.pos,Art.Cyan,3,.65f);
   int completedNode=missionNodes;
   if(Stage==1){MissionSay("city_grid_"+missionNodes,missionNodes==3?93:90,()=>missionNodes==completedNode);if(missionNodes==3)ResolveMission(true);}
   else {MissionSay("orbit_node_"+missionNodes,missionNodes==3?93:90,()=>missionNodes==completedNode&&(completedNode<3||!World.UplinkComplete));if(missionNodes==3){World.SetUplink(0);if(Boss==null&&StageTime<BossWarningTime)for(int side=-1;side<=1;side+=2)SpawnEnemy(1,new Vector3(side*7,1,15),6);}}
  }
 }
 void ConvoyImpact(Hostile payload){
  int i=payload.missionIndex;if(Stage!=0||i<0||i>=World.Civilians.Count||World.Civilians[i].departed)return;
  var c=World.Civilians[i];World.DamageCivilian(i,Difficulty==0?18:25);Burst(c.go.transform.position+Vector3.up*.3f,28,1,.7f);Shockwave(c.go.transform.position,Art.Orange,2.1f,.5f);Sound("ExplosionHeavy",.4f);
  MissionSay(c.health<=0?"convoy_disabled":"convoy_hit",97,()=>Stage==0,false);
 }
 void ResolveMission(bool success){if(missionResolved)return;missionResolved=true;sideObjectiveComplete=success;missionResults[Stage]=success?1:-1;if(success){rescueSignals++;Score+=3000;GainRouteExperience(5);Toast("民用行动完成  +3,000  ·  专精经验 +5",3.5f);}else Toast("民用目标未全部完成 · 战斗继续",3);}
 void MissionBossDefeated(){
  if(Stage==0){ResolveMission(ConvoySurvivors==3);World.DepartCivilians();MissionSay(ConvoySurvivors==3?"coast_clear_all":ConvoySurvivors>0?"coast_clear_some":"coast_clear_lost",96);}
  else if(Stage==1){if(!missionResolved)ResolveMission(false);MissionSay(missionNodes==3?"city_clear_power":"city_clear_partial",96);}
  else {if(!missionResolved)ResolveMission(false);MissionSay(World.UplinkComplete?"orbit_clear_link":"orbit_clear_partial",96);}
  ClearHostileProjectiles();BeginSquadronDeparture(false);
 }
 void ClearHostileProjectiles(){for(int i=Enemies.Count-1;i>=0;i--)if(Enemies[i].kind==10||Enemies[i].kind==11){ReleaseEnemyVisual(Enemies[i]);Enemies.RemoveAt(i);}}
 void DrawLivingMission(){
  if(State!=FlightState.Playing)return;
  if(Stage==0){
   for(int i=0;i<World.Civilians.Count;i++){var c=World.Civilians[i];float y=543+i*16;
    Label(new[]{"曙光","归港","白鹭"}[i],1370,y-4,47,19,10,muted);
    Bar(1420,y+4,104,c.health/100,c.health<=40?Art.Orange:accent,3);
    if(c.health<=0)Label("失能",1529,y-4,35,18,10,Art.Orange);}
  }
  // Ground objective health stays below the target in world space, so aircraft occlude it.
  if(Stage==2&&World.UplinkActive&&!World.UplinkComplete){Panel(581,115,438,52);Label("雪璃正在上传识别密钥  "+Mathf.CeilToInt(World.UplinkProgress*100)+"%",594,123,412,26,16,accent,TextAnchor.MiddleCenter);Bar(600,153,400,World.UplinkProgress,Art.Violet,3);}
  DrawSquadronHUD();DrawSpecializationHUD();
 }
}
}
