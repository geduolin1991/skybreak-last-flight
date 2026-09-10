using UnityEngine;
using System.Collections.Generic;
namespace Skybreak {public partial class SkyGame {
 float counterWindow;bool counterUsed;
 readonly List<LineRenderer> counterLinks=new List<LineRenderer>(3);
 public float CounterWindow=>counterWindow;
 public bool CounterUnlocked=>Stage==0?ConvoySurvivors==3:Stage==1?missionNodes==3:World&&World.UplinkComplete;
 public string CounterTitle=>Stage==0?"船队测距 · 要塞弱点暴露":Stage==1?"电网反相 · 风暴护盾失效":"名册回响 · 战争授权撤销";
 public string BossInstruction=>counterWindow>0?CounterTitle+"  "+counterWindow.ToString("F1")+"s":bossOrder;
 void ClearCounterplay(){counterWindow=0;counterUsed=false;foreach(var line in counterLinks)if(line)Destroy(line.gameObject);counterLinks.Clear();}
 void TickCounterplay(float dt){
  if(Boss==null)return;
  counterWindow=Mathf.Max(0,counterWindow-dt);
  if(!counterUsed&&bossPhase>=1&&CounterUnlocked){
   counterUsed=true;counterWindow=Stage==2?7:Stage==1?6:5;
   Boss.coreExpose=Mathf.Max(Boss.coreExpose,counterWindow);Energy=Mathf.Min(100,Energy+25);
   CancelBullets(false);Toast(CounterTitle+" · 核心伤害 ×1.65",3.5f);MissionSay("frame113_counter_"+Stage,99,()=>Boss!=null&&counterWindow>0,false);
   Sound("ObjectiveChime",.55f);Shockwave(Boss.pos,new Color(.50f,.88f,.74f),4,.65f);
   for(int i=0;i<3;i++){var l=Art.Line(null,"Civilian counter signal "+i,new Vector3[2],new Color(.17f,.56f,.58f),.032f);l.useWorldSpace=true;counterLinks.Add(l);}
  }
  for(int i=0;i<counterLinks.Count;i++){
   var line=counterLinks[i];line.enabled=counterWindow>0;if(!line.enabled)continue;
   Vector3 origin=Stage==0&&i<World.Civilians.Count&&World.Civilians[i].go?World.Civilians[i].go.transform.position:new Vector3((i-1)*6,-9,Stage==1?5:8);
   line.SetPosition(0,origin+Vector3.up*.30f);line.SetPosition(1,Boss.pos+Vector3.up*.1f);
   line.startWidth=line.endWidth=.025f+.008f*Mathf.Sin(StageTime*5+i);
  }
 }
}}
