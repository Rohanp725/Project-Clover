using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    // The pairs three apart on the ring (docs/design/steel/pairs_d3.md), README numbers 23 to 33. Every one rests 6 s
    // (AfflictionPairs.DistanceThreeRest) unless it says more, because most of them hold an enemy or set up a heavy blow.
    public static partial class Reactions
    {
        public const string Cracked = "thermalshock.cracked";
        /// <summary>Seized's live film by name (UpgradeEffect.Ground): what Grounded's discharge runs through.</summary>
        public const string LiveWater = "seized.livewater";
        /// <summary>Fish Poison's fouled slick by name (UpgradeEffect.Ground): what The Tide Brings It's wave carries.</summary>
        public const string FouledSlick = "fishpoison.slick";
        public const string WindRooted = "windthrow.rooted";
        public const string Bulge = "frostheave.bulge";
        public const string Rusting = "rust.rusting";
        public const string PlateFallen = "rust.plate";
        public const string Herb = "shiningherb.herb";
        public const string StoneClimbs = "turnedtostone.climbing";
        public const string StoneClimbed = "turnedtostone.climbed";
        public const string Stone = "turnedtostone.stone";
        public const string StoneSpellproof = "turnedtostone.spellproof";
        public const string Beads = "quicksilver.beads";
        public const string BurningPoint = "kindlingpoint.point";
        public const string Cloud = "grudge.cloud";
        public const string LooseDust = "illwind.loose";

        private static void AddThree(List<Upgrade> to)
        {
            // Fire and Ice (23). The body cracks: 14 damage, and it is Cracked for 5 s. The next heavy blow splits it, 30
            // damage and 40 stagger, and the crack is spent. Both afflictions stay.
            to.Add(Reaction(AfflictionKind.Burn, AfflictionKind.Chill,
                "Burn meets Chill: the body cracks, 14. The next heavy blow within 5 s splits it, 30 and 40 stagger.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 14f, Affliction = AfflictionKind.Chill },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Cracked, Seconds = 5f, EndsWith = ConditionBlow.Heavy } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Cracked, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 30f, Stagger = 40f,
                    Affliction = AfflictionKind.Burn }));

            // Lightning and Water (24), only once a Soaked enemy carries two Static or more (the engine waits for the
            // depth, in either order, and nothing rests until then). Held 1 s (elites 0.4 s); the water runs off into a
            // live film 2.5 m round its feet for 3 s that gives one Static every half second to what stands in it. Its
            // own Static is not spent.
            // A god is held by GodHolds (the man 0.3 s, a godly form 0.12 s, neither again while its guard lasts).
            // The film spares the seized one it ran from (SparesTarget), and is live water by name (Grounded runs its discharge
            // through it).
            to.Add(Reaction(AfflictionKind.Static, AfflictionKind.Soaked,
                "Soaked and two Static or more: the current locks its muscles and holds it 1 s, and the water runs off it into a live film that charges all the others who stand in it for 3 s.",
                new ReactionDef { DepthOf = AfflictionKind.Static, Depth = 2 },
                // Unseen: drawn by Seized's own calm crackle, with no ring at the hold and no bolt from the sky at each of the
                // film's bites (the owner, 2026-11-20: "remove the circles ... make it less flickery").
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hold, Seconds = 1f, Unseen = true },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Radius = 2.5f, Seconds = 3f, Interval = 0.5f, Affliction = AfflictionKind.Static, Stacks = 1, Unseen = true, SparesTarget = true,
                    Ground = LiveWater }));

            // Wind and Life (25). Rooted 2 s (it can turn and swing). The next heavy blow while rooted tears it out:
            // thrown 6 m (twice Thunder Gust's 3) away from the player, 14 damage, 35 stagger, and a torn pit 1.5 m
            // across stands 5 s where it stood, seeding what walks into it; the pit counts as grown.
            // Any shove or throw tears it out (StruckBy.Shove: a Gust, a ram, Riptide, a lift), a heavy blow among them.
            to.Add(Reaction(AfflictionKind.Unsteady, AfflictionKind.Seeded,
                "Unsteady meets Seeded: the seed roots it for 2 s. A heavy blow while it is rooted tears it out and throws it far, 14 and 35 stagger, leaving a pit that seeds what walks in.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = WindRooted, Seconds = 2f, Flags = ConditionFlags.Rooted, EndsWith = ConditionBlow.Shove } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = WindRooted, By = StruckBy.Shove, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 14f, Stagger = 35f,
                    Affliction = AfflictionKind.Seeded },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = WindRooted, By = StruckBy.Shove, Do = UpgradeAction.Hazard, Radius = 0.75f, Seconds = 5f, Interval = 0.5f,
                    Grown = true, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = WindRooted, By = StruckBy.Shove, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 6f }));

            // Ice and Earth (26). After 0.4 s the floor heaves up under it: thrown up 1 s, 12 damage, 25 stagger. What
            // cannot be lifted only reels and takes the stagger.
            // The heaved ground stays as the rimed mound, 1.5 m across, standing 6 s behind it, that nothing walks through
            // (UpgradeAction.Raise; it stands a step back so the lifted body comes down beside it, not inside it).
            to.Add(Reaction(AfflictionKind.Chill, AfflictionKind.Tremor,
                "Chill meets Tremor: the cold gets into the ground and a moment later heaves it up under it, 12 and 25 stagger, thrown into the air for 1 s.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Bulge, Seconds = 0.4f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Bulge, Do = UpgradeAction.Raise, Raises = Clover.World.StandingKind.RimedMound, RaiseAt = Clover.World.StandingPlace.Behind, Reach = 1.1f, Seconds = 6f, Amount = 30f },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Bulge, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 12f, Stagger = 25f, Affliction = AfflictionKind.Tremor },
                // The burst carries the heave's stagger; the lift's own, for what only reels, is kept to a token.
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Bulge, Do = UpgradeAction.Lift, Seconds = 1f, Stagger = 1f }));

            // Water and Metal (27). While it is Soaked its notches do not fade and the rust spreads: one more notch every
            // 2 s. Once it is notched to full and rusts on, a plate falls: for 6 s it takes weapon blows as if notched two
            // deeper (16% more), wet or dry.
            // While it rusts its notches may run past three, up to five, and the plate falls at five.
            to.Add(Reaction(AfflictionKind.Soaked, AfflictionKind.Sundered,
                "Soaked meets Sundered: the wet cuts rust. While it is wet its notches stay and spread, one every 2 s, and once it is rusted through a plate falls: it takes more from your weapon for 6 s.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Rusting, Seconds = 30f, Beat = 2f, While = AfflictionKind.Soaked, Keeps = AfflictionKind.Sundered,
                        Deepens = AfflictionKind.Sundered, DeepensBy = 2 } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Rusting, Needs = AfflictionKind.Sundered, NeedsFull = true, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = PlateFallen, Seconds = 6f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.16f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Rusting, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Sundered, Stacks = 1 }));

            // Life and Light (28). A shining herb lies 6 s on the floor at its feet, on the player's side, a pickup (World.Pickups,
            // three at most): walked over, it heals 8 and makes the next weapon blow critical, and does not flower; left
            // unpicked it flowers at the end, Marking and Seeding every enemy within 3 m of it.
            to.Add(Reaction(AfflictionKind.Seeded, AfflictionKind.Marked,
                "Seeded meets Marked: a shining herb springs up at its feet for 6 s. Walk over it to pick it: you heal 8 and your next blow is critical. Left alone it flowers, Marking and Seeding all within 3 m.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.DropPickup, Drops = Clover.World.PickupKind.Herb, RaiseAt = Clover.World.StandingPlace.Before, Reach = 1f, Seconds = 6f, Limit = 3 },
                new UpgradeEffect { On = UpgradeTrigger.PickedUp, Do = UpgradeAction.Heal, Amount = 8f },
                new UpgradeEffect { On = UpgradeTrigger.PickedUp, Do = UpgradeAction.CriticalNext, Seconds = 4f },
                new UpgradeEffect { On = UpgradeTrigger.PickupLeft, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Marked, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.PickupLeft, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Seeded, Stacks = 1 }));

            // Earth and Dark (29). Stone climbs it over 2 s (30% slower, then 60%), then it stands as stone for 1.5 s:
            // it neither walks nor begins an attack, takes double stagger and half damage from spells. A heavy blow on the
            // stone breaks it out at once for 50 stagger. An elite is slowed half as much and stands as stone half as long;
            // a god takes none of it.
            to.Add(Reaction(AfflictionKind.Tremor, AfflictionKind.Cursed,
                "Tremor meets Cursed: stone climbs it over 2 s, slowing it to a stop, then it stands as stone: double stagger, half from spells. A heavy blow breaks it out, 50 stagger.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = StoneClimbs, Seconds = 1f, Pace = 0.7f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = StoneClimbs, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = StoneClimbed, Seconds = 1f, Pace = 0.4f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = StoneClimbed, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Stone, Seconds = 1.5f, Flags = ConditionFlags.Rooted | ConditionFlags.BeginsNoAttack | ConditionFlags.StaggerTakenShare, Scale = 2f, EndsWith = ConditionBlow.Heavy } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = StoneClimbed, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = StoneSpellproof, Seconds = 1.5f, Flags = ConditionFlags.SpellTakenShare, Scale = 0.5f, EndsWith = ConditionBlow.Heavy } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Stone, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 0.5f, Stagger = 50f, Affliction = AfflictionKind.Tremor }));

            // Metal and Poison (30). While it carries both, each weapon blow on it makes its poison bite at once, at its
            // depth, besides its beat (once every half second at most). The doses are not spent.
            to.Add(Reaction(AfflictionKind.Sundered, AfflictionKind.Poison,
                "Sundered meets Poisoned: mercury runs into the cracks. While it carries both, each of your weapon blows makes its poison bite at once.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Beads, Seconds = 30f, While = AfflictionKind.Sundered, While2 = AfflictionKind.Poison } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Beads, By = StruckBy.Weapon, Cooldown = 0.5f, CooldownPerEnemy = true, Do = UpgradeAction.Bite }));

            // Light and Fire (31). For as long as the Mark's three blows (8 s at most), each weapon blow on it (critical,
            // for it is Marked) deepens its Burn by one; the third sets it alight at the point: all its Burn bursts out,
            // 10 for each Burn, and it goes on burning two deep.
            to.Add(Reaction(AfflictionKind.Marked, AfflictionKind.Burn,
                "Marked meets Burn: the light gathers to a burning point. Each critical blow deepens its Burn, and the third sets it alight, 10 for each Burn.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = BurningPoint, Seconds = 8f, Blows = 3 } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = BurningPoint, By = StruckBy.Weapon, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = BurningPoint, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Burn },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = BurningPoint, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Burst, Radius = 0.6f, Damage = 10f,
                    PerStack = true, Affliction = AfflictionKind.Burn },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = BurningPoint, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 2 }));

            // Dark and Lightning (32), rest 8 s, only once a Cursed enemy carries two Static or more (in either order).
            // A black cloud gathers over it for 8 s, and the enemy's next blow at you, landing or turned (the their-blow
            // moment), calls it down: a black bolt that discharges all its Static at double (14 a stack in 4 m) and stops
            // it for 0.6 s. If it dies first the cloud drifts whole to the nearest enemy within 6 m (ConditionSpec.DriftsTo).
            to.Add(Reaction(AfflictionKind.Cursed, AfflictionKind.Static,
                "Cursed and two Static or more: a black cloud gathers over its head and waits for the next blow it aims at you. Landed or guarded, the cloud strikes it: its Static discharges at double, and it is stopped for a moment.",
                new ReactionDef { Rest = 8f, DepthOf = AfflictionKind.Static, Depth = 2 },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Cloud, Seconds = 8f, EndsWith = ConditionBlow.TheirBlow, DriftsTo = 6f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionAttacked, Condition = Cloud, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Damage = 7f, Stagger = 5f },
                new UpgradeEffect { On = UpgradeTrigger.ConditionAttacked, Condition = Cloud, Do = UpgradeAction.Hold, Seconds = 0.6f }));

            // Poison and Wind (33). Its poison goes loose while it carries both. A heavy blow (which shoves) sheds all of
            // it at once in a plume along its path, away from the player: three patches 1.2 m wide that lie 3 s and dose
            // each enemy in them once a second.
            // Every shove sheds it (StruckBy.Shove: a Gust, a lift, a heavy blow). NEEDS: ground laid behind a moving enemy (the plume
            // along the path it travels, plume-along-path); the doses shed shared among the enemies the plume settles on.
            to.Add(Reaction(AfflictionKind.Poison, AfflictionKind.Unsteady,
                "Poisoned meets Unsteady: its poison goes loose. When your heavy blow shoves it, it sheds all its poison in a plume behind it that doses all who stand in it.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = LooseDust, Seconds = 30f, While = AfflictionKind.Poison, While2 = AfflictionKind.Unsteady } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = LooseDust, By = StruckBy.Shove, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Poison },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = LooseDust, By = StruckBy.Shove, Do = UpgradeAction.Hazard, Shape = HazardShape.Downwind, Count = 3,
                    Reach = 2.2f, Amount = 1.2f, Radius = 0.6f, Seconds = 3f, Interval = 1f, Affliction = AfflictionKind.Poison, Stacks = 1, SparesTarget = true }));
        }
    }
}
