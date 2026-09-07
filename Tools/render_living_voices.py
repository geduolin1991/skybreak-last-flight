"""A new, more projected original cast; event text and performance are both authored.
VoiceDesign directs the reference performance; Base clones that performance.
Base does not accept per-line emotion instructions, so none are silently ignored.
"""
import argparse,json,os,subprocess
from pathlib import Path
root=Path(__file__).resolve().parents[1]
(root/'Build/WorldWork').mkdir(parents=True,exist_ok=True)
models=root.parent/'.tools/skybreak-voice/models'
p=argparse.ArgumentParser();p.add_argument('--design-only',action='store_true');args=p.parse_args()
os.environ.setdefault('HF_HOME',str(root.parent/'.tools/skybreak-voice/hf-cache'))
import mlx.core as mx
import numpy as np
import soundfile as sf
from mlx_audio.tts.utils import load_model
refs=root/'Tools/VoiceReferences1.6';refs.mkdir(exist_ok=True)
roles=json.loads((root/'Tools/voice_cast.json').read_text())
performances=[
 ('全队，注意右侧！俯冲机冲过来了，拦住它！好，打中了！呼……船队还在。听见我的声音了吗？别掉队，我们一起回家。','保留清脆明亮的年轻成年队长声线。这是一段有动作的战斗演出：第一句立即警觉提高投射，拦住它要有爆发和重音；打中了短促明亮惊喜；呼之后自然松一口气；最后面对队友真切温暖地放软声音。音高和力度随事件明显变化，语句紧凑，有急有缓，别平着念。'),
 ('啧，右边这架还挺硬。退后，让姐姐来！火力全开！哈，拆掉了。嗯，这下舒服了。别逞强啊，机翼刮花了，我可要心疼的。','保留有魅力的低暖成熟御姐声线，略带沙质和慵懒笑意。前句像随口调侃，退后突然认真果断，火力全开喊出胸腔的力量、不是尖叫；哈拆掉了是真实畅快的笑声和胜利感；末句放低放松，轻轻关照队友。语气会转折，重音明确，句尾干净，不能全程慢速或气声念稿。'),
 ('锁定了。现在，横移！时间，停下来。嗯……接通了？它认出我们了！我就知道，你还记得。我们终于，可以回家了。','保留清冷通透、克制知性的成年女导航员声线。锁定了迅速确认，横移突然短促紧迫；停下来沉着有力，随后接通了带迟疑和呼吸，认出我们时真实惊喜，末尾如释重负、柔和而带笑。冷静不等于机械，要有明显音高转折与轻重节奏，避免每个字等长和句尾统一下落。')]
for i,role in enumerate(roles):
 role['text']=performances[i][0];role['direction']+=' '+performances[i][1];role['seed']+=16000
(root/'Tools/voice_cast_16.json').write_text(json.dumps(roles,ensure_ascii=False,indent=2)+'\n')
missing=[r for r in roles if not (refs/(r['id']+'.wav')).exists()]
if missing:
 model=load_model(str(models/'Qwen3-TTS-12Hz-1.7B-VoiceDesign-bf16'))
 for role in missing:
  mx.random.seed(role['seed']);print('DESIGN',role['id'],flush=True)
  chunks=list(model.generate_voice_design(text=role['text'],instruct=role['direction'],language='Chinese',temperature=.74,top_p=.95,max_tokens=620))
  wave=np.concatenate([np.array(c.audio).reshape(-1) for c in chunks]);sf.write(refs/(role['id']+'.wav'),wave,model.sample_rate,subtype='PCM_16');print('REFERENCE',role['id'],len(wave)/model.sample_rate,flush=True);mx.clear_cache()
 del model;mx.clear_cache()
if args.design_only:raise SystemExit
# Reuse the tested renderer with dedicated cast/references; all clips are re-performed.
src=(root/'Tools/render_voices.py').read_text().replace("Tools/voice_cast.json","Tools/voice_cast_16.json").replace("Tools/VoiceReferences'","Tools/VoiceReferences1.6'").replace("temperature=.68","temperature=.72").replace("LRA=9","LRA=11")
script=root/'Build/WorldWork/render_voice_takes.py';script.write_text(src.replace("root=Path(__file__).resolve().parents[1]
(root/'Build/WorldWork').mkdir(parents=True,exist_ok=True)","root=Path("+repr(str(root))+")"))
bank=json.loads((root/'Assets/Resources/Voices/voice-bank.json').read_text())['clips']
# Resume by source text and reference fingerprint, never confuse old WAVs with new takes.
import hashlib
fingerprint=hashlib.sha256((root/'Assets/Resources/Voices/voice-bank.json').read_bytes()+b''.join((refs/(r['id']+'.wav')).read_bytes() for r in roles)).hexdigest()
state=root/'Build/WorldWork/voice-render-state.json';done=json.loads(state.read_text()) if state.exists() else {}
if done.get('fingerprint')!=fingerprint:done={'fingerprint':fingerprint,'ids':[]}
for start in range(0,len(bank),12):
 ids=[c['id'] for c in bank[start:start+12] if c['id'] not in done['ids']]
 if not ids:continue
 subprocess.run([__import__('sys').executable,str(script),'--model',str(models/'Qwen3-TTS-12Hz-1.7B-Base-bf16'),'--ids',','.join(ids),'--seed-offset','1607'],check=True)
 done['ids']+=ids;state.write_text(json.dumps(done,indent=2)+'\n')
print('LIVING_VOICES_COMPLETE',len(done['ids']),flush=True)
