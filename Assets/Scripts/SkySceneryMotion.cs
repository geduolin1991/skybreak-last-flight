using UnityEngine;
using System.Collections.Generic;

namespace Skybreak {
public partial class SkyWorld {
 [System.Serializable] public class SceneryAnchor {
  public string name,sector;public Vector3 position,local;public float sectorZ;public bool moving;public int liveParts;
 }
 public float SceneryTravel=>motion;
 // Read-only scene evidence, also available to the public-build browser checks.
 public SceneryAnchor[] ReadSceneryAnchors(){
  var result=new List<SceneryAnchor>();
  foreach(var sector in chunks)foreach(Transform part in sector){
   bool vehicle=traffic.Contains(part)||campaignMovingRoots.Contains(part);
   if(!vehicle&&part.name!="HarborSignal112"&&part.name!="OrbitalObservatory112"&&part.name!="SolarSail112")continue;
   int live=0;foreach(var r in part.GetComponentsInChildren<Renderer>())if(r.enabled)live++;
   result.Add(new SceneryAnchor{name=part.name,sector=sector.name,position=part.position,local=part.localPosition,sectorZ=sector.position.z,moving=vehicle,liveParts=live});
  }
  return result.ToArray();
 }
}
}
