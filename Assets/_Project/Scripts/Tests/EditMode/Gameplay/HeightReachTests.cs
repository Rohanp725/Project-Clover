using System.Linq;
using Clover.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// A blow struck by hand reaches only so far up or down (the owner's play of 2026-11-18: a thirsting on the riverbed
    /// hit a player standing on the deck of the barque above it). Thrown and called blows, and a god's, reach as before.
    /// </summary>
    public class HeightReachTests
    {
        private GameObject made;

        [TearDown]
        public void Clear()
        {
            if (made != null) Object.DestroyImmediate(made);
        }

        [Test]
        public void ABlowByHandReachesTheSameFloorAStepAndAStairButNotARoofOrTheFloorBelow()
        {
            Assert.IsTrue(HeightReach.Within(0f, 0f), "the same floor");
            Assert.IsTrue(HeightReach.Within(4f, 4.45f), "a step up");
            Assert.IsTrue(HeightReach.Within(6f, 4.9f), "a stair's landing below");
            Assert.IsTrue(HeightReach.Within(10f, 10f + HeightReach.Melee), "just in reach");
            Assert.IsFalse(HeightReach.Within(0f, 3.5f), "the barque's deck over the riverbed");
            Assert.IsFalse(HeightReach.Within(10f, 6f), "the street under a roof");
            Assert.IsFalse(HeightReach.Within(0f, -1.3f), "a little more than the reach");
        }

        [TestCase(EnemyKind.ThirstName)]
        [TestCase(EnemyKind.WickName)]
        [TestCase(EnemyKind.GildedLevyName)]
        [TestCase(EnemyKind.SoldierName)]
        [TestCase(EnemyKind.AcolyteName)]
        [TestCase(EnemyKind.CaptainName)]
        public void AnEnemysBlowsByHandLandOnTheFlatAndNotOnAPlayerFarAboveOrBelow(string kind)
        {
            var (enemy, target) = Make(kind);
            foreach (var attack in enemy.Brain.Attacks.Where(a => !a.Ranged && !a.Casts && !a.Beams))
            {
                target.position = enemy.transform.position + new Vector3(0f, 0f, 1.5f);
                Assert.IsTrue(enemy.ReachesHeight(attack), $"{kind} {attack.Name} on the flat");
                target.position = enemy.transform.position + new Vector3(0f, 0.6f, 1.5f);
                Assert.IsTrue(enemy.ReachesHeight(attack), $"{kind} {attack.Name} a step up");
                target.position = enemy.transform.position + new Vector3(0f, 3.5f, 1.5f);
                Assert.IsFalse(enemy.ReachesHeight(attack), $"{kind} {attack.Name} on a deck 3.5 m up");
                target.position = enemy.transform.position + new Vector3(0f, -4f, 1.5f);
                Assert.IsFalse(enemy.ReachesHeight(attack), $"{kind} {attack.Name} on the floor 4 m below");
            }
        }

        [Test]
        public void ThrownAndCalledBlowsAndAGodsReachAsTheyAlwaysDid()
        {
            var (caster, target) = Make(EnemyKind.CasterName);
            target.position = caster.transform.position + new Vector3(0f, 4f, 6f);
            foreach (var attack in caster.Brain.Attacks.Where(a => a.Ranged))
                Assert.IsTrue(caster.ReachesHeight(attack), "a bolt called down reaches a roof: " + attack.Name);
            Object.DestroyImmediate(made);
            var (king, near) = Make(EnemyKind.SunKingName);
            near.position = king.transform.position + new Vector3(0f, 3f, 2f);
            foreach (var attack in king.Brain.Attacks)
                Assert.IsTrue(king.ReachesHeight(attack), "the Fire god's man is left as he was: " + attack.Name);
        }

        // An enemy of a kind and a target, without the scene: only what the height rule reads.
        private (Enemy, Transform) Make(string kind)
        {
            made = new GameObject("height test");
            var target = new GameObject("target").transform;
            target.SetParent(made.transform, false);
            var enemy = new GameObject("enemy").AddComponent<Enemy>();
            enemy.transform.SetParent(made.transform, false);
            enemy.transform.position = new Vector3(0f, 2f, 0f);
            enemy.Configure(kind, target, null, new Renderer[0], null, null, null, null, null, null, null, null);
            typeof(Enemy).GetProperty(nameof(Enemy.Kind)).GetSetMethod(true).Invoke(enemy, new object[] { EnemyKind.ByName(kind) });
            Assert.IsNotNull(enemy.Kind, kind);
            return (enemy, target);
        }
    }
}
