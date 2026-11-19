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
        /// <summary>Any other element: motes and sparks in the colour it is given.</summary>
        Element,
    }

    /// <summary>
    /// The player's lingering effects on the ground and in the air: a trail of fire behind a dodge, the crater a leap
    /// leaves, a thrown flask's burst, an arc of flame that travels on after a cut, a rod that calls bolts, a blade
    /// that falls after a moment, a thing thrown that turns toward its target or comes back. Each is a circle that
    /// bites the enemies in it, once or at intervals, for a while, and may move. A fixed number are kept ready; the
    /// scene has one of these and the player's upgrades call it. With none in the scene the calls do nothing.
    /// </summary>
    public class Hazards : MonoBehaviour
    {
        public int Capacity = 48;
        /// <summary>Seconds between the puffs of effect that show a patch is there, and between those of one that moves.</summary>
        public float ShowEvery = 0.11f;
        public float ShowMovingEvery = 0.035f;
        /// <summary>How fast a homing thing turns, in degrees a second.</summary>
        public float HomingTurn = 260f;

        private struct Zone
        {
            public bool Active;
            public HazardSpec Spec;
            public Vector3 Position, Velocity;
            public float Delay, Left, UntilBite, UntilShow;
            public bool Burst, Turned;
            public int Bitten;
        }

        private static Hazards instance;
        private static PlayerCombat owner;

        private Zone[] zones;
        private HashSet<IDamageable>[] bitten;
        private int next;

        /// <summary>How many patches are alive now, so probes can see them without the screen.</summary>
        public static int Alive
        {
            get
            {
                if (instance == null) return 0;
                int count = 0;
                for (int i = 0; i < instance.zones.Length; i++)
                    if (instance.zones[i].Active) count++;
                return count;
            }
        }

        // Domain reload is off: the hazards and their owner from the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            owner = null;
            Growing = false;
            GrownSecondsScale = 1f;
            GrownDamageScale = 1f;
            FireSecondsScale = 1f;
        }

        /// <summary>Whose blows these are: the damage is dealt through the player's own weapon code, so it earns what a blow earns.</summary>
        public static void OwnedBy(PlayerCombat combat) => owner = combat;

        /// <summary>While true, whatever is laid counts as grown: set round the cast of a spell that grows things.</summary>
        public static bool Growing;
        /// <summary>What the player's numbers make of how long grown things stand and how hard they bite.</summary>
        public static float GrownSecondsScale = 1f, GrownDamageScale = 1f;

        /// <summary>How many times as long fire left on the ground burns: the trail, the crater, the flask, a spell's fire. Patches that move are left alone.</summary>
        public static float FireSecondsScale = 1f;

        /// <summary>True when fire the player has left on the ground is burning under this place.</summary>
        public static bool FireUnder(Vector3 at)
        {
            if (instance == null) return false;
            for (int i = 0; i < instance.zones.Length; i++)
            {
                ref var zone = ref instance.zones[i];
                if (!zone.Active || zone.Spec.Kind != HazardKind.Fire || zone.Delay > 0f || zone.Velocity.sqrMagnitude > 0.01f) continue;
                Vector3 offset = zone.Position - at;
                offset.y = 0f;
                float reach = zone.Spec.Radius + 0.35f;
                if (offset.sqrMagnitude <= reach * reach) return true;
            }
            return false;
        }

        /// <summary>How many things the player has grown stand now.</summary>
        public static int GrownCount
        {
            get
            {
                if (instance == null) return 0;
                int count = 0;
                for (int i = 0; i < instance.zones.Length; i++)
                    if (instance.zones[i].Active && instance.zones[i].Spec.Grown) count++;
                return count;
            }
        }

        /// <summary>True when something the player has grown stands within this many metres of a place.</summary>
        public static bool GrownNear(Vector3 at, float metres)
        {
            if (instance == null) return false;
            for (int i = 0; i < instance.zones.Length; i++)
            {
                if (!instance.zones[i].Active || !instance.zones[i].Spec.Grown) continue;
                Vector3 offset = instance.zones[i].Position - at;
                offset.y = 0f;
                if (offset.sqrMagnitude <= metres * metres) return true;
            }
            return false;
        }

        /// <summary>Bursts everything the player has grown, each where it stands, and it is gone. Returns how many burst.</summary>
        public static int BurstGrown(in SpellBlow blow, float radius)
        {
            if (instance == null || owner == null) return 0;
            int count = 0;
            for (int i = 0; i < instance.zones.Length; i++)
            {
                if (!instance.zones[i].Active || !instance.zones[i].Spec.Grown) continue;
                instance.zones[i].Active = false;
                owner.StrikeArea(instance.zones[i].Position, radius, blow);
                Puff(instance.zones[i].Spec, instance.zones[i].Position, 3f);
                count++;
            }
            return count;
        }

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
        /// Lays a patch of fire or lightning. It waits <paramref name="delay"/> seconds, then bites what is inside it
        /// every <paramref name="biteEvery"/> seconds (or each enemy once only) until its time is up.
        /// </summary>
        public static void Lay(HazardKind kind, Vector3 position, float radius, float seconds, float damage, float stagger, float burnSeconds,
            float biteEvery = 0.4f, bool once = false, float delay = 0f, Vector3 velocity = default, bool burst = false)
        {
            Lay(new HazardSpec
            {
                Kind = kind,
                Position = position,
                Velocity = velocity,
                Radius = radius,
                Seconds = seconds,
                BiteEvery = biteEvery,
                Once = once,
                Delay = delay,
                Burst = burst,
                Blow = new SpellBlow
                {
                    Damage = damage,
                    Stagger = stagger,
                    Affliction = burnSeconds > 0f ? AfflictionKind.Burn : AfflictionKind.None,
                    AfflictionSeconds = burnSeconds,
                    Effect = kind == HazardKind.Fire ? VfxKind.Fire : VfxKind.Shock,
                },
            });
        }

        /// <summary>Lays a patch described in full.</summary>
        public static void Lay(in HazardSpec spec)
        {
            if (instance == null) return;
            bool grown = spec.Grown || Growing;
            if (grown && spec.Limit > 0)
            {
                // Things of one kind share a limit: one more takes the place of the one nearest its end.
                int standing = 0, oldest = -1;
                for (int slot = 0; slot < instance.zones.Length; slot++)
                {
                    if (!instance.zones[slot].Active || !instance.zones[slot].Spec.Grown || instance.zones[slot].Spec.Family != spec.Family) continue;
                    standing++;
                    if (oldest < 0 || instance.zones[slot].Left < instance.zones[oldest].Left) oldest = slot;
                }
                if (standing >= spec.Limit && oldest >= 0) instance.zones[oldest].Active = false;
            }
            // A free place if there is one; only with every place in use is the next in turn given up.
            int i = -1;
            for (int slot = 0; slot < instance.zones.Length && i < 0; slot++)
                if (!instance.zones[slot].Active) i = slot;
            if (i < 0)
            {
                i = instance.next;
                instance.next = (instance.next + 1) % instance.Capacity;
            }
            instance.bitten[i].Clear();
            instance.zones[i] = new Zone
            {
                Active = true,
                Spec = spec,
                Position = spec.Position,
                Velocity = spec.Velocity,
                Delay = spec.Delay,
                Left = spec.Seconds,
                UntilBite = 0f,
                Burst = spec.Burst,
            };
            instance.zones[i].Spec.BiteEvery = Mathf.Max(0.05f, spec.BiteEvery);
            // Ground a reaction left bites as the reaction's own: what it lays starts nothing.
            if (spec.FromReaction || spec.Blow.FromReaction)
            {
                instance.zones[i].Spec.FromReaction = true;
                instance.zones[i].Spec.Blow.FromReaction = true;
            }
            if (spec.Kind == HazardKind.Fire && !spec.Once && spec.Velocity.sqrMagnitude < 0.01f) instance.zones[i].Left = spec.Seconds * FireSecondsScale;
            if (grown)
            {
                instance.zones[i].Spec.Grown = true;
                instance.zones[i].Left = spec.Seconds * GrownSecondsScale;
                instance.zones[i].Spec.Blow.Damage = spec.Blow.Damage * GrownDamageScale;
            }
        }

        private static void Puff(in HazardSpec spec, Vector3 at, float scale)
        {
            switch (spec.Kind)
            {
                case HazardKind.Fire:
                    Vfx.Play(VfxKind.Fire, at, Vector3.up, 0.18f * scale);
                    Vfx.Play(VfxKind.Embers, at + Vector3.up * 0.4f, Vector3.up, 0.2f * scale);
                    break;
                case HazardKind.Lightning:
                    Vfx.Play(VfxKind.Shock, at + Vector3.up * 0.5f, Vector3.up, 0.25f * scale);
                    break;
                default:
                    Vfx.Play(VfxKind.TintPuff, at + Vector3.up * 0.2f, Vector3.up, 0.3f * scale, spec.Tint);
                    break;
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < zones.Length; i++)
            {
                if (!zones[i].Active) continue;
                ref var zone = ref zones[i];
                if (zone.Spec.Follow && zone.Spec.Target != null) zone.Position = zone.Spec.Target.position;
                if (zone.Delay > 0f)
                {
                    zone.Delay -= deltaTime;
                    if (zone.Spec.Warn)
                    {
                        // The spot is marked while the thing is on its way down.
                        zone.UntilShow -= deltaTime;
                        if (zone.UntilShow <= 0f)
                        {
                            zone.UntilShow = 0.16f;
                            if (zone.Spec.Kind == HazardKind.Element) Vfx.Play(VfxKind.TintRing, zone.Position + Vector3.up * 0.15f, Vector3.up, 1f, zone.Spec.Tint);
                            else Vfx.Play(VfxKind.Ring, zone.Position + Vector3.up * 0.15f, Vector3.up);
                        }
                    }
                    continue;
                }
                if (zone.Burst)
                {
                    // It arrives all at once: a flash on the ground the size of what it strikes.
                    zone.Burst = false;
                    Vector3 centre = zone.Position + Vector3.up * 0.4f;
                    if (zone.Spec.Kind == HazardKind.Element)
                    {
                        Vfx.Play(VfxKind.TintRing, zone.Position + Vector3.up * 0.2f, Vector3.up, 1f, zone.Spec.Tint);
                        Vfx.Play(VfxKind.TintSpark, centre, Vector3.up, 2.2f, zone.Spec.Tint);
                        for (int puff = 0; puff < 5; puff++)
                            Vfx.Play(VfxKind.TintPuff, centre + Quaternion.Euler(0f, puff * 72f, 0f) * Vector3.forward * (zone.Spec.Radius * 0.55f), Vector3.up, 0.8f, zone.Spec.Tint);
                    }
                    else
                    {
                        Vfx.Play(VfxKind.Ring, zone.Position + Vector3.up * 0.2f, Vector3.up);
                        Vfx.Play(VfxKind.Burst, centre);
                        if (zone.Spec.Kind == HazardKind.Lightning) LightningBolts.Strike(zone.Position, 0.5f);
                        else
                            for (int puff = 0; puff < 5; puff++)
                                Vfx.Play(VfxKind.Fire, centre + Quaternion.Euler(0f, puff * 72f, 0f) * Vector3.forward * (zone.Spec.Radius * 0.55f), Vector3.up, 0.8f);
                    }
                }
                bool moving = zone.Velocity.sqrMagnitude > 0.01f;
                if (moving) Steer(ref zone, i, deltaTime);
                zone.Position += zone.Velocity * deltaTime;
                zone.Left -= deltaTime;
                zone.UntilShow -= deltaTime;
                if (zone.UntilShow <= 0f)
                {
                    zone.UntilShow = moving ? ShowMovingEvery : ShowEvery;
                    Vector2 spot = Random.insideUnitCircle * (zone.Spec.Radius * (moving ? 0.35f : 0.8f));
                    Puff(zone.Spec, zone.Position + new Vector3(spot.x, 0.3f, spot.y), moving ? 2f : 1f);
                }
                zone.UntilBite -= deltaTime;
                if (zone.UntilBite <= 0f && owner != null)
                {
                    zone.UntilBite = zone.Spec.BiteEvery;
                    int hits = owner.StrikeArea(zone.Position, zone.Spec.Radius, zone.Spec.Blow, zone.Spec.Once ? bitten[i] : null);
                    zone.Bitten++;
                    // A standing storm drops a bolt somewhere in itself each time it bites, whether or not anyone is under it.
                    if (zone.Spec.Kind == HazardKind.Lightning && (hits > 0 || !zone.Spec.Once))
                    {
                        Vector2 spot = zone.Spec.Once ? Vector2.zero : Random.insideUnitCircle * (zone.Spec.Radius * 0.7f);
                        LightningBolts.Strike(zone.Position + new Vector3(spot.x, 0f, spot.y), 0.2f);
                    }
                    else if (zone.Spec.Kind == HazardKind.Element && !zone.Spec.Once)
                    {
                        Vfx.Play(VfxKind.TintSpark, zone.Position + Vector3.up * 0.6f, Vector3.up, 0.8f, zone.Spec.Tint);
                    }
                    if (zone.Spec.Bites > 0 && zone.Bitten >= zone.Spec.Bites) zone.Active = false;
                }
                if (zone.Left <= 0f) zone.Active = false;
            }
        }

        // A thing in flight: it may turn toward the enemy it was thrown at, or come home to the hand that threw it.
        private void Steer(ref Zone zone, int index, float deltaTime)
        {
            float speed = zone.Velocity.magnitude;
            if (zone.Spec.Flight == Flight.Homing && zone.Spec.Target != null && zone.Spec.Target.gameObject.activeInHierarchy)
            {
                Vector3 toward = zone.Spec.Target.position - zone.Position;
                toward.y = 0f;
                if (toward.sqrMagnitude > 0.04f)
                    zone.Velocity = Vector3.RotateTowards(zone.Velocity, toward.normalized * speed, HomingTurn * Mathf.Deg2Rad * deltaTime, 0f);
            }
            else if (zone.Spec.Flight == Flight.Returning && owner != null)
            {
                if (!zone.Turned && zone.Left <= zone.Spec.Seconds * 0.5f)
                {
                    // Half its time gone: it turns for home, and whatever it struck on the way out it may strike again.
                    zone.Turned = true;
                    bitten[index].Clear();
                }
                if (!zone.Turned) return;
                Vector3 home = owner.transform.position - zone.Position;
                home.y = 0f;
                if (home.sqrMagnitude < 0.6f) zone.Active = false;
                else zone.Velocity = home.normalized * speed;
            }
        }
    }
}
