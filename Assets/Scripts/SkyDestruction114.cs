using UnityEngine;
namespace Skybreak {
public enum CrashSurface { Water, Ground, Vacuum }
public partial class SkyWorld {
 // Use the same moving shoreline footprints and inset as the water shader.
 // Ground props are batched and intentionally have no physics colliders.
 public CrashSurface CrashSurfaceAt(Vector3 p,out float height){
  height=Stage==2?-5:Stage==1?-10.10f:-9.94f;
  if(Stage==2)return CrashSurface.Vacuum;
  if(Stage==1)return CrashSurface.Ground;
  foreach(var a in coastAnchors){if(!a.sector)continue;var s=a.shape;s.x-=Mathf.Sign(s.x)*CoastInset;s.y+=a.sector.position.z;
   float dx=Mathf.Abs(p.x-s.x),dz=Mathf.Abs(p.z-s.y);bool inside;
   if(s.w<0)inside=new Vector2(dx/s.z,dz/-s.w).magnitude<1;
   else{var q=new Vector2(dx-s.z+.48f,dz-s.w+.48f);inside=new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-.48f<0;}
   if(inside){height=-9.65f;return CrashSurface.Ground;}
  }
  return CrashSurface.Water;
 }
}
public partial class SkyGame {
 class SurfaceImpact {public Transform root;public ParticleSystem plume;public LineRenderer first,second;public Renderer scorch;public float age,duration,size,travel;public CrashSurface surface;public bool active;}
 readonly SurfaceImpact[] surfaceImpacts=new SurfaceImpact[10];int impactCursor;
 Material impactParticles,scorchMaterial,impactRings;int waterCrashes,groundCrashes,airBreakups,vacuumBreakups,crashEffectsAllocated;
 public int ActiveSurfaceImpacts {get{int n=0;foreach(var s in surfaceImpacts)if(s!=null&&s.active)n++;return n;}}
 void SetupDestruction114(){
  impactParticles=gameObject.AddComponent<SkyOwnedResources>().Keep(new Material(Shader.Find("Skybreak/GlowSprite")));impactParticles.mainTexture=Art.GlowTexture();
  impactRings=gameObject.AddComponent<SkyOwnedResources>().Keep(new Material(Shader.Find("Skybreak/Telegraph")));
  scorchMaterial=gameObject.AddComponent<SkyOwnedResources>().Keep(new Material(Shader.Find("Skybreak/Smoke")));
  for(int i=0;i<surfaceImpacts.Length;i++){
   var root=new GameObject("Reusable surface impact "+i).transform;root.SetParent(pooledActors,false);
   var ps=root.gameObject.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   var main=ps.main;main.playOnAwake=false;main.loop=false;main.simulationSpace=ParticleSystemSimulationSpace.Local;main.maxParticles=56;main.startLifetime=.9f;main.startSpeed=0;main.startSize=.25f;
   var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.enabled=false;
   var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,1,1,0));
   var r=ps.GetComponent<ParticleSystemRenderer>();r.sharedMaterial=impactParticles;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
   var mark=Art.Primitive(root,"Scorch / impact shadow",Vector3.up*.01f,Vector3.one,scorchMaterial,PrimitiveType.Quad);mark.transform.localRotation=Quaternion.Euler(90,0,0);
   surfaceImpacts[i]=new SurfaceImpact{root=root,plume=ps,first=Art.Ring(root,1,Color.white,.045f,40),second=Art.Ring(root,1,Color.white,.025f,40),scorch=mark.GetComponent<Renderer>()};surfaceImpacts[i].first.sharedMaterial=surfaceImpacts[i].second.sharedMaterial=impactRings;for(int j=0;j<40;j++){float a=j*Mathf.PI*2/40;float r0=1+Mathf.Sin(a*7+i)*.035f;var point=new Vector3(Mathf.Cos(a)*r0,0,Mathf.Sin(a)*r0);surfaceImpacts[i].first.SetPosition(j,point);surfaceImpacts[i].second.SetPosition(j,point);}root.gameObject.SetActive(false);crashEffectsAllocated++;
  }
 }
 void SurfaceExplosion(Vector3 position,CrashSurface surface,bool heavy){
  if(surface==CrashSurface.Vacuum){vacuumBreakups++;Burst(position,heavy?32:18,2,heavy?1.15f:.65f);Shockwave(position,Art.Cyan,heavy?2.8f:1.6f,.35f);return;}
  if(surface==CrashSurface.Water)waterCrashes++;else groundCrashes++;
  var s=surfaceImpacts[impactCursor++%surfaceImpacts.Length];s.plume.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);s.root.gameObject.SetActive(true);s.root.position=position+Vector3.up*.06f;s.age=0;s.duration=surface==CrashSurface.Water?1.65f:2.5f;s.size=heavy?2.0f:1.18f;s.travel=World.SceneryTravel;s.surface=surface;s.active=true;
  s.scorch.enabled=surface==CrashSurface.Ground;s.scorch.transform.localScale=Vector3.one*s.size*2.3f;
  Color c=surface==CrashSurface.Water?new Color(.45f,.77f,1,.60f):new Color(1,.33f,.08f,.50f);
  s.first.startColor=s.first.endColor=c;s.second.startColor=s.second.endColor=c;
  var main=s.plume.main;main.gravityModifier=surface==CrashSurface.Water?1.15f:.4f;main.startLifetime=surface==CrashSurface.Water?.88f:1.1f;s.plume.Play();
  int count=MobileMode?(heavy?30:20):(heavy?48:32);
  for(int i=0;i<count;i++){float angle=i*Mathf.PI*2/count;float outward=Random.Range(.8f,2.6f)*s.size;var p=new ParticleSystem.EmitParams{position=Vector3.zero,velocity=new Vector3(Mathf.Cos(angle)*outward,Random.Range(3.3f,7.8f),Mathf.Sin(angle)*outward),startSize=Random.Range(.15f,.29f)*s.size,startColor=surface==CrashSurface.Water?new Color(.53f,.78f,.91f,.60f):new Color(1,.3f,.035f,.72f)};s.plume.Emit(p,1);}if(surface==CrashSurface.Ground){var core=new ParticleSystem.EmitParams{position=Vector3.up*.1f,velocity=Vector3.up*1.5f,startSize=s.size*.85f,startLifetime=.32f,startColor=new Color(1,.52f,.08f,.9f)};s.plume.Emit(core,4);}s.plume.Pause();
  if(surface==CrashSurface.Ground)Burst(position,heavy?22:12,1,.7f);
  if(killGate<=0){killGate=.12f;Sound(surface==CrashSurface.Water?"CrashWater114":"CrashGround114",heavy?.62f:.40f);}
 }
 void TickSurfaceImpacts(float dt){foreach(var s in surfaceImpacts){if(s==null||!s.active)continue;s.age+=dt;s.plume.Simulate(dt,false,false,false);
   if(s.age>=s.duration){s.active=false;s.root.gameObject.SetActive(false);continue;}
   float travel=World.SceneryTravel;s.root.position+=Vector3.back*(travel-s.travel);s.travel=travel;
   float t=s.age/s.duration;float radius=Mathf.Lerp(.16f,s.size*2.1f,Mathf.Sqrt(t));s.first.transform.localScale=Vector3.one*radius;s.second.transform.localScale=Vector3.one*Mathf.Max(.01f,radius-.35f);
   float fade=(1-t)*(s.surface==CrashSurface.Ground?Mathf.Max(0,1-t*3):1);Color color=s.surface==CrashSurface.Water?new Color(.43f,.77f,1,fade*.55f):new Color(1,.3f,.05f,fade*.45f);s.first.startColor=s.first.endColor=color;s.second.startColor=s.second.endColor=color;
   s.first.startWidth=s.first.endWidth=.055f*(1-t);s.second.startWidth=s.second.endWidth=.025f*(1-t);if(s.scorch.enabled)s.scorch.transform.localScale=Vector3.one*s.size*2.3f*Mathf.Min(1,(1-t)*3);
  }}
 void ClearSurfaceImpacts(){foreach(var s in surfaceImpacts){if(s==null)continue;s.plume.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);s.active=false;s.root.gameObject.SetActive(false);}}
}
}
