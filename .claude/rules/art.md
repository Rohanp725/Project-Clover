---
paths:
  - "Assets/_Project/Art/**"
  - "staging/**"
  - "Assets/_Project/ASSETS.csv"
---

# Art: models, textures, materials, shaders

## Names and folders

- Lowercase snake_case: `<thing>_<variant>_<map>`, e.g. `crate_wood_01_albedo.png`, `crate_wood_01_normal.png`,
  `crate_wood_01.fbx`, `crate_wood_01_lod1` inside the FBX. Suffixes decide import settings (see `AssetImportRules.cs`):
  `_albedo`/`_color` sRGB; `_normal` normal map; `_mask`, `_rough`, `_metal`, `_ao`, `_height` linear; `_emissive` sRGB.
  Textures under `Art/Textures/UI/` import as sprites without mipmaps.
- `Art/Models/<Props|Characters|Environment|Vehicles>/`, `Art/Textures/<same>/`, `Art/Materials/`, `Art/Shaders/`,
  `Art/VFX/`, `Art/Animation/`, `Art/Source/` for `.blend` and other editable sources (Git LFS).
- Textures are powers of two, 2048 max (1024 for UI). Texel density target and palette are in `docs/GAME.md`.

## Provenance

- Every file under `Art/` has a row in `Assets/_Project/ASSETS.csv`:
  `path,kind,source,licence,credit,prompt_or_notes,seed,date,approved_by`. Sources are `blender` (made here),
  `polyhaven`, `ambientcg`, `kenney`, `comfyui`, `triposg`, `photo`, `hand`. Licences are written as read on that date
  (`CC0`, `Apache-2.0`, `own`). `ProjectChecks` and the EditMode tests fail on a file without a row.
- Generated or downloaded files go to `staging/<source>/<date>_<name>/` with a `notes.md` (prompt, seed, model, licence,
  what was fixed). Nothing enters `Assets/` until the user approves the staging render or screenshot; then the asset-intake
  skill imports it and writes the manifest row.
- Poly Haven and ambientCG are CC0 but their API terms forbid scraping: fetch single assets on request, cache them, and keep
  the "Powered by Poly Haven" credit in the credits screen. Never open a downloaded `.blend` with Blender's
  Auto Run Python Scripts enabled.

## Blender (through the `blender` MCP server, when Blender runs the add-on)

- Build props as scripted `bpy` steps, not by hand: primitives and modifiers, named objects, applied scale, origin at the
  base, metres, +Y forward in Unity terms (export FBX with Forward `-Z`, Up `Y`, Apply Transform or the Unity preset), triangulated,
  one material per mesh unless atlased, UVs with Smart UV Project or seams, LODs as `<name>_lod0..2` children of an empty.
- Bake lighting-independent maps only (albedo, normal, roughness, metallic, AO); lighting happens in Unity.
- Save the `.blend` to `Art/Source/<name>.blend` before exporting; export to `staging/blender/...` first, never straight into
  `Assets/`. Render a viewport PNG beside the export so the user can judge it without opening Blender.
- `BLENDER_MCP_SAFE_MODE=1` is set: scripts that read or write files directly, run programs or touch the network are
  refused; use Blender's own save, import and export operators. The `blender` server runs locally and unauthenticated:
  stop the add-on server when not in use.

## Materials and shaders

- URP Lit / Simple Lit / Shader Graph only. Materials are created by editor code or `unity command material set_properties`,
  named `m_<thing>_<variant>`, one per asset unless shared on purpose.
- Visual review: put a `ShotStation` wherever the look matters, run `Clover > Review > Capture Stations`, read
  `review/shots/diff.md`, and show the user the current PNG before changing a golden image.
