using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Every outfit the player can wear, by id. Built by PlayerBuilder into Generated/Player/Outfits.asset from the
    /// character models, so nothing in it is set by hand.
    /// </summary>
    public class OutfitSet : ScriptableObject
    {
        [SerializeField] private Outfit[] outfits = new Outfit[0];

        public Outfit[] All => outfits;

        public void Configure(Outfit[] built) => outfits = built ?? new Outfit[0];

        public Outfit Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var outfit in outfits)
                if (outfit != null && outfit.Id == id && outfit.Mesh != null) return outfit;
            return null;
        }
    }
}
