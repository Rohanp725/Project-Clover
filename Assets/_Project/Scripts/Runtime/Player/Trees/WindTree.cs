using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Wind: fighting in motion, written as spell branches (docs/design/steel/elements_a.md, Wind; Phase 4). Six spells of
    /// four marks each and ten passives on the pattern of <see cref="BranchPattern"/>: in the middle Updraft (Rising Cut,
    /// Lifting Wind and Sky Dance, the lift Carried Far, Nothing Beneath and Devotion above it), on the left Slipstream
    /// (Passing Cut and Closing Circle, Turned Aside and the Bag of Winds, Leaf on the Wind at the crown), on the right Gale
    /// (Against the Wall, Gust and Indrawn Breath). Gust is the base spell: it comes with Wind, its Learn is never bought,
    /// and it grows from the root.
    ///
    /// Marks the design keeps keep their ids; the cut ones are in <see cref="RetiredMarks"/>, and Drawing Wind waits for the
    /// Katana's Way there. Leaf on the Wind is done by hand in <see cref="PlayerUpgrades"/> under its id; the rest are data.
    /// What the data cannot say yet is the nearest working version with a <c>NEEDS</c> line, gathered in
    /// docs/design/steel/content_open.tsv.
    /// </summary>
    public static class WindTree
    {
        public const string Unsteady = "wind.unsteady";
        public const string RisingCut = "wind.rising";
        public const string PassingCut = "wind.passing";
        public const string TurnedAside = "wind.turned";
        public const string AgainstTheWall = "wind.wall";
        /// <summary>Kept as a mark by the owner's answer of 2026-11-20, under its old id.</summary>
        public const string HammerDown = "wind.hammer";
        public const string LeafOnTheWind = "wind.leaf";
        public const string NothingBeneath = "wind.nothing";
        public const string CarriedFar = "wind.lift";
        public const string OnTheWind = "wind.devotion";
        // The spells: each is its branch's Learn mark and the branch's id.
        public const string Gust = "wind.base";
        public const string LiftingWind = "wind.lifting";
        public const string SkyDance = "wind.skydance";
        public const string ClosingCircle = "wind.circle";
        public const string BagOfWinds = "wind.bag";
        public const string IndrawnBreath = "wind.indrawn";
        // The god's gift.
        public const string TheWindRemembers = "wind.remembers";
        // Conditions the branches set on enemies.
        public const string TurnedAbout = "wind.lifting.about";

        // Retired in Phase 4 (RetiredMarks): kept as a name because code still asks for it.
        public const string StormFront = "wind.front";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Wind;

            // The words say five; Unsteady is three deep at most in Afflictions today.
            to.Add(BranchPattern.Put(new Upgrade { Id = Unsteady, Name = "Unsteady", Element = E, Kind = UpgradeKind.Root, Icon = UpgradeIcons.Gust,
                Description = "Blows knock an enemy Unsteady, up to five. It is pushed further, turns slower and loses its balance sooner.",
                WeaponStacks = 1 }, BranchPattern.Root));

            // ---- The middle bough: Updraft. Launch and juggle.
            to.Add(BranchPattern.Put(new Upgrade { Id = RisingCut, Name = "Rising Cut", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Unsteady }, Icon = UpgradeIcons.Arrow,
                Description = "A heavy blow on an enemy five deep in Unsteady throws it into the air. It hangs there helpless, and each blow holds it up a little longer, up to three.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Unsteady, NeedsFull = true, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Unsteady, Damage = 0f, Stagger = 0f, Radius = 1f, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Unsteady, NeedsFull = true, Do = UpgradeAction.Lift, Seconds = 1f, Cooldown = 0.5f },
                } },
                BranchPattern.MiddleFoot));

            // NEEDS: the column hanging its enemy 1.0 s (1.3 s with the Empower); the pillars' lift is the form's own 0.8 s.
            SpellBranch.Add(to, E, LiftingWind, "Lifting Wind", SpellRole.Breaker, BranchPattern.MiddleA, RisingCut, SpellForms.Pillars,
                new SpellSpec { Count = 1, Reach = 5f, Radius = 2f, Damage = 8f, Stagger = 40f, Stacks = 1, Delay = 0.35f },
                "A circle is drawn under the enemy faced, up to five metres away, and a column of wind rises from it: 8 damage and 40 stagger, and it goes into the air whatever its footing.",
                UpgradeIcons.Tower,
                new Empower { Cooldown = 7f, Seconds = 1.3f },
                "Lifting Wind waits 7 seconds, not 9, and its enemy hangs longer.",
                // NEEDS: one column that wanders after enemies for 3 s. Until then two columns rise, under the two nearest.
                new ShapeDef { Name = "Dust Devil",
                    Description = "The column wanders after enemies for three seconds, lifting the first it reaches and then the next.",
                    Change = new SpellChange { Count = 2, Delay = 0.6f } },
                // NEEDS: the column not lifting, the enemy swung round to face away (0.5 s for an elite; a god's next attack
                // swings wide). Until then it is lifted as well, and for 1.2 s it begins nothing and the first blow on it is critical.
                new ShapeDef { Name = "Turned About",
                    Description = "The column does not lift; it spins. The enemy is swung round to face away from you for a moment and loses its balance, and your first blow on its back is critical. For what cannot be lifted.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = TurnedAbout, Seconds = 1.2f, Flags = ConditionFlags.BeginsNoAttack | ConditionFlags.TakesCritical, EndsWith = ConditionBlow.Weapon } } } },
                "Opens two shapes of Lifting Wind, chosen at any rest fire: Dust Devil, a column that wanders, or Turned About, a column that spins its enemy round.",
                // NEEDS: only on the enemy a perfect dodge has just taken you past. Until then a perfect dodge's next column
                // starts no wait, and what it takes hangs twice as long.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.InstantNext, Seconds = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Lift, Seconds = 1.6f, Cooldown = 1f },
                },
                "Cast it on the enemy a perfect dodge has just taken you past, and it hangs twice as long.");

            // BESPOKE (built): SkyDanceForm.
            SpellBranch.Add(to, E, SkyDance, "Sky Dance", SpellRole.Breaker, BranchPattern.MiddleB, RisingCut, SpellForms.SkyDance,
                new SpellSpec { Count = 5, Interval = 0.34f, Damage = 10f, Stagger = 60f, Stacks = 1, Reach = 6f, Radius = 3f, LingerDamage = 6f },
                "You throw the enemy faced into the air and go up beside it. Press the light attack as each of five rings closes for a harder cut. Then the stick chooses where it is thrown down, and it bursts harder for each cut in time. 45 mana.",
                UpgradeIcons.Wing,
                new Empower { Mana = 35f, Count = 6 },
                "Sky Dance costs 35 mana, not 45, and closes six rings.",
                // NEEDS: no throw: the enemy carried down the lane and dropped on another, both taking the burst. Until then
                // it is thrown further.
                new ShapeDef { Name = "Carried Off",
                    Description = "There is no throw. You carry it down the lane with you, up to eight metres, and drop it on another enemy, and both take the burst.",
                    Change = new SpellChange { Reach = 8f } },
                // NEEDS: the nearest other enemy taken up beside it, the rings taking turns between them. Until then the first
                // cut throws up whatever stands near.
                new ShapeDef { Name = "Two Rise",
                    Description = "The enemy faced and the nearest other both go up, and the rings take turns between them.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Lift, Radius = 3f, Seconds = 1.5f, Cooldown = 3f } } },
                "Opens two shapes of Sky Dance, chosen at any rest fire: Carried Off, an enemy dropped on another, or Two Rise, two dancing at once.",
                // NEEDS: only when all five cuts were in time, and Tailwind itself lit for 4 s. Until then the dance gives
                // Tailwind's longer, safer dodge and quicker feet for 4 s.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Boon, Stat = PlayerStat.DodgeDistance, Amount = 0.3f, Seconds = 4f, Cooldown = 4f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Boon, Stat = PlayerStat.DodgeSafeTime, Amount = 0.25f, Seconds = 4f, Cooldown = 4f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Boon, Stat = PlayerStat.MoveSpeed, Amount = 0.15f, Seconds = 4f, Cooldown = 4f },
                },
                "Land all five cuts in time, and you come down with Tailwind lit for four seconds at no cost.");

            // Every reaction with Unsteady in it reaches half again as far (PlayerStat.ReactionReach). The throw is data.
            to.Add(BranchPattern.Put(new Upgrade { Id = CarriedFar, Name = "Carried Far", Element = E, Kind = UpgradeKind.Major, Lifts = E, Requires = new[] { RisingCut }, Icon = UpgradeIcons.Gust,
                Description = "Every reaction with Unsteady in it reaches half again as far and throws what it strikes a metre and a half out from where it happened.",
                Stats = new[] { new StatMod(PlayerStat.ReactionReach, 0.5f, AfflictionKind.Unsteady) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = AfflictionKind.Unsteady, Do = UpgradeAction.Push, Radius = 2.5f, Amount = 1.5f } } },
                BranchPattern.MiddleCrown));
            to.Add(BranchPattern.Put(new Upgrade { Id = NothingBeneath, Name = "Nothing Beneath", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { CarriedFar }, Icon = UpgradeIcons.Spiral,
                Description = "An enemy in the air stays there for as long as you keep striking it, and each blow up there gives double mana. Your blows on an enemy that is not Unsteady are weaker.",
                Stats = new[] { new StatMod(PlayerStat.AirBlows, 20f), new StatMod(PlayerStat.ManaFromBlows, 1f, StatWhen.AirborneBlow), new StatMod(PlayerStat.WeaponDamage, -0.15f), new StatMod(PlayerStat.WeaponDamage, 0.15f, AfflictionKind.Unsteady) } },
                BranchPattern.TrunkLeft));
            // NEEDS: spells with no wind-up, cast in the middle of a dodge or a swing without ending it, and a spell cast in a
            // dodge carrying it 2 m further (cast-in-motion: a RuleBook switch that PlayerController's order and PlayerSpells
            // would read, and casts that leave the dodge and the swing running, which only play can prove). Until then every
            // dodge carries further.
            to.Add(BranchPattern.Put(new Upgrade { Id = OnTheWind, Name = "On the Wind", Element = E, Kind = UpgradeKind.Devotion, Requires = new[] { CarriedFar }, Icon = UpgradeIcons.Wing,
                Description = "All in on Wind only. Your spells have no wind-up, and can be cast in the middle of a dodge or a swing without ending it. A spell cast in a dodge carries you two metres further along it. Nearly all of the fight is spent in motion.",
                Stats = new[] { new StatMod(PlayerStat.DodgeDistance, 0.3f) } },
                BranchPattern.TrunkRight));

            // ---- The left bough: Slipstream. The dodge and the guard as weapons.
            // BESPOKE: the dodge should pass through enemies, cut each one passed, and leave it unable to turn for a moment.
            // Until then the end of every dodge cuts whatever stands close by.
            to.Add(BranchPattern.Put(new Upgrade { Id = PassingCut, Name = "Passing Cut", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Unsteady }, Icon = UpgradeIcons.Blade,
                Description = "Where a dodge ends, a cut of wind strikes everything close to you and leaves it Unsteady.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Do = UpgradeAction.Burst, At = EffectAt.Self, Radius = 2.2f, Damage = 10f, Stagger = 8f, Affliction = AfflictionKind.Unsteady, Stacks = 1, Cooldown = 0.5f } } },
                BranchPattern.LeftFoot));

            // BESPOKE (built): CircleForm. The player cannot be touched for the whole run; the design asks for the first half only.
            SpellBranch.Add(to, E, ClosingCircle, "Closing Circle", SpellRole.Movement, BranchPattern.LeftA, PassingCut, SpellForms.Circle,
                new SpellSpec { Reach = 5f, Radius = 3f, Seconds = 1.2f, Damage = 6f, Stagger = 10f, Stacks = 5 },
                "You run a ring round the enemy faced. When the ring closes, the wind inside it rises and takes everything off the ground, five deep in Unsteady.",
                UpgradeIcons.Ring,
                new Empower { Cooldown = 7f, Seconds = 1.5f },
                "Closing Circle waits 7 seconds, not 9, and what it lifts hangs longer.",
                // NEEDS: half the ring run, ending on the enemy's far side. Until then a step through it, and the first blow
                // after is critical.
                new ShapeDef { Name = "Half Circle", Form = SpellForms.Step,
                    Description = "You run only half the ring and end on its far side, behind it. Nothing is lifted, and your first blow on its back is critical.",
                    Spell = new SpellSpec { Reach = 5f, Damage = 6f, Stagger = 10f, Stacks = 1, Seconds = 1.5f },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Boon, Stat = PlayerStat.CritChance, Amount = 1f, Seconds = 1.5f, UntilBlow = true } } },
                // NEEDS: the Empower's longer hang reaching this shape too (an Empower sets its numbers after the shape's).
                new ShapeDef { Name = "Wide Circle",
                    Description = "You run round a group up to six metres across. The column takes everyone inside, but lifts each for only a moment.",
                    Change = new SpellChange { Reach = 7f, Radius = 4f, Seconds = 0.6f } },
                "Opens two shapes of Closing Circle, chosen at any rest fire: Half Circle, ending behind it, or Wide Circle, a ring round a group.",
                // Started as a blow would land (UpgradeEffect.BlowLanding), it is a perfect dodge (UpgradeAction.PerfectDodgeNow). NEEDS: the
                // ring run round the attacker wherever it stands within 8 m (form-pieces).
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, BlowLanding = 0.3f, Do = UpgradeAction.PerfectDodgeNow },
                },
                "Started in the instant a blow would land on you, it counts as a perfect dodge, and the ring is run round the attacker wherever it stands within eight metres.");

            // A thrown attack sent back to its thrower is still to be written by hand.
            to.Add(BranchPattern.Put(new Upgrade { Id = TurnedAside, Name = "Turned Aside", Element = E, Kind = UpgradeKind.Major, Requires = new[] { PassingCut }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard throws everyone near you back and leaves the attacker three deeper in Unsteady.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 3 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Push, At = EffectAt.Self, Radius = 4f, Amount = 3f },
                } },
                BranchPattern.LeftMiddle));

            // NEEDS: the bag as designed: open 8 s, each perfect guard or perfect dodge in that time catching the attacker's
            // wind (thrown back 2 m), thrown things within 2 m caught too, and a second press or the time's end opening it as
            // one cone of 7 m, 12 damage and 25 stagger for each wind held, the thrown things flying back. Until then the cast
            // unties a bag of three winds, and each perfect guard or perfect dodge lets one out at once as that cone.
            SpellBranch.Add(to, E, BagOfWinds, "Bag of Winds", SpellRole.Counter, BranchPattern.LeftB, TurnedAside, SpellForms.Nova,
                new SpellSpec { Radius = 1.5f, Damage = 0f, Stagger = 0f, Stacks = 0 },
                "You untie a bag of wind at your hip. Each blow you perfect-guard or perfect-dodge is caught in it, three at most, and thrown things that come near are caught too. Press again, and it opens: a cone of seven metres that strikes harder for each wind held, and every thrown thing flies back at its thrower.",
                UpgradeIcons.Orb,
                // NEEDS: a fourth wind in the bag (the count's cap is the Learn's).
                new Empower { Cooldown = 8f },
                "Bag of Winds waits 8 seconds, not 10, and holds four winds.",
                // NEEDS: the bag kept 20 s, opened only when you choose. Until then it is the bag as above.
                new ShapeDef { Name = "Kept Wind",
                    Description = "The bag does not open by itself. It keeps its winds for twenty seconds, to be opened when you choose, such as on a staggered enemy." },
                // NEEDS: the gust thrown toward the enemy you next face, not the one caught.
                new ShapeDef { Name = "Loose Neck",
                    Description = "Nothing is kept. Each wind leaves at once as a free Gust toward the enemy you next face.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Cone, Reach = 5f, Arc = 70f, Damage = 8f, Stagger = 20f, Affliction = AfflictionKind.Unsteady, Stacks = 2 },
                        new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 3f },
                        new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Cone, Reach = 5f, Arc = 70f, Damage = 8f, Stagger = 20f, Affliction = AfflictionKind.Unsteady, Stacks = 2 },
                    } },
                "Opens two shapes of Bag of Winds, chosen at any rest fire: Kept Wind, a bag opened when you choose, or Loose Neck, every wind let out at once.",
                // NEEDS: only when three winds are caught within 3 s. Until then a perfect dodge raises the whirlwind, once in ten seconds.
                new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Lift, At = EffectAt.Self, Radius = 6f, Seconds = 1f, Cooldown = 10f } },
                "Catch three winds within three seconds, and the bag bursts by itself as a whirlwind: everything within six metres goes into the air.",
                tally: new TallySpec { Name = "Winds", Cap = 3 },
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Bank, Count = 3 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Spends = 1, Do = UpgradeAction.Cone, Reach = 7f, Arc = 60f, Damage = 12f, Stagger = 25f, Affliction = AfflictionKind.Unsteady, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Spends = 1, Do = UpgradeAction.Cone, Reach = 7f, Arc = 60f, Damage = 12f, Stagger = 25f, Affliction = AfflictionKind.Unsteady, Stacks = 1 },
                });

            // Built by hand (DodgeRoll.PayInstead, PlayerUpgrades.SpendBreath): three breaths kept as the mark's count, one back every
            // two seconds and one for every three blows landed. With none, no dodge.
            to.Add(BranchPattern.Put(new Upgrade { Id = LeafOnTheWind, Name = "Leaf on the Wind", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { TurnedAside }, Icon = UpgradeIcons.Leaf,
                Description = "The dodge costs no stamina. It spends a breath of wind instead: you hold three, and time and blows win them back. With none you cannot dodge. You are easier to wound.",
                Tally = new TallySpec { Name = "Breath", Cap = 3 },
                Stats = new[] { new StatMod(PlayerStat.DamageTaken, 0.2f) } },
                BranchPattern.LeftCrown));

            // ---- The right bough: Gale. Push and throw.
            // NEEDS: a thrown enemy meeting a wall, a pillar or another enemy (the wall collision): 15 damage and 6 more for each
            // Unsteady, 30 stagger, both bodies when two meet. Until then every enemy a spell strikes is shaken where it
            // stands, and so is whoever is beside it.
            to.Add(BranchPattern.Put(new Upgrade { Id = AgainstTheWall, Name = "Against the Wall", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Unsteady }, Icon = UpgradeIcons.Wall,
                Description = "Any enemy you throw that meets a wall, a pillar or another enemy takes 15 damage, 6 more for each Unsteady it carries, and 30 stagger. When two bodies meet, both do.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Burst, Damage = 6f, Stagger = 15f, Radius = 1.6f, Affliction = AfflictionKind.Unsteady, Stacks = 1 } } },
                BranchPattern.RightFoot));

            // Gust, the base spell: cast by Wind's kit, so its Learn carries no form; its shapes cast their own.
            SpellBranch.Add(to, E, Gust, "Gust", SpellRole.Opener, BranchPattern.RightA, Unsteady, "", null,
                "Wind's own spell, given with the element. A rush of air five metres ahead: 8 damage, 20 stagger and Unsteady two deep, and everything in it is thrown three metres straight down the wind.",
                UpgradeIcons.Gust,
                // NEEDS: Unsteady three deep. The base spell is cast by its kit, which takes only the Empower's wait.
                new Empower { Cooldown = 4.5f, Stacks = 3 },
                "Gust waits 4.5 seconds, not 6, and leaves its enemies three deep in Unsteady.",
                // NEEDS: the wind coming down on a point up to 6 m away. Until then it comes down on you, and throws everything
                // in 3 m outward.
                new ShapeDef { Name = "Downburst", Form = SpellForms.Nova,
                    Description = "The wind comes down from above and spreads as a ring. Everything within three metres is thrown two and a half metres outward. Aim it at your own feet to clear a ring round you.",
                    Spell = new SpellSpec { Radius = 3f, Delay = 0.15f, Damage = 8f, Stagger = 20f, Stacks = 2 },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 2.5f } } },
                new ShapeDef { Name = "Long Gust", Form = SpellForms.Spikes,
                    Description = "A narrow lane two metres wide and ten long that throws what it takes five metres. For throwing one enemy into a wall, a pit or its friends.",
                    Spell = new SpellSpec { Reach = 10f, Speed = 25f, Count = 1, Radius = 1f, Damage = 8f, Stagger = 20f, Stacks = 2 },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 5f } } },
                "Opens two shapes of Gust, chosen at any rest fire: Downburst, a ring thrown outward, or Long Gust, one long lane.",
                // NEEDS: only in the instant an enemy's attack mark shows (enemy-begins-attack), elites' blows coming 0.4 s late
                // and gods not turned. Until then each enemy it strikes flinches out of what it had begun.
                new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Flinch, Seconds = 0.4f } },
                "Cast it in the instant an enemy's attack mark shows, and the gust turns its blow aside into empty air.");

            // Kept as a mark (the owner's answer of 2026-11-20), in the right bough's empty middle place, from Against the
            // Wall. A heavy blow on an enemy in the air drives it into the ground and the landing bursts; the heavy blow on it
            // strikes half again as hard (StatWhen.AirborneHeavyBlow).
            to.Add(BranchPattern.Put(new Upgrade { Id = HammerDown, Name = "Hammer Down", Element = E, Kind = UpgradeKind.Major, Requires = new[] { AgainstTheWall }, Icon = UpgradeIcons.Hammer,
                Description = "A heavy blow on an enemy in the air hits half again as hard and drives it into the ground. Where it lands, the ground bursts and throws back everyone near.",
                Stats = new[] { new StatMod(PlayerStat.WeaponDamage, 0.5f, StatWhen.AirborneHeavyBlow) },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, NeedsAirborne = true, Do = UpgradeAction.Drop },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, NeedsAirborne = true, Do = UpgradeAction.Burst, Damage = 14f, Stagger = 30f, Radius = 3f },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, NeedsAirborne = true, Do = UpgradeAction.Push, Radius = 3f, Amount = 2f },
                } },
                BranchPattern.RightMiddle));

            // Re-aimed as the detonator: the burst spends each enemy's Unsteady. It grows from Hammer Down in the bough's middle
            // place, as the pattern grows its stalk (a line from the foot would run over Hammer Down's mark).
            SpellBranch.Add(to, E, IndrawnBreath, "Indrawn Breath", SpellRole.Detonator, BranchPattern.RightB, HammerDown, SpellForms.Reverse,
                new SpellSpec { Reach = 2f, Radius = 7f, Delay = 0.5f, Damage = 10f, Stagger = 25f, Stacks = 2 },
                "You drag everything near into one place before you, then burst it apart. The burst spends each enemy's Unsteady, harder for every stack.",
                UpgradeIcons.Spiral,
                new Empower { Cooldown = 9f, Radius = 8.5f },
                "Indrawn Breath waits 9 seconds, not 11, and draws from further.",
                // NEEDS: the held press (the draw lasting while held) and the point walking with you. Until then it is the full hold.
                new ShapeDef { Name = "Held Breath",
                    Description = "The draw lasts as long as the button is held, up to two seconds, and the point walks with you. Let go to burst.",
                    Change = new SpellChange { Delay = 2f } },
                // NEEDS: the burst not scattering them first.
                new ShapeDef { Name = "All One Way",
                    Description = "The burst does not scatter them. All of them are thrown together five metres the way you face. Gather a crowd, then throw it into a wall.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 5f } } },
                "Opens two shapes of Indrawn Breath, chosen at any rest fire: Held Breath, a draw that walks with you, or All One Way, a crowd thrown together.",
                // NEEDS: a spell cast in the middle of a dodge (spell-windows), the point riding with you, and the burst
                // spending nothing. Until then each enemy it strikes is given two Unsteady back.
                new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 2 } },
                "Cast it in the middle of a dodge: the draw's point rides with you to where the dodge ends, and the burst spends no Unsteady, so every stack stays for the next blow.",
                learnEffects: new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Unsteady, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Unsteady, Damage = 6f, Stagger = 8f, Radius = 0.8f } });

            // BESPOKE (built): RememberForm.
            to.Add(BranchPattern.Put(new Upgrade { Id = TheWindRemembers, Name = "The Wind Remembers", Element = E, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "While Tailwind lasts, every blow you land hangs in the air where it fell, up to sixteen. When Tailwind ends, all of them fall again at once, each half as hard as the blow it remembers.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.Remember, Spell = new SpellSpec { Cost = 0f, Damage = 50f, Stagger = 10f, Stacks = 1 } } } },
                BranchPattern.Gift));
        }
    }
}
