using UnityEngine;
using System.Collections.Generic;
namespace Skybreak {
// Civilian actors belong to the mission, so they cannot wrap with scenery sectors.
public sealed class SkyCivilian {
 public GameObject go; public Vector3 station; public float health=100,age; public int index; public bool departed;
 public LineRenderer wakeLeft,wakeRight; public ParticleSystem smoke;
}
public partial class SkyWorld {
 readonly List<SkyCivilian> civilians=new List<SkyCivilian>();
 readonly List<Transform> traffic=new List<Transform>();
 readonly List<Renderer>[] districtWindows={new List<Renderer>(),new List<Renderer>(),new List<Renderer>()};
 readonly float[] gridPower=new float[3];readonly bool[] gridRestored=new bool[3];
 readonly List<LineRenderer> orbitalLinks=new List<LineRenderer>();
 Material groundSurface;Transform missionRoot,orbitalDish;MaterialPropertyBlock worldBlock=new MaterialPropertyBlock();
 public IReadOnlyList<SkyCivilian> Civilians=>civilians;
 public int PoweredDistricts {get{int n=0;foreach(bool on in gridRestored)if(on)n++;return n;}}
 public bool UplinkActive {get;private set;}public bool UplinkComplete {get;private set;}
 public float UplinkProgress {get;private set;}
 void ResetLivingWorld(){groundSurface=null;civilians.Clear();traffic.Clear();orbitalLinks.Clear();missionRoot=null;orbitalDish=null;UplinkActive=UplinkComplete=false;UplinkProgress=0;for(int i=0;i<3;i++){districtWindows[i].Clear();gridPower[i]=0;gridRestored[i]=false;}}
 public void BeginLivingMission(){
  if(missionRoot)Destroy(missionRoot.gameObject);civilians.Clear();orbitalLinks.Clear();
  missionRoot=new GameObject("Mission · visible civilian operations").transform;missionRoot.SetParent(terrain,false);
  if(Stage==0)for(int i=0;i<3;i++){
   var o=Art.Model("RescueFerry",missionRoot);o.name=new[]{"曙光号 · 312 人","归港号 · 268 人","白鹭号 · 190 人"}[i];o.transform.localScale=Vector3.one*.68f;
   var c=new SkyCivilian{go=o,index=i,station=new Vector3((i-1)*5.6f,-2.68f,i==1?0:3.2f)};o.transform.position=c.station;
   c.wakeLeft=Art.Line(missionRoot,"Port wake",new Vector3[12],new Color(.1f,.29f,.30f,.28f),.075f);c.wakeRight=Art.Line(missionRoot,"Starboard wake",new Vector3[12],new Color(.1f,.29f,.30f,.28f),.075f);c.wakeLeft.useWorldSpace=c.wakeRight.useWorldSpace=true;
   foreach(var wake in new[]{c.wakeLeft,c.wakeRight})if(wake){wake.startColor=new Color(.9f,1,1,.65f);wake.endColor=new Color(.9f,1,1,0);wake.widthCurve=new AnimationCurve(new Keyframe(0,.035f),new Keyframe(.3f,.09f),new Keyframe(1,.025f));}
   civilians.Add(c);
  }
  if(Stage==2){
   var dock=Art.Model("OrbitalDock",missionRoot);dock.transform.position=new Vector3(0,-5.2f,5);dock.transform.localScale=Vector3.one*.72f;
   orbitalDish=new GameObject("Tracking aerial").transform;orbitalDish.SetParent(missionRoot,false);orbitalDish.position=new Vector3(0,-3.6f,10);
   var ring=Art.Ring(orbitalDish,1.8f,new Color(.12f,.42f,.6f),.055f);ring.transform.localRotation=Quaternion.Euler(0,0,38);
   for(int i=0;i<3;i++){var l=Art.Line(missionRoot,"Uplink circuit "+i,new[]{new Vector3((i-1)*6,-2.4f,7),new Vector3(0,-3.2f,10)},new Color(.08f,.15f,.24f),.055f);l.useWorldSpace=true;orbitalLinks.Add(l);}
   for(int i=0;i<2;i++){var ship=Art.Model("CargoShuttle",missionRoot);ship.transform.localScale=Vector3.one*.3f;var c=new SkyCivilian{go=ship,index=i,station=new Vector3(i==0?-5:5,-3.7f,-5)};ship.transform.position=c.station;civilians.Add(c);}
  }
 }
 public void SetDistrictPower(int index){if(index>=0&&index<3)gridRestored[index]=true;}
 public void SetUplink(float progress,bool complete=false){UplinkActive=true;UplinkProgress=Mathf.Clamp01(progress);UplinkComplete=complete;}
 public void DepartCivilians(){foreach(var c in civilians)if(c.health>0)c.departed=true;}
 public void DamageCivilian(int index,float damage){
  if(index<0||index>=civilians.Count)return;var c=civilians[index];if(c.departed||c.health<=0)return;c.health=Mathf.Max(0,c.health-damage);
  if(c.health<=60&&!c.smoke){var go=new GameObject("Damaged engine smoke");go.transform.SetParent(c.go.transform,false);go.transform.localPosition=new Vector3(0,1,-1.2f);c.smoke=go.AddComponent<ParticleSystem>();var main=c.smoke.main;main.startLifetime=2.1f;main.startSpeed=.85f;main.startSize=.4f;main.startColor=new Color(.08f,.09f,.11f,.75f);main.maxParticles=64;var em=c.smoke.emission;em.rateOverTime=12;var sh=c.smoke.shape;sh.shapeType=ParticleSystemShapeType.Cone;sh.angle=12;var renderer=c.smoke.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Art.Mat("Fleet smoke",new Color(.08f,.09f,.11f));}
 }
 public void TickLivingMission(float dt){
  foreach(var c in civilians){if(!c.go)continue;c.age+=dt;Vector3 p=c.station;
   if(c.health<=0){p.y-=Mathf.Min(1.1f,c.age*.008f);c.go.transform.rotation=Quaternion.Euler(0,0,16);}else if(c.departed){c.station+=Vector3.forward*dt*(Stage==0?4.8f:10);p=c.station;}
   else {p.x+=Mathf.Sin(c.age*.32f+c.index)*.14f;p.y+=Mathf.Sin(c.age*1.5f+c.index)*.04f;c.go.transform.rotation=Quaternion.Euler(Mathf.Sin(c.age*.7f)*.65f,Mathf.Sin(c.age*.32f)*1.2f,Mathf.Sin(c.age*.8f)*1.2f);}
   c.go.transform.position=p;
   if(c.wakeLeft){float length=c.health<=0?1.5f:c.departed?7:4;for(int j=0;j<12;j++){float t=j/11f;float spread=.55f+t*.85f;float z=p.z-1.2f-t*length;float ripple=Mathf.Sin(t*19-c.age*4)*.075f*t;c.wakeLeft.SetPosition(j,new Vector3(p.x-spread+ripple,-2.94f,z));c.wakeRight.SetPosition(j,new Vector3(p.x+spread-ripple,-2.94f,z));}}
  }
  if(orbitalDish)orbitalDish.localRotation=Quaternion.Slerp(orbitalDish.localRotation,Quaternion.Euler(0,UplinkActive?0:Mathf.Sin(Time.time*.25f)*45,UplinkActive?0:32),dt*.6f);
  for(int i=0;i<orbitalLinks.Count;i++){float p=UplinkProgress;Color c=gridRestored[i]?new Color(.13f,.66f,.86f):new Color(.12f,.2f,.29f);if(UplinkActive)c*=.7f+p*.7f;orbitalLinks[i].startColor=orbitalLinks[i].endColor=c;orbitalLinks[i].startWidth=orbitalLinks[i].endWidth=UplinkComplete?.11f:.045f;}
 }
 void CreateGroundDetail(Transform t,int index){
  if(Stage==0){if(index%2==0){var b=Art.Model("EvacBus",t);b.transform.localPosition=new Vector3(14.1f,-1.7f,2);b.transform.localScale=Vector3.one*.35f;traffic.Add(b.transform);}return;}
  if(Stage==1){
   for(int side=-1;side<=1;side+=2){var o=Art.Model("CityDistrict",t);o.transform.localPosition=new Vector3(side*6.9f,-3.3f,0);o.transform.localScale=Vector3.one*.57f;
    int district=(index/3)%3;foreach(var r in o.GetComponentsInChildren<MeshRenderer>())if(r.sharedMaterial&&r.sharedMaterial.name.Contains("City_Window"))districtWindows[district].Add(r);
    var car=Art.Model(index%3==0?"EvacBus":"TrafficCar",t);car.transform.localPosition=new Vector3(side*(index%2==0?1.9f:12.1f),-3.04f,side*3);car.transform.localScale=Vector3.one*.28f;car.transform.localRotation=Quaternion.Euler(0,side<0?180:0,0);traffic.Add(car.transform);
   }
   // Fine kerbs, drains and crossing stripes are geometry, not a blurred backdrop.
   var concrete=Art.Mat("Pavement concrete",new Color(.16f,.21f,.24f));var stripe=Art.Mat("Pedestrian ivory",new Color(.44f,.52f,.53f));
   for(int s=-1;s<=1;s+=2)Art.Box(t,"Raised boulevard curb",new Vector3(s*3.6f,-3.1f,0),new Vector3(.16f,.15f,14),concrete);
   for(int n=0;n<8;n++)Art.Box(t,"Zebra crossing",new Vector3(-2.8f+n*.8f,-2.985f,5.5f),new Vector3(.4f,.015f,1.35f),stripe);
  }
  if(Stage==2&&index%2==0){var o=Art.Model("OrbitalDock",t);o.transform.localPosition=new Vector3(index%4==0?-12:12,-7,0);o.transform.localScale=Vector3.one*.45f;}
 }
 void TickGroundLife(float dt){
  if(groundSurface){groundSurface.SetFloat("_Travel",motion);groundSurface.SetVector("_Power",new Vector4(gridPower[0],gridPower[1],gridPower[2],0));}
  for(int i=0;i<3;i++){gridPower[i]=Mathf.MoveTowards(gridPower[i],gridRestored[i]?1:0,dt*.35f);worldBlock.Clear();worldBlock.SetColor("_EmissionColor",new Color(.95f,.59f,.21f)*gridPower[i]*1.1f);worldBlock.SetColor("_Color",Color.Lerp(new Color(.028f,.075f,.10f),new Color(.68f,.42f,.14f),gridPower[i]));foreach(var renderer in districtWindows[i])if(renderer)renderer.SetPropertyBlock(worldBlock);}
  for(int i=0;i<traffic.Count;i++){var t=traffic[i];if(!t)continue;float speed=Stage==0?1.2f:gridRestored[(i/6)%3]?2.4f:.2f;t.localPosition+=Vector3.forward*dt*speed*(t.localRotation.eulerAngles.y>90?-1:1);if(t.localPosition.z>7)t.localPosition+=Vector3.back*14;if(t.localPosition.z<-7)t.localPosition+=Vector3.forward*14;}
 }
}
}
