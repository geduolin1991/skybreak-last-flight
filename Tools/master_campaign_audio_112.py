"""Master only the new campaign cues, ambient beds and event signatures."""
from pathlib import Path
import numpy as np,json,wave
ROOT=Path(__file__).resolve().parents[1];out=ROOT/'Assets/Resources/Audio';sr=44100
report=[]
def save(name,a,loop=True):
 a=np.asarray(a,dtype=np.float64)
 if a.ndim==1:a=np.column_stack((a,a))
 a-=a.mean(axis=0);a*=min(1,.84/max(.0001,float(np.max(np.abs(a)))))
 if loop:
  delta=a[0]-a[-1];n=256;a[:n]-=delta[None,:]*np.linspace(1,0,n)[:,None]
 with wave.open(str(out/(name+'.wav')),'wb') as f:f.setnchannels(2);f.setsampwidth(2);f.setframerate(sr);f.writeframes(np.round(np.clip(a,-.95,.95)*32767).astype('<i2').tobytes())
 item=dict(name=name,seconds=len(a)/sr,peak=float(np.max(np.abs(a))),rms=float(np.sqrt(np.mean(a*a))),loop_step=float(np.max(np.abs(a[0]-a[-1]))),loop=loop)
 assert item['peak']<.95 and item['rms']>.005 and (not loop or item['loop_step']<.003),item
 report.append(item)
for item in json.loads((ROOT/'Tools/Score/campaign-manifest-112.json').read_text()):
 name=item['name'];a=np.fromfile(ROOT/'Build/ScoreRender'/(name+'.f32'),dtype='<f4').reshape(-1,2)
 freq=np.fft.rfftfreq(len(a),1/sr);gain=(1+(38/np.maximum(freq,1))**4)**-.5*(1+(freq/11200)**4)**-.5;gain[0]=0
 a=np.fft.irfft(np.fft.rfft(a,axis=0)*gain[:,None],n=len(a),axis=0)
 a*=min(16,(.14 if name in ['MusicDawn','MusicAftermath'] else .17)/max(.001,float(np.sqrt(np.mean(a*a)))))
 a=np.tanh(a*1.13)/1.13;save(name,a)
rng=np.random.default_rng(112199);t=np.arange(sr*12)/sr
for i,name in enumerate(['AmbienceSea','AmbienceCity','AmbienceOrbit','AmbienceHangar']):
 noise=rng.normal(0,1,(len(t),2));f=np.fft.rfftfreq(len(t),1/sr)
 gain=(1+(f/(640 if i==0 else 2200 if i==1 else 140))**4)**-.5
 gain*=1/(1+(60/np.maximum(f,1))**4)**.5
 noise=np.fft.irfft(np.fft.rfft(noise,axis=0)*gain[:,None],n=len(t),axis=0)
 if i<2:a=noise*(.6+.35*np.sin(2*np.pi*t/12-.5)**2)[:,None]
 else:
  drone=.16*np.sin(2*np.pi*55*t)+.055*np.sin(2*np.pi*110*t)+.018*np.sin(2*np.pi*165*t)
  a=noise*.13+drone[:,None]*(.9+.1*np.sin(2*np.pi*t/12))[:,None]
 a*=.10/max(.001,float(np.sqrt(np.mean(a*a))));save(name,a)
t=np.arange(int(sr*1.35))/sr;a=np.zeros_like(t)
for offset,freq in [(0,587.33),(.15,739.99),(.31,880),(.5,1174.66)]:
 u=np.maximum(0,t-offset);a+=np.sin(2*np.pi*freq*u)*np.exp(-u*5.5)*np.minimum(1,u/.008)*(t>=offset)*.23
a*=np.minimum(1,(1.35-t)/.06);save('ObjectiveChime',a,False)
folder=ROOT/'Build/Campaign112';folder.mkdir(exist_ok=True)
(folder/'audio-mastering.json').write_text(json.dumps(dict(pass_all=True,tracks=report),indent=2)+'\n')
print(json.dumps(report,indent=2))
