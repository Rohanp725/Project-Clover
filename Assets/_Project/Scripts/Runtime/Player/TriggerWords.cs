namespace Clover.Player
{
    /// <summary>
    /// Says in plain words what the player does to make a mark happen, for the cards over the test yard's stones.
    /// The words name both the pad's button and the keyboard's key, as the hint line at the foot of the screen does.
    /// </summary>
    public static class TriggerWords
    {
        public const string SpellButton = "Triangle (U)";
        public const string SurgeButton = "L2 (I)";

        /// <summary>How a mark is set going: a worn spell is cast, a mark with an effect waits for its moment, the rest are always on.</summary>
        public static string HowTo(Upgrade mark)
        {
            if (mark == null) return "";
            if (mark.IsSpell && mark.Spell != null)
            {
                string price = mark.Spell.Cost > 0f ? $" {mark.Spell.Cost:0} mana." : "";
                return $"Hold {SpellButton} to cast.{price}";
            }
            UpgradeEffect first = null;
            foreach (var effect in mark.Effects)
            {
                if (effect == null) continue;
                // The effect that begins the mark's own move says most about when it happens.
                if (effect.Do == UpgradeAction.Form) { first = effect; break; }
                if (first == null) first = effect;
            }
            if (first == null) return "Always on once it is yours.";
            string when = Of(first.On);
            if (first.WhileSurge && first.On != UpgradeTrigger.SurgeStart && first.On != UpgradeTrigger.SurgeEnd)
                when += $", while the surge lasts (press {SurgeButton})";
            return "Happens when " + when + ".";
        }

        public static string Of(UpgradeTrigger trigger)
        {
            switch (trigger)
            {
                case UpgradeTrigger.LightHit: return "a light blow lands";
                case UpgradeTrigger.HeavyHit: return "a heavy blow lands";
                case UpgradeTrigger.FinisherHit: return "a finisher lands (heavy on a staggered enemy)";
                case UpgradeTrigger.SkillHit: return "the weapon skill lands";
                case UpgradeTrigger.AnyHit: return "any weapon blow lands";
                case UpgradeTrigger.Kill: return "an enemy dies";
                case UpgradeTrigger.PerfectGuard: return "you guard at the last moment (a perfect guard)";
                case UpgradeTrigger.Guard: return "you block a blow";
                case UpgradeTrigger.PerfectDodge: return "you dodge at the last moment (a perfect dodge)";
                case UpgradeTrigger.DodgeStart: return "you dodge";
                case UpgradeTrigger.DodgeEnd: return "a dodge ends";
                case UpgradeTrigger.Struck: return "you are struck";
                case UpgradeTrigger.SpellCast: return $"you cast a spell ({SpellButton})";
                case UpgradeTrigger.SpellHit: return "a spell of yours strikes an enemy";
                case UpgradeTrigger.SurgeStart: return $"you light the surge (press {SurgeButton})";
                case UpgradeTrigger.SurgeEnd: return $"the surge ends (lit with {SurgeButton})";
                case UpgradeTrigger.FlaskDrunk: return "you drink a flask";
                case UpgradeTrigger.FlaskThrown: return "you throw a flask (hold the flask button)";
                case UpgradeTrigger.AfflictedHit: return "you strike an enemy that already carries an affliction";
                case UpgradeTrigger.AfflictionFull: return "an affliction on an enemy reaches its full depth";
                case UpgradeTrigger.EverySeconds: return "an enemy is near, every few seconds";
                case UpgradeTrigger.LowHealth: return "your health falls below a third";
                case UpgradeTrigger.ManaFull: return "your mana fills";
                case UpgradeTrigger.CritHit: return "a critical blow lands";
                case UpgradeTrigger.ChainEnd: return "the last blow of a chain lands";
                case UpgradeTrigger.SetOff: return "you set off an affliction";
                case UpgradeTrigger.Shatter: return "a frozen enemy is shattered";
                case UpgradeTrigger.WardSpent: return "a ward turns a blow";
                case UpgradeTrigger.Lifted: return "you throw an enemy into the air";
                case UpgradeTrigger.Landed: return "an enemy you threw comes down";
                case UpgradeTrigger.PoisonBite: return "poison bites an enemy";
                default: return "its moment comes";
            }
        }
    }
}
