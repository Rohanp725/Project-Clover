using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Metal and Lightning (pairs_d5.md, "Metal and Lightning", distance 5; task 4.17), named by its
    /// reaction, Live Iron. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its
    /// Shape mark, Open Plates, opens Drawn Iron and Forged Bolt; Three Smiths (a major), Bare Wire (a minor), the keystone
    /// Iron Calls the Storm, and the duo spell Lightning Seeks Iron. Metal is written first, as the ring names the pair.
    /// </summary>
    public static class LiveIronDuo
    {
        public const string Shape = "liveiron.shape";
        public const string ThreeSmiths = "liveiron.threesmiths";
        public const string BareWire = "liveiron.barewire";
        public const string IronCallsTheStorm = "liveiron.callsstorm";
        public const string LightningSeeksIron = "liveiron.seeksiron";
        /// <summary>The conditions the two shapes set: Drawn Iron's live plates (the arcs as the plain reaction's), and
        /// Forged Bolt's spikes, waiting for the heavy blow.</summary>
        public const string Drawn = "liveiron.drawn";
        public const string Forged = "liveiron.forged";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Metal, B = Elements.Lightning;

            // NEEDS: afflicted-only (Drawn Iron drawing only the enemies its arcs reach; now every enemy within 8 m, once in 0.3
            // s); planted-blades (Forged Bolt's spikes seen standing in it, one hammered in by each weapon blow; now your heavy
            // blow tears out up to three of its Static as the bolt).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Live Iron: Drawn Iron or Forged Bolt", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Live Iron's two shapes, chosen at a rest fire. Drawn Iron: the arcs magnetise, folding the crowd in onto your blade. Forged Bolt: no arcs; your blows hammer its charge into spikes that a heavy blow tears out as one thunderbolt. Either runs instead of the plain Live Iron.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Drawn Iron",
                        Description = "For 6 s every weapon blow on it arcs from iron to iron as before, and the arcs magnetise: each enemy an arc reaches is drawn 1.5 m toward the one struck, plates clattering, so a crowd folds in onto the blade.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Drawn, Seconds = 6f, Is = Reactions.IronLive } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Drawn, By = StruckBy.Weapon, Cooldown = 0.3f, Do = UpgradeAction.Arc,
                                LeapsTo = AfflictionKind.Sundered, Count = 2, Reach = 8f, Amount = 0.35f, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Drawn, By = StruckBy.Weapon, Cooldown = 0.3f, Do = UpgradeAction.Pull, Radius = 8f, Amount = 1.5f },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Forged Bolt",
                        Description = "No arcs. For 6 s its Static holds, and each weapon blow on it hammers one into a glowing iron spike standing in it, three at most; your heavy blow tears them out as one thunderbolt thrown 10 m on: 12 a spike, 20 stagger and two Static to all on the line.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Forged, Seconds = 6f, Keeps = AfflictionKind.Static, EndsWith = ConditionBlow.Heavy, Is = Reactions.IronLive } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Forged, By = StruckBy.Heavy, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Static, Count = 3 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Forged, By = StruckBy.Heavy, Do = UpgradeAction.Lane, Reach = 10f, Radius = 0.6f, DamagePerSpent = 12f, Stagger = 20f,
                                Affliction = AfflictionKind.Static, Stacks = 2 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // Every third weapon blow on each enemy, counted on it, calls the bolt, at most every 0.3 s: blows on an enemy whose
            // iron is live from Live Iron, in whichever shape (UpgradeEffect.InState).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = ThreeSmiths, Name = "Three Smiths", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Hammer,
                Description = "Every third weapon blow on a Live Iron enemy is answered from the sky: a bolt for 12, 15 stagger and a Static.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, InState = Reactions.IronLive, EveryOnEnemy = 3, Do = UpgradeAction.Falling, Radius = 1f, Delay = 0.3f,
                        Damage = 12f, Stagger = 15f, Affliction = AfflictionKind.Static, Stacks = 1, Cooldown = 0.3f },
                },
            }, HeartPattern.LeftLow));

            // Changes its reaction's own numbers while it is in force (reaction-numbers, ReactionNumbers):
            // the arcs carrying 45% of the blow, not 35%.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BareWire, Name = "Bare Wire", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { Shape }, Icon = UpgradeIcons.Hourglass,
                Description = "Live Iron's arcs carry 45% of the blow.",
                ReactionChanges = new[]
                {
                    new ReactionChange { Shape = -1, Index = 1, Amount = 0.45f },
                    new ReactionChange { Shape = 0, Index = 1, Amount = 0.45f },
                },
            }, HeartPattern.RightLow));

            // NEEDS: thrown blades that turn in flight to the nearest enemy carrying Static (aim-at-afflicted); now any spell of
            // yours that strikes a charged enemy lays two more Static, at most every 0.3 s.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = IronCallsTheStorm, Name = "Iron Calls the Storm", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { ThreeSmiths, BareWire }, Icon = UpgradeIcons.Bolt,
                Description = "Every blade you throw (Flung Blade and the rest) turns in flight to the nearest enemy carrying Static and lands with two more.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, Needs = AfflictionKind.Static, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 2, Cooldown = 0.3f } },
            }, HeartPattern.Keystone));

            // The duo spell, a movement on a 10 s cooldown (SpellTimings), bought with 5 Shards once both are carried. The drill
            // form makes you the bolt: in 0.25 s, along the line to the enemy faced, it cuts every enemy on it and notches it
            // (the branch's first element), and each takes two Static.
            // NEEDS: going to the farthest Sundered enemy within 10 m, the enemy faced at half with none (aim-at-afflicted;
            // now the enemy faced, whole); a perfect guard in the 2 s before the cast sending it on to a second (spell-windows).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = LightningSeeksIron, Name = "Lightning Seeks Iron", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Movement, Requires = new[] { ThreeSmiths, BareWire }, Icon = UpgradeIcons.Bolt,
                Description = "You become a bolt and in a quarter of a second, untouchable, go to the farthest notched enemy within 10 m, cutting every enemy on the line for 10, 15 stagger and two Static; arriving, 16 and two Static.",
                Form = SpellForms.Drill,
                Spell = new SpellSpec { Reach = 10f, Radius = 1.2f, Count = 1, Damage = 10f, Stagger = 15f, Stacks = 1, Speed = 40f },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 2 } },
            }, HeartPattern.Spell));
        }
    }
}
