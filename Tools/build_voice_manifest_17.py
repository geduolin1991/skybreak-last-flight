"""Author complete language coverage and event-specific original enemy performances."""
import json
from pathlib import Path
root=Path(__file__).resolve().parents[1]
path=root/'Assets/Resources/Voices/voice-bank.json'
bank=json.loads(path.read_text())
translations={}
for row in (root/'Tools/voice_translations_17.tsv').read_text().splitlines():
    key,en,ja=row.split('\t');translations[key]=(en,ja)
bank['clips']=[c for c in bank['clips'] if c['category']!='boss']
assert len(translations)==len(bank['clips'])==98
for c in bank['clips']:c['en'],c['ja']=translations[c['id']]
bosses=[
    (3,'entry','挑衅','小队也敢挑战利维坦？这条航线，到此为止。','A little squadron, against Leviathan? Your flight ends here.','その小隊でリヴァイアサンに挑むか。お前たちの航路は、ここまでだ。'),
    (3,'phase','强撑镇定','左舷失压？封死隔舱！全炮列，压住她们！','Port side losing pressure? Seal the bulkheads! All batteries, pin them down!','左舷が危険だ！ 隔壁を閉めろ！ 全砲門、撃て！'),
    (3,'critical','惊慌','核心温度失控？不可能！备用回路，快接上！','Core temperature runaway? Impossible! Backup circuit! Connect it, now!','コア温度が制御不能？ ありえん！ 予備回路、早くつなげ！'),
    (3,'death','不服','不！我还没有……认输！','No! I have not... surrendered!','違う！ 私はまだ、負けてなど！'),
    (4,'entry','嘲笑','哎呀，护送完小船，又来陪我玩了？别掉得太快哦。','Oh, done babysitting those boats? Come play. Try not to fall too fast.','あら、小舟のお守りは終わり？ 遊んであげる。すぐに落ちないでね。'),
    (4,'phase','恼怒','打坏我的翼阵？啧，那就贴近点，尝尝雷暴！','You broke my wing array? Tch. Come closer. Taste the storm!','私の翼を壊したの？ ちっ、近くに来なさい。雷の嵐を浴びて！'),
    (4,'critical','恐惧','等等，推力怎么掉了？给我拉起来！快啊！','Wait! Where did my thrust go? Pull up! Come on!','待って、推力が落ちてる？ 上がって！ 早く！'),
    (4,'death','尖叫','不要！弹射器……啊——！','No! The ejector... Aah!','いや、開かない！ ああっ！'),
    (5,'entry','严阵以待','炽天使，接管防线。你们的信号，我会亲自审判。','Seraph, assuming defensive control. I will judge your signal myself.','セラフ、防衛線を引き継ぐ。お前たちの信号は、私が裁定する。'),
    (5,'phase','动摇','护盾节点消失了……你们真的，要推翻这个命令？','The shield nodes are gone... You truly intend to defy this order?','防護ノードが消えた。本当に、この命令に逆らうつもりか。'),
    (5,'critical','悔悟','核心正在崩溃。原来……错的是这条命令。','The core is collapsing. So... the order itself was wrong.','コアが崩壊している。そうか、間違っていたのは、この命令だ。'),
    (5,'death','低声诀别后沉默','到此为止吧。天空……还给你们。','Then let it end. The sky... is yours.','もう、終わりにしよう。空は、お前たちに返す。'),
]
for pilot,event,emotion,zh,en,ja in bosses:
    id=f'commander_{pilot-3}_{event}'
    bank['clips'].append(dict(id=id,pilot=pilot,category='boss',match='',text=zh,en=en,ja=ja,emotion=emotion,clip='Voices/Clips/'+id))
for c in bank['clips']:
    c['clipEn']='Voices/Clips/en/'+c['id'];c['clipJa']='Voices/Clips/ja/'+c['id']
bank.update(version='1.7.0',language='zh,en,ja',synthetic=True)
path.write_text(json.dumps(bank,ensure_ascii=False,indent=2)+'\n')
print('110 events; 330 authored language takes')
