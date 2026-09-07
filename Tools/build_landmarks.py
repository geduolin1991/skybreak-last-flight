"""Build the three chapter landmarks inside Blender through its MCP bridge.

Creates isolated scenes and writes their dependencies into individual .blend
files. Does not delete, overwrite or save the user's previously open scene.
"""
import bpy
import math
import json
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Assets/Art/Models'
PREVIEW = ROOT / 'Build/MCP-1.2/Assets'
PREVIEW.mkdir(parents=True, exist_ok=True)
OUT.mkdir(parents=True, exist_ok=True)


def material(name, color, metallic=.65, rough=.28, glow=0):
    # Own materials keep the model currently open in Blender untouched, and
    # avoid inheriting viewport-only materials with an unconfigured node tree.
    name = 'World_' + name
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    m.diffuse_color = (*color, 1)
    p = m.node_tree.nodes.get('Principled BSDF')
    p.inputs['Base Color'].default_value = (*color, 1)
    p.inputs['Metallic'].default_value = metallic
    p.inputs['Roughness'].default_value = rough
    p.inputs['Emission Color'].default_value = (*color, 1)
    p.inputs['Emission Strength'].default_value = glow
    return m


ivory = material('Ceramic_Ivory', (.65, .76, .79))
dark = material('Titanium_Dark', (.025, .046, .07), .85)
steel = material('Steel_Edge', (.16, .23, .28), .8)
orange = material('Safety_Orange', (.95, .24, .045))
cyan = material('Ion_Cyan', (.04, .75, 1), .2, .17, 4)
purple = material('Core_Violet', (.54, .055, 1), .2, .2, 4)
glass = material('Cockpit_Glass', (.015, .11, .18), .7, .12)


def finish(o, name, mat, bevel=.05):
    o.name = name
    o.data.materials.append(mat)
    if bevel:
        b = o.modifiers.new('Machined edges', 'BEVEL')
        b.width, b.segments = bevel, 3
        o.modifiers.new('Face weighted normals', 'WEIGHTED_NORMAL')
    return o


def box(name, p, size, mat, bevel=.05):
    bpy.ops.mesh.primitive_cube_add(size=1, location=p)
    o = bpy.context.object
    o.scale = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish(o, name, mat, bevel)


def cyl(name, p, radius, depth, mat, vertices=40):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=p)
    return finish(bpy.context.object, name, mat, .025)


def ring(name, p, radius, tube, mat):
    bpy.ops.mesh.primitive_torus_add(major_segments=64, minor_segments=8,
                                   major_radius=radius, minor_radius=tube, location=p)
    o = finish(bpy.context.object, name, mat, 0)
    for poly in o.data.polygons:
        poly.use_smooth = True
    return o


def hull(name, outline, z, height, mat):
    n = len(outline)
    vertices = [(x, y, z) for x, y in outline] + [(x, y, z+height) for x, y in outline]
    faces = [tuple(reversed(range(n))), tuple(range(n, n*2))]
    faces += [(i, (i+1) % n, (i+1) % n+n, i+n) for i in range(n)]
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    o = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(o)
    return finish(o, name, mat, .075)


def rod(name, a, b, radius, mat):
    a, b = Vector(a), Vector(b)
    o = cyl(name, (a+b)*.5, radius, (b-a).length, mat, 12)
    o.rotation_euler = (b-a).to_track_quat('Z', 'Y').to_euler()
    return o


