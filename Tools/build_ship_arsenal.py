"""Original 1.3 airframes, authored in isolated Blender scene via local MCP."""
import bpy, math, json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Assets/Art/Models'
scene=bpy.data.scenes.new('SKYBREAK 1.3 Airframe Design');bpy.context.window.scene=scene
# Reuse manufacturing primitives; never execute the older scene deletion/build.
source=(ROOT/'Tools/build_assets.py').read_text()
exec(source[source.index('def mat('):source.index('def clear():')])
# Reuse canonical material names in exported FBX, avoiding .001 aliases.
for name,var in [('Ceramic_Ivory','ivory'),('Titanium_Dark','dark'),('Steel_Edge','edge'),('Enemy_Red','red'),('Safety_Orange','gold'),('Ion_Cyan','cyan'),('Reactor_Orange','fire'),('Cockpit_Glass','glass'),('Core_Violet','purple')]:
 globals()[var]=bpy.data.materials.get(name)
def mirrored(name,pts,z,t,m,s):return shape(name,[(s*x,y) for x,y in (pts if s>0 else reversed(pts))],z,t,m)
def begin(name):
 c=bpy.data.collections.new(name);scene.collection.children.link(c);bpy.context.view_layer.active_layer_collection=bpy.context.view_layer.layer_collection.children[c.name];return c
def save(name,c):
 bpy.ops.object.select_all(action='DESELECT')
 for o in c.objects:o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False)
 for o in c.objects:o.location.x += {'Kestrel':-6,'Manta':0,'Needle':6}[name]
# Kestrel: swept arrow, exposed twin nacelles, dark-blue panels, forked tail.
c=begin('Kestrel')
shape('Kestrel keel',[(-.36,-1.5),(.36,-1.5),(.47,.1),(0,2.08),(-.47,.1)],-.13,.37,dark)
shape('Pointed upper armor',[(-.3,-1.25),(.3,-1.25),(.25,.8),(0,2.0),(-.25,.8)],.24,.13,ivory)
ell('Kestrel canopy',(0,.48,.48),(.24,.65,.19),glass)
for s in [-1,1]:
 mirrored('Swept falcon wing',[(.28,.9),(1.88,-.52),(1.7,-1.14),(.57,-.66),(.32,-1.37)],.01,.16,ivory,s)
 mirrored('Blue wing inset',[(.48,.39),(1.66,-.55),(1.42,-.64),(.54,-.19)],.18,.018,edge,s)
 mirrored('Cyan identification stripe',[(.53,.3),(1.63,-.52),(1.57,-.59),(.53,.17)],.21,.018,cyan,s)
 box('Vented nacelle',(s*.61,-.8,.12),(.39,1.24,.4),dark,.08)
 cyl('Engine shroud',(s*.61,-1.48,.14),.2,.2,edge);cyl('Blue nozzle',(s*.61,-1.6,.14),.135,.06,cyan)
 for j in range(5):box('Heat exchanger',(s*.61,-.38-j*.18,.34),(.28,.042,.024),edge,.009)
 box('Cannon receiver',(s*.85,.3,.04),(.2,.67,.18),edge)
 cyl('Twin autocannon',(s*.85,.8,.04),.067,.6,dark);cyl('Cyan muzzle',(s*.85,1.11,.04),.08,.04,cyan)
 fin=box('Canted tail',(s*.6,-1.12,.64),(.1,.72,.57),ivory);fin.rotation_euler[1]=s*-.25
