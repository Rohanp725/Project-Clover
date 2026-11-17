using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Plasma: the branch Fire and Lightning share. Burn and Static on one enemy set each other off, and the bursts
    /// travel. It is open to one who holds either and has held or consumed the other in this run. The god's gift at
    /// its crown is given by consuming one of the two into the other.
    /// </summary>
    public static class PlasmaTree
    {
        public const string ChargedEmbers = "plasma.embers";
        public const string ArcFire = "plasma.arcfire";
        public const string TwinBreak = "plasma.twin";
        public const string BallLightning = "plasma.ball";
        public const string ChainReaction = "plasma.chain";
        public const string Meltdown = "plasma.meltdown";
        // Kept from the first slice; not in the design.
        public const string IonisedAir = "plasma.ionised";
        public const string FusedCore = "plasma.fused";
        public const string PlasmaLance = "plasma.lance";

        public static void Add(List<Upgrade> to)
        {
            // The first element is the one whose mix this is: the earlier of the two round the ring.
            const string A = Elements.Fire, B = Elements.Lightning;

            // The foot of the branch. Both afflictions are named; the runner lays only what the held element's root has not laid with the same blow (schema 15.8). Static left by a Fire holder is set off by a heavy blow here, as Lightning's root would.
            to.Add(new Upgrade { Id = ChargedEmbers, Name = "Charged Embers", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, X = 0f, Y = 1f, Icon = UpgradeIcons.Flame,
                Description = "Your blows lay Burn and Static together.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.LightHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Static, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Cooldown = 0.3f },
                } });
            // There is no trigger for "an affliction is set off". A heavy blow on an enemy carrying Static is the discharge;
            // a perfect guard is when Burn and Detonate sets a burn off, and by then the burn is spent, so it asks for nothing.
            to.Add(new Upgrade { Id = ArcFire, Name = "Arc Fire", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { ChargedEmbers }, X = -0.5f, Y = 2.02f, Icon = UpgradeIcons.Chain,
                Description = "A discharge of Static carries Burn to everyone it reaches. A burn set off leaves Static on everyone it reaches.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = AfflictionKind.Static, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 2, Radius = 4f }, new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 2, Radius = 3.2f } } });
            // An effect can ask for one affliction, not two: each is set off if it is there. The flinch that breaks an attack is
            // still to be written by hand. Burn and Detonate, when learned, spends the burn first.
            to.Add(new Upgrade { Id = TwinBreak, Name = "Twin Break", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { ChargedEmbers }, X = 0.5f, Y = 2.02f, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard sets off the Burn on the one who struck, and the Static on it too, each in a burst that strikes all beside it.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Burn, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Burn, Damage = 5f, Stagger = 5f, Radius = 3f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Static, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Damage = 3.5f, Stagger = 5f, Radius = 3f },
                } });
            // Kept from the first slice; not in the design. It was the foot of the branch; it grows from Twin Break now, so the branch stands in two clean columns.
            to.Add(new Upgrade { Id = IonisedAir, Name = "Ionised Air", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { TwinBreak }, X = 0.5f, Y = 3.7f, Icon = UpgradeIcons.Chain,
                Description = "When an affliction on an enemy reaches full depth, an arc leaps from it through three more.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictionFull, Do = UpgradeAction.Arc, Damage = 10f, Stagger = 8f, Count = 3, Reach = 5f, Cooldown = 1f } } });
            to.Add(new Upgrade { Id = ChainReaction, Name = "Chain Reaction", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { ArcFire }, X = -0.5f, Y = 3.7f, Icon = UpgradeIcons.Skull,
                Description = "An enemy that dies carrying both Burn and Static bursts, and passes both on at full depth.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Burn, Needs2 = AfflictionKind.Static, Do = UpgradeAction.Burst, Damage = 18f, Radius = 4f, Affliction = AfflictionKind.Burn, Stacks = 1, CarriesDepth = true, Cooldown = 0.3f }, new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Static, Needs2 = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1, CarriesDepth = true, Radius = 4f, Cooldown = 0.3f } } });
            // BESPOKE (built): BallForm.
            to.Add(new Upgrade { Id = BallLightning, Name = "Ball Lightning", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { ArcFire }, X = 0f, Y = 2.86f, Icon = UpgradeIcons.Orb,
                Description = "Hold the spell button to loose a slow ball of white fire. Strike it to send it where the blow points, larger each time, up to three. It bursts on the first enemy it meets. 40 mana.",
                Form = SpellForms.Ball,
                Spell = new SpellSpec { Cost = 40f, Speed = 2f, Seconds = 8f, Radius = 2.5f, Damage = 16f, Stagger = 20f, Stacks = 2, LingerDamage = 12f } });
            // Kept from the first slice; not in the design.
            to.Add(new Upgrade { Id = FusedCore, Name = "Fused Core", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { IonisedAir }, X = 0.5f, Y = 5.38f, Icon = UpgradeIcons.Orb,
                Description = "Spells hit harder, and leave both a burn and Static on what they strike.",
                Stats = new[] { new StatMod(PlayerStat.SpellDamage, 0.15f) },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1 },
                } });
            // An effect can ask for one affliction, not two, and cannot tell which one filled: this answers any affliction
            // reaching full depth on an enemy that carries Static, with a smaller burst after a shorter wait. Neither is spent.
            to.Add(new Upgrade { Id = Meltdown, Name = "Meltdown", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { ChainReaction }, X = -0.5f, Y = 5.38f, Icon = UpgradeIcons.Burst,
                Description = "When any affliction reaches full depth on an enemy that carries Static, it bursts two seconds later and strikes everything within five metres. This can happen once every six seconds.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictionFull, Needs = AfflictionKind.Static, Do = UpgradeAction.Burst, Delay = 2f, Damage = 30f, Stagger = 45f, Radius = 5f, Cooldown = 6f } } });
            // Kept from the first slice; not in the design.
            to.Add(new Upgrade { Id = PlasmaLance, Name = "Plasma Lance", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { FusedCore }, X = 0f, Y = 6.22f, Icon = UpgradeIcons.Beam,
                Description = "Hold the spell button to throw a bolt of plasma that leaps through three more. 35 mana.",
                Form = SpellForms.Bolt,
                Spell = new SpellSpec { Cost = 35f, Reach = 11f, Damage = 30f, Stagger = 26f, Count = 3, Radius = 5f, Stacks = 2 } });

            // BESPOKE (as built): the burst on a burning enemy, done by hand in PlayerUpgrades under Upgrade.Plasma. It is this
            // branch's crown and no longer Fire's. Still to add: with Charged Embers held the burst also lays two Static.
            to.Add(new Upgrade { Id = Upgrade.Plasma, Name = "Plasma", Element = A, Element2 = B, Kind = UpgradeKind.Signature, X = 0f, Y = 8.68f, Icon = UpgradeIcons.Crown,
                Description = "A heavy blow or a finisher on a burning enemy bursts, and sets alight everyone near." });
        }
    }
}
