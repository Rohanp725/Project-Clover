using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Life: health spent like mana and won back with the weapon, and things that grow where the fight is. Three
    /// boughs from Seeded: on the left Offering (health as the price), in the middle Garden (thorns, saplings, the
    /// tree), on the right Bramble (the seed and the thorn). Form ids are plain strings: see docs/design/FORMS.md.
    /// </summary>
    public static class LifeTree
    {
        public const string Root = "life.seed";
        public const string PaidInBlood = "life.price";
        public const string DeepRoots = "life.deeproots";
        public const string SweetSap = "life.sweetsap";
        public const string ThinBlood = "life.thin";
        public const string Stubborn = "life.stubborn";
        public const string Quickening = "life.quickening";
        public const string RedHarvest = "life.harvest";
        public const string Heartwood = "life.heartwood";
        public const string FallenSeeds = "life.fallen";
        public const string LongSeason = "life.season";
        public const string SharpThorns = "life.sharp";
        public const string Sapling = "life.sapling";
        public const string CrowdedBed = "life.crowded";
        public const string SpreadingBriar = "life.spreading";
        public const string ReadySoil = "life.soil";
        public const string ReapWhatIsSown = "life.reap";
        public const string Shade = "life.shade";
        public const string ElderTree = "life.tree";
        public const string Wildwood = "life.wildwood";
        public const string Ripening = "life.ripen";
        public const string DeepSeed = "life.deepseed";
        public const string WideBurst = "life.wideburst";
        public const string BriarCoat = "life.coat";
        public const string Bark = "life.bark";
        public const string LongRegrowth = "life.longgrowth";
        public const string GreenWood = "life.greenwood";
        public const string BitterRind = "life.rind";
        public const string ThornRing = "life.ring";
        public const string SharedRoots = "life.roots";
        public const string Gift = "life.spring";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Life;

            to.Add(new Upgrade { Id = Root, Name = "Seeded", Element = E, Kind = UpgradeKind.Root, X = 0f, Y = 1.2f, Icon = UpgradeIcons.Leaf,
                Description = "Blows plant a seed in an enemy. Each blow on a Seeded enemy mends you a little. When it dies, it bursts into thorns.",
                WeaponStacks = 1 });

            // Left bough: Offering, health as the price.
            // NEAREST: missing mana is paid in health at half a point a point, left grey. The weapon skill is not included.
            to.Add(new Upgrade { Id = PaidInBlood, Name = "Paid in Blood", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = -1.58f, Y = 2f, Icon = UpgradeIcons.Gem,
                Description = "The mana a spell lacks is paid in health, one point for every two of mana. The health is left grey.", Stats = new[] { new StatMod(PlayerStat.BloodPrice, 0.5f) } });
            to.Add(new Upgrade { Id = DeepRoots, Name = "Deep Roots", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { PaidInBlood }, X = -0.51f, Y = 2.34f, Icon = UpgradeIcons.Heart,
                Description = "You have more health.",
                Stats = new[] { new StatMod(PlayerStat.Health, 12f) } });
            // AfflictionPower is what scales Seeded's mending; it scales the thorn burst as well.
            to.Add(new Upgrade { Id = SweetSap, Name = "Sweet Sap", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { PaidInBlood }, X = -1.31f, Y = 3.16f, Icon = UpgradeIcons.Plus,
                Description = "Seeded works 30% more strongly: blows on Seeded enemies mend more and their thorns burst harder.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionPower, 0.3f) } });
            // NEAREST: Seeded's mending is doubled through the strength of the affliction, which strengthens the thorn burst too.
            to.Add(new Upgrade { Id = ThinBlood, Name = "Thin Blood", Element = E, Kind = UpgradeKind.Major, Requires = new[] { PaidInBlood }, X = -2.07f, Y = 3.76f, Icon = UpgradeIcons.Fist,
                Description = "Below a third of your health, your blows strike a quarter harder, mend twice as much from the Seeded, and thorns burst twice as hard.", Stats = new[] { new StatMod(PlayerStat.WeaponDamage, 0.25f, StatWhen.LowHealth), new StatMod(PlayerStat.AfflictionPower, 1f, StatWhen.LowHealth) } });
            to.Add(new Upgrade { Id = Stubborn, Name = "Stubborn", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { ThinBlood }, X = -1.04f, Y = 4.34f, Icon = UpgradeIcons.Shield,
                Description = "Below a third of your health, wounds take less.",
                Stats = new[] { new StatMod(PlayerStat.DamageTaken, -0.15f, StatWhen.LowHealth) } });
            to.Add(new Upgrade { Id = Quickening, Name = "Quickening", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { ThinBlood }, X = -2.36f, Y = 4.79f, Icon = UpgradeIcons.Gem,
                Description = "Regrowth costs less mana.",
                Stats = new[] { new StatMod(PlayerStat.SurgeCost, -0.2f) } });
            // NEEDS FORM: a timed boon paid with a quarter of present health (every blow critical and mending; thorns at
            // the end if the price was won back). The clone is the nearest: half again on every blow for as long. A
            // spell must cost some mana to pass the test, so it costs 1.
            to.Add(new Upgrade { Id = RedHarvest, Name = "Red Harvest", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { ThinBlood }, X = -1.48f, Y = 5.43f, Icon = UpgradeIcons.Eye,
                Description = "You give 25 health and call a copy of yourself to your side for eight seconds. It lands each of your blows again a beat late, at half its strength.",
                Form = "clone",
                Spell = new SpellSpec { Cost = 0f, HealthCost = 25f, Seconds = 8f, Damage = 50f, Stacks = 1 } });
            // Built by hand (ManaPool.Instead, PlayerUpgrades.PayInHealth): every mana price is 1 health for 2 mana, left grey and never
            // the last point; every mana gain mends 1 health for 3 mana. The bar stays on the screen, empty.
            to.Add(new Upgrade { Id = Heartwood, Name = "Heartwood", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { RedHarvest }, X = -2.69f, Y = 5.94f, Icon = UpgradeIcons.Heart,
                Description = "You have no mana. Spells and skills are paid for in health, and everything that gave mana mends you instead.",
                Stats = new StatMod[0] });

            // Middle bough: Garden, things that grow where you fight.
            to.Add(new Upgrade { Id = FallenSeeds, Name = "Fallen Seeds", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = -0.18f, Y = 3.53f, Icon = UpgradeIcons.Skull,
                Description = "Where a Seeded enemy dies, a thornbush grows and stands for a while.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Seeded, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Grown = true, Limit = 4, Radius = 2f, Seconds = 8f, Interval = 0.5f, Damage = 4f, Affliction = AfflictionKind.Seeded, Stacks = 1 } } });
            to.Add(new Upgrade { Id = LongSeason, Name = "Long Season", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { FallenSeeds }, X = 0.15f, Y = 4.65f, Icon = UpgradeIcons.Hourglass,
                Description = "What you grow stands longer.",
                Stats = new[] { new StatMod(PlayerStat.GrownSeconds, 0.4f) } });
            to.Add(new Upgrade { Id = SharpThorns, Name = "Sharp Thorns", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { FallenSeeds }, X = 0.97f, Y = 3.86f, Icon = UpgradeIcons.Spike,
                Description = "What you grow strikes harder.",
                Stats = new[] { new StatMod(PlayerStat.GrownDamage, 0.25f) } });
            // NEAREST: a sapling bites everything within its reach each beat, not the nearest enemy alone.
            to.Add(new Upgrade { Id = Sapling, Name = "Sapling", Element = E, Kind = UpgradeKind.Major, Requires = new[] { FallenSeeds }, X = -0.4f, Y = 6.14f, Icon = UpgradeIcons.Spike,
                Description = "A perfect guard plants a sapling at your feet, up to three. Each lashes every enemy within its reach and seeds it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Patch, Grown = true, Limit = 3, Radius = 3.5f, Seconds = 12f, Interval = 1.5f, Damage = 6f, Stagger = 8f, Affliction = AfflictionKind.Seeded, Stacks = 1 } } });
            to.Add(new Upgrade { Id = CrowdedBed, Name = "Crowded Bed", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Sapling }, X = -1.17f, Y = 7.06f, Icon = UpgradeIcons.Plus,
                Description = "You may keep one more sapling.",
                Stats = new[] { new StatMod(PlayerStat.GrownLimit, 1f) } });
            to.Add(new Upgrade { Id = SpreadingBriar, Name = "Spreading Briar", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Sapling }, X = -0.26f, Y = 7.32f, Icon = UpgradeIcons.Arrow,
                Description = "Briar Patch covers more ground.",
                Stats = new[] { new StatMod(PlayerStat.SpellReach, 0.15f) } });
            to.Add(new Upgrade { Id = ReadySoil, Name = "Ready Soil", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Sapling }, X = 0.68f, Y = 6.63f, Icon = UpgradeIcons.Gem,
                Description = "Briar Patch waits a fifth less between casts.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellCooldown, -0.2f) } });
            // NEAREST: the nova is the call; everything grown bursts and mends. The seeds in enemies do not burst yet.
            to.Add(new Upgrade { Id = ReapWhatIsSown, Name = "Reap What Is Sown", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Sapling }, X = 0.82f, Y = 7.78f, Icon = UpgradeIcons.Burst,
                Description = "You call in the harvest. The ground bursts round you and everything you have grown bursts with it, each grown thing mending you.",
                Form = "nova",
                Spell = new SpellSpec { Cost = 30f, Radius = 6f, Delay = 0.2f, Damage = 16f, Stagger = 25f, Stacks = 1 }, Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnSpell = true, Do = UpgradeAction.BurstGrown, Damage = 16f, Stagger = 25f, Radius = 3f, Amount = 5f } } });
            to.Add(new Upgrade { Id = Shade, Name = "Shade", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Sapling }, X = -2.21f, Y = 7.16f, Icon = UpgradeIcons.Leaf,
                Description = "Standing near anything you have grown, you mend slowly and your stamina returns faster.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 1f, While = StatWhen.NearGrown, Do = UpgradeAction.Heal, Amount = 2f }, new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 1f, While = StatWhen.NearGrown, Do = UpgradeAction.Stamina, Amount = 5f } } });
            // The rod is the planted totem. It does not yet grow in three shapes, take blows while it grows, or mend the player beneath it.
            to.Add(new Upgrade { Id = ElderTree, Name = "Elder Tree", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Shade }, X = -2.14f, Y = 8.65f, Icon = UpgradeIcons.Tower,
                Description = "You plant a tree a little ahead. For fifteen seconds its roots strike everything in a circle round it, every three seconds. 40 mana.",
                Form = SpellForms.Rod,
                Spell = new SpellSpec { Cost = 45f, Reach = 2.5f, Radius = 3f, Seconds = 15f, Interval = 3f, Damage = 16f, Stagger = 30f, Delay = 3f, Stacks = 1 } });
            // NEAREST: grown things stand three times as long (not for ever) and three more of each may stand (not twice as many). Far from them blows are weaker, as designed.
            to.Add(new Upgrade { Id = Wildwood, Name = "Wildwood", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { ElderTree }, X = -0.98f, Y = 9.65f, Icon = UpgradeIcons.Leaf,
                Description = "What you grow stands three times as long, and you may keep three more of each kind. Far from your garden, your blows are a fifth weaker.",
                Stats = new[] { new StatMod(PlayerStat.GrownSeconds, 2f), new StatMod(PlayerStat.GrownLimit, 3f), new StatMod(PlayerStat.WeaponDamage, -0.2f, StatWhen.FarFromGrown) }, });

            // Right bough: Bramble, the seed and the thorn.
            // NEAREST: Seeded goes three deep, not five, and a heavy blow bursts a seed of any depth.
            to.Add(new Upgrade { Id = Ripening, Name = "Ripening", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = 1.54f, Y = 2.45f, Icon = UpgradeIcons.Burst,
                Description = "A heavy blow on a Seeded enemy bursts its seed at once, without waiting for death, and mends you.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.SetOff, Needs = AfflictionKind.Seeded, Affliction = AfflictionKind.Seeded, Damage = 11f, Stagger = 5f, Radius = 3f, Cooldown = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Heal, Needs = AfflictionKind.Seeded, Amount = 10f, Cooldown = 1f },
                } });
            to.Add(new Upgrade { Id = DeepSeed, Name = "Deep Seed", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Ripening }, X = 2.02f, Y = 3.56f, Icon = UpgradeIcons.Hourglass,
                Description = "Seeded lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, 0.5f) } });
            to.Add(new Upgrade { Id = WideBurst, Name = "Wide Burst", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Ripening }, X = 0.52f, Y = 2.79f, Icon = UpgradeIcons.Ring,
                Description = "Thorns burst wider.",
                Stats = new[] { new StatMod(PlayerStat.BurstReach, 0.33f) } });
            // NEAREST: nothing tells a blow by hand from a thrown one, so any attacker is cut.
            to.Add(new Upgrade { Id = BriarCoat, Name = "Briar Coat", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Ripening }, X = 1.59f, Y = 4.73f, Icon = UpgradeIcons.Claw,
                Description = "An enemy that strikes you, or whose blow you guard, is cut by thorns and Seeded.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Struck, Do = UpgradeAction.Burst, Damage = 8f, Radius = 1.2f, Affliction = AfflictionKind.Seeded, Stacks = 1, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Burst, Damage = 8f, Radius = 1.2f, Affliction = AfflictionKind.Seeded, Stacks = 1, Cooldown = 0.5f },
                } });
            to.Add(new Upgrade { Id = Bark, Name = "Bark", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { BriarCoat }, X = 2.46f, Y = 5.1f, Icon = UpgradeIcons.Shield,
                Description = "Wounds take less while Regrowth lasts.",
                Stats = new[] { new StatMod(PlayerStat.DamageTaken, -0.15f, StatWhen.Surge) } });
            to.Add(new Upgrade { Id = LongRegrowth, Name = "Long Regrowth", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { BriarCoat }, X = 0.63f, Y = 5.52f, Icon = UpgradeIcons.Hourglass,
                Description = "Regrowth lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.SurgeSeconds, 0.5f) } });
            to.Add(new Upgrade { Id = GreenWood, Name = "Green Wood", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { BriarCoat }, X = 1.62f, Y = 5.95f, Icon = UpgradeIcons.Heart,
                Description = "Grey health waits longer before it fades.",
                Stats = new[] { new StatMod(PlayerStat.GreyKept, 0.6f) } });
            to.Add(new Upgrade { Id = BitterRind, Name = "Bitter Rind", Element = E, Kind = UpgradeKind.Major, Requires = new[] { BriarCoat }, X = 2.73f, Y = 6.24f, Icon = UpgradeIcons.Shield,
                Description = "A plain guard seeds the attacker. A perfect guard roots it where it stands for a moment.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Hold, Seconds = 1f } } });
            to.Add(new Upgrade { Id = ThornRing, Name = "Thorn Ring", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { BitterRind }, X = 3.03f, Y = 7.66f, Icon = UpgradeIcons.Ring,
                Description = "You raise a ring of thorns round you and the enemy faced. Nothing walks through it until it falls, you included, and enemies that touch it are cut.",
                Form = "ring",
                Spell = new SpellSpec { Cost = 40f, Reach = 5f, Radius = 4.5f, Seconds = 7f, LingerDamage = 8f, Stacks = 1 } });
            // BESPOKE: a third of each wound is taken off the player and split among Seeded enemies within 8 m, and Seeded
            // no longer mends. Until then: wounds take less, each one cuts everyone near, and Seeded works half as well.
            to.Add(new Upgrade { Id = SharedRoots, Name = "Shared Roots", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { ThornRing }, X = 2.07f, Y = 8.85f, Icon = UpgradeIcons.Chain,
                Description = "Wounds take a fifth less from you, and each blow that lands on you cuts every enemy near. Seeded mends you half as much.",
                Stats = new[]
                {
                    new StatMod(PlayerStat.DamageTaken, -0.2f),
                    new StatMod(PlayerStat.AfflictionPower, -0.5f),
                },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Struck, Do = UpgradeAction.Burst, At = EffectAt.Self, Damage = 6f, Radius = 8f, Cooldown = 0.5f } } });

            // BESPOKE (built): SpringForm. The surge button is pressed again, not held.
            to.Add(new Upgrade { Id = Gift, Name = "First Spring", Element = E, Kind = UpgradeKind.Signature, X = 0f, Y = 12f, Icon = UpgradeIcons.Crown,
                Description = "While Regrowth lasts, press the surge button again and give half your health to the ground. A forest comes up through everything Seeded and holds it. Strike what it holds and the health returns.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.Spring, Spell = new SpellSpec { Cost = 0f, Reach = 12f, Seconds = 4f, Damage = 30f, Stagger = 60f, Count = 50, Stacks = 0 } } } });
        }
    }
}
