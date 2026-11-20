"""Strip sheets of the swing clips at the moment of contact, to judge what the blow's flash and cut streak cover.

For each clip named on the command line (katana_fire ...) it reads review/gifs/swings/frames/<clip>/info.txt, whose
`contacts` line SwingLookProbe.cs writes (the frames on which a cut streak was drawn), and writes
review/gifs/swings/strips/contact_<clip>.png: a column for each contact (up to four, spread over the clip), the middle
640x360 of the frame on the contact and of the frame after it. With `--before <folder>` holding an older
swing_<clip>.gif, its frames at the same numbers are put on top, so the old look can be set against the new
(the clips are filmed at a fixed 20 frames a second, so the same number is the same moment of the same swing).

Run as plain `python AgentScripts/swing_contact_strips.py katana_fire ... </dev/null` (Pillow is the user's package, so not -I).
"""
import os
import sys

from PIL import Image, ImageDraw, ImageFont, ImageSequence

FRAMES = "review/gifs/swings/frames"
OUT = "review/gifs/swings/strips"
CROP_W, CROP_H, LABEL_H = 640, 360, 28


def font(size):
    for face in ("arialbd.ttf", "arial.ttf", "DejaVuSans-Bold.ttf"):
        try:
            return ImageFont.truetype(face, size)
        except OSError:
            continue
    return ImageFont.load_default()


def info_of(folder):
    info = {}
    for line in open(os.path.join(folder, "info.txt"), encoding="utf-8"):
        if "=" in line:
            key, value = line.strip().split("=", 1)
            info[key] = value
    return info


def middle(image):
    image = image.convert("RGB")
    if image.size != (960, 540):
        image = image.resize((960, 540), Image.LANCZOS)
    left, top = (960 - CROP_W) // 2, (540 - CROP_H) // 2
    return image.crop((left, top, left + CROP_W, top + CROP_H))


def gif_frames(path):
    # Pillow composites each GIF frame over the one before, so a frame drawn with only its changed pixels comes out whole.
    return [frame.convert("RGB") for frame in ImageSequence.Iterator(Image.open(path))]


def spread(contacts, most=4):
    if len(contacts) <= most:
        return contacts
    return [contacts[round(i * (len(contacts) - 1) / (most - 1))] for i in range(most)]


def strip(clip, before_folder):
    folder = os.path.join(FRAMES, clip)
    info = info_of(folder)
    contacts = [int(x) for x in info.get("contacts", "").split(",") if x.strip()]
    if not contacts:
        print(f"{clip}: no contacts recorded")
        return
    picks = spread(contacts)
    rows = []
    old = None
    if before_folder:
        path = os.path.join(before_folder, f"swing_{clip}.gif")
        if os.path.exists(path):
            old = gif_frames(path)
    if old:
        rows.append(("before, on contact", [old[min(c, len(old) - 1)] for c in picks]))
    now = lambda n: Image.open(os.path.join(folder, f"{n:03d}.png"))
    count = int(info.get("frames", "0"))
    rows.append(("now, on contact", [now(c) for c in picks]))
    rows.append(("now, a frame after", [now(min(c + 1, count - 1)) for c in picks]))
    sheet = Image.new("RGB", (len(picks) * CROP_W, len(rows) * (CROP_H + LABEL_H) + 34), (18, 16, 24))
    draw = ImageDraw.Draw(sheet)
    draw.text((8, 6), f"{info.get('weapon')} with {info.get('element')}: contacts at frames {', '.join(map(str, picks))}", fill=(240, 228, 200), font=font(20))
    for r, (label, images) in enumerate(rows):
        y = 34 + r * (CROP_H + LABEL_H)
        for c, image in enumerate(images):
            sheet.paste(middle(image), (c * CROP_W, y + LABEL_H))
            draw.text((c * CROP_W + 8, y + 5), f"{label} (frame {picks[c] + (1 if 'after' in label else 0)})", fill=(240, 228, 200), font=font(15))
    os.makedirs(OUT, exist_ok=True)
    out = os.path.join(OUT, f"contact_{clip}.png")
    sheet.save(out, optimize=True)
    print(out)


def main():
    args = sys.argv[1:]
    before = None
    if "--before" in args:
        at = args.index("--before")
        before = args[at + 1]
        del args[at:at + 2]
    for clip in args:
        strip(clip, before)


if __name__ == "__main__":
    main()
