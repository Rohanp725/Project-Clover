using System;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// Enemies whose shadow is cut loose (Dark's charged blade, Shadow Cut Loose; the ledger's shadow-cut-loose): for a
    /// while the shadow lies on the floor apart from the body (<see cref="ConditionFlags.ShadowLoose"/>, the run's point),
    /// and a weapon blow that reaches the shadow strikes the body as well. This keeps the enemies that carry one, so a
    /// swing can find the shadows it reaches. Fixed and small: nothing is allocated after it starts.
    /// </summary>
    public static class LooseShadows
    {
        public const int Most = 16;
        /// <summary>Metres from the body the shadow lies, on the side away from the player.</summary>
        public const float Apart = 2f;
        /// <summary>Metres round its spot within which a blow reaches the shadow.</summary>
        public const float Size = 0.6f;

        private static readonly Component[] bodies = new Component[Most];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Clear();

        /// <summary>Forgets every shadow: each play begins so.</summary>
        public static void Clear() => Array.Clear(bodies, 0, bodies.Length);

        /// <summary>Where the shadow of a body at <paramref name="body"/> lies when it is cut loose by a player at <paramref name="player"/>.</summary>
        public static Vector3 SpotFor(Vector3 body, Vector3 player)
        {
            Vector3 away = body - player;
            away.y = 0f;
            return body + (away.sqrMagnitude > 0.0001f ? away.normalized : Vector3.forward) * Apart;
        }

        /// <summary>A shadow has been cut loose from <paramref name="body"/>: it is kept, so swings can find it.</summary>
        public static void Note(Component body)
        {
            if (body == null) return;
            int free = -1;
            for (int i = 0; i < Most; i++)
            {
                if (bodies[i] == body) return;
                if (free < 0 && !Loose(bodies[i], out _)) free = i;
            }
            if (free >= 0) bodies[free] = body;
        }

        /// <summary>The bodies whose loose shadow a swing from <paramref name="origin"/> along <paramref name="forward"/> reaches (within its reach and arc), and their spots. Returns how many.</summary>
        public static int Reached(Vector3 origin, Vector3 forward, float reach, float arcDegrees, Component[] into)
        {
            int count = 0;
            if (into == null) return 0;
            for (int i = 0; i < Most && count < into.Length; i++)
            {
                if (!Loose(bodies[i], out Vector3 spot)) continue;
                if (Reaches(origin, forward, reach, arcDegrees, spot)) into[count++] = bodies[i];
            }
            return count;
        }

        /// <summary>True when a swing from <paramref name="origin"/> reaches a shadow lying at <paramref name="spot"/>: near enough, and inside the arc unless it is under the feet.</summary>
        public static bool Reaches(Vector3 origin, Vector3 forward, float reach, float arcDegrees, Vector3 spot)
        {
            Vector3 offset = spot - origin;
            offset.y = 0f;
            float distance = Math.Max(0f, offset.magnitude - Size);
            if (distance > reach) return false;
            if (distance <= 0.6f) return true;
            forward.y = 0f;
            return Vector3.Angle(forward, offset) <= arcDegrees * 0.5f + 10f;
        }

        private static bool Loose(Component body, out Vector3 spot)
        {
            spot = Vector3.zero;
            return body != null && body.gameObject.activeInHierarchy && body is IAfflictable held && held.Afflictions.Conditions.LooseShadow(out spot);
        }
    }
}
