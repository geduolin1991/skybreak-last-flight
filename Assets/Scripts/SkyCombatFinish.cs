using UnityEngine;

namespace Skybreak {
public partial class SkyGame {
 // Reuse the killed visual for a short, collision-free fall. The slots are
 // fixed; no model cloning, mesh rebuilding or unbounded debris on a kill.
 struct WreckFlight {
  public Hostile enemy; public Vector3 start; public Quaternion rotation;
  public float age,duration,trail; public bool heavy;
 }
 readonly WreckFlight[] wreckFlights=new WreckFlight[9];int wreckCount;
 public int ActiveWrecks=>wreckCount;
 void RetireDestroyedVisual(Hostile enemy){
  if(!enemy.go)return;
  if(enemy.kind>=9||wreckCount>=(MobileMode?5:9)){ReleaseEnemyVisual(enemy);return;}
  if(enemy.marker){Destroy(enemy.marker.gameObject);enemy.marker=null;}
  if(enemy.aimLine){Destroy(enemy.aimLine.gameObject);enemy.aimLine=null;}
  foreach(var renderer in enemy.renderers)if(renderer)renderer.SetPropertyBlock(null);
  bool heavy=HeavyWreck(enemy);
  wreckFlights[wreckCount++]=new WreckFlight{enemy=enemy,start=enemy.go.transform.position,
   rotation=enemy.go.transform.rotation,heavy=heavy,duration=enemy.boss?1.65f:heavy?.76f:MobileMode?.35f:.48f};
 }
 void TickWrecks(float dt){
  for(int i=wreckCount-1;i>=0;i--){
   var w=wreckFlights[i];w.age+=dt;w.trail-=dt;
   if(w.age>=w.duration||!w.enemy.go){ReleaseEnemyVisual(w.enemy);wreckFlights[i]=wreckFlights[--wreckCount];wreckFlights[wreckCount]=default;continue;}
   float t=w.age/w.duration;float sign=w.start.x>=0?1:-1;
   w.enemy.go.transform.position=w.start+new Vector3(sign*t*(w.enemy.boss?.25f:.55f),-t*t*(w.enemy.boss?3.2f:2.0f),t*.6f);
   w.enemy.go.transform.rotation=w.rotation*Quaternion.Euler(t*(w.enemy.boss?8:32),0,sign*t*(w.heavy?20:65));
   if(w.trail<=0){w.trail=w.heavy?.12f:.16f;Burst(w.enemy.go.transform.position,w.heavy?5:2,1,w.heavy?.45f:.22f);}
   wreckFlights[i]=w;
  }
 }
 void ClearWrecks(){for(int i=0;i<wreckCount;i++){ReleaseEnemyVisual(wreckFlights[i].enemy);wreckFlights[i]=default;}wreckCount=0;}
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
