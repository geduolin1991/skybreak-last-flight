using UnityEngine;
using UnityEngine.Rendering;
namespace Skybreak {
public partial class SkyGame {
 void BuildGroundMap115(){
  var owned=groundRoot.GetComponent<SkyOwnedResources>();var material=owned.Keep(new Material(Shader.Find("Skybreak/GroundBattle")));material.color=GroundArea<2?new Color(.21f,.25f,.27f):new Color(.12f,.18f,.23f);material.SetFloat("_Night",GroundArea<2?0:1);
  var floor=Art.Primitive(groundRoot,"Concrete apron / physical battle surface",new Vector3(0,-.05f,0),new Vector3(10,1,14),material,PrimitiveType.Plane);floor.GetComponent<Renderer>().receiveShadows=true;
  var concrete=Art.Mat("Ground115 retaining wall",new Color(.23f,.26f,.26f));var trim=Art.Mat("Ground115 yellow lane",new Color(.64f,.47f,.19f));
  for(int side=-1;side<=1;side+=2){
   Art.Box(groundRoot,"Raised perimeter curb",new Vector3(side*11.7f,.2f,0),new Vector3(.42f,.4f,42),concrete);
   for(int row=0;row<3;row++){var factory=Art.Model(GroundArea>=2&&row==1?"GroundReactor115":"GroundIndustry115",groundRoot);factory.transform.position=new Vector3(side*(15.3f+row%2*1.1f),0,-12+row*13);factory.transform.localScale=Vector3.one*(.85f+row%2*.13f);factory.transform.rotation=Quaternion.Euler(0,side>0?90:-90,0);}
   for(int row=0;row<6;row++){float z=-14+row*5.6f;Art.Box(groundRoot,"Service light pole",new Vector3(side*11.3f,1.4f,z),new Vector3(.085f,2.8f,.085f),concrete);Art.Box(groundRoot,"Shielded amber luminaire",new Vector3(side*11.15f,2.9f,z),new Vector3(.42f,.13f,.19f),Art.Mat("Ground115 lamps",new Color(.9f,.57f,.20f),true));}
  }
  Vector3[] layout=GroundArea%2==0?new[]{new Vector3(-3.8f,0,-4),new Vector3(4.4f,0,-2.6f),new Vector3(-1,0,2.2f),new Vector3(6.8f,0,4.3f),new Vector3(-7.3f,0,4.6f)}:new[]{new Vector3(-4.0f,0,-4.5f),new Vector3(4.0f,0,-4.5f),new Vector3(0,0,-1),new Vector3(-6.8f,0,2.4f),new Vector3(6.8f,0,2.4f)};
  for(int i=0;i<layout.Length;i++){var p=layout[i];if(GroundArea>=2)p.x=-p.x;var go=Art.Model("GroundBarrier115",groundRoot);go.transform.position=p;groundCovers.Add(new GroundCover115{go=go,p=p,half=new Vector2(1.6f,.5f),hp=260,max=260,low=true});}
  // Heavy fixed structures block both forms. Low barricades permit a mech jump.
  for(int s=-1;s<=1;s+=2){var p=new Vector3(s*9.5f,0,-6.4f);var go=Art.Model("GroundRelay115",groundRoot);go.transform.position=p;go.transform.localScale=Vector3.one*.7f;groundCovers.Add(new GroundCover115{go=go,p=p,half=new Vector2(.6f,.6f),hp=99999,max=99999,low=false});}
  for(int i=0;i<14;i++){var ring=Art.Ring(groundRoot,1,Art.Orange,.045f,48);ring.sharedMaterial=owned.Keep(new Material(Shader.Find("Skybreak/Telegraph")));ring.startColor=ring.endColor=Art.Orange;ring.enabled=false;groundMarks.Add(new GroundMarker115{ring=ring});}
  var exit=Art.Ring(groundRoot,1.75f,new Color(.14f,.85f,.48f),.07f,48);exit.transform.position=groundExitPos;groundExitRing=exit.transform;exit.gameObject.SetActive(false);
  for(int s=-1;s<=1;s+=2)Art.Box(groundRoot,"Exit runway chevron",new Vector3(s*.52f,.025f,10),new Vector3(.10f,.015f,2.1f),trim);
  var rescue=Art.Model("GroundRelay115",groundRoot);rescue.name="Shelter rescue terminal";rescue.transform.position=new Vector3(-8,0,-1);rescue.transform.localScale=Vector3.one*.55f;rescue.SetActive(GroundArea%2==0);
  foreach(var t in rescue.GetComponentsInChildren<Transform>())if(t.name.StartsWith("Motion_"))groundRotors.Add(t);
  var circle=Art.Ring(groundRoot,1.85f,Art.Cyan,.04f,48);circle.transform.position=new Vector3(-8,.07f,-1);groundRescueRing=circle.transform;circle.gameObject.SetActive(GroundArea%2==0);
  // Transit rails and buses are parented to this arena, never to scrolling air scenery.
  for(int side=-1;side<=1;side+=2){
   float x=side*19.4f;
   for(int rail=-1;rail<=1;rail+=2)Art.Box(groundRoot,"Freight rail",new Vector3(x+rail*.36f,.035f,0),new Vector3(.06f,.08f,46),Art.Mat("Ground115 rail steel",new Color(.26f,.30f,.32f),false,.75f));
   for(int tie=0;tie<48;tie++)Art.Box(groundRoot,"Rail sleeper",new Vector3(x,.015f,-23+tie),new Vector3(1.25f,.06f,.13f),concrete);
   var transit=Art.Model(GroundArea<2?"MetroCar112":"EvacBus",groundRoot);transit.transform.position=new Vector3(x,.08f,-9+side*2);transit.transform.localScale=Vector3.one*.7f;groundTransit.Add(transit.transform);
  }
  for(int i=0;i<2;i++){var bus=Art.Model("EvacBus",groundRoot);bus.transform.position=new Vector3(-12.5f,0,-1-i*3);bus.transform.localScale=Vector3.one*.42f;groundEvacBuses.Add(bus.transform);}
  // Freight loading cranes and elevated coolant mains give the aprons scale.
  for(int s=-1;s<=1;s+=2){
   var gantry=Art.Model("GroundCrane115",groundRoot);gantry.name="Elevated cargo gantry";gantry.transform.position=new Vector3(s*14,0,9);gantry.transform.localScale=Vector3.one*.85f;gantry.transform.rotation=Quaternion.Euler(0,90,0);foreach(var t in gantry.GetComponentsInChildren<Transform>())if(t.name.StartsWith("Motion_"))groundRotors.Add(t);
  }
  // The shelter door and evacuation traffic reflect the actual rescue result.
  if(GroundArea==1){Art.Box(groundRoot,"Bridge parapet west",new Vector3(-10.5f,.65f,0),new Vector3(.38f,1.3f,26),concrete);Art.Box(groundRoot,"Bridge parapet east",new Vector3(10.5f,.65f,0),new Vector3(.38f,1.3f,26),concrete);}
  var shelter=Art.Model("GroundIndustry115",groundRoot);shelter.transform.position=new Vector3(-15,0,-1);shelter.transform.localScale=Vector3.one*.85f;
  CombineGroundScenery115();
 }
 void CombineGroundScenery115(){
  var groups=new System.Collections.Generic.Dictionary<Material,System.Collections.Generic.List<CombineInstance>>();var sources=new System.Collections.Generic.List<MeshRenderer>();
  foreach(var filter in groundRoot.GetComponentsInChildren<MeshFilter>()){
   bool keep=false;foreach(var c in groundCovers)if(filter.transform==c.go.transform||filter.transform.IsChildOf(c.go.transform)){keep=true;break;}
   foreach(var t in groundEvacBuses)if(filter.transform==t||filter.transform.IsChildOf(t))keep=true;
   foreach(var t in groundTransit)if(filter.transform==t||filter.transform.IsChildOf(t))keep=true;
   foreach(var t in groundRotors)if(filter.transform==t||filter.transform.IsChildOf(t))keep=true;
   if(keep)continue;var r=filter.GetComponent<MeshRenderer>();var mesh=filter.sharedMesh;if(!r||!r.enabled||!mesh||!mesh.isReadable)continue;
   for(int i=0;i<r.sharedMaterials.Length&&i<mesh.subMeshCount;i++){var m=r.sharedMaterials[i];if(!m)continue;if(!groups.TryGetValue(m,out var list)){list=new System.Collections.Generic.List<CombineInstance>();groups[m]=list;}list.Add(new CombineInstance{mesh=mesh,subMeshIndex=i,transform=groundRoot.worldToLocalMatrix*filter.transform.localToWorldMatrix});}
   sources.Add(r);
  }
  foreach(var group in groups){var mesh=groundRoot.GetComponent<SkyOwnedResources>().Keep(new Mesh{name="Ground arena batch / "+group.Key.name,indexFormat=IndexFormat.UInt32});mesh.CombineMeshes(group.Value.ToArray(),true,true);var go=new GameObject(mesh.name);go.transform.SetParent(groundRoot,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=group.Key;r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;}
  foreach(var r in sources)r.enabled=false;
 }

}
}
