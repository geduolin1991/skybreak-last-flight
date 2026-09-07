"""Offline musical mix: headroom, subsonic removal, gentle high cut, loop audit."""
from pathlib import Path
import numpy as np,json,wave,shutil

ROOT=Path(__file__).resolve().parents[1];sr=44100;out=ROOT/'Assets/Resources/Audio';preview=ROOT/'Build/AudioPreview';preview.mkdir(exist_ok=True)
def save(p,a):
 with wave.open(str(p),'wb') as f:f.setnchannels(2);f.setsampwidth(2);f.setframerate(sr);f.writeframes(np.round(a*32767).astype('<i2').tobytes())
report=[];tracks={}
for row in json.loads((ROOT/'Tools/Score/manifest.json').read_text()):
 name=row['name'];a=np.fromfile(ROOT/'Build/ScoreRender'/f'{name}.f32',dtype='<f4').reshape(-1,2)
 # Periodic filter initialization: feed the final two seconds first.
 f=np.fft.rfftfreq(len(a),1/sr);gain=(1+(38/np.maximum(f,1))**4)**-.5*(1+(f/11000)**4)**-.5;gain[0]=0;a=np.fft.irfft(np.fft.rfft(a,axis=0)*gain[:,None],n=len(a),axis=0)
 a-=a.mean(axis=0);rms=np.sqrt(np.mean(a*a));target=.12 if name.endswith('Intensity') else .135 if name=='MusicHangar' else .17
 a*=min(16,target/max(rms,.00001));a=np.tanh(a*1.15)/1.15
 # Smooth only the sample-boundary offset, retaining musical tails.
 delta=a[0]-a[-1];fade=192;a[:fade]-=delta[None,:]*np.linspace(1,0,fade)[:,None]
 a*=min(1,.88/np.max(np.abs(a)));save(out/f'{name}.wav',a);tracks[name]=a
 report.append(dict(name=name,seconds=len(a)/sr,rms=float(np.sqrt(np.mean(a*a))),peak=float(np.max(abs(a))),loop_step=float(np.max(abs(a[0]-a[-1])))))
for name,title,start in [('MusicHangar','机库_归航主题_1.3',10),('MusicSea','海岸_破晓航线_1.3',24),('MusicCity','都市_雷暴突围_1.3',26),('MusicOrbit','天环_最后信号_1.3',28)]:
 a=tracks[name].copy()
 if name+'Intensity' in tracks:a+=tracks[name+'Intensity']*.35
 a=a[int(start*sr):int((start+24)*sr)];a*=min(1,.92/np.max(abs(a)));save(preview/f'{title}.wav',a)
# Preserve original environmental SFX generator; rebuild only distinct weapon signatures.
rng=np.random.default_rng(1301)
def tone(d):return np.arange(int(sr*d))/sr
for name,d in [('ShotPulse',.11),('ShotScatter',.32),('ShotLance',.58),('ShotSeeker',.16),('ShotMortar',.4),('ShotRailFast',.24),('MissileBurst',.65)]:
 t=tone(d);noise=rng.normal(0,1,len(t));f=np.fft.rfftfreq(len(noise),1/sr);low=np.fft.irfft(np.fft.rfft(noise)/(1+(f/1300)**4)**.5,n=len(noise))
 if name=='ShotPulse':a=(np.sin(2*np.pi*(180*t+2*(1-np.exp(-t*90))))*.75+low*1.1)*np.exp(-t*48)
 elif name in ['ShotScatter','ShotMortar']:a=(low*2.1+np.sin(2*np.pi*(74*t+1.3*(1-np.exp(-t*32))))*.7)*np.exp(-t*(18 if name=='ShotScatter' else 13))
 elif name in ['ShotLance','ShotRailFast']:
  a=(np.sin(2*np.pi*(690*t+4*(1-np.exp(-t*24))))*.45+np.sin(2*np.pi*92*t)*.7+low*.7)*np.exp(-t*(12 if name=='ShotLance' else 23));a+=noise*.13*np.exp(-t*150)
 elif name=='ShotSeeker':a=np.sin(2*np.pi*(420*t+3*(1-np.exp(-t*27))))*.6*np.exp(-t*28)
 else:a=(low*2.5+np.sin(2*np.pi*57*t)*.65)*np.exp(-t*9)
 a*=np.minimum(1,t/.001)*np.minimum(1,(d-t)/.015);a=np.tanh(a);save(out/f'{name}.wav',np.stack([a,a],1)*.79)
(ROOT/'Build/MCP-1.3/music-mix-report.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
licenses=ROOT/'Docs/Licenses';licenses.mkdir(exist_ok=True);shutil.copy2(ROOT.parent/'.tools/skybreak-audio/GeneralUser-GS-LICENSE.txt',licenses/'GeneralUser-GS-LICENSE.txt')
shutil.copy2(ROOT.parent/'.tools/skybreak-audio/package-lock.json',ROOT/'Tools/Score/renderer-package-lock.json')
