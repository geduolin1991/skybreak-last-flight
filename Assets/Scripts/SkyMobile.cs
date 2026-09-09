using System;
using UnityEngine;
namespace Skybreak {
public partial class SkyGame {
 public bool MobileMode {get;private set;}
 float desktopShadowDistance;int desktopShadowCascades,desktopVSync,desktopFrameRate;
 bool mobileFocus;Vector2 mobileMove;float mobileInputAt;Rect mobilePortraitRect;Rect mobileBattleViewport=new Rect(0,0,1,1);string mobilePage="home";
 int mobilePortraitSpeaker,mobilePortraitExpression;bool mobileRadioPresentation;
 [Serializable] class MobileInput {public float x,y;}
 [Serializable] class MobilePortraitBounds {public float x,y,w,h;public int speaker,expression;public bool radio;}
 [Serializable] public class MobileChoice {public string name,description,detail;public bool selected,enabled=true;public int index;}
 [Serializable] public class MobileSnapshot {
  public string eventMessage,routeNode,routeNext,supportPilots;public float routeProgress,coastInset;public int shields,portraitSpeaker,portraitExpression;public Rect battleViewport,portraitBounds;public bool portraitReady;
  public string encounter,debrief;public float stageTime,bossArrival;public string state,page,pilot,age,bio,motto,ship,shipDescription,skill,weapon,route,stage,boss,mission,radioName,radioText,voiceLanguage,voiceId,commanderEvent;
  public int shipIndex,difficulty,hull,maxHull,bombs,score,highScore,combo,salvage,level,unlocked,stageIndex,routeTier,voiceReady,speaker,expression,kills,maxCombo,earnedSalvage;
  public float energy,overdrive,skillCooldown,bossHealth,progress,masterVolume,voiceVolume,moveX,moveY,playerX,playerZ;
  public bool mobile,focus,voiceEnabled,musicEnabled,shakeEnabled,voicePlaying,nova,portrait,radio,canSkill,canBomb,canOverdrive;
  public string bossInstruction;public int bossModules;public bool bossTelegraph;public int wingmen;public float supportSeconds;public float cameraAspect,cameraSize;public Vector2 playerViewport;public Vector2 flightMin,flightMax;public float[] convoy; public MobileChoice[] routes,upgrades,research;
 }
 public void WebMobileMode(string mode){
  bool next=mode=="1";if(next&&!MobileMode){desktopShadowDistance=QualitySettings.shadowDistance;desktopShadowCascades=QualitySettings.shadowCascades;desktopVSync=QualitySettings.vSyncCount;desktopFrameRate=Application.targetFrameRate;}
  if(!next&&MobileMode){QualitySettings.shadowDistance=desktopShadowDistance;QualitySettings.shadowCascades=desktopShadowCascades;QualitySettings.vSyncCount=desktopVSync;Application.targetFrameRate=desktopFrameRate;}
  MobileMode=next;ResetMobileInput();mobileFocus=false;if(!next)mobileBattleViewport=new Rect(0,0,1,1);
  if(Post)Post.MobileQuality=MobileMode;
  // The sea and city sit roughly 48 units from the camera. The old 35-unit
  // cutoff removed their shadows completely, even in the normal mobile view.
  if(MobileMode){QualitySettings.vSyncCount=0;Application.targetFrameRate=60;QualitySettings.shadowDistance=65;QualitySettings.shadowCascades=2;}
 }
 public void WebMobileMove(string json){
  if(!MobileMode||State!=FlightState.Playing){ResetMobileInput();return;}
  try{var input=JsonUtility.FromJson<MobileInput>(json);if(input==null||float.IsNaN(input.x)||float.IsNaN(input.y)||float.IsInfinity(input.x)||float.IsInfinity(input.y)){ResetMobileInput();return;}mobileMove=Vector2.ClampMagnitude(new Vector2(input.x,input.y),1);mobileInputAt=Time.unscaledTime;}catch(ArgumentException){ResetMobileInput();}
 }
 void ResetMobileInput(){mobileMove=Vector2.zero;mobileInputAt=-1;}
 Vector2 MobileMovement=>State==FlightState.Playing&&Time.unscaledTime-mobileInputAt<.35f?mobileMove:Vector2.zero;
 public void WebMobilePage(string page){if(page=="home"||page=="routes"||page=="settings"||page=="dossier"||page=="research"||page=="chapters"||page=="guide")mobilePage=page;}
 public void WebMobilePortrait(string json){try{var r=JsonUtility.FromJson<MobilePortraitBounds>(json);if(r!=null&&FiniteBounds(r)){mobilePortraitRect=new Rect(r.x,r.y,Mathf.Clamp01(r.w),Mathf.Clamp01(r.h));mobilePortraitSpeaker=Mathf.Clamp(r.speaker,0,5);mobilePortraitExpression=Mathf.Clamp(r.expression,0,2);mobileRadioPresentation=r.radio;}}catch(ArgumentException){}}
 public void WebMobileViewport(string json){
  try{var r=JsonUtility.FromJson<MobilePortraitBounds>(json);if(r==null||!FiniteBounds(r)||r.x<0||r.y<0||r.w<.1f||r.h<.1f||r.x+r.w>1.001f||r.y+r.h>1.001f)return;
   mobileBattleViewport=new Rect(r.x,r.y,r.w,r.h);ApplyViewport();
  }catch(ArgumentException){}
 }
 static bool FiniteBounds(MobilePortraitBounds r)=>!float.IsNaN(r.x+r.y+r.w+r.h)&&!float.IsInfinity(r.x+r.y+r.w+r.h);
 public void WebMobileAction(string action){
  if(!MobileMode||string.IsNullOrEmpty(action))return;
  if(action=="release"){ResetMobileInput();return;}
  if(action=="pause"){ResetMobileInput();if(State==FlightState.Playing)Pause();return;}
  if(action=="resume"){ResetMobileInput();if(State==FlightState.Paused)Pause();return;}
  if(action=="launch"){ResetMobileInput();Launch();return;}
  if(action=="home"&&(State==FlightState.Paused||State==FlightState.Victory||State==FlightState.Defeat)){ResetMobileInput();SetupHangar();mobilePage="home";return;}
  if(action=="retry"&&(State==FlightState.Paused||State==FlightState.Victory||State==FlightState.Defeat)){ResetMobileInput();if(Endless)BeginEndless();else BeginRun(Practice);return;}
  if(State==FlightState.Playing){switch(action){case "skill":UsePilotSkill();break;case "bomb":UseBomb();break;case "overdrive":UseOverdrive();break;case "weapon":SwitchWeaponMode();break;case "focus":mobileFocus=!mobileFocus;break;}return;}
  var parts=action.Split(':');int n=-1;if(parts.Length==2&&!int.TryParse(parts[1],out n))n=-1;
  if(State==FlightState.Upgrade&&parts[0]=="upgrade"&&n>=0&&n<3){ResetMobileInput();ApplyUpgrade(n);return;}
  if(State!=FlightState.Hangar)return;
  switch(parts[0]){
   case "play":ResetMobileInput();BeginRun();break;
   case "ship":if(n>=0&&n<3)SelectShip(n);break;
   case "route":if(n>=0&&n<3)SelectSpecialization(n);break;
   case "difficulty":if(n>=0&&n<3){Difficulty=n;SaveSettings();}break;
   case "language":if(n>=0&&n<3)SetVoiceLanguage(n);break;
   case "chapter":if(n>=0&&n<3&&n<=Unlocked){PracticeStage=n;ResetMobileInput();BeginRun(true);}break;
   case "endless":ResetMobileInput();BeginEndless();break;
   case "research":if(n>=0&&n<3)BuyResearch(n);break;
   case "music":MusicEnabled=!MusicEnabled;SaveSettings();break;
   case "shake":ShakeEnabled=!ShakeEnabled;SaveSettings();break;
   case "voice":VoiceEnabled=!VoiceEnabled;SaveSettings();break;
   case "volume":if(n>=0&&n<=100){MasterVolume=n/100f;SaveSettings();}break;
   case "voiceVolume":if(n>=0&&n<=100){VoiceVolume=n/100f;SaveSettings();}break;
   case "previewPilot":PreviewPilotVoice();break;
   case "previewBoss":PreviewCommanderVoice();break;
  }
 }
 void MobileRadio(out int speaker,out string name,out string text,out int expression){
  bool enemyFallback=commanderRadio!=null&&commanderRadioClock>0;
  var entry=VoicePlaying?voiceCurrent:enemyFallback?commanderRadio:null;
  speaker=entry!=null?entry.pilot:pilotSpeaker;name=entry!=null?SpeakerName(speaker):RadioName;
  text=entry!=null?entry.text:dialogClock>0?RadioText:"";
  expression=entry==null?0:entry.id.EndsWith("death")?2:entry.id.EndsWith("entry")?0:1;
 }
 public MobileSnapshot MobileStatus(){
  MobileRadio(out int speaker,out string name,out string line,out int expression);
  int radioActor=string.IsNullOrEmpty(line)?Ship:speaker;
  var s=new MobileSnapshot{eventMessage=toastClock>0?toastText:"",coastInset=World.CoastInset,routeNode=routeNodes[Ship,CurrentRoute,RouteTier-1],routeNext=RouteTier<3?routeNodes[Ship,CurrentRoute,RouteTier]:"全部专精已展开",routeProgress=RouteTier>=3?1:Mathf.Clamp01((routeExperience-(RouteTier==1?0:30))/(RouteTier==1?30f:55f)),shields=routeShield,supportPilots=string.Join(" / ",squadron.ConvertAll(w=>PilotNames[w.pilot])),battleViewport=mobileBattleViewport,portraitBounds=mobilePortraitRect,portraitReady=radioActor>=3?commanderPortraits[Mathf.Clamp(radioActor-3,0,2)]!=null:portraits[Mathf.Clamp(radioActor,0,2)]!=null,
   bossInstruction=Boss!=null?bossOrder:"",bossModules=BossModuleCount,bossTelegraph=bossTell>0,wingmen=WingmanCount,supportSeconds=SupportRemaining,cameraAspect=Cam.aspect,cameraSize=Cam.orthographicSize,playerViewport=Cam.WorldToViewportPoint(PlayerPos),flightMin=Cam.WorldToViewportPoint(new Vector3(-10,1,-10)),flightMax=Cam.WorldToViewportPoint(new Vector3(10,1,10)),encounter=EncounterLabel,debrief=ChapterDebrief,stageTime=StageTime,bossArrival=BossArrivalTime,mobile=MobileMode,state=State.ToString(),page=mobilePage,pilot=PilotNames[Ship],age=PilotAges[Ship],bio=PilotBios[Ship],motto=PilotMottos[Ship],ship=ShipNames[Ship],shipDescription=new[]{"速射主炮 / 蜂群导弹","重型破片 / 聚爆火力","高速机动 / 贯穿轨道炮"}[Ship],skill=ActiveSkillName,weapon=WeaponTitle,route=RouteName,stage=StageNames[Stage],boss=Boss!=null?BossNames[Stage]:"",mission=MissionStatus,radioName=name,radioText=line,voiceLanguage=VoiceLanguageCode,voiceId=VoiceActiveId,commanderEvent=CommanderEvent,
   shipIndex=Ship,difficulty=Difficulty,hull=Hull,maxHull=MaxHull,bombs=Bombs,score=Score,highScore=HighScore,combo=Combo,salvage=salvage,level=PilotLevel(Ship),unlocked=Unlocked,stageIndex=Stage,routeTier=RouteTier,voiceReady=VoiceClipsReady,speaker=speaker,expression=expression,kills=Kills,maxCombo=MaxCombo,earnedSalvage=earnedSalvage,
   energy=Energy,overdrive=Overdrive,skillCooldown=SkillCooldown,bossHealth=Boss!=null?Boss.hp/Boss.maxHp:0,progress=Progress,masterVolume=MasterVolume,voiceVolume=VoiceVolume,moveX=MobileMovement.x,moveY=MobileMovement.y,playerX=PlayerPos.x,playerZ=PlayerPos.z,
   focus=mobileFocus,voiceEnabled=VoiceEnabled,musicEnabled=MusicEnabled,shakeEnabled=ShakeEnabled,voicePlaying=VoicePlaying,nova=NovaActive,portrait=State==FlightState.Hangar&&(mobilePage=="home"||mobilePage=="dossier")||State==FlightState.Briefing||State==FlightState.Victory,
   radio=State==FlightState.Playing&&!string.IsNullOrEmpty(line),canSkill=State==FlightState.Playing&&SkillCooldown<=0,canBomb=State==FlightState.Playing&&Bombs>0&&stageEndClock<=0&&!NovaActive,canOverdrive=State==FlightState.Playing&&Energy>=100&&Overdrive<=0};
  s.portraitSpeaker=mobilePortraitSpeaker;s.portraitExpression=mobilePortraitExpression;
  if(Stage==0&&State!=FlightState.Hangar){s.convoy=new float[World.Civilians.Count];for(int i=0;i<s.convoy.Length;i++)s.convoy[i]=World.Civilians[i].health/100f;}
  if(State==FlightState.Hangar){
   s.routes=new MobileChoice[3];s.research=new MobileChoice[3];int[] levels={researchFire,researchArmor,researchSkill};
   for(int i=0;i<3;i++){s.routes[i]=new MobileChoice{index=i,name=routeNames[Ship,i],description=routeDescriptions[Ship,i].Replace("Q：","技能："),detail="Ⅰ "+routeNodes[Ship,i,0]+"\nⅡ "+routeNodes[Ship,i,1]+"\nⅢ "+routeNodes[Ship,i,2],selected=i==CurrentRoute};int max=i==1?2:3,cost=200+levels[i]*250;s.research[i]=new MobileChoice{index=i,name=new[]{"高效火控","轻质装甲","同步接口"}[i]+" Lv."+levels[i],description=new[]{"每级基础伤害 +4%","每级装甲上限 +1","每级技能冷却 -5%"}[i],detail=levels[i]>=max?"研发完成":cost+" 回收点",enabled=levels[i]<max&&salvage>=cost};}
  }
  if(State==FlightState.Upgrade){s.upgrades=new MobileChoice[3];for(int i=0;i<3;i++)s.upgrades[i]=new MobileChoice{index=i,name=UpgradeNames[UpgradeOptions[i]],description=UpgradeDescriptions[UpgradeOptions[i]],detail="装配并前往下一章"};}
  return s;
 }
 void DrawMobilePresentation(){
  if(Event.current.type!=EventType.Repaint)return;
  GUI.matrix=Matrix4x4.identity;
  // Portrait mode reserves a real radio dock below the camera. It shares the
  // Unity canvas so the same animated pilot rig remains visible on every device.
  if(mobileBattleViewport.y>0)Rect(0,Screen.height*(1-mobileBattleViewport.y),Screen.width,Screen.height*mobileBattleViewport.y,new Color(.028f,.065f,.09f,1));
  Rect r=new Rect(mobilePortraitRect.x*Screen.width,mobilePortraitRect.y*Screen.height,mobilePortraitRect.width*Screen.width,mobilePortraitRect.height*Screen.height);
  if(r.width<=0||r.height<=0)return;
  if(State==FlightState.Hangar&&(mobilePage=="home"||mobilePage=="dossier")||State==FlightState.Briefing||State==FlightState.Victory){r=SkyPortraitRig.FitRect(r,2f/3);Portrait(Ship,r.x,r.y,r.width,r.height);}
  else if(State==FlightState.Playing&&mobileRadioPresentation){int speaker=mobilePortraitSpeaker;
   // The web caption and portrait use one delivered radio snapshot. Sampling
   // the live voice here could show the next actor before its caption arrived.
   if(speaker>=3)DrawCommanderPortrait(speaker,r.x,r.y,r.width,r.height,mobilePortraitExpression);
   else {r=SkyPortraitRig.FitRect(r,SkyPortraitRig.BustAspect);Portrait(speaker,r.x,r.y,r.width,r.height,false,false,true);}
  }
  GUI.enabled=true;
 }
}
}
