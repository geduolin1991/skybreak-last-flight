using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
public static class SkyFrameBuild {
 public static void Prepare(){
  AssetDatabase.Refresh();
  string[] names={"Pearl","Carbon","Steel","Blue","Crimson","Silver","Brass","Ice","Ember","Iris"};
  Color[] colors={new Color(.78f,.86f,.88f),new Color(.018f,.032f,.052f),new Color(.20f,.30f,.37f),new Color(.027f,.19f,.30f),new Color(.47f,.048f,.035f),new Color(.44f,.50f,.69f),new Color(.63f,.37f,.12f),new Color(.08f,.75f,.94f),new Color(1,.38f,.075f),new Color(.60f,.46f,1)};
  for(int i=0;i<names.Length;i++){
   string name="Frame_"+names[i],path="Assets/Art/Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);bool fresh=!mat;
   if(fresh)mat=new Material(Shader.Find("Standard"));mat.name=name;mat.color=colors[i];mat.enableInstancing=true;mat.SetFloat("_Metallic",i==0?.48f:i>=7?.35f:.66f);mat.SetFloat("_Glossiness",i==1?.48f:.64f);
   if(i>=7){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",colors[i]*1.35f);}
   if(fresh)AssetDatabase.CreateAsset(mat,path);else EditorUtility.SetDirty(mat);
  }
  var geometry=new System.Collections.Generic.HashSet<Mesh>();
  foreach(string name in new[]{"AstraFrame","CrimsonFrame","OracleFrame"}){
   SkyBuild.PrepareModel("Assets/Art/Models/"+name+".fbx");var model=Resources.Load<GameObject>("Models/"+name);
   foreach(var f in model.GetComponentsInChildren<MeshFilter>())if(!geometry.Add(f.sharedMesh))throw new Exception("Different frames share a mutable baked mesh: "+name);
   int joints=model.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("Motion_"));
   if(joints<6||model.GetComponentsInChildren<Renderer>().Length>65)throw new Exception("Frame articulation or renderer budget failed: "+name);
   Debug.Log("FRAME_IMPORT "+name+" joints="+joints+" renderers="+model.GetComponentsInChildren<Renderer>().Length);
  }
  PlayerSettings.bundleVersion="1.13.0";AssetDatabase.SaveAssets();Debug.Log("SKYBREAK_FRAMES_PREPARED");
 }
 public static void Build(){Prepare();SkyBuild.BuildCurrentMac();}
}
