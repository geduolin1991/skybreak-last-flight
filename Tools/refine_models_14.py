"""Blender MCP hard-surface refinement and authored environment kit for 1.4."""
import bpy,math,random,json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Assets/Art/Models';random.seed(1404)
scene=bpy.data.scenes.new('SKYBREAK 1.4 Production Models');bpy.context.window.scene=scene
palette={'Ceramic_Ivory':(.65,.76,.79),'Titanium_Dark':(.025,.046,.07),'Steel_Edge':(.16,.23,.28),'Enemy_Red':(.34,.032,.019),'Safety_Orange':(.95,.24,.045),'Ion_Cyan':(.04,.75,1),'Reactor_Orange':(1,.12,.015),'Core_Violet':(.54,.055,1),'Cockpit_Glass':(.015,.11,.18)}
materials={}
for name,col in palette.items():
 m=bpy.data.materials.get(name) or bpy.data.materials.new(name);m.diffuse_color=(*col,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*col,1);p.inputs['Metallic'].default_value=.65;p.inputs['Roughness'].default_value=.3;p.inputs['Emission Color'].default_value=(*col,1);p.inputs['Emission Strength'].default_value=1.5 if name in ['Ion_Cyan','Reactor_Orange','Core_Violet'] else 0;materials[name]=m
ivory,dark,edge,red,gold,cyan,fire,purple,glass=[materials[n] for n in ['Ceramic_Ivory','Titanium_Dark','Steel_Edge','Enemy_Red','Safety_Orange','Ion_Cyan','Reactor_Orange','Core_Violet','Cockpit_Glass']]
current=None;catalog={}
def begin(name,load=False):
 global current
 current=bpy.data.collections.new('P14_'+name);scene.collection.children.link(current);bpy.context.view_layer.active_layer_collection=bpy.context.view_layer.layer_collection.children[current.name]
 if load:
  bpy.ops.import_scene.fbx(filepath=str(ROOT/'Tools/ModelBaselines1.3'/(name+'.fbx')))
  for o in list(bpy.context.selected_objects):
   if o.type=='MESH':
    for j,m in enumerate(o.data.materials):
     canonical=m.name.split('.')[0] if m else 'Titanium_Dark';o.data.materials[j]=materials.get(canonical,dark)
 return current
def finish(o,name,m,bevel=.025):
 o.name=name;o.data.materials.append(m)
 if bevel:
  b=o.modifiers.new('Machined edge fillets','BEVEL');b.width=bevel;b.segments=3
  o.modifiers.new('Weighted surface normals','WEIGHTED_NORMAL')
 return o
def box(name,pos,scale,m=dark,bevel=.025):
 bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.scale=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return finish(o,name,m,bevel)
def cyl(name,pos,r,depth,m=edge,axis='Z',n=32):
 bpy.ops.mesh.primitive_cylinder_add(vertices=n,radius=r,depth=depth,location=pos);o=bpy.context.object
 if axis=='Y':o.rotation_euler.x=math.pi/2
 return finish(o,name,m,min(.018,r*.1))
def torus(name,pos,r,thick,m=cyan,axis='Z'):
 bpy.ops.mesh.primitive_torus_add(major_segments=48,minor_segments=8,major_radius=r,minor_radius=thick,location=pos);o=bpy.context.object
 if axis=='Y':o.rotation_euler.x=math.pi/2
 return finish(o,name,m,0)
def ell(name,pos,scale,m):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=12,location=pos);o=bpy.context.object;o.scale=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 for face in o.data.polygons:face.use_smooth=True
 return finish(o,name,m,0)
def pipe(name,a,b,r=.025,m=edge):
 mid=(Vector(a)+Vector(b))*.5;o=cyl(name,mid,r,(Vector(b)-Vector(a)).length,m);o.rotation_euler=(Vector(b)-Vector(a)).to_track_quat('Z','Y').to_euler();return o
