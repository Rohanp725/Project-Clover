using UnityEngine;

namespace Clover.Player.Swings
{
    /// <summary>
    /// The line along a weapon that sweeps when it is swung, written onto the weapon's prefab by the weapon builder from
    /// the model's own mesh (its edge slot's vertices, or its head's bounds), so a swing effect follows the real path of
    /// that weapon's edge through its animation. A prefab can carry more than one (the bow's two limbs). Read each frame
    /// through <see cref="World"/>; nothing is allocated.
    /// </summary>
    public class BladeLine : MonoBehaviour
    {
        [SerializeField] private BladeLineSpec line;

        /// <summary>The builder writes the measured line.</summary>
        public void Configure(BladeLineSpec measured) => line = measured;

        public BladeLineSpec Spec => line;
        public string Carry => line.Carry;
        public BladeKind Kind => line.Kind;
        public int Count => line.Points != null ? line.Points.Length : 0;

        /// <summary>A point of the line where it is in the world now.</summary>
        public Vector3 World(int index) => transform.TransformPoint(line.Points[index]);
    }
}
