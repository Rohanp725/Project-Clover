using Clover.Gameplay;

namespace Clover.Player
{
    /// <summary>
    /// One thing an upgrade does: when (<see cref="On"/>), under what conditions, and what (<see cref="Do"/>) with
    /// which numbers. Which numbers an action reads is written on each <see cref="UpgradeAction"/>. Distances are in
    /// metres, times in seconds, damage and stagger in the same points weapons use (a light blow of the fists is 8
    /// damage and 10 stagger; a soldier has 60 health and 100 balance).
    /// </summary>
    public class UpgradeEffect
    {
        public UpgradeTrigger On;
        public UpgradeAction Do;
        public EffectAt At = EffectAt.Target;

        // Conditions. All that are set must hold.
        /// <summary>The chance it happens each time the trigger comes: 1 always, 0.25 one time in four.</summary>
        public float Chance = 1f;
        /// <summary>Seconds after it happens before it can happen again.</summary>
        public float Cooldown;
        /// <summary>
        /// True when <see cref="Cooldown"/> runs on each enemy apart (kept in its <see cref="EnemyTally"/>): an elite's
        /// flinch once every 4 s on each elite, not once every 4 s for all of them. With no enemy to keep it, one clock.
        /// </summary>
        public bool CooldownPerEnemy;
        /// <summary>
        /// Above one, it happens only every this many times it would on the same enemy, counted on that enemy (every
        /// fifth bite, every other pass). With no enemy to count on, it does not happen.
        /// </summary>
        public int EveryOnEnemy;
        /// <summary>For <see cref="UpgradeTrigger.EverySeconds"/>: the seconds between.</summary>
        public float Every = 5f;
        /// <summary>The target must already carry this affliction. None asks for nothing.</summary>
        public AfflictionKind Needs = AfflictionKind.None;
        /// <summary>True when it happens only while the surge lasts.</summary>
        public bool WhileSurge;
        /// <summary>The target must carry this one as well as <see cref="Needs"/>.</summary>
        public AfflictionKind Needs2 = AfflictionKind.None;
        /// <summary>The affliction named in <see cref="Needs"/> must be at its full depth.</summary>
        public bool NeedsFull;
        /// <summary>The affliction named in <see cref="Needs"/> must lie at least this deep (Fish Poison's four doses). Zero asks only that it is there.</summary>
        public int NeedsAtLeast;
        /// <summary>Only on enemies of these ranks (an elite's or a god's own numbers in a reaction). None set: on all.</summary>
        public Clover.Gameplay.EnemyRank Ranks;
        /// <summary>The target must be in the air (or reeling).</summary>
        public bool NeedsAirborne;
        /// <summary>Only while the player's health is below this share of the whole. Zero: at any health.</summary>
        public float HealthBelow;
        /// <summary>
        /// Damage, stagger and stacks are multiplied by the stacks the trigger carried: what was spent by a set-off, or what
        /// the dead enemy carried of <see cref="Needs"/>. When the trigger carried none, by what the Spend effect before it
        /// (in the same mark, on the same enemy) took or counted; with a <see cref="Share"/>, by that share of it, rounded
        /// up. A ground effect laid in a shape (a ring, a line) is multiplied in its patches instead (Firebloom's lilies).
        /// </summary>
        public bool PerStack;
        /// <summary>
        /// Not None: what is done is multiplied by how deep this affliction lies on the one the trigger names, at that moment
        /// (Night Sun: 1 healed for each Burn on the enemy struck). Nothing is done when it carries none.
        /// </summary>
        public AfflictionKind PerDepthOf = AfflictionKind.None;
        /// <summary>
        /// Not None: a pull, push, hold or laying over an area takes only the enemies carrying this affliction (The Pull of
        /// Night draws the Soaked alone; Sea Ice's web chills only the Soaked).
        /// </summary>
        public AfflictionKind OnlyCarrying = AfflictionKind.None;
        /// <summary>
        /// Not None: an arc's first leap goes to any enemy, and it leaps on only from those carrying this affliction
        /// (Mirror to Mirror: on from each struck that carries Sundered).
        /// </summary>
        public AfflictionKind LeapsOnFrom = AfflictionKind.None;
        /// <summary>
        /// Not empty: the target (or the dead one, for a kill) must be in this enemy condition, whichever mark set it,
        /// known by its name or by what it is (<see cref="ConditionSpec.Is"/>): Hot Stone, Cold Water asks for a Cracked
        /// enemy, Hel's Half for one whose warmth is stolen. A blow that spent the condition still finds it.
        /// </summary>
        public string InState = "";
        /// <summary>For ForceReaction: one of the pair on the target is enough; the reaction itself goes off, spending nothing (the ledger's forced-alone).</summary>
        public bool Alone;
        /// <summary>
        /// For ForceReaction: the reaction goes off as if its missing partner had just been laid. One of the pair is enough, but
        /// its rest on that enemy still holds, and the conditions it sets there last without the partner (the ledger's
        /// laid-as-condition: The Sun Goes Under, every Burn smouldering, curse or no curse).
        /// </summary>
        public bool AsLaid;
        /// <summary>For Heal: the mending is what the blow a counter just turned would have dealt you, Amount at most (Death Turned Back: 30).</summary>
        public bool ByTurnedWound;
        /// <summary>For Kill: only a death dealt by this reaction's own blow, by its id (The Feast: a Kindling Point burst that kills).</summary>
        public string KilledBy = "";
        /// <summary>
        /// For a Pass with ToState answering a numbered condition's death (ConditionSpec.Numbered): the next on the line by its
        /// number after the dead one's, not the nearest in that state (Eaten in Turn).
        /// </summary>
        public bool InOrder;
        /// <summary>
        /// Above one: weapon blows take turns, and this effect is done only on its <see cref="Turn"/> (0 up to TakesTurns - 1) of
        /// every TakesTurns swings that land; the hits of one swing share a turn (The Yawning Gap: burning, then chilling).
        /// </summary>
        public int TakesTurns;
        public int Turn;
        /// <summary>
        /// For ForceReaction, or a Pass that sets <see cref="Reaction"/> off on the enemy it reaches: the counts of blows the
        /// reaction's conditions keep there start this many blows along (Held Down, What the Yew Keeps: the fig at two coils).
        /// </summary>
        public int CountedBlows;
        /// <summary>For the Reacted and ReactionStruck triggers: only a reaction with this affliction in its pair ("every reaction with Chill in it"). None: any.</summary>
        public AfflictionKind ReactionWith = AfflictionKind.None;
        /// <summary>
        /// For a Lane: not None, it runs from the target to the nearest other enemy within Reach carrying this (or
        /// <see cref="AimAt2"/>), striking all on the line, and stops there (Laid Flat's bolt along the ground to the next
        /// Marked or charged enemy); with none such it strikes the target alone.
        /// </summary>
        /// <summary>
        /// Above zero: the effect is done once on each enemy within this many metres of the target that carries
        /// <see cref="OnlyCarrying"/> (any, when None), as that enemy's own, <see cref="Count"/> of them at most (0: all;
        /// the nearest first). With <see cref="EachOther"/> the target itself is left out. Thundersnow's bolt on every
        /// chilled enemy near; Deep Crack's crack where each Soaked enemy stands (the ledger's area-set-off, spend-soaked).
        /// </summary>
        public float EachWithin;
        public bool EachOther;
        public AfflictionKind AimAt = AfflictionKind.None;
        public AfflictionKind AimAt2 = AfflictionKind.None;
        /// <summary>
        /// For ConditionStruck: only a blow from this side, in degrees from the enemy's face (0) to its back (180), between
        /// <see cref="SideLeast"/> and <see cref="SideMost"/> (the ledger's side-of-blow: Rime's crust breaks only from the
        /// side that faced you, Mock Suns' blows only from a flank). Both zero: any side. A blow whose side is not known is answered.
        /// </summary>
        public float SideLeast;
        public float SideMost;
        /// <summary>The stacks it lays are as many as the trigger carried: what the dead enemy bore of <see cref="Needs"/>, or what a set-off spent.</summary>
        public bool CarriesDepth;
        /// <summary>Only while this state of the player holds.</summary>
        public StatWhen While = StatWhen.Always;
        /// <summary>And while this one holds too (Stone Rise's Master: still for a second as well as rooted).</summary>
        public StatWhen While2 = StatWhen.Always;
        /// <summary>For the SetOff trigger: only when this was the affliction spent. None: any.</summary>
        public AfflictionKind Spent = AfflictionKind.None;
        /// <summary>For the SetOff trigger: only when at least this many stacks were spent.</summary>
        public int SpentAtLeast;
        /// <summary>For a Hazard: it is something grown (counted, stood near, burst with the rest), and at most <see cref="Limit"/> of this effect's stand at once (0: no limit).</summary>
        public bool Grown;
        public int Limit;
        /// <summary>For the SpellCast trigger: only when the spell cast is this mark's own.</summary>
        public bool OwnSpell;
        /// <summary>For SpellCast: only when the spell cast is the element's base spell.</summary>
        public bool BaseSpell;
        /// <summary>The target's poison must have bitten, or be about to bite, within this many seconds (0: not asked).</summary>
        public float OnBeat;
        /// <summary>For Boon: it ends with the next swing of the weapon that lands, if that comes before its time is up.</summary>
        public bool UntilBlow;
        /// <summary>For Boon: it ends with the player's next dodge, if that comes before its time is up (Second Wind's next dodge free; the ledger's until-dodge).</summary>
        public bool UntilDodge;
        /// <summary>It happens only if its mark's count holds this many, and spends them.</summary>
        public int Spends;
        /// <summary>It happens only if its mark's count holds any, spends them all, and carries how many were spent (for <see cref="PerStack"/>).</summary>
        public bool SpendsAll;
        /// <summary>
        /// For the SpellCast and SpellHit triggers: only when the spell is its own branch's (a Master's timing, a shape's
        /// rule). Its mark's <see cref="Upgrade.Branch"/> names the branch.
        /// </summary>
        public bool OwnBranch;
        /// <summary>For SpellHit: only once the cast has struck at least this many, the one now struck among them (Step Into Light's Master: two in one line).</summary>
        public int StruckAtLeast;
        /// <summary>For a spell's moments: only for the casts of this branch (its Learn id), not the mark's own (The Tide Brings It: Riptide's).</summary>
        public string CastOf = "";
        /// <summary>For CutCooldowns, ResetCooldown and InstantNext: the branch (its Learn id) whose slot it acts on. Empty: its own mark's branch.</summary>
        public string Branch = "";
        /// <summary>For the Reacted trigger: only for this reaction (its id, "react.steam"). For ForceReaction: the reaction forced. Empty: any.</summary>
        public string Reaction = "";
        /// <summary>For an effect a reaction runs: what it lays may set off reactions of its own (Sea Ice, Breakers and the like); otherwise it starts none.</summary>
        public bool Cascades;
        /// <summary>For the ConditionBeat, ConditionStruck and ConditionEnded triggers: the condition answered (its <see cref="ConditionSpec.Id"/>).</summary>
        public string Condition = "";
        /// <summary>For ConditionStruck: which blows it answers.</summary>
        public StruckBy By;
        /// <summary>For ConditionStruck: only the condition's Nth weapon blow (Logan Stone's third). For ConditionBeat: only its Nth beat. Zero: every one.</summary>
        public int AtCount;
        /// <summary>
        /// For the condition triggers: it answers the condition named in <see cref="Condition"/> whoever set it (a reaction,
        /// its shape), not only one its own mark set, while its own mark is in force (The Exile Returns at the Grudge's bolt).
        /// Done before the answers of the mark that set the condition, so it can count what they are about to spend.
        /// </summary>
        public bool AnyMark;
        /// <summary>With <see cref="PerStack"/>: the most it is multiplied by (Ash Plume's five flashes); zero for no cap.</summary>
        public int PerMost;
        /// <summary>With <see cref="PerStack"/>: added to what it is multiplied by, before <see cref="PerMost"/> (one more seed).</summary>
        public int PerMore;
        /// <summary>
        /// For the dodge triggers (DodgeStart, DodgeEnd, PerfectDodge): only a dodge that went this way against the enemy
        /// faced (the ledger's dodge-direction). Any: every dodge.
        /// </summary>
        public DodgeWay Way;

