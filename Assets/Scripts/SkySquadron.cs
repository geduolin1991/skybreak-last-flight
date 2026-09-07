using UnityEngine;
using System.Collections.Generic;
namespace Skybreak {
public partial class SkyGame {
 sealed class Wingman {public GameObject go;public int pilot,side;public float fire,age;public Vector3 velocity;}
 readonly List<Wingman> squadron=new List<Wingman>();float supportRemaining,supportDepartAge;bool supportDeparting;
 public int WingmanCount=>squadron.Count;public float SupportRemaining=>Mathf.Max(0,supportRemaining);
 public string SupportingPilots=>string.Join(",",squadron.ConvertAll(w=>w.pilot.ToString()));
 public void CallSquadron(){
  float duration=20+wingmen*5+(Ship==0&&CurrentRoute==1?RouteTier*3:0);
  if(squadron.Count>0){supportRemaining=Mathf.Min(38,supportRemaining+duration*.7f);supportDeparting=false;supportDepartAge=0;MissionSay("support_extend_"+Ship,70,null,false);return;}
  supportRemaining=duration;supportDeparting=false;supportDepartAge=0;int side=-1;
  for(int i=0;i<3;i++)if(i!=Ship){var go=Art.Model(new[]{"Kestrel","Manta","Needle"}[i],null);go.name=PilotNames[i]+" / allied wingman";go.transform.localScale=Vector3.one*(i==1?.48f:.52f);go.transform.position=PlayerPos+new Vector3(side*7.5f,.25f,-10);
   Color color=i==0?Art.Cyan:i==1?Art.Orange:Art.Violet;Art.Exhaust(go.transform,new Vector3(-.6f,.1f,-1.5f),color,.4f);Art.Exhaust(go.transform,new Vector3(.6f,.1f,-1.5f),color,.4f);
   squadron.Add(new Wingman{go=go,pilot=i,side=side,fire=.2f+(side==1?.5f:0)});side=1;
  }
  Toast("双机增援抵达 · "+Mathf.RoundToInt(duration)+" 秒",3);Sound("Pickup",.7f);
 }
 void ClearSquadron(){foreach(var w in squadron)if(w.go)Destroy(w.go);squadron.Clear();supportRemaining=supportDepartAge=0;supportDeparting=false;}
 void BeginSquadronDeparture(bool speak=true){if(squadron.Count==0||supportDeparting)return;supportDeparting=true;supportDepartAge=0;supportRemaining=0;if(speak){var first=squadron[0];MissionSay("support_exit_"+first.pilot,75,()=>supportDeparting&&squadron.Count>0,false);}}
 void TickSquadron(float dt){
  if(squadron.Count==0)return;
  if(!supportDeparting){supportRemaining-=dt;if(supportRemaining<=0)BeginSquadronDeparture();}
  if(supportDeparting)supportDepartAge+=dt;
  for(int i=0;i<squadron.Count;i++){
   var w=squadron[i];w.age+=dt;if(!w.go)continue;
   if(supportDeparting){w.go.transform.position+=new Vector3(w.side*1.2f,.2f,10+supportDepartAge*9)*dt;w.go.transform.rotation=Quaternion.Slerp(w.go.transform.rotation,Quaternion.Euler(-10,w.side*-8,w.side*-9),dt*3);continue;}
   Vector3 target=PlayerPos+new Vector3(w.side*(2.5f+(Ship==1?.4f:0)),.18f,-.6f)+new Vector3(Mathf.Sin(w.age*.8f)*.13f,Mathf.Sin(w.age*1.3f)*.06f,0);
   Vector3 before=w.go.transform.position;w.go.transform.position=Vector3.SmoothDamp(before,target,ref w.velocity,.24f,28,dt);w.go.transform.rotation=Quaternion.Slerp(w.go.transform.rotation,Quaternion.Euler(0,0,Mathf.Clamp(-w.velocity.x*2,-24,24)),dt*7);
   if(w.age>1.1f&&w.age-dt<=1.1f){int pilot=w.pilot;MissionSay("support_enter_"+pilot,78,()=>squadron.Exists(a=>a.pilot==pilot)&&!supportDeparting,false);}
   if((target-w.go.transform.position).sqrMagnitude>9)continue;w.fire-=dt;if(w.fire>0||Enemies.Count==0||stageEndClock>0)continue;
   Vector3 p=w.go.transform.position;p.y=1;float boost=1+(wingmen*.15f)+(Ship==0&&CurrentRoute==1?.1f*RouteTier:0);
   if(w.pilot==0){w.fire=.24f;for(int s=-1;s<=1;s+=2)AddBullet(p+new Vector3(s*.29f,0,.7f),new Vector3(s*2,0,38),true,0,4.6f*boost,.085f,true,kind:2);}
   else if(w.pilot==1){w.fire=.78f;for(int s=-1;s<=1;s++)AddBullet(p+Vector3.forward*.7f,new Vector3(s*5,0,27),true,3,10*boost,.16f,kind:4);}
   else {w.fire=1.2f;Hostile targetEnemy=null;float best=999;foreach(var e in Enemies){if(e.pos.z<p.z+.6f)continue;float d=Vector3.Distance(e.pos,p);if(d<best){best=d;targetEnemy=e;}}if(targetEnemy!=null){Vector3 end=p+(targetEnemy.pos-p).normalized*30;railTraces.Add(new RailTrace{width=.1f,core=Art.Line(null,"Yuki support rail",new[]{p,end},new Color(.85f,.82f,1),.1f),halo=Art.Line(null,"Yuki support corona",new[]{p,end},new Color(.47f,.4f,1,.45f),.25f)});foreach(var e in Enemies.ToArray())if(SegmentDistance(e.pos,p,end)<EnemyRadius(e)+.08f)DamageEnemy(e,25*boost);}}
  }
  if(supportDeparting&&supportDepartAge>2.4f)ClearSquadron();
 }
 void DrawSquadronHUD(){if(squadron.Count==0)return;Panel(490,727,620,48);Label(supportDeparting?"僚机正在脱离":"双机协同  ·  "+Mathf.CeilToInt(supportRemaining)+" 秒",506,738,220,25,15,accent);for(int i=0;i<squadron.Count;i++)Label(PilotNames[squadron[i].pilot]+" / "+ShipNames[squadron[i].pilot],735+i*172,740,170,23,13,paper);Bar(506,769,588,supportRemaining/38,accent,2);}
}
}
