using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Wind: fighting in motion. The dodge is a weapon, the enemy is pushed, turned round and thrown into the air.
    /// Three boughs from Unsteady: on the left Slipstream (the dodge as a weapon), in the middle Updraft (launch and
    /// juggle), on the right Gale (push and pull, and the perfect guard that throws).
    /// </summary>
    public static class WindTree
    {
        public const string Unsteady = "wind.unsteady";
        // Slipstream.
        public const string Slipstream = "wind.slip";
        public const string LongStride = "wind.stride";
        public const string ThinAir = "wind.thinair";
        public const string FollowingDraft = "wind.draft";
        public const string PassingCut = "wind.passing";
        public const string CleanPass = "wind.cleanpass";
        public const string LightFeet = "wind.lightfeet";
        public const string EmptyAir = "wind.emptyair";
        public const string ClosingCircle = "wind.circle";
        public const string LeafOnTheWind = "wind.leaf";
        // Updraft.
        public const string RisingCut = "wind.rising";
        public const string LooseFooting = "wind.loose";
        public const string OffBalance = "wind.offbalance";
        public const string HammerDown = "wind.hammer";
        public const string HangTime = "wind.hang";
        public const string OpenAir = "wind.openair";
        public const string LiftingWind = "wind.lifting";
        public const string SkyDance = "wind.skydance";
        public const string NothingBeneath = "wind.nothing";
        // Gale.
        public const string AgainstTheWall = "wind.wall";
        public const string WideGust = "wind.widegust";
        public const string EasyBreath = "wind.easybreath";
        public const string TurnedAside = "wind.turned";
        public const string StrongArm = "wind.strongarm";
        public const string FairWind = "wind.fairwind";
        public const string RunningStart = "wind.runningstart";
        public const string DrawingWind = "wind.drawing";
        public const string IndrawnBreath = "wind.indrawn";
        public const string StormFront = "wind.front";
        // The god's gift.
        public const string TheWindRemembers = "wind.remembers";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Wind;

            // The words say five; Unsteady is three deep at most in Afflictions today.
            to.Add(new Upgrade { Id = Unsteady, Name = "Unsteady", Element = E, Kind = UpgradeKind.Root, X = 0f, Y = 1.2f, Icon = UpgradeIcons.Gust,
                Description = "Blows knock an enemy Unsteady, up to five. It is pushed further, turns slower and loses its balance sooner.",
                WeaponStacks = 1 });

            // Left bough: Slipstream. The dodge as a weapon.
            // The boons last their 1.2 seconds: every blow in that time is harder, not only the first.
            to.Add(new Upgrade { Id = Slipstream, Name = "Rushing Wake", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Unsteady }, X = -0.94f, Y = 2.51f, Icon = UpgradeIcons.Boot,
                Description = "The first blow that lands soon after any dodge strikes harder and leaves its enemy more Unsteady.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Do = UpgradeAction.Boon, Stat = PlayerStat.WeaponDamage, Amount = 0.3f, Seconds = 1.2f, UntilBlow = true },
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Do = UpgradeAction.Boon, Stat = PlayerStat.AfflictionStacks, Amount = 2f, Seconds = 1.2f, UntilBlow = true },
                } });
            to.Add(new Upgrade { Id = LongStride, Name = "Long Stride", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Slipstream }, X = -0.72f, Y = 3.42f, Icon = UpgradeIcons.Boot,
                Description = "The dodge carries further.",
                Stats = new[] { new StatMod(PlayerStat.DodgeDistance, 0.2f) } });
            // BESPOKE: the dodge should pass through enemies, cut each one passed, and leave it unable to turn for a moment.
            // Until then the end of every dodge cuts whatever stands close by.
            to.Add(new Upgrade { Id = PassingCut, Name = "Passing Cut", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Slipstream }, X = -1.44f, Y = 4.47f, Icon = UpgradeIcons.Blade,
                Description = "Where a dodge ends, a cut of wind strikes everything close to you and leaves it Unsteady.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Do = UpgradeAction.Burst, At = EffectAt.Self, Radius = 2.2f, Damage = 10f, Stagger = 8f, Affliction = AfflictionKind.Unsteady, Stacks = 1, Cooldown = 0.5f } } });
            // BoonSeconds lengthens every timed boon of the marks, Rushing Wake's wait among them (1.2 s to 2 s); the words say so.
            to.Add(new Upgrade { Id = FollowingDraft, Name = "Following Draft", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Slipstream }, X = -1.82f, Y = 2.84f, Icon = UpgradeIcons.Hourglass,
                Description = "Rushing Wake waits longer for its blow, and every other timed gift of your marks lasts longer too.",
                Stats = new[] { new StatMod(PlayerStat.BoonSeconds, 0.67f) } });
            to.Add(new Upgrade { Id = ThinAir, Name = "Thin Air", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Slipstream }, X = -2.05f, Y = 3.75f, Icon = UpgradeIcons.Wing,
                Description = "The dodge is safe a moment longer.",
                Stats = new[] { new StatMod(PlayerStat.DodgeSafeTime, 0.15f) } });
            // The perfect dodge names its attacker now. The stamina is the dodge's usual 25.
            to.Add(new Upgrade { Id = EmptyAir, Name = "Empty Air", Element = E, Kind = UpgradeKind.Major, Requires = new[] { PassingCut }, X = -2.02f, Y = 6.14f, Icon = UpgradeIcons.Eye,
                Description = "A perfect dodge gives back its stamina and leaves the one who struck three deeper in Unsteady.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Stamina, Amount = 25f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 3 },
                } });
            to.Add(new Upgrade { Id = LightFeet, Name = "Light Feet", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { PassingCut }, X = -2.42f, Y = 5.05f, Icon = UpgradeIcons.Wing,
                Description = "The dodge costs less stamina.",
                Stats = new[] { new StatMod(PlayerStat.StaminaCost, -0.2f) } });
            // BESPOKE: Passing Cut's own damage should rise by half. Until then blows on an enemy carrying anything cut deeper.
            to.Add(new Upgrade { Id = CleanPass, Name = "Clean Pass", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { PassingCut }, X = -1.12f, Y = 5.36f, Icon = UpgradeIcons.Blade,
                Description = "Everything you do cuts deeper into an enemy that carries an affliction.",
                Stats = new[] { new StatMod(PlayerStat.DamageToAfflicted, 0.08f) } });
            // BESPOKE (built): CircleForm. The player cannot be touched for the whole run; the design asks for the first half only.
            to.Add(new Upgrade { Id = ClosingCircle, Name = "Closing Circle", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { EmptyAir }, X = -2.14f, Y = 7.49f, Icon = UpgradeIcons.Ring,
                Description = "You run a ring round the enemy faced. When the ring closes, the wind inside it rises and takes everything off the ground.",
                Form = SpellForms.Circle,
                Spell = new SpellSpec { Cost = 35f, Reach = 5f, Radius = 3f, Seconds = 1.2f, Damage = 6f, Stagger = 10f, Stacks = 5 } });
            // Built by hand (DodgeRoll.PayInstead, PlayerUpgrades.SpendBreath): three breaths kept as the mark's count, one back every
            // two seconds and one for every three blows landed. With none, no dodge.
            to.Add(new Upgrade { Id = LeafOnTheWind, Name = "Leaf on the Wind", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { ClosingCircle }, X = -2.91f, Y = 8.83f, Icon = UpgradeIcons.Leaf,
                Description = "The dodge costs no stamina. It spends a breath of wind instead: you hold three, and time and blows win them back. With none you cannot dodge. You are easier to wound.",
                Tally = new TallySpec { Name = "Breath", Cap = 3 },
                Stats = new[] { new StatMod(PlayerStat.DamageTaken, 0.2f) } });

            // Middle bough: Updraft. Launch and juggle.
            to.Add(new Upgrade { Id = RisingCut, Name = "Rising Cut", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Unsteady }, X = 0.28f, Y = 3.08f, Icon = UpgradeIcons.Arrow,
                Description = "A heavy blow on an enemy five deep in Unsteady throws it into the air. It hangs there helpless, and each blow holds it up a little longer, up to three.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Unsteady, NeedsFull = true, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Unsteady, Damage = 0f, Stagger = 0f, Radius = 1f, Cooldown = 0.5f }, new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Unsteady, NeedsFull = true, Do = UpgradeAction.Lift, Seconds = 1f, Cooldown = 0.5f } } });
            // A minor with an effect, because the effect is exactly what the words say.
            to.Add(new Upgrade { Id = LooseFooting, Name = "Loose Footing", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { RisingCut }, X = -0.27f, Y = 4.26f, Icon = UpgradeIcons.Boot,
                Description = "Heavy blows leave an enemy two deeper in Unsteady.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 1 } } });
            to.Add(new Upgrade { Id = OffBalance, Name = "Off Balance", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { RisingCut }, X = 0.71f, Y = 4.18f, Icon = UpgradeIcons.Hourglass,
                Description = "Unsteady lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, 0.5f) } });
            // NEAREST: a heavy blow on an enemy in the air drives it down and bursts where it lands. The blow itself is not yet half again as hard.
            to.Add(new Upgrade { Id = HammerDown, Name = "Hammer Down", Element = E, Kind = UpgradeKind.Major, Requires = new[] { RisingCut }, X = 0.05f, Y = 5.19f, Icon = UpgradeIcons.Hammer,
                Description = "A heavy blow on an enemy in the air drives it into the ground. The landing bursts.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, NeedsAirborne = true, Do = UpgradeAction.Drop }, new UpgradeEffect { On = UpgradeTrigger.HeavyHit, NeedsAirborne = true, Do = UpgradeAction.Burst, Damage = 14f, Stagger = 30f, Radius = 3f }, new UpgradeEffect { On = UpgradeTrigger.HeavyHit, NeedsAirborne = true, Do = UpgradeAction.Push, Radius = 3f, Amount = 2f } } });
            to.Add(new Upgrade { Id = HangTime, Name = "Hang Time", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { HammerDown }, X = 0.6f, Y = 6.04f, Icon = UpgradeIcons.Hourglass,
                Description = "Enemies thrown into the air hang longer.",
                Stats = new[] { new StatMod(PlayerStat.AirSeconds, 0.4f) } });
            to.Add(new Upgrade { Id = OpenAir, Name = "Open Air", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { HammerDown }, X = -0.59f, Y = 6.2f, Icon = UpgradeIcons.Fist,
                Description = "Blows strike an enemy in the air harder.",
                Stats = new[] { new StatMod(PlayerStat.WeaponDamage, 0.2f, StatWhen.AirborneBlow) } });
            to.Add(new Upgrade { Id = LiftingWind, Name = "Lifting Wind", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { HammerDown }, X = -0.09f, Y = 7.11f, Icon = UpgradeIcons.Tower,
                Description = "You raise a column of wind under the enemy faced. It goes up whatever its footing.",
                Form = "pillars",
                Spell = new SpellSpec { Cost = 30f, Count = 1, Reach = 5f, Radius = 2f, Damage = 8f, Stagger = 25f, Stacks = 1, Delay = 0.35f } });
            // BESPOKE (built): SkyDanceForm.
            to.Add(new Upgrade { Id = SkyDance, Name = "Sky Dance", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { LiftingWind }, X = -0.92f, Y = 8.29f, Icon = UpgradeIcons.Wing,
                Description = "You throw the enemy faced into the air and go up beside it. Press the light attack as each of five rings closes for a harder cut. Then the stick chooses where it is thrown down, and it bursts harder for each cut in time. 45 mana.",
                Form = SpellForms.SkyDance,
                Spell = new SpellSpec { Cost = 45f, Count = 5, Interval = 0.34f, Damage = 10f, Stagger = 60f, Stacks = 1, Reach = 6f, Radius = 3f, LingerDamage = 6f } });
            to.Add(new Upgrade { Id = NothingBeneath, Name = "Nothing Beneath", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { SkyDance }, X = -1.32f, Y = 9.77f, Icon = UpgradeIcons.Spiral,
                Description = "An enemy in the air stays there for as long as you keep striking it, and each blow up there gives double mana. Your blows on an enemy that is not Unsteady are weaker.",
                Stats = new[] { new StatMod(PlayerStat.AirBlows, 20f), new StatMod(PlayerStat.ManaFromBlows, 1f, StatWhen.AirborneBlow), new StatMod(PlayerStat.WeaponDamage, -0.15f), new StatMod(PlayerStat.WeaponDamage, 0.15f, AfflictionKind.Unsteady) } });

            // Right bough: Gale. Push and pull.
            // BESPOKE: a pushed enemy that meets a wall or another enemy should stop there and both be hurt. Until then every
            // enemy a spell strikes (Gust is the push) is shaken where it stands, and so is whoever is beside it.
            to.Add(new Upgrade { Id = AgainstTheWall, Name = "Against the Wall", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Unsteady }, X = 1.46f, Y = 3.62f, Icon = UpgradeIcons.Wall,
                Description = "Each enemy your spells strike is hit by a small burst of wind that shakes it and anything beside it and leaves them Unsteady.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Burst, Damage = 6f, Stagger = 15f, Radius = 1.6f, Affliction = AfflictionKind.Unsteady, Stacks = 1 } } });
            // The reach is the stat for every spell; the wider cone is still to be written by hand.
            to.Add(new Upgrade { Id = WideGust, Name = "Wide Gust", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { AgainstTheWall }, X = 1.25f, Y = 5.05f, Icon = UpgradeIcons.Gust,
                Description = "Gust spreads wider and reaches further.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellArc, 0.43f), new StatMod(PlayerStat.BaseSpellReach, 0.2f) } });
            // A thrown attack sent back to its thrower is still to be written by hand.
            to.Add(new Upgrade { Id = TurnedAside, Name = "Turned Aside", Element = E, Kind = UpgradeKind.Major, Requires = new[] { AgainstTheWall }, X = 1.9f, Y = 6.06f, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard throws everyone near you back and leaves the attacker three deeper in Unsteady.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 3 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Push, At = EffectAt.Self, Radius = 4f, Amount = 3f },
                } });
            // The wait of this element's own base spell alone (SpellTimings.BaseCooldownShare), carried first or second.
            to.Add(new Upgrade { Id = EasyBreath, Name = "Easy Breath", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { AgainstTheWall }, X = 2.27f, Y = 4.45f, Icon = UpgradeIcons.Gem,
                Description = "Gust waits a fifth less between casts.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellCooldown, -0.2f) } });
            // PushDistance: the Push action and what spell forms shove.
            to.Add(new Upgrade { Id = StrongArm, Name = "Strong Arm", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { TurnedAside }, X = 1.08f, Y = 6.89f, Icon = UpgradeIcons.Fist,
                Description = "Your pushes throw enemies further.",
                Stats = new[] { new StatMod(PlayerStat.PushDistance, 0.3f) } });
            to.Add(new Upgrade { Id = IndrawnBreath, Name = "Indrawn Breath", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { TurnedAside }, X = 1.06f, Y = 8.07f, Icon = UpgradeIcons.Spiral,
                Description = "You drag everything near into one place before you, then burst it apart.",
                Form = "reverse",
                Spell = new SpellSpec { Cost = 35f, Reach = 2f, Radius = 7f, Delay = 0.5f, Damage = 10f, Stagger = 25f, Stacks = 2 } });
            to.Add(new Upgrade { Id = FairWind, Name = "Fair Wind", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { TurnedAside }, X = 2.81f, Y = 6.4f, Icon = UpgradeIcons.Gem,
                Description = "Tailwind costs less mana.",
                Stats = new[] { new StatMod(PlayerStat.SurgeCost, -0.2f) } });
            to.Add(new Upgrade { Id = RunningStart, Name = "Running Start", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { TurnedAside }, X = 3.06f, Y = 7.38f, Icon = UpgradeIcons.Wing,
                Description = "You move faster while Tailwind lasts.",
                Stats = new[] { new StatMod(PlayerStat.SurgeSpeed, 0.1f) } });
            // BESPOKE (built): PlayerUpgrades throws the base spell free through PlayerElement.CastBaseFree on each heavy blow or finisher that lands.
            to.Add(new Upgrade { Id = StormFront, Name = "Storm Front", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { IndrawnBreath }, X = 2.44f, Y = 8.86f, Icon = UpgradeIcons.Wave,
                Description = "Every heavy blow that lands throws a Gust with it at no cost. Your blows give less mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromBlows, -0.25f) } });
            // BESPOKE: the pull should come as the draw is released, before the cut lands, and only from the cone ahead.
            // Until then the slash that lands drags everything near onto the player and leaves it Unsteady.
            to.Add(new Upgrade { Id = DrawingWind, Name = "Drawing Wind", Element = E, Kind = UpgradeKind.Major, Requires = new[] { TurnedAside }, X = 2.09f, Y = 7.43f, Icon = UpgradeIcons.Blade, Weapon = Weapons.KatanaName,
                Description = "When the Iaido Slash lands, everything within six metres is pulled toward you and left Unsteady.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SkillHit, Do = UpgradeAction.Pull, At = EffectAt.Self, Radius = 6f, Amount = 2.5f, Cooldown = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.SkillHit, Do = UpgradeAction.Afflict, At = EffectAt.Self, Radius = 6f, Affliction = AfflictionKind.Unsteady, Stacks = 2, Cooldown = 1f },
                } });

            // BESPOKE (built): RememberForm.
            to.Add(new Upgrade { Id = TheWindRemembers, Name = "The Wind Remembers", Element = E, Kind = UpgradeKind.Signature, X = 0f, Y = 12f, Icon = UpgradeIcons.Crown,
                Description = "While Tailwind lasts, every blow you land hangs in the air where it fell, up to sixteen. When Tailwind ends, all of them fall again at once, each half as hard as the blow it remembers.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.Remember, Spell = new SpellSpec { Cost = 0f, Damage = 50f, Stagger = 10f, Stacks = 1 } } } });
        }
    }
}
