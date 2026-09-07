using UnityEngine;
using System.Collections.Generic;
namespace Skybreak {
 // Pivots are authored in Blender and retained during material batching.
 public class SkyBossMechanisms:MonoBehaviour {
  class Joint {public Transform part;public Quaternion rest;public bool rotor;public float phase;}
  readonly List<Joint> joints=new List<Joint>();
  public int Count=>joints.Count;
  public float MotionAngle {get{float angle=0;foreach(var j in joints)angle+=Quaternion.Angle(j.rest,j.part.localRotation);return angle;}}
  public void Initialize(){foreach(var t in GetComponentsInChildren<Transform>())if(t.name.StartsWith("Motion_"))joints.Add(new Joint{part=t,rest=t.localRotation,rotor=t.name.Contains("Rotor"),phase=joints.Count*.63f});}
  public void Tick(float age,int phase,bool exposed,bool charging){
   foreach(var j in joints){
    Vector3 axis;float angle;
    if(j.rotor){axis=Vector3.up;angle=age*(75+phase*35)*(j.phase<.1f?1:-1);}
    else{Vector3 radial=Vector3.ProjectOnPlane(j.part.position-transform.position,Vector3.up).normalized;axis=Vector3.Cross(Vector3.up,radial);angle=(exposed?24:charging?-12:6)+Mathf.Sin(age*1.7f+j.phase)*4;}
    j.part.localRotation=Quaternion.AngleAxis(angle,j.part.parent.InverseTransformDirection(axis))*j.rest;
   }
  }
 }
}
