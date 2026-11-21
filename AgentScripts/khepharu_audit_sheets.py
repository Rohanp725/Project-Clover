"""Assemble what KhepharuAudit.cs filmed: a GIF for every filmed move or case, and the contact sheet of the worst frames.

For every review/khepharu/audit/<camera>/<name>/seq/ folder of JPGs (one every 0.1 s, or 0.33 s for the change and
0.385 s for the death) it writes review/khepharu/audit/<camera>/<name>.gif at 960 px wide, in real time, with ffmpeg
(one palette per GIF). Then review/khepharu/audit/worst.png: the frames named on the command line (paths relative to
review/khepharu/audit), each labelled with the caption after a '=' (path=caption), four across.

Run as plain `python AgentScripts/khepharu_audit_sheets.py gifs </dev/null` and
`python AgentScripts/khepharu_audit_sheets.py sheet path=caption ... </dev/null` (Pillow is the user's package, so not -I).
"""
import os
import subprocess
import sys

from PIL import Image, ImageDraw, ImageFont

ROOT = "review/khepharu/audit"


def gifs():
    for camera in sorted(os.listdir(ROOT)):
        base = os.path.join(ROOT, camera)
        if not os.path.isdir(base):
            continue
        for name in sorted(os.listdir(base)):
            seq = os.path.join(base, name, "seq")
            if not os.path.isdir(seq):
                continue
            frames = sorted(f for f in os.listdir(seq) if f.endswith(".jpg"))
            if len(frames) < 2:
                continue
            rate = 3 if name in ("change", "death") else 2 if name == "intro" else 10
            out = os.path.join(base, name + ".gif")
            graph = f"fps={rate},scale=960:-1:flags=lanczos,split[a][b];[a]palettegen=max_colors=200:stats_mode=diff[p];[b][p]paletteuse=dither=none:diff_mode=rectangle"
            command = ["ffmpeg", "-y", "-loglevel", "error", "-framerate", str(rate), "-i", os.path.join(seq, "%03d.jpg"), "-filter_complex", graph, out]
            subprocess.run(command, stdin=subprocess.DEVNULL, check=False)
            print(f"{out}: {len(frames)} frames, {os.path.getsize(out) // 1024} KB" if os.path.exists(out) else f"{out}: FAILED")


def sheet(items):
    tiles = []
    for item in items:
        path, _, caption = item.partition("=")
        full = os.path.join(ROOT, path)
        if not os.path.exists(full):
            print("missing", full)
            continue
        image = Image.open(full).convert("RGB")
        image.thumbnail((640, 360))
        tile = Image.new("RGB", (640, 400), (18, 16, 14))
        tile.paste(image, (0, 0))
        draw = ImageDraw.Draw(tile)
        try:
            font = ImageFont.truetype("arial.ttf", 15)
        except OSError:
            font = ImageFont.load_default()
        draw.text((8, 366), caption[:84], fill=(240, 232, 214), font=font)
        draw.text((8, 382), caption[84:168], fill=(240, 232, 214), font=font)
        tiles.append(tile)
    across = 4
    rows = (len(tiles) + across - 1) // across
    page = Image.new("RGB", (across * 640, rows * 400), (8, 8, 8))
    for i, tile in enumerate(tiles):
        page.paste(tile, ((i % across) * 640, (i // across) * 400))
    out = os.path.join(ROOT, "worst.png")
    page.save(out, optimize=True)
    print(f"{out}: {len(tiles)} frames, {page.size[0]}x{page.size[1]}")


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "gifs":
        gifs()
    elif len(sys.argv) > 1 and sys.argv[1] == "sheet":
        sheet(sys.argv[2:])
    else:
        print(__doc__)
