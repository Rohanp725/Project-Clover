using Clover.Player.Trees.Ways;
using UnityEngine;

namespace Clover.Player.Way.Aspects
{
    /// <summary>
    /// The Stream (flow; way_plan.md 1): five cuts in place of three, a turning cut (10, 200 degrees) and a rising cut (14)
    /// after the built three, on the return cut's and the drawing cut's clips until their own are made. Light Wrist
    /// quickens the cuts from the third on; Stepping Water takes the dodge's cut from the Rising Draw; Unbroken gives the
    /// chain's wind-ups and swings poise against small blows. The dodge that keeps the chain, River Bend's overhead and
    /// both keystones wait for their engine pieces (<see cref="KatanaWay"/>).
    /// </summary>
    public sealed class StreamAspect : WeaponAspect
    {
        public const float TurningDamage = 10f, TurningStagger = 10f, TurningArc = 200f;
        public const float RisingDamage = 14f, RisingStagger = 14f;
        public const float LightWristShare = 0.92f;
        public const float SteppingDamage = 12f, SteppingStagger = 12f, SteppingArc = 140f;
        public const float UnbrokenPoise = 12f;

        public override string Id => KatanaWay.Stream;
        public override string Weapon => Weapons.KatanaName;

        public override void Patch(Weapon weapon, IMarkQuery marks)
        {
            if (weapon?.Combo == null || weapon.Combo.Steps.Count < 2 || marks == null) return;
            var steps = weapon.Combo.Steps;
            // NEEDS: clips of their own for the turning and the rising cut.
            var turning = CopyOf(weapon, 1);
            turning.Damage = TurningDamage;
            turning.Stagger = TurningStagger;
            turning.ArcDegrees = TurningArc;
            var rising = CopyOf(weapon, 0);
            rising.Damage = RisingDamage;
            rising.Stagger = RisingStagger;
            rising.Sweep = 0f;
            steps.Add(turning);
            steps.Add(rising);
            // A dodge in a cut's recovery keeps the chain; the next light carries on from where it was.
            weapon.Combo.ChainKeptThroughDodge = true;
            if (marks.Has(KatanaWay.LightWrist))
                for (int i = 2; i < steps.Count; i++) Retime(steps[i], LightWristShare);
            if (marks.Has(KatanaWay.SteppingWater))
            {
                // From any part of the roll, counting as the chain's next step. NEEDS: its clip, attack_katana_roll_cut (way-clips).
                var stepping = CopyOf(weapon, 0);
                stepping.Damage = SteppingDamage;
                stepping.Stagger = SteppingStagger;
                stepping.ArcDegrees = SteppingArc;
                weapon.Combo.DashAttack = stepping;
                weapon.Combo.DashCutFrom = 0f;
                weapon.Combo.DashCutInChain = true;
            }
            if (marks.Has(KatanaWay.Unbroken))
                for (int i = 0; i < steps.Count; i++) steps[i].Poise = Mathf.Max(steps[i].Poise, UnbrokenPoise);
        }
    }
}
