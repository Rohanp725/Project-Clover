using Clover.Core;
using Clover.Player.Forms;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The charged blade as it is seen on the weapon (IMPLEMENTATION.md 3.3): the element's own stuff along the edge for
    /// as long as the charge lasts, from the built effect pieces. Fire is tongues of flame standing up off the edge with
    /// embers and smoke streaming from it, never a glow of white or gold; Lightning is bolts that jump along it and
    /// crackle; Ice is rime feathers and breath-fog; the others are their element's substance (drops, leaves, grit,
    /// gilt, shade, venom) in its colour. Fewer and smaller as the three blows are spent. The blade is found from the
    /// weapon part shown (its longest axis, the end away from the hand being the tip); bare hands burn at the fists.
    /// </summary>
    [RequireComponent(typeof(PlayerCombat))]
    public class BladeLook : MonoBehaviour
    {
        /// <summary>Pieces held along the edge at a full charge, and seconds between the sparks, embers and smoke it sheds.</summary>
        public int Pieces = 5;
        public float ShedEvery = 0.07f;
        /// <summary>Share of the weapon's length from the hand that is grip, not edge.</summary>
        public float GripShare = 0.28f;

        private const int MostPieces = 6;
        private readonly int[] held = { -1, -1, -1, -1, -1, -1 };
        private PlayerCombat combat;
        private PlayerAnimator animator;
        private Transform handRight, handLeft;
        private ElementInfo shown;
        private FormMesh shape;
        private int shownVersion = -1;
        private int taken;
        private GameObject measured;
        private Transform measuredAt;
        private Vector3 gripLocal, tipLocal;
        private bool hasBlade;
        private float untilShed, untilSmoke;

        /// <summary>For probes: the element shown on the weapon (empty for none) and how many pieces are held.</summary>
        public string ShownElement => shown != null ? shown.Name : "";
        public int PiecesHeld => taken;

        private void Awake()
        {
            combat = GetComponent<PlayerCombat>();
            animator = GetComponent<PlayerAnimator>();
            handRight = WeaponRack.FindSocket(transform, WeaponRack.HandRight);
            handLeft = WeaponRack.FindSocket(transform, WeaponRack.HandLeft);
        }

        private void OnDisable() => Let();

        private void LateUpdate()
        {
            var blade = combat.Blade;
            var element = blade.Charged ? Elements.ByName(blade.Element) : null;
            if (element != shown || (element != null && blade.Version != shownVersion))
            {
                Let();
                shown = element;
                shownVersion = blade.Version;
                if (element != null) Take(element, blade.Left, blade.Blows);
            }
            if (shown == null || taken == 0) return;

            Vector3 grip, tip;
            if (!Edge(out grip, out tip))
            {
                // Bare hands: the element burns at the fists, half the pieces at each.
                PlaceAtFists();
                Shed(handRight != null ? handRight.position : transform.position + Vector3.up, Vector3.up, Vector3.up);
                return;
            }
            Vector3 along = tip - grip;
            float length = along.magnitude;
            Vector3 direction = length > 0.001f ? along / length : Vector3.up;
            Vector3 from = grip + along * GripShare;
            float edge = length * (1f - GripShare);
            for (int i = 0; i < taken; i++)
            {
                float t = taken == 1 ? 0.6f : (i + 0.5f) / taken;
                Place(held[i], from + direction * (edge * t), direction, edge / Mathf.Max(1, taken), i);
            }
            Shed(from + direction * (edge * Random.value), direction, Vector3.up);
        }

        // Takes the pieces for this element, fewer as the blows are spent.
        private void Take(ElementInfo element, int left, int blows)
        {
            shape = ShapeOf(element);
            Color colour = element.Name == Elements.Dark ? ElementSubstance.Shade(element.Colour) : element.Name == Elements.Ice ? ElementSubstance.Pale(element.Colour) : element.Colour;
            int count = Mathf.Clamp(Mathf.CeilToInt(Pieces * Mathf.Max(1, left) / (float)Mathf.Max(1, blows)) + 1, 2, MostPieces);
            for (int i = 0; i < count; i++)
            {
                held[i] = FormFx.Hold(shape, colour, 0.02f);
                if (held[i] >= 0) taken = i + 1;
            }
            // Embers and sparks start at once, so a charge is seen on the frame it lands.
            untilShed = 0f;
            untilSmoke = 0f;
        }

        private void Let()
        {
            for (int i = 0; i < held.Length; i++)
            {
                if (held[i] >= 0) FormFx.Release(held[i], 0.15f);
                held[i] = -1;
            }
            taken = 0;
            shown = null;
        }

        // What the edge is made of: flame stands up off it, a bolt runs along it, rime feathers stand on it; the others are their substance's splinter.
        private static FormMesh ShapeOf(ElementInfo element)
        {
            switch (element.Name)
            {
                case Elements.Fire: return FormMesh.Flame;
                case Elements.Lightning: return FormMesh.Bolt;
                case Elements.Ice: return FormMesh.Crystal;
                case Elements.Life: return FormMesh.Leaf;
                default: return ElementSubstance.Shape(FormMesh.Shard, element.Index);
            }
        }

        private void Place(int handle, Vector3 at, Vector3 direction, float span, int index)
        {
            if (handle < 0) return;
            switch (shape)
            {
                case FormMesh.Flame:
                    // Flame stands up off the edge whatever angle the blade is at, as fire does.
                    FormFx.Place(handle, at, Quaternion.Euler(0f, index * 67f, 0f), new Vector3(0.17f, 0.42f + 0.06f * (index % 2), 0.17f));
                    break;
                case FormMesh.Bolt:
                    // A bolt along each length of the edge, jumping to a new shape many times a second.
                    FormFx.Place(handle, at, Quaternion.LookRotation(direction, Vector3.up), new Vector3(0.09f, 0.09f, span * 1.3f));
                    break;
                case FormMesh.Crystal:
                    // Rime feathers stand out from the edge, a little askew.
                    FormFx.Place(handle, at, Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(70f + index * 13f, index * 90f, 0f), new Vector3(0.09f, 0.22f, 0.09f));
                    break;
                default:
                    FormFx.Place(handle, at, Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(0f, 0f, index * 50f), new Vector3(0.12f, 0.12f, Mathf.Min(0.3f, span)));
                    break;
            }
        }

        private void PlaceAtFists()
        {
            for (int i = 0; i < taken; i++)
            {
                var hand = (i % 2 == 0 ? handRight : handLeft) ?? handRight;
                Vector3 at = hand != null ? hand.position : transform.position + Vector3.up * (1f + i * 0.1f);
                Place(held[i], at + Vector3.up * (0.06f * (i / 2)), Vector3.up, 0.25f, i);
            }
        }

        // What the edge sheds as it moves: embers and smoke for fire, sparks for lightning, fog for ice, motes of the colour for the rest.
        private void Shed(Vector3 at, Vector3 along, Vector3 up)
        {
            untilShed -= Time.deltaTime;
            untilSmoke -= Time.deltaTime;
            if (untilShed > 0f) return;
            untilShed = ShedEvery;
            switch (shown.Name)
            {
                case Elements.Fire:
                    // Embers and small tongues of flame; the scene's smoke puffs are too large and pale for an edge.
                    Vfx.Play(VfxKind.Embers, at, Vector3.up, 0.15f);
                    if (untilSmoke <= 0f)
                    {
                        untilSmoke = 0.25f;
                        Vfx.Play(VfxKind.Fire, at, Vector3.up, 0.1f);
                    }
                    break;
                case Elements.Lightning:
                    if (untilSmoke <= 0f)
                    {
                        untilSmoke = 0.18f;
                        Vfx.Play(VfxKind.Shock, at, along, 0.12f);
                    }
                    break;
                case Elements.Ice:
                    if (untilSmoke <= 0f)
                    {
                        untilSmoke = 0.22f;
                        Vfx.Play(VfxKind.TintPuff, at, Vector3.down, 0.08f, ElementSubstance.Pale(shown.Colour));
                    }
                    break;
                case Elements.Dark:
                    if (untilSmoke <= 0f)
                    {
                        untilSmoke = 0.25f;
                        Vfx.Play(VfxKind.TintPuff, at, Vector3.down, 0.1f, ElementSubstance.Shade(shown.Colour));
                    }
                    break;
                default:
                    if (untilSmoke <= 0f)
                    {
                        untilSmoke = 0.2f;
                        Vfx.Play(VfxKind.TintSpark, at, along, 0.12f, shown.Colour);
                    }
                    break;
            }
        }

        // The edge from the hand to the tip, measured once for each weapon part shown.
        private bool Edge(out Vector3 grip, out Vector3 tip)
        {
            grip = tip = Vector3.zero;
            var part = animator != null ? animator.ShownWeapon : null;
            if (part != measured) Measure(part);
            if (!hasBlade || measuredAt == null) return false;
            grip = measuredAt.TransformPoint(gripLocal);
            tip = measuredAt.TransformPoint(tipLocal);
            return true;
        }

        // The longest axis of the biggest piece of the part is the weapon's length; the end nearer the hand is the grip.
        private void Measure(GameObject part)
        {
            measured = part;
            hasBlade = false;
            measuredAt = null;
            if (part == null) return;
            Renderer biggest = null;
            float most = 0f;
            foreach (var piece in part.GetComponentsInChildren<Renderer>())
            {
                float size = piece.localBounds.size.sqrMagnitude * piece.transform.lossyScale.sqrMagnitude;
                if (size > most)
                {
                    most = size;
                    biggest = piece;
                }
            }
            if (biggest == null) return;
            var bounds = biggest.localBounds;
            Vector3 extent = bounds.extents;
            Vector3 axis = extent.x >= extent.y && extent.x >= extent.z ? new Vector3(extent.x, 0f, 0f) : extent.y >= extent.z ? new Vector3(0f, extent.y, 0f) : new Vector3(0f, 0f, extent.z);
            Vector3 one = bounds.center - axis, other = bounds.center + axis;
            measuredAt = biggest.transform;
            Vector3 hand = handRight != null ? handRight.position : transform.position;
            bool oneNearer = (measuredAt.TransformPoint(one) - hand).sqrMagnitude <= (measuredAt.TransformPoint(other) - hand).sqrMagnitude;
            gripLocal = oneNearer ? one : other;
            tipLocal = oneNearer ? other : one;
            hasBlade = true;
        }
    }
}
