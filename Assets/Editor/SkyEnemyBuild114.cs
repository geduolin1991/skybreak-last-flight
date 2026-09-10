using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
public static class SkyEnemyBuild114 {
 public static readonly string[] Models={"Drone","Interceptor","Gunship","CargoShuttle","DiveBomber","WardDrone","RailLancer","MineTender","DroneCarrier","RepairPod114","OrdnanceRack114","ReactorCell114","WingBeacon114"};
 public static void Prepare(){
  AssetDatabase.Refresh();
  string[] names={"Hull","Edge","Dark","Glass","Hot","Ivory","Green","Blue","Gold"};
  Color[] colors={new Color(.31f,.047f,.042f),new Color(.48f,.55f,.58f),new Color(.023f,.037f,.053f),new Color(.02f,.13f,.19f),new Color(1,.31f,.05f),new Color(.77f,.83f,.82f),new Color(.06f,.86f,.48f),new Color(.045f,.65f,1),new Color(.92f,.64f,.12f)};
  for(int i=0;i<names.Length;i++){string name="Foundry_"+names[i],path="Assets/Art/Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);bool fresh=!mat;if(fresh)mat=new Material(Shader.Find("Standard"));mat.name=name;mat.color=colors[i];mat.enableInstancing=true;mat.SetFloat("_Metallic",i==5?.28f:i==3?.4f:.62f);mat.SetFloat("_Glossiness",i==3?.81f:.53f);if(i==4||i==6||i==7){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",colors[i]*.9f);}if(fresh)AssetDatabase.CreateAsset(mat,path);else EditorUtility.SetDirty(mat);}
  foreach(var name in Models){SkyBuild.PrepareModel("Assets/Art/Models/"+name+".fbx");var model=Resources.Load<GameObject>("Models/"+name);int triangles=model.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3);int renderers=model.GetComponentsInChildren<Renderer>().Length;if(triangles<500||triangles>16000||renderers>16)throw new Exception("Enemy foundry budget / import failed: "+name);Debug.Log("FOUNDRY_IMPORT "+name+" triangles="+triangles+" renderers="+renderers);}
  PlayerSettings.bundleVersion="1.14.0";AssetDatabase.SaveAssets();Debug.Log("SKYBREAK_FOUNDRY_PREPARED");
 }
 public static void Build(){Prepare();SkyBuild.BuildCurrentMac();}
}
