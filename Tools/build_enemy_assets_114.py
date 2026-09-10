"""Original SKYBREAK enemy craft and recoverable equipment, authored with bpy.
Run in a dedicated Blender process. --one previews one asset before a full export.
"""
import bpy,bmesh,math,json,sys
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
one=args[args.index('--one')+1] if '--one' in args else None
OUT=ROOT/'Build/Enemy114/CandidateModels';OUT.mkdir(parents=True,exist_ok=True)
scene=bpy.data.scenes.new('SKYBREAK / enemy foundry 1.14');bpy.context.window.scene=scene
palette={'Hull':(.31,.047,.042),'Edge':(.48,.55,.58),'Dark':(.023,.037,.053),'Glass':(.02,.13,.19),'Hot':(1,.31,.05),'Ivory':(.77,.83,.82),'Green':(.06,.86,.48),'Blue':(.045,.65,1),'Gold':(.92,.64,.12)}
mats={}
for key,c in palette.items():
 m=bpy.data.materials.new('Foundry_'+key);m.diffuse_color=(*c,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*c,1);p.inputs['Metallic'].default_value=.62 if key not in ['Ivory','Glass'] else .25;p.inputs['Roughness'].default_value=.32 if key!='Glass' else .16
 if key in ['Hot','Green','Blue']:p.inputs['Emission Color'].default_value=(*c,1);p.inputs['Emission Strength'].default_value=.65
 mats[key]=m
collection=None;catalog={}
def begin(name):
 global collection
 collection=bpy.data.collections.new(name);scene.collection.children.link(collection);bpy.context.view_layer.active_layer_collection=bpy.context.view_layer.layer_collection.children[collection.name]
def finish(o,name,mat,bevel=.018):
 o.name=name;o.data.materials.clear();o.data.materials.append(mats[mat])
 if bevel:
  m=o.modifiers.new('Manufactured edge','BEVEL');m.width=bevel;m.segments=2
  o.modifiers.new('Surface normals','WEIGHTED_NORMAL')
 return o
def box(name,p,size,mat='Dark',bevel=.018):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.scale=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return finish(o,name,mat,bevel)
def cyl(name,p,r,depth,mat='Edge',axis='Z',n=16):
 bpy.ops.mesh.primitive_cylinder_add(vertices=n,radius=r,depth=depth,location=p);o=bpy.context.object
 if axis=='Y':o.rotation_euler.x=math.pi/2
 return finish(o,name,mat,min(.018,r*.10))
