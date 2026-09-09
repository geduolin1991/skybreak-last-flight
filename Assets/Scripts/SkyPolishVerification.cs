using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
namespace Skybreak {
public partial class SkyGame {
 IEnumerator VerifyPolish17(Action<bool,string> check,string output){
  Ship=0;BeginRun();Launch();ClearBattle();AutoFire=false;spawnClock=999;invuln=999;missionStarted=missionResolved=true;
  check(sphere.vertexCount==77,"Projectile geometry preserves the silhouette with 77 vertices");
  foreach(var c in World.Civilians){Vector3 a=Cam.WorldToViewportPoint(c.go.transform.position),b=Cam.WorldToViewportPoint(World.CivilianAirTarget(c));check(c.go.transform.position.y<-9&&c.go.transform.localScale.x<.5f&&Vector2.Distance(a,b)<.001f,"Distant civilian remains aligned with its projected air interception point");}
  for(int kind=0;kind<12;kind++){
   var e=SpawnEnemy(kind,new Vector3(0,1,5),0);e.missionIndex=-1;var go=e.go;int before=enemyVisualAllocations;DamageEnemy(e,100000);
   check(hitStop==0,"Enemy kind "+kind+" can be destroyed without slowing player movement");
   TickWrecks(2);var recycled=SpawnEnemy(kind,new Vector3(2,1,6),0);check(recycled.go==go&&enemyVisualAllocations==before&&recycled.hp>0,"Enemy kind "+kind+" reuses its clean visual after destruction");ReleaseEnemyVisual(recycled);Enemies.Remove(recycled);
  }
  ClearBattle();StageTime=30;encounterSection=CurrentEncounterSection;eliteSent=true;AutoFire=false;spawnClock=999;invuln=999;missionStarted=missionResolved=true;
  yield return new WaitForSeconds(1.5f);int allocated=shockAllocations;peakKillCostMs=0;renderedSparkPeak=0;var frameMs=new List<float>();
  for(int wave=0;wave<60;wave++){
   for(int j=0;j<3;j++){var e=SpawnEnemy(new[]{0,1,4,5,6,7,8,10}[wave%8],new Vector3(-4+j*4,1,6),0);DamageEnemy(e,100000);}
   for(int frame=0;frame<6;frame++){yield return null;frameMs.Add(Time.unscaledDeltaTime*1000);}
  }
  frameMs.Sort();float sum=0;foreach(float ms in frameMs)sum+=ms;
  File.WriteAllText(Path.Combine(output,"17-performance.json"),"{\"kills\":180,\"frames\":"+frameMs.Count+",\"average_ms\":"+(sum/frameMs.Count).ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+",\"p95_ms\":"+frameMs[(int)(frameMs.Count*.95f)].ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+",\"p99_ms\":"+frameMs[(int)(frameMs.Count*.99f)].ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+",\"max_kill_cpu_ms\":"+peakKillCostMs.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+",\"spark_peak\":"+renderedSparkPeak+",\"new_shock_objects\":"+(shockAllocations-allocated)+"}");
  check(hitStop==0&&renderedSparkPeak<=1000,"180 consecutive kills keep simulation running within the effect budget");
  check(shockAllocations==allocated,"Consecutive destruction reuses prewarmed shockwaves without creating new objects");
  yield return new WaitForSeconds(2);check(shocks.Count==0&&deathClouds.Count==0,"Destruction effects fully return to idle after the stress sequence");
  int oldLanguage=VoiceLanguage;bool oldEnabled=VoiceEnabled,oldCanPlay=voiceCanPlay;VoiceEnabled=true;voiceCanPlay=true;
  ClearBattle();missionStarted=missionResolved=true;
  for(int language=0;language<3;language++){
   SetVoiceLanguage(language);float until=Time.realtimeSinceStartup+25;
   while(VoiceClipsReady<141&&Time.realtimeSinceStartup<until)yield return null;
   bool complete=VoiceClipsReady==141;foreach(var entry in voiceById.Values)complete&=VoiceClip(entry)!=null;
   check(complete,"Language "+language+" preloads all 141 pilot and commander events");
   qaRunning=false;voiceSawState=true;voiceLastState=State;voiceLastRadio=RadioText;ClearVoices();QueueVoiceId("profile_"+language+"_0",100,0,true);
   until=Time.realtimeSinceStartup+4;while(!VoicePlaying&&Time.realtimeSinceStartup<until)yield return null;
   check(VoicePlaying&&voiceCurrent.pilot==language&&voiceAudio.clip==VoiceClip(voiceCurrent),"Language "+language+" plays the selected language's actual pilot recording");
   Pause();float at=VoicePlaybackTime;yield return new WaitForSecondsRealtime(.18f);check(Mathf.Abs(VoicePlaybackTime-at)<.05f,"Language "+language+" dialogue freezes with pause");Pause();ClearVoices();
   QueueVoiceId("commander_"+language+"_entry",100,0,true);until=Time.realtimeSinceStartup+4;while(!VoicePlaying&&Time.realtimeSinceStartup<until)yield return null;
   check(VoicePlaying&&voiceCurrent.pilot==language+3,"Language "+language+" plays a distinct enemy actor without pilot-index errors");
   qaRunning=true;ClearVoices();
  }
  SetVoiceLanguage(0);float readyUntil=Time.realtimeSinceStartup+25;while(VoiceClipsReady<141&&Time.realtimeSinceStartup<readyUntil)yield return null;
  for(int stage=0;stage<3;stage++){
   Stage=stage;BeginStage();Launch();ClearBattle();AutoFire=false;spawnClock=999;invuln=999;missionStarted=missionResolved=true;StageTime=116;bossSpawned=true;SpawnBoss();Boss.pos=new Vector3(0,1,8.2f);stageBanner=warningClock=dialogClock=0;
   check(CommanderEvent=="commander_"+stage+"_entry"&&CommanderExpression==0&&commanderPortraits[stage],"Boss "+stage+" enters with an original portrait and personal challenge");
   yield return new WaitForSeconds(.2f);ScreenCapture.CaptureScreenshot(Path.Combine(output,"17-commander-"+stage+"-entry.png"));yield return new WaitForSeconds(.1f);
   Boss.hp=Boss.maxHp*.6f;TickCommander(.01f);check(CommanderEvent.EndsWith("phase")&&CommanderExpression==1,"Boss "+stage+" reacts to losing its defensive phase");
   Boss.hp=Boss.maxHp*.2f;TickCommander(.01f);check(CommanderEvent.EndsWith("critical"),"Boss "+stage+" changes emotion at critical hull integrity");
   yield return new WaitForSeconds(.2f);ScreenCapture.CaptureScreenshot(Path.Combine(output,"17-commander-"+stage+"-critical.png"));yield return new WaitForSeconds(.1f);
   DamageEnemy(Boss,100000);check(CommanderEvent.EndsWith("death")&&CommanderExpression==2,"Boss "+stage+" delivers its own final reaction exactly at destruction");
   yield return new WaitForSeconds(.2f);ScreenCapture.CaptureScreenshot(Path.Combine(output,"17-commander-"+stage+"-death.png"));yield return new WaitForSeconds(.1f);
   ClearBattle();check(CommanderEvent=="","Leaving combat cancels the defeated commander's portrait and context");
  }
  SetVoiceLanguage(oldLanguage);VoiceEnabled=oldEnabled;voiceCanPlay=oldCanPlay;qaRunning=true;Ship=0;BeginRun();Launch();AutoFire=false;
 }
}
}
