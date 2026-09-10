using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Run in an isolated project copy: this changes only the copy's platform settings.
public static class SkyWebBuild {
 public static void Build() {
  PlayerSettings.WebGL.template="PROJECT:Skybreak";PlayerSettings.bundleVersion="1.13.0";SkyVoiceBuildChecks.Check();
  PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Brotli;
  PlayerSettings.WebGL.decompressionFallback=true;
  PlayerSettings.WebGL.dataCaching=true;
  // Stable names avoid a Unity 6000.6 Bee regeneration loop with hashed loaders.
  PlayerSettings.WebGL.nameFilesAsHashes=false;
  PlayerSettings.WebGL.initialMemorySize=256;
  PlayerSettings.WebGL.maximumMemorySize=2048;
  PlayerSettings.WebGL.exceptionSupport=WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
  PlayerSettings.WebGL.debugSymbolMode=WebGLDebugSymbolMode.Off;
  PlayerSettings.WebGL.powerPreference=WebGLPowerPreference.HighPerformance;
  PlayerSettings.SetScriptingBackend(NamedBuildTarget.WebGL,ScriptingImplementation.IL2CPP);
  PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL,ManagedStrippingLevel.Low);
  PlayerSettings.runInBackground=false;
  PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL,false);
  PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL,new[]{UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3});
  QualitySettings.antiAliasing=2;
  QualitySettings.vSyncCount=1;
  QualitySettings.shadowResolution=ShadowResolution.Medium;
  // Mobile portraits shrink to a fraction of the source image. Mip levels
  // prevent animated hair, armor seams and alpha edges from crawling.
  foreach(string path in Directory.GetFiles("Assets/Resources/Pilots","*.png")) {
   var importer=(TextureImporter)AssetImporter.GetAtPath(path);
   if(importer.mipmapEnabled&&importer.filterMode==FilterMode.Trilinear)continue;
   importer.mipmapEnabled=true;importer.filterMode=FilterMode.Trilinear;importer.SaveAndReimport();
  }
  // This disposable project uses platform-specific defaults. Unsupported
  // AudioImporter override names can silently do nothing; set real defaults.
  // Web audio stays compressed until playback; full-resolution source WAVs remain.
  foreach(string path in Directory.GetFiles("Assets/Resources/Audio","*.wav")) {
   var importer=(AudioImporter)AssetImporter.GetAtPath(path);
   var settings=importer.defaultSampleSettings;
   settings.loadType=AudioClipLoadType.CompressedInMemory;
   settings.quality=path.Contains("Music")?.58f:.7f;
   if(path.Contains("Music")||path.Contains("Ambience")){settings.sampleRateSetting=AudioSampleRateSetting.OverrideSampleRate;settings.sampleRateOverride=32000;}
   if(importer.defaultSampleSettings.Equals(settings))continue;
   importer.defaultSampleSettings=settings;
   importer.SaveAndReimport();
  }
  AssetDatabase.SaveAssets();
  Directory.CreateDirectory("Build");
  string identity=SkyBuildProvenance.Prepare();
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
   scenes=new[]{"Assets/Scenes/Skybreak.unity"},
   target=BuildTarget.WebGL,
   locationPathName="Build/Web",
   options=BuildOptions.None
  });
  File.WriteAllText("Build/web-build-report.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\nSize: "+report.summary.totalSize+"\nTime: "+report.summary.totalTime+"\nIdentity: "+identity+"\n");
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("SKYBREAK_WEB_BUILD_FAILED");
  File.WriteAllText("Build/Web/.nojekyll","");
  Debug.Log("SKYBREAK_WEB_BUILD_OK "+identity);
 }
}
