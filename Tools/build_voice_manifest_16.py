"""Event-bound original Chinese screenplay. Reproducible; old 1.5 screenplay is retained."""
import json
from pathlib import Path
root=Path(__file__).resolve().parents[1]
old=json.loads((root/'Assets/Resources/Voices/voice-bank.json').read_text())['clips']
clips=[c for c in old if c['category']!='story' and not c['id'].startswith(('skill_','route_skill_')) and not c['id'].startswith(('world_','convoy_','coast_','city_','orbit_','support_','ward_','rail_','carrier_','elite_','boss_','core_','repair_','route_up_'))]
for c in clips:
 c['match']=''
 if c['id'].startswith('victory_'):c['text']=['封锁解除了！检查队形，我们回家。','哈，打穿了！收队吧，大家。','攻击指令已终止。我们，做到了。'][c['pilot']]
 if c['id'].startswith('briefing_'):
  stage=int(c['id'].split('_')[-1]);c['text']=[['下方三艘救援船，都是我们要带回家的人。拦住俯冲轰炸机，别让炸弹落到船上！','三座干扰塔切断了城市供电。逐座击破，让街灯和撤离车队重新动起来！','先解除三个封锁接点，再掩护雪璃上传识别码。八秒，我们替她争取。'],['三艘船，七百多口人。嗯，这回可得把前面的轰炸机，清得干干净净。','看见那三座干扰塔没？拆掉它们。等灯亮了，公交车才能把人送出去。','船坞被封死了。拆开三个接点，给雪璃留八秒。剩下的火力，我来扛。'],['三艘民用船已进入航线。敌人的俯冲机，会攻击船队。请优先拦截。','三座干扰塔仍在运行。摧毁一座，对应街区就能恢复供电。','那座船坞……我曾在那里工作。解除三个封锁接点，我需要八秒，重新接通它。']][c['pilot']][stage]
entries=[
('world_intro_0',0,'看到下方三艘白色救援船了。全队，守住这条航线！'),
('world_intro_1',1,'干扰塔出现了。三座，分开拆！别让下面的人，再摸黑等了。'),
('world_intro_2',2,'就是这座船坞。红色接点还在封锁，先击破它们！'),
('convoy_bomber',0,'俯冲机正冲向船队！拦住它！'),
('convoy_bomb_lock',1,'它在瞄准船！赶紧打掉，别让它投弹！'),
('convoy_intercept',0,'拦下了！继续守住船队。'),
('convoy_hit',0,'救援船中弹！盯住下一个炸弹！'),
('convoy_disabled',0,'有一艘失去动力了……掩护剩下的船！'),
('coast_clear_all',1,'要塞哑火了！三艘船都还在，正在驶出封锁线！'),
('coast_clear_some',0,'要塞击破！还能航行的船，立刻撤离。救援队，请接应失去动力的船。'),
('coast_clear_lost',0,'要塞击破……可是，船队失去动力了。通知后方，立刻派救援！'),
('city_grid_1',1,'第一座拆掉了！看，那个街区亮了。'),
('city_grid_2',0,'又一个街区恢复供电！撤离车辆开始移动了。'),
('city_grid_3',1,'三座全部熄火！哈，整条路的灯都回来了！'),
('city_clear_power',0,'制空平台击破！电网稳定，撤离车队可以继续走了。'),
('city_clear_partial',1,'平台打下来了。但还有街区没电，地面队伍，继续抢修！'),
('orbit_node_1',2,'第一条封锁解除。船坞，回应我。'),
('orbit_node_2',0,'第二个接点击破！雪璃，我们还在你身边。'),
('orbit_node_3',2,'接点全部解除！正在上传，掩护我的侧翼！'),
('orbit_connected',2,'接通了……它认出我们了！接驳船，可以进港。'),
('orbit_clear_link',2,'核心停止攻击。识别码已经生效。欢迎回家。'),
('orbit_clear_partial',2,'攻击停止了。识别链路还没接通……让地面队伍继续上传。'),
('support_beacon',0,'支援信标投下来了！接住它，我们就能并肩作战。'),
('ward_contact',2,'紫色护盾由圆环机维持。先打掉发生器！'),
('rail_lock',2,'轨道炮锁定了！现在横移！'),
('carrier_launch',1,'那艘母机还在放小飞机。直接拆母机！'),
('elite_arrive',1,'重装指挥机！打掉它，能拿到一批补给。'),
('elite_down',0,'指挥机击破，补给释放！需要修复的，靠过来。'),
('core_exposed',2,'防护节点全部击破！核心暴露，现在集中火力！'),
('boss_warning_0',0,'利维坦进入航线！先拆它的炮塔，再打核心！'),
('boss_warning_1',1,'暴风眼来了。看准激光预警，别被两边夹住！'),
('boss_warning_2',2,'炽天使接近。它的节点还在供能，先切断它们！')]
for pilot in range(3):
 for family,texts in {
 'support_enter':['苍隼到位！导弹跟着我，清理侧翼！','蝠鲼到位。嗯，给你带了点重火力。','银针就位。轨道炮，替你打开一条直线。'],
 'support_exit':['支援时间到，我先脱离。保持航线！','弹药见底，我回去补充。下一轮，接着陪你。','协同窗口结束。银针脱离，通讯保持。'],
 'support_extend':['收到新信标！继续保持双机支援！','补给接上了。哈，这下还能多打一轮！','支援信标确认。协同时间已延长。'],
 'route_up':['武装同步了！新一阶专精，可以用了！','改装生效！让它们试试这份火力。','专精参数更新。下一轮，我会更准。']
 }.items():entries.append((family+'_'+str(pilot),pilot,texts[pilot]))
skills=[['锁定目标！蜂群，齐射！','队友，跟上！展开护航！','抓紧了！疾风，突进！'],['前方五点！熔炉轰击！','别硬扛，进防壁！装甲，修复！','解除限流！哈，火力全开！'],['你已在射线上。终点，校准！','给我们……留下时间。零时，展开！','坐标锁定。相位，跃迁！']]
for pilot in range(3):
 for route in range(3):entries.append((f'route_skill_{pilot}_{route}',pilot,skills[pilot][route]))
seen={c['id'] for c in clips}
for id,pilot,text in entries:
 assert id not in seen;seen.add(id)
 clips.append({'id':id,'pilot':pilot,'text':text,'category':'bark' if id.startswith('route_skill') else 'story','match':'','clip':'Voices/Clips/'+id})
assert len(seen)==len(clips)
(root/'Assets/Resources/Voices/voice-bank.json').write_text(json.dumps({'version':'1.6.0','clips':clips},ensure_ascii=False,indent=2)+'\n')
(root/'Docs/VOICE-SCREENPLAY-1.6.md').write_text('# 裂空 1.6 实时通讯台本\n\n原创合成声线。对白由真实目标、伤害、击破、支援和路线事件触发；排队过期或上下文不再成立时取消。\n\n'+'\n'.join(f"- `{c['id']}` · {['苍凛','绯音','雪璃'][c['pilot']]}：{c['text']}" for c in clips)+'\n')
print('MANIFEST',len(clips))
