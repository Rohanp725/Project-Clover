using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Blight: the branch Dark and Poison share. Rot that feeds its owner: the poison's bite comes back as health,
    /// the Cursed dead carry the sickness to the living, and the player can take hold of an enemy and drink it. Its
    /// crown, The Reaping, is given when the second of the two gods is consumed.
    /// </summary>
    public static class BlightTree
    {
        public const string Wither = "blight.wither";
        public const string FeedingRot = "blight.feedingrot";
        public const string CarrionFlies = "blight.carrionflies";
        public const string WastingGrip = "blight.wastinggrip";
        public const string GraveSoil = "blight.gravesoil";
        public const string OneDeathFeeds = "blight.onedeathfeeds";
        public const string TheReaping = "blight.reaping";

        // Spell forms by the ids fixed in docs/design/FORMS.md; they are being built while this is written.
        private const string FormBeam = "beam";
        private const string FormField = "field";

        public static void Add(List<Upgrade> to)
        {
            // The first element is the one whose mix this is: the earlier of the two round the ring.
            const string A = Elements.Dark, B = Elements.Poison;
            const AfflictionKind Cursed = AfflictionKind.Cursed, Poisoned = AfflictionKind.Poison;

            // Built in the beat (Afflictions.PoisonOnCursed, kept by PlayerUpgrades): a quarter harder on the Cursed, and it does not fade while the curse is on them.
            to.Add(new Upgrade { Id = Wither, Name = "Wither", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, X = 0f, Y = 1f, Icon = UpgradeIcons.Leaf,
                Description = "Heavy blows both Curse and poison. Poison bites harder on the Cursed, and does not fade from them while the curse lasts.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = Cursed, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = Poisoned, Stacks = 2 },
                } });
            // On the bite itself (UpgradeTrigger.PoisonBite): half a point of health for each dose that bit.
            to.Add(new Upgrade { Id = FeedingRot, Name = "Feeding Rot", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Wither }, X = -0.5f, Y = 2.36f, Icon = UpgradeIcons.Plus,
                Description = "Each time poison bites a Cursed enemy, a little of it comes back to you as health.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PoisonBite, Do = UpgradeAction.Heal, Needs = Cursed, PerStack = true, Amount = 0.5f } } });
            // BESPOKE: the grip that holds an enemy, drinks its poison and throws it. Until then a short held stream bites what stands before the player, and spells that strike the poisoned feed.
            to.Add(new Upgrade { Id = WastingGrip, Name = "Wasting Grip", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { Wither }, X = 0.5f, Y = 2.36f, Icon = UpgradeIcons.Claw,
                Description = "Hold the spell button to brace and pour a short stream of rot at what stands before you. It bites again and again, and striking a poisoned enemy with a spell feeds you. A blow that lands on you breaks it. 40 mana.",
                Form = FormBeam,
                Spell = new SpellSpec { Cost = 40f, Reach = 2.5f, Radius = 0.6f, Seconds = 2.5f, Interval = 0.5f, Damage = 8f, Stagger = 6f, Stacks = 1 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Heal, Needs = Poisoned, Amount = 5f, Cooldown = 1f } } });
            // NEAREST: a burst round the body gives the dead one's doses and a curse to all within five metres. No swarm flies to the nearest.
            to.Add(new Upgrade { Id = CarrionFlies, Name = "Carrion Flies", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { FeedingRot }, X = 0f, Y = 3.48f, Icon = UpgradeIcons.Wing,
                Description = "An enemy that dies Cursed and poisoned bursts a moment later. Everyone near takes the doses it carried, and a curse.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Poison, Needs2 = AfflictionKind.Cursed, Do = UpgradeAction.Burst, Affliction = AfflictionKind.Poison, Damage = 3f, Radius = 5f, Delay = 0.8f, Stacks = 1, CarriesDepth = true, Cooldown = 0.5f }, new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Cursed, Needs2 = AfflictionKind.Poison, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Cursed, Stacks = 1, Radius = 5f, Cooldown = 0.5f } } });
            // The ground lays only the affliction of the element held, and does not hurry the beat.
            to.Add(new Upgrade { Id = GraveSoil, Name = "Grave Soil", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { FeedingRot }, X = -0.5f, Y = 4.6f, Icon = UpgradeIcons.Spike,
                Description = "Hold the spell button to foul the ground round you. What stands on it is bitten every two seconds and takes the affliction of the element you hold. 40 mana.",
                Form = FormField,
                Spell = new SpellSpec { Cost = 40f, Reach = 0.5f, Radius = 5f, Seconds = 8f, Interval = 2f, LingerDamage = 3f, Stacks = 1 } });
            // Built on the beat (UpgradeAction.Bite): every poisoned enemy within twelve metres of the dead one bites on the next step, and each one bitten feeds four points.
            to.Add(new Upgrade { Id = OneDeathFeeds, Name = "One Death Feeds", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { CarrionFlies }, X = 0.5f, Y = 4.6f, Icon = UpgradeIcons.Skull,
                Description = "When a Cursed enemy dies, the poison in every other enemy near it bites at once, and you are fed for each one bitten.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Cursed, Do = UpgradeAction.Bite, Radius = 12f, Amount = 4f, Cooldown = 0.5f } } });

            // BESPOKE (built): ReapingForm.
            to.Add(new Upgrade { Id = TheReaping, Name = "The Reaping", Element = A, Element2 = B, Kind = UpgradeKind.Signature, X = 0f, Y = 5.8f, Icon = UpgradeIcons.Crown,
                Description = "Your finisher on a Cursed, poisoned enemy reaps the field. All the poison in it and in every enemy near lands at once, and half of it is yours.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Form, Form = SpellForms.Reaping, Needs = AfflictionKind.Cursed, Needs2 = AfflictionKind.Poison, Affliction = AfflictionKind.Poison, Spell = new SpellSpec { Cost = 0f, Radius = 9f, Damage = 3f, Stagger = 4f } } } });
        }
    }
}
