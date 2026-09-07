"""Original Blender production assets for the visible, reactive 1.6 campaign.
Run in an isolated Blender process. Existing source scenes are never overwritten.
"""
import bpy, math, random, json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
(ROOT/'Build/WorldWork').mkdir(parents=True,exist_ok=True)
OUT=ROOT/'Assets/Art/Models';OUT.mkdir(parents=True,exist_ok=True)
scene=bpy.data.scenes.new('SKYBREAK 1.6 Living Campaign');bpy.context.window.scene=scene
random.seed(1607);current=None;catalog={}
palette={'Ceramic_Ivory':(.65,.76,.79),'Titanium_Dark':(.025,.046,.07),'Steel_Edge':(.16,.23,.28),'Enemy_Red':(.34,.032,.019),'Safety_Orange':(.95,.24,.045),'Ion_Cyan':(.04,.75,1),'Reactor_Orange':(1,.12,.015),'Core_Violet':(.54,.055,1),'Cockpit_Glass':(.015,.11,.18),'Civilian_White':(.76,.79,.72),'Rescue_Gold':(.95,.65,.15),'City_Concrete':(.24,.29,.32),'City_Window':(.16,.38,.5),'Park_Green':(.08,.23,.16),'Road_Asphalt':(.038,.052,.063)}
mats={}
for name,col in palette.items():
 m=bpy.data.materials.new('V16_'+name);m.diffuse_color=(*col,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*col,1);p.inputs['Metallic'].default_value=.55;p.inputs['Roughness'].default_value=.36
 if name in ['Ion_Cyan','Reactor_Orange','Core_Violet','City_Window']:p.inputs['Emission Color'].default_value=(*col,1);p.inputs['Emission Strength'].default_value=1
 mats[name]=m
def begin(name,load=False):
 global current
 current=bpy.data.collections.new('V16_'+name);scene.collection.children.link(current);bpy.context.view_layer.active_layer_collection=bpy.context.view_layer.layer_collection.children[current.name]
 if load:
  bpy.ops.import_scene.fbx(filepath=str(ROOT/'Tools/ModelBaselines1.5'/(name+'.fbx')))
  for o in bpy.context.selected_objects:
   if o.type=='MESH':
    for i,m in enumerate(o.data.materials):
     n=m.name.split('.')[0].removeprefix('World_').removeprefix('V16_') if m else 'Titanium_Dark';o.data.materials[i]=mats.get(n,mats['Steel_Edge'])
def finish(o,name,mat='Steel_Edge',bevel=.025):
 o.name=name;o.data.materials.clear();o.data.materials.append(mats[mat])
 if bevel:
  mod=o.modifiers.new('Manufactured edge radius','BEVEL');mod.width=bevel;mod.segments=3;o.modifiers.new('Weighted panel normals','WEIGHTED_NORMAL')
 return o
def box(name,p,s,m='Titanium_Dark',b=.025):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.scale=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return finish(o,name,m,b)
def cyl(name,p,r,d,m='Steel_Edge',axis='Z',n=32):
 bpy.ops.mesh.primitive_cylinder_add(vertices=n,radius=r,depth=d,location=p);o=bpy.context.object
 if axis=='Y':o.rotation_euler.x=math.pi/2
 if axis=='X':o.rotation_euler.y=math.pi/2
 return finish(o,name,m,min(.018,r*.1))
def ring(name,p,r,t,m='Ion_Cyan',axis='Z'):
 bpy.ops.mesh.primitive_torus_add(major_segments=48,minor_segments=8,major_radius=r,minor_radius=t,location=p);o=bpy.context.object
 if axis=='Y':o.rotation_euler.x=math.pi/2
 return finish(o,name,m,0)
def ell(name,p,s,m='Titanium_Dark'):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=32,ring_count=16,location=p);o=bpy.context.object;o.scale=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 for f in o.data.polygons:f.use_smooth=True
 return finish(o,name,m,0)
def beam(name,a,b,r=.025,m='Steel_Edge'):
 a,b=Vector(a),Vector(b);o=cyl(name,(a+b)*.5,r,(b-a).length,m);o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();return o
def plate(name,xy,z,d,m='Ceramic_Ivory'):
 n=len(xy);v=[(x,y,z) for x,y in xy]+[(x,y,z+d) for x,y in xy];f=[tuple(range(n-1,-1,-1)),tuple(range(n,n*2))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)];mesh=bpy.data.meshes.new(name);mesh.from_pydata(v,[],f);mesh.update();o=bpy.data.objects.new(name,mesh);current.objects.link(o);return finish(o,name,m)
