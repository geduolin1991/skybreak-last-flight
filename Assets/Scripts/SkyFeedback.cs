using UnityEngine;
using System.Collections.Generic;
namespace Skybreak {
public partial class SkyGame {
 bool reactorWasReady;
 AudioSource impactAudio,musicIntensity;float musicBlend,audioDuck,impactGate,killGate,weaponKick,hurtEdge,bossHpTrail=1;
 readonly Dictionary<string,AudioClip> audioCache=new Dictionary<string,AudioClip>();
 readonly List<ScorePop> scorePops=new List<ScorePop>();readonly List<DeathCloud> deathClouds=new List<DeathCloud>();
 class ScorePop {public Vector3 pos;public string text;public float age,max;public bool heavy;}
 class DeathCloud {public Vector3 pos;public int bursts;public float clock,size;}
 AudioClip Clip(string name){if(!audioCache.TryGetValue(name,out var c)){c=Resources.Load<AudioClip>("Audio/"+name);audioCache[name]=c;}return c;}
 void SetupFeedback(){impactAudio=gameObject.AddComponent<AudioSource>();impactAudio.spatialBlend=0;impactAudio.priority=50;musicIntensity=gameObject.AddComponent<AudioSource>();musicIntensity.loop=true;musicIntensity.spatialBlend=0;musicIntensity.priority=100;SetupVoices();AudioSettings.OnAudioConfigurationChanged+=RestoreSoundtrack;}
 void RestoreSoundtrack(bool changed){if(!music||!music.clip)return;int sample=music.timeSamples;music.Stop();music.timeSamples=Mathf.Clamp(sample,0,music.clip.samples-1);double when=AudioSettings.dspTime+.12;music.PlayScheduled(when);if(musicIntensity&&musicIntensity.clip){musicIntensity.Stop();musicIntensity.timeSamples=Mathf.Clamp(sample,0,musicIntensity.clip.samples-1);musicIntensity.PlayScheduled(when);}}
 void OnDestroy(){if(projectileRim)Destroy(projectileRim);AudioSettings.OnAudioConfigurationChanged-=RestoreSoundtrack;foreach(var font in sizedFonts.Values)if(font){if(font.material)Destroy(font.material);Destroy(font);}sizedFonts.Clear();}
 void MixSoundtrack(float dt){
  if(!music)return;float target=State==FlightState.Playing?(Overdrive>0?1:Boss!=null?.75f:eliteTarget!=null&&Enemies.Contains(eliteTarget)?.5f:CurrentEncounterSection==3?.48f:CurrentEncounterSection==1?.14f:.06f):0;
  if(State==FlightState.Playing&&Boss==null&&Overdrive<=0&&Combo>=16)target=Mathf.Min(.7f,target+.12f);
  musicBlend=Mathf.MoveTowards(musicBlend,target,dt*(target>musicBlend?1.7f:.65f));audioDuck=Mathf.Max(0,audioDuck-dt*2.8f);
  float pause=State==FlightState.Paused?.48f:1;float volume=MusicEnabled?MasterVolume*pause*(1-audioDuck*.45f)*(1-voiceEnvelope*.46f):0;
  music.volume=volume*.72f;if(musicIntensity)musicIntensity.volume=volume*musicBlend*.36f;
 }
 void ResetFeedback(){reactorWasReady=false;scorePops.Clear();deathClouds.Clear();weaponKick=hurtEdge=impactGate=killGate=audioDuck=0;bossHpTrail=1;if(Post){Post.Damage=0;Post.Flash=0;}}
 void TickFeedback(float dt){TickNova(dt);
  bool ready=Energy>=100&&Overdrive<=0;if(ready&&!reactorWasReady){Sound("Combo",.42f);Toast("超载就绪 · 按 E 展开机甲",2.1f);}reactorWasReady=ready;
  impactGate=Mathf.Max(0,impactGate-dt);killGate=Mathf.Max(0,killGate-dt);weaponKick=Mathf.Max(0,weaponKick-dt*8);hurtEdge=Mathf.Max(0,hurtEdge-dt*2.4f);
  if(Boss!=null)bossHpTrail=Mathf.Max(Boss.hp/Boss.maxHp,Mathf.MoveTowards(bossHpTrail,Boss.hp/Boss.maxHp,dt*.35f));else bossHpTrail=1;
  for(int i=scorePops.Count-1;i>=0;i--){var p=scorePops[i];p.age+=dt;p.pos+=Vector3.forward*dt*.6f;if(p.age>=p.max)scorePops.RemoveAt(i);}
  for(int i=deathClouds.Count-1;i>=0;i--){var d=deathClouds[i];d.clock-=dt;if(d.clock>0)continue;var p=d.pos+new Vector3(Random.Range(-d.size,d.size),Random.Range(0,.9f),Random.Range(-d.size,d.size));Burst(p,38,1,d.size*.46f);Shockwave(p,Art.Orange,d.size*1.3f,.35f);shake=Mathf.Max(shake,.22f);if(d.bursts%2==0)Sound("Explosion",.32f);d.bursts--;d.clock=.14f;if(d.bursts<=0)deathClouds.RemoveAt(i);}
 }
 void OnWeaponFired(){weaponKick=Ship==0?.42f:Ship==1?1.0f:WeaponMode==0?1.3f:.65f;string name=Ship==0?(WeaponMode==0?"ShotPulse":"ShotSeeker"):Ship==1?(WeaponMode==0?"ShotScatter":"ShotMortar"):(WeaponMode==0?"ShotLance":"ShotRailFast");var clip=Clip(name);gunAudio.pitch=Random.Range(.975f,1.025f);if(clip)gunAudio.PlayOneShot(clip,MasterVolume*(Ship==0?.26f:Ship==1?.45f:.48f));
  if(Ship!=2)for(int i=-1;i<=1;i+=2){Vector3 pos=PlayerPos+new Vector3(i*(Ship==1?.9f:.6f),0,1);sparks.Add(new Spark{pos=pos,velocity=Vector3.forward*5,life=Ship==1?.09f:.045f,max=.09f,size=Ship==1?.4f:.19f,style=Ship==1?3:0});}
 }
 void OnArmorImpact(Hostile e,float damage){
  e.flash=.095f;e.recoil=Mathf.Min(.16f,e.recoil+.055f+(e.kind>=2?.02f:0));
  if(impactGate<=0){impactGate=.065f;impactAudio.panStereo=Mathf.Clamp(e.pos.x/10,-1,1)*.45f;var clip=Clip(e.boss||e.kind>=2?"ImpactArmor":"ImpactLight");impactAudio.pitch=Random.Range(.88f,1.12f);if(clip)impactAudio.PlayOneShot(clip,MasterVolume*(e.boss?.55f:.42f));}
 }
 void OnTargetDestroyed(Hostile e,int points){
  bool heavy=e.kind>=2;int multiplier=Mathf.Min(10,1+Combo/8);
  if(scorePops.Count<24)scorePops.Add(new ScorePop{pos=e.pos,text=(e.boss?"CORE BREAK  ":heavy?"BREAK  ":"")+"+"+(points*multiplier).ToString("N0"),max=heavy?1.05f:.68f,heavy=heavy});
  Shockwave(e.pos,heavy?Art.Orange:Art.Cyan,e.boss?14:heavy?4.5f:1.7f,heavy?.55f:.24f);
  Burst(e.pos,e.boss?220:heavy?78:36,1,e.boss?2.8f:heavy?1.35f:1);
  if(e.boss){deathClouds.Add(new DeathCloud{pos=e.pos,bursts=11,size=3.5f,clock=.12f});Sound("BossBreak",1);hitStop=.12f;shake=1.25f;audioDuck=.8f;}
  else if(heavy){deathClouds.Add(new DeathCloud{pos=e.pos,bursts=3,size=1.45f,clock=.08f});Sound("ExplosionHeavy",.85f);hitStop=Mathf.Max(hitStop,.048f);shake=Mathf.Max(shake,.48f);audioDuck=Mathf.Max(audioDuck,.32f);}
  else {if(killGate<=0){killGate=.045f;Sound("Explosion",.56f);}shake=Mathf.Max(shake,.16f);}
  if(Combo>0&&Combo%8==0){Sound("Combo",.5f);Toast("连锁击破  ×"+multiplier,1.3f);}
 }
 void RenderEnemyFeedback(Hostile e){
  float flash=Mathf.Clamp01(e.flash/.095f);e.go.transform.position=e.pos+e.visualOffset+(e.owner!=null?Vector3.up*1.45f:Vector3.zero)+new Vector3(Mathf.Sin(e.age*100)*e.recoil,0,-e.recoil*.65f);
  e.go.transform.rotation=Quaternion.Euler(-e.recoil*9,180,e.kind==9?0:Mathf.Cos(e.age*1.4f)*5+Mathf.Sin(e.age*80)*e.recoil*18);
  for(int i=0;i<e.renderers.Length;i++){var renderer=e.renderers[i];if(!renderer)continue;block.Clear();if(flash>0){Color original=e.baseColors[i];block.SetColor("_Color",Color.Lerp(original,new Color(1,.71f,.36f),flash*.8f));block.SetColor("_EmissionColor",new Color(1.6f,.8f,.3f)*flash*(e.boss?.65f:1));}renderer.SetPropertyBlock(block);}
 }
 Color[] RendererColors(Renderer[] renderers){var colors=new Color[renderers.Length];for(int i=0;i<colors.Length;i++){var mat=renderers[i].sharedMaterial;colors[i]=mat?mat.color:Color.white;if(mat&&!mat.IsKeywordEnabled("_EMISSION")){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",Color.black);}}return colors;}
 void DrawImpactFeedback(){if(State!=FlightState.Playing)return;foreach(var p in scorePops){Vector3 v=Cam.WorldToViewportPoint(p.pos+Vector3.up);if(v.z<0||v.x<.17f||v.x>.83f)continue;float a=Mathf.Min(1,(p.max-p.age)*4);Color c=p.heavy?new Color(1,.76f,.34f,a):new Color(.72f,.98f,1,a);Label(p.text,v.x*1600-140,(1-v.y)*900-16,280,35,p.heavy?22:16,c,TextAnchor.MiddleCenter);}}
}
}
