using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Lightning: the single right moment, written as spell branches (docs/design/steel/elements_a.md, Lightning; Phase 4).
    /// Six spells of four marks each and nine passives on the pattern of <see cref="BranchPattern"/>: in the middle Thunder
    /// (the lift Strikes Twice, Chain Lance and Waiting Thunder, and Devotion above them), on the left Charge (Stored Charge,
    /// Thunderclap and Ring of Drums, Single Stroke at the crown), on the right Flicker (Blink and Lightning Rod, Afterimage
    /// and Seven Flashes, Never There at the crown). Thunderclap is the base spell: it comes with Lightning, its Learn is
    /// never bought, and it grows from the root.
    ///
    /// Marks the design keeps keep their ids; the cut ones are in <see cref="RetiredMarks"/>. Static, Stored Charge, Blink,
    /// Afterimage, Single Stroke and Storm Step are done by hand in <see cref="PlayerUpgrades"/> under their ids; the rest
    /// are data. What the data cannot say yet is the nearest working version with a <c>NEEDS</c> line, gathered in
    /// docs/design/steel/content_open.tsv.
    /// </summary>
    public static class LightningTree
    {
        public const string StrikesTwice = "lightning.lift";
        public const string InOneBreath = "lightning.devotion";
        /// <summary>In One Breath: seconds with no spell cooldown after a perfect guard or a perfect dodge.</summary>
        public const float BreathSeconds = 2.5f;
        public const string SingleStroke = "lightning.stroke";
        public const string NeverThere = "lightning.neverthere";
        // The spells: each is its branch's Learn mark and the branch's id.
        public const string Thunderclap = "lightning.base";
        public const string ChainLance = "lightning.lance";
        public const string WaitingThunder = "lightning.waiting";
        public const string SevenFlashes = "lightning.flashes";
        public const string RingOfDrums = "lightning.drums";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Lightning;

            // BESPOKE (as built): light blows build Static and a heavy blow discharges it, done by hand. No WeaponStacks: that would lay it twice.
            to.Add(BranchPattern.Put(new Upgrade { Id = Upgrade.Static, Name = "Static", Element = E, Kind = UpgradeKind.Root, Icon = UpgradeIcons.Bolt,
                Description = "Blows build Static on an enemy, up to five. A heavy blow or a finisher discharges it, and it arcs to everyone near." },
                BranchPattern.Root));

            // ---- The middle bough: Thunder.
            // The reaction itself struck again at half strength half a second later (UpgradeAction.EchoReaction).
            to.Add(BranchPattern.Put(new Upgrade { Id = StrikesTwice, Name = "Strikes Twice", Element = E, Kind = UpgradeKind.Major, Lifts = E, Requires = new[] { Upgrade.Static }, Icon = UpgradeIcons.Burst,
                Description = "Every reaction with Static in it strikes again half a second later where it happened, as lightning comes back down its own channel.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Reacted, ReactionWith = AfflictionKind.Static, Do = UpgradeAction.EchoReaction, Amount = 0.5f, Delay = 0.5f } } },
                BranchPattern.MiddleFoot));

            SpellBranch.Add(to, E, ChainLance, "Chain Lance", SpellRole.Opener, BranchPattern.MiddleA, StrikesTwice, SpellForms.Bolt,
                new SpellSpec { Reach = 12f, Damage = 10f, Stagger = 15f, Count = 5, Radius = 5f, Stacks = 2 },
                "You throw a bolt ahead that leaps through up to five enemies: 10 damage and two Static on each.",
                UpgradeIcons.Beam,
                new Empower { Cooldown = 5.5f, Count = 6 },
                "Chain Lance waits 5.5 seconds, not 7, and leaps six times.",
                // NEEDS: the path steered with the stick while you fight, and the heavy blow calling the return stroke.
                new ShapeDef { Name = "Stepped Leader", Form = SpellForms.Spikes,
                    Description = "The lance is not thrown. A bolt steps out from your blade, one bright step of a metre and a half at a time, up to twelve metres, laying Static on what is near. Then the return stroke lights the whole path at once.",
                    Spell = new SpellSpec { Reach = 12f, Speed = 10f, Count = 8, Radius = 1.5f, Damage = 4f, Stagger = 4f, Stacks = 1 },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Line, Reach = 12f, Count = 8, Radius = 0.75f, Seconds = 0.2f, Interval = 0.2f, Delay = 1.2f, Damage = 14f, Stagger = 10f, Affliction = AfflictionKind.Static, Stacks = 1 } } },
                // The blade it charges lasts five blows, not three (SpellSpec.ChargeBlows).
                new ShapeDef { Name = "Return Stroke",
                    Description = "After its last leap it comes back along the same path to your blade, striking each enemy again for half. Throw, then fight.",
                    Change = new SpellChange { ChargeBlows = 5 },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Burst, Delay = 0.6f, Damage = 5f, Stagger = 5f, Radius = 0.8f } } },
                "Opens two shapes of Chain Lance, chosen at any rest fire: Stepped Leader, a path you draw, or Return Stroke, a bolt that comes back.",
                // NEEDS: only when the lance is thrown within 0.5 s after the finisher. Until then a finisher sends the arc itself.
                new[] { new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Arc, Damage = 10f, Stagger = 10f, Count = 8, Reach = 12f, Affliction = AfflictionKind.Static, Stacks = 2, Cooldown = 7f } },
                "Thrown within half a second after a finisher, it leaps to every enemy within twelve metres.");

            SpellBranch.Add(to, E, WaitingThunder, "Waiting Thunder", SpellRole.Breaker, BranchPattern.MiddleB, StrikesTwice, SpellForms.Waiting,
                new SpellSpec { Count = 5, Seconds = 10f, Reach = 8f, Damage = 8f, Stagger = 6f, Stacks = 1, Speed = 18f },
                "Five bolts hang behind your shoulders for ten seconds. Each light blow sends one down with it, and a heavy blow sends all that are left before it discharges.",
                UpgradeIcons.Spike,
                new Empower { Cooldown = 8f, Count = 7 },
                "Waiting Thunder waits 8 seconds, not 10, and hangs seven bolts.",
                // NEEDS: the bolts falling on the enemy faced whenever you strike anyone, not by themselves.
                new ShapeDef { Name = "Hanging Cloud", Form = SpellForms.Rain,
                    Description = "The bolts hang in a small black cloud over the enemy faced, not over you, and fall on it while you fight. Cut the soldier and drop the bolts on the archer.",
                    Spell = new SpellSpec { Reach = 8f, Radius = 1.5f, Seconds = 10f, Interval = 0.9f, Damage = 8f, Stagger = 6f, Stacks = 1 } },
                // NEEDS: light blows sending nothing; only the heavy brings the bolt.
                new ShapeDef { Name = "Hammer Fall", Form = SpellForms.Brand,
                    Description = "Light blows send nothing. Your heavy blow brings all of them as one bolt in a three-metre circle: five times a bolt's damage and 60 stagger. Save it for the heavy.",
                    Spell = new SpellSpec { Reach = 8f, Seconds = 10f, Count = 1, Radius = 3f, Damage = 40f, Stagger = 60f, Stacks = 2 } },
                "Opens two shapes of Waiting Thunder, chosen at any rest fire: Hanging Cloud over the enemy faced, or Hammer Fall, all of it in one heavy blow.",
                // NEEDS: only within a second of the last bolt falling with a light blow.
                new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Falling, Count = 5, Damage = 8f, Stagger = 6f, Radius = 1.5f, Delay = 0.3f, Affliction = AfflictionKind.Static, Stacks = 1, Cooldown = 10f } },
                "Land the heavy blow within a second of the last bolt falling with a light blow, and all five come back for a second round.");

            // The RuleBook's breath (task 4.4), read by PlayerSpells: after a perfect guard or dodge the four spell slots have no
            // cooldown for 2.5 s (SpellCooldowns.Breathe); what is cast in it waits its whole time once the breath is over.
            to.Add(BranchPattern.Put(new Upgrade { Id = InOneBreath, Name = "In One Breath", Element = E, Kind = UpgradeKind.Devotion, Requires = new[] { StrikesTwice }, Icon = UpgradeIcons.Hourglass,
                Description = "All in on Lightning only. After a perfect guard or a perfect dodge, your spells have no cooldown for a breath. Cast everything you hold in it; then the cooldowns start from full.",
                Rules = book => book.BreathAfterPerfect = BreathSeconds },
                BranchPattern.MiddleCrown));

            // ---- The left bough: Charge.
            // BESPOKE (as built): banked by hand in PlayerUpgrades (the count shown on the HUD); Deep Reserve is folded in, so
            // the count of three is this mark's own.
            to.Add(BranchPattern.Put(new Upgrade { Id = Upgrade.StoredCharge, Name = "Stored Charge", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Static }, Icon = UpgradeIcons.Shield,
                Description = "Perfect guards bank charges, up to three. Each blow spends one for double damage and stagger.",
                Tally = new TallySpec { Name = "Charge", Cap = 3 } },
                BranchPattern.LeftFoot));

            // Thunderclap, the base spell: cast by Lightning's kit, so its Learn carries no form; its shapes cast their own.
            SpellBranch.Add(to, E, Thunderclap, "Thunderclap", SpellRole.Breaker, BranchPattern.LeftA, Upgrade.Static, "", null,
                "Lightning's own spell, given with the element. A bolt falls on the enemy faced, up to ten metres away: 16 damage and 45 stagger, forking to two more.",
                UpgradeIcons.Bolt,
                // The 55 stagger. The Empower's numbers reach the kit's own cast too (PlayerUpgrades.BaseEmpower, read by PlayerElement).
                new Empower { Cooldown = 7f, Stagger = 55f },
                "Thunderclap waits 7 seconds, not 9, and breaks balance harder.",
                new ShapeDef { Name = "Rolling Thunder", Form = SpellForms.Spikes,
                    Description = "Three claps march out in a line, three metres apart, a quarter of a second each.",
                    Spell = new SpellSpec { Reach = 9f, Speed = 12f, Count = 3, Radius = 1.5f, Damage = 16f, Stagger = 30f, Stacks = 1 } },
                // NEEDS: the held press (SpellSpec.HoldForm), the clap growing with the hold. Until then it is the full hold.
                new ShapeDef { Name = "Held Thunder", Form = SpellForms.Bolt,
                    Description = "Hold for up to a second, then one huge clap with no forks: 90 stagger at the full hold.",
                    Spell = new SpellSpec { Reach = 10f, Damage = 20f, Stagger = 90f, Count = 0, Radius = 0.5f, Stacks = 1 } },
                "Opens two shapes of Thunderclap, chosen at any rest fire: Rolling Thunder, claps in a line, or Held Thunder, one great clap.",
                // NEEDS: no wind-up on the instant cast.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.ResetCooldown },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.InstantNext },
                },
                "A perfect guard makes the next Thunderclap instant and ends its wait.");

            // NEEDS: the ring standing 8 s (the drums go with it), and the third beat's great clap in 4 m (30 damage, 60
            // stagger) that discharges everything in it. Until then a cast strikes up three drums that wait to be beaten.
            SpellBranch.Add(to, E, RingOfDrums, "Ring of Drums", SpellRole.Counter, BranchPattern.LeftB, Upgrade.StoredCharge, SpellForms.Nova,
                new SpellSpec { Radius = 1.5f, Damage = 0f, Stagger = 0f, Stacks = 0 },
                "Three drums of cloud and lightning turn behind your shoulders. Each perfect guard or perfect dodge beats one, and a bolt falls on the attacker: 18 damage, 35 stagger and two Static.",
                UpgradeIcons.Ring,
                // NEEDS: the ring standing 10 s.
                new Empower { Cooldown = 8f },
                "Ring of Drums waits 8 seconds, not 10, and the ring stands longer.",
                // NEEDS: a perfect guard beating two at once.
                new ShapeDef { Name = "Drum Roll",
                    Description = "Plain guards beat them too, for a small clap, 8 damage and 15 stagger. For a player who guards rather than dodges.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Burst, Damage = 8f, Stagger = 15f, Radius = 1.5f, Affliction = AfflictionKind.Static, Stacks = 1, Cooldown = 0.5f } } },
                // NEEDS: the beats banked, nothing falling, and the next heavy carrying them all (30% damage and 20 stagger each).
                new ShapeDef { Name = "Silent Drum",
                    Description = "Nothing falls on a beat; each beat is banked, and your next heavy blow carries all of them.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Burst, Damage = 18f, Stagger = 40f, Radius = 1.2f, Affliction = AfflictionKind.Static, Stacks = 2, Cooldown = 3f } } },
                "Opens two shapes of Ring of Drums, chosen at any rest fire: Drum Roll, beaten by plain guards too, or Silent Drum, beats saved for a heavy blow.",
                // NEEDS: all three beaten within 3 s keeping the ring another round. Until then a perfect dodge beats one more.
                new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Falling, Damage = 18f, Stagger = 35f, Radius = 1.2f, Delay = 0.15f, Affliction = AfflictionKind.Static, Stacks = 2, Cooldown = 9f } },
                "Beat all three within three seconds and the ring stays for another round.",
                tally: new TallySpec { Name = "Drums", Cap = 3 },
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Bank, Count = 3 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Spends = 1, Do = UpgradeAction.Falling, Damage = 18f, Stagger = 35f, Radius = 1.2f, Delay = 0.15f, Affliction = AfflictionKind.Static, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Spends = 1, Do = UpgradeAction.Falling, Damage = 18f, Stagger = 35f, Radius = 1.2f, Delay = 0.15f, Affliction = AfflictionKind.Static, Stacks = 2 },
                });

            // Built by hand (the discharge in PlayerUpgrades.OnSwing): Static to ten deep with Lightning held, and a discharge
            // that does not arc and lands twice as hard on the one struck.
            to.Add(BranchPattern.Put(new Upgrade { Id = SingleStroke, Name = "Single Stroke", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { RingOfDrums }, Icon = UpgradeIcons.Blade,
                Description = "Static builds to ten. A discharge no longer arcs: all of it lands on the one struck, twice as hard.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionDepth, 5f) } }, BranchPattern.LeftCrown));

            // ---- The right bough: Flicker.
            // BESPOKE (as built). Still to add: the player is gone for the first tenth of a second and passes through enemies.
            to.Add(BranchPattern.Put(new Upgrade { Id = Upgrade.Blink, Name = "Blink", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Static }, Icon = UpgradeIcons.Boot,
                Description = "Each dodge sends a line of lightning down its path that shocks whatever it crosses." }, BranchPattern.RightFoot));

            // NEEDS (press-on-standing-thing): a second press while the rod stands stepping you to it in a flash (a 2.5 m shock, 14 damage, 20
            // stagger), the rod spent. Until then the rod strikes round itself.
            SpellBranch.Add(to, E, Upgrade.LightningRod, "Lightning Rod", SpellRole.Movement, BranchPattern.RightA, Upgrade.Blink, SpellForms.Rod,
                new SpellSpec { Reach = 2f, Radius = 3.5f, Seconds = 8f, Interval = 0.9f, Damage = 12f, Stagger = 12f, Delay = 0.2f, Stacks = 1 },
                "You plant a sword point-up two metres ahead, and for eight seconds bolts fall on enemies near it, each laying Static.",
                UpgradeIcons.Tower,
                new Empower { Cooldown = 8f, Radius = 4.5f },
                "Lightning Rod waits 8 seconds, not 10, and strikes further round itself.",
                // NEEDS: the pull toward the rod, not toward you.
                new ShapeDef { Name = "Lodestone", Form = SpellForms.Lodestone,
                    Description = "The rod draws every enemy within seven metres toward it, further for each Static they carry, then discharges all of them. There is no step.",
                    Spell = new SpellSpec { Reach = 2f, Radius = 7f, Delay = 0.3f, Damage = 14f, Stagger = 30f, Stacks = 0 } },
                new ShapeDef { Name = "Thrown Rod",
                    Description = "The rod is thrown up to ten metres, into a far enemy or the ground, so the step goes that far.",
                    Change = new SpellChange { Reach = 10f } },
                "Opens two shapes of Lightning Rod, chosen at any rest fire: Lodestone, a rod that pulls, or Thrown Rod, a rod planted far.",
                // Cast as a blow would land (UpgradeEffect.BlowLanding), it is a perfect dodge (UpgradeAction.PerfectDodgeNow). NEEDS: the rod
                // not spent by the step (press-on-standing-thing).
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, BlowLanding = 0.3f, Do = UpgradeAction.PerfectDodgeNow },
                },
                "Cast in the instant a blow would land on you, it counts as a perfect dodge, and the rod is not spent by the step.");

            // BESPOKE (as built). Still to add: the copy repeats the player's last blow and lays its Static.
            to.Add(BranchPattern.Put(new Upgrade { Id = Upgrade.Afterimage, Name = "Afterimage", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Blink }, Icon = UpgradeIcons.Eye,
                Description = "A dodge at the last moment leaves a copy of you that strikes where you stood." }, BranchPattern.RightMiddle));

            // BESPOKE (built): FlashesForm.
            SpellBranch.Add(to, E, SevenFlashes, "Seven Flashes", SpellRole.Detonator, BranchPattern.RightB, Upgrade.Afterimage, SpellForms.Flashes,
                new SpellSpec { Count = 7, Interval = 0.1f, Delay = 0.5f, Damage = 9f, Stagger = 8f, Stacks = 1, Reach = 8f },
                "You vanish, seven cuts are drawn from enemy to enemy, and you stand still on the spot you left with your back turned while they land together. Any enemy at full Static discharges. 45 mana.",
                UpgradeIcons.Star,
                new Empower { Mana = 35f, Damage = 11f },
                "Seven Flashes costs 35 mana, not 45, and each cut is heavier.",
                // NEEDS: all seven cuts on one line through the enemy faced, as one blow of seven.
                new ShapeDef { Name = "One Flash",
                    Description = "Against the enemy faced only. All seven cuts land as one blow, and its Static discharges twice.",
                    Change = new SpellChange { Reach = 3f },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Static, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Damage = 4f, Cooldown = 0.5f } } },
                // NEEDS: ending behind the last enemy cut, and the still shape of light left on the starting spot to be struck.
                new ShapeDef { Name = "Left Behind",
                    Description = "You do not return. You end behind the last enemy cut, and a shape of light takes the still pose on the starting spot and is what can be struck.",
                    Change = new SpellChange { Delay = 0.1f } },
                "Opens two shapes of Seven Flashes, chosen at any rest fire: One Flash on the one faced, or Left Behind, ending in the crowd.",
                // NEEDS: the cast within 0.3 s of a perfect dodge not standing still afterwards. Nothing in the data says it yet.
                null,
                "Cast within a third of a second of a perfect dodge, and you do not stand still afterwards: the cuts land at once.");

            // BESPOKE: a guard that costs twice the stamina for each point caught. Until then the price is a shorter perfect guard.
            // The stamina given back is the dodge's usual 25.
            to.Add(BranchPattern.Put(new Upgrade { Id = NeverThere, Name = "Never There", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { Upgrade.Afterimage }, Icon = UpgradeIcons.Wing,
                Description = "Every dodge carries further and a perfect dodge costs nothing. A blow held on your guard costs twice the stamina.",
                Stats = new[] { new StatMod(PlayerStat.DodgeDistance, 0.3f), new StatMod(PlayerStat.GuardStamina, 1f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Stamina, Amount = 25f } } },
                BranchPattern.RightCrown));

            // BESPOKE (as built): Storm Step, done by hand.
            to.Add(BranchPattern.Put(new Upgrade { Id = Upgrade.StormStep, Name = "Storm Step", Element = E, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "While Overdrive lasts, every dodge costs nothing and shocks what it passes through." }, BranchPattern.Gift));
        }
    }
}
