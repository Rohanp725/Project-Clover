using System.Collections.Generic;
using Clover.Core;
using Clover.Gameplay;
using Clover.World;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// What the player has learned from the chart, and the embers to learn more. Embers are taken from the fallen and
    /// spent at a rest fire; the run remembers both from scene to scene. The weapon, the guard, the dodge and the
    /// element each tell this when something happens (a blow lands, a guard is perfect, a dodge begins), and this does
    /// whatever the learned upgrades add to it.
    ///
    /// Most upgrades are data (<see cref="Upgrade.Stats"/>, <see cref="Upgrade.Effects"/>, <see cref="Upgrade.Form"/>)
    /// and are run here by one general runner: the stats are summed into <see cref="Stats"/> and pushed into the
    /// systems that use them, the effects are sorted by trigger and done when their trigger comes, and a learned spell
    /// is worn in the hold-spell slot. The first sixteen upgrades of Fire and Lightning are done by hand, by id, and
    /// their numbers are fields here.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerUpgrades : MonoBehaviour
    {
        /// <summary>Upgrades the player has from the start of the scene, for trying things out.</summary>
        public List<string> StartWith = new List<string>();
        /// <summary>Embers the player has from the start of the scene, for trying things out.</summary>
        public int StartingPoints;

        public Heat Heat = new Heat();

        [Header("Fire")]
        public float BackdraftReach = 4.5f;
        public float BackdraftArc = 100f;
        public float BackdraftDamage = 16f;
        public float BackdraftStagger = 18f;
        public float TrailDamage = 4f;
        public float TrailSeconds = 2.6f;
        public float CrescentDamage = 18f;
        public float CrescentSpeed = 8f;
        public float CrescentSeconds = 1.1f;
        public float FirebombRange = 9f;
        public float FirebombDamage = 36f;
        public float FirebombStagger = 30f;
        public float FirebombRadius = 2.6f;

        [Header("Lightning")]
        public float StaticDamagePerStack = 7f;
        public float StaticStaggerPerStack = 5f;
        public float StaticRadius = 4f;
        public float StoredChargeMultiplier = 2f;
        public float BlinkDamage = 10f;
        public float BlinkStagger = 14f;
        public float AfterimageDamage = 26f;
        public float AfterimageStagger = 28f;
        public float AfterimageDelay = 0.3f;
        public float DelayedCutsShare = 0.6f;
        public float DelayedCutsDelay = 0.55f;
        public float OverclockSeconds = 2.5f;
        public float OverclockScale = 0.35f;
        public float PlasmaDamage = 18f;
        public float PlasmaStagger = 16f;
        public float PlasmaRadius = 3.2f;
        /// <summary>Seconds the shape left behind by a lightning dodge stays.</summary>
        public float AfterimageSeconds = 0.45f;

        [Header("The runner")]
        /// <summary>Metres within which an enemy counts as "in a fight" for effects that come every few seconds.</summary>
        public float FightRange = 12f;
        /// <summary>The share of health under which the low-health trigger comes.</summary>
        public float LowHealthShare = 1f / 3f;
        /// <summary>Seconds without a wound before the player counts as unhurt, and without a dodge before they count as planted.</summary>
        public float UnhurtSeconds = 8f;
        public float PlantedSeconds = 3f;
        /// <summary>The most weapon damage grey health held can add, where a mark makes it add any.</summary>
        public float GreyStrengthCap = 0.3f;
        /// <summary>White Heat: swing speed for each level of Heat above five, and what burns the player each second for four seconds when struck up there.</summary>
        public float WhiteHeatSwing = 0.05f;
        public float WhiteHeatBurn = 4f;
        public float WhiteHeatBurnSeconds = 4f;

        /// <summary>The player's numbers as the learned upgrades, the surge and timed boons make them.</summary>
        public PlayerStats Stats { get; } = new PlayerStats();
        public int Points => RunState.Points;
        public int Bought { get; private set; }
        /// <summary>The learned spell worn in the hold-spell slot, or null.</summary>
        public Upgrade Worn { get; private set; }
        /// <summary>True when holding the spell button does something of its own: a spell is worn.</summary>
        public bool HasSpecial => Worn != null;
        /// <summary>A spell lent from outside the trees, to be tried: while one is lent it is the spell worn.</summary>
        public Upgrade Lent { get; private set; }
        /// <summary>True while a perfect guard's charge waits on the next blow.</summary>
        public bool ChargeStored => combat != null && combat.NextBlowDamage > 1f;
        public bool ShowsHeat => hasHeat;
        // Counters so tests and probes can see what happened without watching the screen.
        public int Detonations { get; private set; }
        public int Discharges { get; private set; }
        public int Leaps { get; private set; }
        public int FlasksThrown { get; private set; }
        public int PlasmaBursts { get; private set; }
        public int FreeDodges { get; private set; }
        /// <summary>How many times a worn spell has been cast, and how many data-driven effects have been done.</summary>
        public int WornCasts { get; private set; }
        public int EffectsDone { get; private set; }
        /// <summary>How many enemies the Hold action has taken hold of.</summary>
        public int Holds { get; private set; }
        /// <summary>How many enemies the Lift action has thrown into the air.</summary>
        public int Lifts { get; private set; }
        /// <summary>Goes up whenever a count kept by a mark changes; the HUD writes <see cref="TallyText"/> again then.</summary>
        public int TallyVersion { get; private set; }
        /// <summary>The counts kept by the marks in force, as the HUD shows them: "Plates 2/3".</summary>
        public string TallyText { get; private set; } = "";

        private enum Special { None, DeathBurst, Carry }

        private struct Pending
        {
            public UpgradeEffect Effect;
            public Special Special;
            public Component Target;
            public Vector3 At;
            public bool HasAt;
            public float Power;
            public float Radius;
            public AfflictionKind Kind;
            public int Stacks;
            // What the trigger carried: stacks spent or carried by the dead, or points of a count spent.
            public int Spent;
            // Stacks the root already lays with the same blow, not to be laid twice.
            public int Less;
        }

        private const int QueueCapacity = 64;
        private static readonly int TriggerCount = System.Enum.GetValues(typeof(UpgradeTrigger)).Length;
        // Each affliction belongs to one element; an effect that lays another element's affliction is seen in that element's way.
        private static readonly ElementInfo[] lookOf = LooksByAffliction();

        private PlayerController player;
        private PlayerElement element;
        private PlayerCombat combat;
        private PlayerDefence defence;
        private TopDownCamera shaker;
        private HitStop hitStop;
        private Afterimage afterimage;
        private IReadOnlyList<Upgrade> catalogue;
        private bool[] has;
        private float[] readyAt;
        private float[] sinceLast;
        private List<UpgradeEffect>[] byTrigger;
        private readonly Pending[] queue = new Pending[QueueCapacity];
        private int queued;
        private bool flushing;
        private readonly List<IDamageable> toDischarge = new List<IDamageable>();
        private readonly List<IDamageable> toBurst = new List<IDamageable>();
        private bool hasHeat, hasDetonate, hasBackdraft, hasTrail, hasCrescent, hasFirebomb;
        private bool hasStatic, hasCharge, hasBlink, hasAfterimage, hasCuts, hasOverclock, hasStormStep, hasPlasma;
        private float untilRefresh;
        private Vector3 dodgeFrom;
        private ISpellLanding landingForm;
        private SpellCast landingCast;
        private int appliedVersion = -1;
        private bool appliedSurge;
        private bool inFight;
        private float untilFightLook;
        private bool wasLow, wasFull;
        private float emberCarry;
        // The numbers the stats multiply, as the scene set them.
        private bool basesRead;
        private float baseDodgeDistance, baseDodgeSafe, baseDodgeCost, baseDodgeWindow, baseGuardCost, baseGuardWindow;
        private float baseHealth, baseFlaskHeal, baseGuardMana, baseDodgeMana, baseGreyFade, baseMoveSpeed;
        private int baseFlasks;
        private float sinceStruck = 99f, sinceDodge = 99f;
        private float attackStamina = 1f;
        // The counts marks keep, by the mark's place in the catalogue, and which of the marks in force keep one.
        private int[] tally = new int[0];
        private readonly List<int> tallied = new List<int>(8);
        private bool tallyDirty;
        private bool hasRoot;
        private Upgrade castSource;
        private bool hasWhiteHeat;
        private int baseHeatLevels, baseHeatBlows;
        private float baseHeatCool;
        private float selfBurnLeft, selfBurnOwed;
        private readonly System.Text.StringBuilder tallyWords = new System.Text.StringBuilder(64);

        private static ElementInfo[] LooksByAffliction()
        {
            var looks = new ElementInfo[Afflictions.Count];
            foreach (var info in Elements.All) looks[(int)info.Affliction] = info;
            return looks;
        }

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            element = GetComponent<PlayerElement>();
            combat = GetComponent<PlayerCombat>();
            defence = GetComponent<PlayerDefence>();
            if (combat != null) combat.Stats = Stats;
            hitStop = GetComponent<HitStop>();
            afterimage = GetComponent<Afterimage>();
            byTrigger = new List<UpgradeEffect>[TriggerCount];
            for (int i = 0; i < byTrigger.Length; i++) byTrigger[i] = new List<UpgradeEffect>(8);
            CatalogueChanged();
            Heat.Reset();
            foreach (string id in StartWith) RunState.Unlocked.Add(id);
            RunState.Points += StartingPoints;
            Hazards.OwnedBy(combat);
        }

        private void OnEnable()
        {
            CombatEvents.Died += OnEnemyDied;
            CombatEvents.AfflictionFull += OnAfflictionFull;
            CombatEvents.Carried += OnCarried;
            CombatEvents.Shattered += OnShattered;
            CombatEvents.Landed += OnLanded;
            if (defence != null) defence.Revived += OnRevived;
        }

        private void OnDisable()
        {
            CombatEvents.Died -= OnEnemyDied;
            CombatEvents.AfflictionFull -= OnAfflictionFull;
            CombatEvents.Carried -= OnCarried;
            CombatEvents.Shattered -= OnShattered;
            CombatEvents.Landed -= OnLanded;
            if (defence != null) defence.Revived -= OnRevived;
        }

        private void Start()
        {
            var main = Camera.main;
            shaker = main != null ? main.GetComponent<TopDownCamera>() : null;
            ReadBases();
            Refresh(true);
            ApplyStats();
        }

        /// <summary>Looks at the catalogue again, as after a probe has added marks to it.</summary>
        public void CatalogueChanged()
        {
            catalogue = Upgrade.All;
            has = new bool[catalogue.Count];
            readyAt = new float[Upgrade.EffectCount];
            sinceLast = new float[Upgrade.EffectCount];
            tally = new int[catalogue.Count];
            if (player != null && basesRead) Refresh(true);
        }

        private void ReadBases()
        {
            basesRead = true;
            baseHeatLevels = Heat.MaximumLevel;
            baseHeatBlows = Heat.BlowsPerLevel;
            baseHeatCool = Heat.CoolAfter;
            baseDodgeDistance = player.Dodge.Distance;
            baseDodgeSafe = player.Dodge.InvulnerableUntil;
            baseDodgeCost = player.Dodge.StaminaCost;
            baseMoveSpeed = player.MoveSpeed;
            if (defence == null) return;
            baseDodgeWindow = defence.PerfectDodgeWindow;
            baseGuardCost = defence.Guard.StaminaPerDamage;
            baseGuardWindow = defence.Guard.PerfectWindow;
            baseHealth = defence.Health.Maximum;
            baseFlaskHeal = defence.Flasks.HealAmount;
            baseFlasks = defence.Flasks.Maximum;
            baseGuardMana = defence.PerfectGuardMana;
            baseDodgeMana = defence.PerfectDodgeMana;
            baseGreyFade = defence.Health.GreyFadePerSecond;
        }

        /// <summary>True when the upgrade is learned and belongs to the element and weapon the player holds now.</summary>
        public bool Has(string id)
        {
            if (!RunState.Unlocked.Contains(id)) return false;
            return InForce(Upgrade.ById(id));
        }

        private bool InForce(Upgrade upgrade)
        {
            if (upgrade == null) return false;
            if (element != null && element.Kit != null && !upgrade.BelongsTo(element.Kit.Name)) return false;
            // The god's sword counts as the Katana it is.
            if (!string.IsNullOrEmpty(upgrade.Weapon) && combat != null && combat.Weapon != null && !combat.Weapon.Name.Contains(upgrade.Weapon)) return false;
            return true;
        }

        public bool Owns(string id) => RunState.Unlocked.Contains(id);

        /// <summary>The name of the element whose tree the player can learn from now.</summary>
        public string ElementName => element != null && element.Kit != null ? element.Kit.Name : ElementKit.FireName;

        /// <summary>True when the mark is on a tree or branch open to the player now: the element held, or a mix of it with an element held or consumed in this run.</summary>
        public bool Open(Upgrade upgrade)
        {
            if (upgrade == null) return false;
            if (upgrade.IsMix) return UpgradeTree.MixOpen(upgrade, ElementName, RunState.ElementsHeld, RunState.ElementsConsumed);
            return upgrade.Element == ElementName;
        }

        public bool CanBuy(Upgrade upgrade) =>
            upgrade != null && !upgrade.Signature && !RunState.Unlocked.Contains(upgrade.Id) && RunState.Points >= upgrade.Cost
            && Open(upgrade) && UpgradeTree.Reachable(upgrade, RunState.Unlocked);

        public bool TryBuy(Upgrade upgrade)
        {
            if (!CanBuy(upgrade)) return false;
            RunState.Points -= upgrade.Cost;
            RunState.Unlocked.Add(upgrade.Id);
            Bought++;
            // The first spell learned is worn at once; later ones are worn by choosing them on the chart.
            if (upgrade.IsSpell && Worn == null) RunState.WornSpell = upgrade.Id;
            Refresh(true);
            return true;
        }

        /// <summary>
        /// A god's gift, or anything else given and not bought: it is learned at no cost, and a gift that is a spell is
        /// worn at once. False when there was nothing to give or it was already held.
        /// </summary>
        public bool Give(Upgrade gift)
        {
            if (gift == null || RunState.Unlocked.Contains(gift.Id)) return false;
            RunState.Unlocked.Add(gift.Id);
            if (gift.IsSpell) RunState.WornSpell = gift.Id;
            Refresh(true);
            return true;
        }

        /// <summary>Wears a learned spell in the hold-spell slot. False when it is not a spell the player has.</summary>
        public bool Wear(string id)
        {
            var upgrade = Upgrade.ById(id);
            if (upgrade == null || !upgrade.IsSpell || !Has(id)) return false;
            RunState.WornSpell = id;
            Worn = upgrade;
            return true;
        }

        /// <summary>
        /// Lends a spell that is on no tree, in the hold-spell slot, until another is lent or null is given: the yard's
        /// rack of forms uses it. Nothing about the run is written down.
        /// </summary>
        public void Lend(Upgrade spell)
        {
            Lent = spell != null && spell.Spell != null ? spell : null;
            Refresh(true);
        }

        /// <summary>Embers from the fallen; the upgrades may make them count for more.</summary>
        public void Earn(int points)
        {
            if (points <= 0) return;
            float worth = points * Stats.Multiplier(PlayerStat.EmbersEarned) + emberCarry;
            int whole = Mathf.FloorToInt(worth + 0.0001f);
            emberCarry = worth - whole;
            RunState.Points += whole;
        }

        /// <summary>Embers given back as they were spent, ember for ember.</summary>
        public void Refund(int points)
        {
            if (points > 0) RunState.Points += points;
        }

        /// <summary>Taking a new element leaves the old one's learning behind.</summary>
        public void Forget()
        {
            RunState.Unlocked.Clear();
            RunState.WornSpell = "";
            Heat.Reset();
            if (combat != null)
            {
                combat.NextBlowDamage = 1f;
                combat.NextBlowStagger = 1f;
            }
            Refresh(true);
        }

        // What is in force is worked out a few times a second, not asked for by name on every blow; the lists the
        // runner reads are only made again when something has changed.
        private void Refresh(bool force = false)
        {
            untilRefresh = 0.25f;
            bool changed = force;
            for (int i = 0; i < has.Length; i++)
            {
                bool now = RunState.Unlocked.Contains(catalogue[i].Id) && InForce(catalogue[i]);
                if (now == has[i]) continue;
                has[i] = now;
                changed = true;
            }
            if (!changed) return;

            for (int i = 0; i < byTrigger.Length; i++) byTrigger[i].Clear();
            Stats.ClearLearned();
            int weaponStacks = 0;
            Upgrade worn = null, firstSpell = null;
            tallied.Clear();
            hasRoot = false;
            for (int i = 0; i < has.Length; i++)
            {
                // A count is lost with the mark that kept it.
                if (!has[i])
                {
                    tally[i] = 0;
                    continue;
                }
                var upgrade = catalogue[i];
                if (upgrade.Kind == UpgradeKind.Root) hasRoot = true;
                if (upgrade.Tally != null)
                {
                    tallied.Add(i);
                    // A count kept for the run comes back with the scene.
                    if (upgrade.Tally.Kept && tally[i] == 0 && RunState.Tallies.TryGetValue(upgrade.Id, out int kept)) tally[i] = kept;
                }
                foreach (var mod in upgrade.Stats) Stats.AddLearned(mod);
                foreach (var effect in upgrade.Effects) byTrigger[(int)effect.On].Add(effect);
                weaponStacks += upgrade.WeaponStacks;
                if (!upgrade.IsSpell) continue;
                if (firstSpell == null) firstSpell = upgrade;
                if (upgrade.Id == RunState.WornSpell) worn = upgrade;
            }
            // A worn spell that is no longer the player's gives way to one that is.
            RunState.WornSpell = (worn ?? firstSpell) != null ? (worn ?? firstSpell).Id : "";
            Worn = Lent ?? worn ?? firstSpell;

            hasHeat = On(Upgrade.Heat);
            hasDetonate = On(Upgrade.BurnAndDetonate);
            hasBackdraft = On(Upgrade.Backdraft);
            hasTrail = On(Upgrade.FlameTrail);
            hasCrescent = On(Upgrade.BurningCrescent);
            hasFirebomb = On(Upgrade.FirebombFlask);
            hasStatic = On(Upgrade.Static);
            hasCharge = On(Upgrade.StoredCharge);
            hasBlink = On(Upgrade.Blink);
            hasAfterimage = On(Upgrade.Afterimage);
            hasCuts = On(Upgrade.DelayedCuts);
            hasOverclock = On(Upgrade.Overclock);
            hasStormStep = On(Upgrade.StormStep);
            hasPlasma = On(Upgrade.Plasma);
            hasWhiteHeat = On(Trees.FireTree.WhiteHeat);

            if (combat != null)
            {
                // The burn of a blow is Cinder's now (the root lays it as every root does), so nothing is laid by hand here.
                combat.BurnStacksOnHit = 0;
                combat.WeaponAffliction = element != null && element.Info != null ? element.Info.Affliction : AfflictionKind.None;
                combat.WeaponAfflictionStacks = weaponStacks;
            }
            // The flask can be thrown once anything answers its being thrown.
            if (defence != null) defence.HoldToThrow = hasFirebomb || byTrigger[(int)UpgradeTrigger.FlaskThrown].Count > 0;
            if (!hasHeat) Heat.Reset();
            ApplyTallies();
        }

        // The counts are few and change seldom: their words and what they add to the stats are made again when one moves.
        private void ApplyTallies()
        {
            tallyDirty = false;
            Stats.ClearTally();
            tallyWords.Length = 0;
            for (int i = 0; i < tallied.Count; i++)
            {
                var spec = catalogue[tallied[i]].Tally;
                int held = tally[tallied[i]];
                if (spec.PerPoint != 0f && held > 0) Stats.AddTally(spec.Stat, spec.PerPoint * held);
                if (spec.PerPoint2 != 0f && held > 0) Stats.AddTally(spec.Stat2, spec.PerPoint2 * held);
                if (spec.Kept) RunState.Tallies[catalogue[tallied[i]].Id] = held;
                tallyWords.Append("   ").Append(spec.Name).Append(' ').Append(held).Append('/').Append(spec.Cap);
            }
            TallyText = tallyWords.ToString();
            TallyVersion++;
        }

        /// <summary>The count a mark keeps now; zero when it keeps none or is not in force.</summary>
        public int Tally(string id)
        {
            var upgrade = Upgrade.ById(id);
            return upgrade != null && upgrade.Index < tally.Length ? tally[upgrade.Index] : 0;
        }

        private bool On(string id)
        {
            for (int i = 0; i < has.Length; i++)
                if (has[i] && catalogue[i].Id == id) return true;
            return false;
        }

        // The stats are pushed into the systems that use them, and only when a sum has changed.
        private void ApplyStats()
        {
            bool surging = element != null && element.Kit != null && element.Kit.SurgeActive;
            if (!basesRead || (Stats.Version == appliedVersion && surging == appliedSurge)) return;
            appliedVersion = Stats.Version;
            appliedSurge = surging;
            float surgeSpeed = surging ? Stats.Multiplier(PlayerStat.SurgeSpeed) : 1f;
            player.MoveSpeed = baseMoveSpeed * Stats.Multiplier(PlayerStat.MoveSpeed) * surgeSpeed;
            player.Dodge.Distance = baseDodgeDistance * Stats.Multiplier(PlayerStat.DodgeDistance);
            player.Dodge.InvulnerableUntil = baseDodgeSafe * Stats.Multiplier(PlayerStat.DodgeSafeTime);
            player.Dodge.StaminaCost = baseDodgeCost * Stats.Multiplier(PlayerStat.StaminaCost) * Stats.Multiplier(PlayerStat.DodgeStamina);
            attackStamina = Stats.Multiplier(PlayerStat.AttackStamina);
            Afflictions.MoreBlows = Mathf.Max(0, Stats.Whole(PlayerStat.MarkBlows));
            Hazards.GrownSecondsScale = Stats.Multiplier(PlayerStat.GrownSeconds);
            Hazards.GrownDamageScale = Stats.Multiplier(PlayerStat.GrownDamage);
            // Heat as the marks make it: sooner, slower to cool, higher.
            Heat.BlowsPerLevel = Mathf.Max(1, baseHeatBlows - Stats.Whole(PlayerStat.HeatBlows));
            Heat.CoolAfter = baseHeatCool * Stats.Multiplier(PlayerStat.HeatCooling);
            Heat.MaximumLevel = baseHeatLevels + Mathf.Max(0, Stats.Whole(PlayerStat.HeatCap));
            // The held element's affliction may lie deeper than usual.
            Afflictions.SetDeeper(element != null && element.Info != null ? element.Info.Affliction : AfflictionKind.None, Stats.Whole(PlayerStat.AfflictionDepth));
            if (combat != null)
            {
                combat.StatDamage = Stats.Multiplier(PlayerStat.WeaponDamage);
                combat.StatStagger = Stats.Multiplier(PlayerStat.StaggerDealt);
                combat.StatMana = Stats.Multiplier(PlayerStat.ManaFromBlows);
                combat.StatSwing = Stats.Multiplier(PlayerStat.SwingSpeed) * surgeSpeed;
                combat.StatAfflicted = Stats.Multiplier(PlayerStat.DamageToAfflicted);
                combat.CritChance = Mathf.Clamp01(Stats.Sum(PlayerStat.CritChance));
                combat.CritMultiplier = Mathf.Max(1f, 1.5f + Stats.Sum(PlayerStat.CritDamage));
                combat.ExtraStacks = Mathf.Max(0, Stats.Whole(PlayerStat.AfflictionStacks));
                combat.AfflictionSeconds = Stats.Multiplier(PlayerStat.AfflictionSeconds);
                combat.AfflictionPower = Stats.Multiplier(PlayerStat.AfflictionPower);
            }
            if (defence == null) return;
            defence.PerfectDodgeWindow = baseDodgeWindow * Stats.Multiplier(PlayerStat.DodgeSafeTime);
            defence.Guard.StaminaPerDamage = baseGuardCost * Stats.Multiplier(PlayerStat.StaminaCost) * Stats.Multiplier(PlayerStat.GuardStamina);
            defence.Flasks.EarnScale = Stats.Multiplier(PlayerStat.FlaskEarn);
            defence.Guard.PerfectWindow = baseGuardWindow * Stats.Multiplier(PlayerStat.PerfectGuardWindow);
            defence.Health.SetMaximum(Mathf.Max(1f, baseHealth + Stats.Sum(PlayerStat.Health)));
            defence.Flasks.HealAmount = baseFlaskHeal * Stats.Multiplier(PlayerStat.FlaskHeal);
            // A mark may take every flask away.
            defence.Flasks.SetMaximum(Mathf.Max(0, baseFlasks + Stats.Whole(PlayerStat.FlaskCount)));
            defence.DamageTaken = Stats.Multiplier(PlayerStat.DamageTaken);
            defence.PerfectGuardMana = baseGuardMana * Stats.Multiplier(PlayerStat.ManaFromGuards);
            defence.PerfectDodgeMana = baseDodgeMana * Stats.Multiplier(PlayerStat.ManaFromDodges);
            defence.Health.GreyFadePerSecond = baseGreyFade / Stats.Multiplier(PlayerStat.GreyKept);
        }

        private void Update()
        {
            untilRefresh -= Time.unscaledDeltaTime;
            if (untilRefresh <= 0f) Refresh();
            float deltaTime = Time.deltaTime;
            if (hasHeat) Heat.Tick(deltaTime);
            if (hasHeat && Heat.Level > baseHeatLevels)
            {
                // White heat: above the usual five, every level quickens the arm. Given again each frame, it lasts as long as the level does.
                Stats.AddBoon(PlayerStat.SwingSpeed, WhiteHeatSwing * (Heat.Level - baseHeatLevels), 0.25f);
            }
            if (selfBurnLeft > 0f && defence != null && !defence.IsDead)
            {
                // Struck in white heat, the fire is the player's own for a while. It never takes the last point.
                selfBurnLeft -= deltaTime;
                selfBurnOwed += WhiteHeatBurn * deltaTime;
                if (selfBurnOwed >= 1f)
                {
                    selfBurnOwed -= 1f;
                    if (defence.Health.Current > 1.5f) defence.Health.Damage(1f);
                    Vfx.Play(VfxKind.Fire, transform.position + Vector3.up * 1.1f, Vector3.up, 0.4f);
                }
            }
            if (combat != null)
            {
                float owed = Stats.Sum(PlayerStat.GreyStrength);
                // Health that is owed makes the arm stronger, for one who has learned to borrow.
                float borrowed = owed > 0f && defence != null ? Mathf.Min(GreyStrengthCap, defence.Health.Grey * owed) : 0f;
                combat.UpgradeDamage = (hasHeat ? Heat.DamageMultiplier : 1f) * (1f + borrowed);
                // The weapon in hand may have changed, and its chain with it.
                if (combat.Combo != null) combat.Combo.StaminaScale = attackStamina;
            }
            Stats.Tick(deltaTime);
            States(deltaTime);
            ApplyStats();
            Watch(deltaTime);
            Flush();
        }

        // The states a learned number may wait on. Each is a plain question asked every frame; nothing moves unless the answer changed.
        private void States(float deltaTime)
        {
            sinceStruck += deltaTime;
            sinceDodge += deltaTime;
            Stats.SetWhen(StatWhen.Surge, element != null && element.Kit != null && element.Kit.SurgeActive);
            Stats.SetWhen(StatWhen.Unhurt, sinceStruck >= UnhurtSeconds);
            Stats.SetWhen(StatWhen.Risen, Risen);
            Stats.SetWhen(StatWhen.Planted, sinceDodge >= Mathf.Max(0.5f, PlantedSeconds - Stats.Sum(PlayerStat.PlantedSooner)));
            Stats.SetWhen(StatWhen.ManaFull, combat != null && combat.Mana.Current >= combat.Mana.Maximum - 0.001f);
            Stats.SetWhen(StatWhen.NearGrown, Hazards.GrownNear(transform.position, 4f));
            Stats.SetWhen(StatWhen.FarFromGrown, !Hazards.GrownNear(transform.position, 7f));
            if (defence == null) return;
            Stats.SetWhen(StatWhen.Guarding, defence.IsGuarding);
            Stats.SetWhen(StatWhen.LowHealth, !defence.IsDead && defence.Health.Fraction < LowHealthShare);
            Stats.SetWhen(StatWhen.HalfHealth, !defence.IsDead && defence.Health.Fraction < 0.5f);
            Stats.SetWhen(StatWhen.GreyHeld, defence.Health.Grey > 0.5f);
            Stats.SetWhen(StatWhen.HighHealth, !defence.IsDead && defence.Health.Fraction >= 0.5f);
        }

        // The triggers that are not told to this by anyone: time passing in a fight, health falling low, mana filling.
        private void Watch(float deltaTime)
        {
            var timed = byTrigger[(int)UpgradeTrigger.EverySeconds];
            if (timed.Count > 0 && combat != null)
            {
                untilFightLook -= deltaTime;
                if (untilFightLook <= 0f)
                {
                    untilFightLook = 0.5f;
                    inFight = combat.NearestEnemy(FightRange) != null;
                }
                if (inFight)
                {
                    for (int i = 0; i < timed.Count; i++)
                    {
                        var effect = timed[i];
                        sinceLast[effect.Slot] += deltaTime;
                        if (sinceLast[effect.Slot] < Mathf.Max(0.25f, effect.Every)) continue;
                        sinceLast[effect.Slot] = 0f;
                        Consider(effect, null, transform.position, false);
                    }
                }
            }
            if (defence != null)
            {
                bool low = !defence.IsDead && defence.Health.Fraction < LowHealthShare;
                if (low && !wasLow) Trigger(UpgradeTrigger.LowHealth, null, transform.position, false);
                wasLow = low;
            }
            if (combat != null)
            {
                bool full = combat.Mana.Current >= combat.Mana.Maximum - 0.001f;
                if (full && !wasFull) Trigger(UpgradeTrigger.ManaFull, null, transform.position, false);
                wasFull = full;
            }
        }

        // ---- The runner: effects wait in a line and are done on this component's own step, never in the middle of the blow that caused them.

        private void Trigger(UpgradeTrigger trigger, Component target, Vector3 at, bool hasAt = true, int spent = 0, AfflictionSet dead = null, AfflictionKind what = AfflictionKind.None)
        {
            var list = byTrigger[(int)trigger];
            for (int i = 0; i < list.Count; i++)
            {
                var effect = list[i];
                if (effect.Spent != AfflictionKind.None && effect.Spent != what) continue;
                if (effect.OwnSpell && effect.Owner != castSource) continue;
                if (effect.SpentAtLeast > 0 && spent < effect.SpentAtLeast) continue;
                Consider(effect, target, at, hasAt, spent, dead);
            }
        }

        private static bool IsBlow(UpgradeTrigger trigger) =>
            trigger == UpgradeTrigger.AnyHit || trigger == UpgradeTrigger.LightHit || trigger == UpgradeTrigger.HeavyHit || trigger == UpgradeTrigger.FinisherHit
            || trigger == UpgradeTrigger.SkillHit || trigger == UpgradeTrigger.AfflictedHit || trigger == UpgradeTrigger.CritHit || trigger == UpgradeTrigger.ChainEnd;

        private void Consider(UpgradeEffect effect, Component target, Vector3 at, bool hasAt, int spent = 0, AfflictionSet dead = null)
        {
            if (effect.Slot < 0 || effect.Slot >= readyAt.Length) return;
            if (Time.time < readyAt[effect.Slot]) return;
            if (effect.WhileSurge && !(element != null && element.Kit.SurgeActive)) return;
            if (effect.While != StatWhen.Always && !Stats.Holds(effect.While)) return;
            if (effect.NeedsAirborne && !(target is IAfflictable flying && flying.Afflictions.IsAirborne)) return;
            if (effect.HealthBelow > 0f && !(defence != null && !defence.IsDead && defence.Health.Fraction < effect.HealthBelow)) return;
            if (effect.Needs != AfflictionKind.None || effect.Needs2 != AfflictionKind.None)
            {
                // Asked of what the dead one carried, of the trigger's own enemy, or, when the trigger names no one, of the enemy faced.
                var carried = dead;
                if (carried == null)
                {
                    if (target == null && !hasAt && combat != null)
                    {
                        var ahead = combat.EnemyAhead(FightRange);
                        if (ahead != null) target = ahead.GetComponentInParent<IAfflictable>() as Component;
                    }
                    carried = target is IAfflictable afflicted ? afflicted.Afflictions : null;
                }
                if (carried == null) return;
                if (effect.Needs != AfflictionKind.None)
                {
                    if (!carried.Has(effect.Needs)) return;
                    if (effect.NeedsFull && carried.Stacks(effect.Needs) < Afflictions.Depth(effect.Needs)) return;
                    if (dead != null) spent = carried.Stacks(effect.Needs);
                }
                if (effect.Needs2 != AfflictionKind.None && !carried.Has(effect.Needs2)) return;
            }
            // A mark on a mixed branch lays both its elements' afflictions with a blow or a perfect guard. The root of
            // the element held lays that element's own already, so the branch lays only the one not held, and of the
            // held one only what it lays beyond the root.
            int less = 0;
            if (effect.Do == UpgradeAction.Afflict && effect.Radius <= 0f && effect.Owner != null && effect.Owner.IsMix && (IsBlow(effect.On) || effect.On == UpgradeTrigger.PerfectGuard)
                && hasRoot && combat != null && element != null && element.Info != null
                && (effect.Affliction == AfflictionKind.None || effect.Affliction == element.Info.Affliction))
            {
                less = Mathf.Max(1, combat.WeaponAfflictionStacks);
                if (Or(effect.Stacks, 1) - less <= 0) return;
            }
            if (effect.Chance < 1f && Random.value > effect.Chance) return;
            if ((effect.Spends > 0 || effect.SpendsAll) && effect.Owner != null)
            {
                // Paid from the count its mark keeps, or it does not happen.
                int index = effect.Owner.Index;
                int holds = index < tally.Length ? tally[index] : 0;
                int price = effect.SpendsAll ? holds : effect.Spends;
                if (price <= 0 || holds < price) return;
                tally[index] -= price;
                tallyDirty = true;
                spent = price;
            }
            // Things that can set themselves off again are held to a few times a second at most.
            float least = effect.On == UpgradeTrigger.AfflictionFull || effect.On == UpgradeTrigger.Kill ? 0.2f : 0f;
            readyAt[effect.Slot] = Time.time + Mathf.Max(effect.Cooldown, least);
            Enqueue(new Pending { Effect = effect, Target = target, At = at, HasAt = hasAt, Spent = spent, Less = less });
        }

        private void Enqueue(in Pending pending)
        {
            if (queued >= QueueCapacity) return;
            queue[queued++] = pending;
        }

        private void Flush()
        {
            if (flushing || queued == 0) return;
            flushing = true;
            // What is done here may put more in the line; those are done too, as far as the line has room.
            for (int i = 0; i < queued; i++)
            {
                var pending = queue[i];
                queue[i] = default;
                Execute(pending);
            }
            queued = 0;
            flushing = false;
            if (tallyDirty) ApplyTallies();
        }

        private SpellBlow Blow(ElementInfo look, AfflictionKind kind, float damage, float stagger, int stacks) => new SpellBlow
        {
            Damage = damage,
            Stagger = stagger,
            Affliction = stacks > 0 ? kind : AfflictionKind.None,
            Stacks = stacks,
            Effect = ElementLook.HitEffect(look),
            Tinted = ElementLook.Tints(look),
            Tint = look.Colour,
        };

        private static float Or(float value, float usual) => value > 0f ? value : usual;
        private static int Or(int value, int usual) => value > 0 ? value : usual;

        private void Execute(in Pending pending)
        {
            if (combat == null || element == null) return;
            if (pending.Special == Special.DeathBurst)
            {
                var thorns = lookOf[(int)AfflictionKind.Seeded];
                combat.StrikeArea(pending.At, pending.Radius, Blow(thorns, AfflictionKind.None, pending.Power, 10f, 0));
                ElementLook.Strike(thorns, pending.At, 0.2f);
                return;
            }
            if (pending.Special == Special.Carry)
            {
                combat.AfflictArea(pending.At, pending.Radius, pending.Kind, pending.Stacks, pending.Target);
                ElementLook.Ring(lookOf[(int)pending.Kind], pending.At + Vector3.up * 0.2f);
                return;
            }

            var effect = pending.Effect;
            if (effect == null) return;
            EffectsDone++;
            var held = element.Info;
            var kind = effect.Affliction != AfflictionKind.None ? effect.Affliction : held.Affliction;
            var look = lookOf[(int)kind] ?? held;
            Vector3 self = transform.position;
            Transform target = pending.Target != null ? pending.Target.transform : null;
            // With no one given, an effect aimed at a target takes the enemy the player faces.
            if (target == null && !pending.HasAt && effect.At == EffectAt.Target) target = combat.EnemyAhead(FightRange);
            Vector3 centre = effect.At == EffectAt.Self ? self : target != null ? target.position : pending.HasAt ? pending.At : self + transform.forward * 2.5f;
            centre.y = self.y;
            Vector3 toward = centre - self;
            toward.y = 0f;
            Vector3 aim = toward.sqrMagnitude > 0.04f ? toward.normalized : transform.forward;

            // What the trigger carried multiplies what is done: how many are thrown or fall, or how hard it strikes.
            int per = effect.PerStack ? Mathf.Max(1, pending.Spent) : 1;
            bool byCount = effect.Do == UpgradeAction.Projectile || effect.Do == UpgradeAction.Falling || effect.Do == UpgradeAction.Ward;
            float eDamage = byCount ? effect.Damage : effect.Damage * per;
            float eStagger = byCount ? effect.Stagger : effect.Stagger * per;
            float eAmount = effect.Amount * per;
            int eCount = byCount && effect.PerStack ? Or(effect.Count, 1) * per : effect.Count;
            int laid = effect.CarriesDepth ? Mathf.Max(1, pending.Spent) * Or(effect.Stacks, 1) : effect.Stacks;
            int eStacks = effect.CarriesDepth ? laid : effect.Do == UpgradeAction.Afflict ? Mathf.Max(1, Or(effect.Stacks, 1) * per - pending.Less) : byCount ? effect.Stacks : effect.Stacks * per;
            // Who it is done to, when it must be someone: the trigger's own enemy, or the one faced.
            Component subject = pending.Target;
            if (subject == null && target != null) subject = target.GetComponentInParent<IAfflictable>() as Component;

            switch (effect.Do)
            {
                case UpgradeAction.Afflict:
                {
                    int stacks = eStacks;
                    if (effect.Radius > 0f)
                    {
                        combat.AfflictArea(centre, effect.Radius, kind, stacks);
                        ElementLook.Ring(look, centre + Vector3.up * 0.2f);
                    }
                    else if (subject is IAfflictable afflictable)
                    {
                        float seconds = combat.AfflictionSeconds != 1f ? Afflictions.Spec(kind).Seconds * combat.AfflictionSeconds : 0f;
                        CombatEvents.Report(subject, kind, stacks, afflictable.Afflictions.Apply(kind, stacks + combat.ExtraStacks, seconds, combat.AfflictionPower));
                        ElementLook.Puff(look, centre + Vector3.up * 1.2f, Vector3.up, 0.6f);
                    }
                    break;
                }
                case UpgradeAction.Burst:
                {
                    Hazards.Lay(new HazardSpec
                    {
                        Kind = ElementLook.Ground(look), Tint = look.Colour, Position = centre, Radius = Or(effect.Radius, 2.5f), Seconds = 0.12f, BiteEvery = 0.1f,
                        Once = true, Burst = true, Delay = effect.Delay, Blow = Blow(look, kind, eDamage, eStagger, eStacks),
                    });
                    break;
                }
                case UpgradeAction.Cone:
                {
                    transform.rotation = Quaternion.LookRotation(aim, Vector3.up);
                    float reach = Or(effect.Reach, 4.5f);
                    combat.StrikeCone(reach, Or(effect.Arc, 90f), Blow(look, kind, eDamage, eStagger, eStacks));
                    Vector3 hand = self + Vector3.up * 1.1f;
                    for (int i = 1; i <= 3; i++) ElementLook.Puff(look, hand + aim * (reach * i / 3.5f), aim, 0.7f);
                    ElementLook.Sparks(look, hand + aim * 1.5f, aim, 1.5f);
                    break;
                }
                case UpgradeAction.Hazard:
                {
                    int patches = effect.Shape == HazardShape.Patch ? 1 : Or(eCount, 4);
                    float reach = Or(effect.Reach, 4f);
                    var spec = new HazardSpec
                    {
                        Kind = ElementLook.Ground(look), Tint = look.Colour, Radius = Or(effect.Radius, 1f), Seconds = Or(effect.Seconds, 3f), BiteEvery = Or(effect.Interval, 0.5f),
                        Grown = effect.Grown, Family = effect.Slot + 1, Limit = effect.Grown && effect.Limit > 0 ? effect.Limit + Mathf.Max(0, Stats.Whole(PlayerStat.GrownLimit)) : 0,
                        Blow = Blow(look, kind, eDamage, eStagger, eStacks),
                    };
                    for (int i = 0; i < patches; i++)
                    {
                        spec.Position = effect.Shape == HazardShape.Line ? self + aim * (reach * (i + 0.5f) / patches)
                            : effect.Shape == HazardShape.Ring ? centre + Quaternion.Euler(0f, i * 360f / patches, 0f) * Vector3.forward * reach
                            : centre;
                        spec.Delay = effect.Delay + (effect.Shape == HazardShape.Line ? i * 0.06f : 0f);
                        Hazards.Lay(spec);
                    }
                    break;
                }
                case UpgradeAction.Arc:
                {
                    if (target == null) target = combat.EnemyAhead(FightRange);
                    if (target == null) break;
                    combat.StrikeChain(0f, Blow(look, kind, eDamage, eStagger, eStacks), Or(eCount, 2), Or(effect.Reach, 5f), look, target);
                    break;
                }
                case UpgradeAction.Projectile:
                {
                    int thrown = Or(eCount, 1);
                    var spec = new HazardSpec
                    {
                        Kind = ElementLook.Ground(look), Tint = look.Colour, Radius = Or(effect.Radius, 0.9f), Seconds = Or(effect.Seconds, 1.2f), BiteEvery = 0.05f, Once = true,
                        Flight = effect.Flight, Target = target, Blow = Blow(look, kind, eDamage, eStagger, eStacks),
                    };
                    for (int i = 0; i < thrown; i++)
                    {
                        // More than one go out in a fan.
                        Vector3 way = Quaternion.Euler(0f, (i - (thrown - 1) * 0.5f) * 14f, 0f) * aim;
                        spec.Position = self + way * 0.8f;
                        spec.Velocity = way * Or(effect.Speed, 10f);
                        Hazards.Lay(spec);
                    }
                    ElementLook.Sparks(look, self + Vector3.up * 1.1f + aim * 0.6f, aim, 1f);
                    break;
                }
                case UpgradeAction.Falling:
                {
                    int falling = Or(eCount, 1);
                    float radius = Or(effect.Radius, 2f);
                    var spec = new HazardSpec
                    {
                        Kind = ElementLook.Ground(look), Tint = look.Colour, Radius = radius, Seconds = 0.12f, BiteEvery = 0.1f, Once = true, Burst = true, Warn = true,
                        Blow = Blow(look, kind, eDamage, eStagger, eStacks),
                    };
                    for (int i = 0; i < falling; i++)
                    {
                        // The first falls on the spot; the rest fall round it, one after another.
                        spec.Position = i == 0 ? centre : centre + Quaternion.Euler(0f, (i - 1) * 360f / Mathf.Max(1, falling - 1), 0f) * Vector3.forward * (radius * 1.3f);
                        spec.Delay = Or(effect.Delay, 0.6f) + i * 0.12f;
                        Hazards.Lay(spec);
                    }
                    break;
                }
                case UpgradeAction.Form:
                {
                    var form = SpellForms.Get(effect.Form);
                    if (form == null) break;
                    var cast = element.NewCast(effect.Spell ?? plainSpell, effect.Owner != null ? effect.Owner.Name : "", effect.Owner);
                    // Seen and laid in the way of the affliction the effect names; with none, in the element held.
                    cast.Element = look;
                    cast.Colour = look.Colour;
                    cast.Affliction = kind;
                    if (target != null)
                    {
                        cast.Target = target;
                        cast.Aim = target.position;
                    }
                    FormsBegun++;
                    form.Cast(cast);
                    break;
                }
                case UpgradeAction.Heal:
                    if (defence != null) defence.Heal(eAmount);
                    break;
                case UpgradeAction.Mana:
                    combat.Mana.Gain(eAmount);
                    break;
                case UpgradeAction.Stamina:
                    player.Stamina.Restore(eAmount);
                    break;
                case UpgradeAction.Boon:
                    Stats.AddBoon(effect.Stat, effect.Amount, Or(effect.Seconds, 5f));
                    ElementLook.Sparks(held, self + Vector3.up * 1.2f, Vector3.up, 0.6f);
                    break;
                case UpgradeAction.Ward:
                    if (defence != null) defence.GiveWards(Or(eCount, 1));
                    Vfx.Play(VfxKind.TintRing, self + Vector3.up * 0.2f, Vector3.up, 1f, new Color(0.7f, 0.92f, 1f));
                    break;
                case UpgradeAction.Step:
                {
                    float reach = Or(effect.Reach, 8f);
                    Vector3 to;
                    if (target != null)
                    {
                        float gap = Mathf.Min(reach, toward.magnitude);
                        // In front of it, or through it and out the far side.
                        to = effect.Amount > 0f ? self + aim * (gap + effect.Amount) : self + aim * Mathf.Max(0f, gap - 1.2f);
                    }
                    else to = self + transform.forward * (reach * 0.5f);
                    if (afterimage != null) afterimage.Leave(0.35f);
                    ElementLook.Sparks(held, self + Vector3.up, Vector3.up, 1f);
                    player.StepTo(to);
                    if (target != null)
                    {
                        Vector3 back = target.position - transform.position;
                        back.y = 0f;
                        if (back.sqrMagnitude > 0.04f) transform.rotation = Quaternion.LookRotation(back, Vector3.up);
                    }
                    ElementLook.Sparks(held, transform.position + Vector3.up, Vector3.up, 1f);
                    break;
                }
                case UpgradeAction.Pull:
                case UpgradeAction.Push:
                    combat.Displace(centre, Or(effect.Radius, 5f), Or(effect.Amount, 3f), effect.Do == UpgradeAction.Pull);
                    ElementLook.Ring(held, centre + Vector3.up * 0.2f);
                    break;
                case UpgradeAction.SlowTime:
                {
                    WorldClock.SlowEnemies(Or(effect.Seconds, 2f), Or(effect.Amount, 0.35f));
                    Color breath = Color.Lerp(Color.white, held.Colour, 0.5f);
                    breath.a = 0.3f;
                    ScreenFlash.Flash(breath, 0.08f);
                    break;
                }
                case UpgradeAction.Echo:
                {
                    float delay = Or(effect.Delay, 0.3f);
                    if (afterimage != null) afterimage.Leave(delay + 0.25f);
                    Hazards.Lay(new HazardSpec
                    {
                        Kind = ElementLook.Ground(held), Tint = held.Colour, Position = self, Radius = Or(effect.Radius, 2.3f), Seconds = 0.12f, BiteEvery = 0.1f, Once = true, Burst = true,
                        Delay = delay, Blow = Blow(held, kind, combat.LastBlowDamage * Or(effect.Amount, 0.6f), eStagger, 0),
                    });
                    break;
                }
                case UpgradeAction.Helper:
                {
                    int strikes = Or(eCount, 3);
                    float interval = Or(effect.Interval, 0.6f);
                    Hazards.Lay(new HazardSpec
                    {
                        Kind = ElementLook.Ground(look), Tint = look.Colour, Position = centre, Radius = Or(effect.Radius, 2f), Seconds = strikes * interval + 0.2f, BiteEvery = interval,
                        Bites = strikes, Burst = true, Follow = target != null, Target = target, Blow = Blow(look, kind, eDamage, eStagger, eStacks),
                    });
                    break;
                }
                case UpgradeAction.SetOff:
                {
                    if (!(subject is IAfflictable carrier) || !carrier.Afflictions.Has(kind)) break;
                    int depth = carrier.Afflictions.Stacks(kind);
                    float damage = (carrier.Afflictions.SetOff(kind) + eDamage * depth) * Stats.Multiplier(PlayerStat.BurstDamage);
                    combat.StrikeArea(centre, Or(effect.Radius, 3f) * Stats.Multiplier(PlayerStat.BurstReach), Blow(look, kind, damage, eStagger * depth, 0));
                    ElementLook.Strike(look, centre, 0.3f);
                    // Whatever waits on an affliction being spent is told, with how deep it was.
                    Trigger(UpgradeTrigger.SetOff, subject, centre, true, depth, null, kind);
                    break;
                }
                case UpgradeAction.Hold:
                {
                    float seconds = Or(effect.Seconds, 1.5f);
                    if (effect.Radius > 0f) Holds += combat.HoldArea(centre, effect.Radius, seconds);
                    else if (subject is IAfflictable gripped && gripped.Afflictions.Hold(seconds)) Holds++;
                    ElementLook.Ring(look, centre + Vector3.up * 0.2f);
                    ElementLook.Puff(look, centre + Vector3.up * 0.4f, Vector3.up, 0.8f);
                    break;
                }
                case UpgradeAction.Lift:
                {
                    float seconds = Or(effect.Seconds, 1f) + Stats.Sum(PlayerStat.AirSeconds);
                    if (effect.Radius > 0f) Lifts += combat.LiftArea(centre, effect.Radius, Or(effect.Seconds, 1f));
                    else if (subject is IAfflictable thrown)
                    {
                        if (thrown.Afflictions.Lift(seconds)) Lifts++;
                        // What is too great to leave the ground reels where it stands.
                        else combat.StrikeArea(centre, 0.8f, Blow(look, kind, 0f, Or(eStagger, 40f), 0));
                    }
                    else break;
                    ElementLook.Ring(look, centre + Vector3.up * 0.2f);
                    ElementLook.Puff(look, centre + Vector3.up * 0.6f, Vector3.up, 1.2f);
                    Trigger(UpgradeTrigger.Lifted, subject, centre);
                    break;
                }
                case UpgradeAction.Drop:
                    if (subject is IAfflictable driven) driven.Afflictions.Drop();
                    break;
                case UpgradeAction.BurstGrown:
                {
                    int burst = Hazards.BurstGrown(Blow(look, kind, eDamage, eStagger, eStacks), Or(effect.Radius, 3f));
                    if (burst > 0 && defence != null && eAmount > 0f) defence.Heal(eAmount * burst);
                    break;
                }
                case UpgradeAction.Bank:
                {
                    var owner = effect.Owner;
                    if (owner == null || owner.Tally == null || owner.Index >= tally.Length) break;
                    int now = Mathf.Min(owner.Tally.Cap, tally[owner.Index] + Or(eCount, 1));
                    if (now == tally[owner.Index]) break;
                    tally[owner.Index] = now;
                    tallyDirty = true;
                    ElementLook.Sparks(held, self + Vector3.up * 1.4f, Vector3.up, 0.5f);
                    break;
                }
                case UpgradeAction.FlaskFill:
                    if (defence != null && defence.Flasks.Fill(Or(eAmount, 0.25f))) defence.Callout("Flask earned");
                    break;
                case UpgradeAction.SpendHealth:
                    if (defence != null) defence.Health.Chip(eAmount * Stats.Multiplier(PlayerStat.HealthPrices));
                    break;
                case UpgradeAction.Leech:
                    if (defence != null) defence.Heal(combat.LastBlowDamage * Or(eAmount, 0.1f), false);
                    break;
            }
        }

        // ---- What the enemies say of themselves.

        private void OnEnemyDied(Component who, AfflictionSet afflictions)
        {
            if (who == null) return;
            Vector3 at = who.transform.position;
            // Seeded, it bursts into thorns; cursed, its death feeds the one who killed it.
            float burst = afflictions.DeathBurst(out float radius);
            if (burst > 0f) Enqueue(new Pending { Special = Special.DeathBurst, At = at, HasAt = true, Power = burst * Stats.Multiplier(PlayerStat.BurstDamage), Radius = radius * Stats.Multiplier(PlayerStat.BurstReach) });
            float fed = afflictions.HealOnKill;
            if (fed > 0f && defence != null) defence.Heal(fed);
            Trigger(UpgradeTrigger.Kill, null, at, true, 0, afflictions);
        }

        private void OnLanded(Component who)
        {
            if (who != null) Trigger(UpgradeTrigger.Landed, who, who.transform.position);
        }

        private void OnShattered(Component who)
        {
            if (who != null) Trigger(UpgradeTrigger.Shatter, who, who.transform.position);
        }

        private void OnAfflictionFull(Component who, AfflictionKind kind)
        {
            if (who == null) return;
            Vector3 at = who.transform.position;
            Vfx.Play(VfxKind.TintRing, at + Vector3.up * 0.3f, Vector3.up, 1f, Afflictions.Spec(kind).Colour);
            Trigger(UpgradeTrigger.AfflictionFull, who, at);
        }

        private void OnCarried(Component who, AfflictionKind kind, int stacks)
        {
            if (who == null) return;
            Enqueue(new Pending { Special = Special.Carry, Target = who, At = who.transform.position, HasAt = true, Kind = kind, Stacks = stacks,
                Radius = Afflictions.Spec(AfflictionKind.Soaked).CarryRadius });
        }

        // ---- What the player's own systems tell this.

        /// <summary>A blow of the weapon has struck something. Nothing here may strike in turn: the swing is still being counted.</summary>
        public void OnBlow(AttackStep step, IDamageable target, HitOutcome outcome, bool wasAfflicted = false)
        {
            if (outcome == HitOutcome.Blocked) return;
            var struck = target as Component;
            Vector3 at = struck != null ? struck.transform.position : transform.position;
            bool finisher = outcome == HitOutcome.Finisher;
            if (hasHeat && Heat.Landed())
            {
                Vfx.Play(VfxKind.Embers, transform.position + Vector3.up * 1.2f, Vector3.up, 0.8f);
                if (Heat.Level == Heat.MaximumLevel && defence != null) defence.Callout("Heat");
            }
            // The hunger of some surges: a share of each blow comes back.
            if (element != null && element.Kit.SurgeActive && element.Kit.SurgeLeech > 0f && defence != null)
                defence.Heal(combat.LastBlowDamage * element.Kit.SurgeLeech, false);

            // A share of every blow comes back, where a mark gives one.
            float leech = Stats.Sum(PlayerStat.Leech);
            if (leech > 0f && defence != null) defence.Heal(combat.LastBlowDamage * leech, false);

            Trigger(UpgradeTrigger.AnyHit, struck, at);
            if (combat.LastBlowCritical) Trigger(UpgradeTrigger.CritHit, struck, at);
            var chain = combat.Combo.Steps;
            if (step == combat.Combo.ChainHeavy || (chain.Count > 0 && step == chain[chain.Count - 1])) Trigger(UpgradeTrigger.ChainEnd, struck, at);
            if (finisher) Trigger(UpgradeTrigger.FinisherHit, struck, at);
            if (step == combat.Combo.Skill) Trigger(UpgradeTrigger.SkillHit, struck, at);
            else if (step.Heavy || finisher) Trigger(UpgradeTrigger.HeavyHit, struck, at);
            else Trigger(UpgradeTrigger.LightHit, struck, at);
            if (wasAfflicted) Trigger(UpgradeTrigger.AfflictedHit, struck, at);

            var afflictable = target as IAfflictable;
            if (hasPlasma && (step.Heavy || finisher) && afflictable != null && afflictable.Afflictions.Burn.IsBurning)
                toBurst.Add(target);
            if (!hasStatic || afflictable == null) return;
            int stacks = afflictable.Afflictions.Stacks(AfflictionKind.Static);
            if (step.Heavy || finisher)
            {
                if (stacks > 0) toDischarge.Add(target);
                return;
            }
            float seconds = combat.AfflictionSeconds != 1f ? Afflictions.Spec(AfflictionKind.Static).Seconds * combat.AfflictionSeconds : 0f;
            CombatEvents.Report(struck, AfflictionKind.Static, 1, afflictable.Afflictions.Apply(AfflictionKind.Static, 1 + combat.ExtraStacks, seconds, combat.AfflictionPower));
            Vfx.Play(VfxKind.Shock, at + Vector3.up * 1.3f, Vector3.up, 0.15f + 0.08f * stacks);
        }

        /// <summary>The swing is over, having struck this many.</summary>
        public void OnSwing(AttackStep step, int struck)
        {
            foreach (var target in toDischarge)
            {
                // Everything built up on it goes at once, into it and into everyone standing near.
                var body = target as Component;
                if (body == null || !(target is IAfflictable afflictable)) continue;
                int stacks = afflictable.Afflictions.Consume(AfflictionKind.Static);
                if (stacks <= 0) continue;
                Discharges++;
                Trigger(UpgradeTrigger.SetOff, body, body.transform.position, true, stacks, null, AfflictionKind.Static);
                LightningBolts.Strike(body.transform.position, 0.5f);
                combat.StrikeArea(body.transform.position, StaticRadius * Stats.Multiplier(PlayerStat.DischargeReach), StaticDamagePerStack * stacks, StaticStaggerPerStack * stacks, 0f, VfxKind.Shock);
            }
            toDischarge.Clear();
            foreach (var target in toBurst)
            {
                // Fire and storm together: the burn goes up, and the burst lights everyone it reaches.
                var body = (Component)target;
                if (body == null) continue;
                PlasmaBursts++;
                Vector3 at = body.transform.position;
                Trigger(UpgradeTrigger.SetOff, body, at, true, 1, null, AfflictionKind.Burn);
                LightningBolts.Strike(at, 0.5f);
                Vfx.Play(VfxKind.Burst, at + Vector3.up);
                Vfx.Play(VfxKind.Ring, at + Vector3.up * 0.2f, Vector3.up);
                for (int i = 0; i < 5; i++)
                    Vfx.Play(VfxKind.Fire, at + Quaternion.Euler(0f, i * 72f, 0f) * Vector3.forward * (PlasmaRadius * 0.5f) + Vector3.up * 0.5f, Vector3.up, 0.7f);
                combat.StrikeArea(at, PlasmaRadius, PlasmaDamage, PlasmaStagger, 4f, VfxKind.Fire, null, true);
            }
            toBurst.Clear();

            if (step == combat.Combo.Skill)
            {
                Vector3 forward = transform.forward;
                if (hasCrescent)
                    Hazards.Lay(HazardKind.Fire, transform.position + forward * 1.4f, 1.5f, CrescentSeconds, CrescentDamage, 12f, 3f, 0.1f, true, 0.05f, forward * CrescentSpeed);
                if (hasCuts && struck > 0)
                    Hazards.Lay(HazardKind.Lightning, transform.position + forward * (step.Reach * 0.5f), step.Reach * 0.65f, 0.12f, step.Damage * DelayedCutsShare, 22f, 0f, 0.1f, true, DelayedCutsDelay, default, true);
            }
            // The swing is counted: what it set going can be done now.
            Flush();
        }

        /// <summary>A spell struck something; once for each thing it struck.</summary>
        public void OnSpellStruck(IDamageable target, bool wasAfflicted)
        {
            var struck = target as Component;
            if (struck == null) return;
            Vector3 at = struck.transform.position;
            Trigger(UpgradeTrigger.SpellHit, struck, at);
            if (wasAfflicted) Trigger(UpgradeTrigger.AfflictedHit, struck, at);
        }

        /// <summary>The element's own spell has left the hand.</summary>
        public void OnSpellCast() => Trigger(UpgradeTrigger.SpellCast, null, transform.position, false);

        /// <summary>A blow was caught on the guard; the attacker stands at the given place.</summary>
        public void OnGuard(Vector3 attacker)
        {
            NoteGuard(attacker);
            Trigger(UpgradeTrigger.Guard, LastGuardEnemy, attacker);
        }

        /// <summary>Where the last blow caught on the guard came from, and who dealt it; for forms that answer a guard.</summary>
        public Vector3 LastGuardFrom { get; private set; }
        public Component LastGuardEnemy { get; private set; }

        private void NoteGuard(Vector3 attacker)
        {
            LastGuardFrom = attacker;
            LastGuardEnemy = combat != null ? combat.EnemyAt(attacker, 2.5f) : null;
        }

        /// <summary>A blow was turned at the last moment; the attacker stands at the given place.</summary>
        public void OnPerfectGuard(Vector3 attacker)
        {
            NoteGuard(attacker);
            bool detonate = hasDetonate;
            if (hasBackdraft)
            {
                Vector3 toward = attacker - transform.position;
                toward.y = 0f;
                if (toward.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
                int before = combat.StrikeCone(BackdraftReach * Stats.Multiplier(PlayerStat.BackdraftReach), BackdraftArc, BackdraftDamage, BackdraftStagger * Stats.Multiplier(PlayerStat.BackdraftStagger), 3f, VfxKind.Fire, detonate);
                if (detonate && before > 0)
                {
                    Detonations++;
                    Trigger(UpgradeTrigger.SetOff, combat.EnemyAt(attacker, 2.5f), attacker, true, 1, null, AfflictionKind.Burn);
                }
                Vector3 hand = transform.position + Vector3.up * 1.1f;
                for (int i = 1; i <= 3; i++)
                    Vfx.Play(VfxKind.Fire, hand + transform.forward * (BackdraftReach * i / 3.5f), transform.forward, 0.7f);
                Vfx.Play(VfxKind.Embers, hand + transform.forward * 1.5f, transform.forward, 1.5f);
            }
            else if (detonate && combat.StrikeArea(attacker, 1.8f * Stats.Multiplier(PlayerStat.BackdraftReach), 0f, 0f, 0f, VfxKind.Burst, null, true) > 0)
            {
                Detonations++;
            }
            if (hasCharge)
            {
                combat.NextBlowDamage = StoredChargeMultiplier;
                combat.NextBlowStagger = StoredChargeMultiplier * Stats.Multiplier(PlayerStat.ChargeStagger);
                Vfx.Play(VfxKind.Shock, transform.position + Vector3.up * 1.2f, Vector3.up, 0.8f);
            }
            Trigger(UpgradeTrigger.PerfectGuard, combat.EnemyAt(attacker, 2.5f), attacker);
        }

        /// <summary>A blow passed through the very start of a dodge.</summary>
        public void OnPerfectDodge()
        {
            Trigger(UpgradeTrigger.PerfectDodge, null, transform.position, false);
            if (!hasAfterimage) return;
            // What was left standing where the player stood strikes a moment later.
            Vfx.Play(VfxKind.Glint, dodgeFrom + Vector3.up * 1.2f);
            if (afterimage != null && !hasBlink) afterimage.Leave(AfterimageDelay + 0.25f);
            Hazards.Lay(HazardKind.Lightning, dodgeFrom, 2.3f, 0.12f, AfterimageDamage * Stats.Multiplier(PlayerStat.AfterimagePower), AfterimageStagger * Stats.Multiplier(PlayerStat.AfterimagePower), 0f, 0.1f, true, AfterimageDelay, default, true);
        }

        /// <summary>A dodge has begun from this place, in this direction.</summary>
        public void OnDodge(Vector3 from, Vector3 direction, float distance, float seconds)
        {
            dodgeFrom = from;
            DodgeCount++;
            LastDodgeFrom = from;
            LastDodgeDirection = direction;
            sinceDodge = 0f;
            if (hasTrail)
            {
                // Laid behind the roll as it goes, a patch at a time.
                for (int i = 0; i < 4; i++)
                    Hazards.Lay(HazardKind.Fire, from + direction * (distance * (i + 0.5f) / 4.5f), 0.95f, TrailSeconds * Stats.Multiplier(PlayerStat.TrailSeconds), TrailDamage * Stats.Multiplier(PlayerStat.TrailDamage), 0f, 2f, 0.5f, false, seconds * i / 4f);
            }
            // The god's own step: while the surge lasts the dodge is free, and it shocks whether or not Blink was learned.
            bool stormStep = hasStormStep && element != null && element.Kit.SurgeActive;
            if (stormStep)
            {
                FreeDodges++;
                player.Stamina.Restore(player.Dodge.StaminaCost);
            }
            if (hasBlink || stormStep)
            {
                if (afterimage != null) afterimage.Leave(AfterimageSeconds);
                LightningBolts.Strike(from, 0.25f);
                Hazards.Lay(HazardKind.Lightning, from, 1.1f * Stats.Multiplier(PlayerStat.BlinkWidth), seconds, BlinkDamage * Stats.Multiplier(PlayerStat.BlinkPower), BlinkStagger * Stats.Multiplier(PlayerStat.BlinkPower), 0f, 0.05f, true, 0f, direction * (distance / Mathf.Max(0.05f, seconds)));
            }
            Trigger(UpgradeTrigger.DodgeStart, null, from, false);
        }

        /// <summary>A dodge has run its length.</summary>
        public void OnDodgeEnd() => Trigger(UpgradeTrigger.DodgeEnd, null, transform.position, false);

        /// <summary>A blow landed on the player; the attacker stands at the given place.</summary>
        public void OnStruck(Vector3 attacker)
        {
            if (hasWhiteHeat && hasHeat && Heat.Level > baseHeatLevels)
            {
                selfBurnLeft = WhiteHeatBurnSeconds;
                if (defence != null) defence.Callout("Burning");
            }
            Heat.Struck();
            sinceStruck = 0f;
            Trigger(UpgradeTrigger.Struck, combat != null ? combat.EnemyAt(attacker, 2.5f) : null, attacker);
        }

        /// <summary>The element's surge was lit.</summary>
        public void OnSurge()
        {
            Stats.ClearSurge();
            if (element != null)
                foreach (var mod in element.Kit.SurgeMods) Stats.AddSurge(mod);
            if (hasHeat) Heat.Fill();
            if (hasOverclock)
            {
                WorldClock.SlowEnemies(OverclockSeconds, OverclockScale);
                ScreenFlash.Flash(new Color(0.6f, 0.85f, 1f, 0.3f), 0.08f);
            }
            Trigger(UpgradeTrigger.SurgeStart, null, transform.position, false);
        }

        /// <summary>The surge has run out, or the element was changed under it.</summary>
        public void OnSurgeEnd()
        {
            Stats.ClearSurge();
            Trigger(UpgradeTrigger.SurgeEnd, null, transform.position, false);
        }

        /// <summary>A flask was drunk and has healed.</summary>
        public void OnFlaskDrunk() => Trigger(UpgradeTrigger.FlaskDrunk, null, transform.position, false);

        /// <summary>A flask left the hand instead of being drunk.</summary>
        public void OnFlaskThrown()
        {
            FlasksThrown++;
            Transform enemy = combat.EnemyAhead(FirebombRange);
            Vector3 at = enemy != null ? enemy.position : transform.position + transform.forward * 5f;
            Vector3 toward = at - transform.position;
            toward.y = 0f;
            if (toward.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
            Vfx.Play(VfxKind.Glint, transform.position + Vector3.up * 1.8f);
            const float flight = 0.4f;
            ThrownThing.Throw(transform.position + Vector3.up * 1.6f + transform.forward * 0.3f, at + Vector3.up * 0.2f, flight, 2.2f);
            if (hasFirebomb)
            {
                Hazards.Lay(HazardKind.Fire, at, FirebombRadius, 0.12f, FirebombDamage, FirebombStagger, 4f, 0.1f, true, flight, default, true);
                Hazards.Lay(HazardKind.Fire, at, FirebombRadius * 0.85f, 3.5f, 5f, 0f, 2f, 0.5f, false, flight + 0.15f);
            }
            Trigger(UpgradeTrigger.FlaskThrown, enemy != null ? enemy.GetComponentInParent<IDamageable>() as Component : null, at);
        }

        /// <summary>The spell button was held: casts the worn spell. False when the mana is not there.</summary>
        public bool TrySpecial()
        {
            if (Worn == null || Worn.Spell == null) return false;
            var form = SpellForms.Get(Worn.Form);
            if (form == null) return false;
            // A spell may ask health in place of mana, or as well: it is left grey, to be won back.
            float blood = Worn.Spell.HealthCost * Stats.Multiplier(PlayerStat.HealthPrices);
            CoverWithHealth(Worn.Spell.Cost * Stats.Multiplier(PlayerStat.SpellCost));
            if (blood > 0f && (defence == null || defence.Health.Current <= blood)) return false;
            if (!combat.Mana.TrySpend(Worn.Spell.Cost * Stats.Multiplier(PlayerStat.SpellCost))) return false;
            if (blood > 0f) defence.Health.Chip(blood);
            WornCasts++;
            var cast = element.NewCast(Worn.Spell, Worn.Name, Worn);
            if (form is ISpellLanding landing)
            {
                // The form throws the player into the air; it is told again when they come down.
                Leaps++;
                landingForm = landing;
                landingCast = cast;
            }
            else if (cast.Target != null)
            {
                // Thrown at the enemy faced, as the element's own spell is.
                Vector3 toward = cast.Target.position - transform.position;
                toward.y = 0f;
                if (toward.sqrMagnitude > 0.0025f)
                {
                    transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
                    cast.Forward = transform.forward;
                }
            }
            form.Cast(cast);
            castSource = Worn;
            Trigger(UpgradeTrigger.SpellCast, null, transform.position, false);
            castSource = null;
            return true;
        }

        /// <summary>
        /// With a mark that sets a blood price, mana a spell lacks is paid in health: the health is left grey, to be
        /// won back, and never the last point is taken. Does nothing when the mana is there or no such mark is learned.
        /// </summary>
        public void CoverWithHealth(float cost)
        {
            float rate = Stats.Sum(PlayerStat.BloodPrice);
            if (rate <= 0f || combat == null || defence == null || defence.IsDead) return;
            float missing = cost - combat.Mana.Current;
            if (missing <= 0f) return;
            float price = missing * rate * Stats.Multiplier(PlayerStat.HealthPrices);
            if (defence.Health.Current - 1f < price) return;
            defence.Health.Chip(price);
            combat.Mana.Gain(missing + 0.001f);
            HealthPaid += price;
        }

        /// <summary>Health paid in place of mana so far, for tests and probes.</summary>
        public float HealthPaid { get; private set; }

        /// <summary>A ward turned a blow and is gone.</summary>
        public void OnWardSpent() => Trigger(UpgradeTrigger.WardSpent, null, transform.position, false);

        /// <summary>The leap has come down.</summary>
        public void OnLeapLanded()
        {
            if (landingForm == null) return;
            var form = landingForm;
            landingForm = null;
            form.Landed(landingCast);
        }

        /// <summary>The jolt of something heavy landing: a stop of time and a shake of the camera. For spell forms.</summary>
        /// <summary>Everything slowed for a while, the player too: the breath before something great lands. For spell forms.</summary>
        public void Slow(float seconds, float scale)
        {
            if (hitStop != null && seconds > 0f) hitStop.Slow(seconds, scale);
        }

        /// <summary>Forms begun by marks (<see cref="UpgradeAction.Form"/>), for tests and probes.</summary>
        public int FormsBegun { get; private set; }
        /// <summary>Dodges begun, and where the last one went; for forms that answer a dodge.</summary>
        public int DodgeCount { get; private set; }
        public Vector3 LastDodgeFrom { get; private set; }
        public Vector3 LastDodgeDirection { get; private set; }

        /// <summary>
        /// The surge button pressed again while the surge lasts. A form that waits on it says so every frame it waits
        /// (<see cref="ListenForSurge"/>); while one does, the press is its own and does not light the surge again.
        /// </summary>
        public int SurgeAgain { get; private set; }
        public bool ListensForSurge => Time.frameCount - listenedAt <= 2;
        public void ListenForSurge() => listenedAt = Time.frameCount;
        public void HeardSurge() => SurgeAgain++;
        private int listenedAt = -100;
        private static readonly SpellSpec plainSpell = new SpellSpec { Cost = 0f };

        // ---- The Rise.

        /// <summary>True while the player is Risen: stood up again from a death, their health draining.</summary>
        public bool Risen { get; private set; }
        /// <summary>How fast the Risen drain, as a share of the usual.</summary>
        public float RiseDrain => 1f;
        /// <summary>The share of health the Risen stand up with.</summary>
        public float RiseHealth = 0.5f;
        private bool riseSpent;
        private static readonly SpellSpec riseSpell = new SpellSpec { Cost = 0f, Damage = 4f, Count = 50 };

        /// <summary>
        /// The blow that would have killed: with Dark's gift, and once until a finisher on the killer buys it back,
        /// the player stands again instead, Risen. True when they rose.
        /// </summary>
        public bool Rise(Vector3 attacker)
        {
            if (Risen || riseSpent || defence == null || element == null || combat == null || !Has(Trees.DarkTree.LifeForALife)) return false;
            var form = SpellForms.Get(SpellForms.Rise);
            if (form == null) return false;
            riseSpent = true;
            Risen = true;
            defence.Health.Rise(defence.Health.Maximum * RiseHealth);
            var killer = combat.EnemyAt(attacker, 3f);
            var cast = element.NewCast(riseSpell, "Risen", Upgrade.ById(Trees.DarkTree.LifeForALife));
            cast.Target = killer != null ? killer.transform : null;
            if (killer != null) cast.Aim = killer.transform.position;
            form.Cast(cast);
            return true;
        }

        /// <summary>The Rise is over: bought back with a finisher (health returns, and the player may rise again), or lost.</summary>
        public void EndRise(bool lived, float share)
        {
            if (!Risen) return;
            Risen = false;
            if (!lived || defence == null) return;
            riseSpent = false;
            defence.Health.Heal(defence.Health.Maximum * share);
            defence.Callout("A life for a life");
        }

        // Standing up after a real death, the rise is theirs to use again.
        private void OnRevived()
        {
            Risen = false;
            riseSpent = false;
        }

        public void Jolt(float stopSeconds, float shake, float shakeSeconds)
        {
            if (hitStop != null && stopSeconds > 0f) hitStop.Freeze(stopSeconds);
            if (shaker != null && shake > 0f) shaker.Shake(shake, shakeSeconds);
        }
    }
}