save('Kestrel',c)
# Manta: wide crescent flying wing, armored pods, recessed rotary guns and missile cells.
c=begin('Manta')
shape('Broad manta armor',[(-.43,1.22),(0,1.55),(.43,1.22),(1.3,.62),(2.65,.38),(2.45,-1.15),(1.22,-1.47),(.43,-.93),(0,-1.28),(-.43,-.93),(-1.22,-1.47),(-2.45,-1.15),(-2.65,.38),(-1.3,.62)],-.13,.33,dark)
shape('Central command shell',[(-.53,-1.1),(.53,-1.1),(.65,.55),(0,1.5),(-.65,.55)],.2,.26,edge)
ell('Low armored canopy',(0,.66,.54),(.36,.42,.15),glass)
for s in [-1,1]:
 mirrored('Ochre wing armor',[(.68,.61),(2.44,.28),(2.2,-.73),(1.2,-1.0),(.69,-.6)],.21,.13,gold,s)
 mirrored('Wing ivory rim',[(1.25,.55),(2.51,.33),(2.46,.13),(1.2,.36)],.36,.09,ivory,s)
 box('Heavy engine',(s*1.68,-.86,.09),(.6,1.0,.53),edge,.09)
 cyl('Armored thruster',(s*1.68,-1.43,.14),.29,.26,dark);cyl('Amber exhaust',(s*1.68,-1.6,.14),.2,.055,fire)
 box('Missile cassette',(s*1.82,-.2,.5),(.65,1.0,.35),dark,.07)
 for j in range(3):
  for k in [-1,1]:cyl('Hex missile cell',(s*1.82+k*.15,.08-j*.27,.7),.098,.045,gold,'Z',6)
 cyl('Rotary gun housing',(s*.91,.5,.08),.23,.75,edge)
 for k in range(4):
  a=k*math.pi/2;cyl('Rotary barrel',(s*.91+math.cos(a)*.115,1.08,.08+math.sin(a)*.115),.053,.65,dark,verts=12)
 box('Gun shield',(s*.91,.67,.35),(.53,.6,.13),ivory)
 box('Amber fin marker',(s*2.39,-.38,.41),(.08,.5,.04),fire,.015)
save('Manta',c)
# Needle: long split rail spine, tiny canards, floating superconducting housings.
c=begin('Needle')
shape('Needle central spar',[(-.23,-1.85),(.23,-1.85),(.29,.65),(0,2.77),(-.29,.65)],-.12,.31,dark)
shape('White dorsal shell',[(-.26,-1.7),(.26,-1.7),(.34,-.4),(0,1.12),(-.34,-.4)],.2,.22,ivory)
ell('Navigator canopy',(0,-.35,.48),(.22,.64,.18),glass)
for s in [-1,1]:
 mirrored('Forward electromagnetic rail',[(.13,.4),(.4,.65),(.4,2.43),(.2,2.92),(.13,2.6)],.05,.25,edge,s)
 box('Purple rail conductor',(s*.205,1.68,.31),(.065,2.03,.04),purple,.012)
 for j in range(6):box('Rail cooling tooth',(s*.42,.8+j*.26,.16),(.11,.1,.13),ivory,.012)
 mirrored('Backward stabilizer',[(.2,-.55),(1.42,-1.72),(1.35,-2.02),(.39,-1.5)],.0,.13,ivory,s)
 mirrored('Minimal forward canard',[(.27,.55),(.98,.0),(.81,-.28),(.3,.02)],.0,.08,edge,s)
 box('Superconductor engine',(s*.64,-1.15,.1),(.37,1.05,.34),dark,.07)
 cyl('Violet nozzle',(s*.64,-1.75,.14),.135,.14,purple)
 for j in range(4):box('Capacitor light',(s*.64,-.77-j*.2,.3),(.2,.055,.035),purple,.014)
 box('Wingtip transceiver',(s*1.35,-1.78,.24),(.12,.34,.23),edge)
save('Needle',c)
# Product inspection camera and lighting.
bpy.context.view_layer.active_layer_collection=bpy.context.view_layer.layer_collection
bpy.ops.object.camera_add(location=(0,12,22));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,0))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=19;scene.camera=cam
for loc,power,size in [((2,3,12),2100,8),((-8,-3,8),1500,7),((7,0,6),1000,5)]:
 bpy.ops.object.light_add(type='AREA',location=loc);l=bpy.context.object;l.data.energy=power;l.data.shape='DISK';l.data.size=size;l.rotation_euler=(Vector((0,0,0))-l.location).to_track_quat('-Z','Y').to_euler()
scene.world=bpy.data.worlds.new('Airframe studio');scene.world.color=(.14,.17,.23);scene.render.engine='CYCLES';scene.cycles.samples=32;scene.render.resolution_x=1600;scene.render.resolution_y=720;scene.render.resolution_percentage=100
scene.render.filepath=str(ROOT/'Build/MCP-1.3/airframes-blender.png');bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Tools/ShipArsenalWorkshop.blend'),copy=True)
result={'scene':scene.name,'exports':['Kestrel','Manta','Needle'],'render':scene.render.filepath,'objects':len(scene.objects)}
