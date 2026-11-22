using System.IO;
using System.Linq;
using Clover.Player;
using Clover.Player.Trees;
using Clover.Player.Trees.Duo;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// A spell held on its button and loosed on letting go (standin_clearance_plan.md pass 6, the ledger's held-press), and the
    /// Hold interaction on the slot buttons that tells a hold from a tap.
    /// </summary>
    public class HeldPressTests
    {
        private const string ControlsPath = "Assets/_Project/Settings/Controls.inputactions";

        // The interactions written on an action of the asset, read from its text (the editor loads the same text).
        private static string InteractionsOf(string text, string action)
        {
            int at = text.IndexOf("\"name\": \"" + action + "\",", System.StringComparison.Ordinal);
            Assert.That(at, Is.GreaterThanOrEqualTo(0), action + " is in the asset");
            const string key = "\"interactions\": \"";
            int field = text.IndexOf(key, at, System.StringComparison.Ordinal);
            int next = text.IndexOf("\"name\": ", at + 10, System.StringComparison.Ordinal);
            Assert.That(field, Is.GreaterThan(at));
            Assert.That(next < 0 || field < next, action + "'s own interactions");
            int start = field + key.Length;
            return text.Substring(start, text.IndexOf('"', start) - start);
        }

        [Test]
        public void TheSlotButtonsTellAHoldFromATap()
        {
            Assume.That(File.Exists(ControlsPath), "run from the project folder");
            string text = File.ReadAllText(ControlsPath);
            foreach (var action in LayeredButtons.SlotActions)
                Assert.AreEqual("Hold(duration=0.2)", InteractionsOf(text, action), action + " is a hold after a fifth of a second");
            Assert.AreEqual("", InteractionsOf(text, "SpellLayer"), "the layer itself is only held, never asked for a hold");
            Assert.AreEqual(text.Count(c => c == '{'), text.Count(c => c == '}'), "the asset's braces still pair");
        }

        [Test]
        public void AHoldGathersAndALetGoLoosesWithWhatItGathered()
        {
            Assert.AreEqual(0f, SpellSpec.HeldShare(0f, 1f), 1e-5f);
            Assert.AreEqual(0.5f, SpellSpec.HeldShare(0.5f, 1f), 1e-5f);
            Assert.AreEqual(1f, SpellSpec.HeldShare(3f, 1f), 1e-5f, "never more than a full hold");
            Assert.AreEqual(0f, SpellSpec.HeldShare(1f, 0f), 1e-5f, "a spell not held gathers nothing");
            var spec = new SpellSpec { Damage = 8f, Stagger = 22f, HeldStagger = 60f };
            SpellSpec.ApplyHold(spec, 0.5f);
            Assert.AreEqual(41f, spec.Stagger, 1e-4f, "half a hold: half way from 22 to 60");
            Assert.AreEqual(8f, spec.Damage, 1e-5f, "no held damage set: its own");
            var copy = new SpellSpec();
            SpellChange.Copy(new SpellSpec { HoldUpTo = 1.5f, HeldDamage = 3f, HeldStagger = 4f }, copy);
            Assert.AreEqual(1.5f, copy.HoldUpTo, 1e-5f);
            Assert.AreEqual(3f, copy.HeldDamage, 1e-5f);
            Assert.AreEqual(4f, copy.HeldStagger, 1e-5f);
        }

        [Test]
        public void AShapesSecondPressTakesYouBackWhereYouCastIt()
        {
            var road = Upgrade.ById(SpellBranch.ShapeId(LightTree.StepIntoLight)).Shapes.First(each => each.Name == "Aruna's Road");
            var presses = PlayerUpgrades.PressesOf(Upgrade.ById(LightTree.StepIntoLight), road, out float window);
            Assert.AreEqual(3f, window, 1e-5f, "within three seconds");
            Assert.AreEqual(UpgradeAction.ReturnToCast, presses.Single().Single().Do, "back along the road to where you began");
            var back = Upgrade.ById(SpellBranch.ShapeId(MetalTree.ToTheBlade)).Shapes.First(each => each.Name == "There and Back");
            var flight = PlayerUpgrades.PressesOf(Upgrade.ById(MetalTree.ToTheBlade), back, out window).Single().Single();
            Assert.AreEqual(2f, window, 1e-5f, "within two seconds of arriving");
            Assert.AreEqual(UpgradeAction.ReturnToCast, flight.Do);
            Assert.AreEqual(10f, flight.Damage, 1e-5f, "the line cut a second time");
            Assert.IsEmpty(back.Effects, "no stand-in cut a second later");
            Assert.IsNull(PlayerUpgrades.PressesOf(Upgrade.ById(MetalTree.ToTheBlade), null, out window), "no shape worn: no second press");
        }

        [Test]
        public void GatheredAndDiamondThunderAreHeldOnTheirButtons()
        {
            var gathered = Upgrade.ById(SpellBranch.ShapeId(LightTree.Glare)).Shapes.First(each => each.Name == "Gathered");
            Assert.IsTrue(gathered.CastsItsOwn, "cast through its mark, so it can be held");
            Assert.AreEqual(1f, gathered.Spell.HoldUpTo, 1e-5f, "held up to a second");
            Assert.AreEqual(22f, gathered.Spell.Stagger, 1e-5f);
            Assert.AreEqual(60f, gathered.Spell.HeldStagger, 1e-5f);
            Assert.IsFalse(gathered.Effects != null && gathered.Effects.Any(each => each.Do == UpgradeAction.Burst), "no stand-in burst at a full hold");
            Assert.AreEqual(1.5f, Upgrade.ById(TheBoltRunsTrueDuo.DiamondThunder).Spell.HoldUpTo, 1e-5f, "the line held up to 1.5 s");
        }

        [Test]
        public void DaybreakIsGatheredRingByRingAndLoosedWithWhatItHolds()
        {
            var learn = Upgrade.ById(LightTree.Daybreak);
            Assert.AreEqual(8, learn.Spell.Rings, "eight rings at most");
            Assert.AreEqual(6, learn.Spell.RingsFor, "its numbers are written for six");
            var gather = new RingGather();
            Assert.IsFalse(gather.Add(), "nothing gathering");
            gather.Begin(learn, learn.Spell, 7, 18f);
            Assert.IsTrue(gather.Active);
            for (int i = 0; i < 10; i++) gather.Add();
            Assert.AreEqual(8, gather.Rings, "no more than eight");
            Assert.IsFalse(gather.Due(17.9f));
            Assert.IsTrue(gather.Due(18f), "loosed by itself when its window is over");
            var loosed = new SpellSpec();
            Assert.IsTrue(gather.Loose(loosed));
            Assert.IsFalse(gather.Active);
            Assert.AreEqual(16f, loosed.Damage, 1e-4f, "eight rings: 8 a ring over four bites");
            Assert.AreEqual(12f, loosed.Stagger, 1e-4f, "6 stagger a ring over four bites");
            Assert.AreEqual(0, loosed.Rings, "what is loosed gathers no more");
            gather.Begin(learn, learn.Spell, 8, 30f);
            gather.Add();
            gather.Add();
            gather.Loose(loosed);
            Assert.AreEqual(4f, loosed.Damage, 1e-4f, "two rings");
            Assert.AreEqual(0f, RingGather.Scale(0, 6), 1e-5f);
            var presses = PlayerUpgrades.PressesOf(learn, null, out float window);
            Assert.AreEqual(8f, window, 1e-5f);
            Assert.AreEqual(UpgradeAction.Loose, presses[0].Single().Do, "the second press looses it");
            var master = Upgrade.ById(SpellBranch.MasterId(LightTree.Daybreak)).Effects;
            Assert.IsTrue(master.All(each => each.On == UpgradeTrigger.SpellLoosed && each.BlowLanding == 0.2f), "as it is loosed, a blow a fifth of a second from landing");
            Assert.IsTrue(master.Any(each => each.Do == UpgradeAction.Ward), "the blow is turned");
            Assert.IsTrue(master.Any(each => each.Do == UpgradeAction.Boon && each.ThisCast && each.Stat == PlayerStat.CritChance), "the beam is critical");
            var noon = Upgrade.ById(SpellBranch.ShapeId(LightTree.Daybreak)).Shapes.Single(each => each.Name == "High Noon");
            Assert.AreEqual(8, noon.Spell.Rings, "the column is gathered too");
            Assert.AreEqual(UpgradeTrigger.SpellLoosed, noon.Effects.Single(each => each.Do == UpgradeAction.Cleanse).On, "cleansed as it comes down");
        }

        [Test]
        public void RiptidesMasterAddsAPressThatSendsTheWaveOnPastYou()
        {
            var riptide = Upgrade.ById(WaterTree.Riptide);
            var master = Upgrade.ById(SpellBranch.MasterId(WaterTree.Riptide));
            Assert.IsNull(PlayerUpgrades.PressesOf(riptide, null, null, out _), "no press without the Master");
            var presses = PlayerUpgrades.PressesOf(riptide, null, master, out float window);
            Assert.IsNotNull(presses, "the Master's press");
            Assert.AreEqual(2f, window, 1e-5f);
            var carry = presses[0].Single();
            Assert.AreEqual(UpgradeAction.CarryOn, carry.Do);
            Assert.AreEqual(CastMoment.WaveIn, carry.After, "only once the wave has come in");
            Assert.AreEqual(1f, carry.AfterWindow, 1e-5f, "within a second of it");
            Assert.AreEqual(4f, carry.Reach, 1e-5f, "four metres past you");
            Assert.AreEqual(WaterTree.BacksTurned, carry.State.Id, "backs turned");
            Assert.AreEqual(master, carry.Owner, "the press is the Master's own");
            Assert.IsEmpty(master.Effects, "nothing else: the stand-in on every hit is gone");
            var to = PlayerUpgrades.CarriedOnTo(new UnityEngine.Vector3(1f, 0f, 1f), new UnityEngine.Vector3(0f, 0.5f, -2f), 4f);
            Assert.AreEqual(1f, to.x, 1e-4f);
            Assert.AreEqual(-3f, to.z, 1e-4f, "on along the way the wave rolled, level");
            var shaped = new ShapeDef { Presses = new[] { new UpgradeEffect[0] }, PressWindow = 3f };
            PlayerUpgrades.PressesOf(riptide, shaped, master, out float shapeWindow);
            Assert.AreEqual(3f, shapeWindow, 1e-5f, "a shape's own presses come first");
        }

        [Test]
        public void TheTideTurnsEbbsFirstAndFloodsOnTheSecondPress()
        {
            var tide = Upgrade.ById(WaterTree.TheTideTurns);
            Assert.AreEqual(5f, tide.Spell.Ebb, 1e-5f, "drawn five metres back first");
            var presses = PlayerUpgrades.PressesOf(tide, null, out float window);
            Assert.AreEqual(3f, window, 1e-5f, "the flood within three seconds");
            Assert.AreEqual(UpgradeAction.Flood, presses[0].Single().Do);
            var from = new UnityEngine.Vector3(0f, 0f, 0f);
            Assert.AreEqual(8f, PlayerUpgrades.FloodReach(new UnityEngine.Vector3(0f, 0f, -5f), from, 8f, 5f), 1e-4f, "back along the line and three past");
            Assert.AreEqual(5f, PlayerUpgrades.FloodReach(new UnityEngine.Vector3(0f, 0.4f, -2f), from, 8f, 5f), 1e-4f, "from wherever you stand, level");
            Assert.AreEqual(5f, PlayerUpgrades.FloodReach(new UnityEngine.Vector3(0f, 0f, -5f), from, 5f, 5f), 1e-4f, "King Tide's flood stops where you began");
            var shapes = Upgrade.ById(SpellBranch.ShapeId(WaterTree.TheTideTurns)).Shapes;
            var sea = shapes.Single(each => each.Name == "Out to Sea");
            Assert.AreEqual(5f, sea.Spell.Ebb, 1e-5f, "Out to Sea ebbs you too");
            Assert.AreEqual(0, PlayerUpgrades.PressesOf(tide, sea, out _)[0].Length, "and has no flood");
            var king = shapes.Single(each => each.Name == "King Tide");
            Assert.AreEqual(5f, king.Change.Reach, 1e-5f);
            var master = Upgrade.ById(SpellBranch.MasterId(WaterTree.TheTideTurns)).Effects;
            Assert.IsTrue(master.Any(each => each.On == UpgradeTrigger.SpellLoosed && each.BlowLanding == 0.3f && each.Do == UpgradeAction.ResetCooldown),
                "a flood loosed as a blow would land starts no wait");
            Assert.IsFalse(master.Any(each => each.On == UpgradeTrigger.PerfectDodge), "the stand-in is gone");
        }

        [Test]
        public void TheBagCatchesAWindForEachBlowTurnedAndOpensAsOneCone()
        {
            var learn = Upgrade.ById(WindTree.BagOfWinds);
            Assert.AreEqual(RingSource.Turned, learn.Spell.RingsBy, "winds come from blows turned, not landed");
            Assert.AreEqual(3, learn.Spell.Rings);
            Assert.AreEqual(1, learn.Spell.RingsFor, "12 damage and 25 stagger for each wind");
            var bag = new RingGather();
            bag.Begin(learn, learn.Spell, 3, 8f);
            Assert.IsTrue(bag.Add(1f));
            Assert.IsTrue(bag.Add(2f));
            Assert.IsFalse(bag.LastWithin(3, 3f, 2f), "two winds only");
            Assert.IsTrue(bag.Add(3.5f));
            Assert.IsTrue(bag.LastWithin(3, 3f, 3.5f), "three within three seconds");
            Assert.IsFalse(bag.LastWithin(3, 2f, 3.5f));
            var cone = new SpellSpec();
            bag.Loose(cone);
            Assert.AreEqual(36f, cone.Damage, 1e-4f, "three winds");
            Assert.AreEqual(75f, cone.Stagger, 1e-4f);
            Assert.AreEqual(7f, cone.Reach, 1e-5f, "a cone of seven metres");
            var empowered = new SpellSpec();
            SpellChange.Copy(learn.Spell, empowered);
            Upgrade.ById(SpellBranch.EmpowerId(WindTree.BagOfWinds)).Empower.ApplyTo(empowered);
            Assert.AreEqual(4, empowered.Rings, "a fourth wind");
            Assert.AreEqual(8f, PlayerUpgrades.PressesOf(learn, null, out float window) != null ? window : 0f, 1e-5f, "opened by the second press within 8 s");
            Assert.IsTrue(learn.Effects.All(each => each.WhileGathering && each.Do == UpgradeAction.Push && each.Amount == 2f), "each attacker thrown back two metres while the bag is open");

            var shapes = Upgrade.ById(SpellBranch.ShapeId(WindTree.BagOfWinds)).Shapes;
            var kept = shapes.Single(each => each.Name == "Kept Wind");
            Assert.IsTrue(kept.Spell.RingsLapse, "never opens by itself");
            Assert.AreEqual(20f, kept.PressWindow, 1e-5f, "kept twenty seconds");
            var loose = shapes.Single(each => each.Name == "Loose Neck");
            Assert.AreEqual(0, loose.Spell.Rings, "nothing kept");
            Assert.IsTrue(loose.Effects.All(each => each.Do == UpgradeAction.FreeBase && each.WithinOwnCast == 8f), "each wind a free Gust while it is untied");

            var master = Upgrade.ById(SpellBranch.MasterId(WindTree.BagOfWinds)).Effects;
            Assert.IsTrue(master.All(each => each.On == UpgradeTrigger.RingAdded && each.SpentAtLeast == 3 && each.RingsWithin == 3f));
            Assert.IsTrue(master.Any(each => each.Do == UpgradeAction.Lift && each.Radius == 6f), "everything within six metres into the air");
            Assert.IsTrue(master.Any(each => each.Do == UpgradeAction.Spill), "the bag bursts, no cone after");
        }

        [Test]
        public void SecondShadowStandsWhereLeftAndItsPressesChangePlacesOrGiveTheWoundsBack()
        {
            var learn = Upgrade.ById(DarkTree.SecondShadow);
            Assert.IsTrue(learn.Spell.StaysPut, "the shadow is left where you stand");
            var swap = PlayerUpgrades.PressesOf(learn, null, out float window);
            Assert.AreEqual(8f, window, 1e-5f, "within its eight seconds");
            Assert.AreEqual(1, swap.Length, "once");
            Assert.AreEqual(UpgradeAction.SwapWithShadow, swap[0].Single().Do);
            var shapes = Upgrade.ById(SpellBranch.ShapeId(DarkTree.SecondShadow)).Shapes;
            var back = shapes.Single(each => each.Name == "Return to the Shadow");
            Assert.AreEqual(UpgradeAction.ReturnToShadow, PlayerUpgrades.PressesOf(learn, back, out _)[0].Single().Do, "sunk into it, not swapped");
            Assert.IsTrue(back.Effects == null || back.Effects.Length == 0, "the stand-in quarter less taken is gone");
            Assert.AreEqual(0, PlayerUpgrades.PressesOf(learn, shapes.Single(each => each.Name == "Long Shadow"), out _)[0].Length, "Long Shadow leaves no shadow to press");

            Assert.AreEqual(30f, PlayerUpgrades.WoundsSince(80f, 50f), 1e-5f);
            Assert.AreEqual(0f, PlayerUpgrades.WoundsSince(50f, 70f), 1e-5f, "healed since: nothing to give back");
            var health = new RegainHealth { Maximum = 100f };
            health.Fill();
            health.Damage(30f);
            Assert.AreEqual(30f, health.Return(30f), 1e-5f, "every wound since comes back grey");
            Assert.AreEqual(70f, health.Current, 1e-5f, "grey, to be won back, not healed");
            Assert.AreEqual(30f, health.Grey, 1e-5f);
            Assert.AreEqual(0f, health.Return(10f), 1e-5f, "never past the maximum");
            Assert.AreEqual(10f, health.Regain(10f), 1e-5f, "won back by blows");

            var master = Upgrade.ById(SpellBranch.MasterId(DarkTree.SecondShadow)).Effects.Single();
            Assert.AreEqual(UpgradeTrigger.SpellPressed, master.On, "as you change places");
            Assert.AreEqual(CastMoment.Struck, master.After);
            Assert.AreEqual(1f, master.AfterWindow, 1e-5f, "within a second of being struck");
            Assert.AreEqual(UpgradeAction.UndoWound, master.Do, "the health it took comes back");
            Assert.AreEqual(2.5f, Clover.Player.Forms.CloneForm.ShadowReach, 1e-5f);
        }

        [Test]
        public void LightningRodsSecondPressStepsToTheRodAndSpendsItUnlessItsMasterKeepsIt()
        {
            var rod = Upgrade.ById(Upgrade.LightningRod);
            var step = PlayerUpgrades.PressesOf(rod, null, out float window)[0].Single();
            Assert.AreEqual(UpgradeAction.StepToStanding, step.Do);
            Assert.GreaterOrEqual(window, 8f, "while the rod stands");
            Assert.AreEqual(2.5f, step.Radius, 1e-5f, "a 2.5 m shock");
            Assert.AreEqual(14f, step.Damage, 1e-5f);
            Assert.AreEqual(20f, step.Stagger, 1e-5f);
            var lodestone = Upgrade.ById(SpellBranch.ShapeId(Upgrade.LightningRod)).Shapes.Single(each => each.Name == "Lodestone");
            Assert.AreEqual(0, PlayerUpgrades.PressesOf(rod, lodestone, out _)[0].Length, "Lodestone has no step");
            var master = Upgrade.ById(SpellBranch.MasterId(Upgrade.LightningRod)).Effects;
            var inForce = master.Where(each => each.On == UpgradeTrigger.SpellPressed).ToList();
            Assert.IsTrue(PlayerUpgrades.KeepsStanding(inForce, Upgrade.LightningRod), "the Master keeps the rod past the step");
            Assert.IsFalse(PlayerUpgrades.KeepsStanding(new System.Collections.Generic.List<UpgradeEffect>(), Upgrade.LightningRod), "without it the rod is spent");
        }

        [Test]
        public void AHeldStanceStandsWhileHeldBanksEveryBlowAndPaysOnLettingGo()
        {
            Assert.IsTrue(Clover.Player.Forms.ParryForm.StanceHolds(true, 2.9f, 3f), "held, within its most");
            Assert.IsFalse(Clover.Player.Forms.ParryForm.StanceHolds(true, 3f, 3f), "held to its most");
            Assert.IsFalse(Clover.Player.Forms.ParryForm.StanceHolds(false, 0.6f, 3f), "let go");
            Assert.IsTrue(Clover.Player.Forms.ParryForm.StanceHolds(false, 0.1f, 3f), "a tap still stands a moment");
            Assert.AreEqual(3, Clover.Player.Forms.ParryForm.Banked(3, 3), "three at most under the hood");
            Assert.AreEqual(5, Clover.Player.Forms.ParryForm.Banked(4, 0), "no limit");
            Assert.AreEqual(65f, Clover.Player.Forms.ParryForm.DoorDamage(20f, 15f, 3), 1e-4f, "20 and 15 for each blow banked");

            var kiln = Upgrade.ById(FireTree.KilnStance).Spell;
            Assert.IsTrue(kiln.HeldStance);
            Assert.AreEqual(3f, kiln.Seconds, 1e-5f, "held up to three seconds");
            Assert.AreEqual(3, kiln.Bank);
            Assert.AreEqual(15f, kiln.BankDamage, 1e-5f);
            Assert.AreEqual(6f, kiln.Reach, 1e-5f, "a cone of six metres");
            var kilnShapes = Upgrade.ById(SpellBranch.ShapeId(FireTree.KilnStance)).Shapes;
            Assert.IsFalse(kilnShapes.Single(each => each.Name == "The Kiln").Spell.HeldStance, "The Kiln is the stance as it was");
            Assert.AreEqual(0.6f, kilnShapes.Single(each => each.Name == "Firewalker").Change.Seconds, 1e-5f);

            var mountain = Upgrade.ById(EarthTree.MountainStance).Spell;
            Assert.IsTrue(mountain.HeldStance);
            Assert.AreEqual(20f, mountain.BankStagger, 1e-5f, "20 stagger a blow turned");
            Assert.AreEqual(0.5f, mountain.MissedWait, 1e-5f, "five seconds, not ten, when nothing struck");
            var empowered = new SpellSpec();
            SpellChange.Copy(mountain, empowered);
            Upgrade.ById(SpellBranch.EmpowerId(EarthTree.MountainStance)).Empower.ApplyTo(empowered);
            Assert.AreEqual(25f, empowered.BankStagger, 1e-5f, "the Empower's 25 a blow");
            Assert.AreEqual(3f, empowered.Seconds, 1e-5f);
            Assert.IsFalse(Upgrade.ById(EarthTree.MountainStance).Effects.Any(each => each.Do == UpgradeAction.Boon), "the stand-in boon is gone");
            Assert.AreEqual(1f, Clover.Player.Forms.ParryForm.LoadSeconds, 1e-5f, "the heavy blow within a second of letting go");
        }

        [Test]
        public void AGatheringSpellShowsItsCountByItsSlotInPlainWords()
        {
            Assert.AreEqual("3 of 8 rings", Clover.UI.SpellDiamondHud.GatherWords(3, 8, RingSource.Blows), "Daybreak's rings");
            Assert.AreEqual("2 of 3 winds", Clover.UI.SpellDiamondHud.GatherWords(2, 3, RingSource.Turned), "Bag of Winds' winds");
            Assert.AreEqual("0 of 4 winds", Clover.UI.SpellDiamondHud.GatherWords(0, 4, RingSource.Turned));
            Assert.GreaterOrEqual(Clover.UI.SpellDiamondHud.CountSize(0.7f) * 0.7f, Clover.UI.SpellDiamondHud.ReadableMinimum, "readable at the HUD minimum even while the corner is small");
            Assert.AreEqual(24, Clover.UI.SpellDiamondHud.ReadableMinimum, "the style guide's smallest size");
            var daybreak = Upgrade.ById(LightTree.Daybreak);
            var shape = Upgrade.ById(SpellBranch.ShapeId(LightTree.Daybreak));
            Assert.IsTrue(Clover.UI.SpellDiamondHud.SameSpell(daybreak, shape), "its shape is the same spell in the slot");
            Assert.IsFalse(Clover.UI.SpellDiamondHud.SameSpell(daybreak, Upgrade.ById(LightTree.TwoSuns)));
        }
    }
}
