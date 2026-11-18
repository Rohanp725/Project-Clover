using Clover.Core;
using Clover.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Clover.World
{
    /// <summary>
    /// What a death means in a run: it ends. A moment after the player falls the screen goes dark, the run's state is
    /// given up, and the player wakes in the hub. A scene without one of these lets the player stand up again where
    /// they last rested, as the practice arena does.
    /// </summary>
    public class RunEnder : MonoBehaviour
    {
        [SerializeField] private PlayerDefence player;
        [SerializeField] private Image dark;
        [SerializeField] private Text words;

        /// <summary>The scene the player wakes in.</summary>
        public string WakesIn = "Hub";
        /// <summary>Real seconds the fallen player is looked at before the dark begins, and how long the dark takes to close.</summary>
        public float HoldSeconds = 1.3f;
        public float FadeSeconds = 1.1f;
        /// <summary>Real seconds of full dark, with the word on it, before the hub.</summary>
        public float DarkSeconds = 0.9f;

        public bool IsEnding { get; private set; }

        private float elapsed;
        private bool left;
        private float reviveSeconds = 2.5f;

        public void Configure(PlayerDefence playerToWatch, Image darkImage, Text wordsText)
        {
            player = playerToWatch;
            dark = darkImage;
            words = wordsText;
        }

        private void Start()
        {
            // The run decides what a fall means here, not the arena's rule of standing up again.
            if (player != null)
            {
                reviveSeconds = player.ReviveAfterSeconds > 0f ? player.ReviveAfterSeconds : reviveSeconds;
                player.ReviveAfterSeconds = 0f;
            }
            // A god's own ground has no fire to come back to and a fight that cannot be put back by halves: in the
            // playtest a fall there begins the scene again, keeping everything carried.
            restartsScene = PlaytestMode.On && FindAnyObjectByType<SummitDirector>() != null;
            Show(0f);
        }

        /// <summary>True in the playtest where a fall begins the scene again instead of standing the player up at a fire.</summary>
        public bool RestartsInPlaytest => restartsScene;
        private bool restartsScene;

        private void Update()
        {
            if (player == null || left) return;
            // The playtest's rule: a fall loses nothing. The player stands again at the last fire rested at, as in a
            // land, and the area puts its enemies back; the run is not ended.
            if (PlaytestMode.On && !IsEnding && !restartsScene)
            {
                if (player.ReviveAfterSeconds <= 0f) player.ReviveAfterSeconds = reviveSeconds;
                return;
            }
            if (!IsEnding)
            {
                if (!player.IsDead) return;
                IsEnding = true;
                elapsed = 0f;
            }
            elapsed += Time.unscaledDeltaTime;
            Show(Mathf.Clamp01((elapsed - HoldSeconds) / Mathf.Max(0.01f, FadeSeconds)));
            if (elapsed < HoldSeconds + FadeSeconds + DarkSeconds) return;
            left = true;
            Time.timeScale = 1f;
            if (PlaytestMode.On)
            {
                SceneRoutes.Load(SceneManager.GetActiveScene().name);
                return;
            }
            RunState.End(true);
            SceneRoutes.Load(WakesIn);
        }

        private void Show(float amount)
        {
            if (dark != null)
            {
                dark.enabled = amount > 0f;
                dark.color = new Color(0.01f, 0.005f, 0.02f, amount);
            }
            if (words != null)
            {
                words.enabled = amount > 0.5f;
                var colour = words.color;
                colour.a = Mathf.Clamp01((amount - 0.5f) * 2f);
                words.color = colour;
            }
        }
    }
}
