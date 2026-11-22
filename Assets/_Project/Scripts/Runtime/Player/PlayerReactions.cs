using System;
using Clover.Core;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The reactions' adapter on the player (IMPLEMENTATION.md 3.7, 3.15). It hears each reaction the engine reports and
    /// each moment of an enemy condition, notes them, and acts on them after the frame's blows are done (in
    /// <see cref="LateUpdate"/>, never inside the blow that caused them): the reaction is shown (its look and its name
    /// over the enemy), its plain mark's effects go into the effect runner with the enemy as target and their laying
    /// counted as the reaction's (so they start nothing), and the marks in force that answer it are fired. While the pair's
    /// duo Shape mark is learned and in force, the shape chosen at the rest fire runs instead of the plain reaction (task
    /// 4.5, <see cref="ReactionTable.WornShape"/>), and what its conditions do is answered from that shape. A condition's
    /// beat, blow and end run the answers of the mark that set it; a linked condition passes its host's poison bites on.
    /// When an enemy dies carrying a condition whose mark answers its death, those answers are queued at once, while
    /// what it carried can still be read (as the Kill trigger reads it); the runner does them on its next step.
    /// It also pumps the engine once a frame, so a reaction deferred by the frame's budget is not kept waiting.
    /// </summary>
    [RequireComponent(typeof(PlayerUpgrades))]
    public class PlayerReactions : MonoBehaviour
    {
        private const int Most = 32;
        private const int MostBites = 16;
        private const int MostLinks = 24;

        /// <summary>A reaction was shown on an enemy: who, and the pair. The HUD writes its name over them.</summary>
        public event Action<Component, int> Shown;

        // Counts for probes and tests.
        /// <summary>Reactions the engine reported to this player, built or not.</summary>
        public int Heard { get; private set; }
        /// <summary>Reactions shown and run (built ones on a living enemy).</summary>
        public int ShownCount { get; private set; }
        /// <summary>Reactions heard whose mark is not built yet (task 3.4): they rest but do nothing.</summary>
        public int Unbuilt { get; private set; }
        public string LastShown { get; private set; } = "";
        /// <summary>The shape the last reaction shown ran in ("" for the plain reaction), and how many ran shaped.</summary>
        public string LastShape { get; private set; } = "";
        public int ShapedCount { get; private set; }
        public Component LastOn { get; private set; }
        /// <summary>Poison bites passed along a link, and condition moments answered.</summary>
        public int BitesPassed { get; private set; }
        public int ConditionAnswers { get; private set; }
        /// <summary>What could not be kept for the frame's end because too much came at once (task 3.6 sizes the rings from the profile).</summary>
        public int Dropped { get; private set; }
        public int TimesShown(int pair) => pair >= 0 && pair < shownByPair.Length ? shownByPair[pair] : 0;

        private PlayerUpgrades upgrades;
        private PlayerCombat combat;
        private PlayerBuild buildFound;
        private PlayerBuild build => buildFound != null ? buildFound : (buildFound = GetComponent<PlayerBuild>());
        private readonly Build.Loadout scratchLoadout = new Build.Loadout();
        private readonly int[] shownByPair = new int[AfflictionPairs.Count];

        // What was heard this frame, kept in fixed rings so hearing allocates nothing.
        private readonly Component[] reactedWho = new Component[Most];
        private readonly int[] reactedPair = new int[Most];
        private int reactedCount;
        private readonly Component[] conditionWho = new Component[Most];
        private readonly ConditionNews[] conditionNews = new ConditionNews[Most];
        private int conditionCount;
        private readonly Component[] bitHost = new Component[MostBites];
        private readonly float[] bitDamage = new float[MostBites];
        private int bitCount;
        // The enemies joined to a host by a linked condition, so the host's bites can find them without a search.
        private readonly Component[] links = new Component[MostLinks];

        private void Awake()
        {
            upgrades = GetComponent<PlayerUpgrades>();
            combat = GetComponent<PlayerCombat>();
            // The shared engine is new each play: each pair's rest is given to it here.
            ReactionTable.ApplyRests(ReactionEngine.Shared);
        }

        private void OnEnable()
        {
            CombatEvents.Reacted += OnReacted;
            CombatEvents.Condition += OnCondition;
            CombatEvents.PoisonBit += OnPoisonBit;
            CombatEvents.Died += OnDied;
        }

        private void OnDisable()
        {
            CombatEvents.Reacted -= OnReacted;
            CombatEvents.Condition -= OnCondition;
            CombatEvents.PoisonBit -= OnPoisonBit;
            CombatEvents.Died -= OnDied;
            reactedCount = conditionCount = bitCount = 0;
            Array.Clear(reactedWho, 0, reactedWho.Length);
            Array.Clear(conditionWho, 0, conditionWho.Length);
            Array.Clear(conditionNews, 0, conditionNews.Length);
            Array.Clear(bitHost, 0, bitHost.Length);
            Array.Clear(links, 0, links.Length);
            echoCount = 0;
            Array.Clear(echoWho, 0, echoWho.Length);
        }

        // ---- Heard in the middle of a blow: only noted.

        private void OnReacted(Component who, int pair, AfflictionKind laid)
        {
            Heard++;
            if (reactedCount >= Most)
            {
                Dropped++;
                return;
            }
            reactedWho[reactedCount] = who;
            reactedPair[reactedCount] = pair;
            reactedCount++;
        }

        private void OnCondition(Component who, ConditionNews news)
        {
            if (news.Moment == ConditionMoment.Began)
            {
                if ((news.Flags & ConditionFlags.Linked) != 0 && news.Link != null) NoteLink(who);
                return;
            }
            if (conditionCount >= Most)
            {
                Dropped++;
                return;
            }
            conditionWho[conditionCount] = who;
            conditionNews[conditionCount] = news;
            conditionCount++;
        }

        private void OnPoisonBit(Component who, int depth, float damage)
        {
            if (who == null || damage <= 0f || !AnyLink()) return;
            if (bitCount >= MostBites)
            {
                Dropped++;
                return;
            }
            bitHost[bitCount] = who;
            bitDamage[bitCount] = damage;
            bitCount++;
        }

        // Queued, not done: the set is cleared as soon as this returns, so its depth is read now (Smoulder's flare, the jar's break).
        private void OnDied(Component who, AfflictionSet afflictions)
        {
            if (who == null || afflictions == null || upgrades == null || !afflictions.Conditions.Any) return;
            var all = Upgrade.All;
            Vector3 at = who.transform.position;
            for (int slot = 0; slot < EnemyConditions.Slots; slot++)
            {
                var run = afflictions.Conditions.Run(slot);
                // A condition that drifts goes whole to the nearest other enemy (The Grudge's cloud).
                if (run.Active && run.DriftsTo > 0f) Drift(who, at, run);
                if (!run.Active || !run.AnswersDeath || run.Mark < 0 || run.Mark >= all.Count) continue;
                var mark = all[run.Mark];
                var source = SourceOf(mark);
                AnswerDeath(OwnEffects(mark), run, who, at, source, afflictions);
                // A reaction's shape sets conditions of its own; their answers are the shape's (each shape's ids are its own).
                if (ReactionTable.IsReactionShape(mark))
                    for (int shape = 0; shape < mark.Shapes.Length; shape++)
                        if (mark.Shapes[shape] != null) AnswerDeath(ShapeEffects(mark, shape), run, who, at, source, afflictions);
            }
        }

        private readonly System.Collections.Generic.HashSet<IDamageable> driftSkip = new System.Collections.Generic.HashSet<IDamageable>();
        /// <summary>Conditions that drifted to another enemy when their carrier died, counted for probes.</summary>
        public int Drifted { get; private set; }

        private void Drift(Component dead, Vector3 at, in ConditionRun run)
        {
            if (combat == null) return;
            driftSkip.Clear();
            if (dead is IDamageable deadBody) driftSkip.Add(deadBody);
            var next = combat.EnemyNear(at, run.DriftsTo, driftSkip);
            driftSkip.Clear();
            if (next is IAfflictable taker && taker.Afflictions.Conditions.Adopt(run, next) >= 0) Drifted++;
        }

        private void AnswerDeath(UpgradeEffect[] effects, in ConditionRun run, Component who, Vector3 at, LaySource source, AfflictionSet afflictions)
        {
            if (effects == null) return;
            foreach (var effect in effects)
            {
                if (effect.On != UpgradeTrigger.ConditionDied || effect.Condition != run.Id) continue;
                upgrades.Fire(effect, who, at, source, 0f, run.Blows, afflictions);
                ConditionAnswers++;
            }
        }

        // A mark's own effects as they run now: a plain reaction's with its branch's changes, any other's through the runner
        // (a Learn's Empower twins).
        private UpgradeEffect[] OwnEffects(Upgrade mark)
        {
            if (mark.Kind == UpgradeKind.Reaction)
            {
                int pair = ReactionTable.PairOf(mark.Id);
                if (pair >= 0) return upgrades.ReactionEffects(pair, -1);
            }
            return upgrades.EffectsOf(mark);
        }

        // A reaction shape's effects as they run now, with its branch's changes.
        private UpgradeEffect[] ShapeEffects(Upgrade shapeMark, int shape)
        {
            int pair = HeartLayout.PairOf(shapeMark);
            return pair >= 0 ? upgrades.ReactionEffects(pair, shape) : shapeMark.Shapes[shape].Effects;
        }

        // What a reaction lays starts nothing: its plain mark's and its shapes'.
        private static LaySource SourceOf(Upgrade mark) =>
            mark.Kind == UpgradeKind.Reaction || ReactionTable.IsReactionShape(mark) ? LaySource.Reaction : LaySource.Effect;

        // ---- Acted on once the frame's blows are done.

        private void LateUpdate()
        {
            ReactionEngine.Shared.Pump();
            bool acted = false;
            // What is done here may report more (a cascade): those wait for the next frame.
            int reactions = reactedCount;
            for (int i = 0; i < reactions; i++)
            {
                acted |= Act(reactedWho[i], reactedPair[i]);
                reactedWho[i] = null;
            }
            Shift(reactions, ref reactedCount, reactedWho, reactedPair);
            int moments = conditionCount;
            for (int i = 0; i < moments; i++)
            {
                acted |= Answer(conditionWho[i], conditionNews[i]);
                conditionWho[i] = null;
                conditionNews[i] = default;
            }
            ShiftNews(moments);
            for (int i = 0; i < bitCount; i++)
            {
                acted |= PassBite(bitHost[i], bitDamage[i]);
                bitHost[i] = null;
            }
            bitCount = 0;
            // The reactions owed a second strike whose time has come.
            for (int i = 0; i < echoCount; i++)
            {
                if (Time.time < echoAt[i]) continue;
                acted |= StrikeAgain(echoWho[i], echoWhere[i], echoPair[i], echoShare[i], echoCarried[i]);
                echoCount--;
                echoWho[i] = echoWho[echoCount];
                echoWhere[i] = echoWhere[echoCount];
                echoCarried[i] = echoCarried[echoCount];
                echoPair[i] = echoPair[echoCount];
                echoShare[i] = echoShare[echoCount];
                echoAt[i] = echoAt[echoCount];
                echoWho[echoCount] = null;
                i--;
            }
            if (acted && upgrades != null) upgrades.FlushNow();
        }

        // ---- A reaction struck again (the ledger's reaction-stats): Strikes Twice, Spreading Water.
        private const int MostEchoes = 8;
        private readonly Component[] echoWho = new Component[MostEchoes];
        private readonly int[] echoPair = new int[MostEchoes];
        private readonly float[] echoShare = new float[MostEchoes];
        private readonly float[] echoAt = new float[MostEchoes];
        private readonly Vector3[] echoWhere = new Vector3[MostEchoes];
        private readonly int[] echoCarried = new int[MostEchoes];
        private int echoCount;
        /// <summary>Reactions struck again, counted for probes.</summary>
        public int Echoed { get; private set; }

        /// <summary>
        /// The reaction of <paramref name="pair"/> strikes again on <paramref name="who"/> after <paramref name="delay"/>
        /// seconds: its own striking effects (bursts, lanes, arcs, falls, throws, helpers) at <paramref name="share"/> of
        /// their damage, nothing it lays or sets (ReactionRules.StrikesAgain), and nothing that answers a reaction is told.
        /// False when the line is full.
        /// </summary>
        public bool Echo(Component who, int pair, float share, float delay) => who != null && Echo(who, who.transform.position, pair, share, delay);

        /// <summary>
        /// As above, where it happened: on <paramref name="who"/> while it lives, or where it stood once it is gone (Kettle: a
        /// Soaked enemy that dies burning goes up as steam where it fell; the ledger's death-reaction).
        /// </summary>
        public bool Echo(Component who, Vector3 at, int pair, float share, float delay, int carried = 0)
        {
            if (pair < 0 || pair >= AfflictionPairs.Count || echoCount >= MostEchoes) return false;
            echoWho[echoCount] = who;
            echoWhere[echoCount] = at;
            echoCarried[echoCount] = carried;
            echoPair[echoCount] = pair;
            echoShare[echoCount] = share;
            echoAt[echoCount] = Time.time + Mathf.Max(0f, delay);
            echoCount++;
            return true;
        }

        private bool StrikeAgain(Component who, Vector3 where, int pair, float share, int carried)
        {
            if (upgrades == null || ReactionTable.MarkFor(pair) == null) return false;
            // On the enemy while it lives; once it is gone, where it stood, its look left to the blows themselves.
            if (!Alive(who)) who = null;
            Vector3 at = who != null ? who.transform.position : where;
            if (who != null) ReactionLook.Play(pair, at, who.transform, at - transform.position, transform);
            int worn = ReactionTable.WornShapeIndex(pair, Loadout(), RunState.Unlocked);
            var effects = upgrades.ReactionEffects(pair, worn);
            upgrades.ReactingOn(who, pair);
            upgrades.ScaleReaction(share);
            // What its spending would have counted goes with each blow, when it is known (Kettle: all the Burn the dead one carried).
            upgrades.StrikingAgain(true);
            for (int i = 0; i < effects.Length; i++)
                if (effects[i].On == UpgradeTrigger.Reacted && ReactionRules.StrikesAgain(effects[i].Do)) upgrades.Fire(effects[i], who, at, LaySource.Reaction, 0f, carried);
            upgrades.StrikingAgain(false);
            upgrades.ReactedOn();
            Echoed++;
            return true;
        }

        private bool Act(Component who, int pair)
        {
            if (!Alive(who) || pair < 0 || pair >= AfflictionPairs.Count) return false;
            var mark = ReactionTable.MarkFor(pair);
            if (mark == null)
            {
                Unbuilt++;
                return false;
            }
            Vector3 at = who.transform.position;
            // Its look rides on the enemy, and one with a way to it lies along the way from the player through the enemy.
            ReactionLook.Play(pair, at, who.transform, at - transform.position, transform);
            ShownCount++;
            shownByPair[pair]++;
            LastShown = mark.Name;
            LastOn = who;
            Shown?.Invoke(who, pair);
            if (upgrades == null) return false;
            // The shape chosen at the rest fire runs instead of the plain reaction, once its Shape mark is learned and in force.
            int worn = ReactionTable.WornShapeIndex(pair, Loadout(), RunState.Unlocked);
            var shape = worn >= 0 ? ReactionTable.ShapeMarkFor(pair).Shapes[worn] : null;
            LastShape = shape != null ? shape.Name : "";
            if (shape != null) ShapedCount++;
            // As written, or with the numbers of the duo branch's marks in force that change this reaction (Dry Pods, Deep Swell).
            var effects = upgrades.ReactionEffects(pair, worn);
            // What a mark makes of it on this enemy now, asked before anything is spent (Goes Up: at full depth, twice as hard, spending nothing).
            float boost = upgrades.ReactionBoost(who, pair, out bool spares);
            // A ripe seed in the pair makes the reaction's blows stronger, an unripe one softer (In Its Season).
            upgrades.ReactingOn(who, pair);
            if (boost != 1f) upgrades.ScaleReaction(boost);
            for (int i = 0; i < effects.Length; i++)
                if (effects[i].On == UpgradeTrigger.Reacted && !(spares && effects[i].Do == UpgradeAction.Spend)) upgrades.Fire(effects[i], who, at, LaySource.Reaction);
            upgrades.ReactedOn();
            // The marks in force that answer this reaction (or any): lifts and duo marks.
            upgrades.Fire(UpgradeTrigger.Reacted, who, at, mark.Id, LaySource.Reaction);
            return true;
        }

        private bool Answer(Component who, in ConditionNews news)
        {
            if (!Alive(who) || upgrades == null) return false;
            var all = Upgrade.All;
            if (news.Mark < 0 || news.Mark >= all.Count) return false;
            var mark = all[news.Mark];
            var trigger = news.Moment == ConditionMoment.Beat ? UpgradeTrigger.ConditionBeat
                : news.Moment == ConditionMoment.Struck ? UpgradeTrigger.ConditionStruck
                : news.Moment == ConditionMoment.Attacked ? UpgradeTrigger.ConditionAttacked : UpgradeTrigger.ConditionEnded;
            var source = SourceOf(mark);
            Vector3 at = who.transform.position;
            // Nerve Fire is seen at each bite as small jolts over the body, not as one big burst when it begins.
            if (news.Moment == ConditionMoment.Beat && news.Id == Trees.Reactions.Nerves) Forms.AfflictionArt.Jolt(at, news.Count);
            // An end carries the weapon blows the condition took (Burning Sap's weals), for what it does per blow.
            // So does its carrier's blow on the player (Ash Plume's storm, a flash for each blow it took).
            int spent = trigger == UpgradeTrigger.ConditionEnded || trigger == UpgradeTrigger.ConditionAttacked ? news.Blows : 0;
            // The marks in force that answer this condition whoever set it go first, so they can count what the condition's
            // own answers are about to spend (The Exile Returns counts the Static the Grudge's bolt discharges).
            bool any = news.Moment != ConditionMoment.Beat && upgrades.FireCondition(trigger, news, who, at);
            any |= AnswerFrom(OwnEffects(mark), trigger, news, who, at, source, spent);
            // A reaction's shape sets conditions of its own; their moments are answered from the shape.
            if (ReactionTable.IsReactionShape(mark))
                for (int shape = 0; shape < mark.Shapes.Length; shape++)
                    if (mark.Shapes[shape] != null) any |= AnswerFrom(ShapeEffects(mark, shape), trigger, news, who, at, source, spent);
            return any;
        }

        private bool AnswerFrom(UpgradeEffect[] effects, UpgradeTrigger trigger, in ConditionNews news, Component who, Vector3 at, LaySource source, int spent)
        {
            if (effects == null) return false;
            bool any = false;
            foreach (var effect in effects)
            {
                if (effect.On != trigger || effect.Condition != news.Id) continue;
                if (trigger == UpgradeTrigger.ConditionStruck && (!Answers(effect.By, news) || !FromSide(effect, news))) continue;
                // Its carrier's blow turned by a perfect guard, when that is what is asked (Snapped Blade).
                if (trigger == UpgradeTrigger.ConditionAttacked && effect.By == StruckBy.Turned && !news.Turned) continue;
                // An end the freeze brought, when that is what is asked (Stolen Warmth's last warmth).
                if (trigger == UpgradeTrigger.ConditionEnded && effect.By == StruckBy.Frozen && !news.Frozen) continue;
                if (effect.AtCount > 0 && effect.AtCount != (trigger == UpgradeTrigger.ConditionBeat ? news.Count : news.Blows)) continue;
                // An answer that repeats what the condition kept takes its kept number, not the blow's (Collected Twice).
                upgrades.Fire(effect, who, at, source, effect.ByPower ? news.Scale : news.Damage, spent, null, news.Link);
                ConditionAnswers++;
                any = true;
            }
            return any;
        }

        /// <summary>True when the blow came from the side the answer asks for (<see cref="UpgradeEffect.SideLeast"/>), or its side is not known.</summary>
        public static bool FromSide(UpgradeEffect effect, in ConditionNews news) =>
            (effect.SideLeast <= 0f && effect.SideMost <= 0f) || news.Side < 0f || (news.Side >= effect.SideLeast && news.Side <= effect.SideMost);

        /// <summary>True when a condition's blow is one the answer asks for (<see cref="UpgradeEffect.By"/>).</summary>
        public static bool Answers(StruckBy by, in ConditionNews news) =>
            // A shove is no blow: only what answers a shove hears it.
            news.Shoved ? by == StruckBy.Shove
            : by == StruckBy.Any || (by == StruckBy.Weapon && !news.Spell) || (by == StruckBy.Heavy && news.Heavy) || (by == StruckBy.Shatter && news.Shattered)
            || (by == StruckBy.Critical && news.Critical && !news.Spell) || (by == StruckBy.Finisher && news.Finisher && !news.Spell)
            || (by == StruckBy.Shove && news.Heavy && !news.Spell);

        // A host's poison bit: each enemy joined to it takes its share, as a reaction's blow (it starts nothing).
        private bool PassBite(Component host, float damage)
        {
            if (host == null || combat == null) return false;
            bool any = false;
            var colour = Afflictions.Spec(AfflictionKind.Poison).Colour;
            for (int i = 0; i < links.Length; i++)
            {
                var joined = links[i];
                if (!Alive(joined))
                {
                    links[i] = null;
                    continue;
                }
                if (!(joined is IAfflictable carrier) || !carrier.Afflictions.Conditions.LinkedTo(host, out float share) || share <= 0f) continue;
                // The joined body flashes poison's colour, not a plain blow's white (Rot Roots' film of 2026-11-20 at 2.1 s:
                // three bodies lit pure white at the shared bite).
                var blow = new SpellBlow { Damage = damage * share, Effect = VfxKind.TintPuff, Tinted = true, Tint = colour, Struck = colour, FromReaction = true };
                if (combat.StrikeOne(joined, blow, host.transform.position))
                {
                    BitesPassed++;
                    any = true;
                }
            }
            return any;
        }

        private void NoteLink(Component joined)
        {
            int free = -1;
            for (int i = 0; i < links.Length; i++)
            {
                if (links[i] == joined) return;
                if (free < 0 && (!Alive(links[i]) || !(links[i] is IAfflictable held) || !held.Afflictions.Conditions.Has(ConditionFlags.Linked))) free = i;
            }
            if (free >= 0) links[free] = joined;
        }

        private bool AnyLink()
        {
            for (int i = 0; i < links.Length; i++)
                if (links[i] != null) return true;
            return false;
        }

        /// <summary>Whether a thing that can be struck is still there and living: a god's piece answers for its god.</summary>
        internal static bool Alive(Component who)
        {
            if (who == null || !who.gameObject.activeInHierarchy) return false;
            if (who is Enemy enemy) return !enemy.IsDead;
            if (who is GodlyForm form) return !form.IsDead;
            // A god's piece (the head, a claw) answers for the god it belongs to. Asked once a reaction or a moment, not each frame.
            if (who is ColossusPart) return !(who.GetComponentInParent<GodlyForm>() is GodlyForm god) || !god.IsDead;
            return true;
        }

        private Build.Loadout Loadout() => build != null ? build.Read() : scratchLoadout;

        // What arrived while the frame's own were being acted on moves to the front for the next frame.
        private static void Shift(int done, ref int count, Component[] who, int[] pair)
        {
            int left = count - done;
            for (int i = 0; i < left; i++)
            {
                who[i] = who[done + i];
                pair[i] = pair[done + i];
                who[done + i] = null;
            }
            count = left;
        }

        private void ShiftNews(int done)
        {
            int left = conditionCount - done;
            for (int i = 0; i < left; i++)
            {
                conditionWho[i] = conditionWho[done + i];
                conditionNews[i] = conditionNews[done + i];
                conditionWho[done + i] = null;
                conditionNews[done + i] = default;
            }
            conditionCount = left;
        }
    }
}
