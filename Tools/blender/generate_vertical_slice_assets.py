import bpy
import math
import os
import sys


def clear_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for material in list(bpy.data.materials):
        bpy.data.materials.remove(material)


def material(name, color):
    value = bpy.data.materials.new(name)
    value.diffuse_color = (*color, 1.0)
    value.roughness = 0.85
    return value


def uv_sphere(name, location, scale, mat, segments=16, rings=8):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    obj.data.materials.append(mat)
    bpy.ops.object.shade_smooth()
    return obj


def cone(name, location, scale, rotation, mat, vertices=4):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=1, radius2=0, depth=2, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    obj.data.materials.append(mat)
    return obj


def export_selected(path):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y', add_leaf_bones=False)


def build_child(output_dir):
    clear_scene()
    skin = material('Skin', (0.88, 0.62, 0.42))
    cream = material('Tunic', (0.92, 0.84, 0.62))
    blue = material('Cape', (0.12, 0.42, 0.62))
    brown = material('Hair', (0.20, 0.09, 0.04))
    boot = material('Boots', (0.24, 0.13, 0.07))
    root = bpy.data.objects.new('Hero_Root', None)
    bpy.context.collection.objects.link(root)
    parts = [
        uv_sphere('Head', (0, 0, 1.65), (0.43, 0.40, 0.44), skin),
        uv_sphere('Hair', (0, 0.02, 1.82), (0.45, 0.39, 0.29), brown),
        uv_sphere('Body', (0, 0, 0.88), (0.39, 0.30, 0.58), cream),
        uv_sphere('Boot_L', (-0.20, -0.02, 0.18), (0.18, 0.28, 0.16), boot),
        uv_sphere('Boot_R', (0.20, -0.02, 0.18), (0.18, 0.28, 0.16), boot),
    ]
    bpy.ops.mesh.primitive_cube_add(location=(0, 0.25, 0.96), scale=(0.38, 0.05, 0.46))
    cape = bpy.context.object
    cape.name = 'Cape'
    cape.data.materials.append(blue)
    cape.rotation_euler = (math.radians(-8), 0, 0)
    parts.append(cape)
    for part in parts:
        part.parent = root
    bpy.ops.object.select_all(action='DESELECT')
    root.select_set(True)
    for part in parts:
        part.select_set(True)
    export_selected(os.path.join(output_dir, 'BI_Hero_Child.fbx'))


def build_fish(output_dir, filename, body_color, body_scale, tail_scale, predator=False):
    clear_scene()
    body_mat = material('Body', body_color)
    pale = material('Belly', tuple(min(1, c * 1.35) for c in body_color))
    dark = material('Eyes', (0.02, 0.04, 0.05))
    root = bpy.data.objects.new('Fish_Root', None)
    bpy.context.collection.objects.link(root)
    body = uv_sphere('Body', (0, 0, 0), body_scale, body_mat)
    tail = cone('Tail', (body_scale[0] + tail_scale[0] * 0.6, 0, 0), tail_scale, (0, math.radians(90), 0), body_mat, 3)
    fin = cone('DorsalFin', (0, 0, body_scale[2] * .8), (body_scale[0] * .35, body_scale[1] * .15, body_scale[2] * .5), (0, 0, 0), body_mat, 3)
    belly = uv_sphere('Belly', (-body_scale[0] * .2, 0, -body_scale[2] * .2), (body_scale[0] * .65, body_scale[1] * 1.01, body_scale[2] * .55), pale)
    eye_l = uv_sphere('Eye_L', (-body_scale[0] * .72, -body_scale[1] * .78, body_scale[2] * .28), (.055, .035, .055), dark, 12, 6)
    eye_r = uv_sphere('Eye_R', (-body_scale[0] * .72, body_scale[1] * .78, body_scale[2] * .28), (.055, .035, .055), dark, 12, 6)
    parts = [body, tail, fin, belly, eye_l, eye_r]
    if predator:
        snout = cone('RoundedSnout', (-body_scale[0] * 1.05, 0, 0), (.35, body_scale[1] * .8, body_scale[2] * .65), (0, math.radians(-90), 0), body_mat, 12)
        parts.append(snout)
    for part in parts:
        part.parent = root
    bpy.ops.object.select_all(action='DESELECT')
    root.select_set(True)
    for part in parts:
        part.select_set(True)
    export_selected(os.path.join(output_dir, filename))


def main():
    separator = sys.argv.index('--') if '--' in sys.argv else len(sys.argv) - 1
    output_dir = os.path.abspath(sys.argv[separator + 1])
    build_child(output_dir)
    specs = [
        ('BI_Fish_Glimmer.fbx', (0.28, 0.82, 0.78), (0.78, 0.30, 0.36), (0.30, 0.10, 0.36), False),
        ('BI_Fish_Cloud.fbx', (0.95, 0.69, 0.25), (0.58, 0.48, 0.52), (0.24, 0.10, 0.34), False),
        ('BI_Fish_Coral.fbx', (0.95, 0.34, 0.28), (0.76, 0.26, 0.48), (0.30, 0.10, 0.42), False),
        ('BI_Fish_MoonRay.fbx', (0.38, 0.38, 0.82), (0.82, 0.62, 0.18), (0.34, 0.20, 0.30), False),
        ('BI_Fish_Pearl.fbx', (0.88, 0.46, 0.70), (0.38, 0.24, 0.66), (0.22, 0.09, 0.28), False),
        ('BI_Predator_Breaker.fbx', (0.12, 0.42, 0.54), (1.35, 0.52, 0.54), (0.48, 0.15, 0.58), True),
    ]
    for spec in specs:
        build_fish(output_dir, *spec)
    print(f'Generated {len(specs) + 1} commercial-source FBX assets in {output_dir}')


if __name__ == '__main__':
    main()
