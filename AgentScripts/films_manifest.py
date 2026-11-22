"""Write the night films' manifest: every GIF made since a given time, its path, and the doc picture it replaces.

usage: python AgentScripts/films_manifest.py <since yyyy-mm-ddThh:mm (local)> <out.md> </dev/null

Captions come from the docs the pictures sit in:
- reactions, element moments, braziers: the effects review ("The owner's comments on the effects, fourth pass",
  docs/design/feedback/2026-11-22_effects_review_4.md), whose headings name each picture;
- swings: the companion document "Project Clover: Weapon swings on film" (a tab per weapon, a row per element, the
  pictures "Into enemies" and "Trail alone"; docs/design/feedback/2026-11-21_swing_comments.md);
- the rest (way, standins, hands) are new films with no picture yet.
"""
import datetime
import glob
import os
import re
import sys

since = datetime.datetime.strptime(sys.argv[1], "%Y-%m-%dT%H:%M").timestamp()
out = sys.argv[2]

review = open("docs/design/feedback/2026-11-22_effects_review_4.md", encoding="utf-8").read()
headings = re.findall(r"^#### (.+)$", review, re.M)


def slug(text):
    return re.sub(r"[^a-z0-9]+", "_", text.lower()).strip("_")


reaction_caption = {}
element_caption = {}
KINDS = {"spell": "spell", "surge": "surge", "affliction": "affliction", "infused strike": "infused", "katana swing": "hit"}
for heading in headings:
    if " · " in heading:
        name = heading.split(" · ")[0]
        reaction_caption[slug(name.split(" (")[0])] = heading
        if "(" in name:
            reaction_caption[slug(name.split("(")[1].rstrip(")"))] = heading
        reaction_caption[slug(name)] = heading
    m = re.match(r"(\w+): .+ \((.+)\)$", heading)
    if m and m.group(2) in KINDS:
        element_caption[(m.group(1).lower(), KINDS[m.group(2)])] = heading

WEAPON_TABS = {"katana": "Katana", "gsword": "Massive Sword", "hammer": "Great Hammer", "knives": "Twin Knives",
               "scythe": "Scythe", "spear": "Great Spear", "sickle": "Chain Sickle", "bow": "Great Bow", "fists": "Fists",
               "armament": "Full Body Armament", "fan": "War Fan"}
MOMENT_WORDS = {"affliction": "its affliction on an enemy", "spell": "its spell", "hit": "the charged blade's three blows",
                "charged": "the charged blade swung at the air", "infused": "its infused strike",
                "surge": "its surge", "surgemove": "its surge worn while moving and swinging"}

rows = []
for path in sorted(glob.glob("review/gifs/**/*.gif", recursive=True)):
    if os.path.getmtime(path) < since or "/frames/" in path.replace("\\", "/"):
        continue
    path = path.replace("\\", "/")
    name = os.path.basename(path)[:-4]
    # Only a GIF whose frames were filmed since then: an assembly script also rewrites GIFs of older frames.
    folder = os.path.dirname(path)
    key = name.split("_", 1)[1] if "_" in name else name
    frames = os.path.join(folder, "frames", key) if not path.startswith("review/gifs/swings/alone/") else os.path.join("review/gifs/swings/alone", key)
    if path.startswith("review/gifs/way/"):
        frames = os.path.join(folder, "frames", name)
    pngs = glob.glob(os.path.join(frames, "*.png"))
    if not pngs or max(os.path.getmtime(f) for f in pngs) < since:
        continue
    replaces = "new: no doc picture yet"
    if name.startswith("react_"):
        key = name[len("react_"):]
        replaces = f"effects review, reaction \"{reaction_caption[key]}\"" if key in reaction_caption else f"effects review, reaction {key.replace('_', ' ')} (no heading of its own)"
    elif name.startswith("elem_"):
        parts = name.split("_")
        element, moment = parts[1], "_".join(parts[2:])
        if (element, moment) in element_caption:
            replaces = f"effects review, \"{element_caption[(element, moment)]}\""
        else:
            replaces = f"new: {element.title()}, {MOMENT_WORDS.get(moment, moment)} (no picture in the review)"
    elif name.startswith("swing_"):
        parts = name.split("_")
        weapon, element = parts[1], parts[2]
        picture = "Trail alone" if "/alone/" in path else "Into enemies"
        replaces = f"weapon swings doc, tab \"{WEAPON_TABS.get(weapon, weapon)}\", row {element.title()}, picture \"{picture}\""
    elif name == "brazier_lineup":
        replaces = "effects review, \"All eleven braziers in a row (brazier row)\""
    elif name.startswith("brazier_"):
        replaces = "braziers: the single brazier loop of the same name"
    elif path.startswith("review/gifs/way/"):
        replaces = "new: the Katana's Way (NIGHT_FILMS section 3)"
    elif path.startswith("review/gifs/standins/"):
        replaces = "new: the standins merge's pieces (NIGHT_FILMS, Job 3 films owed)"
    elif path.startswith("review/gifs/hands/"):
        replaces = "new: the new hands holding the weapon (close-up, half speed)"
    stamp = datetime.datetime.fromtimestamp(os.path.getmtime(path)).strftime("%H:%M")
    rows.append((path, replaces, stamp))

with open(out, "w", encoding="utf-8") as f:
    f.write(f"# Night films made since {sys.argv[1]}\n\n{len(rows)} GIFs. Columns: path, the doc picture it replaces (caption or alt text), time made.\n\n")
    f.write("| GIF | Replaces | Made |\n|---|---|---|\n")
    for path, replaces, stamp in rows:
        f.write(f"| `{path}` | {replaces} | {stamp} |\n")
print(len(rows), "rows to", out)
