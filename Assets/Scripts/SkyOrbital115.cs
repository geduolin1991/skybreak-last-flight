using UnityEngine;
using UnityEngine.Rendering;
namespace Skybreak {
public partial class SkyWorld {
 Transform distantPlanet115;
 void BuildOrbitalDepth115(){
  var veil=owned.Keep(new Material(Shader.Find("Skybreak/OrbitalDepth")));veil.color=new Color(.08f,.16f,.30f);
  var sky=Art.Primitive(terrain,"Distant blue dust / background only",Cam.transform.position+Cam.transform.forward*150,new Vector3(240,170,1),veil,PrimitiveType.Quad);
  sky.transform.rotation=Cam.transform.rotation;sky.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
  var planet=Art.Model("BluePlanet",terrain);planet.name="Distant planet / softened cloud systems";distantPlanet115=planet.transform;
  planet.transform.localPosition=new Vector3(-21,-48,43);planet.transform.localScale=Vector3.one*43;planet.transform.rotation=Quaternion.Euler(0,0,20);
  var surface=owned.Keep(new Material(Shader.Find("Skybreak/Planet")));
  foreach(var r in planet.GetComponentsInChildren<Renderer>()){r.sharedMaterial=surface;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
  var halo=Art.Model("BluePlanet",terrain);halo.name="Planet atmosphere / soft limb";halo.transform.position=planet.transform.position;halo.transform.rotation=planet.transform.rotation;halo.transform.localScale=Vector3.one*43.9f;
  var atmosphere=owned.Keep(new Material(Shader.Find("Skybreak/OrbitalDepth")));atmosphere.SetFloat("_Shell",1);atmosphere.color=new Color(.08f,.23f,.42f);
  foreach(var r in halo.GetComponentsInChildren<Renderer>()){r.sharedMaterial=atmosphere;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
 }
 void Orbit115(Transform sector,int index){
  for(int side=-1;side<=1;side+=2){
   string model=index%3==1?"OrbitalRefinery115":"OrbitalHabitat115";
   PlaceScenery(sector,model,new Vector3(side*(19+index%2*3),-7.5f-index%3*2,0),index%3==1?.68f:.76f);
  }
 }
 void ToneOrbitalSector115(Transform sector){
  foreach(var renderer in sector.GetComponentsInChildren<MeshRenderer>()){
   var materials=renderer.sharedMaterials;
   for(int i=0;i<materials.Length;i++){
    var source=materials[i];if(!source||source.name.StartsWith("Distant /"))continue;
    if(!landmarkMaterials.TryGetValue(source,out var distant)){distant=owned.Keep(new Material(source){name="Distant / "+source.name});distant.color=source.color*.50f;distant.SetColor("_EmissionColor",source.GetColor("_EmissionColor")*.12f);distant.SetFloat("_Glossiness",.35f);landmarkMaterials[source]=distant;}
    materials[i]=distant;
   }
   renderer.sharedMaterials=materials;
  }
 }
 public void SuspendForGround115(bool active){Scrolling=!active;if(terrain)terrain.gameObject.SetActive(!active);}
 public void GroundLighting115(int chapter){
  sun.color=chapter==0?new Color(1,.88f,.71f):new Color(.60f,.73f,.94f);sun.intensity=chapter==0?1.42f:1.15f;sun.transform.rotation=Quaternion.Euler(45,132,0);
  fillLight.color=chapter==0?new Color(.45f,.65f,.84f):new Color(1,.43f,.22f);fillLight.intensity=.36f;
  RenderSettings.ambientSkyColor=new Color(.23f,.29f,.36f);RenderSettings.ambientEquatorColor=new Color(.13f,.15f,.19f);RenderSettings.ambientGroundColor=new Color(.06f,.065f,.075f);
  Cam.backgroundColor=chapter==0?new Color(.11f,.17f,.21f):new Color(.045f,.075f,.12f);RenderSettings.fogColor=Cam.backgroundColor;RenderSettings.fogDensity=.003f;
 }
}
}
