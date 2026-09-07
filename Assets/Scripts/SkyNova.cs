using UnityEngine;
using System.Collections.Generic;
namespace Skybreak {public partial class SkyGame {
 GameObject novaRoot;Material novaMaterial;float novaClock=-1;Vector3 novaCenter;bool novaDetonated;readonly HashSet<Hostile> novaHit=new HashSet<Hostile>();
 public bool NovaActive=>novaRoot&&novaClock>=0;
 void BeginNova(){if(novaRoot)Destroy(novaRoot);novaClock=0;novaDetonated=false;novaHit.Clear();novaCenter=PlayerPos;
  novaRoot=new GameObject("RIFT NOVA · charge / detonate / shock front");novaRoot.transform.position=novaCenter;
  novaMaterial=novaRoot.AddComponent<SkyOwnedResources>().Keep(new Material(Shader.Find("Skybreak/Nova")));novaMaterial.SetColor("_Color",ShipColor);
  var field=Art.Primitive(novaRoot.transform,"Expanding plasma shockfront",Vector3.up*.18f,new Vector3(64,64,1),novaMaterial,PrimitiveType.Quad);field.transform.localRotation=Quaternion.Euler(90,0,0);
  for(int i=0;i<80;i++){float a=i*Mathf.PI*2/80;Vector3 p=novaCenter+new Vector3(Mathf.Cos(a),.2f,Mathf.Sin(a))*Random.Range(1.3f,4.4f);sparks.Add(new Spark{pos=p,velocity=(novaCenter-p)*3,life=.22f,max=.22f,size=.11f,style=0});}
  Sound("NovaCharge",.85f);audioDuck=.3f;Toast("裂 空 · NOVA BREAK",1.9f);
 }
 void TickNova(float dt){if(!NovaActive)return;novaClock+=dt;float p=Mathf.Clamp01((novaClock-.18f)/1.55f);novaMaterial.SetFloat("_Progress",p);
  if(!novaDetonated&&novaClock>=.18f){novaDetonated=true;Sound("Bomb",.85f);shake=Mathf.Max(shake,1.15f);hitStop=Mathf.Max(hitStop,.085f);Burst(novaCenter,180,0,2.8f);
   for(int i=0;i<3;i++){var ring=Art.Ring(novaRoot.transform,.7f,ShipColor,.075f,96);ring.transform.localRotation=Quaternion.Euler(28+i*56,0,i*60);}
  }
  if(novaDetonated){float radius=32*(1-Mathf.Pow(1-p,2));foreach(var e in Enemies.ToArray())if(!novaHit.Contains(e)&&Vector3.Distance(e.pos,novaCenter)<=radius){novaHit.Add(e);Shockwave(e.pos,ShipColor,e.boss?4:1.5f,.4f);DamageEnemy(e,e.boss?420:280);}
   foreach(var ring in novaRoot.GetComponentsInChildren<LineRenderer>()){ring.transform.localScale=Vector3.one*(1+p*15);ring.startWidth=ring.endWidth=(1-p)*.11f;}
  }
  if(Post){Vector3 v=Cam.WorldToViewportPoint(novaCenter);Post.Ripple=new Vector4(v.x,v.y,p,novaDetonated?(1-p)*.65f:0);}
  if(novaClock>1.85f){Destroy(novaRoot);novaClock=-1;if(Post)Post.Ripple=Vector4.zero;}
 }
 void ClearNova(){if(novaRoot)Destroy(novaRoot);novaClock=-1;novaHit.Clear();if(Post)Post.Ripple=Vector4.zero;}
}}
