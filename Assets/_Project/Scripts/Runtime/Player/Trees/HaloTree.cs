using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Halo: the branch Metal and Light share. The perfect guard made radiant: blades of light gather behind the
    /// shoulders, and a guard at the exact instant looses them, blinds, and sends back what was thrown. Its crown,
    /// Six Mirrors, is a spell given by a god and never sold.
    /// </summary>
    public static class HaloTree
    {
        public const string BrightSteel = "halo.brightsteel";
        public const string HaloOfBlades = "halo.haloofblades";
        public const string MirrorEdge = "halo.mirroredge";
        public const string AnsweringDraw = "halo.answeringdraw";
        public const string SunWheels = "halo.sunwheels";
        public const string BlindingGuard = "halo.blindingguard";
        public const string SixMirrors = "halo.sixmirrors";

        public static void Add(List<Upgrade> to)
        {
            // The first element is the one whose mix this is: the earlier of the two round the ring.
            const string A = Elements.Metal, B = Elements.Light;
            const AfflictionKind Notch = AfflictionKind.Sundered, Mark = AfflictionKind.Marked;

            // The foot of the branch: it grows from no mark (the design's "metal.sunder or light.mark" cannot be written, a mix may only grow from its own branch).
            to.Add(new Upgrade { Id = BrightSteel, Name = "Bright Steel", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, X = 0f, Y = 1f, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard flashes off the blade. The attacker is Marked, and its armour is notched.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = Mark, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = Notch, Stacks = 1 },
                } });
            to.Add(new Upgrade { Id = HaloOfBlades, Name = "Halo of Blades", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { BrightSteel }, X = -0.5f, Y = 2.36f, Icon = UpgradeIcons.Sun,
                Description = "Each critical blow forges a blade of light in a ring behind your shoulders, up to six. A perfect guard looses them all at the one who struck.",
                Tally = new TallySpec { Name = "Blades", Cap = 6 }, Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.CritHit, Do = UpgradeAction.Bank, Count = 1 }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, SpendsAll = true, PerStack = true, Do = UpgradeAction.Projectile, Flight = Flight.Homing, Count = 1, Damage = 9f, Stagger = 8f, Speed = 16f, Affliction = AfflictionKind.Sundered, Stacks = 1 } } });
            // NEEDS: a small extension, the perfect guard check on the skill's release. Here the Iaido Slash marks and notches what it cuts, and a perfect guard makes the blows of the next moment critical.
            to.Add(new Upgrade { Id = AnsweringDraw, Name = "Answering Draw", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { BrightSteel }, X = 0.5f, Y = 2.36f, Icon = UpgradeIcons.Blade, Weapon = Weapons.KatanaName,
                Description = "Needs the Katana. Release the Iaido Slash at the instant a blow would land. The blow is turned, and the draw cuts as light: critical, and through any guard.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SkillHit, Do = UpgradeAction.Afflict, Affliction = Mark, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SkillHit, Do = UpgradeAction.Afflict, Affliction = Notch, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.CritChance, Amount = 1f, Seconds = 1.5f },
                } });
            // NEEDS: a trigger "a perfect guard against something thrown", the damage of what was turned, and a critical effect. Here every perfect guard sends one bright bolt back.
            to.Add(new Upgrade { Id = MirrorEdge, Name = "Mirror Edge", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { HaloOfBlades }, X = 0f, Y = 3.48f, Icon = UpgradeIcons.Beam,
                Description = "A perfect guard against anything thrown sends it back as light.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Projectile, Flight = Flight.Straight, Damage = 20f, Stagger = 30f, Speed = 18f, Seconds = 1.5f, Affliction = Mark, Stacks = 1 } } });
            // The discs form orbits and strikes. NEEDS: the perfect guard that sends both wheels out through the attacker and back.
            to.Add(new Upgrade { Id = SunWheels, Name = "Sun Wheels", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { HaloOfBlades }, X = -0.5f, Y = 4.6f, Icon = UpgradeIcons.Spiral,
                Description = "Hold the spell button and two wheels of bright steel turn round you. A perfect guard sends both out through the attacker and back. 40 mana.",
                Form = "discs",
                Spell = new SpellSpec { Cost = 40f, Count = 2, Radius = 2.5f, Seconds = 9f, Interval = 0.5f, Damage = 5f, Stagger = 4f, Stacks = 1 } });
            // NEAREST: everyone within six metres is held for a moment (they cannot begin an attack, nor move), not only those in the cone ahead.
            to.Add(new Upgrade { Id = BlindingGuard, Name = "Blinding Guard", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { MirrorEdge }, X = 0.5f, Y = 4.6f, Icon = UpgradeIcons.Eye,
                Description = "A perfect guard blinds everyone before you. They are Marked, and for a breath none of them can begin an attack. A blow held on a plain guard costs more stamina.",
                Stats = new[] { new StatMod(PlayerStat.GuardStamina, 0.2f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Cone, Reach = 6f, Arc = 120f, Damage = 0f, Stagger = 15f, Affliction = AfflictionKind.Marked, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Hold, At = EffectAt.Self, Radius = 6f, Seconds = 1.5f } } });

            // BESPOKE in the design; the flash form carries it: six lines of light cross the enemy faced and the cuts land together, the player untouchable meanwhile.
            // Still to come: each cut critical and laying a notch as well as a mark, half damage to others on a line, and the 0.3 s in which the cast can be lost.
            to.Add(new Upgrade { Id = SixMirrors, Name = "Six Mirrors", Element = A, Element2 = B, Kind = UpgradeKind.Signature, X = 0f, Y = 5.8f, Icon = UpgradeIcons.Crown,
                Description = "Hold the spell button and six mirrors of steel stand in a ring round the enemy you face. You pass through it from each one in turn, faster than can be followed. The cuts land when you are back where you began. 60 mana.",
                Form = "flash",
                Spell = new SpellSpec { Cost = 60f, Reach = 8f, Count = 6, Interval = 0.15f, Delay = 0.3f, Damage = 14f, Stagger = 12f, Stacks = 1 } });
        }
    }
}
