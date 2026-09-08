"""Generate original English/Japanese casts and emotion-directed enemy radio takes.
No real-person source voices. VoiceDesign performs the enemy events directly;
Base reproduces each original pilot reference in its native target language.
"""
import hashlib,json,os,subprocess,time
from pykakasi import kakasi
_kana=kakasi()
# Counter words and inflected kanji need authored readings; a character converter
# cannot choose these reliably in a combat script.
JAPANESE_READINGS={"天翼":"てんよく","後ね":"あとね","救助船":"きゅうじょせん","三基":"さんき","一基":"いっき","七百人":"ななひゃくにん","民間船":"みんかんせん","入りました":"はいりました","失った":"うしなった","型ドローン":"がたドローン","入って":"はいって","射線上":"しゃせんじょう","来なさい":"きなさい"}
def japanese_reading(text):
 for word,reading in JAPANESE_READINGS.items():text=text.replace(word,reading)
 return "".join(x["hira"] for x in _kana.convert(text))
from pathlib import Path
root=Path(__file__).resolve().parents[1]
os.environ.setdefault('HF_HOME',str(root.parent/'.tools/skybreak-voice/hf-cache'))
import mlx.core as mx
import numpy as np
import soundfile as sf
import imageio_ffmpeg
from mlx_audio.tts.utils import load_model
models=root.parent/'.tools/skybreak-voice/models'
work=root/'Build/VoiceWork1.7';work.mkdir(parents=True,exist_ok=True)
refs=root/'Tools/VoiceReferences1.7';refs.mkdir(exist_ok=True)
bank=json.loads((root/'Assets/Resources/Voices/voice-bank.json').read_text())['clips']
ff=imageio_ffmpeg.get_ffmpeg_exe()
roles=[
 'Adult woman, 24. Clear, bright upper-mid voice. Warm decisive squad leader, energetic and sincere. Crisp articulation, varied emphasis, real urgency in action, warm relief afterwards.',
 'Adult woman, 27. Warm lower-mid voice, smooth slightly smoky timbre, relaxed attractive confidence and playful smile. Unhurried conversational rhythm, powerful decisive projection in danger, no whispering.',
 'Adult woman, 25. Cool clear light female voice with a focused resonant core, thoughtful reserved navigator. Precise without robotic syllables, subtle hesitation turning into genuine hope and relief.',
 'Adult man, 43. Deep rough resonant naval commander. Proud veteran with clipped authoritative phrasing and chest projection, a little gravel. Arrogant calm shattered into forceful alarm under pressure.',
 'Adult woman, 31. Silvery mezzo voice with a teasing smile, aristocratic enemy ace. Amused taunting entrance, sharp angry defense, real loss of composure and breathless fear in defeat.',
 'Adult man, 35. Low-mid velvety controlled voice. Austere orbital commander, quiet authority, careful measured words, inner conflict that becomes restrained regret. No robotic voice effect.',
]
reference_text={
 'en':[
  "Watch your right! A bomber's coming in. Stop it! Yes, got it! Whew... the convoy's safe. Stay with me. We're going home together.",
  "Tough little thing, isn't it? Stand back. Let me handle this! Full firepower! Ha, there we go. Easy now. I don't want those lovely wings getting scratched.",
  "Target locked. Move sideways, now! Time... stand still. Wait, is it connected? It recognizes us! I knew you remembered. We can finally go home.",
 ],
 'ja':[
  '右に注意！ 爆撃機が来る、止めて！ よし、当たった！ ふう、船団は無事。私の声、聞こえる？ 一緒に帰ろうね。',
  'あら、なかなか硬いじゃない。下がって、私に任せて！ 火力全開！ はっ、片づいた。無理しないでね。翼に傷がついたら、私が悲しいわ。',
  '目標ロック。今、横に避けて！ 時間よ、止まって。あ、つながった？ 私たちを認識してる！ 覚えていてくれたんですね。やっと、帰れる。',
 ]}
languages={'zh':'Chinese','en':'English','ja':'Japanese'}
records_path=work/'generation.json';records=json.loads(records_path.read_text()) if records_path.exists() else {}
def key(c,lang):return lang+'/'+c['id']
def dest(c,lang):return root/'Assets/Resources/Voices/Clips'/('' if lang=='zh' else lang)/(c['id']+'.wav')
def fingerprint(c,lang):return hashlib.sha256(((c['text'] if lang=='zh' else c[lang])+((' / kana-v2 / authored-readings' if any(w in c['ja'] for w in JAPANESE_READINGS) else ' / kana-v2') if lang=='ja' else '')).encode()).hexdigest()
def done(c,lang):
 p=dest(c,lang);r=records.get(key(c,lang),{})
 return p.exists() and r.get('text_hash')==fingerprint(c,lang) and r.get('sha256')==hashlib.sha256(p.read_bytes()).hexdigest()
