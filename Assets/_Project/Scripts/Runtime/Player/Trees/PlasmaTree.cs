using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Plasma: the branch Fire and Lightning share. It is open to one who holds either and has held or consumed the
    /// other in this run. The god's gift at its crown is given by consuming the storm into Fire.
    /// </summary>
    public static class PlasmaTree
    {
        public const string IonisedAir = "plasma.ionised";
        public const string FusedCore = "plasma.fused";
        public const string PlasmaLance = "plasma.lance";

        public static void Add(List<Upgrade> to)
        {
            // The first element is the one whose mix this is: the earlier of the two round the ring.
            const string A = Elements.Fire, B = Elements.Lightning;

            to.Add(new Upgrade { Id = IonisedAir, Name = "Ionised Air", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, X = 0f, Y = 1f, Icon = UpgradeIcons.Chain,
                Description = "When an affliction on an enemy reaches full depth, an arc leaps from it through three more.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictionFull, Do = UpgradeAction.Arc, Damage = 10f, Stagger = 8f, Count = 3, Reach = 5f, Cooldown = 1f } } });
            to.Add(new Upgrade { Id = FusedCore, Name = "Fused Core", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { IonisedAir }, X = 0.45f, Y = 2f, Icon = UpgradeIcons.Orb,
                Description = "Spells hit harder, and leave both a burn and Static on what they strike.",
                Stats = new[] { new StatMod(PlayerStat.SpellDamage, 0.15f) },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1 },
                } });
            to.Add(new Upgrade { Id = PlasmaLance, Name = "Plasma Lance", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { FusedCore }, X = -0.45f, Y = 3f, Icon = UpgradeIcons.Beam,
                Description = "Hold the spell button to throw a bolt of plasma that leaps through three more. 35 mana.",
                Form = SpellForms.Bolt,
                Spell = new SpellSpec { Cost = 35f, Reach = 11f, Damage = 30f, Stagger = 26f, Count = 3, Radius = 5f, Stacks = 2 } });

            to.Add(new Upgrade { Id = Upgrade.Plasma, Name = "Plasma", Element = A, Element2 = B, Kind = UpgradeKind.Signature, X = 0f, Y = 4.6f, Icon = UpgradeIcons.Crown,
                Description = "A heavy blow or a finisher on a burning enemy bursts, and sets alight everyone near." });
        }
    }
}
