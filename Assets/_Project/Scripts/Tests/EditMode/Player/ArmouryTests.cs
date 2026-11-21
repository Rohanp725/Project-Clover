using System.Collections.Generic;
using System.Linq;
using Clover.Player;
using Clover.Player.Build;
using Clover.Player.Trees.Ways;
using Clover.Player.Way;
using Clover.Player.Way.Aspects;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The armoury and the Katana's Way (IMPLEMENTATION.md 2.5 and 6.1, task 5.2): every aspect has its mark and every
    /// aspect mark its code; <see cref="Armoury.Resolve(string, string, IMarkQuery)"/> gives the god's weapon for a god's
    /// bough and a patched plain weapon otherwise, a fresh one each time; every patch keeps every step's animation name one
    /// a weapon already has (the first pass retimes existing clips); each patch follows its marks; and the Way is the shape
    /// the owner decided (way_plan.md 0 and 1): ten marks to a plain bough at 23 Memories, twelve to the god's at 25, the
    /// trunk with its five element infusion marks under their old ids, 132 in all.
    /// </summary>
    public class ArmouryTests
    {
        private const string Katana = Weapons.KatanaName;

        private static List<Upgrade> Way => Upgrade.All.Where(mark => mark.Tree == MarkTree.Way && mark.Weapon == Katana).ToList();

        // Every step name some weapon already has a clip for.
        private static HashSet<string> KnownClips()
        {
            var names = new HashSet<string>();
            foreach (var weapon in Weapons.All)
                foreach (var step in weapon.AllSteps())
                    names.Add(step.Name);
            return names;
        }

        private static AttackStep Step(Weapon weapon, int index) => weapon.Combo.Steps[index];

        [Test]
        public void EveryAspectHasItsMarkOnItsWayAndEveryAspectMarkHasItsCode()
        {
            foreach (var aspect in Armoury.All)
            {
                var mark = Upgrade.ById(aspect.Id);
                Assert.IsNotNull(mark, aspect.Id + " has no mark to open it");
                Assert.AreEqual(UpgradeKind.Aspect, mark.Kind, aspect.Id);
                Assert.AreEqual(MarkTree.Way, mark.Tree, aspect.Id);
                Assert.AreEqual(aspect.Weapon, mark.Weapon, aspect.Id);
                Assert.AreEqual(aspect.Id, mark.Aspect, aspect.Id + ": an aspect's mark is its own bough's");
                Assert.AreEqual(aspect.GodBough, mark.Currency == Currency.None, aspect.Id + ": a god's bough is given by Take, a plain one bought");
                Assert.AreEqual(aspect.GodBough, aspect.Build() != null, aspect.Id + ": only a god's bough builds its own weapon");
            }
            foreach (var mark in Upgrade.All.Where(mark => mark.Kind == UpgradeKind.Aspect))
                Assert.IsNotNull(Armoury.ById(mark.Id), mark.Id + " opens an aspect with no code");
            var katana = new List<WeaponAspect>();
            Assert.AreEqual(4, Armoury.AspectsOf(Katana, katana));
            CollectionAssert.AreEqual(new[] { KatanaWay.Sheath, KatanaWay.Stream, KatanaWay.Answer, KatanaWay.Storm }, katana.Select(aspect => aspect.Id).ToArray());
            Assert.AreEqual(0, Armoury.AspectsOf(Weapons.ScytheName, katana), "the Scythe's Way is not written yet");
            Assert.IsNotNull(Armoury.TrunkOf(Katana));
            Assert.IsNull(Armoury.TrunkOf(Weapons.ScytheName));
        }

        [Test]
        public void ResolveGivesTheGodsWeaponForAGodsBoughAndAPatchedPlainWeaponOtherwise()
        {
            var storm = Armoury.Resolve(Katana, KatanaWay.Storm, new MarkSet(KatanaWay.Storm));
            Assert.AreEqual(Weapons.StormKatanaName, storm.Name, "the god's bough chosen on the Katana");
            Assert.AreEqual(StormKatanaBough.FallingDamage, Step(storm, 2).Damage, 1e-4f, "his third cut, down a lane");
            Assert.Greater(Step(storm, 2).LineWidth, 0f);
            Assert.AreEqual(Weapons.StormKatanaName, Armoury.Resolve(Weapons.StormKatanaName, "", MarkSet.None).Name, "his sword carried from the rack is his bough");
            Assert.AreEqual(Weapons.StormKatanaName, Armoury.Resolve(Weapons.StormKatanaName, KatanaWay.Sheath, new MarkSet(KatanaWay.Sheath)).Name, "his sword outranks a plain aspect");

            var sheath = Armoury.Resolve(Katana, KatanaWay.Sheath, new MarkSet(KatanaWay.Sheath));
            Assert.AreEqual(Katana, sheath.Name);
            Assert.AreEqual(2, sheath.Combo.Steps.Count, "the draw and the return cut");
            Assert.AreEqual(Weapons.Katana().Combo.Steps[0].Damage * KatanaWay.DrawShare, Step(sheath, 0).Damage, 1e-4f);

            Assert.AreEqual(3, Armoury.Resolve(Katana, KatanaWay.Sheath, MarkSet.None).Combo.Steps.Count, "an aspect not in force leaves the weapon plain");
            Assert.AreEqual(3, Armoury.Resolve(Katana, "", MarkSet.None).Combo.Steps.Count);
            Assert.AreEqual(Weapons.ScytheName, Armoury.Resolve(Weapons.ScytheName, KatanaWay.Sheath, new MarkSet(KatanaWay.Sheath)).Name, "a Katana's aspect on another weapon is nothing");
            Assert.AreEqual(Weapons.Scythe().Combo.Steps.Count, Armoury.Resolve(Weapons.ScytheName, KatanaWay.Sheath, new MarkSet(KatanaWay.Sheath)).Combo.Steps.Count);
            Assert.AreEqual(Weapons.SunSpearName, Armoury.Resolve(Weapons.SunSpearName, "", MarkSet.None).Name, "a god's weapon whose bough is not written is still itself");
            Assert.AreEqual(Katana, Armoury.Resolve(Katana, "katana.nothing", new MarkSet("katana.nothing")).Name);
        }

        [Test]
        public void ResolvingALoadoutTakesItsActiveAspect()
        {
            var loadout = new Loadout { Weapon = Katana };
            loadout.SetAspect(Katana, KatanaWay.Stream);
            Assert.AreEqual(5, Armoury.Resolve(loadout, new MarkSet(KatanaWay.Stream)).Combo.Steps.Count, "five cuts");
            loadout.Weapon = Weapons.StormKatanaName;
            Assert.AreEqual(Weapons.StormKatanaName, Armoury.Resolve(loadout, new MarkSet(KatanaWay.Stream)).Name);
            Assert.AreEqual(Weapons.FistsName, Armoury.Resolve((Loadout)null, null).Name, "nothing carried");
        }

        [Test]
        public void EachResolveIsAFreshWeaponAndNoPatchLeaks()
        {
            var all = new MarkSet(Way.Select(mark => mark.Id));
            var first = Armoury.Resolve(Katana, KatanaWay.Stream, all);
            var second = Armoury.Resolve(Katana, KatanaWay.Stream, all);
            Assert.AreNotSame(first, second);
            Assert.AreNotSame(first.Combo.Steps[3], second.Combo.Steps[3]);
            Assert.AreEqual(first.Combo.Steps.Count, second.Combo.Steps.Count, "the same patch twice is the same weapon");
            Assert.AreEqual(Step(first, 0).Reach, Step(second, 0).Reach, 1e-5f);
            Assert.AreEqual(3, Weapons.Katana().Combo.Steps.Count, "the plain Katana is untouched");
            Assert.AreEqual(10f, Weapons.Katana().Combo.Steps[0].Damage, 1e-5f);
            Assert.AreNotSame(first.Combo.Steps[3], first.Combo.Steps[0], "an added cut is a copy, not the step it was made from");
        }

        [Test]
        public void EveryAspectsPatchKeepsEveryStepsAnimationNameKnown()
        {
            var known = KnownClips();
            var way = Way;
            var trunk = way.Where(mark => mark.Aspect == "").Select(mark => mark.Id).ToList();
            foreach (var aspect in Armoury.All)
            {
                var bough = way.Where(mark => mark.Aspect == aspect.Id).ToList();
                var keystones = bough.Where(mark => mark.Kind == UpgradeKind.Keystone).ToList();
                // Bare; with everything but the keystones; with each keystone set in turn.
                var sets = new List<MarkSet> { new MarkSet(aspect.Id) };
                var stem = bough.Where(mark => mark.Kind != UpgradeKind.Keystone).Select(mark => mark.Id).Concat(trunk).ToList();
                sets.Add(new MarkSet(stem));
                foreach (var keystone in keystones) sets.Add(new MarkSet(stem.Concat(new[] { keystone.Id })));
                foreach (var marks in sets)
                {
                    var weapon = Armoury.Resolve(aspect.Weapon, aspect.Id, marks);
                    foreach (var step in weapon.AllSteps())
                        Assert.IsTrue(known.Contains(step.Name), $"{aspect.Id} plays '{step.Name}', which no weapon has a clip for");
                    Assert.Greater(weapon.Combo.Steps.Count, 0, aspect.Id + " leaves a chain");
                }
            }
            var plain = Weapons.Katana();
            foreach (var step in new[] { AnswerAspect.Riposte(plain), AnswerAspect.ThroughTheGap(plain) })
            {
                Assert.IsNotNull(step);
                Assert.IsTrue(known.Contains(step.Name), "the answer plays " + step.Name);
            }
        }

        [Test]
        public void TheSheathsPatchFollowsItsMarks()
        {
            float draw = Weapons.Katana().Combo.Steps[0].Damage;
            float stamina = Weapons.Katana().Combo.Steps[0].StaminaCost;
            var sharp = Armoury.Resolve(Katana, KatanaWay.Sheath, new MarkSet(KatanaWay.Sheath, KatanaWay.SharpDraw));
            Assert.AreEqual(draw * KatanaWay.SharpDrawShare, Step(sharp, 0).Damage, 1e-4f, "Sharp Draw");
            Assert.AreEqual(Weapons.Katana().Combo.Steps[0].Stagger + KatanaWay.DrawStagger, Step(sharp, 0).Stagger, 1e-4f);
            Assert.AreEqual(Weapons.Katana().Combo.Steps[1].Damage, Step(sharp, 1).Damage, 1e-4f, "the return cut is a plain cut");
            var shrine = Armoury.Resolve(Katana, KatanaWay.Sheath, new MarkSet(KatanaWay.Sheath, KatanaWay.SharpDraw, KatanaWay.AHundredDays));
            Assert.AreEqual(1, shrine.Combo.Steps.Count, "every light a draw");
            Assert.AreEqual(draw * KatanaWay.HundredDaysShare, Step(shrine, 0).Damage, 1e-4f);
            Assert.AreEqual(0.42f, Step(shrine, 0).Total, 0.005f, "drawn and sheathed in 0.42 s");
            var light = Armoury.Resolve(Katana, KatanaWay.Sheath, new MarkSet(KatanaWay.Sheath, KatanaWay.LightScabbard));
            Assert.AreEqual(stamina - SheathAspect.LightScabbardStamina, Step(light, 0).StaminaCost, 1e-4f);
        }

        [Test]
        public void TheStreamsPatchFollowsItsMarks()
        {
            var plain = Weapons.Katana();
            var stream = Armoury.Resolve(Katana, KatanaWay.Stream, new MarkSet(KatanaWay.Stream));
            Assert.AreEqual(5, stream.Combo.Steps.Count);
            Assert.AreEqual(StreamAspect.TurningDamage, Step(stream, 3).Damage, 1e-4f);
            Assert.AreEqual(StreamAspect.TurningArc, Step(stream, 3).ArcDegrees, 1e-4f);
            Assert.AreEqual(StreamAspect.RisingDamage, Step(stream, 4).Damage, 1e-4f);
            Assert.IsNull(stream.Combo.DashAttack, "no cut out of the roll without a mark for it");
            var wrist = Armoury.Resolve(Katana, KatanaWay.Stream, new MarkSet(KatanaWay.Stream, KatanaWay.LightWrist));
            Assert.AreEqual(plain.Combo.Steps[0].Total, Step(wrist, 0).Total, 1e-4f, "the first two cuts as they were");
            Assert.AreEqual(plain.Combo.Steps[2].Total * StreamAspect.LightWristShare, Step(wrist, 2).Total, 1e-4f, "the third on, quicker");
            Assert.Less(Step(wrist, 4).Total, Step(stream, 4).Total);
            var rising = Armoury.Resolve(Katana, KatanaWay.Stream, new MarkSet(KatanaWay.Stream, KatanaWay.RisingDraw));
            Assert.AreEqual(KatanaTrunk.RisingDamage, rising.Combo.DashAttack.Damage, 1e-4f, "the trunk's Rising Draw in the Stream");
            var stepping = Armoury.Resolve(Katana, KatanaWay.Stream, new MarkSet(KatanaWay.Stream, KatanaWay.RisingDraw, KatanaWay.SteppingWater));
            Assert.AreEqual(StreamAspect.SteppingArc, stepping.Combo.DashAttack.ArcDegrees, 1e-4f, "Stepping Water takes the Rising Draw's place");
            var unbroken = Armoury.Resolve(Katana, KatanaWay.Stream, new MarkSet(KatanaWay.Stream, KatanaWay.Unbroken));
            Assert.IsTrue(unbroken.Combo.Steps.All(step => step.Poise >= StreamAspect.UnbrokenPoise));
        }

        [Test]
        public void TheAnswersPatchFollowsItsMarks()
        {
            var plain = Weapons.Katana();
            var answer = Armoury.Resolve(Katana, KatanaWay.Answer, new MarkSet(KatanaWay.Answer));
            for (int i = 0; i < 3; i++) Assert.AreEqual(plain.Combo.Steps[i].Damage * AnswerAspect.LightShare, Step(answer, i).Damage, 1e-4f);
            Assert.AreEqual(plain.Combo.Heavy.Damage, answer.Combo.Heavy.Damage, 1e-4f, "only the light blows");
            Assert.IsNull(answer.Combo.GuardAttack);
            var full = Armoury.Resolve(Katana, KatanaWay.Answer, new MarkSet(KatanaWay.Answer, KatanaWay.TurnedEdge, KatanaWay.GuardCut, KatanaWay.KiriOtoshi));
            Assert.AreEqual(plain.PerfectGuardStagger + AnswerAspect.TurnedEdgeStagger, full.PerfectGuardStagger, 1e-4f);
            Assert.AreEqual(AnswerAspect.GuardCutDamage, full.Combo.GuardAttack.Damage, 1e-4f);
            Assert.AreEqual(2, full.Combo.Steps.Count, "Kiri-otoshi: the light chain is two cuts");
            Assert.IsTrue(full.AllSteps().Contains(full.Combo.GuardAttack), "the guard's cut is among the steps that need a clip");
            Assert.AreEqual(AnswerAspect.RiposteDamage, AnswerAspect.Riposte(plain).Damage, 1e-4f);
            Assert.IsTrue(AnswerAspect.ThroughTheGap(plain).PassThrough);
        }

        [Test]
        public void TheStormKatanasPatchFollowsItsMarks()
        {
            var bare = Armoury.Resolve(Katana, KatanaWay.Storm, new MarkSet(KatanaWay.Storm));
            Assert.IsNull(bare.Combo.ChargedHeavy, "the Katana has no charged heavy until the Red Draw");
            var marks = new MarkSet(KatanaWay.Storm, KatanaWay.LongBlade, KatanaWay.TheRedDraw, KatanaWay.TheFallingCut);
            var storm = Armoury.Resolve(Katana, KatanaWay.Storm, marks);
            Assert.AreEqual(Step(bare, 0).Reach + StormKatanaBough.LongBladeReach, Step(storm, 0).Reach, 1e-4f, "Long Blade, once a step");
            Assert.AreEqual(bare.Combo.Heavy.Reach + StormKatanaBough.LongBladeReach, storm.Combo.Heavy.Reach, 1e-4f);
            Assert.AreEqual(StormKatanaBough.FallingReach + StormKatanaBough.FallingCutReach + StormKatanaBough.LongBladeReach, Step(storm, 2).Reach, 1e-4f, "The Falling Cut's longer lane");
            var red = storm.Combo.ChargedHeavy;
            Assert.IsNotNull(red, "the Red Draw on the held heavy");
            Assert.AreEqual(StormKatanaBough.RedDrawDamage, red.Damage, 1e-4f);
            Assert.IsTrue(red.PassThrough);
            Assert.AreEqual(StormKatanaBough.RedDrawHold, storm.Combo.ChargeTime, 1e-4f);
            Assert.Greater(storm.Combo.SkillChargeTime, storm.Combo.ChargeTime, "the skill of the old rule waits longer than the red draw");
            Assert.AreNotSame(storm.Combo.Skill, red, "a copy of the Thunder Draw, not the skill itself");
        }

        [Test]
        public void TheKatanasTrunkFollowsItsMarksInEveryAspect()
        {
            var plain = Weapons.Katana();
            var trunk = new MarkSet(KatanaWay.SettledHands, KatanaWay.RisingDraw, KatanaWay.Temper3, KatanaWay.Temper5);
            foreach (string aspect in new[] { "", KatanaWay.Sheath, KatanaWay.Answer, KatanaWay.Storm })
            {
                var weapon = Armoury.Resolve(Katana, aspect, new MarkSet(new[] { aspect, KatanaWay.SettledHands, KatanaWay.RisingDraw, KatanaWay.Temper3, KatanaWay.Temper5 }));
                Assert.IsNotNull(weapon.Combo.DashAttack, aspect + ": the Rising Draw");
                Assert.Less(Step(weapon, 0).Recovery, Weapons.Katana().Combo.Steps[0].Recovery * 0.95f, aspect + ": Settled Hands");
            }
            var bare = Armoury.Resolve(Katana, "", trunk);
            Assert.AreEqual(plain.Combo.Steps[0].Recovery * KatanaTrunk.SettledRecovery, Step(bare, 0).Recovery, 1e-4f);
            Assert.AreEqual(plain.Combo.Steps[0].Reach + KatanaTrunk.TemperThreeReach, Step(bare, 0).Reach, 1e-4f, "Temper III's drawing cut");
            Assert.AreEqual(plain.Carrier.Reach + KatanaTrunk.TemperThreeReach, bare.Carrier.Reach, 1e-4f, "and its carrier");
            Assert.AreEqual(plain.Combo.Finisher.Damage * KatanaTrunk.TemperFiveFinisher, bare.Combo.Finisher.Damage, 1e-4f, "Temper V's finisher");
            Assert.AreEqual(plain.Combo.Heavy.Recovery, bare.Combo.Heavy.Recovery, 1e-5f, "Settled Hands is the lights only");
        }

        [Test]
        public void TheKatanasWayIsTheShapeTheOwnerDecided()
        {
            var way = Way;
            Assert.IsTrue(way.All(mark => mark.Currency == Currency.Memories || mark.Id == KatanaWay.Storm), "bought with Memories, the god's bough given");
            var trunk = way.Where(mark => mark.Aspect == "").ToList();
            Assert.AreEqual(20, trunk.Count, "five tempers, four infusions, five basics, the technique and five element infusion marks");
            Assert.AreEqual(38, trunk.Sum(mark => mark.Cost), "the trunk's 28 and the element infusion marks' 10");
            foreach (string id in new[] { KatanaWay.Sheath, KatanaWay.Stream, KatanaWay.Answer })
            {
                var bough = way.Where(mark => mark.Aspect == id).ToList();
                Assert.AreEqual(10, bough.Count, id + ": a plain bough is ten marks");
                Assert.AreEqual(23, bough.Sum(mark => mark.Cost), id + ": 23 Memories");
                Assert.AreEqual(2, bough.Count(mark => mark.Kind == UpgradeKind.Keystone), id + ": two keystones");
                Assert.AreEqual(1, bough.Count(mark => mark.Kind == UpgradeKind.Minor), id + ": one Number in the stem");
            }
            var storm = way.Where(mark => mark.Aspect == KatanaWay.Storm).ToList();
            Assert.AreEqual(12, storm.Count, "a god's bough is twelve marks");
            Assert.AreEqual(25, storm.Sum(mark => mark.Cost));
            Assert.AreEqual(2, storm.Count(mark => mark.Kind == UpgradeKind.Keystone), "two god moves");
            Assert.AreEqual(132, way.Sum(mark => mark.Cost), "the Katana with its five element infusion marks");
            // Each bough is bought in order: its aspect first, then one road of marks forking into two paths, each to a keystone.
            foreach (var aspect in Armoury.All)
            {
                var bough = way.Where(mark => mark.Aspect == aspect.Id).ToList();
                foreach (var mark in bough.Where(mark => mark.Id != aspect.Id))
                    Assert.AreEqual(1, mark.Requires.Length, mark.Id + " grows from one mark of its bough");
                Assert.IsEmpty(Upgrade.ById(aspect.Id).Requires, aspect.Id + " grows from the trunk's foot");
                var forks = bough.Where(mark => bough.Count(other => other.Requires.Contains(mark.Id)) == 2).ToList();
                Assert.AreEqual(1, forks.Count, aspect.Id + " forks once, at the end of its stem");
                foreach (var keystone in bough.Where(mark => mark.Kind == UpgradeKind.Keystone))
                    Assert.IsFalse(bough.Any(other => other.Requires.Contains(keystone.Id)), keystone.Id + " ends its path");
            }
        }

        [Test]
        public void TemperIsTheOnlyRawDamageAndOpensWithTheClimb()
        {
            var tempers = Way.Where(mark => mark.Kind == UpgradeKind.Temper).ToList();
            Assert.AreEqual(5, tempers.Count);
            CollectionAssert.AreEqual(new[] { 1, 1, 2, 2, 3 }, tempers.Select(mark => mark.Cost).ToArray());
            Assert.AreEqual(0.30f, tempers.Sum(mark => mark.Stats.Where(stat => stat.Stat == PlayerStat.WeaponDamage).Sum(stat => stat.Amount)), 1e-4f, "+30% at V");
            Assert.AreEqual(0.25f, tempers.Sum(mark => mark.Stats.Where(stat => stat.Stat == PlayerStat.StaggerDealt).Sum(stat => stat.Amount)), 1e-4f, "+25% stagger at V");
            Assert.IsTrue(Way.Where(mark => mark.Kind != UpgradeKind.Temper).All(mark => mark.Stats.All(stat => stat.Stat != PlayerStat.WeaponDamage)), "no other mark is raw damage");
            var learned = new HashSet<string>();
            for (int i = 0; i < tempers.Count; i++)
            {
                Assert.AreEqual(i, WayRules.TemperRank(Katana, learned, Upgrade.All));
                learned.Add(tempers[i].Id);
            }
            Assert.IsFalse(MarkRules.GateOpen(Upgrade.ById(KatanaWay.Temper5), null, 6, false), "Temper V waits for the seventh god");
            Assert.IsTrue(MarkRules.GateOpen(Upgrade.ById(KatanaWay.Temper5), null, 7, false));
            Assert.IsTrue(MarkRules.GateOpen(Upgrade.ById(KatanaWay.Temper1), null, 0, false));
        }

        [Test]
        public void TheTechniqueWaitsForTheLastRetainer()
        {
            var technique = Upgrade.ById(KatanaWay.BeforeTheThunder);
            Assert.AreEqual(UpgradeKind.Technique, technique.Kind);
            Assert.AreEqual(KatanaWay.LastRetainerFlag, technique.Gate);
            var wallet = new Clover.Player.Growth.Wallet { Memories = 99 };
            var flags = new HashSet<string>();
            Assert.IsFalse(MarkRules.CanBuy(technique, new HashSet<string>(), wallet, true, false, flags.Contains, 9));
            flags.Add(KatanaWay.LastRetainerFlag);
            Assert.IsTrue(MarkRules.CanBuy(technique, new HashSet<string>(), wallet, true, false, flags.Contains, 0));
        }

        [Test]
        public void TheMarksThatLeftTheElementTreesWorkOnTheKatanasTrunkWhileTheirElementIsCarried()
        {
            var elements = new Dictionary<string, string>
            {
                { Upgrade.BurningCrescent, Elements.Fire }, { Upgrade.DelayedCuts, Elements.Lightning }, { KatanaWay.DrawingWind, Elements.Wind },
                { KatanaWay.WinterDraw, Elements.Ice }, { KatanaWay.AnsweringDraw, Elements.Light },
            };
            CollectionAssert.AreEquivalent(elements.Keys, RetiredMarks.Moving.Select(mark => mark.Id), "every mark that left a tree has its home here");
            foreach (var pair in elements)
            {
                var mark = Upgrade.ById(pair.Key);
                Assert.IsNotNull(mark, pair.Key);
                Assert.AreEqual(MarkTree.Way, mark.Tree, pair.Key);
                Assert.AreEqual(Katana, mark.Weapon, pair.Key);
                Assert.IsEmpty(mark.Aspect, pair.Key + " is on the trunk");
                Assert.AreEqual(pair.Value, mark.Element, pair.Key);
                Assert.IsFalse(mark.IsMix, pair.Key);
                Assert.AreEqual(2, mark.Cost, pair.Key + ": 2 Memories, as every element infusion mark");
                var learned = new HashSet<string> { pair.Key };
                var carrying = new Loadout { Weapon = Katana };
                carrying.Attune(pair.Value, "");
                Assert.IsTrue(MarkRules.InForce(mark, carrying, learned), pair.Key + " with its element and the Katana");
                carrying.Weapon = Weapons.StormKatanaName;
                Assert.IsTrue(MarkRules.InForce(mark, carrying, learned), pair.Key + " with the Storm Katana");
                carrying.Weapon = Weapons.GreatSpearName;
                Assert.IsFalse(MarkRules.InForce(mark, carrying, learned), pair.Key + " with another weapon");
                carrying.Weapon = Katana;
                carrying.Attune(pair.Value == Elements.Earth ? Elements.Water : Elements.Earth, "");
                Assert.IsFalse(MarkRules.InForce(mark, carrying, learned), pair.Key + " without its element");
            }
        }

        [Test]
        public void ABoughsMarksSleepWhenAnotherIsChosenAndWakeWhenItReturns()
        {
            var learned = new HashSet<string> { KatanaWay.Sheath, KatanaWay.SharpDraw, KatanaWay.Stream, KatanaWay.Temper1 };
            var loadout = new Loadout { Weapon = Katana };
            Assert.IsTrue(WayRules.SettleBought(Upgrade.ById(KatanaWay.Sheath), loadout));
            Assert.IsTrue(MarkRules.InForce(Upgrade.ById(KatanaWay.SharpDraw), loadout, learned));
            Assert.IsTrue(WayRules.ChooseAspect(loadout, Upgrade.ById(KatanaWay.Stream), learned, false));
            Assert.IsFalse(MarkRules.InForce(Upgrade.ById(KatanaWay.SharpDraw), loadout, learned), "asleep");
            Assert.IsTrue(MarkRules.InForce(Upgrade.ById(KatanaWay.Temper1), loadout, learned), "the trunk works in every aspect");
            Assert.IsTrue(WayRules.ChooseAspect(loadout, Upgrade.ById(KatanaWay.Sheath), learned, false));
            Assert.IsTrue(MarkRules.InForce(Upgrade.ById(KatanaWay.SharpDraw), loadout, learned), "awake, still learned");
            Assert.AreEqual(1 + 2 + 2 + 1, WayRules.Spent(Katana, learned, Upgrade.All));
        }
    }
}