def panel(name,pts,z,t,m):
 n=len(pts);v=[(x,y,z) for x,y in pts]+[(x,y,z+t) for x,y in pts];f=[tuple(reversed(range(n))),tuple(range(n,n*2))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)];mesh=bpy.data.meshes.new(name);mesh.from_pydata(v,[],f);mesh.update();o=bpy.data.objects.new(name,mesh);current.objects.link(o);return finish(o,name,m,.025)
def octagon(cx,cy,w,d):return [(cx-w*.35,cy-d*.5),(cx+w*.35,cy-d*.5),(cx+w*.5,cy-d*.35),(cx+w*.5,cy+d*.35),(cx+w*.35,cy+d*.5),(cx-w*.35,cy+d*.5),(cx-w*.5,cy+d*.35),(cx-w*.5,cy-d*.35)]
def plate(pos,w,d,m=ivory):return panel('Inset service armor',octagon(pos[0],pos[1],w,d),pos[2],.055,m)
def bolts(cx,cy,z,w,d):
 for a in [-1,1]:
  for b in [-1,1]:cyl('Flush hex fastener',(cx+a*w*.39,cy+b*d*.38,z),.025,.014,edge,n=6)
def vent(cx,cy,z,w,d,glow=None):
 plate((cx,cy,z),w,d,edge);box('Recessed heat exchanger',(cx,cy,z+.057),(w*.85,d*.86,.012),dark,.005)
 for j in range(7):box('Angled radiator fin',(cx,cy-d*.33+j*d*.11,z+.07),(w*.72,.022,.024),edge,.004)
 if glow:box('Thermal status strip',(cx+w*.43,cy,z+.078),(.024,d*.72,.017),glow,.004)
def center(o):
 return sum((o.matrix_world@Vector(c) for c in o.bound_box),Vector())/8
def mechanism(name,pivot,parts):
 root=bpy.data.objects.new(name,None);current.objects.link(root);root.location=pivot
 bpy.context.view_layer.update()
 for o in parts:
  matrix=o.matrix_world.copy();o.parent=root;o.matrix_world=matrix
 return root
