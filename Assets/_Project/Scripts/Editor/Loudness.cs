using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Integrated loudness (ITU-R BS.1770-4, the LUFS number) and sample peak for every clip under Assets/_Project/Audio, so
    /// one report says which clips sit too loud or too quiet for their folder. Targets are per asset, not a mix level: they
    /// only mean "clips of one kind sit near each other". Tune them as the mix settles.
    /// WAV files are read directly, so the import settings do not matter; other formats go through AudioClip.GetData and
    /// fail for streaming clips, which is one reason sources are WAV.
    /// </summary>
    public static class Loudness
    {
        public struct Result { public bool ok; public float lufs, peakDb; public string why; }

        public const string AudioDir = "Assets/_Project/Audio";
        public const string ReportPath = "review/audio/loudness.md";

        /// <summary>Folder under Audio/ -> target integrated loudness (LUFS).</summary>
        public static readonly Dictionary<string, float> Targets = new Dictionary<string, float>
        {
            ["Music"] = -16f,
            ["Ambience"] = -30f,
            ["SFX"] = -20f,
            ["Voice"] = -20f,
            ["UI"] = -22f,
        };
        public const float DefaultTarget = -20f;
        public const float Tolerance = 4f;   // LU either side before a row says quiet or loud
        public const float ClipPeakDb = -0.3f;

        private static readonly Dictionary<string, (long stamp, Result result)> cache = new Dictionary<string, (long, Result)>();

        /// <summary>The first folder under Audio/, which is the clip's category; null outside Audio/.</summary>
        public static string CategoryOf(string assetPath)
        {
            string p = assetPath.Replace('\\', '/');
            if (!p.StartsWith(AudioDir + "/")) return null;
            string rest = p.Substring(AudioDir.Length + 1);
            int slash = rest.IndexOf('/');
            return slash < 0 ? "(root)" : rest.Substring(0, slash);
        }

        public static float TargetFor(string category) => category != null && Targets.TryGetValue(category, out var t) ? t : DefaultTarget;

        public static Result Of(string assetPath)
        {
            long stamp = File.Exists(assetPath) ? File.GetLastWriteTimeUtc(assetPath).Ticks : 0;
            if (cache.TryGetValue(assetPath, out var c) && c.stamp == stamp) return c.result;
            var r = Measure(assetPath);
            if (r.ok || r.why != "loading") cache[assetPath] = (stamp, r);
            return r;
        }

        private static Result Measure(string assetPath)
        {
            try
            {
                float[] data; int channels, rate;
                if (assetPath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) && ReadWav(assetPath, out data, out channels, out rate))
                    return Compute(data, channels, rate);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (clip == null) return new Result { why = "no clip" };
                if (clip.loadState != AudioDataLoadState.Loaded)
                {
                    clip.LoadAudioData();
                    if (clip.loadState != AudioDataLoadState.Loaded) return new Result { why = "loading" };
                }
                data = new float[clip.samples * clip.channels];
                if (!clip.GetData(data, 0)) return new Result { why = "GetData failed (streaming clip; use a WAV source)" };
                return Compute(data, clip.channels, clip.frequency);
            }
            catch (Exception e) { return new Result { why = e.Message }; }
        }

        // ------------------------------------------------------------------------------------------ BS.1770-4

        public static Result Compute(float[] interleaved, int channels, int sampleRate)
        {
            if (channels < 1 || sampleRate < 8000 || interleaved.Length < channels) return new Result { why = "empty" };
            int frames = interleaved.Length / channels;
            float peak = 0f;
            for (int i = 0; i < interleaved.Length; i++) { float a = Math.Abs(interleaved[i]); if (a > peak) peak = a; }

            // K-weighting: a high shelf then a high-pass, with coefficients derived for this sample rate (as pyloudnorm does).
            var shelf = Biquad.HighShelf(sampleRate, 1681.974450955533, 3.999843853973347, 0.7071752369554196);
            var hp = Biquad.HighPass(sampleRate, 38.13547087602444, 0.5003270373238773);

            int block = Math.Max(1, (int)Math.Round(0.4 * sampleRate));
            int hop = Math.Max(1, (int)Math.Round(0.1 * sampleRate));
            if (frames < block) { block = frames; hop = frames; }
            int blocks = 1 + (frames - block) / hop;

            // Per channel: filter, then prefix sums of squares, so a block's mean square is two lookups.
            var energy = new double[blocks];
            for (int ch = 0; ch < channels; ch++)
            {
                double weight = channels >= 5 && (ch == 3 || ch == 4) ? 1.41 : 1.0;   // surround sides count more
                var s1 = shelf.Fresh();
                var s2 = hp.Fresh();
                var prefix = new double[frames + 1];
                for (int n = 0; n < frames; n++)
                {
                    double y = s2.Step(s1.Step(interleaved[n * channels + ch]));
                    prefix[n + 1] = prefix[n] + y * y;
                }
                for (int b = 0; b < blocks; b++)
                {
                    int start = b * hop;
                    energy[b] += weight * (prefix[start + block] - prefix[start]) / block;
                }
            }

            // Gating: drop blocks under -70 LUFS, then blocks more than 10 LU under the mean of what is left.
            const double offset = -0.691;
            double Lk(double z) => z > 0 ? offset + 10.0 * Math.Log10(z) : double.NegativeInfinity;
            var above = energy.Where(z => Lk(z) > -70.0).ToArray();
            if (above.Length == 0) return new Result { why = "silent", peakDb = Db(peak) };
            double relative = Lk(above.Average()) - 10.0;
            var kept = above.Where(z => Lk(z) > relative).ToArray();
            if (kept.Length == 0) kept = above;
            return new Result { ok = true, lufs = (float)Lk(kept.Average()), peakDb = Db(peak) };
        }

        private static float Db(float amplitude) => amplitude > 0f ? 20f * (float)Math.Log10(amplitude) : -120f;

        private class Biquad
        {
            public double b0, b1, b2, a1, a2;
            private double x1, x2, y1, y2;

            public Biquad Fresh() => new Biquad { b0 = b0, b1 = b1, b2 = b2, a1 = a1, a2 = a2 };

            public double Step(double x)
            {
                double y = b0 * x + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
                x2 = x1; x1 = x; y2 = y1; y1 = y;
                return y;
            }

            public static Biquad HighShelf(int fs, double f0, double gainDb, double q)
            {
                double A = Math.Pow(10.0, gainDb / 40.0), w0 = 2.0 * Math.PI * f0 / fs, alpha = Math.Sin(w0) / (2.0 * q), c = Math.Cos(w0);
                double sa = 2.0 * Math.Sqrt(A) * alpha;
                double a0 = (A + 1) - (A - 1) * c + sa;
                return new Biquad
                {
                    b0 = A * ((A + 1) + (A - 1) * c + sa) / a0,
                    b1 = -2 * A * ((A - 1) + (A + 1) * c) / a0,
                    b2 = A * ((A + 1) + (A - 1) * c - sa) / a0,
                    a1 = 2 * ((A - 1) - (A + 1) * c) / a0,
                    a2 = ((A + 1) - (A - 1) * c - sa) / a0,
                };
            }

            public static Biquad HighPass(int fs, double f0, double q)
            {
                double w0 = 2.0 * Math.PI * f0 / fs, alpha = Math.Sin(w0) / (2.0 * q), c = Math.Cos(w0);
                double a0 = 1 + alpha;
                return new Biquad { b0 = (1 + c) / 2 / a0, b1 = -(1 + c) / a0, b2 = (1 + c) / 2 / a0, a1 = -2 * c / a0, a2 = (1 - alpha) / a0 };
            }
        }

        // ------------------------------------------------------------------------------------------ WAV

        /// <summary>PCM 8/16/24/32-bit and 32-bit float WAV (plain or WAVE_FORMAT_EXTENSIBLE). False for anything else.</summary>
        public static bool ReadWav(string path, out float[] data, out int channels, out int rate)
        {
            data = null; channels = 0; rate = 0;
            var bytes = File.ReadAllBytes(path);
            if (bytes.Length < 12 || Encoding.ASCII.GetString(bytes, 0, 4) != "RIFF" || Encoding.ASCII.GetString(bytes, 8, 4) != "WAVE") return false;
            int pos = 12, format = 0, bits = 0, dataStart = -1, dataLen = 0;
            while (pos + 8 <= bytes.Length)
            {
                string id = Encoding.ASCII.GetString(bytes, pos, 4);
                int len = BitConverter.ToInt32(bytes, pos + 4);
                int body = pos + 8;
                if (id == "fmt ")
                {
                    format = BitConverter.ToUInt16(bytes, body);
                    channels = BitConverter.ToUInt16(bytes, body + 2);
                    rate = BitConverter.ToInt32(bytes, body + 4);
                    bits = BitConverter.ToUInt16(bytes, body + 14);
                    if (format == 0xFFFE && len >= 26) format = BitConverter.ToUInt16(bytes, body + 24);   // extensible: the sub-format's first two bytes
                }
                else if (id == "data") { dataStart = body; dataLen = Math.Min(len, bytes.Length - body); }
                if (len < 0) return false;
                pos = body + len + (len & 1);
            }
            if (dataStart < 0 || channels == 0 || rate == 0) return false;
            int bytesPer = bits / 8;
            if (bytesPer == 0) return false;
            int count = dataLen / bytesPer;
            data = new float[count];
            if (format == 1)
            {
                switch (bits)
                {
                    case 8: for (int i = 0; i < count; i++) data[i] = (bytes[dataStart + i] - 128) / 128f; break;
                    case 16: for (int i = 0; i < count; i++) data[i] = BitConverter.ToInt16(bytes, dataStart + i * 2) / 32768f; break;
                    case 24:
                        for (int i = 0; i < count; i++)
                        {
                            int o = dataStart + i * 3;
                            int v = bytes[o] | (bytes[o + 1] << 8) | ((sbyte)bytes[o + 2] << 16);
                            data[i] = v / 8388608f;
                        }
                        break;
                    case 32: for (int i = 0; i < count; i++) data[i] = BitConverter.ToInt32(bytes, dataStart + i * 4) / 2147483648f; break;
                    default: return false;
                }
                return true;
            }
            if (format == 3 && bits == 32)
            {
                for (int i = 0; i < count; i++) data[i] = BitConverter.ToSingle(bytes, dataStart + i * 4);
                return true;
            }
            return false;
        }

        // ------------------------------------------------------------------------------------------ Report

        private static readonly string[] ClipExtensions = { ".wav", ".ogg", ".mp3", ".flac", ".aif", ".aiff" };

        /// <summary>Every clip under Audio/ with its loudness, peak, folder target and a verdict, as review/audio/loudness.md.</summary>
        [MenuItem("Clover/Audio/Loudness Report")]
        public static void WriteReport()
        {
            var rows = new List<(string category, string file, Result r)>();
            if (Directory.Exists(AudioDir))
            {
                foreach (var path in Directory.GetFiles(AudioDir, "*", SearchOption.AllDirectories)
                             .Where(p => ClipExtensions.Contains(Path.GetExtension(p).ToLowerInvariant())).OrderBy(p => p, StringComparer.Ordinal))
                {
                    string asset = path.Replace('\\', '/');
                    rows.Add((CategoryOf(asset), asset.Substring(AudioDir.Length + 1), Of(asset)));
                }
            }

            int quiet = 0, loud = 0, clipping = 0;
            var body = new StringBuilder();
            foreach (var group in rows.GroupBy(r => r.category).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                float target = TargetFor(group.Key);
                body.AppendLine($"## {group.Key} (target {target:F0} LUFS)");
                body.AppendLine();
                body.AppendLine("| clip | LUFS | peak dBFS | verdict |");
                body.AppendLine("|---|---|---|---|");
                foreach (var (category, file, r) in group)
                {
                    string verdict;
                    if (!r.ok) verdict = r.why;
                    else if (r.lufs - target > Tolerance) { verdict = $"loud by {r.lufs - target:F1}"; loud++; }
                    else if (target - r.lufs > Tolerance) { verdict = $"quiet by {target - r.lufs:F1}"; quiet++; }
                    else verdict = "ok";
                    if (r.ok && r.peakDb > ClipPeakDb) { verdict += ", clipping"; clipping++; }
                    string lufs = r.ok ? r.lufs.ToString("F1") : "-";
                    string peak = r.ok || r.why == "silent" ? r.peakDb.ToString("F1") : "-";
                    body.AppendLine($"| {file} | {lufs} | {peak} | {verdict} |");
                }
                body.AppendLine();
            }

            var sb = new StringBuilder();
            sb.AppendLine("# Loudness of the sounds in the game");
            sb.AppendLine();
            sb.AppendLine($"Written {DateTime.Now:yyyy-MM-dd HH:mm} by Clover > Audio > Loudness Report. Integrated loudness per clip (BS.1770-4), sample peak in dBFS, and the folder target from Loudness.cs (tolerance +/-{Tolerance:F0} LU; clipping above {ClipPeakDb:F1} dBFS).");
            sb.AppendLine();
            sb.AppendLine($"{rows.Count} clips: {quiet} quiet, {loud} loud for their folder, {clipping} clipping.");
            sb.AppendLine();
            sb.Append(body);
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, sb.ToString());
            Debug.Log($"[Clover.Audio] {rows.Count} clips measured: {quiet} quiet, {loud} loud, {clipping} clipping. Report: {ReportPath}");
        }
    }
}
