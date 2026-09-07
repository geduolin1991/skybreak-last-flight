#if UNITY_WEBGL && !UNITY_EDITOR
using UnityEngine;
using System.Runtime.InteropServices;

namespace Skybreak {
public partial class SkyGame {
 [DllImport("__Internal")] static extern void SkyWebReport(string json);
 [System.Serializable] class WebSnapshot {
  public string state,music;public int ship,weapon,stage,hull,bombs,kills,enemies,bullets;
  public float stageTime,fps,musicTime,bossHp;public bool musicPlaying,nova;
 }
 // The HTML start button supplies the user gesture needed by browser audio.
 public void WebActivate() {
  WebGLInput.captureAllKeyboardInput=false;
  Application.targetFrameRate=60;
  if(music&&music.clip&&!music.isPlaying)RestoreSoundtrack(false);
 }
 public void WebFocusLost(){SaveSettings();if(State==FlightState.Playing)Pause();}
 // Read-only diagnostics used to verify the same public browser build.
 public void WebStatus() {
  SkyWebReport(JsonUtility.ToJson(new WebSnapshot {
   state=State.ToString(),ship=Ship,weapon=WeaponMode,stage=Stage,hull=Hull,bombs=Bombs,
   kills=Kills,enemies=Enemies.Count,bullets=Bullets.Count,stageTime=StageTime,fps=smoothedFps,
   music=music&&music.clip?music.clip.name:"",musicTime=music?music.time:0,
   musicPlaying=music&&music.isPlaying,bossHp=Boss!=null?Boss.hp:0,nova=NovaActive
  }));
 }
}
}
#endif
