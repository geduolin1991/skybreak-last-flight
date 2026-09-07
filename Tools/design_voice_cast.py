"""Design original reference voices from character direction, without real-person recordings."""
import argparse,json,os
from pathlib import Path
root=Path(__file__).resolve().parents[1]
p=argparse.ArgumentParser();p.add_argument('--model',required=True);args=p.parse_args()
os.environ.setdefault('HF_HOME',str(root.parent/'.tools/skybreak-voice/hf-cache'))
import mlx.core as mx
import numpy as np
import soundfile as sf
from mlx_audio.tts.utils import load_model
roles=json.loads((root/'Tools/voice_cast.json').read_text())
out=root/'Build/VoiceWork/Auditions';out.mkdir(parents=True,exist_ok=True)
model=load_model(args.model)
for role in roles:
    mx.random.seed(role['seed'])
    chunks=list(model.generate_voice_design(text=role['text'],instruct=role['direction'],language='Chinese',temperature=.75,top_p=.95,max_tokens=480))
    wave=np.concatenate([np.array(chunk.audio).reshape(-1) for chunk in chunks])
    sf.write(out/(role['id']+'-reference.wav'),wave,model.sample_rate,subtype='PCM_16')
    print(role['id'],len(wave)/model.sample_rate,flush=True);mx.clear_cache()
print('Auditions saved. The approved references in Tools/VoiceReferences have not been replaced.')
