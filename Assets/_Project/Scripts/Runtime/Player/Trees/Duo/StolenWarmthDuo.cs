using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Dark and Ice (pairs_d5.md, "Dark and Ice", distance 5; task 4.17), named by its reaction, Stolen
    /// Warmth. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark, Let
    /// the Cold In, opens Cold Shade and The Long Night; Hel's Half (a major), Deep Veins (a minor), the keystone No Summer,
    /// the duo spell Ice in the Blood and the duo surge Three Winters. Dark is written first, as the ring names the pair.
    /// </summary>
    public static class StolenWarmthDuo
    {
        public const string Shape = "stolenwarmth.shape";
        public const string HelsHalf = "stolenwarmth.helshalf";
        public const string DeepVeins = "stolenwarmth.deepveins";
        public const string NoSummer = "stolenwarmth.nosummer";
        public const string IceInTheBlood = "stolenwarmth.iceinblood";
        public const string ThreeWinters = "stolenwarmth.threewinters";
        /// <summary>The conditions the two shapes set: Cold Shade's draw (as the plain reaction's, its shade left when it
        /// dies) and The Long Night's warmth given to the dark.</summary>
        public const string Shade = "stolenwarmth.shade";
        public const string LongNight = "stolenwarmth.longnight";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Dark, B = Elements.Ice;

            // NEEDS: freeze-moment (Cold Shade's shade left when it is shattered; now when it dies with its warmth stolen) and
            // standing-things (the shade struck by a heavy blow to shatter it a second time, 30 and two Chill in 3 m; now a
            // patch that chills for 4 s).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Stolen Warmth: Cold Shade or The Long Night", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Stolen Warmth's two shapes, chosen at a rest fire. Cold Shade: the warmth is drawn as before, and a shattered enemy leaves its shadow behind as black ice. The Long Night: no health is drawn; the warmth goes to the dark, chilling all near while it is frozen. Either runs instead of the plain Stolen Warmth.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Cold Shade",
                        Description = "Its warmth is drawn as before, a Chill on it and 2 health to you with each weapon blow. When it is shattered its shadow stays behind as a figure of black ice for 4 s, chilling a stack a second all within 2.5 m, and a heavy blow on the shade shatters it a second time: 30, two Chill in 3 m.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition,
                                State = new ConditionSpec { Id = Shade, Seconds = 30f, While = AfflictionKind.Cursed, While2 = AfflictionKind.Chill, Keeps = AfflictionKind.Chill } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Shade, By = StruckBy.Weapon, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Shade, By = StruckBy.Weapon, Ranks = EnemyRank.Plain | EnemyRank.Elite, Cooldown = 0.25f, Do = UpgradeAction.Heal, Amount = 2f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Shade, By = StruckBy.Weapon, Ranks = EnemyRank.God, Cooldown = 0.25f, Do = UpgradeAction.Heal, Amount = 1f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = Shade, Do = UpgradeAction.Hazard, Radius = 2.5f, Seconds = 4f, Interval = 1f, Affliction = AfflictionKind.Chill, Stacks = 1 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "The Long Night",
                        Description = "The warmth goes to the dark instead of to you: no health is drawn, its Chill does not fade and each of your weapon blows chills it, and while it is frozen every other enemy within 3 m gains a Chill each second.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition,
                                State = new ConditionSpec { Id = LongNight, Seconds = 30f, Beat = 1f, While = AfflictionKind.Cursed, While2 = AfflictionKind.Chill, Keeps = AfflictionKind.Chill } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = LongNight, By = StruckBy.Weapon, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = LongNight, Needs = AfflictionKind.Chill, NeedsFull = true, Do = UpgradeAction.Afflict, Radius = 3f,
                                Affliction = AfflictionKind.Chill, Stacks = 1 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: a kill answered by the condition it died in (condition-needs: warmth stolen); now any Cursed, chilled enemy
            // that dies, at most every half second. Laid by the mark, the Curse and the Chill may start Stolen Warmth on each.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = HelsHalf, Name = "Hel's Half", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Skull,
                Description = "Killing an enemy whose warmth is stolen Curses every enemy within 4 m and gives each two Chill.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Cursed, Needs2 = AfflictionKind.Chill, Do = UpgradeAction.Afflict, Radius = 4f, Affliction = AfflictionKind.Cursed, Stacks = 1, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Cursed, Needs2 = AfflictionKind.Chill, Do = UpgradeAction.Afflict, Radius = 4f, Affliction = AfflictionKind.Chill, Stacks = 2, Cooldown = 0.5f },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: a mark that changes the reaction's own numbers (reaction-numbers); until then it does nothing.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DeepVeins, Name = "Deep Veins", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { Shape }, Icon = UpgradeIcons.Hourglass,
                Description = "Each blow on an enemy whose warmth is stolen draws 3 health, not 2, and 12 a second at most.",
            }, HeartPattern.RightLow));

            // NEEDS: a state of an enemy within 8 m being Frozen (freeze-moment); now Hunger's health price is set aside for 4 s
            // after each shatter.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = NoSummer, Name = "No Summer", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { HelsHalf, DeepVeins }, Icon = UpgradeIcons.Moon,
                Description = "Hunger costs no health while any enemy within 8 m is Frozen.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.Boon, Stat = PlayerStat.HealthPrices, Amount = -1f, Seconds = 4f, Cooldown = 0.5f } },
            }, HeartPattern.Keystone));

            // The duo spell, a detonator on a 12 s cooldown (SpellTimings), bought with 5 Shards once both are carried. The
            // brand form sets the sign of black frost on the enemy faced for 6 s; the weapon blows set it off, cursing (the
            // branch's first element) and chilling two deep all within 3 m.
            // NEEDS: each of the next four weapon blows freezing a vein for 8 and two Chill, the fourth breaking it from within,
            // frozen or not, for 35 and 50 stagger (form-pieces; now the brand's own set-off).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = IceInTheBlood, Name = "Ice in the Blood", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Detonator, Requires = new[] { HelsHalf, DeepVeins }, Icon = UpgradeIcons.Snowflake,
                Description = "A sign of black frost is set on the enemy you face for 6 s. Each of your next four weapon blows on it freezes a vein, 8 and two Chill, and the fourth breaks it from within: 35, 50 stagger, and a Curse and two Chill on everything within 3 m.",
                Form = SpellForms.Brand,
                Spell = new SpellSpec { Reach = 8f, Seconds = 6f, Count = 4, Radius = 3f, Damage = 35f, Stagger = 50f, Stacks = 1 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 2 } },
            }, HeartPattern.Spell));

            // The duo surge. NEEDS: its own 40 mana and 10 s in the Hand's surge slot (duo-surge-slot), the frost on the floor
            // round you, and statues of black ice a heavy blow shatters for 25, two Chill and a Curse in 3 m (standing-things);
            // now while any surge lasts each enemy that dies leaves a patch that chills within 2.5 m for 5 s.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = ThreeWinters, Name = "Three Winters", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { IceInTheBlood }, Icon = UpgradeIcons.Moon,
                Description = "A duo surge. While your surge lasts frost creeps over the floor round you, and every enemy that dies freezes where it falls as a statue of black ice for 5 s, chilling a stack a second within 2.5 m; a heavy blow on a statue shatters it, 25, two Chill and a Curse in 3 m.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, WhileSurge = true, Do = UpgradeAction.Hazard, Radius = 2.5f, Seconds = 5f, Interval = 1f, Affliction = AfflictionKind.Chill, Stacks = 1 },
                },
            }, HeartPattern.RightTop));
        }
    }
}
