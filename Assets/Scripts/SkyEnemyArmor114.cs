using UnityEngine;
namespace Skybreak {
public partial class SkyGame {
 static bool HasSiegeArmor(Hostile e)=>!e.boss&&e.owner==null&&(e.kind==2||e.kind==8);
 int armorOpenings,armorBreaks;
 float EnemyArmorScale(Hostile e){if(!HasSiegeArmor(e))return 1;return e.armorBroken?1.12f:e.ventOpen>0?1.70f:.70f;}
 void OpenEnemyRadiators(Hostile e){if(!HasSiegeArmor(e)||e.armorBroken)return;e.ventOpen=e.kind==8?2.2f:1.65f;armorOpenings++;}
 void TickEnemyArmor(Hostile e,float dt){
  e.ventOpen=Mathf.Max(0,e.ventOpen-dt);
  if(HasSiegeArmor(e)&&!e.armorBroken&&e.hp/e.maxHp<=.35f){e.armorBroken=true;armorBreaks++;Burst(e.pos,12,1,.6f);Shockwave(e.pos,Art.Orange,1.7f,.25f);if(killGate<=0){Sound("ArmorBreak114",.48f);killGate=.10f;}}
  var visual=e.go?e.go.GetComponent<SkyEnemyVisual>():null;if(visual==null)return;
  float target=HasSiegeArmor(e)&&(e.ventOpen>0||e.armorBroken)?1:0;e.ventBlend=Mathf.MoveTowards(e.ventBlend,target,dt*5);
  for(int i=0;i<visual.vents.Length;i++)if(visual.vents[i])visual.vents[i].localRotation=visual.ventRest[i]*Quaternion.Euler(0,0,(visual.vents[i].name.StartsWith("Motion_Vent_L")?-1:1)*e.ventBlend*62);
 }
 Hostile NearestArmoredThreat(){Hostile best=null;float bestDistance=1000;foreach(var e in Enemies){if(!HasSiegeArmor(e)||e.retreating||e.pos.z>13||e.pos.z<-10)continue;float d=(e.pos-PlayerPos).sqrMagnitude;if(d<bestDistance){best=e;bestDistance=d;}}return best;}
 string ArmorStatus(Hostile e)=>e==null?"":(e.kind==8?"蜂巢母舰":"铁砧重炮机")+" · "+(e.armorBroken?"装甲破裂":e.ventOpen>0?"散热口暴露 · 集中火力":"重装防护 · 齐射后反击");
 void DrawEnemyArmor114(){if(State!=FlightState.Playing)return;var e=NearestArmoredThreat();if(e==null)return;
  var v=Cam.WorldToViewportPoint(e.pos);if(v.x<.12f||v.x>.88f||v.y<.18f||v.y>.80f)return;
  float x=v.x*1600-105,y=(1-v.y)*900-46;Color c=e.ventOpen>0||e.armorBroken?accent:Art.Orange;
  Label(ArmorStatus(e),x-60,y-19,330,23,12,c,TextAnchor.MiddleCenter);Bar(x,y+6,210,e.hp/e.maxHp,c,3);
 }
}
}
