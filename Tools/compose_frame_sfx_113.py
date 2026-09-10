"""Short original mechanical deployment and weapon cues, with bounded headroom."""
from pathlib import Path
import numpy as np
import soundfile as sf
ROOT=Path(__file__).resolve().parents[1];RATE=24000
rng=np.random.default_rng(113)
def write(name,wave):
 peak=np.max(np.abs(wave));wave=wave/max(peak,1e-8)*.58
 sf.write(ROOT/'Assets/Resources/Audio'/(name+'.wav'),wave,RATE,subtype='PCM_16')
def tone(freq,t):return np.sin(2*np.pi*freq*t)+.18*np.sin(2*np.pi*freq*2.01*t)
for i,base in enumerate([164.81,82.41,329.63]):
 t=np.arange(int(RATE*.85))/RATE
 envelope=np.minimum(t/.015,1)*np.exp(-t*5)
 w=tone(base,t)*envelope*.45
 for at in [.015,.16,.31]:
  age=np.maximum(0,t-at);gate=t>=at
  w+=gate*(rng.normal(0,.3,len(t))*np.exp(-age*85)+tone(base*3.0,age)*np.exp(-age*45)*.20)
 w+=np.sin(2*np.pi*(base*2*t+180*t*t))*np.sin(np.minimum(t/.55,1)*np.pi)**2*np.exp(-t*2)*.25
 write('FrameDeploy'+str(i),w)
t=np.arange(int(RATE*.32))/RATE
write('FrameSlash',(rng.normal(0,.18,len(t))+np.sin(2*np.pi*(1400*t-1200*t*t))*.3)*np.sin(np.minimum(t/.06,1)*np.pi/2)*np.exp(-t*13))
t=np.arange(int(RATE*.52))/RATE
write('FrameSiege',(np.sin(2*np.pi*(120*t-80*t*t))*.7+rng.normal(0,.25,len(t))*np.exp(-t*12))*np.minimum(t/.004,1)*np.exp(-t*10))
t=np.arange(int(RATE*.44))/RATE
write('FramePrism',(tone(587.33,t)*.35+tone(1174.66,t)*.18+rng.normal(0,.08,len(t))*np.exp(-t*20))*np.minimum(t/.009,1)*np.exp(-t*10))
print('FRAME_SFX_READY: six original mono cues; peak <= -4.7 dBFS')
