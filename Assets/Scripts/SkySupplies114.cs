using UnityEngine;
namespace Skybreak {public partial class SkyGame {
 static readonly string[] SupplyModels={"RepairPod114","OrdnanceRack114","ReactorCell114","WingBeacon114"};
 static readonly string[] SupplyNames={"装甲修复舱","裂空弹药架","反应堆电池","双机支援信标"};
 public void SpawnSupply(Vector3 p,int kind){
  kind=Mathf.Clamp(kind,0,3);var go=Art.Model(SupplyModels[kind],null);go.name=SupplyNames[kind];go.transform.position=p;go.transform.localScale=Vector3.one*1.15f;
  var color=kind==0?new Color(.10f,.92f,.48f):kind==1?Art.Orange:kind==3?new Color(1,.74f,.15f):Art.Cyan;
  var halo=Art.Ring(go.transform,.79f,color,.035f,32);halo.transform.localPosition=Vector3.down*.18f;
  Supplies.Add(new Supply{go=go,pos=p,kind=kind,halo=halo});
 }
 void UpdateSupplies(float dt){for(int i=Supplies.Count-1;i>=0;i--){var p=Supplies[i];p.age+=dt;Vector3 to=PlayerPos-p.pos;p.pos+=to.sqrMagnitude<16?to.normalized*dt*9:Vector3.back*dt*3;
   p.go.transform.position=p.pos+Vector3.up*(Mathf.Sin(p.age*2.8f)*.08f);p.go.transform.rotation=Quaternion.Euler(-12,Mathf.Sin(p.age*.85f)*13,Mathf.Sin(p.age*1.2f)*4);
   if(p.halo)p.halo.transform.localScale=Vector3.one*(1+Mathf.Sin(p.age*3.5f)*.035f);
   if(to.sqrMagnitude<1.1f){if(p.kind==3)CallSquadron();else if(p.kind==0)Hull=Mathf.Min(MaxHull,Hull+1);else if(p.kind==1)Bombs=Mathf.Min(5,Bombs+1);else Energy=Mathf.Min(100,Energy+25);Score+=500;Sound("Pickup");Burst(p.pos,18,0);if(p.kind!=3)Toast(p.kind==0?"装甲修复 +1":p.kind==1?"裂空炸弹 +1":"反应堆能量 +25",1.45f);Destroy(p.go);Supplies.RemoveAt(i);}else if(p.pos.z<-15){Destroy(p.go);Supplies.RemoveAt(i);}
  }}
}}
