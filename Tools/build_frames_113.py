"""Blender-authored, articulated original flight frames; Z up, +Y armour front.
Exports source .blend, FBX, and neutral studio previews. No downloaded assets.
"""
import bpy, bmesh, math, os, json
from mathutils import Vector
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
exec(open(os.path.join(ROOT,'Tools','build_assets.py')).read().split("fighter('Kestrel'")[0])
PREVIEW=os.path.join(ROOT,'Build','Frame113');os.makedirs(PREVIEW,exist_ok=True)
pearl=mat('Frame_Pearl',(.78,.86,.88),.48,.28);carbon=mat('Frame_Carbon',(.018,.032,.052),.72,.32)
steel=mat('Frame_Steel',(.20,.30,.37),.8,.24);blue=mat('Frame_Blue',(.027,.19,.30),.66,.31)
crimson=mat('Frame_Crimson',(.47,.048,.035),.60,.31);silver=mat('Frame_Silver',(.44,.50,.69),.63,.27)
brass=mat('Frame_Brass',(.63,.37,.12),.72,.3)
ice=mat('Frame_Ice',(.08,.75,.94),.35,.22,1.5);ember=mat('Frame_Ember',(1,.38,.075),.35,.24,1.5);iris=mat('Frame_Iris',(.60,.46,1),.35,.22,1.5)

def assembly(name,pivot,fn):
 before=set(bpy.data.objects);fn();items=set(bpy.data.objects)-before
 root=bpy.data.objects.new('Motion_'+name,None);bpy.context.collection.objects.link(root);root.location=pivot
 bpy.context.view_layer.update()
 for item in items:
  if item.parent is None:
   matrix=item.matrix_world.copy();item.parent=root;item.matrix_world=matrix
 return root

def fix_normals(mesh):
 # X/Z armour plates may be mirrored: explicit outward normals are required
 # for Unity backface culling, even though Blender previews shade both sides.
 bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
 if bm.calc_volume(signed=True)<0:bmesh.ops.reverse_faces(bm,faces=list(bm.faces))
 bm.to_mesh(mesh);bm.free();mesh.update()

def plate(name,outline,y,depth,material,bevel=.025):
 n=len(outline);cx=sum(x for x,z in outline)/n;cz=sum(z for x,z in outline)/n
 verts=[(x,y,z) for x,z in outline]+[(cx+(x-cx)*.88,y+depth,cz+(z-cz)*.94) for x,z in outline]
 faces=[tuple(reversed(range(n))),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();fix_normals(mesh)
 obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);return finish(obj,name,material,bevel)

def armor(name,x,z,w,h,y,depth,material):
 return plate(name,[(x-w*.42,z-h*.5),(x+w*.30,z-h*.5),(x+w*.5,z-h*.13),(x+w*.36,z+h*.42),(x-w*.20,z+h*.5),(x-w*.5,z+h*.18)],y,depth,material)

def strut(name,a,b,r,material):
 d=Vector(b)-Vector(a);obj=cyl(name,(Vector(a)+Vector(b))*.5,r,d.length,material,'Z',12);obj.rotation_euler=d.to_track_quat('Z','Y').to_euler();return obj

def ring(name,pos,r,material,start=0,end=math.tau):
 n=max(8,int((end-start)*12));verts=[];faces=[]
 for i in range(n+1):
  a=start+(end-start)*i/n
  for rad,dep in [(r-.055,-.035),(r+.055,-.035),(r+.055,.035),(r-.055,.035)]:verts.append((pos[0]+math.cos(a)*rad,pos[1]+dep,pos[2]+math.sin(a)*rad))
 for i in range(n):
  for j in range(4):faces.append((i*4+j,i*4+(j+1)%4,(i+1)*4+(j+1)%4,(i+1)*4+j))
 faces.extend([(3,2,1,0),(n*4,n*4+1,n*4+2,n*4+3)])
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();fix_normals(mesh);o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);return finish(o,name,material,.008)

def slots(x,y,z,count,width):
 for i in range(count):box('Recessed cooling louvre',(x,y,z+i*.10),(width,.028,.042),carbon,.006)

def nozzle(x,y,z,r,accent):
 cyl('Engine titanium lip',(x,y,z),r,.32,steel,'Z',20);cyl('Exhaust recess',(x,y,z-.17),r*.78,.025,carbon,'Z',20);cyl('Engine throat',(x,y,z-.19),r*.5,.025,accent,'Z',16)

