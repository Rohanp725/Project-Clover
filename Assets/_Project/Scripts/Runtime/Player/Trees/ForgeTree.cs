using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Forge: the duo branch Earth and Metal share, reshaped by pairs_d1_d2.md ("Earth and Metal", task 4.15) and drawn in
    /// the chart's heart (<see cref="HeartPattern"/>). Its reaction is Anvil Stone; a new Shape mark opens its two shapes,
    /// Ringing Iron (the old plain reaction) and Hammer Scale. Trip Hammer, offered to Earth's or Metal's tree, whose rosters
    /// are full, is kept hidden for the Evolutions (<see cref="SpareSpells"/>). Kept: Hammer and Anvil (the reaction's stone feeds it), Struck Ore,
    /// Buckled (the standing rule while both are on), Folded Steel (its folds now a fight's growth, kept to the next rest
    /// fire), Drawn From Stone (the duo spell) and World Anvil, begun by a finisher, the branch's top mark.
    /// </summary>
    public static class ForgeTree
    {
        public const string Shape = "forge.shape";
        public const string HammerAndAnvil = "forge.hammeranvil";
        public const string StruckOre = "forge.struckore";
        public const string Buckled = "forge.buckled";
        public const string DrawnFromStone = "forge.drawnfromstone";
        public const string FoldedSteel = "forge.foldedsteel";
        public const string WorldAnvil = "forge.worldanvil";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Earth, B = Elements.Metal;
            const AfflictionKind Tremor = AfflictionKind.Tremor, Notch = AfflictionKind.Sundered;
            // Laid as stacks, more than any affliction holds: "to full depth", whatever the depth is.
            const int ToFull = 99;

            // NEEDS: flakes thrown as pieces at the four nearest enemies (Hammer Scale; now an arc from it through four).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Anvil Stone: Ringing Iron or Hammer Scale", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Anvil Stone's two shapes, chosen at a rest fire. Ringing Iron: no stone, the shaken armour rings at once. Hammer Scale: no ring, flakes of glowing scale fly from it into the enemies near. Either runs instead of the plain Anvil Stone.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Ringing Iron",
                        Description = "No stone: the shaken armour rings like struck iron at once, its Sundered to full and a ring of sparks out 3 m, 10, 15 stagger and a notch on each enemy it reaches.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Afflict, Affliction = Notch, Stacks = ToFull },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 3f, Damage = 10f, Stagger = 15f, Affliction = Notch, Stacks = 1 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Hammer Scale",
                        Description = "No ring: flakes of glowing scale fly from it at the nearest enemies within 6 m, up to four, 8, a notch and a Tremor each.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Arc, FromTarget = true, Count = 4, Reach = 6f, Damage = 8f, Affliction = Notch, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Arc, FromTarget = true, Count = 4, Reach = 6f, Affliction = Tremor, Stacks = 1 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: a condition "the target stands within 1.5 m of a wall or pillar" (or Anvil Stone's stone). Here the second
            // strike comes on an enemy already shaken.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = HammerAndAnvil, Name = "Hammer and Anvil", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Hammer,
                Description = "A heavy blow on an enemy with stone at its back strikes twice: once from your weapon, once from the stone. Heavy blows lay Tremor and Sundered together.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = Tremor, Do = UpgradeAction.Echo, Amount = 0.6f, Delay = 0.15f, Radius = 2.3f, Stagger = 20f, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = Tremor, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = Notch, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: a pillar that stands and can be broken by a heavy blow. Here the spike strikes and breaks into its three blades at once.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = StruckOre, Name = "Struck Ore", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Spike,
                Description = "A perfect guard strikes a spike of ore from the ground under the attacker and shakes it, and three blades fly from you that notch what they pass.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Falling, Radius = 1.2f, Delay = 0.2f, Damage = 10f, Stagger = 20f, Affliction = Tremor, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Projectile, Flight = Flight.Straight, Count = 3, Damage = 9f, Stagger = 4f, Affliction = Notch, Stacks = 1 },
                },
            }, HeartPattern.RightLow));

            // Built (Afflictions.BuckledShare): an enemy carrying Tremor and Sundered together takes 40% more stagger from every source.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Buckled, Name = "Buckled", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { StruckOre }, Icon = UpgradeIcons.Anvil,
                Description = "An enemy both shaken and notched loses its balance far sooner.",
                Stats = new StatMod[0],
            }, HeartPattern.RightHigh));

            // Re-aimed (steel/README.md, settled): the folds are a fight's growth, not the weapon's, so they last only to the
            // next rest fire. NEEDS: a count that empties at a rest fire (now it is kept while the mark is in force and lost
            // with it, not kept for the run).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = FoldedSteel, Name = "Folded Steel", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { Buckled }, Icon = UpgradeIcons.Blade,
                Description = "Every finisher folds your blade once more, up to ten, until your next rest fire. Each fold adds to every blow. Each fold makes it heavier in the hand.",
                Tally = new TallySpec { Name = "Folds", Cap = 10, Stat = PlayerStat.WeaponDamage, PerPoint = 0.03f, Stat2 = PlayerStat.AttackStamina, PerPoint2 = 0.01f, Kept = false },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Bank, Count = 1 } },
            }, HeartPattern.Keystone));

            // The duo spell. BESPOKE: a held pose draws a sword of stone and iron twice the player's height; for three swings
            // (or 10 s) the weapon is replaced: each a heavy blow over 180 degrees to 4 m for 40 damage, 60 stagger, Tremor 1
            // and a notch, through any guard; the third throws shards 6 m. Until then: one such swing, as a wave.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DrawnFromStone, Name = "Drawn From Stone", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Breaker, Requires = new[] { HammerAndAnvil, StruckOre }, Icon = UpgradeIcons.Blade,
                Description = "You draw a sword of stone and iron from the ground and give one great swing, a half circle before you. 50 mana.",
                Form = SpellForms.Wave,
                Spell = new SpellSpec { Cost = 50f, Reach = 4f, Arc = 180f, Damage = 40f, Stagger = 60f, Stacks = 1 },
            }, HeartPattern.Spell));

            // BESPOKE (built): AnvilForm. The branch's top mark, begun by a finisher.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = WorldAnvil, Name = "World Anvil", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { DrawnFromStone }, Icon = UpgradeIcons.Crown,
                Description = "Your finisher is struck on an anvil that rises out of the ground. Its ring breaks the armour of everyone near and shakes them, and a blade is forged for each one it reaches, up to six. They fly with your next heavy blow.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Form, Form = SpellForms.Anvil, Affliction = AfflictionKind.Tremor, Spell = new SpellSpec { Cost = 0f, Radius = 6f, Damage = 15f, Stagger = 30f, Stacks = 3, Count = 6, LingerDamage = 10f } } },
            }, HeartPattern.Crown));
        }
    }
}
