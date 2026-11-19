using System;

namespace Clover.Player.Magic
{
    /// <summary>
    /// The charged blade (STEEL_AND_ELEMENT.md 3.3; IMPLEMENTATION.md 3.3): a slot spell that lands charges the blade, and
    /// the next three weapon blows carry its element, each laying the element's affliction and striking 20% harder; the
    /// third does its element's one extra thing. A fresh charge replaces the old one. Infused strikes, reactions and
    /// effects never charge it (the caller only charges on a slot spell's landing). Counted per move: a swing that lands
    /// on three enemies spends one charge and carries it to all three. Plain C#, no allocation.
    /// </summary>
    [Serializable]
    public class ChargedBlade
    {
        /// <summary>Blows a charge lasts, and what each multiplies its damage by.</summary>
        public int Blows = 3;
        public float DamageShare = 1.2f;
        /// <summary>The Edge Kept Bright and The Edge Drinks will set this (Phase 6); Normal until then.</summary>
        public BladeMode Mode = BladeMode.Normal;

        /// <summary>The element the blade carries, empty when it carries none.</summary>
        public string Element { get; private set; } = "";
        /// <summary>Charged blows still to come.</summary>
        public int Left { get; private set; }
        /// <summary>How many blows the current charge was given, so a longer charge still numbers its blows from one.</summary>
        public int Length { get; private set; }
        public bool Charged => Left > 0 && Element.Length > 0;
        /// <summary>Goes up on every charge and every blow spent, so a sign can redraw only when it changes.</summary>
        public int Version { get; private set; }
        /// <summary>Counts for probes.</summary>
        public int Charges { get; private set; }
        public int Spent { get; private set; }

        private int used;

        /// <summary>The blade takes the element for this many blows (the usual three when none is said). It replaces any charge already there.</summary>
        public void Charge(string element, int blows = 0)
        {
            if (string.IsNullOrEmpty(element)) return;
            Element = element;
            Left = blows > 0 ? blows : Blows;
            Length = Left;
            used = 0;
            Charges++;
            Version++;
        }

        /// <summary>
        /// A weapon move landed with the blade charged: says what it carries and counts it down. With nothing on the
        /// blade it returns an empty blow (<see cref="ChargedBlow.Carried"/> false) and changes nothing.
        /// </summary>
        public ChargedBlow OnBlowLanded()
        {
            if (!Charged) return default;
            // Numbered by blows spent rather than blows left, so a blade that never fades still finds its third.
            used++;
            bool third = Mode == BladeMode.NeverFades ? used % Blows == 0 : Left == 1;
            var blow = new ChargedBlow(Element, DamageShare, third, Mode == BladeMode.NeverFades ? (used - 1) % Blows + 1 : used);
            if (Mode != BladeMode.NeverFades) Left--;
            Spent++;
            Version++;
            if (Left <= 0) Element = "";
            return blow;
        }

        /// <summary>The blade goes plain: a rest, a fall, a new scene.</summary>
        public void Clear()
        {
            if (Left == 0 && Element.Length == 0) return;
            Left = 0;
            Element = "";
            Version++;
        }
    }

    /// <summary>How a charged blade behaves; the Heresies and keystones that change it set this.</summary>
    public enum BladeMode
    {
        Normal,
        /// <summary>The Edge Kept Bright: a critical blow lights it as a landed spell does.</summary>
        LitByCritical,
        /// <summary>The Edge Drinks: it never runs out.</summary>
        NeverFades,
    }

    /// <summary>What one charged blow carries: the element, its damage share, whether it is the third, and which of the three it is.</summary>
    public readonly struct ChargedBlow
    {
        public readonly string Element;
        public readonly float DamageShare;
        public readonly bool Third;
        public readonly int Number;
        public bool Carried => !string.IsNullOrEmpty(Element);

        public ChargedBlow(string element, float damageShare, bool third, int number)
        {
            Element = element;
            DamageShare = damageShare;
            Third = third;
            Number = number;
        }
    }
}
