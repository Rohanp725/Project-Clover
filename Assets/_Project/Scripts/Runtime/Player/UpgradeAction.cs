namespace Clover.Player
{
    /// <summary>
    /// What an upgrade's effect does when its trigger comes. The fields of <see cref="UpgradeEffect"/> each one reads
    /// are listed with it; a field left at zero takes the value in brackets.
    /// </summary>
    public enum UpgradeAction
    {
        /// <summary>Lays stacks of an affliction on the target, or with a Radius on everything within it. Reads: Affliction, Stacks (1), Radius (0), At.</summary>
        Afflict,
        /// <summary>A burst of damage and stagger in a circle. Reads: Damage, Stagger, Radius (2.5), Delay, Affliction, Stacks, At.</summary>
        Burst,
        /// <summary>A cone thrown forward from the player, who turns to the target first. Reads: Damage, Stagger, Reach (4.5), Arc (90), Affliction, Stacks; Amount: each enemy struck thrown that many metres straight away from the player.</summary>
        Cone,
        /// <summary>Ground that bites what stands on it, for a while. Reads: Shape, Damage (each bite), Stagger, Radius (1) of each patch, Seconds (3), Interval (0.5) between bites, Count (4) patches for a line or ring, Reach (4): the line's length or the ring's radius, Delay, Affliction, Stacks, At.</summary>
        Hazard,
        /// <summary>An arc that strikes the target and leaps on to others near. Reads: Damage, Stagger, Count (2) leaps, Reach (5) of each leap, Affliction, Stacks; LeapsTo or FromTarget: it starts from the target without striking it.</summary>
        Arc,
        /// <summary>Something thrown that strikes each enemy it passes once. Reads: Flight, Damage, Stagger, Radius (0.9), Speed (10), Seconds (1.2) in the air, Count (1) thrown in a fan, Affliction, Stacks.</summary>
        Projectile,
        /// <summary>A blade or pillar that falls after a delay, on a spot marked on the ground. Reads: Damage, Stagger, Radius (2), Delay (0.6), Count (1); more than one fall round the first, Affliction, Stacks, At.</summary>
        Falling,
        /// <summary>Heals the player. Reads: Amount.</summary>
        Heal,
        /// <summary>Gives mana. Reads: Amount.</summary>
        Mana,
        /// <summary>Gives stamina. Reads: Amount.</summary>
        Stamina,
        /// <summary>A timed change to one of the player's numbers. Reads: Stat, Amount, Seconds (5).</summary>
        Boon,
        /// <summary>A ward that turns aside the next blow that would land. Reads: Count (1) wards given; no more than three are held.</summary>
        Ward,
        /// <summary>A short step through the air to the target. Reads: Reach (8), the furthest it goes; Amount: metres beyond the target to land (0 stops in front of it).</summary>
        Step,
        /// <summary>Drags enemies toward a point. Reads: Radius (5), Amount (3) metres, At. Shape Downwind: the target alone, toward the player.</summary>
        Pull,
        /// <summary>Throws enemies away from a point. Reads: Radius (5), Amount (3) metres, At. Shape Downwind: the target alone, straight away from the player (Thunder Gust).</summary>
        Push,
        /// <summary>Slows time for everything but the player. Reads: Seconds (2), Amount (0.35): the share of its speed the world keeps.</summary>
        SlowTime,
        /// <summary>A copy of the player is left where they stand and repeats the last blow a moment later. Reads: Amount (0.6): the share of that blow's damage; Delay (0.3), Radius (2.3), Stagger.</summary>
        Echo,
        /// <summary>A helper that strikes the target a few times and goes. Reads: Damage, Stagger, Count (3) strikes, Interval (0.6), Radius (2), Affliction, Stacks.</summary>
        Helper,
        /// <summary>Sets an affliction on the target off: its stacks are spent for a burst round it. Reads: Affliction (the element's own), Damage: added for each stack spent, Stagger for each stack, Radius (3).</summary>
        SetOff,
        /// <summary>Holds the target still: it cannot move, turn or begin an attack. Elites and gods are held for 0.4 of the time, the colossus not at all, as with Frozen. Reads: Seconds (1.5), Radius (0: the target alone), At.</summary>
        Hold,
        /// <summary>Adds to the count its own mark keeps (<see cref="Upgrade.Tally"/>), up to the cap. Reads: Count (1).</summary>
        Bank,
        /// <summary>Fills part of a flask; a whole one is added to the belt if there is room. Reads: Amount: the share of a flask (0.25).</summary>
        FlaskFill,
        /// <summary>Takes health from the player and leaves it grey, to be won back by blows. Never the last point. Reads: Amount.</summary>
        SpendHealth,
        /// <summary>Heals the player by a share of the damage of the last weapon blow. Reads: Amount: the share (0.1).</summary>
        Leech,
        /// <summary>Throws the target into the air: it hangs helpless and comes down, and blows hold it up a little longer. Elites and gods hang 0.6 of the time; what cannot be lifted reels instead and takes the Stagger. Reads: Seconds (1), Radius (0: the target alone), Stagger (40) for what only reels, At.</summary>
        Lift,
        /// <summary>Drives an enemy that is in the air into the ground at once. Reads: nothing.</summary>
        Drop,
        /// <summary>Bursts everything the player has grown, each where it stands, and heals for each. Reads: Damage, Stagger, Radius (3), Amount: health for each burst, Affliction, Stacks.</summary>
        BurstGrown,
        /// <summary>Casts a spell form by name (<see cref="UpgradeEffect.Form"/>) with the numbers in <see cref="UpgradeEffect.Spell"/>, at the trigger's enemy when it has one: how a god's gift or a crown is begun. Costs nothing.</summary>
        Form,
        /// <summary>Makes poison bite at once, out of turn: on the target, or with a Radius on everyone within it. Reads: Radius (0), Amount: health for each one bitten, At.</summary>
        Bite,
        /// <summary>Lays stacks on the nearest other enemy within reach of the target, and nothing on the target itself: a dose that leaps. Reads: Affliction, Stacks (1), Reach (4; longer with LeapReach).</summary>
        Pass,
        /// <summary>Sets a pair's reaction off on the target now, through its rest, when the target carries both. Reads: Reaction (the reaction's id).</summary>
        ForceReaction,
        /// <summary>Makes the target flinch: what it had begun is broken, unless it is steadfast. Reads: Seconds (0.4).</summary>
        Flinch,
        /// <summary>Sets an enemy condition (<see cref="Clover.Gameplay.EnemyConditions"/>) on the target, or with a Radius on the enemies near it (State.ToOthers: not on the target, each joined to it). Reads: State, Radius, Count (all within the radius), Affliction and Stacks laid on each that takes it.</summary>
        Condition,
        /// <summary>Takes stacks of an affliction off the target with no burst; the next effects of the same mark may count them (DamagePerSpent, PerStack). Reads: Affliction, Share (0: all; else that share, rounded up), Count (the most), CountOnly (count them and take none).</summary>
        Spend,
        /// <summary>
        /// A strike that runs down a lane from the target straight away from the player, patch by patch, as one blow: each
        /// enemy on it is struck once. Reads: Damage, Stagger, Affliction, Stacks, Reach (8) its length, Radius (0.6) half
        /// its width, Interval (0.03) seconds from one patch to the next, Delay; Reaction: the reaction whose designed
        /// look's bolt is drawn down it.
        /// </summary>
        Lane,
        /// <summary>Takes seconds off every running spell and strike clock, as a landed blow does (IMPLEMENTATION.md 3.1). Reads: Amount (seconds).</summary>
        CutCooldowns,
        /// <summary>Ends the clock of a branch's slot at once: its own branch's, or the one named in Branch. Nothing when that spell sits in no slot. Reads: Branch.</summary>
        ResetCooldown,
        /// <summary>The next cast of a branch's slot starts no clock (its own branch's, or Branch's), within Seconds (0: until it is cast). Reads: Branch, Seconds.</summary>
        InstantNext,
        /// <summary>Keeps the target frozen Seconds longer; nothing on one that is not frozen. Reads: Seconds.</summary>
        KeepFrozen,
        /// <summary>Raises the player's Heat by levels, while Heat is learned. Reads: Count (1).</summary>
        RaiseHeat,
        /// <summary>Turns the player's tide at once, high to low or low to high, while a mark keeps the tide. Reads: nothing.</summary>
        TurnTide,
        /// <summary>Makes the surge that burns last longer. Reads: Amount (seconds added), Seconds (the most it may last in all; 0 for no cap).</summary>
        LengthenSurge,
        /// <summary>Empties the count its own mark keeps (a spell's first press starting its tally again). Reads: nothing.</summary>
        ClearCount,
        /// <summary>A short step straight away from the target, through nothing solid. Reads: Reach (3) metres.</summary>
        StepBack,
        /// <summary>No blow can land on the player for a moment. Reads: Seconds (0.3).</summary>
        Untouchable,
        /// <summary>
        /// The player and the target change places: the target goes where the player stood as the spell was cast, the player
        /// where the target stood. An elite or a god is not moved: the player comes up behind it instead. Reads: nothing.
        /// </summary>
        SwapPlaces,
        /// <summary>The target keeps the affliction until it dies: it no longer fades (Stone Does Not Forgive). Reads: Affliction.</summary>
        KeepForGood,
        /// <summary>Freezes the target at once for Seconds, whatever its Chill (Glass Spear); Amount above 1 makes it shatter that much harder (Hard Water). Reads: Seconds (1.5), Amount.</summary>
        Freeze,
        /// <summary>
        /// Banks a share of what the trigger carried (the damage a poison bite or set-off really dealt) as health given back
        /// over about two seconds (Venom). Reads: Share (0.1).
        /// </summary>
        Mend,
        /// <summary>
        /// The condition named in Condition on the target (by its name or by what it is) takes a heavy weapon blow now, as
        /// if one landed on it: its answers to a heavy blow are done, and one a heavy blow spends ends (Hot Stone, Cold
        /// Water: a perfect guard splits the crack, in whichever shape cracked it). The enemy itself takes nothing. Reads:
        /// Condition.
        /// </summary>
        StrikeCondition,
        /// <summary>
        /// Shatters the target if it is frozen, as a heavy blow would: the breaking's damage, plus Damage and Stagger, and the
        /// shatter told to whatever answers one (Brittle to the Core). Nothing on one that is not frozen. Reads: Damage, Stagger.
        /// </summary>
        Shatter,
        /// <summary>Clears every affliction laid on the player (High Noon). Reads nothing.</summary>
        Cleanse,
        /// <summary>
        /// Raises the target, and every enemy the same cast struck before it, to the depth of Affliction the sickest of them
        /// carries, Stacks at least (Share Alike: every one passed raised to the sickest one's depth). Reads: Affliction, Stacks.
        /// </summary>
        EvenDepth,
        /// <summary>
        /// Loads the player's edge for its next Count weapon blows on any enemy (light ones only with LightOnly): each lays
        /// Stacks of Affliction on what it strikes, or with SetsDepth raises it to the depth the target carries now
        /// (Carried on the Blade, Dust on the Blade). Reads: Affliction, Stacks, Count (3), Seconds (0: until spent), SetsDepth, LightOnly.
        /// </summary>
        CoatBlade,
        /// <summary>
        /// Every enemy within Radius (10) of the player that carries Affliction gives all of it up, gathered for the next
        /// set-off of this cast that carries doses on (<see cref="UpgradeEffect.PassesOn"/>), with no limit (All Into One).
        /// Reads: Affliction, Radius.
        /// </summary>
        Gather,
        /// <summary>
        /// The reaction whose moment this answers strikes again on the target: its own striking effects re-run at Amount
        /// (0.5) of their damage after Delay, laying and setting nothing (Strikes Twice: half, 0.5 s later where it happened;
        /// Spreading Water: on each other Soaked enemy near, with EachWithin). With Reaction named, that reaction, in the shape
        /// worn and past its rest, where the trigger's enemy is or fell (Kettle: steam at full where a Soaked enemy died
        /// burning). Reads: Amount, Delay, Reaction.
        /// </summary>
        EchoReaction,
        /// <summary>
        /// Done as the reaction named in Reaction fires, not after: on an enemy that meets Needs (NeedsFull, NeedsAtLeast)
        /// then, the reaction's own damage is multiplied by Amount, and with SparesSpend it spends nothing (Goes Up; the
        /// ledger's reaction-numbers-when). Nothing is done at its own moment. Reads: Reaction, Needs, Amount, SparesSpend.
        /// </summary>
        BoostReaction,
        /// <summary>
        /// Ends the condition named in Condition (by its name or what it is) on the target now, and its end is answered at
        /// once as when its time runs out (Cold Snap: the running Sleet Charge strikes; the ledger's condition-ends-now).
        /// Reads: Condition.
        /// </summary>
        EndCondition,
        /// <summary>
        /// Counts as a perfect dodge of the enemy blow about to land on the player within Seconds (0.3): what a perfect dodge
        /// gives, its marks answered, and the blow missed (the shared movement rule). Nothing when none is about to land.
        /// Reads: Seconds.
        /// </summary>
        PerfectDodgeNow,
        /// <summary>
        /// Drinks every cloud and trail of Affliction within Radius (6) of the player and half of each enemy's doses there into
        /// a store, then lays the store on the enemies this cast struck, split evenly among them and rounded up (The Yellow
        /// Gale, the owner's answer of 2026-11-22). Reads: Affliction, Radius.
        /// </summary>
        ShareStore,
        /// <summary>
        /// The player's next light weapon blow within Seconds (1) lands as a heavy blow: the weapon's heavy stagger, and every
        /// mark that answers a heavy blow answers it (Gone to Ash: the first blow behind the attacker; the ledger's
        /// blow-made-heavy). Reads: Seconds.
        /// </summary>
        NextHeavy,
        /// <summary>
        /// Lights the worn surge for Seconds at no cost, when it is the surge of the element named in SurgeOf (Sky Dance's
        /// Master: Tailwind for 4 s; the ledger's surge-from-mark). A surge already lit is kept at least that long. Reads:
        /// SurgeOf, Seconds.
        /// </summary>
        FreeSurge,
        /// <summary>
        /// Keeps only Amount (0.5) of its own spell's running wait, as if it had been that long from the start (Whirlpool's
        /// Master: its cooldown halved; Blown Through: half its own wait back; the ledger's own-clock-share). Reads: Amount.
        /// </summary>
        KeepWaitShare,
        /// <summary>
        /// Steps the player back in a straight line to where they stood as the spell was cast, cutting what stands on the way for
        /// Damage and Stagger in a lane Radius (0.6) wide either side, laying Affliction (Aruna's Road: back along the road; There
        /// and Back: the flight back, cutting the line again). Reads: Damage, Stagger, Radius, Affliction, Stacks.
        /// </summary>
        ReturnToCast,
        /// <summary>
        /// Tends the grown thing of the player's nearest the effect's place within its Radius (the ledger's at-grown): it stands
        /// Seconds longer, lays <see cref="UpgradeEffect.BitesLay"/> with every bite when that names one, and flares where it
        /// stands when the effect has Damage, Stagger or Stacks (Reach: the flare's size). Nothing when nothing grown is that near.
        /// </summary>
        Tend,
        /// <summary>
        /// Every grown thing of the player's within Reach of the target, Count at most, throws a bolt into it (Damage,
        /// Stagger, Affliction and Stacks); each bolt is a moment of its own (<see cref="UpgradeTrigger.GrownBolt"/>; the ledger's grown-sources).
        /// </summary>
        GrownBolts,
        /// <summary>
        /// Asked as its own branch's spell is cast, before the form runs, with the effect's CastAfter window: when it holds, the
        /// spell lands with no stillness after it (its Delay set to nothing: Seven Flashes' Master, cast in a perfect dodge's
        /// moment, does not stand still and its cuts land at once). Nothing is done when its moment comes.
        /// </summary>
        Quicken,
        /// <summary>Looses the spell its own branch began and gathered (SpellSpec.Rings) now, with the rings it holds: Daybreak's second press.</summary>
        Loose,
        /// <summary>
        /// Each enemy the last wave brought in (Riptide: PlayerUpgrades.NoteBrought) is carried on past the player along the
        /// wave's way, to Reach metres beyond them (4), and given the effect's State (backs turned): Riptide's Master, pressed again.
        /// </summary>
        CarryOn,
        /// <summary>
        /// Looses the flood of its own branch's ebb (SpellSpec.Ebb) now: the spell's form cast from where the player stands back
        /// toward where they began the ebb, reaching on past it by the spell's Reach less the ebb (The Tide Turns' second press).
        /// </summary>
        Flood,
        /// <summary>Ends its own branch's gathered spell where it is, loosing nothing (Bag of Winds' Master: the bag burst as a whirlwind instead).</summary>
        Spill,
        /// <summary>Throws the base spell of its mark's element at no cost, the way the player faces, as Storm Front's Gust is (Loose Neck: each wind a free Gust).</summary>
        FreeBase,
        /// <summary>The player and the copy their spell left standing change places (Second Shadow's second press). Nothing when none stands.</summary>
        SwapWithShadow,
        /// <summary>
        /// The player sinks into the copy their spell left standing and rises where it lies, the copy gone, and every wound taken
        /// since it was cast comes back as grey health to win back (Return to the Shadow; the ledger's wounds-returned).
        /// </summary>
        ReturnToShadow,
        /// <summary>The health the last blow that landed on the player took, given back, when it landed within CastWindow (1) seconds (Second Shadow's Master).</summary>
        UndoWound,
        /// <summary>
        /// Steps the player in a flash to the thing its own branch's spell left standing (Lightning Rod's rod), with a shock round
        /// where they land (Radius 2.5, Damage, Stagger, Affliction, Stacks), and the thing is spent, unless a KeepStanding of the
        /// same branch is in force. Nothing when none stands.
        /// </summary>
        StepToStanding,
        /// <summary>A mark of a branch whose StepToStanding leaves its thing standing (Lightning Rod's Master). Nothing is done when its moment comes.</summary>
        KeepStanding,
        /// <summary>
        /// Wins back Amount grey health at once, as blows do; with Seconds, for that long each weapon blow landed within Reach of
        /// something its mark's effects of the same Kin grew wins it back (Corpse Flowers: 3 a blow within 3 m of a flower, 8 s).
        /// </summary>
        Regain,
        /// <summary>Drives every blade of yours standing in the target at once (PlantedBlades; Flung Blade's Master on the attacker).</summary>
        DriveBlades,
        /// <summary>
        /// Throws Count more of the element's own blades, each into a different enemy within Reach of you, never the one faced
        /// nor one already holding a blade (Cherry Red's fan). Each is planted and driven as the kit's own blade is.
        /// </summary>
        PlantBlades,
        /// <summary>
        /// Calls every blade of yours home at once (All Blades Home on the surge): each strikes every enemy on its line to you
        /// for Damage, Stagger and the affliction, and each adds Seconds to the surge.
        /// </summary>
        CallBladesHome,
        /// <summary>
        /// Raises a standing thing (World.StandingThings): UpgradeEffect.Raises, at UpgradeEffect.RaiseAt Reach metres from the
        /// enemy named (or ahead of you), for Seconds, with Amount health, bursting Delay seconds after it is first struck when
        /// set, drawing enemies within Draws onto it, and Limit of its mark's at once (the oldest crumbling).
        /// </summary>
        Raise,
        /// <summary>Breaks the standing thing of its own mark nearest where the moment was (Raised Stone shattered by a heavy blow).</summary>
        BreakStanding,
        /// <summary>
        /// Pieces thrown out of the enemy the moment names, each its own striker that ends at the first it strikes and never
        /// strikes the enemy it left (UpgradeEffect.Thrown: a fan of Count over Arc, a ring, one at each of the Count nearest
        /// others within Reach, or a cone Reach long and Arc wide out of its far side). Each lays BitesLay as well.
        /// </summary>
        Pieces,
        /// <summary>
        /// Drops pickups on the floor (World.Pickups, UpgradeEffect.Drops): one at the place RaiseAt names, Reach metres off, or
        /// Count of them laid as a ring's patches are (Shape, PerStack); each lies Seconds (0: 8) and is taken by walking over
        /// it, Radius (0: 1 m); Limit of its mark at once. Its taking and its being left are UpgradeTrigger.PickedUp and PickupLeft.
        /// </summary>
        DropPickup,
        /// <summary>Draws every pickup of the kind Drops names across the floor to you (The Whole Mountain).</summary>
        DrawPickups,
        /// <summary>Your next weapon blow within Seconds is critical, light or heavy (a Shining Herb picked).</summary>
        CriticalNext,
        /// <summary>The spell of its own branch now standing lasts Seconds longer (Uraeus' Master: the cobra a second longer for each spit on a burning enemy).</summary>
        LengthenOwn,
        /// <summary>Does nothing but play its UpgradeEffect.Look (a creature drawn by its recipe look as the spell is cast).</summary>
        PlayLook,
        /// <summary>
        /// Bursts every patch of ground the effect named by UpgradeEffect.BurstsGround has laid and still stands, all at once, each
        /// where it lies: Damage, Stagger and Stacks of the Affliction to all in each, and the patch is gone (Weeping Sore's
        /// puddles; the ledger's puddles-burst).
        /// </summary>
        BurstGround,
        /// <summary>For Seconds, nothing interrupts your heavy blow being wound up or swung (The Wreck: struck from inside it).</summary>
        HeavyUnbroken,
        /// <summary>
        /// Lays ground of the player's own by its name (Ground; World.OwnGrounds) for Seconds: Radius round where it is done, or
        /// with Shape Line a lane Reach long ahead of you, Radius either side (AcrossFront: across your front instead, Amount
        /// metres ahead). It bites nothing; the stats and marks ask who stands in it (The Closed Ring's stones, The Drawn Line).
        /// </summary>
        OwnGround,
        /// <summary>The dodge just begun costs no stamina: what it took is given back (The Land Was Sea's slide on wet sand).</summary>
        FreeDodge,
        /// <summary>
        /// You scatter for Seconds: the body is not drawn and nothing can touch you, then you stand again where you are by then
        /// (Gone to Ash: scattered as ash, standing again 0.4 s later, behind the attacker or where you were).
        /// </summary>
        Scatter,
    }
}
