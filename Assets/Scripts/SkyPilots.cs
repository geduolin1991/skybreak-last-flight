using UnityEngine;
using System;
using System.Collections.Generic;
namespace Skybreak {
public partial class SkyGame {
 public string[] PilotNames={"苍凛","绯音","雪璃"};public string[] PilotCallsigns={"AOI / BLUE DAWN","RIN / CRIMSON HEART","YUKI / ZERO HOUR"};public string[] SkillNames={"苍蓝协奏","绯焰舰装","零时领域"};public string[] PilotAges={"24 岁 · 第七飞行队队长","27 岁 · 舰装工程师","25 岁 · 天环前导航员"};
 string[] PilotBios={"在海岸救援行动中失去过整支小队。她把每一次起飞都当成与同伴的约定：带所有人回家。冷静之外，是从未放弃的温柔。","从小在海上工厂长大，能听出每一台引擎的故障。她用玩笑掩饰担心，驾驶自己改装的重型战机，为队友挡下最密集的火力。","曾参与天环的导航系统研发。战争指令失控后，她带着唯一的人类识别密钥离开轨道。她要亲口告诉自己的造物：保护不是囚禁。"};
 string[] PilotMottos={"“约好了，一起看见明天的天空。”","“放心，有我在，火力管够！”","“如果计算里没有希望……就重新计算。”"};
 Texture2D[] portraits=new Texture2D[3],blinks=new Texture2D[3];Material[] portraitMats=new Material[3];public float SkillCooldown,SkillTime;bool cutinIsOverdrive;float cutinClock,featureClock,toastClock;string toastText="";int pilotSpeaker;int[] pilotXp=new int[3],pilotBest=new int[3];int salvage,totalRuns,rescueSignals,earnedSalvage,earnedXp;bool rewardSaved;public int[] UpgradeOptions={0,1,2};int wingmen,hunters,focusLevel,leechLevel,critLevel,temporalLevel;bool eliteSent,sideObjectiveComplete;Hostile eliteTarget;
 readonly List<Shock> shocks=new List<Shock>();
 class Shock {public LineRenderer line;public float age,max,radius;}
 string[] UpgradeNames={"火力校准","复合装甲","临界反应堆","双生僚机","猎手导引","精准放大器","战场回收","熔核弹头","时间晶格"};
 string[] UpgradeDescriptions={"所有武器伤害 +15%\n射击速度 +12%\n苍隼追加侧翼炮弹","最大装甲 +2\n立即修复 3 点装甲\n提高持续作战能力","充能效率提升\n超载持续时间 +1 秒\n补充 1 枚炸弹与 50 能量","真实双机支援延长 5 秒\n僚机伤害提升 15%\n拾取金色支援信标召集","导引副武器伤害 +60%\n导弹发射更频繁\n优先歼灭重型目标","精准飞行时伤害 +35%\n适合收束火力击破 Boss\n不改变正常移动速度","每击破 18 架敌机\n自动修复 1 点装甲\n立即修复 1 点装甲","25% 起的概率触发双倍伤害\n多级强化提高暴击概率\n所有主武器均可触发","驾驶员技能冷却 -20%\n立即刷新技能\n与角色固有能力协同"};
 void LoadPilotProgress(){LoadResearch();for(int i=0;i<3;i++){pilotXp[i]=PlayerPrefs.GetInt("pilot-xp-"+i);pilotBest[i]=PlayerPrefs.GetInt("pilot-best-"+i);}salvage=PlayerPrefs.GetInt("salvage");totalRuns=PlayerPrefs.GetInt("runs");}
 void SavePilotProgress(){if(qaRunning)return;for(int i=0;i<3;i++){PlayerPrefs.SetInt("pilot-xp-"+i,pilotXp[i]);PlayerPrefs.SetInt("pilot-best-"+i,pilotBest[i]);}PlayerPrefs.SetInt("salvage",salvage);PlayerPrefs.SetInt("runs",totalRuns);PlayerPrefs.Save();}
 int PilotLevel(int pilot)=>Mathf.Min(10,1+pilotXp[pilot]/100000);
 void RewardPilot(){if(rewardSaved)return;rewardSaved=true;earnedSalvage=Mathf.Max(25,Score/250)+(State==FlightState.Victory?300:0);earnedXp=Score;if(qaRunning)return;salvage+=earnedSalvage;pilotXp[Ship]+=Score;pilotBest[Ship]=Mathf.Max(pilotBest[Ship],Score);totalRuns++;SavePilotProgress();}
 void ResetFeatures(){toastClock=0;toastText="";earnedSalvage=earnedXp=0;hitStop=0;SkillCooldown=0;SkillTime=0;cutinClock=0;wingmen=hunters=focusLevel=leechLevel=critLevel=temporalLevel=0;rewardSaved=false;rescueSignals=0;sideObjectiveComplete=false;eliteSent=false;featureClock=0;}
 void ResetStageFeatures(){SkillTime=0;eliteSent=false;sideObjectiveComplete=false;eliteTarget=null;SkillCooldown=Mathf.Max(0,SkillCooldown-8);pilotSpeaker=Ship;ClearShocks();}
 void TickFeatures(float dt){SkillCooldown=Mathf.Max(0,SkillCooldown-dt);SkillTime=Mathf.Max(0,SkillTime-dt);cutinClock=Mathf.Max(0,cutinClock-dt);toastClock=Mathf.Max(0,toastClock-dt);featureClock+=dt;
 for(int i=shocks.Count-1;i>=0;i--){var s=shocks[i];s.age+=dt;float t=s.age/s.max;if(t>=1){ReturnShock(s);shocks.RemoveAt(i);continue;}s.line.transform.localScale=Vector3.one*Mathf.Lerp(.1f,s.radius,1-Mathf.Pow(1-t,3));s.line.startWidth=s.line.endWidth=(1-t)*.075f;}
 if(!eliteSent&&StageTime>=52&&StageTime<115&&stageEndClock==0){eliteSent=true;eliteTarget=SpawnEnemy(8,new Vector3(0,1,18),4);eliteTarget.elite=true;eliteTarget.hp=eliteTarget.maxHp=620+Stage*180;eliteTarget.go.transform.localScale=Vector3.one*.75f;MissionSay("elite_arrive",82,()=>eliteTarget!=null&&Enemies.Contains(eliteTarget));Toast("可选目标 · 截获干扰指挥机",4);Sound("Warning",.5f);}
 }
 void Toast(string text,float time=2.5f){toastText=text;toastClock=time;}
 void Shockwave(Vector3 pos,Color color,float radius=6,float duration=.55f){if(shocks.Count>=64)return;var s=shockPool.Count>0?shockPool.Pop():CreateShock();s.age=0;s.max=duration;s.radius=radius;var l=s.line;l.sharedMaterial=Art.GlowMat(color);l.transform.position=pos+Vector3.up*.05f;l.transform.localScale=Vector3.one*.1f;l.startWidth=l.endWidth=.075f;l.gameObject.SetActive(true);shocks.Add(s);}
 public void UsePilotSkill(){if(State!=FlightState.Playing||SkillCooldown>0||stageEndClock>0)return;SkillCooldown=(Ship==0?18:Ship==1?22:24)*Mathf.Pow(.8f,temporalLevel)*(1-researchSkill*.05f);SkillTime=Ship==0?2:Ship==1?5:4.5f;cutinClock=1.65f;cutinIsOverdrive=false;invuln=Mathf.Max(invuln,1.2f);Sound("Overdrive",.8f);QueueVoiceId("route_skill_"+Ship+"_"+CurrentRoute,80);shake=.28f;Shockwave(PlayerPos,Ship==1?Art.Orange:Ship==2?Art.Violet:Art.Cyan,8);pilotSpeaker=Ship;
 ApplyRouteSkill();
 }
 void RewardElite(Hostile e){if(e.elite){eliteTarget=null;Score+=3500;SpawnSupply(e.pos,3);SpawnSupply(e.pos+Vector3.right,0);Energy=Mathf.Min(100,Energy+30);Toast("指挥机击破  +3,500  /  双机补给已投放",4);MissionSay("elite_down",80,null,false);}if(leechLevel>0&&Kills%18==0){Hull=Mathf.Min(MaxHull,Hull+leechLevel);Toast("战场回收 · 装甲已修复");}}
 void RollUpgrades(){var bag=new List<int>();for(int i=0;i<9;i++)bag.Add(i);var r=new System.Random(Environment.TickCount);for(int i=0;i<3;i++){int pick=r.Next(bag.Count);UpgradeOptions[i]=bag[pick];bag.RemoveAt(pick);}}
 public void ApplyFeatureUpgrade(int slot){int n=UpgradeOptions[slot];if(n==0){upgradeFire++;DamageBoost+=.15f;}else if(n==1){upgradeArmor+=2;Hull=Mathf.Min(MaxHull,Hull+3);}else if(n==2){upgradeEnergy++;Energy=Mathf.Min(100,Energy+50);Bombs=Mathf.Min(5,Bombs+1);}else if(n==3)wingmen++;else if(n==4)hunters++;else if(n==5)focusLevel++;else if(n==6){leechLevel++;Hull=Mathf.Min(MaxHull,Hull+1);}else if(n==7)critLevel++;else if(n==8){temporalLevel++;SkillCooldown=0;}}
 readonly float[] portraitShownAt=new float[3];
 readonly int[] portraitLastFrame={-2,-2,-2};
 void InitPortraits(){
  if(portraitMats[0])return;
  for(int i=0;i<3;i++){
   string name=new[]{"Aoi","Rin","Yuki"}[i];
   portraits[i]=Resources.Load<Texture2D>("Pilots/"+name);
   blinks[i]=Resources.Load<Texture2D>("Pilots/"+name+"Blink");
   portraitMats[i]=new Material(Shader.Find("Skybreak/Pilot"));
   portraitMats[i].SetTexture("_BlinkTex",blinks[i]?blinks[i]:portraits[i]);
   SkyPortraitRig.Configure(portraitMats[i],i);
  }
 }
 void Portrait(int pilot,float x,float y,float w,float h,bool face=false,bool fade=true){
  if(Event.current.type!=EventType.Repaint)return;
  InitPortraits();if(!portraits[pilot])return;
  if(!face){
   if(portraitLastFrame[pilot]<Time.frameCount-1)portraitShownAt[pilot]=Time.unscaledTime;
   portraitLastFrame[pilot]=Time.frameCount;
  }
  float clock=portraitCaptureTime>=0?portraitCaptureTime:Time.unscaledTime;
  float age=portraitCaptureTime>=0?portraitCaptureTime:Time.unscaledTime-portraitShownAt[pilot];
  var m=portraitMats[pilot];SkyPortraitRig.Animate(m,pilot,clock,age,face,fade);
  Graphics.DrawTexture(new Rect(x,y,w,h),portraits[pilot],m);
 }
 void DrawRadio(){
  bool enemyFallback=commanderRadio!=null&&commanderRadioClock>0;
  if(dialogClock<=0&&!VoiceRadioActive&&!enemyFallback)return;
  var entry=VoiceRadioActive?voiceCurrent:enemyFallback?commanderRadio:null;
  int speaker=entry!=null?entry.pilot:pilotSpeaker;string line=entry!=null?entry.text:RadioText;
  string name=entry!=null?SpeakerName(speaker)+(speaker<3?" / 小队通讯":""):RadioName;
  Panel(26,318,222,287);
  if(speaker>=3)DrawCommanderPortrait(speaker,34,325,205,141,entry!=null&&entry.id.EndsWith("death")?2:entry!=null&&entry.id.EndsWith("entry")?0:1);
  else Portrait(speaker,34,325,205,141,true,false);
  Rect(26,462,222,143,new Color(.004f,.009f,.016f,.98f));Label(name,41,475,193,27,13,speaker>=3?Art.Orange:accent);
  // Stable whole captions avoid allocating a substring and rebuilding glyphs every frame.
  Label(line,41,513,190,83,15,paper);
 }

