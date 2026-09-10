"""Original ground arsenal and orbital infrastructure, Blender 5.2 native bpy.
Editable source collections retain moving assemblies; Unity imports material batches.
"""
from pathlib import Path
import bpy, math, json
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
exec((ROOT/'Tools/build_campaign_world_112.py').read_text().split("begin('HarborSignal112')")[0])
extra={'Ground_Sand':(.40,.34,.24),'Ground_Red':(.31,.055,.039),'Ground_Blue':(.05,.22,.32),'Ground_Pearl':(.68,.76,.77),'Ground_Purple':(.32,.27,.44),'Ground_Orange':(.93,.35,.08)}
for name,color in extra.items():
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(*color,1);mats['Campaign_'+name]=m

def shell(name,p,w,d,h,material='Ivory'):
 x,y,z=p;pts=[(-.40,-.5),(.40,-.5),(.5,-.30),(.43,.36),(.25,.5),(-.25,.5),(-.43,.36),(-.5,-.30)];n=len(pts)
 verts=[(x+a*w,y+b*d,z) for a,b in pts]+[(x+a*w*.80,y+b*d*.90,z+h) for a,b in pts]
 faces=[tuple(reversed(range(n))),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new(name,mesh);current.objects.link(o);return finish(o,name,material,.035)
def tube(name,p,r,d,material='Graphite'):
 o=cyl(name,p,r,d,material,16);o.rotation_euler[0]=math.pi/2;return o
def tread(x,y,w,length):
 box('Recessed track drive',(x,y,.38),(w,length,.62),'Graphite',.14)
 for j in range(5):
  for s in [-1,1]:
   o=cyl('Suspension road wheel',(x+s*w*.5,y-length*.36+j*length*.18,.36),.25,.10,'Copper',16);o.rotation_euler[1]=math.pi/2
 for j in range(12):box('Individual tread pad',(x,y-length*.48+j*length/12,.705),(w+.07,.12,.055),'Graphite',.006)
 box('Overhanging fender',(x,y,.77),(w+.16,length*.88,.10),'Ivory')
 for j in [-1,1]:box('Angular mud guard',(x,y+j*length*.44,.50),(w+.08,.11,.51),'Graphite')
def vents(p,w,d,material='Graphite'):
 x,y,z=p;box('Inset service well',(x,y,z),(w,d,.045),'Graphite',.012)
 for j in range(5):box('Cooling louvre',(x,y-d*.38+j*d*.19,z+.035),(w*.87,.045,.028),material,.005)
def tank(v,enemy=False,boss=False):
 name='BastionCrawler115' if boss else 'SiegeCrawler115' if enemy else ['KestrelTank115','MantaTank115','NeedleTank115'][v];begin(name)
 hull='Ground_Red' if enemy or boss else ['Ground_Blue','Ground_Red','Ground_Purple'][v]
 w=[1.60,2.12,1.28][v];length=[2.50,3.05,3.2][v]
 shell('Cast lower hull',(0,0,.31),w,length,.53,hull);shell('Sloped glacis',(0,.2,.83),w*.92,length*.77,.30,'Ivory')
 for s in [-1,1]:
  tread(s*(w*.57),0,.48 if v!=1 else .66,length)
  vents((s*.41,-.85,1.11),.44,.62,'Copper');box('Front running lamp',(s*.5,length*.29,1.11),(.18,.10,.085),'Light')
  for j in range(3):box('Reactive cheek armour',(s*(w*.44),-.50+j*.36,1.03),(.16,.26,.15),hull)
  for j in range(2):tube('Engine exhaust',(s*.42+j*.1,-length*.50,.68),.08,.20)
  beam('Radio aerial',(s*.57,-.65,1.12),(s*.57,-.77,1.72),.018)
 before=set(current.objects)
 cyl('Turret race',(0,.18,1.19),w*.30,.18,'Copper',32);shell('Faceted turret',(0,.06,1.29),w*.86,1.36,.36,hull)
 cyl('Commander hatch',(-.24,-.2,1.72),.18,.06,'Graphite',20);box('Gunner periscope',(.24,.02,1.77),(.18,.28,.10),'Glass')
 barrels=2 if v!=2 else 1
 for j in range(barrels):
  x=(j-(barrels-1)*.5)*(.50 if v==1 else .32);d=2.25 if v==2 else 1.65 if v==1 else 1.30
  tube('Recoil sleeve',(x,.89,1.45),.15 if v==1 else .10,.66,'Graphite');tube('Rifled long barrel',(x,1.2+d*.2,1.45),.075 if v!=1 else .11,d,'Copper')
  tube('Muzzle brake',(x,1.2+d*.7,1.45),.13,.22,hull);tube('Bored dark muzzle',(x,1.32+d*.7,1.45),.085,.015,'Graphite')
  if v==2:
   for side in [-1,1]:box('Split accelerator rail',(side*.13,1.85,1.46),(.075,1.32,.18),'Ivory');box('Rail inlay',(side*.171,1.85,1.47),(.02,1.18,.06),'Light')
 if v==1:
  for s in [-1,1]:
   box('Armoured missile pod',(s*.90,.02,1.54),(.36,.85,.42),hull)
   for dx in [-.08,.08]:tube('Guided rocket aperture',(s*.90+dx,.47,1.57),.057,.025,'Graphite')
 if v==0:
  for s in [-1,1]:shell('Swept stabilizer',(s*.60,-.58,1.54),.3,.85,.10,'Ivory')
 pivot('Motion_Turret',(0,.18,1.2),set(current.objects)-before)
 if boss:
  for s in [-1,1]:
   tread(s*2.05,-.2,.7,3.65)
   tube('Siege support gun',(s*1.25,1.05,1.95),.22,2.15,'Copper');tube('Support muzzle',(s*1.25,2.18,1.95),.16,.08,'Graphite')
   box('Side blast deflector',(s*1.91,.1,1.23),(.52,2.45,.5),hull)
  cyl('Exposed thermal core',(0,-.68,1.78),.42,.38,'Orange' if False else 'Ground_Orange',24)
 save(name)

for i in range(3):tank(i)
tank(1,enemy=True);tank(1,boss=True)
begin('SiegeWalker115')
shell('Walker reactor carapace',(0,0,1.10),1.65,1.80,.57,'Ground_Red');vents((0,-.3,1.72),.92,.65,'Copper')
for s in [-1,1]:
 for sy in [-1,1]:
  knee=(s*1.05,sy*.70,.68);beam('Exposed hydraulic femur',(s*.58,sy*.5,1.37),knee,.14,'Copper');beam('Armoured tibia',knee,(s*1.23,sy*.99,.17),.13,'Ivory');shell('Stabilizer foot',(s*1.23,sy*.99,.04),.42,.60,.13,'Graphite')
 tube('Twin pulse emitter',(s*.52,.72,1.55),.13,1.45,'Graphite');tube('Emitter tip',(s*.52,1.46,1.55),.08,.04,'Ground_Orange')
save('SiegeWalker115')
begin('MortarBattery115')
shell('Mortar foundation',(0,0,.02),2.0,2.0,.33,'Graphite');cyl('Traverse housing',(0,0,.58),.75,.58,'Ground_Red',24)
for s in [-1,1]:
 shell('Recoil anchor',(s*.75,0,.0),.38,1.65,.3,'Ivory');o=tube('Elevated mortar tube',(s*.22,.22,1.05),.2,1.2,'Copper');o.rotation_euler[0]=.7
 cyl('Rangefinder lens',(s*.71,.0,.85),.12,.06,'Ground_Orange',12)
save('MortarBattery115')
begin('GroundBarrier115')
shell('Sloped ceramic barricade',(0,0,0),3.2,1.0,.82,'Sand' if False else 'Ground_Sand');box('Steel impact face',(0,-.33,.62),(2.68,.09,.28),'Graphite')
for s in [-1,1]:box('Tie-down shoe',(s*1.25,0,.11),(.28,1.04,.19),'Copper');box('Warning reflector',(s*1.03,-.391,.66),(.17,.035,.12),'Ground_Orange')
save('GroundBarrier115')
begin('GroundRelay115')
cyl('Octagonal relay foundation',(0,0,.16),.83,.32,'Graphite',8);cyl('Shield coil',(0,0,.79),.43,1.08,'Glass',24)
for z in [.35,.65,.95,1.28]:ring('Induction collar',(0,0,z),.46,.065,'Copper')
for s in [-1,1]:box('Armoured relay pillar',(s*.55,0,.84),(.18,.45,1.39),'Ivory');box('Status slit',(s*.55,-.25,.93),(.055,.04,.70),'Light')
objects=[]
for s in [-1,1]:objects.append(box('Signal rotor',(s*.36,0,1.77),(.6,.16,.12),'Ivory'))
pivot('Motion_Rotor_Relay',(0,0,1.77),objects);save('GroundRelay115')
begin('GroundIndustry115')
shell('Industrial foundation',(0,0,0),6,5.3,.36,'Ground_Sand');shell('Reinforced turbine hall',(0,0,.30),4.8,3.5,1.6,'Ivory');panel_roof(0,0,2.0,4.3,3.0)
for s in [-1,1]:
 for j in range(4):box('Clerestory window',(s*2.34,-1.0+j*.66,1.67),(.04,.39,.37),'Glass')
 beam('Cooling pipe',(s*2.40,-1.5,.55),(s*2.40,1.4,.55),.13,'Copper')
 cyl('Auxiliary storage drum',(s*1.75,-2.25,.93),.44,1.30,'Graphite',24);ring('Drum clamping strap',(s*1.75,-2.25,1.28),.45,.035,'Copper')
box('Loading door',(0,-1.76,.89),(2.06,.06,1.15),'Graphite')
for j in range(6):box('Shutter slat',(0,-1.80,.40+j*.18),(1.98,.025,.045),'Copper',.003)
for s in [-1,1]:beam('Roof handrail',(s*2.13,-1.45,2.44),(s*2.13,1.45,2.44),.025,'Graphite')
save('GroundIndustry115')

def solar(x,y,z,w,d):
 box('Radiator perimeter frame',(x,y,z),(w,d,.11),'Graphite')
 for j in range(3):
  for k in range(5):box('Photovoltaic tile',(x-w*.31+j*w*.31,y-d*.40+k*d*.20,z+.066),(w*.28,d*.18,.022),'Solar',.002)
 for s in [-1,1]:beam('Radiator support',(x-w*.43*s,y-d*.40,z-.08),(x+w*.43*s,y+d*.40,z-.08),.035,'Copper')
for v,name in enumerate(['OrbitalHabitat115','OrbitalRefinery115']):
 begin(name);beam('Central service spine',(0,-5,0),(0,5,0),.32,'Graphite')
 for sy in [-1,1]:
  y=sy*2.4
  if v==0:
   o=tube('Pressure hull',(0,y,.20),1.0,2.9,'Ivory')
   for d in [-1.20,0,1.20]:
    o=ring('Hull segment collar',(0,y+d,.2),1.025,.095,'Copper');o.rotation_euler[0]=math.pi/2
   for s in [-1,1]:
    for j in range(5):box('Recessed viewport strip',(s*.84,y-1+j*.49,.67),(.18,.26,.12),'Glass')
   box('Equipment pallet',(0,y,1.22),(1.05,1.48,.17),'Graphite');vents((0,y,1.32),.88,1.1,'Copper')
  else:
   cyl('Refinery thermal drum',(0,y,.23),1.38,1.6,'Ivory',40);cyl('Dark sunken cap',(0,y,1.05),1.16,.05,'Graphite',32)
   for z in [-.48,.13,.89]:ring('Segment seam',(0,y,z),1.4,.06,'Copper')
   for a in range(0,360,45):
    c,s=math.cos(math.radians(a)),math.sin(math.radians(a));beam('External coolant riser',(c*1.43,y+s*1.43,-.4),(c*1.43,y+s*1.43,.95),.05,'Graphite')
  for s in [-1,1]:
   beam('Deployable radiator boom',(s*.8,y,0),(s*3.3,y,0),.12,'Ivory');solar(s*3.2,y,.16,2.5,3.2)
 cyl('Central docking collar',(0,0,.22),1.22,.30,'Graphite',40);ring('Dock guide rim',(0,0,.4),1.08,.08,'Ivory');cyl('Airlock port',(0,0,.39),.70,.13,'Glass',32)
 for s in [-1,1]:
  beam('Dock approach rail',(s*.46,-.86,.5),(s*.46,-1.54,.5),.025,'Ivory')
  for y in [-.35,.35]:box('Service amber',(s*.95,y,.49),(.09,.17,.06),'Ground_Orange')
 save(name)
# Keep every authored source collection, offset for inspection, without a render job.
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Tools/GroundOrbit1.15.blend'))
(ROOT/'Build/Ground115/asset-catalog.json').write_text(json.dumps(catalog,indent=2)+'\n')
print('GROUND_ORBIT_115_ASSETS_READY',len(catalog))
