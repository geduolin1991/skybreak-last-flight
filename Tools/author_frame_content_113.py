from pathlib import Path
import json
root=Path(__file__).resolve().parents[1]
entries=[
('deploy_0',0,'岚刃展开！侧翼交给我，跟上！','Skyrazor, deploy! I have the flanks. Stay with me!','スカイレイザー、展開！両脇は任せて、ついてきて！'),
('deploy_1',1,'赤垒，炮架锁定。都躲到我身后！','Bastion. Cannons locked. Everyone, get behind me!','バスティオン、砲架固定。みんな、私の後ろへ！'),
('deploy_2',2,'霜环锁定。让这道光，穿过封锁。','Parallax, locked. Let this light pierce the blockade.','パララックス、ロック完了。この光で、封鎖を貫きます。'),
('counter_0',0,'是船队的测距信号！弱点标出来了，开火！','The convoy is sending range data! Weak point marked. Fire!','船団から測距信号！弱点が見えた、撃って！'),
('counter_1',1,'把电送进它的护盾。哈，城市可不是你的电池！',"Send the power into its shield. Ha! This city isn't your battery!",'電力を敵の盾へ。ふふ、この街はあなたの電池じゃないのよ！'),
('counter_2',2,'名册正在回应！天环，听清楚——他们是人，不是目标！',"The registry is answering! Listen, Sky Array. These are people, not targets!",'名簿が応答しています！聞いて、天環。この人たちは、標的じゃない！'),
('signature',2,'签名是我的……但封锁命令不是。我要亲手撤销它！',"That's my signature... but I never ordered a blockade. I'll revoke it myself!",'署名は、私のもの。でも封鎖は命じていない。私の手で、取り消します！')]
p=root/'Assets/Resources/Voices/voice-bank.json';bank=json.loads(p.read_text());bank['clips']=[c for c in bank['clips'] if not c['id'].startswith('frame113_')]
for key,pilot,zh,en,ja in entries:
 event='frame113_'+key
 bank['clips'].append(dict(id=event,pilot=pilot,text=zh,en=en,ja=ja,category='story',emotion='focused urgency with a decisive release',match='',clip='Voices/Clips/'+event,clipEn='Voices/Clips/en/'+event,clipJa='Voices/Clips/ja/'+event))
bank['version']='1.13.0';p.write_text(json.dumps(bank,ensure_ascii=False,indent=2)+'\n')
p=root/'Assets/Resources/Campaign/campaign.json';story=json.loads(p.read_text());story['version']='1.13.0'
for seq in story['sequences']:
 if seq['id']=='after_0':
  seq['beats'][0]['body']=seq['beats'][0]['body'].split(' 船员记录下')[0].split(' 暴风眼并非只靠')[0].split(' 雪璃发现封锁指令')[0].split(' 最后回应天环的')[0]+' 船员记录下利维坦切换武装时的测距特征。海面上传来的回信，也成为了战机反击的坐标。'
 if seq['id']=='before_1':
  seq['beats'][0]['body']=seq['beats'][0]['body'].split(' 船员记录下')[0].split(' 暴风眼并非只靠')[0].split(' 雪璃发现封锁指令')[0].split(' 最后回应天环的')[0]+' 暴风眼并非只靠自身供能，它还在抽取居民电网的电力。夺回三个街区，就能把这条供能线路反过来使用。'
 if seq['id']=='before_2':
  seq['beats'][0]['body']=seq['beats'][0]['body'].split(' 船员记录下')[0].split(' 暴风眼并非只靠')[0].split(' 雪璃发现封锁指令')[0].split(' 最后回应天环的')[0]+' 雪璃发现封锁指令使用了她留下的导航签名。原本为救援开放的权限，被改成了拒绝平民通行的命令。她必须带着海面上保存的名册，亲手撤回这份授权。'
 if seq['id']=='ending_0':
  seq['beats'][0]['body']=seq['beats'][0]['body'].split(' 船员记录下')[0].split(' 暴风眼并非只靠')[0].split(' 雪璃发现封锁指令')[0].split(' 最后回应天环的')[0]+' 最后回应天环的，不是更高级的武器口令，而是船队上传的七百七十个名字。被保护的人，亲手切断了战争的识别链。'
p.write_text(json.dumps(story,ensure_ascii=False,indent=2)+'\n')
s=(root/'Tools/render_campaign_voices_112.py').read_text().replace('Build/VoiceWork1.12','Build/VoiceWork1.13').replace("startswith('campaign_')","startswith('frame113_')").replace('campaign-112-native-reading-1','frame-113-native-reading-1').replace('11200+pilot','11300+pilot').replace('SKYBREAK_CAMPAIGN_VOICES_COMPLETE','SKYBREAK_FRAME_VOICES_COMPLETE')
(root/'Tools/render_frame_voices_113.py').write_text(s)
