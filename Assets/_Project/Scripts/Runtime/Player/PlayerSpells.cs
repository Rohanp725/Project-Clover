using Clover.Core;
using Clover.Gameplay;
using Clover.Player.Magic;
using Clover.World;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The four spell slots in play (IMPLEMENTATION.md 3.1, 3.2): hold L2 and press a face button, or press 1 to 4, to
    /// cast what sits in a slot; each slot has its own cooldown, cut by weapon play (a landed blow, a perfect guard, a
    /// perfect dodge, a finisher). It also keeps the fight's clock, and out of a fight lets mana settle toward half.
    /// The rules are plain classes in <c>Magic/</c>; this adapts them. <see cref="PlayerController"/> calls
    /// <see cref="Advance"/> right after the element, in its one order. The controller adds it to a player built before
    /// it existed, so no scene had to be rebuilt.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerSpells : MonoBehaviour
    {
        public SpellCooldowns Cooldowns = new SpellCooldowns();
        public CooldownCuts Cuts = new CooldownCuts();
        public FightClock Fight = new FightClock();
        /// <summary>Seconds a slot press is remembered while the player is busy (mid-swing, rolling).</summary>
        public float PressBufferTime = 0.25f;
        /// <summary>Seconds between looks for an aware enemy near: four times a second, not every frame.</summary>
        public float FightSampleSeconds = 0.25f;

        /// <summary>What sits in the four slots: West, North, East, South.</summary>
        public SpellSlots Slots { get; } = new SpellSlots();
        // Counts for tests and probes.
        public int Casts { get; private set; }
        public int CutsTaken { get; private set; }
        public float SecondsCut { get; private set; }
        public int ReadyFlashes { get; private set; }
        public int Refusals { get; private set; }
        /// <summary>The slot the last spell was cast from, or -1.</summary>
        public int LastSlot { get; private set; } = -1;
        public int SlotsVersion { get; private set; }

        /// <summary>A slot's spell was cast (the slot), and a slot came ready again (the slot). For the HUD.</summary>
        public event System.Action<int> SlotCast;
        public event System.Action<int> SlotReady;

        private PlayerController player;
        private PlayerElement element;
        private PlayerCombat combat;
        private PlayerDefence defence;
        private PlayerUpgrades upgrades;
        private AreaDirector director;
        private Enemy[] looseEnemies;
        private IBoss[] bosses;
        private int buffered = -1;
        private float bufferLeft;
        private float untilSample;
        private float sampled;
        private int heldSlot = -1;
        private int seenVersion = -1;
        private string seenElement;
        private Upgrade seenLent;
        private bool subscribed;
        private float regather = 5f;
        private System.Func<string, bool> stillTheirs;

        private void Awake()
        {
            stillTheirs = StillTheirs;
            player = GetComponent<PlayerController>();
            element = GetComponent<PlayerElement>();
            combat = GetComponent<PlayerCombat>();
            defence = GetComponent<PlayerDefence>();
            upgrades = GetComponent<PlayerUpgrades>();
            // The slots chosen in the last scene of the run come with the player.
            Slots.CopyFrom(RunState.SpellSlots);
        }

        private void OnEnable() => Subscribe(true);
        private void OnDisable() => Subscribe(false);

        private void Start()
        {
            Subscribe(true);
            // Who can start a fight here: the area's enemies (or the scene's, where there is no director) and the gods.
            director = FindAnyObjectByType<AreaDirector>();
            if (director == null) looseEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude);
            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            int count = 0;
            foreach (var behaviour in behaviours) if (behaviour is IBoss) count++;
            bosses = new IBoss[count];
            count = 0;
            foreach (var behaviour in behaviours) if (behaviour is IBoss boss) bosses[count++] = boss;
            Look();
        }

        private void Subscribe(bool on)
        {
            if (combat == null || defence == null || on == subscribed) return;
            subscribed = on;
            if (on)
            {
                combat.BlowLanded += OnBlowLanded;
                combat.Finished += OnFinished;
                defence.PerfectGuarded += OnPerfectGuard;
                defence.PerfectDodged += OnPerfectDodge;
            }
            else
            {
                combat.BlowLanded -= OnBlowLanded;
                combat.Finished -= OnFinished;
                defence.PerfectGuarded -= OnPerfectGuard;
                defence.PerfectDodged -= OnPerfectDodge;
            }
        }

        private void OnBlowLanded(AttackStep step) => CutAll(Cuts.ForBlow(step));
        private void OnFinished() => CutAll(Cuts.ForFinisher);
        private void OnPerfectGuard() => CutAll(Cuts.ForPerfectGuard);
        private void OnPerfectDodge() => CutAll(Cuts.ForPerfectDodge);

        private void CutAll(float seconds)
        {
            if (seconds <= 0f) return;
            CutsTaken++;
            SecondsCut += seconds;
            Flash(Cooldowns.Cut(seconds));
        }

        private void Flash(int ready)
        {
            if (ready == 0) return;
            for (int i = 0; i < SpellSlots.Count; i++)
            {
                if ((ready & (1 << i)) == 0) continue;
                ReadyFlashes++;
                SlotReady?.Invoke(i);
            }
        }

        /// <summary>The mark in a slot (null for an empty slot or the element's base spell).</summary>
        public Upgrade MarkIn(int slot)
        {
            string id = Slots[slot];
            if (string.IsNullOrEmpty(id) || SpellSlots.IsBase(id)) return null;
            if (upgrades != null && upgrades.Lent != null && upgrades.Lent.Id == id) return upgrades.Lent;
            return Upgrade.ById(id);
        }

        /// <summary>The name a slot's spell goes by: the base spell's own, a mark's, or empty.</summary>
        public string NameIn(int slot)
        {
            string id = Slots[slot];
            if (string.IsNullOrEmpty(id)) return "";
            if (SpellSlots.IsBase(id)) return element != null && element.Kit != null ? element.Kit.SpellName : "";
            var mark = MarkIn(slot);
            return mark != null ? mark.Name : "";
        }

        /// <summary>A slot's spell is paid in mana (and how much), not by its clock.</summary>
        public float ManaIn(int slot)
        {
            var mark = MarkIn(slot);
            return mark != null && upgrades != null ? upgrades.PriceOf(mark) : 0f;
        }

        /// <summary>True while the button of the slot the last spell came from is held: a spell held for longer (Eye of the Storm) reads it.</summary>
        public bool CastButtonHeld => heldSlot >= 0 && player != null && player.Buttons != null && player.Buttons.SlotHeld(heldSlot);

        public void DropHolds()
        {
            buffered = -1;
            bufferLeft = 0f;
        }

        /// <summary>Puts a spell in one slot (the chart, a lent spell, an older probe).</summary>
        public void SetSlot(int slot, string id)
        {
            Slots.Set(slot, id);
            Changed();
        }

        /// <summary>The chart's stopgap: the spell goes in the next slot round (West, North, East, South). Returns the slot.</summary>
        public int PutInNext(string id)
        {
            int slot = Slots.PutInNext(id);
            Changed();
            return slot;
        }

        /// <summary>A gift that is a spell: the first empty slot, else North. It stays where it is if already slotted.</summary>
        public void Place(string id)
        {
            if (Slots.SlotOf(id) >= 0) return;
            int empty = Slots.FirstEmpty();
            SetSlot(empty >= 0 ? empty : 1, id);
        }

        /// <summary>A rest: every clock ends and the fight is over.</summary>
        public void Rested()
        {
            Cooldowns.ResetAll();
            Fight.Reset();
        }

        private void Changed()
        {
            SlotsVersion++;
            Slots.CopyTo(RunState.SpellSlots);
        }

        // A slot keeps a spell only while it is in force (or lent) now.
        private bool StillTheirs(string id)
        {
            if (upgrades == null) return false;
            if (upgrades.Lent != null && upgrades.Lent.Id == id) return true;
            var list = upgrades.SpellsInForce;
            for (int i = 0; i < list.Count; i++)
                if (list[i] == id) return true;
            return false;
        }

        // The slots look again when what is in force, the element or the lent spell has changed: never every frame.
        private void Look()
        {
            if (upgrades == null || element == null) return;
            // Not before the marks have been read once: the slots carried from the last scene would all look stale.
            if (upgrades.SpellsVersion == 0) return;
            string held = element.HasElement && element.Info != null ? element.Info.Name : "";
            if (seenVersion == upgrades.SpellsVersion && held == seenElement && seenLent == upgrades.Lent) return;
            seenVersion = upgrades.SpellsVersion;
            seenElement = held;
            if (upgrades.Lent != null && upgrades.Lent != seenLent) Slots.Set(1, upgrades.Lent.Id);
            seenLent = upgrades.Lent;
            Slots.Settle(SpellSlots.BaseId(held), stillTheirs, upgrades.SpellsInForce);
            Changed();
        }

        // The clocks, the fight and mana run every frame on the game's time, whatever the body is doing: a spell
        // that holds the body (a stance, a leap) must not stop the other slots' clocks.
        private void Update()
        {
            // The slots look again here too, so a spell bought at a rest shows in its slot while the chart is still open.
            Look();
            float deltaTime = Time.deltaTime;
            Flash(Cooldowns.Tick(deltaTime));
            AdvanceFight(deltaTime);
        }

        /// <summary>One frame of the buttons: a slot pressed is cast when the player is free. <paramref name="free"/> as the element's.</summary>
        public void Advance(float deltaTime, bool free)
        {
            Look();
            var buttons = player.Buttons;
            int pressed = buttons != null ? buttons.SlotCast : -1;
            // A menu's last frame (the guide closing on this press) is not a cast.
            if (pressed >= 0 && !Clover.UI.GuidePage.BusyThisFrame)
            {
                buffered = pressed;
                bufferLeft = PressBufferTime;
            }
            if (buffered >= 0 && free && (element == null || !element.IsCasting))
            {
                int slot = buffered;
                buffered = -1;
                TryCast(slot);
            }
            if (bufferLeft > 0f)
            {
                bufferLeft -= deltaTime;
                if (bufferLeft <= 0f) buffered = -1;
            }
        }

        private void AdvanceFight(float deltaTime)
        {
            sampled += deltaTime;
            untilSample -= deltaTime;
            if (untilSample <= 0f)
            {
                untilSample = FightSampleSeconds;
                // A fight's start brings mana up to half: no fight is begun empty.
                if (Fight.Tick(sampled, AwareEnemyNear()) && combat != null) combat.Mana.AtLeast(combat.Mana.SettleLevel);
                sampled = 0f;
            }
            if (combat != null) combat.Mana.Settle(deltaTime, Fight.InFight);
        }

        private bool AwareEnemyNear()
        {
            // With no director the scene's enemies are looked for again now and then, so later ones are seen too.
            if (director == null && (looseEnemies == null || (regather -= FightSampleSeconds) <= 0f))
            {
                regather = 5f;
                looseEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude);
            }
            if (bosses != null)
                for (int i = 0; i < bosses.Length; i++)
                    if (bosses[i] is Object alive && alive != null && bosses[i].InFight) return true;
            var enemies = director != null ? director.Enemies : looseEnemies;
            if (enemies == null) return false;
            float most = Fight.FightRange * Fight.FightRange;
            Vector3 here = transform.position;
            for (int i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.isActiveAndEnabled || !enemy.InFight) continue;
                if ((enemy.transform.position - here).sqrMagnitude <= most) return true;
            }
            return false;
        }

        /// <summary>Casts what sits in a slot now, if its clock is ready and its price can be paid. True when it was cast.</summary>
        public bool TryCast(int slot)
        {
            if (slot < 0 || slot >= SpellSlots.Count || element == null) return false;
            if (!element.HasElement) return false;
            string id = Slots[slot];
            if (string.IsNullOrEmpty(id))
            {
                Refuse("Nothing in that slot");
                return false;
            }
            if (!Cooldowns.Ready(slot))
            {
                Refuse("Not ready");
                return false;
            }
            float seconds;
            if (SpellSlots.IsBase(id))
            {
                if (element.IsCasting) return false;
                // A spell goes where the player faces; with the mouse in use, that is toward the cursor.
                player.FacePointer();
                if (!element.CastBase()) return false;
                seconds = SpellTimings.Base(element.Info.Name).Cooldown;
            }
            else
            {
                var mark = MarkIn(slot);
                if (mark == null || mark.Spell == null || upgrades == null) return false;
                if (upgrades.PriceOf(mark) > combat.Mana.Current && !combat.Mana.Sealed && upgrades.Stats.Sum(PlayerStat.BloodPrice) <= 0f)
                {
                    Refuse("No mana");
                    return false;
                }
                player.FacePointer();
                if (!upgrades.Cast(mark))
                {
                    Refuse(mark.Spell.HealthCost > 0f ? "Not enough health" : "Cannot cast that here");
                    return false;
                }
                // A spell from outside the table (a lent one) that asks no mana still waits.
                seconds = mark.Spell.Cooldown > 0f ? mark.Spell.Cooldown : mark.Spell.Cost > 0f ? 0f : SpellTimings.DefaultCooldown;
            }
            Cooldowns.Start(slot, seconds);
            Casts++;
            LastSlot = slot;
            heldSlot = slot;
            SlotCast?.Invoke(slot);
            return true;
        }

        private void Refuse(string why)
        {
            Refusals++;
            if (defence != null) defence.Callout(why);
        }
    }
}
