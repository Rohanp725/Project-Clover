using System.Collections.Generic;
using Clover.Core;
using Clover.Player;
using Clover.UI;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The sponsor's voice in a land: it speaks unasked when the player first wakes there, the first time each rest
    /// fire is lit, and the first time the player is turned back to a fire after a fall. Everything it says is read at
    /// the player's own pace in the reading box. The waking words are said once per saved game.
    /// </summary>
    public class LandVoice : MonoBehaviour
    {
        [SerializeField] private PlayerDefence defence;
        [SerializeField] private RestPoint[] fires = new RestPoint[0];
        // One entry per fire: its lines with a line break between them.
        [SerializeField, TextArea] private string[] fireWords = new string[0];

        [TextArea] public string[] Waking = new string[0];
        [TextArea] public string[] AfterAFall = new string[0];
        /// <summary>The saved-progress key that remembers the waking words were said.</summary>
        public string WakingKey = "land.woke";
        public float SpeaksAfter = 2.5f;

        public int Spoken { get; private set; }

        private readonly Queue<string[]> waiting = new Queue<string[]>();
        private bool[] said;
        private bool fallSaid;
        private float untilWaking;
        private float pause;

        public void Configure(PlayerDefence playerDefence, RestPoint[] restFires, string[] wordsAtEachFire)
        {
            defence = playerDefence;
            fires = restFires;
            fireWords = wordsAtEachFire;
        }

        private void Start()
        {
            said = new bool[fires.Length];
            untilWaking = Waking.Length > 0 && !Progress.Has(WakingKey) ? SpeaksAfter : -1f;
            if (defence != null) defence.Revived += OnRevived;
        }

        private void OnDestroy()
        {
            if (defence != null) defence.Revived -= OnRevived;
        }

        private void OnRevived()
        {
            if (fallSaid || AfterAFall.Length == 0) return;
            fallSaid = true;
            waiting.Enqueue(AfterAFall);
            pause = 1f;
        }

        private void Update()
        {
            if (untilWaking > 0f)
            {
                untilWaking -= Time.deltaTime;
                if (untilWaking <= 0f)
                {
                    Progress.Unlock(WakingKey);
                    waiting.Enqueue(Waking);
                }
            }
            for (int i = 0; i < fires.Length; i++)
            {
                if (said[i] || fires[i] == null || !fires[i].IsLit) continue;
                said[i] = true;
                if (i < fireWords.Length && !string.IsNullOrEmpty(fireWords[i])) waiting.Enqueue(fireWords[i].Split('\n'));
            }
            if (pause > 0f)
            {
                pause -= Time.deltaTime;
                return;
            }
            if (waiting.Count == 0 || ReadingBox.Instance == null || ReadingBox.Instance.IsOpen) return;
            if (defence != null && defence.IsDead) return;
            Spoken++;
            ReadingBox.Show(SponsorLines.Name, waiting.Dequeue());
        }
    }
}
