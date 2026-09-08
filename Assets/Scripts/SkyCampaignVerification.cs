using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
namespace Skybreak {
public partial class SkyGame {
 IEnumerator VerifyCampaign(){
  qaRunning=true;Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=60;
  string output=Path.Combine(Directory.GetCurrentDirectory(),"QA");foreach(string arg in Environment.GetCommandLineArgs())if(arg.StartsWith("--sky-output="))output=arg.Substring(13);Directory.CreateDirectory(output);
  Difficulty=0;Ship=0;foreach(string arg in Environment.GetCommandLineArgs())if(arg.StartsWith("--sky-ship="))Ship=Mathf.Clamp(int.Parse(arg.Substring(11)),0,2);selectedRoutes[Ship]=0;AutoFire=true;MouseControl=false;researchFire=researchArmor=researchSkill=0;pilotXp[Ship]=0;
  var report=new List<string>();report.Add("Airframe: "+Ship+" / "+ShipNames[Ship]);report.Add("Natural campaign simulation: new pilot, no research, no invulnerability override, no time/health skips; 2x simulation speed.");
  UnityEngine.Random.InitState(50319+Ship);BeginRun();Launch();Time.timeScale=2;int recordedStage=-1,recordedBeat=-1;float[] bossTimes={-1,-1,-1};int[] phasesSeen={0,0,0};int peakThreats=0;float firstKill=-1;float elapsed=0,nextDecision=0;Vector2 control=Vector2.zero;int dodges=0,bombsUsed=0,skillsUsed=0;float start=Time.realtimeSinceStartup;
  while(Time.realtimeSinceStartup-start<450){
   yield return null;
   if(State==FlightState.Victory||State==FlightState.Defeat){report.Add("Final chapter end: "+StageTime.ToString("F2")+"s; boss phase mask "+phasesSeen[Stage]);break;}
   if(State==FlightState.Upgrade){report.Add("Chapter "+(Stage+1)+" end: "+StageTime.ToString("F2")+"s; boss phase mask "+phasesSeen[Stage]);int pick=0;for(int i=0;i<3;i++)if(UpgradeOptions[i]==0||(Hull<MaxHull/2&&UpgradeOptions[i]==1))pick=i;ApplyUpgrade(pick);Launch();Time.timeScale=2;}
   if(State!=FlightState.Playing)continue;
   if(Kills>0&&firstKill<0)firstKill=CampaignTime;peakThreats=Mathf.Max(peakThreats,ActiveAirThreats);
   if(Boss!=null&&bossPhase>=0)phasesSeen[Stage]|=1<<bossPhase;
   if(Boss!=null&&bossTimes[Stage]<0){bossTimes[Stage]=StageTime;report.Add("Boss "+(Stage+1)+" arrival: "+StageTime.ToString("F2")+"s, hull "+Hull+", kills "+Kills);}
   if(Stage*10+CurrentEncounterSection!=recordedBeat){recordedBeat=Stage*10+CurrentEncounterSection;report.Add("Beat "+Stage+"/"+CurrentEncounterSection+": "+StageTime.ToString("F1")+"s / "+EncounterLabel+" / threats "+ActiveAirThreats);}
   if(Stage!=recordedStage){recordedStage=Stage;report.Add("Entered chapter "+(Stage+1)+" at "+CampaignTime.ToString("F1")+"s, hull "+Hull+"/"+MaxHull);Debug.Log("SKYBREAK_CAMPAIGN_STAGE "+(Stage+1));}
   float dt=Mathf.Min(Time.deltaTime,.05f);elapsed+=dt;nextDecision-=dt;
   if(SkillCooldown<=0&&Enemies.Count>0){UsePilotSkill();if(SkillCooldown>0)skillsUsed++;}
   if(Energy>=100&&Overdrive<=0)UseOverdrive();
   if(nextDecision<=0){nextDecision=.1f;float best=float.MaxValue;Vector2 bestMove=Vector2.zero;
    float targetX=0,targetZ=-7;Hostile target=Boss;float targetValue=-999;if(Boss!=null&&BossModuleCount>0){float nearest=float.MaxValue;foreach(var module in Enemies)if(module.owner==Boss){float distance=Mathf.Abs(module.pos.x-PlayerPos.x);if(distance<nearest){nearest=distance;target=module;}}}
    if(target==null)foreach(var e in Enemies){float value=e.kind==11?180:e.kind==4&&e.missionIndex>=0?140:e.kind==9?130:e.elite?100:20-Mathf.Abs(e.pos.x-PlayerPos.x)-Mathf.Abs(e.pos.z-PlayerPos.z)*.2f;if(e.pos.z>PlayerPos.z+2&&value>targetValue){targetValue=value;target=e;}}
    if(target!=null)targetX=target.pos.x;
    foreach(var supply in Supplies)if(supply.pos.z<PlayerPos.z+5&&(supply.kind!=0||Hull<MaxHull)){targetX=supply.pos.x;targetZ=Mathf.Clamp(supply.pos.z,-9,-2);break;}
    for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++){
     Vector2 m=Vector2.ClampMagnitude(new Vector2(x,z),1);Vector3 velocity=new Vector3(m.x,0,m.y)*(Ship==2?13.5f:Ship==1?10:12)*RouteMobility;
     Vector3 pos=PlayerPos+velocity*.3f;float cost=Mathf.Abs(pos.x-targetX)*.1f+Mathf.Abs(pos.z-targetZ)*.13f;
     if(Mathf.Abs(pos.x)>9.6f||pos.z<-9.5f||pos.z>7)cost+=30;
     foreach(var b in Bullets){if(b.friendly)continue;Vector3 delta=b.pos-PlayerPos,relative=b.velocity-velocity;float t=Mathf.Clamp(-Vector3.Dot(delta,relative)/Mathf.Max(.01f,relative.sqrMagnitude),0,.6f);float d=(delta+relative*t).magnitude;
      if(d<1.2f)cost+=(1.2f-d)*10;if(d<.53f)cost+=80;
     }
     foreach(var beam in Beams)if(beam.age>.85f&&Mathf.Abs(pos.x-beam.x)<1.1f)cost+=100;
     foreach(var e in Enemies)if(Vector3.Distance(pos,e.pos)<(e.boss?3.5f:2))cost+=100;
     if(cost<best){best=cost;bestMove=m;}
    }
    control=bestMove;if(control.x!=0) dodges++;
    if(best>40&&invuln<.15f&&Bombs>0){UseBomb();bombsUsed++;}
   }
   Fly(control,false,dt);
  }
  Time.timeScale=1;report.Add("First kill at "+firstKill.ToString("F2")+"s; peak active air threats "+peakThreats);
  bool timingPass=true;for(int stage=0;stage<3;stage++){bool ok=bossTimes[stage]>=chapterBeats[stage][6]&&bossTimes[stage]<chapterBeats[stage][6]+.2f;timingPass&=ok;report.Add((ok?"PASS ":"FAIL ")+"Chapter "+(stage+1)+" natural boss arrival: "+bossTimes[stage].ToString("F2")+"s");}
  report.Add((State==FlightState.Victory?"PASS ":"FAIL ")+"Natural campaign reaches ending: "+State);
  report.Add("Campaign clock: "+CampaignTime.ToString("F1")+"s; wall time: "+(Time.realtimeSinceStartup-start).ToString("F1")+"s");
  report.Add("Score: "+Score+"; hull: "+Hull+"/"+MaxHull+"; received hits: "+campaignDamage+"; kills: "+Kills+"; grazes: "+Grazes+"; rescued signals: "+rescueSignals);
  report.Add("Civilian outcomes: coast "+missionResults[0]+", city "+missionResults[1]+", orbit "+missionResults[2]+"; specialization tier: "+RouteTier);
  report.Add("Skill activations: "+skillsUsed+"; emergency bombs: "+bombsUsed+"; steering decisions: "+dodges);
  ScreenCapture.CaptureScreenshot(Path.Combine(output,"08-natural-campaign.png"));yield return new WaitForSecondsRealtime(.4f);
  WriteVerificationIdentity(output,"campaign");File.WriteAllLines(Path.Combine(output,"campaign-results.txt"),report);foreach(var line in report)Debug.Log(line);Debug.Log("SKYBREAK_CAMPAIGN_QA_COMPLETE");Application.Quit(State==FlightState.Victory&&timingPass?0:2);
 }
}
}
