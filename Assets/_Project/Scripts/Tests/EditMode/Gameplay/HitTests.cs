using Clover.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>Which hits are felt as blows: ground that only lays an affliction must not shove, flinch or meet a guard.</summary>
    public class HitTests
    {
        [Test]
        public void ABiteWithNoDamageNoBalanceAndNothingSetOffOnlyLays()
        {
            var soaking = new Hit(0f, 0f, Vector3.forward, affliction: AfflictionKind.Soaked, afflictionStacks: 1);
            Assert.IsTrue(soaking.OnlyLays);
        }

        [Test]
        public void DamageBalanceAHeavyOrADetonationMakeItABlow()
        {
            Assert.IsFalse(new Hit(4f, 0f, Vector3.forward).OnlyLays, "damage");
            Assert.IsFalse(new Hit(0f, 5f, Vector3.forward).OnlyLays, "balance");
            Assert.IsFalse(new Hit(0f, 0f, Vector3.forward, heavy: true).OnlyLays, "a heavy");
            Assert.IsFalse(new Hit(0f, 0f, Vector3.forward, detonate: true).OnlyLays, "a detonation");
        }
    }
}
