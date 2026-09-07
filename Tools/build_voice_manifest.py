"""Author the original Mandarin performance script for SKYBREAK's adult pilots."""
from pathlib import Path
import json
root=Path(__file__).resolve().parents[1]
clips=[]
def add(key,pilot,text,category='bark',match=''):
 clips.append(dict(id=key,pilot=pilot,text=text,category=category,match=match,clip='Voices/Clips/'+key))
roles=[
 dict(select=['我是苍凛。准备就绪，随时可以起飞。','准备好了吗？这次，也一起回来。'],launch=['跟紧我，出发！'],skill=['苍蓝协奏，掩护交给我！','侧翼清空，一起上！'],bomb=['裂空，展开！','这条路，由我打开！'],overdrive=['天翼展开，全力突破！'],hurt=['没事，我还能飞。','擦到了，继续前进。'],danger=['装甲告急。稳住，先避开弹幕！'],victory=['听见了吗？他们在说，欢迎回家。'],defeat=['别放弃。我们还会一起起飞。'],profile=['我是苍凛。约好了，一起看见明天的天空。']),
 dict(select=['我在这儿。放心，火力管够。','选我呀？嗯，有眼光。'],launch=['坐稳了，开工！'],skill=['绯焰舰装，齐射！','让我来，加点火力！'],bomb=['退后，大家伙来了！','这一发，送你们的！'],overdrive=['引擎全开！这才叫痛快！'],hurt=['啧，刮花我的机翼了。','小伤。待会儿再修。'],danger=['装甲快撑不住了。先躲，别硬扛！'],victory=['干得漂亮。回家吧，该听听海的声音了。'],defeat=['人没事就好。修好了，我们再来。'],profile=['别急嘛。先听听引擎的声音。有我在，火力管够。']),
 dict(select=['雪璃，航线已确认。','你的信号，我收到了。'],launch=['航线清晰。我们走。'],skill=['时间，停下来。','给我们，留一点时间。'],bomb=['清除阻碍。裂空，释放。','现在，打开航路！'],overdrive=['解除限制，重写航线！'],hurt=['损伤可控，不用担心。','我还在。继续。'],danger=['警告，装甲临界。先保持距离。'],victory=['把选择天空的权利，还给他们。我们做到了。'],defeat=['如果计算里没有希望，就重新计算。'],profile=['我是雪璃。如果计算里没有希望，就重新计算。'])]
for pilot,events in enumerate(roles):
 for event,variants in events.items():
  for i,line in enumerate(variants):add(f'{event}_{pilot}_{i}',pilot,line,'profile' if event=='profile' else 'bark')
briefings=[
 ['海面下是最后一支撤离船队。跟紧我，我们替他们打开航线。','这场雷暴不是天气。干扰塔困住了整座城市。我们去把灯重新点亮。','天环把所有人类信号都判成了威胁。带着大家的声音，飞进它的心脏。'],
 ['船队还在等我们。嗯，引擎状态不错。来吧，把前面的封锁轰开。','看到那些熄灭的街灯了吗？干扰塔就是罪魁祸首。走，给这座城重新通上电。','轨道上的大家伙，还真是不肯让路啊。那就用我们的方式，敲开它的大门。'],
 ['撤离船队已抵达近海。前方存在封锁。我们打开航线，带他们回家。','雷暴来自轨道干扰。找到城市的供电节点，或许还能救下被困的人。','天环曾经想保护我们。现在，它忘了自由是什么。让我亲口告诉它。']]
for pilot,lines in enumerate(briefings):
 for stage,line in enumerate(lines):add(f'briefing_{pilot}_{stage}',pilot,line,'briefing')
stories=[
 ('coast_escort',0,'船队就在下方。清理它们前面的空域，我来照看侧翼。'),
 ('city_crossfire',1,'他们开始交叉锁定了。瞄准线停住后再转向，别急着交炸弹。'),
 ('orbit_docks',2,'这些是以前的维护船坞……跟着环形航标走，巡逻机正在换班。'),
 ('coast_lights',1,'船队开始移动了。每一盏灯，都是一个等着回家的人。'),
 ('city_childhood',0,'小时候，我以为城市的灯永远不会熄灭……所以这次一定要赢。'),
 ('orbit_stars',2,'天环听得到我。我知道它还记得，我们曾一起看过星星。'),
 ('coast_supply',2,'补给运输机正在穿越防线。击落它，回收武装和修复模块。'),
 ('city_lasers',0,'小心道路上方的瞄准线。先离开标记，再反击。'),
 ('orbit_graze',1,'擦过弹幕可以储存超载能量。核心即将暴露。'),
 ('coast_elite',1,'前方是干扰指挥机！击落它，我就能把撤离信号送出去。'),
 ('city_elite',1,'城市的供电钥匙就在那台重装机里。别让它跑了！'),
 ('orbit_elite',2,'它正在覆盖人类识别码。帮我争取一次重新连接的机会。'),
 ('coast_safe',0,'撤离信号已连通。再向前一步，就是他们的明天。'),
 ('coast_missed',0,'船队还在等待回音。先打穿要塞，我们仍有机会。'),
 ('city_safe',1,'电网回来了！看，下方的灯正在重新亮起。'),
 ('city_missed',1,'干扰还没解除。把剩下的能量留给制空平台。'),
 ('orbit_safe',2,'密钥完整。我会让天环记起，它曾经想保护的人。'),
 ('orbit_missed',2,'密钥受损，但我的声音还在。让我亲口告诉它。'),
 ('coast_clear',0,'要塞沉默了。撤离船队已通过防线，我们前往城市。'),
 ('city_clear',1,'雷暴正在消散。最后一个指令源在轨道上——准备爬升。'),
 ('orbit_clear',2,'收到人类识别码。天环停止攻击……我们把天空还给了所有人。'),
 ('rescue_complete',1,'漂亮！信号恢复了。你的侧翼交给我。')]
for key,pilot,line in stories:add('story_'+key,pilot,line,'story',line)
for stage,boss in enumerate(['利维坦 · 海上要塞','暴风眼 · 制空平台','炽天使 · 天环核心']):
 original=boss+'已进入航线。注意核心暴露与激光预警。'
 line=['利维坦进入航线。先拆掉炮台，等核心暴露再集中火力。','暴风眼接近。留意激光预警，别停在瞄准线上。','炽天使已苏醒。摧毁护盾节点，我们就能碰到它的核心。'][stage]
 add('story_boss_'+str(stage),2,line,'story',original)
assert len({c['id'] for c in clips})==len(clips)
p=root/'Assets/Resources/Voices/voice-bank.json';p.parent.mkdir(parents=True,exist_ok=True)
p.write_text(json.dumps({'version':'1.5.0','language':'zh-CN','synthetic':True,'clips':clips},ensure_ascii=False,indent=2)+'\n')
print(len(clips),'original lines;',sum(len(c['text']) for c in clips),'characters')
