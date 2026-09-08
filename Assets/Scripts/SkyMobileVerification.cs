using System;using System.Collections;using UnityEngine;
namespace Skybreak {
public partial class SkyGame {
 IEnumerator VerifyMobile18(Action<bool,string> check){
  bool oldAuto=AutoFire;int oldShip=Ship;WebMobileMode("1");check(MobileMode&&Post.MobileQuality,"Touch mode selects lightweight rendering");
  SetupHangar();WebMobileAction("ship:2");WebMobileAction("route:1");check(Ship==2&&CurrentRoute==1,"Touch hangar selects airframe and specialization");
  WebMobileAction("route:invalid");check(CurrentRoute==1,"Invalid touch option cannot change selection");
  WebMobileAction("chapter:9");check(State==FlightState.Hangar,"Touch training rejects unavailable chapter");
  WebMobileAction("play");check(State==FlightState.Briefing,"Touch deploy opens briefing");WebMobileAction("launch");check(State==FlightState.Playing,"Touch confirmation launches actual flight");
  spawnClock=999;invuln=999;WebMobileMove("{\"x\":5,\"y\":5}");check(MobileMovement.magnitude<=1.001f&&MobileMovement.x>0,"Touch movement clamps diagonal speed");
  Vector3 before=PlayerPos;Fly(MobileMovement,false,.1f);check(PlayerPos.x>before.x&&PlayerPos.z>before.z,"Touch vector moves real aircraft on both axes");
  yield return new WaitForSecondsRealtime(.4f);check(MobileMovement==Vector2.zero,"Lost touch transport expires without stuck movement");
  WebMobileMove("{\"x\":-1,\"y\":0}");WebMobileAction("release");check(MobileMovement==Vector2.zero,"Finger release clears movement immediately");
  WebMobileAction("focus");check(mobileFocus,"Touch precision toggle enables slow flight");int w=WeaponMode;WebMobileAction("weapon");check(WeaponMode!=w,"Touch weapon control changes actual fire mode");
  SkillCooldown=0;WebMobileAction("skill");check(SkillCooldown>0,"Touch skill executes specialization ability");
  Bombs=2;WebMobileAction("bomb");check(Bombs==1&&NovaActive,"Touch bomb triggers real nova and consumes one charge");
  Energy=100;Overdrive=0;WebMobileAction("overdrive");check(Overdrive>0&&Energy==0,"Touch overdrive obeys energy consumption");
  WebMobileMove("{\"x\":1,\"y\":0}");WebMobileAction("pause");check(State==FlightState.Paused&&MobileMovement==Vector2.zero,"Touch pause freezes flight and cancels steering");
  int pausedBombs=Bombs;WebMobileAction("bomb");check(Bombs==pausedBombs,"Paused touch ability cannot consume ammunition");
  WebMobileAction("resume");check(State==FlightState.Playing&&MobileMovement==Vector2.zero,"Resume never restores stale touch movement");
  WebMobileAction("pause");WebMobileAction("home");check(State==FlightState.Hangar&&Enemies.Count==0,"Touch return to hangar clears battle");
  var info=MobileStatus();check(info.routes.Length==3&&info.research.Length==3&&info.pilot==PilotNames[Ship],"Touch menus expose current pilot, routes and research");
  WebMobileMode("0");Ship=oldShip;selectedRoutes[2]=0;AutoFire=oldAuto;check(!MobileMode&&!Post.MobileQuality,"Desktop rendering remains available after touch mode");SetupHangar();
 }
}
}
