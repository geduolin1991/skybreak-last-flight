using System;
using UnityEditor;
using UnityEngine;
public static class SkyCampaignBuild {
 public static void Prepare(){
  AssetDatabase.Refresh();
  string[] names={"Ivory","Graphite","Glass","Copper","Light","Window","Solar","Green"};
  Color[] colors={new Color(.67f,.73f,.72f),new Color(.055f,.095f,.12f),new Color(.075f,.23f,.30f),new Color(.46f,.24f,.12f),new Color(.3f,.76f,.82f),new Color(.68f,.44f,.17f),new Color(.035f,.11f,.23f),new Color(.095f,.25f,.16f)};
  for(int i=0;i<names.Length;i++){
   string name="Campaign_"+names[i],path="Assets/Art/Materials/"+name+".mat";
   var mat=AssetDatabase.LoadAssetAtPath<Material>(path);bool fresh=!mat;
   if(fresh)mat=new Material(Shader.Find("Standard"));mat.name=name;mat.color=colors[i];mat.enableInstancing=true;
   mat.SetFloat("_Metallic",i==0||i==7?.08f:i==2||i==6?.42f:.65f);mat.SetFloat("_Glossiness",i==0||i==7?.32f:i==2||i==6?.78f:.58f);
   if(i==4||i==5){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",colors[i]*(i==4?1.8f:.9f));}
   if(fresh)AssetDatabase.CreateAsset(mat,path);else EditorUtility.SetDirty(mat);
  }
  foreach(string name in new[]{"HarborSignal112","CivicHospital112","ElevatedRail112","MetroCar112","OrbitalObservatory112","SolarSail112"})SkyBuild.PrepareModel("Assets/Art/Models/"+name+".fbx");
  PlayerSettings.bundleVersion="1.14.0";AssetDatabase.SaveAssets();SkyVoiceBuildChecks.Check();Debug.Log("SKYBREAK_CAMPAIGN_PREPARED");
 }
 public static void Build(){Prepare();SkyBuild.BuildCurrentMac();}
}
