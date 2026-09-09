using UnityEngine;
using System.Collections.Generic;

namespace Skybreak {
public partial class SkyWorld {
 sealed class CampaignJoint {public Transform part;public Quaternion rest;public bool rotor;public int district;}
 sealed class CampaignTrain {public Transform part;public Vector3 start;public float travel;public int district;}
 readonly List<CampaignJoint> campaignJoints=new List<CampaignJoint>();
 readonly List<CampaignTrain> campaignTrains=new List<CampaignTrain>();
 readonly List<Transform> campaignMovingRoots=new List<Transform>();
 Transform tableauRoot;readonly Transform[] tableauPlanes=new Transform[3];
 ParticleSystem rainSystem;float weatherAge,tableauAge,stormAmount=1;
 public float StormAmount=>stormAmount;
 public int CampaignMovingParts=>campaignJoints.Count+campaignTrains.Count;
 public int MovingEvacuationVehicles {get{int count=0;foreach(var c in civilians)if(c.departed&&c.go&&c.go.activeSelf)count++;return count;}}
 void ResetCampaignWorld(){
  campaignJoints.Clear();campaignTrains.Clear();campaignMovingRoots.Clear();rainSystem=null;
  tableauRoot=null;for(int i=0;i<3;i++)tableauPlanes[i]=null;weatherAge=tableauAge=0;stormAmount=1;
 }
 void CollectCampaignJoints(GameObject model,int district){
  foreach(var t in model.GetComponentsInChildren<Transform>())if(t.name.StartsWith("Motion_"))
   campaignJoints.Add(new CampaignJoint{part=t,rest=t.localRotation,rotor=t.name.Contains("Rotor"),district=district});
 }
 bool IsCampaignMoving(Transform part){foreach(var root in campaignMovingRoots)if(root&&(part==root||part.IsChildOf(root)))return true;return false;}
 void BuildCampaignWorld(){
  if(Stage==0)for(int side=-1;side<=1;side+=2){
   var beacon=Art.Model("HarborSignal112",terrain);beacon.transform.localPosition=new Vector3(side*11.5f,-3,11);
   beacon.transform.localScale=Vector3.one*.52f;CollectCampaignJoints(beacon,0);
  }
  if(Stage==2){
   var observatory=Art.Model("OrbitalObservatory112",terrain);observatory.transform.localPosition=new Vector3(-7.5f,-8.2f,18);
   observatory.transform.localScale=Vector3.one*.68f;CollectCampaignJoints(observatory,0);
   for(int side=-1;side<=1;side+=2){
    var sail=Art.Model("SolarSail112",terrain);sail.transform.localPosition=new Vector3(side*10.6f,-7,side<0?-8:15);
    sail.transform.localScale=Vector3.one*.47f;sail.transform.localRotation=Quaternion.Euler(0,side*16,0);CollectCampaignJoints(sail,side<0?1:2);
   }
  }
 }
 void BuildDistrictTransit(Transform sector,int index){
  if(Stage!=1||index%3!=2)return;
  float side=index%2==0?-1:1;
  var rail=Art.Model("ElevatedRail112",sector);rail.transform.localPosition=new Vector3(side*2.9f,-3.07f,0);rail.transform.localScale=Vector3.one*.61f;
  var train=Art.Model("MetroCar112",sector);train.transform.localPosition=new Vector3(side*2.9f,-2.07f,-2.5f);train.transform.localScale=Vector3.one*.50f;
  campaignMovingRoots.Add(train.transform);campaignTrains.Add(new CampaignTrain{part=train.transform,start=train.transform.localPosition,district=(index/3)%3});
 }
 void TickCampaignWorld(float dt){
  weatherAge+=dt;float targetStorm=Stage==1?Mathf.Lerp(1,.18f,PoweredDistricts/3f):0;
  stormAmount=Mathf.MoveTowards(stormAmount,targetStorm,dt*.18f);
  if(rainSystem){var emission=rainSystem.emission;emission.rateOverTime=110+stormAmount*410;}
  if(groundSurface)groundSurface.SetFloat("_Storm",stormAmount);
  if(Stage==1){RenderSettings.fogDensity=Mathf.Lerp(.0018f,.0035f,stormAmount);Cam.backgroundColor=Color.Lerp(new Color(.095f,.17f,.23f),new Color(.035f,.072f,.11f),stormAmount);RenderSettings.fogColor=Cam.backgroundColor;}
  foreach(var joint in campaignJoints){
   if(!joint.part)continue;
   if(joint.rotor)joint.part.localRotation=joint.rest*Quaternion.Euler(0,weatherAge*22,0);
   else {
    float align=gridRestored[joint.district]?0:Mathf.Sin(weatherAge*.16f+joint.district)*18+24;
    var target=joint.rest*Quaternion.Euler(joint.part.name.Contains("Solar")?align:0,0,joint.part.name.Contains("Solar")?0:align);
    joint.part.localRotation=Quaternion.Slerp(joint.part.localRotation,target,1-Mathf.Exp(-dt*.7f));
   }
  }
  foreach(var train in campaignTrains){
   if(!train.part)continue;if(gridRestored[train.district])train.travel+=dt*1.65f;
   train.part.localPosition=train.start+Vector3.forward*Mathf.Repeat(train.travel,5.2f);
  }
 }
 public void PresentCampaignTableau(int stage,int shot,bool aftermath){
  if(Stage!=stage){SetStage(stage);BeginLivingMission();}
  else if(civilians.Count==0&&missionRoot==null)BeginLivingMission();
  Scrolling=false;tableauAge=0;
  if(tableauRoot)Destroy(tableauRoot.gameObject);
  tableauRoot=new GameObject("Campaign flyby / presentation only").transform;tableauRoot.SetParent(terrain,false);
  for(int i=0;i<3;i++){
   var plane=Art.Model(new[]{"Kestrel","Manta","Needle"}[i],tableauRoot);tableauPlanes[i]=plane.transform;
   plane.transform.localScale=Vector3.one*.54f;plane.transform.position=new Vector3((i-1)*3.6f,1,-3.5f+Mathf.Abs(i-1)*2);
   Art.Exhaust(plane.transform,new Vector3(-.55f,0,-1),Art.Cyan,.55f);Art.Exhaust(plane.transform,new Vector3(.55f,0,-1),Art.Cyan,.55f);
  }
  if(aftermath&&Stage==2)SetUplink(1,true);
 }
 public void ShowCampaignAftermath(int[] results,int[] masks,float[] hull){
  if(Stage==0)for(int i=0;i<civilians.Count;i++){var c=civilians[i];c.health=hull[i];c.departed=false;if(c.go)c.go.SetActive(c.health>0);}
  if(Stage==1)for(int i=0;i<3;i++)if((masks[1]&(1<<i))!=0)SetDistrictPower(i);
  if(Stage==2){for(int i=0;i<3;i++)if((masks[2]&(1<<i))!=0)SetDistrictPower(i);SetUplink(results[2]>0?1:0,results[2]>0);}
 }
 public void TickCampaignTableau(float dt){
  tableauAge+=dt;TickGroundLife(dt);TickCampaignWorld(dt);TickLivingMission(dt);UpdateRescueWakes();UpdateChapterAtmosphere(dt);
  for(int i=0;i<3;i++)if(tableauPlanes[i]){
   var plane=tableauPlanes[i];plane.position=new Vector3((i-1)*3.6f+Mathf.Sin(tableauAge*.25f+i)*.25f,1+Mathf.Sin(tableauAge*.65f+i)*.055f,-3.5f+Mathf.Abs(i-1)*2+Mathf.Sin(tableauAge*.14f)*.55f);
   plane.rotation=Quaternion.Euler(2,0,Mathf.Sin(tableauAge*.3f+i)*3);
  }
 }
 public void EndCampaignTableau(){if(tableauRoot)Destroy(tableauRoot.gameObject);tableauRoot=null;for(int i=0;i<3;i++)tableauPlanes[i]=null;}
}
}
