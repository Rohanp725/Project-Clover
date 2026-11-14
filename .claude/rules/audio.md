---
paths:
  - "Assets/_Project/Audio/**"
  - "Assets/_Project/Scripts/Editor/Loudness.cs"
  - "review/audio/**"
---

# Audio

- Folders decide import settings and loudness targets: `Audio/Music` (streaming Vorbis, -16 LUFS), `Audio/Ambience`
  (streaming Vorbis, -30 LUFS), `Audio/SFX` (ADPCM, decompress on load, mono, -20 LUFS), `Audio/Voice` (Vorbis in memory,
  mono, -20 LUFS), `Audio/UI` (ADPCM, -22 LUFS). Targets are per asset, not a mix level, and mean "clips of one kind sit
  near each other"; tune them in `Loudness.cs` as the mix settles. Mixer assets live in `Audio/Mixers`.
- Sources are 48 kHz, 16- or 24-bit WAV, named `<kind>_<thing>_<variant>`: `sfx_door_wood_open_01.wav`,
  `amb_forest_night_loop.wav`, `mus_title_theme.wav`, `vo_<character>_<line_id>.wav`, `ui_click_01.wav`. Loops are trimmed to
  zero crossings and say `_loop`. Unity compresses on import; keep the WAV as the source so the loudness meter can read it.
- Every clip has a row in `Assets/_Project/ASSETS.csv` with its source (`recorded`, `stable_audio`, `ace_step`, `kokoro`,
  `dia`, `freesound_cc0`, `hand`) and licence. Voice clones need written consent; the consent note goes in the row.
- Measuring and fixing with ffmpeg (on PATH):
  `ffmpeg -i in.wav -af ebur128=peak=true -f null -` prints integrated LUFS and true peak;
  `ffmpeg -i in.wav -af loudnorm=I=-20:TP=-1.5:LRA=11:print_format=summary -f null -` then the second pass with the measured
  values gives a clip at target. Trim: `-af silenceremove=start_periods=1:start_threshold=-50dB`. Keep the originals in `staging/audio/`.
- `Clover > Audio > Loudness Report` writes `review/audio/loudness.md` (LUFS, peak, verdict per clip). Run it after adding
  or replacing clips; clipping (peak above -0.3 dBFS) is always fixed.
- In the game, 3D sounds are mono with a sensible rolloff; stereo is for music, ambience beds and UI. One `AudioSource`
  per voice, pooled; no `PlayClipAtPoint` in hot paths.
