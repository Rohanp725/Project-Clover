---
name: blender-prop
description: Build or fix a 3D prop or kit piece in Blender through the blender MCP server with scripted bpy, bake its maps, export FBX to staging and render a check image. Use when the user asks for a model, a mesh fix, LODs or a texture bake.
---

# Blender prop

Needs Blender open with the MCP for Blender add-on's server started (N panel > MCP for Blender > Start MCP Server). If the
`blender` MCP tools are missing or fail to connect, say so and stop; do not install anything.

Safe mode is on: scripts may not read or write files directly, run programs or use the network. Use Blender's operators
(`bpy.ops.wm.save_as_mainfile`, `bpy.ops.export_scene.fbx`, `bpy.ops.render.render(write_still=True)`) for every file action.

1. **Brief first**: dimensions in metres, silhouette references from `docs/GAME.md`, triangle budget and texture size for
   the asset class, which LODs, whether it is modular (snap size). Write the brief into `staging/blender/<date>_<name>/notes.md`.
2. **Scene**: `get_scene_info`; start from a clean file (`bpy.ops.wm.read_homefile(use_empty=True)`), units metric, scale 1.
3. **Build with code**, one short `execute_blender_code` step at a time: primitives, modifiers (bevel, boolean, array,
   mirror), apply scale and rotation, origin at the base centre, name objects `<name>_lod0`. Check after each step with
   `get_object_info` or a viewport screenshot; do not pile ten operations into one script.
4. **Topology and UVs**: triangulate (modifier, applied on export), remove doubles, mark seams or Smart UV Project with
   margin 0.02, pack islands, consistent texel density. LODs: duplicate `_lod0`, Decimate (collapse) to ~50% and ~25%,
   parent all to an empty named `<name>`.
5. **Materials**: one Principled BSDF per material slot, textures named `<name>_<map>`. Bake albedo, normal (OpenGL, Unity
   flips on import as a normal map), roughness, metallic and AO to `<size>` square images via the bake operator; no lighting
   in the bake. Palette or dithering rules come from `docs/GAME.md`.
6. **Save and export**: save the `.blend` to `Assets/_Project/Art/Source/<name>.blend` (a Unity-ignored source, LFS);
   export FBX to `staging/blender/<date>_<name>/<name>.fbx` with `axis_forward='-Z'`, `axis_up='Y'`, `apply_scale_options='FBX_SCALE_ALL'`,
   `use_mesh_modifiers=True`, `mesh_smooth_type='FACE'`, `add_leaf_bones=False`, selected objects only; save baked images as PNG
   beside it through the image save operator.
7. **Check image**: set a camera framing the prop, render a 1024 px still to `staging/.../check.png`, Read it and describe
   it. Show the user; only after approval run the asset-intake skill.
8. **Stop** the add-on server when the session's Blender work is done, or ask the user to.

Report: the brief, triangle counts per LOD, texture sizes, the paths written, and the check image.
