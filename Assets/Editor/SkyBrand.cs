using UnityEngine;
using UnityEditor;
using System.IO;
public static class SkyBrand {
 // The game's small vector-like flight emblem, drawn at icon resolution.
 public static void Apply(){
  const int n=512;var tex=new Texture2D(n,n,TextureFormat.RGBA32,false);var pixels=new Color[n*n];
  for(int y=0;y<n;y++)for(int x=0;x<n;x++){
   float px=(x+.5f)/n*2-1,py=(y+.5f)/n*2-1;float r=Mathf.Sqrt(px*px+py*py);Color c=Color.Lerp(new Color(.015f,.025f,.05f),new Color(.02f,.12f,.18f),Mathf.Clamp01(1-r));
   float side=Mathf.Abs(px);bool wing=py<.69f-side*1.6f&&py>-.57f+side*.34f&&side>.06f&&side<.69f;
   bool notch=py<-.09f-side*.64f;bool spine=side<.055f&&py>-.49f&&py<.28f;
   if(wing&&!notch)c=Color.Lerp(new Color(.2f,.75f,.85f),new Color(.86f,1,1),(py+1)/2);
   if(spine)c=new Color(1,.35f,.09f);
   if(Mathf.Abs(r-.84f)<.003f&&py<.3f)c=new Color(.07f,.33f,.42f);
   float corner=Mathf.Sqrt(Mathf.Pow(Mathf.Max(0,side-.71f),2)+Mathf.Pow(Mathf.Max(0,Mathf.Abs(py)-.71f),2));c.a=1-Mathf.SmoothStep(.275f,.29f,corner);pixels[y*n+x]=c;
  }
  tex.SetPixels(pixels);tex.Apply();Directory.CreateDirectory("Assets/Art/UI");string path="Assets/Art/UI/FlightEmblem.png";File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);var icon=AssetDatabase.LoadAssetAtPath<Texture2D>(path);PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{icon});
 }
}
