using UnityEngine;
using System;
using System.Collections;
using System.IO;

namespace Skybreak {
public partial class SkyGame {
 bool portraitCaptureGrid;
 float portraitCaptureTime=-1;

 // Opt-in visual capture uses the shipping portrait renderer and never saves
 // progress or preferences. It closes itself after producing the preview.
 IEnumerator CapturePortraitMotion() {
  qaRunning=true;Application.runInBackground=true;AudioListener.volume=0;
  QualitySettings.vSyncCount=0;Application.targetFrameRate=60;
  Screen.SetResolution(1600,900,FullScreenMode.Windowed);
  string output=Path.Combine(Directory.GetCurrentDirectory(),"PortraitPreview");
  foreach(string arg in Environment.GetCommandLineArgs())
   if(arg.StartsWith("--sky-output="))output=arg.Substring(13);
  Directory.CreateDirectory(output);Directory.CreateDirectory(Path.Combine(output,"Frames"));
  yield return new WaitForSecondsRealtime(1);
  InitPortraits();
  if(!portraitMats[0].shader.isSupported){Debug.LogError("Portrait shader unsupported");Application.Quit(2);yield break;}
  for(int pilot=0;pilot<3;pilot++) {
   SelectShip(pilot);menuPage=0;portraitCaptureTime=1.15f;
   yield return new WaitForSecondsRealtime(.25f);
   ScreenCapture.CaptureScreenshot(Path.Combine(output,"hangar-"+pilot+".png"));
   yield return new WaitForSecondsRealtime(.2f);
   menuPage=4;portraitCaptureTime=3.25f;
   yield return new WaitForSecondsRealtime(.2f);
   ScreenCapture.CaptureScreenshot(Path.Combine(output,"dossier-"+pilot+".png"));
   yield return new WaitForSecondsRealtime(.2f);
  }
  portraitCaptureGrid=true;menuPage=0;
  // Include every gesture and the loop boundary, not only the breathing cycle.
  for(int frame=0;frame<384;frame++) {
   portraitCaptureTime=frame/24f;
   yield return new WaitForEndOfFrame();
   var capture=ScreenCapture.CaptureScreenshotAsTexture();
   File.WriteAllBytes(Path.Combine(output,"Frames",frame.ToString("D4")+".jpg"),capture.EncodeToJPG(90));
   Destroy(capture);
   yield return null;
  }
  File.WriteAllText(Path.Combine(output,"capture-report.txt"),
   "SKYBREAK "+Application.version+" native player portrait capture\n"+
   "3 hangars, 3 dossiers, 384 frames at 24 fps (16 seconds).\n"+
   "All three profiles use the shipping material, pose evaluator, and portrait draw path.\n"+
   "Clock override is active only in this opt-in capture mode.\n"+
   "Face crops reduce sway and suppress torso secondary motion.\n"+
   "PlayerPrefs writes disabled; audio muted; application exits after capture.\n");
  WriteVerificationIdentity(output,"portrait");
  Debug.Log("SKYBREAK_PORTRAIT_CAPTURE_COMPLETE");Application.Quit();
 }
 void DrawPortraitMotionPreview() {
  Rect(0,0,1600,900,new Color(.012f,.029f,.045f,1));
  Label("SKYBREAK  /  PILOT IDLE MOTION",50,22,1050,35,20,accent);
  Label(Application.version,1410,22,130,35,16,muted,TextAnchor.MiddleRight);
  for(int pilot=0;pilot<3;pilot++) {
   float x=32+pilot*526;
   Portrait(pilot,x+16,75,476,714,false,true);
   Line(x+25,795,451,new Color(.17f,.35f,.41f));
   Label(PilotNames[pilot]+"  /  "+PilotAges[pilot],x+25,812,468,31,18,accent);
   Label("轻转 · 俯身 · 自然呼吸 · 柔和随动",x+25,852,468,28,15,muted);
  }
 }
}
}
