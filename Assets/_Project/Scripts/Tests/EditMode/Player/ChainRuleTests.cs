using Clover.Player;
using Clover.Player.Trees.Ways;
using Clover.Player.Way;
using Clover.Player.Way.Aspects;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The chain rules the Katana's Way changes (way_build_plan.md 2.7, "chain rules", and 3.1 and 3.3): the Stream keeps
    /// the chain through a dodge out of a cut's recovery; Stepping Water cuts from any part of a roll and counts as the
    /// chain's next step; the Rising Draw cuts from the last 40% of a roll; Zanshin's turn takes 0.15 s. With none of
    /// them, the chain plays as before.
    /// </summary>
    public class ChainRuleTests
    {
        private static StaminaPool Stamina()
        {
            var stamina = new StaminaPool { Maximum = 1000f };
            stamina.Fill();
            return stamina;
        }

        // Lets the chain run in small steps, as frames would.
        private static void Run(WeaponCombo combo, StaminaPool stamina, float seconds)
        {
            for (float t = 0f; t < seconds; t += 0.01f) combo.Tick(0.01f, stamina);
        }

        private static WeaponCombo Cut(Weapon weapon, StaminaPool stamina)
        {
            var combo = weapon.Combo;
            combo.Press();
            combo.Tick(0.001f, stamina);
            return combo;
        }

        [Test]
        public void UnderTheStreamADodgeOutOfARecoveryKeepsTheChain()
        {
            foreach (bool stream in new[] { false, true })
            {
                var weapon = stream ? Armoury.Resolve(Weapons.KatanaName, KatanaWay.Stream, new MarkSet(KatanaWay.Stream)) : Weapons.Katana();
                var stamina = Stamina();
                var combo = Cut(weapon, stamina);
                Assert.AreSame(combo.Steps[0], combo.Current);
                Run(combo, stamina, combo.Steps[0].Windup + combo.Steps[0].Active + 0.03f);
                Assert.AreEqual(AttackPhase.Recovery, combo.Phase);
                combo.Cancel();
                // The roll: longer than the chain's grace.
                for (int i = 0; i < 40; i++) combo.Idle(0.01f, true);
                Run(combo, stamina, 0.05f);
                combo.Press();
                combo.Tick(0.001f, stamina);
                if (stream) Assert.AreSame(combo.Steps[1], combo.Current, "the Stream: the chain goes on");
                else Assert.AreSame(combo.Steps[0], combo.Current, "plain: the dodge ended the chain, as before");
            }
        }

        [Test]
        public void SteppingWaterCutsFromAnyPartOfTheRollAndCountsAsTheNextStep()
        {
            Assert.AreEqual(0.5f, Weapons.Katana().Combo.DashCutFrom, 1e-6f, "plain: the roll's second half");
            Assert.AreEqual(KatanaTrunk.RisingFrom, Armoury.Resolve(Weapons.KatanaName, "", new MarkSet(KatanaWay.RisingDraw)).Combo.DashCutFrom, 1e-6f, "the Rising Draw: the last 40%");
            var weapon = Armoury.Resolve(Weapons.KatanaName, KatanaWay.Stream, new MarkSet(KatanaWay.Stream, KatanaWay.SteppingWater, KatanaWay.RisingDraw));
            var combo = weapon.Combo;
            Assert.AreEqual(0f, combo.DashCutFrom, 1e-6f, "Stepping Water takes the Rising Draw's input");
            Assert.IsTrue(combo.DashCutInChain);
            var stamina = Stamina();
            Cut(weapon, stamina);
            Run(combo, stamina, combo.Steps[0].Total + 0.05f);
            Assert.AreEqual(AttackPhase.None, combo.Phase);
            combo.Press(false, true);
            combo.Tick(0.001f, stamina);
            Assert.AreSame(combo.DashAttack, combo.Current, "the cut out of the roll");
            Run(combo, stamina, combo.DashAttack.Total + 0.05f);
            combo.Press();
            combo.Tick(0.001f, stamina);
            Assert.AreSame(combo.Steps[2], combo.Current, "it was the chain's second step: the third follows");
            Assert.IsFalse(Weapons.Katana().Combo.DashCutInChain, "plain: the dodge's cut is outside the chain");
        }

        [Test]
        public void ZanshinsTurnIsSharedOverItsTime()
        {
            Assert.AreEqual(0.5f, KatanaTrunk.TurnShare(0.05f, 0.1f), 1e-5f);
            Assert.AreEqual(1f, KatanaTrunk.TurnShare(0.05f, 0.03f), 1e-5f, "the last step finishes it");
            Assert.AreEqual(0.15f, KatanaTrunk.ZanshinTurnSeconds, 1e-6f);
            Assert.AreEqual(8f, KatanaTrunk.ZanshinRange, 1e-6f);
        }
    }
}
