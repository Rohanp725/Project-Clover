using System.Collections.Generic;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// One weapon technique: its attacks, how it is carried and shown, and how its bearer guards and dodges. The player
    /// holds one at a time. A new weapon is a new entry in <see cref="Weapons"/>, a model under its <see cref="Carry"/>
    /// name in the weapon builder, and clips under its <see cref="Prefix"/>; nothing else needs to know it exists.
    /// </summary>
    public class Weapon
    {
        public string Name;
        /// <summary>What the screen calls it; empty means <see cref="Name"/>. The Armament's is its element's name and "Armament".</summary>
        public string HudName = "";
        public WeaponCombo Combo;
        /// <summary>The technique the chord plays (R1 and R2 together), set by the armoury from the Way's marks in force; null: none, the chord is a heavy.</summary>
        [System.NonSerialized] public AttackStep Technique;
        /// <summary>The Way's aspect the armoury built it in, whose counts and windows the player keeps; null: the plain weapon.</summary>
        [System.NonSerialized] public Way.WeaponAspect Aspect;
        /// <summary>
        /// The move an infused strike plays with the element on it (steel/weapons.md 0.4): the Katana's drawing cut, the
        /// Hammer's drop. Null: the first light. Its shape (<see cref="Footprint"/>) is what each element's strike acts on.
        /// </summary>
        public AttackStep Carrier;
        public AttackStep CarrierStep => Carrier ?? (Combo != null && Combo.Steps.Count > 0 ? Combo.Steps[0] : Combo?.Heavy);
        /// <summary>Which set of objects on the character is this weapon: a name the <see cref="WeaponRack"/> knows. Empty: nothing is shown.</summary>
        public string Carry = "";
        /// <summary>The prefix of its animation clips: <c>idle_p</c>, <c>attack_p_1</c>. Empty for the Katana, whose clips have no prefix.</summary>
        public string Prefix = "";
        /// <summary>True for a weapon that leaves the hand when idle (the Katana in its sheath). Great weapons stay in the hand.</summary>
        public bool Stowed;
        /// <summary>Where on the body it sits: in the hand, in the off hand (a seam for the knives and the bow), and at rest.</summary>
        public string HandSocket = WeaponRack.HandRight;
        public string OffHandSocket = "";
        public string RestSocket = WeaponRack.Back;
        /// <summary>The colour of the arc its blows leave; a light value that may pass white.</summary>
        public Color SlashColour = new Color(2.2f, 2.6f, 3.2f, 0.9f);
        /// <summary>The name of its skill, for the screen.</summary>
        public string SkillName = "";
        /// <summary>The animation state held while the heavy button charges the skill.</summary>
        public string ChargeAnimation = "charge";
        /// <summary>The animation states of standing and running, the guard, the dodge, and the guard's two answers to a blow (empty: none is played).</summary>
        public string LocomotionState = "locomotion";
        public string GuardState = "guard";
        public string RollState = "roll";
        public string GuardHitState = "";
        public string PerfectGuardState = "";
        /// <summary>For a weapon that can leave the hand: how the arms wait for it, how it is caught, and the run after it. A state that was never baked is not played.</summary>
        public string AwayState = "";
        public string CatchState = "";
        public string DashState = "";
        /// <summary>The numbers of its guard and its dodge, copied onto the player's when it is taken up. Null: the usual ones.</summary>
        public Guard GuardProfile;
        public DodgeRoll DodgeProfile;
        /// <summary>The share of walking speed its bearer keeps, and keeps behind the guard.</summary>
        public float MoveFactor = 1f;
        public float GuardMoveFactor = 0.45f;
        /// <summary>What a blow's damage and stagger are multiplied by when it lands on the target's back (not a finisher, not a shot).</summary>
        public float BackMultiplier = 1f;
        /// <summary>Metres within which its attacks turn to face an enemy; zero means the usual. A bow looks further than a sword.</summary>
        public float AimRange;
        /// <summary>Full width in degrees, round where the stick points, in which its attacks pick an enemy to face; zero means the usual.</summary>
        public float AimConeDegrees;
        /// <summary>What a perfect guard with it does to the attacker besides: arrows left standing in it, metres it is yanked toward the bearer, stagger added.</summary>
        public int PerfectGuardPlants;
        public float PerfectGuardPull;
        public float PerfectGuardStagger;
        /// <summary>True for a weapon whose colours are the held element's (the Armament): its arcs follow the element.</summary>
        public bool ElementColoured;

        public const string KatanaCarry = "katana";
        /// <summary>True when the character carries the Katana: sheath on the hip, blade in the hand for attacks.</summary>
        public bool UsesSword
        {
            get => Carry == KatanaCarry;
            set { if (value) { Carry = KatanaCarry; Stowed = true; } else if (Carry == KatanaCarry) Carry = ""; }
        }

        public string ShownName => string.IsNullOrEmpty(HudName) ? Name : HudName;

        /// <summary>Every attack the weapon has, for checks that each one has an animation.</summary>
        public IEnumerable<AttackStep> AllSteps()
        {
            foreach (var step in Combo.Steps) yield return step;
            if (Combo.Heavy != null) yield return Combo.Heavy;
            if (Combo.ChainHeavy != null) yield return Combo.ChainHeavy;
            if (Combo.ChainHeavies != null)
                foreach (var step in Combo.ChainHeavies)
                    if (step != null) yield return step;
            if (Combo.ChargedHeavy != null) yield return Combo.ChargedHeavy;
            if (Combo.GuardAttack != null) yield return Combo.GuardAttack;
            if (Combo.DashAttack != null) yield return Combo.DashAttack;
            if (Combo.Skill != null) yield return Combo.Skill;
            if (Combo.Skill != null && Combo.Skill.FollowStep != null) yield return Combo.Skill.FollowStep;
            if (Combo.Finisher != null) yield return Combo.Finisher;
            if (Carrier != null) yield return Carrier;
        }

        /// <summary>Sets the usual state names from the clip prefix: <c>locomotion_p</c>, <c>guard_p</c>, <c>roll_p</c> and the rest.</summary>
        public Weapon Named(string prefix, bool ownRoll)
        {
            Prefix = prefix;
            LocomotionState = "locomotion_" + prefix;
            GuardState = "guard_" + prefix;
            GuardHitState = "guard_hit_" + prefix;
            PerfectGuardState = "guard_perfect_" + prefix;
            ChargeAnimation = "charge_" + prefix;
            RollState = ownRoll ? "roll_" + prefix : "roll";
            AwayState = "away_" + prefix;
            CatchState = "catch_" + prefix;
            DashState = "dash_" + prefix;
            return this;
        }
    }
}
