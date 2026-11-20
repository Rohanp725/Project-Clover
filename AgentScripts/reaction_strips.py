"""Lay out strips of the reaction films for reading at a glance.

For each folder review/gifs/reactions/frames/<name>/ named on the command line (all when none), writes
review/gifs/strips/reactions/react_<name>.png: eight frames spread from when the look is shown to the clip's end, two rows of
four at 480x270, each labelled with its time, under a title with the reaction, its kind (bespoke, designed, composed) and
how many enemies stood. Run as plain `python AgentScripts/reaction_strips.py <name> ... </dev/null` (Pillow is the
user's package, so not -I).
"""
import os
import sys

from PIL import Image, ImageDraw, ImageFont

FRAMES = "review/gifs/reactions/frames"
OUT = "review/gifs/strips/reactions"


def font(size):
    for face in ("arialbd.ttf", "arial.ttf", "DejaVuSans-Bold.ttf"):
        try:
            return ImageFont.truetype(face, size)
        except OSError:
            continue
    return ImageFont.load_default()


def strip(name):
    folder = os.path.join(FRAMES, name)
    paths = sorted(p for p in os.listdir(folder) if p.endswith(".png"))
    if not paths:
        return None
    info = {}
    if os.path.exists(os.path.join(folder, "info.txt")):
        for line in open(os.path.join(folder, "info.txt"), encoding="utf-8"):
            if "=" in line:
                key, value = line.strip().split("=", 1)
                info[key] = value
    start = int(info.get("shown_frame", info.get("start", "0")))
    picks = [round(start + i * (len(paths) - 1 - start) / 7) for i in range(8)]
    sheet = Image.new("RGB", (1920, 580), (18, 16, 24))
    draw = ImageDraw.Draw(sheet)
    draw.text((10, 8), f"{info.get('name', name)} ({info.get('kind', '?')}, {info.get('enemies', '?')} enemies, {len(paths) / 20:.1f} s)",
              fill=(240, 228, 200), font=font(22))
    small = font(16)
    for i, index in enumerate(picks):
        image = Image.open(os.path.join(folder, paths[index])).convert("RGB").resize((480, 270), Image.LANCZOS)
        x, y = (i % 4) * 480, 40 + (i // 4) * 270
        sheet.paste(image, (x, y))
        draw.text((x + 6, y + 4), f"t {(index - start) / 20:.2f} s", fill=(255, 240, 120), font=small)
    os.makedirs(OUT, exist_ok=True)
    out = os.path.join(OUT, f"react_{name}.png")
    sheet.save(out, optimize=True)
    return out


if __name__ == "__main__":
    names = sys.argv[1:] or sorted(n for n in os.listdir(FRAMES) if os.path.isdir(os.path.join(FRAMES, n)))
    for name in names:
        print(strip(name) or f"{name}: no frames")
