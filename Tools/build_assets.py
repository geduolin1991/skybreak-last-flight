import bpy, math, random, os
from mathutils import Vector
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT=os.path.join(ROOT,'Assets','Art','Models')
random.seed(17)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def mat(name,color,metal=.5,rough=.32,emission=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
 p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=rough
 p.inputs['Emission Color'].default_value=(*color,1);p.inputs['Emission Strength'].default_value=emission
 return m
ivory=mat('Ceramic_Ivory',(.65,.76,.79));dark=mat('Titanium_Dark',(.025,.046,.07),.85);edge=mat('Steel_Edge',(.16,.23,.28),.8);red=mat('Enemy_Red',(.34,.032,.019));gold=mat('Safety_Orange',(.95,.24,.045));cyan=mat('Ion_Cyan',(.04,.75,1),.2,.17,4);fire=mat('Reactor_Orange',(1,.12,.015),.1,.24,5);glass=mat('Cockpit_Glass',(.015,.11,.18),.7,.12);purple=mat('Core_Violet',(.54,.055,1),.2,.2,5)
def finish(o,name,m,bevel=.035):
 o.name=name;o.data.materials.append(m)
 if bevel:
  mod=o.modifiers.new('Manufactured edge radius','BEVEL');mod.width=bevel;mod.segments=2
  mod=o.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
 return o
def box(name,pos,size,m,bev=.035):
 bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.scale=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return finish(o,name,m,bev)
def ell(name,pos,size,m):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=20,ring_count=10,location=pos);o=bpy.context.object;o.scale=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return finish(o,name,m,0)
def cyl(name,pos,r,depth,m,axis='Y',verts=24):
 bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=r,depth=depth,location=pos);o=bpy.context.object
 if axis=='Y':o.rotation_euler[0]=math.pi/2
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return finish(o,name,m,.025)
def shape(name,points,z,thick,m):
 n=len(points);v=[(x,y,z) for x,y in points]+[(x,y,z+thick) for x,y in points];faces=[tuple(reversed(range(n))),tuple(range(n,n*2))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(v,[],faces);mesh.update();o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);return finish(o,name,m)
def clear():
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def export(name):
 bpy.ops.object.select_all(action='SELECT')
 bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,name+'.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False,path_mode='AUTO')
 bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'Tools',name+'.blend'))
 print('EXPORTED',name)
def fighter(name,variant=0,enemy=False):
 clear();hull=red if enemy else ivory;glow=fire if enemy else cyan;span=[1.9,2.3,1.5][variant]
 shape('Tapered titanium fuselage',[(-.35,-1.4),(.35,-1.4),(.46,-.2),(.2,1.6),(0,2),(-.2,1.6),(-.46,-.2)],-.1,.35,dark)
 shape('Upper ceramic spine',[(-.26,-1.1),(.26,-1.1),(.32,.4),(0,1.9),(-.32,.4)],.22,.13,hull)
 ell('Smoked canopy',(0,.48,.4),(.22,.58,.18),glass)
 for s in [-1,1]:
  pts=[(s*.25,.8),(s*span,-.75),(s*(span-.08),-1.2),(s*.75,-.72),(s*.4,-1.35)]
  if s<0:pts.reverse()
  shape('Swept main wing',pts,.02,.13,hull)
  shape('Wing accent',[(s*.5,.35),(s*(span-.2),-.76),(s*(span-.4),-.81),(s*.5,.08)] if s>0 else [(s*.5,.08),(s*(span-.4),-.81),(s*(span-.2),-.76),(s*.5,.35)],.17,.018,gold if not enemy else edge)
  box('Engine nacelle',(s*.56,-.82,.14),(.42,1.3,.38),edge,.08)
  cyl('Exhaust shroud',(s*.56,-1.5,.14),.19,.26,dark)
  cyl('Engine aperture',(s*.56,-1.65,.14),.13,.035,glow)
  cyl('Wing cannon',(s*1.1,-.15,.0),.065,1.1,dark,verts=12)
  cyl('Muzzle luminous collar',(s*1.1,.4,.0),.077,.06,glow,verts=12)
  fin=shape('Vertical stabilizer',[(s*.47,-1.3),(s*.58,-1.3),(s*.58,-.6),(s*.47,-.6)],.25,.52,hull)
  fin.rotation_euler[1]=s*-.16
  box('Navigation light',(s*(span-.13),-.97,.18),(.11,.2,.055),glow,.02)
  for i in range(4):box('Engine vent',(s*.56,-.6-i*.14,.345),(.28,.042,.03),dark,.01)
  if variant==1:
   box('Missile rack',(s*1.22,-.7,-.18),(.32,.8,.23),edge)
   for k in [-1,1]:cyl('Micro missile',(s*1.22+k*.09,-.52,-.27),.062,.83,gold,verts=12)
  if variant==2:
   shape('Forward canard',[(s*.18,.95),(s*.9,.4),(s*.8,.16),(s*.3,.45)] if s>0 else [(s*.3,.45),(s*.8,.16),(s*.9,.4),(s*.18,.95)],.08,.08,edge)
 box('Rescue chevron',(0,-.55,.38),(.07,.4,.03),glow,.01)
 export(name)