def save(c,lang,wave,sr):
 size=int(sr*.02);frames=np.pad(wave,(0,(-len(wave))%size)).reshape(-1,size)
 rms=np.sqrt((frames**2).mean(1));active=np.flatnonzero(rms>max(.0015,float(rms.max())*.015))
 if not len(active):raise RuntimeError('Silent '+key(c,lang))
 begin=max(0,int(active[0]*size-sr*.08));end=min(len(wave),int((active[-1]+1)*size+sr*.18));wave=wave[begin:end]
 seconds=len(wave)/sr
 if not .3<=seconds<=24:raise RuntimeError('Duration '+key(c,lang)+' '+str(seconds))
 temp=work/'trim.wav';sf.write(temp,wave,sr,subtype='PCM_16');out=dest(c,lang);out.parent.mkdir(exist_ok=True)
 subprocess.run([ff,'-hide_banner','-loglevel','error','-y','-i',str(temp),'-af','highpass=f=65,loudnorm=I=-18:TP=-2:LRA=11','-ar','24000','-ac','1','-c:a','pcm_s16le',str(out)],check=True)
 records[key(c,lang)]={'seconds':seconds,'text_hash':fingerprint(c,lang),'sha256':hashlib.sha256(out.read_bytes()).hexdigest(),'original_synthetic_voice':True}
 records_path.write_text(json.dumps(records,indent=2)+'\n');print('DONE',key(c,lang),round(seconds,2),flush=True)
reference_text['ja']=[japanese_reading(t) for t in reference_text['ja']]
model=load_model(str(models/'Qwen3-TTS-12Hz-1.7B-VoiceDesign-bf16'))
for lang in ('en','ja'):
 for pilot in range(3):
  path=refs/(lang+'-'+str(pilot)+('-v2' if lang=='ja' else '')+'.wav')
  if path.exists():continue
  mx.random.seed(17000+pilot*177+(0 if lang=='en' else 1234))
  direction=roles[pilot]+' Native fluent '+languages[lang]+'. Professional original game character, close clean studio voice, no music. Speak to a trusted teammate, with changing emotional beats, never flat narration.'
  print('DESIGN REFERENCE',lang,pilot,flush=True)
  result=list(model.generate_voice_design(text=reference_text[lang][pilot],instruct=direction,language=languages[lang],temperature=.72,top_p=.95,max_tokens=650))
  sf.write(path,np.concatenate([np.array(r.audio).reshape(-1) for r in result]),model.sample_rate,subtype='PCM_16');mx.clear_cache()
emotions={'entry':'Entrance transmission. '+ 'Use the character personality: confident taunt for naval commander, lightly laughing mocking smile for female ace, solemn firm readiness for orbital commander.',
 'phase':'Their defenses are failing. Angry strained command for the naval captain; sharp irritated disbelief for the ace; troubled questioning for the orbital commander.',
 'critical':'Near defeat. Naval captain urgently shouts orders with panic breaking through. Female ace frightened breathless and urgent, losing control. Orbital commander quietly shaken with sorrowful realization.',
 'death':'Final radio line in an exploding cockpit. Naval captain forcefully shouting denial, finish with defiance. Female ace cries out in genuine panic and ends in a short frightened scream. Orbital commander soft weary regret, a final goodbye followed by silence.'}
for lang in languages:
 for c in bank:
  if c['category']!='boss' or done(c,lang):continue
  event=c['id'].split('_')[-1];pilot=c['pilot'];mx.random.seed(27000+pilot*177+list(languages).index(lang)*1000)
  direction=roles[pilot]+' Native fluent '+languages[lang]+'. '+('日本語の母語話者。日本のアニメゲームの自然な演技。ひらがなの文章は正確な読みの指定です。言葉を足さず、省略せず、はっきりと発音する。 ' if lang=='ja' else '')+emotions[event]+' Only perform the supplied words. Original fictional game character. Dry clean voice only, no sound effects or music.'
  print('PERFORM',key(c,lang),flush=True)
  result=list(model.generate_voice_design(text=c['text'] if lang=='zh' else japanese_reading(c[lang]) if lang=='ja' else c[lang],instruct=direction,language=languages[lang],temperature=.72,top_p=.95,max_tokens=430))
  save(c,lang,np.concatenate([np.array(r.audio).reshape(-1) for r in result]),model.sample_rate);mx.clear_cache()
del model;mx.clear_cache()
model=load_model(str(models/'Qwen3-TTS-12Hz-1.7B-Base-bf16'))
for lang in ('en','ja'):
 for pilot in range(3):
  entries=[c for c in bank if c['pilot']==pilot and not done(c,lang)]
  for offset in range(0,len(entries),4):
   group=entries[offset:offset+4];mx.random.seed(1707+offset+pilot*1000);start=time.monotonic()
   print('GROUP',lang,pilot,[c['id'] for c in group],flush=True)
   results=model.batch_generate(texts=[japanese_reading(c[lang]) if lang=='ja' else c[lang] for c in group],ref_audio=str(refs/(lang+'-'+str(pilot)+('-v2' if lang=='ja' else '')+'.wav')),ref_text=reference_text[lang][pilot],lang_code=languages[lang],temperature=.7,top_p=.95,max_tokens=440,stream=False)
   got=set()
   for result in results:
    i=result.sequence_idx;got.add(i);save(group[i],lang,np.array(result.audio).reshape(-1),model.sample_rate)
   assert got==set(range(len(group)))
   print('GROUP_SECONDS',round(time.monotonic()-start,1),flush=True);mx.clear_cache()
assert all(done(c,l) for c in bank for l in ('en','ja'))
assert all(done(c,'zh') for c in bank if c['category']=='boss')
print('TRILINGUAL_RENDER_COMPLETE',len(records),flush=True)