def stamp(text,x,y,z,size,material):
 bpy.ops.object.text_add(location=(x,y,z),rotation=(math.pi/2,0,math.pi));o=bpy.context.object;o.data.body=text;o.data.align_x='CENTER';o.data.size=size;o.data.extrude=.001;o.data.materials.append(material);bpy.ops.object.convert(target='MESH');o.name='Hull stencil '+text

def leg(sign,variant):
 x=sign*(.38 if variant!=1 else .60);hull=[pearl,crimson,silver][variant];accent=[ice,ember,iris][variant];width=[.42,.67,.31][variant]
 def make():
  strut('Hip actuator',(x,-.10,1.65),(x*1.2,-.17,.91),.12 if variant!=1 else .18,steel)
  armor('Thigh tapered shell',x,1.23,width,.70,.0,.23,hull);cyl('Knee bearing',(x*1.15,.03,.79),width*.40,.35,carbon,'Y',16)
  armor('Floating knee cap',x*1.15,.80,width*.95,.30,.17,.18,pearl);armor('Split greave',x*1.25,.37,width*.96,.65,-.12,.34,hull)
  box('Shin seam',(x*1.25,.27,.38),(.045,.02,.33),accent,.006)
  plate('Flight toe',[(x*1.25-width*.45,-.02),(x*1.25+width*.45,-.02),(x*1.25+width*.30,.16),(x*1.25-width*.3,.24)],.10,.38,pearl)
  nozzle(x*1.25,-.30,.30,width*.38,accent)
  if variant==1:slots(x*1.25,.30,.28,3,width*.60)
 assembly('Leg_'+('L' if sign<0 else 'R'),(x,0,1.65),make)

def torso(v):
 hull=[pearl,crimson,silver][v];accent=[ice,ember,iris][v];w=[1.06,1.62,.86][v]
 armor('Reactor keel',0,2.05,w*.53,.95,-.3,.38,carbon)
 if v==0:
  plate('Falcon breastplate',[(-.58,2.9),(0,3.00),(.58,2.9),(.40,2.52),(0,2.18),(-.40,2.52)],-.17,.62,pearl)
  plate('Blue flight keel',[(-.30,2.72),(0,2.85),(.30,2.72),(0,2.32)],.47,.038,blue)
  plate('Reactor lens',[(-.10,2.61),(.10,2.61),(0,2.36)],.52,.026,ice,.008)
  for side in [-1,1]:
   armor('Chest segmented intake',side*.42,2.68,.23,.33,.43,.06,steel);slots(side*.42,.507,2.61,3,.14)
 elif v==1:
  armor('Fortress reactor casing',0,2.60,1.70,.75,-.28,.74,carbon)
  for side in [-1,1]:
   plate('Overlapping siege breastplate',[(side*.06,2.92),(side*.82,2.98),(side*.85,2.49),(side*.31,2.34)],.35,.20,crimson)
   armor('Chest ceramic insert',side*.50,2.68,.52,.14,.57,.03,pearl)
   slots(side*.47,.578,2.48,2,.43)
  cyl('Recessed round furnace',(0,.52,2.53),.16,.10,steel,'Y',20);cyl('Furnace iris',(0,.58,2.53),.10,.025,ember,'Y',20)
 else:
  plate('Oracle split cowl',[(-.54,3.05),(-.14,2.89),(-.08,2.36),(-.32,2.16),(-.48,2.56)],.01,.36,pearl)
  plate('Oracle split cowl R',[(.54,3.05),(.14,2.89),(.08,2.36),(.32,2.16),(.48,2.56)],.01,.36,silver)
  for z in [2.32,2.51,2.70]:
   cyl('Exposed superconducting spine',(0,.19,z),.13,.12,steel,'Z',16);box('Spine phase light',(0,.34,z),(.06,.025,.07),iris,.006)
 for s in [-1,1]:
  armor('Segmented rib',s*w*.27,2.13,w*.28,.34,-.02,.26,steel);strut('Waist piston',(s*.22,.20,2.23),(s*.24,.18,1.65),.037,brass)
  plate('Floating hip skirt',[(s*.07,1.8),(s*.62,1.82),(s*.78,1.3),(s*.25,1.4)] if s>0 else [(s*.25,1.4),(s*.78,1.3),(s*.62,1.82),(s*.07,1.8)],-.02,.24,hull);nozzle(s*.32,-.55,2.08,.23 if v!=1 else .30,accent)
 armor('Pelvic lock',0,1.70,.57,.40,-.18,.47,carbon);stamp(['07','12','03'][v],-.28 if v==2 else 0,.60 if v==1 else .49,2.79,.09,pearl if v==1 else steel)

