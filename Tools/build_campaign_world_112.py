"""Blender-authored landmarks for the three-act campaign; separate new assets.
Run in an isolated --background --factory-startup process, never in a user scene.
"""
from pathlib import Path
import bpy,math,json
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Assets/Art/Models';OUT.mkdir(parents=True,exist_ok=True)
scene=bpy.context.scene
for o in list(scene.objects):bpy.data.objects.remove(o,do_unlink=True)
palette={
 'Campaign_Ivory':(.67,.73,.72), 'Campaign_Graphite':(.055,.095,.12),
 'Campaign_Glass':(.075,.23,.30), 'Campaign_Copper':(.46,.24,.12),
 'Campaign_Light':(.3,.76,.82), 'Campaign_Window':(.68,.44,.17),
 'Campaign_Solar':(.035,.11,.23), 'Campaign_Green':(.095,.25,.16)}
mats={};catalog={};current=None
for name,color in palette.items():
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
 bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(*color,1)
 bs.inputs['Roughness'].default_value=.26 if name.endswith('Glass') else .42
 bs.inputs['Metallic'].default_value=.35 if name.endswith(('Graphite','Copper','Solar')) else .07
 mats[name]=m
def begin(name):
 global current
 current=bpy.data.collections.new(name);scene.collection.children.link(current)
def finish(obj,name,mat='Graphite',bevel=.025):
 obj.name=name
 for c in list(obj.users_collection):c.objects.unlink(obj)
 current.objects.link(obj);obj.data.materials.append(mats['Campaign_'+mat])
 if bevel:
  mod=obj.modifiers.new('Machined edge','BEVEL');mod.width=bevel;mod.segments=2
  obj.modifiers.new('Corner normals','WEIGHTED_NORMAL')
 return obj
def box(name,p,s,mat='Graphite',bevel=.025):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.scale=s
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 return finish(o,name,mat,bevel)
def cyl(name,p,r,d,mat='Graphite',n=24):
 bpy.ops.mesh.primitive_cylinder_add(vertices=n,radius=r,depth=d,location=p)
 return finish(bpy.context.object,name,mat,min(.018,r*.07))
def ring(name,p,r,t,mat='Copper'):
 bpy.ops.mesh.primitive_torus_add(major_segments=40,minor_segments=6,major_radius=r,minor_radius=t,location=p)
 return finish(bpy.context.object,name,mat,0)
def beam(name,a,b,r=.04,mat='Graphite'):
 a,b=Vector(a),Vector(b);o=cyl(name,(a+b)*.5,r,(a-b).length,mat,12);o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();return o
def panel_roof(x,y,z,w,d):
 box('Roof shadow reveal',(x,y,z),(w,d,.09),'Graphite')
 box('Metal coping',(x,y,z+.08),(w+.12,d+.12,.1),'Ivory')
 for side in [-1,1]:
  box('Cooling well',(x+side*w*.23,y,z+.2),(.54,1,.14),'Graphite')
  for j in range(6):box('Cooling fins',(x+side*w*.23,y-.4+j*.16,z+.3),(.51,.055,.07),'Copper',.008)
def pivot(name,p,objects):
 root=bpy.data.objects.new(name,None);current.objects.link(root);root.location=p;bpy.context.view_layer.update()
 for o in objects:o.parent=root;o.matrix_parent_inverse=root.matrix_world.inverted()
 return root