def drone():
 clear();ell('Armored lens',(0,0,0),(.46,.65,.3),red);ell('Target sensor',(0,.42,.19),(.25,.2,.09),fire)
 for s in [-1,1]:
  shape('Blade',[(s*.23,.55),(s*1.1,.1),(s*.9,-.5),(s*.24,-.35)] if s>0 else [(s*.24,-.35),(s*.9,-.5),(s*1.1,.1),(s*.23,.55)],0,.1,dark)
  cyl('Thruster',(s*.52,-.46,.02),.1,.2,fire)
 export('Drone')
def boss(name,v):
 clear();glow=[fire,cyan,purple][v];hull=[edge,red,dark][v]
 if v==1:
  # A broad, swept storm platform, with exposed twin turbine wells.
  shape('Storm crescent',[(-5.4,-1.9),(-5.15,.5),(-3.7,1.85),(-1.25,1.25),(0,2.9),(1.25,1.25),(3.7,1.85),(5.15,.5),(5.4,-1.9),(3.8,-.9),(1.1,-1.4),(0,-2.35),(-1.1,-1.4),(-3.8,-.9)],-.32,.66,red)
  shape('Diamond bridge',[(-.8,-1.8),(.8,-1.8),(1.05,.8),(0,2.9),(-1.05,.8)],.34,.55,edge)
  shape('Command glass',[(-.36,.8),(.36,.8),(0,2.05)],.91,.08,glass)
  cyl('Storm heart',(0,-.35,.98),.48,.12,cyan,'Z')
  for s in [-1,1]:
   cyl('Turbine armored drum',(s*3.1,-.05,.36),1.34,.75,dark,'Z',48)
   cyl('Recessed turbine well',(s*3.1,-.05,.78),1.12,.08,edge,'Z',48)
   for radius in [1.08,1.3]:
    bpy.ops.mesh.primitive_torus_add(major_radius=radius,minor_radius=.055,major_segments=64,minor_segments=8,location=(s*3.1,-.05,.84));finish(bpy.context.object,'Turbine ion ring',cyan,0)
   cyl('Axle cap',(s*3.1,-.05,.89),.34,.2,red,'Z')
   for j in range(8):
    a=j*math.pi/4
    blade=box('Turbine blade',(s*3.1+math.cos(a)*.7,-.05+math.sin(a)*.7,.88),(.58,.17,.07),ivory,.02);blade.rotation_euler[2]=a+.45
   shape('Forward stabilizer',[(s*1.35,.5),(s*2.05,2.75),(s*2.55,2.92),(s*2.52,.65)] if s>0 else [(s*2.52,.65),(s*2.55,2.92),(s*2.05,2.75),(s*1.35,.5)],.15,.26,ivory)
   box('Missile magazine',(s*4.58,-.38,.21),(.62,2.1,.48),dark,.1)
   for j in range(5):box('Magazine seam',(s*4.58,-1+j*.36,.48),(.43,.06,.04),gold,.01)
   for x in [.12,-.12]:
    cyl('Storm railgun',(s*1.55+x,1.12,.59),.1,2.4,edge)
    cyl('Railgun aperture',(s*1.55+x,2.34,.59),.11,.07,cyan)
   for x in [-.28,.28]:cyl('Rear turbine jet',(s*3.1+x,-1.42,.12),.2,.35,fire)
  export(name);return
 if v==2:
  # A radial orbital seraph, its six long feathers framing the exposed core.
  cyl('Hexagonal core vessel',(0,0,.05),1.53,.65,dark,'Z',6)
  cyl('Core armored crown',(0,0,.45),1.24,.26,ivory,'Z',12)
  cyl('Core recess',(0,0,.63),.94,.14,dark,'Z',48)
  ell('Violet singularity',(0,0,.78),(.67,.67,.31),purple)
  for radius,z in [(.88,.87),(1.7,.18),(2,.07)]:
   bpy.ops.mesh.primitive_torus_add(major_radius=radius,minor_radius=.065,major_segments=96,minor_segments=8,location=(0,0,z));finish(bpy.context.object,'Orbital conduit',purple,0)
  for j in range(6):
   a=j*math.pi/3+math.pi/6
   def turn(points):return [(x*math.cos(a)-y*math.sin(a),x*math.sin(a)+y*math.cos(a)) for x,y in points]
   shape('Seraph feather '+str(j),turn([(1.32,-.29),(2.4,-.61),(4.7,-.37),(5.25,0),(4.4,.46),(2.3,.56),(1.32,.29)]),-.12,.34,ivory)
   shape('Inset feather panel',turn([(2.2,-.28),(4.25,-.2),(4.73,0),(4.15,.2),(2.2,.24)]),.23,.08,edge)
   shape('Ion filament',turn([(1.6,-.05),(4.4,-.04),(4.72,0),(4.4,.04),(1.6,.05)]),.32,.02,purple)
   p=turn([(2.15,0)])[0];cyl('Feather gimbal',(p[0],p[1],.29),.24,.24,dark,'Z')
   p=turn([(3.8,0)])[0];cyl('Satellite emitter',(p[0],p[1],.39),.16,.2,purple,'Z')
  for s in [-1,1]:
   shape('Core forward fang',[(s*.38,1),(s*.75,2.5),(s*.25,3.22),(s*.13,1.2)] if s>0 else [(s*.13,1.2),(s*.25,3.22),(s*.75,2.5),(s*.38,1)],.17,.24,dark)
  export(name);return
 shape('Command hull',[(-1.4,-2),(1.4,-2),(1.8,.5),(.85,2.5),(0,3),(-.85,2.5),(-1.8,.5)],-.25,.85,hull)
 shape('Dorsal armor',[(-.8,-1.8),(.8,-1.8),(.9,1.1),(0,2.4),(-.9,1.1)],.6,.18,ivory if v==0 else edge)
 cyl('Reactor housing',(0,-.25,.8),.67,.25,dark,'Z')
 cyl('Exposed core',(0,-.25,.97),.47,.11,glow,'Z')
 for s in [-1,1]:
  pts=[(s*.9,1.6),(s*3.7,.4),(s*4,-.7),(s*3.6,-2),(s*1.2,-1.4)]
  if s<0:pts.reverse()
  shape('Armored carrier wing',pts,-.1,.55,hull)
  box('Side reactor',(s*2.6,-.65,.48),(1.1,2.5,.55),dark,.15)
  box('Armor panel',(s*2.6,-.55,.78),(.93,1.5,.2),edge,.09)
  for j in range(6):box('Reactor slats',(s*2.6,-1.2+j*.28,.92),(.68,.08,.06),glow,.01)
  for y in [-1.1,.3,1.3]:
   cyl('Rotary turret',(s*1.35,y,.77),.3,.3,edge,'Z');cyl('Weapon barrel',(s*1.35,y+.4,.98),.1,.8,dark);cyl('Barrel muzzle',(s*1.35,y+.8,.98),.13,.08,glow)
  for k in [-1,0,1]:cyl('Heavy exhaust',(s*2.6+k*.25,-1.96,.3),.13,.22,glow)
  if v>0:
   box('Outer siege pylon',(s*3.6,.25,.4),(.6,3.8,.6),red if v==1 else ivory,.15)
   cyl('Rail cannon',(s*3.6,2.2,.5),.18,1.8,dark)
   cyl('Rail aperture',(s*3.6,3.11,.5),.19,.06,glow)
 if v==2:
  for r in [1,1.3]:
   bpy.ops.mesh.primitive_torus_add(major_radius=r,minor_radius=.055,major_segments=64,minor_segments=8,location=(0,-.25,1.15));finish(bpy.context.object,'Core halo',glow,0)
 export(name)
fighter('Kestrel',0);fighter('Manta',1);fighter('Needle',2);fighter('Interceptor',2,True);fighter('Gunship',1,True);drone();boss('Leviathan',0);boss('Tempest',1);boss('Seraph',2)
