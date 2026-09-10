using UnityEngine;
namespace Skybreak {
public partial class SkyGame {
 void DrawGround115(){
  if(State==FlightState.Briefing){Rect(0,0,1600,900,new Color(.005f,.013f,.020f,.86f));Portrait(Ship,70,175,380,610);SmallTag("GROUND OPERATIONS / 断链行动",516,138,410,accent);Label("0"+(GroundArea+1)+" / "+GroundTitle,513,195,950,70,43,paper);Label(GroundBrief,518,296,930,275,22,paper);Label("WASD 移动   ·   R 变形   ·   E 跃迁 / 防盾   ·   Q 特技   ·   SPACE 支援轰击",518,613,940,70,18,accent);if(Button("确认登陆  →",518,712,370,64,true))Launch();if(Button("返回机库",917,712,235,64))SetupHangar();return;}
  Rect(0,0,1600,74,ink);Label("断链行动 / "+GroundTitle,30,17,500,35,23,paper);Label(GroundObjective,530,23,890,38,18,accent,TextAnchor.MiddleCenter);if(Button("Ⅱ",1470,11,90,49))Pause();
  Panel(22,98,226,171);Label(GroundForm,39,112,196,40,19,paper);Label("装甲 "+Hull+" / "+MaxHull,39,160,196,30,19,accent);Bar(40,204,184,Hull/(float)MaxHull,accent,7);Label("得分 "+Score.ToString("D7"),39,225,196,27,16,muted);
  var boss=GroundBoss;if(boss!=null){Panel(1352,100,225,155);Label(GroundArea==1?"守门人 / 攻城机兵":"冥炉 / 指令核心",1371,117,184,44,20,Art.Orange);Bar(1371,171,184,boss.hp/boss.max,Art.Orange,6);Label(GroundNodeCount>0?"护盾节点仍在供能":boss.fire>2.6f&&boss.tell<=0?"散热开启 · 伤害提升":"正面装甲 · 侧后方较弱",1371,198,184,48,16,paper);}
  else {Panel(1352,101,225,151);Label("战场状态",1370,117,185,28,16,muted);Label("敌军 "+groundUnits.Count+"  /  掩体 "+GroundCoverCount,1370,152,185,32,18,paper);Label("救援 "+GroundRescued+" / 2",1370,198,185,25,16,accent);}
  if(GroundArea%2==0&&!groundRescueDone){Panel(23,294,225,102);Label("西侧蓝圈 · 连接救援终端",39,306,190,46,17,accent);Bar(39,372,189,groundRescue/2.5f,accent,5);}
  if(groundGateOpen){Panel(500,96,600,77);Label("北侧绿色撤离区 · 停留完成转移",518,109,564,33,20,new Color(.32f,.95f,.65f),TextAnchor.MiddleCenter);Bar(521,158,555,groundExit/1.5f,accent,4);}
  Panel(24,658,329,178);Label(GroundWeapon,42,674,296,42,18,paper);Label("R  "+(GroundTank?"展开地面机甲":"折叠为履带形态")+(groundSwitch>0?" · 冷却":""),42,727,296,31,17,accent);Label("E  "+GroundUtilityName+"  "+(groundUtility>0?groundUtility.ToString("F1")+"s":"就绪")+(groundGuard>0?" / 盾 "+groundGuardHits:""),42,771,296,43,17,paper);
  Panel(1260,659,317,176);Label("Q  "+GroundSkillName,1279,678,273,37,20,paper);Label(groundSkill>0?"冷却 "+groundSkill.ToString("F1")+" 秒":"特技就绪",1279,723,273,28,17,accent);Label("SPACE 支援轰击  ×"+Bombs,1279,780,273,34,18,paper);
  if(dialogClock>0&&State==FlightState.Playing){Panel(23,418,225,214);Portrait(pilotSpeaker,28,420,87,80,false,false,true);Label(RadioName,119,432,115,62,14,accent);Label(RadioText,39,508,193,116,16,paper);}
  Rect(0,856,1600,44,ink);Label("自动锁定可见敌人  ·  SHIFT 精准移动  ·  掩体挡住双方直射弹  ·  橙圈是迫击炮预警",28,869,1450,27,15,muted);
  if(State==FlightState.Paused){Rect(0,0,1600,900,new Color(0,0,0,.70f));Panel(523,239,554,418);Label("地面行动暂停",560,280,478,55,35,paper);if(Button("继续行动",560,375,478,64,true))Pause();if(Button("重新开始断链行动",560,460,478,56))BeginGround115();if(Button("返回机库",560,537,478,56))SetupHangar();}
  if(State==FlightState.Upgrade){Rect(0,0,1600,900,new Color(.002f,.006f,.012f,.93f));Label("越过断桥 · 为下一章整备",120,171,1360,66,41,paper);Label("原始识别密钥藏在城外冥炉。选择强化，继续地面突击。",120,254,1360,44,23,muted);string[] names={"高压火控","附加复合装甲","轻型执行器"},descriptions={"所有地面武器伤害 +22%","装甲上限 +2，并立即修复","两种地面形态移动速度 +20%"};for(int i=0;i<3;i++){float x=120+i*465;Panel(x,366,430,286);Label(names[i],x+26,400,378,44,28,accent);Label(descriptions[i],x+26,460,378,77,21,paper);if(Button("装配并继续 →",x+26,562,378,57,true))GroundUpgrade115(i);}return;}
  if(State==FlightState.Victory||State==FlightState.Defeat){Rect(0,0,1600,900,new Color(.003f,.01f,.018f,.93f));Portrait(Ship,97,218,355,595);Label(State==FlightState.Victory?"断链行动完成":"机体失去动力",491,160,1000,68,43,paper);Label(State==FlightState.Victory?GroundOutcome:"掩体可以替你承受直射弹，机甲的推进跃迁能避开迫击炮。尝试切换形态，再次突破。",496,273,941,196,23,paper);Label("救援 "+GroundRescued+" / 2    ·    击破 "+Kills+"    ·    得分 "+Score.ToString("N0"),496,504,941,44,23,accent);if(Button("再次出击",496,641,340,67,true))BeginGround115();if(Button("返回机库",872,641,340,67))SetupHangar();}
 }
 void PopulateGroundStatus115(MobileSnapshot s){
  s.ground=true;s.groundTank=GroundTank;s.groundArea=GroundArea;s.groundRescued=GroundRescued;s.groundEnemies=groundUnits.Count;s.groundCover=GroundCoverCount;s.groundGuard=groundGuardHits;s.groundSwitch=groundSwitch;s.groundUtility=groundUtility;s.groundExit=groundExit/1.5f;s.groundJump=groundJump;s.groundCoverHits=groundCoverHits;s.groundTransforms=groundTransforms;s.groundDodges=groundDodges;
  s.stage=GroundTitle;s.stageIndex=GroundArea;s.mission=GroundObjective;s.encounter=GroundForm;s.weapon=GroundWeapon;s.skill=GroundSkillName;s.route="断链行动";s.routeTier=GroundArea/2+1;s.routeNode="R 变形 · E "+GroundUtilityName;s.routeNext="掩体挡弹；机甲跃迁越障，履带防盾承伤";s.routeProgress=GroundArea%2==0?groundRescue/2.5f:groundExit/1.5f;s.convoy=null;
  s.skillCooldown=groundSkill;s.canSkill=State==FlightState.Playing&&groundSkill<=0;s.canBomb=State==FlightState.Playing&&Bombs>0;s.canOverdrive=State==FlightState.Playing&&groundUtility<=0&&groundTransform<=0;s.overdrive=0;s.energy=100;s.frameName=GroundUtilityName;s.shields=groundGuard>0?groundGuardHits:0;
  s.radioName=RadioName;s.radioText=RadioText;s.speaker=pilotSpeaker;s.radio=State==FlightState.Playing&&dialogClock>0;s.voicePlaying=false;s.voiceId="";s.armoredThreat="";s.armoredHealth=0;
  s.practice=false;s.ending="断链行动完成";s.outcome=GroundOutcome;s.debrief="越过断桥："+(GroundRescued>0?"避难所已开启，工人们正在撤离。":"救援终端未接通。")+"下一步攻入冥炉，切断错误指令。";
  var boss=GroundBoss;s.boss=boss==null?"":GroundArea==1?"守门人":"冥炉";s.bossHealth=boss==null?0:boss.hp/boss.max;s.bossModules=GroundNodeCount;s.bossTelegraph=boss!=null&&boss.tell>0;s.bossInstruction=boss==null?"":GroundNodeCount>0?"先拆除两侧护盾节点":boss.fire>2.6f&&boss.tell<=0?"散热窗口 · 核心易伤":"避开橙圈 · 从侧后方攻击";
  if(State==FlightState.Upgrade){s.upgrades=new[]{new MobileChoice{index=0,name="高压火控",description="地面武器伤害 +22%",detail="装配并突入冥炉"},new MobileChoice{index=1,name="复合装甲",description="装甲上限 +2，并修复",detail="装配并突入冥炉"},new MobileChoice{index=2,name="轻型执行器",description="机甲与履带移动速度 +20%",detail="装配并突入冥炉"}};}
 }
 bool GroundMobileAction115(string action){if(!GroundActive)return false;
  if(action=="release"){ResetMobileInput();return true;}if(action=="pause"&&State==FlightState.Playing)Pause();else if(action=="resume"&&State==FlightState.Paused)Pause();else if(action=="launch")Launch();
  else if(action=="home"&&(State==FlightState.Paused||State==FlightState.Victory||State==FlightState.Defeat||State==FlightState.Briefing))SetupHangar();
  else if(action=="retry"&&(State==FlightState.Paused||State==FlightState.Victory||State==FlightState.Defeat))BeginGround115();
  else if(action.StartsWith("upgrade:")&&State==FlightState.Upgrade){if(int.TryParse(action.Substring(8),out int n))GroundUpgrade115(n);}
  else if(State==FlightState.Playing){if(action=="weapon")SwitchGroundForm115();else if(action=="skill")GroundSkill115();else if(action=="overdrive")GroundUtility115();else if(action=="bomb")GroundBomb115();else if(action=="focus")mobileFocus=!mobileFocus;}
  return true;
 }
}
}
