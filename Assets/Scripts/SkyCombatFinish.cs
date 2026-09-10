using UnityEngine;

namespace Skybreak {
public partial class SkyGame {
 // Reuse the killed visual for a short, collision-free fall. The slots are
 // fixed; no model cloning, mesh rebuilding or unbounded debris on a kill.
 struct WreckFlight {
  public Hostile enemy; public Vector3 start; public Quaternion rotation;
  public float age,duration,trail; public bool heavy,airburst;
 }
 readonly WreckFlight[] wreckFlights=new WreckFlight[9];int wreckCount;
 public int ActiveWrecks=>wreckCount;
 bool ImmediateAirBreakup(Hostile e)=>!e.boss&&(e.owner!=null||(e.kind==0&&Kills%3==0)||NovaActive);
 void RetireDestroyedVisual(Hostile enemy){
  if(!enemy.go)return;
  if(enemy.kind>=9&&!enemy.boss){if(enemy.kind==9){float y;var surface=World.CrashSurfaceAt(enemy.go.transform.position,out y);var p=enemy.go.transform.position;p.y=y;SurfaceExplosion(p,surface,true);}ReleaseEnemyVisual(enemy);return;}
  if(wreckCount>=(MobileMode?5:9)){airBreakups++;ReleaseEnemyVisual(enemy);return;}
  if(enemy.marker){Destroy(enemy.marker.gameObject);enemy.marker=null;}if(enemy.aimLine){Destroy(enemy.aimLine.gameObject);enemy.aimLine=null;}
  for(int i=0;i<enemy.renderers.Length;i++){var r=enemy.renderers[i];if(!r)continue;block.Clear();block.SetColor("_Color",enemy.baseColors[i]*.42f);block.SetColor("_EmissionColor",Color.black);r.SetPropertyBlock(block);}
  bool heavy=HeavyWreck(enemy);bool airburst=ImmediateAirBreakup(enemy);
  if(airburst)airBreakups++;
  wreckFlights[wreckCount++]=new WreckFlight{enemy=enemy,start=enemy.go.transform.position,rotation=enemy.go.transform.rotation,heavy=heavy,airburst=airburst,duration=airburst?.23f:enemy.boss?1.65f:heavy?1.24f:.96f};
 }
 void TickWrecks(float dt){
  for(int i=wreckCount-1;i>=0;i--){var w=wreckFlights[i];w.age+=dt;w.trail-=dt;float t=Mathf.Clamp01(w.age/w.duration);
   if(w.enemy.go){float sign=w.start.x>=0?1:-1;Vector3 p=w.start+new Vector3(sign*t*(w.enemy.boss?.3f:.85f),0,t*.7f);float height;var surface=World.CrashSurfaceAt(p,out height);
    p.y=Mathf.Lerp(w.start.y,w.airburst?w.start.y-.35f:height,t*t);w.enemy.go.transform.position=p;w.enemy.go.transform.rotation=w.rotation*Quaternion.Euler(t*(w.heavy?28:68),t*22,sign*t*(w.heavy?30:150));
    if(w.airburst)w.enemy.go.transform.localScale*=Mathf.Max(.01f,1-dt*6);
    else if(w.trail<=0){w.trail=w.heavy?.09f:.13f;Burst(p,w.heavy?5:3,1,w.heavy?.42f:.25f);}
    if(t>=1&&!w.airburst)SurfaceExplosion(p,surface,w.heavy);
   }
   if(t>=1||!w.enemy.go){ReleaseEnemyVisual(w.enemy);wreckFlights[i]=wreckFlights[--wreckCount];wreckFlights[wreckCount]=default;}else wreckFlights[i]=w;
  }
  TickSurfaceImpacts(dt);
 }
 void ClearWrecks(){for(int i=0;i<wreckCount;i++){ReleaseEnemyVisual(wreckFlights[i].enemy);wreckFlights[i]=default;}wreckCount=0;ClearSurfaceImpacts();}
 void QuiesceDefeatedBattle(){
  // The victory transmission is a safe exit. A defeated boss cannot leave
  // live lasers or delayed fire that kills the player during its last words.
  CancelBullets(false);
  foreach(var beam in Beams)if(beam.go)Destroy(beam.go);Beams.Clear();
  foreach(var enemy in Enemies){
   if(enemy.boss||enemy.hp<=0)continue;
   enemy.fire=3600;FinishWeaponTell(enemy);
   if(enemy.owner==null&&enemy.kind<9)enemy.retreating=true;
  }
  invuln=Mathf.Max(invuln,.5f);
 }
}
}
