"""Lays the effects probe's frames (review/shots/current/fx_<element>_<moment>.png) out as sheets.

python AgentScripts/fx_sheets.py [element ...] </dev/null   (not -I: it needs Pillow from the user's packages)

Per element: review/shots/current/sheets/fx_<element>.png, its six moments in a 3 x 2 grid, labelled.
With "compare": review/shots/current/fx_compare_sheet.png, every element's six moments in one grid (rows the
elements, columns the moments), small enough to read at a glance.
"""
import os
import sys
from PIL import Image, ImageDraw

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "review", "shots", "current")
ELEMENTS = ["fire", "lightning", "wind", "ice", "water", "life", "earth", "metal", "light", "dark", "poison"]
MOMENTS = ["affliction", "spell", "hit", "charged", "infused", "surge"]


def frame(element, moment, width, height):
    path = os.path.join(ROOT, f"fx_{element}_{moment}.png")
    if not os.path.exists(path):
        return Image.new("RGB", (width, height), (40, 40, 40))
    picture = Image.open(path).convert("RGB")
    # The middle of the frame, where the player and the host stand: a 16:9 crop of its central 70%.
    w, h = picture.size
    cw, ch = int(w * 0.7), int(h * 0.7)
    left, top = (w - cw) // 2, (h - ch) // 2 - int(h * 0.06)
    top = max(0, top)
    picture = picture.crop((left, top, left + cw, top + ch))
    return picture.resize((width, height), Image.LANCZOS)


def label(draw, x, y, text):
    draw.rectangle([x, y, x + 8 * len(text) + 10, y + 20], fill=(20, 20, 24))
    draw.text((x + 5, y + 4), text, fill=(240, 236, 225))


def element_sheet(element):
    width, height = 640, 360
    sheet = Image.new("RGB", (width * 3, height * 2), (20, 20, 24))
    draw = ImageDraw.Draw(sheet)
    for i, moment in enumerate(MOMENTS):
        x, y = (i % 3) * width, (i // 3) * height
        sheet.paste(frame(element, moment, width, height), (x, y))
        label(draw, x + 6, y + 6, f"{element} {moment}")
    os.makedirs(os.path.join(ROOT, "sheets"), exist_ok=True)
    out = os.path.join(ROOT, "sheets", f"fx_{element}.png")
    sheet.save(out)
    print(out)


def compare_sheet():
    width, height = 320, 180
    sheet = Image.new("RGB", (width * len(MOMENTS), height * len(ELEMENTS) + 24), (20, 20, 24))
    draw = ImageDraw.Draw(sheet)
    for c, moment in enumerate(MOMENTS):
        label(draw, c * width + 6, 2, moment)
    for r, element in enumerate(ELEMENTS):
        for c, moment in enumerate(MOMENTS):
            sheet.paste(frame(element, moment, width, height), (c * width, 24 + r * height))
        label(draw, 6, 24 + r * height + 6, element)
    out = os.path.join(ROOT, "fx_compare_sheet.png")
    sheet.save(out)
    print(out)


if __name__ == "__main__":
    wanted = sys.argv[1:] or ELEMENTS
    for name in wanted:
        if name == "compare":
            compare_sheet()
        else:
            element_sheet(name)
