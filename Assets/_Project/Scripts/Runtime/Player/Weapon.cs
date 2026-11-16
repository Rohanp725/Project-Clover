using System.Collections.Generic;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>One weapon technique: its attacks and how it is shown. The player holds one at a time.</summary>
    public class Weapon
    {
        public string Name;
        public WeaponCombo Combo;
        /// <summary>True when the character carries the sword: sheath on the hip, blade in the hand for attacks.</summary>
        public bool UsesSword;
        /// <summary>The colour of the arc its blows leave; a light value that may pass white.</summary>
        public Color SlashColour = new Color(2.2f, 2.6f, 3.2f, 0.9f);
        /// <summary>The name of its skill, for the screen.</summary>
        public string SkillName = "";
        /// <summary>The animation state held while the heavy button charges the skill.</summary>
        public string ChargeAnimation = "charge";

        /// <summary>Every attack the weapon has, for checks that each one has an animation.</summary>
        public IEnumerable<AttackStep> AllSteps()
        {
            foreach (var step in Combo.Steps) yield return step;
            if (Combo.Heavy != null) yield return Combo.Heavy;
            if (Combo.ChainHeavy != null) yield return Combo.ChainHeavy;
            if (Combo.Skill != null) yield return Combo.Skill;
            if (Combo.Finisher != null) yield return Combo.Finisher;
        }
    }
}
