"""Author the complete three-act campaign and its original trilingual dialogue.
Preserves existing combat takes; new events use the established fictional cast.
"""
from pathlib import Path
import json

ROOT = Path(__file__).resolve().parents[1]
voices = []
sequences = []

def line(key, pilot, zh, en, ja, emotion='quiet resolve', category='cinematic'):
    event = 'campaign_' + key
    voices.append(dict(id=event, pilot=pilot, text=zh, en=en, ja=ja,
        category=category, emotion=emotion, match='', clip='Voices/Clips/'+event,
        clipEn='Voices/Clips/en/'+event, clipJa='Voices/Clips/ja/'+event))
    return event

def beat(key, pilot, world, heading, body, zh, en, ja, emotion='quiet resolve'):
    return dict(voice=line(key,pilot,zh,en,ja,emotion), world=world,
                heading=heading, body=body, speaker=pilot)

def sequence(key,title,beats):
    sequences.append(dict(id=key,title=title,beats=beats))

sequence('before_0','序幕 / 最后一条航线',[
    beat('open_0',2,0,'天空忘记了我们的名字',
         '停战后的第七天，轨道防卫网「天环」仍在执行净空协议。居民身份库被封锁，民用航线上的每一个信号都被当作敌人。曾参与天环导航系统的雪璃，带回了重新打开识别链路的方法。',
         '它不是认不出我们。有人锁住了它的记忆。',
         "It hasn't forgotten us. Someone locked its memories away.",
         '私たちを忘れたわけじゃない。誰かが記憶を閉ざしたんです。'),
    beat('open_1',1,0,'七百七十个明天',
         '曙光号、归港号和白鹭号已离开避难港。三艘船载着七百七十名居民，也带着最后一份离线身份名册。守望军的海上要塞正封死出海口。',
         '三艘船，七百七十个人。今天，一个都别掉队。',
         'Three ships. Seven hundred and seventy people. Nobody gets left behind today.',
         '三隻に七百七十人。今日は、誰も置いていかないわ。','warm firmness'),
    beat('open_2',0,0,'第七飞行小队，出击',
         '苍凛领航，绯音承担火力掩护，雪璃负责识别密钥。先送船队通过海岸，再夺回城市导航站，最后抵达天环。被你选中的战机将担任本次行动的先锋。',
         '先护住船队，再去夺回天空。第七小队，跟上我！',
         'Protect the convoy, then take back the sky. Seventh Squadron, with me!',
         'まず船団を守る。それから空を取り戻す。第七小隊、私に続いて！','decisive anticipation')])

sequence('after_0','幕间 / 从海面传来的回信',[
    beat('coast_after_0',0,0,'要塞沉默以后',
         '利维坦的武装从雷达上消失。撤离结果已写入本次行动记录：幸存的船只驶向外海，受损船只等待救援。舰长加兰的封锁命令，终于没有挡住这条航线。',
         '海上火力停了。救援队，接下来的航线交给你们。',
         'The sea guns are silent. Rescue teams, the route is yours.',
         '海上砲火、停止。救助隊、ここからの航路をお願いします。','relieved but focused'),
    beat('coast_after_1',2,0,'名册只是第一把钥匙',
         '船队把身份名册传回小队，但轨道网络仍拒绝连接。必须恢复城内三处供电站，让医院、避难站和导航中心重新上线。完整救下船队，还会获得下一章的僚机支援信标。',
         '名册收到了。下一步，让城里的导航站重新亮起来。',
         'We have the registry. Next, bring the city navigation stations back online.',
         '名簿を受信。次は、街の航法局に明かりを戻しましょう。')])

