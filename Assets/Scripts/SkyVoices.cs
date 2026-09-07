using System;
using System.Collections.Generic;
using UnityEngine;
namespace Skybreak {
[Serializable] public class SkyVoiceEntry {public string id,text,category,match,clip;public int pilot;}
[Serializable] public class SkyVoiceBank {public string version,language;public bool synthetic;public SkyVoiceEntry[] clips;}
public partial class SkyGame {
 public bool VoiceEnabled=true;public float VoiceVolume=.9f;
 AudioSource voiceAudio;SkyVoiceEntry voiceCurrent;int voicePriority;
 readonly Dictionary<string,SkyVoiceEntry> voiceById=new Dictionary<string,SkyVoiceEntry>();
 readonly Dictionary<string,SkyVoiceEntry> voiceByRadio=new Dictionary<string,SkyVoiceEntry>();
 readonly Dictionary<string,AudioClip> voiceClips=new Dictionary<string,AudioClip>();
 readonly Dictionary<string,int> voiceVariants=new Dictionary<string,int>();
 readonly Dictionary<string,float> voiceCooldowns=new Dictionary<string,float>();
 class VoiceRequest {public SkyVoiceEntry entry;public int priority;public float ready,expires;public System.Func<bool> valid;}
 readonly List<VoiceRequest> voiceQueue=new List<VoiceRequest>();
 System.Func<bool> voiceContextCheck;
 float voiceEnvelope,voiceFade=-1,voiceStartedAt;bool voicePaused,voiceSawState;
 int voiceLastShip=-1;string voiceLastRadio="";FlightState voiceLastState;
 bool voiceCanPlay;
 public string VoiceActiveId=>voiceCurrent!=null?voiceCurrent.id:"";
 public bool VoicePlaying=>voiceAudio&&voiceAudio.isPlaying&&voiceCurrent!=null;
 public float VoicePlaybackTime=>voiceAudio&&voiceAudio.clip?voiceAudio.time:0;
 bool VoiceRadioActive=>VoicePlaying&&voiceCurrent.category=="story";

