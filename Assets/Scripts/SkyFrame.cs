using UnityEngine;
using System.Collections.Generic;
namespace Skybreak {
public class SkyFrame:MonoBehaviour {
 Transform[] joints;Quaternion[] poses;
 void Start(){var list=new List<Transform>();foreach(var t in GetComponentsInChildren<Transform>())if(t.name=="Arm_L"||t.name=="Arm_R"||t.name=="Wing_L"||t.name=="Wing_R")list.Add(t);joints=list.ToArray();poses=new Quaternion[joints.Length];for(int i=0;i<joints.Length;i++)poses[i]=joints[i].localRotation;}
 void Update(){if(joints==null)return;float t=Time.time;for(int i=0;i<joints.Length;i++){float sign=joints[i].name.EndsWith("L")?-1:1;float a=joints[i].name.StartsWith("Wing")?Mathf.Sin(t*2.2f)*5:Mathf.Sin(t*12+i)*3;joints[i].localRotation=poses[i]*Quaternion.Euler(a,0,sign*Mathf.Sin(t*1.5f)*3);}}
}
public partial class SkyGame {
 GameObject frameForm,airForm;float frameBlend;
 void PrepareFrame(){airForm=Player.transform.childCount>0?Player.transform.GetChild(0).gameObject:null;frameForm=Art.Model(new[]{"AstraFrame","CrimsonFrame","OracleFrame"}[Ship],Player.transform);frameForm.transform.localPosition=new Vector3(0,-.4f,-.7f);frameForm.transform.localRotation=Quaternion.Euler(36,0,0);frameForm.AddComponent<SkyFrame>();frameForm.SetActive(false);frameBlend=0;}
 void AnimateFrame(float dt){if(!frameForm)return;bool active=Overdrive>0;frameBlend=Mathf.MoveTowards(frameBlend,active?1:0,dt*4.5f);frameForm.SetActive(frameBlend>0);if(airForm)airForm.SetActive(frameBlend<.6f);frameForm.transform.localScale=Vector3.one*Mathf.Max(.001f,frameBlend)*1.04f;if(airForm)airForm.transform.localScale=Vector3.one*(1-frameBlend*.8f);}
}
}