sequence('before_1','第二幕 / 暴雨中的名字',[
    beat('city_before_0',1,1,'没有天亮的城市',
         '人为雷暴覆盖主干道，救护车与撤离巴士停在熄灭的交通灯前。干扰塔把能源导向制空平台「暴风眼」，居民却以为供电永远不会再回来。',
         '下面还有撤离巴士。把电抢回来，给他们一条能走的路。',
         'Evacuation buses are still down there. Take back the power. Give them a way out.',
         '下には避難バスが残ってる。電気を取り戻して、通れる道を作るわよ。','concern turning to resolve'),
    beat('city_before_1',2,1,'恢复的不是一串数字',
         '三个干扰节点对应三个真实街区。摧毁节点后，街区照明会逐片恢复，撤离车辆重新行驶，轨道雷击也会减弱。居民的识别数据将沿着恢复的电网送往天环。',
         '灯每亮起一片，就有一批人的名字能传上去。',
         'Every district we light up sends another group of names to the sky.',
         '街区に明かりが戻るたび、そこにいる人たちの名前が空へ届きます。','gentle conviction'),
    beat('city_before_2',0,1,'穿过暴风眼',
         '守望军王牌卡西娅把风暴当作不可逾越的边界。小队必须从她的旋转弹幕中穿过，拆除制空平台的涡轮，让雨中的人看见真正的天空。',
         '她能封住天空，封不住我们回家的路。出发！',
         "She can close the skies, but she can't close our way home. Move out!",
         '空を塞がれても、帰り道まで諦めない。行こう！','confident challenge')])

sequence('after_1','幕间 / 城市抬起了头',[
    beat('city_after_0',1,1,'风暴正在退去',
         '暴风眼停止运转。已恢复供电的街区继续疏散居民，剩余街区交由地面队伍抢修。城市终于能听见小队的通讯，雨声里第一次传来回应。',
         '听，地面频道接通了。嗯……这声音可比爆炸好听。',
         'Listen. The ground channel is back. Mm... that sounds better than explosions.',
         '聞いて。地上の回線が戻ったわ。ふふ、爆発よりずっといい音ね。','soft relieved smile'),
    beat('city_after_1',2,1,'最后一项权限',
         '雪璃确认，净空协议只能从轨道核心解除。三处导航站全部恢复时，小队能得到额外反应堆能量。即使任务有遗憾，行动仍会继续；未接通的数据将影响最后的救援方式。',
         '我曾经相信这套系统。现在，我要让它学会停下来。',
         'I used to believe in this system. Now I have to teach it how to stop.',
         '私は、この仕組みを信じていました。今度は、止まることを教えます。','restrained remorse becoming resolve')])

sequence('before_2','第三幕 / 把选择还给人类',[
    beat('orbit_before_0',2,2,'寂静天环',
         '大气层在机翼下沉成蓝色弧线。轨道接驳船正在等待靠港，防卫阵列却仍把它们列为威胁。三枚封锁接点保护着身份上传通道。',
         '接驳船还在等。解除三个接点，我来完成上传。',
         "The shuttles are waiting. Release the three contacts. I'll handle the upload.",
         '連絡船が待っています。三つの接点を解除してください。転送は私が。','precise, calm urgency'),
    beat('orbit_before_1',0,2,'命令的尽头',
         '轨道指挥官诺克特仍在维持最后的防线。他知道战争已经结束，却不敢相信交还控制权以后会发生什么。小队必须击破炽天使的防护节点，终止那条永不结束的命令。',
         '战争已经结束了。最后这道命令，由我们来终止。',
         "The war is over. We'll put an end to this last order.",
         '戦争はもう終わった。残された命令は、私たちが終わらせる。','solemn determination'),
    beat('orbit_before_2',1,2,'一起回去',
         '这次任务有两个目标：停止核心的攻击，并尽可能保住居民的身份链路。击毁核心可以打开天空；完成上传，才能让接驳船立即得到安全许可。每一次救援都会写进归航后的故事。',
         '别把最后一次飞行说得那么悲壮。我们三个，都要回去。',
         "Don't make this last flight sound like a farewell. All three of us are going home.",
         '最後の飛行を、お別れみたいに言わないで。三人で帰るんだから。','affectionate reassurance')])