def panel(name,pts,z,t,mat='Hull'):
 n=len(pts);v=[(x,y,z) for x,y in pts]+[(x,y,z+t) for x,y in pts];faces=[tuple(reversed(range(n))),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(v,[],faces);mesh.update();bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free();o=bpy.data.objects.new(name,mesh);collection.objects.link(o);return finish(o,name,mat)
def loft(name,rings,mat='Hull',cx=0,cz=0,n=16):
 # Elliptical cross sections create a continuous aerodynamic shell, not stacked cubes.
 verts=[(cx+math.cos(a*math.tau/n)*w,y,cz+math.sin(a*math.tau/n)*h) for y,w,h in rings for a in range(n)]
 faces=[tuple(reversed(range(n))),tuple(range((len(rings)-1)*n,len(rings)*n))]
 for j in range(len(rings)-1):
  for i in range(n):faces.append((j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i))
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free();o=bpy.data.objects.new(name,mesh);collection.objects.link(o)
 for p in mesh.polygons:p.use_smooth=len(p.vertices)==4
 return finish(o,name,mat,0)
def ring(name,p,r,t,mat='Edge',axis='Z'):
 bpy.ops.mesh.primitive_torus_add(major_segments=24,minor_segments=6,major_radius=r,minor_radius=t,location=p);o=bpy.context.object
 if axis=='Y':o.rotation_euler.x=math.pi/2
 return finish(o,name,mat,0)
def wing(side,points,z=.04,t=.085,mat='Hull',name='Swept wing'):
 return panel(name,[(side*x,y) for x,y in points],z,t,mat)
def canopy(length=1,width=.23,y=.15,z=.24):
 loft('Obsidian observation canopy',[(y-length*.5,width*.65,.02),(y-length*.25,width,.16),(y+length*.3,width*.65,.13),(y+length*.5,.035,.025)],'Glass',cz=z)
 for s in [-1,1]:box('Canopy spine',(s*width*.78,y,z+.11),(.028,length*.7,.035),'Edge')
def vent(x,y,z,w=.3,d=.5,hot=True):
 box('Recessed heat channel',(x,y,z),(w,d,.06),'Dark')
 for i in range(5):box('Heat exchanger fin',(x,y-d*.36+i*d*.18,z+.042),(w*.87,.025,.03),'Edge',.004)
 if hot:box('Heat-status edge',(x+w*.49,y,z+.04),(.023,d*.8,.025),'Hot',.004)
def nozzle(x,y,z,r=.21):
 cyl('Armored engine housing',(x,y+.27,z),r*1.2,.58,'Dark','Y');cyl('Nozzle chamber',(x,y,z),r,.09,'Edge','Y');cyl('Recessed ion exhaust',(x,y-.055,z),r*.68,.025,'Hot','Y');ring('Nozzle segmented lip',(x,y-.025,z),r*.89,.025,'Edge','Y')
 for s in [-1,1]:box('Engine fin',(x+s*r*.9,y+.2,z+r*.1),(.04,.46,r*.52),'Hull')
def gun(x,y,z,r=.08,length=.6):
 cyl('Cannon cradle',(x,y-length*.25,z),r*1.6,length*.5,'Edge','Y');cyl('Barrel shroud',(x,y,z),r,length,'Dark','Y');ring('Muzzle collar',(x,y+length*.5,z),r,.018,'Edge','Y')
def hatch(name,x,y,z,w,d):
 parts=[];parts.append(panel('Cooling armored leaf',[(x-w/2,y-d/2),(x+w/2,y-d/2),(x+w*.42,y+d/2),(x-w*.42,y+d/2)],z,.07,'Hull'))
 for i in range(3):parts.append(box('Heat warning chevron',(x,y-d*.23+i*d*.22,z+.08),(w*.72,.035,.015),'Gold',.004))
 root=bpy.data.objects.new(name,None);collection.objects.link(root);root.location=(x+(w/2 if x>0 else -w/2),y,z);bpy.context.view_layer.update()
 for o in parts:world=o.matrix_world.copy();o.parent=root;o.matrix_world=world
 return root
def craft(name):
 begin(name)
 if name=='Drone':
  loft('Scythe drone body',[(-.9,.12,.08),(-.5,.31,.15),(.2,.30,.19),(.73,.08,.06)])
  for s in [-1,1]:
   wing(s,[(.14,.4),(.54,.6),(1.07,.15),(1.05,-.55),(.7,-.7),(.46,-.2)],.02);cyl('Ducted rotor',(s*.75,-.12,.1),.27,.10,'Dark');ring('Intake rim',(s*.75,-.12,.18),.25,.035,'Edge')
   for j in range(5):a=j*math.tau/5;o=box('Rotor blade',(s*.75+math.cos(a)*.12,-.12+math.sin(a)*.12,.18),(.22,.045,.03),'Edge',.003);o.rotation_euler.z=a+.3
  cyl('Single hot optical eye',(0,.45,.22),.12,.06,'Hot');nozzle(0,-.9,.0,.13)
 elif name=='Interceptor':
  loft('Needle interceptor fuselage',[(-1.3,.12,.10),(-.7,.25,.17),(.2,.32,.22),(1.15,.15,.14),(1.7,.015,.015)])
  canopy(1.1,.22,.5,.21)
  for s in [-1,1]:
   wing(s,[(.2,.38),(1.55,-.72),(1.43,-1.12),(.39,-.56)],.05);wing(s,[(.18,-.6),(.53,-1.3),(.75,-1.28),(.32,-.17)],.29,.06,'Edge');wing(s,[(.36,-.06),(1.12,-.65),(.99,-.72),(.36,-.3)],.145,.012,'Ivory');nozzle(s*.4,-1.28,.04,.18);gun(s*.57,.27,.0,.057,.62)
 elif name in ['Gunship','DroneCarrier','CargoShuttle']:
  carrier=name=='DroneCarrier';cargo=name=='CargoShuttle';span=2.25 if carrier else 1.64
  loft('Armored continuous hull',[(-1.43,.30,.18),(-.95,.61,.35),(.45,.7,.36),(1.13,.41,.28),(1.5,.17,.09)])
  canopy(.73,.27,.67,.33)
  for s in [-1,1]:
   wing(s,[(.4,.95),(span*.78,.65),(span,-.5),(span*.89,-1.1),(.45,-.85)],.03,.2)
   wing(s,[(.71,.40),(span*.81,.1),(span*.85,-.58),(.72,-.78)],.25,.07,'Edge')
   nozzle(s*(1.54 if carrier else .94),-1.28,.13,.29 if carrier else .25)
   vent(s*.35,-.71,.41,.37,.65);box('Exposed vulnerable radiator',(s*.76,.14,.29),(.37,.72,.06),'Hot');hatch('Motion_Vent_'+('L' if s<0 else 'R'),s*.76,.14,.33,.44,.80)
   if carrier:
    box('Drone bay opening',(s*1.48,.40,.32),(.69,1.14,.10),'Dark');wing(s,[(1.09,1),(1.65,.95),(1.85,-.06),(1.55,-.26)],.39,.10,'Hull');box('Launch runner',(s*1.46,.47,.41),(.055,.85,.025),'Gold');gun(s*.44,1.13,.0,.08,.5)
   elif cargo:
    for i in range(3):box('Ribbed armored cargo pod',(s*1.13,.33-i*.4,.36),(.62,.32,.30),'Ivory',.07)
    gun(s*.39,.95,.06,.07,.38)
   else:
    cyl('Rotary weapon gearbox',(s*1.26,.30,.16),.22,.48,'Edge','Y')
    for j in range(5):a=j*math.tau/5;gun(s*1.26+math.cos(a)*.12,.8,.16+math.sin(a)*.12,.04,.80)
  panel('Dorsal serial stripe',[(-.10,-.98),(.1,-.98),(.09,.2),(-.09,.2)],.45,.014,'Ivory')
 elif name=='DiveBomber':
  loft('Dive bomber armored nose',[(-1.43,.2,.12),(-.54,.37,.26),(.64,.29,.24),(1.23,.08,.05)])
  canopy(.8,.23,.35,.25)
  for s in [-1,1]:
   wing(s,[(.2,.7),(.65,.78),(1.3,.12),(1.73,-.75),(1.28,-.89),(.58,-.23)],.09,.12);wing(s,[(.68,.45),(1.02,.1),(1.51,-.7),(1.27,-.69),(.62,-.14)],.22,.025,'Ivory')
   loft('External bomb pod',[(-.77,.16,.13),(-.3,.2,.19),(.47,.17,.15),(.78,.04,.04)],'Dark',s*.87,-.11);ring('Payload arming band',(s*.87,.21,-.11),.185,.033,'Hot','Y');nozzle(s*.36,-1.4,.06,.18);gun(s*.3,.75,.0,.055,.45)
 elif name=='WardDrone':
  loft('Shield command spindle',[(-.97,.17,.10),(-.5,.27,.20),(.4,.30,.25),(.94,.06,.02)],'Dark');cyl('Hexagonal field emitter',(0,0,.28),.43,.12,'Edge',n=6);cyl('Warm shield iris',(0,0,.37),.25,.08,'Hot',n=12)
  for j in range(6):
   a=j*math.tau/6;points=[(.56,-.21),(.98,-.26),(1.13,.1),(.82,.36),(.52,.14)];pts=[(x*math.cos(a)-y*math.sin(a),x*math.sin(a)+y*math.cos(a)) for x,y in points];panel('Segmented shield vane',pts,.02,.15);cyl('Shield coupling',(math.cos(a)*.74,math.sin(a)*.74,.23),.10,.05,'Hot',n=8)
 elif name=='RailLancer':
  loft('Lancer center keel',[(-1.63,.18,.14),(-.68,.31,.24),(.40,.22,.16),(1.44,.05,.04)],'Dark');canopy(.67,.2,-.22,.23)
  for s in [-1,1]:
   wing(s,[(.22,-.08),(1.0,-.95),(.93,-1.52),(.38,-1.22)],.08,.14);gun(s*.38,.65,.13,.13,2.2)
   for j in range(5):ring('Accelerator winding',(s*.38,.15+j*.25,.13),.145,.035,'Edge','Y')
   nozzle(s*.47,-1.61,.08,.17);box('Charged rail indicator',(s*.38,.9,.30),(.045,1.04,.028),'Hot')
 elif name=='MineTender':
  loft('Minelayer command hull',[(-1.2,.21,.19),(-.49,.42,.28),(.64,.38,.27),(1.0,.13,.09)]);canopy(.6,.27,.49,.3)
  for s in [-1,1]:
   wing(s,[(.3,.52),(1.27,.13),(1.34,-1.0),(.94,-1.32),(.51,-.36)],-.02,.23)
   for j in range(3):cyl('Mine deployment well',(s*.89,-.82+j*.38,.25),.21,.16,'Dark');ring('Payload retaining collar',(s*.89,-.82+j*.38,.35),.18,.025,'Edge');cyl('Payload indicator',(s*.89,-.82+j*.38,.36),.065,.018,'Hot')
   nozzle(s*.36,-1.25,.02,.19)
 for s in [-1,1]:box('Faction double stripe',(s*.11,-.4,.51 if name in ['Gunship','DroneCarrier','CargoShuttle'] else .32),(.045,.23,.015),'Ivory',.002)
def pickup(kind):
 name=['RepairPod114','OrdnanceRack114','ReactorCell114','WingBeacon114'][kind];begin(name);col=['Green','Hot','Blue','Gold'][kind]
 if kind==0:
  panel('Chamfered rescue case',[(-.4,-.47),(.4,-.47),(.52,-.31),(.52,.31),(.34,.5),(-.34,.5),(-.52,.31),(-.52,-.31)],-.17,.30,'Ivory');box('Equipment face',(0,0,.15),(.72,.76,.045),'Dark');box('Repair cross vertical',(0,0,.19),(.13,.52,.05),col);box('Repair cross horizontal',(0,0,.20),(.49,.14,.05),col)
  for s in [-1,1]:box('Rescue handle',(s*.49,0,.11),(.07,.43,.1),'Edge');box('Green identification rail',(s*.36,0,.2),(.035,.57,.03),col)
 elif kind==1:
  panel('Twin ordnance sled',[(-.47,-.63),(.47,-.63),(.47,.45),(.28,.65),(-.28,.65),(-.47,.45)],-.12,.13,'Edge')
  for s in [-1,1]:
   loft('Recoverable missile',[(-.53,.12,.12),(.26,.13,.13),(.54,.06,.06),(.68,.01,.01)],'Ivory',s*.24,.1);ring('Safety band',(s*.24,.0,.1),.135,.035,'Hot','Y');wing(s,[(.14,-.3),(.46,-.57),(.46,-.69),(.15,-.52)],.04,.08,'Dark')
 elif kind==2:
  cyl('Energy cell lower cap',(0,0,-.2),.38,.14,'Edge',n=8);cyl('Energy cell top cap',(0,0,.26),.30,.13,'Ivory',n=8);cyl('Cyan induction core',(0,0,.01),.25,.43,col,n=16)
  for i in range(4):ring('Induction coil',(0,0,-.12+i*.10),.29,.022,'Dark')
  for s in [-1,1]:box('Cell carrying cage',(s*.36,0,.04),(.085,.30,.55),'Dark')
  panel('Charge arrow',[(-.13,-.12),(.04,-.12),(.0,.01),(.13,.01),(-.02,.24),(.0,.08),(-.13,.08)],.34,.025,col)
 else:
  loft('Support beacon chassis',[(-.55,.12,.07),(-.15,.22,.13),(.23,.18,.13),(.57,.04,.02)],'Ivory');cyl('Comms dish',(0,.13,.19),.26,.085,'Edge',n=12);cyl('Beacon light',(0,.13,.25),.12,.05,'Gold')
  for s in [-1,1]:wing(s,[(.13,.17),(.62,.07),(.75,-.37),(.48,-.43),(.2,-.15)],.04,.075,'Gold');box('Array insulator',(s*.49,-.15,.14),(.16,.38,.025),'Dark')
 return name
def save(name,index):
 bpy.context.view_layer.update();bpy.ops.object.select_all(action='DESELECT')
 for o in collection.all_objects:o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False)
 dg=bpy.context.evaluated_depsgraph_get();tri=0
 for o in collection.all_objects:
  if o.type=='MESH':m=o.evaluated_get(dg).to_mesh();m.calc_loop_triangles();tri+=len(m.loop_triangles);o.evaluated_get(dg).to_mesh_clear()
 catalog[name]={'triangles':tri,'source_objects':len(collection.all_objects),'materials':sorted({m.name for o in collection.all_objects if o.type=='MESH' for m in o.data.materials})}
 for o in collection.all_objects:
  if not o.parent:o.location+=Vector(((index%4)*5,(index//4)*5,0))
names=['Drone','Interceptor','Gunship','CargoShuttle','DiveBomber','WardDrone','RailLancer','MineTender','DroneCarrier']
for name in names:
 if one and one!=name:continue
 craft(name);save(name,len(catalog))
for kind in range(4):
 name=['RepairPod114','OrdnanceRack114','ReactorCell114','WingBeacon114'][kind]
 if one and one!=name:continue
 pickup(kind);save(name,len(catalog))
# A studio view is for inspecting manufactured form; game screenshots remain the acceptance view.
bpy.ops.object.camera_add(location=(5,-8,12));camera=bpy.context.object;camera.rotation_euler=(Vector((0,0,0))-camera.location).to_track_quat('-Z','Y').to_euler();scene.camera=camera;camera.data.type='ORTHO';camera.data.ortho_scale=6 if one else 23
if not one:camera.location+=(Vector((7.5,7.5,0)));camera.rotation_euler=(Vector((7.5,7.5,0))-camera.location).to_track_quat('-Z','Y').to_euler()
for p,power,size in [((1,3,8),1300,7),((-5,-1,4),900,5),((4,-5,4),700,4)]:
 bpy.ops.object.light_add(type='AREA',location=p);lamp=bpy.context.object;lamp.data.energy=power;lamp.data.shape='DISK';lamp.data.size=size;lamp.rotation_euler=(Vector((0,0,0))-lamp.location).to_track_quat('-Z','Y').to_euler()
scene.world=bpy.data.worlds.new('Neutral studio');scene.world.color=(.13,.13,.13);scene.render.engine='CYCLES';scene.cycles.samples=20;scene.render.resolution_x=1100;scene.render.resolution_y=1100;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.render.filepath=str(OUT/('preview-'+(one or 'fleet')+'.png'))
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/('Foundry-'+(one or 'all')+'.blend')));bpy.ops.render.render(write_still=True)
(OUT/('catalog-'+(one or 'all')+'.json')).write_text(json.dumps(catalog,indent=2)+'\n')
print('SKYBREAK_FOUNDRY_COMPLETE '+json.dumps(catalog))