def surface_detail(v):
 before=set(bpy.data.objects)
 # Layered service plates and fasteners read as manufactured armour up close.
 for side in [-1,1]:
  for z in [.28,1.18,1.78]:
   x=side*(.49 if v!=1 else .74)
   for dx in [-.10,.10]:cyl('Recessed fastener',(x+dx,.295,z),.021,.013,steel,'Y',8)
  for z in [1.57,1.72]:
   strut('Hip hydraulic rod',(side*.37,-.20,z),(side*.66,-.14,z-.20),.030,brass)
  for k in range(3):
   armor('Finger knuckle',side*(1.04 if v==0 else 1.37 if v==1 else .87)+(k-1)*.07,1.42,.057,.13,.13,.10,carbon)
  armor('Rear calf fin',side*(.57 if v!=1 else .88),.43,.16,.64,-.31,.07,blue if v==0 else crimson if v==1 else pearl)

 bpy.context.view_layer.update()
 for item in set(bpy.data.objects)-before:
  label=item.name;side='L' if item.location.x<0 else 'R'
  joint='Arm_' if label.startswith('Finger knuckle') else 'Leg_' if label.startswith('Rear calf fin') or label.startswith('Recessed fastener') and item.location.z<1.4 else None
  if joint:
   matrix=item.matrix_world.copy();item.parent=bpy.data.objects.get('Motion_'+joint+side);item.matrix_world=matrix

def head(v):
 hull=[pearl,crimson,silver][v];cyl('Neck gimbal',(0,0,3.01),.14,.18,steel,'Z',16);armor('Helmet crown',0,3.28,.48 if v!=1 else .61,.48,-.13,.36,hull)
 plate('Recessed face mask',[(-.20,3.35),(.20,3.35),(.13,3.08),(0,3.01),(-.13,3.08)],.24,.06,carbon)
 if v==1:
  cyl('Cyclops targeting glass',(0,.325,3.24),.078,.035,ember,'Y',20);armor('Raised blast collar',0,2.99,.90,.22,.04,.31,crimson)
 elif v==0:
  plate('Paired optical slit',[(-.18,3.30),(0,3.24),(.18,3.30),(0,3.18)],.32,.015,ice,.003)
  for s in [-1,1]:plate('Falcon swept antenna',[(s*.08,3.40),(s*.48,3.80),(s*.29,3.35)],-.01,.065,pearl,.01)
 else:
  plate('Diamond optic',[(0,3.40),(.12,3.24),(0,3.07),(-.12,3.24)],.33,.02,iris,.003);plate('Telemetry crest',[(-.05,3.42),(.06,3.94),(.15,3.38)],-.13,.12,pearl,.01)

def astra():
 torso(0);head(0)
 for s in [-1,1]:
  leg(s,0)
  def arm():
   armor('Aerodynamic pauldron',s*.89,2.70,.67,.49,-.04,.35,pearl);strut('Upper arm linkage',(s*.78,0,2.60),(s*.97,.04,2.03),.13,carbon)
   armor('Arm front guard',s*.97,2.18,.36,.52,.09,.26,blue);cyl('Elbow bearing',(s*1.0,.04,1.94),.14,.26,steel,'Y',16);armor('Wrist gauntlet',s*1.04,1.74,.43,.55,.09,.32,pearl)
   if s==1:
    plate('Skyrazor blade frame',[(.99,1.73),(1.39,2.34),(1.92,3.99),(1.79,2.24),(1.25,1.57)],.10,.16,blue);plate('Luminous cutting edge',[(1.40,2.37),(1.92,3.99),(1.79,2.35),(1.48,1.94)],.29,.025,ice,.008)
   else:
    plate('Aegis arm shield',[(-1.04,2.08),(-1.66,2.45),(-1.76,1.49),(-1.36,1.15),(-.94,1.64)],.24,.21,blue)
    for i in range(2):cyl('Pulse muzzle',(-1.19-i*.22,.40,2.48),.074,.78,steel,'Z',16)
  assembly('Arm_'+('L' if s<0 else 'R'),(s*.65,0,2.73),arm)
  def wing():
   for i in range(3):
    x=.61+i*.36;tip=3.35-i*.22;top=3.32-i*.61;points=[(s*x,2.48-i*.24),(s*tip,top),(s*(tip-.48),top-.60),(s*(x+.32),1.66-i*.2)]
    plate('Vector feather',points if s>0 else list(reversed(points)),-.50-i*.065,.14,pearl if i!=1 else blue)
    strut('Feather ion rail',(s*(x+.34),-.30-i*.065,2.37-i*.22),(s*(tip-.40),-.30-i*.065,top-.20),.020,ice)
  assembly('Wing_'+('L' if s<0 else 'R'),(s*.55,-.38,2.51),wing)

