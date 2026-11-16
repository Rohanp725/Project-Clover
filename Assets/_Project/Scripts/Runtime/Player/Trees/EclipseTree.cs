using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Eclipse: the branch Light and Dark share. Light pays in mana for the clean fight and Dark in health for the
    /// desperate one; whoever holds both is paid twice for walking the line between them. Its crown, Totality, is
    /// given when the second of the two gods is consumed.
    /// </summary>
    public static class EclipseTree
    {
        public const string Twilight = "eclipse.twilight";
        public const string DayAndNight = "eclipse.daynight";
        public const string LowSun = "eclipse.lowsun";
        public const string BlackSun = "eclipse.blacksun";
        public const string DuskAndDawn = "eclipse.duskdawn";
        public const string TheDividingLine = "eclipse.dividingline";
        public const string Totality = "eclipse.totality";

        // Spell forms by the ids fixed in docs/design/FORMS.md; they are being built while this is written.
        private const string FormReverse = "reverse";
        private const string FormStep = "step";

        public static void Add(List<Upgrade> to)
        {
            // The first element is the one whose mix this is: the earlier of the two round the ring.
            const string A = Elements.Light, B = Elements.Dark;

            to.Add(new Upgrade { Id = Twilight, Name = "Twilight", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, X = 0f, Y = 1f, Icon = UpgradeIcons.Moon,
                Description = "The blow that ends a chain both Marks and Curses. Critical blows on an enemy that bears both feed you.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.ChainEnd, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.ChainEnd, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Cursed, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.CritHit, Needs = AfflictionKind.Marked, Needs2 = AfflictionKind.Cursed, Do = UpgradeAction.Leech, Amount = 0.15f } } });
            // The mana of the second is doubled either way round; the healing of a dodge that follows a guard is not yet.
            to.Add(new Upgrade { Id = DayAndNight, Name = "Day and Night", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { Twilight }, X = -0.5f, Y = 2.36f, Icon = UpgradeIcons.Sun,
                Description = "A perfect guard is of the day and gives mana. A perfect dodge is of the night and gives health. Do one after the other, and the second gives double.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Heal, Amount = 5f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Boon, Stat = PlayerStat.ManaFromGuards, Amount = 1f, Seconds = 6f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.ManaFromDodges, Amount = 1f, Seconds = 6f },
                } });
            // BESPOKE: the stored cut of light that lands with the heavy blow, and the line wiped by a blow taken. The step form gives the cut through and the critical heavy blow that answers it.
            to.Add(new Upgrade { Id = DuskAndDawn, Name = "Dusk and Dawn", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { Twilight }, X = 0.5f, Y = 2.36f, Icon = UpgradeIcons.Blade,
                Description = "Hold the spell button to cut through the enemy you face with a blade of dark. It wounds nothing yet. Land a heavy blow before the line fades and the cut of light answers along it, and both land together. 45 mana.",
                Form = FormStep,
                Spell = new SpellSpec { Cost = 45f, Reach = 5f, Damage = 10f, Stagger = 10f, Stacks = 1, Seconds = 3f } });
            // Blows give more mana always, and far more for a while each time health falls below a third; it does not yet rise evenly as health falls.
            to.Add(new Upgrade { Id = LowSun, Name = "Low Sun", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { DayAndNight }, X = 0f, Y = 3.48f, Icon = UpgradeIcons.Gem,
                Description = "The less health you have, the more mana every blow gives.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromBlows, 0.2f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.LowHealth, Do = UpgradeAction.Boon, Stat = PlayerStat.ManaFromBlows, Amount = 0.6f, Seconds = 12f } } });
            // The sun pulls and then bursts at one strength; it does not yet grow with the blows landed under it.
            to.Add(new Upgrade { Id = BlackSun, Name = "Black Sun", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { DayAndNight }, X = -0.5f, Y = 4.6f, Icon = UpgradeIcons.Orb,
                Description = "Hold the spell button to hang a black sun over the enemy you face. It draws everything beneath it inward, and it grows with each blow you land in its shadow. Then it sets, and its rim breaks outward as light. 50 mana.",
                Form = FormReverse,
                Spell = new SpellSpec { Cost = 50f, Reach = 5f, Radius = 5f, Delay = 3f, Damage = 40f, Stagger = 40f, Stacks = 1 } });
            // NEAREST: the day and night halves are as designed. The burst comes only on falling below a third, not on crossing the half either way.
            to.Add(new Upgrade { Id = TheDividingLine, Name = "The Dividing Line", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { LowSun }, X = 0.5f, Y = 4.6f, Icon = UpgradeIcons.Beam,
                Description = "Above half your health your blows are of the day: critical on the Marked, and rich in mana. Below half they are of the night: they Curse and they feed you. Each time you cross the line, it bursts.",
                Stats = new[] { new StatMod(PlayerStat.CritDamage, 0.2f, StatWhen.HighHealth), new StatMod(PlayerStat.ManaFromBlows, 0.2f, StatWhen.HighHealth) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AnyHit, While = StatWhen.HalfHealth, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Cursed, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.AnyHit, While = StatWhen.HalfHealth, Do = UpgradeAction.Leech, Amount = 0.08f }, new UpgradeEffect { On = UpgradeTrigger.LowHealth, Do = UpgradeAction.Burst, At = EffectAt.Self, Affliction = AfflictionKind.Cursed, Damage = 16f, Stagger = 25f, Radius = 3.5f, Stacks = 1, Cooldown = 4f } } });

            // BESPOKE (built): TotalityForm. The returning light gives back half of what each enemy lost in the dark, 80 at most.
            to.Add(new Upgrade { Id = Totality, Name = "Totality", Element = A, Element2 = B, Kind = UpgradeKind.Signature, X = 0f, Y = 5.8f, Icon = UpgradeIcons.Crown,
                Description = "When your surge begins, the sun is covered. While the dark lasts every enemy is Marked and Cursed and every blow you land is critical and feeds you. When the light returns, it strikes all you wounded.",
                Stats = new[] { new StatMod(PlayerStat.CritChance, 1f, StatWhen.Surge) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Heal, WhileSurge = true, Amount = 1f, Cooldown = 0.2f }, new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.Totality, Affliction = AfflictionKind.Marked, Spell = new SpellSpec { Cost = 0f, Reach = 14f, Stacks = 3, Damage = 50f, Count = 80, Stagger = 30f } } } });
        }
    }
}
