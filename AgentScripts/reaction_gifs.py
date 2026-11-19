"""Assemble the reaction GIFs from the frames ReactionGifFrames.cs filmed in the game.

For every folder review/gifs/reactions/frames/<name>/ (000.png, 001.png, ... at 20 frames a second, and info.txt) it
writes review/gifs/reactions/react_<name>.gif: looping, 50 ms a frame, the last frame held 0.6 s longer. One palette of
255 colours is made for the whole GIF from a spread of its frames (so the floor does not flicker between frames), every
frame is mapped to it without dithering, and each frame after the first marks the pixels that did not change as
transparent, which is what keeps a 960x540 GIF small. Then review/gifs/reactions/contact_sheet.png: the impact frame of
each reaction (info.txt's impact_frame), labelled with its name and whether its look is designed or composed.

Run as plain `python AgentScripts/reaction_gifs.py </dev/null` (Pillow and numpy are the user's packages, so not -I).
Optional arguments: names of reactions to assemble only those.
"""
import os
import sys

import numpy
from PIL import Image, ImageDraw, ImageFont

ROOT = "review/gifs/reactions"
FRAMES = os.path.join(ROOT, "frames")
FRAME_MS = 50
HOLD_MS = 600
TRANSPARENT = 255


def info_of(folder):
    info = {}
    path = os.path.join(folder, "info.txt")
    if os.path.exists(path):
        for line in open(path, encoding="utf-8"):
            if "=" in line:
                key, value = line.strip().split("=", 1)
                info[key] = value
    return info


def frame_paths(folder):
    return sorted(os.path.join(folder, name) for name in os.listdir(folder) if name.endswith(".png"))


def palette_for(images):
    # A montage of up to twelve frames spread across the reaction, so the palette holds the look's colours as well as
    # the floor's.
    picks = images if len(images) <= 12 else [images[round(i * (len(images) - 1) / 11)] for i in range(12)]
    width, height = picks[0].size
    montage = Image.new("RGB", (width, height * len(picks)))
    for i, image in enumerate(picks):
        montage.paste(image, (0, i * height))
    montage = montage.resize((width // 2, height * len(picks) // 2), Image.BILINEAR)
    return montage.quantize(colors=255, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE)


def write_gif(name, folder, out_path):
    images = [Image.open(path).convert("RGB") for path in frame_paths(folder)]
    if not images:
        return None
    palette_image = palette_for(images)
    palette = palette_image.getpalette()[: 255 * 3] + [0, 0, 0]
    frames = []
    previous = None
    for image in images:
        indices = numpy.array(image.quantize(palette=palette_image, dither=Image.Dither.NONE), dtype=numpy.uint8)
        shown = indices.copy()
        if previous is not None:
            shown[indices == previous] = TRANSPARENT
        previous = indices
        frame = Image.fromarray(shown, mode="P")
        frame.putpalette(palette)
        frames.append(frame)
    durations = [FRAME_MS] * len(frames)
    durations[-1] += HOLD_MS
    frames[0].save(out_path, save_all=True, append_images=frames[1:], duration=durations, loop=0,
                   transparency=TRANSPARENT, disposal=1, optimize=False)
    return len(frames)


def label_font(size):
    for face in ("arialbd.ttf", "arial.ttf", "DejaVuSans-Bold.ttf"):
        try:
            return ImageFont.truetype(face, size)
        except OSError:
            continue
    return ImageFont.load_default()


def contact_sheet(entries, out_path):
    columns, thumb_w, thumb_h, label_h = 7, 480, 270, 30
    rows = (len(entries) + columns - 1) // columns
    sheet = Image.new("RGB", (columns * thumb_w, rows * (thumb_h + label_h)), (18, 16, 24))
    draw = ImageDraw.Draw(sheet)
    font = label_font(18)
    for i, (name, folder, info) in enumerate(entries):
        paths = frame_paths(folder)
        index = min(int(info.get("impact_frame", "0")), len(paths) - 1)
        thumb = Image.open(paths[index]).convert("RGB").resize((thumb_w, thumb_h), Image.LANCZOS)
        x, y = (i % columns) * thumb_w, (i // columns) * (thumb_h + label_h)
        sheet.paste(thumb, (x, y + label_h))
        kind = info.get("kind", "?")
        text = f"{info.get('name', name)}  ({kind}, impact {float(info.get('impact', '0')):.2f} s)"
        draw.text((x + 8, y + 5), text, fill=(240, 228, 200) if kind == "designed" else (190, 200, 230), font=font)
    sheet.save(out_path, optimize=True)


def main():
    only = set(sys.argv[1:])
    names = sorted(name for name in os.listdir(FRAMES) if os.path.isdir(os.path.join(FRAMES, name)))
    entries = []
    for name in names:
        folder = os.path.join(FRAMES, name)
        info = info_of(folder)
        entries.append((name, folder, info))
        if only and name not in only:
            continue
        out_path = os.path.join(ROOT, f"react_{name}.gif")
        count = write_gif(name, folder, out_path)
        if count:
            print(f"{name}: {count} frames, {os.path.getsize(out_path) / 1e6:.2f} MB")
    # The sheet in the order the game numbers the pairs is not known here; alphabetical is easy to search.
    contact_sheet(entries, os.path.join(ROOT, "contact_sheet.png"))
    print(f"contact sheet of {len(entries)}")


if __name__ == "__main__":
    main()
