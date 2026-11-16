using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Pyre: the branch Poison and Fire share. Poison is the fuel and fire is the spark: doses are laid with patience
    /// and then lit, and a slow death becomes an explosion. Its crown, Light the Pyres, is a spell, given when the
    /// second of the two gods is consumed.
    /// </summary>
    public static class PyreTree
    {
        public const string Fuel = "pyre.fuel";
        public const string GoesUp = "pyre.flashpoint";
        public const string FuneralFire = "pyre.funeralfire";
        public const string FireEater = "pyre.fireeater";
        public const string LitFuse = "pyre.slowfuse";
        public const string SlowBurn = "pyre.slowburn";
        public const string LightThePyres = "pyre.lightthepyres";

        // Spell forms by the ids fixed in docs/design/FORMS.md; they are being built while this is written.
        private const string FormBeam = "beam";
        private const string FormBrand = "brand";
        private const string FormPillars = "pillars";

        public static void Add(List<Upgrade> to)
        {
            // The first element is the one whose mix this is: the earlier of the two round the ring.
            const string A = Elements.Poison, B = Elements.Fire;
            const AfflictionKind Poisoned = AfflictionKind.Poison, Burn = AfflictionKind.Burn;

            to.Add(new Upgrade { Id = Fuel, Name = "Fuel", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, X = 0f, Y = 1f, Icon = UpgradeIcons.Drop,
                Description = "Heavy blows both poison and burn.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = Poisoned, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = Burn, Stacks = 1 },
                } });
            // It answers any affliction filling on a burning enemy, so a burn that fills first sets off poison of any depth.
            to.Add(new Upgrade { Id = GoesUp, Name = "Goes Up", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { Fuel }, X = -0.5f, Y = 2.36f, Icon = UpgradeIcons.Burst,
                Description = "When a burning enemy's poison reaches its full depth, it goes up. The poison explodes and sets alight everyone near.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictionFull, Do = UpgradeAction.SetOff, Needs = AfflictionKind.Poison, NeedsFull = true, Needs2 = AfflictionKind.Burn, Affliction = AfflictionKind.Poison, Stagger = 4f, Radius = 3f, Cooldown = 0.5f }, new UpgradeEffect { On = UpgradeTrigger.AfflictionFull, Do = UpgradeAction.Afflict, Needs = AfflictionKind.Poison, NeedsFull = true, Needs2 = AfflictionKind.Burn, Affliction = AfflictionKind.Burn, Stacks = 2, Radius = 3f, Cooldown = 0.5f } } });
            // BESPOKE: a cone that stands for as long as the button is held. The beam form is the nearest held breath; it lays only the affliction of the element held.
            to.Add(new Upgrade { Id = FireEater, Name = "Fire Eater", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { Fuel }, X = 0.5f, Y = 2.36f, Icon = UpgradeIcons.Wave,
                Description = "Hold the spell button to take a mouthful and breathe it out as green fire, for as long as you hold. It poisons and burns all it sweeps across. 40 mana.",
                Form = FormBeam,
                Spell = new SpellSpec { Cost = 40f, Reach = 5f, Radius = 1.2f, Seconds = 2f, Interval = 0.25f, Damage = 4f, Stagger = 3f, Stacks = 1 } });
            // Every death does it, whatever the dead one carried.
            to.Add(new Upgrade { Id = FuneralFire, Name = "Funeral Fire", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { GoesUp }, X = 0f, Y = 3.48f, Icon = UpgradeIcons.Skull,
                Description = "An enemy that dies poisoned and burning explodes, and leaves the ground burning where it fell.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Poison, Needs2 = AfflictionKind.Burn, Do = UpgradeAction.Burst, Affliction = AfflictionKind.Poison, Damage = 20f, Stagger = 20f, Radius = 3.5f, Stacks = 2, Cooldown = 0.5f }, new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Poison, Needs2 = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1, Radius = 3.5f, Cooldown = 0.5f }, new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Poison, Needs2 = AfflictionKind.Burn, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Affliction = AfflictionKind.Burn, Damage = 5f, Radius = 1.5f, Seconds = 4f, Interval = 0.5f, Stacks = 1, Cooldown = 0.5f } } });
            // The fuse is the brand form: the next blow of the weapon on the enemy sets it off, at one size. It does not yet burn down by time, grow with doses or answer a perfect guard.
            to.Add(new Upgrade { Id = LitFuse, Name = "Lit Fuse", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { GoesUp }, X = -0.5f, Y = 4.6f, Icon = UpgradeIcons.Target,
                Description = "Hold the spell button to set a fuse on the enemy you face. It burns down while you work. Every dose you lay makes the blast greater, and a perfect guard against that enemy sets it off at once, and harder. 45 mana.",
                Form = FormBrand,
                Spell = new SpellSpec { Cost = 45f, Reach = 8f, Seconds = 5f, Count = 1, Radius = 3.5f, Damage = 40f, Stagger = 40f, Stacks = 2 } });
            // BESPOKE: burn biting on the poison's beat, each stronger for the depth of the other. Until then both simply work harder.
            to.Add(new Upgrade { Id = SlowBurn, Name = "Slow Burn", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { FuneralFire }, X = 0.5f, Y = 4.6f, Icon = UpgradeIcons.Hourglass,
                Description = "Burn and poison keep one time. Both bite together on the beat, and each bites harder for the depth of the other.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionPower, 0.3f) } });

            // BESPOKE (built): PyresForm.
            to.Add(new Upgrade { Id = LightThePyres, Name = "Light the Pyres", Element = A, Element2 = B, Kind = UpgradeKind.Signature, X = 0f, Y = 5.8f, Icon = UpgradeIcons.Crown,
                Description = "Hold the spell button and drive your weapon into the ground. A line of fire runs to every poisoned or burning enemy near. Stand through three breaths, and a pillar of flame rises under each. 60 mana.",
                Form = SpellForms.Pyres,
                Spell = new SpellSpec { Cost = 60f, Count = 6, Reach = 9f, Radius = 2f, Damage = 40f, Stagger = 40f, Stacks = 2, Delay = 2.2f } });
        }
    }
}
