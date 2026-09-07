"""Check local generated dialogue for missing words, silence and clipping with independent ASR."""
import argparse,os,json,re
from pathlib import Path
root=Path(__file__).resolve().parents[1];p=argparse.ArgumentParser();p.add_argument('--model',required=True);p.add_argument('--ids',default='');args=p.parse_args()
os.environ.setdefault('HF_HOME',str(root.parent/'.tools/skybreak-voice/hf-cache'))
import numpy as np
import soundfile as sf
from pypinyin import lazy_pinyin
from mlx_audio.stt import load
model=load(args.model);entries=json.loads((root/'Assets/Resources/Voices/voice-bank.json').read_text())['clips'];out=root/'Build/VoiceWork/asr-results.json'
existing=json.loads(out.read_text()) if out.exists() else [];results={r['id']:r for r in existing};selected=set(args.ids.split(',')) if args.ids else None

def phonemes(s):return lazy_pinyin(''.join(re.findall(r'[\u4e00-\u9fff]',s)))
def distance(a,b):
 row=list(range(len(b)+1))
 for i,x in enumerate(a):
  nextrow=[i+1]
  for j,y in enumerate(b):nextrow.append(min(nextrow[-1]+1,row[j+1]+1,row[j]+(x!=y)))
  row=nextrow
 return row[-1]
for entry in entries:
 if selected and entry['id'] not in selected:continue
 audio=root/('Assets/Resources/'+entry['clip']+'.wav');wave,sr=sf.read(audio);actual=model.generate(str(audio),language='Chinese').text
 expected=phonemes(entry['text']);observed=phonemes(actual);error=distance(expected,observed)/max(1,len(expected))
 record={'id':entry['id'],'expected':entry['text'],'recognized':actual,'syllable_error_rate':round(error,4),'seconds':len(wave)/sr,'peak':float(np.max(np.abs(wave))),'rms':float(np.sqrt(np.mean(wave**2)))}
 results[entry['id']]=record;out.write_text(json.dumps(list(results.values()),ensure_ascii=False,indent=2)+'\n')
 print('REVIEW' if error>.15 else 'OK',entry['id'],round(error,3),actual,flush=True)
assert len(results)==len(entries) or selected
print('ASR_COMPLETE',len(results),'REVIEW',sum(r['syllable_error_rate']>.15 for r in results.values()),flush=True)