def save(name):
 bpy.context.view_layer.update();bpy.ops.object.select_all(action='DESELECT')
 for o in current.all_objects:o.select_set(True)
 bpy.context.view_layer.objects.active=next(o for o in current.objects if o.type=='MESH')
 bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False)
 deps=bpy.context.evaluated_depsgraph_get();catalog[name]={'objects':len(current.all_objects),'vertices':sum(len(o.evaluated_get(deps).data.vertices) for o in current.all_objects if o.type=='MESH'),'animated_pivots':[o.name for o in current.all_objects if o.name.startswith('Motion_')]}
 index=len(catalog)-1
 for o in current.objects:
  if not o.parent:o.location+=Vector(((index%3)*24,(index//3)*30,0))

begin('HarborSignal112')
cyl('Octagonal stone foundation',(0,0,.12),1.65,.24,'Ivory',8)
cyl('Weathered plinth',(0,0,.34),1.21,.25,'Graphite',12)
cyl('Navigation tower',(0,0,1.75),.68,2.65,'Ivory',12)
for z in [.58,1.4,2.15,2.95]:ring('Tower courses',(0,0,z),.71,.045,'Copper')
for a in range(0,360,60):
 x,y=math.cos(math.radians(a)),math.sin(math.radians(a))
 box('Tower slit glazing',(x*.665,y*.665,2),(.14,.08,.72),'Glass')
 beam('Balcony rail post',(x*1.14,y*1.14,2.85),(x*1.14,y*1.14,3.35),.035,'Ivory')
cyl('Observation balcony',(0,0,2.88),1.18,.12,'Graphite')
ring('Balcony handrail',(0,0,3.35),1.15,.04,'Ivory')
cyl('Lantern glass',(0,0,3.32),.60,.65,'Glass')
rotating=[]
for side in [-1,1]:
 rotating.append(box('Fresnel lens',(side*.39,0,3.36),(.17,.67,.32),'Light'))
 rotating.append(box('Lamp hood',(side*.32,0,3.62),(.40,.77,.10),'Graphite'))
pivot('Motion_RotorBeacon',(0,0,3.36),rotating)
cyl('Lantern cap',(0,0,3.74),.82,.13,'Copper')
beam('Lightning rod',(0,0,3.78),(0,0,4.23),.028,'Graphite')
for i in range(4):box('Walkway slab',(0,-1.8-i*.42,.04),(1.05,.32,.08),'Ivory')
save('HarborSignal112')

begin('CivicHospital112')
box('Terraced medical campus',(0,0,-.10),(10.4,12,.3),'Graphite',.12)
for x,y,w,d,h in [(-3,-.8,3.1,7.8,2.2),(2.9,-.8,3.4,8.2,3.1),(0,3.8,3.1,2.7,1.7)]:
 box('Recessed podium',(x,y,.22),(w+.28,d+.28,.4),'Ivory',.07)
 box('Rounded hospital wing',(x,y,h*.5+.3),(w,d,h),'Ivory',.13)
 panel_roof(x,y,h+.36,w,d)
 for side in [-1,1]:
  for z in [.95,1.55,2.15,2.75]:
   if z>h+.16:continue
   for k in range(9):box('Recessed patient window',(x+side*(w*.5+.01),y-d*.42+k*d*.104,z),(.025,d*.062,.30),'Window',.006)
  for k in range(9):box('Structural facade rib',(x+side*(w*.5+.04),y-d*.43+k*d*.107,h*.5+.4),(.075,.045,h-.18),'Graphite',.006)
box('Emergency entry canopy',(0,-2.4,1.1),(2.6,1.6,.15),'Glass',.07)
for side in [-1,1]:beam('Entry canopy strut',(side*1.1,-2.8,.12),(side*1.1,-2.8,1.12),.05,'Ivory')
cyl('Helipad rim',(2.9,.6,3.6),1.28,.07,'Copper',48)
cyl('Helipad surface',(2.9,.6,3.65),1.17,.055,'Graphite',48)
for side in [-1,1]:box('Helipad H upright',(2.9+side*.34,.6,3.692),(.15,1.15,.017),'Ivory',.004)
box('Helipad H crossbar',(2.9,.6,3.694),(.8,.15,.017),'Ivory',.004)
for y in [-4.8,-3.2,-1.6,0,1.6]:
 for x in [-.86,.86]:
  box('Courtyard planter',(x,y,.16),(.48,.75,.28),'Copper')
  cyl('Garden foliage',(x,y,.4),.32,.38,'Green',12)
for side in [-1,1]:
 for k in range(8):box('Rescue approach light',(side*4.93,-5+k*1.45,.14),(.12,.23,.06),'Light',.007)
save('CivicHospital112')

begin('ElevatedRail112')
for side in [-1,1]:
 box('Rail viaduct girder',(side*.97,0,1.25),(.23,12.8,.44),'Ivory',.05)
 box('Rail safety curb',(side*1.12,0,1.55),(.13,12.8,.2),'Graphite',.025)
 beam('Contact rail',(side*.53,-6.35,1.61),(side*.53,6.35,1.61),.04,'Copper')
 for y in [-5,0,5]:
  box('Viaduct pier',(side*.82,y,.49),(.42,.95,1.2),'Ivory',.06)
  beam('Y pier brace',(side*.82,y,.65),(side*.22,y,1.40),.13,'Ivory')
for y in range(-6,7):box('Rail sleepers',(0,y,1.52),(1.75,.15,.1),'Graphite',.016)
box('Platform',(2.05,.8,1.46),(1.6,6,.23),'Ivory',.05)
for k in range(12):box('Tactile platform strip',(1.37,-1.7+k*.46,1.59),(.15,.23,.025),'Copper',.004)
for y in [-1.6,.6,2.8]:
 beam('Canopy supports',(2.38,y,1.58),(2.38,y,2.55),.055,'Graphite')
 beam('Canopy cantilever',(2.38,y,2.5),(1.28,y,2.6),.055,'Graphite')
box('Platform glass canopy',(1.85,.65,2.64),(1.62,5.9,.075),'Glass',.025)
save('ElevatedRail112')

begin('MetroCar112')
box('Rail vehicle underframe',(0,0,.35),(1.14,3.8,.3),'Graphite',.10)
box('Streamlined carriage',(0,0,.86),(1.18,3.8,.98),'Ivory',.20)
box('Continuous roof band',(0,0,1.39),(1.05,3.45,.12),'Graphite',.08)
for side in [-1,1]:
 for y in [-1.4,-.85,-.3,.3,.85,1.4]:box('Carriage glazing',(side*.593,y,1.04),(.03,.37,.39),'Glass',.03)
 box('Route identity stripe',(side*.603,0,.70),(.016,3.30,.08),'Copper',.005)
 for y in [-1.3,1.3]:
  o=cyl('Rail wheel',(side*.49,y,.23),.19,.16,'Graphite',20);o.rotation_euler.y=math.pi/2
 box('Headlamp',(side*.36,1.87,.83),(.18,.04,.12),'Light',.018)
box('Driver windshield',(0,1.88,1.12),(.89,.04,.33),'Glass',.04)
save('MetroCar112')

begin('OrbitalObservatory112')
cyl('Faceted operations hull',(0,0,.42),2.8,.8,'Ivory',12)
ring('Pressure seam',(0,0,.84),2.70,.08,'Graphite')
cyl('Operations glazing',(0,0,.94),2.28,.24,'Glass',12)
cyl('Dish pedestal',(0,0,1.35),.52,.6,'Graphite',12)
moving=[]
for i in range(12):
 a=i*math.pi/6;next_a=a+math.pi/6
 moving.append(beam('Parabolic receiver spoke',(0,0,1.66),(math.cos(a)*2.25,math.sin(a)*2.25,2.48),.055,'Ivory'))
 moving.append(beam('Receiver perimeter',(math.cos(a)*2.25,math.sin(a)*2.25,2.48),(math.cos(next_a)*2.25,math.sin(next_a)*2.25,2.48),.07,'Copper'))
moving.append(ring('Receiver inner mesh',(0,0,2.0),1.06,.10,'Glass'))
moving.append(beam('Receiver focus',(0,0,1.7),(0,0,3.06),.06,'Graphite'))
moving.append(cyl('Receiver focus cap',(0,0,3.1),.15,.15,'Light',16))
pivot('Motion_TrackingDish',(0,0,1.5),moving)
for side in [-1,1]:
 box('Docked utility spine',(side*3.2,0,.35),(1.2,.7,.45),'Graphite',.08)
 for y in [-1.55,0,1.55]:
  box('Thermal radiator',(side*3.5,y,.55),(1.3,1.2,.09),'Solar')
  for k in range(5):box('Radiator veins',(side*3.5,y-.45+k*.23,.61),(1.21,.022,.018),'Copper',.003)
for i in range(12):
 a=i*math.pi/6;cyl('Rim docking latch',(math.cos(a)*2.7,math.sin(a)*2.7,.91),.10,.14,'Copper',8)
save('OrbitalObservatory112')

begin('SolarSail112')
beam('Antenna keel',(0,-5,.18),(0,5,.18),.13,'Ivory')
cyl('Attitude hub',(0,0,.36),.68,.7,'Graphite',12)
ring('Status collar',(0,0,.73),.6,.055,'Light')
for side in [-1,1]:
 moving=[]
 moving.append(beam('Panel hinge',(side*.5,0,.25),(side*5.3,0,.25),.12,'Copper'))
 for x in range(4):
  px=side*(1.35+x*1.02)
  moving.append(box('Solar panel rim',(px,0,.34),(.96,7.7,.11),'Ivory',.025))
  moving.append(box('Photovoltaic field',(px,0,.405),(.86,7.55,.025),'Solar',.008))
  for k in range(18):moving.append(box('Photovoltaic cell bus',(px,-3.55+k*.42,.425),(.83,.018,.008),'Copper',.001))
 pivot('Motion_SolarWing'+str(side),(side*.6,0,.25),moving)
for y in [-4.65,4.65]:
 cyl('Control jet',(0,y,.32),.22,.4,'Graphite',12)
 ring('Thermal collar',(0,y,.55),.19,.02,'Light')
save('SolarSail112')

scene.world=bpy.data.worlds.new('Campaign asset workshop');scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.12,.17,.23,1)
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Tools/CampaignWorld1.12.blend'),compress=True)
folder=ROOT/'Build/Campaign112';folder.mkdir(parents=True,exist_ok=True)
(folder/'model-catalog.json').write_text(json.dumps(catalog,indent=2)+'\n')
print('SKYBREAK_CAMPAIGN_MODELS_COMPLETE',json.dumps(catalog))
