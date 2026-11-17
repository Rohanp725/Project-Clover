# Lays the eight captures of one situation (aspect_<what>_<w>x<h>.png) on one sheet, each 300 high, two rows.
import sys, glob, os
from PIL import Image
what = sys.argv[1]
order = ["1280x720", "1920x1200", "1280x800", "1024x768", "2560x1440", "3840x2160", "2560x1080", "3840x1080"]
tiles = []
for size in order:
    path = f"review/shots/current/aspect_{what}_{size}.png"
    if not os.path.exists(path):
        continue
    image = Image.open(path).convert("RGB")
    high = 330
    tiles.append(image.resize((int(image.width * high / image.height), high), Image.LANCZOS))
rows = [tiles[:4], tiles[4:6], tiles[6:]]
rows = [row for row in rows if row]
width = max(sum(t.width for t in row) + 6 * (len(row) - 1) for row in rows)
sheet = Image.new("RGB", (width, len(rows) * 336), (255, 0, 255))
for r, row in enumerate(rows):
    x = 0
    for t in row:
        sheet.paste(t, (x, r * 336))
        x += t.width + 6
os.makedirs("review/shots/current/sheets", exist_ok=True)
sheet.save(f"review/shots/current/sheets/aspect_{what}.png")
print("sheet", what, len(tiles))
