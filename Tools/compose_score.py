"""Original SKYBREAK 1.3 score: 8-bar phrases, section changes, authored harmony.
Outputs note events for GeneralUser GS 2.0.3 through SpessaSynth (no borrowed MIDI).
"""
from pathlib import Path
import json,random
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Tools/Score';OUT.mkdir(exist_ok=True)
rng=random.Random(13003)
# D minor melody in question/answer phrases; silence is part of the phrasing.
phrases=[[(0,74,1), (1.5,77,.5),(2,81,1.5)],[(0,79,.75),(1,77,.75),(2.5,76,1.3)],[(0,72,1.5),(2,77,.75),(3,79,.8)],[(0,76,2.5),(3,72,.75)],[(0,74,.75),(1,77,.75),(2,79,.5),(2.75,81,1)],[(0,84,1.5),(2,81,.75),(3,79,.75)],[(0,77,1),(1.5,76,.5),(2.5,74,1.25)],[(0,73,1.5),(2,76,.65),(3,81,.75)]]
answer=[[(0,81,1.5),(2,86,1.5)],[(0,84,1.5),(2,81,.75),(3,79,.75)],[(0,81,.75),(1,79,.75),(2,77,1.5)],[(0,79,2.75)],[(0,79,.5),(1,81,.5),(2,82,1.5)],[(0,81,2),(2.5,77,.75)],[(0,79,1),(1.5,77,.5),(2.5,76,1)],[(0,73,1.5),(2,76,.75),(3,73,.75)]]
# voiced Dm9, Bbmaj7, Fadd9, C, Gm7, Dm/F, Bb, A7.
harmony=[(38,[53,57,60,64]),(34,[53,57,62,65]),(41,[53,57,60,67]),(36,[52,55,60,62]),(43,[53,58,62,65]),(41,[53,57,62,69]),(34,[53,58,62,65]),(33,[52,55,61,67])]
names=['MusicSea','MusicCity','MusicOrbit','MusicHangar'];manifest=[]
for stage,name in enumerate(names):
 calm=stage==3;bpm=[148,158,140,88][stage];beat=60/bpm;bars=64 if not calm else 32;transpose=[0,-5,3,0][stage]
 # Program, channel volume, pan, reverb. True sampled piano, guitars, bass, drums.
 channels=[[0,77,54,45],[48,60,80,57],[33,97,64,12],[27,67,26,24],[30,58,96,20],[60,58,69,42],[11,62,90,48],[73,61,46,48],[50,43,64,62],[0,90,64,18]]
 base=[];drive=[]
 def note(bus,ch,pitch,t,d,vel):
  offset=(rng.uniform(-.007,.007) if ch in [0,3,4,9] else 0)
  start=max(0,(t*beat)+offset);pitch=pitch if ch==9 else pitch+transpose
  v=max(1,min(127,int(vel+rng.uniform(-4,4))))
  bus.extend([[round(start,6),'on',ch,pitch,v],[round(start+d*beat,6),'off',ch,pitch,0]])
 for bar in range(bars):
  root,chord=harmony[bar%8];t=bar*4;section=bar//8;chorus=section in [2,3,6,7];bridge=section==4;build=section==5
  # Warm legato harmony with restrained expression and alternating voicings.
  if True:
   for p in chord:note(base,1,p,t,3.8,42 if calm else 47 if chorus else 36)
   if stage==2:
    for p in chord[1:]:note(base,8,p+12,t,3.9,34)
  for k in ([0,2] if calm else [0,.75,1.5,2,2.75,3.5]):
   note(base,2,root+(12 if k==3.5 else 0),t+k,1.7 if calm else .42,62 if calm else 82)
  # Flowing piano broken chords, octave-separated from melody.
  if calm or bridge or section in [0,1,5]:
   for k in range(8):note(base,0,chord[[0,2,1,3,2,1,3,2][k]],t+k*.5,.85,47+(9 if k%4==0 else 0))
  elif bar%2==0:
   for p in chord:note(base,0,p+12,t,2.6,61)
  if not calm:
   rhythm=[0,.75,1.5,2.5,3.25] if stage==1 else [0,1.5,2,3.5]
   for k in rhythm:
    for p in [root+12,root+19]:note(base,3,p,t+k,.35,63 if chorus else 52)
   if not bridge:
    for k in ([0,1.5,2,2.75] if stage==1 else [0,2,2.5]):note(base,9,36,t+k,.12,99 if chorus else 87)
    for k in [1,3]:note(base,9,38 if stage!=2 else 40,t+k,.15,95 if chorus else 79)
    for k in range(8):note(base,9,42,t+k*.5,.13,48 if k%2 else 66)
    if chorus:
     for k in [1.5,3.5]:note(base,9,46,t+k,.23,57)
   if bar%8==0:note(base,9,49,t,2,65 if section else 45)
   if bar%8==7:
    for k,p in enumerate([45,47,43,41]):note(base,9,p,t+3+k*.25,.22,66+k*4)
   if build:
    for k in range(4):note(base,9,38,t+2+k*.5,.13,48+k*5)
  elif section>0:
   for k in [0,2]:note(base,9,36,t+k,.14,45)
   for k in [1,3]:note(base,9,37,t+k,.1,51)
   for k in range(4):note(base,9,42,t+k,.1,36)
  # Motif appears as lyrical piano, guitar/horn reply, with a quiet middle section.
  if calm or section in [1,2,3,6,7]:
   phrase=(answer if chorus else phrases)[bar%8]
   lead=0 if calm else (5 if stage==2 else 4 if chorus else 0)
   for k,p,d in phrase:
    note(base,lead,p,t+k,d*.96,76 if chorus else 68)
    if calm:note(base,6,p-12,t+k+.03,d,31)
    if chorus and bar%8>=4:note(base,1,p-12,t+k,d*.92,43)
  if bridge:
   for k,p,d in phrases[bar%8]:note(base,7,p,t+k,d,51)
  # Additional battle stem is supportive: no competing lead melody.
  if not calm:
   for k in [0,.75,1.5,2,2.75,3.5]:
    for p in [root+12,root+19]:note(drive,4,p,t+k,.35,65)
   for k in [0,2.5]:note(drive,9,36,t+k,.13,85)
   for k in [1,3]:note(drive,9,38,t+k,.15,74)
   for k in range(8):note(drive,9,51,t+k*.5,.2,52+(8 if k%2==0 else 0))
   if bar%4==0:
    for p in chord:note(drive,5,p,t,2.5,53)
   if bar%8==7:
    for k in range(4):note(drive,9,45-k,t+3+k*.25,.2,74)
 for suffix,events in [('',base)]+([] if calm else [('Intensity',drive)]):
  data=dict(name=name+suffix,bpm=bpm,bars=bars,seconds=bars*4*beat,channels=channels,events=sorted(events),soundbank='GeneralUser GS 2.0.3')
  (OUT/(name+suffix+'.json')).write_text(json.dumps(data,separators=(',',':')));manifest.append({k:data[k] for k in ['name','bpm','bars','seconds']})
(OUT/'manifest.json').write_text(json.dumps(manifest,indent=2));print(manifest)
