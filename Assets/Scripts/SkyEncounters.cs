using UnityEngine;
using System;

namespace Skybreak {
public partial class SkyGame {
 // Combat time excludes briefings and pauses. Later chapters build on the
 // same readable rhythm, while their objectives and formations change.
 static readonly float[][] chapterBeats={
  new[]{0f,12,30,48,54,70,75},
  new[]{0f,14,34,58,64,90,95},
  new[]{0f,16,38,64,70,105,110}
 };
 static readonly string[][] beatNames={
  new[]{"接敌 · 清理先锋","救援 · 拦截袭船机","反击 · 截获指挥机","喘息 · 回收补给","突破 · 撕开封锁","利维坦正在接近","决战 · 击破海上要塞"},
  new[]{"入城 · 寻找电源","交火 · 夺回街区","反击 · 切断干扰","喘息 · 补给窗口","突围 · 穿越雷暴","暴风眼正在接近","决战 · 摧毁制空平台"},
  new[]{"接近 · 解除封锁","潜入 · 打开接点","反击 · 夺回识别链路","喘息 · 重整编队","冲刺 · 掩护上传","炽天使正在接近","决战 · 终止战争指令"}
 };
 public float BossArrivalTime=>chapterBeats[Mathf.Clamp(Stage,0,2)][6];
 float BossWarningTime=>chapterBeats[Mathf.Clamp(Stage,0,2)][5];
 float EliteArrivalTime=>chapterBeats[Mathf.Clamp(Stage,0,2)][2];
 public string EncounterLabel=>beatNames[Mathf.Clamp(Stage,0,2)][CurrentEncounterSection];
 public bool EncounterRecovery=>CurrentEncounterSection==3;
 public float BossCountdown=>Mathf.Max(0,BossArrivalTime-StageTime);
 int encounterSection=-1,beatWave;float corridorClock;
 bool bossArrived;
 int CurrentEncounterSection {get{int section=0;var beats=chapterBeats[Mathf.Clamp(Stage,0,2)];for(int i=1;i<beats.Length;i++)if(StageTime>=beats[i])section=i;return section;}}
 int ActiveAirThreats {get{int n=0;foreach(var e in Enemies)if(!e.boss&&e.owner==null&&e.kind<9&&!e.retreating)n++;return n;}}
 int AirThreatBudget=>(Difficulty==0?9:12)+Stage*2;
 float EncounterMusic=>CurrentEncounterSection==0?.10f:CurrentEncounterSection==1?.28f:CurrentEncounterSection==2?.58f:CurrentEncounterSection==3?.02f:CurrentEncounterSection==4?.68f:.02f;

 void ResetEncounterDirector(){encounterSection=-1;beatWave=0;corridorClock=0;bossArrived=false;}
 void TickEncounterDirector(float dt) {
  World.SetMissionProgress(StageTime/BossWarningTime,sideObjectiveComplete);
  if(stageEndClock>0)return;
  int section=CurrentEncounterSection;
  float speed=Stage==0?8:Stage==1?10:9;
  World.Speed=Mathf.MoveTowards(World.Speed,speed*(section==3?.68f:section==5?.55f:section==4?1.12f:1),dt*3);
  if(section==encounterSection)return;
  encounterSection=section;beatWave=0;corridorClock=3;
  if(section>0&&section<5)Toast(EncounterLabel,3.2f);
  if(section==3){
   WithdrawAirThreats();
   SpawnSupply(PlayerPos+new Vector3(-1.5f,0,5),Hull<MaxHull?0:2);
   SpawnSupply(PlayerPos+new Vector3(1.5f,0,7),2);
   comboClock=Mathf.Max(comboClock,chapterBeats[Stage][4]-StageTime+4);
  }
  if(section==5&&!bossSpawned){WithdrawAirThreats();warningClock=5;Sound("Warning",.75f);MissionSay("boss_warning_"+Stage,96,()=>Boss==null&&!bossArrived);bossSpawned=true;}
  if(section==1||section==2||section==4)spawnClock=Mathf.Min(spawnClock,.8f);
 }

 void WithdrawAirThreats(){foreach(var e in Enemies)if(!e.boss&&e.owner==null&&e.kind<9){e.retreating=true;FinishWeaponTell(e);}}
 bool TickWithdrawal(Hostile e,float dt){
  if(!e.retreating)return false;
  e.pos+=new Vector3(e.pos.x>=0?1.3f:-1.3f,0,10)*dt;
  if(e.pos.z>25){RemoveSpecialEnemy(e);return true;}
  RenderEnemyFeedback(e);e.go.transform.rotation=Quaternion.Slerp(e.go.transform.rotation,Quaternion.identity,dt*4);return true;
 }

