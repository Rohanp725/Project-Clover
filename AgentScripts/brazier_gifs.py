"""Assemble the brazier GIFs and strips from the frames BrazierGifFrames.cs filmed in the game.

For every folder review/gifs/braziers/frames/<element>_<view>/ it writes review/gifs/braziers/brazier_<element>_<view>.gif
(looping, 50 ms a frame, the element GIFs' palette and transparency, element_gifs.write_gif), and for every element a strip
review/gifs/strips/braziers/brazier_<element>.png: four frames of the loop close up over four at the game camera, each
labelled. Run as plain `python AgentScripts/brazier_gifs.py </dev/null` (Pillow and numpy are the user's packages).
"""
import os
import sys

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from element_gifs import label_font, write_gif  # noqa: E402

FRAMES = "review/gifs/braziers/frames"
OUT = "review/gifs/braziers"
STRIPS = "review/gifs/strips/braziers"
THUMB = (480, 270)


def frames_of(folder):
    return [os.path.join(folder, name) for name in sorted(os.listdir(folder)) if name.endswith(".png")]


def main():
    os.makedirs(STRIPS, exist_ok=True)
    names = sorted(name for name in os.listdir(FRAMES) if os.path.isdir(os.path.join(FRAMES, name)))
    for name in names:
        paths = frames_of(os.path.join(FRAMES, name))
        if not paths:
            continue
        images = [Image.open(path).convert("RGB") for path in paths]
        size = write_gif(images, os.path.join(OUT, f"brazier_{name}.gif"))
        print(f"{name}: {len(images)} frames, {size / 1e6:.2f} MB")
    elements = sorted({name.rsplit("_", 1)[0] for name in names})
    font, big = label_font(16), label_font(22)
    for element in elements:
        rows = [view for view in ("close", "game") if os.path.isdir(os.path.join(FRAMES, f"{element}_{view}"))]
        if not rows:
            continue
        sheet = Image.new("RGB", (THUMB[0] * 4, THUMB[1] * len(rows) + 40), (18, 16, 24))
        draw = ImageDraw.Draw(sheet)
        draw.text((10, 8), f"brazier: {element} (one 2.4 s loop; top close, bottom the game camera)", fill=(240, 228, 200), font=big)
        for r, view in enumerate(rows):
            paths = frames_of(os.path.join(FRAMES, f"{element}_{view}"))
            for c in range(4):
                index = round(c * (len(paths) - 1) / 4)
                image = Image.open(paths[index]).convert("RGB").resize(THUMB, Image.LANCZOS)
                sheet.paste(image, (c * THUMB[0], 40 + r * THUMB[1]))
                draw.text((c * THUMB[0] + 6, 44 + r * THUMB[1]), f"{view} t {index / 20.0:.2f} s", fill=(255, 240, 120), font=font)
        sheet.save(os.path.join(STRIPS, f"brazier_{element}.png"), optimize=True)
        print(f"strip {element}")


if __name__ == "__main__":
    main()
