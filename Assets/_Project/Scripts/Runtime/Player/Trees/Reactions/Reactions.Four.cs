using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    // The pairs four apart on the ring (docs/design/steel/pairs_d4.md), README numbers 34 to 44. Order does not matter in
    // any of them; most give their own rest.
    public static partial class Reactions
    {
        public const string Rocking = "loganstone.rocking";
        public const string Leaning = "loganstone.lean";
        public const string Felled = "loganstone.felled";
        public const string Brittle = "coldshort.brittle";
        public const string PlatesOff = "coldshort.cracked";
        public const string Bare = "coldshort.bare";
        public const string LightNet = "caughtlight.caught";
        public const string Coils = "girdled.coils";
        public const string Bound = "girdled.bound";
        public const string Tight = "girdled.tight";
        public const string Damp = "chokedamp.damp";
        public const string DampHeavy = "chokedamp.heavy";
        public const string Glowing = "redhot.glowing";
        public const string Thread = "boltrunstrue.thread";
        public const string Winded = "breathtaken.winded";
        public const string VenomCrust = "venomrime.crust";

        private static void AddFour(List<Upgrade> to)
        {
            // Fire and Water (34). The water flashes off the burning body: Soaked is spent and half the Burn (rounded up)
            // goes with it, in a scalding puff of 2.5 m: 8 damage and 4 more for each Burn spent, 15 stagger, to every
            // enemy in it. Once every 3 s on one enemy.
            to.Add(Reaction(AfflictionKind.Burn, AfflictionKind.Soaked,
                "Burn meets Soaked: the water flashes off in a scalding puff, 8 and 4 for each Burn it takes, to all within 2.5 m. The water is gone and half the Burn with it.",
                3f,
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Soaked },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Burn, Share = 0.5f },
                // Round 4: the scald waits until the steam has visibly flashed off (the look's drop lands at 0.45 s).
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Affliction = AfflictionKind.Soaked, Radius = 2.5f, Damage = 8f, DamagePerSpent = 4f, Stagger = 15f, Delay = LandsAfter }));

            // Lightning and Life (35). The fern itself throws one bolt to the nearest other Seeded enemy within 8 m: 8
            // damage, 10 stagger and one Static. The enemy keeps both afflictions.
            // Every grown thing of the player's within 8 m throws a bolt into it too (UpgradeAction.GrownBolts: four at most, the
            // same numbers).
            to.Add(Reaction(AfflictionKind.Static, AfflictionKind.Seeded,
                "Static meets Seeded: a red fern of lightning flowers on its skin and throws a bolt to the nearest Seeded enemy within 8 m, 8 and a Static.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Arc, LeapsTo = AfflictionKind.Seeded, Count = 1, Reach = 8f, Damage = 8f, Stagger = 10f,
                    Affliction = AfflictionKind.Static, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.GrownBolts, Count = 4, Reach = 8f, Damage = 8f, Stagger = 10f,
                    Affliction = AfflictionKind.Static, Stacks = 1 }));

            // Wind and Earth (36), rest 6 s. It rocks for 3 s: the first blow breaks its balance at 1.25 times, the next at
            // 1.5, and the third weapon blow throws it down hard: flat 1.8 s, and its fall strikes 20 damage, 30 stagger and
            // a Tremor in 3 m. An elite rocks and then reels (40 stagger); a god takes only the stagger.
            to.Add(Reaction(AfflictionKind.Unsteady, AfflictionKind.Tremor,
                "Unsteady meets Tremor: it rocks on its feet for 3 s and each blow breaks its balance harder. Your third weapon blow throws it down hard, 20 and a Tremor to all within 3 m.",
                6f,
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Rocking, Seconds = 3f, Blows = 3 } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Leaning, Seconds = 3f, Flags = ConditionFlags.StaggerTakenShare, Scale = 1.25f } },
                // Set again from the same mark it is refreshed with the new number and keeps its time.
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Rocking, By = StruckBy.Weapon, AtCount = 1, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Leaning, Seconds = 2f, Flags = ConditionFlags.StaggerTakenShare, Scale = 1.5f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Rocking, By = StruckBy.Weapon, AtCount = 3, Ranks = EnemyRank.Plain, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Felled, Seconds = 1.8f, Flags = ConditionFlags.Down } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Rocking, By = StruckBy.Weapon, AtCount = 3, Ranks = EnemyRank.Plain, Do = UpgradeAction.Burst, Radius = 3f,
                    Damage = 20f, Stagger = 30f, Affliction = AfflictionKind.Tremor, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Rocking, By = StruckBy.Weapon, AtCount = 3, Ranks = EnemyRank.Elite, Do = UpgradeAction.Burst, Radius = 0.5f,
                    Stagger = 40f, Affliction = AfflictionKind.Tremor }));

            // Ice and Metal (37), rest 5 s. Brittle 5 s: its notches do not fade. The next heavy blow cracks its plates
            // off: for 6 s it takes 25% more from weapon blows, and its notches stay for the rest of the fight. No burst.
            // NEEDS: an affliction that never fades for the fight: two minutes of keeping stands in.
            to.Add(Reaction(AfflictionKind.Chill, AfflictionKind.Sundered,
                "Chill meets Sundered: frost fills the notches and the armour goes brittle for 5 s. Your next heavy blow cracks the plates off: 25% more from your weapon for 6 s, and the notches stay.",
                5f,
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Brittle, Seconds = 5f, Keeps = AfflictionKind.Sundered, EndsWith = ConditionBlow.Heavy } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Brittle, By = StruckBy.Heavy, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = PlatesOff, Seconds = 6f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.25f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Brittle, By = StruckBy.Heavy, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Bare, Seconds = 120f, Keeps = AfflictionKind.Sundered } }));

            // Water and Light (38), rest 6 s. A net of light on the wet floor 3 m round it for 4 s: what stands in it is
            // Marked, and what stood in it as it fell is 25% slower for the 4 s. The mark that made it is not spent: the
            // net renews it.
            // NEEDS: ground that slows: an enemy that walks onto the net later is Marked but not slowed.
            to.Add(Reaction(AfflictionKind.Soaked, AfflictionKind.Marked,
                "Soaked meets Marked: a net of light lies on the wet floor 3 m round it for 4 s. All in it are Marked and caught, a quarter slower.",
                6f,
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Radius = 3f, Seconds = 4f, Interval = 0.5f, Affliction = AfflictionKind.Marked, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, Radius = 3f, State = new ConditionSpec { Id = LightNet, Seconds = 4f, Pace = 0.75f } }));

            // Life and Dark (39), rest 6 s. One coil at once: slowed 25%, and it takes 6% more from the weapon a coil.
            // Each weapon blow draws one coil tighter and mends the player 1; at four coils (the third blow) it is
            // girdled: held 1.5 s while the roots crush, 20 damage and 40 stagger, and a fifth of that (4) heals the
            // player. The roots hold 8 s for the blows to come.
            // Coiled, it neither leaps nor steps back (ConditionFlags.Mired on the binding roots).
            // NEEDS: standing things that block: the hollow trunk left for 8 s where it dies girdled, blocking and Seeding and
            // Cursing what touches it.
            to.Add(Reaction(AfflictionKind.Seeded, AfflictionKind.Cursed,
                "Seeded meets Cursed: black fig roots coil round it, slowing it. Each weapon blow draws them tighter, mends you 1 and opens it to your weapon; at four coils they crush it, 20, held 1.5 s.",
                6f,
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Coils, Seconds = 8f, Blows = 3 } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Bound, Seconds = 8f, Pace = 0.75f, Flags = ConditionFlags.Mired } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Tight, Seconds = 8f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.06f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Coils, By = StruckBy.Weapon, Do = UpgradeAction.Heal, Amount = 1f },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Coils, By = StruckBy.Weapon, AtCount = 1, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Tight, Seconds = 8f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.12f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Coils, By = StruckBy.Weapon, AtCount = 2, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Tight, Seconds = 8f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.18f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Coils, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Tight, Seconds = 8f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.24f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Coils, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Hold, Seconds = 1.5f },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Coils, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 20f, Stagger = 40f,
                    Affliction = AfflictionKind.Cursed },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Coils, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Heal, Amount = 4f }));

            // Earth and Poison (40), rest 6 s. The damp pools under it and runs to each other shaken enemy within 7 m (three
            // at most): for 5 s each takes a dose every second and moves 20% slower (and, shaken, regains no balance).
            // NEEDS: the pools as ground (2 m, left where they pool, dosing whatever stands in them); here the damp goes
            // with the enemies it reached.
            to.Add(Reaction(AfflictionKind.Tremor, AfflictionKind.Poison,
                "Tremor meets Poisoned: the cracked ground breathes out a heavy damp that pools under it and runs to three shaken enemies near. In it they take a dose each second and move slower.",
                6f,
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Damp, Seconds = 5f, Beat = 1f } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = DampHeavy, Seconds = 5f, Pace = 0.8f } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, Radius = 7f, Count = 3, LeapsTo = AfflictionKind.Tremor,
                    State = new ConditionSpec { Id = Damp, Seconds = 5f, Beat = 1f, ToOthers = true } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, Radius = 7f, Count = 3, LeapsTo = AfflictionKind.Tremor,
                    State = new ConditionSpec { Id = DampHeavy, Seconds = 5f, Pace = 0.8f, ToOthers = true } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Damp, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Poison, Stacks = 1 }));

            // Metal and Fire (41), rest 5 s. The armour glows red 5 s; each heavy blow on it throws a spray of sparks out
            // of its far side, a cone 3 m long (two bursts on the player's line beyond it): 6 damage and a Burn to each.
            // Its burns bite twice as hard while it glows (ConditionSpec.BitesHarder).
            to.Add(Reaction(AfflictionKind.Sundered, AfflictionKind.Burn,
                "Sundered meets Burn: the notched armour heats red for 5 s. Each heavy blow on it sprays sparks out of its far side, 6 and a Burn to all behind it.",
                5f,
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Glowing, Seconds = 5f, BitesHarder = AfflictionKind.Burn, BiteScale = 2f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Glowing, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 2,
                    Reach = 1.9f, Amount = 1.1f, Radius = 0.8f, Damage = 6f, Affliction = AfflictionKind.Burn, Stacks = 1 }));

            // Light and Lightning (42), rest 5 s. A thread of light, and 0.5 s later a straight bolt that runs down a lane
            // from it, straight away from the player, 8 m long and 1.2 m wide (the owner's word of 2026-11-20): 20 damage,
            // 35 stagger and a Static to it and to everyone on the lane; then its own Static discharges at full (7 damage
            // and 5 stagger a stack in 4 m). The mark is not spent.
            to.Add(Reaction(AfflictionKind.Marked, AfflictionKind.Static,
                "Marked meets Static: a thread of light stands from it, then a straight bolt runs from it down an 8 m lane away from you: 20 and 35 stagger to everyone on it, and its Static discharges, 7 a stack in 4 m.",
                5f,
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Thread, Seconds = 0.5f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Thread, Do = UpgradeAction.Lane, Reach = 8f, Radius = 0.6f, Damage = 20f, Stagger = 35f, Affliction = AfflictionKind.Static,
                    Reaction = Clover.Gameplay.AfflictionPairs.Id(Clover.Gameplay.AfflictionPairs.Index(AfflictionKind.Marked, AfflictionKind.Static)) },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Thread, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Stagger = 5f }));

            // Dark and Wind (43), rest 6 s. Winded 1.5 s (an elite 0.6 s): it begins no attack (one already swung is
            // finished) and takes 30% more stagger. A god is winded too: beginning no attack is not a hold.
            to.Add(Reaction(AfflictionKind.Cursed, AfflictionKind.Unsteady,
                "Cursed meets Unsteady: a black gust draws its breath out. Winded for 1.5 s it begins no attack and takes more stagger.",
                6f,
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.Plain | EnemyRank.God, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Winded, Seconds = 1.5f, Flags = ConditionFlags.BeginsNoAttack | ConditionFlags.StaggerTakenShare, Scale = 1.3f } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.Elite, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Winded, Seconds = 0.6f, Flags = ConditionFlags.BeginsNoAttack | ConditionFlags.StaggerTakenShare, Scale = 1.3f } }));

            // Poison and Ice (44). While both lie on it its poison does not fade, and each bite lays a Chill, until it
            // freezes.
            to.Add(Reaction(AfflictionKind.Poison, AfflictionKind.Chill,
                "Poisoned meets Chill: while both lie on it the poison does not fade, and each bite lays a Chill, until it freezes.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = VenomCrust, Seconds = 30f, OnBite = true, While = AfflictionKind.Poison, While2 = AfflictionKind.Chill, Keeps = AfflictionKind.Poison } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = VenomCrust, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 }));
        }
    }
}
