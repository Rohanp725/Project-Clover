using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Dark's Pawned Heart. The player's health drops to one and the rest is set down behind them as a beating heart
    /// in a ring that closes over its time. While it beats the weapon strikes far harder and its swings cost little.
    /// Leave it, then walk back onto the heart, and the health returns, with a share of the damage dealt meanwhile. If the ring closes
    /// first the heart stops: the player keeps what they have and half of what was pawned comes back grey.
    /// Reads: Seconds, Damage (percent the weapon gains), Count (percent of damage dealt that returns as health), Radius (of the ring).
    /// </summary>
    public class HeartForm : RunningForm
    {
        public override string Id => SpellForms.Heart;

        private const float Behind = 1.8f;
        private const float Touch = 1f;
        // It must be left before it can be come back for: this far.
        private const float Left = 2.6f;

        /// <summary>What walking back onto the heart returns: all that was pawned and a share of the damage dealt meanwhile.</summary>
        public static float Redeemed(float pawned, float dealt, float percent) => Mathf.Max(0f, pawned) + Mathf.Max(0f, dealt) * percent / 100f;

        /// <summary>Seconds after a perfect dodge in which touching the heart counts as touching it during the dodge.</summary>
        public const float TouchWindow = 0.6f;

        /// <summary>How near the hung heart's enemy the player must come to touch it, fighting in under it.</summary>
        public const float UnderHung = 1.6f;

        /// <summary>True while the heart's strength is the weapon's: always, except a buried heart's, only within its ring's radius of it.</summary>
        public static bool Strong(HeartPlace place, float distance, float radius) => place != HeartPlace.Buried || distance <= Mathf.Max(0.5f, radius);

        /// <summary>
        /// True when the player touches the heart now: behind them, only after leaving it (2.6 m) and coming back within a metre;
        /// hung over an enemy, by coming within <see cref="UnderHung"/> of it; buried, never (it rises by itself as the ring closes).
        /// </summary>
        public static bool Touched(HeartPlace place, bool left, float distance)
        {
            switch (place)
            {
                case HeartPlace.Buried: return false;
                case HeartPlace.Hung: return distance <= UnderHung;
                default: return left && distance < Touch;
            }
        }

        /// <summary>True when the ring's closing redeems the heart whole (buried), rather than stopping it (half pawned left grey).</summary>
        public static bool RisesWhenClosed(HeartPlace place) => place == HeartPlace.Buried;

        /// <summary>The share of the damage dealt the heart returns, in percent: its own, and more when touched during a perfect dodge (the ledger's heart-touch).</summary>
        public static float TouchPercent(float percent, bool inPerfectDodge, float more) => percent + (inPerfectDodge ? Mathf.Max(0f, more) : 0f);

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            var health = cast.Defence.Health;
            run.T0 = Mathf.Max(0f, health.Current - 1f);
            health.Damage(run.T0);
            // Behind the player, under their feet, or over the enemy faced (followed in Tick).
            run.Victim = cast.Spec.HeartAt == HeartPlace.Hung ? FormKit.Victim(cast) : null;
            run.A = cast.Spec.HeartAt == HeartPlace.Buried ? cast.Origin
                : run.Victim != null ? run.Victim.transform.position : cast.Origin - cast.Forward * Behind;
            SpellWorks.Solids(run)[0] = FormFx.Hold(FormMesh.Orb, new Color(0.16f, 0.02f, 0.2f), 0.07f);
            var marks = SpellWorks.Marks(run);
            marks[0] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 3f));
            marks[1] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour, 2f));
            FormFx.Lay(FormMark.Scorch, run.A, 4f, FormKit.Stain(cast.Colour, 0.8f), Mathf.Max(1f, cast.Spec.Seconds));
            FormFx.Run(FormMark.Streak, cast.Origin, -cast.Forward, Behind, 0.6f, FormKit.Lit(cast.Colour, 3.4f), 0.3f, 0.1f);
            Clover.Core.ScreenFlash.Flash(new Color(0.1f, 0f, 0.14f, 0.55f), 0.2f);
            SpellWorks.Gesture(cast, FormKit.Brace, 0.4f, 0f, 0.9f);
            if (cast.Defence != null) cast.Defence.Callout(cast.Name);
            if (cast.Upgrades != null) cast.Upgrades.Jolt(0.1f, 0.3f, 0.25f);
            return 0;
        }

        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage) => run.T1 += damage;

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            float seconds = Mathf.Max(1f, cast.Spec.Seconds);
            float left = 1f - run.Age / seconds;
            Vector3 here = cast.Player.transform.position;
            var place = cast.Spec.HeartAt;
            // Hung over its enemy, it follows it; if that one falls, it stays where it hung.
            if (place == HeartPlace.Hung && FormKit.Alive(run.Victim)) run.A = run.Victim.transform.position;
            float lift = place == HeartPlace.Hung ? 2.6f : place == HeartPlace.Buried ? 0.1f : 0.75f;
            // It beats: quicker as the ring closes, and each beat throws a pulse that is wider the more has been dealt.
            float rate = Mathf.Lerp(3.2f, 1.2f, left);
            float beat = Mathf.Repeat(run.Age * rate, 1f);
            if (beat < run.T2)
            {
                float pulse = 3f + Mathf.Min(7f, run.T1 / 25f);
                FormFx.Lay(FormMark.Ring, run.A, 0.8f, FormKit.Lit(cast.Colour, 2.6f), 0.5f, pulse * 2f);
                ElementLook.Puff(cast.Element, run.A + Vector3.up * 0.7f, Vector3.up, 0.6f);
            }
            run.T2 = beat;
            float swell = 0.62f * (1f + 0.3f * Mathf.Max(0f, 1f - beat * 4f));
            FormFx.Place(solids[0], run.A + Vector3.up * lift, Quaternion.Euler(0f, run.Age * 25f, 12f), new Vector3(swell, swell * 1.15f, swell));
            FormFx.Heat(solids[0], beat < 0.08f ? 0.6f : 0f);
            float ring = Mathf.Max(0.3f, cast.Spec.Radius * 2f * left);
            FormFx.PlaceMark(marks[0], run.A, Quaternion.identity, new Vector3(ring, 1f, ring));
            FormFx.PlaceMark(marks[1], run.A, Quaternion.Euler(0f, -run.Age * 40f, 0f), new Vector3(2.2f, 1f, 2.2f));
            Vector3 gap = here - run.A;
            gap.y = 0f;
            if (cast.Upgrades != null && Strong(place, gap.magnitude, cast.Spec.Radius))
            {
                cast.Upgrades.Stats.AddBoon(PlayerStat.WeaponDamage, cast.Spec.Damage / 100f, 0.25f);
                cast.Upgrades.Stats.AddBoon(PlayerStat.AttackStamina, -1f, 0.25f);
            }
            if (gap.magnitude > Left) run.Flag = true;
            if (Touched(place, run.Flag, gap.magnitude))
            {
                // Redeemed: for more, when the heart is touched in a perfect dodge and a mark says so (Pawned Heart's Master).
                bool inDodge = cast.Defence != null && Time.time - cast.Defence.LastPerfectDodgeAt <= TouchWindow;
                float more = cast.Upgrades != null ? cast.Upgrades.Stats.Sum(PlayerStat.HeartTouchMore) : 0f;
                float back = Redeemed(run.T0, run.T1, TouchPercent(cast.Spec.Count, inDodge, more));
                cast.Defence.Health.Heal(back);
                LastReturned = back;
                cast.Defence.Callout("Redeemed");
                FormFx.Lay(FormMark.Ring, here, 1f, FormKit.Lit(cast.Colour, 4f), 0.45f, 8f);
                FormFx.Lay(FormMark.Star, here, 1.5f, FormKit.Lit(cast.Colour, 3.4f), 0.25f, 5f);
                SigKit.Column(here, FormKit.Lit(cast.Colour, 3.4f), 1.8f, 0.3f);
                SigKit.Moment(cast, 0.12f, 0.4f, 0f, 1f, 0.35f);
                return false;
            }
            if (run.Age < seconds) return true;
            if (RisesWhenClosed(place))
            {
                // Buried: it rises whole where it lay, with its share of what was dealt.
                float risen = Redeemed(run.T0, run.T1, cast.Spec.Count);
                cast.Defence.Health.Heal(risen);
                LastReturned = risen;
                cast.Defence.Callout("The heart rises");
                SigKit.Column(run.A, FormKit.Lit(cast.Colour, 3.4f), 1.8f, 0.3f);
                FormFx.Lay(FormMark.Ring, run.A, 1f, FormKit.Lit(cast.Colour, 4f), 0.45f, 8f);
                return false;
            }
            // The ring closed: the heart stops, and half of what was pawned is only owed.
            float half = run.T0 * 0.5f;
            cast.Defence.Health.Heal(half);
            cast.Defence.Health.Chip(half);
            LastReturned = -half;
            cast.Defence.Callout("The heart stops");
            FormFx.Lay(FormMark.Crack, run.A, 3f, FormKit.Lit(cast.Colour, 2f), 1.5f);
            Clover.Core.ScreenFlash.Flash(new Color(0f, 0f, 0f, 0.5f), 0.25f);
            return false;
        }

        /// <summary>What the last heart gave back: health when it was redeemed, or, below zero, the grey health left when it stopped. For probes.</summary>
        public static float LastReturned { get; private set; }

        // Domain reload is off: a number from the last play must not be read in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => LastReturned = 0f;
    }
}
