using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// How a surge is seen on the body, by element: flames licking up it, lightning crawling over it, streaks of
    /// wind circling it, a turning shell of ice, a ring of water, leaves climbing round it over flowers, stones
    /// circling its feet, blades hanging at its shoulders, a crown of rays, smoke pouring down it, bubbles rising
    /// off it. Six solids of <see cref="FormFx"/> are held for as long as the surge lasts and placed every frame;
    /// nothing is remembered here between frames but what the caller keeps.
    /// </summary>
    public static class SurgeLook
    {
        public const int Pieces = 6;

        private static readonly Color Petal = new Color(1f, 0.72f, 0.85f);
        private static readonly Color Ember = new Color(1f, 0.86f, 0.4f);

        /// <summary>Takes the pieces for an element's surge. The handles are the caller's to keep and to hand back.</summary>
        public static void Begin(ElementInfo element, int[] handles)
        {
            End(handles);
            if (element == null) return;
            // The element art's surge, and for the elements whose built pieces it replaces, nothing else.
            if (SurgeArt.Begin(element) && SurgeArt.ReplacesBuilt(element)) return;
            Color colour = element.Colour;
            for (int k = 0; k < Pieces && k < handles.Length; k++)
            {
                switch (element.Index)
                {
                    case ElementSubstance.Fire: handles[k] = FormFx.Hold(FormMesh.Flame, k % 2 == 0 ? colour : Ember, 0.03f); break;
                    case ElementSubstance.Lightning: handles[k] = FormFx.Hold(FormMesh.Bolt, k % 2 == 0 ? Color.white : colour, 0.02f); break;
                    case ElementSubstance.Wind: handles[k] = FormFx.Hold(k < 4 ? FormMesh.Gust : FormMesh.Leaf, k < 4 ? ElementSubstance.Pale(colour) : new Color(0.5f, 0.78f, 0.3f), 0.02f); break;
                    case ElementSubstance.Ice: handles[k] = FormFx.Hold(FormMesh.Crystal, k % 2 == 0 ? colour : Color.white, 0.03f); break;
                    case ElementSubstance.Water: handles[k] = FormFx.Hold(FormMesh.Droplet, k % 2 == 0 ? colour : ElementSubstance.Pale(colour), 0.03f); break;
                    case ElementSubstance.Life: handles[k] = FormFx.Hold(k < 3 ? FormMesh.Leaf : FormMesh.Flower, k < 3 ? colour : Petal, 0.03f); break;
                    case ElementSubstance.Earth: handles[k] = FormFx.Hold(k % 2 == 0 ? FormMesh.Rock : FormMesh.Pebble, colour, 0.04f); break;
                    case ElementSubstance.Metal: handles[k] = k < 4 ? FormFx.Hold(FormMesh.Blade, colour, 0.035f) : -1; break;
                    case ElementSubstance.Light: handles[k] = FormFx.Hold(FormMesh.Ray, Color.Lerp(colour, Color.white, 0.4f), 0.015f); break;
                    case ElementSubstance.Dark: handles[k] = FormFx.Hold(FormMesh.Cloud, k % 2 == 0 ? ElementSubstance.Shade(colour) : new Color(colour.r * 0.55f, colour.g * 0.5f, colour.b * 0.6f), 0.03f); break;
                    default: handles[k] = FormFx.Hold(FormMesh.Bubble, k % 2 == 0 ? colour : ElementSubstance.Pale(colour), 0.03f); break;
                }
                if (element.Index == ElementSubstance.Lightning) FormFx.Heat(handles[k], 0.5f);
            }
        }

        /// <summary>Hands the pieces back.</summary>
        public static void End(int[] handles)
        {
            SurgeArt.End();
            for (int k = 0; k < handles.Length; k++)
            {
                FormFx.Release(handles[k], 0.15f);
                handles[k] = -1;
            }
        }

        /// <summary>Places the pieces round a body standing at a point, at a time on the game's clock.</summary>
        public static void Place(ElementInfo element, int[] handles, Vector3 feet, float time)
        {
            if (element == null) return;
            SurgeArt.Place(feet, time);
            for (int k = 0; k < Pieces && k < handles.Length; k++)
            {
                if (handles[k] < 0) continue;
                float share = k / (float)Pieces;
                Vector3 at;
                Quaternion turn;
                Vector3 scale;
                switch (element.Index)
                {
                    case ElementSubstance.Fire:
                    {
                        // Each tongue climbs the body from the feet and thins out at the shoulders, over and over.
                        float climb = Frac(time * 1.1f + share);
                        at = feet + Ring(k * 60f + time * 30f, 0.5f) + Vector3.up * (climb * 1.3f);
                        turn = Quaternion.Euler(0f, k * 77f, 0f);
                        scale = new Vector3(0.75f, 1.3f, 0.75f) * (1f - 0.6f * climb);
                        break;
                    }
                    case ElementSubstance.Lightning:
                    {
                        // Somewhere new on the body fourteen times a second.
                        int beat = Mathf.FloorToInt(time * 14f) * 7 + k * 31;
                        at = feet + Ring(ElementSubstance.Hash(beat) * 360f, 0.3f) + Vector3.up * (0.2f + 1.5f * ElementSubstance.Hash(beat + 1));
                        turn = Quaternion.Euler(ElementSubstance.Hash(beat + 2) * 360f, ElementSubstance.Hash(beat + 3) * 360f, 0f);
                        scale = new Vector3(1.5f, 1.5f, 0.45f + 0.5f * ElementSubstance.Hash(beat + 4));
                        break;
                    }
                    case ElementSubstance.Wind:
                    {
                        float angle = time * 520f + k * 90f;
                        at = feet + Ring(angle, 0.62f) + Vector3.up * (k < 4 ? 0.3f + k * 0.42f : Frac(time * 0.7f + share) * 2f);
                        turn = Quaternion.Euler(0f, angle + 90f, k < 4 ? 0f : time * 400f);
                        scale = k < 4 ? new Vector3(1.6f, 1.6f, 1.25f) : Vector3.one * 0.3f;
                        break;
                    }
                    case ElementSubstance.Ice:
                    {
                        // A shell of crystals turning slowly, each leaning out from the body.
                        float angle = time * 45f + k * 60f;
                        at = feet + Ring(angle, 0.72f) + Vector3.up * (0.35f + 0.55f * (k % 2) + 0.06f * Mathf.Sin(time * 2f + k));
                        turn = Quaternion.Euler(0f, angle, 0f) * Quaternion.Euler(22f, 0f, 0f);
                        scale = Vector3.one * 0.62f;
                        break;
                    }
                    case ElementSubstance.Water:
                    {
                        // A ring of water round the waist, tilted, each drop chasing the next.
                        float angle = time * 250f + k * 60f;
                        at = feet + Ring(angle, 0.68f) + Vector3.up * (0.95f + 0.3f * Mathf.Sin((angle + time * 40f) * Mathf.Deg2Rad));
                        turn = Quaternion.Euler(0f, angle + 90f, 0f);
                        scale = new Vector3(0.75f, 0.75f, 0.62f);
                        break;
                    }
                    case ElementSubstance.Life:
                    {
                        if (k < 3)
                        {
                            float climb = Frac(time * 0.45f + k / 3f);
                            float angle = time * 190f + k * 120f;
                            at = feet + Ring(angle, 0.5f) + Vector3.up * (climb * 1.9f);
                            turn = Quaternion.Euler(-25f, angle + 90f, time * 150f);
                            scale = Vector3.one * (0.42f * Mathf.Sin(climb * Mathf.PI) + 0.08f);
                        }
                        else
                        {
                            at = feet + Ring(k * 120f + time * 12f, 0.55f) + Vector3.up * 0.06f;
                            turn = Quaternion.Euler(0f, time * 30f + k * 40f, 0f);
                            scale = Vector3.one * (0.75f + 0.08f * Mathf.Sin(time * 3f + k));
                        }
                        break;
                    }
                    case ElementSubstance.Earth:
                    {
                        float angle = time * 38f + k * 60f;
                        at = feet + Ring(angle, 0.8f) + Vector3.up * (0.3f + 0.12f * Mathf.Sin(time * 1.6f + k * 2f));
                        turn = Quaternion.Euler(time * 20f + k * 50f, angle, k * 35f);
                        scale = Vector3.one * (k % 2 == 0 ? 0.36f : 0.5f);
                        break;
                    }
                    case ElementSubstance.Metal:
                    {
                        // Four blades hanging point down at the shoulders, turning about the body.
                        float angle = time * 60f + k * 90f;
                        at = feet + Ring(angle, 0.7f) + Vector3.up * (2f + 0.05f * Mathf.Sin(time * 3f + k));
                        turn = SigKit.Down * Quaternion.Euler(0f, 0f, angle);
                        scale = Vector3.one * 0.85f;
                        break;
                    }
                    case ElementSubstance.Light:
                    {
                        // A crown of rays standing out from above the head.
                        float angle = time * 28f + k * 60f;
                        Vector3 out_ = Ring(angle, 1f);
                        at = feet + Vector3.up * 2.05f + out_ * 0.22f;
                        turn = Quaternion.LookRotation(out_ + Vector3.up * 0.35f);
                        scale = new Vector3(1.3f, 1.3f, 0.6f + 0.14f * Mathf.Sin(time * 5f + k * 2f));
                        break;
                    }
                    case ElementSubstance.Dark:
                    {
                        // Smoke that pours down off the shoulders and spreads at the feet.
                        float fall = Frac(time * 0.55f + share);
                        at = feet + Ring(k * 60f + time * 18f, 0.3f + 0.35f * fall) + Vector3.up * (1.7f * (1f - fall) + 0.1f);
                        turn = Quaternion.Euler(0f, k * 50f, 0f);
                        scale = new Vector3(0.75f, 0.5f, 0.75f) * (0.5f + fall);
                        break;
                    }
                    default:
                    {
                        float rise = Frac(time * 0.75f + share);
                        at = feet + Ring(k * 60f + Mathf.Sin(time * 2f + k) * 25f, 0.42f) + Vector3.up * (rise * 2f);
                        turn = Quaternion.LookRotation(Vector3.up);
                        scale = Vector3.one * (0.3f + 0.3f * rise) * (rise > 0.93f ? 0.2f : 1f);
                        break;
                    }
                }
                FormFx.Place(handles[k], at, turn, scale);
            }
        }

        private static float Frac(float value) => value - Mathf.Floor(value);

        private static Vector3 Ring(float degrees, float radius)
        {
            float angle = degrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
        }
    }
}
