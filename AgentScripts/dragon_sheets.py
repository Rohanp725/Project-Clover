# Lays the frames DragonViewProbe took out as sheets, so many can be read at once.
# usage: python AgentScripts/dragon_sheets.py </dev/null     (from the project folder)
# Writes review/shots/current/sheets/<group>.png: rim_<place> (four frames over time), <move>_<side> (the eight places,
# in the order sw se nw ne / s e n w), things_<move> (each side's three frames in a row).
import glob, os, re
from PIL import Image

shots = "review/shots/current"
out = os.path.join(shots, "sheets")
os.makedirs(out, exist_ok=True)
for old in glob.glob(os.path.join(out, "*.png")):
    # The sheets of other probes (aspect_*) are theirs to keep.
    if not os.path.basename(old).startswith("aspect_"):
        os.remove(old)

def sheet(name, files, columns, width):
    files = [f for f in files if os.path.exists(f)]
    if not files:
        return
    tiles = []
    for f in files:
        image = Image.open(f).convert("RGB")
        tiles.append(image.resize((width, round(image.height * width / image.width))))
    height = tiles[0].height
    rows = (len(tiles) + columns - 1) // columns
    page = Image.new("RGB", (columns * width, rows * height))
    for i, tile in enumerate(tiles):
        page.paste(tile, ((i % columns) * width, (i // columns) * height))
    page.save(os.path.join(out, name + ".png"))
    print(name, len(files))

places = ["sw", "se", "nw", "ne", "s", "e", "n", "w"]
sides = ["east", "west", "southeast", "southwest", "southsoutheast", "southsouthwest"]
moves = ["slither", "flop", "coil", "scythe", "flops", "sting"]
for place in ["middle"] + places:
    sheet("rim_" + place, [f"{shots}/dragon_view_rim_{place}_{i}.png" for i in range(1, 5)], 2, 960)
for move in moves:
    for side in sides:
        sheet(f"{move}_{side}", [f"{shots}/dragon_view_{move}_{side}_{place}.png" for place in places], 4, 640)
    sheet("things_" + move, [f"{shots}/dragon_view_things_{move}_{side}_{i}.png" for side in sides for i in range(1, 4)], 3, 640)
