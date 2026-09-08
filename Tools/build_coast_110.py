"""Original coastal scenery and tileable wave data for SKYBREAK 1.10.
Run in Blender: --background --python Tools/build_coast_110.py.
No downloaded assets or external textures. FBX geometry is baked by Unity per material.
"""
from pathlib import Path
import bpy, math, random, json
import numpy as np
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.name='SKYBREAK · Dawn coast 1.10'
rng=random.Random(1100);catalog={};materials={}
palette={'Coast_Chalk':(.54,.59,.56),'Coast_Stone':(.18,.24,.27),'Coast_Sand':(.62,.59,.44),'Coast_Foliage':(.12,.28,.17),'Coast_Navy':(.06,.14,.22),'Coast_Teal':(.10,.32,.36),'Coast_Rust':(.48,.22,.12),'Coast_Glass':(.09,.25,.33),'Coast_Ivory':(.72,.76,.71)}
for name,color in palette.items():
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);materials[name]=m
current=None

def begin(name):
 global current
 current=bpy.data.collections.new(name);scene.collection.children.link(current)
 bpy.context.view_layer.active_layer_collection=bpy.context.view_layer.layer_collection.children[name]

def finish(o,name,mat,bevel=0):
 o.name=name;o.data.materials.append(materials[mat])
 if bevel:
  mod=o.modifiers.new('Soft manufactured edge','BEVEL');mod.width=bevel;mod.segments=2
  o.modifiers.new('Weighted corner normals','WEIGHTED_NORMAL')
 return o

def box(name,p,s,mat='Coast_Chalk',bevel=.03):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.scale=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return finish(o,name,mat,bevel)

def cyl(name,p,r,h,mat='Coast_Navy',n=16):
 bpy.ops.mesh.primitive_cylinder_add(vertices=n,radius=r,depth=h,location=p);return finish(bpy.context.object,name,mat,.012)

def beam(name,a,b,r=.035,mat='Coast_Navy'):
 a,b=Vector(a),Vector(b);o=cyl(name,(a+b)*.5,r,(b-a).length,mat,10);o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();return o

def plate(name,points,z,h,mat):
 n=len(points);v=[(x,y,z+dz) for dz in (0,h) for x,y in points];f=[tuple(reversed(range(n))),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(v,[],f);mesh.update();o=bpy.data.objects.new(name,mesh);current.objects.link(o);return finish(o,name,mat,.03)

def rock(name,p,s,mat='Coast_Stone',sub=2):
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub,radius=1,location=p);o=bpy.context.object
 for v in o.data.vertices:v.co*=rng.uniform(.88,1.12)
 o.scale=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return finish(o,name,mat)