def hatch(x,y,z,w=.4,d=.5,m='Ceramic_Ivory'):
 box('Recessed armor seam',(x,y,z),(w,d,.04),'Titanium_Dark',.01);box('Service panel',(x,y,z+.026),(w*.9,d*.9,.045),m,.015)
 for sx in [-1,1]:
  for sy in [-1,1]:cyl('Flush hex screw',(x+sx*w*.32,y+sy*d*.32,z+.055),.019,.012,'Steel_Edge',n=6)
def vent(x,y,z,w=.32,d=.55):
 box('Radiator well',(x,y,z),(w,d,.04),'Titanium_Dark',.01)
 for k in range(7):box('Heat exchanger fin',(x,y-d*.37+k*d*.12,z+.03),(w*.9,.025,.036),'Steel_Edge',.006)
def engine(x,y,z,r=.23,col='Reactor_Orange'):
 cyl('Engine pressure vessel',(x,y,z),r,.85,'Titanium_Dark','Y');cyl('Exhaust rim',(x,y-.48,z),r*1.08,.12,'Steel_Edge','Y');ring('Ignition ring',(x,y-.56,z),r*.82,.025,col,'Y');cyl('Dark exhaust throat',(x,y-.58,z),r*.68,.07,'Titanium_Dark','Y')
 for k in range(8):
  a=k*math.pi/4;beam('Nozzle petals',(x+math.cos(a)*r,y-.34,z+math.sin(a)*r),(x+math.cos(a)*r*1.1,y-.5,z+math.sin(a)*r*1.1),.02)
def gun(x,y,z,r=.10):
 cyl('Gun shroud',(x,y,z),r*1.6,.5,'Titanium_Dark','Y');cyl('Gun barrel',(x,y+.45,z),r,.75,'Steel_Edge','Y');ring('Muzzle brake',(x,y+.82,z),r*1.1,.022,'Safety_Orange','Y');cyl('Muzzle bore',(x,y+.85,z),r*.64,.025,'Titanium_Dark','Y')
