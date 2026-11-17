namespace Clover.Player.Forms
{
    /// <summary>The solid shapes a spell form can ask <see cref="FormFx"/> for. <see cref="FormMeshes"/> draws each.</summary>
    public enum FormMesh
    {
        /// <summary>A throwing blade, pointing along +Z.</summary>
        Blade,
        /// <summary>A great sword, pointing along +Z, one metre long before it is scaled.</summary>
        Sword,
        /// <summary>A plain splinter of the element, pointing along +Z: a bullet, a drop of rain.</summary>
        Shard,
        /// <summary>A fang standing on the ground.</summary>
        Spike,
        /// <summary>A six-sided column standing on the ground.</summary>
        Pillar,
        /// <summary>A toothed wheel lying flat.</summary>
        Disc,
        /// <summary>A cut stone of twenty faces.</summary>
        Orb,
        /// <summary>A slab of wall standing across X.</summary>
        Slab,
        DragonSkull,
        DragonJaw,
        DragonSpine,
        /// <summary>A boring point along +Z.</summary>
        Drill,
        /// <summary>A small horned helper standing on the ground, facing +Z.</summary>
        Minion,
        /// <summary>Tongues of flame standing on the ground.</summary>
        Flame,
        /// <summary>A ball of flame flying along +Z, its tongues trailing behind.</summary>
        Fireball,
        /// <summary>One jagged length of lightning along +Z with a fork off it.</summary>
        Bolt,
        /// <summary>A cluster of ice crystals standing on the ground.</summary>
        Crystal,
        /// <summary>A breaking wave one metre wide across X, rolling toward +Z, its lip curling over.</summary>
        Crest,
        /// <summary>A drop of water flying along +Z, fat in front.</summary>
        Droplet,
        /// <summary>A spout of water standing on the ground.</summary>
        Spout,
        /// <summary>A rough block of rock standing on the ground.</summary>
        Rock,
        /// <summary>A lump of stone flying along +Z.</summary>
        Pebble,
        /// <summary>A flat pointed leaf along +Z.</summary>
        Leaf,
        /// <summary>A curling thorned vine standing on the ground.</summary>
        Vine,
        /// <summary>A flower of five petals lying open, facing up.</summary>
        Flower,
        /// <summary>A lumpy cloud about one metre across, centred.</summary>
        Cloud,
        /// <summary>A bubble with a smaller one behind it, along +Z.</summary>
        Bubble,
        /// <summary>A needle of light along +Z with a cross of flare at its foot.</summary>
        Ray,
        /// <summary>A curved streak of moving air along +Z.</summary>
        Gust,
        /// <summary>A line of foam one metre wide across X: what rides the lip of a wave.</summary>
        Foam,
    }
}
