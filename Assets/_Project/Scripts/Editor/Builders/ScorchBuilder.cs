using Clover.Gameplay;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds the pieces burning ground is drawn with (<see cref="ScorchedGround"/>): for each lane and disc a quad of char
    /// with ember cracks and a field of flames over it, and for each ring a wall of flame. The meshes are made by the
    /// component when it wakes; here are only the renderers and the two shared materials. Used by the Sun-Bearer (its
    /// hot floor) and by the two spearmen (the planted spear's ring of fire).
    /// </summary>
    public static class ScorchBuilder
    {
        public static ScorchedGround Build(Transform parent, string name, int lanes, int discs, int rings)
        {
            string folder = BuildKit.GeneratedRoot + "/Enemies";
            var ground = BuildKit.Scorch(folder, "m_scorch_ground");
            var flames = BuildKit.FlameField(folder, "m_flame_field");
            var holder = BuildKit.Group(name, parent);
            var laneGround = new MeshRenderer[lanes];
            var laneFlames = new MeshRenderer[lanes];
            for (int i = 0; i < lanes; i++)
            {
                laneGround[i] = Piece(holder, "Lane Char " + (i + 1), ground);
                laneFlames[i] = Piece(holder, "Lane Flames " + (i + 1), flames);
            }
            var discGround = new MeshRenderer[discs];
            var discFlames = new MeshRenderer[discs];
            for (int i = 0; i < discs; i++)
            {
                discGround[i] = Piece(holder, "Disc Char " + (i + 1), ground);
                discFlames[i] = Piece(holder, "Disc Flames " + (i + 1), flames);
            }
            var ringFlames = new MeshRenderer[rings];
            for (int i = 0; i < rings; i++) ringFlames[i] = Piece(holder, "Ring Flames " + (i + 1), flames);
            var scorch = holder.gameObject.AddComponent<ScorchedGround>();
            scorch.Configure(laneGround, laneFlames, discGround, discFlames, ringFlames);
            return scorch;
        }

        private static MeshRenderer Piece(Transform holder, string name, Material material)
        {
            var piece = new GameObject(name);
            piece.transform.SetParent(holder, false);
            piece.AddComponent<MeshFilter>();
            var shown = piece.AddComponent<MeshRenderer>();
            shown.sharedMaterial = material;
            shown.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            shown.receiveShadows = false;
            shown.enabled = false;
            return shown;
        }
    }
}
