using System.Collections.Generic;
using Clover.Core;
using Clover.Gameplay;
using Clover.World;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// What the player has learned from their element's tree, and the embers to learn more. Embers are taken from the
    /// fallen and spent at a rest fire; the run remembers both from scene to scene. The weapon, the guard, the dodge and
    /// the element each tell this when something happens (a blow lands, a guard is perfect, a dodge begins), and this
    /// does whatever the learned upgrades add to it. The numbers of every upgrade are fields here.
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
        public float MeteorCost = 50f;
        public float MeteorRange = 9f;
        public float MeteorSeconds = 0.62f;
        public float MeteorHeight = 3.4f;
        public float MeteorDamage = 46f;
        public float MeteorStagger = 55f;
        public float MeteorRadius = 3.2f;

        [Header("Lightning")]
        public int StaticMaximum = 5;
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
        public float RodCost = 40f;
        public float RodSeconds = 6f;
        public float RodRadius = 3.5f;
        public float RodDamage = 12f;
        public float OverclockSeconds = 2.5f;
        public float OverclockScale = 0.35f;
        public float PlasmaDamage = 18f;
        public float PlasmaStagger = 16f;
        public float PlasmaRadius = 3.2f;
        /// <summary>Seconds the shape left behind by a lightning dodge stays.</summary>
        public float AfterimageSeconds = 0.45f;

        public int Points => RunState.Points;
        public int Bought { get; private set; }
        /// <summary>True when holding the spell button does something of its own: the leap, or the rod.</summary>
        public bool HasSpecial => has[Index(Upgrade.MeteorLeap)] || has[Index(Upgrade.LightningRod)];
        /// <summary>True while a perfect guard's charge waits on the next blow.</summary>
        public bool ChargeStored => combat != null && combat.NextBlowDamage > 1f;
        public bool ShowsHeat => has[Index(Upgrade.Heat)];
        // Counters so tests and probes can see what happened without watching the screen.
        public int Detonations { get; private set; }
        public int Discharges { get; private set; }
        public int Leaps { get; private set; }
        public int FlasksThrown { get; private set; }
        public int PlasmaBursts { get; private set; }
        public int FreeDodges { get; private set; }

        private PlayerController player;
        private PlayerElement element;
        private PlayerCombat combat;
        private PlayerDefence defence;
        private TopDownCamera shaker;
        private HitStop hitStop;
        private readonly bool[] has = new bool[Upgrade.All.Count];
        private readonly Dictionary<IDamageable, int> staticOn = new Dictionary<IDamageable, int>();
        private readonly Dictionary<IDamageable, float> staticAt = new Dictionary<IDamageable, float>();
        /// <summary>Seconds without a blow before the Static on an enemy has leaked away.</summary>
        public float StaticLasts = 8f;
        private readonly List<IDamageable> toDischarge = new List<IDamageable>();
        private readonly List<IDamageable> toBurst = new List<IDamageable>();
        private Afterimage afterimage;
        private float untilRefresh;
        private Vector3 dodgeFrom;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            element = GetComponent<PlayerElement>();
            combat = GetComponent<PlayerCombat>();
            defence = GetComponent<PlayerDefence>();
            hitStop = GetComponent<HitStop>();
            afterimage = GetComponent<Afterimage>();
            Heat.Reset();
            foreach (string id in StartWith) RunState.Unlocked.Add(id);
            RunState.Points += StartingPoints;
            Hazards.OwnedBy(combat);
        }

        private void Start()
        {
            var main = Camera.main;
            shaker = main != null ? main.GetComponent<TopDownCamera>() : null;
            Refresh();
        }

        private static int Index(string id)
        {
            for (int i = 0; i < Upgrade.All.Count; i++)
                if (Upgrade.All[i].Id == id) return i;
            return 0;
        }

        /// <summary>True when the upgrade is learned and belongs to the element and weapon the player holds now.</summary>
        public bool Has(string id)
        {
            if (!RunState.Unlocked.Contains(id)) return false;
            var upgrade = Upgrade.ById(id);
            if (upgrade == null) return false;
            if (element != null && element.Kit != null && upgrade.Element != element.Kit.Name) return false;
            // The god's sword counts as the Katana it is.
            if (!string.IsNullOrEmpty(upgrade.Weapon) && combat != null && combat.Weapon != null && !combat.Weapon.Name.Contains(upgrade.Weapon)) return false;
            return true;
        }

        public bool Owns(string id) => RunState.Unlocked.Contains(id);

        /// <summary>The name of the element whose tree the player can learn from now.</summary>
        public string ElementName => element != null && element.Kit != null ? element.Kit.Name : ElementKit.FireName;

        public bool CanBuy(Upgrade upgrade) =>
            upgrade != null && !RunState.Unlocked.Contains(upgrade.Id) && RunState.Points >= upgrade.Cost && upgrade.Element == ElementName;

        public bool TryBuy(Upgrade upgrade)
        {
            if (!CanBuy(upgrade)) return false;
            RunState.Points -= upgrade.Cost;
            RunState.Unlocked.Add(upgrade.Id);
            Bought++;
            Refresh();
            return true;
        }

        public void Earn(int points)
        {
            if (points > 0) RunState.Points += points;
        }

        /// <summary>Taking a new element leaves the old one's learning behind.</summary>
        public void Forget()
        {
            RunState.Unlocked.Clear();
            Heat.Reset();
            staticOn.Clear();
            staticAt.Clear();
            if (combat != null)
            {
                combat.NextBlowDamage = 1f;
                combat.NextBlowStagger = 1f;
            }
            Refresh();
        }

        // What is in force is worked out a few times a second, not asked for by name on every blow.
        private void Refresh()
        {
            untilRefresh = 0.25f;
            for (int i = 0; i < has.Length; i++) has[i] = Has(Upgrade.All[i].Id);
            if (combat != null) combat.BurnStacksOnHit = has[Index(Upgrade.BurnAndDetonate)] ? 1 : 0;
            if (defence != null) defence.HoldToThrow = has[Index(Upgrade.FirebombFlask)];
            if (!has[Index(Upgrade.Heat)]) Heat.Reset();
        }

        private void Update()
        {
            untilRefresh -= Time.unscaledDeltaTime;
            if (untilRefresh <= 0f) Refresh();
            if (has[Index(Upgrade.Heat)]) Heat.Tick(Time.deltaTime);
            if (combat != null) combat.UpgradeDamage = has[Index(Upgrade.Heat)] ? Heat.DamageMultiplier : 1f;
        }

        /// <summary>A blow of the weapon has struck something. Nothing here may strike in turn: the swing is still being counted.</summary>
        public void OnBlow(AttackStep step, IDamageable target, HitOutcome outcome)
        {
            if (outcome == HitOutcome.Blocked) return;
            if (has[Index(Upgrade.Heat)] && Heat.Landed())
            {
                Vfx.Play(VfxKind.Embers, transform.position + Vector3.up * 1.2f, Vector3.up, 0.8f);
                if (Heat.Level == Heat.MaximumLevel && defence != null) defence.Callout("Heat");
            }
            if (has[Index(Upgrade.Plasma)] && (step.Heavy || outcome == HitOutcome.Finisher) && target is IBurnable burning && burning.Burn.IsBurning)
                toBurst.Add(target);
            if (!has[Index(Upgrade.Static)]) return;
            staticOn.TryGetValue(target, out int stacks);
            // Left alone, it leaks away: an enemy met again later does not still carry it.
            if (stacks > 0 && staticAt.TryGetValue(target, out float last) && Time.time - last > StaticLasts) stacks = 0;
            staticAt[target] = Time.time;
            staticOn[target] = stacks;
            if (step.Heavy || outcome == HitOutcome.Finisher)
            {
                if (stacks > 0) toDischarge.Add(target);
                return;
            }
            staticOn[target] = Mathf.Min(StaticMaximum, stacks + 1);
            Vfx.Play(VfxKind.Shock, ((Component)target).transform.position + Vector3.up * 1.3f, Vector3.up, 0.15f + 0.08f * stacks);
        }

        /// <summary>The swing is over, having struck this many.</summary>
        public void OnSwing(AttackStep step, int struck)
        {
            foreach (var target in toDischarge)
            {
                // Everything built up on it goes at once, into it and into everyone standing near.
                int stacks = staticOn[target];
                staticOn.Remove(target);
                var body = (Component)target;
                if (body == null) continue;
                Discharges++;
                LightningBolts.Strike(body.transform.position, 0.5f);
                combat.StrikeArea(body.transform.position, StaticRadius, StaticDamagePerStack * stacks, StaticStaggerPerStack * stacks, 0f, VfxKind.Shock);
            }
            toDischarge.Clear();
            foreach (var target in toBurst)
            {
                // Fire and storm together: the burn goes up, and the burst lights everyone it reaches.
                var body = (Component)target;
                if (body == null) continue;
                PlasmaBursts++;
                Vector3 at = body.transform.position;
                LightningBolts.Strike(at, 0.5f);
                Vfx.Play(VfxKind.Burst, at + Vector3.up);
                Vfx.Play(VfxKind.Ring, at + Vector3.up * 0.2f, Vector3.up);
                for (int i = 0; i < 5; i++)
                    Vfx.Play(VfxKind.Fire, at + Quaternion.Euler(0f, i * 72f, 0f) * Vector3.forward * (PlasmaRadius * 0.5f) + Vector3.up * 0.5f, Vector3.up, 0.7f);
                combat.StrikeArea(at, PlasmaRadius, PlasmaDamage, PlasmaStagger, 4f, VfxKind.Fire, null, true);
            }
            toBurst.Clear();

            if (step != combat.Combo.Skill) return;
            Vector3 forward = transform.forward;
            if (has[Index(Upgrade.BurningCrescent)])
                Hazards.Lay(HazardKind.Fire, transform.position + forward * 1.4f, 1.5f, CrescentSeconds, CrescentDamage, 12f, 3f, 0.1f, true, 0.05f, forward * CrescentSpeed);
            if (has[Index(Upgrade.DelayedCuts)] && struck > 0)
                Hazards.Lay(HazardKind.Lightning, transform.position + forward * (step.Reach * 0.5f), step.Reach * 0.65f, 0.12f, step.Damage * DelayedCutsShare, 22f, 0f, 0.1f, true, DelayedCutsDelay, default, true);
        }

        /// <summary>A blow was turned at the last moment; the attacker stands at the given place.</summary>
        public void OnPerfectGuard(Vector3 attacker)
        {
            bool detonate = has[Index(Upgrade.BurnAndDetonate)];
            if (has[Index(Upgrade.Backdraft)])
            {
                Vector3 toward = attacker - transform.position;
                toward.y = 0f;
                if (toward.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
                int before = combat.StrikeCone(BackdraftReach, BackdraftArc, BackdraftDamage, BackdraftStagger, 3f, VfxKind.Fire, detonate);
                if (detonate && before > 0) Detonations++;
                Vector3 hand = transform.position + Vector3.up * 1.1f;
                for (int i = 1; i <= 3; i++)
                    Vfx.Play(VfxKind.Fire, hand + transform.forward * (BackdraftReach * i / 3.5f), transform.forward, 0.7f);
                Vfx.Play(VfxKind.Embers, hand + transform.forward * 1.5f, transform.forward, 1.5f);
            }
            else if (detonate && combat.StrikeArea(attacker, 1.8f, 0f, 0f, 0f, VfxKind.Burst, null, true) > 0)
            {
                Detonations++;
            }
            if (has[Index(Upgrade.StoredCharge)])
            {
                combat.NextBlowDamage = StoredChargeMultiplier;
                combat.NextBlowStagger = StoredChargeMultiplier;
                Vfx.Play(VfxKind.Shock, transform.position + Vector3.up * 1.2f, Vector3.up, 0.8f);
            }
        }

        /// <summary>A blow passed through the very start of a dodge.</summary>
        public void OnPerfectDodge()
        {
            if (!has[Index(Upgrade.Afterimage)]) return;
            // What was left standing where the player stood strikes a moment later.
            Vfx.Play(VfxKind.Glint, dodgeFrom + Vector3.up * 1.2f);
            if (afterimage != null && !has[Index(Upgrade.Blink)]) afterimage.Leave(AfterimageDelay + 0.25f);
            Hazards.Lay(HazardKind.Lightning, dodgeFrom, 2.3f, 0.12f, AfterimageDamage, AfterimageStagger, 0f, 0.1f, true, AfterimageDelay, default, true);
        }

        /// <summary>A dodge has begun from this place, in this direction.</summary>
        public void OnDodge(Vector3 from, Vector3 direction, float distance, float seconds)
        {
            dodgeFrom = from;
            if (has[Index(Upgrade.FlameTrail)])
            {
                // Laid behind the roll as it goes, a patch at a time.
                for (int i = 0; i < 4; i++)
                    Hazards.Lay(HazardKind.Fire, from + direction * (distance * (i + 0.5f) / 4.5f), 0.95f, TrailSeconds, TrailDamage, 0f, 2f, 0.5f, false, seconds * i / 4f);
            }
            // The god's own step: while the surge lasts the dodge is free, and it shocks whether or not Blink was learned.
            bool stormStep = has[Index(Upgrade.StormStep)] && element != null && element.Kit.SurgeActive;
            if (stormStep)
            {
                FreeDodges++;
                player.Stamina.Restore(player.Dodge.StaminaCost);
            }
            if (has[Index(Upgrade.Blink)] || stormStep)
            {
                if (afterimage != null) afterimage.Leave(AfterimageSeconds);
                LightningBolts.Strike(from, 0.25f);
                Hazards.Lay(HazardKind.Lightning, from, 1.1f, seconds, BlinkDamage, BlinkStagger, 0f, 0.05f, true, 0f, direction * (distance / Mathf.Max(0.05f, seconds)));
            }
        }

        /// <summary>A blow landed on the player.</summary>
        public void OnStruck() => Heat.Struck();

        /// <summary>The element's surge was lit.</summary>
        public void OnSurge()
        {
            if (has[Index(Upgrade.Heat)]) Heat.Fill();
            if (has[Index(Upgrade.Overclock)])
            {
                WorldClock.SlowEnemies(OverclockSeconds, OverclockScale);
                ScreenFlash.Flash(new Color(0.6f, 0.85f, 1f, 0.35f), 0.1f);
            }
        }

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
            Hazards.Lay(HazardKind.Fire, at, FirebombRadius, 0.12f, FirebombDamage, FirebombStagger, 4f, 0.1f, true, flight, default, true);
            Hazards.Lay(HazardKind.Fire, at, FirebombRadius * 0.85f, 3.5f, 5f, 0f, 2f, 0.5f, false, flight + 0.15f);
        }

        /// <summary>The spell button was held. Does the upgrade's own move; false when the mana is not there.</summary>
        public bool TrySpecial()
        {
            if (has[Index(Upgrade.MeteorLeap)])
            {
                if (!combat.Mana.TrySpend(MeteorCost)) return false;
                Leaps++;
                Transform enemy = combat.EnemyAhead(MeteorRange);
                Vector3 to = transform.position + transform.forward * 5f;
                if (enemy != null)
                {
                    // Down beside them, not inside them.
                    Vector3 gap = enemy.position - transform.position;
                    gap.y = 0f;
                    to = transform.position + gap.normalized * Mathf.Max(0f, gap.magnitude - 1f);
                }
                Vfx.Play(VfxKind.Fire, transform.position + Vector3.up * 0.3f, Vector3.up, 1.4f);
                Vfx.Play(VfxKind.Ring, transform.position + Vector3.up * 0.2f, Vector3.up);
                player.Leap(to, MeteorSeconds, MeteorHeight);
                return true;
            }
            if (has[Index(Upgrade.LightningRod)])
            {
                if (!combat.Mana.TrySpend(RodCost)) return false;
                Vector3 at = transform.position + transform.forward * 2.5f;
                LightningBolts.Strike(at, 0.6f);
                Hazards.Lay(HazardKind.Lightning, at, RodRadius, RodSeconds, RodDamage, 12f, 0f, 0.9f, false, 0.2f);
                if (defence != null) defence.Callout("Lightning Rod");
                return true;
            }
            return false;
        }

        /// <summary>The leap has come down.</summary>
        public void OnLeapLanded()
        {
            if (!has[Index(Upgrade.MeteorLeap)]) return;
            Vector3 at = transform.position;
            combat.StrikeArea(at, MeteorRadius, MeteorDamage, MeteorStagger, 4f, VfxKind.Fire, null, false, true);
            Hazards.Lay(HazardKind.Fire, at, MeteorRadius * 0.8f, 4f, 5f, 0f, 2f, 0.5f, false, 0.2f);
            Vfx.Play(VfxKind.Impact, at + Vector3.up * 0.5f);
            Vfx.Play(VfxKind.Burst, at + Vector3.up * 0.6f);
            Vfx.Play(VfxKind.Ring, at + Vector3.up * 0.2f, Vector3.up);
            for (int i = 0; i < 6; i++)
                Vfx.Play(VfxKind.Fire, at + Quaternion.Euler(0f, i * 60f, 0f) * Vector3.forward * (MeteorRadius * 0.6f) + Vector3.up * 0.4f, Vector3.up, 0.8f);
            ScreenFlash.Flash(new Color(1f, 0.75f, 0.4f, 0.4f), 0.08f);
            if (hitStop != null) hitStop.Freeze(0.12f);
            if (shaker != null) shaker.Shake(0.45f, 0.3f);
        }
    }
}