sequence('ending_0','结局 / 再一次黎明',[
    beat('ending_full_0',2,2,'名字重新被听见',
         '天环停止开火，完整的身份链路恢复。轨道接驳船沿着点亮的引导灯进入船坞。净空协议被永久解除，今后的每一次防卫命令都需要人类重新确认。',
         '确认，所有民用信号已被识别。天空，交还给你们。',
         'Confirmed. Every civilian signal is recognized. The sky belongs to you again.',
         '確認。すべての民間信号を認識しました。この空を、皆さんに返します。','wonder and release'),
    beat('ending_full_1',1,1,'有灯的归途',
         '医院、避难站与导航中心同时恢复供电。撤离车队不再等待许可，沿着雨后亮起的道路驶向安全区。人们开始说起明天，而不再只问还要等多久。',
         '车队动起来了。慢慢开，明天还有很长的路呢。',
         "The convoy's moving. Take it easy. There's a long road ahead tomorrow.",
         '車列が動き出したわ。ゆっくりでいい。明日も、その先もあるんだから。','warm, relaxed relief'),
    beat('ending_full_2',0,0,'七百七十个明天，都到了',
         '三艘救援船全部越过封锁线。曙光号、归港号和白鹭号在晨光里发回同一条短讯：我们到家了。第七小队的三架战机从港口上空掠过，返航灯依次点亮。',
         '七百七十人，全员到港。我们答应过的，做到了。',
         'Seven hundred and seventy, all safely ashore. We kept our promise.',
         '七百七十人、全員到着。約束、守れたね。','joyful tears held back')])

sequence('ending_1','结局 / 带着名字归航',[
    beat('ending_partial_0',2,2,'天空记住了他们',
         '识别链路接通，天环停止攻击，轨道接驳船获准靠港。但这次行动并非没有遗憾；海岸或城市仍有尚未完成的救援。被救下的人，不会被战报里的数字代替。',
         '通道接通了。我会记住这一次，没能及时回应的名字。',
         "The channel is open. I'll remember the names we couldn't answer in time.",
         '回線はつながりました。間に合わなかった名前も、忘れません。','quiet mixed relief'),
    beat('ending_partial_1',1,1,'修复还在继续',
         '地面抢修队接过小队留下的坐标，为未亮起的街区恢复供电。幸存的撤离车队带来药品与人员，暂时失能的目标被列入下一批救援名单。',
         '把剩下的坐标发给救援队。没做完的事，我们接着做。',
         "Send the remaining coordinates to rescue. We'll finish what we started.",
         '残りの座標を救助隊へ。やり残したことは、これから片づけるわ。','steady compassion'),
    beat('ending_partial_2',0,0,'归航不是遗忘',
         '海上封锁已经解除。三架战机带着居民的名字返航，也带着需要兑现的下一次承诺。天空重新开放，救援终于可以不再伴随炮火。',
         '今天先把他们送回家。明天，我们再去接剩下的人。',
         "Let's bring them home today. Tomorrow, we go back for the others.",
         '今日は、この人たちを家へ。明日は、残っている人たちを迎えに行こう。','hope through responsibility')])

sequence('ending_2','结局 / 最后的守望',[
    beat('ending_guard_0',2,2,'枪声停了，回音还没有到',
         '炽天使被摧毁，天环再也不能开火。身份上传没有及时完成，接驳船必须等待人工引导。雪璃留下仍可使用的链路，让救援队逐条确认民用信号。',
         '攻击停止了。识别还没完成，我会守着这条回线。',
         "The attacks have stopped. Identification isn't finished. I'll stay on this channel.",
         '攻撃は止まりました。識別はまだ終わっていない。この回線は、私が守ります。','weary determination'),
    beat('ending_guard_1',1,1,'一盏一盏，重新点亮',
         '城市用已经恢复的设备建立临时导航。地面队伍从最需要帮助的街区开始，把自动系统没能完成的事交回人手里。车队在人工信号下缓慢前进。',
         '系统帮不上忙，就由人来。一个一个接，我们有耐心。',
         "If the system can't help, we will. One at a time. We've got patience.",
         '仕組みが助けてくれないなら、人がやるまでよ。一人ずつ、焦らずに。','practical, reassuring warmth'),
    beat('ending_guard_2',0,0,'下一次起飞的理由',
         '航线已经打开，小队却没有马上离开。三架战机继续为救援船提供掩护，直到最后一条求救信号得到回应。战争的结尾，不是一声爆炸，而是有人终于抵达岸边。',
         '我们已经打开了航线。现在，守到最后一个人回家。',
         "The route is open. Now we hold it until the last person gets home.",
         '航路は開いた。最後の一人が帰るまで、ここを守ろう。','resolute, quietly hopeful')])

