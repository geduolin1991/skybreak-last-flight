"""Five original campaign cues: separate boss identities and resolved endings.
Authored note events rendered with the existing licensed GeneralUser soundbank.
"""
from pathlib import Path
import random,json
ROOT=Path(__file__).resolve().parents[1];out=ROOT/'Tools/Score';rng=random.Random(112041)
manifest=[]
channels=[[0,78,56,43],[48,67,79,54],[33,85,64,14],[29,63,26,22],[30,64,99,21],[60,74,61,43],[11,54,83,48],[73,61,44,54],[52,44,66,61],[0,82,64,18]]
motif=[[(0,74,1.2),(1.5,77,.45),(2.2,81,1.35)],[(0,79,.7),(1.1,77,.7),(2.6,76,1.1)],[(0,77,1.3),(1.8,79,.5),(2.6,84,1)],[(0,81,2.3),(3,79,.65)],[(0,77,.6),(1,74,1.2),(2.7,72,.8)],[(0,76,1.8),(2.4,77,.5),(3.1,79,.7)],[(0,81,1.7),(2.2,77,1.2)],[(0,76,1.4),(2,73,1.65)]]
minor=[(38,[50,57,60,65]),(34,[53,58,62,65]),(41,[53,57,60,67]),(33,[52,57,61,67])]
major=[(38,[54,57,61,66]),(35,[54,59,62,66]),(43,[55,59,62,66]),(33,[52,57,61,64])]
for index,(name,bpm,bars) in enumerate([('MusicLeviathan',132,32),('MusicTempest',156,32),('MusicSeraph',128,32),('MusicDawn',88,24),('MusicAftermath',84,24)]):
 events=[];beat=60/bpm;calm=index>=3
 def note(ch,pitch,t,d,vel):
  t=max(0,t*beat+(rng.uniform(-.006,.006) if ch in [0,3,4,9] else 0));v=max(1,min(120,int(vel+rng.uniform(-3,3))))
  events.extend([[round(t,6),'on',ch,pitch,v],[round(t+d*beat,6),'off',ch,pitch,0]])
 for bar in range(bars):
  t=bar*4;phrase=bar%8;section=bar//8;root,chord=(major if index==3 else minor)[(bar//2)%4]
  lift=section in [1,3];space=section==2;transpose=2 if index==1 else 0
  root+=transpose;chord=[p+transpose for p in chord]
  for p in chord:note(1,p,t,3.88,35 if calm else 48 if lift else 38)
  if index==2:
   for p in chord[1:]:note(8,p+12,t,3.92,37 if lift else 28)
  bass=[0,2] if calm else [0,1.5,2,3.25] if index!=1 else [0,.75,1.5,2.5,3.5]
  for offset in bass:note(2,root,t+offset,1.6 if calm else .38,55 if calm else 85)
  if calm or space:
   for k in range(8):note(0,chord[[0,2,1,3,2,0,1,2][k]],t+k*.5,.95,42+(8 if k%4==0 else 0))
  if not calm:
   for offset in ([0,2/3,4/3,2,8/3,10/3] if index==0 else [0,.75,1.5,2,2.75,3.5]):
    for p in [root+12,root+19]:note(3 if index==0 else 4,p,t+offset,.28,68 if lift else 55)
   if not space:
    for offset in ([0,1.5,2.5] if index==0 else [0,.75,2,3.5] if index==1 else [0,2]):note(9,36,t+offset,.13,98)
    for offset in [1,3]:note(9,38 if index!=2 else 40,t+offset,.18,92 if lift else 76)
    for k in range(8):note(9,42 if index!=2 else 51,t+k*.5,.15,47+(18 if k%2==0 else 0))
   else:
    for offset in [0,2]:note(9,41,t+offset,.2,68)
   if phrase==0:note(9,49,t,2,61)
   if phrase==7:
    for k,p in enumerate([45,47,43,41]):note(9,p,t+3+k*.25,.22,64+k*6)
  elif section>0:
   note(9,36,t,.14,38);note(9,37,t+2,.13,34)
  if bar>=4:
   lead=0 if calm else 5 if index in [0,2] and lift else 4 if index==1 else 7
   for offset,pitch,duration in motif[phrase]:
    pitch+=transpose
    if index==3 and pitch%12 in [0,5,10]:pitch+=1
    if index==4 and section==2 and phrase>=6:pitch=74 if phrase==6 else 69
    note(lead,pitch,t+offset,duration,67 if calm else 82 if lift else 67)
    if calm:note(6,pitch-12,t+offset+.04,duration,28)
    if index==2 and lift:note(8,pitch-12,t+offset,duration,35)
  if lift and not calm and bar%2==1:
   for k in range(4):note(0,chord[3-k]+12,t+k*.75,.50,48)
 data=dict(name=name,bpm=bpm,bars=bars,seconds=bars*4*beat,channels=channels,events=sorted(events),soundbank='GeneralUser GS 2.0.3',authorship='Original SKYBREAK campaign cue, 1.12.0')
 (out/(name+'.json')).write_text(json.dumps(data,separators=(',',':'))+'\n');manifest.append({k:data[k] for k in ['name','bpm','bars','seconds']})
(out/'campaign-manifest-112.json').write_text(json.dumps(manifest,indent=2)+'\n');print(json.dumps(manifest))
