using UnityEngine;
using System;
using System.Collections;
using System.IO;
namespace Skybreak {public partial class SkyGame {
 IEnumerator VerifyArsenal(Action<bool,string> check,string output){
  for(int ship=0;ship<3;ship++){
   Ship=ship;BeginRun();Launch();AutoFire=false;invuln=999;stageBanner=dialogClock=toastClock=0;spawnClock=999;
   float cadence=ShotCadence;SwitchWeaponMode();check(WeaponMode==1&&Mathf.Abs(cadence-ShotCadence)>.04f&&Weapon==ship,"Airframe "+ship+" owns two distinct firing modes");SwitchWeaponMode();
   for(int mode=0;mode<2;mode++){
    ClearBattle();WeaponMode=mode;var target=SpawnEnemy(2,new Vector3(0,1,2),0);target.hp=target.maxHp=1000;float hp=target.hp;
    var flank=SpawnEnemy(0,new Vector3(2,1,2),0);flank.hp=flank.maxHp=1000;float fh=flank.hp;
    var rear=SpawnEnemy(0,new Vector3(0,1,6),0);rear.hp=rear.maxHp=1000;float rh=rear.hp;
    Shoot(true);if(ship==0&&mode==1)check(Bullets.Exists(b=>b.kind==2&&b.homing),"Kestrel pursuit mode emits homing projectiles");
    if(ship==1&&mode==0)check(Bullets.FindAll(b=>b.kind==3).Count==7,"Manta fires seven short-lived shrapnel pellets");
    if(ship==2)check(target.hp<hp&&rear.hp<rh&&railTraces.Count>0,"Needle mode "+mode+" pierces two separated targets with rail trace");
    yield return new WaitForSeconds(.34f);check(target.hp<hp||Bullets.Exists(b=>b.friendly),"Airframe "+ship+" mode "+mode+" produces live attack");
    Shoot(false);yield return new WaitForSeconds(.08f);ScreenCapture.CaptureScreenshot(Path.Combine(output,"arsenal-"+ship+"-"+mode+".png"));yield return new WaitForSeconds(.25f);
   }
  }
  Ship=1;BeginRun();Launch();AutoFire=false;ClearBattle();var direct=SpawnEnemy(2,new Vector3(0,1,0),0);direct.hp=1000;var nearby=SpawnEnemy(0,new Vector3(1.7f,1,0),0);nearby.hp=1000;var far=SpawnEnemy(0,new Vector3(5,1,0),0);far.hp=1000;
  AddBullet(new Vector3(0,1,-2),Vector3.forward*20,true,3,20,.2f,kind:4);UpdateBullets(.1f);check(direct.hp<1000&&nearby.hp<1000&&far.hp==1000,"Manta rocket damages nearby targets and respects splash radius");
  ClearBattle();Ship=0;BeginRun();Launch();AutoFire=false;
 }
}}
