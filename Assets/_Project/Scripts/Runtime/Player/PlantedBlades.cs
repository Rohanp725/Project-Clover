using UnityEngine;

namespace Clover.Player
{
    /// <summary>What a blade of yours standing in an enemy or the ground is, which decides how it is driven.</summary>
    public enum PlantedKind
    {
        /// <summary>Flung Blade's own: driven by the next weapon blow on its enemy, then home to the hand.</summary>
        Flung,
        /// <summary>Cherry Red's: glowing as it cools over six seconds; driven in its first two it strikes double.</summary>
        CherryRed,
        /// <summary>The Nail: only heavy blows drive it, deeper each time (12, 16, 24), and it never comes home.</summary>
        Nail,
    }

    /// <summary>One blade of yours standing in an enemy or in the ground. Plain data: <see cref="PlantedBlades"/> keeps them.</summary>
    public struct PlantedBlade
    {
        public bool Active;
        public PlantedKind Kind;
        /// <summary>The body it stands in (the enemy, in play); null when it stands in the ground.</summary>
        public object Host;
        /// <summary>Where it stands now: kept by the form that shows it, so marks can find it and fly to it.</summary>
        public Vector3 Place;
        public float PlantedAt;
        /// <summary>How many times it has been driven (The Nail goes deeper each time).</summary>
        public int Drives;
        /// <summary>A drive is owed: the form showing it strikes with it on its next step.</summary>
        public bool Driven;
        /// <summary>Called home without a drive: the oldest making way, or every blade called home by All Blades Home.</summary>
        public bool Recalled;
        /// <summary>A heavy blow drives every blade standing in its enemy, not only the oldest (Flung Blade's Empower).</summary>
        public bool HeavyDrivesAll;
        /// <summary>It does not fade with time (All Blades Home): it stands until driven or called.</summary>
        public bool NeverFades;
        /// <summary>A plain drive's damage, as the orders it was thrown under gave it.</summary>
        public float Drive;
        /// <summary>Which planting this is, so a form holding an old handle never moves a newer blade in the same place.</summary>
        public int Stamp;
    }

    /// <summary>The rules a thrown blade is planted and driven by, as the marks in force set them for one throw.</summary>
    public struct BladeOrders
    {
        public PlantedKind Kind;
        /// <summary>Blades of yours one enemy may hold at once; one more and the oldest in it comes home.</summary>
        public int MostInOne;
        public bool HeavyDrivesAll;
        /// <summary>A plain drive's damage, before the kind's own rule (12, or Flung Blade's Empower's 16).</summary>
        public float Drive;
        public bool NeverFades;

        public static BladeOrders Plain => new BladeOrders { Kind = PlantedKind.Flung, MostInOne = 1, Drive = BladeRules.KitDrive };
    }

    /// <summary>The plain numbers of planted blades (metal.md: Flung Blade, its Empower, Cherry Red and The Nail).</summary>
    public static class BladeRules
    {
        /// <summary>The kit's own drive, the number every other drive is written against.</summary>
        public const float KitDrive = 12f;
        /// <summary>Flung Blade's Empower: drives for 16, three may stand in one enemy, and a heavy blow drives them all.</summary>
        public const float EmpoweredDrive = 16f;
        public const int EmpoweredMostInOne = 3;
        /// <summary>Cherry Red: driven within its first two seconds it strikes double; it has cooled to black by six.</summary>
        public const float RedSeconds = 2f, CoolSeconds = 6f;
        /// <summary>The Nail's drives, deeper each heavy blow; the third brings its enemy to its knees.</summary>
        public static readonly float[] NailDrives = { 12f, 16f, 24f };
        public const int NailKneelsAt = 3;
        /// <summary>The Nail stands this long in its enemy, waiting for the heavy blows (its Nailed condition's ten seconds).</summary>
        public const float NailSeconds = 10f;
        /// <summary>A blade that struck no one stands in the ground this long before it comes home (unless it never fades).</summary>
        public const float GroundSeconds = 0.45f;
        /// <summary>To the Blade's reach to a blade of yours, and its Empower's.</summary>
        public const float ToBladeReach = 12f, ToBladeEmpoweredReach = 14f;
        /// <summary>All Blades Home: sixteen at most stand at once.</summary>
        public const int MostStanding = 16;

        /// <summary>True when a blow drives a blade of this kind: The Nail answers heavy blows only, the others any weapon blow.</summary>
        public static bool DrivenBy(PlantedKind kind, bool heavy) => kind != PlantedKind.Nail || heavy;

        /// <summary>How hot a Cherry Red blade still glows, 1 as it lands to 0 when it has cooled.</summary>
        public static float Glow(float plantedAt, float now) => 1f - Mathf.Clamp01((now - plantedAt) / CoolSeconds);

        /// <summary>True while a Cherry Red blade is still red enough to strike double.</summary>
        public static bool StillRed(float plantedAt, float now) => now - plantedAt <= RedSeconds;

