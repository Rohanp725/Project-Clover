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

        public void Configure(PlayerDefence playerToWatch, Image darkImage, Text wordsText)
        {
            player = playerToWatch;
            dark = darkImage;
            words = wordsText;
        }

        private void Start()
        {
            // The run decides what a fall means here, not the arena's rule of standing up again.
            if (player != null) player.ReviveAfterSeconds = 0f;
            Show(0f);
        }

        private void Update()
        {
            if (player == null || left) return;
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
            RunState.End(true);
            SceneManager.LoadScene(WakesIn);
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
