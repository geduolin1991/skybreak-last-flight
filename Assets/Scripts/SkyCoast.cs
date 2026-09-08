using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace Skybreak {
public partial class SkyWorld {
 struct ShoreAnchor {public Transform sector;public Vector4 shape;}
 readonly List<ShoreAnchor> coastAnchors=new List<ShoreAnchor>(20);
 readonly Dictionary<Transform,Vector2> coastTrafficLimits=new Dictionary<Transform,Vector2>();
 readonly Vector4[] visibleShores=new Vector4[8];
 readonly float[] shoreDistances=new float[8];
 static readonly int shoreProperty=Shader.PropertyToID("_Shore");
 public int CoastFootprints {get;private set;}
 public bool OceanReady=>ocean&&ocean.shader.isSupported&&ocean.GetTexture("_WaveMap");
 public Vector4 RescueWake(int index)=>ocean&&index>=0&&index<3?ocean.GetVector(wakeProperties[index]):Vector4.zero;

 void CreateOcean() {
  var water=Art.Primitive(terrain,"Dawn bay · blue water and shoreline foam",new Vector3(0,-3,20),new Vector3(18,1,30),Art.Mat("sea",Color.blue),PrimitiveType.Plane);
  ocean=owned.Keep(new Material(Shader.Find("Skybreak/Ocean")));
  ocean.SetTexture("_WaveMap",Resources.Load<Texture2D>("World/CoastalWaveSpectrum"));
  var renderer=water.GetComponent<Renderer>();renderer.sharedMaterial=ocean;renderer.shadowCastingMode=ShadowCastingMode.Off;
 }
 void Harbor(Transform sector,int index) {
  for(int side=-1;side<=1;side+=2) {
   int pattern=(index+(side==1?2:0))%5;
   bool port=pattern==0||pattern==3;
   float scale=port?.86f:pattern==4?.63f:.92f;
   float x=side*(port?17.5f:17.8f),z=side*(index%2==0?1.3f:-1.3f);
   PlaceScenery(sector,port?"DawnHarbor":"DawnCove",new Vector3(x,-3,z),scale);
   coastAnchors.Add(new ShoreAnchor{sector=sector,shape=new Vector4(x,z,4.15f*scale,(port?5.9f:-5.8f)*scale)});
   if(port) {
    var bus=Art.Model("EvacBus",sector);bus.transform.localPosition=new Vector3(x-1.7f*scale,-2.54f,z);
    bus.transform.localScale=Vector3.one*.26f;traffic.Add(bus.transform);
    coastTrafficLimits[bus.transform]=new Vector2(z-4.5f*scale,z+4.5f*scale);
   }
  }
 }
 void UpdateCoast() {
  if(!ocean)return;
  // Only the nearest four pairs can enter the orthographic view. Fixed
  // buffers keep scrolling allocation-free and cap water shader work.
  float center=Cam.transform.position.z+(Cam.transform.position.y+10)*.57735027f;
  for(int i=0;i<8;i++){shoreDistances[i]=float.MaxValue;visibleShores[i]=new Vector4(10000,10000,1,1);}
  foreach(var anchor in coastAnchors) {
   if(!anchor.sector)continue;Vector4 shape=anchor.shape;shape.y+=anchor.sector.position.z;
   float distance=Mathf.Abs(shape.y-center);
   for(int i=0;i<8;i++)if(distance<shoreDistances[i]) {
    for(int j=7;j>i;j--){shoreDistances[j]=shoreDistances[j-1];visibleShores[j]=visibleShores[j-1];}
    shoreDistances[i]=distance;visibleShores[i]=shape;break;
   }
  }
  ocean.SetVectorArray(shoreProperty,visibleShores);CoastFootprints=coastAnchors.Count;
 }
}
}
