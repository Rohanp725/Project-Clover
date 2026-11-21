"""Assemble the swing-look GIFs from the frames SwingLookProbe.cs filmed in the game.

For every folder review/gifs/swings/frames/<weapon>_<element>/ (000.png, 001.png, ... at 20 frames a second, and
info.txt) it writes review/gifs/swings/swing_<weapon>_<element>.gif: looping, 50 ms a frame, the last frame held 0.6 s
longer, one palette of 255 colours for the whole GIF from a spread of its frames, no dithering, and the pixels that did
not change since the frame shown before left transparent (counting a change of less than NEAR, summed over red, green
and blue, as none). A GIF over 4.9 MB is made again smaller (0.8, 0.65, 0.5, then 0.4 of 960x540) until it fits. Then
review/gifs/swings/contact_sheet.png: a row per weapon (in the probe's order) and a column per element (the ring's
order), each cell the frame of its clip most unlike the first (where the swing is), labelled with the blows, landings
and trails the probe counted; a clip not whole or not set off is labelled in red.

Run as plain `python AgentScripts/swing_gifs.py </dev/null` (Pillow and numpy are the user's packages, so not -I).
Optional arguments: folder names (katana_fire) to assemble only those, or `sheet` for the sheet alone; the sheet is
always made of every folder. The argument `alone` reads the clips filmed with the enemies stood off
(review/gifs/swings/alone/<weapon>_<element>/, AgentScripts/swing_gif_alone.txt) and writes their GIFs and sheet beside
them, as review/gifs/swings/alone/swing_<weapon>_<element>.gif and review/gifs/swings/alone/contact_sheet.png.
"""
import os
import sys

import numpy
from PIL import Image, ImageDraw, ImageFont

ALONE = "alone" in sys.argv[1:]
ROOT = "review/gifs/swings/alone" if ALONE else "review/gifs/swings"
FRAMES = ROOT if ALONE else os.path.join(ROOT, "frames")
FRAME_MS = 50
HOLD_MS = 600
TRANSPARENT = 255
NEAR = 24
MOST_BYTES = 4.9e6
# The longest, brightest clips (Light's, about 250 frames) still passed 4.9 MB at half size: 0.4 is the last step.
SCALES = (1.0, 0.8, 0.65, 0.5, 0.4)
WEAPONS = ["katana", "gsword", "hammer", "knives", "scythe", "spear", "sickle", "bow", "fists", "armament", "fan"]
WEAPON_NAMES = {
    "katana": "Katana", "gsword": "Massive Sword", "hammer": "Great Hammer", "knives": "Twin Knives", "scythe": "Scythe",
    "spear": "Great Spear", "sickle": "Chain Sickle", "bow": "Great Bow", "fists": "Fists", "armament": "Armament",
    # No moves of its own yet: swung on the Katana's.
    "fan": "War Fan*",
}
ELEMENTS = ["Fire", "Lightning", "Wind", "Ice", "Water", "Life", "Earth", "Metal", "Light", "Dark", "Poison"]


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


SCENE_COLOURS = 112
EFFECT_COLOURS = 255 - SCENE_COLOURS
MOST_EFFECT_PIXELS = 400000


def colours_of(image, count):
    quantized = image.quantize(colors=count, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE)
    used = len(set(quantized.getdata()))
    return quantized.getpalette()[: used * 3]


