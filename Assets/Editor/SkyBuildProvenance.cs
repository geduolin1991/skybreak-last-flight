using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;

// Bind a playable build and its QA reports to the exact production inputs.
public static class SkyBuildProvenance {
 const string Identity="Assets/Resources/SkyBuildIdentity.txt";
 public static string Fingerprint() {
  string[] settings={"ProjectSettings.asset","GraphicsSettings.asset","QualitySettings.asset",
   "TagManager.asset","InputManager.asset","ProjectVersion.txt","TimeManager.asset"};
  var files=Directory.GetFiles("Assets","*",SearchOption.AllDirectories)
   .Where(p=>!p.Replace('\\','/').StartsWith(Identity,StringComparison.Ordinal))
   .Concat(new[]{"Packages/manifest.json","Packages/packages-lock.json"})
   .Concat(settings.Select(p=>"ProjectSettings/"+p))
   .Where(File.Exists).Select(p=>p.Replace('\\','/')).OrderBy(p=>p,StringComparer.Ordinal);
  using(var content=new MemoryStream())using(var sha=SHA256.Create()) {
   foreach(string path in files) {
    byte[] name=Encoding.UTF8.GetBytes(path+"\0");content.Write(name,0,name.Length);
    byte[] digest;using(var stream=File.OpenRead(path))digest=sha.ComputeHash(stream);
    content.Write(digest,0,digest.Length);
   }
   return BitConverter.ToString(sha.ComputeHash(content.ToArray())).Replace("-","").ToLowerInvariant();
  }
 }
 public static string Prepare() {
  AssetDatabase.SaveAssets();string id=Fingerprint();
  File.WriteAllText(Identity,id+"\n",new UTF8Encoding(false));
  AssetDatabase.ImportAsset(Identity,ImportAssetOptions.ForceSynchronousImport);
  AssetDatabase.SaveAssets();return id;
 }
 public static void Record(string id) {
  if(Fingerprint()!=id)throw new InvalidOperationException("Build inputs changed during compilation; rebuild before release.");
  File.WriteAllText("Build/build-identity.txt",id+"\n",new UTF8Encoding(false));
 }
}