def export(name):
 bpy.ops.object.select_all(action='DESELECT')
 for o in current.objects:o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=str(ROOT/'Assets/Art/Models'/f'{name}.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_space_transform=True,add_leaf_bones=False)
 deps=bpy.context.evaluated_depsgraph_get();catalog[name]={'objects':len(current.objects),'vertices':sum(len(o.evaluated_get(deps).data.vertices) for o in current.objects)}
 for o in current.objects:o.location.x+=len(catalog)*14

begin('DawnHarbor')
outline=[(-4,-5.7),(-2.8,-6.1),(3.6,-6.1),(4.2,-5.3),(4.2,4.9),(2.6,5.8),(-3.4,5.8),(-4.2,4.2)]
plate('Weathered seawall',outline,-.55,.85,'Coast_Stone')
plate('Pale inset quay',[(x*.96,y*.98) for x,y in outline],.3,.14,'Coast_Chalk')
# Paved lanes and utility trenches bring the quay to a human scale.
box('Service lane',(1.7,0,.455),(1.3,10,.02),'Coast_Navy',0)
for j in range(10):
 y=-4.7+j*.98;box('Road center paint',(1.7,y,.473),(.06,.42,.008),'Coast_Ivory',0)
 box('Concrete expansion joint',(-.9,y,.455),(4.8,.024,.008),'Coast_Stone',0)
for side in (-1,1):
 for j in range(11):
  y=-5+j;beam('Quay rail post',(side*3.9,y,.46),(side*3.9,y,.91),.024)
  if j%2==0:cyl('Mooring cleat',(side*3.7,y,.51),.09,.23,'Coast_Rust',12)
 beam('Quay safety railing',(side*3.9,-5,.88),(side*3.9,5,.88),.024,'Coast_Ivory')
 for j in range(8):
  y=-4.7+j*1.3;box('Recessed rubber fender',(side*4.12,y,.09),(.14,.38,.42),'Coast_Navy')
# Staggered terracotta/blue-green freight containers, with corrugations and locks.
for i,(x,y,z) in enumerate([(-2.1,-3.5,.47),(-.3,-3.5,.47),(-2.1,-1.1,.47),(-2.1,-3.5,1.47)]):
 mat='Coast_Teal' if i%2==0 else 'Coast_Rust';box('Freight container',(x,y,z+.47),(1.48,2.05,.92),mat,.045)
 for k in range(9):box('Corrugated roof rib',(x-.63+k*.157,y,z+.943),(.038,1.96,.025),'Coast_Chalk',.008)
 for side in (-1,1):beam('Container locking bar',(x+side*.48,y-1.035,z+.12),(x+side*.48,y-1.035,z+.81),.021,'Coast_Ivory')
# Ferry terminal: asymmetric curved canopy, glass hall and rooftop plant.
box('Ferry terminal foundation',(-1.6,2.5,.6),(3.9,3.6,.32),'Coast_Chalk',.12)
box('Terminal glass hall',(-1.6,2.5,1.2),(3.45,2.75,1.0),'Coast_Glass',.05)
for x in (-3.15,-2.4,-1.6,-.8,-.05):
 beam('Terminal window mullion',(x,1.08,.74),(x,1.08,1.72),.032,'Coast_Ivory')
for j in range(10):
 x=-3.6+j*.44;box('Canopy folded panel',(x,2.45,1.92+.16*math.cos(j*.34)),(.47,3.55,.12),'Coast_Ivory',.025)
for y in (3.4,4.15):box('Roof ventilation plant',(-1.6,y,2.09),(1.1,.45,.18),'Coast_Navy')
# Open lattice gantry and suspended cable, much finer than solid blocking.
for x in (2.7,3.65):
 for y in (-.3,2.0):
  beam('Gantry leg',(x,y,.5),(x,y,3.0),.055,'Coast_Rust')
 for j in range(3):beam('Diagonal gantry brace',(x,-.3,.55+j*.78),(x,2,1.3+j*.78),.024,'Coast_Ivory')
beam('Crane outreach',(.4,.9,3.1),(4.6,.9,3.1),.09,'Coast_Rust')
beam('Hoist cable',(.8,.9,3.1),(.8,.9,1.45),.012)
cyl('Hoist pulley',(.8,.9,1.45),.08,.13,'Coast_Navy',12)
for y in (-5.2,5.2):
 cyl('Dock light mast',(3.3,y,1.5),.035,2.05,'Coast_Ivory',12);box('Dock light',(3.3,y,2.54),(.29,.15,.1),'Coast_Ivory')
export('DawnHarbor')

begin('DawnCove')
# An irregular terraced islet with a submerged rock skirt, sandy ledges,
# vegetation clusters and a lighthouse. Silhouette is continuous, not cube rubble.
angles=np.arange(32)*math.tau/32
radius=[1+.11*math.sin(a*3+.4)+.08*math.sin(a*7) for a in angles]
for ring,(sx,sy,z,h,mat) in enumerate([(4.3,6,-.58,.60,'Coast_Stone'),(3.95,5.7,.02,.17,'Coast_Sand'),(3.45,4.9,.19,.52,'Coast_Stone'),(2.85,4.15,.71,.21,'Coast_Foliage')]):
 points=[(math.cos(a)*sx*r,math.sin(a)*sy*r) for a,r in zip(angles,radius)];plate('Eroded shoreline terrace '+str(ring),points,z,h,mat)
for i in range(26):
 a=i*2.399;rr=rng.uniform(.72,1.05);rock('Salt-weathered shore boulder',(math.cos(a)*3.6*rr,math.sin(a)*5.3*rr,.2),(rng.uniform(.35,.8),rng.uniform(.4,.9),rng.uniform(.25,.65)))
for i in range(15):
 a=i*2.399;r=math.sqrt(i)*.60
 rock('Coastal scrub',(math.cos(a)*r,math.sin(a)*r*1.3,.99),(rng.uniform(.4,.75),rng.uniform(.4,.8),rng.uniform(.22,.45)),'Coast_Foliage')
cyl('Lighthouse stepped foundation',(0,2.35,1.05),.8,.3,'Coast_Chalk',32)
cyl('Lighthouse tapered tower',(0,2.35,2.09),.43,1.85,'Coast_Ivory',24)
for z in (1.57,2.22):cyl('Lighthouse painted band',(0,2.35,z),.44,.21,'Coast_Rust',24)
cyl('Lighthouse gallery',(0,2.35,3.04),.65,.12,'Coast_Chalk',32)
cyl('Lighthouse lantern',(0,2.35,3.31),.38,.43,'Coast_Glass',24)
for i in range(8):
 a=i*math.tau/8;beam('Lantern frame',(math.cos(a)*.4,2.35+math.sin(a)*.4,3.08),(math.cos(a)*.4,2.35+math.sin(a)*.4,3.55),.022,'Coast_Ivory')
bpy.ops.mesh.primitive_cone_add(vertices=24,radius1=.59,radius2=.07,depth=.38,location=(0,2.35,3.69));finish(bpy.context.object,'Lantern roof','Coast_Rust')
for j in range(10):box('Footpath stair',(0,.3+j*.15,.94+j*.011),(.64,.145,.065),'Coast_Sand',.015)
export('DawnCove')

# Periodic multi-directional wave spectrum. Two gradient channels and two
# independent height scales pack into one mipmapped RGBA texture.
size=512;y,x=np.mgrid[0:size,0:size].astype(np.float32)/size
np_rng=np.random.default_rng(1100)
def field(low,high,count):
 h=np.zeros((size,size),np.float32)
 for _ in range(count):
  kx,ky=np_rng.integers(-high,high+1,2)
  if not low<=math.hypot(kx,ky)<=high:continue
  phase=np_rng.uniform(0,math.tau);h+=np.sin(math.tau*(x*kx+y*ky)+phase)/max(1,math.hypot(kx,ky))
 return h/max(.01,float(h.std()))
h=field(5,26,180);gx=(np.roll(h,-1,1)-np.roll(h,1,1));gy=(np.roll(h,-1,0)-np.roll(h,1,0))
norm=max(float(gx.std()),float(gy.std()))*3.4
packed=np.stack([np.clip(.5+gx/norm*.5,0,1),np.clip(.5+gy/norm*.5,0,1),np.clip(.5+h*.16,0,1),np.clip(.5+field(1,5,45)*.17,0,1)],axis=-1)
image=bpy.data.images.new('CoastalWaveSpectrum',width=size,height=size,alpha=True)
image.colorspace_settings.name='Non-Color';image.pixels.foreach_set(packed.reshape(-1));image.filepath_raw=str(ROOT/'Assets/Resources/World/CoastalWaveSpectrum.png');image.file_format='PNG'
Path(image.filepath_raw).parent.mkdir(parents=True,exist_ok=True);image.save()
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Tools/Coast1.10.blend'),compress=True)
report=ROOT/'Build/Verify1.10.0';report.mkdir(parents=True,exist_ok=True);(report/'coast-models.json').write_text(json.dumps(catalog,indent=2)+'\n')
print('SKYBREAK_COAST_110',json.dumps(catalog))
