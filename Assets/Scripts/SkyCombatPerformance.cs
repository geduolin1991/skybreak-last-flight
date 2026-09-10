using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
namespace Skybreak {
// Pools are owned by this game instance and survive chapter changes.
public sealed class SkyEnemyVisual : MonoBehaviour {
 public int kind; public Transform[] vents;public Quaternion[] ventRest; public Renderer[] renderers; public Color[] colors;
}
public partial class SkyGame {
 readonly Stack<SkyEnemyVisual>[] enemyVisualPool=new Stack<SkyEnemyVisual>[12];
 readonly Stack<Shock> shockPool=new Stack<Shock>();
 Transform pooledActors;int shockAllocations,enemyVisualAllocations;
 int renderedSparkPeak;float lastKillCostMs,peakKillCostMs;
 void SetupCombatPerformance(){
  pooledActors=new GameObject("Reusable combat effects").transform;pooledActors.SetParent(transform,false);
  for(int i=0;i<enemyVisualPool.Length;i++)enemyVisualPool[i]=new Stack<SkyEnemyVisual>();
  for(int i=0;i<40;i++)shockPool.Push(CreateShock());
  // Tiny unlit rounds do not need Unity's 515-vertex default sphere.
  sphere=BuildProjectileMesh();
  foreach(string name in new[]{"Explosion","ExplosionHeavy","BossBreak","ImpactLight","ImpactArmor","MissileBurst","ShotPulse","ShotScatter","ShotLance","ShotSeeker","ShotMortar","ShotRailFast","Pickup","Combo","Click","Hit","Warning","Bomb","Overdrive","NovaCharge"}){var c=Clip(name);if(c)c.LoadAudioData();}
  SetupDestruction114();StartCoroutine(WarmEnemyVisuals());
 }
 System.Collections.IEnumerator WarmEnemyVisuals(){
  for(int kind=0;kind<12;kind++){for(int i=0;i<(kind<2?4:2);i++){var v=CreateEnemyVisual(kind);v.gameObject.SetActive(false);enemyVisualPool[kind].Push(v);}yield return null;}
 }
 SkyEnemyVisual CreateEnemyVisual(int kind){
  var go=Art.Model(EnemyModels[kind],pooledActors);var v=go.AddComponent<SkyEnemyVisual>();v.kind=kind;var vents=new List<Transform>();foreach(var t in go.GetComponentsInChildren<Transform>())if(t.name.StartsWith("Motion_Vent_"))vents.Add(t);v.vents=vents.ToArray();v.ventRest=new Quaternion[v.vents.Length];for(int i=0;i<v.vents.Length;i++)v.ventRest[i]=v.vents[i].localRotation;
  v.renderers=go.GetComponentsInChildren<Renderer>();v.colors=RendererColors(v.renderers);enemyVisualAllocations++;return v;
 }
 SkyEnemyVisual RentEnemyVisual(int kind){var pool=enemyVisualPool[kind];var v=pool.Count>0?pool.Pop():CreateEnemyVisual(kind);for(int i=0;i<v.vents.Length;i++)v.vents[i].localRotation=v.ventRest[i];v.gameObject.SetActive(true);return v;}
 void ReleaseEnemyVisual(Hostile e){
  if(!e.go)return;
  var v=e.go.GetComponent<SkyEnemyVisual>();
  if(e.marker){Destroy(e.marker.gameObject);e.marker=null;}if(e.aimLine){Destroy(e.aimLine.gameObject);e.aimLine=null;}
  if(!v||e.boss||e.owner!=null){Destroy(e.go);e.go=null;return;}
  foreach(var r in v.renderers)if(r)r.SetPropertyBlock(null);
  v.transform.SetParent(pooledActors,false);v.gameObject.SetActive(false);
  if(enemyVisualPool[v.kind].Count<12)enemyVisualPool[v.kind].Push(v);else Destroy(v.gameObject);
  e.go=null;
 }
 Shock CreateShock(){var l=Art.Ring(pooledActors,1,Color.white,.1f,40);l.gameObject.SetActive(false);shockAllocations++;return new Shock{line=l};}
 void ReturnShock(Shock s){if(!s.line)return;s.line.gameObject.SetActive(false);shockPool.Push(s);}
 void ClearShocks(){foreach(var s in shocks)ReturnShock(s);shocks.Clear();}
 static bool HeavyWreck(Hostile e)=>e.boss||e.elite||e.owner!=null||e.kind==2||e.kind==3||e.kind==8||e.kind==9;
 Mesh BuildProjectileMesh(){
  const int sides=10,rings=6;var vertices=new Vector3[(rings+1)*(sides+1)];var triangles=new int[rings*sides*6];int t=0;
  for(int y=0;y<=rings;y++)for(int x=0;x<=sides;x++){float a=x*Mathf.PI*2/sides,b=y*Mathf.PI/rings;vertices[y*(sides+1)+x]=new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a))*.5f;}
  for(int y=0;y<rings;y++)for(int x=0;x<sides;x++){int a=y*(sides+1)+x,b=a+sides+1;triangles[t++]=a;triangles[t++]=b+1;triangles[t++]=b;triangles[t++]=a;triangles[t++]=a+1;triangles[t++]=b+1;}
  var mesh=new Mesh{name="Projectile / 77 vertices"};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
 }
}
}
