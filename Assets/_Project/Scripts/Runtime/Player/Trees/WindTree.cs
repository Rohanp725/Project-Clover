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
                Description = "The first blow after any dodge strikes harder and leaves its enemy more Unsteady.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Do = UpgradeAction.Boon, Stat = PlayerStat.WeaponDamage, Amount = 0.3f, Seconds = 1.2f },
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Do = UpgradeAction.Boon, Stat = PlayerStat.AfflictionStacks, Amount = 2f, Seconds = 1.2f },
                } });
            to.Add(new Upgrade { Id = LongStride, Name = "Long Stride", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Slipstream }, X = -0.72f, Y = 3.42f, Icon = UpgradeIcons.Boot,
                Description = "The dodge carries further.",
                Stats = new[] { new StatMod(PlayerStat.DodgeDistance, 0.2f) } });
            // BESPOKE: the dodge should pass through enemies, cut each one passed, and leave it unable to turn for a moment.
            // Until then the end of every dodge cuts whatever stands close by.
            to.Add(new Upgrade { Id = PassingCut, Name = "Passing Cut", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Slipstream }, X = -1.44f, Y = 4.47f, Icon = UpgradeIcons.Blade,
                Description = "Dodge through an enemy to cut it as you pass. It is left facing where you were.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Do = UpgradeAction.Burst, At = EffectAt.Self, Radius = 2.2f, Damage = 10f, Stagger = 8f, Affliction = AfflictionKind.Unsteady, Stacks = 1, Cooldown = 0.5f } } });
            // BESPOKE: Slipstream's boon should wait two seconds for its blow, not 1.2. Until then the weapon is swung a little faster.
            to.Add(new Upgrade { Id = FollowingDraft, Name = "Following Draft", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Slipstream }, X = -1.82f, Y = 2.84f, Icon = UpgradeIcons.Hourglass,
                Description = "Rushing Wake waits longer for its blow.",
                Stats = new[] { new StatMod(PlayerStat.SwingSpeed, 0.05f) } });
            to.Add(new Upgrade { Id = ThinAir, Name = "Thin Air", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Slipstream }, X = -2.05f, Y = 3.75f, Icon = UpgradeIcons.Wing,
                Description = "The dodge is safe a moment longer.",
                Stats = new[] { new StatMod(PlayerStat.DodgeSafeTime, 0.15f) } });
            // A perfect dodge names no attacker: the stacks go on the enemy faced. The stamina is the dodge's usual 25.
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
                Description = "Passing Cut bites deeper.",
                Stats = new[] { new StatMod(PlayerStat.DamageToAfflicted, 0.08f) } });
            // BESPOKE: the run round the enemy, the line drawn behind, and the column that takes everything inside off the ground.
            // Until then the ring form: a ring closed round the player and the enemy faced that bites what touches it.
            to.Add(new Upgrade { Id = ClosingCircle, Name = "Closing Circle", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { EmptyAir }, X = -2.14f, Y = 7.49f, Icon = UpgradeIcons.Ring,
                Description = "Hold the spell button to run a ring round the enemy faced. When the ring closes, the wind inside it rises and takes everything off the ground. 35 mana.",
                Form = "ring",
                Spell = new SpellSpec { Cost = 35f, Reach = 5f, Radius = 3f, Seconds = 3f, LingerDamage = 4f, Stacks = 3 } });
            // BESPOKE: three breaths of wind that pay for the dodge in place of stamina, won back by time and by blows.
            // Until then the dodge costs half its stamina, and blows that land on the player hurt more.
            to.Add(new Upgrade { Id = LeafOnTheWind, Name = "Leaf on the Wind", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { ClosingCircle }, X = -2.91f, Y = 8.83f, Icon = UpgradeIcons.Leaf,
                Description = "The dodge costs no stamina. It spends a breath of wind instead: you hold three, and blows win them back. You are easier to wound.",
                Stats = new[] { new StatMod(PlayerStat.StaminaCost, -0.5f), new StatMod(PlayerStat.DamageTaken, 0.2f) } });

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
                Description = "Hold the spell button to raise a column of wind under the enemy faced. It goes up whatever its footing. 30 mana.",
                Form = "pillars",
                Spell = new SpellSpec { Cost = 30f, Count = 1, Reach = 5f, Radius = 2f, Damage = 8f, Stagger = 25f, Stacks = 1, Delay = 0.35f } });
            // BESPOKE: the dance in the air, the five timed cuts and the throw to a chosen spot. Until then the flash form:
            // five cuts across the enemy faced that land together.
            to.Add(new Upgrade { Id = SkyDance, Name = "Sky Dance", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { LiftingWind }, X = -0.92f, Y = 8.29f, Icon = UpgradeIcons.Wing,
                Description = "With an enemy in the air, hold the spell button to go up after it. Strike in time, up to five, then send it down where you choose. 45 mana.",
                Form = "flash",
                Spell = new SpellSpec { Cost = 45f, Count = 5, Interval = 0.25f, Delay = 0.3f, Damage = 10f, Stagger = 12f, Stacks = 1, Reach = 6f } });
            to.Add(new Upgrade { Id = NothingBeneath, Name = "Nothing Beneath", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { SkyDance }, X = -1.32f, Y = 9.77f, Icon = UpgradeIcons.Spiral,
                Description = "An enemy in the air stays there for as long as you keep striking it, and each blow up there gives double mana. Your blows on an enemy that is not Unsteady are weaker.",
                Stats = new[] { new StatMod(PlayerStat.AirBlows, 20f), new StatMod(PlayerStat.ManaFromBlows, 1f, StatWhen.AirborneBlow), new StatMod(PlayerStat.WeaponDamage, -0.15f), new StatMod(PlayerStat.WeaponDamage, 0.15f, AfflictionKind.Unsteady) } });

            // Right bough: Gale. Push and pull.
            // BESPOKE: a pushed enemy that meets a wall or another enemy should stop there and both be hurt. Until then every
            // enemy a spell strikes (Gust is the push) is shaken where it stands, and so is whoever is beside it.
            to.Add(new Upgrade { Id = AgainstTheWall, Name = "Against the Wall", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Unsteady }, X = 1.46f, Y = 3.62f, Icon = UpgradeIcons.Wall,
                Description = "An enemy pushed into a wall or into another enemy is hurt and shaken by it. So is the one it hits.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Burst, Damage = 6f, Stagger = 15f, Radius = 1.6f, Affliction = AfflictionKind.Unsteady, Stacks = 1 } } });
            // The reach is the stat for every spell; the wider cone is still to be written by hand.
            to.Add(new Upgrade { Id = WideGust, Name = "Wide Gust", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { AgainstTheWall }, X = 1.25f, Y = 5.05f, Icon = UpgradeIcons.Gust,
                Description = "Gust spreads wider and reaches further.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellArc, 0.43f), new StatMod(PlayerStat.BaseSpellReach, 0.2f) } });
            // A thrown attack sent back to its thrower is still to be written by hand.
            to.Add(new Upgrade { Id = TurnedAside, Name = "Turned Aside", Element = E, Kind = UpgradeKind.Major, Requires = new[] { AgainstTheWall }, X = 1.9f, Y = 6.06f, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard throws the attacker back and leaves it three deeper in Unsteady. A thrown attack is sent back to its thrower.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 3 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Push, At = EffectAt.Self, Radius = 4f, Amount = 3f },
                } });
            // The cost is the stat for every spell, the base one and whatever is worn.
            to.Add(new Upgrade { Id = EasyBreath, Name = "Easy Breath", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { AgainstTheWall }, X = 2.27f, Y = 4.45f, Icon = UpgradeIcons.Gem,
                Description = "Gust costs less mana.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellCost, -0.25f) } });
            // There is no number for push distance alone: Unsteady is what makes an enemy fly further, so it works more strongly.
            to.Add(new Upgrade { Id = StrongArm, Name = "Strong Arm", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { TurnedAside }, X = 1.08f, Y = 6.89f, Icon = UpgradeIcons.Fist,
                Description = "Your pushes throw enemies further.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionPower, 0.15f) } });
            to.Add(new Upgrade { Id = IndrawnBreath, Name = "Indrawn Breath", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { TurnedAside }, X = 1.06f, Y = 8.07f, Icon = UpgradeIcons.Spiral,
                Description = "Hold the spell button to drag everything near into one place before you, then burst it apart. 35 mana.",
                Form = "reverse",
                Spell = new SpellSpec { Cost = 35f, Reach = 2f, Radius = 7f, Delay = 0.5f, Damage = 10f, Stagger = 25f, Stacks = 2 } });
            to.Add(new Upgrade { Id = FairWind, Name = "Fair Wind", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { TurnedAside }, X = 2.81f, Y = 6.4f, Icon = UpgradeIcons.Gem,
                Description = "Tailwind costs less mana.",
                Stats = new[] { new StatMod(PlayerStat.SurgeCost, -0.2f) } });
            to.Add(new Upgrade { Id = RunningStart, Name = "Running Start", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { TurnedAside }, X = 3.06f, Y = 7.38f, Icon = UpgradeIcons.Wing,
                Description = "You move faster while Tailwind lasts.",
                Stats = new[] { new StatMod(PlayerStat.SurgeSpeed, 0.1f) } });
            // A cone and a push stand in for the free Gust: the Gust itself, with its own upgrades, is still to be written by hand.
            to.Add(new Upgrade { Id = StormFront, Name = "Storm Front", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { IndrawnBreath }, X = 2.44f, Y = 8.86f, Icon = UpgradeIcons.Wave,
                Description = "Every heavy blow throws a Gust with it at no cost. Your blows give less mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromBlows, -0.25f) },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Cone, Damage = 8f, Stagger = 10f, Reach = 5f, Arc = 70f, Affliction = AfflictionKind.Unsteady, Stacks = 2, Cooldown = 0.4f },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Push, At = EffectAt.Self, Radius = 4.5f, Amount = 3f, Cooldown = 0.4f },
                } });
            // BESPOKE: the pull should come as the draw is released, before the cut lands, and only from the cone ahead.
            // Until then the slash that lands drags everything near onto the player and leaves it Unsteady.
            to.Add(new Upgrade { Id = DrawingWind, Name = "Drawing Wind", Element = E, Kind = UpgradeKind.Major, Requires = new[] { TurnedAside }, X = 2.09f, Y = 7.43f, Icon = UpgradeIcons.Blade, Weapon = Weapons.KatanaName,
                Description = "The Iaido Slash draws breath first. Everything before you is pulled onto the cut.",
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
