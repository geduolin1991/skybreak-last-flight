using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Skybreak;
public class SkyVoiceImporter:AssetPostprocessor {
 void OnPreprocessAudio(){
  if(!assetPath.StartsWith("Assets/Resources/Voices/Clips/"))return;
  var importer=(AudioImporter)assetImporter;var settings=importer.defaultSampleSettings;
  bool web=EditorUserBuildSettings.activeBuildTarget==BuildTarget.WebGL;settings.loadType=web?AudioClipLoadType.CompressedInMemory:AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=web?.62f:.85f;settings.preloadAudioData=true;importer.loadInBackground=true;
  importer.forceToMono=true;importer.defaultSampleSettings=settings;
 }
}
public static class SkyVoiceBuildChecks {
 public static void Check(){
  var asset=Resources.Load<TextAsset>("Voices/voice-bank");if(!asset)throw new Exception("Voice bank missing");
  var bank=JsonUtility.FromJson<SkyVoiceBank>(asset.text);if(bank.clips.Length!=141)throw new Exception("Expected 141 authored dialogue events");
  foreach(var line in bank.clips)foreach(var path in new[]{line.clip,line.clipEn,line.clipJa}){var clip=Resources.Load<AudioClip>(path);if(!clip||clip.length<.3f||clip.length>24||clip.channels!=1)throw new Exception("Voice asset invalid: "+line.id);}
  Directory.CreateDirectory("Build");File.WriteAllText("Build/voice-assets-check.txt","PASS 423 authored mono voice clips in Chinese, English and Japanese\n");Debug.Log("SKYBREAK_VOICE_ASSETS_OK");
 }
}
