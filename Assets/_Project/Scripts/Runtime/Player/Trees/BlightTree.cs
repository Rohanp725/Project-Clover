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

            // Poison does not yet bite harder on the Cursed or stay for as long as the Curse does: everything the player lays lasts longer.
            to.Add(new Upgrade { Id = Wither, Name = "Wither", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, X = 0f, Y = 1f, Icon = UpgradeIcons.Leaf,
                Description = "Heavy blows both Curse and poison. Poison bites harder on the Cursed, and does not fade from them.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, 0.25f) },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = Cursed, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = Poisoned, Stacks = 2 },
                } });
            // There is no trigger for the bite itself: a blow or a spell on a poisoned enemy feeds, once a beat.
            to.Add(new Upgrade { Id = FeedingRot, Name = "Feeding Rot", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Wither }, X = -0.5f, Y = 2.36f, Icon = UpgradeIcons.Plus,
                Description = "Each time poison bites a Cursed enemy, a little of it comes back to you as health.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictedHit, Do = UpgradeAction.Heal, Needs = Poisoned, Amount = 2f, Cooldown = 2f } } });
            // BESPOKE: the grip that holds an enemy, drinks its poison and throws it. Until then a short held stream bites what stands before the player, and spells that strike the poisoned feed.
            to.Add(new Upgrade { Id = WastingGrip, Name = "Wasting Grip", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { Wither }, X = 0.5f, Y = 2.36f, Icon = UpgradeIcons.Claw,
                Description = "Hold the spell button to seize the enemy before you and keep hold. While you hold, its poison bites again and again and every bite feeds you. Let go to throw it at the others. A blow from anyone breaks your grip. 40 mana.",
                Form = FormBeam,
                Spell = new SpellSpec { Cost = 40f, Reach = 2.5f, Radius = 0.6f, Seconds = 2.5f, Interval = 0.5f, Damage = 8f, Stagger = 6f, Stacks = 1 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Heal, Needs = Poisoned, Amount = 5f, Cooldown = 1f } } });
            // NEAREST: a burst round the body gives the dead one's doses and a curse to all within five metres. No swarm flies to the nearest.
            to.Add(new Upgrade { Id = CarrionFlies, Name = "Carrion Flies", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { FeedingRot }, X = 0f, Y = 3.48f, Icon = UpgradeIcons.Wing,
                Description = "An enemy that dies Cursed and poisoned lets go a swarm. It finds the nearest living enemy and gives it everything the dead one carried.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Poison, Needs2 = AfflictionKind.Cursed, Do = UpgradeAction.Burst, Affliction = AfflictionKind.Poison, Damage = 3f, Radius = 5f, Delay = 0.8f, Stacks = 1, CarriesDepth = true, Cooldown = 0.5f }, new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Cursed, Needs2 = AfflictionKind.Poison, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Cursed, Stacks = 1, Radius = 5f, Cooldown = 0.5f } } });
            // The ground lays only the affliction of the element held, and does not hurry the beat.
            to.Add(new Upgrade { Id = GraveSoil, Name = "Grave Soil", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { FeedingRot }, X = -0.5f, Y = 4.6f, Icon = UpgradeIcons.Spike,
                Description = "Hold the spell button to foul the ground round you. On it every enemy is Cursed and poison bites twice as often. 40 mana.",
                Form = FormField,
                Spell = new SpellSpec { Cost = 40f, Reach = 0.5f, Radius = 5f, Seconds = 8f, Interval = 2f, LingerDamage = 3f, Stacks = 1 } });
            // NEAREST: a set burst on everyone within twelve metres and a set mending when a Cursed enemy dies. The poison itself does not bite.
            to.Add(new Upgrade { Id = OneDeathFeeds, Name = "One Death Feeds", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { CarrionFlies }, X = 0.5f, Y = 4.6f, Icon = UpgradeIcons.Skull,
                Description = "When a Cursed enemy dies, the poison in every other enemy bites at once, and you are fed by all of it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Cursed, Do = UpgradeAction.Burst, Affliction = AfflictionKind.Poison, Damage = 10f, Stagger = 4f, Radius = 12f, Cooldown = 1f }, new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Cursed, Do = UpgradeAction.Heal, Amount = 5f, Cooldown = 1f } } });

            // BESPOKE: the great blade. Until then a finisher on a Cursed, poisoned enemy sets off that enemy's own poison across nine metres and mends a set amount.
            to.Add(new Upgrade { Id = TheReaping, Name = "The Reaping", Element = A, Element2 = B, Kind = UpgradeKind.Signature, X = 0f, Y = 5.8f, Icon = UpgradeIcons.Crown,
                Description = "Your finisher on a Cursed, poisoned enemy reaps the field. All the poison in it and in every enemy near lands at once, and half of it is yours.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.SetOff, Needs = AfflictionKind.Cursed, Needs2 = AfflictionKind.Poison, Affliction = AfflictionKind.Poison, Damage = 3f, Stagger = 4f, Radius = 9f }, new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Needs = AfflictionKind.Cursed, Needs2 = AfflictionKind.Poison, Do = UpgradeAction.Heal, Amount = 15f } } });
        }
    }
}
