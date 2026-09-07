using UnityEngine;
namespace Skybreak {public partial class SkyGame {
 SkyBossMechanisms bossMechanisms;LineRenderer coreShield;string bossOrder="";float bossTell;int bossShots;float bossShotClock;
 public int BossModuleCount {get{int n=0;foreach(var e in Enemies)if(e.owner==Boss&&e.owner!=null&&e.hp>0)n++;return n;}}
 float BossDamageScale(Hostile e)=>e.boss?(BossModuleCount>0?.28f:e.coreExpose>0?1.65f:1):1;
 void InitBossEncounter(){bossMechanisms=Boss.go.AddComponent<SkyBossMechanisms>();bossMechanisms.Initialize();bossTell=0;bossShots=0;Boss.attackClock=1.3f;Boss.bossAttackIndex=-1;Boss.coreExpose=0;
  coreShield=Art.Ring(Boss.go.transform,1.15f,Art.Cyan,.075f,96);coreShield.transform.localPosition=Vector3.up*.7f;
  int count=Stage==2?3:2;for(int i=0;i<count;i++){var offset=Stage==2?new Vector3(Mathf.Cos(i*Mathf.PI*2/3)*3.35f,0,Mathf.Sin(i*Mathf.PI*2/3)*2):new Vector3(i==0?-3.25f:3.25f,0,-.3f);
   var node=SpawnEnemy(2,Boss.pos+offset,0);Destroy(node.go);node.go=Art.Model(Stage==2?"ShieldEmitter":"SiegeTurret",null);node.go.transform.localScale=Vector3.one*(Stage==2?.9f:1.05f);node.go.transform.position=node.pos;node.owner=Boss;node.mountOffset=offset;node.hp=node.maxHp=Stage==0?210:Stage==1?270:220;node.fire=2.6f+i*.5f;node.renderers=node.go.GetComponentsInChildren<Renderer>();node.baseColors=RendererColors(node.renderers);
  }
  bossOrder=Stage==0?"拆除左右炮台，击穿海上要塞":Stage==1?"摧毁两座涡轮，关闭风暴护盾":"破坏三枚轨道节点，暴露天环核心";
 }
 void ClearBossEncounter(){bossMechanisms=null;if(coreShield)Destroy(coreShield.gameObject);bossTell=0;bossShots=0;bossOrder="";}
 void BossModuleDestroyed(Hostile e){if(e.owner==null||e.owner!=Boss)return;Shockwave(e.pos,Art.Orange,5,.6f);Toast("武装破坏 · 火力压制减弱",2);if(BossModuleCount==0){Boss.coreExpose=8;MissionSay("core_exposed",95,()=>Boss!=null&&Boss.coreExpose>0,false);bossOrder="护盾崩溃 · 核心易伤 8 秒";Sound("BossBreak",.45f);CancelBullets(false);shake=.7f;Shockwave(Boss.pos,Art.Cyan,10,.75f);}}
 void TickBossModule(Hostile e,float dt){if(e.owner==null||!e.owner.go||e.owner.hp<=0){Destroy(e.go);Enemies.Remove(e);return;}Vector3 offset=e.mountOffset;
  if(Stage==2)offset=Quaternion.Euler(0,Mathf.Sin(e.age*.35f)*28,0)*offset;
  e.pos=e.owner.pos+offset;UpdateWeaponTell(e);if(e.fire<=0&&e.pos.z<12){EnemyAttack(e);FinishWeaponTell(e);e.fire=(Stage==1?1.65f:2.3f)/(Difficulty==2?1.15f:1);}RenderEnemyFeedback(e);
 }
 void TickBossEncounter(Hostile e,int phase,float dt){if(bossMechanisms)bossMechanisms.Tick(e.age,phase,e.coreExpose>0,bossTell>0);e.coreExpose=Mathf.Max(0,e.coreExpose-dt);if(coreShield){coreShield.enabled=BossModuleCount>0;coreShield.transform.localRotation=Quaternion.Euler(Mathf.Sin(e.age)*20,0,Mathf.Cos(e.age*.7f)*20);coreShield.transform.localScale=Vector3.one*(1+Mathf.Sin(e.age*4)*.045f);}
  if(e.pos.z>9)return;e.attackClock-=dt;
  if(bossTell>0){bossTell-=dt;if(bossTell<=0){bossShots=phase+2;bossShotClock=0;}}
  if(bossShots>0){bossShotClock-=dt;if(bossShotClock<=0){FireBossSetpiece(e,phase,e.bossAttackIndex,bossShots);bossShots--;bossShotClock=.26f;}}
  if(e.attackClock<=0&&bossShots==0&&bossTell<=0){e.bossAttackIndex=(e.bossAttackIndex+1)%4;e.lockedAim=PlayerPos;bossTell=Difficulty==0?1.35f:1.05f;e.attackClock=bossTell+(phase==0?3.8f:phase==1?3.15f:2.65f);bossOrder=BossAttackName(e.bossAttackIndex);Sound("Warning",.27f);
   if(e.bossAttackIndex==2){float safe=Stage==2?Mathf.Clamp(e.lockedAim.x,-6,6):((e.bossAttackIndex+phase)%2==0?-4:4);for(int i=-2;i<=2;i++)if(Mathf.Abs(i*4-safe)>2.2f)AddBeam(i*4);}
  }
 }
 string BossAttackName(int attack){string[,] names={{"鱼雷扇面 · 横向穿越缝隙","交错炮击 · 离开锁定航线","封锁轰炸 · 寻找安全通道","护航拦截 · 清理侧翼"},{"双涡旋 · 穿过旋转缺口","风暴矛 · 锁定后侧移","电弧封锁 · 躲进空白航道","雷暴花环 · 小幅精确闪避"},{"六翼星芒 · 读出弹幕缺口","零点追猎 · 离开标记位置","裁决光栅 · 保持在安全带","恒星坍缩 · 交错环形弹幕"}};return names[Stage,attack];}
 void FireBossSetpiece(Hostile e,int phase,int attack,int volley){float speed=(5.2f+phase*.65f)*DifficultySpeed;int density=Difficulty==0?0:Difficulty==1?2:4;
  if(attack==0){if(Stage==0){for(int side=-1;side<=1;side+=2){Vector3 p=e.pos+new Vector3(side*2.7f,0,-1);for(int j=-3-density/2;j<=3+density/2;j++){float angle=j*13+side*volley*4;AddBullet(p,Quaternion.Euler(0,angle,0)*Vector3.back*speed,false,1,1,.19f);}}}
   else {int n=(Stage==1?15:23)+density+phase*3;for(int side=-1;side<=1;side+=2)for(int j=0;j<n;j++){float a=j*Mathf.PI*2/n+side*e.age*.22f+volley*.07f;if(Mathf.Abs(Mathf.DeltaAngle(a*Mathf.Rad2Deg,180+Mathf.Sin(e.age*.4f)*35))<18)continue;AddBullet(e.pos+new Vector3(side*(Stage==1?2.8f:1.2f),0,0),new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*speed,false,side<0?1:2,1,.17f,false,side*(Stage==1?9:4));}}
  }else if(attack==1){for(int side=-1;side<=1;side+=2){Vector3 p=e.pos+new Vector3(side*3,0,-.5f);Vector3 aim=(e.lockedAim-p).normalized;for(int k=-2-phase;k<=2+phase;k++)AddBullet(p,Quaternion.Euler(0,k*10+side*(volley-1)*2,0)*aim*(speed+2),false,1,1,.19f);}}
  else if(attack==2){for(int k=-5;k<=5;k++){float x=k*1.8f;if(Mathf.Abs(x-e.lockedAim.x)<1.5f)continue;AddBullet(new Vector3(x,1,12),Vector3.back*(speed*.8f),false,2,1,.18f);}}
  else if(Stage==0){if(volley==phase+2)for(int side=-1;side<=1;side+=2)SpawnEnemy(1,new Vector3(side*8,1,17),5);for(int j=0;j<18+density;j++){float a=j*Mathf.PI*2/(18+density)+volley*.13f;AddBullet(e.pos,new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*speed,false,1);}}
  else {int n=Stage==1?20:28;for(int j=0;j<n;j++){float a=j*Mathf.PI*2/n+volley*.11f;AddBullet(e.pos,new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*speed,false,2,1,.16f,false,(volley%2==0?1:-1)*7);}}
 }
 void DrawBossMechanic(){if(Boss==null||State!=FlightState.Playing)return;string state=BossModuleCount>0?"护盾在线 · 剩余武装 "+BossModuleCount:Boss.coreExpose>0?"核心暴露 · 伤害 ×1.65  "+Boss.coreExpose.ToString("F1")+"s":"核心失去保护";Label(state,430,84,740,29,17,BossModuleCount>0?Art.Cyan:Art.Orange,TextAnchor.MiddleCenter);
  Label(bossOrder,394,120,812,38,bossTell>0?21:16,bossTell>0?Art.Orange:paper,TextAnchor.MiddleCenter);if(bossTell>0)Bar(600,161,400,bossTell/(Difficulty==0?1.35f:1.05f),Art.Orange,3);
  foreach(var e in Enemies)if(e.owner==Boss){Vector3 v=Cam.WorldToViewportPoint(e.pos+Vector3.up*1.45f);Bar(v.x*1600-40,(1-v.y)*900+35,80,e.hp/e.maxHp,Art.Orange,4);}
 }
}}
