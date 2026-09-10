using UnityEngine;
using System;
using System.Collections.Generic;
namespace Skybreak {
// Ground operations use the same combat renderer/audio/input bridge, with a
// separate finite mission director. Flight saves and the three-act air campaign
// retain their original progression.
public partial class SkyGame {
 sealed class GroundUnit115 {
  public GameObject go;public Transform turret;public Renderer[] renderers;public Vector3 p,aim,home;public int kind;public float hp,max,age,fire,tell,flash,radius,heading;public bool dead;public LineRenderer line;
 }
 sealed class GroundCover115 {public GameObject go;public Vector3 p;public Vector2 half;public float hp,max;public bool low;}
 sealed class GroundMarker115 {public LineRenderer ring;public Vector3 p;public float age=99,duration,radius;public bool hostile,blast;}
 public bool GroundActive {get;private set;}
 public bool GroundTank {get;private set;}
 public int GroundArea {get;private set;}
 public int GroundRescued {get;private set;}
 public int GroundEnemies=>groundUnits.Count;
 public int GroundCoverCount=>groundCovers.FindAll(c=>c.hp>0).Count;
 public float GroundSkillCooldown=>groundSkill;
 public float GroundUtilityCooldown=>groundUtility;
 public float GroundSwitchCooldown=>groundSwitch;
 public float GroundExitProgress=>groundExit;
 public string GroundForm=>GroundTank?new[]{"隼式突击履带","赤垒攻城履带","霜针狙击履带"}[Ship]:FrameName+" · 地面机甲";
 public string GroundWeapon=>GroundTank?new[]{"双联机炮 / 自动锁定","重型聚爆炮 / 范围破甲","磁轨长炮 / 贯穿"}[Ship]:new[]{"腕炮 / 翼刃突击","重拳机炮 / 近距压制","棱镜步枪 / 精准射击"}[Ship];
 public string GroundSkillName=>new[]{"翼刃环切","震荡齐射","棱镜贯穿"}[Ship];
 public string GroundUtilityName=>GroundTank?"展开防盾":"推进跃迁";
 public string GroundTitle=>new[]{"铁雨登陆","断桥守门人","熔炉外围","最后一枚密钥"}[GroundArea];
 public string GroundObjective {
  get {if(State==FlightState.Victory)return "断链行动完成";if(groundGateOpen)return "前往北侧绿色撤离区 · 停留 1.5 秒";
   if(GroundArea%2==0)return "摧毁供能节点 "+(2-GroundNodeCount)+" / 2 · 清除守军 "+groundUnits.Count;
   return GroundNodeCount>0?"拆除两侧护盾节点 "+(2-GroundNodeCount)+" / 2":"击破 "+(GroundArea==1?"守门人":"冥炉")+" · 炮击后核心暴露";}
 }
 int GroundNodeCount=>groundUnits.FindAll(e=>e.kind==3||e.kind==5).Count;
 GroundUnit115 GroundBoss=>groundUnits.Find(e=>e.kind==4);
 string GroundBrief=>new[]{
  "撤离船队离岸前，天环切断了地面通道。小队降落旧货运港，从这里夺回防空密钥。\n摧毁两座供能节点，清除守军后前往北侧。西侧救援终端可以开启避难所。\nR 切换机甲 / 履带；E 跃迁 / 防盾；自动锁定可见目标。掩体能挡住双方直射弹。",
  "节点里没有防空密码，只有一份被抹去的平民名单。有人把避难所标成了敌方阵地。\n守门人堵住铁路桥：先拆两侧护盾节点，再趁重炮射击后的蓝色散热窗口反击。避开橙色迫击炮落点。",
  "我们找到了真正的指令源：城外的冥炉。它在复制错误识别码，轨道上的天环才会追杀平民。\n摧毁冷却节点并突破防线。左侧维修站里还有被困的工程师，靠近救援终端保持连接即可救出。",
  "敌方指挥官打开了最后一道防线，却把自己的控制权限写进了核心。\n拆除外部护盾，击破冥炉。核心装甲正面更厚，机甲可以绕侧突击；履带可利用掩体和防盾稳定输出。"
 }[GroundArea];
 string GroundOutcome=>GroundRescued==2?"两座避难所已开启。工程师们带回原始识别密钥，证明了那些被标记为敌人的信号来自平民。小队把证据交给空中编队——最后航线，终于有了另一条生路。":GroundRescued==1?"冥炉停止复制错误指令。我们救出了一处避难所的人，但另一处只留下断续的呼救。已找回的密钥将帮助空中编队；失联者的名字也会留在通讯记录中。":"冥炉已经停机，错误指令源被切断。但两处避难所没有接入救援，小队只带回残缺的密钥。天空仍在等待一次更完整的回答。";
 readonly List<GroundUnit115> groundUnits=new List<GroundUnit115>(16);
 readonly List<GroundCover115> groundCovers=new List<GroundCover115>(12);
 readonly List<GroundMarker115> groundMarks=new List<GroundMarker115>(14);
 readonly List<Transform> groundRotors=new List<Transform>();
 readonly List<Transform> groundEvacBuses=new List<Transform>(),groundTransit=new List<Transform>();
 Transform groundRoot,groundMech,groundTank,groundAir,groundLockRing,groundTurret,groundExitRing,groundRescueRing;SkyFrame groundRig;
 Vector2 groundVelocity;Vector3 groundAim=Vector3.forward,groundDashDir;Vector3 groundExitPos=new Vector3(0,.10f,10);
 float groundLanding;
 float groundClock,groundFire,groundSkill,groundUtility,groundSwitch,groundJump,groundGuard,groundExit,groundRescue,groundTransform,groundStride,groundAimYaw,groundPower=1,groundMobility=1,groundFade;
 bool groundPurgeRounds;
 bool groundGateOpen,groundReinforced,groundRescueDone;int groundGuardHits,groundShots,groundCoverHits,groundDodges,groundTransforms;
 public void BeginGround115(){
  EndGround115();EndStory(false);ClearBattle();ResetFeatures();ResetSpecialization();Practice=false;Endless=false;EndlessLoop=0;
  GroundActive=true;GroundArea=0;GroundTank=false;GroundRescued=0;groundPower=groundMobility=1;groundTransforms=groundCoverHits=groundDodges=0;upgradeArmor=upgradeFire=upgradeEnergy=0;
  Score=Kills=Combo=MaxCombo=Grazes=campaignDamage=0;CampaignTime=0;DamageBoost=1;Hull=MaxHull;Bombs=3;Energy=100;earnedSalvage=0;
  if(showcase)showcase.SetActive(false);CreatePlayer();BuildGroundPlayer115();BeginGroundArea115();
 }
 void BuildGroundPlayer115(){
  foreach(var r in Player.GetComponentsInChildren<Renderer>())r.enabled=false;
  foreach(var p in Player.GetComponentsInChildren<ParticleSystem>())p.gameObject.SetActive(false);
  if(frameForm)frameForm.SetActive(false);if(hitPoint)hitPoint.gameObject.SetActive(false);if(focusRing)focusRing.gameObject.SetActive(false);
  groundMech=Art.Model(new[]{"AstraFrame","CrimsonFrame","OracleFrame"}[Ship],Player.transform).transform;groundMech.localScale=Vector3.one*.78f;groundMech.localPosition=Vector3.up*1.66f;
  groundAir=Art.Model(new[]{"Kestrel","Manta","Needle"}[Ship],Player.transform).transform;groundAir.gameObject.SetActive(false);
  groundRig=groundMech.gameObject.AddComponent<SkyFrame>();groundRig.Initialize(Ship);
  groundTank=Art.Model(new[]{"KestrelTank115","MantaTank115","NeedleTank115"}[Ship],Player.transform).transform;groundTank.localScale=Vector3.one*1.10f;
  groundTurret=null;foreach(var t in groundTank.GetComponentsInChildren<Transform>())if(t.name.StartsWith("Motion_Turret"))groundTurret=t;
 }
 void ClearGroundArea115(){
  foreach(var e in groundUnits)if(e.go)Destroy(e.go);groundUnits.Clear();groundCovers.Clear();groundRotors.Clear();groundEvacBuses.Clear();groundTransit.Clear();groundMarks.Clear();
  if(groundRoot){groundRoot.gameObject.SetActive(false);Destroy(groundRoot.gameObject);}groundRoot=null;
  Bullets.Clear();sparks.Clear();ClearSurfaceImpacts();foreach(var s in frameStrokes){s.age=99;if(s.line)s.line.enabled=false;}
 }
 void EndGround115(){if(!GroundActive)return;GroundActive=false;ClearGroundArea115();if(Player){Destroy(Player);Player=null;}groundMech=groundTank=null;groundRig=null;groundTurret=null;if(World)World.SuspendForGround115(false);}
 void BeginGroundArea115(){
  ClearGroundArea115();ClearVoices();ResetMobileInput();Stage=GroundArea/2;StageTime=0;groundClock=groundFire=groundSkill=groundUtility=groundSwitch=groundJump=groundGuard=groundExit=groundRescue=0;
  groundGateOpen=groundReinforced=groundRescueDone=false;groundLanding=1.25f;groundPurgeRounds=false;groundGuardHits=0;groundFade=.8f;groundVelocity=Vector2.zero;groundAim=Vector3.forward;groundAimYaw=0;
  State=FlightState.Briefing;Time.timeScale=1;World.SuspendForGround115(true);World.GroundLighting115(Stage);
  PlayerPos=new Vector3(0,1,-8.8f);Player.transform.position=new Vector3(0,0,-8.8f);Player.transform.rotation=Quaternion.identity;Player.transform.localScale=Vector3.one*.72f;
  groundRoot=new GameObject("Ground operation / "+GroundTitle).transform;groundRoot.gameObject.AddComponent<SkyOwnedResources>();BuildGroundMap115();
  RadioName=PilotNames[Ship]+" / 地面行动";RadioText=GroundBrief;pilotSpeaker=Ship;dialogClock=16;invuln=2;
  SetMusicCue(GroundArea%2==0?"MusicCity":"MusicTempest",GroundArea%2==0?"MusicCityIntensity":null);SetAmbience(1);
  // Fixed initial encounters; no infinite adds or time-only victory gates.
  if(GroundArea%2==0){SpawnGround115(3,new Vector3(-6,1,7));SpawnGround115(3,new Vector3(6,1,8));SpawnGround115(0,new Vector3(-4,1,3));SpawnGround115(1,new Vector3(4,1,5));SpawnGround115(2,new Vector3(1,1,8));}
  else {SpawnGround115(5,new Vector3(-6.5f,1,5));SpawnGround115(5,new Vector3(6.5f,1,5));SpawnGround115(4,new Vector3(0,1,7));SpawnGround115(0,new Vector3(-4,1,0));SpawnGround115(1,new Vector3(4,1,1));}
  UpdateGroundPose115(0,Vector2.zero);
 }
 void GroundRadio115(string line,int actor=-1){ClearVoices();pilotSpeaker=actor<0?Ship:actor;RadioName=PilotNames[Mathf.Clamp(pilotSpeaker,0,2)]+" / 地面通讯";RadioText=line;dialogClock=8;Sound("Click",.22f);}
 public void SwitchGroundForm115(){if(!GroundActive||State!=FlightState.Playing||groundLanding>0||groundSwitch>0||groundJump>0)return;GroundTank=!GroundTank;groundSwitch=1.3f;groundTransform=.42f;groundVelocity*=.35f;groundGuard=0;groundGuardHits=0;groundTransforms++;Sound("FrameDeploy"+Ship,.55f);Burst(PlayerPos,12,0,.44f);}
 public void GroundUtility115(){if(!GroundActive||State!=FlightState.Playing||groundLanding>0||groundUtility>0||groundTransform>0)return;
  if(GroundTank){groundGuard=3;groundGuardHits=6;groundUtility=8;Sound("ImpactArmor",.6f);}
  else {groundJump=.52f;groundDashDir=groundVelocity.sqrMagnitude>.1f?new Vector3(groundVelocity.x,0,groundVelocity.y).normalized:groundAim;groundUtility=4.2f;groundDodges++;Sound("Overdrive",.35f);}
 }
 public void GroundSkill115(){if(!GroundActive||State!=FlightState.Playing||groundLanding>0||groundSkill>0)return;groundSkill=Ship==0?9:Ship==1?12:10;
  Sound(Ship==0?"FrameSlash":Ship==1?"FrameSiege":"FramePrism",.6f);
  if(Ship==0){foreach(var e in groundUnits.ToArray())if(Vector3.Distance(e.p,PlayerPos)<5.2f)DamageGround115(e,180);GroundRing115(PlayerPos,5.1f,.38f,false,false);CancelBullets(false);}
  else if(Ship==1){Vector3 p=GroundTarget115()?.p??PlayerPos+groundAim*5;GroundRing115(p,3.6f,.48f,false,true);}
  else {Vector3 end=GroundRayEnd115(PlayerPos,PlayerPos+groundAim*26,false);FrameRay(PlayerPos,end,Art.Violet,.22f,.4f);foreach(var e in groundUnits.ToArray())if(SegmentDistance(e.p,PlayerPos,end)<e.radius+.2f)DamageGround115(e,270);}
  groundRig.Recoil();
 }
 public void GroundBomb115(){if(!GroundActive||State!=FlightState.Playing||Bombs<=0)return;Bombs--;invuln=1.7f;CancelBullets(false);Sound("NovaCharge",.6f);foreach(var e in groundUnits.ToArray())GroundRing115(e.p,2.1f,.8f,false,true);shake=.15f;}
 void UpdateGround115(float dt){
  if(Input.GetKeyDown(KeyCode.Escape)&&(State==FlightState.Playing||State==FlightState.Paused))Pause();
  if(State==FlightState.Briefing){if(!qaRunning&&(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.Space)))Launch();UpdateCamera(dt);return;}
  if(State!=FlightState.Playing)return;
  if(!qaRunning){if(Input.GetKeyDown(KeyCode.R))SwitchGroundForm115();if(Input.GetKeyDown(KeyCode.E))GroundUtility115();if(Input.GetKeyDown(KeyCode.Q))GroundSkill115();if(Input.GetKeyDown(KeyCode.Space))GroundBomb115();}
  Vector2 movement=qaRunning?Vector2.zero:MobileMode?MobileMovement:Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")),1);
  bool focus=!qaRunning&&(MobileMode?mobileFocus:Input.GetKey(KeyCode.LeftShift));if(!qaRunning)GroundMove115(movement,focus,dt);TickGround115(dt,MobileMode||AutoFire||Input.GetMouseButton(0)||Input.GetKey(KeyCode.Z));UpdateCamera(dt);
 }
 public void GroundMove115(Vector2 move,bool focus,float dt){
  if(groundLanding>0)return;
  float speed=(GroundTank?(Ship==1?4.7f:5.4f):(Ship==2?8.8f:8))*groundMobility*(focus?.5f:1);
  groundVelocity=Vector2.MoveTowards(groundVelocity,move*speed,dt*(GroundTank?22:60));
  Vector3 delta=new Vector3(groundVelocity.x,0,groundVelocity.y)*dt;
  if(groundJump>0)delta=groundDashDir*15*dt;
  Vector3 next=PlayerPos;next.x+=delta.x;if(GroundMoveBlocked115(next,groundJump>0)||(groundJump<=0&&GroundActorBlocked115(next)))next.x=PlayerPos.x;next.z+=delta.z;if(GroundMoveBlocked115(next,groundJump>0)||(groundJump<=0&&GroundActorBlocked115(next)))next.z=PlayerPos.z;
  next.x=Mathf.Clamp(next.x,-9.8f,9.8f);next.z=Mathf.Clamp(next.z,-10,10.4f);PlayerPos=next;moveLast=move;
  if(groundVelocity.sqrMagnitude>.1f)groundStride+=dt*groundVelocity.magnitude;
 }
 bool GroundMoveBlocked115(Vector3 p,bool jumping){foreach(var c in groundCovers)if(c.hp>0&&!(jumping&&c.low)&&Mathf.Abs(c.p.x-p.x)<c.half.x+.45f&&Mathf.Abs(c.p.z-p.z)<c.half.y+.45f)return true;return false;}
 bool GroundActorBlocked115(Vector3 p){foreach(var e in groundUnits)if((e.p-p).sqrMagnitude<(e.radius+.36f)*(e.radius+.36f))return true;return false;}
 void ResolveGroundLanding115(){
  for(int pass=0;pass<3;pass++){
   foreach(var c in groundCovers)if(c.hp>0){float x=PlayerPos.x-c.p.x,z=PlayerPos.z-c.p.z,hx=c.half.x+.47f,hz=c.half.y+.47f;if(Mathf.Abs(x)>=hx||Mathf.Abs(z)>=hz)continue;if(hx-Mathf.Abs(x)<hz-Mathf.Abs(z))PlayerPos.x=c.p.x+(x<0?-hx:hx);else PlayerPos.z=c.p.z+(z<0?-hz:hz);}
   foreach(var e in groundUnits){Vector3 d=PlayerPos-e.p;d.y=0;float r=e.radius+.47f;if(d.magnitude<r){if(d.sqrMagnitude<.001f)d=Vector3.back;PlayerPos=e.p+d.normalized*r;}}
   PlayerPos.x=Mathf.Clamp(PlayerPos.x,-9.8f,9.8f);PlayerPos.z=Mathf.Clamp(PlayerPos.z,-10,10.4f);
  }
 }
 void TickGround115(float dt,bool fire){
  if(groundLanding>0){groundLanding=Mathf.Max(0,groundLanding-dt);UpdateGroundPose115(dt,Vector2.zero);return;}
  groundClock+=dt;StageTime+=dt;CampaignTime+=dt;invuln=Mathf.Max(0,invuln-dt);groundFire-=dt;groundSkill=Mathf.Max(0,groundSkill-dt);groundUtility=Mathf.Max(0,groundUtility-dt);groundSwitch=Mathf.Max(0,groundSwitch-dt);groundTransform=Mathf.Max(0,groundTransform-dt);float jumpBefore=groundJump;groundJump=Mathf.Max(0,groundJump-dt);if(jumpBefore>0&&groundJump<=0)ResolveGroundLanding115();groundGuard=Mathf.Max(0,groundGuard-dt);dialogClock=Mathf.Max(0,dialogClock-dt);groundFade=Mathf.Max(0,groundFade-dt);shake=Mathf.Max(0,shake-dt*2);hurtEdge=Mathf.Max(0,hurtEdge-dt*2);comboClock-=dt;if(comboClock<=0)Combo=0;
  foreach(var s in frameStrokes)if(s.age<s.duration){s.age+=dt;float k=Mathf.Clamp01(1-s.age/s.duration);s.line.startWidth=s.line.endWidth=s.width*k;s.line.enabled=k>0;}
  GroundUnit115 target=GroundTarget115();if(target!=null)groundAim=(target.p-PlayerPos).normalized;groundAim.y=0;groundAimYaw=Mathf.Atan2(groundAim.x,groundAim.z)*Mathf.Rad2Deg;
  if(fire&&groundFire<=0&&target!=null&&groundTransform<=0)GroundFire115();
  TickGroundUnits115(dt);TickGroundRounds115(dt);TickGroundMarkers115(dt);TickSurfaceImpacts(dt);UpdateSparks(dt);UpdateGroundPose115(dt,moveLast);
  foreach(var bus in groundEvacBuses)if(bus&&groundRescueDone)bus.position+=Vector3.forward*dt*2.4f;
  foreach(var car in groundTransit)if(car&&car.position.z<24)car.position+=Vector3.forward*dt*.7f;
  foreach(var t in groundRotors)if(t){if(t.name.Contains("Hoist"))t.localRotation=Quaternion.Euler(0,0,Mathf.Sin(groundClock*.45f)*3);else t.Rotate(0,dt*30,0,Space.Self); }
  if(State!=FlightState.Playing)return;
  if(GroundArea%2==0&&!groundReinforced&&GroundNodeCount<2){groundReinforced=true;SpawnGround115(1,new Vector3(-8,1,9));SpawnGround115(0,new Vector3(8,1,9));GroundRadio115("节点被切断，敌人正从两侧增援。先找掩体，别停在迫击炮的橙色圈里。",(Ship+1)%3);}
  if(!groundGateOpen&&groundUnits.Count==0){groundGateOpen=true;GroundRadio115(GroundArea%2==0?"通道打开了。北侧是绿色撤离区；西侧还有救援终端，离开前可以去确认。":"核心停机。抵达北侧撤离区，我们带着证据离开。",(Ship+2)%3);}
  if(groundExitRing)groundExitRing.gameObject.SetActive(groundGateOpen);
  if(groundGateOpen){groundExit=Vector3.Distance(new Vector3(PlayerPos.x,.10f,PlayerPos.z),groundExitPos)<2?groundExit+dt:0;if(groundExit>=1.5f)CompleteGroundArea115();}
  if(GroundArea%2==0&&!groundRescueDone){Vector3 p=new Vector3(-8,1,-1.0f);groundRescue=Vector3.Distance(PlayerPos,p)<2.1f?groundRescue+dt:0;if(groundRescue>=2.5f){groundRescueDone=true;GroundRescued++;Hull=Mathf.Min(MaxHull,Hull+1);Score+=1200;Sound("Pickup",.7f);GroundRadio115(GroundArea==0?"避难所的门开了！他们不是敌人，是还没来得及撤离的港口工人。":"工程师已安全撤出。他们手里有原始识别密钥——这就是天环追杀平民的证据。",(Ship+1)%3);}}
  if(groundRescueRing)groundRescueRing.gameObject.SetActive(!groundRescueDone&&GroundArea%2==0);
 }
 void CompleteGroundArea115(){
  ResetMobileInput();Bullets.Clear();Hull=Mathf.Min(MaxHull,Hull+1);GroundRadio115("区域安全。");
  if(GroundArea==3){State=FlightState.Victory;SetMusicCue(GroundRescued==2?"MusicDawn":"MusicAftermath");HighScore=Mathf.Max(HighScore,Score);if(!qaRunning){PlayerPrefs.SetInt("ground115_completed",1);PlayerPrefs.SetInt("ground115_best",Mathf.Max(Score,PlayerPrefs.GetInt("ground115_best",0)));PlayerPrefs.SetInt("ground115_rescued",Mathf.Max(GroundRescued,PlayerPrefs.GetInt("ground115_rescued",0)));SaveSettings();}return;}
  if(GroundArea==1){State=FlightState.Upgrade;Bombs=Mathf.Min(5,Bombs+1);return;}
  GroundArea++;BeginGroundArea115();
 }
 void GroundUpgrade115(int n){if(!GroundActive||State!=FlightState.Upgrade||n<0||n>2)return;if(n==0)groundPower=1.22f;else if(n==1){upgradeArmor+=2;Hull=Mathf.Min(MaxHull,Hull+2);}else groundMobility=1.2f;GroundArea=2;BeginGroundArea115();Sound("Pickup",.7f);}
 void GroundHurt115(Vector3 from){if(invuln>0||groundJump>.08f)return;if(groundGuard>0&&groundGuardHits>0&&Vector3.Dot((from-PlayerPos).normalized,groundAim)>.10f){groundGuardHits--;Sound("ImpactArmor",.35f);Burst(PlayerPos+groundAim,4,0,.3f);return;}Hull--;invuln=1.25f;Combo=0;hurtEdge=.6f;shake=.28f;Sound("Hit",.65f);if(Hull<=0){State=FlightState.Defeat;GroundRadio115("机体失去动力。保留现场记录，我们再试一次。",Ship);} }
 void UpdateGroundPose115(float dt,Vector2 movement){
  if(!groundMech||!groundTank)return;bool landing=groundLanding>.48f;groundAir.gameObject.SetActive(landing);if(landing){groundAir.localPosition=new Vector3(0,(groundLanding-.48f)*7,0);groundAir.localRotation=Quaternion.Euler(-groundLanding*8,0,groundLanding*9);}
  float jump=groundJump>0?Mathf.Sin((1-groundJump/.52f)*Mathf.PI)*2:0;
  Player.transform.position=new Vector3(PlayerPos.x,jump,PlayerPos.z);Player.transform.localScale=Vector3.one*.72f;Player.transform.rotation=Quaternion.identity;
  groundMech.gameObject.SetActive(!GroundTank&&!landing);groundTank.gameObject.SetActive(GroundTank&&!landing);
  if(GroundTank){float yaw=groundVelocity.sqrMagnitude>.2f?Mathf.Atan2(groundVelocity.x,groundVelocity.y)*Mathf.Rad2Deg:groundTank.localEulerAngles.y;groundTank.localRotation=Quaternion.Slerp(groundTank.localRotation,Quaternion.Euler(0,yaw,0),1-Mathf.Exp(-dt*6));if(groundTurret)groundTurret.rotation=Quaternion.Euler(0,groundAimYaw+180,0);groundTank.localPosition=Vector3.up*(Mathf.Sin(groundStride*11)*.012f);}
  else {groundMech.localPosition=Vector3.up*1.66f;groundMech.localRotation=Quaternion.Slerp(groundMech.localRotation,Quaternion.Euler(0,groundAimYaw,0),1-Mathf.Exp(-dt*12));groundRig.GroundTick115(dt,groundVelocity.magnitude,groundJump>0);}
  if(groundTransform>0){float squash=Mathf.Sin(groundTransform/.42f*Mathf.PI);(GroundTank?groundTank:groundMech).localPosition=Vector3.up*((GroundTank?0:1.66f)-squash*.14f);}
  if(groundGuard>0&&groundGuardHits>0)FrameRay(PlayerPos+groundAim*1.2f+Vector3.Cross(groundAim,Vector3.up)*1.4f,PlayerPos+groundAim*1.2f-Vector3.Cross(groundAim,Vector3.up)*1.4f,Art.Cyan,.07f,.07f);
 }
}
}