 void SetupVoices(){
  voiceCanPlay=Application.platform!=RuntimePlatform.WebGLPlayer;
  VoiceEnabled=PlayerPrefs.GetInt("voiceEnabled",1)==1;VoiceVolume=Mathf.Clamp01(PlayerPrefs.GetFloat("voiceVolume",.9f));
  voiceAudio=gameObject.AddComponent<AudioSource>();voiceAudio.spatialBlend=0;voiceAudio.priority=12;voiceAudio.playOnAwake=false;voiceAudio.loop=false;
  var text=Resources.Load<TextAsset>("Voices/voice-bank");if(!text)return;
  var bank=JsonUtility.FromJson<SkyVoiceBank>(text.text);if(bank==null||bank.clips==null)return;
  foreach(var entry in bank.clips){voiceById[entry.id]=entry;if(!string.IsNullOrEmpty(entry.match))voiceByRadio[entry.match]=entry;}
 }
 void SaveVoiceSettings(){PlayerPrefs.SetInt("voiceEnabled",VoiceEnabled?1:0);PlayerPrefs.SetFloat("voiceVolume",VoiceVolume);}
 public void ActivateVoices(){voiceCanPlay=true;voiceLastShip=-1;}
 void ClearVoices(){voiceQueue.Clear();if(voiceAudio)voiceAudio.Stop();voiceCurrent=null;voiceFade=-1;voicePaused=false;voiceContextCheck=null;}
 void FinishVoice(){voiceContextCheck=null;if(voiceAudio)voiceAudio.Stop();voiceCurrent=null;voiceFade=-1;}
 void SyncVoicePause(){if(!voiceAudio)return;if(State==FlightState.Paused){voiceAudio.Pause();voicePaused=true;}else if(voicePaused){voiceAudio.UnPause();voicePaused=false;}}
 AudioClip VoiceClip(SkyVoiceEntry entry){
  if(!voiceClips.TryGetValue(entry.id,out var clip)){clip=Resources.Load<AudioClip>(entry.clip);voiceClips[entry.id]=clip;if(clip)clip.LoadAudioData();}
  return clip;
 }
 void QueueVoice(SkyVoiceEntry entry,int priority,float delay=0,bool keep=false){
  if(entry==null||!VoiceEnabled||!voiceCanPlay||qaRunning)return;
  bool story=entry.category=="story"||entry.category=="briefing";
  if(voiceCurrent!=null&&!story&&!keep&&priority<=voicePriority&&voiceAudio.clip&&voiceAudio.clip.length-voiceAudio.time>.55f)return;
  if(!story&&!keep){if(voiceQueue.Exists(r=>r.priority>=priority))return;voiceQueue.RemoveAll(r=>r.entry.category=="bark"&&r.priority<=priority);}
  voiceQueue.RemoveAll(r=>r.entry.id==entry.id);
  if(voiceQueue.Count>=5)voiceQueue.RemoveAt(0);
  voiceQueue.Add(new VoiceRequest{entry=entry,priority=priority,ready=Time.unscaledTime+delay,expires=Time.unscaledTime+(story||keep?24:2.5f)});
  if(voiceCurrent!=null&&priority>voicePriority)voiceFade=.07f;
 }
 void QueueVoiceId(string id,int priority,float delay=0,bool keep=false){if(voiceById.TryGetValue(id,out var entry))QueueVoice(entry,priority,delay,keep);}
 public void PlayPilotBark(string family,int pilot,int priority){
  if(!VoiceEnabled||!voiceCanPlay||qaRunning)return;
  string key=family+"_"+pilot;float now=Time.unscaledTime;
  if(voiceCooldowns.TryGetValue(key,out float until)&&now<until)return;
  voiceCooldowns[key]=now+(family=="hurt"?10:family=="danger"?14:family=="bomb"?3.5f:2);
  voiceVariants.TryGetValue(key,out int next);string id=key+"_"+next;
  if(!voiceById.ContainsKey(id)){next=0;id=key+"_0";}
  voiceVariants[key]=next+1;QueueVoiceId(id,priority);
 }
 public void PreviewPilotVoice(){
  if(!VoiceEnabled){Toast("请先在声音设置中开启角色配音",2);return;}
  ClearVoices();QueueVoiceId("profile_"+Ship+"_0",100,.05f,true);
 }
 void ObserveVoiceEvents(){
  if(!voiceSawState||State!=voiceLastState){
   var previous=voiceLastState;voiceSawState=true;voiceLastState=State;
   if(State==FlightState.Hangar){ClearVoices();voiceLastShip=-1;voiceLastRadio=RadioText;}
   else if(State==FlightState.Briefing){
    ClearVoices();voiceCooldowns.Clear();string id="briefing_"+Ship+"_"+Stage;
    if(voiceById.TryGetValue(id,out var entry)){RadioText=entry.text;RadioName=PilotNames[Ship]+" / 小队通讯";pilotSpeaker=Ship;QueueVoice(entry,90,.3f,true);}
    voiceLastRadio=RadioText;
   }else if(State==FlightState.Playing&&previous==FlightState.Briefing){ClearVoices();QueueVoiceId("launch_"+Ship+"_0",65,.08f);}
   else if(State==FlightState.Defeat){ClearVoices();QueueVoiceId("defeat_"+Ship+"_0",100,.65f,true);}
   else if(State==FlightState.Victory)QueueVoiceId("victory_"+Ship+"_0",60,.5f,true);
  }
  if(State==FlightState.Hangar&&Ship!=voiceLastShip){
   voiceLastShip=Ship;voiceQueue.Clear();if(voiceCurrent!=null)voiceFade=.06f;
   string key="select_"+Ship;voiceVariants.TryGetValue(key,out int variant);voiceVariants[key]=(variant+1)%2;
   QueueVoiceId(key+"_"+variant,50,.22f,true);
  }
  if(State==FlightState.Playing&&RadioText!=voiceLastRadio){
   voiceLastRadio=RadioText;
   if(voiceByRadio.TryGetValue(RadioText,out var entry)){pilotSpeaker=entry.pilot;RadioName=PilotNames[entry.pilot]+" / 小队通讯";RadioText=entry.text;voiceLastRadio=RadioText;QueueVoice(entry,90,0,true);}
  }
 }
 void UpdateVoices(float dt){
  if(!voiceAudio)return;
  if(voiceCanPlay&&!qaRunning)ObserveVoiceEvents();
  if(!VoiceEnabled||qaRunning)ClearVoices();
  else if(voiceCanPlay){
   if(State==FlightState.Paused){if(!voicePaused){voiceAudio.Pause();voicePaused=true;}foreach(var request in voiceQueue){request.ready+=dt;request.expires+=dt;}voiceStartedAt+=dt;}
   else {
    if(voicePaused){voiceAudio.UnPause();voicePaused=false;}
    if(voiceFade>=0){voiceFade-=dt;if(voiceFade<=0)FinishVoice();}
    if(voiceCurrent!=null&&!voiceAudio.isPlaying&&Time.unscaledTime-voiceStartedAt>.2f)FinishVoice();
    float now=Time.unscaledTime;voiceQueue.RemoveAll(r=>r.expires<now||(r.valid!=null&&!r.valid()));if(voiceCurrent!=null&&voiceContextCheck!=null&&!voiceContextCheck())FinishVoice();
    if(voiceCurrent==null){
     VoiceRequest next=null;foreach(var request in voiceQueue)if(request.ready<=now&&(next==null||request.priority>next.priority))next=request;
     if(next!=null){
      var clip=VoiceClip(next.entry);
      if(!clip)voiceQueue.Remove(next);
      else if(clip.loadState==AudioDataLoadState.Loaded){
       voiceQueue.Remove(next);voiceContextCheck=next.valid;voiceCurrent=next.entry;voicePriority=next.priority;voiceStartedAt=now;voiceFade=-1;
       voiceAudio.clip=clip;voiceAudio.pitch=1;voiceAudio.time=0;voiceAudio.Play();
      }
     }
    }
   }
  }
  float target=VoicePlaying&&VoiceVolume>.001f?1:0;voiceEnvelope=Mathf.MoveTowards(voiceEnvelope,target,dt*(target>voiceEnvelope?8:2.7f));
  voiceAudio.volume=MasterVolume*VoiceVolume*(voiceFade>=0?Mathf.Clamp01(voiceFade/.07f):1);
  if(gunAudio)gunAudio.volume=Mathf.Lerp(1,.61f,voiceEnvelope);
  if(impactAudio)impactAudio.volume=Mathf.Lerp(1,.7f,voiceEnvelope);
  if(sfx)sfx.volume=Mathf.Lerp(1,.83f,voiceEnvelope);
 }
 void DrawVoiceCaption(){
  if(!VoicePlaying||voiceCurrent.category=="story"||State==FlightState.Briefing||State==FlightState.Paused)return;
  bool hangar=State==FlightState.Hangar;if(hangar&&menuPage!=0&&menuPage!=4&&menuPage!=6)return;
  bool selector=hangar&&menuPage==0;float x=selector?474:478,y=selector?792:hangar?803:785,w=selector?526:644,h=selector?60:53;
  Panel(x,y,w,h);
  Label(PilotNames[voiceCurrent.pilot]+"  ·  "+voiceCurrent.text,x+16,y+(selector?9:14),w-32,h-12,selector?16:17,paper,TextAnchor.UpperCenter);
 }
 void DrawAudioSettings(){
  Panel(318,99,965,733);SmallTag("VOICE & SOUND",359,128,216,accent);Label("声音与配音",356,180,800,66,43,paper);
  Label("三位驾驶员，三种声音",359,281,840,36,23,accent);
  Label("苍凛 · 清亮温柔     绯音 · 低柔慵懒     雪璃 · 清冷知性",359,333,840,43,18,muted);
  ToggleRow("角色配音","剧情与战斗语音；重要通讯优先播放。",ref VoiceEnabled,412);
  Label("配音音量",360,511,211,36,20,paper);
  if(Button("−",608,501,61,47)){VoiceVolume=Mathf.Max(0,VoiceVolume-.1f);SaveSettings();}
  Bar(693,523,298,VoiceVolume,accent,6);
  if(Button("+",1013,501,61,47)){VoiceVolume=Mathf.Min(1,VoiceVolume+.1f);SaveSettings();}
  Label(Mathf.RoundToInt(VoiceVolume*100)+"%",1106,511,98,32,19,paper,TextAnchor.MiddleRight);
  Label("说话时，音乐与枪炮声会柔和降低；台词结束后自动恢复。",359,594,840,54,17,muted);
  if(Button("试听当前驾驶员  ·  "+PilotNames[Ship],359,668,482,60,true))PreviewPilotVoice();
  Label("原创合成声线 · 中文",876,685,329,30,15,muted,TextAnchor.UpperRight);
  if(Button("← 返回设置",42,765,226,55)){menuPage=1;SaveSettings();}
 }
}
}
