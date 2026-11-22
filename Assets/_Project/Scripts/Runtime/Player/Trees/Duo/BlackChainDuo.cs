using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Metal and Dark (pairs_d1_d2.md, "Metal and Dark", distance 2; task 4.15), named by its reaction,
    /// Black Chain. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark,
    /// Hidden Chain, opens Drawn Tight and Shared Wound; Dead Weight and Rust in the Curse, the keystone One Noose for Two,
    /// and the duo spell The Smith's Net. Metal is written first: the duo spell is cast in Metal.
    /// </summary>
    public static class BlackChainDuo
    {
        public const string Shape = "blackchain.shape";
        public const string DeadWeight = "blackchain.deadweight";
        public const string RustInTheCurse = "blackchain.rust";
        public const string OneNooseForTwo = "blackchain.noose";
        public const string TheSmithsNet = "blackchain.net";
        /// <summary>The condition Shared Wound sets on the nearest other enemy, joined to the one that reacted.</summary>
        public const string SharedWound = "blackchain.shared";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Metal, B = Elements.Dark;

            // Shared Wound's chain passes half of every blow, weapon or spell (ConditionFlags.Chained, SharesSpells), with no leash.
            // NEEDS: the two dragged together and colliding (Drawn Tight; now the snap strikes both where they stand); a curse
            // laid on one running along the chain (Shared Wound).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Black Chain: Drawn Tight or Shared Wound", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Black Chain's two shapes, chosen at a rest fire. Drawn Tight: the chain snaps taut at once and the two are dragged together. Shared Wound: no leash, but half of every blow is shared. Either runs instead of the plain Black Chain.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Drawn Tight",
                        Description = "The chain snaps taut at once: it and the nearest other enemy within 6 m are dragged together and collide, 12 and 25 stagger each, and the chain falls away.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 0.6f, Damage = 12f, Stagger = 25f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Arc, FromTarget = true, Count = 1, Reach = 6f, Damage = 12f, Stagger = 25f, Affliction = AfflictionKind.Sundered, Stacks = 0 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Shared Wound",
                        Description = "No leash, but the chain to the nearest other enemy within 6 m holds for 8 s, and half of every blow on either is shared with the other.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, Radius = 6f, Count = 1,
                                State = new ConditionSpec { Id = SharedWound, Seconds = 8f, Scale = 0.5f, ToOthers = true, Flags = ConditionFlags.Chained, SharesSpells = true } },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // Its chain-mate (ToChainMate: EnemyChains.MateOf, found as it dies) is dragged to where it fell, held 1 s and Cursed.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DeadWeight, Name = "Dead Weight", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Chain,
                Description = "When one of a chained pair dies, the chain drags the other to where it fell, holds it 1 s and Curses it again.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, ToChainMate = true, Do = UpgradeAction.Pull, Cooldown = 0.2f },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, ToChainMate = true, Do = UpgradeAction.Hold, Seconds = 1f, Cooldown = 0.2f },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, ToChainMate = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Cursed, Stacks = 1, Cooldown = 0.2f },
                },
            }, HeartPattern.LeftLow));

            // Notches kept from fading while the enemy is Cursed (AfflictionRules.KeepWhile).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = RustInTheCurse, Name = "Rust in the Curse", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Skull,
                Description = "Notches on a Cursed enemy do not fade while the curse lasts.",
                Rules = book => book.Afflictions.KeepWhile(AfflictionKind.Sundered, AfflictionKind.Cursed),
            }, HeartPattern.RightLow));

            // The finisher lands on its chain-mate (ToChainMate) at half the blow's damage (ByPower, Share 0.5) and half its stagger
            // (UpgradeEffect.StaggerOfBlow: the weapon's last blow's stagger as it was dealt).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = OneNooseForTwo, Name = "One Noose for Two", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { DeadWeight, RustInTheCurse }, Icon = UpgradeIcons.Ring,
                Description = "A finisher on a chained enemy lands on its chain-mate too, at half its damage and stagger.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.FinisherHit, ToChainMate = true, Do = UpgradeAction.Burst, Radius = 0.5f, ByPower = true, Share = 0.5f, StaggerOfBlow = 0.5f } },
            }, HeartPattern.Keystone));

            // The duo spell, bought with Shards once both are carried (no god gives it): an opener on a 16 s cooldown (SpellTimings). The bomb form drops a weight on the aim
            // point; what is under it is notched (the branch's first element), Cursed and held, so Black Chain binds them.
            // NEEDS: a net that chains everything under it to everything else under it for 6 s, none leaving the circle for 3 s,
            // and a quarter of every weapon blow on one landing on all the others (linked enemies).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheSmithsNet, Name = "The Smith's Net", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { DeadWeight, RustInTheCurse }, Icon = UpgradeIcons.Hammer,
                Description = "You throw a net of fine black chain up to 7 m; it opens and falls a moment later, seen only as a shimmer and its shadow growing on the floor. Everything under it is Cursed, notched and held.",
                Form = SpellForms.Bomb,
                Spell = new SpellSpec { Reach = 7f, Radius = 2f, Delay = 0.4f, Damage = 6f, Stagger = 10f, Stacks = 1 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Cursed, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Hold, Seconds = 1.5f },
                },
            }, HeartPattern.Spell));
        }
    }
}
