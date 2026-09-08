using System;
using UnityEditor;
using UnityEngine;

public static class SkyCoastBuild {
 public static void Prepare() {
  AssetDatabase.Refresh();
  string[] names={"Coast_Chalk","Coast_Stone","Coast_Sand","Coast_Foliage","Coast_Navy","Coast_Teal","Coast_Rust","Coast_Glass","Coast_Ivory"};
  Color[] colors={new Color(.54f,.59f,.56f),new Color(.18f,.24f,.27f),new Color(.62f,.59f,.44f),new Color(.12f,.28f,.17f),new Color(.06f,.14f,.22f),new Color(.10f,.32f,.36f),new Color(.48f,.22f,.12f),new Color(.09f,.25f,.33f),new Color(.72f,.76f,.71f)};
  for(int i=0;i<names.Length;i++) {
   string path="Assets/Art/Materials/"+names[i]+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(!mat){mat=new Material(Shader.Find("Standard")){name=names[i]};AssetDatabase.CreateAsset(mat,path);}
   mat.color=colors[i];mat.SetFloat("_Metallic",i==7?.35f:i==4?.18f:.02f);mat.SetFloat("_Glossiness",i==7?.65f:i==4?.3f:.22f);mat.enableInstancing=true;EditorUtility.SetDirty(mat);
  }
  foreach(string name in new[]{"DawnHarbor","DawnCove"})SkyBuild.PrepareModel("Assets/Art/Models/"+name+".fbx");
  var texture=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/World/CoastalWaveSpectrum.png");
  texture.textureType=TextureImporterType.Default;texture.sRGBTexture=false;texture.mipmapEnabled=true;texture.filterMode=FilterMode.Trilinear;texture.wrapMode=TextureWrapMode.Repeat;texture.anisoLevel=2;texture.alphaSource=TextureImporterAlphaSource.FromInput;texture.alphaIsTransparency=false;texture.textureCompression=TextureImporterCompression.Uncompressed;texture.maxTextureSize=512;texture.SaveAndReimport();
  AssetDatabase.SaveAssets();
  foreach(string name in new[]{"DawnHarbor","DawnCove"})if(!Resources.Load<GameObject>("Models/"+name))throw new Exception("Coast prefab missing: "+name);
  Debug.Log("SKYBREAK_COAST_PREPARED");
 }
}