 void DrawFeatures(){if(State==FlightState.Playing){Panel(1352,289,222,124);Label("驾驶员技  [Q]",1370,304,190,24,13,muted);Label(ActiveSkillName,1370,338,190,31,21,paper);Label(SkillCooldown>0?Mathf.CeilToInt(SkillCooldown)+" 秒":"READY TO CAST",1371,382,187,23,12,SkillCooldown>0?muted:accent);DrawRadio();if(toastClock>0)Label(toastText,432,180,736,40,18,accent,TextAnchor.MiddleCenter);}
 if(cutinClock>0&&State==FlightState.Playing){float a=Mathf.Clamp01(cutinClock/.25f);float enter=Mathf.Clamp01((1.65f-cutinClock)*5);float x=Mathf.Lerp(1700,1010,1-Mathf.Pow(1-enter,3));Rect(x,490,553,144,new Color(.005f,.012f,.026f,.9f*a));Portrait(Ship,x+175,420,368,250,true);Label("LINK BURST",x+22,508,284,26,12,accent);Label(cutinIsOverdrive?"天翼机甲展开":ActiveSkillName,x+22,550,342,49,29,paper);Line(x+22,617,480,accent);}
 }
 void DrawDossier(){Panel(318,99,965,733);Portrait(Ship,776,109,486,723);GUI.DrawTexture(new Rect(318,99,794,733),gradient);SmallTag("PILOT ARCHIVE / 0"+(Ship+1),351,125,250,accent);Label(PilotNames[Ship],350,182,507,83,53,paper);Label(PilotAges[Ship],352,278,459,35,16,accent);Label(PilotMottos[Ship],352,339,478,67,22,paper);Label(PilotBios[Ship],352,432,433,131,17,muted);Label("羁绊等级  "+PilotLevel(Ship)+"  /  10",352,582,445,38,20,paper);Bar(354,640,396,PilotLevel(Ship)>=10?1:(pilotXp[Ship]%100000)/100000f,accent,5);Label("专属最高分  "+pilotBest[Ship].ToString("N0"),352,672,422,30,16,muted);Label("Lv.3 初始能量 +10  ·  Lv.6 装甲上限 +1",352,721,459,41,14,accent);if(Button("试听驾驶员声线",42,609,226,55,true))PreviewPilotVoice();if(Button("机体研发  →",42,686,226,55,true))menuPage=5;if(Button("← 返回机库",42,765,226,55))menuPage=0;}
}
}
