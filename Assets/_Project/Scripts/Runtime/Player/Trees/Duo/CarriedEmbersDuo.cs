using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Fire and Wind (pairs_d1_d2.md, "Fire and Wind", distance 2; task 4.15), named by its reaction,
    /// Carried Embers. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark
    /// opens Fire Whirl and Hot Wind; Fan the Flame and Long Embers, the keystone The Wind Carries It, and the duo spell The
    /// Fire Breathes In. Fire is written first: the duo spell is cast in Fire.
    /// </summary>
    public static class CarriedEmbersDuo
    {
        public const string Shape = "carriedembers.shape";
        public const string FanTheFlame = "carriedembers.fan";
        public const string LongEmbers = "carriedembers.long";
        public const string TheWindCarriesIt = "carriedembers.carries";
        public const string TheFireBreathesIn = "carriedembers.breathesin";
        /// <summary>The condition Fire Whirl sets: the turning column of flame on the enemy.</summary>
        public const string Whirl = "carriedembers.whirl";
        private const string Reaction = "react.carriedembers";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Fire, B = Elements.Wind;

            // NEEDS: a moving whirl hazard that pulls (Fire Whirl; now a column on the enemy that draws in and lights what is
            // near once a second); a lane that shoves all in it (Hot Wind; now the lane burns them and only the enemy is shoved).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Carried Embers: Fire Whirl or Hot Wind", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Carried Embers' two shapes, chosen at a rest fire. Fire Whirl: the flame stands up on it as a turning column that draws enemies in and lights them. Hot Wind: one long gust downwind instead of embers. Either runs instead of the plain Carried Embers.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Fire Whirl",
                        Description = "The flame stands up on it as a turning column for 3 s: once a second it draws every enemy within 3 m 1 m toward it and sets everything within 2 m burning.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Whirl, Seconds = 3f, Beat = 1f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Whirl, Do = UpgradeAction.Pull, Radius = 3f, Amount = 1f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Whirl, Do = UpgradeAction.Afflict, Radius = 2f, Affliction = AfflictionKind.Burn, Stacks = 1 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Hot Wind",
                        Description = "One long gust instead of embers: a lane 8 m long and 2 m wide downwind, away from you, that lays a Burn on all in it, and the enemy itself is shoved 2 m.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Lane, Reach = 8f, Radius = 1f, Damage = 0f, Affliction = AfflictionKind.Burn, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 2f },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // A perfect dodge names no attacker: the enemy faced is fanned.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = FanTheFlame, Name = "Fan the Flame", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Gust,
                Description = "A perfect dodge fans the flame on the attacker: one Burn deeper, and Carried Embers fires again at once, whatever its rest.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Needs = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Needs = AfflictionKind.Burn, Needs2 = AfflictionKind.Unsteady, Do = UpgradeAction.ForceReaction, Reaction = Reaction },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: a mark that changes the reaction's own numbers (reaction-numbers); now a fourth ember flies 7 m.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = LongEmbers, Name = "Long Embers", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { Shape }, Icon = UpgradeIcons.Flame,
                Description = "When Carried Embers fires, one more ember flies 2 m further.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Hazard, Shape = HazardShape.Downwind, Count = 1, Reach = 7f, Amount = 1f,
                        Radius = 1f, Seconds = 3f, Interval = 0.5f, Damage = 3f, Affliction = AfflictionKind.Burn, Stacks = 1, Delay = 0.45f },
                },
            }, HeartPattern.RightLow));

            // As if it had reacted, with no Unsteady needed and no rest; each throw costs it one Burn.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheWindCarriesIt, Name = "The Wind Carries It", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { FanTheFlame, LongEmbers }, Icon = UpgradeIcons.Burst,
                Description = "Your heavy blows on a burning enemy throw its embers downwind as if it had reacted, with no Unsteady needed and no rest; each throw costs it one Burn. Where you stand when you strike aims the fire.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Burn, Do = UpgradeAction.Hazard, Shape = HazardShape.Downwind, Count = 3, Reach = 5f, Amount = 1f,
                        Radius = 1f, Seconds = 3f, Interval = 0.5f, Damage = 3f, Affliction = AfflictionKind.Burn, Stacks = 1, Delay = 0.35f },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Burn, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Burn, Count = 1 },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with Shards once both are carried (no god gives it): an opener on a 16 s cooldown (SpellTimings). The reverse form draws enemies in to a
            // point and bursts there.
            // NEEDS: a ring of flame that stands 6 s and burns inside it every half second, Unsteady every 2 s on all within
            // 8 m, and heavy blows that throw toward it (pull-to-point); now a 3 s draw that bursts in fire.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheFireBreathesIn, Name = "The Fire Breathes In", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { FanTheFlame, LongEmbers }, Icon = UpgradeIcons.Sun,
                Description = "A ring of flame stands at the aim point and the fire draws its own wind: every enemy near is pulled toward it, unsteadied as it comes, and the fire bursts over them.",
                Form = SpellForms.Reverse,
                Spell = new SpellSpec { Reach = 6f, Radius = 6f, Delay = 3f, Damage = 20f, Stagger = 20f, Stacks = 2 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 1 } },
            }, HeartPattern.Spell));
        }
    }
}
