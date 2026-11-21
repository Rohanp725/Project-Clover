using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// How a landed blow is marked on what it struck when the weapon carries an element: the owner's choice of
    /// 2026-11-19, "Tint and soften". Under an element the white flash on the body and the white cut streak covered most
    /// of the element's swing trail at the moment of contact, so both take the colour the trail is drawn in, softened,
    /// and the trail shows through. A plain weapon's blow passes a clear tint and keeps its white at full strength. The
    /// yellow ring of a finisher or a stagger is a gameplay signal and is not drawn here.
    /// </summary>
    public static class StruckLook
    {
        /// <summary>The owner's values: about half, so the trail shows through and the blow still reads as one.</summary>
        public const float ChosenTintStrength = 0.5f, ChosenFlashStrength = 0.5f;

        /// <summary>How strongly the cut streak is drawn under an element, as a share of a plain blow's (its brightness and its cover).</summary>
        public static float TintStrength = ChosenTintStrength;

        /// <summary>How bright the body's flash is under an element, as a share of a plain blow's white.</summary>
        public static float FlashStrength = ChosenFlashStrength;

        // Domain reload is off: a value tuned live in one play must not carry into the next.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            TintStrength = ChosenTintStrength;
            FlashStrength = ChosenFlashStrength;
        }

        /// <summary>True for an element's tint; a plain blow carries a clear one (alpha zero).</summary>
        public static bool Tinted(Color tint) => tint.a > 0f;

        /// <summary>
        /// The most the palest channel of a tinted stroke and a tinted flash may reach. Lit to half a plain blow's
        /// brightness, a pale hue (Ice's, Earth's, Metal's, Light's, Lightning's) had every channel past one and came out
        /// white on film (the swing films of 2026-11-20); held under these, it keeps its hue and is only less bright.
        /// </summary>
        public const float PalestStroke = 0.6f, PalestFlash = 0.3f;

        private static float Peak(Color colour) => Mathf.Max(colour.r, Mathf.Max(colour.g, colour.b));
        private static float Palest(Color colour) => Mathf.Min(colour.r, Mathf.Min(colour.g, colour.b));

        // The tint's hue lit to the given brightness at its strongest channel, so a dark element's colour is not lost,
        // and no brighter than keeps its palest channel at the most given, so a pale one is not lit to white.
        private static Color Lit(Color tint, float brightness, float alpha, float palest)
        {
            float peak = Peak(tint);
            float scale = peak > 1e-4f ? brightness / peak : 0f;
            float low = Palest(tint);
            if (low * scale > palest) scale = palest / low;
            return new Color(tint.r * scale, tint.g * scale, tint.b * scale, alpha);
        }

        /// <summary>The body's flash: <paramref name="white"/> for a plain blow, the element's colour at <see cref="FlashStrength"/> of its brightness under one (at most, see <see cref="PalestFlash"/>).</summary>
        public static Color Flash(Color white, Color tint) => Tinted(tint) ? Lit(tint, Peak(white) * FlashStrength, white.a, PalestFlash) : white;

        /// <summary>The bright stroke of the cut streak: <paramref name="plain"/> for a plain blow, the element's colour at <see cref="TintStrength"/> under one (at most, see <see cref="PalestStroke"/>).</summary>
        public static Color Stroke(Color plain, Color tint) => Tinted(tint) ? Lit(tint, Peak(plain) * TintStrength, plain.a * TintStrength, PalestStroke) : plain;

        /// <summary>
        /// A colour to throw a bright one-shot effect in (a heavy blow's star, a glint, a crackle, whose own material is
        /// lit far past white): the element's hue at <paramref name="brightness"/> at its strongest channel, its palest
        /// held under half of that so the material's glow takes the hue and not white (the swing films of 2026-11-20:
        /// white starbursts where Metal's and Lightning's blows landed).
        /// </summary>
        public static Color Glow(Color tint, float brightness) => Lit(tint, brightness, 1f, brightness * 0.45f);

        /// <summary>The ink under the cut streak: unchanged for a plain blow, as faint as the stroke under an element.</summary>
        public static Color Ink(Color plain, Color tint) => Tinted(tint) ? new Color(plain.r, plain.g, plain.b, plain.a * TintStrength) : plain;
    }
}