        // Numbers.
        /// <summary>The affliction laid or set off. None means the element's own.</summary>
        public AfflictionKind Affliction = AfflictionKind.None;
        /// <summary>Stacks of the affliction laid. For actions that deal damage, zero lays none.</summary>
        public int Stacks;
        public float Damage;
        public float Stagger;
        public float Radius;
        public float Reach;
        /// <summary>Full width of a cone, in degrees.</summary>
        public float Arc;
        public float Seconds;
        public float Delay;
        public float Interval;
        public float Speed;
        /// <summary>A plain amount: health, mana, stamina, metres, or a share, as the action says.</summary>
        public float Amount;
        public int Count;
        /// <summary>For <see cref="UpgradeAction.Boon"/>: the stat changed.</summary>
        public PlayerStat Stat;
        public HazardShape Shape = HazardShape.Patch;
        public Flight Flight = Flight.Straight;
        /// <summary>For <see cref="UpgradeAction.Form"/>: the form cast, by its name in <see cref="SpellForms"/>, and the numbers it reads.</summary>
        public string Form = "";
        public SpellSpec Spell;
        /// <summary>For <see cref="UpgradeAction.Condition"/>: the condition set.</summary>
        public ConditionSpec State;
        /// <summary>For <see cref="UpgradeAction.Spend"/>: the share of the stacks taken, rounded up (0: all). With <see cref="PerStack"/>: the share of the count used.</summary>
        public float Share;
        /// <summary>For Spend: the stacks are only counted, for the effects after it, and none is taken.</summary>
        public bool CountOnly;
        /// <summary>Damage added for each stack the Spend effect before it (in the same mark) took.</summary>
        public float DamagePerSpent;
        /// <summary>For Hazard: the ground it lays leaves the trigger's own enemy alone (Ill Wind's plume does not dose the one that shed it).</summary>
        public bool SparesTarget;
        /// <summary>For Burst: it strikes without its flash on the ground, its look drawn elsewhere (Nerve Fire's jolts at each bite).</summary>
        public bool Unseen;
        /// <summary>
        /// For an Arc: it leaps only to enemies carrying this, from the target without striking it again (Live Iron). None:
        /// an arc as usual. For a Condition with a Radius: only the enemies carrying this take it (Chokedamp's shaken).
        /// </summary>
        public AfflictionKind LeapsTo = AfflictionKind.None;
        /// <summary>For an Arc: it starts from the target without striking it and leaps to whoever is nearest, carrying anything (Glint's ray).</summary>
        public bool FromTarget;

