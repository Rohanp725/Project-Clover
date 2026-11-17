using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// A thing read at the player's own pace, like any <see cref="Notice"/>; the first time its last line has been
    /// read, the dormant enemies it names get up behind the reader, and one timed line says so (a scare).
    /// </summary>
    public class WakingNotice : Notice
    {
        [SerializeField] private Enemy[] sleepers = new Enemy[0];
        public string Scare = "Something behind you is warm.";

        public bool Woken { get; private set; }

        public void Configure(Enemy[] whoWakes) => sleepers = whoWakes;

        public override void Use(PlayerInteraction who)
        {
            if (!Available) return;
            var defence = who.Defence;
            ReadingBox.Show(Speaker, Lines, () =>
            {
                if (Woken) return;
                Woken = true;
                foreach (var sleeper in sleepers)
                    // A dormant enemy wakes when it is struck: this is a touch, not a blow.
                    if (sleeper != null && !sleeper.IsDead && sleeper.IsDormant) sleeper.TakeHit(new Hit(0.01f, 0f, Vector3.forward));
                if (defence != null) defence.Callout(Scare);
            });
        }
    }
}
