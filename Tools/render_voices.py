"""Render original pilot voices locally with Qwen3-TTS; never downloads a real person's voice."""
import argparse,hashlib,json,os,time,subprocess
from pathlib import Path
root=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser();parser.add_argument('--model',required=True);parser.add_argument('--ids',default='');parser.add_argument('--seed-offset',type=int,default=0);args=parser.parse_args()
os.environ.setdefault('HF_HOME',str(root.parent/'.tools/skybreak-voice/hf-cache'))
import mlx.core as mx
import numpy as np
import soundfile as sf
import imageio_ffmpeg
from mlx_audio.tts.utils import load_model
bank=json.loads((root/'Assets/Resources/Voices/voice-bank.json').read_text())['clips'];roles=json.loads((root/'Tools/voice_cast.json').read_text())
raw=root/'Build/VoiceWork/Raw';out=root/'Assets/Resources/Voices/Clips';raw.mkdir(parents=True,exist_ok=True);out.mkdir(parents=True,exist_ok=True)
selected=set(args.ids.split(',')) if args.ids else None
model=load_model(args.model);ff=imageio_ffmpeg.get_ffmpeg_exe();records=[]
for pilot,role in enumerate(roles):
 entries=[c for c in bank if c['pilot']==pilot and (c['id'] in selected if selected else not (out/(c['id']+'.wav')).exists())]
 for offset in range(0,len(entries),4):
  group=entries[offset:offset+4];seed=(int(hashlib.sha256('|'.join(c['id'] for c in group).encode()).hexdigest()[:8],16)+args.seed_offset)%2**32
  mx.random.seed(seed);start=time.monotonic();print('GROUP',pilot,[c['id'] for c in group],flush=True)
  results=model.batch_generate(texts=[c['text'] for c in group],ref_audio=str(root/'Tools/VoiceReferences'/(role['id']+'.wav')),ref_text=role['text'],lang_code='Chinese',temperature=.68,top_p=.95,max_tokens=360,stream=False)
  done=set()
  for result in results:
   index=result.sequence_idx;c=group[index];done.add(index)
   wave=np.array(result.audio).reshape(-1);sr=model.sample_rate
   sf.write(raw/(c['id']+'.wav'),wave,sr,subtype='PCM_16')
   # Trim only the edges; keep breaths and deliberate pauses inside each line.
   size=max(1,int(sr*.02));pad=(-len(wave))%size
   frames=np.pad(wave,(0,pad)).reshape(-1,size);rms=np.sqrt((frames**2).mean(1));active=np.flatnonzero(rms>max(.0015,float(rms.max())*.015))
   if not len(active):raise RuntimeError('Silent voice: '+c['id'])
   begin=max(0,int(active[0]*size-sr*.075));end=min(len(wave),int((active[-1]+1)*size+sr*.16));trim=wave[begin:end]
   if len(trim)/sr>24:raise RuntimeError('Unexpected long take: '+c['id'])
   temp=raw/(c['id']+'-trim.wav');sf.write(temp,trim,sr,subtype='PCM_16')
   subprocess.run([ff,'-hide_banner','-loglevel','error','-y','-i',str(temp),'-af','highpass=f=65,loudnorm=I=-18:TP=-2:LRA=9','-ar','24000','-ac','1','-c:a','pcm_s16le',str(out/(c['id']+'.wav'))],check=True)
   record={'id':c['id'],'seed':seed,'seconds':len(trim)/sr,'source':'original designed reference','sha256':hashlib.sha256((out/(c['id']+'.wav')).read_bytes()).hexdigest()};records.append(record)
   print('DONE',c['id'],round(record['seconds'],2),flush=True)
  assert done==set(range(len(group))), 'Model omitted a sequence'
  print('GROUP_WALL',round(time.monotonic()-start,2),flush=True);mx.clear_cache()
record_path=root/'Build/VoiceWork'/('generation-'+str(args.seed_offset)+'.json');record_path.write_text(json.dumps(records,ensure_ascii=False,indent=2)+'\n')
print('RENDER_COMPLETE',len(records),flush=True)
