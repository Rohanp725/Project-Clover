using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Glacier: the branch Ice and Water share. Water that has stopped: a Soaked enemy freezes sooner, guarding builds
    /// ice on the player, and the great spell is a wave caught as it breaks. Its crown is given by consuming the
    /// neighbour's god.
    /// </summary>
    public static class GlacierTree
    {
        public const string ColdWater = "glacier.coldwater";
        public const string FlashFreeze = "glacier.flash";
        public const string Icebreaker = "glacier.breaker";
        public const string ColdInTheBones = "glacier.bones";
        public const string CreepingIce = "glacier.creeping";
        public const string TheWaveStandsStill = "glacier.wave";
        public const string Gift = "glacier.weight";

        public static void Add(List<Upgrade> to)
        {
            // The first element is the one whose mix this is: the earlier of the two round the ring.
            const string A = Elements.Ice, B = Elements.Water;

            // Both afflictions on every blow, the second element's too (the owner's of 2026-11-19); with the first element's root learned, the root's stack is that element's share and is not laid twice.
            to.Add(new Upgrade { Id = ColdWater, Name = "Cold Water", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, X = 0f, Y = 1f, Icon = UpgradeIcons.Drop,
                Description = "Your blows lay Chill and Soaked together.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                } });
            // Built (Afflictions.FlashFreezeAt): a Soaked enemy freezes at three Chill, and the freeze spends the Soaked.
            to.Add(new Upgrade { Id = FlashFreeze, Name = "Flash Freeze", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { ColdWater }, X = -0.5f, Y = 2.36f, Icon = UpgradeIcons.Snowflake,
                Description = "A Soaked enemy freezes at three Chill, not five.",
                Stats = new StatMod[0] });
            // The spikes form gives the line of ice; holding enemies by the feet is not in it yet.
            to.Add(new Upgrade { Id = CreepingIce, Name = "Creeping Ice", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { ColdWater }, X = 0.5f, Y = 2.36f, Icon = UpgradeIcons.Spike,
                Description = "Hold the spell button to send a line of ice spikes eight metres along the ground. Each enemy on the line is struck once. 35 mana.",
                Form = "spikes",
                Spell = new SpellSpec { Cost = 35f, Reach = 8f, Speed = 20f, Count = 1, Radius = 0.6f, Damage = 8f, Stagger = 10f, Stacks = 2 } });
            to.Add(new Upgrade { Id = Icebreaker, Name = "Icebreaker", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { FlashFreeze }, X = -0.5f, Y = 4.6f, Icon = UpgradeIcons.Hammer,
                Description = "Shattering an enemy gives mana and fills a little of an empty gourd.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.Mana, Amount = 12f }, new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.FlaskFill, Amount = 0.33f } } });
            // NEAREST: an enemy's wind-up cannot be changed, so a blow on a Soaked enemy lengthens the player's own moment to guard for a while.
            to.Add(new Upgrade { Id = ColdInTheBones, Name = "Cold in the Bones", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { FlashFreeze }, X = 0f, Y = 3.48f, Icon = UpgradeIcons.Hourglass,
                Description = "For four seconds after you strike a Soaked enemy, the moment for a perfect guard is a quarter longer.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictedHit, Do = UpgradeAction.Boon, Needs = AfflictionKind.Soaked, Stat = PlayerStat.PerfectGuardWindow, Amount = 0.25f, Seconds = 4f, Cooldown = 0.5f } } });
            // BESPOKE (built): FrozenWaveForm.
            to.Add(new Upgrade { Id = TheWaveStandsStill, Name = "The Wave Stands Still", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { ColdInTheBones }, X = 0.5f, Y = 4.6f, Icon = UpgradeIcons.Wave,
                Description = "Hold the spell button to throw a great wave. Let go, and it freezes where it stands with everything in it. One heavy blow shatters the wall and all it holds. 50 mana.",
                Form = SpellForms.FrozenWave,
                Spell = new SpellSpec { Cost = 50f, Reach = 9f, Speed = 6f, Arc = 6f, LingerDamage = 10f, Seconds = 4f, Damage = 45f, Stagger = 60f, Stacks = 5 } });

            // BESPOKE (built): PlatesForm.
            to.Add(new Upgrade { Id = Gift, Name = "Weight of Ice", Element = A, Element2 = B, Kind = UpgradeKind.Signature, X = 0f, Y = 5.8f, Icon = UpgradeIcons.Crown,
                Description = "Each blow you guard lays a plate of ice on you, up to five. A perfect guard lays two. Plates turn a little of every wound. Your next heavy blow spends them all in one great edge of ice.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Form, Form = SpellForms.Plates, Affliction = AfflictionKind.Chill, Spell = new SpellSpec { Cost = 0f, Count = 1, Damage = 12f, Stagger = 12f, Reach = 1.2f, Radius = 1.3f, Stacks = 1 } }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Form, Form = SpellForms.Plates, Affliction = AfflictionKind.Chill, Spell = new SpellSpec { Cost = 0f, Count = 2, Damage = 12f, Stagger = 12f, Reach = 1.2f, Radius = 1.3f, Stacks = 1 } } } });
        }
    }
}
