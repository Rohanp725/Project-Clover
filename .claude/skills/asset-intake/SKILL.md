---
name: asset-intake
description: Move an approved model, texture, material source or other art file from staging into Assets with the right name, folder, import settings and manifest row, then verify it in the editor. Use when the user approves something in staging or hands over a file.
---

# Asset intake

Nothing enters `Assets/` without the user's approval of what it looks like. Approval is on a render or screenshot from
`staging/`, not on a description.

1. **Check the file**: licence and source are known (ask if not); name is lowercase snake_case with the right suffix
   (`.claude/rules/art.md`); textures are powers of two; models are in metres with applied transforms; no `.blend` with
   embedded scripts from an untrusted source. Downloaded archives are extracted into their own folder under `staging/`.
2. **Place it**: `unity command assets import -- --source "staging/<...>/<file>" --path "Assets/_Project/Art/<Kind>/<group>/<name>.<ext>"`
   (see `unity command --query "assets import" --detail full` for the parameter names). `AssetImportRules` applies the import
   settings; read them back with `assets get_import_settings` and confirm sRGB, normal map and max size are right.
3. **Manifest row**: append to `Assets/_Project/ASSETS.csv`:
   `Assets/_Project/Art/...,<model|texture|material|shader|vfx|anim|source>,<source>,<licence>,<credit>,<prompt or notes>,<seed or ->,<yyyy-mm-dd>,<user>`.
   Quote a field that contains a comma. The source `.blend` gets its own row under `Art/Source/`.
4. **Material** (models): create or update it with `unity command material set_properties` or the builder that owns the
   prop; URP Lit, textures by suffix. Assign in the prefab through `component set_field`, not by editing YAML.
5. **Verify**: `unity --json recompile` is not needed for assets; instead `unity command console get -- --tail 20 --level warn`
   for import warnings, then `Clover > Review > Capture Stations` (or a play-mode probe) and Read the PNG that shows the asset.
   Run `unity command test run -- --mode editor --filter ConventionsTests` so the manifest test passes.
6. **Tidy**: leave the staging folder in place (it is git-ignored) until the user says the asset is final; then delete it.

Report: the asset path, the manifest row written, the import settings confirmed, and the screenshot path.
