using System;
using UnityEngine;

namespace Clover.Player.Swings
{
    /// <summary>
    /// One layer of an element's swing trail: where on the blade line it is laid, how long each part of it lasts, its
    /// colour from head to tail, and how its element moves it once it is left behind (fire licks up, water and venom
    /// fall, wind blows on and apart, shade lags and drains back to the blade, ice freezes where it is and shatters).
    /// </summary>
    [Serializable]
    public struct TrailLayer
    {
        public TrailForm Form;
        /// <summary>A sweep: the window of the weapon's own trail (<see cref="SwingShape.SweepFrom"/> to <see cref="SwingShape.SweepTo"/>) it covers, 0 to 1.</summary>
        public float From, To;
        /// <summary>A strand: where on the blade line it follows (0 base, 1 tip) and its width in metres before the weapon's shape scales it.</summary>
        public float At, Width;
        /// <summary>Seconds each part of it lasts, before the weapon's shape scales it.</summary>
        public float Seconds;
        public GlowStyle Style;
        /// <summary>Its colour where it leaves the blade and at the end of its life; alpha included.</summary>
        public Color Head, Tail;
        /// <summary>Metres a second it drifts up (fire) or down (water, venom, dust) once left.</summary>
        public float Rise;
        /// <summary>Metres a second it carries on the way the blade was going (wind blowing on).</summary>
        public float Drift;
        /// <summary>Metres a second its parts are blown apart, each its own way across the trail.</summary>
        public float Scatter;
        /// <summary>The share of its width drawn back to its inner edge over its life (shade draining).</summary>
        public float Drain;
        /// <summary>Seconds it is drawn behind the blade: a smear that lags.</summary>
        public float Lag;
        /// <summary>The share of its life it stands whole and still after the swing before it goes (ice freezing in the air).</summary>
        public float Hold;
        /// <summary>Metres its parts are thrown aside, thrown again twenty times a second (lightning).</summary>
        public float Jitter;
        /// <summary>How much thinner its tail is than its head, 0 to 1; below 0 it spreads as it goes (dust, mist, wind).</summary>
        public float Taper;
        /// <summary>How fast its outer edge climbs once left, in its own widths a second (for <see cref="Ribbon.LickSeconds"/>), in tongues of flame each its own height (fire licking).</summary>
        public float Lick;
        /// <summary>How many parts of the trail one tongue of <see cref="Lick"/> spans.</summary>
        public float LickEvery;
    }
}