def palette_for(images):
    # One palette for the whole GIF, in two shares. The scene's: a montage of up to twelve frames spread across the
    # clip. The effects': only the pixels that differ from the first frame (the trails, the sheath, the landings),
    # gathered over every frame. Drawn from whole frames alone the 255 colours went almost all to the floor's purples
    # and the thin trails were mapped to the nearest floor grey, so every element but Fire's bright flame came out
    # grey in the GIF though the frames were in colour (the owner, 2026-11-20, on the alone GIFs).
    picks = images if len(images) <= 12 else [images[round(i * (len(images) - 1) / 11)] for i in range(12)]
    width, height = picks[0].size
    montage = Image.new("RGB", (width, height * len(picks)))
    for i, image in enumerate(picks):
        montage.paste(image, (0, i * height))
    montage = montage.resize((width // 2, height * len(picks) // 2), Image.BILINEAR)
    scene = colours_of(montage, SCENE_COLOURS)

    first = numpy.asarray(images[0], dtype=numpy.int16)
    moved = []
    for image in images[1:]:
        frame = numpy.asarray(image, dtype=numpy.int16)
        changed = numpy.abs(frame - first).sum(axis=2) > 48
        if changed.any():
            moved.append(frame[changed].astype(numpy.uint8))
    effect = []
    if moved:
        pixels = numpy.concatenate(moved)
        if len(pixels) > MOST_EFFECT_PIXELS:
            pixels = pixels[numpy.linspace(0, len(pixels) - 1, MOST_EFFECT_PIXELS).astype(numpy.int64)]
        side = int(numpy.ceil(numpy.sqrt(len(pixels))))
        padded = numpy.zeros((side * side, 3), dtype=numpy.uint8)
        padded[: len(pixels)] = pixels
        # The padding repeats the first moved pixel, not black, so it does not take colours of its own.
        padded[len(pixels):] = pixels[0]
        effect = colours_of(Image.fromarray(padded.reshape(side, side, 3), "RGB"), EFFECT_COLOURS)

    colours = (scene + effect)[: 255 * 3]
    # Short of 255 the rest repeat the first colour, so a pixel is never sent to a spare black entry.
    while len(colours) < 255 * 3:
        colours += colours[:3]
    palette_image = Image.new("P", (1, 1))
    # Index 255 is the transparent one: it repeats colour 0 too, so the nearest-colour search (first match wins)
    # never picks it for a real pixel, and a black pixel is not turned see-through.
    palette_image.putpalette(colours + colours[:3])
    return palette_image


def write_gif(images, out_path):
    palette_image = palette_for(images)
    palette = palette_image.getpalette()[: 256 * 3]
    colours = numpy.array(palette, dtype=numpy.int32).reshape(256, 3)
    frames = []
    seen = None
    for image in images:
        indices = numpy.array(image.quantize(palette=palette_image, dither=Image.Dither.NONE), dtype=numpy.uint8)
        wanted = colours[indices]
        if seen is None:
            shown = indices
            seen = wanted
        else:
            # What the viewer sees is the last colour written there, so the change is measured against that.
            changed = numpy.abs(wanted - seen).sum(axis=2) > NEAR
            shown = numpy.where(changed, indices, TRANSPARENT).astype(numpy.uint8)
            seen[changed] = wanted[changed]
        frame = Image.fromarray(shown, mode="P")
        frame.putpalette(palette)
        frames.append(frame)
    durations = [FRAME_MS] * len(frames)
    durations[-1] += HOLD_MS
    frames[0].save(out_path, save_all=True, append_images=frames[1:], duration=durations, loop=0,
                   transparency=TRANSPARENT, disposal=1, optimize=False)
    return os.path.getsize(out_path)


def make_gif(folder, out_path):
    originals = [Image.open(path).convert("RGB") for path in frame_paths(folder)]
    if not originals:
        return None
    if originals[0].size != (960, 540):
        originals = [image.resize((960, 540), Image.LANCZOS) for image in originals]
    for scale in SCALES:
        if scale == 1.0:
            images = originals
        else:
            size = (round(originals[0].width * scale), round(originals[0].height * scale))
            images = [image.resize(size, Image.LANCZOS) for image in originals]
        size_bytes = write_gif(images, out_path)
        if size_bytes <= MOST_BYTES:
            break
    return len(originals), size_bytes, images[0].size


def label_font(size):
    for face in ("arialbd.ttf", "arial.ttf", "DejaVuSans-Bold.ttf"):
        try:
            return ImageFont.truetype(face, size)
        except OSError:
            continue
    return ImageFont.load_default()


def strongest(paths, start):
    # The frame that differs most from the first (the soldiers standing, the blade charged): where the swing is.
    first = numpy.asarray(Image.open(paths[0]).convert("RGB").resize((240, 135)), dtype=numpy.int32)
    best, best_index = -1, min(start, len(paths) - 1)
    for index, path in enumerate(paths[start:], start):
        frame = numpy.asarray(Image.open(path).convert("RGB").resize((240, 135)), dtype=numpy.int32)
        changed = int((numpy.abs(frame - first).sum(axis=2) > 60).sum())
        if changed > best:
            best, best_index = changed, index
    return best_index


def contact_sheet(entries, out_path):
    # A row a weapon, a column an element.
    rows = [tag for tag in WEAPONS if any(info.get("tag") == tag for _, _, info in entries)]
    if not rows:
        return
    thumb_w, thumb_h, label_h, side_w, head_h = 320, 180, 26, 150, 34
    sheet = Image.new("RGB", (side_w + len(ELEMENTS) * thumb_w, head_h + len(rows) * (thumb_h + label_h)), (18, 16, 24))
    draw = ImageDraw.Draw(sheet)
    font = label_font(13)
    big = label_font(20)
    for column, element in enumerate(ELEMENTS):
        draw.text((side_w + column * thumb_w + 8, 6), element, fill=(240, 228, 200), font=big)
    for row, tag in enumerate(rows):
        draw.text((10, head_h + row * (thumb_h + label_h) + label_h + thumb_h // 2 - 12), WEAPON_NAMES.get(tag, tag), fill=(240, 228, 200), font=big)
    for name, folder, info in entries:
        paths = frame_paths(folder)
        if not paths or info.get("tag") not in rows or info.get("element") not in ELEMENTS:
            continue
        index = strongest(paths, int(info.get("start", "0")))
        thumb = Image.open(paths[index]).convert("RGB").resize((thumb_w, thumb_h), Image.LANCZOS)
        row = rows.index(info.get("tag"))
        column = ELEMENTS.index(info.get("element"))
        x, y = side_w + column * thumb_w, head_h + row * (thumb_h + label_h)
        sheet.paste(thumb, (x, y + label_h))
        seconds = int(info.get("frames", "0")) / 20.0
        text = f"blows {info.get('blows', '?')}, landings {info.get('landings', '?')}, trails {info.get('most_trails', '?')}  ({seconds:.1f} s)"
        good = info.get("ended") == "True" and info.get("set_off") == "True"
        draw.text((x + 6, y + 6), text, fill=(240, 228, 200) if good else (255, 120, 110), font=font)
    sheet.save(out_path, optimize=True)


def main():
    if not os.path.isdir(FRAMES):
        print(f"no frames under {FRAMES}: run SwingLookProbe first")
        return
    only = set(name for name in sys.argv[1:] if name not in ("sheet", "alone"))
    names = [name for name in os.listdir(FRAMES) if os.path.isdir(os.path.join(FRAMES, name))]
    entries = []
    for name in names:
        folder = os.path.join(FRAMES, name)
        info = info_of(folder)
        # A folder without its info.txt is still being filmed (or was cut off): left out.
        if "element" in info:
            entries.append((name, folder, info))
    entries.sort(key=lambda entry: int(entry[2].get("order", "9999")))
    for name, folder, info in entries:
        if (only or "sheet" in sys.argv[1:]) and name not in only:
            continue
        out_path = os.path.join(ROOT, f"swing_{name}.gif")
        made = make_gif(folder, out_path)
        if made:
            count, size_bytes, size = made
            # Flushed line by line: a run of 121 GIFs takes minutes, and piped through tail it must show where it got to.
            print(f"{name}: {count} frames, {count / 20.0:.2f} s, {size[0]}x{size[1]}, {size_bytes / 1e6:.2f} MB", flush=True)
    contact_sheet(entries, os.path.join(ROOT, "contact_sheet.png"))
    print(f"contact sheet of {len(entries)}", flush=True)
    # Every clip of the grid should have its GIF, newer than its frames: say which do not.
    missing, stale = [], []
    for tag in WEAPONS:
        for element in ELEMENTS:
            name = f"{tag}_{element.lower()}"
            gif = os.path.join(ROOT, f"swing_{name}.gif")
            info = os.path.join(FRAMES, name, "info.txt")
            if not os.path.exists(gif):
                missing.append(name)
            elif os.path.exists(info) and os.path.getmtime(gif) < os.path.getmtime(info):
                stale.append(name)
    print(f"GIFs: {len(WEAPONS) * len(ELEMENTS) - len(missing)} of {len(WEAPONS) * len(ELEMENTS)} present, "
          f"{len(stale)} older than their frames"
          + (f"; missing {', '.join(missing)}" if missing else "") + (f"; stale {', '.join(stale)}" if stale else ""), flush=True)


if __name__ == "__main__":
    main()
