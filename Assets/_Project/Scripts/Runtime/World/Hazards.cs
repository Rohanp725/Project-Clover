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
        // Set from the crowd probe (Phase 3.6): twelve enemies with about seventeen reactions a second kept 40 alive at the
        // peak, near the old 48, so there is room to spare. Not serialized, so a prefab built with 48 does not keep it.
        [System.NonSerialized] public int Capacity = 96;
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

        // The parts of one shape share who they have struck. A burst lasts a fraction of a second, so a few groups in
        // turn are plenty; a group whose place has been taken by a newer one strikes as its parts would alone.
        private const int Groups = 8;
        private readonly HashSet<IDamageable>[] groupBitten =
        {
            new HashSet<IDamageable>(), new HashSet<IDamageable>(), new HashSet<IDamageable>(), new HashSet<IDamageable>(),
            new HashSet<IDamageable>(), new HashSet<IDamageable>(), new HashSet<IDamageable>(), new HashSet<IDamageable>(),
        };
        private readonly int[] groupIds = new int[Groups];
        private int lastGroup;

        /// <summary>A new group for the parts of one shape (<see cref="HazardSpec.Group"/>); zero when there is no ground.</summary>
        public static int NewGroup()
        {
            if (instance == null) return 0;
            int id = ++instance.lastGroup;
            int slot = id % Groups;
            instance.groupIds[slot] = id;
            instance.groupBitten[slot].Clear();
            return id;
        }

        // Whom a bite must leave alone: those already struck by a once-only zone or its group, and the one it spares.
        private HashSet<IDamageable> LeftAlone(int index, in Zone zone)
        {
            HashSet<IDamageable> set = null;
            if (zone.Spec.Once)
            {
                int group = zone.Spec.Group, slot = group % Groups;
                set = group != 0 && groupIds[slot] == group ? groupBitten[slot] : bitten[index];
            }
            if (zone.Spec.Spares != null)
            {
                if (set == null)
                {
                    // Bitten again and again: the zone's own set holds only the one spared.
                    set = bitten[index];
                    set.Clear();
                }
                set.Add(zone.Spec.Spares);
            }
            return set;
        }

        /// <summary>Ends every patch laid in one group at once (a rod spent by the step to it). Returns how many ended.</summary>
        public static int EndGroup(int group)
        {
            if (instance == null || group == 0) return 0;
            int ended = 0;
            for (int i = 0; i < instance.zones.Length; i++)
            {
                ref var zone = ref instance.zones[i];
                if (!zone.Active || zone.Spec.Group != group) continue;
                zone.Active = false;
                Puff(zone.Spec, zone.Position, 1.5f);
                ended++;
            }
            return ended;
        }

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
            GrowsBackAfter = 0f;
            Regrowing.Clear();
            GrownBitesMask = 0;
        }

        /// <summary>
        /// Above zero: seconds after a grown thing is spent (burst, or its bites used up) that it grows back where it stood
        /// (Wood Grows Back: 6; the ledger's grows-back). Set from the player's marks.
        /// </summary>
        public static float GrowsBackAfter;
        /// <summary>
        /// What every grown thing lays besides its own blow, one a bite, as bits at <c>1 &lt;&lt; (int)kind</c> (Ore in the Root:
        /// a notch; Hidden in the Tree: a Chill; the ledger's grown-sources). Set from the player's marks.
        /// </summary>
        public static int GrownBitesMask;

        /// <summary>
        /// Writes where the player's grown things within <paramref name="metres"/> of <paramref name="at"/> stand, as many as
        /// <paramref name="into"/> holds (Lightning Flower: each throws a bolt). Returns how many.
        /// </summary>
        public static int GrownWithin(Vector3 at, float metres, Vector3[] into)
        {
            if (instance == null || into == null) return 0;
            int count = 0;
            for (int i = 0; i < instance.zones.Length && count < into.Length; i++)
            {
                if (!instance.zones[i].Active || !instance.zones[i].Spec.Grown) continue;
                if (!GrownRules.Within(instance.zones[i].Position, at, metres)) continue;
                into[count++] = instance.zones[i].Position;
            }
            return count;
        }

        /// <summary>The grown things waiting to grow back, each with its time.</summary>
        public static readonly RegrowQueue Regrowing = new RegrowQueue();
        /// <summary>Grown things grown back, counted for probes.</summary>
        public static int GrownBack { get; private set; }

        // A grown thing is spent: with Wood Grows Back it is put in line to grow back where it stood, as it was laid.
        private static void Spent(in Zone zone)
        {
            if (GrowsBackAfter <= 0f || !zone.Spec.Grown) return;
            var spec = zone.Spec;
            spec.Position = zone.Position;
            spec.Delay = 0f;
            // Laid again, its damage is scaled once more by the player's numbers: take the scaling off it first.
            if (GrownDamageScale > 0f) spec.Blow.Damage /= GrownDamageScale;
            Regrowing.Add(spec, Time.time + GrowsBackAfter);
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

        /// <summary>
        /// True when <paramref name="at"/> stands in ground of the player's that lays <paramref name="kind"/> with its bites (their own
        /// water: ground that Soaks), settled and not moving.
        /// </summary>
        public static bool OwnGroundUnder(Vector3 at, AfflictionKind kind)
        {
            if (instance == null || kind == AfflictionKind.None) return false;
            for (int i = 0; i < instance.zones.Length; i++)
            {
                ref var zone = ref instance.zones[i];
                if (!zone.Active || zone.Spec.Blow.Affliction != kind || zone.Delay > 0f || zone.Velocity.sqrMagnitude > 0.01f || zone.Spec.Once) continue;
                if (GroundRules.Within(at, zone.Position, zone.Spec.Radius + 0.35f)) return true;
            }
            return false;
        }

        /// <summary>
        /// Ends every settled patch laid by <paramref name="source"/>, writing where each lay and how wide it was, as many as fit;
        /// returns how many were ended (a heavy blow bursting every puddle of Weeping Sore's).
        /// </summary>
        public static int TakeAllOf(object source, Vector3[] at, float[] radius)
        {
            if (instance == null || source == null) return 0;
            int taken = 0;
            for (int i = 0; i < instance.zones.Length && taken < at.Length && taken < radius.Length; i++)
            {
                ref var zone = ref instance.zones[i];
                if (!zone.Active || zone.Delay > 0f || !ReferenceEquals(zone.Spec.Source, source)) continue;
                at[taken] = zone.Position;
                radius[taken] = zone.Spec.Radius;
                taken++;
                zone.Active = false;
            }
            return taken;
        }

        /// <summary>Writes where each settled patch of ground of the given name lies and how wide it is, as many as fit; returns how many.</summary>
        public static int AllNamed(string name, Vector3[] at, float[] radius)
        {
            if (instance == null || string.IsNullOrEmpty(name)) return 0;
            int found = 0;
            for (int i = 0; i < instance.zones.Length && found < at.Length && found < radius.Length; i++)
            {
                ref var zone = ref instance.zones[i];
                if (!zone.Active || zone.Delay > 0f || zone.Spec.Name != name) continue;
                at[found] = zone.Position;
                radius[found] = zone.Spec.Radius;
                found++;
            }
            return found;
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
        public static bool GrownNear(Vector3 at, float metres) => GrownNear(at, metres, 0);

        /// <summary>The place of the thing the player grew nearest <paramref name="at"/> within <paramref name="metres"/>; false when none stands that near.</summary>
        public static bool NearestGrown(Vector3 at, float metres, out Vector3 where)
        {
            where = at;
            if (instance == null) return false;
            float best = metres * metres;
            bool found = false;
            for (int i = 0; i < instance.zones.Length; i++)
            {
                if (!instance.zones[i].Active || !instance.zones[i].Spec.Grown) continue;
                Vector3 offset = instance.zones[i].Position - at;
                offset.y = 0f;
                if (offset.sqrMagnitude > best) continue;
                best = offset.sqrMagnitude;
                where = instance.zones[i].Position;
                found = true;
            }
            return found;
        }

        /// <summary>As above, only things grown in <paramref name="family"/> (0: any; Corpse Flowers' own flowers).</summary>
        /// <summary>
        /// The thing the player grew within <paramref name="reach"/> of <paramref name="from"/> that stands nearest
        /// <paramref name="aim"/> (Root Walk: the one nearest where you aim, within twelve metres of you); false for none.
        /// </summary>
        public static bool NearestGrownTo(Vector3 aim, Vector3 from, float reach, out Vector3 where)
        {
            where = from;
            if (instance == null) return false;
            bool found = false;
            float best = float.MaxValue;
            for (int i = 0; i < instance.zones.Length; i++)
            {
                if (!instance.zones[i].Active || !instance.zones[i].Spec.Grown) continue;
                Vector3 position = instance.zones[i].Position;
                if (!GrownRules.Within(position, from, reach)) continue;
                Vector3 offset = position - aim;
                offset.y = 0f;
                if (offset.sqrMagnitude >= best) continue;
                best = offset.sqrMagnitude;
                where = position;
                found = true;
            }
            return found;
        }

        /// <summary>Moves the grown thing standing at <paramref name="at"/> (within half a metre) to <paramref name="to"/> (Changeling's change of places). True when one moved.</summary>
        public static bool MoveGrown(Vector3 at, Vector3 to)
        {
            if (instance == null) return false;
            for (int i = 0; i < instance.zones.Length; i++)
            {
                if (!instance.zones[i].Active || !instance.zones[i].Spec.Grown) continue;
                Vector3 offset = instance.zones[i].Position - at;
                offset.y = 0f;
                if (offset.sqrMagnitude > 0.25f) continue;
                instance.zones[i].Position = new Vector3(to.x, instance.zones[i].Position.y, to.z);
                instance.zones[i].Spec.Position = instance.zones[i].Position;
                return true;
            }
            return false;
        }

        public static bool GrownNear(Vector3 at, float metres, int family)
        {
            if (instance == null) return false;
            for (int i = 0; i < instance.zones.Length; i++)
            {
                if (!instance.zones[i].Active || !instance.zones[i].Spec.Grown) continue;
                if (family != 0 && instance.zones[i].Spec.Family != family) continue;
                Vector3 offset = instance.zones[i].Position - at;
                offset.y = 0f;
                if (offset.sqrMagnitude <= metres * metres) return true;
            }
            return false;
        }

        /// <summary>
        /// Tends the grown thing of the player's nearest <paramref name="at"/> within <paramref name="metres"/> (the ledger's
        /// at-grown): it stands <paramref name="longer"/> seconds more, lays <paramref name="alsoLays"/> with every bite from
        /// now on when that names one, and where it stands is given back. False when nothing grown stands that near.
        /// </summary>
        public static bool Tend(Vector3 at, float metres, float longer, AfflictionKind alsoLays, out Vector3 where)
        {
            where = Vector3.zero;
            if (instance == null) return false;
            int best = -1;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < instance.zones.Length; i++)
            {
                if (!instance.zones[i].Active || !instance.zones[i].Spec.Grown) continue;
                Vector3 offset = instance.zones[i].Position - at;
                offset.y = 0f;
                float distance = offset.sqrMagnitude;
                if (distance > metres * metres || distance >= bestDistance) continue;
                best = i;
                bestDistance = distance;
            }
            if (best < 0) return false;
            ref var zone = ref instance.zones[best];
            zone.Left += Mathf.Max(0f, longer);
            if (alsoLays != AfflictionKind.None) zone.Spec.AlsoLays = alsoLays;
            where = zone.Position;
            return true;
        }

        /// <summary>
        /// Drinks up every patch that lays <paramref name="kind"/> within <paramref name="radius"/> of a place (a cloud, a trail),
        /// and it is gone. Returns the doses it held: each patch's own, at least one.
        /// </summary>
        public static int Drink(AfflictionKind kind, Vector3 at, float radius)
        {
            if (instance == null || kind == AfflictionKind.None) return 0;
            int doses = 0;
            for (int i = 0; i < instance.zones.Length; i++)
            {
                ref var zone = ref instance.zones[i];
                if (!zone.Active || zone.Spec.Blow.Affliction != kind) continue;
                Vector3 offset = zone.Position - at;
                offset.y = 0f;
                float reach = radius + zone.Spec.Radius;
                if (offset.sqrMagnitude > reach * reach) continue;
                zone.Active = false;
                doses += Mathf.Max(1, zone.Spec.Blow.Stacks);
                Puff(zone.Spec, zone.Position, 2f);
            }
            return doses;
        }

        /// <summary>Bursts everything the player has grown, each where it stands, and it is gone. Returns how many burst.</summary>
        public static int BurstGrown(in SpellBlow blow, float radius) => BurstGrown(blow, radius, Vector3.zero, 0f, null);

        /// <summary>
        /// Bursts what the player has grown within <paramref name="reach"/> of <paramref name="from"/> (everything when the reach
        /// is 0), each where it stands, and it is gone; where each burst is written into <paramref name="burstAt"/> as far as
        /// it holds (the ledger's grown-bursts: the moment each burst is). Returns how many burst.
        /// </summary>
        public static int BurstGrown(in SpellBlow blow, float radius, Vector3 from, float reach, Vector3[] burstAt)
        {
            if (instance == null || owner == null) return 0;
            int count = 0;
            for (int i = 0; i < instance.zones.Length; i++)
            {
                if (!instance.zones[i].Active || !instance.zones[i].Spec.Grown) continue;
                if (!GrownRules.Within(instance.zones[i].Position, from, reach)) continue;
                instance.zones[i].Active = false;
                Spent(instance.zones[i]);
                owner.StrikeArea(instance.zones[i].Position, radius, blow);
                Puff(instance.zones[i].Spec, instance.zones[i].Position, 3f);
                if (burstAt != null && count < burstAt.Length) burstAt[count] = instance.zones[i].Position;
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
            float biteEvery = 0.4f, bool once = false, float delay = 0f, Vector3 velocity = default, bool burst = false, Color struck = default)
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
                    // Ground left by the element on the weapon bites with that element's softened flash (Hit.Tint).
                    Struck = struck,
                },
            });
        }

        /// <summary>Lays a patch described in full.</summary>
        public static void Lay(in HazardSpec spec)
        {
            if (instance == null) return;
            bool grown = spec.Grown || Growing;
            if (spec.Limit > 0)
            {
                // Things of one kind share a limit (grown things, a mark's helpers): one more takes the place of the one nearest its end.
                int standing = 0, oldest = -1;
                for (int slot = 0; slot < instance.zones.Length; slot++)
                {
                    if (!instance.zones[slot].Active || instance.zones[slot].Spec.Grown != grown || instance.zones[slot].Spec.Family != spec.Family) continue;
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
            // What grows back comes up where it stood (Wood Grows Back).
            while (Regrowing.TakeDue(Time.time, out HazardSpec again))
            {
                Lay(again);
                GrownBack++;
            }
            for (int i = 0; i < zones.Length; i++)
            {
                if (!zones[i].Active) continue;
                ref var zone = ref zones[i];
                // A helper walking to its enemy bites nothing on the way, and keeps to it once there.
                if (zone.Spec.WalksTo > 0f && zone.Delay <= 0f && zone.Spec.Target != null && zone.Spec.Target.gameObject.activeInHierarchy
                    && !WalkedTo(ref zone.Position, zone.Spec.Target.position, zone.Spec.Radius, zone.Spec.WalksTo * deltaTime))
                {
                    zone.Left -= deltaTime;
                    zone.UntilShow -= deltaTime;
                    if (zone.UntilShow <= 0f && !zone.Spec.Quiet)
                    {
                        zone.UntilShow = ShowMovingEvery;
                        Puff(zone.Spec, zone.Position + Vector3.up * 0.3f, 1.5f);
                    }
                    if (zone.Left <= 0f) zone.Active = false;
                    continue;
                }
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
                if (zone.UntilShow <= 0f && !zone.Spec.Quiet)
                {
                    zone.UntilShow = moving ? ShowMovingEvery : ShowEvery;
                    Vector2 spot = Random.insideUnitCircle * (zone.Spec.Radius * (moving ? 0.35f : 0.8f));
                    Puff(zone.Spec, zone.Position + new Vector3(spot.x, 0.3f, spot.y), moving ? 2f : 1f);
                }
                zone.UntilBite -= deltaTime;
                if (zone.UntilBite <= 0f && owner != null)
                {
                    zone.UntilBite = zone.Spec.BiteEvery;
                    if (zone.Spec.CrackReach > 0f)
                    {
                        // A stone that cracks once, when an enemy first stands at it (Fire-Cracked): nothing until then.
                        if (owner.EnemyNear(zone.Position, zone.Spec.Radius, null) != null)
                        {
                            owner.StrikeArea(zone.Position, zone.Spec.CrackReach, zone.Spec.Blow);
                            Vfx.Play(VfxKind.Burst, zone.Position + Vector3.up * 0.3f);
                            zone.Active = false;
                            continue;
                        }
                        if (zone.Left <= 0f) zone.Active = false;
                        continue;
                    }
                    int hits = zone.Spec.Seams > 0
                        ? owner.StrikeStar(zone.Position, zone.Spec.Seams, zone.Spec.Radius, GroundRules.SeamHalf, zone.Spec.SeamWay, zone.Spec.Blow, LeftAlone(i, zone))
                        : owner.StrikeArea(zone.Position, zone.Spec.Radius, zone.Spec.Blow, LeftAlone(i, zone));
                    if (zone.Spec.AlsoLays != AfflictionKind.None && (!zone.Spec.StopsAtFirst || hits > 0)) owner.AfflictArea(zone.Position, zone.Spec.Radius, zone.Spec.AlsoLays, 1);
                    if (zone.Spec.StopsAtFirst && hits > 0)
                    {
                        zone.Active = false;
                        Spent(zone);
                        continue;
                    }
                    // What the player's marks make every grown thing lay with its bites (Ore in the Root, Hidden in the Tree).
                    if (zone.Spec.Grown && GrownBitesMask != 0 && hits > 0)
                        for (int k = 1; k < 32; k++)
                            if ((GrownBitesMask & (1 << k)) != 0) owner.AfflictArea(zone.Position, zone.Spec.Radius, (AfflictionKind)k, 1);
                    // Ground that keeps acting: it draws in, and its mark sets what it sets on whoever stands in it now.
                    if (zone.Spec.Pull > 0f) owner.Displace(zone.Position, zone.Spec.Radius, GroundRules.PullStep(zone.Spec.Pull, zone.Spec.BiteEvery), true);
                    if (zone.Spec.Carry.sqrMagnitude > 0f) owner.ShoveArea(zone.Position, zone.Spec.Radius, zone.Spec.Carry);
                    if (zone.Spec.Acts != null) GroundActs?.Invoke(zone.Spec.Acts, zone.Position, zone.Spec.Radius, zone.Spec.BiteEvery, zone.Spec.LaidFor);
                    zone.Bitten++;
                    // A standing storm drops a bolt somewhere in itself each time it bites, whether or not anyone is under it.
                    if (zone.Spec.Quiet) { }
                    else if (zone.Spec.Kind == HazardKind.Lightning && (hits > 0 || !zone.Spec.Once))
                    {
                        Vector2 spot = zone.Spec.Once ? Vector2.zero : Random.insideUnitCircle * (zone.Spec.Radius * 0.7f);
                        LightningBolts.Strike(zone.Position + new Vector3(spot.x, 0f, spot.y), 0.2f);
                    }
                    else if (zone.Spec.Kind == HazardKind.Element && !zone.Spec.Once)
                    {
                        Vfx.Play(VfxKind.TintSpark, zone.Position + Vector3.up * 0.6f, Vector3.up, 0.8f, zone.Spec.Tint);
                    }
                    if (zone.Spec.Bites > 0 && zone.Bitten >= zone.Spec.Bites)
                    {
                        zone.Active = false;
                        Spent(zone);
                    }
                }
                if (zone.Left <= 0f)
                {
                    zone.Active = false;
                    // Ground that keeps acting is told as its time runs out, for what flares at its end.
                    if (zone.Spec.Acts != null && owner != null) GroundEnds?.Invoke(zone.Spec.Acts, zone.Position, zone.Spec.Radius, zone.Spec.BiteEvery, zone.Spec.LaidFor);
                }
            }
        }

        /// <summary>
        /// Told at each bite of ground that keeps acting (HazardSpec.Acts): the mark's effect, where the ground lies, how wide it
        /// is, the seconds between its bites and the enemy it was laid for (or null). The player's marks set its state on whoever
        /// stands in it, do its effects on each one, and mend the player standing in it.
        /// </summary>
        public static event System.Action<object, Vector3, float, float, object> GroundActs;

        // Domain reload is off: a listener from the last play must not be told in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetGroundActs()
        {
            GroundActs = null;
            GroundEnds = null;
        }

        /// <summary>Told as ground that keeps acting runs out of time, with the same news as <see cref="GroundActs"/>.</summary>
        public static event System.Action<object, Vector3, float, float, object> GroundEnds;

        /// <summary>
        /// One step of a helper walking to its enemy: moves <paramref name="at"/> up to <paramref name="step"/> toward
        /// <paramref name="target"/>, and is true once it stands within reach of it (a little inside its own bite).
        /// </summary>
        public static bool WalkedTo(ref Vector3 at, Vector3 target, float radius, float step)
        {
            Vector3 gap = target - at;
            gap.y = 0f;
            float reach = Mathf.Max(0.5f, radius * 0.6f);
            if (gap.magnitude <= reach) return true;
            at += Vector3.ClampMagnitude(gap, Mathf.Min(step, gap.magnitude - reach * 0.9f));
            return false;
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
