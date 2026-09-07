using UnityEngine;
using System;

namespace Skybreak {
public partial class SkyGame {
 // Authored chapter identities: escort formations, urban crossfire, then
 // orbital sweeps. Short supply intervals break up the heavy encounters.
 static readonly int[][] chapterWaves={
  new[]{0,1,2,0,3,6,1,5,2,3,0,6},
  new[]{1,4,2,3,5,6,4,1,3,2,5,6},
  new[]{5,2,4,0,3,6,1,5,4,3,2,6}
 };
 static readonly string[][] missionOrders={
  new[]{"清理近海防线","护送撤离航线","截获干扰指挥机","压制最后封锁","突破海上要塞"},
  new[]{"进入都市低空","避开交叉火网","夺回城市供电","穿越雷暴走廊","摧毁制空平台"},
  new[]{"接近轨道船坞","穿越巡逻环","保护识别密钥","打开核心航路","唤醒天环核心"}
 };
 int encounterSection=-1;
 int CurrentEncounterSection=>StageTime<24?0:StageTime<52?1:StageTime<80?2:StageTime<115?3:4;

 void ResetEncounterDirector(){encounterSection=-1;}
 void TickEncounterDirector(float dt) {
  World.SetMissionProgress(StageTime,sideObjectiveComplete);
  int section=CurrentEncounterSection;
  if(section==encounterSection)return;
  encounterSection=section;
  if(section==1) {
   pilotSpeaker=(Ship+1)%3;RadioName=PilotNames[pilotSpeaker]+" / 航线引导";
   RadioText=new[]{"船队就在下方。清理它们前面的空域，我来照看侧翼。",
    "他们开始交叉锁定了。瞄准线停住后再转向，别急着交炸弹。",
    "这些是以前的维护船坞……跟着环形航标走，巡逻机正在换班。"}[Stage];dialogClock=7;
  } else if(section==3) {
   pilotSpeaker=Stage==2?2:Ship;RadioName=PilotNames[pilotSpeaker]+" / 小队通讯";
   RadioText=Stage==0?(sideObjectiveComplete?"撤离信号已连通。再向前一步，就是他们的明天。":"船队还在等待回音。先打穿要塞，我们仍有机会。"):
    Stage==1?(sideObjectiveComplete?"电网回来了！看，下方的灯正在重新亮起。":"干扰还没解除。把剩下的能量留给制空平台。"):
    sideObjectiveComplete?"密钥完整。我会让天环记起，它曾经想保护的人。":"密钥受损，但我的声音还在。让我亲口告诉它。";
   dialogClock=7;
  }
 }

 void SpawnChapterWave(int wave) {
  int stage=Mathf.Clamp(Stage,0,2),type=chapterWaves[stage][wave%chapterWaves[stage].Length];
  float offset=Mathf.Sin(wave*2.7f)*4.5f;
  if(type==0||type==5) {
   int count=4+stage;
   for(int i=0;i<count;i++) {
    float x=(i-(count-1)*.5f)*(type==0?2.7f:3.0f);
    var e=SpawnEnemy(0,new Vector3(x,1,17+Mathf.Abs(i-(count-1)*.5f)*.8f),type);
    e.phase=wave*.42f+i*.14f;
   }
  } else if(type==1||type==2) {
   int count=4+stage;float sign=type==1?-1:1;
   for(int i=0;i<count;i++) {
    var e=SpawnEnemy(0,new Vector3(sign*(7.7f-i*1.8f),1,17+i*1.18f),6);
    e.phase=type==1?0:Mathf.PI;
   }
  } else if(type==3) {
   SpawnEnemy(2,new Vector3(offset,1,18),3);
   for(int s=-1;s<=1;s+=2)SpawnEnemy(1,new Vector3(Mathf.Clamp(offset+s*3,-8,8),1,20),1);
  } else if(type==4) {
   for(int s=-1;s<=1;s+=2) {
    var e=SpawnEnemy(1,new Vector3(s*6.7f,1,18),4);e.phase=s<0?0:Mathf.PI;
    e.fire=s<0?1.9f:2.55f;
   }
  } else SpawnEnemy(3,new Vector3(offset,1,18),3);
  // Keep the elite interception readable; later sections provide the climax.
  if(StageTime>=80&&wave%4==0) {
   SpawnEnemy(2,new Vector3(-offset,1,21),4);
   if(Stage>0&&Beams.Count<2)AddBeam(Mathf.Round(PlayerPos.x/3)*3);
  }
 }