        // Depth moved between enemies (the ledger's depth-moved).
        /// <summary>For CoatBlade: each blow raises what it strikes to the depth loaded, rather than laying that many more.</summary>
        public bool SetsDepth;
        /// <summary>For CoatBlade: only light blows take the coat (Dust on the Blade's three light blows).</summary>
        public bool LightOnly;
        /// <summary>
        /// For SetOff: the burst carries on the doses of the one before it in the same cast, and what it burst with is carried
        /// to the next (One by One: each passing its doses to the next before it goes, the last the largest; All Into One:
        /// the doses gathered first). It bursts on carried doses alone when it has none of its own.
        /// </summary>
        public bool PassesOn;
        /// <summary>For Pass: the doses go to the enemy the condition is joined to (Rot Roots' host), as deep as it can take, not to the nearest.</summary>
        public bool ToLink;
        /// <summary>
        /// Done to the enemy chained to the trigger's (its chain-mate: <see cref="Clover.Gameplay.EnemyChains"/>), with the
        /// trigger's enemy's place kept as the point (Dead Weight drags the mate there); nothing when it is chained to no one.
        /// </summary>
        public bool ToChainMate;
        /// <summary>
        /// Done to the enemy whose blow last landed on you (Grasp's Master), or whose blow you last turned or dodged (Glare's
        /// Master), within the effect's CastWindow (1 s when none is named); nothing when there is none that recent.
        /// </summary>
        public bool ToStruckBy;
        /// <summary>
        /// In force only while a cast of its own branch still stands (the tree, the blades), and, with <see cref="WithinOwn"/>
        /// above zero, only while you stand that near where it was cast (the ledger's while-spell-stands).
        /// </summary>
        public bool WhileOwnStands;
        public float WithinOwn;
        public bool ToTurnedBy;
        /// <summary>For Tend: what the tended grown thing lays with every bite from then on (The Host: a dose of poison).</summary>
        public AfflictionKind BitesLay = AfflictionKind.None;
        /// <summary>
        /// For Pass: what is pushed past the taker's depth bursts it at once (its own set-off) and passes on to the next, a
        /// few times at most (Eaten in Turn). Without it, what does not fit is lost.
        /// </summary>
        public bool Overflow;
        /// <summary>
        /// For Pass: not empty, the nearest other enemy in this condition (by its name or what it is) takes the doses before any
        /// other (the next on Eaten in Turn's line). For PayNow: the condition that pays out.
        /// </summary>
        public string ToState = "";
        /// <summary>For an Arc from the target: each enemy the arc reaches is drawn this many metres toward the target, and no one else (Drawn Iron; the ledger's afflicted-only).</summary>
        public float DrawsIn;
        /// <summary>
        /// For a Condition: the condition keeps what the trigger carried (a reaction's blow's damage) as its number. For a
        /// condition's answer: its damage is that kept number, by Share (1: all of it) (Collected Twice repeats the reaction's damage).
        /// </summary>
        public bool ByPower;
        /// <summary>For BoostReaction: the reaction spends nothing when it fires (its own Spend effects are not done).</summary>
        public bool SparesSpend;
        /// <summary>The target must be frozen (Fire Under the Ice: a burning Frozen enemy shatters on any blow).</summary>
        public bool NeedsFrozen;
        /// <summary>Only just after a red attack was turned, by a perfect guard or a counter (the ledger's red-attack).</summary>
        public bool RedOnly;
        /// <summary>
        /// Above zero: the target must be showing an attack (its wind-up), begun no more than this many seconds before (the
        /// ledger's enemy-begins-attack; 99: anywhere in its wind-up; 0.3: in the instant its attack mark shows).
        /// </summary>
        public float NeedsWindup;
        /// <summary>Above zero: only when an enemy's blow is about to land on the player within this many seconds (the shared movement rule's timing: 0.3).</summary>
        public float BlowLanding;
        /// <summary>For a spell's moments: only when the cast was timed to the poison's beat, within 0.2 s of a bite on any enemy within 8 m (the ledger's cast-on-beat).</summary>
        public bool OnCastBeat;
        /// <summary>
        /// Only on the enemy whose blow was just turned or dodged (a counter's guard, a perfect guard or a perfect dodge), not
        /// on all its answer strikes (the ledger's attacker-of-turned-blow: The Golden Chair, Gust at the Heel, Ill Omen).
        /// </summary>
        public bool NeedsAttacker;
        /// <summary>
        /// For Boon: held by the cast whose moment this answers, not by the player, so only that cast's own blows and bursts
        /// are made stronger (One by One's Master: its own bursts, not a reaction's or Volatile's in the same moment).
        /// </summary>
        public bool ThisCast;
        /// <summary>
        /// With Needs: how deep the target carries it as the moment comes is the trigger's count (PerStack reads it), and a
        /// Condition keeps that count as its run's number. Every effect of a moment is asked before any of them is done, so a
        /// shape counts what its Learn spends in the same moment (Red-Hot: 6 more a notch the pour spent; the ledger's
        /// shape-before-learn).
        /// </summary>
        public bool CountsNeeds;
        /// <summary>
        /// Above zero: only within this many seconds of its branch's own spell being cast (Poured on the Edge's wave and mending,
        /// only in its 12 s; a shape's rule held to its cast's window).
        /// </summary>
        public float WithinOwnCast;
        /// <summary>For Spends: paid from the count its branch's Learn keeps, not its own mark's (Sapling's Strangler and Dodder spend the window's saplings).</summary>
        public bool PaysLearn;
        /// <summary>Above zero: grown things laid by the effects of one mark with the same number share one limit (Sapling's guard and dodge plantings: three at most in all).</summary>
        public int Kin;
        /// <summary>For FreeSurge: the element whose surge it lights.</summary>
        public string SurgeOf = "";
        /// <summary>
        /// Only for a cast begun within CastWindow seconds after this moment (the ledger's spell-windows: Chain Lance's Master
        /// within 0.5 s after a finisher, Poured Bronze's within 1 s of a perfect guard). For a death, CastAfterCarrying names
        /// what the dead must have carried (Reap What Is Sown's: a Seeded one).
        /// </summary>
        public CastMoment CastAfter;
        public float CastWindow;
        /// <summary>
        /// Only within AfterWindow seconds after this moment, asked as the trigger comes rather than of the cast (Waiting
        /// Thunder's Master: a heavy blow within a second of the last bolt falling with a light blow).
        /// </summary>
        public CastMoment After;
        public float AfterWindow;
        /// <summary>Only while its own branch's spell is begun and gathering (Bag of Winds open: each attacker's wind thrown back).</summary>
        public bool WhileGathering;
        /// <summary>
        /// Above zero: only while the player stands within this many metres of something they grew; with <see cref="Kin"/> set,
        /// only of what this mark's effects of the same Kin grew (Corpse Flowers: a blow within 3 m of a flower).
        /// </summary>
        public float NearGrown;
        /// <summary>With NearGrown: done at the grown thing nearest the player within it, not where the trigger was (Briar Patch's Master: the thicket round it).</summary>
        public bool AtGrown;
        /// <summary>For RingAdded: the last SpentAtLeast rings all came within this many seconds (Bag of Winds' Master: three within three).</summary>
        public float RingsWithin;
        public AfflictionKind CastAfterCarrying = AfflictionKind.None;
        /// <summary>
        /// For Raise: the standing thing raised (World.StandingThings), where (RaiseAt, Reach metres off), for Seconds, with
        /// Amount health (0: no blow breaks it), bursting Delay seconds after it is first struck when above zero, drawing
        /// enemies within <see cref="Draws"/> onto itself, and Limit of its mark's standing at once.
        /// </summary>
        public Clover.World.StandingKind Raises;
        public Clover.World.StandingPlace RaiseAt;
        public float Draws;
        /// <summary>Above zero: only for an enemy with stone at its back, a standing thing or a wall within this many metres behind it as you see it (Hammer and Anvil).</summary>
        public float StoneAtBack;
        /// <summary>Above zero: only while you stand within this many metres of a standing thing of the kind <see cref="Raises"/> names (Frozen Footing: a mound).</summary>
        public float NearStanding;
        /// <summary>For Pieces: how they leave the enemy (a fan away from you, a ring, each at a nearest other, or a cone out of its far side).</summary>
        public PieceAim Thrown;
        /// <summary>For DropPickup: the pickup dropped (World.Pickups). For PickedUp and PickupLeft with AnyMark: the kind answered, whoever dropped it.</summary>
        public Clover.World.PickupKind Drops;
        /// <summary>For PickedUp: answers the taking of any pickup of the kind Drops names, whoever dropped it (The Whole Mountain).</summary>
        public bool AnyDrops;
        /// <summary>A recipe look played as it is done (Forms.CreatureLooks: an effect-only creature), on its enemy or where it is done.</summary>
        public string Look = "";
        /// <summary>
        /// For a Hazard: ground that keeps acting. <see cref="Inside"/> is set at each bite on every enemy standing in it (for a
        /// little past the next bite, GroundRules.StateSeconds); <see cref="Pull"/> draws what stands in it to its middle, metres a
        /// second; <see cref="Mends"/> heals the player standing in it, health a second.
        /// </summary>
        public ConditionSpec Inside;
        public float Pull;
        public float Mends;
        /// <summary>
        /// For a Hazard: effects done at each bite on each enemy standing in the ground, as that enemy's own, so their own tests
        /// (Needs, Ranks, CooldownPerEnemy) are asked of it: an enemy in the slick dosed four deep floats up (the ledger's
        /// ground-states). <see cref="InsideSpares"/> leaves out the enemy the ground was laid for, which its mark already answered.
        /// </summary>
        public UpgradeEffect[] InsideDo;
        public bool InsideSpares;
        /// <summary>
        /// For a Hazard: effects done on each enemy standing in the ground as its time runs out, as that enemy's own (the ledger's
        /// ground-flare: Coal Seam flaring every burning enemy on it).
        /// </summary>
        public UpgradeEffect[] InsideAtEnd;
        /// <summary>
        /// For a Hazard: effects done for you at each bite while you stand in the ground (the ledger's in-own-ground: The Wreck's
        /// heavy blows unbroken inside it). Done with no enemy named, at your feet.
        /// </summary>
        public UpgradeEffect[] YouInside;
        /// <summary>
        /// For a Hazard: the name its ground answers to. At each bite every enemy standing in it is told to the marks in force
        /// as <see cref="UpgradeTrigger.InGround"/>, and those whose <see cref="Condition"/> names this ground answer (Seen in the
        /// Water on the net of light).
        /// </summary>
        public string Ground = "";
        /// <summary>
        /// Carries what stands in it along its way, away from the player: for a Hazard, metres a second at each bite (Rip Current's
        /// foam); for a Lane, metres once, everything on the lane together (Hot Wind's gust). The ledger's lane-shove.
        /// </summary>
        public float Carry;
        /// <summary>
        /// For a Burst: above zero, it breaks up only along this many seams running out from its middle like the arms of a star,
        /// each as long as its Radius (GroundRules.SeamHalf either side): only what stands on a seam is struck, not a disc (the
        /// ledger's seams: Earthlight).
        /// </summary>
        public int Seams;
        /// <summary>For a Hazard: the ground goes with the enemy it was laid on (Heavy Silver's mercury pooling under it as it walks).</summary>
        public bool Follows;
        /// <summary>For a Hazard: its <see cref="InsideDo"/> is done only on those that moved since its last bite; one standing still is spared (Step Current).</summary>
        public bool InsideMovingOnly;
        /// <summary>
        /// For a Hazard: above zero, it does nothing until an enemy first stands in it, then cracks once, striking all within this
        /// many metres with its blow, and is gone (Fire-Cracked's stones).
        /// </summary>
        public float CracksAt;
        /// <summary>
        /// For SetOff: the discharge runs through every patch of ground of this name (UpgradeEffect.Ground) standing now, striking
        /// all in each in full, not a circle round its enemy; with none standing, only round its enemy (Grounded: live water).
        /// </summary>
        public string ThroughGround = "";
        /// <summary>For a Cone: it opens behind you, the way you face turned about, and you do not turn (Wings Behind's half ring of feathers).</summary>
        public bool Behind;
        /// <summary>For a Push along the line (Shape Downwind): thrown sideways across your front, to your left, along a sweep from your right (Sweep of Wings), not away from you.</summary>
        public bool Sideways;
        /// <summary>
        /// For Afflict with a Radius, or Hazard with Shape Line, on a dodge's end: along the path the dodge just took, from where it
        /// began to where it ended, not round you (Seed on the Wind, Wake; the ledger's dodge-path).
        /// </summary>
        public bool AlongDodge;
        /// <summary>For OwnGround or Hazard with Shape Line: the lane lies across your front, Amount metres ahead, not ahead of you (The Drawn Line).</summary>
        public bool AcrossFront;
        /// <summary>It goes off only once in each fight (the FightClock's), not once in so many seconds (The Burning Tree's first lily).</summary>
        public bool OncePerFight;
        /// <summary>For BurstGround: the Hazard effect whose standing patches it bursts.</summary>
        public UpgradeEffect BurstsGround;

        /// <summary>True when a Hazard effect lays ground that keeps acting, told to the marks at each bite (Hazards.GroundActs).</summary>
        public bool ActsAsGround => Inside != null || Mends > 0f || InsideDo != null || InsideAtEnd != null || YouInside != null || !string.IsNullOrEmpty(Ground);

        /// <summary>A copy with every number, its slot and its mark (an Empower's twin of its Learn's effect).</summary>
        internal UpgradeEffect Twin() => (UpgradeEffect)MemberwiseClone();

        /// <summary>Its place among all the effects in the catalogue; set when the catalogue is gathered.</summary>
        public int Slot { get; internal set; } = -1;
        /// <summary>The mark this effect belongs to. Set when the catalogue is gathered.</summary>
        public Upgrade Owner { get; internal set; }
    }
}
