# Lays each effect's moment from BodySizeProbe out as one sheet: the four bodies' close pictures side by side (small to
# largest), and under them the game camera's view of the row. Needs Pillow (run without -I).
import glob, os, re
from PIL import Image

folder = "review/shots/current/size"
out = os.path.join(folder, "sheets")
os.makedirs(out, exist_ok=True)
bodies = ["small", "medium", "large", "largest"]
moments = sorted({re.sub(r"_(small|medium|large|largest|view)\.png$", "", os.path.basename(p)) for p in glob.glob(os.path.join(folder, "*_*.png"))})
for moment in moments:
    tiles = [os.path.join(folder, f"{moment}_{b}.png") for b in bodies]
    tiles = [Image.open(t) for t in tiles if os.path.exists(t)]
    if not tiles:
        continue
    w, h = tiles[0].size
    view_path = os.path.join(folder, f"{moment}_view.png")
    view = None
    if os.path.exists(view_path):
        full = Image.open(view_path)
        fw, fh = full.size
        view = full
        view = view.resize((w * len(tiles), int(view.size[1] * w * len(tiles) / view.size[0])))
    sheet = Image.new("RGB", (w * len(tiles), h + (view.size[1] if view else 0)), (20, 20, 24))
    for i, t in enumerate(tiles):
        sheet.paste(t, (i * w, 0))
    if view:
        sheet.paste(view, (0, h))
    sheet.save(os.path.join(out, f"{moment}.png"))
    print(moment)
