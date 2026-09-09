using UnityEngine;
namespace Skybreak {
public partial class SkyGame {
 AudioSource outgoingMusic,outgoingIntensity,ambience,outgoingAmbience;
 float scoreFade=1,ambientFade=1,outgoingBlend;
 AudioSource LoopSource(int priority){var a=gameObject.AddComponent<AudioSource>();a.loop=true;a.spatialBlend=0;a.priority=priority;a.playOnAwake=false;a.volume=0;return a;}
 void SetupCampaignAudio(){outgoingMusic=LoopSource(100);outgoingIntensity=LoopSource(100);ambience=LoopSource(150);outgoingAmbience=LoopSource(150);}
 void SetMusicCue(string name,string layer=null){
  var next=Clip(name);if(!next||!music)return;
  if(music.clip==next&&(music.isPlaying||AudioSettings.dspTime<musicScheduledUntil))return;
  outgoingMusic.Stop();outgoingIntensity.Stop();
  var swap=outgoingMusic;outgoingMusic=music;music=swap;
  swap=outgoingIntensity;outgoingIntensity=musicIntensity;musicIntensity=swap;
  outgoingBlend=musicBlend;musicBlend=0;scoreFade=outgoingMusic.isPlaying?0:1;
  music.clip=next;music.volume=0;musicIntensity.clip=layer==null?null:Clip(layer);musicIntensity.volume=0;
  double when=AudioSettings.dspTime+.06;musicScheduledUntil=when;music.PlayScheduled(when);
  if(musicIntensity.clip)musicIntensity.PlayScheduled(when);
 }
 void SetAmbience(int stage){
  var next=Clip(new[]{"AmbienceSea","AmbienceCity","AmbienceOrbit","AmbienceHangar"}[stage]);
  if(!next||ambience.clip==next)return;
  outgoingAmbience.Stop();var swap=outgoingAmbience;outgoingAmbience=ambience;ambience=swap;
  ambience.clip=next;ambience.volume=0;ambience.Play();ambientFade=outgoingAmbience.isPlaying?0:1;
 }
 void EnterBossScore(){SetMusicCue(new[]{"MusicLeviathan","MusicTempest","MusicSeraph"}[Stage]);}
 void PlayEpilogueScore(){SetMusicCue(CampaignEnding==0?"MusicDawn":"MusicAftermath");}
 void MixCampaignAudio(float dt,float volume){
  scoreFade=Mathf.MoveTowards(scoreFade,1,dt/1.3f);ambientFade=Mathf.MoveTowards(ambientFade,1,dt/2);
  float incoming=Mathf.Sin(scoreFade*Mathf.PI*.5f),outgoing=Mathf.Cos(scoreFade*Mathf.PI*.5f);
  music.volume=volume*.72f*incoming;musicIntensity.volume=volume*musicBlend*.36f*incoming;
  outgoingMusic.volume=volume*.72f*outgoing;outgoingIntensity.volume=volume*outgoingBlend*.36f*outgoing;
  if(scoreFade>=1){if(outgoingMusic.isPlaying)outgoingMusic.Stop();if(outgoingIntensity.isPlaying)outgoingIntensity.Stop();}
  float bed=MasterVolume*(State==FlightState.Paused?.035f:.085f)*(1-voiceEnvelope*.55f);
  ambience.volume=bed*Mathf.Sin(ambientFade*Mathf.PI*.5f);outgoingAmbience.volume=bed*Mathf.Cos(ambientFade*Mathf.PI*.5f);
  if(ambientFade>=1&&outgoingAmbience.isPlaying)outgoingAmbience.Stop();
 }
 void RestoreCampaignAudio(){scoreFade=ambientFade=1;outgoingMusic.Stop();outgoingIntensity.Stop();outgoingAmbience.Stop();if(ambience.clip&&!ambience.isPlaying)ambience.Play();}
}
}
