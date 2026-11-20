using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Earth: build stone and break it over the enemy, written as spell branches (docs/design/steel/elements_b.md, Earth;
    /// Phase 4). Six spells of four marks each and nine passives on the pattern of <see cref="BranchPattern"/>: in the
    /// middle Weight (Heavy Hand, Under the Mound and Cairn, The Slow Hand at the crown), on the left Standing (Rooted,
    /// Quake Step and Mountain Stance, Immovable at the crown, grown from the stance), on the right Ground (Standing Stones,
    /// Stone Rise, the lift Left in Rubble, Fissure, and the Devotion Stone Remembers at the crown), and the third keystone,
    /// The Closed Ring, at the top of the trunk grown from Cairn's Master (the owner allowed three keystones, 2026-11-20). Stone Rise is the base
    /// spell: it comes with Earth, its Learn is never bought, and it grows from the root.
    ///
    /// Marks the design keeps keep their ids; the cut ones are in <see cref="RetiredMarks"/> (Stone Skin lives on as a
    /// shape of Mountain Stance, and Mountain Cleaver as Earth's all-in infused strike in <see cref="ElementSteel"/>). Heavy
    /// Hand is done by hand in <see cref="PlayerUpgrades"/> under its id; the rest are data. What the data cannot say yet is
    /// the nearest working version with a <c>NEEDS</c> line, gathered in docs/design/steel/content_open.tsv.
    /// </summary>
    public static class EarthTree
    {
        public const string Root = "earth.tremor";
        public const string Rooted = "earth.rooted";
        public const string HeavyHand = "earth.heavyhand";
        public const string StandingStones = "earth.standingstones";
        public const string Immovable = "earth.immovable";
        public const string TheSlowHand = "earth.slowhand";
        public const string TheClosedRing = "earth.closedring";
        public const string LeftInRubble = "earth.lift";
        public const string StoneRemembers = "earth.devotion";
        // The spells: each is its branch's Learn mark and the branch's id.
        public const string StoneRise = "earth.base";
        public const string Fissure = "earth.fissure";
        public const string QuakeStep = "earth.quakestep";
        public const string MountainStance = "earth.stance";
        public const string UnderTheMound = "earth.mound";
        public const string Cairn = "earth.cairn";
        // The god's gift.
        public const string HandOfTheMountain = "earth.hand";
        // Conditions the branches set on enemies.
        public const string Raised = "earth.base.raised";
        public const string Boulder = "earth.base.boulder";
        public const string Glowing = "earth.fissure.glow";
        public const string Basalt = "earth.fissure.basalt";
        public const string OnTheFault = "earth.fissure.fault";
        public const string ThrownDown = "earth.quakestep.down";
        public const string Jarred = "earth.stance.jarred";
        public const string Heaped = "earth.cairn.heaped";
        public const string Unbroken = "earth.cairn.unbroken";

        // Retired in Phase 4 (RetiredMarks): kept as a name because code still asks for it.
        public const string NearToHand = "earth.nearathand";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Earth;
            const AfflictionKind T = AfflictionKind.Tremor;

            // The design lays Tremor with heavy blows only; a root lays its affliction with every blow, as the schema asks.
            // NEEDS: Earth's base buff from GAME.md, afflictions on you lasting half as long (afflictions-on-player).
            to.Add(BranchPattern.Put(new Upgrade { Id = Root, Name = "Tremor", Element = E, Kind = UpgradeKind.Root, Icon = UpgradeIcons.Rock,
                Description = "Heavy blows and finishers shake an enemy's footing, up to three deep. A shaken enemy is slow, and its balance does not come back.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = T, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Afflict, Affliction = T, Stacks = 3 } } },
                BranchPattern.Root));

            // ---- The middle bough: Weight. Slow, certain blows, and the stones that fall with them.
            // Built (PlayerCombat.HeavyUnbroken, StatWhen.HeavySwing): nothing interrupts a heavy blow being wound up or swung, and blows taken then do a third less.
            to.Add(BranchPattern.Put(new Upgrade { Id = HeavyHand, Name = "Heavy Hand", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Hammer,
                Description = "Your heavy blows are not stopped by being struck. You take less from that blow, and yours still lands.",
                Stats = new[] { new StatMod(PlayerStat.DamageTaken, -0.35f, StatWhen.HeavySwing) } },
                BranchPattern.MiddleFoot));

            // The movement spell: the step form carries you through the enemy faced and strikes as you come out. NEEDS: going
            // under the ground along the stick, untouchable, and the ring of sod 2.5 m round where you come up (form-pieces).
            SpellBranch.Add(to, E, UnderTheMound, "Under the Mound", SpellRole.Movement, BranchPattern.MiddleA, HeavyHand, SpellForms.Step,
                new SpellSpec { Reach = 8f, Damage = 12f, Stagger = 25f, Stacks = 1 },
                "You sink into the ground and travel as a hump of heaving turf up to eight metres along the stick, and nothing can touch you. You burst out in a ring of thrown sod and stones two and a half metres round: 12 damage, 25 stagger and Tremor.",
                UpgradeIcons.Arrow,
                new Empower { Cooldown = 8f, Reach = 10f },
                "Under the Mound waits 8 seconds, not 9, and carries you ten metres.",
                // NEEDS: the bank blocking bodies (standing-things), and enemies on the line thrown off it as it rises. Until
                // then the bank is ground along your path that bites and shakes what stands on it.
                new ShapeDef { Name = "Ditch and Bank",
                    Description = "The turf you heave stays up behind you as a bank a metre high along your whole path for five seconds. Nothing walks through it, and enemies standing on the line as it rises are thrown off it, shaken. You draw a wall as you go.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Line, Reach = 8f, Count = 6, Radius = 0.6f, Seconds = 5f, Interval = 1f, Damage = 4f, Stagger = 10f, Affliction = T, Stacks = 1 } } },
                // NEEDS: held under for up to 1.5 s and steered (held-press). Until then what it comes up under is thrown up.
                new ShapeDef { Name = "Barrow Road",
                    Description = "Hold to stay under for up to a second and a half and steer. Come up under an enemy, and it is thrown up as by Stone Rise, and you stand behind it.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Lift, Seconds = 0.6f } } },
                "Opens two shapes of Under the Mound, chosen at any rest fire: Ditch and Bank, a wall drawn along your path, or Barrow Road, steered under the ground to come up beneath an enemy.",
                // NEEDS: the shared movement rule's timing (started as a blow would land, their-blow-moment), and the next blow
                // landing as a heavy blow. Until then every cast gives what a perfect dodge gives, and the next blow that lands
                // breaks half again as much balance and lays one Tremor more.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Mana, Amount = 12f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.CutCooldowns, Amount = 1.5f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Boon, Stat = PlayerStat.StaggerDealt, Amount = 0.5f, Seconds = 3f, UntilBlow = true },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Boon, Stat = PlayerStat.AfflictionStacks, Amount = 1f, Seconds = 3f, UntilBlow = true },
                },
                "Go under in the instant a blow would land on you, as a perfect dodge, and you come up at once behind the attacker. Your next blow lands as a heavy blow, with two Tremor, and breaks half again as much balance.");

            // The larger spell: the stamp is a nova that strikes for nothing; each shaken enemy it reaches has a stone fall on
            // it for each depth of Tremor, is heaped and slowed, and its Tremor is spent. The count comes first and the spend
            // last, so the stones are counted before the Tremor goes. NEEDS: the slow as 15% a stone (count-from-depth).
            SpellBranch.Add(to, E, Cairn, "Cairn", SpellRole.Detonator, BranchPattern.MiddleB, HeavyHand, SpellForms.Nova,
                new SpellSpec { Radius = 10f, Delay = 0.3f, Damage = 0f, Stagger = 0f, Stacks = 0 },
                "You stamp the weapon's butt, and stones fall from the Cailleach's apron onto every enemy within ten metres that carries Tremor, one stone for each depth. Each stone lands for 12 damage and 15 stagger, and the heap for 10 more. The Tremor is spent, and the stones stay heaped on it for four seconds, each slowing it 15%. 40 mana.",
                UpgradeIcons.Comet,
                // NEEDS: the stones landing for 14 (an Empower that changes its Learn's effects, empower-effects).
                new Empower { Mana = 35f },
                "Cairn costs 35 mana, not 40, and each stone lands for 14.",
                // NEEDS: the stones landing soft and waiting, so nothing falls at the cast (shape-sets-aside), and the burst as 18
                // a stone (count-from-depth). Until then the stones still fall, and the heavy blow bursts the heap for 36.
                new ShapeDef { Name = "Unbroken Cairn",
                    Description = "The stones land soft and wait. Your next heavy blow on a heaped enemy breaks its cairn outward: 18 damage a stone to everything within two and a half metres. The burst waits for your blow.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = T, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Unbroken, Seconds = 4f, EndsWith = ConditionBlow.Heavy } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Unbroken, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 2.5f, Damage = 36f, Stagger = 20f, Stacks = 0 },
                    } },
                // NEEDS: the heap flung in place of staying (shape-sets-aside), a stone for each depth (count-from-depth). Until
                // then the stones still fall and heap, and two stones' worth is flung on.
                new ShapeDef { Name = "Thrown Stones",
                    Description = "The stones do not stay. Each enemy's heap is flung at once at the nearest other enemy within five metres: 12 damage and Tremor a stone. One shaken enemy shakes the next.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = T, Do = UpgradeAction.Arc, FromTarget = true, Count = 1, Reach = 5f, Damage = 24f, Stagger = 10f, Affliction = T, Stacks = 2 } } },
                "Opens two shapes of Cairn, chosen at any rest fire: Unbroken Cairn, heaps your heavy blow bursts, or Thrown Stones, heaps flung on to the next enemy.",
                // NEEDS: still for 1 s, not only Rooted (stood-still), and every stone striking twice (count-from-depth). Until
                // then, cast while Rooted, half the mana comes back and a second fall of two stones' worth lands on each.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, While = StatWhen.Planted, Do = UpgradeAction.Mana, Amount = 20f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, While = StatWhen.Planted, Needs = T, Do = UpgradeAction.Falling, Radius = 0.8f, Delay = 0.8f, Damage = 24f, Stagger = 30f, Stacks = 0 },
                },
                "Cast it after standing still for a second, and every stone strikes twice and half the mana comes back.",
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = T, Do = UpgradeAction.Spend, Affliction = T, CountOnly = true },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = T, Do = UpgradeAction.Falling, PerStack = true, Radius = 0.8f, Delay = 0.4f, Damage = 12f, Stagger = 15f, Stacks = 0 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = T, Do = UpgradeAction.Burst, Unseen = true, Radius = 0.6f, Delay = 0.4f, Damage = 10f, Stagger = 0f, Stacks = 0 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = T, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Heaped, Seconds = 4f, Pace = 0.7f } },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = T, Do = UpgradeAction.Spend, Affliction = T },
                });

            // BESPOKE in part: every blow is slower and dearer and light blows far stronger, as designed. Light blows do not yet count as heavy: no guard is passed, no flinch forced, and heavy-blow triggers do not fire.
            to.Add(BranchPattern.Put(new Upgrade { Id = TheSlowHand, Name = "The Slow Hand", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { HeavyHand }, Icon = UpgradeIcons.Hammer,
                Description = "Every swing is slower and costs more stamina, and every blow shakes its target. Your light blows strike 35% harder and break balance 60% sooner.",
                WeaponStacks = 1,
                Stats = new[] { new StatMod(PlayerStat.SwingSpeed, -0.22f), new StatMod(PlayerStat.AttackStamina, 0.2f), new StatMod(PlayerStat.WeaponDamage, 0.35f, StatWhen.LightBlow), new StatMod(PlayerStat.StaggerDealt, 0.6f, StatWhen.LightBlow) } },
                BranchPattern.MiddleCrown));

            // The third keystone, back under its id (the owner's word of 2026-11-20: trees may keep three). It stands at the
            // top of the trunk, grown from Cairn's Master, the stones' own spell.
            // NEEDS: the ring as a wall: nothing walks, rolls or is thrown across it while the surge lasts, an enemy driven into
            // a stone takes 20 damage and 35 stagger, and inside it the player is Rooted (closed-ring). Until then: eight stones
            // in a ring round the player that strike and shake what stands among them, and less harm taken.
            to.Add(BranchPattern.Put(new Upgrade { Id = TheClosedRing, Name = "The Closed Ring", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { SpellBranch.MasterId(Cairn) }, Icon = UpgradeIcons.Ring,
                Description = "Bedrock raises eight stones in a ring round you for ten seconds. They strike and shake what stands among them, and you take a little less harm. Bedrock costs a quarter more mana.",
                Stats = new[] { new StatMod(PlayerStat.SurgeCost, 0.25f) },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Ring, Reach = 6f, Count = 8, Radius = 1f, Seconds = 10f, Interval = 1f, Damage = 8f, Stagger = 20f, Affliction = T, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Boon, Stat = PlayerStat.DamageTaken, Amount = -0.08f, Seconds = 10f },
                } },
                BranchPattern.TrunkRight));

            // ---- The left bough: Standing. Hold your ground and be paid for it.
            // NEAREST: Rooted is the planted state (no dodge begun for three seconds): less damage and a cheaper guard while it holds. Not built: nothing pushing you back.
            to.Add(BranchPattern.Put(new Upgrade { Id = Rooted, Name = "Rooted", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Tower,
                Description = "Fight without rolling for a few breaths and you are Rooted. Rooted, you take less harm and your guard costs less. A roll ends it.",
                Stats = new[] { new StatMod(PlayerStat.DamageTaken, -0.08f, StatWhen.Planted), new StatMod(PlayerStat.GuardStamina, -0.3f, StatWhen.Planted) } },
                BranchPattern.LeftFoot));

            // The nova is the stamp, and everything it strikes flinches: what it had begun is broken unless it is steadfast.
            // NEEDS: the double stagger on an enemy showing an attack, and its attack broken only then (enemy-begins-attack).
            SpellBranch.Add(to, E, QuakeStep, "Quake Step", SpellRole.Breaker, BranchPattern.LeftA, Rooted, SpellForms.Nova,
                new SpellSpec { Radius = 5f, Delay = 0.45f, Damage = 16f, Stagger = 30f, Stacks = 2 },
                "You raise a foot and bring it down. The ground heaves in a ring five metres round you: 16 damage, 30 stagger and two Tremor. An enemy showing an attack takes double stagger and its attack is broken; a god or an elite takes only the stagger.",
                UpgradeIcons.Boot,
                new Empower { Cooldown = 8f, Radius = 6f },
                "Quake Step waits 8 seconds, not 9, and its ring reaches six metres.",
                // NEEDS: the stamp placed on the enemy faced, not the nearest (form-pieces).
                new ShapeDef { Name = "Struck From Afar", Form = SpellForms.Pillars,
                    Description = "The stamp lands not round you but on the shadow of the enemy faced, up to eight metres off, as a ring two and a half metres round it. Its attack breaks wherever it stands, and it is held a moment. You break the archer's draw without crossing the room.",
                    Spell = new SpellSpec { Count = 1, Reach = 8f, Radius = 2.5f, Delay = 0.45f, Damage = 16f, Stagger = 30f, Stacks = 2 },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Hold, Seconds = 0.6f } } },
                // NEEDS: thrown down only when the second ring catches it too, and at that moment (struck-twice). Until then what
                // the first ring catches is thrown down for a second.
                new ShapeDef { Name = "The Second Wave", Form = SpellForms.Nova,
                    Description = "One stamp sends out two rings: a fast thin one of dust to five metres, 8 damage and Tremor, then a moment later a slow heavy one of heaved slabs to seven metres, 18 damage and 40 stagger. An enemy caught by both is thrown off its feet for a second.",
                    Spell = new SpellSpec { Radius = 5f, Delay = 0.45f, Damage = 8f, Stagger = 0f, Stacks = 1 },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Burst, At = EffectAt.Self, Radius = 7f, Delay = 1.15f, Damage = 18f, Stagger = 40f, Stacks = 0 },
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = ThrownDown, Seconds = 1f, Flags = ConditionFlags.Down } },
                    } },
                "Opens two shapes of Quake Step, chosen at any rest fire: Struck From Afar, the stamp landed on a far enemy, or The Second Wave, a quick ring and then a heavy one.",
                // NEEDS: an enemy broken out of its wind-up as the moment (enemy-begins-attack). Until then a Quake Step that
                // strikes anything ends its own wait, once in eighteen seconds.
                new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.ResetCooldown, Cooldown = 18f } },
                "Break an enemy out of its wind-up, and Quake Step is ready again at once.",
                learnEffects: new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Flinch, Seconds = 0.4f } });

            // Grey Stone, re-aimed as the Learn: the parry form stands up to 2.5 s and turns the first blow, answering with a
            // light jar of stone that shakes, slows the attacker's next wind-up and loads the next heavy blow. NEEDS: every blow
            // turned, not only the first, the load counted for each and given to the next heavy blow alone within 1 s of
            // letting go, and the 5 s wait when nothing struck (held-press).
            SpellBranch.Add(to, E, MountainStance, "Mountain Stance", SpellRole.Counter, BranchPattern.LeftB, Rooted, SpellForms.Parry,
                new SpellSpec { Seconds = 2.5f, Reach = 3f, Arc = 120f, Damage = 6f, Stagger = 10f, Stacks = 2 },
                "You go wholly to stone for up to two and a half seconds, unable to move or answer. Every blow is turned, red ones too, and each one that strikes the stone jars its attacker: two Tremor, and its next attack comes late. Let go, and your next heavy blow within a second carries 20 more stagger for each blow the stone turned.",
                UpgradeIcons.Fist,
                // NEEDS: 25 stagger a blow turned (held-press).
                new Empower { Cooldown = 9f, Seconds = 3f },
                "Mountain Stance waits 9 seconds, not 10, you stand as stone up to three seconds, and each blow turned loads 25 stagger.",
                // Stone Skin, once a major, folded in with Aftershock. NEEDS: the answer set aside (shape-sets-aside), and a plate
                // taking 60% of one blow, not the whole of it (plates). Until then the plates are wards, given when the stance
                // turns a blow.
                new ShapeDef { Name = "Stone Skin",
                    Description = "The blow is turned but not answered. Two plates of stone grow on your shoulders instead. Each takes most of one blow and stops its flinch, then breaks in a burst three metres round you: 14 damage, 24 stagger and Tremor.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Ward, Count = 2, Cooldown = 1f },
                        new UpgradeEffect { On = UpgradeTrigger.WardSpent, Do = UpgradeAction.Burst, At = EffectAt.Self, Radius = 3f, Damage = 14f, Stagger = 24f, Affliction = T, Stacks = 1 },
                    } },
                // The stance as it was built; the answer is a cone at the one who struck, not a fist under it.
                new ShapeDef { Name = "Fist of Rock", Form = SpellForms.Parry,
                    Description = "You stand as stone for up to a second and a half. The first blow to land, red ones too, is turned for nothing, and a fist of rock rises under the one who struck: 30 damage, 60 stagger and two Tremor. One blow, answered at once.",
                    Spell = new SpellSpec { Seconds = 1.5f, Reach = 4f, Arc = 90f, Damage = 30f, Stagger = 60f, Stacks = 2 } },
                "Opens two shapes of Mountain Stance, chosen at any rest fire: Stone Skin, plates that take blows for you, or Fist of Rock, one blow answered at once.",
                // NEEDS: a red attack as the blow turned (red-attack). Until then any blow turned holds its attacker (gods and
                // elites for 0.4 of the time), and the stance is ready again once in twenty seconds.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Hold, Seconds = 1.5f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.ResetCooldown, Cooldown = 20f },
                },
                "Turn a red attack, and its attacker is stunned for a second and a half, less for a god or an elite, and Mountain Stance is ready again.",
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Jarred, Seconds = 3f, Flags = ConditionFlags.WindupSlowed, Scale = 0.7f } },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Boon, Stat = PlayerStat.StaggerDealt, Amount = 0.5f, Seconds = 3.5f, UntilBlow = true },
                });

            // BESPOKE: the dodge button becomes a brace that moves nowhere; its first 0.2 s is a perfect guard against any blow, red ones too; Rooted never ends.
            // Until then: the roll hardly moves, the guard is cheap and you take less, as one who is always Rooted. It grows
            // from Mountain Stance's Learn, whose line passes clear, as the old tree had it.
            to.Add(BranchPattern.Put(new Upgrade { Id = Immovable, Name = "Immovable", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { MountainStance }, Icon = UpgradeIcons.Tower,
                Description = "Your roll hardly moves you. You take 16% less harm, your guard and dodge cost 30% less stamina, and the moment of your perfect guard is longer.",
                Stats = new[]
                {
                    new StatMod(PlayerStat.DodgeDistance, -0.9f),
                    new StatMod(PlayerStat.DamageTaken, -0.16f),
                    new StatMod(PlayerStat.StaminaCost, -0.3f),
                    new StatMod(PlayerStat.PerfectGuardWindow, 0.3f),
                } },
                BranchPattern.LeftCrown));

            // ---- The right bough: Ground. Stone raised and broken over the enemy.
            // NEEDS: re-aimed as two of your stones standing at once, an enemy driven into one hurt and breaking it
            // (standing-things, thrown-into). Until then a pillar strikes again whatever a spell of yours strikes.
            to.Add(BranchPattern.Put(new Upgrade { Id = StandingStones, Name = "Standing Stones", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Tower,
                Description = "When a spell of yours strikes an enemy, a pillar of stone strikes it again a moment after and shakes it. Once every second and a half.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Falling, Radius = 1.5f, Delay = 0.5f, Damage = 20f, Stagger = 35f, Affliction = T, Stacks = 1, Cooldown = 1.5f } } },
                BranchPattern.RightFoot));

            // Stone Rise, the base spell: cast by Earth's kit, so its Learn carries no form; its shapes cast their own or add to it.
            SpellBranch.Add(to, E, StoneRise, "Stone Rise", SpellRole.Opener, BranchPattern.RightA, Root, "", null,
                "Earth's own spell, given with the element. A quarter second after the cast the ground breaks under the enemy faced, within five metres: 14 damage, 30 stagger and Tremor, and it is thrown up for a moment. Three rocks stand behind it for four and a half seconds, and nothing walks through them.",
                UpgradeIcons.Rock,
                // NEEDS: 18 damage and 36 stagger (base-spell-marks). The base spell is cast by its kit, which takes only the Empower's wait.
                new Empower { Cooldown = 6f, Damage = 18f, Stagger = 36f },
                "Stone Rise waits 6 seconds, not 7, and strikes for 18 damage and 36 stagger.",
                // NEEDS: no rocks behind, and the pillar left beside the enemy for 6 s, blocking bodies and thrown attacks, and
                // struck itself (standing-things). Until then the kit's rocks stand, and a heavy blow on the lifted enemy within
                // 6 s shatters the stone as a cone of shards from you through it.
                new ShapeDef { Name = "Raised Stone",
                    Description = "No rocks behind. The pillar that lifted the enemy stays beside it for six seconds and blocks bodies and thrown attacks. A heavy blow of yours on it shatters it into a cone of shards away from you, five metres and 70 degrees: 18 damage, 20 stagger and Tremor. Raise it, fight round it, break it through them.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Raised, Seconds = 6f, EndsWith = ConditionBlow.Heavy } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Raised, By = StruckBy.Heavy, Do = UpgradeAction.Cone, Reach = 5f, Arc = 70f, Damage = 18f, Stagger = 20f, Affliction = T, Stacks = 1 },
                    } },
                // NEEDS: a boulder falling from above with its shadow growing (form-pieces), lying 6 s as a thing struck and
                // rolled, breaking on a wall or a large enemy (standing-things). Until then stone rises under the nearest enemy
                // after 0.6 s, and a heavy blow on it within 6 s crushes a lane eight metres on from it.
                new ShapeDef { Name = "Dropped From the Apron", Form = SpellForms.Pillars,
                    Description = "A boulder falls from above onto the spot, its shadow growing for a moment: 24 damage, 45 stagger and two Tremor. It lies there six seconds. A heavy blow on it rolls it eight metres along the blow, crushing a lane: 16 damage, 30 stagger and Tremor to each, until it breaks on a wall or a large enemy.",
                    Spell = new SpellSpec { Count = 1, Reach = 5f, Radius = 1.5f, Delay = 0.6f, Damage = 24f, Stagger = 45f, Stacks = 2 },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Boulder, Seconds = 6f, EndsWith = ConditionBlow.Heavy } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Boulder, By = StruckBy.Heavy, Do = UpgradeAction.Lane, Reach = 8f, Radius = 0.8f, Interval = 0.04f, Damage = 16f, Stagger = 30f, Affliction = T, Stacks = 1 },
                    } },
                "Opens two shapes of Stone Rise, chosen at any rest fire: Raised Stone, a pillar to fight round and break, or Dropped From the Apron, a boulder to roll through them.",
                // NEEDS: still for 1 s as well as Rooted (stood-still), and the throw before the rocks stand. Until then, cast
                // while Rooted, what it strikes is thrown three metres away from you and shaken once more.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, While = StatWhen.Planted, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 3f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, While = StatWhen.Planted, Do = UpgradeAction.Afflict, Affliction = T, Stacks = 1 },
                },
                "Cast it while you stand your ground, Rooted or still for a second, and the stone rises under the enemy's own feet and throws it three metres away from you, two deep in Tremor, before it stands.");

            // NEEDS: every reaction with Tremor in it striking 20% harder (reaction-stats), and the rubble standing as one of
            // your stones that heavy blows drive enemies into (standing-things, thrown-into). The heap that bites is data.
            to.Add(BranchPattern.Put(new Upgrade { Id = LeftInRubble, Name = "Left in Rubble", Element = E, Kind = UpgradeKind.Major, Lifts = E, Requires = new[] { StandingStones }, Icon = UpgradeIcons.Rock,
                Description = "Every reaction with Tremor in it leaves a knee-high heap of rubble where it happened. It stands six seconds and shakes whatever stands in it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = T, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 1f, Seconds = 6f, Interval = 1f, Damage = 3f, Stagger = 8f, Affliction = T, Stacks = 1 } } },
                BranchPattern.RightMiddle));

            // The breaker: the spikes form runs the crack and breaks it open, and what it catches is held.
            // NEEDS: the crack glowing 0.9 s before it breaks open (form-times).
            SpellBranch.Add(to, E, Fissure, "Fissure", SpellRole.Breaker, BranchPattern.RightB, LeftInRubble, SpellForms.Spikes,
                new SpellSpec { Reach = 8f, Speed = 20f, Count = 1, Radius = 1f, Damage = 20f, Stagger = 35f, Stacks = 1 },
                "You drive the weapon into the ground. A crack runs eight metres ahead in under half a second, glows, then breaks open in spikes: 20 damage, 35 stagger and Tremor. Ordinary enemies are held where they stand for 1.2 seconds.",
                UpgradeIcons.Spike,
                new Empower { Cooldown = 9f, Damage = 26f },
                "Fissure waits 9 seconds, not 10, and its spikes do 26.",
                // NEEDS: the columns standing 4 s as a wall (standing-things) and toppling one after another away from you.
                // Until then what the spikes caught carries the row for 4 s, and a heavy blow on it topples a lane on from it.
                new ShapeDef { Name = "Basalt",
                    Description = "The spikes come up as a row of six-sided columns that stand four seconds as a wall. A heavy blow on the row topples it away from you one column after another, like dominoes: 20 damage and 30 stagger from each column that falls on an enemy.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Basalt, Seconds = 4f, EndsWith = ConditionBlow.Heavy } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Basalt, By = StruckBy.Heavy, Do = UpgradeAction.Lane, Reach = 6f, Radius = 0.7f, Interval = 0.08f, Damage = 20f, Stagger = 30f, Stacks = 0 },
                    } },
                // NEEDS: the open crack shown for 6 s, erupting along its whole length on a heavy blow within 2 m of it, twice at
                // most (form-pieces, count-rules). Until then the crack strikes for nothing, and a heavy blow on an enemy it
                // crossed within 6 s erupts under that enemy.
                new ShapeDef { Name = "Fault Line", Form = SpellForms.Spikes,
                    Description = "The crack does not break open by itself. It stays open six seconds, and any heavy blow you land within two metres of it erupts the whole line: 24 damage, 40 stagger and two Tremor. It can be erupted twice. You lay the line, then fight beside it.",
                    Spell = new SpellSpec { Reach = 8f, Speed = 20f, Count = 1, Radius = 1f, Damage = 0f, Stagger = 0f, Stacks = 0 },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = OnTheFault, Seconds = 6f } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = OnTheFault, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 2f, Damage = 24f, Stagger = 40f, Affliction = T, Stacks = 2, Cooldown = 0.5f },
                    } },
                "Opens two shapes of Fissure, chosen at any rest fire: Basalt, a wall of columns to topple, or Fault Line, a crack that waits for your heavy blows.",
                // NEEDS: the heavy blow landed on the crack's line while it glows (form-times, spell-windows). Until then a heavy
                // blow on an enemy within 0.9 s of the spikes striking it erupts again under it for half as much.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Glowing, Seconds = 0.9f, EndsWith = ConditionBlow.Heavy } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Glowing, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 1f, Damage = 10f, Stagger = 18f, Affliction = T, Stacks = 1 },
                },
                "Land a heavy blow on the crack's line while it glows, and it erupts at once, half as hard again.",
                learnEffects: new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Hold, Seconds = 1.2f } });

            // NEEDS: nothing you raise falling by itself, eight at most and the oldest crumbling, and an enemy driven into one
            // by a heavy blow, a finisher or a quake (standing-things, thrown-into). Until then what you leave
            // on the ground stands three times as long, and a heavy blow strikes its enemy as if driven into stone, once in 3 s.
            to.Add(BranchPattern.Put(new Upgrade { Id = StoneRemembers, Name = "Stone Remembers", Element = E, Kind = UpgradeKind.Devotion, Requires = new[] { LeftInRubble }, Icon = UpgradeIcons.Moon,
                Description = "All in on Earth only. Nothing you raise falls by itself: stones, boulders, columns and slabs stand until something breaks them, eight at most. An enemy driven into one by a heavy blow, a finisher or a quake takes 20 damage, 35 stagger and Tremor, and the stone holds. The longer the fight, the more of the floor is yours.",
                Stats = new[] { new StatMod(PlayerStat.GroundSeconds, 2f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Burst, Radius = 1f, Damage = 20f, Stagger = 35f, Affliction = T, Stacks = 1, Cooldown = 3f } } },
                BranchPattern.RightCrown));

            // BESPOKE (built): HandForm.
            to.Add(BranchPattern.Put(new Upgrade { Id = HandOfTheMountain, Name = "Hand of the Mountain", Element = E, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "While Bedrock lasts, a hand of stone as tall as a tower follows your arm. Each heavy blow you land, it lands again a moment after.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.Hand, Spell = new SpellSpec { Cost = 0f, Damage = 70f, Stagger = 40f, Radius = 3f, Delay = 0.5f, Stacks = 1 } } } },
                BranchPattern.Gift));
        }
    }
}
