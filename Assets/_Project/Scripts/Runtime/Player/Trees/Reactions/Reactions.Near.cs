using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    // The pairs one and two apart on the ring (docs/design/steel/pairs_d1_d2.md), README numbers 1 to 22.
    public static partial class Reactions
    {
        // Conditions they set, by name: their answers name them too, and the probes read them.
        public const string LiveFlame = "flashover.live";
        public const string Thrown = "thundergust.thrown";
        public const string Crust = "rime.crust";
        public const string StillWater = "supercooled.still";
        public const string Flower = "quickening.flower";
        public const string Wedged = "splitrock.wedged";
        public const string Anvil = "anvilstone.anvil";
        public const string Disc = "diamondring.disc";
        public const string Jar = "shutinthedark.jar";
        public const string Charging = "sleetcharge.charging";
        public const string Capsized = "capsize.down";
        public const string Asleep = "winterseed.asleep";
        public const string SunkInSand = "groundgives.sunk";
        public const string Thorns = "ironthorns.thorns";
        public const string Chain = "blackchain.chain";
        public const string SapWaits = "burningsap.wait";
        public const string Sunk = "smoulder.sunk";
        public const string Nerves = "nervefire.nerves";

        private static void AddNear(List<Upgrade> to)
        {
            // ---- Distance 1.

            // Fire and Lightning (1). Flame carries current: the Static on it discharges through the fire at once, and
            // none of the Burn is spent. The discharge is Static's own set-off (7 a stack in 3 m) with 4 stagger a stack
            // (20 at full depth), and it flinches.
            // NEEDS: discharges that bend toward one enemy: for 4 s every discharge or arc within 6 m bends into the live
            // flame and bursts again (5 a Burn stack in 2.5 m, once every 0.8 s). The live flame is set as a condition so
            // that piece has something to find.
            to.Add(Reaction(AfflictionKind.Burn, AfflictionKind.Static,
                "Static meets Burn: the flame on it goes live and the Static discharges through it at once, 7 a stack in 3 m. The Burn is not spent.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 3f, Stagger = 4f },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Flinch, Seconds = 0.4f },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = LiveFlame, Seconds = 4f } }));

            // Lightning and Wind (2). Thrown 3 m straight away from the player; where it stops (0.4 s on) its Static goes
            // off as a heavy blow sets it off: 7 damage and 5 stagger a stack in 4 m. What cannot be shoved is not thrown
            // and discharges where it stands.
            // NEEDS: thrown into something: what the body hits on the way takes the discharge first.
            to.Add(Reaction(AfflictionKind.Static, AfflictionKind.Unsteady,
                "Static meets Unsteady: thunder and wind throw it 3 m away from you, and where it lands its Static goes off, 7 a stack in 4 m.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 3f },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Thrown, Seconds = 0.4f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Thrown, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Stagger = 5f }));

            // Wind and Ice (3). A crust of rime for 5 s; the next heavy blow breaks it and the ice blows out of its far
            // side: a cone 4 m long (two bursts on the player's line beyond it, reaching 0.7 to 4.5 m past it), 10 damage,
            // 2 Chill and 1 Unsteady to everything in it.
            // NEEDS: the side of a blow: only a heavy from the side facing the player when it reacted (within 90 degrees)
            // breaks the crust; one from behind wastes it.
            to.Add(Reaction(AfflictionKind.Unsteady, AfflictionKind.Chill,
                "Chill meets Unsteady: rime grows on it for 5 s. Your next heavy blow breaks it and the ice blows out of its far side, 10, 2 Chill and 1 Unsteady to all behind it.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Crust, Seconds = 5f, EndsWith = ConditionBlow.Heavy } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Crust, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 2,
                    Reach = 2.6f, Amount = 1.6f, Radius = 1.1f, Damage = 10f, Affliction = AfflictionKind.Chill, Stacks = 2 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Crust, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 2,
                    Reach = 2.6f, Amount = 1.6f, Radius = 1.1f, Affliction = AfflictionKind.Unsteady, Stacks = 1 }));

            // Ice and Water (4). Still below freezing for 6 s: the next weapon blow freezes it outright whatever its Chill
            // (Chill laid to full), an elite takes 3 Chill instead and a god 2, and the Soaked is spent.
            to.Add(Reaction(AfflictionKind.Chill, AfflictionKind.Soaked,
                "Chill meets Soaked: the water on it goes still for 6 s. Your next weapon blow freezes it outright and spends the water.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = StillWater, Seconds = 6f, EndsWith = ConditionBlow.Weapon } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = StillWater, By = StruckBy.Weapon, Ranks = EnemyRank.Plain, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = ToFull },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = StillWater, By = StruckBy.Weapon, Ranks = EnemyRank.Elite, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 3 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = StillWater, By = StruckBy.Weapon, Ranks = EnemyRank.God, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 2 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = StillWater, By = StruckBy.Weapon, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Soaked }));

            // Water and Life (5). The seed splits its coat: Seeded to full depth, and a flower opens for 4 s. A heavy blow
            // bursts it: the player heals 10, and everything within 3 m is Soaked and Seeded. Left alone it withers.
            to.Add(Reaction(AfflictionKind.Soaked, AfflictionKind.Seeded,
                "Soaked meets Seeded: the seed flowers at once, to full. A heavy blow within 4 s bursts the flower: you heal 10 and all within 3 m are Soaked and Seeded.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = ToFull },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Flower, Seconds = 4f, EndsWith = ConditionBlow.Heavy } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Flower, By = StruckBy.Heavy, Do = UpgradeAction.Heal, Amount = 10f },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Flower, By = StruckBy.Heavy, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Flower, By = StruckBy.Heavy, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Seeded, Stacks = 1 }));

            // Life and Earth (6). Roots wedge into the shaken ground: held 2 s (an elite 0.8 s, a god by GodHolds), and a
            // heavy blow while it is held splits the floor round it: 10 damage, Tremor 1 and Seeded in 3 m.
            to.Add(Reaction(AfflictionKind.Seeded, AfflictionKind.Tremor,
                "Seeded meets Tremor: roots wedge it into the ground and hold it 2 s. A heavy blow while it is held splits the floor, 10, Tremor and Seeded to all within 3 m.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hold, Seconds = 2f },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.Plain, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Wedged, Seconds = 2f, EndsWith = ConditionBlow.Heavy } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.Elite, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Wedged, Seconds = 0.8f, EndsWith = ConditionBlow.Heavy } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Wedged, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 3f, Damage = 10f, Affliction = AfflictionKind.Tremor, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Wedged, By = StruckBy.Heavy, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Seeded, Stacks = 1 }));

            // Earth and Metal (7). An anvil at its back for 5 s; the next heavy blow drives it on: Sundered to full, and a
            // ring of sparks in 3 m: 10 damage, 15 stagger and a notch on each.
            // NEEDS: standing things that block: the iron-faced stone itself, which also counts as "stone at its back"
            // for Hammer and Anvil.
            to.Add(Reaction(AfflictionKind.Tremor, AfflictionKind.Sundered,
                "Tremor meets Sundered: an anvil heaves up at its back for 5 s. Your next heavy blow drives it on: its armour breaks to full and sparks ring out, 10 and a notch to all within 3 m.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Anvil, Seconds = 5f, EndsWith = ConditionBlow.Heavy } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Anvil, By = StruckBy.Heavy, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Sundered, Stacks = ToFull },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Anvil, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 3f, Damage = 10f, Stagger = 15f,
                    Affliction = AfflictionKind.Sundered, Stacks = 1 }));

            // Metal and Light (8). A ray from it to the nearest other enemy within 6 m (never back into it): 10 damage,
            // 15 stagger, Marked and a notch.
            to.Add(Reaction(AfflictionKind.Sundered, AfflictionKind.Marked,
                "Marked meets Sundered: the light catches in its broken armour and is thrown at the nearest enemy within 6 m, 10, Marked and a notch.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Arc, FromTarget = true, Count = 1, Reach = 6f, Damage = 10f, Stagger = 15f,
                    Affliction = AfflictionKind.Marked, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Pass, Reach = 6f, Affliction = AfflictionKind.Sundered, Stacks = 1 }));

            // Light and Dark (9). Dark closes over it in 0.5 s and a bead of light breaks at the rim: 16 damage, 30
            // stagger; and for 4 s its Mark is not spent by blows.
            to.Add(Reaction(AfflictionKind.Marked, AfflictionKind.Cursed,
                "Marked meets Cursed: dark closes over it and a bead of light breaks at the rim, 16 and 30 stagger. For 4 s its Mark is not spent by blows.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 0.5f, Delay = 0.5f, Damage = 16f, Stagger = 30f, Affliction = AfflictionKind.Marked },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Disc, Seconds = 4f, Keeps = AfflictionKind.Marked } }));

            // Dark and Poison (10), rest 8 s. Sealed as a jar for 8 s: its doses stop fading. When the 8 s end, or it
            // dies, the jar breaks: 5 a dose in 3 m, and half its doses (rounded up) laid on all it reaches.
            // NEEDS: the jar's ring on the floor (4 m across); other poisoned enemies that die inside it crawling to it
            // and giving it every dose (to twelve, past the usual eight); each kill inside feeding the player 4.
            to.Add(Reaction(AfflictionKind.Cursed, AfflictionKind.Poison,
                "Cursed meets Poisoned: it is sealed as a jar for 8 s and its doses stop fading. When the time ends or it dies the jar breaks, 5 a dose in 3 m, and half its doses spill on all it reaches.",
                new ReactionDef { Rest = 8f },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Jar, Seconds = 8f, Keeps = AfflictionKind.Poison } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Jar, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Poison, CountOnly = true },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Jar, Do = UpgradeAction.Burst, Radius = 3f, DamagePerSpent = 5f, Affliction = AfflictionKind.Poison },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Jar, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Poison, Stacks = 1, PerStack = true, Share = 0.5f },
                new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = Jar, Needs = AfflictionKind.Poison, Do = UpgradeAction.Burst, Radius = 3f, Damage = 5f, PerStack = true,
                    Affliction = AfflictionKind.Poison },
                new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = Jar, Needs = AfflictionKind.Poison, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Poison,
                    Stacks = 1, PerStack = true, Share = 0.5f }));

            // Poison and Fire (11). A fuse of 0.6 s, then it bangs: 4 a dose and 6 a Burn stack in 2.5 m, 30 stagger, 1
            // Burn on all reached; half its doses (rounded up) are spent. Counted as it reacts, struck as it bangs.
            to.Add(Reaction(AfflictionKind.Poison, AfflictionKind.Burn,
                "Burn meets Poisoned: the poison catches like powder and bangs, 4 a dose and 6 a Burn stack to all within 2.5 m, and lights them. Half its doses are spent.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Poison, CountOnly = true },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 2.5f, Delay = 0.6f, DamagePerSpent = 4f, Stagger = 30f, Affliction = AfflictionKind.Burn, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Burn, CountOnly = true },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 2.5f, Delay = 0.6f, DamagePerSpent = 6f, Affliction = AfflictionKind.Burn },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Poison, Share = 0.5f }));

            // ---- Distance 2.

            // Fire and Wind (12). The wind strips three embers off it: they fly 5 m downwind (away from the player, along
            // the line to it) and land 1 m apart as small fires (1 m, 3 s, 3 a bite, 1 Burn); whatever stands within
            // 1.5 m of a landing is set alight as they land. The player aims the fire by where they stand.
            to.Add(Reaction(AfflictionKind.Burn, AfflictionKind.Unsteady,
                "Burn meets Unsteady: the wind strips three embers off it and carries them 5 m on, away from you. Where they land small fires burn, and lights what stands near.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Shape = HazardShape.Downwind, Count = 3, Reach = 5f, Amount = 1f,
                    Radius = 1f, Seconds = 3f, Interval = 0.5f, Damage = 3f, Affliction = AfflictionKind.Burn, Stacks = 1, Delay = 0.35f },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 3, Reach = 5f, Amount = 1f,
                    Radius = 1.5f, Damage = 0f, Affliction = AfflictionKind.Burn, Stacks = 1, Delay = 0.35f }));

            // Lightning and Ice (13). For 3 s it gains one Static every 0.5 s (to full); at the end a bolt discharges all
            // of it (7 damage and 5 stagger a stack in 4 m) and everything the discharge reaches takes 1 Chill. A heavy
            // blow discharges it early as Lightning always does.
            // NEEDS: a freeze moment: "at once if it freezes".
            to.Add(Reaction(AfflictionKind.Static, AfflictionKind.Chill,
                "Static meets Chill: the frost charges itself, a Static every half second for 3 s. Then a bolt falls and discharges it all, 7 a stack in 4 m, chilling all it reaches.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Charging, Seconds = 3f, Beat = 0.5f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Charging, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Charging, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Stagger = 5f },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Charging, Do = UpgradeAction.Afflict, Radius = 4f, Affliction = AfflictionKind.Chill, Stacks = 1 }));

            // Wind and Water (14). It slips and goes down on its back for 1.2 s; an elite only staggers (25), a god takes 15
            // stagger.
            to.Add(Reaction(AfflictionKind.Unsteady, AfflictionKind.Soaked,
                "Soaked meets Unsteady: it slips and goes down for 1.2 s. The great only stagger.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.Plain, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Capsized, Seconds = 1.2f, Flags = ConditionFlags.Down } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.Elite, Do = UpgradeAction.Burst, Radius = 0.5f, Stagger = 25f },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.God, Do = UpgradeAction.Burst, Radius = 0.5f, Stagger = 15f }));

            // Ice and Life (15). The seed sleeps under the frost: its Seeded stops fading while the Chill lasts. When the
            // Chill ends (a freeze ending by thaw or shatter ends it too) the seed wakes: thorns of 12 a Seeded stack in
            // 2.5 m (36 at most), seeding what they reach, and the player heals 3 a stack.
            to.Add(Reaction(AfflictionKind.Chill, AfflictionKind.Seeded,
                "Chill meets Seeded: the seed sleeps under the frost and stops fading. When the cold ends it wakes as thorns, 12 a seed in 2.5 m, and you heal 3 a seed.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Asleep, Seconds = 30f, While = AfflictionKind.Chill, Keeps = AfflictionKind.Seeded } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Asleep, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Seeded, CountOnly = true },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Asleep, Do = UpgradeAction.Burst, Radius = 2.5f, DamagePerSpent = 12f, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Asleep, Do = UpgradeAction.Heal, Amount = 3f, PerStack = true }));

            // Water and Earth (16). Shaken wet ground turns to sand: it sinks to the knees for 4 s, slowed by half (an
            // elite by a quarter; a god takes only the Tremor), and a heavy blow on it while sunk holds it 0.5 s.
            // NEEDS: sinking (no dodge, leap or step back, the body drawn lower) and the pool itself (2 m across, slowing
            // other enemies that wade in by 30%).
            to.Add(Reaction(AfflictionKind.Soaked, AfflictionKind.Tremor,
                "Tremor meets Soaked: the wet floor under it turns to sand and it sinks to the knees for 4 s, slowed by half. A heavy blow holds it fast.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = SunkInSand, Seconds = 4f, Pace = 0.5f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = SunkInSand, By = StruckBy.Heavy, Do = UpgradeAction.Hold, Seconds = 0.5f }));

            // Life and Metal (17). Iron thorns grow out of it for 6 s; each weapon blow snaps one off and it flies at the
            // nearest other enemy within 6 m: 6 damage, Seeded and a notch. Three thorns.
            // NEEDS: a count set from depth: one thorn for each notch it carried (three at most); three are built.
            to.Add(Reaction(AfflictionKind.Seeded, AfflictionKind.Sundered,
                "Seeded meets Sundered: iron thorns grow through its armour for 6 s. Each of your weapon blows snaps one off at the nearest enemy within 6 m, 6, Seeded and a notch.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Thorns, Seconds = 6f, Blows = 3 } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Thorns, By = StruckBy.Weapon, Do = UpgradeAction.Arc, FromTarget = true, Count = 1, Reach = 6f, Damage = 6f,
                    Affliction = AfflictionKind.Seeded, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Thorns, By = StruckBy.Weapon, Do = UpgradeAction.Pass, Reach = 6f, Affliction = AfflictionKind.Sundered, Stacks = 1 }));

            // Earth and Light (18). 0.5 s after it cracks, light breaks up through the floor: 14 damage, 25 stagger and
            // Marked in 3 m; its own Mark is renewed (three more blows).
            // NEEDS: the seams: only enemies standing on the star of cracks, not a disc, are struck.
            to.Add(Reaction(AfflictionKind.Tremor, AfflictionKind.Marked,
                "Tremor meets Marked: the floor under it cracks and light breaks up through the seams, 14 and Marked to all within 3 m. Its own Mark is renewed.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 3f, Delay = 0.5f, Damage = 14f, Stagger = 25f, Affliction = AfflictionKind.Marked, Stacks = 1 }));

            // Metal and Dark (19). A chain to the nearest other enemy within 6 m for 5 s, set as a condition on that one,
            // joined to this.
            // NEEDS: links that pass blows: 30% of weapon damage dealt to either is dealt to the other (gods too); and the
            // leash: the two cannot move more than 4 m apart (gods never leashed); with no one near, chained to the floor
            // within 2 m of the spot. The link is not the poison-passing kind, so Rot Roots' bites do not run along it.
            to.Add(Reaction(AfflictionKind.Sundered, AfflictionKind.Cursed,
                "Cursed meets Sundered: a black chain binds it to the nearest enemy within 6 m for 5 s.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, Radius = 6f, Count = 1,
                    State = new ConditionSpec { Id = Chain, Seconds = 5f, Scale = 0.3f, ToOthers = true } }));

            // Light and Poison (20), rest 6 s. The sun wakes the sap for 3 s; each weapon blow in that time (each is
            // critical while the Mark lasts) raises a weal. At the end the poison bites at once, and each weal bursts,
            // two doses on every enemy within 2.5 m.
            // NEEDS: poison held back: for the 3 s it stops biting, and at the end every bite it held back lands at once,
            // doubled.
            to.Add(Reaction(AfflictionKind.Marked, AfflictionKind.Poison,
                "Marked meets Poisoned: the sun wakes the sap for 3 s. Each of your blows on it raises a weal; then the poison bites and each weal bursts, two doses to all within 2.5 m.",
                new ReactionDef { Rest = 6f },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = SapWaits, Seconds = 3f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = SapWaits, Do = UpgradeAction.Bite },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = SapWaits, SpentAtLeast = 1, Do = UpgradeAction.Afflict, Radius = 2.5f, Affliction = AfflictionKind.Poison,
                    Stacks = 2, PerStack = true }));

            // Dark and Fire (21). The fire sinks in while the curse lasts: its Burn goes on biting, stops fading, and
            // deepens by one every 2 s (a god's every 3 s) to full. When the curse ends or it dies it flares: every Burn
            // stack set off at once, 10 a stack in 3 m, and what is reached is set alight.
            // NEEDS: the hidden flame: its Burn shown as a dark red glow in cracks, black smoke, until the flare.
            to.Add(Reaction(AfflictionKind.Cursed, AfflictionKind.Burn,
                "Cursed meets Burn: the fire sinks in out of sight and deepens every 2 s while the curse lasts. When the curse ends or it dies it flares, 10 a stack in 3 m.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.Plain | EnemyRank.Elite, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Sunk, Seconds = 30f, Beat = 2f, While = AfflictionKind.Cursed, Keeps = AfflictionKind.Burn } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.God, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Sunk, Seconds = 30f, Beat = 3f, While = AfflictionKind.Cursed, Keeps = AfflictionKind.Burn } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Sunk, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Sunk, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Burn },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Sunk, Needs = AfflictionKind.Burn, Do = UpgradeAction.Burst, Radius = 3f, Damage = 10f, PerStack = true,
                    Affliction = AfflictionKind.Burn },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Sunk, Needs = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Burn, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = Sunk, Needs = AfflictionKind.Burn, Do = UpgradeAction.Burst, Radius = 3f, Damage = 10f, PerStack = true,
                    Affliction = AfflictionKind.Burn },
                new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = Sunk, Needs = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Burn, Stacks = 1 }));

            // Poison and Lightning (22). While both last, each bite of poison spends one Static and jolts it: 7 damage, 5
            // stagger and a 0.3 s flinch that breaks a wind-up. An elite flinches once every 4 s; a god never flinches and
            // takes 6 more stagger instead.
            // NEEDS: per-enemy cooldowns: the elites' 4 s is one clock shared by every elite.
            to.Add(Reaction(AfflictionKind.Poison, AfflictionKind.Static,
                "Poisoned meets Static: while both last, each bite of poison spends a Static and jolts it, 7 and a flinch that breaks its wind-up.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = Nerves, Seconds = 30f, OnBite = true, While = AfflictionKind.Poison, While2 = AfflictionKind.Static } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Nerves, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Static, Count = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Nerves, Ranks = EnemyRank.Plain | EnemyRank.Elite, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 7f, Stagger = 5f,
                    Affliction = AfflictionKind.Static, Unseen = true },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Nerves, Ranks = EnemyRank.God, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 7f, Stagger = 11f,
                    Affliction = AfflictionKind.Static, Unseen = true },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Nerves, Ranks = EnemyRank.Plain, Do = UpgradeAction.Flinch, Seconds = 0.3f },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Nerves, Ranks = EnemyRank.Elite, Cooldown = 4f, Do = UpgradeAction.Flinch, Seconds = 0.3f }));
        }
    }
}
