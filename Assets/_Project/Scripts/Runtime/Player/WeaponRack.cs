using System;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Every weapon model the character can carry, hung on the skeleton's sockets by the builder and shown one set at
    /// a time. Each set has what is seen with the weapon in hand and what is seen with it at rest on the body. Parts
    /// named for the accent and the inlay take the held element's colour; the rest stay the muted metal of the world.
    /// </summary>
    public class WeaponRack : MonoBehaviour
    {
        public const string HandRight = "socket_hand_R";
        public const string HandLeft = "socket_hand_L";
        public const string Back = "socket_back";
        public const string Hip = "socket_hip";

        [Serializable]
        public class Set
        {
            public string Carry;
            public GameObject[] InHand;
            public GameObject[] AtRest;
            /// <summary>The parts that take the element's colour, and how brightly each glows with it (the inlay) or is only dyed by it (the accent).</summary>
            public Renderer[] Tinted;
            /// <summary>Which material of each part is the one to colour.</summary>
            public int[] Index;
            public float[] Glow;
            /// <summary>The colours those parts have with no element held.</summary>
            public Color[] Plain;
        }

        [SerializeField] private Set[] sets = new Set[0];

        private static readonly int BaseColour = Shader.PropertyToID("_BaseColor");
        private static readonly int Emission = Shader.PropertyToID("_EmissionColor");

        private MaterialPropertyBlock block;
        private string shownCarry;
        private int shownState = -1;
        private Color shownTint;
        private float shownHeat = -1f;
        private bool tinted;

        public void Configure(Set[] carried) => sets = carried;

        /// <summary>A socket (or any part) by name, anywhere under a root. For Awake, not for every frame.</summary>
        public static Transform FindSocket(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindSocket(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        public bool Has(string carry)
        {
            foreach (var set in sets)
                if (set.Carry == carry) return true;
            return false;
        }

        /// <summary>The weapon's part held in the hand (else the one at rest), shown now or not; null for a carry with none.</summary>
        public GameObject PartFor(string carry)
        {
            if (string.IsNullOrEmpty(carry)) return null;
            foreach (var set in sets)
            {
                if (set.Carry != carry) continue;
                if (set.InHand != null)
                    foreach (var part in set.InHand)
                        if (part != null) return part;
                if (set.AtRest != null)
                    foreach (var part in set.AtRest)
                        if (part != null) return part;
            }
            return null;
        }

        /// <summary>The part of the weapon shown now (in the hand or at rest), or null with none: where a charged blade is drawn. No allocation.</summary>
        public GameObject ShownPart
        {
            get
            {
                if (string.IsNullOrEmpty(shownCarry)) return null;
                foreach (var set in sets)
                {
                    if (set.Carry != shownCarry) continue;
                    var parts = shownState == 1 ? set.InHand : set.AtRest;
                    if (parts == null) return null;
                    foreach (var part in parts)
                        if (part != null && part.activeInHierarchy) return part;
                }
                return null;
            }
        }

        /// <summary>Shows one weapon, in the hand or at rest, and hides the others. Cheap to call every frame: it acts only on a change.</summary>
        public void Show(string carry, bool inHand)
        {
            int state = inHand ? 1 : 0;
            if (carry == shownCarry && state == shownState) return;
            shownCarry = carry;
            shownState = state;
            foreach (var set in sets)
            {
                bool mine = set.Carry == carry;
                Switch(set.InHand, mine && inHand);
                Switch(set.AtRest, mine && !inHand);
            }
        }

        /// <summary>
        /// Gives the accent and the inlay the element's colour. <paramref name="heat"/> is how brightly the inlay
        /// burns with it: dim at rest, bright for a surge or a skill. With no element they go back to plain iron.
        /// </summary>
        public void Tint(bool hasElement, Color colour, float heat)
        {
            if (hasElement == tinted && colour == shownTint && Mathf.Abs(heat - shownHeat) < 0.02f) return;
            tinted = hasElement;
            shownTint = colour;
            shownHeat = heat;
            if (block == null) block = new MaterialPropertyBlock();
            foreach (var set in sets)
            {
                if (set.Tinted == null) continue;
                for (int i = 0; i < set.Tinted.Length; i++)
                {
                    var part = set.Tinted[i];
                    if (part == null) continue;
                    float glow = set.Glow != null && i < set.Glow.Length ? set.Glow[i] : 0f;
                    Color plain = set.Plain != null && i < set.Plain.Length ? set.Plain[i] : Color.grey;
                    int index = set.Index != null && i < set.Index.Length ? set.Index[i] : 0;
                    part.GetPropertyBlock(block, index);
                    block.SetColor(BaseColour, hasElement ? (glow > 0f ? Color.Lerp(plain, colour, 0.6f) : colour) : plain);
                    block.SetColor(Emission, hasElement && glow > 0f ? colour * (glow * heat) : Color.black);
                    part.SetPropertyBlock(block, index);
                }
            }
        }

        private static void Switch(GameObject[] parts, bool shown)
        {
            if (parts == null) return;
            foreach (var part in parts)
                if (part != null && part.activeSelf != shown) part.SetActive(shown);
        }
    }
}