        /// <summary>
        /// The damage of the drive a blade is owed now, its <see cref="PlantedBlade.Drives"/> already counting this one, as a
        /// multiple of <paramref name="kitDamage"/> (the kit's cast damage, which already carries the player's spell damage):
        /// its own Drive for a plain blade, double that while a Cherry Red is red, the Nail's step for the drive it is on.
        /// </summary>
        public static float DriveDamage(in PlantedBlade blade, float kitDamage, float now)
        {
            float scale = (blade.Drive > 0f ? blade.Drive : KitDrive) / KitDrive;
            if (blade.Kind == PlantedKind.Nail) scale = NailDrives[Mathf.Clamp(blade.Drives - 1, 0, NailDrives.Length - 1)] / KitDrive;
            else if (blade.Kind == PlantedKind.CherryRed && StillRed(blade.PlantedAt, now)) scale *= 2f;
            return kitDamage * scale;
        }

        /// <summary>True when a blade is spent once driven: every kind but the Nail, and the Nail at its third.</summary>
        public static bool SpentByDrive(in PlantedBlade blade) => blade.Kind != PlantedKind.Nail || blade.Drives >= NailKneelsAt;
    }

    /// <summary>
    /// Every blade of yours standing in an enemy or the ground (the ledger's planted-blades and blades-home), so marks can
    /// count, test, drive and fly to them. The form that threw a blade shows it and keeps its place; a weapon blow, a mark or
    /// a flight to it asks for a drive here, and the form strikes with it on its next step. A blade is named by the handle
    /// <see cref="Plant"/> returns (its place in the list and which planting it was), never by its place alone.
    /// </summary>
    public static class PlantedBlades
    {
        public const int Most = BladeRules.MostStanding;
        private const int SlotBits = 5;
        private static readonly PlantedBlade[] blades = new PlantedBlade[Most];
        private static int stamps;

        // Domain reload is off: no blade from the last play may stand in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Clear();

        public static void Clear()
        {
            for (int i = 0; i < Most; i++) blades[i] = default;
            stamps = 0;
        }

        /// <summary>
        /// Plants a blade in <paramref name="host"/> (null: the ground) at <paramref name="at"/>. One more than its enemy may
        /// hold calls the oldest there home, and with every place taken the oldest of all gives its place up. Returns the
        /// blade's handle.
        /// </summary>
        public static int Plant(in BladeOrders orders, object host, Vector3 at, float now)
        {
            if (host != null && CountIn(host) >= Mathf.Max(1, orders.MostInOne)) RecallSlot(OldestIn(host));
            int slot = -1;
            for (int i = 0; i < Most && slot < 0; i++)
                if (!blades[i].Active) slot = i;
            // Every place taken: the oldest standing blade goes home, and a blade already going home gives its place up at once.
            if (slot < 0)
            {
                slot = OldestIn(null, true);
                if (slot < 0) slot = 0;
                RecallSlot(slot);
            }
            stamps = (stamps + 1) & 0xFFFFFF;
            blades[slot] = new PlantedBlade
            {
                Active = true, Kind = orders.Kind, Host = host, Place = at, PlantedAt = now,
                HeavyDrivesAll = orders.HeavyDrivesAll, NeverFades = orders.NeverFades, Drive = orders.Drive, Stamp = stamps,
            };
            return (stamps << SlotBits) | slot;
        }

        public static PlantedBlade Get(int handle)
        {
            int slot = SlotOf(handle);
            return slot >= 0 ? blades[slot] : default;
        }

        /// <summary>True while the blade stands: neither driven home nor called.</summary>
        public static bool Standing(int handle) => StandingSlot(SlotOf(handle));

        /// <summary>The form keeps the blade's place as its enemy moves.</summary>
        public static void SetPlace(int handle, Vector3 at)
        {
            int slot = SlotOf(handle);
            if (slot >= 0) blades[slot].Place = at;
        }

        /// <summary>Its enemy fell: the blade stands in the ground where it was.</summary>
        public static void ToGround(int handle, Vector3 at)
        {
            int slot = SlotOf(handle);
            if (slot < 0) return;
            blades[slot].Host = null;
            blades[slot].Place = at;
        }

        /// <summary>Blades standing in one body now.</summary>
        public static int CountIn(object host)
        {
            int count = 0;
            for (int i = 0; i < Most; i++)
                if (StandingSlot(i) && host != null && ReferenceEquals(blades[i].Host, host)) count++;
            return count;
        }

        /// <summary>All the blades standing now.</summary>
        public static int Count()
        {
            int count = 0;
            for (int i = 0; i < Most; i++)
                if (StandingSlot(i)) count++;
            return count;
        }

