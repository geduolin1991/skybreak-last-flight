using UnityEngine;
using System.IO;

namespace Skybreak {
public partial class SkyGame {
 void WriteVerificationIdentity(string folder,string kind) {
  var identity=Resources.Load<TextAsset>("SkyBuildIdentity");
  if(!identity){Debug.LogError("Missing build identity; this QA report cannot authorize a release.");return;}
  File.WriteAllText(Path.Combine(folder,kind+"-build-identity.txt"),identity.text.Trim()+"\n");
 }
}
}
