using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using System;
using System.IO;
using System.Linq;
using Skybreak;
public class SkyModelImporter:AssetPostprocessor {
 void OnPreprocessTexture(){if(!assetPath.StartsWith("Assets/Resources/Pilots"))return;var t=(TextureImporter)assetImporter;bool web=EditorUserBuildSettings.activeBuildTarget==BuildTarget.WebGL;t.mipmapEnabled=web;t.filterMode=web?FilterMode.Trilinear:FilterMode.Bilinear;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.CompressedHQ;t.wrapMode=TextureWrapMode.Clamp;t.sRGBTexture=true;t.alphaIsTransparency=true;}
 void OnPreprocessModel(){if(!assetPath.StartsWith("Assets/Art/Models/"))return;var m=(ModelImporter)assetImporter;m.importCameras=false;m.importLights=false;m.importAnimation=false;m.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;m.globalScale=1;m.meshCompression=ModelImporterMeshCompression.Off;}
 void OnPreprocessAudio(){if(!assetPath.StartsWith("Assets/Resources/Audio"))return;var a=(AudioImporter)assetImporter;var s=a.defaultSampleSettings;bool web=EditorUserBuildSettings.activeBuildTarget==BuildTarget.WebGL;s.loadType=web?AudioClipLoadType.CompressedInMemory:assetPath.Contains("Music")?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;s.compressionFormat=AudioCompressionFormat.Vorbis;s.quality=web?(assetPath.Contains("Music")?.58f:.7f):.8f;if(web&&(assetPath.Contains("Music")||assetPath.Contains("Ambience"))){s.sampleRateSetting=AudioSampleRateSetting.OverrideSampleRate;s.sampleRateOverride=32000;}s.preloadAudioData=true;a.defaultSampleSettings=s;}
}
public static class SkyBuild {
 [MenuItem("Skybreak/1 · Prepare game and open scene")]
 public static void Prepare(){Directory.CreateDirectory("Assets/Resources/Models");Directory.CreateDirectory("Assets/Art/Materials");Directory.CreateDirectory("Assets/Scenes");AssetDatabase.Refresh();
 foreach(var f in Directory.GetFiles("Assets/Art/Models","*.fbx"))PrepareModel(f);

 var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var game=new GameObject("SKYBREAK · Game Director");game.AddComponent<SkyGame>();var cam=new GameObject("Flight camera");cam.tag="MainCamera";var camera=cam.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=14;cam.transform.position=new Vector3(0,34,-19);cam.transform.rotation=Quaternion.Euler(60,0,0);cam.AddComponent<AudioListener>();
 EditorSceneManager.SaveScene(scene,"Assets/Scenes/Skybreak.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Skybreak.unity",true)};
 SkyBrand.Apply();PlayerSettings.companyName="Skybreak Studio";PlayerSettings.productName="SKYBREAK";PlayerSettings.bundleVersion="1.12.1";PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Standalone,"studio.skybreak.lastflight");PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=false;PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);PlayerSettings.SetArchitecture(UnityEditor.Build.NamedBuildTarget.Standalone,2);PlayerSettings.usePlayerLog=true;PlayerSettings.SplashScreen.show=false;QualitySettings.antiAliasing=4;QualitySettings.shadowDistance=75;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;QualitySettings.pixelLightCount=4;QualitySettings.vSyncCount=1;
 var graphics=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset").FirstOrDefault();if(graphics){var so=new SerializedObject(graphics);var keep=so.FindProperty("m_InstancingStripping");if(keep!=null)keep.intValue=2;var arr=so.FindProperty("m_AlwaysIncludedShaders");foreach(string s in new[]{"Standard","Skybreak/Ocean","Skybreak/Finish","Particles/Standard Unlit","Skybreak/Projectile","Skybreak/Planet","Skybreak/GlowSprite","Skybreak/Pilot","Skybreak/Smoke","Skybreak/Atmosphere","Skybreak/Telegraph","Skybreak/ProjectileRim","Skybreak/Nova","Skybreak/Ground"}){var shader=Shader.Find(s);if(shader){bool found=false;for(int i=0;i<arr.arraySize;i++)if(arr.GetArrayElementAtIndex(i).objectReferenceValue==shader)found=true;if(!found){int i=arr.arraySize;arr.InsertArrayElementAtIndex(i);arr.GetArrayElementAtIndex(i).objectReferenceValue=shader;}}}so.ApplyModifiedPropertiesWithoutUndo();}
 AssetDatabase.SaveAssets();Debug.Log("SKYBREAK_PREPARE_OK");}

 public static void PrepareModel(string f){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(f);if(!prefab)throw new Exception("Missing model "+f);var o=(GameObject)PrefabUtility.InstantiatePrefab(prefab);PrefabUtility.UnpackPrefabInstance(o,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);foreach(var renderer in o.GetComponentsInChildren<Renderer>()){var src=renderer.sharedMaterials;for(int i=0;i<src.Length;i++){var original=src[i];string name=original?original.name:"Titanium_Dark";if(name.StartsWith("World_"))name=name.Substring(6);if(name.StartsWith("V16_"))name=name.Substring(4);string path="Assets/Art/Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat){mat=new Material(Shader.Find("Standard"));mat.name=name;Color c=new Color(.16f,.23f,.28f);bool emission=false;switch(name){case "Ceramic_Ivory":c=new Color(.65f,.76f,.79f);break;case "Titanium_Dark":c=new Color(.025f,.046f,.07f);break;case "Steel_Edge":c=new Color(.16f,.23f,.28f);break;case "Enemy_Red":c=new Color(.34f,.032f,.019f);break;case "Safety_Orange":c=new Color(.95f,.24f,.045f);break;case "Ion_Cyan":c=new Color(.04f,.75f,1);emission=true;break;case "Reactor_Orange":c=new Color(1,.12f,.015f);emission=true;break;case "Core_Violet":c=new Color(.54f,.055f,1);emission=true;break;case "Volcanic_Basalt":c=new Color(.13f,.19f,.20f);break;case "Planet_Ocean":c=new Color(.015f,.09f,.18f);break;case "Civilian_White":c=new Color(.76f,.79f,.72f);break;case "Rescue_Gold":c=new Color(.95f,.65f,.15f);break;case "City_Concrete":c=new Color(.24f,.29f,.32f);break;case "City_Window":c=new Color(.16f,.38f,.5f);emission=true;break;case "Park_Green":c=new Color(.08f,.23f,.16f);break;case "Road_Asphalt":c=new Color(.038f,.052f,.063f);break;case "Cockpit_Glass":c=new Color(.015f,.11f,.18f);break;}mat.color=c;mat.SetFloat("_Metallic",name=="Ceramic_Ivory"?.45f:.72f);mat.SetFloat("_Glossiness",name=="Cockpit_Glass"?.92f:.62f);if(emission){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",c*3);}if(name=="Volcanic_Basalt"||name=="City_Concrete"||name=="Park_Green"){mat.SetFloat("_Metallic",.05f);mat.SetFloat("_Glossiness",.2f);}mat.enableInstancing=true;AssetDatabase.CreateAsset(mat,path);}src[i]=mat;}renderer.sharedMaterials=src;}
 if(!f.Contains("Frame"))CombineModel(o);PrefabUtility.SaveAsPrefabAsset(o,"Assets/Resources/Models/"+Path.GetFileNameWithoutExtension(f)+".prefab");UnityEngine.Object.DestroyImmediate(o);}

 static void CombineModel(GameObject root){
  // Bake static geometry per material while keeping authored moving pivots intact.
  var moving=root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Motion_")).ToArray();
  foreach(var part in moving){part.SetParent(root.transform,true);CombinePart(part.gameObject,Array.Empty<Transform>());}
  CombinePart(root,moving);
 }
 static void CombinePart(GameObject root,Transform[] keep){
  var groups=new System.Collections.Generic.Dictionary<Material,System.Collections.Generic.List<CombineInstance>>();
  foreach(var f in root.GetComponentsInChildren<MeshFilter>()){
   if(keep.Any(t=>f.transform==t||f.transform.IsChildOf(t)))continue;
   var r=f.GetComponent<MeshRenderer>();if(!r||!f.sharedMesh)continue;
   for(int i=0;i<r.sharedMaterials.Length&&i<f.sharedMesh.subMeshCount;i++){
    var m=r.sharedMaterials[i];if(!m)continue;if(!groups.ContainsKey(m))groups[m]=new System.Collections.Generic.List<CombineInstance>();
    groups[m].Add(new CombineInstance{mesh=f.sharedMesh,subMeshIndex=i,transform=root.transform.worldToLocalMatrix*f.transform.localToWorldMatrix});
   }
  }
  var old=root.transform.Cast<Transform>().Where(t=>!keep.Contains(t)).ToArray();
  foreach(var pair in groups){
   var mesh=new Mesh();mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.CombineMeshes(pair.Value.ToArray(),true,true);mesh.name=root.name+"_"+pair.Key.name;
   string path="Assets/Art/Models/"+mesh.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
   if(existing){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);
   var part=new GameObject(pair.Key.name);part.transform.SetParent(root.transform,false);part.AddComponent<MeshFilter>().sharedMesh=mesh;part.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;
  }
  foreach(var t in old)UnityEngine.Object.DestroyImmediate(t.gameObject);
 }
 [MenuItem("Skybreak/2 · Build macOS game")]
 public static void BuildMac(){BuildAt("Build/SKYBREAK.app");}
 [MenuItem("Skybreak/4 · Build candidate game")]
 public static void BuildCandidate(){BuildAt("Build/SKYBREAK-1.12.app");}
 [MenuItem("Skybreak/5 · Build current imported game for macOS")]
 public static void BuildCurrentMac(){BuildAt("Build/SKYBREAK.app",false);Check();}
 static void BuildAt(string appPath,bool prepare=true){if(prepare)Prepare();else {if(!File.Exists("Assets/Scenes/Skybreak.unity"))throw new Exception("Prepare the scene before building");PlayerSettings.bundleVersion="1.12.1";AssetDatabase.SaveAssets();}SkyVoiceBuildChecks.Check();Directory.CreateDirectory("Build");string buildIdentity=SkyBuildProvenance.Prepare();var options=new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Skybreak.unity"},locationPathName=appPath,target=BuildTarget.StandaloneOSX,options=BuildOptions.None};var report=BuildPipeline.BuildPlayer(options);File.WriteAllText("Build/build-report.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\nSize: "+report.summary.totalSize+"\nTime: "+report.summary.totalTime);if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed");SkyBuildProvenance.Record(buildIdentity);File.WriteAllText("Build/build-app-path.txt",appPath);Debug.Log("SKYBREAK_BUILD_OK");}
 [MenuItem("Skybreak/3 · Run geometry and rules checks")]
 public static void Check(){foreach(var name in new[]{"Kestrel","Manta","Needle","Drone","Interceptor","Gunship","Leviathan","Tempest","Seraph","AstraFrame","CrimsonFrame","OracleFrame","EvacCarrier","StormRelay","OrbitalGate","SiegeTurret","ShieldEmitter","HarborPort","StormCity","OrbitHabitat","BasaltIsland"}){var g=Resources.Load<GameObject>("Models/"+name);if(!g||g.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh?f.sharedMesh.vertexCount:0)<100)throw new Exception("Model failed: "+name);}if(SkyGame.SegmentDistance(Vector3.zero,new Vector3(-10,0,0),new Vector3(10,0,0))>.001f)throw new Exception("Swept collision missed");if(Mathf.Abs(SkyGame.SegmentDistance(new Vector3(0,0,2),new Vector3(-10,0,0),new Vector3(10,0,0))-2)>.001f)throw new Exception("Collision distance invalid");foreach(var name in new[]{"MusicSea","MusicCity","MusicOrbit","MusicHangar","MusicSeaIntensity","MusicCityIntensity","MusicOrbitIntensity","ShotPulse","ShotScatter","ShotLance","ShotSeeker","ShotMortar","ShotRailFast","MissileBurst","ImpactLight","ImpactArmor","ExplosionHeavy","BossBreak","Combo","Shot","Bomb","NovaCharge","Explosion"})if(!Resources.Load<AudioClip>("Audio/"+name))throw new Exception("Audio missing "+name);if(!Resources.Load<Font>("Fonts/NotoSansSC-Medium"))throw new Exception("Chinese font missing");Debug.Log("SKYBREAK_RULES_OK");}
 public static void All(){BuildMac();Check();}
}
