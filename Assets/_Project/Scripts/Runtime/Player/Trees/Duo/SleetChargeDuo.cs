using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Ice and Lightning (pairs_d1_d2.md, "Lightning and Ice", distance 2; task 4.5), the first of the 44
    /// new branches, named by its reaction, Sleet Charge (IMPLEMENTATION.md 2.3: one file per pair under Trees/Duo/). Drawn in
    /// the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark opens Sleet Charge's two
    /// shapes, Thundersnow and Glass Spear; two majors (Charged Hail, Cold Snap), a keystone (Nothing Resists) and the duo
    /// spell, Glass Storm, the crown.
    ///
    /// Ice is written first, though the ring puts Lightning first: a duo branch's first element is the one its duo spell is
    /// cast in, and Glass Storm is clear ice that lays Chill with each pass and charges the blade with Ice (the design's
    /// "a duo spell charges with its first element"). Storage order does not matter to any rule (<see cref="HeartLayout.PairOf(Upgrade)"/>).
    /// </summary>
    public static class SleetChargeDuo
    {
        public const string Shape = "sleetcharge.shape";
        public const string ChargedHail = "sleetcharge.hail";
        public const string ColdSnap = "sleetcharge.snap";
        public const string NothingResists = "sleetcharge.resists";
        public const string GlassStorm = "sleetcharge.glassstorm";
        /// <summary>The conditions the two shapes set, each in place of the plain reaction's charging.</summary>
        public const string Snow = "sleetcharge.snow";
        public const string Spear = "sleetcharge.spear";

        // Laid as stacks, more than any affliction holds: "to full depth", whatever the depth is.
        private const int ToFull = 99;

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Ice, B = Elements.Lightning;

            // Sleet Charge's two shapes. Both keep the charging (a Static every half second for 3 s); they change how it ends.
            // NEEDS: a set-off on every enemy in reach that carries a mark, each discharging its own Static (Thundersnow: now a
            // bolt leaps from one chilled enemy to the next within 6 m); frozen for set seconds whatever its Chill (Glass Spear:
            // now Chill to full, which freezes for the Chill's own time); the clear ice spear standing where the bolt struck.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Sleet Charge: Thundersnow or Glass Spear", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Sleet Charge's two shapes, chosen at a rest fire. Thundersnow: the bolt that ends the charge falls on every chilled enemy near. Glass Spear: the bolt freezes as it lands and the enemy is Frozen at once. Either runs instead of the plain Sleet Charge.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Thundersnow",
                        Description = "The cloud is wide: the charge builds as before, and when it ends the discharge lands as one bolt on every chilled enemy within 6 m, each struck for 7 and 5 stagger and charged.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Snow, Seconds = 3f, Beat = 0.5f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Snow, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Snow, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Stagger = 5f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Snow, Do = UpgradeAction.Arc, FromTarget = true, LeapsTo = AfflictionKind.Chill, Count = 6, Reach = 6f,
                                Damage = 7f, Stagger = 5f, Affliction = AfflictionKind.Static, Stacks = 1 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Glass Spear",
                        Description = "The bolt freezes as it lands: when the charge ends and discharges, a spear of clear ice stands where it struck and the enemy is Frozen at once, whatever its Chill, ready for a heavy blow. The great take 3 Chill, a god 2.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Spear, Seconds = 3f, Beat = 0.5f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Spear, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Spear, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Stagger = 5f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Spear, Ranks = EnemyRank.Plain, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = ToFull },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Spear, Ranks = EnemyRank.Elite, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 3 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Spear, Ranks = EnemyRank.God, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 2 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: a shatter's own shards as the strikers (each shard that strikes lays a Static); now a ring of Static
            // round the shattered enemy, 3 m.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = ChargedHail, Name = "Charged Hail", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Burst,
                Description = "Shattering an enemy that carried Static throws its charge into the shards: everything the shards strike within 3 m takes a Static.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Shatter, Needs = AfflictionKind.Static, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1, Radius = 3f, Cooldown = 0.2f },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: a condition ended on demand (the Sleet Charge already running strikes at once); now the charge is filled
            // and set off at once, and a new Sleet Charge may begin from the Static laid.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = ColdSnap, Name = "Cold Snap", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard against a chilled enemy makes its Sleet Charge strike at once, with full Static: it is charged to full and the charge goes off, 7 a stack in 4 m.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Chill, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = ToFull, Cooldown = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Chill, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Stagger = 5f, Cooldown = 1f },
                },
            }, HeartPattern.RightLow));

            // NEEDS: the moment an arc leaves an enemy, and an arc that keeps its full damage from one to the next; now a
            // discharge on a chilled enemy runs on as an arc through every chilled enemy within 5 m of the last, four at most.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = NothingResists, Name = "Nothing Resists", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { ChargedHail, ColdSnap }, Icon = UpgradeIcons.Chain,
                Description = "Cold carries lightning without loss: Static discharged from a chilled enemy runs on at full strength to every chilled enemy within 5 m of the last, 7 and a Static each.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = AfflictionKind.Static, Needs = AfflictionKind.Chill, Do = UpgradeAction.Arc, FromTarget = true, LeapsTo = AfflictionKind.Chill,
                        Count = 4, Reach = 5f, Damage = 7f, Stagger = 5f, Affliction = AfflictionKind.Static, Stacks = 1, Cooldown = 0.5f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, the crown (pairs_d1_d2.md): an opener on a 14 s cooldown (SpellTimings). The Discs form turns the
            // shards; each pass lays the branch's first element's Chill, and half the passes (by chance) a Static.
            // NEEDS: a count of passes per enemy, so it is every other pass and not half of them at random.
            // NEEDS: a weapon blow that flings an orbiting piece (each landed blow flings the nearest shard into the enemy struck,
            // 8, a Chill and a Static; a heavy blow flings all that are left); hard glassy splinters with threads of lightning
            // between them, never a glowing ring.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = GlassStorm, Name = "Glass Storm", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { ChargedHail, ColdSnap }, Icon = UpgradeIcons.Orb,
                Description = "Six shards of clear ice turn round you at 2 m for 8 s, threads of lightning jumping between them. Each shard that passes through an enemy cuts it for 4 and lays a Chill, and half the passes a Static.",
                Form = SpellForms.Discs,
                Spell = new SpellSpec { Count = 6, Radius = 2f, Seconds = 8f, Interval = 0.5f, Damage = 4f, Stagger = 4f, Stacks = 1 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1, Chance = 0.5f },
                },
            }, HeartPattern.Spell));
        }
    }
}
