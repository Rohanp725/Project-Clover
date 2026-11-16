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

            // With the held element's own root learned as well, that element's affliction is laid twice by each blow.
            to.Add(new Upgrade { Id = ColdWater, Name = "Cold Water", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, X = 0f, Y = 1f, Icon = UpgradeIcons.Drop,
                Description = "Your blows lay Chill and Soaked together.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                } });
            // NEAREST: the depth at which Chill freezes cannot be changed, so a Soaked enemy takes one more Chill from a blow or a spell, and reaches five sooner.
            to.Add(new Upgrade { Id = FlashFreeze, Name = "Flash Freeze", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { ColdWater }, X = -0.5f, Y = 2.36f, Icon = UpgradeIcons.Snowflake,
                Description = "A Soaked enemy freezes at three Chill, not five.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictedHit, Do = UpgradeAction.Afflict, Needs = AfflictionKind.Soaked, Affliction = AfflictionKind.Chill, Stacks = 1, Cooldown = 1f } } });
            // The spikes form gives the line of ice; holding enemies by the feet is not in it yet.
            to.Add(new Upgrade { Id = CreepingIce, Name = "Creeping Ice", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { ColdWater }, X = 0.5f, Y = 2.36f, Icon = UpgradeIcons.Spike,
                Description = "Hold the spell button to send a line of ice along the ground. Those it reaches are held to the floor by their feet: they can strike but cannot turn or step. 35 mana.",
                Form = "spikes",
                Spell = new SpellSpec { Cost = 35f, Reach = 8f, Speed = 20f, Count = 1, Radius = 0.6f, Damage = 8f, Stagger = 10f, Stacks = 2 } });
            to.Add(new Upgrade { Id = Icebreaker, Name = "Icebreaker", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { FlashFreeze }, X = -0.5f, Y = 4.6f, Icon = UpgradeIcons.Hammer,
                Description = "Shattering an enemy gives mana and fills a little of an empty gourd.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.Mana, Amount = 12f }, new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.FlaskFill, Amount = 0.33f } } });
            // NEAREST: an enemy's wind-up cannot be changed, so a blow on a Soaked enemy lengthens the player's own moment to guard for a while.
            to.Add(new Upgrade { Id = ColdInTheBones, Name = "Cold in the Bones", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { FlashFreeze }, X = 0f, Y = 3.48f, Icon = UpgradeIcons.Hourglass,
                Description = "An enemy both Soaked and chilled is slow to wind up its blows. The moment to guard them is easier to find.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictedHit, Do = UpgradeAction.Boon, Needs = AfflictionKind.Soaked, Stat = PlayerStat.PerfectGuardWindow, Amount = 0.25f, Seconds = 4f, Cooldown = 0.5f } } });
            // BESPOKE: a wave rolls out while the button is held and freezes where it stands on release, with everything
            // in it; one heavy blow shatters all of it. Until then the wall form: it rises ahead, stands, and bursts for a heavy blow.
            to.Add(new Upgrade { Id = TheWaveStandsStill, Name = "The Wave Stands Still", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { ColdInTheBones }, X = 0.5f, Y = 4.6f, Icon = UpgradeIcons.Wave,
                Description = "Hold the spell button to throw a great wave. Let go, and it freezes where it stands with everything in it. One heavy blow shatters the wall and all it holds. 50 mana.",
                Form = "wall",
                Spell = new SpellSpec { Cost = 50f, Reach = 4f, Arc = 6f, Delay = 0.4f, Seconds = 4f, Radius = 5f, Damage = 45f, Stagger = 60f, Stacks = 5 } });

            // BESPOKE: guarded blows lay plates of ice, five at most, each turning a little of every wound; the next heavy
            // blow spends them all for reach, damage and Chill. Until then: a guard hardens the player and the next blows.
            to.Add(new Upgrade { Id = Gift, Name = "Weight of Ice", Element = A, Element2 = B, Kind = UpgradeKind.Signature, X = 0f, Y = 5.8f, Icon = UpgradeIcons.Crown,
                Description = "Each blow you guard lays a plate of ice on you, up to five. A perfect guard lays two. Plates turn a little of every wound. Your next heavy blow spends them all in one great edge of ice.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Boon, Stat = PlayerStat.DamageTaken, Amount = -0.1f, Seconds = 12f },
                    new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Boon, Stat = PlayerStat.WeaponDamage, Amount = 0.2f, Seconds = 6f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.DamageTaken, Amount = -0.15f, Seconds = 12f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.WeaponDamage, Amount = 0.4f, Seconds = 6f },
                } });
        }
    }
}