 void TickEncounterSpawns(float dt){
  // No upper time cutoff: slow frames or a late resumed frame cannot miss a boss.
  if(StageTime>=BossArrivalTime){if(!bossArrived){bossSpawned=true;SpawnBoss();}return;}
  int section=CurrentEncounterSection;
  if(section==3||section>=5)return;
  corridorClock-=dt;
  if(section==4&&Stage>0&&corridorClock<=0){
   corridorClock=Stage==1&&missionNodes==3?12:8;
   if(Beams.Count==0&&!(Stage==2&&World.UplinkComplete)){
    float lane=Mathf.Clamp(Mathf.Round(PlayerPos.x/4)*4,-8,8);AddBeam(lane);
    if(Stage==1&&missionNodes<3&&Mathf.Abs(lane)>2)AddBeam(-lane);
   }
  }
  spawnClock-=dt;
  if(spawnClock>0||ActiveAirThreats>=AirThreatBudget)return;
  SpawnWave(waveIndex++);beatWave++;
  float spacing=section==0?4.4f:section==1?5.6f:section==2?6.5f:4.7f;
  spawnClock=spacing*(Difficulty==0?1:Difficulty==1?.88f:.78f);
 }

 void SpawnAuthoredWave(){
  int section=CurrentEncounterSection;int budget=AirThreatBudget-ActiveAirThreats;
  if(budget<=0||section==3||section>=5)return;
  float side=beatWave%2==0?-1:1;float x=side*5.8f;
  if(section==0){
   if(beatWave==1){if(Stage>0)SpawnEnemy(Stage==1?6:5,new Vector3(x,1,17),4);else SpawnEnemy(1,new Vector3(x,1,17),6);}
   else SpawnFormation(Mathf.Min(budget,3+Stage),side,beatWave%2);
  }else if(section==1){
   // Escort bombers come from the mission director; these are their cover.
   if(Stage==0)SpawnFormation(Mathf.Min(budget,3),-side,1);
   else if(beatWave%2==0)SpawnEnemy(Stage==1?6:7,new Vector3(x,1,17),4);
   else {SpawnEnemy(5,new Vector3(x*.65f,1,17),0);if(budget>1)SpawnEnemy(1,new Vector3(x*.65f+side*1.8f,1,18),4);}
  }else if(section==2){
   if(beatWave%2==0)SpawnFormation(Mathf.Min(budget,2+Stage),side,1);
   else SpawnEnemy(Stage==0?6:Stage==1?7:5,new Vector3(-x,1,18),4);
  }else {
   int kind=beatWave%3==0?5:beatWave%3==1?6:Stage==2?8:2;
   SpawnEnemy(kind,new Vector3(x*.7f,1,17),4);
   if(budget>1)SpawnEnemy(1,new Vector3(x*.7f-side*2,1,18.5f),6);
   if(Stage>0&&budget>2)SpawnEnemy(0,new Vector3(-x,1,19),6);
  }
 }
 void SpawnFormation(int count,float side,int pattern){
  for(int i=0;i<count;i++){
   float x=pattern==0?(i-(count-1)*.5f)*3:side*(6.5f-i*2.2f);
   var e=SpawnEnemy(0,new Vector3(x,1,17+i*.8f),pattern==0?0:6);e.phase=side<0?0:Mathf.PI;
  }
 }

 public string ChapterDebrief=>Stage==0?(missionResults[0]>0?"三艘救援船全部通过。船队送来下一航段的支援信标。":"海上封锁解除，救援队正在接应失能船只。我们继续前往城市。"):Stage==1?(missionResults[1]>0?"三个街区恢复供电。地面导航站将为轨道行动补充反应堆能量。":"制空平台已摧毁，未亮起的街区仍需抢修。前往天环切断指令源。"):(missionResults[2]>0?"识别链路接通，接驳船进港。战争指令终于停止了。":"核心停止攻击，地面队伍正在继续接通识别链路。");

 // Retained classic formation library for authored variants and diagnostics.
 static readonly int[][] chapterWaves={
  new[]{0,1,2,0,3,6,1,5,2,3,0,6},
  new[]{1,4,2,3,5,6,4,1,3,2,5,6},
  new[]{5,2,4,0,3,6,1,5,4,3,2,6}
 };

 void SpawnClassicChapterWave(int wave) {
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
  if(CurrentEncounterSection==4&&wave%4==0) {
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
  Panel(1352,440,222,Stage==0?153:145);
  Label(EncounterLabel,1370,456,188,28,12,EncounterRecovery?accent:muted);
  Label(Boss!=null?"击破防护节点与核心":Stage==0?"拦截袭船轰炸机":Stage==1?(sideObjectiveComplete?"保护撤离车队":"摧毁三座干扰塔"):World.UplinkComplete?"掩护接驳船进港":World.UplinkActive?"掩护识别密钥上传":"解除三个封锁接点",1370,490,188,48,19,paper);
  string detail=MissionStatus;bool eliteActive=eliteTarget!=null&&Enemies.Contains(eliteTarget);
  Label(Stage==0?"":detail,1370,551,188,26,12,sideObjectiveComplete?accent:muted);
  if(eliteActive)Bar(1370,Stage==0?590:578,186,eliteTarget.hp/eliteTarget.maxHp,Art.Orange,3);
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
  StageTime=chapterBeats[Stage][4];TickEncounterDirector(0);
  check(encounterSection==4,"Mission director enters the final approach");
  check(World.SectorRenderersAfter<World.SectorRenderersBefore,
   "Scenery batching reduces active sector renderers: "+World.SectorRenderersBefore+" -> "+World.SectorRenderersAfter);
 }
}
}
