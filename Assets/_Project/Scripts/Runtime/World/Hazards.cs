using System.Collections.Generic;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>What a patch of ground does to the enemies standing on it.</summary>
    public enum HazardKind
    {
        /// <summary>Burns: flame and embers, and it sets alight what it touches.</summary>
        Fire,
        /// <summary>Shocks: sparks and a bolt from the sky each time it bites.</summary>
        Lightning,
    }

    /// <summary>
    /// The player's lingering effects on the ground: a trail of fire behind a dodge, the crater a leap leaves, a thrown
    /// flask's burst, an arc of flame that travels on after a cut, a rod that calls bolts. Each is a circle that bites
    /// the enemies in it, once or at intervals, for a while, and may move. A fixed number are kept ready; the scene has
    /// one of these and the player's upgrades call it. With none in the scene the calls do nothing.
    /// </summary>
    public class Hazards : MonoBehaviour
    {
        public int Capacity = 24;
        /// <summary>Seconds between the puffs of effect that show a patch is there.</summary>
        public float ShowEvery = 0.11f;

        private struct Zone
        {
            public bool Active;
            public HazardKind Kind;
            public Vector3 Position, Velocity;
            public float Radius, Delay, Left, BiteEvery, UntilBite, Damage, Stagger, BurnSeconds, UntilShow;
            public bool Once, Burst;
        }

        private static Hazards instance;
        private static PlayerCombat owner;

        private Zone[] zones;
        private HashSet<IDamageable>[] bitten;
        private int next;

        // Domain reload is off: the hazards and their owner from the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            owner = null;
        }

        /// <summary>Whose blows these are: the damage is dealt through the player's own weapon code, so it earns what a blow earns.</summary>
        public static void OwnedBy(PlayerCombat combat) => owner = combat;

        private void Awake()
        {
            zones = new Zone[Capacity];
            bitten = new HashSet<IDamageable>[Capacity];
            for (int i = 0; i < Capacity; i++) bitten[i] = new HashSet<IDamageable>();
        }

        private void OnEnable() => instance = this;

        private void OnDisable()
        {
            if (instance == this) instance = null;
        }

        /// <summary>
        /// Lays a patch. It waits <paramref name="delay"/> seconds, then bites what is inside it every
        /// <paramref name="biteEvery"/> seconds (or each enemy once only) until its time is up.
        /// </summary>
        public static void Lay(HazardKind kind, Vector3 position, float radius, float seconds, float damage, float stagger, float burnSeconds,
            float biteEvery = 0.4f, bool once = false, float delay = 0f, Vector3 velocity = default, bool burst = false)
        {
            if (instance == null) return;
            int i = instance.next;
            instance.next = (instance.next + 1) % instance.Capacity;
            instance.bitten[i].Clear();
            instance.zones[i] = new Zone
            {
                Active = true,
                Kind = kind,
                Position = position,
                Velocity = velocity,
                Radius = radius,
                Delay = delay,
                Left = seconds,
                BiteEvery = Mathf.Max(0.05f, biteEvery),
                UntilBite = 0f,
                Damage = damage,
                Stagger = stagger,
                BurnSeconds = burnSeconds,
                Once = once,
                Burst = burst,
            };
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < zones.Length; i++)
            {
                if (!zones[i].Active) continue;
                ref var zone = ref zones[i];
                if (zone.Delay > 0f)
                {
                    zone.Delay -= deltaTime;
                    continue;
                }
                if (zone.Burst)
                {
                    // It arrives all at once: a flash on the ground the size of what it strikes.
                    zone.Burst = false;
                    Vector3 centre = zone.Position + Vector3.up * 0.4f;
                    Vfx.Play(VfxKind.Ring, zone.Position + Vector3.up * 0.2f, Vector3.up);
                    Vfx.Play(VfxKind.Burst, centre);
                    if (zone.Kind == HazardKind.Lightning) LightningBolts.Strike(zone.Position, 0.5f);
                    else
                        for (int puff = 0; puff < 5; puff++)
                            Vfx.Play(VfxKind.Fire, centre + Quaternion.Euler(0f, puff * 72f, 0f) * Vector3.forward * (zone.Radius * 0.55f), Vector3.up, 0.8f);
                }
                zone.Position += zone.Velocity * deltaTime;
                zone.Left -= deltaTime;
                zone.UntilShow -= deltaTime;
                if (zone.UntilShow <= 0f)
                {
                    zone.UntilShow = ShowEvery;
                    Vector2 spot = Random.insideUnitCircle * (zone.Radius * 0.8f);
                    Vector3 at = zone.Position + new Vector3(spot.x, 0.3f, spot.y);
                    if (zone.Kind == HazardKind.Fire)
                    {
                        Vfx.Play(VfxKind.Fire, at, Vector3.up, 0.18f);
                        Vfx.Play(VfxKind.Embers, at + Vector3.up * 0.4f, Vector3.up, 0.2f);
                    }
                    else
                    {
                        Vfx.Play(VfxKind.Shock, at + Vector3.up * 0.5f, Vector3.up, 0.25f);
                    }
                }
                zone.UntilBite -= deltaTime;
                if (zone.UntilBite <= 0f && owner != null)
                {
                    zone.UntilBite = zone.BiteEvery;
                    int hits = owner.StrikeArea(zone.Position, zone.Radius, zone.Damage, zone.Stagger, zone.BurnSeconds,
                        zone.Kind == HazardKind.Fire ? VfxKind.Fire : VfxKind.Shock, zone.Once ? bitten[i] : null);
                    // A standing storm drops a bolt somewhere in itself each time it bites, whether or not anyone is under it.
                    if (zone.Kind == HazardKind.Lightning && (hits > 0 || !zone.Once))
                    {
                        Vector2 spot = zone.Once ? Vector2.zero : Random.insideUnitCircle * (zone.Radius * 0.7f);
                        LightningBolts.Strike(zone.Position + new Vector3(spot.x, 0f, spot.y), 0.2f);
                    }
                }
                if (zone.Left <= 0f) zone.Active = false;
            }
        }
    }
}
