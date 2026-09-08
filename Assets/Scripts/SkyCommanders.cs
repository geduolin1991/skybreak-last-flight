using UnityEngine;
namespace Skybreak {
public partial class SkyGame {
 readonly Texture2D[] commanderPortraits=new Texture2D[3];
 static readonly string[] CommanderNames={"加兰 / 利维坦舰长","卡西娅 / 暴风眼王牌","诺克特 / 天环指挥官"};
 SkyVoiceEntry commanderRadio;float commanderRadioClock;bool commanderPhaseSaid,commanderCriticalSaid;
 public string CommanderEvent=>commanderRadio!=null?commanderRadio.id:"";
 public int CommanderExpression=>commanderRadio==null?0:commanderRadio.id.EndsWith("death")?2:commanderRadio.id.EndsWith("entry")?0:1;
 string SpeakerName(int actor)=>actor<3?PilotNames[Mathf.Clamp(actor,0,2)]:CommanderNames[Mathf.Clamp(actor-3,0,2)];
 void SetupCommanders(){StartCoroutine(LoadCommanderArt());}
 System.Collections.IEnumerator LoadCommanderArt(){for(int i=0;i<3;i++){var r=Resources.LoadAsync<Texture2D>("BossPortraits/"+new[]{"Garan","Cassia","Noct"}[i]);yield return r;commanderPortraits[i]=r.asset as Texture2D;}}
 void ResetCommander(){commanderRadio=null;commanderRadioClock=0;commanderPhaseSaid=commanderCriticalSaid=false;}
 void CommanderSay(string eventName){
  string id="commander_"+Stage+"_"+eventName;if(!voiceById.TryGetValue(id,out var entry))return;
  commanderRadio=entry;commanderRadioClock=VoiceDuration(entry)+.45f;
  bool death=eventName=="death";int stage=Stage;var target=Boss;
  // A death transmission invalidates the commander's unfinished threat.
  voiceQueue.RemoveAll(r=>r.entry.category=="boss");
  if(voiceCurrent!=null&&voiceCurrent.category=="boss")FinishVoice();
  QueueVoice(entry,death?120:eventName=="critical"?108:99,0,true);
  var request=voiceQueue.Find(r=>r.entry==entry);if(request!=null){request.valid=()=>Stage==stage&&State==FlightState.Playing&&(death?stageEndClock>0:Boss==target&&Boss!=null&&Boss.hp>0);request.expires=Time.unscaledTime+5;}
 }
 void TickCommander(float dt){
  commanderRadioClock=Mathf.Max(0,commanderRadioClock-dt);
  if(Boss==null)return;
  float health=Boss.hp/Boss.maxHp;
  if(health<=.24f&&!commanderCriticalSaid){commanderCriticalSaid=commanderPhaseSaid=true;CommanderSay("critical");}
  else if(health<=.66f&&!commanderPhaseSaid){commanderPhaseSaid=true;CommanderSay("phase");}
 }
 float VoiceDuration(SkyVoiceEntry entry){if(entry!=null&&voiceClips.TryGetValue(VoiceKey(entry),out var c)&&c)return c.length;return entry==null?0:Mathf.Clamp(entry.text.Length*.14f,2,6);}
 void DrawCommanderPortrait(int actor,float x,float y,float w,float h,int expression){
  var texture=commanderPortraits[Mathf.Clamp(actor-3,0,2)];if(!texture)return;
  // Preserve each frame's aspect and crop its lower chest for the radio window.
  float frameAspect=texture.width/3f/texture.height;float crop=Mathf.Min(1,frameAspect/(w/h));
  var rect=new Rect(x,y+Mathf.Sin(Time.unscaledTime*1.8f)*.6f,w,h);
  GUI.DrawTextureWithTexCoords(rect,texture,new Rect(expression/3f,Mathf.Clamp01(1-crop-.035f),1f/3,crop));
  if(expression>0)Rect(x,y+h-3,w,3,new Color(1,.25f,.13f,.65f));
 }
}
}