def crimson_frame():
 torso(1);head(1)
 for s in [-1,1]:
  leg(s,1);armor('Citadel shoulder',s*1.14,2.61,1.00,.82,-.35,.73,crimson);armor('Ceramic strip',s*1.16,2.72,.83,.16,.39,.045,pearl);slots(s*1.15,.415,2.43,3,.54);stamp('B-12',s*1.18,.44,2.77,.095,pearl)
  def cannon():
   armor('Siege breech',s*1.12,3.05,.59,.75,-.38,.55,carbon)
   for dx in [-.15,.15]:
    cyl('Recoil shroud',(s*1.12+dx,-.15,3.52),.13,1.40,steel,'Z',16);cyl('Muzzle brake',(s*1.12+dx,-.15,4.20),.17,.22,crimson,'Z',16);cyl('Bored muzzle',(s*1.12+dx,-.15,4.325),.12,.016,carbon,'Z',16);cyl('Hot aperture',(s*1.12+dx,-.15,4.337),.065,.017,ember,'Z',16)
   box('Heat strip',(s*1.12,.147,3.24),(.07,.025,.43),ember,.005)
  assembly('Cannon_'+('L' if s<0 else 'R'),(s*1.12,-.19,2.99),cannon)
  def arm():
   strut('Heavy elbow',(s*1.16,0,2.36),(s*1.36,.09,1.62),.19,steel);armor('Forearm shield',s*1.37,1.71,.60,.72,.04,.34,crimson)
   if s<0:
    plate('Tower buckler',[(-1.05,2.01),(-1.84,2.34),(-2.06,1.55),(-1.88,.94),(-1.21,1.19)],.40,.22,pearl);slots(-1.63,.635,1.51,4,.46)
   else:
    cyl('Gatling stator',(1.52,.29,1.80),.30,.69,carbon,'Z',20)
    for j in range(5):
     a=j*math.tau/5;cyl('Gatling barrel',(1.52+math.cos(a)*.18,.29+math.sin(a)*.18,2.29),.055,.70,steel,'Z',12)
    cyl('Barrel retaining ring',(1.52,.29,2.48),.26,.07,crimson,'Z',20)
  assembly('Arm_'+('L' if s<0 else 'R'),(s*.96,0,2.5),arm)
  def rack():
   armor('Deployable missile rack',s*2.02,2.60,.62,.85,-.50,.39,carbon)
   for x in [-.14,.14]:
    for z in [-.19,.02,.23]:cyl('Missile well',(s*2.02+x,-.074,2.57+z),.085,.05,steel,'Y',12);cyl('Missile nose',(s*2.02+x,-.042,2.57+z),.052,.024,ember,'Y',12)
  assembly('Rack_'+('L' if s<0 else 'R'),(s*1.55,-.40,2.65),rack)

