using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The spine of a tail or a neck: a chain of points a fixed distance apart, with its tip leading. Wherever the
    /// tip goes it lays a trail, and every point behind it lies on that trail, so each part of the body passes where
    /// the part ahead of it passed; drawn back, it goes out the way it came in. On top of that a sideways wave can
    /// travel down it, larger toward the tip, and each point is sprung to where it is asked to be, more loosely
    /// toward the tip, so the tip lags behind a quick movement and then goes past it: it whips. Plain numbers and
    /// vectors; nothing here is allocated once it is made.
    /// </summary>
    public class SerpentSpine
    {
        /// <summary>How hard a point near the root is pulled to where it is asked to be, and one at the tip.</summary>
        public float RootStiffness = 420f;
        public float TipStiffness = 70f;
        /// <summary>One is no overshoot at all; less lets the tip go past and come back.</summary>
        public float Damping = 0.5f;
        /// <summary>False for a neck, which may stretch to reach what it holds.</summary>
        public bool HoldLengths = true;

        public int Count => points.Length;
        /// <summary>The length of a stretch between two points; the mean of them when they differ.</summary>
        public float Spacing { get; private set; }
        /// <summary>The length of the stretch from point <paramref name="index"/> to the next.</summary>
        public float Stretch(int index) => lengths[index];
        /// <summary>Where each point is now, root first and tip last.</summary>
        public Vector3[] Points => points;
        /// <summary>Where each point is asked to be. Filled by <see cref="PlaceAlongTrail"/>; whoever owns the spine may then move them (lift them, wave them) before <see cref="Step"/>.</summary>
        public Vector3[] Targets => targets;
        /// <summary>Where the tip is on its trail.</summary>
        public Vector3 LeadPoint => lead;

        private readonly Vector3[] points;
        private readonly Vector3[] targets;
        private readonly Vector3[] velocity;
        private readonly Vector3[] trail;
        private readonly float[] lengths;
        private readonly float trailStep;
        private int top = -1;
        private Vector3 lead;
        private Vector3 beyond = Vector3.back;

        /// <summary>A spine whose stretches are not all one length, as a model's bones may not be: one length for each stretch, root end first.</summary>
        public SerpentSpine(float[] stretches, int trailCapacity = 512, float trailStep = 0.2f) : this(stretches.Length + 1, 1f, trailCapacity, trailStep)
        {
            float total = 0f;
            for (int i = 0; i < stretches.Length; i++)
            {
                lengths[i] = Mathf.Max(0.01f, stretches[i]);
                total += lengths[i];
            }
            Spacing = total / stretches.Length;
        }

        public SerpentSpine(int count, float spacing, int trailCapacity = 512, float trailStep = 0.2f)
        {
            count = Mathf.Max(2, count);
            Spacing = Mathf.Max(0.01f, spacing);
            lengths = new float[count - 1];
            for (int i = 0; i < lengths.Length; i++) lengths[i] = Spacing;
            points = new Vector3[count];
            targets = new Vector3[count];
            velocity = new Vector3[count];
            trail = new Vector3[Mathf.Max(8, trailCapacity)];
            this.trailStep = Mathf.Max(0.01f, trailStep);
        }

        /// <summary>Begins again with the tip at a point and the whole body lying straight behind it, the way given.</summary>
        public void Seed(Vector3 tip, Vector3 behind)
        {
            beyond = behind.sqrMagnitude > 0.0001f ? behind.normalized : Vector3.back;
            top = 0;
            trail[0] = tip;
            lead = tip;
            PlaceAlongTrail();
            Snap();
        }

        /// <summary>Moves the tip on to a new point. The trail remembers the way.</summary>
        public void Lead(Vector3 to)
        {
            lead = to;
            if (top >= 0 && (to - trail[top]).sqrMagnitude < trailStep * trailStep) return;
            if (top >= trail.Length - 1)
            {
                // Full: the oldest half is forgotten. The body is far shorter than what is kept.
                int keep = trail.Length / 2;
                System.Array.Copy(trail, trail.Length - keep, trail, 0, keep);
                top = keep - 1;
            }
            trail[++top] = to;
        }

        /// <summary>Draws the tip back along its own trail by a distance, as a snake goes out the way it came in.</summary>
        public void Retreat(float distance)
        {
            while (distance > 0f && top >= 0)
            {
                Vector3 to = trail[top] - lead;
                float length = to.magnitude;
                if (length > distance)
                {
                    lead += to / length * distance;
                    return;
                }
                distance -= length;
                lead = trail[top];
                if (top == 0) break;
                top--;
            }
            // Past where the trail began: straight on, the way the body lay when it was seeded.
            if (distance > 0f)
            {
                lead += beyond * distance;
                trail[0] = lead;
                top = 0;
            }
        }

        /// <summary>Asks every point to lie on the trail, each one spacing further behind the tip than the last.</summary>
        public void PlaceAlongTrail()
        {
            int last = points.Length - 1;
            int k = top;
            Vector3 at = lead;
            Vector3 heading = beyond;
            targets[last] = lead;
            for (int j = 1; j <= last; j++)
            {
                float need = lengths[last - j];
                while (true)
                {
                    if (k < 0)
                    {
                        at += heading * need;
                        break;
                    }
                    Vector3 to = trail[k] - at;
                    float length = to.magnitude;
                    if (length >= need)
                    {
                        heading = to / length;
                        at += heading * need;
                        break;
                    }
                    need -= length;
                    if (length > 0.0001f) heading = to / length;
                    at = trail[k];
                    k--;
                    // Where the trail runs out the body lies straight on, the way it was seeded.
                    if (k < 0) heading = beyond;
                }
                targets[last - j] = at;
            }
        }

        /// <summary>
        /// Adds a sideways wave to where the points are asked to be: nothing at the root, growing to its full size
        /// at the tip, and travelling toward the tip as time passes.
        /// </summary>
        public void Wave(float time, float size, float length, float speed, float growth = 1.5f)
        {
            int last = points.Length - 1;
            // From the tip back, so each point's sideways is read from the line as it lay before any of it was moved.
            for (int i = last; i >= 1; i--)
            {
                Vector3 along = targets[i] - targets[i - 1];
                var side = new Vector3(-along.z, 0f, along.x);
                if (side.sqrMagnitude < 0.000001f) continue;
                float share = Mathf.Pow(i / (float)last, growth);
                float phase = i * Spacing / Mathf.Max(0.01f, length) * Mathf.PI * 2f - time * speed;
                targets[i] += side.normalized * (size * share * Mathf.Sin(phase));
            }
        }

        /// <summary>Puts every point exactly where it is asked to be, at rest.</summary>
        public void Snap()
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = targets[i];
                velocity[i] = Vector3.zero;
            }
            Hold();
        }

        /// <summary>Moves every point toward where it is asked to be: the root at once, the rest on their springs, and then the lengths between them are put right.</summary>
        public void Step(float deltaTime)
        {
            float step = Mathf.Min(deltaTime, 1f / 30f);
            int last = points.Length - 1;
            points[0] = targets[0];
            velocity[0] = Vector3.zero;
            for (int i = 1; i <= last; i++)
            {
                float stiffness = Mathf.Lerp(RootStiffness, TipStiffness, i / (float)last);
                float drag = 2f * Damping * Mathf.Sqrt(stiffness);
                Vector3 pull = (targets[i] - points[i]) * stiffness - velocity[i] * drag;
                velocity[i] += pull * step;
                points[i] += velocity[i] * step;
            }
            Hold();
        }

        /// <summary>Puts the lengths between the points right again, after whoever owns the spine has moved points out of things the body cannot enter.</summary>
        public void Settle() => Hold();

        private void Hold()
        {
            if (!HoldLengths) return;
            for (int i = 1; i < points.Length; i++)
            {
                Vector3 to = points[i] - points[i - 1];
                float length = to.magnitude;
                Vector3 way = length > 0.0001f ? to / length : -beyond;
                points[i] = points[i - 1] + way * lengths[i - 1];
            }
        }
    }
}
