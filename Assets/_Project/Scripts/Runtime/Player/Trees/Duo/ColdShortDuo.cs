using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Ice and Metal (pairs_d4.md, "Ice and Metal", distance 4; task 4.17), named by its reaction, Cold
    /// Short. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark, Plate
    /// or Snap, opens Shed Plate and Snapped Blade; Rimed Edge and Glass Bell (majors), the keystone Brittle to the Core, and
    /// the duo spell Cold Chisel, bought with Shards once both are carried. Ice is written first, as the ring names the pair.
    /// </summary>
    public static class ColdShortDuo
    {
        public const string Shape = "coldshort.shape";
        public const string RimedEdge = "coldshort.rimededge";
        public const string GlassBell = "coldshort.glassbell";
        public const string BrittleToTheCore = "coldshort.core";
        public const string ColdChisel = "coldshort.chisel";
        /// <summary>The conditions the two shapes set (Shed Plate's brittleness as the plain reaction's, its cracked plates, the
        /// black ice's slip; Snapped Blade's brittle weapon), and Glass Bell's own brittle time.</summary>
        public const string PlateBrittle = "coldshort.plate";
        public const string PlateCracked = "coldshort.platecracked";
        public const string PlateBare = "coldshort.platebare";
        public const string Slip = "coldshort.slip";
        public const string Snapped = "coldshort.snapped";
        public const string Ringing = "coldshort.ringing";
        private const string Reaction = "react.coldshort";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Ice, B = Elements.Metal;

            // NEEDS: ground that acts on whoever walks in later (ground-states): Shed Plate's black ice throwing flat whoever
            // steps on it in its 4 s; now those within 1.2 m as the plate falls slip. A plain guard against a brittle enemy
            // counted as perfect, breaking its weapon for 50 stagger and a 1 s hold (guard-made-perfect); now Snapped Blade's
            // brittle weapon deals half for the 5 s from the reaction.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Cold Short: Shed Plate or Snapped Blade", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Cold Short's two shapes, chosen at a rest fire. Shed Plate: the cracked plate falls as black ice that throws down those who step on it. Snapped Blade: the armour holds but its own weapon goes brittle and snaps. Either runs instead of the plain Cold Short.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Shed Plate",
                        Description = "It goes brittle and your next heavy blow cracks the plates off, as before; and the cracked plate falls as a sheet of black ice 2 m across under it for 4 s. Enemies on it slip flat for 0.6 s.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = PlateBrittle, Seconds = 5f, Keeps = AfflictionKind.Sundered, EndsWith = ConditionBlow.Heavy, Is = Reactions.Brittle } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = PlateBrittle, By = StruckBy.Heavy, Do = UpgradeAction.Condition,
                                State = new ConditionSpec { Id = PlateCracked, Seconds = 6f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.25f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = PlateBrittle, By = StruckBy.Heavy, Do = UpgradeAction.Condition,
                                State = new ConditionSpec { Id = PlateBare, Seconds = 120f, Keeps = AfflictionKind.Sundered } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = PlateBrittle, By = StruckBy.Heavy, Do = UpgradeAction.Hazard, Radius = 1f, Seconds = 4f, Interval = 1f,
                                Affliction = AfflictionKind.Chill },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = PlateBrittle, By = StruckBy.Heavy, Do = UpgradeAction.Condition, Radius = 1.2f,
                                State = new ConditionSpec { Id = Slip, Seconds = 0.6f, Flags = ConditionFlags.Down } },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Snapped Blade",
                        Description = "The armour does not crack under your blow; its own weapon goes brittle instead. For 5 s its notches do not fade and its blows deal half, as a weapon that snaps on your guard.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition,
                                State = new ConditionSpec { Id = Snapped, Seconds = 5f, Keeps = AfflictionKind.Sundered, Flags = ConditionFlags.DamageDealtShare, Scale = 0.5f, Is = Reactions.Brittle } },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = RimedEdge, Name = "Rimed Edge", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Snowflake,
                Description = "Your heavy blows lay a Chill and a notch together.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // Each weapon blow on an enemy brittle from Cold Short, in whichever shape (UpgradeEffect.InState); the heavy blow
            // that cracks the brittleness still rings.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = GlassBell, Name = "Glass Bell", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Ring,
                Description = "Each blow on a brittle enemy rings like glass: 3 mana, and 0.3 s off every running spell cooldown.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, InState = Reactions.Brittle, Cooldown = 0.1f, Do = UpgradeAction.Mana, Amount = 3f },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, InState = Reactions.Brittle, Cooldown = 0.1f, Do = UpgradeAction.CutCooldowns, Amount = 0.3f },
                },
            }, HeartPattern.RightLow));

            // Cold Short on a frozen enemy shatters it as a heavy blow would (UpgradeAction.Shatter): the ice breaks, what it
            // held takes the breaking and 40 stagger, and whatever answers a shatter hears it.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BrittleToTheCore, Name = "Brittle to the Core", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { RimedEdge, GlassBell }, Icon = UpgradeIcons.Gem,
                Description = "Cold Short on a frozen enemy shatters it as well, the shatter and the crack landing together.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Shatter, Stagger = 40f, Affliction = AfflictionKind.Chill },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with 5 Shards once both are carried (no god gives it): a detonator on a 10 s cooldown
            // (SpellTimings). The brand form sets the chisel in the enemy faced, cast in Ice (a Chill each tap); each of the next
            // three blows taps it in, laying a notch, and the tap that finds three notches drives it home: 36 and 70 stagger, and
            // Cold Short at once.
            // NEEDS: only light blows tapping and the first heavy driving it, 24 and 12 more a notch (count-rules); now any three
            // blows tap, the third's burst driving it when three notches lie.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = ColdChisel, Name = "Cold Chisel", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Detonator, Requires = new[] { RimedEdge, GlassBell }, Icon = UpgradeIcons.Spike,
                Description = "A chisel of black ice sets point-first in the enemy you face, within 8 m. Each of your next blows on it taps it in, a notch and a Chill with a higher ringing note; the tap that finds three notches drives it home and splits the armour, 36 and 70 stagger, and Cold Short at once.",
                Form = SpellForms.Brand,
                Spell = new SpellSpec { Reach = 8f, Seconds = 6f, Count = 3, Radius = 1f, Damage = 6f, Stagger = 8f, Stacks = 1 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Needs = AfflictionKind.Sundered, NeedsAtLeast = 3, Cooldown = 1f, Do = UpgradeAction.Burst, Radius = 0.5f,
                        Damage = 36f, Stagger = 70f, Affliction = AfflictionKind.Sundered },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Needs = AfflictionKind.Sundered, NeedsAtLeast = 3, Cooldown = 1f, Do = UpgradeAction.ForceReaction, Reaction = Reaction },
                },
            }, HeartPattern.Spell));
        }
    }
}