def save(name):
 bpy.ops.object.select_all(action='DESELECT')
 for o in current.all_objects:o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False)
 deps=bpy.context.evaluated_depsgraph_get();catalog[name]={'objects':len(current.all_objects),'vertices':sum(len(o.evaluated_get(deps).data.vertices) for o in current.all_objects if o.type=='MESH')}
 slot=len(catalog)-1
 for o in current.objects:
  if not o.parent:o.location+=Vector(((slot%5)*18,(slot//5)*22,0))

# Distinct hostile silhouettes and readable weapon construction.
for kind,name in enumerate(['DiveBomber','WardDrone','RailLancer','MineTender','DroneCarrier','CargoShuttle']):
 begin(name)
 if kind==0:
  plate('Arrowhead armored fuselage',[(-.55,-1.4),(.55,-1.4),(.72,.3),(0,2),(-.72,.3)],0,.35,'Enemy_Red');ell('Armored cockpit',(0,.65,.4),(.3,.7,.22),'Cockpit_Glass')
  for s in [-1,1]:
   plate('Swept attack wing',[(s*.4,.6),(s*2.2,-.8),(s*2.35,-1.5),(s*.5,-.85)],.05,.15,'Steel_Edge');engine(s*.85,-.8,.18,.25);cyl('External bomb',(s*.76,.15,-.04),.19,1,'Safety_Orange','Y');hatch(s*1.3,-.75,.24,.45,.52,'Enemy_Red');gun(s*.35,.65,.08,.075)
 elif kind==1:
  cyl('Shield hub',(0,0,.12),.6,.3,'Titanium_Dark');ring('Shield induction circle',(0,0,.22),1.06,.095,'Steel_Edge');ell('Shield core',(0,0,.4),(.31,.31,.27),'Core_Violet')
  for i in range(6):
   a=i*math.pi/3;x,y=math.cos(a),math.sin(a);beam('Emitter outrigger',(x*.4,y*.4,.1),(x*1.5,y*1.5,.1),.095);ell('Generator pod',(x*1.3,y*1.3,.16),(.25,.35,.22),'Enemy_Red');ring('Emitter status',(x*1.3,y*1.3,.35),.16,.025,'Core_Violet')
 elif kind==2:
  plate('Lancer spine',[(-.4,-1.6),(.4,-1.6),(.5,.2),(0,1.5),(-.5,.2)],0,.26,'Enemy_Red');ell('Sensor canopy',(0,-.2,.36),(.25,.5,.18),'Cockpit_Glass')
  for s in [-1,1]:
   beam('Rail spar',(s*.46,.1,.18),(s*.46,2.9,.18),.10);plate('Reinforced stabilizer',[(s*.4,-.4),(s*1.65,-1.5),(s*1.75,-1.95),(s*.5,-1.3)],0,.12,'Steel_Edge');engine(s*.6,-1.1,.14,.19)
   for j in range(6):ring('Rail capacitor coil',(s*.46,.3+j*.4,.18),.145,.024,'Reactor_Orange','Y')
 elif kind==3:
  plate('Mine tender deck',[(-1.45,-1),(-1.55,.8),(-.9,1.4),(.9,1.4),(1.55,.8),(1.45,-1)],0,.4,'Enemy_Red');hatch(0,.1,.45,.55,.8)
  for s in [-1,1]:
   engine(s*1.25,-.6,.16,.28);vent(s*.7,.75,.45,.5,.7)
   for j in range(3):cyl('Mine dispenser',(s*.66,-.65+j*.42,.42),.23,.18);ring('Arming collar',(s*.66,-.65+j*.42,.55),.19,.02,'Reactor_Orange')
 elif kind==4:
  plate('Carrier main hull',[(-1,-2),(1,-2),(1.2,1.45),(.5,2.5),(-.5,2.5),(-1.2,1.45)],0,.5,'Titanium_Dark');box('Flight operations bridge',(0,.7,.6),(.72,1,.4),'Enemy_Red');box('Bridge glazing',(0,1.12,.82),(.59,.07,.16),'Cockpit_Glass')
  for s in [-1,1]:
   plate('Launch sponson',[(s*.7,1.1),(s*2.4,.45),(s*2.6,-1.65),(s*.8,-1.9)],.02,.33,'Enemy_Red');engine(s*1.8,-1.55,.1,.38);gun(s*.75,1.4,.22)
   for j in range(3):box('Drone launch track',(s*1.65,-1+j*.6,.38),(.65,.42,.055),'Steel_Edge');box('Hangar runway lights',(s*2.08,-1+j*.6,.41),(.07,.3,.035),'Reactor_Orange')
 else:
  box('Cargo pressure shell',(0,0,.15),(1.3,2.8,.75),'Steel_Edge',.16);ell('Forward cockpit',(0,1.38,.35),(.52,.65,.36),'Cockpit_Glass')
  for s in [-1,1]:
   engine(s*1,-.8,.16,.3,'Ion_Cyan');plate('Lifting wing',[(s*.6,.6),(s*1.65,.1),(s*1.8,-1.2),(s*.6,-.6)],0,.14,'Safety_Orange');gun(s*.85,.55,.1,.075)
  for j in range(5):box('Cargo restraints',(0,-1+j*.47,.57),(1.4,.06,.07),'Safety_Orange',.01)
 save(name)

begin('RescueFerry')
plate('Displacement hull',[(-.7,-2.4),(.7,-2.4),(.95,-1.7),(.95,1.45),(.6,2.2),(0,2.65),(-.6,2.2),(-.95,1.45),(-.95,-1.7)],-.25,.44,'Titanium_Dark')
plate('Walkaround deck',[(-.85,-2.05),(.85,-2.05),(.85,1.35),(0,2.3),(-.85,1.35)],.16,.13,'Civilian_White')
box('Passenger cabin',(0,-.05,.58),(1.3,2.5,.65),'Civilian_White',.13);box('Bridge upper',(0,.82,1.02),(1.08,.85,.42),'Civilian_White',.09);box('Bridge windshield',(0,1.245,1.10),(.91,.025,.18),'Cockpit_Glass',.008)
for s in [-1,1]:
 for j in range(9):box('Cabin window',(s*.659,-1.12+j*.28,.65),(.025,.15,.21),'Cockpit_Glass',.005)
 for j in range(12):beam('Safety railing stanchion',(s*.8,-1.95+j*.34,.31),(s*.8,-1.95+j*.34,.57),.018,'Civilian_White')
 beam('Upper handrail',(s*.8,-1.95,.57),(s*.8,1.75,.57),.022,'Civilian_White');ell('Inflatable lifeboat',(s*.68,-1.6,.56),(.16,.4,.15),'Rescue_Gold')
 for j in range(3):cyl('Life raft canister',(s*.37,-.75+j*.44,1),.13,.27,'Civilian_White','Y')
beam('Radio mast',(0,.65,1.19),(0,.65,1.82),.04);beam('Radar crossbar',(-.4,.65,1.63),(.4,.65,1.63),.035);box('Red cross long',(0,-.35,1.005),(.1,.52,.013),'Safety_Orange',.002);box('Red cross short',(0,-.35,1.008),(.42,.11,.013),'Safety_Orange',.002)
save('RescueFerry')

begin('EvacBus');box('Electric evacuation bus',(0,0,.45),(.8,2.1,.7),'Civilian_White',.12);box('Roof battery',(0,-.1,.85),(.64,1.25,.17),'Steel_Edge',.04)
for s in [-1,1]:
 for j in [-.65,.65]:cyl('Bus wheel',(s*.43,j,.25),.22,.12,'Titanium_Dark','X',24)
 for j in range(6):box('Passenger glazing',(s*.407,-.74+j*.29,.61),(.023,.21,.26),'Cockpit_Glass',.004)
 box('Rescue lane stripe',(s*.42,0,.38),(.024,1.82,.095),'Rescue_Gold',.003)
box('Front glass',(0,1.05,.64),(.68,.035,.3),'Cockpit_Glass',.02);save('EvacBus')

begin('TrafficCar');box('Car body',(0,0,.2),(.7,1.32,.3),'Steel_Edge',.13);ell('Glass cabin',(0,-.06,.43),(.3,.42,.24),'Cockpit_Glass')
for s in [-1,1]:
 for y in [-.4,.4]:cyl('Wheel',(s*.35,y,.14),.16,.1,'Titanium_Dark','X',20)
 box('Headlamp',(s*.22,.65,.25),(.14,.04,.08),'Civilian_White',.01)
save('TrafficCar')

begin('GridPylon');cyl('Pylon plinth',(0,0,.09),.85,.18,'City_Concrete',n=8);cyl('Switchgear pedestal',(0,0,.48),.57,.62,'Titanium_Dark',n=8);cyl('Interference column',(0,0,1.3),.24,1.3,'Steel_Edge');ring('Jamming resonator',(0,0,1.88),.68,.085,'Core_Violet')
for i in range(4):
 a=i*math.pi/2;x,y=math.cos(a),math.sin(a);beam('Ceramic isolator',(x*.35,y*.35,.75),(x*.58,y*.58,1.68),.075,'Civilian_White');ell('Emitter tip',(x*.58,y*.58,1.74),(.12,.12,.18),'Core_Violet')
for j in range(6):ring('Insulated bus coil',(0,0,.75+j*.16),.29,.027,'Safety_Orange')
save('GridPylon')

begin('OrbitalDock');cyl('Docking collar',(0,0,.0),2,.27,'Steel_Edge',n=48);ring('Dock approach ring',(0,0,.2),1.7,.065,'Ion_Cyan');cyl('Dock negative space',(0,0,.15),1.45,.16,'Titanium_Dark');box('Access bridge',(0,-2.2,.02),(1.4,2.8,.35),'Ceramic_Ivory')
for s in [-1,1]:
 box('Solar wing spar',(s*3.1,0,0),(2.8,.18,.18),'Steel_Edge')
 for i in range(4):
  box('Solar panel',(s*(2.05+i*.64),0,.06),(.57,3.5,.065),'Cockpit_Glass',.012)
  for j in range(7):box('Photovoltaic row',(s*(2.05+i*.64),-1.5+j*.5,.102),(.54,.018,.006),'Ion_Cyan',.001)
 for j in range(3):hatch(s*.47,-1.6-j*.45,.24,.29,.35)
for i in range(8):
 a=i*math.pi/4;box('Dock alignment beacon',(math.cos(a)*1.78,math.sin(a)*1.78,.27),(.09,.09,.05),'Rescue_Gold',.01)
save('OrbitalDock')

begin('CityDistrict')
box('District foundation',(0,0,-.25),(8,12,.4),'City_Concrete',.1)
for x,y,w,d,h in [(-2.2,-3,2.5,3.2,1.5),(2,-3,2.6,3.3,2.2),(-2.3,2.2,2.3,4.2,2.8),(1.8,2.3,2.9,3.7,1.8)]:
 box('Building podium',(x,y,.18),(w+.35,d+.35,.5),'City_Concrete',.1);box('Architectural tower',(x,y,.35+h*.5),(w,d,h),'Steel_Edge',.08);box('Parapet coping',(x,y,h+.38),(w+.1,d+.1,.15),'City_Concrete',.025)
 for z in [.8,1.25,1.7,2.15,2.6]:
  if z>h+.15:continue
  for s in [-1,1]:
   for j in range(5):box('Window bay',(x-w*.37+j*w*.185,y+s*(d*.5+.012),z),(w*.12,.025,.22),'City_Window',.006)
   for j in range(6):box('Side window',(x+s*(w*.5+.015),y-d*.39+j*d*.156,z),(.025,d*.09,.22),'City_Window',.006)
 for s in [-1,1]:vent(x+s*w*.22,y,h+.49,.38,.7);cyl('Roof extractor',(x+s*w*.24,y+.8,h+.56),.2,.18)
 hatch(x,y-.8,h+.47,.6,.5,'City_Concrete')
for j in range(9):
 for s in [-1,1]:cyl('Street tree trunk',(s*3.72,-5+j*1.22,.22),.065,.45,'Titanium_Dark',n=10);ell('Street tree canopy',(s*3.72,-5+j*1.22,.62),(.27,.32,.4),'Park_Green')
save('CityDistrict')

begin('SupplyBeacon');cyl('Support transponder capsule',(0,0,0),.37,.48,'Ceramic_Ivory',n=12);ring('Transponder aerial',(0,0,.31),.45,.055,'Ion_Cyan');cyl('Transponder core',(0,0,.34),.21,.22,'Ion_Cyan')
for s in [-1,1]:plate('Deployable wing',[(s*.27,-.15),(s*.82,-.35),(s*.77,.3),(s*.27,.12)],-.06,.07,'Rescue_Gold')
save('SupplyBeacon')

# Additional geometry remains visible at the actual top-down camera scale.
for name in ['Kestrel','Manta','Needle','Drone','Interceptor','Gunship','Leviathan','Tempest','Seraph','AstraFrame','CrimsonFrame','OracleFrame','EvacCarrier','StormRelay','OrbitalGate','SiegeTurret','ShieldEmitter','HarborPort','StormCity','OrbitHabitat','BasaltIsland']:
 begin(name,True)
 if name in ['Kestrel','Manta','Needle']:
  for s in [-1,1]:
   hatch(s*.42,-.6,.46,.26,.52);beam('Hydraulic feed line',(s*.45,-1,.33),(s*.44,.3,.34),.021)
   for j in range(3):box('Wing insignia',(s*(1.0+j*.18),-.5,.31),(.09,.44-j*.06,.025),'Civilian_White',.003)
   gun(s*(.57 if name=='Kestrel' else .92 if name=='Manta' else .28),.55,.15,.068)
 elif name in ['Drone','Interceptor','Gunship']:
  for s in [-1,1]:hatch(s*.35,-.2,.28,.3,.35,'Enemy_Red');vent(s*.48,-.5,.3,.22,.4);gun(s*.5,.15,.05,.055)
 elif name in ['Leviathan','Tempest','Seraph']:
  for s in [-1,1]:
   for j in range(3):hatch(s*(1+j*.48),-.6+j*.18,.6,.4,.6,'Enemy_Red');vent(s*(1+j*.48),.45,.57,.3,.6)
   beam('Dorsal armored conduit',(s*.65,-2,.53),(s*.65,.8,.58),.055);cyl('Navigation radar',(s*.85,-.2,.82),.25,.06,'Steel_Edge');ring('Radar etched rim',(s*.85,-.2,.86),.2,.018,'Reactor_Orange')
 elif 'Frame' in name:
  for s in [-1,1]:hatch(s*.35,-.12,.55,.24,.35);ring('Torso link collar',(s*.48,0,.25),.13,.025,'Ion_Cyan')
 elif name in ['HarborPort','StormCity','OrbitHabitat']:
  for s in [-1,1]:
   for j in range(5):hatch(s*1.5,-2+j*.9,.13,.6,.55,'City_Concrete');box('Ground directional strip',(s*2.2,-2+j*.9,.18),(.07,.42,.018),'Rescue_Gold',.002)
 elif name=='EvacCarrier':
  for s in [-1,1]:
   for j in range(8):ell('Packed rescue raft',(s*.9,-2+j*.52,.8),(.15,.22,.14),'Rescue_Gold')
 save(name)

scene.world=bpy.data.worlds.new('Production studio');scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs['Color'].default_value=(.12,.17,.23,1);scene.world.node_tree.nodes['Background'].inputs['Strength'].default_value=.6
bpy.data.libraries.write(str(ROOT/'Tools/LivingCampaign1.6.blend'),{scene},fake_user=True,compress=True)
(ROOT/'Build/WorldWork/model-catalog.json').write_text(json.dumps(catalog,indent=2)+'\n')
print('SKYBREAK_16_MODELS_COMPLETE',len(catalog))
