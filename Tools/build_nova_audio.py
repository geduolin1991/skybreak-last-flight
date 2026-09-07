from pathlib import Path
import numpy as np,wave
out=Path(__file__).resolve().parents[1]/'Assets/Resources/Audio';sr=44100;rng=np.random.default_rng(1404)
def write(name,a):
 a=np.tanh(a)*.9;st=np.stack([a,np.roll(a,47)],1);st[:220]*=np.linspace(0,1,220)[:,None];st[-220:]*=np.linspace(1,0,220)[:,None]
 with wave.open(str(out/(name+'.wav')),'wb') as f:f.setnchannels(2);f.setsampwidth(2);f.setframerate(sr);f.writeframes((st*32767).astype('<i2').tobytes())
t=np.arange(int(.27*sr))/sr;n=rng.normal(0,1,len(t));write('NovaCharge',(np.sin(2*np.pi*(150*t+1450*t*t))*.5+n*.055)*np.sin(np.pi*t/.27)**.7)
t=np.arange(int(2.7*sr))/sr;n=rng.normal(0,1,len(t));f=np.fft.rfftfreq(len(t),1/sr);low=np.fft.irfft(np.fft.rfft(n)/(1+(f/440)**4)**.5,n=len(t));a=(low*2.7+np.sin(2*np.pi*(42*t+2.2*(1-np.exp(-t*22))))*.75)*np.exp(-t*2.2);a+=n*.22*np.exp(-t*65)
for at,gain in [(.19,.5),(.37,.32),(.69,.2)]:a+=np.roll(low,int(at*sr))*gain*np.exp(-np.maximum(0,t-at)*4)*(t>=at)
write('Bomb',a);print('Nova charge and layered detonation written')