def carrier():
    hull('Twin-keel rescue hull', [(-2.65,-6), (2.65,-6), (2.85,3.9), (1.85,6.3),
                                  (0,7), (-1.85,6.3), (-2.85,3.9)], -.6, 1.05, dark)
    hull('Beveled flight deck', [(-2.55,-5.8), (2.55,-5.8), (2.65,3.6),
                                (1.7,6), (0,6.55), (-1.7,6), (-2.65,3.6)], .47, .22, ivory)
    box('Recessed runway', (-.6, .1, .71), (2.7, 10.8, .035), steel, .015)
    for y in range(-5, 6):
        box('Deck center stripe', (-.6, y, .742), (.08, .48, .016), ivory, .006)
        for x in [-1.86, .66]:
            box('Recessed deck guide', (x, y, .745), (.055, .4, .023), cyan, .005)
    for s in [-1, 1]:
        box('Outer engine pod', (s*2.8, -2.1, -.1), (.7, 5.8, .82), steel, .18)
        for y in [-4.3, -3.6, -2.9, -2.2, -1.5]:
            box('Engine cooling fin', (s*3.17, y, -.08), (.15, .11, .67), dark, .012)
        for y in [-4.5, -1, 2.5, 4]:
            box('Rescue pod', (s*2.2, y, .98), (.44, .84, .46), orange, .13)
        for y in range(-5, 6, 2):
            rod('Deck safety rail', (s*2.45,y,.76), (s*2.45,y,1.04), .024, ivory)
        rod('Deck guard rail', (s*2.45,-5,1.04), (s*2.45,5,1.04), .022, steel)
    box('Island command base', (1.55, .5, 1.1), (.95, 3.1, .85), steel, .14)
    box('Bridge ceramic cap', (1.55, 1.15, 1.65), (1.2, 1.6, .46), ivory, .14)
    box('Wraparound bridge window', (1.55, 1.94, 1.63), (.92, .04, .2), glass, .025)
    cyl('Radar platform', (1.55,.2,2.1), .35,.1, steel)
    rod('Radar mast', (1.55,.2,1.6), (1.55,.2,3), .065, steel)
    box('Navigation radar', (1.55,.2,2.8), (1.1,.18,.23), ivory, .07)
    box('IFF light', (1.55,.2,3.06), (.12,.12,.12), cyan, .025)
    ring('Rescue pad', (-.62,-3.7,.765), .81,.023, orange)
    box('Rescue cross horizontal', (-.62,-3.7,.76), (.85,.16,.024), ivory,.008)
    box('Rescue cross vertical', (-.62,-3.7,.76), (.16,.85,.024), ivory,.008)


def relay():
    cyl('Relay foundation', (0,0,.18), 3.35,.7, dark, 12)
    cyl('Armored base rim', (0,0,.61), 3.12,.26, ivory, 12)
    cyl('Reactor well', (0,0,.86), 2.42,.33, steel)
    for r in [2.38,2.82]:
        ring('Induction coil', (0,0,1.08), r,.065, purple)
    cyl('Central housing', (0,0,1.68), 1.08,1.75,dark,12)
    cyl('Phase array crown', (0,0,2.72), 1.48,.3,ivory,12)
    cyl('Signal crystal', (0,0,2.87), .7,.32,purple,12)
    for i in range(6):
        a = i*math.pi/3
        x,y=math.cos(a),math.sin(a)
        support=box('Angled outer buttress',(x*2.45,y*2.45,1.16),(.56,1.55,1.85),steel,.12)
        support.rotation_euler[2]=a+math.pi/2
        rod('Power feed',(x*2.28,y*2.28,1.93),(x*1.14,y*1.14,2.4),.065,purple)
        cyl('Perimeter transmitter',(x*3.08,y*3.08,1),.22,1.15,ivory,16)
        cyl('Transmitter tip',(x*3.08,y*3.08,1.6),.13,.09,purple,16)
        for k in [-1,0,1]:
            vent=box('Cooling grille',(x*1.12-k*y*.17,y*1.12+k*x*.17,1.65),(.065,.08,.94),orange,.01)
            vent.rotation_euler[2]=a
    for a in [math.pi/4,5*math.pi/4]:
        x,y=math.cos(a)*1.42,math.sin(a)*1.42
        rod('Aerial spar',(x,y,2.65),(x,y,4.9),.06,ivory)
        ring('Aerial halo',(x,y,4.25),.49,.028,purple)


