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
    }
}
