using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Fire: never letting up, written as spell branches (docs/design/steel/elements_a.md, Fire; Phase 4). Six spells of
    /// four marks each and ten passives on the pattern of <see cref="BranchPattern"/>: in the middle Pressure (Heat, Uraeus
    /// and Meteor Leap, the lift, White Heat and Devotion), on the left Answer (Breath of the Lioness and Kiln Stance,
    /// Flame Trail and Wildfire Rush), on the right Ruin (the Firebomb Flask, Flame Wave, Catching Fire and Black Pitch).
    /// Flame Wave is the base spell: it comes with Fire, its Learn is never bought, and it grows from the root.
    ///
    /// Marks the design keeps keep their ids (<c>fire.backdraft</c> is Breath of the Lioness now); the cut ones are in
    /// <see cref="RetiredMarks"/>. Heat, Breath of the Lioness, Flame Trail, the Firebomb Flask and White Heat are done by
    /// hand in <see cref="PlayerUpgrades"/> under their ids; the rest are data. What the data cannot say yet is the nearest
    /// working version with a <c>NEEDS</c> line, gathered in docs/design/steel/content_open.tsv.
    /// </summary>
    public static class FireTree
    {
        public const string Cinder = "fire.cinder";
        public const string CatchingFire = "fire.catch";
        public const string WhiteHeat = "fire.whiteheat";
        public const string FuelToIt = "fire.lift";
        public const string FireWithoutEnd = "fire.devotion";
        // The two keystones the owner picked on 2026-11-20 (keystones_fire_water_life.md).
        public const string IntoTheFire = "fire.intothefire";
        public const string BladeDrinksFire = "fire.drinks";
        /// <summary>Fire Without End: a burn left alone deepens by one this often, up to this depth.</summary>
        public const float DevotionDeepensEvery = 4f;
        public const int DevotionDeepensTo = 5;
        // The spells: each is its branch's Learn mark and the branch's id.
        public const string FlameWave = "fire.base";
        public const string Uraeus = "fire.uraeus";
        public const string WildfireRush = "fire.rush";
        public const string KilnStance = "fire.kiln";
        public const string BlackPitch = "fire.pitch";
        // The god's gift.
        public const string FallingSun = "fire.sun";
        // Conditions the branches set on enemies.
        public const string TorchLit = "fire.torch";
        /// <summary>Black Pitch's coat on an enemy, and The Torch's deep coat before it is lit.</summary>
        public const string PitchCoat = "fire.pitch.coat";
        public const string TorchCoat = "fire.torch.coat";

        // Retired in Phase 4 (RetiredMarks): kept as names because hand-written code still asks for them, and finds none.
        public const string OpenFurnace = "fire.furnace";
        public const string ScorchedEarth = "fire.scorched";
        /// <summary>Ash Fall's ash on those the landing caught: their balance does not recover for its five seconds.</summary>
        public const string InTheAsh = "fire.meteor.ash";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Fire;

            to.Add(BranchPattern.Put(new Upgrade { Id = Cinder, Name = "Cinder", Element = E, Kind = UpgradeKind.Root, Icon = UpgradeIcons.Flame,
                Description = "Every blow of the weapon sets a burn, and each blow deepens it, up to five.",
                WeaponStacks = 1 }, BranchPattern.Root));

            // ---- The middle bough: Pressure.
            // BESPOKE (as built): Heat, done by hand in PlayerUpgrades.
            to.Add(BranchPattern.Put(new Upgrade { Id = Upgrade.Heat, Name = "Heat", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Cinder }, Icon = UpgradeIcons.Fist,
                Description = "Blows landed without being struck build Heat, up to five. Each level adds damage. A blow taken puts it out. Kindle fills it at once." },
                BranchPattern.MiddleFoot));

            // Flame Wave, the base spell: cast by Fire's kit, so its Learn carries no form; its shapes cast their own.
            SpellBranch.Add(to, E, FlameWave, "Flame Wave", SpellRole.Opener, BranchPattern.RightA, Cinder, "", null,
                "Fire's own spell, given with the element. Fire runs out along the ground in a cone, 5.5 metres and 70 degrees, and three patches of it burn on for three seconds.",
                UpgradeIcons.Wave,
                // The patches burning 5 s. The Empower's numbers reach the kit's own cast too (PlayerUpgrades.BaseEmpower, read by PlayerElement).
                new Empower { Cooldown = 5.5f, Seconds = 5f },
                "Flame Wave waits 5.5 seconds, not 7, and its patches burn five seconds.",
                new ShapeDef { Name = "Cinder Fan", Form = SpellForms.Wave,
                    Description = "Close and wide: 3 metres and 150 degrees. What it touches burns two deep and is thrown back a metre and a half. The shape for being surrounded.",
                    Spell = new SpellSpec { Reach = 3f, Arc = 150f, Damage = 22f, Stagger = 20f, Stacks = 2 },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 1.5f } } },
                new ShapeDef { Name = "Running Fire", Form = SpellForms.Spikes,
                    Description = "One narrow tongue of flame runs 12 metres along the ground in under a second and stays as a low burning line for four seconds. Whatever crosses it takes two Burn. Lay it across a path, or reach an archer.",
                    Spell = new SpellSpec { Reach = 12f, Speed = 15f, Count = 10, Radius = 0.5f, Damage = 12f, Stagger = 10f, Stacks = 2 },
                    // The line left burning behind the tongue.
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Line, Reach = 12f, Count = 8, Radius = 0.6f, Seconds = 4f, Interval = 0.5f, Damage = 3f, Affliction = AfflictionKind.Burn, Stacks = 2 } } },
                "Opens two shapes of Flame Wave, chosen at any rest fire: Cinder Fan, close and wide, or Running Fire, one long burning line.",
                // NEEDS: the wave leaving from the blade along the swing. Its cooldown not starting is the window below.
                new[] { new UpgradeEffect { On = UpgradeTrigger.ChargedBlowThird, Do = UpgradeAction.InstantNext, Seconds = 0.5f } },
                "Cast Flame Wave in the same instant as the charged blade's third blow lands, and its cooldown does not start.");

            // The cobra form: on the shoulder, it spits at what begins an attack within reach (read off the enemy's wind-up), and
            // the venom burns at its feet for 2 s (CobraForm, CobraRules).
            SpellBranch.Add(to, E, Uraeus, "Uraeus", SpellRole.Opener, BranchPattern.MiddleA, Upgrade.Heat, SpellForms.Cobra,
                new SpellSpec { Count = 6, Seconds = 8f, Reach = 8f, Damage = 8f, Stagger = 6f, Stacks = 2, Speed = 12f },
                "A cobra of flame rises from your shoulder and stands eight seconds. Whatever comes within eight metres to strike, it spits burning venom at: 8 damage and two Burn, six spits at most.",
                UpgradeIcons.Fang,
                new Empower { Cooldown = 8f, Count = 8 },
                "Uraeus waits 8 seconds, not 10, and spits eight times.",
                // NEEDS: the spit holding the enemy's wind-up for 0.3 s.
                new ShapeDef { Name = "Coiled",
                    Description = "It stays coiled close and spits only at what attacks you from within three metres. Each spit lays three Burn and holds the enemy's wind-up a moment, so you have longer to read it.",
                    Change = new SpellChange { Reach = 3f, Stacks = 3 } },
                new ShapeDef { Name = "Set Down", Form = SpellForms.Cobra,
                    Description = "It is planted where you cast it, two metres tall, and guards a circle of nine metres round itself whether you are there or not. Fight beside it, or leave it on a door.",
                    Spell = new SpellSpec { Reach = 0.5f, Radius = 9f, Seconds = 8f, Interval = 1.2f, Damage = 8f, Stagger = 6f, Delay = 0.2f, Stacks = 2, Count = 7, Planted = true } },
                "Opens two shapes of Uraeus, chosen at any rest fire: Coiled, close and holding, or Set Down, a cobra left to guard a place.",
                // And the cobra stands a second longer for each (UpgradeAction.LengthenOwn).
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Burn, Do = UpgradeAction.LengthenOwn, Seconds = 1f },
                },
                "Each spit that lands on an enemy already burning lays twice the Burn: the fire is fed.");

            to.Add(BranchPattern.Put(new Upgrade { Id = FuelToIt, Name = "Fuel to It", Element = E, Kind = UpgradeKind.Major, Lifts = E, Requires = new[] { Upgrade.Heat }, Icon = UpgradeIcons.Burst,
                // Every reaction with Burn in it 25% harder (PlayerStat.ReactionDamage). The ground burning is data.
                Description = "Every reaction with Burn in it strikes 25% harder and leaves the ground where it happened burning for three seconds.",
                Stats = new[] { new StatMod(PlayerStat.ReactionDamage, 0.25f, AfflictionKind.Burn) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = AfflictionKind.Burn, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 1.5f, Seconds = 3f, Interval = 0.5f, Damage = 3f, Affliction = AfflictionKind.Burn, Stacks = 1 } } },
                BranchPattern.MiddleCrown));
            // Built by hand in PlayerUpgrades (hasWhiteHeat).
            to.Add(BranchPattern.Put(new Upgrade { Id = WhiteHeat, Name = "White Heat", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { FuelToIt }, Icon = UpgradeIcons.Star,
                Description = "Heat rises to eight, and above five your blows come faster. Struck above five, you burn.",
                Stats = new[] { new StatMod(PlayerStat.HeatCap, 3f) } }, BranchPattern.TrunkLeft));
            // The RuleBook's affliction rules (task 4.4), read by every enemy's set: no burn goes out by itself, and each one left
            // alone (nothing new laid on it) deepens by one every 4 s, up to five. Only death, a burn set off, or water (Steam
            // takes half) ends one.
            to.Add(BranchPattern.Put(new Upgrade { Id = FireWithoutEnd, Name = "Fire Without End", Element = E, Kind = UpgradeKind.Devotion, Requires = new[] { FuelToIt }, Icon = UpgradeIcons.Sun,
                Description = "All in on Fire only. Burns never go out by themselves; only death or water ends them. A burn left alone deepens by one every four seconds, up to five. Light everything once, then fight the one in front of you while the rest of the room cooks.",
                Rules = book =>
                {
                    book.Afflictions.Keep(AfflictionKind.Burn);
                    book.Afflictions.Deepen(AfflictionKind.Burn, DevotionDeepensEvery, DevotionDeepensTo);
                } },
                BranchPattern.TrunkRight));

            // ---- The left bough: Answer.
            // BESPOKE (as built): the cone of fire, done by hand; Burn and Detonate is folded in (PlayerUpgrades.hasDetonate).
            to.Add(BranchPattern.Put(new Upgrade { Id = Upgrade.Backdraft, Name = "Breath of the Lioness", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Cinder }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard throws a cone of fire back at the one who struck, as Sekhmet's breath was the desert wind. Against a burning enemy, its burn goes off too.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 2 } } },
                BranchPattern.LeftFoot));

            // Shut Door (SpellSpec.HeldStance): the stance held up to 3 s, every blow turned counted as a perfect guard and banked as
            // smoke under a hood (three at most), and the door opened on letting go: a cone of 6 m, 20 damage and 15 for each
            // blow banked, three Burn.
            SpellBranch.Add(to, E, KilnStance, "Kiln Stance", SpellRole.Counter, BranchPattern.LeftA, Upgrade.Backdraft, SpellForms.Parry,
                new SpellSpec { Seconds = 3f, Reach = 6f, Arc = 90f, Damage = 20f, Stagger = 50f, Stacks = 3, HeldStance = true, Bank = 3, BankDamage = 15f },
                "Shut Door: you stand ready in flame. Every blow that reaches you, red or not, is turned and counts as a perfect guard, and the door opens on them in a cone of flame.",
                UpgradeIcons.Tower,
                new Empower { Cooldown = 7f, Stacks = 4 },
                "Kiln Stance waits 7 seconds, not 9, and its answer lays more Burn.",
                new ShapeDef { Name = "The Kiln",
                    Description = "The stance as it was: up to 1.2 seconds, and the first blow turned is answered at once by a pillar of fire under the attacker, 30 damage, 50 stagger and three Burn. For single blows.",
                    Spell = new SpellSpec { Seconds = 1.2f, Reach = 4f, Arc = 90f, Damage = 30f, Stagger = 50f, Stacks = 3 } },
                // Walking at half the pace in the stance (SpellSpec.Walk, read by the parry form).
                new ShapeDef { Name = "Firewalker",
                    Description = "You can walk at half speed in the stance, but it holds only 0.6 seconds before the door opens. Walk into the swing.",
                    Change = new SpellChange { Seconds = 0.6f, Walk = 0.5f } },
                "Opens two shapes of Kiln Stance, chosen at any rest fire: The Kiln, the pillar for single blows, or Firewalker, a stance you walk in.",
                new[]
                {
                    // A pillar under a burning attacker as the answer lands: its burn goes off and it is thrown down.
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Burn, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Burn, Damage = 10f, Radius = 1.5f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Burn, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = "fire.kiln.down", Seconds = 1.2f, Flags = ConditionFlags.Down } },
                },
                "Turn a blow from an attacker already burning, and a pillar of fire rises under it with the answer: its burn goes off, ten a stack, and it is thrown down.");

            // BESPOKE (as built): the line of fire behind the dodge.
            to.Add(BranchPattern.Put(new Upgrade { Id = Upgrade.FlameTrail, Name = "Flame Trail", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Backdraft }, Icon = UpgradeIcons.Boot,
                Description = "The dodge leaves a line of fire behind it that burns what follows." }, BranchPattern.LeftMiddle));

            // The Answer bough's keystone (the owner's pick of 2026-11-20). Built by hand in PlayerUpgrades (DodgeTermsFor,
            // OnDodgeEnd): which way a dodge goes against the enemy faced (dodge-direction). Toward or through it: safe for its
            // whole length, through bodies, and everything passed set burning two deep. Away: twice the stamina and never
            // perfect. The perfect dodge's Heat level is data.
            to.Add(BranchPattern.Put(new Upgrade { Id = IntoTheFire, Name = "Into the Fire", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { Upgrade.FlameTrail }, Icon = UpgradeIcons.Boot,
                Description = "Dodge toward an enemy and you pass right through it, safe for the whole dodge, and set everything you pass on fire, two Burn each. A perfect dodge toward it also raises your Heat a level. Dodging away costs twice the stamina and can never be perfect.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Way = DodgeWay.Toward, Do = UpgradeAction.RaiseHeat, Count = 1 } } },
                BranchPattern.LeftCrown));

            // BESPOKE (built): DrillForm leaves the line burning and raises Heat a level for each enemy passed.
            SpellBranch.Add(to, E, WildfireRush, "Wildfire Rush", SpellRole.Movement, BranchPattern.LeftB, Upgrade.FlameTrail, SpellForms.Drill,
                new SpellSpec { Reach = 7f, Radius = 1f, Count = 1, Damage = 14f, Stagger = 20f, Stacks = 2, Speed = 20f, Seconds = 2.6f, LingerDamage = 4f },
                "You drive seven metres through the enemies ahead in a line of fire and end behind them. Each one passed is set burning two deep and raises your Heat. The line burns on.",
                UpgradeIcons.Arrow,
                new Empower { Cooldown = 6f, Seconds = 4f },
                "Wildfire Rush waits 6 seconds, not 8, and its line burns four seconds.",
                // Every enemy it meets is carried before you and thrown down at the end (SpellSpec.Carries: FormCarry in the drill).
                new ShapeDef { Name = "Fire Front",
                    Description = "You go five metres as the front of a grass fire, a wall of flame four metres wide, slower. Every enemy it meets is carried before you, burning two deep, and thrown down at the end. The drive that pushes a crowd instead of passing through it.",
                    Change = new SpellChange { Reach = 5f, Radius = 2f, Speed = 7f, Carries = true } },
                // NEEDS: the drive going back out of the fight, not on through it.
                new ShapeDef { Name = "Smoke Road",
                    Description = "You go six metres back out of the fight in a roll of black smoke, and the line of fire is left between you and them. The escape.",
                    Change = new SpellChange { Reach = 6f, Damage = 4f } },
                "Opens two shapes of Wildfire Rush, chosen at any rest fire: Fire Front, a wall that carries a crowd, or Smoke Road, the escape.",
                // Started as a blow would land (UpgradeEffect.BlowLanding), it is a perfect dodge (UpgradeAction.PerfectDodgeNow) and Heat
                // rises five levels, to five from nothing.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, BlowLanding = 0.3f, Do = UpgradeAction.PerfectDodgeNow },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, BlowLanding = 0.3f, Do = UpgradeAction.RaiseHeat, Count = 5 },
                },
                "Started in the instant a blow would land on you, it counts as a perfect dodge, and your Heat goes to five.");

            // ---- The right bough: Ruin.
            // BESPOKE (as built): the thrown flask and its burst.
            to.Add(BranchPattern.Put(new Upgrade { Id = Upgrade.FirebombFlask, Name = "Firebomb Flask", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Cinder }, Icon = UpgradeIcons.Flask,
                Description = "Hold the flask button to throw it. No healing: a great burst of fire where it lands, flames left on the ground there, and three stacks of Burn on everything near.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.FlaskThrown, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 3, Radius = 2.6f } } },
                BranchPattern.RightFoot));

            SpellBranch.Add(to, E, Upgrade.MeteorLeap, "Meteor Leap", SpellRole.Breaker, BranchPattern.MiddleB, Upgrade.Heat, SpellForms.Leap,
                new SpellSpec { Reach = 9f, Seconds = 0.62f, Height = 3.4f, Damage = 46f, Stagger = 55f, Radius = 3.2f, LingerDamage = 5f, Stacks = 0, Lasts = 4f },
                "You leap out of the fight and come down on the enemy faced as a falling stone, leaving a burning crater. 45 mana.",
                UpgradeIcons.Comet,
                // The crater burns 6 s, not the Learn's 4 (SpellSpec.Lasts, read by the leap's form).
                new Empower { Mana = 35f, Lasts = 6f },
                "Meteor Leap costs 35 mana, not 45, and its crater burns longer.",
                new ShapeDef { Name = "Falling Star", Form = SpellForms.Bomb,
                    Description = "You do not leap. A burning stone falls on the aim point after a breath, its shadow growing under it, and you go on fighting. It breaks balance harder.",
                    Spell = new SpellSpec { Delay = 0.8f, Reach = 9f, Radius = 3.2f, Damage = 46f, Stagger = 60f, Stacks = 1, Seconds = 4f, LingerDamage = 5f } },
                // Those the landing catches in the ash keep their balance from recovering for its five seconds (ConditionFlags.
                // BalanceHeld); one that walks into the ash later is held too while it stands there (UpgradeEffect.Inside).
                new ShapeDef { Name = "Ash Fall",
                    Description = "The landing throws hot ash over six metres. For five seconds enemies in the ash gain a Burn a second and their balance does not recover.",
                    // The landing's first strike lays the ash where you came down (once a landing).
                    Effects = new[] {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = InTheAsh, Seconds = 5f, Flags = ConditionFlags.BalanceHeld } },
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Patch, Radius = 3f, Seconds = 5f, Interval = 1f, Damage = 2f, Affliction = AfflictionKind.Burn, Stacks = 1, Cooldown = 1f,
                            Inside = new ConditionSpec { Id = InTheAsh, Seconds = 1f, Flags = ConditionFlags.BalanceHeld } } } },
                "Opens two shapes of Meteor Leap, chosen at any rest fire: Falling Star, a stone you call down, or Ash Fall, a landing that leaves hot ash.",
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Burn, NeedsFull = true, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Burn, Damage = 10f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Burn, NeedsFull = true, Do = UpgradeAction.Mana, Amount = 22f, Cooldown = 1f },
                },
                "Land it on an enemy at five Burn and its burn goes off with the blow, ten a stack, and half the mana comes back.");

            to.Add(BranchPattern.Put(new Upgrade { Id = CatchingFire, Name = "Catching Fire", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.FirebombFlask }, Icon = UpgradeIcons.Skull,
                Description = "A burning enemy that dies throws its burn onto everyone near it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1, CarriesDepth = true, Radius = 4f } } },
                BranchPattern.RightMiddle));

            // The Ruin bough's keystone (the owner's pick of 2026-11-20). Built by hand: Fire's root lays nothing with the
            // weapon (PlayerUpgrades.Refresh, root-silenced), burns do no damage by themselves (Afflictions.BurnBiteOff), and
            // each weapon blow on a burning enemy takes Burn off it and strikes 6 harder for each (PlayerCombat.DrinksBurn):
            // one, three for a heavy blow, all of it for a finisher.
            to.Add(BranchPattern.Put(new Upgrade { Id = BladeDrinksFire, Name = "The Blade Drinks Fire", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { CatchingFire }, Icon = UpgradeIcons.Blade,
                Description = "Your weapon no longer sets enemies on fire. It drinks the fire instead. Each blow on a burning enemy takes one Burn off it and hits 6 harder for it. A heavy blow takes three, a finisher takes all. Burns no longer hurt by themselves, so your fire must come from spells, the ground and reactions."
            }, BranchPattern.RightCrown));

            // The pitch is a coat (a condition, 8 s) on the enemy faced; each weapon blow on a coated enemy spreads it to every
            // other within 2 m and counts on the mark's tally; the first Burn laid on any coated enemy lights them all at once
            // (UpgradeAction.LightCoat: 16 and 8 more a spreading blow, 70 at most, 35 stagger, 3 Burn, the floor burning 3 s).
            // The jar is a quiet brand (SpellSpec.Quiet): only the throw's gesture, the coat does the rest.
            SpellBranch.Add(to, E, BlackPitch, "Black Pitch", SpellRole.Detonator, BranchPattern.RightB, CatchingFire, SpellForms.Brand,
                new SpellSpec { Reach = 7f, Seconds = 8f, Count = 3, Radius = 2f, Damage = 0f, Stagger = 0f, Stacks = 0, Quiet = true },
                "You throw a jar of embalmer's pitch at the enemy faced. For eight seconds blows on it splash the pitch over everyone near, and fire takes them all.",
                UpgradeIcons.Orb,
                new Empower { Cooldown = 9f, Seconds = 10f, Effects = new[] { new EffectChange { Index = 0, ConditionSeconds = 10f }, new EffectChange { Index = 1, ConditionSeconds = 10f } } },
                "Black Pitch waits 9 seconds, not 11, and the coat lasts ten seconds.",
                // The pool is ground that coats whoever stands in it at each bite (Inside), lit the same way; unseen until the
                // pitch's look (lk_g_pitch) is in. The Learn's coat on the enemy faced is set aside (SetsAside 0).
                new ShapeDef { Name = "Pitch Pool", Form = SpellForms.Brand, SetsAside = new[] { 0 },
                    Description = "Thrown on the floor: a pool of pitch three metres across for eight seconds that coats whatever walks through it, lit the same way.",
                    Spell = new SpellSpec { Reach = 7f, Damage = 0f, Stagger = 0f, Stacks = 0, Quiet = true },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, Radius = 1.5f, Seconds = 8f, Interval = 0.5f, Damage = 0f, Stagger = 0f, Stacks = 0,
                        Affliction = AfflictionKind.Burn, Unseen = true, Inside = new ConditionSpec { Id = PitchCoat, Seconds = 8f } } } },
                // One enemy coated deep: the coat set aside for the torch's own (SetsAside 0, 1: nothing spreads), lit only when a
                // Burn touches it (AfflictionLaid), then burning 8 s and setting alight all within 2 m each second.
                new ShapeDef { Name = "The Torch", SetsAside = new[] { 0, 1 },
                    Description = "One enemy coated deep, and nothing splashes. Lit, it burns for eight seconds and sets alight everyone within two metres of it each second as it walks.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = TorchCoat, Seconds = 8f } },
                        new UpgradeEffect { On = UpgradeTrigger.AfflictionLaid, Spent = AfflictionKind.Burn, InState = TorchCoat, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = TorchLit, Seconds = 8f, Beat = 1f } },
                        new UpgradeEffect { On = UpgradeTrigger.AfflictionLaid, Spent = AfflictionKind.Burn, InState = TorchCoat, Do = UpgradeAction.EndCondition, Condition = TorchCoat },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = TorchLit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1, Radius = 2f },
                    } },
                "Opens two shapes of Black Pitch, chosen at any rest fire: Pitch Pool on the floor, or The Torch, one enemy that walks burning through its friends.",
                // A finisher on a coated enemy lights the coat as a Burn would (the Learn's numbers), and every other burning enemy within
                // 10 m goes up with it, its Burn spent, at half the coat's own burst (UpgradeEffect.BurningShare).
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, InState = PitchCoat, Do = UpgradeAction.LightCoat, Condition = PitchCoat, Reach = 30f, Radius = 1.2f,
                        Damage = 16f, DamagePerSpent = 8f, Amount = 70f, Stagger = 35f, Affliction = AfflictionKind.Burn, Stacks = 3, Seconds = 3f, Cooldown = 0.2f, BurningShare = 0.5f, BurningWithin = 10f, PaysLearn = true },
                },
                "Light it with a finisher, and every other burning enemy near goes up with the coated, at half.",
                tally: new TallySpec { Name = "Pitch", Cap = 7 },
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = PitchCoat, Seconds = 8f } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = PitchCoat, By = StruckBy.Weapon, Do = UpgradeAction.Condition, EachWithin = 2f, EachOther = true, State = new ConditionSpec { Id = PitchCoat, Seconds = 8f } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = PitchCoat, By = StruckBy.Weapon, Do = UpgradeAction.Bank, Count = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.AfflictionLaid, Spent = AfflictionKind.Burn, InState = PitchCoat, Do = UpgradeAction.LightCoat, Condition = PitchCoat, Reach = 30f, Radius = 1.2f,
                        Damage = 16f, DamagePerSpent = 8f, Amount = 70f, Stagger = 35f, Affliction = AfflictionKind.Burn, Stacks = 3, Seconds = 3f, Cooldown = 0.2f },
                });

            // BESPOKE (built): SunForm.
            to.Add(BranchPattern.Put(new Upgrade { Id = FallingSun, Name = "Falling Sun", Element = E, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "Light Kindle and the sun begins its road. At dawn it is rolled along the ground to the enemy you face, and the ground it crosses burns. At noon it is carried up, and every burn near you deepens by itself. When Kindle ends, or when you press the surge button again, it sets on the enemy below it, heavier for every burn it finds. A blow taken sends it under, to rise again.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.Sun, Spell = new SpellSpec { Cost = 0f, Damage = 60f, Stagger = 80f, Radius = 4f, Stacks = 2, LingerDamage = 5f } } } },
                BranchPattern.Gift));
        }
    }
}
