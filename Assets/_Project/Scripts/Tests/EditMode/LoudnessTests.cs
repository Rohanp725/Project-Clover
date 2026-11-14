using System;
using Clover.EditorTools;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>The loudness meter against signals whose answer is known, so a change to the filters is caught.</summary>
    public class LoudnessTests
    {
        private static float[] Sine(float amplitude, float hz, int rate, float seconds, int channels)
        {
            int frames = (int)(rate * seconds);
            var data = new float[frames * channels];
            for (int n = 0; n < frames; n++)
            {
                float v = amplitude * (float)Math.Sin(2 * Math.PI * hz * n / rate);
                for (int c = 0; c < channels; c++) data[n * channels + c] = v;
            }
            return data;
        }

        [Test]
        public void FullScaleThousandHertzSineIsAboutMinusThreeLufsPerChannel()
        {
            // BS.1770: a 0 dBFS 1 kHz sine in one channel reads -3.01 LUFS; the K-weighting is flat at 1 kHz.
            var r = Loudness.Compute(Sine(1f, 1000f, 48000, 2f, 1), 1, 48000);
            Assert.That(r.ok, Is.True, r.why);
            Assert.That(r.lufs, Is.EqualTo(-3.0f).Within(0.3f));
            Assert.That(r.peakDb, Is.EqualTo(0f).Within(0.05f));
        }

        [Test]
        public void StereoCopyOfTheSameSineReadsThreeDecibelsLouder()
        {
            var mono = Loudness.Compute(Sine(0.5f, 1000f, 48000, 2f, 1), 1, 48000);
            var stereo = Loudness.Compute(Sine(0.5f, 1000f, 48000, 2f, 2), 2, 48000);
            Assert.That(stereo.lufs - mono.lufs, Is.EqualTo(3.0f).Within(0.2f));
        }

        [Test]
        public void SilenceIsReportedNotMeasured()
        {
            var r = Loudness.Compute(new float[48000], 1, 48000);
            Assert.That(r.ok, Is.False);
            Assert.That(r.why, Is.EqualTo("silent"));
        }

        [Test]
        public void FolderTargetsComeFromThePathUnderAudio()
        {
            Assert.That(Loudness.CategoryOf("Assets/_Project/Audio/Music/mus_title.wav"), Is.EqualTo("Music"));
            Assert.That(Loudness.CategoryOf("Assets/_Project/Audio/SFX/doors/sfx_door_01.wav"), Is.EqualTo("SFX"));
            Assert.That(Loudness.CategoryOf("Assets/_Project/Art/x.png"), Is.Null);
            Assert.That(Loudness.TargetFor("Ambience"), Is.EqualTo(-30f));
            Assert.That(Loudness.TargetFor("Unknown"), Is.EqualTo(Loudness.DefaultTarget));
        }
    }
}