def gate():
    for r,z,t in [(5.6,0,.26),(4.72,.08,.14),(5.95,-.23,.09)]:
        ring('Orbital station pressure ring',(0,0,z),r,t,steel if r>5 else ivory)
    ring('Navigation circuit',(0,0,.25),5.55,.053,cyan)
    ring('Inner lock circuit',(0,0,.17),4.55,.04,purple)
    for i in range(8):
        a=i*math.pi/4
        x,y=math.cos(a),math.sin(a)
        rod('Radial truss',(x*4.68,y*4.68,0),(x*6.65,y*6.65,0),.13,steel)
        pod=box('Habitation pressure module',(x*5.58,y*5.58,.16),(.86,1.7,.83),ivory,.22)
        pod.rotation_euler[2]=a
        panel=box('Radiator wing',(x*7.22,y*7.22,-.08),(2.35,1.5,.06),glass,.025)
        panel.rotation_euler[2]=a
        for k in [-.65,0,.65]:
            p=box('Solar busbar',(x*(7.22+k),y*(7.22+k),-.032),(.035,1.4,.018),cyan,.004)
            p.rotation_euler[2]=a
        tip=box('Dock beacon',(x*4.55,y*4.55,.27),(.28,.13,.1),orange,.025)
        tip.rotation_euler[2]=a
    for s in [-1,1]:
        box('Docking spine',(0,s*6.22,-.12),(.8,2.7,.45),dark,.13)
        box('Docking collar',(0,s*7.61,-.1),(1.3,.3,.62),ivory,.12)


def setup_preview(scene, span):
    scene.render.engine='CYCLES'
    scene.cycles.samples=24
    scene.cycles.use_denoising=True
    scene.render.resolution_x=1000
    scene.render.resolution_y=800
    scene.render.resolution_percentage=100
    scene.world=bpy.data.worlds.new(scene.name+' atmosphere')
    scene.world.use_nodes=True
    scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.025,.045,.08,1)
    scene.world.node_tree.nodes['Background'].inputs[1].default_value=.4
    bpy.ops.object.camera_add(location=(span*.72,-span*.92,span*1.02))
    camera=bpy.context.object
    camera.rotation_euler=(Vector((0,0,.5))-camera.location).to_track_quat('-Z','Y').to_euler()
    camera.data.type='ORTHO';camera.data.ortho_scale=span
    scene.camera=camera
    for name,power,color,pos,size in [
        ('Cool key',1900,(.58,.79,1),(-7,-4,12),8),
        ('Amber rim',2600,(1,.53,.24),(8,6,9),6),
        ('Soft fill',1300,(.25,.65,1),(-7,6,4),7)]:
        data=bpy.data.lights.new(name,'AREA');data.energy=power;data.color=color;data.shape='DISK';data.size=size
        o=bpy.data.objects.new(name,data);scene.collection.objects.link(o);o.location=pos
        o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()


report=[]
for name,build,span in [('EvacCarrier',carrier,18),('StormRelay',relay,10),('OrbitalGate',gate,20)]:
    scene=bpy.data.scenes.new('SKYBREAK '+name)
    bpy.context.window.scene=scene
    build()
    meshes=[o for o in scene.objects if o.type=='MESH']
    bpy.ops.object.select_all(action='DESELECT')
    for o in meshes:o.select_set(True)
    bpy.context.view_layer.objects.active=meshes[0]
    bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,
        object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,
        bake_space_transform=True,add_leaf_bones=False,path_mode='AUTO')
    setup_preview(scene,span)
    bpy.data.libraries.write(str(ROOT/'Tools'/(name+'.blend')),{scene},fake_user=True)
    scene.render.filepath=str(PREVIEW/(name+'.png'))
    bpy.ops.render.render(write_still=True,scene=scene.name)
    evaluated=sum(len(o.evaluated_get(bpy.context.evaluated_depsgraph_get()).data.polygons) for o in meshes)
    report.append({'asset':name,'meshes':len(meshes),'evaluated_polygons':evaluated,
                   'fbx_bytes':(OUT/(name+'.fbx')).stat().st_size,'source':name+'.blend',
                   'preview':str(PREVIEW/(name+'.png'))})
(PREVIEW/'asset-report.json').write_text(json.dumps(report,indent=2))
result={'created':report,'original_scene_saved':False}
