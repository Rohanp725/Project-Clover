using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Forge: the branch Earth and Metal share. The battlefield is a smithy: stone is the anvil, the weapon is the
    /// hammer, and the enemy lies between. Blows are few, heavy and timed. Its crown, World Anvil, is given by a
    /// god and never sold.
    /// </summary>
    public static class ForgeTree
    {
        public const string HammerAndAnvil = "forge.hammeranvil";
        public const string StruckOre = "forge.struckore";
        public const string Buckled = "forge.buckled";
        public const string TripHammer = "forge.triphammer";
        public const string DrawnFromStone = "forge.drawnfromstone";
        public const string FoldedSteel = "forge.foldedsteel";
        public const string WorldAnvil = "forge.worldanvil";

        public static void Add(List<Upgrade> to)
        {
            // The first element is the one whose mix this is: the earlier of the two round the ring.
            const string A = Elements.Earth, B = Elements.Metal;
            const AfflictionKind Tremor = AfflictionKind.Tremor, Notch = AfflictionKind.Sundered;

            // The foot of the branch: it grows from no mark (the design's "earth.tremor or metal.sunder" cannot be written, a mix may only grow from its own branch).
            // NEEDS: a condition "the target stands within 1.5 m of a wall or pillar". Here the second strike comes on an enemy already shaken.
            to.Add(new Upgrade { Id = HammerAndAnvil, Name = "Hammer and Anvil", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, X = 0f, Y = 1f, Icon = UpgradeIcons.Hammer,
                Description = "A heavy blow on an enemy with stone at its back strikes twice: once from your weapon, once from the stone.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = Tremor, Do = UpgradeAction.Echo, Amount = 0.6f, Delay = 0.15f, Radius = 2.3f, Stagger = 20f, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = Tremor, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = Notch, Stacks = 1 },
                } });
            // NEEDS: a pillar that stands and can be broken by a heavy blow. Here the spike strikes and breaks into its three blades at once.
            to.Add(new Upgrade { Id = StruckOre, Name = "Struck Ore", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { HammerAndAnvil }, X = -0.5f, Y = 2.36f, Icon = UpgradeIcons.Spike,
                Description = "A perfect guard strikes a spike of ore from the ground under the attacker. It stands. A heavy blow breaks it into blades.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Falling, Radius = 1.2f, Delay = 0.2f, Damage = 10f, Stagger = 20f, Affliction = Tremor, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Projectile, Flight = Flight.Straight, Count = 3, Damage = 9f, Stagger = 4f, Affliction = Notch, Stacks = 1 },
                } });
            // NEEDS: a condition on two afflictions at once, and stagger taken by an enemy from every source. Here blows on a shaken enemy raise the weapon's stagger for a while.
            to.Add(new Upgrade { Id = Buckled, Name = "Buckled", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { StruckOre }, X = 0f, Y = 3.48f, Icon = UpgradeIcons.Anvil,
                Description = "An enemy both shaken and notched loses its balance far sooner.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictedHit, Needs = Tremor, Do = UpgradeAction.Boon, Stat = PlayerStat.StaggerDealt, Amount = 0.4f, Seconds = 3f } } });
            // The sword form is the one great thing that falls after a delay. NEEDS: the timing check that doubles the hammer and a heavy blow landing together.
            to.Add(new Upgrade { Id = TripHammer, Name = "Trip Hammer", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { StruckOre }, X = -0.5f, Y = 4.6f, Icon = UpgradeIcons.Hammer,
                Description = "Hold the spell button and a great hammer forms above the enemy you face, and falls. Land a heavy blow of your own as it lands and both strike twice as hard. 40 mana.",
                Form = "sword",
                Spell = new SpellSpec { Cost = 40f, Reach = 10f, Delay = 1f, Radius = 2.5f, Damage = 30f, Stagger = 50f, Stacks = 1, Seconds = 1f, LingerDamage = 2f } });
            // BESPOKE: a held pose draws a sword of stone and iron twice the player's height; for three swings (or 10 s) the weapon is replaced: each a heavy blow over 180 degrees
            // to 4 m for 40 damage, 60 stagger, Tremor 1 and a notch, through any guard; the third throws shards 6 m. Until then: one such swing, as a wave.
            to.Add(new Upgrade { Id = DrawnFromStone, Name = "Drawn From Stone", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { TripHammer }, X = 0f, Y = 5.72f, Icon = UpgradeIcons.Blade,
                Description = "Hold the spell button to thrust your hand into the ground and draw out a sword of stone and iron far too large for you. It gives three great swings. The third breaks it. 50 mana.",
                Form = SpellForms.Wave,
                Spell = new SpellSpec { Cost = 50f, Reach = 4f, Arc = 180f, Damage = 40f, Stagger = 60f, Stacks = 1 } });
            to.Add(new Upgrade { Id = FoldedSteel, Name = "Folded Steel", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { Buckled }, X = 0.5f, Y = 4.6f, Icon = UpgradeIcons.Blade,
                Description = "Every finisher folds your blade once more, up to ten. Each fold adds to every blow for the rest of the run. Each fold makes it heavier in the hand.",
                Tally = new TallySpec { Name = "Folds", Cap = 10, Stat = PlayerStat.WeaponDamage, PerPoint = 0.03f, Stat2 = PlayerStat.AttackStamina, PerPoint2 = 0.01f, Kept = true }, Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Bank, Count = 1 } } });

            // BESPOKE (built): AnvilForm.
            to.Add(new Upgrade { Id = WorldAnvil, Name = "World Anvil", Element = A, Element2 = B, Kind = UpgradeKind.Signature, X = 0f, Y = 7.96f, Icon = UpgradeIcons.Crown,
                Description = "Your finisher is struck on an anvil that rises out of the ground. Its ring breaks the armour of everyone near and shakes them, and a blade is forged for each one it reaches, up to six. They fly with your next heavy blow.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Form, Form = SpellForms.Anvil, Affliction = AfflictionKind.Tremor, Spell = new SpellSpec { Cost = 0f, Radius = 6f, Damage = 15f, Stagger = 30f, Stacks = 3, Count = 6, LingerDamage = 10f } } } });
        }
    }
}
