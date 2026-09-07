using UnityEngine;
namespace Skybreak {
public partial class SkyGame {
 void AdvancedBossAttack(Hostile e,int phase){int attack=(int)(e.age/7)%3;float speed=(4.3f+phase*.65f)*DifficultySpeed;
 if(Stage==1){if(attack==0){for(int s=-1;s<=1;s+=2){Vector3 p=e.pos+new Vector3(s*2.8f,0,-1);int n=10+phase*3;for(int k=0;k<n;k++){float a=k*Mathf.PI*2/n+e.age*.7f*s;AddBullet(p,new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*speed,false,s<0?1:2,1,.16f);}}}
 else if(attack==1){for(int row=-1;row<=1;row+=2){Vector3 p=e.pos+new Vector3(row*3,0,-1);Vector3 aim=(PlayerPos-p).normalized;for(int k=-2;k<=2;k++)AddBullet(p,Quaternion.Euler(0,k*13,0)*aim*(speed+2),false,1,1,.2f);}}
 else{if(Beams.Count==0){AddBeam(PlayerPos.x);if(phase>0)AddBeam(PlayerPos.x+(PlayerPos.x>0?-4:4));}for(int k=-5;k<=5;k++)AddBullet(e.pos,new Vector3(k*.55f,0,-speed),false,2,1,.16f);}}
 else{if(attack==0){int n=26+phase*6;float gap=Mathf.Sin(e.age*.27f)*.8f+Mathf.PI;for(int k=0;k<n;k++){float a=k*Mathf.PI*2/n+e.age*.15f;float d=Mathf.Abs(Mathf.DeltaAngle(a*Mathf.Rad2Deg,gap*Mathf.Rad2Deg));if(d<17)continue;AddBullet(e.pos,new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*speed,false,2,1,.17f,false,phase>0?7:0);}}
 else if(attack==1){for(int s=-1;s<=1;s+=2)for(int k=-2;k<=2;k++){Vector3 p=e.pos+new Vector3(s*2.7f,0,-.4f);Vector3 aim=(PlayerPos-p).normalized;AddBullet(p,Quaternion.Euler(0,k*12,0)*aim*(speed+1),false,s<0?1:2,1,.18f,false,s*(6+phase*2));}}
 else{if(Beams.Count==0){float safeX=Mathf.Sin(e.age*.3f)*6;for(int k=-2;k<=2;k++)if(Mathf.Abs(k*4-safeX)>2.7f)AddBeam(k*4);}for(int k=0;k<12+phase*4;k++){float a=k*Mathf.PI*2/(12+phase*4)+e.age;AddBullet(e.pos,new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*speed,false,1);}}}
 }
}
}
