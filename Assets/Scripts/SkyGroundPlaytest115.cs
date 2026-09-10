using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
namespace Skybreak {
public partial class SkyGame {
 Vector3 GroundPathStep115(Vector3 target){
  // Test controller navigation follows the actual cover footprints. A greedy
  // direct-line bot can otherwise mistake a necessary detour for a game lock.
  const int n=27;const float unit=.75f,origin=-9.75f;var distance=new int[n*n];var blocked=new bool[n*n];var queue=new int[n*n];
  int startX=Mathf.Clamp(Mathf.RoundToInt((PlayerPos.x-origin)/unit),0,n-1),startY=Mathf.Clamp(Mathf.RoundToInt((PlayerPos.z-origin)/unit),0,n-1),goal=0;float nearest=float.MaxValue;
  for(int y=0;y<n;y++)for(int x=0;x<n;x++){int i=y*n+x;distance[i]=-1;Vector3 p=new Vector3(origin+x*unit,1,origin+y*unit);blocked[i]=GroundMoveBlocked115(p,false)||GroundActorBlocked115(p);float d=(p-target).sqrMagnitude;if(!blocked[i]&&d<nearest){nearest=d;goal=i;}}
  blocked[startY*n+startX]=false;int head=0,tail=0;queue[tail++]=goal;distance[goal]=0;int[] dx={1,0,-1,0},dy={0,1,0,-1};
  while(head<tail){int i=queue[head++],x=i%n,y=i/n;for(int k=0;k<4;k++){int xx=x+dx[k],yy=y+dy[k];if(xx<0||xx>=n||yy<0||yy>=n)continue;int j=yy*n+xx;if(blocked[j]||distance[j]>=0)continue;distance[j]=distance[i]+1;queue[tail++]=j;}}
  int current=startY*n+startX;if(current==goal)return target;int best=current,bestD=distance[current]<0?9999:distance[current];
  for(int k=0;k<4;k++){int x=startX+dx[k],y=startY+dy[k];if(x<0||x>=n||y<0||y>=n)continue;int j=y*n+x;if(distance[j]>=0&&distance[j]<bestD){best=j;bestD=distance[j];}}
  return new Vector3(origin+(best%n)*unit,1,origin+(best/n)*unit);
 }
 IEnumerator GroundPlaytest115(){
  qaRunning=true;AutoFire=true;Application.runInBackground=true;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;Difficulty=0;
  string folder=Path.Combine(Directory.GetCurrentDirectory(),"Build/Ground115/Playtest");foreach(string arg in Environment.GetCommandLineArgs())if(arg.StartsWith("--sky-output="))folder=arg.Substring(13);Directory.CreateDirectory(folder);
  var report=new List<string>();int testedShip=0;foreach(string arg in Environment.GetCommandLineArgs())if(arg.StartsWith("--sky-ship="))int.TryParse(arg.Substring(11),out testedShip);Ship=Mathf.Clamp(testedShip,0,2);BeginGround115();Launch();
  float start=Time.realtimeSinceStartup,decision=0,stuck=0,formClock=0;int lastArea=-1,decisions=0;Vector2 control=Vector2.zero;Vector3 last=PlayerPos;float worst=0,total=0;int frames=0;var frameSamples=new List<float>();
  while(Time.realtimeSinceStartup-start<320&&State!=FlightState.Victory&&State!=FlightState.Defeat){
   if(State==FlightState.Briefing)Launch();if(State==FlightState.Upgrade)GroundUpgrade115(Hull<MaxHull-1?1:0);
   if(State==FlightState.Playing){
    float dt=Mathf.Min(Time.deltaTime,.05f);decision-=dt;formClock+=dt;
    if(lastArea!=GroundArea){lastArea=GroundArea;report.Add("AREA "+GroundArea+" at "+CampaignTime.ToString("F1")+"s, hull "+Hull);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"area-"+GroundArea+".png"));}
    if(decision<=0){decision=.15f;decisions++;Vector3 target;
     if(GroundArea%2==0&&!groundRescueDone)target=new Vector3(-8,1,-1);
     else if(groundGateOpen)target=new Vector3(0,1,10);
     else {var enemy=GroundTarget115();target=enemy==null?new Vector3(0,1,0):enemy.p;Vector3 away=PlayerPos-target;target+=away.normalized*5.2f;}
     target=GroundPathStep115(target);float best=float.NegativeInfinity;control=Vector2.zero;
     for(int i=0;i<17;i++){
      float a=i*Mathf.PI/8;Vector2 v=i==16?Vector2.zero:new Vector2(Mathf.Sin(a),Mathf.Cos(a));Vector3 proposed=PlayerPos+new Vector3(v.x,0,v.y)*.65f;if(Mathf.Abs(proposed.x)>9.5f||proposed.z<-9.8f||proposed.z>10.3f||GroundMoveBlocked115(proposed,false))continue;
      float score=-Vector3.Distance(proposed,target);foreach(var mark in groundMarks)if(mark.hostile&&mark.age<mark.duration&&Vector3.Distance(proposed,mark.p)<mark.radius+1)score-=12;
      foreach(var b in Bullets)if(!b.friendly&&Vector3.Distance(proposed,b.pos+b.velocity*.3f)<1.1f)score-=8;
      if(score>best){best=score;control=v;}
     }
     if(Vector3.Distance(last,PlayerPos)<.09f&&!groundGateOpen)stuck+=.15f;else stuck=0;last=PlayerPos;
     if(stuck>.8f&&!GroundTank&&groundUtility<=0){groundVelocity=control.sqrMagnitude>.1f?control:Vector2.up;GroundUtility115();stuck=0;}
     if(groundSkill<=0&&groundUnits.Count>0)GroundSkill115();
     if(GroundTank&&groundUtility<=0&&Bullets.Exists(b=>!b.friendly&&(b.pos-PlayerPos).sqrMagnitude<20))GroundUtility115();
     if(groundUnits.Count>=5&&Bombs>0&&GroundArea%2==1)GroundBomb115();
     if(formClock>12&&groundSwitch<=0){SwitchGroundForm115();formClock=0;}
    }
    GroundMove115(control,false,dt);worst=Mathf.Max(worst,Time.unscaledDeltaTime*1000);total+=Time.unscaledDeltaTime*1000;frames++;frameSamples.Add(Time.unscaledDeltaTime*1000);
   }
   yield return null;
  }
  report.Add((State==FlightState.Victory?"PASS ":"FAIL ")+"Ground campaign completes with normal weapons, cooldowns, collision and finite player hull: "+State);
  report.Add("SHIP "+Ship+" KILLS "+Kills+" HULL "+Hull+" RESCUED "+GroundRescued+" SECONDS "+CampaignTime.ToString("F2")+" DECISIONS "+decisions+" TRANSFORMS "+groundTransforms);
  frameSamples.Sort();report.Add("PERF mean="+(total/Mathf.Max(1,frames)).ToString("F2")+"ms p95="+(frames>0?frameSamples[(int)((frames-1)*.95f)]:0).ToString("F2")+"ms max="+worst.ToString("F2")+"ms frames="+frames);
  ScreenCapture.CaptureScreenshot(Path.Combine(folder,"result.png"));yield return new WaitForSecondsRealtime(.2f);WriteVerificationIdentity(folder,"ground-playtest");File.WriteAllLines(Path.Combine(folder,"playtest.txt"),report);foreach(var line in report)Debug.Log(line);Debug.Log("GROUND_PLAYTEST_115_COMPLETE");Application.Quit(State==FlightState.Victory?0:2);
 }
}
}
