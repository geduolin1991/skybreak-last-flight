using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace Skybreak {public partial class SkyGame {
 IEnumerator VerifyFrames113(Action<bool,string> check,string output){
  for(int ship=0;ship<3;ship++){
   Ship=ship;BeginRun();Launch();ClearBattle();AutoFire=false;missionStarted=missionResolved=true;spawnClock=999;stageBanner=dialogClock=toastClock=0;invuln=999;
   Energy=100;UseOverdrive();yield return new WaitForSeconds(.45f);ClearVoices();cutinClock=toastClock=0;
   check(frameForm.activeSelf&&!airForm.activeSelf&&FrameJoints==new[]{6,8,11}[ship],"Frame "+ship+" deploys its distinct authored assemblies");
   check(frameForm.GetComponentsInChildren<Renderer>().Length<=65,"Frame "+ship+" stays below 65 material renderers");
   var target=SpawnEnemy(2,PlayerPos+new Vector3(.72f,0,4.5f),0);target.hp=target.maxHp=5000;
   var flank=SpawnEnemy(2,PlayerPos+new Vector3(-3,0,7),0);flank.hp=flank.maxHp=5000;
   Shoot(false);bool piercingEmitted=Bullets.Exists(b=>b.friendly&&b.pierce>0),siegeEmitted=Bullets.Exists(b=>b.kind==4);yield return new WaitForSeconds(.32f);
   check(target.hp<5000,"Frame "+ship+" primary weapon damages a real target");
   if(ship==0)check(piercingEmitted,"Skyrazor emits piercing pursuit pulses alongside a blade arc");
   if(ship==1)check(frameGuard==8&&siegeEmitted,"Bastion deploys eight shield charges and siege projectiles");
   if(ship==2)check(flank.hp<5000&&FrameStrokes<=12,"Parallax prisms attack a separate flank target within the effect pool");
   Overdrive=5;ScreenCapture.CaptureScreenshot(Path.Combine(output,"113-frame-"+ship+"-combat.png"));yield return new WaitForSeconds(.18f);
   WeaponMode=1;float cadence=ShotCadence;Shoot(true);check(cadence!=new[]{.13f,.64f,.80f}[ship],"Frame "+ship+" retains a distinct alternate weapon mode");
   // A presentation close-up uses the same live model and its actual deployed pose.
   var oldPos=Cam.transform.position;float oldSize=Cam.orthographicSize;
   State=FlightState.Paused;Cam.transform.position=PlayerPos+new Vector3(0,8.66f,-5);Cam.orthographicSize=4.2f;Cam.transform.rotation=Quaternion.Euler(60,0,0);
   yield return new WaitForEndOfFrame();var rt=RenderTexture.GetTemporary(900,900,24);var oldTarget=Cam.targetTexture;Cam.targetTexture=rt;Cam.Render();var oldActive=RenderTexture.active;RenderTexture.active=rt;var texture=new Texture2D(900,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,900,900),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(output,"113-frame-"+ship+"-detail.png"),texture.EncodeToPNG());Destroy(texture);RenderTexture.active=oldActive;Cam.targetTexture=oldTarget;RenderTexture.ReleaseTemporary(rt);Cam.transform.position=oldPos;Cam.orthographicSize=oldSize;State=FlightState.Playing;
   if(ship==1){Bullets.Clear();for(int i=0;i<12;i++)AddBullet(PlayerPos+new Vector3((i%3-1)*.3f,0,1.6f),Vector3.back,false,1);UpdateBullets(.01f);check(frameGuard==0&&Bullets.Count==4,"Bastion intercepts eight frontal rounds then exhausts its finite guard");}
   Overdrive=0;yield return new WaitForSeconds(.4f);check(!frameForm.activeSelf&&airForm.activeSelf&&airForm.transform.localScale==Vector3.one,"Frame "+ship+" folds back without aircraft stretching");
   ClearBattle();check(FrameStrokes==0&&frameGuard==0,"Frame "+ship+" effects and guard clear on stage reset");
  }
  for(int stage=0;stage<3;stage++){
   Ship=0;BeginRun();Stage=stage;BeginStage();Launch();ClearBattle();AutoFire=false;invuln=999;StageTime=BossArrivalTime;bossArrived=bossSpawned=true;SpawnBoss();Boss.pos=new Vector3(0,1,8.2f);Boss.go.transform.position=Boss.pos;bossPhase=1;Boss.hp=Boss.maxHp*.60f;stageBanner=dialogClock=toastClock=0;
   if(stage==0)World.Civilians[0].health=0;else missionNodes=0;
   TickCounterplay(0);check(counterWindow==0&&!counterUsed,"Chapter "+stage+" does not grant a civilian counter without its objective");
   if(stage==0)World.Civilians[0].health=100;else if(stage==1)missionNodes=3;else World.SetUplink(1,true);
   TickCounterplay(0);check(counterWindow>=5&&counterUsed&&counterLinks.Count==3,"Chapter "+stage+" objective opens its authored counter window");
   float hp=Boss.hp;DamageEnemy(Boss,100);check(Mathf.Abs(hp-Boss.hp-165)<.1f,"Chapter "+stage+" counter bypasses active shield modules with visible vulnerability");
   ClearVoices();cutinClock=0;yield return new WaitForSeconds(.22f);ScreenCapture.CaptureScreenshot(Path.Combine(output,"113-counter-"+stage+".png"));yield return new WaitForSeconds(.15f);
   TickCounterplay(8);float remaining=counterWindow;TickCounterplay(0);check(remaining==0&&counterWindow==0,"Chapter "+stage+" counter is a single earned opening");
   hp=Boss.hp;DamageEnemy(Boss,100);check(Mathf.Abs(hp-Boss.hp-28)<.1f,"Chapter "+stage+" shield behaviour returns when its counter window closes");
   ClearBattle();check(counterLinks.Count==0&&counterWindow==0,"Chapter "+stage+" signal effects cannot leak into another map");
  }
  for(int ship=0;ship<3;ship++)check(Clip("FrameDeploy"+ship)!=null&&voiceById.ContainsKey("frame113_deploy_"+ship)&&voiceById.ContainsKey("frame113_counter_"+ship),"Frame "+ship+" has deployment foley and trilingual tactical dialogue");
  Ship=0;BeginRun();Launch();AutoFire=false;
 }
}}