for i,(zh,en,ja) in enumerate([
    ('下次起飞，也让我听见你的声音。',"Next time we fly, let me hear your voice again.",'次に飛ぶ時も、あなたの声を聞かせてね。'),
    ('回去喝杯热的吧。这次，我请。',"Let's get something warm when we're back. This one's on me.",'帰ったら、温かいものでも飲みましょ。今度は私のおごり。'),
    ('天空这么大，终于可以自己选择航线了。',"The sky is so wide. At last, we can choose our own course.",'空はこんなに広い。やっと、自分で航路を選べます。')]):
    sequence('pilot_'+str(i),'尾声 / 第七小队',[
        beat('pilot_'+str(i),i,0,['苍凛的承诺','绯音的归处','雪璃的新航线'][i],
             ['苍凛把这次行动的通讯频率保留下来。以后，只要有人在这条频率上求助，第七小队就会回应。',
              '绯音没有再把返航当成侥幸。她在驾驶舱里贴上一张新清单：修好机翼，补足弹药，带队友去喝一杯热的。',
              '雪璃交出了天环的独占权限。她不再替别人决定安全的边界，而是继续飞行，和队友一起寻找新的航线。'][i],
             zh,en,ja,'intimate natural closing')])

combat=[
 ('beacon',0,'防波堤的航标亮了！沿着灯带，护住船队。','The breakwater beacons are lit! Follow the lights and cover the convoy.','防波堤の標識が点いた！光の先へ、船団を守って！','bright recognition'),
 ('blockade',1,'封锁线就在前面。别急，先拆掉它的侧翼。',"The blockade is ahead. Easy. Take its flanks apart first.",'封鎖線はすぐそこ。焦らず、まずは両脇を崩すわよ。','confident tactical direction'),
 ('metro',1,'电回来了，撤离车开始动了。掩护他们通过路口！',"Power's back. The evacuation vehicles are moving. Cover the crossing!",'電気が戻った。避難車両が動いてる。交差点を抜けるまで援護して！','alert encouragement'),
 ('weather',0,'三个街区都亮了！雷暴正在减弱，抓住这个窗口。','All three districts are lit! The storm is weakening. Take this opening.','三つの街区が全部点いた！嵐が弱まってる。今がチャンス！','exhilarated confirmation'),
 ('array',2,'太阳翼开始转向。第一段通道，已经属于我们。',"The solar wings are turning. The first channel is ours.",'太陽翼が向きを変えています。最初の回線は、もう私たちのものです。','focused discovery'),
 ('uplink_hold',0,'还有封锁接点没解除。保持火力，给雪璃争取时间！','There are still locked contacts. Keep firing. Buy Yuki more time!','まだ封鎖接点が残ってる。射撃を続けて、ユキに時間を！','urgent rally')]
for key,pilot,zh,en,ja,emotion in combat:
    line(key,pilot,zh,en,ja,emotion,'story')

folder=ROOT/'Assets/Resources/Campaign';folder.mkdir(parents=True,exist_ok=True)
(folder/'campaign.json').write_text(json.dumps(dict(version='1.12.0',sequences=sequences),ensure_ascii=False,indent=2)+'\n')
path=ROOT/'Assets/Resources/Voices/voice-bank.json';bank=json.loads(path.read_text())
bank['clips']=[c for c in bank['clips'] if not c['id'].startswith('campaign_')]+voices
bank['version']='1.12.0';path.write_text(json.dumps(bank,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(sequences=len(sequences),new_voice_events=len(voices),total_events=len(bank['clips']),languages=['zh','en','ja'])))
