"""Independent ASR and signal review of every new multilingual take. Resume by WAV hash."""
import json,os,re,hashlib,unicodedata
from pathlib import Path
root=Path(__file__).resolve().parents[1]
os.environ.setdefault('HF_HOME',str(root.parent/'.tools/skybreak-voice/hf-cache'))
import numpy as np
import soundfile as sf
from mlx_audio.stt import load
from pypinyin import lazy_pinyin
model=load(str(root.parent/'.tools/skybreak-voice/models/Qwen3-ASR-0.6B-8bit'))
entries=json.loads((root/'Assets/Resources/Voices/voice-bank.json').read_text())['clips']
out=root/'Build/VoiceWork1.12/asr.json';results=json.loads(out.read_text()) if out.exists() else {}
def norm(text,lang):
 if lang=='zh':return lazy_pinyin(''.join(re.findall(r'[\u4e00-\u9fff]',text)))
 if lang=='en':return re.findall(r'[a-z]+',text.lower().replace("'",''))
 return list(re.sub(r'[\W_]+','',unicodedata.normalize('NFKC',text)))
def distance(a,b):
 row=list(range(len(b)+1))
 for i,x in enumerate(a):
  n=[i+1]
  for j,y in enumerate(b):n.append(min(n[-1]+1,row[j+1]+1,row[j]+(x!=y)))
  row=n
 return row[-1]
for lang,language in [('zh','Chinese'),('en','English'),('ja','Japanese')]:
 for c in entries:
  if not c['id'].startswith('campaign_'):continue
  path=root/('Assets/Resources/'+c['clip' if lang=='zh' else 'clipEn' if lang=='en' else 'clipJa']+'.wav');key=lang+'/'+c['id'];sha=hashlib.sha256(path.read_bytes()).hexdigest()
  if key in results and results[key]['sha256']==sha:continue
  wave,sr=sf.read(path);actual=model.generate(str(path),language=language).text;expected=c['text'] if lang=='zh' else c[lang]
  a=norm(expected,lang);b=norm(actual,lang);error=distance(a,b)/max(1,len(a))
  result={'expected':expected,'recognized':actual,'error_rate':round(error,3),'seconds':len(wave)/sr,'peak':float(np.max(np.abs(wave))),'rms':float(np.sqrt(np.mean(wave**2))),'sha256':sha}
  assert result['peak']<.99 and result['rms']>.005 and .3<result['seconds']<24,(key,result)
  results[key]=result;out.write_text(json.dumps(results,ensure_ascii=False,indent=2)+'\n')
  print('REVIEW' if error>(.24 if lang=='ja' else .18) else 'OK',key,round(error,2),actual,flush=True)
print('ASR_COMPLETE',len(results),flush=True)
