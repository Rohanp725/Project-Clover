---
name: audio-intake
description: Analyse, trim, normalise and install a sound as a game clip with ffmpeg, then measure it with the Loudness Report. Use when adding, replacing or generating a sound effect, ambience, voice line or music track.
---

# Audio intake

Tools: `ffmpeg` and `ffprobe` on PATH. Originals stay in `staging/audio/<date>_<name>/` with a `notes.md` (source, licence,
prompt and seed when generated, what was done to it).

1. **Inspect**: `ffprobe -hide_banner <in>` (rate, channels, bit depth, duration) and
   `ffmpeg -hide_banner -i <in> -af ebur128=peak=true -f null -` (integrated LUFS, loudness range, true peak). Say what you
   heard about in words: spectrum, noise floor, clicks at the ends.
2. **Decide the folder** from `.claude/rules/audio.md` (Music, Ambience, SFX, Voice, UI); that sets the target LUFS.
3. **Process** in one ffmpeg call where possible, writing 48 kHz WAV:
   - trim leading silence: `-af silenceremove=start_periods=1:start_threshold=-50dB`;
   - mono for 3D SFX and voice: `-ac 1`; keep stereo for music, ambience and UI;
   - loudness: two-pass `loudnorm` with `I=<target>:TP=-1.5:LRA=11` (first pass `print_format=json`, second pass with the
     `measured_*` values and `linear=true`), or a plain gain `volume=<dB>dB` when the clip is short and already clean;
   - loops: cut on zero crossings and check the seam with `-af "aloop=loop=2:size=<samples>"` rendered to a check file;
   - fades: `afade=t=in:d=0.01,afade=t=out:st=<len-0.02>:d=0.02` on one-shots to kill clicks;
   - output `-c:a pcm_s16le -ar 48000`.
4. **Install**: copy to `Assets/_Project/Audio/<Folder>/<kind>_<thing>_<variant>.wav` with
   `unity command assets import -- --source <processed.wav> --path <asset path>`; `AssetImportRules` sets load type and format.
5. **Manifest row** in `Assets/_Project/ASSETS.csv` (`path,kind=audio,source,licence,credit,notes,seed,date,approved_by`).
6. **Measure in Unity**: `unity command editor invoke_menu -- --path "Clover/Audio/Loudness Report"`, then read
   `review/audio/loudness.md` for the new clip's row: verdict `ok`, no `clipping`.
7. **Hear it in context** when it is a gameplay sound: wire it with `component set_field`, run a play-mode probe that
   triggers it, and ask the user to listen; loudness numbers do not replace ears.

Report: the ffmpeg commands run, before and after LUFS and peak, the asset path, the manifest row, the report verdict.
