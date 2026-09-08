using System;
using System.Collections.Generic;
using UnityEngine;
namespace Skybreak {
[Serializable] public class SkyVoiceEntry {public string id,text,en,ja,category,match,clip,clipEn,clipJa,emotion;public int pilot;}
[Serializable] public class SkyVoiceBank {public string version,language;public bool synthetic;public SkyVoiceEntry[] clips;}
public partial class SkyGame {
 public bool VoiceEnabled=true;public float VoiceVolume=.9f;public int VoiceLanguage;
 int voiceLoadEpoch;readonly HashSet<string> voiceLoading=new HashSet<string>();readonly HashSet<string> voiceFailed=new HashSet<string>();
 public string VoiceLanguageCode=>VoiceLanguage==1?"en":VoiceLanguage==2?"ja":"zh";
 public int VoiceClipsReady=>voiceClips.Count;
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
 bool VoiceRadioActive=>VoicePlaying&&(voiceCurrent.category=="story"||voiceCurrent.category=="boss");

 void SetupVoices(){
  voiceCanPlay=Application.platform!=RuntimePlatform.WebGLPlayer;
  VoiceLanguage=Mathf.Clamp(PlayerPrefs.GetInt("voiceLanguage",0),0,2);VoiceEnabled=PlayerPrefs.GetInt("voiceEnabled",1)==1;VoiceVolume=Mathf.Clamp01(PlayerPrefs.GetFloat("voiceVolume",.9f));
  voiceAudio=gameObject.AddComponent<AudioSource>();voiceAudio.spatialBlend=0;voiceAudio.priority=12;voiceAudio.playOnAwake=false;voiceAudio.loop=false;
  var text=Resources.Load<TextAsset>("Voices/voice-bank");if(!text)return;
  var bank=JsonUtility.FromJson<SkyVoiceBank>(text.text);if(bank==null||bank.clips==null)return;
  foreach(var entry in bank.clips){voiceById[entry.id]=entry;if(!string.IsNullOrEmpty(entry.match))voiceByRadio[entry.match]=entry;}
  StartCoroutine(WarmVoiceBank(voiceLoadEpoch));
 }
 void SaveVoiceSettings(){PlayerPrefs.SetInt("voiceLanguage",VoiceLanguage);PlayerPrefs.SetInt("voiceEnabled",VoiceEnabled?1:0);PlayerPrefs.SetFloat("voiceVolume",VoiceVolume);}
 public void ActivateVoices(){voiceCanPlay=true;voiceLastShip=-1;}
 void ClearVoices(){voiceQueue.Clear();if(voiceAudio)voiceAudio.Stop();voiceCurrent=null;voiceFade=-1;voicePaused=false;voiceContextCheck=null;}
 void FinishVoice(){voiceContextCheck=null;if(voiceAudio)voiceAudio.Stop();voiceCurrent=null;voiceFade=-1;}
 void SyncVoicePause(){if(!voiceAudio)return;if(State==FlightState.Paused){voiceAudio.Pause();voicePaused=true;}else if(voicePaused){voiceAudio.UnPause();voicePaused=false;}}
 string VoiceKey(SkyVoiceEntry entry)=>VoiceLanguageCode+"/"+entry.id;
 string VoicePath(SkyVoiceEntry entry)=>VoiceLanguage==1?entry.clipEn:VoiceLanguage==2?entry.clipJa:entry.clip;
 public void SetVoiceLanguage(int language){
  int next=Mathf.Clamp(language,0,2);if(next==VoiceLanguage)return;
  ClearVoices();voiceLoadEpoch++;VoiceLanguage=next;voiceLastShip=-1;
  foreach(var clip in voiceClips.Values)if(clip)Resources.UnloadAsset(clip);
  voiceClips.Clear();voiceLoading.Clear();voiceFailed.Clear();voiceVariants.Clear();voiceCooldowns.Clear();
  StartCoroutine(WarmVoiceBank(voiceLoadEpoch));SaveSettings();
 }
 System.Collections.IEnumerator WarmVoiceBank(int epoch){
  foreach(var entry in voiceById.Values){if(epoch!=voiceLoadEpoch)yield break;yield return LoadVoiceClip(entry,epoch);}
 }
 System.Collections.IEnumerator LoadVoiceClip(SkyVoiceEntry entry,int epoch){
  string key=VoiceKey(entry);if(voiceClips.ContainsKey(key)||!voiceLoading.Add(key))yield break;
  string path=VoicePath(entry);var request=Resources.LoadAsync<AudioClip>(path);yield return request;
  var clip=request.asset as AudioClip;
  if(epoch!=voiceLoadEpoch){yield break;}
  voiceLoading.Remove(key);
  if(clip){voiceClips[key]=clip;if(clip.loadState==AudioDataLoadState.Unloaded)clip.LoadAudioData();}
  else {voiceFailed.Add(key);Debug.LogWarning("Voice asset unavailable: "+key);}
 }
 AudioClip VoiceClip(SkyVoiceEntry entry){
  string key=VoiceKey(entry);if(voiceClips.TryGetValue(key,out var clip))return clip;
  if(!voiceLoading.Contains(key)&&!voiceFailed.Contains(key))StartCoroutine(LoadVoiceClip(entry,voiceLoadEpoch));
  return null;
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
      if(!clip){if(voiceFailed.Contains(VoiceKey(next.entry)))voiceQueue.Remove(next);}
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
  if(!VoicePlaying||voiceCurrent.category=="story"||(voiceCurrent.category=="boss"&&State!=FlightState.Hangar)||State==FlightState.Briefing||State==FlightState.Paused)return;
  bool hangar=State==FlightState.Hangar;if(hangar&&menuPage!=0&&menuPage!=4&&menuPage!=6)return;
  bool selector=hangar&&menuPage==0;float x=selector?474:478,y=selector?792:hangar?803:785,w=selector?526:644,h=selector?60:53;
  Panel(x,y,w,h);
  Label(SpeakerName(voiceCurrent.pilot)+"  ·  "+voiceCurrent.text,x+16,y+(selector?9:14),w-32,h-12,selector?16:17,paper,TextAnchor.UpperCenter);
 }
 void DrawAudioSettings(){
  Panel(318,99,965,733);SmallTag("VOICE & SOUND",359,128,216,accent);Label("声音与配音",356,177,800,66,43,paper);
  Label("配音语言",359,265,200,35,22,paper);
  string[] languages={"中文","English","日本語"};
  for(int i=0;i<3;i++)if(Button(languages[i],599+i*202,257,185,51,VoiceLanguage==i))SetVoiceLanguage(i);
  Label("六位角色完整配音 · 保留中文字幕 · 选择会自动保存",359,328,840,31,17,muted);
  ToggleRow("角色配音","驾驶员与敌方指挥官；重要战况优先。",ref VoiceEnabled,393);
  Label("配音音量",360,488,211,36,20,paper);
  if(Button("−",608,477,61,47)){VoiceVolume=Mathf.Max(0,VoiceVolume-.1f);SaveSettings();}
  Bar(693,499,298,VoiceVolume,accent,6);
  if(Button("+",1013,477,61,47)){VoiceVolume=Mathf.Min(1,VoiceVolume+.1f);SaveSettings();}
  Label(Mathf.RoundToInt(VoiceVolume*100)+"%",1106,488,98,32,19,paper,TextAnchor.MiddleRight);
  Label("通讯会降低音乐与枪炮音量，结束后恢复。\n敌方会在入场、受挫、濒危和败北时作出不同反应。",359,562,840,67,17,muted);
  if(Button("试听驾驶员 · "+PilotNames[Ship],359,670,410,60,true))PreviewPilotVoice();
  if(Button("试听敌方 · "+new[]{"加兰","卡西娅","诺克特"}[Ship],799,670,410,60))PreviewCommanderVoice();
  Label(voiceClips.Count<voiceById.Count?"正在准备配音 "+voiceClips.Count+" / "+voiceById.Count:"原创合成声线 · "+languages[VoiceLanguage]+" · 中文字幕",359,760,846,30,15,muted);
  if(Button("← 返回设置",42,765,226,55)){menuPage=1;SaveSettings();}
 }
 void PreviewCommanderVoice(){if(!VoiceEnabled){Toast("请先开启角色配音",2);return;}ClearVoices();QueueVoiceId("commander_"+Ship+"_entry",100,.05f,true);}
}
}
