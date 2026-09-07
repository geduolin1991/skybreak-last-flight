using UnityEngine;
using System;
using System.Collections;
using System.IO;
namespace Skybreak {public partial class SkyGame {
 IEnumerator VerifySetpieces(Action<bool,string> check,string output){
  Ship=0;BeginRun();Launch();AutoFire=false;ClearBattle();invuln=999;spawnClock=999;stageBanner=dialogClock=toastClock=0;
  var near=SpawnEnemy(2,PlayerPos+Vector3.forward*5,0);near.hp=near.maxHp=2000;var far=SpawnEnemy(2,PlayerPos+new Vector3(3,0,16),0);far.hp=far.maxHp=2000;
  UseBomb();check(NovaActive&&novaMaterial.shader.isSupported&&bombFlash<.1f,"Nova uses supported world plasma shader instead of a full-screen flash");
  int stock=Bombs;UseBomb();check(Bombs==stock,"Active nova prevents duplicate stock consumption");
  float deadline=Time.realtimeSinceStartup+1;while(!novaDetonated&&Time.realtimeSinceStartup<deadline)yield return null;check(novaDetonated,"Nova charge transitions to detonation");ScreenCapture.CaptureScreenshot(Path.Combine(output,"nova-01-detonation.png"));
  yield return new WaitForSeconds(.21f);check(near.hp<2000&&far.hp==2000,"Expanding shock front hits near targets before distant targets");ScreenCapture.CaptureScreenshot(Path.Combine(output,"nova-02-wavefront.png"));
  yield return new WaitForSeconds(.5f);check(far.hp<2000,"Shock front reaches distant targets");float hp=far.hp;ScreenCapture.CaptureScreenshot(Path.Combine(output,"nova-03-aftermath.png"));yield return new WaitForSeconds(.6f);check(Mathf.Abs(far.hp-hp)<.01f,"Nova damages a target only once");
  yield return new WaitForSeconds(.5f);check(!NovaActive&&Post.Ripple==Vector4.zero,"Nova cleans up visual distortion and world field");
  for(int stage=0;stage<3;stage++){
   Stage=stage;BeginStage();Launch();ClearBattle();spawnClock=999;StageTime=116;bossSpawned=true;SpawnBoss();Boss.pos=new Vector3(0,1,8.2f);Boss.go.transform.position=Boss.pos;stageBanner=dialogClock=toastClock=0;
   check(BossModuleCount==(stage==2?3:2),"Boss "+stage+" spawns destructible weapon modules");float before=Boss.hp;DamageEnemy(Boss,100);check(Mathf.Abs(before-Boss.hp-28)<.1f,"Boss "+stage+" shield reduces core damage");
   if(stage>0)check(bossMechanisms&&bossMechanisms.Count==(stage==1?2:6),"Boss "+stage+" retains Blender mechanical pivots");
   float angle=bossMechanisms.MotionAngle;yield return new WaitForSeconds(.25f);if(stage>0)check(Mathf.Abs(bossMechanisms.MotionAngle-angle)>.1f,"Boss "+stage+" animates authored mechanical joints");ScreenCapture.CaptureScreenshot(Path.Combine(output,"boss-"+stage+"-armored.png"));yield return new WaitForSeconds(.08f);
   foreach(var module in Enemies.ToArray())if(module.owner==Boss)DamageEnemy(module,10000);
   check(BossModuleCount==0&&Boss.coreExpose>7,"Boss "+stage+" loses modules and exposes core");before=Boss.hp;DamageEnemy(Boss,100);check(Mathf.Abs(before-Boss.hp-165)<.1f,"Boss "+stage+" exposed core takes bonus damage");
   long emitted=enemyRoundsSpawned;yield return new WaitForSeconds(3.2f);check(enemyRoundsSpawned>emitted,"Boss "+stage+" named attack sequence fires live projectiles");ScreenCapture.CaptureScreenshot(Path.Combine(output,"boss-"+stage+"-core-open.png"));
   yield return new WaitForSeconds(.2f);DamageEnemy(Boss,100000);check(Enemies.TrueForAll(e=>e.owner==null),"Boss defeat removes owned modules");
  }
  Ship=0;BeginRun();Launch();AutoFire=false;
 }
}}
