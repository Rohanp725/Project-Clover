using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The great chart as data: the ring of elements, the catalogue of marks and the rules every tree file must keep
    /// (ids, costs, what grows from what, where a mark may sit, that the stick can reach it), and the pattern tree the
    /// schema document gives, proved for all eleven elements at once.
    /// </summary>
    public class UpgradeChartTests
    {
        private static readonly Vector2[] Pushes =
        {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right,
            new Vector2(1f, 1f), new Vector2(-1f, 1f), new Vector2(1f, -1f), new Vector2(-1f, -1f),
        };

        // The worked layout of docs/UPGRADE_SCHEMA.md: a root, three boughs with their minors, and the god's gift at the crown.
        private static readonly (string slot, float x, float y)[] PatternTree =
        {
            ("root", 0f, 1.2f),
            ("L1", -1.21f, 2.4f), ("L2", -1.61f, 4.2f), ("LS", -2.0f, 6.0f), ("LK", -2.4f, 7.8f),
            ("la", -1.98f, 3.42f), ("lb", -2.4f, 5.23f), ("lc", -1.2f, 4.97f), ("ld", -2.8f, 7.03f), ("le", -1.6f, 6.77f),
            ("M1", 0f, 2.9f), ("M2", 0f, 4.7f), ("MS1", 0f, 6.5f), ("M3", 0f, 8.3f), ("MK", 0f, 10.1f), ("MS2", 0.75f, 9.3f),
            ("m1", 0.62f, 3.8f), ("m2", -0.62f, 5.75f), ("m3", 0.62f, 7.4f), ("m4", -0.62f, 9.2f),
            ("R1", 1.21f, 2.4f), ("R2", 1.61f, 4.2f), ("RS", 2.0f, 6.0f), ("RK", 2.4f, 7.8f),
            ("ra", 1.98f, 3.42f), ("rb", 2.4f, 5.23f), ("rc", 1.2f, 4.97f), ("rd", 2.8f, 7.03f), ("re", 1.6f, 6.77f),
            ("gift", 0f, 12f),
        };

        private static readonly (string slot, float x, float y)[] PatternMix =
        {
            ("x1", 0f, 1f), ("x2", 0.45f, 2f), ("x3", -0.45f, 3f), ("x4", 0.45f, 4f), ("x5", -0.45f, 5f), ("x6", 0.45f, 6f), ("xs", 0f, 7.2f), ("xgift", 0f, 8.6f),
        };

        private static bool Walkable(List<Vector2> places, int from, out int reachedCount)
        {
            var reached = new HashSet<int> { from };
            var toVisit = new Queue<int>();
            toVisit.Enqueue(from);
            while (toVisit.Count > 0)
            {
                int at = toVisit.Dequeue();
                foreach (var push in Pushes)
                {
                    int next = UpgradeTree.Step(places, at, push);
                    if (reached.Add(next)) toVisit.Enqueue(next);
                }
            }
            reachedCount = reached.Count;
            return reached.Count == places.Count;
        }

        [Test]
        public void ElevenElementsStandRoundTheRingEachWithItsColourAfflictionAndTheMixItSharesWithTheNext()
        {
            Assert.AreEqual(11, Elements.All.Count);
            string[] order = { "Fire", "Lightning", "Wind", "Ice", "Water", "Life", "Earth", "Metal", "Light", "Dark", "Poison" };
            string[] mixes = { "Plasma", "Tempest", "Blizzard", "Glacier", "Bloom", "Overgrowth", "Forge", "Halo", "Eclipse", "Blight", "Pyre" };
            var afflictions = new HashSet<AfflictionKind>();
            for (int i = 0; i < 11; i++)
            {
                var element = Elements.All[i];
                Assert.AreEqual(order[i], element.Name);
                Assert.AreEqual(i, element.Index);
                Assert.AreEqual(i * 360f / 11f, element.Bearing, 0.001f);
                Assert.AreEqual(order[(i + 1) % 11], element.MixWith);
                Assert.AreEqual(mixes[i], element.MixName);
                Assert.AreSame(element, Elements.ByName(element.Name));
                Assert.AreNotEqual(AfflictionKind.None, element.Affliction);
                Assert.IsTrue(afflictions.Add(element.Affliction), element.Name);
                Assert.AreEqual(Afflictions.Spec(element.Affliction).Colour, element.Colour, element.Name + " and its affliction share a colour");
                Assert.GreaterOrEqual(UpgradeIcons.IndexOf(element.Icon), 0, element.Name);
                Assert.AreEqual(mixes[i], Elements.MixName(element.Name, element.MixWith));
                Assert.AreEqual(mixes[i], Elements.MixName(element.MixWith, element.Name));
                Assert.AreEqual(element.Bearing + 180f / 11f, Elements.MixBearing(element.MixWith, element.Name), 0.001f);
            }
            Assert.IsNull(Elements.ByName("Mud"));
            Assert.AreEqual("", Elements.MixName(Elements.Fire, Elements.Ice));
            Assert.IsTrue(Elements.AreNeighbours(Elements.Poison, Elements.Fire));
            Assert.IsFalse(Elements.AreNeighbours(Elements.Fire, Elements.Water));
        }

        [Test]
        public void EveryElementHasAWorkingKitInItsOwnName()
        {
            foreach (var element in Elements.All)
            {
                var kit = ElementKit.ByName(element.Name, false);
                Assert.AreEqual(element.Name, kit.Name);
                Assert.IsNotEmpty(kit.SpellName);
                Assert.IsNotEmpty(kit.SurgeName);
                Assert.Greater(kit.SpellCost, 0f);
                Assert.Greater(kit.SpellDamage, 0f);
                Assert.Greater(kit.SpellReach, 0f);
                Assert.Greater(kit.SurgeSeconds, 0f);
                Assert.AreEqual(element.Affliction, kit.Affliction);
                var mana = new ManaPool();
                mana.Gain(100f);
                Assert.IsTrue(kit.TryCast(mana), element.Name);
                Assert.IsTrue(kit.TickCast(kit.SpellWindup + 0.01f), element.Name);
                Assert.IsTrue(kit.TrySurge(mana), element.Name);
                if (!string.IsNullOrEmpty(kit.SpellForm)) Assert.IsNotNull(SpellForms.Get(kit.SpellForm), element.Name + "'s base spell names a form that does not exist");
                Assert.IsTrue(kit.SurgeActive);
                // The nine new ones lay their affliction with the spell and with every blow of the surge.
                if (element.Name != Elements.Fire && element.Name != Elements.Lightning)
                {
                    Assert.Greater(kit.SpellStacks, 0, element.Name);
                    Assert.Greater(kit.SurgeStacks, 0, element.Name);
                }
            }
            Assert.AreEqual(Elements.Fire, ElementKit.ByName("", false).Name);
            // A cheaper spell and a longer surge, as the upgrades can make them.
            var cheap = ElementKit.Fire();
            var pool = new ManaPool();
            pool.Gain(20f);
            Assert.IsFalse(cheap.TryCast(pool));
            Assert.IsTrue(cheap.TryCast(pool, 0.8f));
            pool.Gain(100f);
            Assert.IsTrue(cheap.TrySurge(pool, 1f, 1.5f));
            Assert.AreEqual(cheap.SurgeSeconds * 1.5f, cheap.SurgeLeft, 0.001f);
            Assert.AreEqual(1f, cheap.SurgeFraction, 0.001f);
        }

        [Test]
        public void EveryMarkIsWholeItsOwnIdItsWordsAPictureThatExistsAndWhatItGrowsFrom()
        {
            var ids = new HashSet<string>();
            foreach (var upgrade in Upgrade.All)
            {
                Assert.IsTrue(ids.Add(upgrade.Id), "two marks share the id " + upgrade.Id);
                Assert.IsNotEmpty(upgrade.Name, upgrade.Id);
                Assert.IsNotEmpty(upgrade.Description, upgrade.Id);
                Assert.IsFalse(upgrade.Description.Contains("!"), upgrade.Id + " shouts");
                Assert.AreSame(upgrade, Upgrade.ById(upgrade.Id));
                Assert.IsNotNull(Elements.ByName(upgrade.Element), upgrade.Id + " names no element");
                Assert.GreaterOrEqual(UpgradeIcons.IndexOf(upgrade.Icon), 0, upgrade.Id + " names a picture that does not exist: " + upgrade.Icon);
                // A reaction is any pair's, on no tree: ReactionCatalogueTests checks its two elements.
                if (upgrade.IsMix && !upgrade.IsHidden)
                    Assert.AreEqual(upgrade.Element2, Elements.ByName(upgrade.Element).MixWith, upgrade.Id + ": a mix names the earlier element first and its neighbour second");
                if (!upgrade.IsHidden)
                    Assert.AreEqual(upgrade.Kind == UpgradeKind.Mix || (upgrade.Signature && upgrade.IsMix), upgrade.IsMix, upgrade.Id + ": only mix marks and a mix's own gift have a second element");
                foreach (string id in upgrade.Requires)
                {
                    var parent = Upgrade.ById(id);
                    Assert.IsNotNull(parent, upgrade.Id + " grows from something that does not exist: " + id);
                    Assert.IsFalse(parent.Signature, upgrade.Id + " grows from a god's gift");
                    Assert.IsTrue(parent.Element == upgrade.Element && parent.Element2 == upgrade.Element2, upgrade.Id + " grows from a mark on another tree");
                    Assert.Greater(upgrade.Y, parent.Y - 0.01f, upgrade.Id + " lies inward of " + id);
                }
                // What it costs: nothing for a god's gift, its kind's cost for the rest, three to five for a mix.
                if (upgrade.Signature) Assert.AreEqual(0, upgrade.Cost, upgrade.Id);
                else if (upgrade.Kind == UpgradeKind.Mix) Assert.That(upgrade.Cost, Is.InRange(3, 5), upgrade.Id);
                else Assert.AreEqual(Upgrade.UsualCost(upgrade.Kind), upgrade.Cost, upgrade.Id);
                // A spell names a form that exists and carries its numbers.
                if (upgrade.Kind == UpgradeKind.Spell) Assert.IsTrue(upgrade.IsSpell, upgrade.Id + " is a spell with no form");
                if (upgrade.IsSpell)
                {
                    Assert.IsNotNull(SpellForms.Get(upgrade.Form), upgrade.Id + " names a form that does not exist: " + upgrade.Form);
                    Assert.IsNotNull(upgrade.Spell, upgrade.Id + " has a form and no numbers");
                    // A spell is paid for: by its cooldown (basic spells), in mana (larger spells), or in health where its words say so.
                    Assert.Greater(upgrade.Spell.Cooldown + upgrade.Spell.Cost + upgrade.Spell.HealthCost, 0f, upgrade.Id);
                }
                foreach (var effect in upgrade.Effects)
                {
                    Assert.AreSame(upgrade, effect.Owner, upgrade.Id);
                    // What banks or spends a count needs a count to bank into.
                    if (effect.Do == UpgradeAction.Bank || effect.Spends > 0 || effect.SpendsAll) Assert.IsNotNull(upgrade.Tally, upgrade.Id + " banks or spends a count it does not keep");
                    Assert.GreaterOrEqual(effect.Slot, 0, upgrade.Id);
                    Assert.Less(effect.Slot, Upgrade.EffectCount, upgrade.Id);
                    Assert.That(effect.Chance, Is.InRange(0.01f, 1f), upgrade.Id);
                }
            }
            Assert.AreEqual(Upgrade.All.SelectMany(upgrade => upgrade.Effects).Count(), Upgrade.EffectCount);
            Assert.AreEqual(Upgrade.All.SelectMany(upgrade => upgrade.Effects).Select(effect => effect.Slot).Distinct().Count(), Upgrade.EffectCount);
        }

        // README.md ("Names and moved spells") frees the names of the plain minors the drafts cut, and the pair files reuse
        // one for a reaction. The minor stays until its tree is reshaped (Phase 4), so until then the two share it.
        private static readonly Dictionary<string, string> NamesFreedForAReaction = new Dictionary<string, string> { ["Quickening"] = "react.quickening" };

        [Test]
        public void NoTwoMarksShareAName()
        {
            var names = new Dictionary<string, string>();
            foreach (var upgrade in Upgrade.All)
            {
                // The reaction named for a freed minor is the one mark let share it.
                if (NamesFreedForAReaction.TryGetValue(upgrade.Name, out string reaction) && upgrade.Id == reaction) continue;
                Assert.IsFalse(names.TryGetValue(upgrade.Name, out string other), $"{upgrade.Id} and {other} are both called {upgrade.Name}");
                names[upgrade.Name] = upgrade.Id;
            }
        }

        [Test]
        public void TheKitsCarryTheNamesTheDesignsGiveTheirBaseSpellsAndSurges()
        {
            var named = new Dictionary<string, (string spell, string surge)>
            {
                { Elements.Fire, ("Flame Wave", "Kindle") }, { Elements.Lightning, ("Thunderclap", "Overdrive") }, { Elements.Wind, ("Gust", "Tailwind") },
                { Elements.Ice, ("Hoarfrost", "Glaze") }, { Elements.Water, ("Riptide", "Wellspring") }, { Elements.Life, ("Briar Patch", "Regrowth") },
                { Elements.Earth, ("Stone Rise", "Bedrock") }, { Elements.Metal, ("Flung Blade", "Temper") }, { Elements.Light, ("Glare", "Clarity") },
                { Elements.Dark, ("Grasp", "Hunger") }, { Elements.Poison, ("Miasma", "Fever") },
            };
            foreach (var element in Elements.All)
            {
                var kit = ElementKit.ByName(element.Name, false);
                Assert.AreEqual(named[element.Name].spell, kit.SpellName, element.Name);
                Assert.AreEqual(named[element.Name].surge, kit.SurgeName, element.Name);
            }
        }

        [Test]
        public void EveryElementHasItsTreeItsGiftAndTheCrownOfEachBranchItShares()
        {
            foreach (var element in Elements.All)
            {
                int own = Upgrade.All.Count(upgrade => upgrade.Element == element.Name && !upgrade.IsMix);
                Assert.That(own, Is.InRange(30, 40), element.Name + " has thirty to forty marks of its own");
                var gift = Gifts.SignatureOf(element.Name);
                Assert.IsNotNull(gift, element.Name + " has no god's gift");
                Assert.AreEqual(element.Name, gift.Element);
                Assert.IsFalse(gift.IsMix);
                var crown = Gifts.CrownOf(element.Name, element.MixWith);
                Assert.IsNotNull(crown, element.MixName + " has no crown");
                Assert.AreSame(crown, Gifts.CrownOf(element.MixWith, element.Name), "in either order of holding");
                Assert.IsTrue(crown.IsMix && crown.BelongsTo(element.Name) && crown.BelongsTo(element.MixWith));
                Assert.GreaterOrEqual(Upgrade.All.Count(upgrade => upgrade.Kind == UpgradeKind.Mix && upgrade.Element == element.Name), 6, element.MixName);
            }
            Assert.IsNull(Gifts.CrownOf(Elements.Fire, Elements.Water), "no branch between elements that are not neighbours");
            Assert.IsNull(Gifts.CrownOf(Elements.Fire, Elements.Fire));
            Assert.AreEqual(Upgrade.StormStep, Gifts.SignatureOf(Elements.Lightning).Id, "the storm's own gift is unchanged");
            Assert.AreEqual(Upgrade.Plasma, Gifts.CrownOf(Elements.Fire, Elements.Lightning).Id, "and Plasma is still what the storm leaves the fire");
        }

        // The cheapest road from the root to a mark: every mark on the way is paid for, taking the cheapest parent each time.
        private static int CheapestRoad(Upgrade upgrade, Dictionary<string, int> known)
        {
            if (known.TryGetValue(upgrade.Id, out int cost)) return cost;
            int least = 0;
            if (upgrade.Requires.Length > 0) least = upgrade.Requires.Min(id => CheapestRoad(Upgrade.ById(id), known));
            return known[upgrade.Id] = least + upgrade.Cost;
        }

        [Test]
        public void ARunCannotBuyAWholeTreeButCanReachOneKeystone()
        {
            // The castle and its god pay nineteen embers today.
            const int run = 19;
            var roads = new Dictionary<string, int>();
            foreach (var element in Elements.All)
            {
                var sold = Upgrade.All.Where(upgrade => upgrade.Element == element.Name && !upgrade.IsMix && !upgrade.Signature).ToList();
                int whole = sold.Sum(upgrade => upgrade.Cost);
                Assert.Greater(whole, run * 3, element.Name + ": the whole tree is far more than a run pays, so a path is chosen");
                var keystones = sold.Where(upgrade => upgrade.Kind == UpgradeKind.Keystone).ToList();
                Assert.AreEqual(3, keystones.Count, element.Name + " ends its three boughs in three keystones");
                int nearest = keystones.Min(keystone => CheapestRoad(keystone, roads));
                Assert.LessOrEqual(nearest, run + 3, $"{element.Name}: the cheapest road to a keystone costs {nearest}, within reach of about one area and its god");
                // And no run pays for two of them.
                int two = keystones.Select(keystone => CheapestRoad(keystone, roads)).OrderBy(c => c).Take(2).Sum() - Upgrade.UsualCost(UpgradeKind.Root);
                Assert.Greater(two, run, element.Name + ": two keystones are beyond one run");
            }
        }

        [Test]
        public void TheFirstSixteenKeepTheirIdsAndNames()
        {
            var first = new Dictionary<string, string>
            {
                { "fire.heat", "Heat" }, { "fire.detonate", "Burn and Detonate" }, { "fire.backdraft", "Backdraft" }, { "fire.trail", "Flame Trail" },
                { "fire.crescent", "Burning Crescent" }, { "fire.firebomb", "Firebomb Flask" }, { "fire.meteor", "Meteor Leap" },
                { "lightning.static", "Static" }, { "lightning.charge", "Stored Charge" }, { "lightning.blink", "Blink" }, { "lightning.afterimage", "Afterimage" },
                { "lightning.cuts", "Delayed Cuts" }, { "lightning.rod", "Lightning Rod" }, { "lightning.overclock", "Overclock" },
                { "lightning.stormstep", "Storm Step" }, { "fire.plasma", "Plasma" },
            };
            foreach (var pair in first)
            {
                var upgrade = Upgrade.ById(pair.Key);
                Assert.IsNotNull(upgrade, pair.Key);
                Assert.AreEqual(pair.Value, upgrade.Name);
            }
            // The two hold moves are spells now, worn in the hold-spell slot.
            Assert.AreEqual(SpellForms.Leap, Upgrade.ById(Upgrade.MeteorLeap).Form);
            Assert.AreEqual(SpellForms.Rod, Upgrade.ById(Upgrade.LightningRod).Form);
            Assert.IsTrue(Upgrade.ById(Upgrade.StormStep).Signature);
            Assert.IsTrue(Upgrade.ById(Upgrade.Plasma).Signature);
        }

        [Test]
        public void KindsHaveTheirUsualCosts()
        {
            Assert.AreEqual(1, Upgrade.UsualCost(UpgradeKind.Minor));
            Assert.AreEqual(2, Upgrade.UsualCost(UpgradeKind.Root));
            Assert.AreEqual(3, Upgrade.UsualCost(UpgradeKind.Major));
            Assert.AreEqual(4, Upgrade.UsualCost(UpgradeKind.Spell));
            Assert.AreEqual(6, Upgrade.UsualCost(UpgradeKind.Keystone));
            Assert.AreEqual(0, Upgrade.UsualCost(UpgradeKind.Signature));
        }

        [Test]
        public void EveryTreeIsOneRootWithEverythingElseGrowingFromSomethingAndCostsMoreThanARunPays()
        {
            foreach (var element in Elements.All)
            {
                var tree = Upgrade.All.Where(upgrade => upgrade.Element == element.Name && !upgrade.IsMix).ToList();
                if (tree.Count == 0) continue;
                var sold = tree.Where(upgrade => !upgrade.Signature).ToList();
                Assert.AreEqual(1, sold.Count(upgrade => upgrade.Kind == UpgradeKind.Root), element.Name + " has one root");
                foreach (var upgrade in sold)
                    Assert.AreEqual(upgrade.Kind == UpgradeKind.Root, upgrade.Requires.Length == 0, upgrade.Id + ": only the root grows from the foot of the tree");
                // A path must be chosen: the castle and its god give nineteen embers, and the whole tree costs more.
                Assert.Greater(sold.Sum(upgrade => upgrade.Cost), 19, element.Name);
                Assert.LessOrEqual(tree.Count(upgrade => upgrade.Signature), 1, element.Name + " has one god's gift at most");
            }
            // The two trees of the slice are there, grown by six marks each, and the branch they share.
            Assert.AreEqual(33, Upgrade.All.Count(upgrade => upgrade.Element == Elements.Fire && !upgrade.IsMix && !upgrade.Signature));
            Assert.AreEqual(35, Upgrade.All.Count(upgrade => upgrade.Element == Elements.Lightning && !upgrade.IsMix && !upgrade.Signature));
            Assert.AreEqual(9, Upgrade.All.Count(upgrade => upgrade.Kind == UpgradeKind.Mix && upgrade.Element == Elements.Fire && upgrade.Element2 == Elements.Lightning));
        }

        [Test]
        public void EveryMarkSitsInsideItsWedgeAndNoTwoMarksAnywhereSitTooNear()
        {
            // Hidden marks (the reactions) are never drawn, so they have no place.
            var all = Upgrade.All.Where(upgrade => !upgrade.IsHidden).ToList();
            foreach (var upgrade in all)
            {
                if (upgrade.IsMix)
                {
                    Assert.LessOrEqual(Mathf.Abs(upgrade.X), UpgradeTree.MixHalfWidth + 0.001f, upgrade.Id);
                    Assert.That(upgrade.Y, Is.InRange(1f, UpgradeTree.MixHeight), upgrade.Id);
                }
                else
                {
                    Assert.LessOrEqual(Mathf.Abs(upgrade.X), UpgradeTree.HalfWidth(upgrade.Y) + 0.001f, upgrade.Id + " is outside its tree's wedge");
                    Assert.That(upgrade.Y, Is.InRange(1f, UpgradeTree.TreeHeight), upgrade.Id);
                }
            }
            for (int i = 0; i < all.Count; i++)
                for (int j = i + 1; j < all.Count; j++)
                    Assert.GreaterOrEqual((UpgradeTree.OnChart(all[i]) - UpgradeTree.OnChart(all[j])).magnitude, UpgradeTree.LeastSpacing, all[i].Id + " and " + all[j].Id);
        }

        [Test]
        public void EveryMarkOpenToAnElementCanBeWalkedToFromItsRootWithTheStick()
        {
            foreach (var element in Elements.All)
            {
                // As the menu shows it: the element's own tree and the two branches it shares, with the tree upright.
                var mine = Upgrade.All.Where(upgrade => upgrade.BelongsTo(element.Name)).ToList();
                if (mine.Count == 0) continue;
                var places = mine.Select(upgrade => UpgradeTree.Upright(UpgradeTree.OnChart(upgrade), element.Bearing)).ToList();
                int root = mine.FindIndex(upgrade => upgrade.Kind == UpgradeKind.Root && !upgrade.IsMix);
                Assert.IsTrue(Walkable(places, Mathf.Max(0, root), out int reached), $"{element.Name}: the stick reaches {reached} of {places.Count}");
            }
        }

        [Test]
        public void StandingATreeUprightPutsItsOwnMarksStraightAboveTheMiddle()
        {
            foreach (var element in Elements.All)
            {
                Vector2 up = UpgradeTree.Upright(UpgradeTree.OnChart(element.Name, 0f, 3f), element.Bearing);
                Assert.AreEqual(0f, up.x, 0.001f, element.Name);
                Assert.AreEqual(UpgradeTree.EmblemDistance + 3f, up.y, 0.001f, element.Name);
                Vector2 right = UpgradeTree.Upright(UpgradeTree.OnChart(element.Name, 1f, 0f), element.Bearing);
                Assert.AreEqual(1f, right.x, 0.001f, element.Name + ": positive X is to the right of an upright tree");
            }
            // Fire grows straight up the chart; a quarter of the way round would grow to the right.
            Assert.AreEqual(0f, UpgradeTree.Emblem(Elements.Fire).x, 0.001f);
            Assert.Greater(UpgradeTree.Emblem(Elements.Wind).x, 0f);
        }

        [Test]
        public void ThePatternTreeOfTheSchemaFillsAllElevenElementsAndEveryMixWithoutACrowdAndTheStickReachesAll()
        {
            Assert.AreEqual(30, PatternTree.Length);
            var chart = new List<(string name, string element, string element2, Vector2 place)>();
            foreach (var element in Elements.All)
            {
                foreach (var (slot, x, y) in PatternTree)
                {
                    Assert.LessOrEqual(Mathf.Abs(x), UpgradeTree.HalfWidth(y) + 0.001f, slot);
                    chart.Add((element.Name + "." + slot, element.Name, "", UpgradeTree.OnChart(element.Name, x, y)));
                }
                foreach (var (slot, x, y) in PatternMix)
                {
                    Assert.LessOrEqual(Mathf.Abs(x), UpgradeTree.MixHalfWidth + 0.001f, slot);
                    chart.Add((element.MixName + "." + slot, element.Name, element.MixWith, UpgradeTree.OnChart(Elements.MixBearing(element.Name, element.MixWith), UpgradeTree.MixDistance, x, y)));
                }
            }
            Assert.AreEqual(11 * 38, chart.Count);
            for (int i = 0; i < chart.Count; i++)
                for (int j = i + 1; j < chart.Count; j++)
                    Assert.GreaterOrEqual((chart[i].place - chart[j].place).magnitude, UpgradeTree.LeastSpacing, chart[i].name + " and " + chart[j].name);
            foreach (var element in Elements.All)
            {
                var places = chart.Where(mark => mark.element == element.Name || mark.element2 == element.Name)
                    .Select(mark => UpgradeTree.Upright(mark.place, element.Bearing)).ToList();
                Assert.AreEqual(30 + 8 + 8, places.Count);
                Assert.IsTrue(Walkable(places, 0, out int reached), $"{element.Name}: the stick reaches {reached} of {places.Count}");
            }
        }

        [Test]
        public void AMarkIsReachableOnceAnyOneOfWhatItGrowsFromIsLearned()
        {
            var learned = new HashSet<string>();
            Assert.IsTrue(UpgradeTree.Reachable(Upgrade.ById(FireTree.Cinder), learned));
            Assert.IsFalse(UpgradeTree.Reachable(Upgrade.ById(Upgrade.Backdraft), learned));
            learned.Add(FireTree.Cinder);
            Assert.IsTrue(UpgradeTree.Reachable(Upgrade.ById(Upgrade.Backdraft), learned));
            Assert.IsFalse(UpgradeTree.Reachable(Upgrade.ById(Upgrade.BurnAndDetonate), learned));
            // Chain Lance grows from Overclock or from Capacitor: either opens it.
            var lance = Upgrade.ById(LightningTree.ChainLance);
            Assert.AreEqual(2, lance.Requires.Length);
            Assert.IsFalse(UpgradeTree.Reachable(lance, learned));
            learned.Add(LightningTree.Capacitor);
            Assert.IsTrue(UpgradeTree.Reachable(lance, learned));
            Assert.IsFalse(UpgradeTree.Reachable(null, learned));
        }

        [Test]
        public void AMixedBranchOpensToOneWhoHoldsEitherElementAndHasHeldOrConsumedTheOther()
        {
            var mark = Upgrade.ById(PlasmaTree.IonisedAir);
            var none = new HashSet<string>();
            var held = new HashSet<string> { Elements.Fire };
            Assert.IsTrue(mark.BelongsTo(Elements.Fire));
            Assert.IsTrue(mark.BelongsTo(Elements.Lightning));
            Assert.IsFalse(mark.BelongsTo(Elements.Wind));
            Assert.IsFalse(UpgradeTree.MixOpen(mark, Elements.Fire, held, none), "Fire alone does not open Plasma");
            Assert.IsTrue(UpgradeTree.MixOpen(mark, Elements.Fire, held, new HashSet<string> { Elements.Lightning }), "consumed");
            held.Add(Elements.Lightning);
            Assert.IsTrue(UpgradeTree.MixOpen(mark, Elements.Fire, held, none), "held earlier in the run");
            Assert.IsTrue(UpgradeTree.MixOpen(mark, Elements.Lightning, held, none), "and from the other side");
            Assert.IsFalse(UpgradeTree.MixOpen(mark, Elements.Wind, held, none));
            Assert.IsFalse(UpgradeTree.MixOpen(Upgrade.ById(Upgrade.Heat), Elements.Fire, held, none), "a tree's own mark is not a mix");
        }

        [Test]
        public void EveryTreeFileIsGatheredIntoTheCatalogue()
        {
            var files = typeof(UpgradeTrees).Assembly.GetTypes()
                .Where(type => type.Namespace == typeof(UpgradeTrees).Namespace && type != typeof(UpgradeTrees) && type.IsAbstract && type.IsSealed).ToList();
            Assert.GreaterOrEqual(files.Count, 3);
            foreach (var file in files)
            {
                var add = file.GetMethod("Add", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(List<Upgrade>) }, null);
                Assert.IsNotNull(add, file.Name + " has no public static Add(List<Upgrade>)");
                var own = new List<Upgrade>();
                add.Invoke(null, new object[] { own });
                Assert.Greater(own.Count, 0, file.Name);
                foreach (var upgrade in own)
                    Assert.IsNotNull(Upgrade.ById(upgrade.Id), file.Name + " is not listed in UpgradeTrees.AddAll: " + upgrade.Id + " is missing from the catalogue");
            }
        }

        [Test]
        public void TheFourFirstSpellFormsAreInTheLibraryUnderTheirNames()
        {
            foreach (string id in new[] { SpellForms.Wave, SpellForms.Bolt, SpellForms.Leap, SpellForms.Rod })
            {
                var form = SpellForms.Get(id);
                Assert.IsNotNull(form, id);
                Assert.AreEqual(id, form.Id);
            }
            Assert.AreEqual(SpellForms.All.Count, SpellForms.All.Select(form => form.Id).Distinct().Count());
            Assert.IsTrue(SpellForms.Get(SpellForms.Leap) is ISpellLanding);
            Assert.IsNull(SpellForms.Get("nothing"));
        }

        [Test]
        public void EveryPictureNameIsListedOnceAndEachElementHasItsOwn()
        {
            Assert.AreEqual(UpgradeIcons.All.Count, UpgradeIcons.All.Distinct().Count());
            Assert.GreaterOrEqual(UpgradeIcons.All.Count, 30);
            Assert.AreEqual(11, Elements.All.Select(element => element.Icon).Distinct().Count());
            Assert.AreEqual(-1, UpgradeIcons.IndexOf("nothing"));
        }
    }
}
