using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Skybreak {
public partial class SkyGame {
 IEnumerator VerifyScenery121(Action<bool,string> check,string output){
  for(int stage=0;stage<3;stage++){
   Stage=stage;BeginStage();Launch();ClearBattle();AutoFire=false;spawnClock=999;invuln=999;missionStarted=missionResolved=true;
   World.Scrolling=false;
   yield return null; // Sector batching retires its original renderers at frame end.
   World.VerifySceneryMotion(check);
   for(int i=0;i<3;i++)World.SetDistrictPower(i);
   yield return new WaitForSeconds(.1f);
   ScreenCapture.CaptureScreenshot(Path.Combine(output,"121-scenery-"+stage+".png"));
   yield return new WaitForSeconds(.1f);
  }
  Ship=0;BeginRun();Launch();AutoFire=false;
 }
}
public partial class SkyWorld {
 public void VerifySceneryMotion(Action<bool,string> check){
  var fixtures=new List<Transform>();
  foreach(var part in terrain.GetComponentsInChildren<Transform>())
   if(part.name=="HarborSignal112"||part.name=="OrbitalObservatory112"||part.name=="SolarSail112")fixtures.Add(part);
  bool attached=true;foreach(var part in fixtures)attached&=chunks.Contains(part.parent);
  check(attached,"Stage "+Stage+" landmarks belong to the scrolling map, with no free-standing scenery at the world root");
  check(Stage==1||fixtures.Count==4,"Stage "+Stage+" retains its authored campaign landmarks");
  bool movingGeometry=true;foreach(var joint in campaignJoints){int count=0;foreach(var r in joint.part.GetComponentsInChildren<Renderer>())if(r.enabled)count++;movingGeometry&=count>0;}
  check(movingGeometry,"Stage "+Stage+" moving lamps, dishes and panels survive static geometry batching");

  float oldAspect=Cam.aspect,oldSize=Cam.orthographicSize;
  foreach(float aspect in new[]{.636f,1.777778f,2.1641f}){
   Cam.aspect=aspect;Cam.orthographicSize=SkyGame.MobileCameraSize(aspect);FrameCoast();UpdateCoast();
   bool supported=true;
   if(Stage==0)foreach(var part in fixtures){
    bool onQuay=false;foreach(var shore in coastAnchors){
     if(shore.sector!=part.parent||shore.shape.w<0)continue;
     var p=part.localPosition;p.x+=Mathf.Sign(shore.shape.x)*CoastInset;
     float radius=part.localScale.x*1.65f;
     onQuay|=Mathf.Abs(p.x-shore.shape.x)+radius<shore.shape.z&&Mathf.Abs(p.z-shore.shape.y)+radius<shore.shape.w;
    }
    supported&=onQuay;
   }
   check(supported,"Stage "+Stage+" / aspect "+aspect+" beacon foundations remain on their own quay after reframing");
   var planes=GeometryUtility.CalculateFrustumPlanes(Cam);
   bool rigid=true,continuous=true,hiddenWrap=true;int wraps=0;
   for(int frame=0;frame<1200;frame++){
    var before=ReadSceneryAnchors();var sectorBefore=new float[chunks.Count];
    for(int i=0;i<chunks.Count;i++){
     sectorBefore[i]=chunks[i].position.z;
     if(sectorBefore[i]-.05f*Speed<-45)foreach(var r in chunks[i].GetComponentsInChildren<Renderer>())
      if(r.enabled&&GeometryUtility.TestPlanesAABB(planes,r.bounds))hiddenWrap=false;
    }
    AdvanceScenery(.05f);
    var after=ReadSceneryAnchors();
    for(int i=0;i<before.Length;i++){
     var a=before[i];var b=after[i];bool recycled=b.sectorZ>a.sectorZ+1;
     Vector3 relative=b.position-a.position-Vector3.forward*(b.sectorZ-a.sectorZ);
     if(!a.moving)rigid&=relative.sqrMagnitude<.000001f;
     else if(!recycled)continuous&=relative.magnitude<=.121f;
    }
    for(int i=0;i<chunks.Count;i++)if(chunks[i].position.z>sectorBefore[i]+1)wraps++;
   }
   check(rigid,"Stage "+Stage+" / aspect "+aspect+" all rigid props match their sector motion for 60 simulated seconds");
   check(continuous,"Stage "+Stage+" / aspect "+aspect+" cars and trains never teleport within a visible sector");
   check(wraps>=30&&hiddenWrap,"Stage "+Stage+" / aspect "+aspect+" repeated map recycling occurs entirely outside the camera");
  }
  Cam.aspect=oldAspect;Cam.orthographicSize=oldSize;FrameCoast();UpdateCoast();
  if(Stage==1){
   var stations=new Vector3[civilians.Count];for(int i=0;i<stations.Length;i++)stations[i]=civilians[i].station;
   for(int i=0;i<120;i++)TickLivingMission(.05f);
   bool grounded=true;for(int i=0;i<stations.Length;i++)grounded&=(civilians[i].go.transform.position-stations[i]).sqrMagnitude<.000001f&&Quaternion.Angle(civilians[i].go.transform.rotation,Quaternion.identity)<.001f;
   check(grounded,"Road vehicles do not inherit floating-ship sway, bob or roll");
  }
  var paused=ReadSceneryAnchors();Update();var pausedAfter=ReadSceneryAnchors();bool still=true;
  for(int i=0;i<paused.Length;i++)still&=paused[i].position==pausedAfter[i].position;
  check(still,"Stage "+Stage+" paused scenery does not advance");
 }
}
}
