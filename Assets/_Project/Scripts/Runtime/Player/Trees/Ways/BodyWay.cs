using System;
using System.Collections.Generic;

namespace Clover.Player.Trees.Ways
{
    /// <summary>
    /// The Body branch at the foot of every Way (way_plan.md 0.4, the shape decided by the owner on 2026-11-19, the numbers
    /// the plan's; way_build_plan.md 2.4; task 5.4): twelve marks, 28 Memories, always in force whatever is carried. A
    /// stem of four, then two sides each ending in a bearing: the enduring body (Full Flask, Steady, Hard Frame, The
    /// Enduring Body) and the light body (Deep Lungs, Quick Breath, Long Step, The Light Body). Both sides can be bought
    /// and all their numbers count; of the two bearings only the one set at a rest fire is in force (question G5), and the
    /// first bought sets itself (phase5_questions.md 5.1 Q3). Found pieces add on top (<see cref="Way.BodyPieces"/>).
    ///
    /// Every number is a stat on its mark, pushed into play by <see cref="PlayerUpgrades"/>; nothing here needs an effect,
    /// because the HUD's bars growing longer is the proof (way_build_plan.md 2.4).
    /// </summary>
    public static class BodyWay
    {
        public const string Health1 = "body.health1";
        public const string Stamina1 = "body.stamina1";
        public const string FourthFlask = "body.fourthflask";
        public const string Health2 = "body.health2";
        public const string FullFlask = "body.fullflask";
        public const string Steady = "body.steady";
        public const string HardFrame = "body.hardframe";
        public const string EnduringBody = "body.enduring";
        public const string DeepLungs = "body.deeplungs";
        public const string QuickBreath = "body.quickbreath";
        public const string LongStep = "body.longstep";
        public const string LightBody = "body.light";

        /// <summary>A Body mark's price, and a bearing's.</summary>
        public const int MarkPrice = 2, BearingPrice = 4;

        // The numbers (way_plan.md 0.4), kept with the words that state them.
        public const float HealthOne = 10f, HealthTwo = 10f, HardFrameHealth = 15f;
        public const float StaminaOne = 10f, DeepLungsStamina = 15f;
        /// <summary>A flask heals 45; Full Flask makes it 55.</summary>
        public const float FlaskHealBefore = 45f, FlaskHealAfter = 55f;
        public const float SteadyPoise = 6f;
        /// <summary>Stamina starts to come back after 0.8 s; Quick Breath makes it 0.6.</summary>
        public const float QuickBreathDelay = -0.2f;
        public const float LongStepMetres = 0.6f;
        /// <summary>The Enduring Body: a flinch of 0.35 s becomes 0.2, the push half as far, grey health fading after 4 s, not 2.5.</summary>
        public const float EnduringFlinch = -0.15f, EnduringKnockback = -0.5f, EnduringGreyDelay = 1.5f;
        /// <summary>The Light Body: one dodge may flow on, and the wait after a dodge is 0.1 s shorter.</summary>
        public const float LightRecovery = -0.1f;

        public static void Add(List<Upgrade> to)
        {
            var health1 = Mark(to, Health1, "Health I", UpgradeKind.Minor, "", UpgradeIcons.Heart, "More health.", new StatMod(PlayerStat.Health, HealthOne));
            Mark(to, Stamina1, "Stamina I", UpgradeKind.Minor, health1.Id, UpgradeIcons.Hourglass, "More stamina.", new StatMod(PlayerStat.StaminaMost, StaminaOne));
            Mark(to, FourthFlask, "Fourth Flask", UpgradeKind.Minor, Stamina1, UpgradeIcons.Flask, "Carry one more flask.", new StatMod(PlayerStat.FlaskCount, 1f));
            Mark(to, Health2, "Health II", UpgradeKind.Minor, FourthFlask, UpgradeIcons.Heart, "More health.", new StatMod(PlayerStat.Health, HealthTwo));

            // The enduring body.
            Mark(to, FullFlask, "Full Flask", UpgradeKind.Minor, Health2, UpgradeIcons.Flask, "Each flask heals more.",
                new StatMod(PlayerStat.FlaskHeal, FlaskHealAfter / FlaskHealBefore - 1f));
            Mark(to, Steady, "Steady", UpgradeKind.Major, FullFlask, UpgradeIcons.Shield, "Small hits don't make you flinch, and your swings are harder to stop.",
                new StatMod(PlayerStat.Poise, SteadyPoise));
            Mark(to, HardFrame, "Hard Frame", UpgradeKind.Minor, Steady, UpgradeIcons.Heart, "Much more health.", new StatMod(PlayerStat.Health, HardFrameHealth));
            Mark(to, EnduringBody, "The Enduring Body", UpgradeKind.Keystone, HardFrame, UpgradeIcons.Rock,
                "Hits knock you back less, and you recover faster. Lost health stays longer before it fades, so you have more time to win it back.",
                new StatMod(PlayerStat.FlinchSeconds, EnduringFlinch), new StatMod(PlayerStat.KnockbackTaken, EnduringKnockback), new StatMod(PlayerStat.GreyDelay, EnduringGreyDelay));

            // The light body.
            Mark(to, DeepLungs, "Deep Lungs", UpgradeKind.Minor, Health2, UpgradeIcons.Gust, "Much more stamina.", new StatMod(PlayerStat.StaminaMost, DeepLungsStamina));
            Mark(to, QuickBreath, "Quick Breath", UpgradeKind.Minor, DeepLungs, UpgradeIcons.Hourglass, "Stamina starts coming back sooner.",
                new StatMod(PlayerStat.StaminaDelay, QuickBreathDelay));
            Mark(to, LongStep, "Long Step", UpgradeKind.Minor, QuickBreath, UpgradeIcons.Boot, "Your dodge goes further.", new StatMod(PlayerStat.DodgeMetres, LongStepMetres));
            Mark(to, LightBody, "The Light Body", UpgradeKind.Keystone, LongStep, UpgradeIcons.Wing,
                "Dodge again just as a dodge ends to flow into a second one for half the stamina.",
                new StatMod(PlayerStat.DodgeFlow, 1f), new StatMod(PlayerStat.DodgeRecovery, LightRecovery));
        }

        // A mark of the Body: bought with Memories, on no weapon, grown from one mark.
        private static Upgrade Mark(List<Upgrade> to, string id, string name, UpgradeKind kind, string grows, string icon, string words, params StatMod[] stats)
        {
            var mark = new Upgrade
            {
                Id = id, Name = name, Kind = kind, Tree = MarkTree.Body, Currency = Currency.Memories, Weapon = "", Aspect = "",
                Requires = string.IsNullOrEmpty(grows) ? Array.Empty<string>() : new[] { grows }, Icon = icon, Description = words,
                Cost = kind == UpgradeKind.Keystone ? BearingPrice : MarkPrice, Stats = stats,
            };
            to.Add(mark);
            return mark;
        }
    }
}
