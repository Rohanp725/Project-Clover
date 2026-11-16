using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>The element's spell and surge, burning, and the promise that every attack in the game has an animation.</summary>
    public class ElementAndWeaponTests
    {
        private static ManaPool Mana(float amount)
        {
            var pool = new ManaPool { Maximum = 100f };
            pool.Empty();
            pool.Gain(amount);
            return pool;
        }

        [Test]
        public void ASpellCostsManaAndLeavesTheHandPartWayThroughTheCast()
        {
            var kit = new ElementKit { SpellCost = 25f, SpellWindup = 0.2f, SpellRecovery = 0.3f };
            kit.Reset();
            var mana = Mana(30f);
            Assert.IsTrue(kit.TryCast(mana));
            Assert.AreEqual(5f, mana.Current, 0.001f);
            Assert.IsFalse(kit.TryCast(mana), "one cast at a time");
            Assert.IsFalse(kit.TickCast(0.1f));
            Assert.IsTrue(kit.TickCast(0.15f), "this is the moment the spell is released");
            Assert.IsFalse(kit.TickCast(0.1f), "and it is released once");
            Assert.IsTrue(kit.IsCasting);
            kit.TickCast(0.2f);
            Assert.IsFalse(kit.IsCasting);
        }

        [Test]
        public void ASpellWithoutTheManaDoesNotStartAndAnInterruptedOneIsNotReleased()
        {
            var kit = new ElementKit { SpellCost = 25f, SpellWindup = 0.2f, SpellRecovery = 0.3f };
            kit.Reset();
            Assert.IsFalse(kit.TryCast(Mana(10f)));
            var mana = Mana(50f);
            kit.TryCast(mana);
            kit.TickCast(0.1f);
            kit.Interrupt();
            Assert.IsFalse(kit.TickCast(0.5f));
            Assert.AreEqual(25f, mana.Current, 0.001f, "the mana is gone all the same");
        }

        [Test]
        public void TheSurgeRunsItsTimeAndCanBeToppedUp()
        {
            var kit = new ElementKit { SurgeCost = 40f, SurgeSeconds = 10f };
            kit.Reset();
            var mana = Mana(100f);
            Assert.IsTrue(kit.TrySurge(mana));
            Assert.IsFalse(kit.TickSurge(6f));
            Assert.IsTrue(kit.TrySurge(mana), "lighting it again fills its time");
            Assert.AreEqual(10f, kit.SurgeLeft, 0.001f);
            Assert.IsFalse(kit.TickSurge(9f));
            Assert.IsTrue(kit.TickSurge(1.5f), "it reports the moment it runs out");
            Assert.IsFalse(kit.SurgeActive);
            Assert.IsFalse(kit.TrySurge(mana), "20 mana is not enough for another");
        }

        [Test]
        public void BurningDealsItsDamageOverItsTimeAndDoesNotStack()
        {
            var burn = new BurnStatus { DamagePerSecond = 5f, MaximumSeconds = 8f };
            burn.Reset();
            burn.Ignite(4f);
            burn.Ignite(2f);
            Assert.AreEqual(4f, burn.SecondsLeft, 0.001f, "a shorter burn does not cut a longer one");
            float total = 0f;
            for (int i = 0; i < 100; i++) total += burn.Tick(0.1f);
            Assert.AreEqual(20f, total, 0.01f);
            Assert.IsFalse(burn.IsBurning);
            burn.Ignite(100f);
            Assert.AreEqual(8f, burn.SecondsLeft, 0.001f);
        }

        [Test]
        public void EveryWeaponAttackHasAClipOnThePlayerModel()
        {
            var clips = Baked(Clover.EditorTools.Forge.PlayerAnimations.ClipFolder);
            foreach (var weapon in Weapons.All)
            {
                foreach (var step in weapon.AllSteps())
                    Assert.Contains(step.Name, clips, $"{weapon.Name} has an attack with no animation");
                Assert.Contains(weapon.ChargeAnimation, clips, $"{weapon.Name} has no charge animation");
            }
            foreach (string needed in new[] { "idle", "run", "idle_fist", "run_fist", "roll", PlayerAnimator.DeathState, PlayerAnimator.HurtState, PlayerAnimator.CastState, PlayerAnimator.GuardState, PlayerAnimator.FistGuardState, PlayerAnimator.DrinkState })
                Assert.Contains(needed, clips);
        }

        [Test]
        public void EveryEnemyAttackHasAClipOnItsModel()
        {
            foreach (var kind in EnemyKind.All)
            {
                var clips = Baked(Clover.EditorTools.Forge.EnemyAnimations.ClipFolder);
                foreach (var attack in kind.Brain.Attacks)
                    Assert.Contains(attack.Name, clips, $"{kind.Name} has an attack with no animation");
                foreach (string suffix in new[] { "_idle_loop", "_walk_loop", "_hurt", "_stagger", "_death" })
                    Assert.Contains(kind.Prefix + suffix, clips, kind.Name);
            }
        }

        [Test]
        public void TheFistsAreFasterAndTheKatanaReachesFurther()
        {
            var fists = Weapons.Fists().Combo;
            var katana = Weapons.Katana().Combo;
            Assert.Less(fists.Steps[0].Total, katana.Steps[0].Total);
            Assert.Less(fists.Steps[0].Reach, katana.Steps[0].Reach);
            Assert.Greater(fists.Steps.Count, katana.Steps.Count, "the fists have the longer chain");
        }

        // The names of the animation clips baked into a folder by the forge.
        private static List<string> Baked(string folder) =>
            AssetDatabase.FindAssets("t:AnimationClip", new[] { folder })
                .Select(guid => System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid))).ToList();

    }
}
