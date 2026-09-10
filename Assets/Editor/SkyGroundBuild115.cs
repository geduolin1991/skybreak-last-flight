using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
public static class SkyGroundBuild115 {
 public static readonly string[] Models={"KestrelTank115","MantaTank115","NeedleTank115","SiegeCrawler115","BastionCrawler115","SiegeWalker115","MortarBattery115","GroundBarrier115","GroundRelay115","GroundIndustry115","OrbitalHabitat115","OrbitalRefinery115","GroundCrane115","GroundReactor115","FurnaceWalker115"};
 public static void Prepare(){
  Debug.Log("SKYBREAK_GROUND_PROJECT "+Application.dataPath+" Unity "+Application.unityVersion);AssetDatabase.Refresh();
  string[] names={"Sand","Red","Blue","Pearl","Purple","Orange"};Color[] colors={new Color(.40f,.34f,.24f),new Color(.31f,.055f,.039f),new Color(.05f,.22f,.32f),new Color(.68f,.76f,.77f),new Color(.32f,.27f,.44f),new Color(.93f,.35f,.08f)};
  for(int i=0;i<names.Length;i++){string name="Ground_"+names[i],path="Assets/Art/Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);bool fresh=!mat;if(fresh)mat=new Material(Shader.Find("Standard"));mat.name=name;mat.color=colors[i];mat.enableInstancing=true;mat.SetFloat("_Metallic",i==0?.07f:.43f);mat.SetFloat("_Glossiness",i==0?.23f:.44f);if(i==5){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",colors[i]*.38f);}if(fresh)AssetDatabase.CreateAsset(mat,path);else EditorUtility.SetDirty(mat);}
  foreach(var name in Models){SkyBuild.PrepareModel("Assets/Art/Models/"+name+".fbx");var model=Resources.Load<GameObject>("Models/"+name);int tri=model.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3);if(tri<300||tri>30000)throw new Exception("Ground asset budget/import failed "+name+" "+tri);Debug.Log("GROUND_IMPORT "+name+" triangles="+tri+" renderers="+model.GetComponentsInChildren<Renderer>().Length);}
  var graphics=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset").First();var so=new SerializedObject(graphics);var arr=so.FindProperty("m_AlwaysIncludedShaders");foreach(string name in new[]{"Skybreak/OrbitalDepth","Skybreak/GroundBattle"}){var shader=Shader.Find(name);if(!shader)throw new Exception("Missing shader "+name);bool found=false;for(int i=0;i<arr.arraySize;i++)if(arr.GetArrayElementAtIndex(i).objectReferenceValue==shader)found=true;if(!found){int index=arr.arraySize;arr.InsertArrayElementAtIndex(index);arr.GetArrayElementAtIndex(index).objectReferenceValue=shader;}}
  so.ApplyModifiedPropertiesWithoutUndo();PlayerSettings.bundleVersion="1.15.0";AssetDatabase.SaveAssets();Debug.Log("SKYBREAK_GROUND_PREPARED");
 }
 public static void Build(){Prepare();SkyBuild.BuildCurrentMac();}
}
