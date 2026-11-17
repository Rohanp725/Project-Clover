using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Blizzard: the branch Wind and Ice share. Cold that moves: an enemy both chilled and Unsteady freezes without
    /// help, and a Frozen enemy is something to be thrown. Its crown is given by consuming the neighbour's god.
    /// </summary>
    public static class BlizzardTree
    {
        public const string BitingWind = "blizzard.biting";
        public const string Windchill = "blizzard.windchill";
        public const string ColdFront = "blizzard.front";
        public const string SlidingIce = "blizzard.sliding";
        public const string Whiteout = "blizzard.whiteout";
        public const string Avalanche = "blizzard.avalanche";
        public const string Gift = "blizzard.dust";

        public static void Add(List<Upgrade> to)
        {
            // The first element is the one whose mix this is: the earlier of the two round the ring.
            const string A = Elements.Wind, B = Elements.Ice;

            // With the held element's own root learned as well, that element's affliction is laid twice by each blow.
            to.Add(new Upgrade { Id = BitingWind, Name = "Biting Wind", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, X = 0f, Y = 1f, Icon = UpgradeIcons.Gust,
                Description = "Your blows lay Unsteady and Chill together.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 },
                } });
            // NEAREST: only the enemy faced grows colder, once every two seconds.
            to.Add(new Upgrade { Id = Windchill, Name = "Windchill", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { BitingWind }, X = -0.5f, Y = 2.36f, Icon = UpgradeIcons.Snowflake,
                Description = "The enemy you face, if it is both Unsteady and chilled, grows colder by itself.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 2f, Needs = AfflictionKind.Unsteady, Needs2 = AfflictionKind.Chill, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 } } });
            // A perfect dodge names no attacker: the cone goes at the enemy faced, and the push and the Unsteady go out round the player.
            to.Add(new Upgrade { Id = ColdFront, Name = "Cold Front", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { BitingWind }, X = 0.5f, Y = 2.36f, Icon = UpgradeIcons.Boot,
                Description = "A perfect dodge leaves a gust of ice behind you. It throws the attacker back, two deeper in Chill and Unsteady.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Cone, Damage = 10f, Stagger = 20f, Reach = 4f, Arc = 90f, Affliction = AfflictionKind.Chill, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Afflict, At = EffectAt.Self, Radius = 4f, Affliction = AfflictionKind.Unsteady, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Push, At = EffectAt.Self, Radius = 4f, Amount = 3f },
                } });
            // BESPOKE (small): a light blow sends a Frozen enemy sliding until it meets a wall or another enemy, and it
            // shatters there. Until then: a light blow on a chilled enemy throws back those near and a burst follows.
            to.Add(new Upgrade { Id = SlidingIce, Name = "Sliding Ice", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { Windchill }, X = 0f, Y = 3.48f, Icon = UpgradeIcons.Arrow,
                Description = "A light blow on a chilled enemy throws everything close to you back, and half a second later ice bursts round the one struck.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.LightHit, Do = UpgradeAction.Push, Needs = AfflictionKind.Chill, At = EffectAt.Self, Radius = 3.5f, Amount = 5f, Cooldown = 1.5f },
                    new UpgradeEffect { On = UpgradeTrigger.LightHit, Do = UpgradeAction.Burst, Needs = AfflictionKind.Chill, Damage = 20f, Stagger = 20f, Radius = 2.5f, Delay = 0.5f, Cooldown = 1.5f },
                } });
            // The rain form gives the snow and its bite. Losing sight of the player, the slow turn and blows from behind being critical are not in it yet.
            to.Add(new Upgrade { Id = Whiteout, Name = "Whiteout", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { Windchill }, X = -0.5f, Y = 4.6f, Icon = UpgradeIcons.Spiral,
                Description = "Hold the spell button to call down driving snow in a wide circle for eight seconds. Shards fall inside it again and again, each a small bite. 40 mana.",
                Form = "rain",
                Spell = new SpellSpec { Cost = 40f, Reach = 0.5f, Radius = 7f, Seconds = 8f, Interval = 0.6f, Damage = 3f, Stagger = 4f, Stacks = 1 } });
            // The dragon form carries what it meets along its line, as the rolling snow does; it does not bury them Frozen where it stops.
            to.Add(new Upgrade { Id = Avalanche, Name = "Avalanche", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { SlidingIce }, X = 0.5f, Y = 4.6f, Icon = UpgradeIcons.Wave,
                Description = "Hold the spell button to send a great head of snow down a lane nine metres long. It strikes what it passes and carries it along, then bites hard where it stops. 50 mana.",
                Form = "dragon",
                Spell = new SpellSpec { Cost = 50f, Reach = 9f, Radius = 2f, Speed = 4.5f, Damage = 25f, Stagger = 30f, Stacks = 4 } });

            // BESPOKE (built): DustForm.
            to.Add(new Upgrade { Id = Gift, Name = "Diamond Dust", Element = A, Element2 = B, Kind = UpgradeKind.Signature, X = 0f, Y = 5.8f, Icon = UpgradeIcons.Crown,
                Description = "Shards from a shattered enemy do not fall. They hang round you, four from each, up to twelve, and your next dodge throws them down its path.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.Form, Form = SpellForms.Dust, Affliction = AfflictionKind.Chill, Spell = new SpellSpec { Cost = 0f, Count = 4, Damage = 6f, Stagger = 4f, Reach = 9f, Stacks = 1 } } } });
        }
    }
}
