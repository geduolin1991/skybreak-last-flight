using System;
using System.Collections.Generic;
using UnityEngine;

namespace Skybreak {
[Serializable] public sealed class SkyCampaignRecord { public int[] results,masks; public float[] hull; }
[Serializable] public sealed class SkyStoryBeat {
 public string voice,heading,body; public int world,speaker;
}
[Serializable] public sealed class SkyStorySequence {
 public string id,title; public SkyStoryBeat[] beats;
}
[Serializable] public sealed class SkyStoryLibrary {
 public string version; public SkyStorySequence[] sequences;
}

public partial class SkyGame {
 SkyStoryLibrary storyLibrary; SkyCampaignRecord replayRecord;
 readonly List<SkyStoryBeat> storyBeats=new List<SkyStoryBeat>();
 string storyTitle="",storyId=""; int storyIndex,endingMask;
 bool storyActive,storyAuto; float storyAge; string lastCampaignReport="";
 readonly int[] chapterNodeMasks=new int[3];readonly int[] chapterNodes=new int[3]; readonly float[] rescuedHull=new float[3];
 public bool StoryActive=>storyActive;
 public int StoryIndex=>storyIndex;
 public int StoryCount=>storyBeats.Count;
 public int StorySpeaker=>storyActive?storyBeats[storyIndex].speaker:Ship;
 public int CampaignEnding=>missionResults[2]>0?(missionResults[0]>0&&missionResults[1]>0?0:1):2;
 static readonly string[] endingTitles={"再一次黎明","带着名字归航","最后的守望"};
 public string EndingTitle=>endingTitles[CampaignEnding];
 public string CampaignReport {
  get {
   int afloat=0,people=0;int[] passengers={312,268,190};
   for(int i=0;i<3;i++)if(rescuedHull[i]>0){afloat++;people+=passengers[i];}
   string sea=missionResults[0]==0?"海岸：尚未完成":$"海岸：{afloat} / 3 艘船通过封锁 · {people} 人抵达外海";
   string city=missionResults[1]==0?"城市：尚未完成":$"城市：{chapterNodes[1]} / 3 街区恢复供电";
   string orbit=missionResults[2]==0?"天环：尚未完成":missionResults[2]>0?"天环：身份上传完成 · 接驳船安全靠港":"天环：攻击已停止 · 仍需人工引导";
   return sea+"\n"+city+"\n"+orbit;
  }
 }
 void LoadCampaignStory(){
  if(storyLibrary!=null)return;
  var asset=Resources.Load<TextAsset>("Campaign/campaign");
  storyLibrary=asset?JsonUtility.FromJson<SkyStoryLibrary>(asset.text):new SkyStoryLibrary{sequences=new SkyStorySequence[0]};
  endingMask=PlayerPrefs.GetInt("campaign-ending-mask",0);
  lastCampaignReport=PlayerPrefs.GetString("campaign-last-report","");
 }
 void ResetStoryCampaign(){
  EndStory(false);for(int i=0;i<3;i++){chapterNodes[i]=0;chapterNodeMasks[i]=0;rescuedHull[i]=0;}
 }
 void CaptureChapterOutcome(){
  chapterNodes[Stage]=missionNodes;
  if(Stage==0)for(int i=0;i<3;i++)rescuedHull[i]=i<World.Civilians.Count?World.Civilians[i].health:0;
 }
 void RecordCampaignEnding(){
  if(Practice||Endless||qaRunning)return;
  LoadCampaignStory();endingMask|=1<<CampaignEnding;lastCampaignReport=CampaignReport;
  PlayerPrefs.SetInt("campaign-last-ending",CampaignEnding);PlayerPrefs.SetString("campaign-ending-data-"+CampaignEnding,JsonUtility.ToJson(new SkyCampaignRecord{results=missionResults,masks=chapterNodeMasks,hull=rescuedHull}));
  PlayerPrefs.SetInt("campaign-ending-mask",endingMask);PlayerPrefs.SetString("campaign-last-report",lastCampaignReport);PlayerPrefs.Save();
 }
 SkyStorySequence FindStory(string id){LoadCampaignStory();foreach(var s in storyLibrary.sequences)if(s.id==id)return s;return null;}
 bool CanReplayStory(SkyStorySequence sequence){
  string id=sequence.id;if(id=="before_0")return true;
  if(id.StartsWith("ending_"))return (endingMask&(1<<int.Parse(id.Substring(7))))!=0;
  if(id.StartsWith("pilot_"))return endingMask!=0;
  int stage=int.Parse(id.Substring(id.Length-1));return Unlocked>=stage+(id.StartsWith("after_")?1:0);
 }
 void StartStory(string id,bool appendPilot=false){
  var sequence=FindStory(id);if(sequence==null||sequence.beats==null||sequence.beats.Length==0)return;
  EndStory(false);storyBeats.Clear();storyBeats.AddRange(sequence.beats);
  if(appendPilot){var coda=FindStory("pilot_"+Ship);if(coda!=null)storyBeats.AddRange(coda.beats);}
  replayRecord=null;
  if(State==FlightState.Hangar&&(id.StartsWith("ending_")||id.StartsWith("pilot_"))){int ending=id.StartsWith("ending_")?int.Parse(id.Substring(7)):PlayerPrefs.GetInt("campaign-last-ending",0);string saved=PlayerPrefs.GetString("campaign-ending-data-"+ending,"");if(saved.Length>0)replayRecord=JsonUtility.FromJson<SkyCampaignRecord>(saved);SetMusicCue(ending==0?"MusicDawn":"MusicAftermath");}
  storyId=id;storyTitle=sequence.title;storyIndex=0;storyActive=true;storyAuto=false;
  ResetMobileInput();Time.timeScale=1;if(Player)Player.SetActive(false);if(showcase)showcase.SetActive(false);
  if(hitPoint)hitPoint.gameObject.SetActive(false);if(focusRing)focusRing.gameObject.SetActive(false);
  World.Scrolling=false;PresentStoryBeat();
 }
 void PresentStoryBeat(){
  storyAge=0;ClearVoices();var beat=storyBeats[storyIndex];
  World.PresentCampaignTableau(beat.world,storyIndex,storyId.StartsWith("ending_")||storyId.StartsWith("pilot_"));
  if(storyId.StartsWith("ending_")||storyId.StartsWith("pilot_")){
   if(State!=FlightState.Hangar)World.ShowCampaignAftermath(missionResults,chapterNodeMasks,rescuedHull);
   else if(replayRecord!=null&&replayRecord.results.Length==3&&replayRecord.masks.Length==3&&replayRecord.hull.Length==3)World.ShowCampaignAftermath(replayRecord.results,replayRecord.masks,replayRecord.hull);
  }else if(State==FlightState.Hangar)ChangeMusic(beat.world);
  QueueVoiceId(beat.voice,110,.18f,true);
 }
 public void AdvanceStory(){if(!storyActive)return;if(++storyIndex>=storyBeats.Count){EndStory();return;}PresentStoryBeat();Sound("Click",.4f);}
 public void SkipStory(){if(storyActive)EndStory();}
 void EndStory(bool restore=true){
  if(!storyActive)return;storyActive=false;ClearVoices();World.EndCampaignTableau();
  if(!restore)return;
  if(State==FlightState.Briefing&&World.Stage!=Stage){World.SetStage(Stage);World.BeginLivingMission();}
  World.Scrolling=State==FlightState.Briefing||State==FlightState.Hangar;
  if(Player)Player.SetActive(State==FlightState.Briefing);
  if(showcase)showcase.SetActive(State==FlightState.Hangar);if(State==FlightState.Hangar){World.SetStage(0);ChangeMusic(3);World.Speed=3;World.Scrolling=true;}
  Cam.transform.position=cameraBase;Cam.transform.rotation=Quaternion.Euler(60,0,0);
 }
 void TickStory(float dt){
  storyAge+=dt;World.TickCampaignTableau(dt);
  float sway=Mathf.Sin(storyAge*.13f+storyIndex*.55f);
  Cam.transform.position=cameraBase+new Vector3(sway*.65f,0,Mathf.Sin(storyAge*.09f)*.45f);
  if(!qaRunning){
   if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.Space))AdvanceStory();
   else if(Input.GetKeyDown(KeyCode.Escape))SkipStory();
  }
  if(storyActive&&storyAuto){
   var beat=storyBeats[storyIndex];voiceById.TryGetValue(beat.voice,out var entry);
   float duration=entry!=null?VoiceDuration(entry):4;
   float readTime=Mathf.Max(9,beat.body.Length*.12f);
   if(storyAge>Mathf.Max(readTime,duration+.7f)&&!VoicePlaying&&!voiceQueue.Exists(r=>r.entry.id==beat.voice))AdvanceStory();
  }
 }
 string StoryQuote {get{if(!storyActive)return "";return voiceById.TryGetValue(storyBeats[storyIndex].voice,out var entry)?entry.text:"";}}
 public void ReplayCampaignStory(int index){
  LoadCampaignStory();if(State!=FlightState.Hangar||index<0||index>=storyLibrary.sequences.Length)return;
  var s=storyLibrary.sequences[index];if(!CanReplayStory(s))return;StartStory(s.id);
 }
 void DrawStory(){
  var beat=storyBeats[storyIndex];GUI.DrawTexture(new Rect(0,0,1240,900),gradient);
  Rect(0,0,1600,72,new Color(.004f,.014f,.022f,.92f));SmallTag(storyTitle,55,22,470,accent);
  Label((storyIndex+1).ToString("00")+" / "+storyBeats.Count.ToString("00"),1418,25,130,26,18,muted,TextAnchor.MiddleRight);
  Portrait(beat.speaker,984,118,442,663,false,true);
  Label(beat.heading,68,170,850,125,46,paper);Line(72,304,95,accent);
  Rect(48,326,852,412,new Color(.003f,.014f,.024f,.62f));Line(48,326,852,new Color(.2f,.5f,.58f,.5f));
  Label(beat.body,72,346,802,216,23,paper);
  Label(PilotNames[beat.speaker]+" / 小队通讯",73,606,778,30,16,accent);
  Label(StoryQuote,72,650,810,84,25,paper);
  Rect(0,784,1600,116,new Color(.004f,.014f,.022f,.96f));
  if(Button(storyAuto?"自动播放 · 开":"自动播放 · 关",72,813,249,54,storyAuto))storyAuto=!storyAuto;
  if(Button("跳过本段 [ESC]",346,813,282,54))SkipStory();
  if(Button(storyIndex+1<storyBeats.Count?"继续剧情  →  [ENTER]":"返回行动  →  [ENTER]",1071,809,473,62,true))AdvanceStory();
 }
 void DrawCampaignArchive(){
  LoadCampaignStory();Panel(318,99,965,733);SmallTag("FLIGHT RECORD / COMPLETE CAMPAIGN",353,126,340,accent);
  Label("战役档案",352,174,880,65,40,paper);Label("序幕、章间与结局会随行动解锁，可随时重温。",356,251,850,37,17,muted);
  for(int i=0;i<storyLibrary.sequences.Length;i++){
   var s=storyLibrary.sequences[i];float x=356+(i%2)*446,y=312+(i/2)*73;bool unlocked=CanReplayStory(s);
   if(Button(unlocked?s.title:"未解锁 · "+s.title,x,y,423,57,false,unlocked))ReplayCampaignStory(i);
  }
  if(Button("← 返回章节",42,765,226,55))menuPage=3;
 }
 void DrawCampaignResult(){
  Rect(0,69,1600,787,new Color(.003f,.012f,.022f,.90f));Panel(282,120,1036,689);
  SmallTag("CAMPAIGN COMPLETE / 0"+(CampaignEnding+1),323,152,330,accent);
  Label(EndingTitle,318,205,900,77,50,paper);Label(CampaignReport,325,315,925,120,21,paper);
  Line(325,458,948,new Color(.2f,.38f,.45f));
  Label("行动得分  "+Score.ToString("N0"),325,493,866,54,33,paper);
  Label("最高连击  "+MaxCombo+"   ·   击破  "+Kills+"   ·   擦弹  "+Grazes+"   ·   回收 +"+earnedSalvage,325,563,925,36,18,accent);
  Label("本次结局已加入战役档案。完成更多民用目标，可以看见不同的归航。",325,616,925,48,16,muted);
  if(Button("再次出击",324,697,278,63,true))BeginRun();
  if(Button("重温本次结局",620,697,324,63))StartStory("ending_"+CampaignEnding,true);
  if(Button("返回机库",962,697,310,63)){SetupHangar();menuPage=0;}
 }
 void PopulateStorySnapshot(MobileSnapshot s){
  LoadCampaignStory();s.voiceTotal=voiceById.Count;s.storyActive=storyActive;s.storyAuto=storyAuto;
  s.storyIndex=storyIndex;s.storyCount=storyBeats.Count;s.storySpeaker=StorySpeaker;s.storyTitle=storyTitle;
  s.ending=EndingTitle;s.outcome=CampaignReport;s.lastReport=lastCampaignReport;s.practice=Practice;
  if(storyActive){var beat=storyBeats[storyIndex];s.storyHeading=beat.heading;s.storyBody=beat.body;s.storyQuote=StoryQuote;s.storyName=PilotNames[beat.speaker];}
  if(State==FlightState.Hangar&&mobilePage=="archive"){
   s.stories=new MobileChoice[storyLibrary.sequences.Length];for(int i=0;i<s.stories.Length;i++){
    var story=storyLibrary.sequences[i];bool unlocked=CanReplayStory(story);
    s.stories[i]=new MobileChoice{index=i,name=story.title,description=unlocked?story.beats[0].heading:"继续战役或完成对应结局后解锁",detail=unlocked?"重温剧情":"未解锁",enabled=unlocked};
   }
  }
 }
 void TickCampaignRadio(){
  if(stageEndClock>0||Boss!=null)return;
  if(Stage==0){if(StageTime>33)MissionSay("campaign_beacon",66,()=>Stage==0&&Boss==null);if(StageTime>56)MissionSay("campaign_blockade",66,()=>Stage==0&&Boss==null);}
  else if(Stage==1){if(missionNodes>0)MissionSay("campaign_metro",68,()=>missionNodes>0&&Boss==null);if(sideObjectiveComplete)MissionSay("campaign_weather",68,()=>sideObjectiveComplete&&Boss==null);}
  else {if(missionNodes>0)MissionSay("campaign_array",67,()=>missionNodes>0&&Boss==null);if(StageTime>78&&missionNodes<3)MissionSay("campaign_uplink_hold",80,()=>missionNodes<3&&Boss==null);}
 }
}
}