        /// <summary>
        /// A weapon blow landed on <paramref name="host"/>: the oldest blade there it drives is owed a drive, or, for a heavy
        /// blow when a blade there drives all on one, every one it drives. Returns how many were driven.
        /// </summary>
        public static int WeaponBlow(object host, bool heavy)
        {
            if (host == null) return 0;
            bool all = false;
            if (heavy)
                for (int i = 0; i < Most && !all; i++)
                    if (StandingSlot(i) && ReferenceEquals(blades[i].Host, host) && blades[i].HeavyDrivesAll) all = true;
            int driven = 0;
            while (true)
            {
                int oldest = -1;
                for (int i = 0; i < Most; i++)
                {
                    if (!StandingSlot(i) || blades[i].Driven || !ReferenceEquals(blades[i].Host, host) || !BladeRules.DrivenBy(blades[i].Kind, heavy)) continue;
                    if (oldest < 0 || blades[i].PlantedAt < blades[oldest].PlantedAt) oldest = i;
                }
                if (oldest < 0) break;
                DriveSlot(oldest);
                driven++;
                if (!all) break;
            }
            return driven;
        }

        /// <summary>Every blade standing in <paramref name="host"/> is driven at once (Flung Blade's Master on a perfect guard). Returns how many.</summary>
        public static int DriveAllIn(object host)
        {
            if (host == null) return 0;
            int driven = 0;
            for (int i = 0; i < Most; i++)
                if (StandingSlot(i) && !blades[i].Driven && ReferenceEquals(blades[i].Host, host))
                {
                    DriveSlot(i);
                    driven++;
                }
            return driven;
        }

        /// <summary>The blade is owed a drive (To the Blade arriving at it); its count of drives goes up.</summary>
        public static void Drive(int handle) => DriveSlot(SlotOf(handle));

        /// <summary>The form asks whether its blade is owed a drive, and takes it: true once for each drive.</summary>
        public static bool TakeDrive(int handle, out PlantedBlade blade)
        {
            int slot = SlotOf(handle);
            blade = slot >= 0 ? blades[slot] : default;
            if (slot < 0 || !blades[slot].Driven) return false;
            blades[slot].Driven = false;
            blade = blades[slot];
            return true;
        }

        /// <summary>Calls the blade home with no drive: it stands no longer.</summary>
        public static void Recall(int handle) => RecallSlot(SlotOf(handle));

        /// <summary>Calls every standing blade home (All Blades Home's surge). Returns how many; <paramref name="places"/> gets where each stood.</summary>
        public static int RecallAll(Vector3[] places)
        {
            int count = 0;
            for (int i = 0; i < Most; i++)
            {
                if (!StandingSlot(i)) continue;
                if (places != null && count < places.Length) places[count] = blades[i].Place;
                blades[i].Recalled = true;
                count++;
            }
            return count;
        }

        /// <summary>The form is done with the blade: it leaves the list.</summary>
        public static void Remove(int handle)
        {
            int slot = SlotOf(handle);
            if (slot >= 0) blades[slot] = default;
        }

        /// <summary>
        /// The standing blade to fly to from <paramref name="from"/> within <paramref name="reach"/>: the nearest in front of
        /// <paramref name="facing"/> (within 90 degrees either side), else the nearest at all. Its handle, or -1 for none.
        /// </summary>
        public static int Nearest(Vector3 from, Vector3 facing, float reach)
        {
            int ahead = -1, any = -1;
            float aheadSq = reach * reach, anySq = reach * reach;
            facing.y = 0f;
            for (int i = 0; i < Most; i++)
            {
                if (!StandingSlot(i)) continue;
                Vector3 offset = blades[i].Place - from;
                offset.y = 0f;
                float sq = offset.sqrMagnitude;
                if (sq > reach * reach) continue;
                if (sq < anySq || any < 0)
                {
                    anySq = sq;
                    any = i;
                }
                if (Vector3.Dot(offset, facing) >= 0f && (sq < aheadSq || ahead < 0))
                {
                    aheadSq = sq;
                    ahead = i;
                }
            }
            int found = ahead >= 0 ? ahead : any;
            return found >= 0 ? (blades[found].Stamp << SlotBits) | found : -1;
        }

        // The place a handle names, or -1 when that planting has left the list.
        private static int SlotOf(int handle)
        {
            if (handle < 0) return -1;
            int slot = handle & ((1 << SlotBits) - 1);
            return slot < Most && blades[slot].Active && blades[slot].Stamp == handle >> SlotBits ? slot : -1;
        }

        private static bool StandingSlot(int slot) => slot >= 0 && blades[slot].Active && !blades[slot].Recalled;

        private static void DriveSlot(int slot)
        {
            if (!StandingSlot(slot)) return;
            blades[slot].Driven = true;
            blades[slot].Drives++;
        }

        private static void RecallSlot(int slot)
        {
            if (slot >= 0 && blades[slot].Active) blades[slot].Recalled = true;
        }

        // The longest-standing blade in a body (or anywhere, with any), not yet called.
        private static int OldestIn(object host, bool any = false)
        {
            int oldest = -1;
            for (int i = 0; i < Most; i++)
            {
                if (!StandingSlot(i) || (!any && !ReferenceEquals(blades[i].Host, host))) continue;
                if (oldest < 0 || blades[i].PlantedAt < blades[oldest].PlantedAt) oldest = i;
            }
            return oldest;
        }
    }
}
