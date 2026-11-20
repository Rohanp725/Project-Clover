using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Halo: the duo branch Metal and Light share, reshaped by pairs_d1_d2.md ("Metal and Light", task 4.15) and drawn in the
    /// chart's heart (<see cref="HeartPattern"/>). Its reaction is Glint; a new Shape mark opens its two shapes, Mirror to
    /// Mirror and Burning Mirror. Sun Wheels, offered to Metal's tree, whose roster is full, is kept hidden for the
    /// Evolutions (<see cref="SpareSpells"/>); Answering Draw needs the Katana and moves to the Katana's infusion marks on
    /// the Way (<see cref="RetiredMarks.Moving"/>).
    /// Kept: Bright Steel, Halo of Blades, Mirror Edge, Blinding Guard (the keystone) and Six Mirrors, the god's gift at the
    /// top that is also the branch's spell. Open while both are carried.
    /// </summary>
    public static class HaloTree
    {
        public const string Shape = "halo.shape";
        public const string BrightSteel = "halo.brightsteel";
        public const string HaloOfBlades = "halo.haloofblades";
        public const string MirrorEdge = "halo.mirroredge";
        public const string BlindingGuard = "halo.blindingguard";
        public const string SixMirrors = "halo.sixmirrors";
        /// <summary>The condition Burning Mirror sets: the white-hot point the next weapon blow strikes.</summary>
        public const string WhitePoint = "glint.point";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Metal, B = Elements.Light;
            const AfflictionKind Notch = AfflictionKind.Sundered, Mark = AfflictionKind.Marked;

            // NEEDS: an arc that leaps on only from those that carry an affliction (Mirror to Mirror leaps from each struck
            // that carries Sundered; now through four, whatever they carry); a blow made critical by a condition (Burning
            // Mirror; now the blow is answered by a burst of 10 on it).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Glint: Mirror to Mirror or Burning Mirror", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Glint's two shapes, chosen at a rest fire. Mirror to Mirror: the ray goes on from enemy to enemy across the room. Burning Mirror: no ray, the light gathers into one white-hot point for your next blow. Either runs instead of the plain Glint.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Mirror to Mirror",
                        Description = "The ray goes on from every enemy it strikes, up to four in all within 6 m of each other, drawing a zigzag across the room: 10, 15 stagger and a Mark each, and its notch passed on.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Arc, FromTarget = true, Count = 4, Reach = 6f, Damage = 10f, Stagger = 15f, Affliction = Mark, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Pass, Reach = 6f, Affliction = Notch, Stacks = 1 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Burning Mirror",
                        Description = "No ray: the light gathers into the cracks to one white-hot point for 5 s, and the next weapon blow on it strikes hard and lays two notches.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = WhitePoint, Seconds = 5f, EndsWith = ConditionBlow.Weapon } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = WhitePoint, By = StruckBy.Weapon, Do = UpgradeAction.Afflict, Affliction = Notch, Stacks = 2 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = WhitePoint, By = StruckBy.Weapon, Do = UpgradeAction.Burst, Radius = 0.6f, Damage = 10f, Affliction = Mark, Stacks = 0 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BrightSteel, Name = "Bright Steel", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard flashes off the blade. The attacker is Marked, and its armour is notched.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = Mark, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = Notch, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = HaloOfBlades, Name = "Halo of Blades", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Sun,
                Description = "Each critical blow forges a blade of light in a ring behind your shoulders, up to six. A perfect guard looses them all at the one who struck.",
                Tally = new TallySpec { Name = "Blades", Cap = 6 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.CritHit, Do = UpgradeAction.Bank, Count = 1 }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, SpendsAll = true, PerStack = true, Do = UpgradeAction.Projectile, Flight = Flight.Homing, Count = 1, Damage = 9f, Stagger = 8f, Speed = 16f, Affliction = AfflictionKind.Sundered, Stacks = 1 } },
            }, HeartPattern.RightLow));

            // NEEDS: a trigger "a perfect guard against something thrown", the damage of what was turned, and a critical effect. Here every perfect guard sends one bright bolt back.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = MirrorEdge, Name = "Mirror Edge", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { HaloOfBlades }, Icon = UpgradeIcons.Beam,
                Description = "A perfect guard throws a bolt of light straight back at the attacker. It strikes hard and Marks what it hits.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Projectile, Flight = Flight.Straight, Damage = 20f, Stagger = 30f, Speed = 18f, Seconds = 1.5f, Affliction = Mark, Stacks = 1 } },
            }, HeartPattern.RightHigh));

            // NEAREST: everyone within six metres is held for a moment (they cannot begin an attack, nor move), not only those in the cone ahead.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BlindingGuard, Name = "Blinding Guard", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { BrightSteel, MirrorEdge }, Icon = UpgradeIcons.Eye,
                Description = "A perfect guard blinds those before you: they are Marked and their balance shaken. Everyone within six metres, on any side, is held still for a breath. A blow held on a plain guard costs a fifth more stamina.",
                Stats = new[] { new StatMod(PlayerStat.GuardStamina, 0.2f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Cone, Reach = 6f, Arc = 120f, Damage = 0f, Stagger = 15f, Affliction = AfflictionKind.Marked, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Hold, At = EffectAt.Self, Radius = 6f, Seconds = 1.5f } },
            }, HeartPattern.Keystone));

            // BESPOKE (built): MirrorsForm. The god's gift at the top, which the pair file names the branch's duo spell; kept a
            // gift so the crown a consumed god gives is still there (phase4_questions.md, task 4.15).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SixMirrors, Name = "Six Mirrors", Element = A, Element2 = B, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "Six mirrors of steel stand in a ring round the enemy you face. You pass through it from each one in turn, faster than can be followed. The cuts land when you are back where you began. 60 mana.",
                Form = SpellForms.Mirrors,
                Spell = new SpellSpec { Cost = 60f, Reach = 8f, Radius = 3f, Count = 6, Interval = 0.13f, Delay = 0.3f, Damage = 14f, Stagger = 12f, Stacks = 1 },
            }, HeartPattern.Crown));
        }
    }
}
