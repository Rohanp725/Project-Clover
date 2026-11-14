# staging

Generated and downloaded files wait here until the user approves them. Nothing in this folder is in git except this file,
and nothing enters `Assets/` from here except through the asset-intake or audio-intake skill, which also writes the row in
`Assets/_Project/ASSETS.csv`.

Layout: `staging/<source>/<yyyy-mm-dd>_<name>/` with a `notes.md` saying where the file came from, its licence as read that
day, the prompt, seed and model if generated, and what was done to it. Sources: `blender`, `comfyui`, `polyhaven`,
`ambientcg`, `kenney`, `audio`, `downloads`.

Downloaded archives are extracted into their own empty folder here and inspected before anything inside them is opened in
Blender or Unity. Never open a `.blend` from an untrusted source with Auto Run Python Scripts enabled.
