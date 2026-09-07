using UnityEngine;
namespace Skybreak {
public partial class SkyGame {
 int researchFire,researchArmor,researchSkill;
 void LoadResearch(){researchFire=PlayerPrefs.GetInt("research-fire");researchArmor=PlayerPrefs.GetInt("research-armor");researchSkill=PlayerPrefs.GetInt("research-skill");}
 void SaveResearch(){if(qaRunning)return;PlayerPrefs.SetInt("research-fire",researchFire);PlayerPrefs.SetInt("research-armor",researchArmor);PlayerPrefs.SetInt("research-skill",researchSkill);SavePilotProgress();}
 public bool BuyResearch(int type){int level=type==0?researchFire:type==1?researchArmor:researchSkill;int max=type==1?2:3;int cost=200+level*250;if(level>=max||salvage<cost)return false;salvage-=cost;if(type==0)researchFire++;if(type==1)researchArmor++;if(type==2)researchSkill++;SaveResearch();Sound("Pickup");return true;}
 void DrawWorkshop(){Panel(318,99,965,733);SmallTag("SQUADRON DEVELOPMENT",357,128,257,accent);Label("把每次出击，变成下一次的力量。",356,188,863,71,36,paper);Label("回收点  "+salvage+"    /    累计行动  "+totalRuns,359,283,827,40,19,accent);string[] names={"高效火控","轻质装甲","同步接口"};string[] desc={"每级令全战机基础伤害 +4%","每级令全战机装甲上限 +1","每级令驾驶员技能冷却缩短 5%"};int[] levels={researchFire,researchArmor,researchSkill};for(int i=0;i<3;i++){float y=379+i*125;Panel(356,y,852,107);Label(names[i]+"   Lv."+levels[i],377,y+16,570,32,23,paper);Label(desc[i],378,y+65,561,31,15,muted);int max=i==1?2:3;int cost=200+levels[i]*250;bool can=levels[i]<max&&salvage>=cost;if(Button(levels[i]>=max?"已完成研发":cost+" 回收点",988,y+27,196,54,can,can))BuyResearch(i);}if(Button("← 返回档案",42,765,226,55))menuPage=4;}
}
}