def save(name):
 bpy.ops.object.select_all(action='DESELECT')
 for o in current.all_objects:o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False)
 deps=bpy.context.evaluated_depsgraph_get();verts=sum(len(o.evaluated_get(deps).data.vertices) for o in current.objects if o.type=='MESH');catalog[name]={'objects':len(current.objects),'evaluated_vertices':verts}
 # Studio layout is applied only after exporting game coordinates.
 slot=len(catalog)-1
 for o in current.objects:
  if not o.parent:o.location+=Vector(((slot%4)*14,(slot//4)*18,0))
# Three playable airframes: no silhouette reset; add legible construction layers.
for name in ['Kestrel','Manta','Needle']:
 begin(name,True);glow=cyan if name=='Kestrel' else fire if name=='Manta' else purple
 if name=='Kestrel':
  for s in [-1,1]:
   plate((s*1.08,-.48,.21),.48,.54,edge);bolts(s*1.08,-.48,.28,.48,.54);vent(s*.61,-.74,.34,.34,.62,glow)
   torus('Nozzle annular diffuser',(s*.61,-1.63,.14),.148,.018,edge,'Y')
   for j in range(6):a=j*math.pi/3;pipe('Exhaust petal',(s*.61+math.cos(a)*.16,-1.39,.14+math.sin(a)*.16),(s*.61+math.cos(a)*.19,-1.58,.14+math.sin(a)*.19),.016,edge)
   pipe('Canopy support rail',(s*.2,.0,.48),(s*.12,.92,.46),.025,edge)
  plate((0,-.42,.39),.34,.52,edge);bolts(0,-.42,.46,.34,.52)
 elif name=='Manta':
  for s in [-1,1]:
   vent(s*.7,-.45,.49,.4,.77,glow);plate((s*1.15,-.63,.4),.47,.5,ivory);bolts(s*1.15,-.63,.48,.47,.5)
   for j in range(3):
    for k in [-1,1]:torus('Missile launch tube collar',(s*1.82+k*.15,.08-j*.27,.737),.106,.016,edge)
   for j in [-1,1]:cyl('Gun trunnion',(s*.91+j*.29,.54,.17),.13,.17,dark,'Y');torus('Gun feed drive',(s*.91+j*.29,.64,.17),.09,.025,gold,'Y')
   torus('Afterburner diffuser',(s*1.68,-1.64,.14),.23,.035,edge,'Y')
   for j in range(5):box('Flank heat sink',(s*2.36,-.08-j*.15,.3),(.14,.055,.15),edge,.012)
  plate((0,-.46,.5),.68,.83,edge);bolts(0,-.46,.57,.68,.83)
 else:
  for s in [-1,1]:
   for j in range(5):cyl('Superconducting coil',(s*.29,.7+j*.37,.2),.17,.11,edge,'Y');torus('Coil pulse ring',(s*.29,.74+j*.37,.2),.15,.018,glow,'Y')
   vent(s*.64,-1.12,.31,.3,.69);pipe('Coolant return',(s*.48,-1.45,.21),(s*.47,.6,.22),.027,edge)
   torus('Nozzle',(s*.64,-1.85,.14),.16,.025,edge,'Y');bolts(s*.63,-1.2,.39,.33,.55)
  plate((0,-1.2,.45),.36,.49,edge)
 save(name)
# Enemy craft: different combat silhouettes and visible machinery, not player recolors.
for name in ['Drone','Interceptor','Gunship']:
 begin(name,True)
 if name=='Drone':
  torus('Armored sensor surround',(0,.42,.28),.27,.052,dark);ell('Optical eye',(0,.42,.31),(.16,.18,.1),fire)
  for s in [-1,1]:cyl('Ducted impeller',(s*.76,-.02,.15),.27,.13,edge);torus('Duct intake lip',(s*.76,-.02,.25),.22,.025,red)
  for s in [-1,1]:
   for j in range(6):a=j*math.pi/3;o=box('Impeller blade',(s*.76+math.cos(a)*.13,-.02+math.sin(a)*.13,.25),(.17,.047,.025),dark,.007);o.rotation_euler.z=a+.5
 else:
  for s in [-1,1]:
   vent(s*.56,-.8,.36,.39,.77,fire);plate((s*.86,-.58,.18),.52,.67,red);bolts(s*.86,-.58,.25,.52,.67)
   pipe('Exposed weapon feed',(s*.43,-.35,.43),(s*1.08,.22,.15),.03,edge)
   if name=='Gunship':
    cyl('Heavy rotary mount',(s*1.32,.08,.13),.25,.6,edge,'Y')
    for j in range(5):a=j*math.pi*2/5;cyl('Heavy cannon tube',(s*1.32+math.cos(a)*.13,.6,.13+math.sin(a)*.13),.048,.73,dark,'Y')
    for j in range(3):box('Side armor applique',(s*1.68,-.6+j*.26,.19),(.4,.18,.15),red,.04)
   else:panel('Intercept forward fin',[(s*.28,.7),(s*.7,1.28),(s*.95,.96),(s*.51,.3)],.16,.08,red)
  vent(0,-.35,.4,.3,.54,fire)
 save(name)
# Boss hulls acquire layered armor, turbine vanes, conduits and command details.
for name in ['Leviathan','Tempest','Seraph']:
 begin(name,True);glow=fire if name=='Leviathan' else cyan if name=='Tempest' else purple
 if name=='Leviathan':
  for s in [-1,1]:
   for j in range(4):plate((s*2.65,-1.25+j*.61,.95),.86,.5,ivory if j%2 else edge);bolts(s*2.65,-1.25+j*.61,1.02,.86,.5)
   for j in range(4):cyl('Siege missile silo',(s*3.25,-1.1+j*.53,.62),.2,.17,dark);torus('Silo locking ring',(s*3.25,-1.1+j*.53,.74),.17,.027,edge)
   pipe('Armored coolant manifold',(s*1.9,-1.6,.83),(s*1.9,1.35,.83),.075,edge)
   vent(s*.75,-1,.9,.5,.65,glow)
  for y in [.5,1.0,1.5]:plate((0,y,.81),.8,.34,edge)
  cyl('Command bridge',(0,.42,1.03),.39,.38,dark,n=8);ell('Bridge optics',(0,.63,1.25),(.27,.17,.12),glass)
 elif name=='Tempest':
  for s in [-1,1]:
   for j in range(16):a=j*math.pi/8;plate((s*3.1+math.cos(a)*1.22,math.sin(a)*1.22,.91),.19,.22,edge)
   for j in range(5):vent(s*4.53,-.94+j*.36,.5,.4,.23)
   pipe('Turbine bypass',(s*1.7,-1.1,.65),(s*2.15,-1.1,.65),.1,edge)
   plate((s*.58,-1,.91),.65,.77,ivory);bolts(s*.58,-1,.99,.65,.77)
  torus('Reactor cage',(0,-.35,1.11),.54,.075,dark)
 else:
  for j in range(6):
   a=j*math.pi/3+math.pi/6
   for d in [2.5,3.15,3.8,4.4]:
    x,y=math.cos(a)*d,math.sin(a)*d;o=plate((x,y,.35),.53,.44,edge)
    for v in o.data.vertices:
     dx,dy=v.co.x-x,v.co.y-y;v.co.x=x+dx*math.cos(a)-dy*math.sin(a);v.co.y=y+dx*math.sin(a)+dy*math.cos(a)
    cyl('Feather energy coupler',(x,y,.45),.095,.06,glow,n=12)
   x,y=math.cos(a)*1.65,math.sin(a)*1.65;cyl('Iris servo',(x,y,.37),.25,.3,edge);torus('Iris servo band',(x,y,.57),.19,.04,glow)
  for radius in [.98,1.35]:torus('Nested containment halo',(0,0,.83),radius,.065,ivory)
 if name=='Tempest':
  for side in [-1,1]:mechanism('Motion_Rotor_'+('L' if side<0 else 'R'),(side*3.1,0,.6),[o for o in list(current.objects) if o.type=='MESH' and o.name.startswith('Turbine blade') and center(o).x*side>0])
 if name=='Seraph':
  parts=[o for o in current.objects if o.type=='MESH' and o.name.startswith(('Seraph feather','Inset feather','Ion filament','Satellite emitter','Feather energy','Feather gimbal','Inset service armor')) and center(o).xy.length>1.8]
  for j in range(6):
   a=j*math.pi/3+math.pi/6
   selected=[o for o in parts if abs(math.atan2(math.sin(math.atan2(center(o).y,center(o).x)-a),math.cos(math.atan2(center(o).y,center(o).x)-a)))<math.pi/6]
   mechanism('Motion_Feather_'+str(j),(math.cos(a)*1.65,math.sin(a)*1.65,.35),selected)
 save(name)
# Destructible boss attachments.
for name in ['SiegeTurret','ShieldEmitter']:
 begin(name);cyl('Armored rotating base',(0,0,0),.62,.25,dark,n=12);torus('Bearing race',(0,0,.17),.49,.055,edge)
 if name=='SiegeTurret':
  panel('Turret armor',octagon(0,0,1.03,1.05),.2,.37,red);vent(0,-.2,.59,.65,.4,fire)
  for s in [-1,1]:
   cyl('Cannon mantle',(s*.3,.44,.35),.2,.35,edge,'Y')
   for j in range(4):a=j*math.pi/2;cyl('Autocannon barrel',(s*.3+math.cos(a)*.085,.99,.35+math.sin(a)*.085),.04,.85,dark,'Y')
   cyl('Muzzle cap',(s*.3,1.44,.35),.14,.045,fire,'Y')
 else:
  ell('Shield crystal',(0,0,.43),(.32,.32,.51),purple)
  for i in range(3):a=i*math.pi*2/3;pipe('Magnetic arm',(math.cos(a)*.51,math.sin(a)*.51,.13),(math.cos(a)*.26,math.sin(a)*.26,.95),.085,ivory)
  torus('Containment ring',(0,0,.63),.49,.047,purple)
 save(name)
# Port kit: shaped deck, ribbed cargo, round tanks, railings and a lattice crane.
begin('HarborPort');panel('Chamfered breakwater',octagon(0,0,8.5,12),-.5,.7,dark);panel('Inset concrete deck',octagon(0,0,8.1,11.6),.2,.13,edge)
for x in [-3.8,3.8]:
 for j in range(12):pipe('Guardrail post',(x,-5.3+j*.97,.36),(x,-5.3+j*.97,.78),.032,edge)
 pipe('Continuous dock rail',(x,-5.3,.78),(x,5.3,.78),.027,ivory)
 for y in [-4,-2,0,2,4]:cyl('Mooring bollard',(x*.88,y,.48),.13,.27,gold)
for i,(x,y) in enumerate([(-2,-3),(-2,-.2),(1.4,-3.2),(1.4,-.7)]):
 box('Cargo frame',(x,y,.94),(1.75,2.35,1.13),dark,.08);box('Cargo shell',(x,y,.95),(1.6,2.24,1.03),gold if i%2 else ivory,.045)
 for j in range(9):box('Cargo corrugation',(x-.65+j*.165,y,1.49),(.055,2.1,.028),edge,.008)
 for s in [-1,1]:pipe('Container locking bar',(x+s*.48,y-1.18,.5),(x+s*.48,y-1.18,1.39),.027,edge)
for x in [-2,0]:cyl('Fuel reservoir',(x,3.7,1),.83,1.2,ivory);torus('Reservoir rim',(x,3.7,1.63),.76,.045,edge)
for x in [1.6,3.0]:
 for y in [2.7,4.7]:pipe('Crane lattice leg',(x,y,.35),(x,y,4.0),.1,edge)
 for j in range(4):pipe('Crane diagonal brace',(x,2.7,.4+j*.85),(x,4.7,1.25+j*.85),.045,gold)
pipe('Crane upper beam',(1.6,2.7,4),(3,2.7,4),.13,gold);pipe('Crane outreach',(-1,2.7,4),(3.5,2.7,4),.12,gold);pipe('Lifting cable',(-.7,2.7,4),(-.7,2.7,2.2),.012,edge)
save('HarborPort')
# Architectural modules: terraces, chamfered plans, mullions and mechanical roofs.
begin('StormCity')
for tower,(cx,cy,h,w,d) in enumerate([(-1.7,-2,7.6,2.6,3.6),(1.6,2.1,10,2.7,3.4)]):
 panel('Chamfered tower plinth',octagon(cx,cy,w+1.1,d+1.1),0,.55,edge)
 for level in range(3):
  z=.55+level*h/3;ww=w*(1-level*.13);dd=d*(1-level*.12);panel('Setback glass facade',octagon(cx,cy,ww,dd),z,h/3-.05,glass)
  panel('Terrace slab',octagon(cx,cy,ww+.2,dd+.2),z,.12,ivory)
  for floor in range(3):
   fz=z+.48+floor*h/9
   for s in [-1,1]:box('Window belt',(cx+s*ww*.505,cy,fz),(.025,dd*.65,.13),cyan,.008);box('Window belt',(cx,cy+s*dd*.505,fz),(ww*.66,.025,.13),cyan,.008)
  for s in [-1,1]:
   for k in [-1,0,1]:pipe('Facade mullion',(cx+k*ww*.28,cy+s*dd*.51,z),(cx+k*ww*.28,cy+s*dd*.51,z+h/3),.027,edge)
 vent(cx,cy,h+.58,w*.48,d*.48);cyl('Roof water plant',(cx+.3,cy-.6,h+.9),.34,.48,edge);pipe('Antenna',(cx,cy,h+.6),(cx,cy,h+2),.035,edge);ell('Beacon',(cx,cy,h+2.02),(.08,.08,.08),fire)
save('StormCity')
# Orbital structures built from rings, pressure vessels and open trusses.
begin('OrbitHabitat')
for y in [-4,0,4]:
 cyl('Pressure compartment',(0,y,0),1.03,3.65,ivory,'Y');torus('Docking frame',(0,y-1.8,0),1.07,.13,edge,'Y');torus('Illuminated collar',(0,y+1.1,0),1.05,.035,cyan,'Y')
 for j in range(8):a=j*math.pi/4;pipe('Longitudinal rib',(math.cos(a)*1.05,y-1.8,math.sin(a)*1.05),(math.cos(a)*1.05,y+1.8,math.sin(a)*1.05),.035,edge)
for side in [-1,1]:
 for y in [-3.3,.5,4.3]:
  pipe('Radiator truss',(0,y,0),(side*5.7,y,0),.12,edge)
  for k in range(4):
   x=side*(1.8+k*.95);panel('Photovoltaic tile',octagon(x,y,.83,2.85),0,.065,glass)
   for j in range(6):box('Solar cell grid',(x,y-1.2+j*.48,.07),(.76,.02,.008),edge,.003)
   box('Solar bus',(x,y,.078),(.018,2.6,.008),cyan,.003)
save('OrbitHabitat')
# Replaces the obvious cube shoreline stones.
begin('BasaltIsland')
for i in range(13):
 a=i*2.399;r=math.sqrt(i)*.84;size=random.uniform(.7,1.7)
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1,location=(math.cos(a)*r,math.sin(a)*r,-.1+size*.45));o=bpy.context.object;o.scale=(size,size*.9,size*.9);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 for v in o.data.vertices:v.co*=random.uniform(.87,1.12)
 finish(o,'Weathered basalt',dark,0)
save('BasaltIsland')
# A selected clean studio render shows all six craft and the three bosses.
bpy.context.view_layer.active_layer_collection=bpy.context.view_layer.layer_collection
for c in scene.collection.children:
 if c.name.split('.')[0] not in ['P14_Kestrel','P14_Manta','P14_Needle','P14_Drone','P14_Interceptor','P14_Gunship','P14_Leviathan','P14_Tempest','P14_Seraph']:c.hide_render=True
bpy.ops.object.camera_add(location=(22,-22,70));cam=bpy.context.object;cam.rotation_euler=(Vector((22,17,0))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=60;scene.camera=cam
for loc,power,size in [((15,12,35),9000,24),((40,35,25),6500,19),((-10,15,18),4000,14)]:
 bpy.ops.object.light_add(type='AREA',location=loc);l=bpy.context.object;l.data.energy=power;l.data.size=size;l.rotation_euler=(Vector((22,17,0))-l.location).to_track_quat('-Z','Y').to_euler()
scene.world=bpy.data.worlds.new('Production studio');scene.world.color=(.12,.16,.21);scene.render.engine='CYCLES';scene.cycles.samples=24;scene.render.resolution_x=1600;scene.render.resolution_y=1200;scene.render.resolution_percentage=100;scene.render.filepath=str(ROOT/'Build/MCP-1.4/model-overview.png');bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Tools/ProductionModels1.4.blend'),copy=True)
(ROOT/'Build/MCP-1.4/model-catalog.json').write_text(json.dumps(catalog,indent=2));result=catalog
