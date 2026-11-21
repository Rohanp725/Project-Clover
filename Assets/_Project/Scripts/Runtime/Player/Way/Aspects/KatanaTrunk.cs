using Clover.Player.Trees.Ways;

namespace Clover.Player.Way.Aspects
{
    /// <summary>
    /// What the Katana's trunk does to its moves in every aspect (way_plan.md 1, "Trunk basics"; steel/weapons.md 1,
    /// "Temper"): Settled Hands' quicker lights, the Rising Draw as the dodge's cut (on the drawing cut's clip until
    /// katana_roll_cut is made), Temper III's longer drawing cut and Temper V's harder finisher. The rest of the trunk is
    /// stats and effects on its marks, or waits for its engine piece (<see cref="KatanaWay"/>).
    /// </summary>
    public sealed class KatanaTrunk : WayTrunk
    {
        public const float SettledRecovery = 0.9f;
        public const float RisingDamage = 12f, RisingStagger = 12f, RisingArc = 150f, RisingReach = 2.4f;
        public const float TemperThreeReach = 0.2f;
        public const float TemperFiveFinisher = 1.15f;

        public override string Weapon => Weapons.KatanaName;

        public override void Patch(Weapon weapon, IMarkQuery marks)
        {
            if (weapon?.Combo == null || marks == null) return;
            var steps = weapon.Combo.Steps;
            if (marks.Has(KatanaWay.SettledHands))
                for (int i = 0; i < steps.Count && i < 3; i++) steps[i].Recovery *= SettledRecovery;
            if (marks.Has(KatanaWay.RisingDraw) && steps.Count > 0)
            {
                // NEEDS: its own clip, katana_roll_cut; the drawing cut's is played.
                var rising = steps[0].Clone();
                rising.Damage = RisingDamage;
                rising.Stagger = RisingStagger;
                rising.ArcDegrees = RisingArc;
                rising.Reach = RisingReach;
                weapon.Combo.DashAttack = rising;
            }
            if (marks.Has(KatanaWay.Temper3))
            {
                if (steps.Count > 0) steps[0].Reach += TemperThreeReach;
                if (weapon.Carrier != null) weapon.Carrier.Reach += TemperThreeReach;
            }
            if (marks.Has(KatanaWay.Temper5) && weapon.Combo.Finisher != null) weapon.Combo.Finisher.Damage *= TemperFiveFinisher;
        }
    }
}
