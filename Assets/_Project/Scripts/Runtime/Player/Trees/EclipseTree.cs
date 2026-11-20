using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Eclipse: the duo branch Light and Dark share, reshaped by pairs_d1_d2.md ("Light and Dark", task 4.15) and drawn in
    /// the chart's heart (<see cref="HeartPattern"/>). Its reaction is Diamond Ring; the Shape mark opens its two shapes,
    /// Shadow Bands and Through the Throat. Twilight is cut (it only laid both; the Shape mark takes its place) and Black
    /// Sun, offered to Dark's tree, is cut because that roster is full; both are in <see cref="RetiredMarks"/>. Kept: Day
    /// and Night, Low Sun, The Dividing Line (the keystone), Dusk and Dawn (the duo spell, re-aimed at the pair) and
    /// Totality, begun by the surge, the branch's duo surge and top mark. Open while both are carried.
    /// </summary>
    public static class EclipseTree
    {
        public const string Shape = "eclipse.shape";
        public const string DayAndNight = "eclipse.daynight";
        public const string LowSun = "eclipse.lowsun";
        public const string DuskAndDawn = "eclipse.duskdawn";
        public const string TheDividingLine = "eclipse.dividingline";
        public const string Totality = "eclipse.totality";
        /// <summary>The conditions the two shapes set, each keeping the Mark as the plain reaction's disc does.</summary>
        public const string Bands = "diamondring.bands";
        public const string Throat = "diamondring.throat";

        // Spell forms by the ids fixed in docs/design/FORMS.md.
        private const string FormStep = "step";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Light, B = Elements.Dark;

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Diamond Ring: Shadow Bands or Through the Throat", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Diamond Ring's two shapes, chosen at a rest fire. Shadow Bands: as the dark closes, bands of shadow ripple out and Curse every enemy they cross. Through the Throat: the light slips out of its far side as a line. Either runs instead of the plain Diamond Ring.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Shadow Bands",
                        Description = "Dark closes over it and the bead of light breaks at the rim as before, 16 and 30 stagger, and as the dark closes bands of shadow ripple out across the floor to 5 m and Curse every enemy they cross. For 4 s its Mark is not spent by blows.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 0.5f, Delay = 0.5f, Damage = 16f, Stagger = 30f, Affliction = AfflictionKind.Marked },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Afflict, Radius = 5f, Affliction = AfflictionKind.Cursed, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Bands, Seconds = 4f, Keeps = AfflictionKind.Marked } },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Through the Throat",
                        Description = "The light slips out of its far side as a line 6 m long: 12 damage and a Mark on everything on the line. For 4 s its Mark is not spent by blows.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Lane, Reach = 6f, Radius = 0.6f, Damage = 12f, Affliction = AfflictionKind.Marked, Stacks = 1, Delay = 0.3f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Throat, Seconds = 4f, Keeps = AfflictionKind.Marked } },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // The mana of the second is doubled either way round; the healing of a dodge that follows a guard is not yet.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DayAndNight, Name = "Day and Night", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Sun,
                Description = "A perfect dodge heals a little. For a while after a perfect dodge, a perfect guard gives double mana, and for a while after a perfect guard, a perfect dodge gives double mana.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Heal, Amount = 5f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Boon, Stat = PlayerStat.ManaFromGuards, Amount = 1f, Seconds = 6f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.ManaFromDodges, Amount = 1f, Seconds = 6f },
                },
            }, HeartPattern.LeftLow));

            // Blows give more mana always, and far more for a while each time health falls below a third; it does not yet rise evenly as health falls.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = LowSun, Name = "Low Sun", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Gem,
                Description = "Every blow gives a fifth more mana. Each time your health falls below a third, blows give far more for a while.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromBlows, 0.2f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.LowHealth, Do = UpgradeAction.Boon, Stat = PlayerStat.ManaFromBlows, Amount = 0.6f, Seconds = 12f } },
            }, HeartPattern.RightLow));

            // NEAREST: the day and night halves are as designed. The burst comes only on falling below a third, not on crossing the half either way.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheDividingLine, Name = "The Dividing Line", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { DayAndNight, LowSun }, Icon = UpgradeIcons.Beam,
                Description = "At half your health or more, your critical blows strike a fifth harder and blows give a fifth more mana. Below half, every blow Curses and feeds you. Each time your health falls below a third, a burst round you Curses all it strikes.",
                Stats = new[] { new StatMod(PlayerStat.CritDamage, 0.2f, StatWhen.HighHealth), new StatMod(PlayerStat.ManaFromBlows, 0.2f, StatWhen.HighHealth) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AnyHit, While = StatWhen.HalfHealth, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Cursed, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.AnyHit, While = StatWhen.HalfHealth, Do = UpgradeAction.Leech, Amount = 0.08f }, new UpgradeEffect { On = UpgradeTrigger.LowHealth, Do = UpgradeAction.Burst, At = EffectAt.Self, Affliction = AfflictionKind.Cursed, Damage = 16f, Stagger = 25f, Radius = 3.5f, Stacks = 1, Cooldown = 4f } },
            }, HeartPattern.Keystone));

            // The duo spell, re-aimed at the pair (pairs_d1_d2.md): you step into the enemy's shadow and come out of its far rim
            // as the bead of light, cutting; the cut sets off Diamond Ring at once, whatever the rest, and the heavy blow after
            // it is critical. The step form gives the cut through and the critical heavy blow that answers it.
            // NEEDS: the 0.4 s gone in the shadow; Diamond Ring set off by the cut on an enemy carrying only one of the pair
            // (forced-alone; now only on one Marked and Cursed already).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DuskAndDawn, Name = "Dusk and Dawn", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Movement, Requires = new[] { DayAndNight, LowSun }, Icon = UpgradeIcons.Blade,
                Description = "You step into the shadow of the enemy you face and come out of its far side as the light, cutting. On one Marked and Cursed the cut sets off Diamond Ring at once, whatever its rest, and your next heavy blow in the moment after is critical. 45 mana.",
                Form = FormStep,
                Spell = new SpellSpec { Cost = 45f, Reach = 5f, Damage = 10f, Stagger = 10f, Stacks = 1, Seconds = 3f },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.ForceReaction, Reaction = "react.diamondring" } },
            }, HeartPattern.Spell));

            // BESPOKE (built): TotalityForm. The returning light gives back half of what each enemy lost in the dark, 80 at most.
            // The branch's duo surge, begun by the surge, and its top mark. NEEDS: a duo surge chosen in the Hand's surge slot.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Totality, Name = "Totality", Element = A, Element2 = B, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "When your surge begins, the sun is covered. While the dark lasts every enemy is Marked and Cursed and every blow you land is critical and feeds you. When the light returns, it strikes all you wounded.",
                Stats = new[] { new StatMod(PlayerStat.CritChance, 1f, StatWhen.Surge) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Heal, WhileSurge = true, Amount = 1f, Cooldown = 0.2f }, new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.Totality, Affliction = AfflictionKind.Marked, Spell = new SpellSpec { Cost = 0f, Reach = 14f, Stacks = 3, Damage = 50f, Count = 80, Stagger = 30f } } },
            }, HeartPattern.Crown));
        }
    }
}
