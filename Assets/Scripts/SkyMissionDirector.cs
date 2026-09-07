using UnityEngine;
using System;
using System.Collections.Generic;
namespace Skybreak {
public partial class SkyGame {
 readonly List<Hostile> missionTargets=new List<Hostile>();
 readonly HashSet<string> missionSpoken=new HashSet<string>();
 float convoyWaveClock,missionSupplyClock,uplinkClock;int missionNodes;bool missionStarted,missionResolved;
 readonly int[] missionResults=new int[3];
 public int MissionNodes=>missionNodes;
 public int ConvoySurvivors {get{int n=0;if(World)foreach(var c in World.Civilians)if(c.health>0)n++;return n;}}
 public float ConvoyIntegrity {get{float sum=0;if(World)foreach(var c in World.Civilians)sum+=c.health;return sum/300f;}}
 public string MissionStatus=>Stage==0?"撤离船队  "+ConvoySurvivors+" / 3  ·  "+Mathf.CeilToInt(ConvoyIntegrity*100)+"%":Stage==1?"城市电网  "+missionNodes+" / 3":"轨道接点  "+missionNodes+" / 3"+(World.UplinkActive?"  ·  "+Mathf.CeilToInt(World.UplinkProgress*100)+"%":"");
 void ResetCampaignMission(){for(int i=0;i<3;i++)missionResults[i]=0;}
 void BeginMission(){missionTargets.Clear();missionSpoken.Clear();missionNodes=0;missionStarted=missionResolved=false;convoyWaveClock=14;missionSupplyClock=11;uplinkClock=0;World.BeginLivingMission();}
 void MissionSay(string id,int priority=85,Func<bool> valid=null,bool once=true){
  if(once&&!missionSpoken.Add(id))return;if(!voiceById.TryGetValue(id,out var entry))return;
  if(valid!=null&&!valid())return;
  pilotSpeaker=entry.pilot;RadioName=PilotNames[entry.pilot]+" / 实时通讯";RadioText=entry.text;voiceLastRadio=RadioText;dialogClock=Mathf.Clamp(entry.text.Length*.14f,3,7);
  int stage=Stage;Func<bool> context=()=>Stage==stage&&State==FlightState.Playing&&(valid==null||valid());
  QueueVoice(entry,priority,0,true);var request=voiceQueue.Find(r=>r.entry.id==id);if(request!=null){request.valid=context;request.expires=Time.unscaledTime+8;}
 }
 void TickMission(float dt){
  World.TickLivingMission(dt);TickSquadron(dt);TickSpecialization(dt);
  if(stageEndClock>0)return;
  if(!missionStarted&&StageTime>3){missionStarted=true;
   MissionSay("world_intro_"+Stage,88);
   if(Stage>0)for(int i=0;i<3;i++){
    var e=SpawnEnemy(9,new Vector3((i-1)*6,1,Stage==1?5+i%2*3:7+i%2*2),0);e.missionIndex=i;e.hp=e.maxHp=(Difficulty==0?210:280)*(1+Stage*.15f);missionTargets.Add(e);
   }
  }
  if(!missionStarted)return;
  missionSupplyClock-=dt;if(missionSupplyClock<=0&&StageTime<102){missionSupplyClock=35;SpawnSupply(new Vector3(Mathf.Clamp(PlayerPos.x,-7,7),1,13),3);MissionSay("support_beacon",64,()=>Supplies.Exists(p=>p.kind==3),false);}
  if(Stage==0&&StageTime<110){convoyWaveClock-=dt;if(convoyWaveClock<=0&&ConvoySurvivors>0){convoyWaveClock=22;int target=((int)(StageTime/22))%3;if(World.Civilians[target].health<=0)for(int i=0;i<3;i++)if(World.Civilians[i].health>0){target=i;break;}
    var e=SpawnEnemy(4,new Vector3(World.Civilians[target].station.x,1,17),0);e.missionIndex=target;MissionSay("convoy_bomber",93,()=>Enemies.Contains(e),false);
   }}
  if(Stage==2&&missionNodes==3&&!missionResolved){uplinkClock+=dt;World.SetUplink(uplinkClock/8);if(uplinkClock>=8){World.SetUplink(1,true);ResolveMission(true);World.DepartCivilians();MissionSay("orbit_connected",94);}}
 }
 void OnMissionTargetDestroyed(Hostile e){
  if(e.kind==4&&e.missionIndex>=0)MissionSay("convoy_intercept",71,()=>Stage==0&&ConvoySurvivors>0,false);
  if(e.kind==9&&!missionResolved){missionTargets.Remove(e);missionNodes++;World.SetDistrictPower(e.missionIndex);Score+=1800;SpawnSupply(e.pos,2);Shockwave(e.pos,Art.Cyan,3,.65f);
   int completedNode=missionNodes;
   if(Stage==1){MissionSay("city_grid_"+missionNodes,missionNodes==3?93:90,()=>missionNodes==completedNode);if(missionNodes==3)ResolveMission(true);}
   else {MissionSay("orbit_node_"+missionNodes,missionNodes==3?93:90,()=>missionNodes==completedNode&&(completedNode<3||!World.UplinkComplete));if(missionNodes==3)World.SetUplink(0);}
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
 void ClearHostileProjectiles(){for(int i=Enemies.Count-1;i>=0;i--)if(Enemies[i].kind==10||Enemies[i].kind==11){if(Enemies[i].go)Destroy(Enemies[i].go);Enemies.RemoveAt(i);}}
 void DrawLivingMission(){
  if(State!=FlightState.Playing)return;
  if(Stage==0)for(int i=0;i<World.Civilians.Count;i++){var c=World.Civilians[i];if(!c.go||c.station.z>18)continue;Vector3 v=Cam.WorldToViewportPoint(c.go.transform.position+Vector3.back*1.9f);float x=v.x*1600,y=(1-v.y)*900;if(x<280||x>1320||y<100||y>780)continue;Rect(x-62,y-5,124,39,new Color(.005f,.03f,.05f,.82f));Label(new[]{"曙光号","归港号","白鹭号"}[i]+(c.health<=0?" · 失能":c.departed?" · 撤离":""),x-58,y-2,116,23,12,c.health>40?paper:Art.Orange,TextAnchor.MiddleCenter);Bar(x-50,y+24,100,c.health/100,c.health>40?accent:Art.Orange,3);}
  foreach(var e in missionTargets){if(!e.go||!Enemies.Contains(e))continue;Vector3 v=Cam.WorldToViewportPoint(e.pos);float x=v.x*1600,y=(1-v.y)*900;Rect(x-52,y+28,104,35,new Color(.02f,.015f,.03f,.88f));Label((Stage==1?"干扰塔 ":"封锁接点 ")+(e.missionIndex+1),x-50,y+29,100,22,12,paper,TextAnchor.MiddleCenter);Bar(x-43,y+56,86,e.hp/e.maxHp,Art.Orange,3);}
  if(Stage==2&&World.UplinkActive&&!World.UplinkComplete){Panel(581,115,438,52);Label("雪璃正在上传识别密钥  "+Mathf.CeilToInt(World.UplinkProgress*100)+"%",594,123,412,26,16,accent,TextAnchor.MiddleCenter);Bar(600,153,400,World.UplinkProgress,Art.Violet,3);}
  DrawSquadronHUD();DrawSpecializationHUD();
 }
}
}
