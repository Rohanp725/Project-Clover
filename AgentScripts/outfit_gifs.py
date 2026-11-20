"""Assemble the trial outfit's GIFs and comparison sheets from what OutfitTrialProbe.cs filmed in the Proving Ground.

For every folder review/gifs/outfits/frames/<body>_<clip>/ (000.png ... at 20 frames a second) it writes
review/gifs/outfits/<body>_<clip>.gif with swing_gifs.py's writer (looping, 50 ms a frame, the last held 0.6 s longer,
one 255-colour palette, no dithering, unchanged pixels transparent). Then the sheets, the current ronin's row over
ronin_gen's: review/gifs/outfits/sheet_idle.png (front, three-quarter, side and back; close at half size, and at the
game's own distance at 1:1 and 2x nearest neighbour) and review/gifs/outfits/sheet_<clip>.png (eight frames spread
over the clip, at 0.6 of the 960x540 cut).

Run as plain `python AgentScripts/outfit_gifs.py </dev/null` (Pillow and numpy are the user's packages, so not -I).
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import swing_gifs  # noqa: E402
from PIL import Image, ImageDraw  # noqa: E402

ROOT = "review/gifs/outfits"
FRAMES = os.path.join(ROOT, "frames")
STILLS = "review/shots/current/outfit_trial"
BODIES = [("ronin_current", "current ronin"), ("ronin_gen", "ronin_gen (trial)")]
CLIPS = ["chain", "heavy", "roll", "hammer"]
FACINGS = ["front", "three_quarter", "side", "back"]
GROUND = (18, 16, 24)
TEXT = (240, 228, 200)


def gifs():
    for body, _ in BODIES:
        for clip in CLIPS:
            folder = os.path.join(FRAMES, f"{body}_{clip}")
            if not os.path.isdir(folder):
                print(f"missing {folder}")
                continue
            out = os.path.join(ROOT, f"{body}_{clip}.gif")
            made = swing_gifs.make_gif(folder, out)
            if made:
                count, size_bytes, size = made
                print(f"{out}: {count} frames, {count / 20.0:.2f} s, {size[0]}x{size[1]}, {size_bytes / 1e6:.2f} MB")


def idle_sheet():
    font = swing_gifs.label_font(18)
    cell, label_h, side_w = 360, 28, 190
    columns = []
    for facing in FACINGS:
        columns += [(facing, "near"), (facing, "game"), (facing, "game2x")]
    sheet = Image.new("RGB", (side_w + len(columns) * cell, label_h + len(BODIES) * cell), GROUND)
    draw = ImageDraw.Draw(sheet)
    for column, (facing, kind) in enumerate(columns):
        draw.text((side_w + column * cell + 8, 4), f"{facing} {kind}", fill=TEXT, font=font)
    for row, (body, name) in enumerate(BODIES):
        draw.text((10, label_h + row * cell + cell // 2 - 10), name, fill=TEXT, font=font)
        for column, (facing, kind) in enumerate(columns):
            source = "near" if kind == "near" else "game"
            path = os.path.join(STILLS, f"{body}_idle_{facing}_{source}.png")
            if not os.path.exists(path):
                continue
            image = Image.open(path).convert("RGB")
            if kind == "near":
                image = image.resize((cell, cell), Image.LANCZOS)
            elif kind == "game2x":
                # The middle half at 2x, nearest neighbour, so the game's own pixels show.
                w, h = image.size
                image = image.crop((w // 4, h // 4, w * 3 // 4, h * 3 // 4)).resize((cell, cell), Image.NEAREST)
            sheet.paste(image, (side_w + column * cell, label_h + row * cell))
    out = os.path.join(ROOT, "sheet_idle.png")
    sheet.save(out, optimize=True)
    print(out)


def clip_sheet(clip, picks=8, scale=0.6):
    font = swing_gifs.label_font(18)
    width, height = round(960 * scale), round(540 * scale)
    label_h, side_w = 28, 190
    sheet = Image.new("RGB", (side_w + picks * width, label_h + len(BODIES) * height), GROUND)
    draw = ImageDraw.Draw(sheet)
    for row, (body, name) in enumerate(BODIES):
        draw.text((10, label_h + row * height + height // 2 - 10), name, fill=TEXT, font=font)
        folder = os.path.join(FRAMES, f"{body}_{clip}")
        if not os.path.isdir(folder):
            continue
        paths = swing_gifs.frame_paths(folder)
        # The same frame numbers for both bodies, so each column is the same moment of the move.
        indices = [round(3 + i * (len(paths) - 4) / (picks - 1)) for i in range(picks)]
        for column, index in enumerate(indices):
            index = min(index, len(paths) - 1)
            image = Image.open(paths[index]).convert("RGB").resize((width, height), Image.LANCZOS)
            sheet.paste(image, (side_w + column * width, label_h + row * height))
            if row == 0:
                draw.text((side_w + column * width + 8, 4), f"frame {index}", fill=TEXT, font=font)
    out = os.path.join(ROOT, f"sheet_{clip}.png")
    sheet.save(out, optimize=True)
    print(out)


def main():
    if not os.path.isdir(FRAMES):
        print(f"no frames under {FRAMES}: run OutfitTrialProbe first")
        return
    gifs()
    idle_sheet()
    for clip in CLIPS:
        clip_sheet(clip)


if __name__ == "__main__":
    main()
