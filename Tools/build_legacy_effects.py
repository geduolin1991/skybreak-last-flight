"""Original melodic electro-rock score, sample-aligned intensity stems, layered SFX."""
import numpy as np, wave, os
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Assets/Resources/Audio';OUT.mkdir(parents=True,exist_ok=True)
SR=44100;rng=np.random.default_rng(90626)
def timeline(d):return np.arange(int(SR*d),dtype=np.float32)/SR
def hz(n):return 440*2**((n-69)/12)
def smooth(x,n):return np.convolve(x,np.ones(n,dtype=np.float32)/n,mode='same')
def env(t,d,attack=.006,release=.05):return np.minimum(1,t/attack)*np.minimum(1,np.maximum(0,d-t)/release)
def write(name,a,target=None,folder=OUT):
 a=np.asarray(a,dtype=np.float32)
 if a.ndim==1:a=np.stack([a,a],axis=1)
 a-=np.mean(a,axis=0)
 if target:a*=target/max(.0001,float(np.sqrt(np.mean(a*a))))
 a=np.tanh(a*1.15)/1.15
 peak=float(np.max(np.abs(a)));a*=min(1,.94/max(.001,peak))
 fade=min(int(SR*.003),len(a)//3);a[:fade]*=np.linspace(0,1,fade)[:,None];a[-fade:]*=np.linspace(1,0,fade)[:,None]
 with wave.open(str(folder/(name+'.wav')),'wb') as f:
  f.setnchannels(2);f.setsampwidth(2);f.setframerate(SR);f.writeframes((a*32767).astype('<i2').tobytes())
 print(name,'seconds',round(len(a)/SR,2),'RMS',round(float(np.sqrt(np.mean(a*a))),3),'peak',round(float(np.max(np.abs(a))),3))
 return a

# Weapon bodies: a low transient, mechanical crack, then a short tuned tail.
for name,start,body,decay in [('ShotPulse',980,130,42),('ShotScatter',560,95,30),('ShotLance',1840,175,37)]:
 t=timeline(.19);noise=rng.normal(0,1,len(t));phase=2*np.pi*(body*t+(start-body)/55*(1-np.exp(-t*55)))
 a=np.sin(phase)*np.exp(-t*decay)*.76+(noise-smooth(noise,7))*.2*np.exp(-t*100)
 a+=np.sin(2*np.pi*body*2*t)*np.exp(-t*46)*.28
 write(name,a*env(t,.19,.0005,.01))
 if name=='ShotPulse':write('Shot',a)
for name,freq in [('ImpactLight',430),('ImpactArmor',205)]:
 t=timeline(.22);a=(rng.normal(0,1,len(t))*.38+np.sin(2*np.pi*freq*t)*.6+np.sin(2*np.pi*freq*2.71*t)*.25)*np.exp(-t*32)
 a+=np.sin(2*np.pi*95*t)*np.exp(-t*24)*.25;write(name,a*env(t,.22,.0004,.015))
for name,d,frequency in [('Explosion',.85,76),('ExplosionHeavy',1.4,52),('BossBreak',2.7,37),('Bomb',2.2,43)]:
 t=timeline(d);noise=rng.normal(0,1,len(t));low=smooth(noise,39)*4;mid=smooth(noise,5)*.4
 a=(low*.8+mid)*np.exp(-t*(3.8/d))+np.sin(2*np.pi*(frequency*t+1.6*(1-np.exp(-t*12))))*np.exp(-t*4/d)*.65
 a+=(noise-smooth(noise,12))*.27*np.exp(-t*72)
 a+=np.sin(2*np.pi*(frequency*2*t))*np.exp(-t*9/d)*.2
 write(name,a*env(t,d,.0006,.09))
t=timeline(.34);write('Hit',(np.sin(2*np.pi*113*t)+.4*rng.normal(0,1,len(t)))*np.exp(-t*15)*.8)
t=timeline(.55);write('Pickup',(np.sin(2*np.pi*880*t)+.4*np.sin(2*np.pi*1320*t))*np.exp(-t*8)*.4)
t=timeline(.4);write('Combo',(np.sin(2*np.pi*(660*t+660*t*t))+.4*np.sin(2*np.pi*1320*t))*np.exp(-t*10)*.4)
t=timeline(.95);write('Warning',(np.sin(2*np.pi*390*t)+.35*np.sin(2*np.pi*780*t))*(np.sin(2*np.pi*4.2*t)>0)*env(t,.95,.02,.08)*.36)
t=timeline(1.65);write('Overdrive',(np.sin(2*np.pi*(110*t+520*t*t))*.45+np.sin(2*np.pi*70*t)*.3)*env(t,1.65,.012,.5))
t=timeline(.075);write('Click',np.sin(2*np.pi*1350*t)*np.exp(-t*65)*.25)
print('Original layered score and impact audio complete')
