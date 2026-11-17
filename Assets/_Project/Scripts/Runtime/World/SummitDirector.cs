using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// Runs the fight at the summit: makes the storm worse when the god enters his second phase; when the swordsman
    /// falls, brings up what he became (the storm dragon; the colossus where a scene is built with that) out of the cloud; marks that one's fall; and then sets out
    /// the choice every god leaves behind, to take his element or to consume it. Once the player has chosen, the way
    /// out opens.
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
        [SerializeField] private GodlyForm colossus;
        [SerializeField] private BossHud bossHud;
        [SerializeField] private StormLight stormOverhead;

        /// <summary>Real seconds after the god falls before what he leaves appears.</summary>
        public float RewardDelay = 3.2f;
        /// <summary>Real seconds between the swordsman's fall and the rising of what he became.</summary>
        public float RisingDelay = 2.4f;
        public string GodlyName = "Narukami,  the Storm Dragon";
        /// <summary>What is said as the godly form begins to come, at its roar, and when its fury begins.</summary>
        public string RisingCallout = "Something moves in the cloud";
        public string RoarCallout = "The storm dragon";
        public string FuryCallout = "The storm breaks";
        /// <summary>Seconds between the sky's own flashes once he has fallen: the storm is over, and the sky nearly still.</summary>
        public float ClearedSkyMin = 40f;
        public float ClearedSkyMax = 90f;

        public bool Felled { get; private set; }
        public bool HasChosen { get; private set; }
        /// <summary>True from the swordsman's fall: the godly form has been called.</summary>
        public bool SecondFormCalled { get; private set; }

        private float untilRewards = -1f;
        private float untilRising = -1f;
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

        /// <summary>Gives the god a godly form to rise when the swordsman falls, and the bar that will show it.</summary>
        public void ConfigureSecondForm(GodlyForm godlyForm, BossHud hud, StormLight sky = null)
        {
            colossus = godlyForm;
            bossHud = hud;
            stormOverhead = sky;
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
            if (colossus != null)
            {
                colossus.Died += OnColossusDied;
                colossus.Roared += OnColossusRoared;
                colossus.Enraged += OnColossusEnraged;
            }
            if (boss == null) return;
            boss.Died += OnBossDied;
            boss.PhaseChanged += OnPhaseChanged;
        }

        private void OnDestroy()
        {
            if (colossus != null)
            {
                colossus.Died -= OnColossusDied;
                colossus.Roared -= OnColossusRoared;
                colossus.Enraged -= OnColossusEnraged;
            }
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
            ScreenFlash.Flash(new Color(0.8f, 0.9f, 1f, 0.38f), 0.09f);
            if (player != null) player.Callout("The storm answers him");
        }

        private void OnBossDied(Enemy who)
        {
            if (Felled || SecondFormCalled) return;
            if (colossus == null)
            {
                Fell(who.transform.position);
                return;
            }
            // The man is dead. What he made of himself is not.
            SecondFormCalled = true;
            if (hitStop != null) hitStop.Slow(1.4f, 0.25f);
            ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.5f), 0.09f);
            LightningBolts.Strike(who.transform.position, 1f);
            if (player != null)
            {
                player.Callout("The storm takes him");
                // A breath before the worst of it: the flasks are not given back, but the wounds of the duel are eased.
                player.Health.Heal(player.Health.Maximum * 0.35f);
            }
            if (rods != null)
            {
                rods.MinSecondsBetween = 0.4f;
                rods.MaxSecondsBetween = 1.2f;
                stormRaised = true;
            }
            untilRising = RisingDelay;
        }

        private void OnColossusDied(GodlyForm who) => Fell(new Vector3(0f, 0f, 8f));

        // His name is said when he says it himself: at the roar, as the bar that carries it appears.
        private void OnColossusRoared(GodlyForm who)
        {
            if (player != null) player.Callout(RoarCallout);
        }

        private void OnColossusEnraged(GodlyForm who)
        {
            if (player != null) player.Callout(FuryCallout);
        }

        private void Fell(Vector3 where)
        {
            if (Felled) return;
            Felled = true;
            stormRaised = false;
            RunState.GodsFelled++;
            if (hitStop != null) hitStop.Slow(1.8f, 0.2f);
            ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.55f), 0.1f);
            LightningBolts.Strike(where, 1f);
            if (player != null) player.Callout("A god falls");
            // The storm was his. With him gone the rods stand quiet and the sky all but stops flashing.
            if (rods != null)
            {
                rods.MinSecondsBetween = 6f;
                rods.MaxSecondsBetween = 14f;
                rods.enabled = colossus == null;
            }
            if (stormOverhead != null && colossus != null)
            {
                stormOverhead.MinSecondsBetween = ClearedSkyMin;
                stormOverhead.MaxSecondsBetween = ClearedSkyMax;
            }
            untilRewards = RewardDelay;
        }

        private void Update()
        {
            // The fight begun again from its start (the player fell and he is whole): the storm goes back to what it was.
            if (stormRaised && !SecondFormCalled && boss != null && !boss.IsDead && boss.Brain.Phase == 0 && rods != null)
            {
                stormRaised = false;
                rods.MinSecondsBetween = calmMin;
                rods.MaxSecondsBetween = calmMax;
            }
            if (untilRising >= 0f)
            {
                untilRising -= Time.unscaledDeltaTime;
                if (untilRising < 0f && colossus != null)
                {
                    colossus.Rise();
                    if (bossHud != null) bossHud.Show(colossus, GodlyName);
                    if (player != null) player.Callout(RisingCallout);
                }
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
