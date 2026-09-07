using UnityEngine;
namespace Skybreak {
public partial class SkyGame {
 readonly int[] selectedRoutes=new int[3];int routeExperience,routeTier=1,routeShotCount,routeShield;float routeShieldClock,routePulseClock,routeRepairClock;
 LineRenderer routeField;public int CurrentRoute=>selectedRoutes[Ship];public int RouteTier=>routeTier;
 readonly string[,] routeNames={{"蜂群猎手","护航指挥","疾风突袭"},{"攻城熔炉","不动壁垒","红莲反应堆"},{"终点校准","时间守望","相位游猎"}};
 readonly string[,] routeSkills={{"蜂群齐射","双机护航","疾风突进"},{"熔炉轰击","装甲壁垒","红莲解限"},{"处决轨道炮","时域静止","相位跃迁"}};
 readonly string[,] routeDescriptions={
  {"追击导弹锁定散兵，主炮更快。\nQ：贯穿蜂群沿整条航线追击。","移动护盾抵消命中，强化队友。\nQ：召回双机并重建护盾。","高速侧移、掠过敌弹积蓄能量。\nQ：短暂无敌突进，留下脉冲航迹。"},
  {"爆炸半径扩大，持续压制重甲。\nQ：前方熔炉区域持续引爆。","增加装甲，正面拦截敌弹。\nQ：修复装甲，展开五秒防壁。","提高连射与机动，以连续火力压制。\nQ：七秒解限，加速发射燃烧弹。"},
  {"蓄能炮更强，惩罚暴露与残血核心。\nQ：按目标损伤追加处决伤害。","减速附近弹幕，护盾自动恢复。\nQ：延长零时领域并清除近身威胁。","机动更快，擦弹加速技能循环。\nQ：沿移动方向跃迁并发射相位弹。"}
 };
 readonly string[,,] routeNodes={
  {{"双联炮射速 +8%","每六轮追加追踪弹","蜂群数量与贯穿提升"},{"一层再生护盾","支援时间与火力提升","护航技能冷却缩短"},{"机动 +12%","擦弹额外获得能量","突进脉冲伤害翻倍"}},
  {{"爆炸半径 +20%","聚爆伤害提升","熔炉持续四秒"},{"初始装甲 +2","正面拦截次数增加","防壁期间缓慢修复"},{"射速 +10%","解限期间机动提升","额外燃烧导弹"}},
  {{"轨道炮伤害 +16%","精准飞行伤害提升","残血目标追加伤害"},{"近身弹幕减速","一层再生护盾","零时领域延长"},{"机动 +10%","擦弹缩短技能冷却","跃迁追加两枚相位弹"}}
 };
 string RouteName=>routeNames[Ship,CurrentRoute];string ActiveSkillName=>routeSkills[Ship,CurrentRoute];
 float RouteDamage=>CurrentRoute==0?1+(Ship==2?.16f:.06f)*RouteTier:1;
 float RouteCadence=>Ship==0&&CurrentRoute==0?1.08f:Ship==1&&CurrentRoute==2?1.1f+(RouteTier-1)*.035f:1;
 float RouteMobility=>CurrentRoute==2?1+(Ship==1?.07f:.10f)+RouteTier*.02f+(SkillTime>0?.17f:0):1;
 int RouteArmor=>Ship==1&&CurrentRoute==1?2:0;
 void LoadSpecializations(){for(int i=0;i<3;i++)selectedRoutes[i]=Mathf.Clamp(PlayerPrefs.GetInt("specialization-"+i,0),0,2);}
 public void SelectSpecialization(int route){selectedRoutes[Ship]=Mathf.Clamp(route,0,2);if(!qaRunning){PlayerPrefs.SetInt("specialization-"+Ship,CurrentRoute);PlayerPrefs.Save();}Sound("Click",.7f);}
 void ResetSpecialization(){routeExperience=0;routeTier=1;routeShotCount=0;routeShield=CurrentRoute==1&&Ship==0?1:0;routeShieldClock=0;ClearRouteField();}
 void ClearRouteField(){if(routeField)Destroy(routeField.gameObject);routeField=null;routePulseClock=0;}
 void GainRouteExperience(int amount){routeExperience+=amount;int next=routeExperience>=85?3:routeExperience>=30?2:1;if(next>routeTier){routeTier=next;Toast(RouteName+" · 专精 "+routeTier+" 阶已生效",3);Shockwave(PlayerPos,ShipColor,2.5f,.6f);MissionSay("route_up_"+Ship,63,null,false);}}
 void TickSpecialization(float dt){
  if(CurrentRoute==1){routeShieldClock+=dt;int max=Ship==1?2+RouteTier:Ship==0?RouteTier:RouteTier>=2?1:0;float interval=Ship==1?5.5f:12;
   if(routeShieldClock>=interval){routeShieldClock=0;routeShield=Mathf.Min(max,routeShield+1);}
   if(!routeField)routeField=Art.Ring(null,Ship==1?1.6f:1.1f,new Color(ShipColor.r,ShipColor.g,ShipColor.b,.45f),.035f);routeField.transform.position=PlayerPos+Vector3.up*.12f;routeField.enabled=routeShield>0||SkillTime>0;
   if(Ship==1)for(int i=Bullets.Count-1;i>=0;i--){var b=Bullets[i];Vector3 d=b.pos-PlayerPos;if(b.friendly||d.z<.45f||d.sqrMagnitude>3.1f)continue;if(routeShield<=0&&SkillTime<=0)break;Bullets.RemoveAt(i);if(SkillTime<=0)routeShield--;Burst(b.pos,5,0,.35f);}
  }
  if(SkillTime>0){if(Ship==1&&CurrentRoute==1&&RouteTier==3){routeRepairClock+=dt;if(routeRepairClock>=2.4f){routeRepairClock=0;Hull=Mathf.Min(MaxHull,Hull+1);Burst(PlayerPos,8,6,.25f);}}routePulseClock-=dt;if(routePulseClock<=0){routePulseClock=.42f;
    if(Ship==1&&CurrentRoute==0){Vector3 center=PlayerPos+Vector3.forward*5;Shockwave(center,Art.Orange,4+RouteTier*.5f,.5f);foreach(var e in Enemies.ToArray())if((e.pos-center).sqrMagnitude<36)DamageEnemy(e,28+RouteTier*6);Burst(center,18,1,.65f);}
    if(Ship==0&&CurrentRoute==2){Shockwave(PlayerPos,Art.Cyan,2.5f,.35f);foreach(var e in Enemies.ToArray())if((e.pos-PlayerPos).sqrMagnitude<9)DamageEnemy(e,RouteTier==3?50:25);}
    if(Ship==1&&CurrentRoute==2&&RouteTier>=2)for(int s=-1;s<=1;s+=2)AddBullet(PlayerPos+Vector3.right*s,Vector3.forward*30,true,3,12,.15f,true,kind:4);
   }}
 }
 bool AbsorbRouteHit(){if(CurrentRoute!=1||routeShield<=0||Ship==1)return false;routeShield--;invuln=.75f;Shockwave(PlayerPos,ShipColor,2,.35f);Sound("ImpactArmor",.6f);Toast("护盾抵消命中",1.2f);return true;}
 float RouteBulletSpeed(Round b){if(b.friendly)return 1;if(Ship==2&&SkillTime>0)return CurrentRoute==1?.18f:.32f;if(Ship==2&&CurrentRoute==1&&(b.pos-PlayerPos).sqrMagnitude<16)return .76f;return 1;}
 void RouteGraze(){if(CurrentRoute==2){Energy=Mathf.Min(100,Energy+(Ship==0&&RouteTier>=2?2:.5f));if(Ship==2&&RouteTier>=2)SkillCooldown=Mathf.Max(0,SkillCooldown-.22f);}}
 void RouteFire(float damage){routeShotCount++;if(Ship==0&&CurrentRoute==0&&RouteTier>=2&&routeShotCount%6==0)for(int s=-1;s<=1;s+=2)AddBullet(PlayerPos+Vector3.right*s*.9f,new Vector3(s*8,0,27),true,0,10*damage,.13f,true,pierce:RouteTier==3?1:0,kind:2);}
 void ApplyRouteSkill(){
  SkillCooldown=(Ship==0?20:Ship==1?23:24)*Mathf.Pow(.8f,temporalLevel)*(1-researchSkill*.05f)*(CurrentRoute==2?.8f:1)*(Ship==0&&CurrentRoute==1&&RouteTier==3?.8f:1);routePulseClock=routeRepairClock=0;
  if(Ship==0){
   if(CurrentRoute==0){SkillTime=2;for(int i=0;i<16+RouteTier*2;i++){float a=i*Mathf.PI*2/(16+RouteTier*2);AddBullet(PlayerPos,new Vector3(Mathf.Sin(a)*17,0,Mathf.Cos(a)*17),true,0,18+RouteTier*3,.13f,true,pierce:RouteTier==3?2:1,kind:2);}CancelBullets(true);}
   else if(CurrentRoute==1){SkillTime=3;routeShield=RouteTier+1;CallSquadron();supportRemaining=Mathf.Max(supportRemaining,12+RouteTier*3);CancelBullets(true);}
   else {SkillTime=3;invuln=1.8f;Vector3 d=new Vector3(moveLast.x,0,moveLast.y);if(d.sqrMagnitude<.1f)d=Vector3.forward;PlayerPos+=d.normalized*3;ClampRoutePosition();}
  }else if(Ship==1){
   if(CurrentRoute==0){SkillTime=RouteTier==3?4:2.5f;foreach(var e in Enemies.ToArray())if(e.pos.z>PlayerPos.z)DamageEnemy(e,e.boss?150:95);CancelBullets(true);}
   else if(CurrentRoute==1){SkillTime=5;routeShield=3+RouteTier;Hull=Mathf.Min(MaxHull,Hull+1);CancelBullets(true);}
   else {SkillTime=7;invuln=1.4f;CancelBullets(true);}
  }else {
   if(CurrentRoute==0){SkillTime=2;FireRail(110+RouteTier*25,true);foreach(var e in Enemies.ToArray())if(e.pos.z>PlayerPos.z&&Mathf.Abs(e.pos.x-PlayerPos.x)<1.7f)DamageEnemy(e,Mathf.Min(260,(1-e.hp/e.maxHp)*220));}
   else if(CurrentRoute==1){SkillTime=5+RouteTier*.6f;routeShield=1;for(int i=Bullets.Count-1;i>=0;i--)if(!Bullets[i].friendly&&(Bullets[i].pos-PlayerPos).sqrMagnitude<20)Bullets.RemoveAt(i);}
   else {SkillTime=3;Vector3 d=new Vector3(moveLast.x,0,moveLast.y);if(d.sqrMagnitude<.1f)d=Vector3.forward;var before=PlayerPos;PlayerPos+=d.normalized*4.5f;ClampRoutePosition();Shockwave(before,Art.Violet,2.3f,.4f);invuln=2;for(int i=-(RouteTier==3?3:2);i<=(RouteTier==3?3:2);i++)AddBullet(PlayerPos+Vector3.right*i*.28f,Vector3.forward*30,true,2,30,.14f,true,pierce:1,kind:2);}
  }
 }
 void ClampRoutePosition(){PlayerPos.x=Mathf.Clamp(PlayerPos.x,-10,10);PlayerPos.z=Mathf.Clamp(PlayerPos.z,-10,10);if(Player)Player.transform.position=PlayerPos;}
 void DrawSpecializationHUD(){if(State!=FlightState.Playing)return;Label(RouteName+"  "+RouteTier+" 阶",1370,598,190,26,13,ShipColor);Bar(1370,631,188,RouteTier==3?1:routeExperience/(RouteTier==1?30f:85f),ShipColor,3);}
 void DrawSpecializations(){
  Panel(258,97,1278,737);SmallTag("AIRFRAME SPECIALIZATION",292,124,257,ShipColor);Label(ShipNames[Ship]+" · 战术专精",292,173,1144,66,42,paper);Label("出击前选择一条路线。击破敌人与完成民用目标积累经验，在本次行动中升至三阶。",295,253,1174,56,18,muted);
  for(int route=0;route<3;route++){float x=293+route*403;bool selected=route==CurrentRoute;Panel(x,338,378,387);Rect(x,338,378,3,selected?ShipColor:muted*.5f);SmallTag("0"+(route+1)+(selected?" / 已装配":" / 可装配"),x+20,359,174,selected?ShipColor:muted);Label(routeNames[Ship,route],x+20,408,338,49,30,paper);Label(routeDescriptions[Ship,route],x+20,469,337,91,16,muted);
   for(int tier=0;tier<3;tier++){float y=575+tier*36;Rect(x+21,y+5,7,7,selected?ShipColor:muted);Label((tier+1)+" 阶  "+routeNodes[Ship,route,tier],x+40,y-2,317,29,14,paper);}
   if(Button(selected?"已装配 · "+routeSkills[Ship,route]:"装配这条路线 →",x+19,678,340,44,selected))SelectSpecialization(route);
  }
  Label("无需解锁即可选择全部九条路线。换机后，各自保留专精选择。",296,772,889,31,15,muted);if(Button("返回机库  →",1230,759,283,49,true))menuPage=0;
 }
}
}
