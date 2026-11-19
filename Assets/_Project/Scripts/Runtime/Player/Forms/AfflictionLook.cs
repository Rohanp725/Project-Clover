using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// How each affliction is seen on the body that carries it, beyond the coloured pip under its bars: flames
    /// licking up a burning one, lightning jumping over a charged one, air circling the feet of an unsteady one,
    /// frost crystals on a chilled one (a cage of them when it is frozen), water running off a soaked one, a vine
    /// and a flower on a seeded one, stones hopping round a shaken one, splinters of plate falling from a sundered
    /// one, a ray standing over a marked one, smoke sinking onto a cursed one, bubbles rising off a poisoned one.
    /// Each call throws one small piece that looks after itself; whoever watches the body calls a few times a second.
    /// </summary>
    public static class AfflictionLook
    {
        /// <summary>Seconds between pieces for one affliction on one body: quicker the deeper it lies.</summary>
        public static float Interval(int depth) => Mathf.Max(0.16f, 0.42f - 0.05f * depth);

        /// <summary>Throws one piece of an affliction's look on a body standing at a point. <paramref name="beat"/> is any number that changes from call to call.</summary>
        public static void Show(AfflictionKind kind, int depth, bool frozen, Vector3 feet, int beat)
        {
            // The approved effect art, unless the owner's switch has the old effects showing.
            if (AfflictionArt.Show(kind, depth, frozen, feet, beat)) return;
            Color colour = Afflictions.Spec(kind).Colour;
            float turn = ElementSubstance.Hash(beat) * 360f;
            Vector3 round = FormRules.OnRing(0, 1, 0.42f, turn);
            float more = 1f + 0.12f * depth;
            switch (kind)
            {
                case AfflictionKind.Burn:
                    FormFx.Erupt(FormMesh.Flame, feet + round + Vector3.up * (0.2f + 0.9f * ElementSubstance.Hash(beat + 1)), Quaternion.Euler(0f, turn, 0f), new Vector3(0.45f, 0.8f * more, 0.45f), colour, 0.06f, 0.14f, 0.2f, 0.025f);
                    break;
                case AfflictionKind.Static:
                {
                    Vector3 at = feet + round * 0.8f + Vector3.up * (0.3f + 1.2f * ElementSubstance.Hash(beat + 1));
                    FormFx.Fly(FormMesh.Bolt, at, at, Quaternion.Euler(ElementSubstance.Hash(beat + 2) * 360f, turn, 0f), new Vector3(4.5f, 4.5f, 1.5f * more), Color.Lerp(colour, Color.white, 0.6f), 0.26f, 0.025f);
                    break;
                }
                case AfflictionKind.Unsteady:
                {
                    Vector3 along = Vector3.Cross(Vector3.up, round).normalized;
                    Vector3 at = feet + round * 1.5f + Vector3.up * 0.35f;
                    FormFx.Fly(FormMesh.Gust, at, at + along * 1.1f, Quaternion.LookRotation(along), new Vector3(3f, 3f, 1.7f * more), ElementSubstance.Pale(colour), 0.4f, 0.03f);
                    break;
                }
                case AfflictionKind.Chill:
                    if (frozen) FormFx.Erupt(FormMesh.Crystal, feet + round * 1.3f, Quaternion.Euler(0f, turn + 180f, 0f) * Quaternion.Euler(-14f, 0f, 0f), new Vector3(0.9f, 1.9f, 0.9f), Color.white, 0.05f, 0.5f, 0.15f);
                    else FormFx.Erupt(FormMesh.Crystal, feet + round * 1.1f, Quaternion.Euler(0f, turn, 0f), new Vector3(0.5f, 0.5f * more, 0.5f), colour, 0.06f, 0.45f, 0.2f, 0.03f);
                    break;
                case AfflictionKind.Soaked:
                {
                    Vector3 from = feet + round * 0.8f + Vector3.up * 1.5f;
                    FormFx.Fly(FormMesh.Droplet, from, from + Vector3.down * 1.4f, Quaternion.LookRotation(Vector3.down), Vector3.one * 0.6f, beat % 2 == 0 ? colour : ElementSubstance.Pale(colour), 0.45f, 0.025f);
                    if (beat % 2 == 0) FormFx.Lay(FormMark.Ring, feet, 0.5f, FormKit.Lit(colour, 1.6f, 0.8f), 0.5f, 1.8f);
                    break;
                }
                case AfflictionKind.Seeded:
                    if (beat % 4 == 0) FormFx.Erupt(FormMesh.Flower, feet + round * 0.5f + Vector3.up * (0.9f + ElementSubstance.Hash(beat + 1)), Quaternion.Euler(-30f, turn, 0f), Vector3.one * 0.7f, new Color(1f, 0.72f, 0.85f), 0.1f, 0.5f, 0.2f, 0.025f);
                    else FormFx.Erupt(FormMesh.Vine, feet + round * 0.9f, Quaternion.Euler(0f, turn, 0f), new Vector3(0.8f, 0.9f * more, 0.8f), colour, 0.12f, 0.4f, 0.25f, 0.03f);
                    break;
                case AfflictionKind.Tremor:
                {
                    Vector3 from = feet + round * 1.4f;
                    FormFx.Fly(FormMesh.Pebble, from, from + Vector3.up * (0.5f + 0.3f * depth), Random.rotation, Vector3.one * 0.6f, colour, 0.42f, 0.03f);
                    if (beat % 3 == 0) FormFx.Lay(FormMark.Crack, feet, 2.2f, FormKit.Lit(colour, 1.4f, 0.7f), 0.5f, -1f, 0f, turn);
                    break;
                }
                case AfflictionKind.Sundered:
                {
                    Vector3 from = feet + round * 0.7f + Vector3.up * 1.3f;
                    FormFx.Fly(FormMesh.Blade, from, from + round * 1.2f + Vector3.down * 1.2f, Random.rotation, Vector3.one * 0.8f, colour, 0.5f, 0.03f);
                    break;
                }
                case AfflictionKind.Marked:
                {
                    Vector3 over = feet + Vector3.up * 2.2f;
                    FormFx.Fly(FormMesh.Ray, over, over, Quaternion.LookRotation((round + Vector3.up * 0.45f).normalized), new Vector3(2.2f, 2.2f, 1.7f * more), Color.white, 0.55f, 0.01f);
                    break;
                }
                case AfflictionKind.Cursed:
                {
                    Vector3 from = feet + round * 0.9f + Vector3.up * 2.3f;
                    FormFx.Fly(FormMesh.Cloud, from, feet + round * 0.5f + Vector3.up * 0.9f, Quaternion.Euler(0f, turn, 0f), new Vector3(1.1f, 0.75f, 1.1f), beat % 2 == 0 ? ElementSubstance.Shade(colour) : new Color(colour.r * 0.55f, colour.g * 0.5f, colour.b * 0.6f), 0.55f, 0.02f);
                    break;
                }
                case AfflictionKind.Poison:
                {
                    Vector3 from = feet + round * 0.8f + Vector3.up * 0.5f;
                    FormFx.Fly(FormMesh.Bubble, from, from + Vector3.up * 1.7f, Quaternion.LookRotation(Vector3.up), Vector3.one * (0.45f + 0.06f * depth), beat % 2 == 0 ? colour : ElementSubstance.Pale(colour), 0.7f, 0.02f);
                    break;
                }
            }
        }
    }
}
