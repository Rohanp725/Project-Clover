using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// Runs the fight at the summit: makes the storm worse when the god enters his second phase, marks his fall, and
    /// then sets out the choice every god leaves behind, to take his element or to consume it. Once the player has
    /// chosen, the way back down opens.
    /// </summary>
    public class SummitDirector : MonoBehaviour
    {
        [SerializeField] private Enemy boss;
        [SerializeField] private PlayerDefence player;
        [SerializeField] private HitStop hitStop;
        [SerializeField] private StormRods rods;
        [SerializeField] private GodReward take;
        [SerializeField] private GodReward consume;
        [SerializeField] private SceneDoor wayOut;

        /// <summary>Real seconds after the god falls before what he leaves appears.</summary>
        public float RewardDelay = 3.2f;

        public bool Felled { get; private set; }
        public bool HasChosen { get; private set; }

        private float untilRewards = -1f;
        private float calmMin, calmMax;
        private bool stormRaised;

        public void Configure(Enemy bossToWatch, PlayerDefence playerToServe, HitStop stop, StormRods stormRods, GodReward takeReward, GodReward consumeReward, SceneDoor door)
        {
            boss = bossToWatch;
            player = playerToServe;
            hitStop = stop;
            rods = stormRods;
            take = takeReward;
            consume = consumeReward;
            wayOut = door;
        }

        private void Start()
        {
            if (take != null) take.gameObject.SetActive(false);
            if (consume != null) consume.gameObject.SetActive(false);
            if (wayOut != null) wayOut.Open = false;
            if (rods != null)
            {
                calmMin = rods.MinSecondsBetween;
                calmMax = rods.MaxSecondsBetween;
            }
            if (boss == null) return;
            boss.Died += OnBossDied;
            boss.PhaseChanged += OnPhaseChanged;
        }

        private void OnDestroy()
        {
            if (boss == null) return;
            boss.Died -= OnBossDied;
            boss.PhaseChanged -= OnPhaseChanged;
        }

        private void OnPhaseChanged(Enemy who)
        {
            // The storm comes down with him: the bolts around the summit fall three times as often.
            if (rods != null)
            {
                rods.MinSecondsBetween = 0.7f;
                rods.MaxSecondsBetween = 2f;
                stormRaised = true;
            }
            ScreenFlash.Flash(new Color(0.8f, 0.9f, 1f, 0.45f), 0.12f);
            if (player != null) player.Callout("The storm answers him");
        }

        private void OnBossDied(Enemy who)
        {
            if (Felled) return;
            Felled = true;
            stormRaised = false;
            RunState.GodsFelled++;
            if (hitStop != null) hitStop.Slow(1.8f, 0.2f);
            ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.8f), 0.16f);
            LightningBolts.Strike(who.transform.position, 1f);
            if (player != null) player.Callout("A god falls");
            if (rods != null)
            {
                rods.MinSecondsBetween = 6f;
                rods.MaxSecondsBetween = 14f;
            }
            untilRewards = RewardDelay;
        }

        private void Update()
        {
            // The fight begun again from its start (the player fell and he is whole): the storm goes back to what it was.
            if (stormRaised && boss != null && !boss.IsDead && boss.Brain.Phase == 0 && rods != null)
            {
                stormRaised = false;
                rods.MinSecondsBetween = calmMin;
                rods.MaxSecondsBetween = calmMax;
            }
            if (untilRewards < 0f) return;
            untilRewards -= Time.unscaledDeltaTime;
            if (untilRewards > 0f) return;
            untilRewards = -1f;
            Reveal(take);
            Reveal(consume);
        }

        private static void Reveal(GodReward reward)
        {
            if (reward == null) return;
            reward.gameObject.SetActive(true);
            Vfx.Play(VfxKind.Burst, reward.transform.position + Vector3.up);
            Vfx.Play(VfxKind.Ring, reward.transform.position + Vector3.up * 0.2f, Vector3.up);
        }

        /// <summary>The player has chosen: the other thing fades, and the way down opens.</summary>
        public void Chose(GodReward chosen)
        {
            HasChosen = true;
            if (take != null) take.gameObject.SetActive(false);
            if (consume != null) consume.gameObject.SetActive(false);
            if (wayOut != null) wayOut.Open = true;
        }
    }
}
