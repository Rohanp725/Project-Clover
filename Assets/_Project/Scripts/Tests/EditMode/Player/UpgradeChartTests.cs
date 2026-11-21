using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Magic;
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
        public void NoMarkTellsOfTheOldSpellButtonOrAPriceItDoesNotAsk()
        {
            // Spells are cast from the four slots and most wait on a cooldown; a price is named only where mana is paid, and it is the table's.
            foreach (var upgrade in Upgrade.All)
            {
                StringAssert.DoesNotContain("spell button", upgrade.Description, upgrade.Id);
                var price = System.Text.RegularExpressions.Regex.Match(upgrade.Description, @"(\d+) mana\.$");
                if (!price.Success) continue;
                Assert.IsTrue(upgrade.IsSpell && upgrade.Spell != null, upgrade.Id + " names a price but is no spell");
                Assert.AreEqual(float.Parse(price.Groups[1].Value), upgrade.Spell.Cost, 1e-3f, upgrade.Id + " names a price it does not ask");
            }
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
                // A mark of the Way or the Body names an element only when it works while that element is carried.
                if (!upgrade.IsWay || !string.IsNullOrEmpty(upgrade.Element))
                    Assert.IsNotNull(Elements.ByName(upgrade.Element), upgrade.Id + " names no element");
                Assert.GreaterOrEqual(UpgradeIcons.IndexOf(upgrade.Icon), 0, upgrade.Id + " names a picture that does not exist: " + upgrade.Icon);
                // A reaction is any pair's, on no tree: ReactionCatalogueTests checks its two elements.
                // A duo branch in the heart is any pair's, in either order (task 4.5); a mix still on the ring is a neighbour's.
                if (upgrade.IsMix && !upgrade.IsHidden && upgrade.Tree == MarkTree.Duo)
                    Assert.GreaterOrEqual(HeartLayout.PairOf(upgrade), 0, upgrade.Id + ": a duo mark names two different elements");
                else if (upgrade.IsMix && !upgrade.IsHidden)
                    Assert.AreEqual(upgrade.Element2, Elements.ByName(upgrade.Element).MixWith, upgrade.Id + ": a mix names the earlier element first and its neighbour second");
                if (!upgrade.IsHidden)
                    Assert.AreEqual(upgrade.Kind == UpgradeKind.Mix || (upgrade.Signature && upgrade.IsMix), upgrade.IsMix, upgrade.Id + ": only mix marks and a mix's own gift have a second element");
                foreach (string id in upgrade.Requires)
                {
                    var parent = Upgrade.ById(id);
                    Assert.IsNotNull(parent, upgrade.Id + " grows from something that does not exist: " + id);
                    Assert.IsFalse(parent.Signature, upgrade.Id + " grows from a god's gift");
                    if (upgrade.IsWay)
                        Assert.IsTrue(parent.Tree == upgrade.Tree && parent.Weapon == upgrade.Weapon && parent.Aspect == upgrade.Aspect, upgrade.Id + " grows from a mark of another Way or bough");
                    else
                        Assert.IsTrue(parent.Element == upgrade.Element && parent.Element2 == upgrade.Element2, upgrade.Id + " grows from a mark on another tree");
                    Assert.Greater(upgrade.Y, parent.Y - 0.01f, upgrade.Id + " lies inward of " + id);
                }
                // What it costs: nothing for a god's gift or a given mark, its kind's cost for the rest, one to five for a mix (a duo
                // branch's minor is 1, pairs_d1_d2.md).
                if (upgrade.Signature || upgrade.Currency == Currency.None) Assert.AreEqual(0, upgrade.Cost, upgrade.Id);
                else if (upgrade.Kind == UpgradeKind.Mix) Assert.That(upgrade.Cost, Is.InRange(1, 5), upgrade.Id);
                // The Way's temper ranks are 1, 1, 2, 2 and 3 Memories (steel/weapons.md 0.2); every other Way mark its kind's price in Memories.
                else if (upgrade.IsWay && upgrade.Kind == UpgradeKind.Temper) Assert.That(upgrade.Cost, Is.InRange(1, 3), upgrade.Id);
                else Assert.AreEqual(Upgrade.UsualCost(upgrade), upgrade.Cost, upgrade.Id);
                // The Way and the Body are bought with Memories, or given; nothing else is.
                if (upgrade.IsWay) Assert.AreNotEqual(Currency.Shards, upgrade.Currency, upgrade.Id + ": the Way and the Body are bought with Memories");
                else Assert.AreNotEqual(Currency.Memories, upgrade.Currency, upgrade.Id + ": Memories buy only the Way and the Body");
                // A spell names a form that exists and carries its numbers; an element's base spell is cast by its kit instead.
                if (upgrade.Kind == UpgradeKind.Spell && !upgrade.IsBaseSpell) Assert.IsTrue(upgrade.IsSpell, upgrade.Id + " is a spell with no form");
                if (upgrade.IsBaseSpell) Assert.IsFalse(upgrade.IsSpell, upgrade.Id + ": a base spell's Learn is cast by its kit, not a form of its own");
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
            // A Shape mark's two shapes carry effects of their own, each with its slot and its Shape mark as owner; so do a
            // spell's later presses (Three Weasels).
            var every = Upgrade.All.SelectMany(upgrade => upgrade.Effects.Concat(upgrade.Shapes.SelectMany(shape => shape.Effects))
                .Concat(upgrade.Presses != null ? upgrade.Presses.SelectMany(press => press) : Enumerable.Empty<UpgradeEffect>())).ToList();
            foreach (var upgrade in Upgrade.All.Where(upgrade => upgrade.Presses != null))
                foreach (var effect in upgrade.Presses.SelectMany(press => press))
                    Assert.AreSame(upgrade, effect.Owner, upgrade.Id + "'s press");
            foreach (var upgrade in Upgrade.All)
                foreach (var effect in upgrade.Shapes.SelectMany(shape => shape.Effects))
                    Assert.AreSame(upgrade, effect.Owner, upgrade.Id + "'s shape");
            Assert.AreEqual(every.Count, Upgrade.EffectCount);
            Assert.AreEqual(every.Select(effect => effect.Slot).Distinct().Count(), Upgrade.EffectCount);
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
        public void EveryElementHasItsTreeAndItsGiftAndNoGodGivesADuoCrown()
        {
            foreach (var element in Elements.All)
            {
                int own = Upgrade.All.Count(upgrade => upgrade.Element == element.Name && !upgrade.IsMix && !upgrade.IsWay);
                Assert.That(own, Is.InRange(30, 40), element.Name + " has thirty to forty marks of its own");
                var gift = Gifts.SignatureOf(element.Name);
                Assert.IsNotNull(gift, element.Name + " has no god's gift");
                Assert.AreEqual(element.Name, gift.Element);
                Assert.IsFalse(gift.IsMix);
                // A branch reshaped into the heart is four to eight marks (pairs_d1_d2.md).
                Assert.GreaterOrEqual(Upgrade.All.Count(upgrade => upgrade.Kind == UpgradeKind.Mix && !upgrade.IsHidden && upgrade.BelongsTo(element.Name) && upgrade.BelongsTo(element.MixWith)), 4, element.MixName);
                // An element's base spell (its branch's Learn) comes free with the element, and is never sold (the 4.1 default, the owner's of 2026-11-20).
                var learn = Upgrade.ById(SpellSlots.BaseId(element.Name));
                if (learn != null)
                {
                    Assert.IsTrue(learn.Given, learn.Id + " comes with its element");
                    Assert.IsFalse(MarkRules.CanBuy(learn, new HashSet<string>(), 999, true, true), learn.Id + " is never sold");
                }
            }
            Assert.AreEqual(Upgrade.StormStep, Gifts.SignatureOf(Elements.Lightning).Id, "the storm's own gift is unchanged");

            // The owner's decision of 2026-11-20: consuming a god gives no duo crown. No duo mark is a god's gift, and every
            // duo branch's top marks (the old crowns too) are bought for 5 Shards once both elements are carried.
            Assert.IsFalse(Upgrade.All.Any(upgrade => upgrade.Signature && upgrade.IsMix), "no duo mark is a god's gift");
            var tops = new[]
            {
                Upgrade.Plasma, TempestTree.FallingThunder, BlizzardTree.Gift, GlacierTree.Gift, BloomTree.Gift, OvergrowthTree.TheGroundWakes,
                ForgeTree.WorldAnvil, HaloTree.SixMirrors, EclipseTree.Totality, BlightTree.TheReaping, PyreTree.LightThePyres,
            };
            foreach (string id in tops)
            {
                var top = Upgrade.ById(id);
                Assert.IsNotNull(top, id);
                Assert.AreEqual(UpgradeKind.Mix, top.Kind, id);
                Assert.AreEqual(5, top.Cost, id + " costs 5 Shards");
                Assert.IsFalse(top.Given, id + " is bought, not given");
                var learned = new HashSet<string>(top.Requires.Take(1));
                Assert.IsTrue(MarkRules.CanBuy(top, learned, 5, true, false), id + " is sold once what it grows from is learned");
                var both = new Clover.Player.Build.Loadout();
                both.Attune(top.Element, top.Element2);
                Assert.IsTrue(MarkRules.Open(top, both, false), id + " is open with both carried");
                var one = new Clover.Player.Build.Loadout();
                one.Attune(top.Element, "");
                Assert.IsFalse(MarkRules.Open(top, one, false), id + " is shut with one");
            }
            Assert.IsTrue(Upgrade.ById(HaloTree.SixMirrors).IsSpell && Upgrade.ById(PyreTree.LightThePyres).IsSpell, "Six Mirrors and Light the Pyres are their branches' duo spells");
            // Every duo branch's duo spell, the new ones too, is a mark bought with Shards.
            foreach (var spell in Upgrade.All.Where(upgrade => upgrade.IsMix && upgrade.IsSpell && !upgrade.IsHidden))
                Assert.IsTrue(spell.Kind == UpgradeKind.Mix && spell.Cost == 5 && !spell.Given, spell.Id + " is a duo spell for 5 Shards");
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
                var sold = Upgrade.All.Where(upgrade => upgrade.Element == element.Name && !upgrade.IsMix && !upgrade.IsWay && !upgrade.Signature).ToList();
                int whole = sold.Sum(upgrade => upgrade.Cost);
                Assert.Greater(whole, run * 3, element.Name + ": the whole tree is far more than a run pays, so a path is chosen");
                var keystones = sold.Where(upgrade => upgrade.Kind == UpgradeKind.Keystone).ToList();
                // A tree reshaped into spell branches keeps one to three keystones beside its Devotion (elements_a.md, "What to
                // cut", 10, widened by the owner's word of 2026-11-20 to allow three).
                bool reshaped = sold.Any(upgrade => upgrade.Part != BranchPart.None);
                if (reshaped) Assert.That(keystones.Count, Is.InRange(1, 3), element.Name + " keeps one to three keystones beside its Devotion");
                else Assert.AreEqual(3, keystones.Count, element.Name + " ends its three boughs in three keystones");
                int nearest = keystones.Min(keystone => CheapestRoad(keystone, roads));
                Assert.LessOrEqual(nearest, run + 3, $"{element.Name}: the cheapest road to a keystone costs {nearest}, within reach of about one area and its god");
                // And no run pays for two of them.
                if (keystones.Count < 2) continue;
                int two = keystones.Select(keystone => CheapestRoad(keystone, roads)).OrderBy(c => c).Take(2).Sum() - Upgrade.UsualCost(UpgradeKind.Root);
                Assert.Greater(two, run, element.Name + ": two keystones are beyond one run");
            }
        }

        [Test]
        public void TheFirstSixteenKeepTheirIdsOrAreRetiredOrMoving()
        {
            // Phase 4 kept every mark the design keeps under its id (Backdraft is Breath of the Lioness now), retired the cut
            // ones (Burn and Detonate, folded into it; Overclock) and sent the Katana's two to its Way, where phase 5 took them
            // back under the same ids.
            var first = new Dictionary<string, string>
            {
                { "fire.heat", "Heat" }, { "fire.backdraft", "Breath of the Lioness" }, { "fire.trail", "Flame Trail" },
                { "fire.firebomb", "Firebomb Flask" }, { "fire.meteor", "Meteor Leap" },
                { "lightning.static", "Static" }, { "lightning.charge", "Stored Charge" }, { "lightning.blink", "Blink" }, { "lightning.afterimage", "Afterimage" },
                { "lightning.rod", "Lightning Rod" }, { "lightning.stormstep", "Storm Step" }, { "fire.plasma", "Plasma" },
            };
            Assert.IsTrue(RetiredMarks.IsRetired(Upgrade.BurnAndDetonate));
            Assert.IsTrue(RetiredMarks.IsRetired(Upgrade.Overclock));
            Assert.IsTrue(RetiredMarks.IsMoving(Upgrade.BurningCrescent));
            Assert.IsTrue(RetiredMarks.IsMoving(Upgrade.DelayedCuts));
            foreach (string gone in new[] { Upgrade.BurnAndDetonate, Upgrade.Overclock })
                Assert.IsNull(Upgrade.ById(gone), gone);
            foreach (string moved in new[] { Upgrade.BurningCrescent, Upgrade.DelayedCuts })
            {
                var mark = Upgrade.ById(moved);
                Assert.IsNotNull(mark, moved + " has its home on the Katana's Way");
                Assert.AreEqual(MarkTree.Way, mark.Tree, moved);
                Assert.AreEqual(Weapons.KatanaName, mark.Weapon, moved);
                Assert.IsEmpty(mark.Aspect, moved + " is on the trunk");
            }
            Assert.AreEqual("Burning Crescent", Upgrade.ById(Upgrade.BurningCrescent).Name);
            Assert.AreEqual("Delayed Cuts", Upgrade.ById(Upgrade.DelayedCuts).Name);
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
            // Plasma is bought with Shards since gods give no duo crowns (the owner's decision of 2026-11-20).
            Assert.IsFalse(Upgrade.ById(Upgrade.Plasma).Signature);
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
                var tree = Upgrade.All.Where(upgrade => upgrade.Element == element.Name && !upgrade.IsMix && !upgrade.IsWay).ToList();
                if (tree.Count == 0) continue;
                var sold = tree.Where(upgrade => !upgrade.Signature).ToList();
                Assert.AreEqual(1, sold.Count(upgrade => upgrade.Kind == UpgradeKind.Root), element.Name + " has one root");
                foreach (var upgrade in sold)
                    Assert.AreEqual(upgrade.Kind == UpgradeKind.Root, upgrade.Requires.Length == 0, upgrade.Id + ": only the root grows from the foot of the tree");
                // A path must be chosen: the castle and its god give nineteen embers, and the whole tree costs more.
                Assert.Greater(sold.Sum(upgrade => upgrade.Cost), 19, element.Name);
                Assert.LessOrEqual(tree.Count(upgrade => upgrade.Signature), 1, element.Name + " has one god's gift at most");
            }
            // The two trees reshaped into spell branches (Phase 4): six branches of four and their passives, and the branch they
            // share, reshaped as a duo branch (task 4.5): the Shape mark, Twin Break, Ionised Air, Chain Reaction, Meltdown and
            // Ball Lightning, with Plasma at its top.
            // Fire has its two new keystones, Into the Fire and The Blade Drinks Fire (the owner's picks of 2026-11-20).
            Assert.AreEqual(35, Upgrade.All.Count(upgrade => upgrade.Element == Elements.Fire && !upgrade.IsMix && !upgrade.IsWay && !upgrade.Signature && !upgrade.IsHidden));
            Assert.AreEqual(32, Upgrade.All.Count(upgrade => upgrade.Element == Elements.Lightning && !upgrade.IsMix && !upgrade.IsWay && !upgrade.Signature && !upgrade.IsHidden));
            // Plasma Lance, the spare spell kept hidden for the Evolutions, is on no chart; Plasma itself is a duo mark now.
            Assert.AreEqual(7, Upgrade.All.Count(upgrade => upgrade.Kind == UpgradeKind.Mix && !upgrade.IsHidden && upgrade.Element == Elements.Fire && upgrade.Element2 == Elements.Lightning));
        }

        [Test]
        public void EveryMarkSitsInsideItsWedgeAndNoTwoMarksAnywhereSitTooNear()
        {
            // Hidden marks (the reactions) are never drawn, so they have no place; the heart's duo branches are drawn one pair
            // at a time in the ring's middle (TheHeartsBranchesSitOnItsPatternClearOfEachOther).
            var all = Upgrade.All.Where(upgrade => !upgrade.IsHidden && upgrade.Tree != MarkTree.Duo && !upgrade.IsWay).ToList();
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
                // As the menu shows it: the element's own tree and the ring's branches it shares, with the tree upright, and in
                // the heart the duo branch of each partner built there, one at a time.
                var mine = Upgrade.All.Where(upgrade => upgrade.BelongsTo(element.Name) && RingLayout.Instance.Draws(upgrade)).ToList();
                if (mine.Count == 0) continue;
                var places = mine.Select(upgrade => UpgradeTree.Upright(UpgradeTree.OnChart(upgrade), element.Bearing)).ToList();
                int root = mine.FindIndex(upgrade => upgrade.Kind == UpgradeKind.Root && !upgrade.IsMix);
                Assert.IsTrue(Walkable(places, Mathf.Max(0, root), out int reached), $"{element.Name}: the stick reaches {reached} of {places.Count}");
                foreach (var partner in Elements.All)
                {
                    int pair = HeartLayout.PairOf(element.Name, partner.Name);
                    var heart = Upgrade.All.Where(upgrade => HeartLayout.Instance.Shows(upgrade, pair)).ToList();
                    if (heart.Count == 0) continue;
                    var withHeart = places.Concat(heart.Select(upgrade => HeartLayout.Instance.Place(upgrade))).ToList();
                    Assert.IsTrue(Walkable(withHeart, Mathf.Max(0, root), out int got), $"{element.Name} with {partner.Name}'s branch in the heart: the stick reaches {got} of {withHeart.Count}");
                }
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
        public void TheSpellBranchPatternFillsAllElevenElementsAndEveryMixWithoutACrowdAndTheStickReachesAll()
        {
            // The worked layout of docs/UPGRADE_SCHEMA.md 20.4: three boughs, two stalks of four on each, passives on the boughs and trunk.
            var pattern = BranchPattern.All;
            // 36 places, and the top of the trunk added for the marks the owner kept on 2026-11-20.
            Assert.AreEqual(37, pattern.Count);
            Assert.AreEqual(pattern.Count, pattern.Select(place => place.Name).Distinct().Count());
            foreach (var place in pattern)
            {
                Assert.LessOrEqual(Mathf.Abs(place.X), UpgradeTree.HalfWidth(place.Y) * 0.9f + 0.001f, place.Name + " is too near its tree's edge");
                Assert.That(place.Y, Is.InRange(1f, UpgradeTree.TreeHeight), place.Name);
                if (place.From.Length == 0) continue;
                var parent = BranchPattern.Of(place.From);
                Assert.Greater(place.Y, parent.Y - 0.01f, place.Name + " lies inward of what it grows from");
            }
            foreach (string stalk in new[] { BranchPattern.LeftA, BranchPattern.LeftB, BranchPattern.MiddleA, BranchPattern.MiddleB, BranchPattern.RightA, BranchPattern.RightB })
            {
                var four = BranchPattern.Stalk(stalk);
                for (int i = 1; i < 4; i++) Assert.Greater(four[i].Y, four[i - 1].Y, stalk + " grows outward in order");
            }
            var chart = new List<(string name, string element, string element2, Vector2 place)>();
            foreach (var element in Elements.All)
            {
                foreach (var place in pattern)
                    chart.Add((element.Name + "." + place.Name, element.Name, "", UpgradeTree.OnChart(element.Name, place.X, place.Y)));
                foreach (var (slot, x, y) in PatternMix)
                    chart.Add((element.MixName + "." + slot, element.Name, element.MixWith, UpgradeTree.OnChart(Elements.MixBearing(element.Name, element.MixWith), UpgradeTree.MixDistance, x, y)));
            }
            for (int i = 0; i < chart.Count; i++)
                for (int j = i + 1; j < chart.Count; j++)
                    Assert.GreaterOrEqual((chart[i].place - chart[j].place).magnitude, UpgradeTree.LeastSpacing, chart[i].name + " and " + chart[j].name);
            foreach (var element in Elements.All)
            {
                var places = chart.Where(mark => mark.element == element.Name || mark.element2 == element.Name)
                    .Select(mark => UpgradeTree.Upright(mark.place, element.Bearing)).ToList();
                Assert.AreEqual(37 + 8 + 8, places.Count);
                Assert.IsTrue(Walkable(places, 0, out int reached), $"{element.Name}: the stick reaches {reached} of {places.Count}");
            }
        }

        // The trees written as spell branches (Phase 4), with how many passives each design gives (its root, majors,
        // keystones, Devotion, lift and gift). A tree is added here when it is reshaped.
        private static readonly Dictionary<string, int> ReshapedPassives = new Dictionary<string, int>
        {
            // Fire, Water and Life have three keystones since the owner's picks of 2026-11-20 (keystones_fire_water_life.md).
            [Elements.Fire] = 12,
            [Elements.Lightning] = 9,
            // Wind and Ice keep Hammer Down and Clean Break as marks (the owner's answer of 2026-11-20).
            [Elements.Wind] = 11,
            [Elements.Ice] = 11,
            // Water and Life keep Spilled Water and Shade as marks too.
            [Elements.Water] = 13,
            [Elements.Life] = 13,
            // Earth's table gives three keystones, and it keeps all three since the owner allowed it (2026-11-20).
            [Elements.Earth] = 10,
            // Metal's table gives three keystones too: Edge Guard, Master's Answer and All Blades Home.
            [Elements.Metal] = 10,
            // Light's table gives three keystones as well: Unblemished, Condemned and Two Suns.
            [Elements.Light] = 10,
            // Dark's table gives two keystones, Never Sated and Dead Already, and four majors.
            [Elements.Dark] = 10,
            // Poison's table gives three keystones: Slow Death, Volatile and Plague Bearer.
            [Elements.Poison] = 10,
        };

        [Test]
        public void EveryTreeWithSpellBranchesIsListedAsReshaped()
        {
            foreach (var element in Elements.All)
            {
                bool branches = Upgrade.All.Any(mark => mark.Element == element.Name && !mark.IsMix && mark.Part != BranchPart.None);
                Assert.AreEqual(branches, ReshapedPassives.ContainsKey(element.Name), element.Name + ": a reshaped tree is listed in ReshapedPassives, and only one");
            }
        }

        [Test]
        public void TheReshapedTreesSitOnThePatternEachMarkInAPlaceOfItsOwn()
        {
            foreach (string element in ReshapedPassives.Keys)
            {
                var tree = Upgrade.All.Where(mark => mark.Element == element && !mark.IsMix && !mark.IsWay && !mark.IsHidden).ToList();
                var taken = new HashSet<string>();
                foreach (var mark in tree)
                {
                    var place = BranchPattern.All.FirstOrDefault(p => Mathf.Abs(p.X - mark.X) < 0.001f && Mathf.Abs(p.Y - mark.Y) < 0.001f);
                    Assert.IsNotNull(place.Name, mark.Id + " sits on no place of the pattern");
                    Assert.IsTrue(taken.Add(place.Name), mark.Id + " shares the place " + place.Name);
                }
            }
        }

        [Test]
        public void NewKindsHaveTheSharedPricesAndAGivenMarkCostsNothing()
        {
            // steel/README.md, "Mark prices", and IMPLEMENTATION.md 2.1.
            Assert.AreEqual(6, Upgrade.UsualCost(UpgradeKind.Devotion));
            Assert.AreEqual(5, Upgrade.UsualCost(UpgradeKind.Evolution));
            Assert.AreEqual(8, Upgrade.UsualCost(UpgradeKind.Heresy));
            Assert.AreEqual(2, Upgrade.UsualCost(UpgradeKind.Aspect));
            Assert.AreEqual(2, Upgrade.UsualCost(UpgradeKind.Technique));
            Assert.AreEqual(1, Upgrade.UsualCost(UpgradeKind.Temper));
            Assert.AreEqual(0, Upgrade.UsualCost(UpgradeKind.Bond));
            Assert.AreEqual(0, Upgrade.UsualCost(UpgradeKind.Reaction));
            Assert.AreEqual(0, Upgrade.UsualCost(new Upgrade { Kind = UpgradeKind.Spell, Currency = Currency.None }));
            Assert.AreEqual(4, Upgrade.UsualCost(new Upgrade { Kind = UpgradeKind.Spell }));
            // A branch is 11 Shards, Learn 4, Empower 1, Shape 3, Master 3; a base spell's Learn is given.
            var rush = BranchOf(FireTree.WildfireRush);
            CollectionAssert.AreEqual(new[] { 4, 1, 3, 3 }, rush.Select(mark => mark.Cost).ToArray());
            CollectionAssert.AreEqual(new[] { 0, 1, 3, 3 }, BranchOf(FireTree.FlameWave).Select(mark => mark.Cost).ToArray());
            Assert.AreEqual(Currency.None, Upgrade.ById(FireTree.FlameWave).Currency);
            Assert.IsFalse(MarkRules.CanBuy(Upgrade.ById(FireTree.FlameWave), new HashSet<string>(), 999, true, true), "a base spell is never sold, in the playtest either");
            Assert.AreEqual(6, Upgrade.ById(FireTree.FireWithoutEnd).Cost);
            Assert.AreEqual(3, Upgrade.ById(FireTree.FuelToIt).Cost, "a reaction lift is a major");
        }

        // A branch's four marks, Learn first.
        private static List<Upgrade> BranchOf(string learn) => new List<Upgrade>
        {
            Upgrade.ById(learn), Upgrade.ById(SpellBranch.EmpowerId(learn)), Upgrade.ById(SpellBranch.ShapeId(learn)), Upgrade.ById(SpellBranch.MasterId(learn)),
        };

        [Test]
        public void EverySpellBranchIsFourMarksThatShareItAndGrowInOrder()
        {
            var learns = Upgrade.All.Where(mark => mark.Part == BranchPart.Learn).ToList();
            Assert.GreaterOrEqual(learns.Count, 12, "Fire's and Lightning's six each");
            var parts = new[] { BranchPart.Learn, BranchPart.Empower, BranchPart.Shape, BranchPart.Master };
            var kinds = new[] { UpgradeKind.Spell, UpgradeKind.Minor, UpgradeKind.Major, UpgradeKind.Major };
            foreach (var learn in learns)
            {
                var branch = BranchOf(learn.Id);
                for (int i = 0; i < 4; i++)
                {
                    var mark = branch[i];
                    Assert.IsNotNull(mark, learn.Id + " lacks its " + parts[i]);
                    Assert.AreEqual(learn.Id, mark.Branch, mark.Id);
                    Assert.AreEqual(parts[i], mark.Part, mark.Id);
                    Assert.AreEqual(kinds[i], mark.Kind, mark.Id);
                    Assert.AreEqual(learn.Element, mark.Element, mark.Id);
                    Assert.IsNotEmpty(mark.Description, mark.Id);
                    if (i > 0)
                    {
                        CollectionAssert.AreEqual(new[] { branch[i - 1].Id }, mark.Requires, mark.Id + " grows from the mark before it");
                        Assert.Greater(mark.Y, branch[i - 1].Y, mark.Id + " lies outward of the mark before it");
                    }
                }
                Assert.AreNotEqual(SpellRole.None, learn.Role, learn.Id + " has a role icon for the Hand");
                Assert.AreEqual(learn.IsBaseSpell, learn.Currency == Currency.None, learn.Id + ": only a base spell's Learn is given");
                Assert.IsTrue(learn.IsBaseSpell || learn.IsSpell, learn.Id);
                var empower = branch[1].Empower;
                Assert.IsNotNull(empower, learn.Id);
                // An Empower strengthens the spell or shortens its wait (or lowers its price), or both (the owner's answer of
                // 2026-11-20). Standing Light's waits on its ledger piece (in-own-ground) and does nothing yet.
                bool shortens = empower.Cooldown + empower.Mana > 0f;
                if (learn.Id != LightTree.StandingLight)
                    Assert.IsTrue(shortens || empower.Strengthens, learn.Id + ": an Empower makes the spell stronger or its wait shorter");
                var shapes = branch[2].Shapes;
                Assert.AreEqual(2, shapes.Length, learn.Id + " has two shapes");
                foreach (var shape in shapes)
                {
                    Assert.IsNotEmpty(shape.Name, learn.Id);
                    Assert.IsNotEmpty(shape.Description, learn.Id + "'s " + shape.Name);
                    if (!string.IsNullOrEmpty(shape.Form)) Assert.IsNotNull(SpellForms.Get(shape.Form), learn.Id + "'s " + shape.Name + " names a form that does not exist");
                    if (shape.Spell != null) Assert.IsFalse(string.IsNullOrEmpty(shape.Form) && learn.IsBaseSpell, learn.Id + "'s " + shape.Name + ": a base spell's shape with numbers needs a form to cast");
                    foreach (var effect in shape.Effects) Assert.AreSame(branch[2], effect.Owner);
                }
                Assert.AreNotEqual(shapes[0].Name, shapes[1].Name, learn.Id);
                // Only a branch's own marks answer OwnBranch.
                foreach (var mark in branch)
                    foreach (var effect in mark.Effects.Concat(mark.Shapes.SelectMany(shape => shape.Effects)))
                        if (effect.OwnBranch) Assert.IsNotEmpty(effect.Owner.Branch, mark.Id);
            }
            // A passive answers no branch.
            foreach (var mark in Upgrade.All.Where(mark => mark.Part == BranchPart.None))
            {
                Assert.IsEmpty(mark.Branch, mark.Id);
                Assert.IsEmpty(mark.Shapes, mark.Id);
                Assert.IsNull(mark.Empower, mark.Id);
                foreach (var effect in mark.Effects) Assert.IsFalse(effect.OwnBranch, mark.Id + " answers its own branch and is on none");
            }
        }

        [Test]
        public void EveryReshapedTreeIsSixBranchesADevotionALiftAndItsPassives()
        {
            foreach (string element in ReshapedPassives.Keys)
            {
                var tree = Upgrade.All.Where(mark => mark.Element == element && !mark.IsMix && !mark.IsWay && !mark.IsHidden).ToList();
                Assert.AreEqual(6, tree.Count(mark => mark.Part == BranchPart.Learn), element);
                Assert.AreEqual(24, tree.Count(mark => mark.Part != BranchPart.None), element);
                Assert.AreEqual(1, tree.Count(mark => mark.IsBaseSpell), element + "'s base spell is one of its six");
                Assert.AreEqual(SpellSlots.BaseId(element), tree.Single(mark => mark.IsBaseSpell).Id);
                Assert.AreEqual(1, tree.Count(mark => mark.Kind == UpgradeKind.Devotion), element);
                Assert.AreEqual(1, tree.Count(mark => mark.Lifts == element), element + " has one reaction lift");
                // One larger spell paid in mana (steel/README.md); Dark's is paid in health instead (Pawned Heart).
                Assert.LessOrEqual(tree.Count(mark => mark.Kind == UpgradeKind.Spell && SpellTimings.For(mark.Id).IsLarger), 1, element + " has one larger spell paid in mana at most");
                Assert.GreaterOrEqual(tree.Count(mark => mark.Kind == UpgradeKind.Spell && (SpellTimings.For(mark.Id).IsLarger || (mark.Spell != null && mark.Spell.HealthCost > 0f))), 1, element + " has its larger spell");
                Assert.IsTrue(tree.All(mark => mark.Currency != Currency.Memories), element + "'s tree is bought with Shards");
            }
            // Each design's passives, with its Devotion, its lift and its gift.
            foreach (var pair in ReshapedPassives)
                Assert.AreEqual(pair.Value, Upgrade.All.Count(mark => mark.Element == pair.Key && !mark.IsMix && !mark.IsWay && !mark.IsHidden && mark.Part == BranchPart.None), pair.Key);
        }

        [Test]
        public void HiddenMarksAreNeverDrawnAndTheRingLayoutPlacesAsTheTreeDoes()
        {
            var ring = RingLayout.Instance;
            foreach (var mark in Upgrade.All)
            {
                // The heart's duo branches are drawn by HeartLayout, not on the ring.
                Assert.AreEqual(!mark.IsHidden && mark.Tree == MarkTree.Element, ring.Draws(mark), mark.Id);
                if (!ring.Draws(mark)) continue;
                Assert.AreEqual(UpgradeTree.OnChart(mark), ring.Place(mark), mark.Id);
                // The ring's trees and branches are bought with Shards, or given.
                Assert.AreNotEqual(Currency.Memories, mark.Currency, mark.Id);
            }
            Assert.IsFalse(ring.Draws(null));
            Assert.AreEqual(UpgradeTree.Emblem(Elements.Fire), ring.Foot(Upgrade.ById(FireTree.Cinder)));
            // A mix not reshaped yet grows from its ring gap (none is left once tasks 4.15 to 4.19 are done).
            var ringMix = Upgrade.All.FirstOrDefault(mark => mark.IsMix && ring.Draws(mark));
            if (ringMix != null) Assert.AreEqual(UpgradeTree.MixFoot(ringMix.Element, ringMix.Element2), ring.Foot(ringMix), ringMix.Id);
            Assert.IsFalse(ring.Draws(Upgrade.ById(PlasmaTree.IonisedAir)), "Plasma is in the heart now; its gap on the ring is empty");
            Assert.AreEqual(HeartLayout.FootPlace, HeartLayout.Instance.Foot(Upgrade.ById(PlasmaTree.IonisedAir)));
        }

        [Test]
        public void TheHeartsBranchesSitOnItsPatternClearOfEachOther()
        {
            var heart = HeartLayout.Instance;
            var drawn = Upgrade.All.Where(heart.Draws).ToList();
            Assert.IsNotEmpty(drawn, "Plasma at least is in the heart");
            foreach (var group in drawn.GroupBy(HeartLayout.PairOf))
            {
                Assert.GreaterOrEqual(group.Key, 0, "every duo mark names a pair");
                string name = AfflictionPairs.Name(group.Key);
                var branch = group.ToList();
                var taken = new HashSet<string>();
                foreach (var mark in branch)
                {
                    var place = HeartPattern.All.FirstOrDefault(p => Mathf.Abs(p.X - mark.X) < 0.001f && Mathf.Abs(p.Y - mark.Y) < 0.001f);
                    Assert.IsNotNull(place.Name, mark.Id + " sits on no place of the heart");
                    Assert.IsTrue(taken.Add(place.Name), mark.Id + " shares the place " + place.Name);
                    Assert.IsTrue(mark.Kind == UpgradeKind.Mix || mark.Signature, mark.Id + " is a duo mark");
                    Assert.LessOrEqual(heart.Place(mark).magnitude, HeartPattern.Radius + 0.001f, mark.Id);
                    // A branch grows within its pair, and its lines pass clear of the branch's other marks.
                    foreach (string id in mark.Requires)
                    {
                        var parent = Upgrade.ById(id);
                        Assert.AreEqual(group.Key, HeartLayout.PairOf(parent), mark.Id + " grows from another pair's branch");
                        Assert.IsTrue(heart.Draws(parent), mark.Id + " grows from a mark not in the heart");
                        foreach (var other in branch)
                            if (other != mark && other != parent)
                                Assert.GreaterOrEqual(SegmentDistance(heart.Place(other), heart.Place(parent), heart.Place(mark)), LineClearance, $"the line {id} to {mark.Id} passes {other.Id}");
                    }
                }
                // One Shape mark, at the foot, with the reaction's two shapes; it grows from nothing on the chart.
                var shapes = branch.Where(ReactionTable.IsReactionShape).ToList();
                Assert.AreEqual(1, shapes.Count, name + " has one Shape mark");
                Assert.AreSame(shapes[0], ReactionTable.ShapeMarkFor(group.Key), name);
                Assert.AreEqual(HeartPattern.Of(HeartPattern.Foot).Y, shapes[0].Y, 1e-4f, name + ": the Shape mark stands at the foot");
                Assert.IsEmpty(shapes[0].Requires);
                Assert.AreEqual(2, shapes[0].Shapes.Length);
                Assert.AreNotEqual(shapes[0].Shapes[0].Name, shapes[0].Shapes[1].Name);
                foreach (var shape in shapes[0].Shapes)
                {
                    Assert.IsNotEmpty(shape.Description, name + ": " + shape.Name);
                    Assert.IsTrue(shape.Effects.Any(effect => effect.On == UpgradeTrigger.Reacted), name + ": " + shape.Name + " does something when the pair reacts");
                }
                // The shared prices: Shape 3, Minor 1, Major 3, Keystone 5, the duo spell 5 (pairs_d1_d2.md). A crown that casts a
                // form (Falling Thunder, begun by a finisher) is the god's gift, not the duo spell.
                Assert.AreEqual(3, shapes[0].Cost, name);
                foreach (var mark in branch.Where(mark => mark.IsSpell && !mark.Signature)) Assert.AreEqual(5, mark.Cost, mark.Id + ": a duo spell costs 5");
                Assert.LessOrEqual(branch.Count(mark => mark.IsSpell && !mark.Signature), 1, name + " has one duo spell at most");
                Assert.LessOrEqual(branch.Count(mark => mark.Signature), 1, name + " has one crown at most");
                Assert.IsTrue(branch.All(mark => mark.Cost == 0 || mark.Cost == 1 || mark.Cost == 3 || mark.Cost == 5), name + ": every price is a shared one");
                // Every other mark of the branch grows from something in it.
                foreach (var mark in branch.Where(mark => !ReactionTable.IsReactionShape(mark) && !mark.Signature))
                    Assert.IsNotEmpty(mark.Requires, mark.Id + " grows from nothing");
            }
        }

        // How near, in steps, a line may pass another mark of its branch.
        private const float LineClearance = 0.4f;

        private static float SegmentDistance(Vector2 point, Vector2 from, Vector2 to)
        {
            Vector2 along = to - from;
            float t = Mathf.Clamp01(Vector2.Dot(point - from, along) / along.sqrMagnitude);
            return (point - (from + along * t)).magnitude;
        }

        [Test]
        public void TheHeartPatternIsSpacedClearAndWalkedFromEveryTree()
        {
            // The heart (IMPLEMENTATION.md 4.2, task 4.5): one duo branch at a time in the ring's middle, upright on the screen.
            var heart = HeartPattern.All;
            Assert.AreEqual(10, heart.Count);
            Assert.AreEqual(heart.Count, heart.Select(place => place.Name).Distinct().Count());
            var at = heart.ToDictionary(place => place.Name, place => new Vector2(place.X, place.Y));
            foreach (var place in heart)
            {
                Assert.LessOrEqual(at[place.Name].magnitude, HeartPattern.Radius + 0.001f, place.Name + " lies outside the heart");
                if (place.From.Length == 0) continue;
                Assert.Greater(place.Y, HeartPattern.Of(place.From).Y, place.Name + " lies below what it grows from");
                foreach (var other in heart)
                    if (other.Name != place.Name && other.Name != place.From)
                        Assert.GreaterOrEqual(SegmentDistance(at[other.Name], at[place.From], at[place.Name]), LineClearance, $"the line {place.From} to {place.Name} passes {other.Name}");
            }
            for (int i = 0; i < heart.Count; i++)
                for (int j = i + 1; j < heart.Count; j++)
                    Assert.GreaterOrEqual((at[heart[i].Name] - at[heart[j].Name]).magnitude, 1.2f, heart[i].Name + " and " + heart[j].Name + " crowd each other");
            // The foot's knot and the title stand clear of the branch, and the whole heart inside the emblems' ring.
            foreach (var place in at.Values)
            {
                Assert.GreaterOrEqual((place - HeartLayout.FootPlace).magnitude, UpgradeTree.LeastSpacing);
                Assert.GreaterOrEqual((place - HeartLayout.TitlePlace).magnitude, UpgradeTree.LeastSpacing);
            }
            Assert.Less(HeartPattern.Radius + 1f, UpgradeTree.EmblemDistance - 1f, "the heart keeps clear of the emblems");
            // Every tree upright with both its ring branches and the heart's ten places below it: the stick reaches all from the root.
            foreach (var element in Elements.All)
            {
                var places = BranchPattern.All.Select(place => UpgradeTree.Upright(UpgradeTree.OnChart(element.Name, place.X, place.Y), element.Bearing)).ToList();
                var before = Elements.All[(element.Index + Elements.Count - 1) % Elements.Count];
                foreach (var (_, x, y) in PatternMix)
                {
                    places.Add(UpgradeTree.Upright(UpgradeTree.OnChart(Elements.MixBearing(element.Name, element.MixWith), UpgradeTree.MixDistance, x, y), element.Bearing));
                    places.Add(UpgradeTree.Upright(UpgradeTree.OnChart(Elements.MixBearing(before.Name, element.Name), UpgradeTree.MixDistance, x, y), element.Bearing));
                }
                int ring = places.Count;
                places.AddRange(at.Values);
                for (int i = ring; i < places.Count; i++)
                    for (int j = 0; j < ring; j++)
                        Assert.GreaterOrEqual((places[i] - places[j]).magnitude, UpgradeTree.LeastSpacing, element.Name + ": the heart crowds the ring");
                Assert.IsTrue(Walkable(places, 0, out int reached), $"{element.Name}: the stick reaches {reached} of {places.Count}");
            }
        }

        [Test]
        public void NoRetiredIdIsUsedAgain()
        {
            foreach (var mark in Upgrade.All)
            {
                Assert.IsFalse(RetiredMarks.IsRetired(mark.Id), mark.Id + " was retired and must not be used again");
                // A mark that left an element tree for a weapon's Way is used again only there (phase 5).
                if (RetiredMarks.IsMoving(mark.Id)) Assert.AreEqual(MarkTree.Way, mark.Tree, mark.Id + " left its tree for a weapon's Way");
            }
            foreach (var moving in RetiredMarks.Moving)
                Assert.IsNotNull(Upgrade.ById(moving.Id), moving.Id + " has its home on a weapon's Way");
        }

        [Test]
        public void TheFourMarksTheOwnerKeptStandOnTheirTreesUnderTheirOldIds()
        {
            // The owner's answer of 2026-11-20: Hammer Down, Clean Break, Spilled Water and Shade are marks, not retired.
            var kept = new (string id, string tree, string place, string from)[]
            {
                (WindTree.HammerDown, Elements.Wind, BranchPattern.RightMiddle, WindTree.AgainstTheWall),
                (IceTree.CleanBreak, Elements.Ice, BranchPattern.RightMiddle, IceTree.Brittle),
                (WaterTree.SpilledWater, Elements.Water, BranchPattern.TrunkTop, WaterTree.SpreadingWater),
                (LifeTree.Shade, Elements.Life, BranchPattern.TrunkTop, LifeTree.RootAndBranch),
            };
            foreach (var (id, tree, place, from) in kept)
            {
                var mark = Upgrade.ById(id);
                Assert.IsNotNull(mark, id + " is a mark again");
                Assert.IsFalse(RetiredMarks.IsRetired(id), id + " is out of RetiredMarks, so a save keeps it and is refunded nothing");
                Assert.AreEqual(UpgradeKind.Major, mark.Kind, id);
                Assert.AreEqual(3, mark.Cost, id);
                Assert.AreEqual(tree, mark.Element, id);
                CollectionAssert.AreEqual(new[] { from }, mark.Requires, id);
                var at = BranchPattern.Of(place);
                Assert.AreEqual(at.X, mark.X, 0.001f, id + " stands at " + place);
                Assert.AreEqual(at.Y, mark.Y, 0.001f, id + " stands at " + place);
            }
            // The stalk above each middle place grows from it, as the pattern grows its stalks: a line from the foot would
            // run over the kept mark.
            CollectionAssert.AreEqual(new[] { WindTree.HammerDown }, Upgrade.ById(WindTree.IndrawnBreath).Requires);
            CollectionAssert.AreEqual(new[] { IceTree.CleanBreak }, Upgrade.ById(IceTree.SkiRun).Requires);
            // Hammer Down's heavy blow on an enemy in the air strikes half again as hard.
            var stats = new PlayerStats();
            foreach (var mod in Upgrade.ById(WindTree.HammerDown).Stats) stats.AddLearned(mod);
            Assert.AreEqual(0.5f, stats.InAirHeavy(PlayerStat.WeaponDamage), 0.001f);
            Assert.AreEqual(0f, stats.InAir(PlayerStat.WeaponDamage), 0.001f, "a light blow in the air is not raised by it");
            Assert.AreEqual(0f, stats.Blow(PlayerStat.WeaponDamage, true, false, false), 0.001f, "nor a heavy blow on the ground");
        }

        [Test]
        public void AMarkIsReachableOnceAnyOneOfWhatItGrowsFromIsLearned()
        {
            var learned = new HashSet<string>();
            Assert.IsTrue(UpgradeTree.Reachable(Upgrade.ById(FireTree.Cinder), learned));
            Assert.IsFalse(UpgradeTree.Reachable(Upgrade.ById(Upgrade.Backdraft), learned));
            learned.Add(FireTree.Cinder);
            Assert.IsTrue(UpgradeTree.Reachable(Upgrade.ById(Upgrade.Backdraft), learned));
            Assert.IsFalse(UpgradeTree.Reachable(Upgrade.ById(FireTree.KilnStance), learned));
            learned.Add(Upgrade.Backdraft);
            Assert.IsTrue(UpgradeTree.Reachable(Upgrade.ById(FireTree.KilnStance), learned));
            // Chain Lance grows from the lift on its bough.
            var lance = Upgrade.ById(LightningTree.ChainLance);
            CollectionAssert.AreEqual(new[] { LightningTree.StrikesTwice }, lance.Requires);
            Assert.IsFalse(UpgradeTree.Reachable(lance, learned));
            learned.Add(LightningTree.StrikesTwice);
            Assert.IsTrue(UpgradeTree.Reachable(lance, learned));
            // A base spell comes with its element: its branch's Empower is open with nothing learned.
            Assert.IsTrue(UpgradeTree.Reachable(Upgrade.ById(SpellBranch.EmpowerId(FireTree.FlameWave)), new HashSet<string>()));
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
            // The trees, and the duo branches written one file to a pair under Trees/Duo/ (task 4.5).
            string trees = typeof(UpgradeTrees).Namespace;
            var files = typeof(UpgradeTrees).Assembly.GetTypes()
                .Where(type => (type.Namespace == trees || type.Namespace == trees + ".Duo") && type != typeof(UpgradeTrees) && type.IsAbstract && type.IsSealed).ToList();
            Assert.IsTrue(files.Contains(typeof(Clover.Player.Trees.Duo.SleetChargeDuo)));
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
