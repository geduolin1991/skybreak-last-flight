"""Refine the volcanic coast and author a smooth orbital planet silhouette in Blender."""
import bpy,json,math
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.name='SKYBREAK 1.6 · coast and orbit'
bpy.ops.import_scene.fbx(filepath=str(ROOT/'Tools/ModelBaselines1.5/BasaltIsland.fbx'))
rock=list(bpy.context.selected_objects)
tex=bpy.data.textures.new('Fractured volcanic grain',type='CLOUDS');tex.noise_scale=.37;tex.noise_depth=2
for o in rock:
 if o.type!='MESH':continue
 o.name='Basalt erosion / '+o.name
 sub=o.modifiers.new('Rock surface tessellation','SUBSURF');sub.subdivision_type='SIMPLE';sub.levels=2;sub.render_levels=2
 dis=o.modifiers.new('Eroded fine stone','DISPLACE');dis.texture=tex;dis.strength=.11;dis.mid_level=.5
 for p in o.data.polygons:p.use_smooth=True
 m=bpy.data.materials.new('V16_Volcanic_Basalt');m.diffuse_color=(.13,.19,.20,1);o.data.materials.clear();o.data.materials.append(m)
bpy.ops.export_scene.fbx(filepath=str(ROOT/'Assets/Art/Models/BasaltIsland.fbx'),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',bake_space_transform=True,add_leaf_bones=False)
for o in rock:
 if not o.parent:o.location.x-=15
bpy.ops.object.select_all(action='DESELECT')
bpy.ops.mesh.primitive_uv_sphere_add(segments=128,ring_count=64,radius=.5);o=bpy.context.object;o.name='BluePlanet'
for f in o.data.polygons:f.use_smooth=True
m=bpy.data.materials.new('V16_Planet_Ocean');m.diffuse_color=(.015,.09,.18,1);o.data.materials.append(m)
bpy.ops.export_scene.fbx(filepath=str(ROOT/'Assets/Art/Models/BluePlanet.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_space_transform=True,add_leaf_bones=False)
bpy.data.libraries.write(str(ROOT/'Tools/Environment1.6.blend'),{scene},fake_user=True,compress=True)
p=ROOT/'Build/WorldWork/model-catalog.json';catalog=json.loads(p.read_text());catalog['BluePlanet']={'objects':1,'vertices':len(o.data.vertices)}
deps=bpy.context.evaluated_depsgraph_get();catalog['BasaltIsland']={'objects':len(rock),'vertices':sum(len(o.evaluated_get(deps).data.vertices) for o in rock if o.type=='MESH')};p.write_text(json.dumps(catalog,indent=2)+'\n')
print('ENVIRONMENT_COMPLETE',catalog['BasaltIsland'],catalog['BluePlanet'])