def oracle():
 torso(2);head(2)
 for s in [-1,1]:
  leg(s,2)
  def arm():
   armor('Needle shoulder cap',s*.73,2.74,.44,.43,-.12,.31,pearl);strut('Manipulator',(s*.67,-.02,2.64),(s*.86,.07,1.80),.095,steel);armor('Precision bracer',s*.87,1.98,.29,.66,.03,.29,silver)
   if s>0:
    armor('Rifle receiver',1.11,2.05,.43,1.07,.12,.32,carbon)
    for dx in [-.17,.17]:
     plate('Split accelerator rail',[(1.1+dx-.065,2.37),(1.1+dx+.065,2.37),(1.1+dx+.10,4.50),(1.1+dx-.06,4.17)],.10,.18,pearl);box('Induction stripe',(1.1+dx,.31,3.52),(.034,.02,1.36),iris,.005)
    for z in [2.55,2.84,3.13]:box('Magnetic rail collar',(1.10,.14,z),(.58,.28,.055),steel,.012)
    cyl('Optic scope',(1.46,.16,2.72),.075,.52,carbon,'Z',16)
   else:ring('Navigation gyroscope',(-1.1,.30,1.87),.32,steel);cyl('Navigation glass',(-1.1,.32,1.87),.17,.075,iris,'Y',20)
  assembly('Arm_'+('L' if s<0 else 'R'),(s*.58,0,2.74),arm)
  for i in range(3):
   def petal():
    x=s*(1.58+i*.35);z=3.23-i*.88;plate('Forked prism drone',[(x-s*.19,z-.44),(x+s*.03,z-.55),(x+s*.19,z+.10),(x+s*.39,z+.49),(x+s*.10,z+.73)],-.58,.20,pearl);plate('Drone graphite inset',[(x,z-.25),(x+s*.07,z-.24),(x+s*.25,z+.35),(x+s*.09,z+.38)],-.35,.025,carbon);strut('Prism ion edge',(x,-.34,z-.28),(x+s*.20,-.34,z+.40),.024,iris);cyl('Drone lens',(x+s*.06,-.32,z+.02),.073,.04,iris,'Y',12)
   assembly('Petal_'+('L' if s<0 else 'R')+str(i),(s*(1.58+i*.35),-.58,3.23-i*.88),petal)
 def halo():
  for a,b in [(0.12,1.43),(1.70,2.99),(3.25,4.56),(4.83,6.11)]:ring('Parallax crown',(0,-.66,3.23),1.08,silver,a,b)
  for j in range(8):
   a=j*math.tau/8;box('Crown phase marker',(math.cos(a)*1.08,-.60,3.23+math.sin(a)*1.08),(.055,.05,.055),iris,.007)
 assembly('Halo',(0,-.66,3.23),halo)

def export_frame(name):
 for obj in list(bpy.context.scene.objects):
  if obj.parent is None:obj.location.z-=2.10
 bpy.context.view_layer.update();bpy.ops.object.select_all(action='SELECT');bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,name+'.fbx'),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=False,add_leaf_bones=False);bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'Tools',name+'.blend'))

def render(name):
 scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.render.resolution_x=1120;scene.render.resolution_y=1120;scene.render.resolution_percentage=100;scene.world.color=(.045,.065,.09);scene.view_settings.view_transform='AgX'
 bpy.ops.object.camera_add(location=(5.8,11.8,5.0));camera=bpy.context.object;camera.rotation_euler=(Vector((0,0,.15))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=8.2;scene.camera=camera
 for label,pos,power,size,col in [('Key',(1,7,8),1500,6,(.78,.90,1)),('Rim',(-5,-4,5),1900,5,(.18,.65,1)),('Fill',(5,1,1),600,4,(1,.53,.30))]:
  bpy.ops.object.light_add(type='AREA',location=pos);o=bpy.context.object;o.name=label;o.data.energy=power;o.data.shape='DISK';o.data.size=size;o.data.color=col;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
 scene.render.image_settings.file_format='PNG';scene.render.filepath=os.path.join(PREVIEW,name+'.png');bpy.ops.render.render(write_still=True)

stats=[]
for name,build in [('AstraFrame',astra),('CrimsonFrame',crimson_frame),('OracleFrame',oracle)]:
 clear();build();surface_detail(['AstraFrame','CrimsonFrame','OracleFrame'].index(name));bpy.context.view_layer.update();deps=bpy.context.evaluated_depsgraph_get();triangles=0
 for o in bpy.context.scene.objects:
  if o.type=='MESH':
   obj=o.evaluated_get(deps);mesh=obj.to_mesh();mesh.calc_loop_triangles();triangles+=len(mesh.loop_triangles);obj.to_mesh_clear()
 stats.append(dict(name=name,triangles=triangles,moving_assemblies=sum(o.name.startswith('Motion_') for o in bpy.context.scene.objects)));export_frame(name);render(name)
open(os.path.join(PREVIEW,'geometry.json'),'w').write(json.dumps(stats,indent=2)+'\n');print('SKYBREAK_FRAMES_113_EXPORTED',stats)
