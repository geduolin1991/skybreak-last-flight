using UnityEngine;
using UnityEngine.Rendering;

namespace Skybreak {
public partial class SkyWorld {
 Light fillLight;
 static readonly int[] wakeProperties={Shader.PropertyToID("_Wake0"),Shader.PropertyToID("_Wake1"),Shader.PropertyToID("_Wake2")};
 readonly float[] renderedGridPower={-1,-1,-1};
 public int StarDrawCalls {get;private set;}

 void SetChapterLighting(){
  sun.color=Stage==0?new Color(1,.84f,.66f):Stage==1?new Color(.63f,.73f,1):new Color(.68f,.82f,1);
  sun.transform.rotation=Quaternion.Euler(Stage==0?39:Stage==1?53:32,Stage==2?-58:-35,0);
  sun.shadowStrength=.72f;sun.shadowBias=.035f;sun.shadowNormalBias=.18f;
  if(fillLight){fillLight.color=Stage==0?new Color(.36f,.67f,.92f):new Color(1,.47f,.26f);fillLight.intensity=Stage==0?.30f:Stage==1?.23f:.34f;}
  RenderSettings.ambientSkyColor=Stage==0?new Color(.20f,.29f,.38f):Stage==1?new Color(.13f,.19f,.31f):new Color(.10f,.16f,.26f);
  RenderSettings.ambientEquatorColor=Stage==0?new Color(.075f,.13f,.17f):new Color(.055f,.075f,.12f);
  RenderSettings.ambientGroundColor=Stage==0?new Color(.045f,.085f,.085f):new Color(.025f,.035f,.065f);
  RenderSettings.fogDensity=Stage==0?.0025f:Stage==1?.0035f:.0015f;
  for(int i=0;i<renderedGridPower.Length;i++)renderedGridPower[i]=-1;
  StarDrawCalls=0;
 }

 void UpdateRescueWakes(){
  if(!ocean)return;
  for(int i=0;i<3;i++){
   Vector4 wake=Vector4.zero;
   if(i<civilians.Count){var c=civilians[i];if(c.go){Vector3 p=c.go.transform.position;wake=new Vector4(p.x,p.z,c.health>0?1:0,c.departed?1:0);}}
   ocean.SetVector(wakeProperties[i],wake);
  }
 }

 void BuildStarField(){
  const int count=180;var vertices=new Vector3[count*4];var uv=new Vector2[count*4];var triangles=new int[count*6];
  Random.InitState(90);
  for(int i=0;i<count;i++){
   float s=Random.Range(.035f,.13f)*.5f;Vector3 p=new Vector3(Random.Range(-95,95),Random.Range(-30,-20),Random.Range(-50,180));int v=i*4,t=i*6;
   vertices[v]=p+new Vector3(-s,0,-s);vertices[v+1]=p+new Vector3(-s,0,s);vertices[v+2]=p+new Vector3(s,0,s);vertices[v+3]=p+new Vector3(s,0,-s);
   uv[v]=Vector2.zero;uv[v+1]=Vector2.up;uv[v+2]=Vector2.one;uv[v+3]=Vector2.right;
   triangles[t]=v;triangles[t+1]=v+1;triangles[t+2]=v+2;triangles[t+3]=v;triangles[t+4]=v+2;triangles[t+5]=v+3;
  }
  var mesh=owned.Keep(new Mesh{name="Distant stars / single batch"});mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
  var go=new GameObject("Distant stars · 180 points / one draw");go.transform.SetParent(terrain,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
  var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=Art.Mat("stars",new Color(.45f,.65f,.9f),true);renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;StarDrawCalls=1;
 }
}
}
