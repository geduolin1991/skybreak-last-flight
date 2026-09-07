import os
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
source=open(os.path.join(ROOT,'Tools','build_assets.py')).read();exec(source.split("fighter('Kestrel'")[0])
def group(name,items,pivot):
 o=bpy.data.objects.new(name,None);bpy.context.collection.objects.link(o);o.location=pivot
 for item in items:
  bpy.context.view_layer.update();matrix=item.matrix_world.copy();item.parent=o;item.matrix_world=matrix
 return o
for variant,name in enumerate(['AstraFrame','CrimsonFrame','OracleFrame']):
 clear();hull=ivory;accent=[cyan,fire,purple][variant];secondary=[edge,red,dark][variant]
 box('Abdominal reactor',(0,0,1.8),(.55,.42,.5),dark,.1)
 box('Chest armor',(0,0,2.3),(1.05,.65,.7),hull,.15)
 shape('Chest collar',[(-.55,-.2),(.55,-.2),(.45,.27),(0,.48),(-.45,.27)],2.5,.18,secondary)
 ell('Core crystal',(0,.36,2.4),(.14,.055,.18),accent)
 box('Pelvis armor',(0,0,1.38),(.76,.47,.35),secondary,.08)
 box('Helmet',(0,.03,3),(.42,.4,.46),hull,.1)
 box('Visor',(0,.245,3.01),(.3,.04,.1),accent,.025)
 for sign in [-1,1]:
  a=box('V crest',(sign*.21,.16,3.19),(.055,.045,.48),hull,.015);a.rotation_euler[1]=sign*.65
  shoulder=box('Pauldron',(sign*.86,0,2.48),(.64,.72,.58),hull,.13)
  box('Shoulder ion slit',(sign*.87,.385,2.48),(.4,.028,.08),accent,.01)
  parts=[]
  parts.append(cyl('Shoulder bearing',(sign*.78,0,2.28),.19,.3,dark,'Z'))
  parts.append(box('Upper arm',(sign*.88,0,2.03),(.31,.33,.56),secondary,.07))
  parts.append(ell('Elbow',(sign*.9,.04,1.72),(.16,.15,.16),edge))
  parts.append(box('Forearm weapon',(sign*.94,.17,1.6),(.39,.47,.57),hull,.08))
  parts.append(cyl('Arm railgun',(sign*.96,.55,1.72),.11,.76,dark))
  parts.append(cyl('Arm aperture',(sign*.96,.96,1.72),.115,.055,accent))
  if variant==1:
   for k in [-1,1]:parts.append(cyl('Gatling barrel',(sign*.96+k*.12,.64,1.65),.062,1.03,edge,verts=12))
  group('Arm_L' if sign<0 else 'Arm_R',parts,(sign*.8,0,2.35))
  box('Thigh armor',(sign*.26,0,1.02),(.38,.42,.64),hull,.08)
  ell('Knee joint',(sign*.27,.04,.69),(.18,.19,.15),dark)
  box('Shin armor',(sign*.3,-.05,.37),(.34,.43,.58),secondary,.07)
  box('Shin light',(sign*.3,.17,.38),(.06,.03,.32),accent,.01)
  box('Flight boot',(sign*.31,.16,.06),(.36,.76,.22),hull,.07)
  cyl('Back thruster',(sign*.38,-.55,2),.2,.58,edge,'Z')
  cyl('Thruster glow',(sign*.38,-.55,1.68),.14,.06,accent,'Z')
  wing=[]
  points=[(sign*.45,-.35),(sign*2.25,-.62),(sign*2.4,-1.04),(sign*1.1,-.86),(sign*.45,-.65)]
  if sign<0:points.reverse()
  wing.append(shape('Flight wing',points,2.25,.16,hull))
  wing.append(box('Wing light',(sign*1.8,-.81,2.42),(.5,.11,.04),accent,.01))
  wing.append(box('Wing stabilizer',(sign*1.5,-.69,2.7),(.12,.44,.6),secondary,.03))
  group('Wing_L' if sign<0 else 'Wing_R',wing,(sign*.48,-.42,2.3))
 if variant==2:
  bpy.ops.mesh.primitive_torus_add(major_radius=.7,minor_radius=.03,major_segments=64,minor_segments=8,location=(0,-.48,3.3));finish(bpy.context.object,'Oracle halo',purple,0)
 bpy.ops.object.select_all(action='SELECT')
 bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,name+'.fbx'),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_space_transform=False,add_leaf_bones=False)
 bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'Tools',name+'.blend'))
 print('FRAME',name)
