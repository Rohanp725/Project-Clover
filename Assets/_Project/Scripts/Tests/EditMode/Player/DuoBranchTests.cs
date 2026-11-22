using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Build;
using Clover.Player.Forms;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The duo branches and the heart (IMPLEMENTATION.md 2.3, 2.4, 4.2; task 4.5): a duo branch of any pair opens while both
    /// its elements are carried; the heart shows the carried pair, or in the playtest the pair picked; a reaction wears the
    /// shape chosen at the rest fire instead of its plain self once the pair's Shape mark is learned and both are carried,
    /// and a swap of the shape changes what it runs.
    /// </summary>
    public class DuoBranchTests
    {
        private static Upgrade Duo(string id, string one, string other, MarkTree tree = MarkTree.Duo) =>
            new Upgrade { Id = id, Name = id, Element = one, Element2 = other, Kind = UpgradeKind.Mix, Tree = tree, Description = "A test mark." };

        private static Loadout Hand(string first, string second)
        {
            var loadout = new Loadout();
            loadout.Attune(first, second);
            return loadout;
        }

        [TearDown]
        public void GatherTheGameAgain() => Upgrade.Gather(null);

        [Test]
        public void AMarkOfAnyPairOpensWhileBothItsElementsAreCarried()
        {
            var glass = Duo("t.glass", Elements.Ice, Elements.Lightning);
            Assert.IsFalse(Elements.AreNeighbours(Elements.Ice, Elements.Lightning), "a pair two apart, with no ring gap of its own");
            Assert.IsTrue(MarkRules.Open(glass, Hand(Elements.Lightning, Elements.Ice), false), "both carried");
            Assert.IsTrue(MarkRules.Open(glass, Hand(Elements.Ice, Elements.Lightning), false), "in either order");
            Assert.IsFalse(MarkRules.Open(glass, Hand(Elements.Lightning, ""), false), "one alone");
            Assert.IsFalse(MarkRules.Open(glass, Hand(Elements.Lightning, Elements.Lightning), false), "all in on one");
            Assert.IsFalse(MarkRules.Open(glass, Hand(Elements.Fire, Elements.Lightning), false), "with another partner");
            Assert.IsTrue(MarkRules.Open(glass, Hand("", ""), true), "the playtest's free chart opens every branch");
            Assert.IsFalse(MarkRules.Open(glass, null, false));
            var fire = new Upgrade { Id = "t.fire", Element = Elements.Fire, Kind = UpgradeKind.Major };
            Assert.IsTrue(MarkRules.Open(fire, Hand(Elements.Lightning, Elements.Fire), false), "an element's mark while its element is carried, first or second");
            Assert.IsFalse(MarkRules.Open(fire, Hand(Elements.Lightning, Elements.Ice), false));
            Assert.IsFalse(MarkRules.Open(new Upgrade { Id = "t.react", Element = Elements.Fire, Element2 = Elements.Water, Kind = UpgradeKind.Reaction, Tree = MarkTree.Hidden }, Hand(Elements.Fire, Elements.Water), true),
                "a reaction is on no chart");
        }

        [Test]
        public void TheHeartDrawsOnlyItsDuoBranchesAndKnowsTheirPair()
        {
            var heart = HeartLayout.Instance;
            var glass = Duo("t.glass", Elements.Ice, Elements.Lightning);
            var ringMix = Duo("t.tempest", Elements.Lightning, Elements.Wind, MarkTree.Element);
            Assert.IsTrue(heart.Draws(glass));
            Assert.IsFalse(heart.Draws(ringMix), "a mix not reshaped yet stays on the ring");
            Assert.IsFalse(RingLayout.Instance.Draws(glass), "and the ring leaves the heart's branches to it");
            Assert.IsFalse(heart.Draws(new Upgrade { Id = "t.fire", Element = Elements.Fire, Tree = MarkTree.Duo }), "a mark of one element is never in the heart");
            int sleet = AfflictionPairs.Index(AfflictionKind.Static, AfflictionKind.Chill);
            Assert.AreEqual(sleet, HeartLayout.PairOf(glass));
            Assert.AreEqual(sleet, HeartLayout.PairOf(Elements.Lightning, Elements.Ice), "either order");
            Assert.AreEqual(-1, HeartLayout.PairOf(Elements.Fire, Elements.Fire));
            Assert.IsTrue(heart.Shows(glass, sleet));
            Assert.IsFalse(heart.Shows(glass, HeartLayout.PairOf(Elements.Fire, Elements.Lightning)));
            Assert.AreEqual("Ice and Lightning: Sleet Charge", HeartLayout.Title(Elements.Ice, Elements.Lightning));
            HeartPattern.Put(ringMix, HeartPattern.Spell);
            Assert.AreEqual(MarkTree.Duo, ringMix.Tree, "a mark put in the heart's pattern is drawn in the heart");
            Assert.AreEqual(HeartPattern.Of(HeartPattern.Spell).Y, heart.Place(ringMix).y, 1e-5f);
        }

        [Test]
        public void TheHeartShowsTheCarriedPairOrInThePlaytestThePickedOne()
        {
            var marks = new List<Upgrade>
            {
                Duo("t.plasma", Elements.Fire, Elements.Lightning),
                Duo("t.glass", Elements.Ice, Elements.Lightning),
                Duo("t.tempest", Elements.Lightning, Elements.Wind, MarkTree.Element),
            };
            // The game: the other element carried, built or not; a Hand of one shows nothing.
            Assert.AreEqual(Elements.Ice, HeartLayout.PartnerFor(Elements.Lightning, Hand(Elements.Lightning, Elements.Ice), false, "", marks));
            Assert.AreEqual(Elements.Water, HeartLayout.PartnerFor(Elements.Fire, Hand(Elements.Water, Elements.Fire), false, "", marks), "an unbuilt pair is still the pair carried");
            Assert.AreEqual("", HeartLayout.PartnerFor(Elements.Fire, Hand(Elements.Fire, ""), false, "", marks));
            Assert.AreEqual("", HeartLayout.PartnerFor(Elements.Wind, Hand(Elements.Fire, Elements.Lightning), false, "", marks), "a tree not carried");
            // The playtest: the pick, built or not; else the next partner round the ring (the owner's answer of 2026-11-20).
            Assert.AreEqual(Elements.Ice, HeartLayout.PartnerFor(Elements.Lightning, Hand(Elements.Fire, ""), true, Elements.Ice, marks));
            Assert.AreEqual(Elements.Wind, HeartLayout.PartnerFor(Elements.Lightning, Hand(Elements.Fire, ""), true, "", marks), "Wind is the next partner after Lightning");
            Assert.AreEqual(Elements.Wind, HeartLayout.PartnerFor(Elements.Fire, Hand(Elements.Fire, ""), true, Elements.Wind, marks), "a pick is kept even with nothing built");
            Assert.AreEqual(Elements.Fire, HeartLayout.PartnerFor(Elements.Lightning, Hand(Elements.Lightning, Elements.Fire), true, "", marks), "with no pick, the pair carried");
            // The shoulder buttons step through all ten partners in turn, built or not.
            Assert.AreEqual(Elements.Water, HeartLayout.NextPartner(Elements.Lightning, Elements.Ice, 1));
            Assert.AreEqual(Elements.Wind, HeartLayout.NextPartner(Elements.Lightning, Elements.Fire, 1));
            Assert.AreEqual(Elements.Poison, HeartLayout.NextPartner(Elements.Lightning, Elements.Fire, -1));
            Assert.AreEqual(Elements.Ice, HeartLayout.NextPartner(Elements.Wind, "", 1));
            var seen = new HashSet<string>();
            string at = "";
            for (int i = 0; i < 10; i++) seen.Add(at = HeartLayout.NextPartner(Elements.Fire, at, 1));
            Assert.AreEqual(10, seen.Count, "ten presses meet all ten partners");
            Assert.IsFalse(seen.Contains(Elements.Fire), "never the tree looked at");
            Assert.AreEqual(Elements.Lightning, HeartLayout.NextPartner(Elements.Fire, at, 1), "the eleventh comes round again");
        }

        [Test]
        public void AReactionWearsItsChosenShapeInsteadOfThePlainOnceTheShapeIsLearnedAndBothAreCarried()
        {
            // A test Shape mark for Steam (Fire and Water), gathered after the game's own Steam branch, so ReactionTable, which keeps
            // the last Shape mark of a pair, takes it in the branch's place; the plain Steam is the game's.
            var first = new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 1f };
            var second = new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Radius = 2f };
            var shapeMark = HeartPattern.Put(new Upgrade
            {
                Id = "t.steam.shape", Name = "Steam: Kettle or Geyser", Element = Elements.Fire, Element2 = Elements.Water, Kind = UpgradeKind.Mix, Part = BranchPart.Shape,
                Description = "A test shape.",
                Shapes = new[] { new ShapeDef { Name = "Kettle", Effects = new[] { first } }, new ShapeDef { Name = "Geyser", Effects = new[] { second } } },
            }, HeartPattern.Foot);
            Upgrade.Gather(list => list.Add(shapeMark));
            int steam = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Soaked);
            var plain = ReactionTable.MarkFor(steam);
            Assert.IsNotNull(plain, "the plain Steam is built");
            Assert.IsTrue(ReactionTable.IsReactionShape(shapeMark));
            Assert.AreSame(shapeMark, ReactionTable.ShapeMarkFor(steam));
            Assert.AreEqual("react.steam", ReactionTable.ShapeKey(steam));
            Assert.AreSame(shapeMark, first.Owner, "a shape's effects belong to its Shape mark");

            var hand = Hand(Elements.Water, Elements.Fire);
            var learned = new HashSet<string>();
            Assert.AreSame(plain.Effects, ReactionTable.EffectsFor(steam, hand, learned), "before the Shape is bought, the plain reaction");
            learned.Add(shapeMark.Id);
            var worn = ReactionTable.EffectsFor(steam, hand, learned);
            Assert.AreEqual(1, worn.Count);
            Assert.AreSame(first, worn[0], "bought, the first shape runs instead");
            Assert.AreEqual("Kettle", ReactionTable.WornShape(steam, hand, learned).Name);

            SpellBranches.Choose(hand.Shapes, ReactionTable.ShapeKey(steam), 1);
            Assert.AreSame(second, ReactionTable.EffectsFor(steam, hand, learned)[0], "the shape swapped at the rest fire changes what the reaction runs");
            Assert.AreEqual("Geyser", ReactionTable.WornShape(steam, hand, learned).Name);
            Assert.AreNotEqual(ReactionTable.EffectsFor(steam, hand, learned)[0].Do, ReactionTable.EffectsFor(steam, Hand(Elements.Water, Elements.Fire), learned)[0].Do,
                "a Hand with no choice wears the first shape");

            Assert.AreSame(plain.Effects, ReactionTable.EffectsFor(steam, Hand(Elements.Fire, Elements.Lightning), learned), "with the pair broken, the plain reaction (from the world's water)");
            Assert.AreSame(plain.Effects, ReactionTable.EffectsFor(steam, hand), "asked without what is learned, the plain reaction");
            // A reaction's shape is never a standing effect: it runs only when the pair reacts.
            Assert.IsNull(SpellBranches.Worn(shapeMark, hand));
            Assert.IsTrue(MarkRules.InForce(shapeMark, hand, learned), "its Shape mark needs no slot");
        }

        [Test]
        public void EveryReactionShapeRidesAReactionsMomentsAndAnswersOnlyTheConditionsItSets()
        {
            // The pickups a reaction drops give it two moments more: taken, or left to lie (World.Pickups, PlayerUpgrades.AnswerPickup);
            // and a condition that draws your dodge, a third: you were carried to the one in it (Eagle's Sight).
            var reactionTriggers = new[] { UpgradeTrigger.Reacted, UpgradeTrigger.ConditionBeat, UpgradeTrigger.ConditionStruck, UpgradeTrigger.ConditionEnded, UpgradeTrigger.ConditionDied, UpgradeTrigger.ConditionAttacked,
                UpgradeTrigger.PickedUp, UpgradeTrigger.PickupLeft, UpgradeTrigger.CarriedTo };
            var shapeMarks = Upgrade.All.Where(ReactionTable.IsReactionShape).ToList();
            Assert.GreaterOrEqual(shapeMarks.Count, 2, "Flashover's and Sleet Charge's at least");
            var conditionIds = new HashSet<string>();
            foreach (var mark in shapeMarks)
            {
                Assert.AreEqual(MarkTree.Duo, mark.Tree, mark.Id + " is drawn in the heart");
                Assert.IsEmpty(mark.Effects, mark.Id + ": a reaction's Shape mark does nothing of its own; its shapes do");
                Assert.IsNotNull(ReactionTable.MarkFor(HeartLayout.PairOf(mark)), mark.Id + " shapes a reaction that is built");
                foreach (var shape in mark.Shapes)
                {
                    var sets = new HashSet<string>(shape.Effects.Where(effect => effect.Do == UpgradeAction.Condition).Select(effect => effect.State.Id));
                    foreach (string id in sets) Assert.IsTrue(conditionIds.Add(id), $"{mark.Id}'s {shape.Name} sets '{id}', which another shape sets too");
                    foreach (var effect in shape.Effects)
                    {
                        CollectionAssert.Contains(reactionTriggers, effect.On, $"{mark.Id}'s {shape.Name} has an effect on {effect.On}");
                        Assert.AreSame(mark, effect.Owner);
                        if (effect.On != UpgradeTrigger.Reacted && effect.On != UpgradeTrigger.PickedUp && effect.On != UpgradeTrigger.PickupLeft)
                            Assert.IsTrue(sets.Contains(effect.Condition), $"{mark.Id}'s {shape.Name} answers '{effect.Condition}', which it never sets");
                    }
                }
            }
        }

        [Test]
        public void FlashoverAndSleetChargeChangeWithTheShapeWorn()
        {
            // The two built branches (task 4.5), on the game's own catalogue.
            int flashover = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Static);
            int sleet = AfflictionPairs.Index(AfflictionKind.Static, AfflictionKind.Chill);
            var learned = new HashSet<string> { Clover.Player.Trees.PlasmaTree.Shape, Clover.Player.Trees.Duo.SleetChargeDuo.Shape };
            var plasma = Hand(Elements.Fire, Elements.Lightning);
            Assert.AreEqual("Corona", ReactionTable.WornShape(flashover, plasma, learned).Name);
            Assert.IsTrue(ReactionTable.EffectsFor(flashover, plasma, learned).Any(effect => effect.Do == UpgradeAction.Condition && effect.State.Id == Clover.Player.Trees.PlasmaTree.Corona));
            Assert.IsFalse(ReactionTable.EffectsFor(flashover, plasma, learned).Any(effect => effect.Do == UpgradeAction.SetOff), "Corona spends nothing");
            SpellBranches.Choose(plasma.Shapes, ReactionTable.ShapeKey(flashover), 1);
            Assert.AreEqual("Fulgurite", ReactionTable.WornShape(flashover, plasma, learned).Name);
            Assert.AreEqual(2, ReactionTable.EffectsFor(flashover, plasma, learned).Count(effect => effect.Do == UpgradeAction.Hazard), "Fulgurite lays its patch");
            Assert.IsTrue(ReactionTable.EffectsFor(flashover, Hand(Elements.Fire, ""), learned).Any(effect => effect.Do == UpgradeAction.SetOff), "with Fire alone, the plain Flashover discharges");

            var storm = Hand(Elements.Ice, Elements.Lightning);
            Assert.AreEqual("Thundersnow", ReactionTable.WornShape(sleet, storm, learned).Name);
            SpellBranches.Choose(storm.Shapes, ReactionTable.ShapeKey(sleet), 1);
            var spear = ReactionTable.EffectsFor(sleet, storm, learned);
            Assert.IsTrue(spear.Any(effect => effect.On == UpgradeTrigger.ConditionEnded && effect.Do == UpgradeAction.Freeze),
                "Glass Spear freezes as the charge ends");
            Assert.AreEqual(0, storm.ShapeOf(ReactionTable.ShapeKey(flashover)), "each reaction keeps its own choice");
        }

        [Test]
        public void TheExileReturnsAndGivenBonesAnswerOnlyTheGrudgesBoltAndAshPlumeFallsFiveAtMost()
        {
            // The owner's answers of 2026-11-20: the two answer the Grudge's own bolt, in whichever shape calls it, and no
            // other discharge on a Cursed enemy; Ash Plume's storm is capped at five.
            var bolts = new List<string> { Clover.Player.Trees.Reactions.Cloud, Clover.Player.Trees.Duo.TheGrudgeDuo.FirstStrike, Clover.Player.Trees.Duo.TheGrudgeDuo.AshPlume };
            foreach (string id in new[] { Clover.Player.Trees.Duo.TheGrudgeDuo.TheExileReturns, Clover.Player.Trees.Duo.TheGrudgeDuo.GivenBones })
            {
                var mark = Upgrade.ById(id);
                Assert.IsNotEmpty(mark.Effects, id);
                foreach (var effect in mark.Effects)
                {
                    Assert.IsTrue(effect.AnyMark, id + " answers the cloud whoever set it");
                    Assert.AreNotEqual(UpgradeTrigger.SetOff, effect.On, id + " no longer answers any discharge");
                    Assert.Contains(effect.Condition, bolts, id + " answers the Grudge's bolt alone");
                }
                foreach (string bolt in bolts)
                    Assert.IsTrue(mark.Effects.Any(effect => effect.Condition == bolt), id + " answers the bolt of " + bolt);
                // First Strike's bolt is its heavy blow; the rest come on the enemy's blow or the storm's end.
                Assert.IsTrue(mark.Effects.Where(effect => effect.Condition == Clover.Player.Trees.Duo.TheGrudgeDuo.FirstStrike).All(effect => effect.On == UpgradeTrigger.ConditionStruck && effect.By == StruckBy.Heavy));
                // It counts the Static before the bolt spends it, and what follows the count is by it.
                int count = System.Array.FindIndex(mark.Effects, effect => effect.Do == UpgradeAction.Spend);
                Assert.IsTrue(count >= 0 && mark.Effects[count].CountOnly && mark.Effects[count].Affliction == AfflictionKind.Static, id);
                Assert.IsTrue(mark.Effects[count + 1].PerStack, id + "'s answer after the count is by the Static counted");
            }
            Assert.AreEqual(14f, Upgrade.ById(Clover.Player.Trees.Duo.TheGrudgeDuo.GivenBones).Effects.First(effect => effect.Do == UpgradeAction.Burst).Damage, "twice as hard: another 14 a Static");
            var plume = Upgrade.ById(Clover.Player.Trees.Duo.TheGrudgeDuo.Shape).Shapes.Single(shape => shape.Name == "Ash Plume");
            var storms = plume.Effects.Where(effect => effect.Do == UpgradeAction.Falling).ToList();
            Assert.AreEqual(2, storms.Count, "on the enemy's blow and at the end");
            Assert.IsTrue(storms.All(effect => effect.PerStack && effect.PerMost == 5), "five bolts at most");
        }

        private static int Pair(string name)
        {
            for (int pair = 0; pair < AfflictionPairs.Count; pair++)
                if (AfflictionPairs.Name(pair) == name) return pair;
            return -1;
        }

        [Test]
        public void AMarkChangesItsReactionsOwnNumbersInCopiesThatKeepTheOriginalsSlots()
        {
            // Dry Pods: one more seed, four at most, and lilies 2 s longer, in the plain Firebloom and Between Two Fires.
            int firebloom = Pair("Firebloom");
            var changers = ReactionNumbers.Changers(firebloom);
            Assert.AreEqual(1, changers.Length);
            Assert.AreEqual(Clover.Player.Trees.Duo.FirebloomDuo.DryPods, changers[0].Id);
            var written = ReactionNumbers.Written(firebloom, -1);
            var changed = ReactionNumbers.Changed(firebloom, 1, -1);
            Assert.AreEqual(5f, written[1].Seconds, "the reaction as written is left as it is");
            Assert.AreEqual(7f, changed[1].Seconds);
            Assert.AreEqual(1, changed[1].PerMore);
            Assert.AreEqual(4, changed[1].PerMost);
            for (int i = 0; i < written.Length; i++)
            {
                Assert.AreNotSame(written[i], changed[i]);
                Assert.AreEqual(written[i].Slot, changed[i].Slot, "a copy shares its original's slot");
                Assert.AreSame(written[i].Owner, changed[i].Owner, "and its mark, so a condition it sets is answered from the same mark");
            }
            Assert.AreEqual(7f, ReactionNumbers.Changed(firebloom, 1, 0)[1].Seconds, "Between Two Fires' lilies too");
            Assert.IsNull(ReactionNumbers.Changed(firebloom, 0, -1), "with no changer in force there is no copy");

            // Deep Swell: a condition's own seconds are changed on a copy of it, never on the original.
            int drowned = Pair("Drowned Weight");
            var swollen = ReactionNumbers.Changed(drowned, 1, -1)[0].State;
            var plain = ReactionNumbers.Written(drowned, -1)[0].State;
            Assert.AreNotSame(plain, swollen);
            Assert.AreEqual(plain.Id, swollen.Id, "the same condition, so its answers still find it");
            Assert.AreEqual(5f, plain.Seconds);
            Assert.AreEqual(7f, swollen.Seconds);
            Assert.AreEqual(5f, ReactionNumbers.Changed(drowned, 1, -1)[2].Radius, "the burst reaches 5 m");

            // A pair with no changer has none; a pair's changers keep their order.
            Assert.IsNull(ReactionNumbers.Changers(Pair("Steam")));
        }

        [Test]
        public void EveryReactionChangeNamesAnEffectThatIsThereAndChangesIt()
        {
            int marks = 0;
            foreach (var mark in Upgrade.All.Where(mark => mark.ReactionChanges != null))
            {
                marks++;
                Assert.IsTrue(mark.IsMix, mark.Id + " is a duo branch's mark");
                Assert.IsEmpty(mark.Effects, mark.Id + " changes its reaction's numbers and keeps no stand-in effects");
                int pair = HeartLayout.PairOf(mark);
                var changers = ReactionNumbers.Changers(pair);
                int bit = System.Array.IndexOf(changers, mark);
                Assert.GreaterOrEqual(bit, 0, mark.Id);
                foreach (var change in mark.ReactionChanges)
                {
                    var written = ReactionNumbers.Written(pair, change.Shape);
                    Assert.That(change.Index, Is.InRange(0, written.Length - 1), mark.Id + " names an effect that is there");
                    var copy = ReactionNumbers.Changed(pair, 1 << bit, change.Shape)[change.Index];
                    var before = written[change.Index];
                    bool differs = copy.Damage != before.Damage || copy.Stagger != before.Stagger || copy.Radius != before.Radius || copy.Reach != before.Reach
                        || copy.Seconds != before.Seconds || copy.Count != before.Count || copy.Stacks != before.Stacks || copy.Amount != before.Amount
                        || copy.PerMore != before.PerMore || copy.PerMost != before.PerMost || (copy.State != null && before.State != null && copy.State.Seconds != before.State.Seconds);
                    Assert.IsTrue(differs, mark.Id + "'s change at " + change.Shape + "." + change.Index + " changes a number");
                }
            }
            Assert.GreaterOrEqual(marks, 18, "the d5 minors and the 4.15 and 4.16 ones");
        }

        [Test]
        public void ThreeWeaselsIsThreePressesAndUnderTheBlackWaterChangesPlaces()
        {
            // second-press: the slot pressed again within 4 s does the next weasel, each press's effects in their own slots.
            var weasels = Upgrade.ById(Clover.Player.Trees.Duo.SickleWindDuo.ThreeWeasels);
            Assert.AreEqual(2, weasels.Presses.Length, "two presses after the cast: three weasels");
            Assert.AreEqual(4f, weasels.PressWindow);
            var second = weasels.Presses[0];
            Assert.IsTrue(second.Any(effect => effect.Do == UpgradeAction.Untouchable), "the pass is untouchable");
            Assert.IsTrue(second.Any(effect => effect.Do == UpgradeAction.Step && effect.Amount > 0f), "straight through the enemy");
            var cut = second.Single(effect => effect.Do == UpgradeAction.Burst);
            Assert.AreEqual(12f, cut.Damage);
            Assert.AreEqual(AfflictionKind.Sundered, cut.Affliction);
            var third = weasels.Presses[1];
            Assert.IsTrue(third.Any(effect => effect.Do == UpgradeAction.StepBack), "a step back out of reach");
            var bind = third.Single(effect => effect.Do == UpgradeAction.Heal);
            Assert.IsTrue(bind.SpendsAll && bind.PerStack && bind.Amount == 2f, "2 health for each blow since the first press");
            Assert.AreEqual(8, weasels.Tally.Cap, "eight at most");
            Assert.IsTrue(weasels.Effects.Any(effect => effect.On == UpgradeTrigger.SpellCast && effect.Do == UpgradeAction.ClearCount), "the first press starts the count again");
            Assert.IsTrue(weasels.Effects.Any(effect => effect.On == UpgradeTrigger.AnyHit && effect.Do == UpgradeAction.Bank));
            foreach (var effect in weasels.Presses.SelectMany(press => press))
            {
                Assert.AreSame(weasels, effect.Owner);
                Assert.GreaterOrEqual(effect.Slot, 0);
            }
            Assert.AreEqual(SpellForms.Rush, weasels.Form, "the first weasel is a rush of its own, not the step through");
            // place swaps: the enemy where you stood, you where it stood.
            var water = Upgrade.ById(Clover.Player.Trees.Duo.DrownedWeightDuo.UnderTheBlackWater);
            Assert.AreEqual(UpgradeAction.SwapPlaces, water.Effects[0].Do, "the change of places comes first, then what is done to it");
            Assert.IsTrue(water.Effects.Any(effect => effect.Do == UpgradeAction.Hazard && effect.Affliction == AfflictionKind.Cursed), "the pool curses as well as soaks");
            Assert.IsTrue(water.Effects.Any(effect => effect.Do == UpgradeAction.Boon && effect.Stat == PlayerStat.WeaponDamage && effect.Amount == 0.5f && effect.UntilBlow), "the next blow half again");
        }

        [Test]
        public void ThreeWeaselsFirstPressStopsInFrontAndTheSecondPassesThrough()
        {
            // The owner's answer of 2026-11-20, laid out as the Phase 4b probe has it: the player at z 2, the enemy at z 6.
            var weasels = Upgrade.ById(Clover.Player.Trees.Duo.SickleWindDuo.ThreeWeasels);
            float reach = weasels.Spell.Reach;
            float start = 2f, enemy = 6f;

            // The step form it used to be went 1 m through: z 2 to 7.
            Assert.AreEqual(7f, start + FormRules.StepRun(enemy - start, reach), 0.001f, "the old first press, for the record");

            // The first press: a rush that stops 1.2 m in front, still on the near side.
            Assert.IsInstanceOf<RushForm>(SpellForms.Get(weasels.Form));
            float rushed = start + FormRules.RushRun(enemy - start, reach);
            Assert.AreEqual(enemy - FormRules.RushShort, rushed, 0.001f, "just in front of the enemy");
            Assert.Less(rushed, enemy, "it does not pass through");
            Assert.AreEqual(start, start + FormRules.RushRun(1f, reach), 0.001f, "already that near, it stays put");
            Assert.AreEqual(start + reach, start + FormRules.RushRun(9f, reach), 0.001f, "farther than its reach, it runs its reach");

            // The rush throws the enemy 2 m; a body too great to be thrown stays. Either way the second press goes straight
            // through and out the far side, the distance its step names beyond.
            var pass = weasels.Presses[0].Single(effect => effect.Do == UpgradeAction.Step);
            foreach (float thrown in new[] { 0f, 2f })
            {
                float there = enemy + thrown;
                float passed = rushed + FormRules.MarkStep(there - rushed, pass.Reach, pass.Amount);
                Assert.Greater(passed, there, $"thrown {thrown} m: the second press comes out beyond the enemy");
                Assert.AreEqual(there + pass.Amount, passed, 0.001f, $"thrown {thrown} m: {pass.Amount} m beyond it");
            }

            // A mark's step with nothing beyond stops in front, as the rush does.
            Assert.AreEqual(4f - FormRules.RushShort, FormRules.MarkStep(4f, 8f, 0f), 0.001f);
        }
    }
}
