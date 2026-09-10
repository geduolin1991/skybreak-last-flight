"""Original short destruction foley; deterministic, no downloaded sound assets."""
from pathlib import Path
import numpy as np
import wave as wavefile
root=Path(__file__).resolve().parents[1];rate=24000;rng=np.random.default_rng(114)
def write(name,wave):
 wave=wave/max(np.max(np.abs(wave)),1e-8)*.65
 with wavefile.open(str(root/'Assets/Resources/Audio'/(name+'.wav')),'wb') as stream:
  stream.setparams((1,2,rate,0,'NONE','not compressed'));stream.writeframes((wave*32767).astype('<i2').tobytes())
t=np.arange(int(rate*1.25))/rate;n=rng.normal(0,1,len(t));low=np.convolve(n,np.ones(22)/22,'same')
water=low*np.exp(-t*3.2)*1.9+n*.065*np.exp(-np.maximum(0,t-.1)*6)*(t>.1)+np.sin(2*np.pi*(105*t-28*t*t))*np.exp(-t*13)*.25
write('CrashWater114',water*np.minimum(t/.004,1))
t=np.arange(int(rate*.86))/rate;n=rng.normal(0,1,len(t));low=np.convolve(n,np.ones(15)/15,'same')
write('CrashGround114',(low*1.3+np.sin(2*np.pi*(95*t-35*t*t))*.5)*np.exp(-t*6)*np.minimum(t/.003,1))
t=np.arange(int(rate*.38))/rate;n=rng.normal(0,1,len(t));write('ArmorBreak114',(n*.3+np.sin(2*np.pi*730*t)*.18+np.sin(2*np.pi*1231*t)*.12)*np.exp(-t*18)*np.minimum(t/.003,1))
print('Three original cues generated; PCM24k mono, peak <= .65')
