"""Render only new campaign takes using the established original pilot references.
No network model download, real-person imitation, or change to existing takes.
"""
from pathlib import Path
import hashlib,json,os,subprocess,time
ROOT=Path(__file__).resolve().parents[1]
os.environ.setdefault('HF_HOME',str(ROOT.parent/'.tools/skybreak-voice/hf-cache'))
import mlx.core as mx
import numpy as np
import soundfile as sf
import imageio_ffmpeg
from pykakasi import kakasi
from mlx_audio.tts.utils import load_model

work=ROOT/'Build/VoiceWork1.12';work.mkdir(parents=True,exist_ok=True)
records_path=work/'generation.json'
records=json.loads(records_path.read_text()) if records_path.exists() else {}
bank=[c for c in json.loads((ROOT/'Assets/Resources/Voices/voice-bank.json').read_text())['clips'] if c['id'].startswith('campaign_')]
roles=json.loads((ROOT/'Tools/voice_cast_16.json').read_text())
refs={
 'zh':[(ROOT/'Tools/VoiceReferences1.6'/(r['id']+'.wav'),r['text']) for r in roles],
 'en':[(ROOT/'Tools/VoiceReferences1.7'/(f'en-{i}.wav'),t) for i,t in enumerate([
  "Watch your right! A bomber's coming in. Stop it! Yes, got it! Whew... the convoy's safe. Stay with me. We're going home together.",
  "Tough little thing, isn't it? Stand back. Let me handle this! Full firepower! Ha, there we go. Easy now. I don't want those lovely wings getting scratched.",
  "Target locked. Move sideways, now! Time... stand still. Wait, is it connected? It recognizes us! I knew you remembered. We can finally go home."])],
 'ja':[(ROOT/'Tools/VoiceReferences1.7'/(f'ja-{i}-v2.wav'),t) for i,t in enumerate([
  '右に注意！ 爆撃機が来る、止めて！ よし、当たった！ ふう、船団は無事。私の声、聞こえる？ 一緒に帰ろうね。',
  'あら、なかなか硬いじゃない。下がって、私に任せて！ 火力全開！ はっ、片づいた。無理しないでね。翼に傷がついたら、私が悲しいわ。',
  '目標ロック。今、横に避けて！ 時間よ、止まって。あ、つながった？ 私たちを認識してる！ 覚えていてくれたんですね。やっと、帰れる。'])]
}
converter=kakasi()
readings={'七百七十人':'ななひゃくななじゅうにん','七百七十':'ななひゃくななじゅう','防波堤':'ぼうはてい','三隻':'さんせき','三人':'さんにん','第七小隊':'だいななしょうたい','明日':'あした','識別':'しきべつ','制空':'せいくう','救助隊':'きゅうじょたい','民間':'みんかん','太陽翼':'たいようよく','接点':'せってん','航法局':'こうほうきょく'}
def kana(text):
 for before,after in readings.items():text=text.replace(before,after)
 return ''.join(piece['hira'] for piece in converter.convert(text))
def path(c,lang):return ROOT/('Assets/Resources/'+c[{'zh':'clip','en':'clipEn','ja':'clipJa'}[lang]]+'.wav')
def signature(c,lang):
 ref,transcript=refs[lang][c['pilot']]
 return hashlib.sha256((c['text'] if lang=='zh' else c[lang]).encode()+ref.read_bytes()+b'campaign-112-native-reading-1').hexdigest()
def done(c,lang):
 p=path(c,lang);record=records.get(lang+'/'+c['id'],{})
 return p.exists() and record.get('text_reference_hash')==signature(c,lang) and record.get('sha256')==hashlib.sha256(p.read_bytes()).hexdigest()
def save(c,lang,audio,sr):
 wave=np.asarray(audio).reshape(-1);window=int(sr*.02)
 rms=np.sqrt(np.mean(np.pad(wave,(0,(-len(wave))%window)).reshape(-1,window)**2,axis=1))
 audible=np.flatnonzero(rms>max(.0015,float(rms.max())*.016))
 assert len(audible),c['id']
 wave=wave[max(0,int(audible[0]*window-sr*.08)):min(len(wave),int((audible[-1]+1)*window+sr*.2))]
 assert .3<len(wave)/sr<24,(c['id'],len(wave)/sr)
 temp=work/'take.wav';sf.write(temp,wave,sr,subtype='PCM_16')
 p=path(c,lang);p.parent.mkdir(parents=True,exist_ok=True)
 subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(),'-hide_banner','-loglevel','error','-y','-i',str(temp),'-af','highpass=f=65,loudnorm=I=-18:TP=-2:LRA=11','-ar','24000','-ac','1','-c:a','pcm_s16le',str(p)],check=True)
 final,rate=sf.read(p)
 records[lang+'/'+c['id']]={'text_reference_hash':signature(c,lang),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'seconds':len(final)/rate,'peak':float(np.max(np.abs(final))),'rms':float(np.sqrt(np.mean(final**2))),'synthetic_original_cast':True}
 records_path.write_text(json.dumps(records,indent=2)+'\n')
 print('DONE',lang,c['id'],round(len(final)/rate,2),flush=True)

if not all(done(c,lang) for lang in refs for c in bank):
 model=load_model(str(ROOT.parent/'.tools/skybreak-voice/models/Qwen3-TTS-12Hz-1.7B-Base-bf16'))
 for lang,language in [('zh','Chinese'),('en','English'),('ja','Japanese')]:
  for pilot in range(3):
   entries=[c for c in bank if c['pilot']==pilot and not done(c,lang)]
   ref,transcript=refs[lang][pilot]
   for offset in range(0,len(entries),3):
    group=entries[offset:offset+3];mx.random.seed(11200+pilot*113+offset+list(refs).index(lang)*179)
    print('GROUP',lang,pilot,[c['id'] for c in group],flush=True)
    results=model.batch_generate(texts=[c['text'] if lang=='zh' else kana(c['ja']) if lang=='ja' else c['en'] for c in group],ref_audio=str(ref),ref_text=kana(transcript) if lang=='ja' else transcript,lang_code=language,temperature=.7,top_p=.95,max_tokens=520,stream=False)
    got=set()
    for result in results:
     got.add(result.sequence_idx);save(group[result.sequence_idx],lang,result.audio,model.sample_rate)
    assert got==set(range(len(group)))
    mx.clear_cache()
assert all(done(c,lang) for lang in refs for c in bank)
print('SKYBREAK_CAMPAIGN_VOICES_COMPLETE',len(records),flush=True)
