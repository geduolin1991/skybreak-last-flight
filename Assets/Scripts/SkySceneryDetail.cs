using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace Skybreak {
public partial class SkyWorld {
 public int SectorRenderersBefore { get; private set; }
 public int SectorRenderersAfter { get; private set; }
 Material mist;
 readonly Dictionary<Material,Material> landmarkMaterials=new Dictionary<Material,Material>();
 float missionProgress;
 bool signalRecovered;

 void AddChapterLandmark(Transform sector,int index) {
  string model=null;Vector3 position=Vector3.zero;float size=1;
  if(Stage==2&&index%5==2){model="OrbitalGate";position=new Vector3(0,-9,0);size=1.25f;}
  if(model==null)return;
  var landmark=Art.Model(model,sector);landmark.transform.localPosition=position;
  landmark.transform.localScale=Vector3.one*size;
  // The ring passes behind the boss: keep its silhouette in the background.
  // Share the variants across sectors without modifying aircraft materials.
  if(Stage==2)foreach(var renderer in landmark.GetComponentsInChildren<MeshRenderer>()) {
   var materials=renderer.sharedMaterials;
   for(int i=0;i<materials.Length;i++) {
    var source=materials[i];if(!source)continue;
    if(!landmarkMaterials.TryGetValue(source,out var distant)) {
     distant=owned.Keep(new Material(source){name="Distant / "+source.name});
     Color color=source.color;distant.color=new Color(color.r*.32f,color.g*.32f,color.b*.36f,color.a);
     distant.SetColor("_EmissionColor",source.GetColor("_EmissionColor")*.12f);
     distant.SetFloat("_Glossiness",.22f);distant.SetFloat("_Metallic",.35f);
     landmarkMaterials.Add(source,distant);
    }
    materials[i]=distant;
   }
   renderer.sharedMaterials=materials;
  }
 }

 // A sector moves as one unit. Consolidate its opaque geometry while leaving
 // particle systems, lines and their children intact. Materials stay shared.
 void CombineSector(Transform sector) {
  var groups=new Dictionary<Material,List<CombineInstance>>();
  var sources=new List<MeshRenderer>();
  foreach(var filter in sector.GetComponentsInChildren<MeshFilter>()) {
   // Vehicles keep their transforms so restored power can move them. All
   // static district geometry, pavement and crossings share the sector batch.
   bool moving=false;foreach(var car in traffic)if(car&&(filter.transform==car||filter.transform.IsChildOf(car))){moving=true;break;}
   if(moving)continue;
   var renderer=filter.GetComponent<MeshRenderer>();var mesh=filter.sharedMesh;
   if(!renderer||!renderer.enabled||!mesh||!mesh.isReadable)continue;
   var materials=renderer.sharedMaterials;
   if(materials.Length!=mesh.subMeshCount)continue;
   bool opaque=true;foreach(var mat in materials)if(!mat||mat.renderQueue>=3000)opaque=false;
   if(!opaque)continue;
   for(int i=0;i<materials.Length;i++) {
    if(!groups.TryGetValue(materials[i],out var group)) {
     group=new List<CombineInstance>();groups.Add(materials[i],group);
    }
    group.Add(new CombineInstance{mesh=mesh,subMeshIndex=i,
     transform=sector.worldToLocalMatrix*filter.transform.localToWorldMatrix});
   }
   sources.Add(renderer);
  }
  SectorRenderersBefore+=sector.GetComponentsInChildren<Renderer>().Length;
  foreach(var entry in groups) {
   var mesh=owned.Keep(new Mesh{name="Sector / "+entry.Key.name,indexFormat=IndexFormat.UInt32});
   mesh.CombineMeshes(entry.Value.ToArray(),true,true);if(Stage==0)TrackCoastMesh(mesh);
   var part=new GameObject("Batched / "+entry.Key.name);part.transform.SetParent(sector,false);
   part.AddComponent<MeshFilter>().sharedMesh=mesh;
   var renderer=part.AddComponent<MeshRenderer>();renderer.sharedMaterial=entry.Key;
   renderer.receiveShadows=true;renderer.shadowCastingMode=ShadowCastingMode.On;
   if(Stage==1&&(entry.Key.name.Contains("Ion_Cyan")||entry.Key.name.Contains("City_Window"))){int sectorIndex=0;int.TryParse(sector.name.Replace("Scenery sector ",""),out sectorIndex);districtWindows[(sectorIndex/3)%3].Add(renderer);}
  }
  foreach(var district in districtWindows)district.RemoveAll(renderer=>sources.Contains(renderer as MeshRenderer));
  foreach(var renderer in sources) {
   renderer.enabled=false;
   if(renderer.transform.childCount==0)Destroy(renderer.gameObject);
   else {Destroy(renderer.GetComponent<MeshFilter>());Destroy(renderer);}
  }
  foreach(var renderer in sector.GetComponentsInChildren<Renderer>())
   if(renderer.enabled)SectorRenderersAfter++;
 }

 void CreateLowMist() {
  mist=null;if(Stage==2)return;
  var shader=Shader.Find("Skybreak/Atmosphere");if(!shader)return;
  mist=owned.Keep(new Material(shader));
  mist.SetColor("_Tint",Stage==0?new Color(.45f,.59f,.70f,.012f):new Color(.24f,.28f,.45f,.018f));
  var layer=Art.Primitive(terrain,"Low sea mist",new Vector3(0,-1.85f,20),
   new Vector3(15,1,26),mist,PrimitiveType.Plane);
  layer.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
  layer.GetComponent<Renderer>().receiveShadows=false;
 }

 public void SetMissionProgress(float progress,bool rescued) {
  missionProgress=Mathf.Clamp01(progress);signalRecovered=rescued;
 }
 void UpdateChapterAtmosphere(float dt) {
  if(mist)mist.SetFloat("_Travel",motion);
  float target=Stage==1?(signalRecovered?1.48f:1.05f):Stage==0?1.3f+missionProgress*.2f:1.22f;
  sun.intensity=Mathf.Lerp(sun.intensity,target,1-Mathf.Exp(-dt*.55f));
 }
}
}
