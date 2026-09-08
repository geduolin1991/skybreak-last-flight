"""Author three small, original environment sets for SKYBREAK 1.9.
Run with Blender --background --python Tools/build_environment_19.py.
The previous production scenes and models are preserved.
"""
from pathlib import Path
import bpy, math, random, json
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.name='SKYBREAK 1.9 · Environmental storytelling'
random.seed(1909);catalog={};materials={}
palette={'City_Concrete':(.24,.29,.32),'Steel_Edge':(.16,.23,.28),'Titanium_Dark':(.025,.046,.07),'Ceramic_Ivory':(.65,.76,.79),'Park_Green':(.08,.23,.16),'Rescue_Gold':(.95,.65,.15),'Ion_Cyan':(.04,.75,1),'City_Window':(.16,.38,.5),'Safety_Orange':(.95,.24,.045)}
for name,color in palette.items():
 mat=bpy.data.materials.new(name);mat.diffuse_color=(*color,1);materials[name]=mat
current=None

def begin(name):
 global current
 current=bpy.data.collections.new(name);scene.collection.children.link(current)
 bpy.context.view_layer.active_layer_collection=bpy.context.view_layer.layer_collection.children[name]

def finish(obj,name,material,bevel=0):
 obj.name=name;obj.data.materials.append(materials[material])
 if bevel:
  b=obj.modifiers.new('Machined edge bevel','BEVEL');b.width=bevel;b.segments=2
  obj.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
 return obj

def box(name,p,s,material='City_Concrete',bevel=.04):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.scale=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return finish(o,name,material,bevel)

def cylinder(name,p,r,d,material='Steel_Edge',vertices=20):
 bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=d,location=p);return finish(bpy.context.object,name,material,.015)

def beam(name,a,b,r=.04,material='Steel_Edge'):
 a,b=Vector(a),Vector(b);o=cylinder(name,(a+b)*.5,r,(b-a).length,material,12);o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();return o

def export(name):
 bpy.ops.object.select_all(action='DESELECT')
 for o in current.objects:o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=str(ROOT/'Assets/Art/Models'/f'{name}.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_space_transform=True,add_leaf_bones=False)
 deps=bpy.context.evaluated_depsgraph_get();catalog[name]={'objects':len(current.objects),'vertices':sum(len(o.evaluated_get(deps).data.vertices) for o in current.objects)}
 # Move only the saved authoring scene display; exports remain centered.
 for o in current.objects:o.location.x+=len(catalog)*12

begin('CoastalBreakwater')
box('Wave-cut foundation',(0,0,.12),(2.8,5.4,.44),bevel=.18)
for side in (-1,1):
 for j in range(7):
  y=-2.3+j*.74;o=box('Sloped wave armor',(side*1.3,y,.25),(.70,.64,.70),bevel=.09);o.rotation_euler.y=side*.48
  cylinder('Mooring bollard',(side*.92,y,.5),.09,.24)
box('Safety walkway',(0,0,.39),(.95,5,.1),'Ceramic_Ivory')
for j in range(9):box('Rescue route stripe',(0,-2.2+j*.55,.452),(.7,.08,.018),'Rescue_Gold',0)
for y in (-2.15,2.15):
 cylinder('Navigation beacon plinth',(0,y,.7),.27,.5)
 cylinder('Beacon mast',(0,y,1.25),.055,.75)
 cylinder('Caged navigation lamp',(0,y,1.64),.13,.15,'City_Window')
for side in (-1,1):beam('Walkway handrail',(side*.58,-2.4,.79),(side*.58,2.4,.79),.025)
export('CoastalBreakwater')

begin('CivicPlaza')
box('Inset plaza paving',(0,0,.015),(5.4,4.5,.12),'City_Concrete',.1)
for i in range(7):box('Paving joint',(-2.3+i*.77,0,.082),(.018,4.1,.006),'Titanium_Dark',0)
cylinder('Circular fountain surround',(0,0,.19),.90,.25,'Ceramic_Ivory',40)
cylinder('Recessed reflecting basin',(0,0,.32),.71,.025,'City_Window',40)
cylinder('Fountain sculpture',(0,0,.65),.14,.62,'Steel_Edge',24)
for x in (-1.9,1.9):
 for y in (-1.45,1.45):
  box('Raised planter',(x,y,.23),(.92,.88,.40),'Ceramic_Ivory',.10)
  box('Garden soil',(x,y,.445),(.78,.74,.025),'Titanium_Dark',0)
  cylinder('Tree trunk',(x,y,.81),.07,.80,'Steel_Edge',12)
  for dz,radius in ((0,.48),(.25,.40),(.5,.27)):
   bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=radius,location=(x,y,1.23+dz));finish(bpy.context.object,'Sculpted tree canopy','Park_Green')
 for y in (-.75,.75):
  for j in range(4):box('Bench timber slat',(x+j*.065,y,.35),(.045,.86,.08),'Rescue_Gold',.012)
  for z in (-.30,.30):beam('Bench leg',(x+.10,y+z,.08),(x+.10,y+z,.30),.035)
for x in (-.95,.95):
 for y in (-1.8,1.8):
  cylinder('Path lamp',(x,y,.40),.06,.7,'Steel_Edge',12);cylinder('Powered lamp cap',(x,y,.79),.09,.11,'City_Window',16)
export('CivicPlaza')

begin('OrbitalTruss')
for y in (-2.8,2.8):
 for x in (-.7,.7):beam('Longitudinal lattice beam',(x,y,-.3),(x,-y,-.3),.08)
for j in range(6):
 y=-2.8+j*1.12;beam('Diagonal pressure bracing',(-.7,y,-.3),(.7,y+1.12,-.3),.045)
for side in (-1,1):
 beam('Solar array support',(0,0,-.3),(side*2.6,0,-.3),.085)
 for y in (-1.1,1.1):
  box('Solar panel frame',(side*2,y,-.27),(2.1,2,.12),'Ceramic_Ivory',.045)
  box('Photovoltaic glass',(side*2,y,-.195),(1.92,1.82,.025),'Titanium_Dark',.01)
  for x in range(7):box('Solar cell divider',(side*2-.82+x*.27,y,-.173),(.018,1.78,.01),'Ion_Cyan',0)
  for row in range(4):box('Solar bus conductor',(side*2,y-.68+row*.45,-.16),(1.9,.013,.01),'Steel_Edge',0)
cylinder('Pressurized hub',(0,0,0),.62,.52,'Ceramic_Ivory',32)
cylinder('Coupling ring',(0,0,.30),.49,.10,'Rescue_Gold',32)
for y in (-2.5,2.5):box('Dock navigation chevron',(0,y,-.15),(.8,.2,.06),'City_Window')
export('OrbitalTruss')
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Tools/Environment1.9.blend'),compress=True)
(ROOT/'Build/Verify1.9/environment-models.json').write_text(json.dumps(catalog,indent=2)+'\n')
print('SKYBREAK_ENVIRONMENT_19',json.dumps(catalog))
