using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    // The pairs five apart on the ring, the farthest (docs/design/steel/pairs_d5.md), README numbers 45 to 55. Order
    // does not matter in any of them.
    public static partial class Reactions
    {
        public const string Grounded = "earthed.grounded";
        public const string Suns = "mocksuns.suns";
        public const string Swollen = "drownedweight.swollen";
        public const string SwollenHeavy = "drownedweight.heavy";
        public const string Roots = "rotroots.roots";
        public const string IronLive = "liveiron.live";
        public const string Draw = "stolenwarmth.draw";
        public const string Floating = "fishpoison.float";

        private static void AddFive(List<Upgrade> to)
        {
            // Fire and Life (45). The Seeded is spent: one burning seed for each stack it carried (one to three) is thrown
            // 2.5 m out round it and grows at once into a flame lily, a grown thing that stands 5 s and burns what stands
            // within 1 m (3 damage and a Burn every 0.5 s). Its Burn stays.
            // NEEDS: pickups: the first time the player passes within 1 m of a lily they pick its fire up (3 health and one
            // charged blow of Burn on the blade).
            to.Add(Reaction(AfflictionKind.Burn, AfflictionKind.Seeded,
                "Burn meets Seeded: the heat splits the seeds and each grows into a flame lily round it for 5 s, burning what stands near.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Seeded },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Shape = HazardShape.Ring, Count = 1, PerStack = true, Reach = 2.5f, Radius = 1f, Seconds = 5f,
                    Interval = 0.5f, Damage = 3f, Grown = true, Affliction = AfflictionKind.Burn, Stacks = 1 }));

            // Lightning and Earth (46). All its Static runs into the floor at once: 5 damage and 3 stagger for each stack
            // spent, and one Tremor, to every enemy within 4 m; it is rooted 1 s.
            // NEEDS: a burst that spares the one it starts from: the grounded enemy stands on its own figure and takes it
            // too. And the scorched fern left on the floor for 3 s.
            to.Add(Reaction(AfflictionKind.Static, AfflictionKind.Tremor,
                "Static meets Tremor: its charge runs into the ground as a burnt fern 4 m round it, 5 and 3 stagger a stack and a Tremor to all on it. It is rooted 1 s.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Static },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 4f, Damage = 5f, Stagger = 3f, PerStack = true, Affliction = AfflictionKind.Static },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 4f, Affliction = AfflictionKind.Tremor, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Grounded, Seconds = 1f, Flags = ConditionFlags.Rooted } }));

            // Wind and Metal (47). Its loose plates whirl round it 1.5 m for 3 s: one shard for each notch, plus one, each
            // cutting whatever it passes, the enemy itself included, every 0.4 s: 3 damage a shard and 4 stagger.
            // NEEDS: a hazard's stagger scaled by a count: the stagger is one shard's whatever the shards.
            to.Add(Reaction(AfflictionKind.Unsteady, AfflictionKind.Sundered,
                "Unsteady meets Sundered: the wind tears its loose plates off and whirls them round it for 3 s, cutting all they pass, it too, 3 a shard.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Sundered, CountOnly = true },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Helper, Radius = 1.5f, Count = 7, Interval = 0.4f, Damage = 3f, DamagePerSpent = 3f, Stagger = 4f,
                    Affliction = AfflictionKind.Sundered }));

            // Ice and Light (48). Two mock suns hang by it for 4 s: a weapon blow on it lays two Chill and does not spend
            // the Mark (its blows are critical, as the Mark makes them).
            // NEEDS: the side of a blow: only a blow from its left or right, not its face, is critical, lays the Chill and
            // spares the Mark; and the suns themselves, turning with it.
            to.Add(Reaction(AfflictionKind.Chill, AfflictionKind.Marked,
                "Chill meets Marked: two mock suns hang beside it for 4 s. Your weapon blows on it lay two Chill and do not spend its Mark.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Suns, Seconds = 4f, Keeps = AfflictionKind.Marked } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Suns, By = StruckBy.Weapon, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 2 }));

            // Water and Dark (49). Swollen 5 s: 20% slower (a god is not slowed) and 15% more from weapon blows. When it
            // dies, or a heavy blow lands while it is swollen, it bursts in black water to 4 m: 14 damage, 20 stagger,
            // Soaked and Cursed on everything reached.
            // NEEDS: a finisher-only blow: the entry bursts it on a finisher, not on any heavy blow.
            UpgradeEffect[] BlackWater(UpgradeTrigger on) => new[]
            {
                new UpgradeEffect { On = on, Condition = Swollen, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 4f, Damage = 14f, Stagger = 20f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                new UpgradeEffect { On = on, Condition = Swollen, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 4f, Affliction = AfflictionKind.Cursed, Stacks = 1 },
            };
            var struck = BlackWater(UpgradeTrigger.ConditionStruck);
            var died = BlackWater(UpgradeTrigger.ConditionDied);
            to.Add(Reaction(AfflictionKind.Soaked, AfflictionKind.Cursed,
                "Soaked meets Cursed: the water turns black and it swells for 5 s, slower and open to your weapon. When it dies or a heavy blow lands it bursts, 14 with Soaked and Cursed to all within 4 m.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Swollen, Seconds = 5f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.15f, EndsWith = ConditionBlow.Heavy } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = SwollenHeavy, Seconds = 5f, Pace = 0.8f } },
                struck[0], struck[1], died[0], died[1]));

            // Life and Poison (50). Pale roots run from its feet to up to three other enemies within 5 m and hold 6 s; each
            // is Seeded once as they arrive, and every bite of poison on it bites each of them for half.
            // NEEDS: the roots seen on the floor (grey-white threads, a drip running along them with each bite).
            to.Add(Reaction(AfflictionKind.Seeded, AfflictionKind.Poison,
                "Seeded meets Poisoned: roots run from it to three enemies near and hold 6 s. Each is Seeded, and every bite of poison on it bites them too, for half.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, Radius = 5f, Count = 3, Affliction = AfflictionKind.Seeded, Stacks = 1,
                    State = new ConditionSpec { Id = Roots, Seconds = 6f, Flags = ConditionFlags.Linked, Scale = 0.5f, ToOthers = true } }));

            // Earth and Fire (51), rest 6 s. Three stones thrown up 1.5 m round it lie 6 s, red-hot, and burn what touches
            // them (3 damage and a Burn each half second).
            // NEEDS: things on the floor a weapon swing knocks along: a swing over a stone, or a heavy within 1.5 m, bats it
            // 7 m into the first enemy (14 damage, 20 stagger, a Burn and a Tremor) where it cracks into hot sherds (6 and
            // a Burn in 1.5 m).
            to.Add(Reaction(AfflictionKind.Tremor, AfflictionKind.Burn,
                "Tremor meets Burn: the shaken floor throws up three stones round it and its fire heats them red. For 6 s they burn what touches them.",
                6f,
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Shape = HazardShape.Ring, Count = 3, Reach = 1.5f, Radius = 0.5f, Seconds = 6f, Interval = 0.5f,
                    Damage = 3f, Affliction = AfflictionKind.Burn, Stacks = 1 }));

            // Metal and Lightning (52). For 6 s every weapon blow on it arcs only from iron to iron: to the nearest other
            // Sundered enemy within 8 m, never a clean one, for 35% of the blow, laying a notch where it lands; it leaps
            // once more from there (two at most; one arc every 0.3 s).
            to.Add(Reaction(AfflictionKind.Sundered, AfflictionKind.Static,
                "Sundered meets Static: for 6 s every blow of yours on it arcs to the nearest other notched enemy within 8 m, for a third of the blow, and notches it.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = IronLive, Seconds = 6f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = IronLive, By = StruckBy.Weapon, Cooldown = 0.3f, Do = UpgradeAction.Arc,
                    LeapsTo = AfflictionKind.Sundered, Count = 2, Reach = 8f, Amount = 0.35f, Affliction = AfflictionKind.Sundered, Stacks = 1 }));

            // Light and Wind (53). Three golden feathers ride 6 m downwind (away from the player), each Marking and laying
            // one Unsteady on what it touches. The first enemy keeps its Mark. Three bursts on the player's line beyond it
            // stand in for the feathers.
            // NEEDS: things thrown from an enemy: a fan of three feathers across 40 degrees, each stopping at the first it
            // touches; and the direction it was last shoved, which the feathers ride before the player's line.
            to.Add(Reaction(AfflictionKind.Marked, AfflictionKind.Unsteady,
                "Marked meets Unsteady: its mark breaks into three golden feathers that ride the wind 6 m away from you, Marking what they touch and making it Unsteady.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 3, Reach = 3.5f, Amount = 1.4f, Radius = 0.7f, Delay = 0.3f,
                    Affliction = AfflictionKind.Marked, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 3, Reach = 3.5f, Amount = 1.4f, Radius = 0.7f, Delay = 0.3f,
                    Affliction = AfflictionKind.Unsteady, Stacks = 1 }));

            // Dark and Ice (54), rest 6 s. While both last its Chill does not fade, and each weapon blow on it draws warmth
            // out: one Chill on it, and 2 health to the player (8 a second at most; a god gives half).
            // NEEDS: a freeze moment: when it freezes the last warmth is torn out (2 health for every Chill it carried) and
            // it stays frozen 1 s longer.
            to.Add(Reaction(AfflictionKind.Cursed, AfflictionKind.Chill,
                "Cursed meets Chill: the curse takes its warmth. While both last its Chill does not fade, and each of your weapon blows chills it and draws 2 health into you.",
                6f,
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Draw, Seconds = 30f, While = AfflictionKind.Cursed, While2 = AfflictionKind.Chill, Keeps = AfflictionKind.Chill } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Draw, By = StruckBy.Weapon, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Draw, By = StruckBy.Weapon, Ranks = EnemyRank.Plain | EnemyRank.Elite, Cooldown = 0.25f, CooldownPerEnemy = true, Do = UpgradeAction.Heal, Amount = 2f },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Draw, By = StruckBy.Weapon, Ranks = EnemyRank.God, Cooldown = 0.25f, CooldownPerEnemy = true, Do = UpgradeAction.Heal, Amount = 1f }));

            // Poison and Water (55), rest 6 s. The water runs off fouled into a slick 3 m across for 5 s: what stands in it
            // is Soaked. If it carries four doses or more as it reacts it floats up stunned: lifted and held 1.5 s (an
            // elite 0.5 s; a god takes 30 stagger instead), and the next weapon blow on it is critical and spends two
            // doses for 6 damage each.
            // NEEDS: ground that answers depth: any enemy in the slick that comes to four doses floats up too.
            to.Add(Reaction(AfflictionKind.Poison, AfflictionKind.Soaked,
                "Poisoned meets Soaked: the water runs off it fouled into a slick that soaks all in it. Dosed four deep it floats up stunned, and your next weapon blow on it is critical.",
                6f,
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Radius = 1.5f, Seconds = 5f, Interval = 0.5f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = AfflictionKind.Poison, NeedsAtLeast = 4, Ranks = EnemyRank.Plain, Do = UpgradeAction.Lift, Seconds = 1.5f },
                // An elite hangs 0.6 of a lift's time: 0.83 s asked is the entry's 0.5 s.
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = AfflictionKind.Poison, NeedsAtLeast = 4, Ranks = EnemyRank.Elite, Do = UpgradeAction.Lift, Seconds = 0.83f },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = AfflictionKind.Poison, NeedsAtLeast = 4, Ranks = EnemyRank.God, Do = UpgradeAction.Burst, Radius = 0.5f, Stagger = 30f,
                    Affliction = AfflictionKind.Poison },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = AfflictionKind.Poison, NeedsAtLeast = 4, Ranks = EnemyRank.Plain | EnemyRank.Elite, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Floating, Seconds = 1.5f, Flags = ConditionFlags.TakesCritical, EndsWith = ConditionBlow.Weapon } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Floating, By = StruckBy.Weapon, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Poison, Count = 2 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Floating, By = StruckBy.Weapon, Do = UpgradeAction.Burst, Radius = 0.5f, DamagePerSpent = 6f,
                    Affliction = AfflictionKind.Poison }));
        }
    }
}
