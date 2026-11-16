using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>Something in the world the player can walk up to and use with the interact button.</summary>
    public abstract class Interactable : MonoBehaviour
    {
        /// <summary>What using it does, in a few words, shown while the player stands in range.</summary>
        public string Prompt = "Use";
        public float Range = 2.6f;

        /// <summary>False once there is nothing left to do here.</summary>
        public virtual bool Available => true;

        public abstract void Use(PlayerInteraction who);

        /// <summary>
        /// For a thing in the test yard whose gift depends on the element held: what it would give now, what that
        /// does and what sets it going, as a few short lines for its <see cref="StoneCard"/>. Empty for the rest.
        /// </summary>
        public virtual string Card(PlayerElement element) => "";

        /// <summary>Changes whenever <see cref="Card"/> would say something else though the element is the same (a gift already given).</summary>
        public virtual string CardStamp => "";
    }
}
