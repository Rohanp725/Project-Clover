# Writes into combos4_recipe.json which body each kit piece belongs to, as motion.body, from the Blender pass's own specs.
#
# The pass's specs give every piece its body ("body": n in staging/blender/2026-11-20_reactions4/tools/specs_*.gltf, the
# station builder r4lib_blender.py placing it about that body), but the recipe export (r4recipe_blender.py) wrote only
# follow_body and the orbit's centre_is, never the body itself. Without it the builder had to guess: it read the number
# in a piece's name as its body ("spark15", "heavechip4" counted as bodies 15 and 4), which dropped pieces or put them
# on the wrong enemy. Run again whenever the recipe is exported anew:
#   python -I AgentScripts/recipe_bodies.py staging/blender/2026-11-20_reactions4/tools Assets/_Project/Art/VFX/Combos/combos4_recipe.json </dev/null
# Keeps the file's own layout (indent 1, CRLF) so only the new fields change.
import glob
import json
import os
import sys

tools, recipe_path = sys.argv[1], sys.argv[2]
truth = {}
for path in sorted(glob.glob(os.path.join(tools, 'specs_*.gltf'))):
    with open(path, encoding='utf-8') as handle:
        nodes = json.load(handle).get('nodes', [])
    for node in nodes:
        spec = node.get('extras', {}).get('R')
        if not spec:
            continue
        spec = json.loads(spec)
        for index, piece in enumerate(spec['pieces']):
            name = 'r4_%s_%03d_%s' % (spec['slug'], index, piece.get('n', 'ribbon' if 'path' in piece else 'piece'))
            truth[name] = 'player' if piece.get('pl') else piece.get('body', 0)

with open(recipe_path, 'rb') as handle:
    raw = handle.read().decode('utf-8')
crlf = '\r\n' in raw
recipe = json.loads(raw)
written = unknown = disagree = 0
for look in recipe['looks']:
    for group in look['groups']:
        for at, (instance, motion) in enumerate(zip(group['instances'], group['motion'])):
            name = instance[0]
            if name not in truth:
                unknown += 1
                print('no spec for', name)
                continue
            body = truth[name]
            if body == 'player':
                continue
            if motion.get('follow_body') is not None and motion['follow_body'] != body:
                disagree += 1
                print('follow_body disagrees with the spec:', name, motion['follow_body'], body)
            # Placed right after the verb, so it reads first.
            rebuilt = {}
            for key, value in motion.items():
                if key == 'body':
                    continue
                rebuilt[key] = value
                if key == 'verb':
                    rebuilt['body'] = body
            if 'body' not in rebuilt:
                rebuilt['body'] = body
            group['motion'][at] = rebuilt
            written += 1
    for ribbon in look.get('ribbons', []):
        name = ribbon.get('object', '')
        centre = ribbon.get('centre', '')
        if name in truth and truth[name] != 'player' and centre.startswith('body ') and int(centre[5:]) != truth[name]:
            disagree += 1
            print('ribbon centre disagrees with the spec:', name, centre, truth[name])

conventions = recipe['conventions']
conventions['motion.body'] = (
    "The body (index into bodies; 0 is the main body) the piece belongs to: it is placed about that body and shown on "
    "it. Restored from the pass's own specs (staging/blender/2026-11-20_reactions4/tools/specs_*.gltf) by "
    "AgentScripts/recipe_bodies.py, as the export wrote only follow_body and orbit.centre_is. Numbers in a piece's name "
    "are not bodies (spark15, heavechip4). Pieces on the player carry on_player instead.")
out = json.dumps(recipe, indent=1)
if crlf:
    out = out.replace('\n', '\r\n')
if raw.endswith('\r\n') or raw.endswith('\n'):
    out += '\r\n' if crlf else '\n'
with open(recipe_path, 'wb') as handle:
    handle.write(out.encode('utf-8'))
print(f'bodies written {written}, pieces without a spec {unknown}, disagreements {disagree}')
