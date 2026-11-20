using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Light and Lightning (pairs_d4.md, "Light and Lightning", distance 4; task 4.17), named by its
    /// reaction, The Bolt Runs True. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried.
    /// Its Shape mark, Flat or Again, opens Laid Flat and Struck Again; Charged Light and Diamond Guard (majors), the keystone
    /// The Diamond Falls, the duo spell Diamond Thunder and the duo surge The Net of Jewels, each bought with Shards once both
    /// are carried. Light is written first, as the ring names the pair.
    /// </summary>
    public static class TheBoltRunsTrueDuo
    {
        public const string Shape = "boltrunstrue.shape";
        public const string ChargedLight = "boltrunstrue.chargedlight";
        public const string DiamondGuard = "boltrunstrue.diamondguard";
        public const string TheDiamondFalls = "boltrunstrue.diamondfalls";
        public const string DiamondThunder = "boltrunstrue.diamondthunder";
        public const string TheNetOfJewels = "boltrunstrue.net";
        /// <summary>The conditions the two shapes set (Laid Flat's thread along the ground, Struck Again's standing thread),
        /// and the opening Diamond Thunder leaves on what it strikes.</summary>
        public const string Flat = "boltrunstrue.flat";
        public const string Again = "boltrunstrue.again";
        public const string Opened = "boltrunstrue.opened";
        private const string Reaction = "react.theboltrunstrue";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Light, B = Elements.Lightning;

            // NEEDS: a line laid from one enemy to the nearest carrying a given affliction, all on it struck (aim-at-afflicted):
            // Laid Flat's bolt along the ground to the nearest Marked or Static enemy within 10 m; now an arc to the nearest
            // Marked one.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "The Bolt Runs True: Laid Flat or Struck Again", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens The Bolt Runs True's two shapes, chosen at a rest fire. Laid Flat: the thread lies along the ground to the next marked enemy and the bolt runs it. Struck Again: no great bolt; the thread stays, and each critical blow calls a smaller one down it. Either runs instead of the plain The Bolt Runs True.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Laid Flat",
                        Description = "The thread of light lies along the ground to the nearest other Marked enemy within 10 m, and 0.5 s later the bolt runs the line: 18 and two Static to it and to the one at the far end. Then its own Static discharges, as before.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Flat, Seconds = 0.5f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Flat, Do = UpgradeAction.Burst, Radius = 0.6f, Damage = 18f, Stagger = 20f, Affliction = AfflictionKind.Static, Stacks = 2 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Flat, Do = UpgradeAction.Arc, FromTarget = true, LeapsTo = AfflictionKind.Marked, Count = 1, Reach = 10f,
                                Damage = 18f, Stagger = 20f, Affliction = AfflictionKind.Static, Stacks = 2 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Flat, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Stagger = 5f },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Struck Again",
                        Description = "No great bolt. The thread stays on it while its Mark lasts, and each critical blow of your weapon on it calls a smaller bolt down the thread: 9, 15 stagger and a Static.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Again, Seconds = 30f, While = AfflictionKind.Marked } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Again, By = StruckBy.Weapon, Do = UpgradeAction.Falling, Delay = 0.1f, Radius = 0.8f, Damage = 9f, Stagger = 15f,
                                Affliction = AfflictionKind.Static, Stacks = 1 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = ChargedLight, Name = "Charged Light", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Sun,
                Description = "Your light blows lay Static on a Marked enemy, and a discharge Marks every enemy it reaches.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.LightHit, Needs = AfflictionKind.Marked, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = AfflictionKind.Static, Cooldown = 0.3f, Do = UpgradeAction.Afflict, Radius = 4f, Affliction = AfflictionKind.Marked, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DiamondGuard, Name = "Diamond Guard", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard against a Marked enemy calls the bolt onto it at once, whatever it carries. Once every 5 s.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Marked, Cooldown = 5f, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Marked, Cooldown = 5f, Do = UpgradeAction.ForceReaction, Reaction = Reaction },
                },
            }, HeartPattern.RightLow));

            // NEEDS: the third critical blow on one enemy counted (count-rules); now one critical blow in three on a Marked
            // enemy, by chance, charges it and calls the bolt, at most once in 5 s.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheDiamondFalls, Name = "The Diamond Falls", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { ChargedLight, DiamondGuard }, Icon = UpgradeIcons.Gem,
                Description = "The Bolt Runs True no longer needs Static: the third critical blow on a Marked enemy calls it, once every 5 s.",
                Effects = new[]
                {
                    // The Static laid on the Marked enemy sets the bolt off, through the pair's own rest.
                    new UpgradeEffect { On = UpgradeTrigger.CritHit, Needs = AfflictionKind.Marked, Chance = 0.34f, Cooldown = 5f, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1 },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with 5 Shards once both are carried (no god gives it): a detonator on an 11 s cooldown
            // (SpellTimings). The spikes form runs one stroke 14 m along the floor, cast in Light (a Mark on each); every Static
            // it meets discharges, and your first blow on each enemy struck within 2 s is critical.
            // NEEDS: the line held on the button for up to 1.5 s, turning with you, loosed on letting go (held-press), and a
            // perfect guard while it is held loosing it at once for twice the damage (spell-windows).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DiamondThunder, Name = "Diamond Thunder", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Detonator, Requires = new[] { ChargedLight, DiamondGuard }, Icon = UpgradeIcons.Beam,
                Description = "A straight line of light lies on the floor from your hand, 14 m, and lightning runs the whole line in one stroke: 22 and 40 stagger to every enemy on it, each Marked, and every Static it meets discharges. Your first blow on each within 2 s is critical.",
                Form = SpellForms.Spikes,
                Spell = new SpellSpec { Reach = 14f, Speed = 40f, Count = 1, Radius = 0.7f, Damage = 22f, Stagger = 40f, Stacks = 1 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Stagger = 5f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Condition,
                        State = new ConditionSpec { Id = Opened, Seconds = 2f, Flags = ConditionFlags.TakesCritical, EndsWith = ConditionBlow.Weapon } },
                },
            }, HeartPattern.Spell));

            // The duo surge. NEEDS: its own 45 mana and 10 s in the Hand's surge slot (duo-surge-slot); threads linking every
            // Marked enemy within 12 m (six at most) that carry a discharge or a bolt on one to all the others for half
            // (links-pass-blows); now a discharge on a Marked enemy arcs on through up to five Marked enemies within 12 m,
            // half a discharge's 7 a stack each, and each critical blow on a Marked enemy lays a Static on every enemy within
            // 12 m, Marked or not (afflicted-only).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheNetOfJewels, Name = "The Net of Jewels", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { DiamondThunder }, Icon = UpgradeIcons.Chain,
                Description = "A duo surge. While your surge lasts every Marked enemy within 12 m is linked to every other by threads of light: a discharge on one runs the threads to all the others for half, and each critical blow on one gives every linked enemy a Static.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SetOff, WhileSurge = true, Spent = AfflictionKind.Static, Needs = AfflictionKind.Marked, Cooldown = 0.5f, Do = UpgradeAction.Arc, FromTarget = true,
                        LeapsTo = AfflictionKind.Marked, Count = 5, Reach = 12f, Damage = 3.5f, Stagger = 2.5f, PerStack = true, Affliction = AfflictionKind.Static },
                    new UpgradeEffect { On = UpgradeTrigger.CritHit, WhileSurge = true, Needs = AfflictionKind.Marked, Cooldown = 0.3f, Do = UpgradeAction.Afflict, Radius = 12f, Affliction = AfflictionKind.Static, Stacks = 1 },
                },
            }, HeartPattern.RightTop));
        }
    }
}
