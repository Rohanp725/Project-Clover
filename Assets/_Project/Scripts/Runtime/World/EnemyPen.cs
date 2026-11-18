using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// A walled pen on the Proving Ground holding one of every enemy of a land, each kept out of sight until called.
    /// A post at the pen's edge lets one out, or a set of them together, or puts them all away again; a fall or a rest
    /// puts them away too. The pen's enemies are not the area's: they give no Shards, and nothing brings them back by
    /// itself.
    /// </summary>
    public class EnemyPen : MonoBehaviour
    {
        [SerializeField] private Enemy[] enemies = new Enemy[0];
        [SerializeField] private PlayerDefence player;
        [SerializeField] private AreaDirector director;

        /// <summary>The land's name, for the posts' words.</summary>
        public string Land = "";

        public Enemy[] Enemies => enemies;
        public int Released { get; private set; }
        public int Cleared { get; private set; }

        public int Out
        {
            get
            {
                int count = 0;
                foreach (var enemy in enemies)
                    if (enemy != null && enemy.gameObject.activeSelf && !enemy.IsDead) count++;
                return count;
            }
        }

        public void Configure(string land, Enemy[] penned, PlayerDefence playerToWatch, AreaDirector area)
        {
            Land = land;
            enemies = penned;
            player = playerToWatch;
            director = area;
        }

        private void Start()
        {
            if (player != null) player.Revived += Clear;
            if (director != null) director.Rested += Clear;
        }

        private void OnDestroy()
        {
            if (player != null) player.Revived -= Clear;
            if (director != null) director.Rested -= Clear;
        }

        /// <summary>Lets out the enemies at these places in the pen's list, fresh and where they first stood; the rest are put away.</summary>
        public void Release(int[] which)
        {
            Released++;
            for (int i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null) continue;
                bool wanted = System.Array.IndexOf(which, i) >= 0;
                if (!wanted)
                {
                    if (enemy.gameObject.activeSelf) enemy.Restore();
                    enemy.gameObject.SetActive(false);
                    continue;
                }
                // One never let out has not woken yet: waking it stands it up fresh; one let out before is put back.
                if (!enemy.gameObject.activeSelf) enemy.gameObject.SetActive(true);
                enemy.Restore();
            }
            Vfx.Play(VfxKind.Ring, transform.position + Vector3.up * 0.2f, Vector3.up, 2f);
        }

        /// <summary>Puts every enemy of the pen away.</summary>
        public void Clear()
        {
            Cleared++;
            foreach (var enemy in enemies)
            {
                if (enemy == null || !enemy.gameObject.activeSelf) continue;
                // Put back first, so whatever it held (the player in a grip, a flask) is let go before it goes.
                enemy.Restore();
                enemy.gameObject.SetActive(false);
            }
        }
    }
}