 void UpdateWeaponTell(Hostile e) {
  bool eligible=!e.boss&&(e.kind>=2||e.pattern==4)&&e.pos.z<13&&e.pos.z>PlayerPos.z+1;
  if(!eligible){e.aimReady=false;if(e.aimLine)e.aimLine.enabled=false;return;}
  if(!e.aimReady&&e.fire<=.65f) {
   e.aimReady=true;e.lockedAim=PlayerPos;
   // Even enemies held offscreen until their timer expires must telegraph.
   e.fire=Mathf.Max(e.fire,.65f);
  }
  if(!e.aimReady)return;
  if(!e.aimLine) {
   e.aimLine=Art.Line(e.go.transform,"Weapon lock",new[]{e.pos,e.lockedAim},Art.Orange,.045f);
   var shader=Shader.Find("Skybreak/Telegraph");
   if(shader)e.aimLine.sharedMaterial=e.go.AddComponent<SkyOwnedResources>().Keep(new Material(shader));
   e.aimLine.useWorldSpace=true;
  }
  e.aimLine.enabled=true;
  Vector3 direction=e.lockedAim-e.pos;direction.y=0;
  e.aimLine.SetPosition(0,e.pos+Vector3.back*.6f);
  e.aimLine.SetPosition(1,e.pos+direction.normalized*24);
  float alpha=.13f+(1-Mathf.Clamp01(e.fire/.65f))*.24f;
  Color color=new Color(1,.47f,.17f,alpha);
  e.aimLine.startColor=e.aimLine.endColor=color;
 }
 void FinishWeaponTell(Hostile e){e.aimReady=false;if(e.aimLine)e.aimLine.enabled=false;}

 void DrawMissionOrder() {
  if(State!=FlightState.Playing||stageBanner>0||warningClock>1.7f)return;
  Panel(1352,440,222,145);
  Label("当前目标",1370,456,188,24,13,muted);
  Label(missionOrders[Stage][CurrentEncounterSection],1370,490,188,48,19,paper);
  string detail=CurrentEncounterSection==2?(sideObjectiveComplete?"支援链路已恢复":"可选 · 截获指挥机"):
   CurrentEncounterSection==4?"注意阶段转换与激光预警":"航线 "+Mathf.Min(100,Mathf.FloorToInt(StageTime/115*100))+"%";
  bool eliteActive=eliteTarget!=null&&Enemies.Contains(eliteTarget);
  if(eliteSent&&CurrentEncounterSection<4)
   detail=sideObjectiveComplete?"支援链路已恢复":eliteActive?"支线交战中 · 优先压制":"支线已错失 · 继续前进";
  Label(detail,1370,551,188,26,12,sideObjectiveComplete?accent:muted);
  if(eliteActive)Bar(1370,578,186,eliteTarget.hp/eliteTarget.maxHp,Art.Orange,3);
 }

 // Integration checks exercise the actual director and live target lock.
 void VerifyEncounterRules(Action<bool,string> check) {
  AutoFire=false;ClearBattle();StageTime=0;
  var enemy=SpawnEnemy(2,new Vector3(2,1,8),4);enemy.fire=.01f;
  UpdateWeaponTell(enemy);Vector3 locked=enemy.lockedAim;
  check(enemy.aimReady&&enemy.fire>=.64f&&enemy.aimLine&&enemy.aimLine.enabled,
   "Heavy enemy gives a visible 0.65 second firing warning");
  PlayerPos+=Vector3.right*2;UpdateWeaponTell(enemy);
  check(enemy.lockedAim==locked,"Telegraphed aim remains locked while player moves");
  EnemyAttack(enemy);FinishWeaponTell(enemy);
  check(Bullets.Exists(b=>!b.friendly)&&!enemy.aimLine.enabled,"Locked attack fires and clears its telegraph");
  ClearBattle();StageTime=24;TickEncounterDirector(0);
  check(encounterSection==1&&RadioText.Length>0,"Mission director advances to the second chapter beat");
  StageTime=80;TickEncounterDirector(0);
  check(encounterSection==3,"Mission director enters the final approach");
  check(World.SectorRenderersAfter<World.SectorRenderersBefore,
   "Scenery batching reduces active sector renderers: "+World.SectorRenderersBefore+" -> "+World.SectorRenderersAfter);
 }
}
}
