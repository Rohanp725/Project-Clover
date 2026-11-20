"""Lay out strips of the element films for reading at a glance.

For each folder review/gifs/elements/frames/<name>/ named on the command line (all when none), writes
review/gifs/strips/elements/<name>.png: eight frames spread from the moment's start to its end, two rows of four at
480x270, each labelled with its time from the start, under a title with the clip's label. A ninth frame name can be
forced with name:frame. Run as plain `python AgentScripts/element_strips.py fire_affliction ... </dev/null` (Pillow is
the user's package, so not -I).
"""
import os
import sys

from PIL import Image, ImageDraw, ImageFont

FRAMES = "review/gifs/elements/frames"
OUT = "review/gifs/strips/elements"
THUMB = (480, 270)
COUNT = 8


def font(size):
    for face in ("arialbd.ttf", "arial.ttf", "DejaVuSans-Bold.ttf"):
        try:
            return ImageFont.truetype(face, size)
        except OSError:
            continue
    return ImageFont.load_default()


def info_of(folder):
    info = {}
    path = os.path.join(folder, "info.txt")
    if os.path.exists(path):
        for line in open(path, encoding="utf-8"):
            if "=" in line:
                key, value = line.strip().split("=", 1)
                info[key] = value
    return info


def strip(name):
    folder = os.path.join(FRAMES, name)
    paths = sorted(p for p in os.listdir(folder) if p.endswith(".png"))
    if not paths:
        return None
    info = info_of(folder)
    start = int(info.get("start", "0"))
    last = len(paths) - 1
    picks = [round(start + i * (last - start) / (COUNT - 1)) for i in range(COUNT)]
    title_h = 40
    sheet = Image.new("RGB", (THUMB[0] * 4, THUMB[1] * 2 + title_h), (18, 16, 24))
    draw = ImageDraw.Draw(sheet)
    draw.text((10, 8), f"{name}: {info.get('label', '')} ({len(paths) / 20.0:.1f} s, {info.get('enemies', '?')} enemies)", fill=(240, 228, 200), font=font(22))
    small = font(16)
    for i, index in enumerate(picks):
        image = Image.open(os.path.join(folder, paths[index])).convert("RGB").resize(THUMB, Image.LANCZOS)
        x, y = (i % 4) * THUMB[0], title_h + (i // 4) * THUMB[1]
        sheet.paste(image, (x, y))
        draw.text((x + 6, y + 4), f"t {(index - start) / 20.0:.2f} s", fill=(255, 240, 120), font=small)
    os.makedirs(OUT, exist_ok=True)
    out = os.path.join(OUT, name + ".png")
    sheet.save(out, optimize=True)
    return out


def main():
    names = sys.argv[1:] or sorted(n for n in os.listdir(FRAMES) if os.path.isdir(os.path.join(FRAMES, n)))
    for name in names:
        made = strip(name)
        print(made or f"{name}: no frames")


if __name__ == "__main__":
    main()
