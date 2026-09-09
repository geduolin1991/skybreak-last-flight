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
 struct FramedCoast {public Transform part;public float origin;public int side;}
 readonly List<FramedCoast> framedCoast=new List<FramedCoast>(20);
 struct CoastMesh {public Mesh mesh;public Vector3[] original,framed;}
 readonly List<CoastMesh> coastMeshes=new List<CoastMesh>(120);
 float coastInset=-1;
 public float CoastInset=>Mathf.Max(0,coastInset);
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
    TrackCoast(bus.transform,side);
    coastTrafficLimits[bus.transform]=new Vector2(z-4.5f*scale,z+4.5f*scale);
   }
  }
 }
 void TrackCoast(Transform part,int side){framedCoast.Add(new FramedCoast{part=part,origin=part.localPosition.x,side=side});}
 void TrackCoastMesh(Mesh mesh){var points=mesh.vertices;coastMeshes.Add(new CoastMesh{mesh=mesh,original=points,framed=new Vector3[points.Length]});}
 bool FrameCoast(){
  if(Stage!=0)return false;
  float next=Mathf.Clamp(18-Cam.orthographicSize*Cam.aspect,0,5);
  if(Mathf.Abs(next-coastInset)<.005f)return false;coastInset=next;
  // Translate whole banks, including their vehicles. Geometry and the flight
  // corridor retain their scale; rotation never reconstructs the level.
  foreach(var item in framedCoast)if(item.part){var p=item.part.localPosition;p.x=item.origin-item.side*next;item.part.localPosition=p;}
  // Coast banks consist of disconnected islands, wholly on either side of
  // x=0. Shift their cached vertices only on viewport changes, preserving each
  // triangle, the original normals and a single draw per shared material.
  foreach(var item in coastMeshes)if(item.mesh){for(int i=0;i<item.original.Length;i++){var p=item.original[i];p.x-=Mathf.Sign(p.x)*next;item.framed[i]=p;}item.mesh.SetVertices(item.framed);item.mesh.RecalculateBounds();}
  return true;
 }
 void UpdateCoast() {
  if(!ocean)return;
  // Only the nearest four pairs can enter the orthographic view. Fixed
  // buffers keep scrolling allocation-free and cap water shader work.
  float center=Cam.transform.position.z+(Cam.transform.position.y+10)*.57735027f;
  for(int i=0;i<8;i++){shoreDistances[i]=float.MaxValue;visibleShores[i]=new Vector4(10000,10000,1,1);}
  foreach(var anchor in coastAnchors) {
   if(!anchor.sector)continue;Vector4 shape=anchor.shape;shape.x-=Mathf.Sign(shape.x)*CoastInset;shape.y+=anchor.sector.position.z;
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
