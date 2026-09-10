"""Authored cargo cranes, a reactor complex and the final ground commander."""
from pathlib import Path
import bpy,math,json
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
exec((ROOT/'Tools/build_campaign_world_112.py').read_text().split("begin('HarborSignal112')")[0])
begin('GroundCrane115')
for s in [-1,1]:
 box('Gantry rail foundation',(s*2.45,0,.12),(.85,3.7,.24),'Graphite')
 for sy in [-1,1]:
  beam('Braced A-frame',(s*2.45,sy*1.40,.2),(s*2.02,0,3.8),.15,'Ivory')
  beam('Diagonal truss',(s*2.45,sy*1.40,.5),(s*2.02,0,2.5),.065,'Copper')
 box('Hoist rail',(0,s*.34,3.89),(5.6,.16,.21),'Graphite')
 for x in [-2,-1,0,1,2]:beam('Lattice connector',(x-.40,-.32,3.87),(x+.40,.32,3.87),.05,'Copper')
box('Hoist service cage',(.7,0,3.95),(1.0,.95,.58),'Ivory')
objects=[]
for s in [-1,1]:objects.append(beam('Hanging hoist cable',(.7+s*.27,0,3.68),(.7+s*.27,0,1.64),.021,'Graphite'))
objects.append(box('Hook spreader',(.7,0,1.6),(.90,.36,.14),'Copper'))
pivot('Motion_Hoist',(.7,0,3.70),objects)
box('Operators cabin',(-1.61,.30,3.35),(.83,1.02,.65),'Ivory');box('Cabin glass',(-1.60,-.22,3.39),(.65,.025,.34),'Glass')
save('GroundCrane115')
begin('GroundReactor115')
box('Reactor plinth',(0,0,.20),(5.2,5.2,.4),'Graphite');cyl('Reactor pressure vessel',(0,0,2.1),1.67,3.9,'Ivory',48)
for z in [.43,1.13,2.0,3.1,4.02]:ring('Pressure vessel course',(0,0,z),1.7,.085,'Copper')
cyl('Upper thermal dome',(0,0,4.12),1.48,.18,'Graphite',48)
fan=[]
for i in range(8):
 a=i*math.tau/8;o=box('Cooling fan blade',(math.cos(a)*.85,math.sin(a)*.85,4.27),(.72,.18,.08),'Copper');o.rotation_euler[2]=a+.42;fan.append(o)
pivot('Motion_Rotor_Fan',(0,0,4.27),fan)
for i in range(8):
 a=i*math.tau/8;c,s=math.cos(a),math.sin(a);beam('External service ladder',(c*1.78,s*1.78,.3),(c*1.78,s*1.78,3.75),.047,'Graphite');box('Flow sensor',(c*1.8,s*1.8,2.4),(.18,.17,.2),'Light')
for s in [-1,1]:
 beam('Feed main',(s*1.5,0,.9),(s*3.3,0,.9),.2,'Copper');box('Valve station',(s*2.53,0,.97),(.72,1.3,.83),'Graphite');cyl('Valve wheel',(s*2.53,0,1.5),.28,.08,'Copper',16)
save('GroundReactor115')
begin('FurnaceWalker115')
cyl('Hexagonal armored pelvis',(0,0,.86),1.25,.72,'Graphite',12);cyl('Ring reactor casing',(0,0,1.57),1.55,.65,'Ivory',24);cyl('Open furnace well',(0,0,1.94),1.2,.08,'Graphite',32)
for r in [.62,1.06]:ring('Induction loop',(0,0,2.04),r,.08,'Copper')
cyl('Exposed command kernel',(0,0,2.12),.46,.38,'Light',20)
for s in [-1,1]:
 for y in [-1.22,0,1.22]:
  beam('Hydraulic shoulder',(s*.95,y*.68,1.40),(s*2.07,y,1.06),.19,'Copper');beam('Segmented walking strut',(s*2.07,y,1.06),(s*2.44,y*1.2,.18),.16,'Ivory');box('Stabilized claw foot',(s*2.44,y*1.2,.09),(.65,.82,.18),'Graphite')
 for y in [-1,1]:
  box('Remote cannon housing',(s*1.52,y*.88,1.89),(.48,.72,.4),'Graphite');o=cyl('Long induction barrel',(s*1.52,y*.88+.59,1.89),.12,1.50,'Copper',16);o.rotation_euler[0]=math.pi/2
  o=cyl('Bored gun aperture',(s*1.52,y*.88+1.36,1.89),.095,.04,'Graphite',16);o.rotation_euler[0]=math.pi/2
for i in range(12):
 a=i*math.tau/12;box('Segmented blast tile',(math.cos(a)*1.40,math.sin(a)*1.40,2.02),(.30,.23,.18),'Ivory')
save('FurnaceWalker115')
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Tools/GroundLandmarks1.15.blend'))
(ROOT/'Build/Ground115/landmark-catalog.json').write_text(json.dumps(catalog,indent=2)+'\n');print('GROUND_LANDMARKS_READY')
