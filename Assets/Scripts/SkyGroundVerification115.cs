using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Skybreak {
public partial class SkyGame {
 IEnumerator VerifyGround115(){
  qaRunning=true;Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=60;AutoFire=false;Difficulty=0;string folder=Path.Combine(Directory.GetCurrentDirectory(),"Build/Ground115/QA");foreach(string arg in Environment.GetCommandLineArgs())if(arg.StartsWith("--sky-output="))folder=arg.Substring(13);Directory.CreateDirectory(folder);var results=new List<string>();Action<bool,string> check=(ok,name)=>{results.Add((ok?"PASS ":"FAIL ")+name);Debug.Log(results[results.Count-1]);};
  yield return new WaitForSecondsRealtime(.8f);
  for(int ship=0;ship<3;ship++){Ship=ship;BeginGround115();check(GroundActive&&State==FlightState.Briefing&&GroundArea==0,"Ship "+ship+" enters independent ground briefing");qaRunning=false;UpdateVoices(.016f);qaRunning=true;check(RadioText==GroundBrief&&voiceQueue.Count==0,"Ship "+ship+" ground briefing cannot be replaced by air-campaign dialogue");Launch();invuln=999;PlayerPos=new Vector3(0,1,-3.5f);float landingDeadline=Time.realtimeSinceStartup+3;while(groundLanding>0&&Time.realtimeSinceStartup<landingDeadline)yield return null;check(groundMech.gameObject.activeSelf&&!groundTank.gameObject.activeSelf,"Ship "+ship+" starts in ground mecha form");ScreenCapture.CaptureScreenshot(Path.Combine(folder,"ground-mech-"+ship+".png"));yield return new WaitForSeconds(.15f);
   SwitchGroundForm115();yield return new WaitForSeconds(.5f);check(GroundTank&&groundTank.gameObject.activeSelf&&!groundMech.gameObject.activeSelf,"Ship "+ship+" transforms into its authored tank");check(groundMech.GetComponentsInChildren<Renderer>(true).Where(r=>r is MeshRenderer).Min(r=>r.bounds.min.y)>-.15f,"Ship "+ship+" ground mecha feet remain above the floor");check(groundTurret!=null,"Ship "+ship+" has an independently aimed turret");ScreenCapture.CaptureScreenshot(Path.Combine(folder,"ground-tank-"+ship+".png"));yield return new WaitForSeconds(.15f);
   float previous=groundUtility;GroundUtility115();check(groundGuardHits==6&&groundUtility>7,"Tank shield has six finite interceptions and cooldown");invuln=0;groundAim=Vector3.forward;int hull=Hull;for(int i=0;i<6;i++)GroundHurt115(PlayerPos+Vector3.forward*2);check(Hull==hull&&groundGuardHits==0,"Six frontal hits exhaust ground shield without hull loss");GroundHurt115(PlayerPos+Vector3.forward*2);check(Hull==hull-1,"Seventh frontal hit reaches tank hull");invuln=999;
  }
  Ship=0;BeginGround115();Launch();AutoFire=false;invuln=999;groundLanding=0;yield return new WaitForSeconds(.15f);
  var cover=groundCovers[0];PlayerPos=new Vector3(cover.p.x,1,cover.p.z-1.10f);float beforeZ=PlayerPos.z;GroundMove115(Vector2.up,false,.12f);check(PlayerPos.z==beforeZ,"Low cover blocks normal mech movement");groundVelocity=Vector2.up;GroundUtility115();GroundMove115(Vector2.up,false,.13f);check(PlayerPos.z>beforeZ+1,"Mech propulsion crosses low cover");groundJump=0;groundUtility=0;GroundUtility115();float cd=groundUtility;GroundUtility115();check(groundUtility==cd,"Jump cannot be repeatedly activated during cooldown");groundJump=0;
  float hp=cover.hp;Bullets.Clear();AddBullet(new Vector3(cover.p.x,1,cover.p.z-3),Vector3.forward*35,true,0,40);TickGroundRounds115(.13f);check(Bullets.Count==0&&cover.hp<hp,"Swept friendly projectile hits and damages visible cover");
  hp=cover.hp;Bullets.Clear();AddBullet(new Vector3(cover.p.x,1,cover.p.z-3),Vector3.forward*35,false,1);TickGroundRounds115(.13f);check(Bullets.Count==0&&cover.hp<hp,"Enemy projectile is stopped by the same cover");
  groundJump=.01f;PlayerPos=new Vector3(cover.p.x,1,cover.p.z);TickGround115(.02f,false);check(!GroundMoveBlocked115(PlayerPos,false),"Jump landing resolves out of a barricade instead of trapping the chassis");
  check(GroundActorBlocked115(groundUnits[0].p),"Ground enemies and node bases have solid player footprints");
  DamageCover115(cover,1000);check(cover.hp<=0&&!GroundMoveBlocked115(cover.p,false),"Destroyed barricade opens its former path");
  PlayerPos=new Vector3(0,1,-8);float clock=groundClock;Pause();yield return new WaitForSecondsRealtime(.25f);check(Mathf.Abs(groundClock-clock)<.001f,"Ground pause freezes AI, shots and cooldowns");Pause();
  groundRescueDone=false;PlayerPos=new Vector3(-8,1,-1);for(int i=0;i<51;i++)TickGround115(.05f,false);check(GroundRescued==1&&groundRescueDone,"Maintained shelter connection rescues people and updates mission state");
  GroundArea=1;BeginGroundArea115();Launch();invuln=999;var boss=GroundBoss;float shielded=GroundDamageScale115(boss);foreach(var e in groundUnits.ToArray())if(e.kind==5)DamageGround115(e,99999);boss.fire=4;boss.tell=0;float open=GroundDamageScale115(boss);boss.fire=1;float closed=GroundDamageScale115(boss);check(shielded<.1f&&open>closed&&GroundNodeCount==0,"Boss nodes protect the core; destroying them exposes a real post-volley weakness");
  ScreenCapture.CaptureScreenshot(Path.Combine(folder,"ground-boss.png"));yield return new WaitForSeconds(.2f);
  for(int area=0;area<4;area++){GroundArea=area;BeginGroundArea115();Launch();invuln=999;groundLanding=0;groundReinforced=true;foreach(var e in groundUnits.ToArray())if(e.kind!=4)DamageGround115(e,99999);if(GroundBoss!=null)DamageGround115(GroundBoss,999999);TickGround115(.01f,false);check(groundGateOpen,"Area "+area+" opens the exit only after combat objectives are cleared");PlayerPos=new Vector3(0,1,10);for(int i=0;i<31&&State==FlightState.Playing;i++)TickGround115(.05f,false);check(area==3?State==FlightState.Victory:area==1?State==FlightState.Upgrade:State==FlightState.Briefing&&GroundArea==area+1,"Area "+area+" reaches its intended continuation");if(area==1)GroundUpgrade115(0);}
  check(GroundOutcome.Length>40,"Ground campaign has an outcome based on rescued shelters");ScreenCapture.CaptureScreenshot(Path.Combine(folder,"ground-ending.png"));yield return new WaitForSeconds(.2f);SetupHangar();check(!GroundActive&&groundUnits.Count==0&&!groundRoot,"Hangar return removes ground entities");
  // Runtime camera evidence for the requested orbital background.
  BeginRun();Stage=2;BeginStage();Launch();AutoFire=false;ClearBattle();spawnClock=999;missionStarted=missionResolved=true;invuln=999;stageBanner=dialogClock=toastClock=0;World.Scrolling=false;
  yield return new WaitForSeconds(.6f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"orbit-depth.png"));yield return new WaitForSeconds(.2f);check(World.GetComponentsInChildren<Transform>().Any(t=>t.name=="Distant planet / softened cloud systems"),"Orbital stage uses the softer distant planet and atmosphere");
  SpawnBoss();Boss.pos=new Vector3(0,1,8.2f);Boss.go.transform.position=Boss.pos;yield return new WaitForSeconds(.3f);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"orbit-boss-depth.png"));yield return new WaitForSeconds(.2f);
  WriteVerificationIdentity(folder,"ground");File.WriteAllLines(Path.Combine(folder,"ground-results.txt"),results);Debug.Log("SKYBREAK_GROUND_QA_COMPLETE");Application.Quit(results.Exists(x=>x.StartsWith("FAIL"))?2:0);
 }
}
}
