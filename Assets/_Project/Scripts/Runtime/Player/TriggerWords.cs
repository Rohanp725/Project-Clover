namespace Clover.Player
{
    /// <summary>
    /// Says in plain words what the player does to make a mark happen, for the cards over the test yard's stones and the
    /// chart. The buttons are named for the device in use, as the hint line at the foot of the screen does.
    /// </summary>
    public static class TriggerWords
    {
        /// <summary>The four spell slots' buttons ("L2 + Square, Triangle, Circle, Cross", or "1 2 3 4").</summary>
        public static string SpellButton => ControlNames.Spells(ControlDevices.Current);
        public static string SurgeButton => ControlNames.Of("Surge");

        /// <summary>How a mark is set going: a worn spell is cast, a mark with an effect waits for its moment, the rest are always on.</summary>
        public static string HowTo(Upgrade mark)
        {
            if (mark == null) return "";
            if (mark.IsSpell && mark.Spell != null)
            {
                string price = mark.Spell.Cost > 0f ? $" {mark.Spell.Cost:0} mana." : mark.Spell.Cooldown > 0f ? $" {mark.Spell.Cooldown:0} s cooldown." : "";
                return $"Put it in a spell slot at a rest, then cast it with that slot ({SpellButton}).{price}";
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
                case UpgradeTrigger.ChargedBlow: return "a blow of your charged blade lands";
                case UpgradeTrigger.ChargedBlowThird: return "the third blow of your charged blade lands";
                case UpgradeTrigger.Reacted: return "two afflictions react on an enemy";
                case UpgradeTrigger.ConditionBeat: return "a state this laid on an enemy comes to its beat";
                case UpgradeTrigger.ConditionStruck: return "you strike an enemy in a state this laid";
                case UpgradeTrigger.ConditionEnded: return "a state this laid on an enemy ends";
                case UpgradeTrigger.ConditionDied: return "an enemy dies in a state this laid";
                case UpgradeTrigger.TheirBlow: return "an enemy's blow lands on you or is turned";
                case UpgradeTrigger.ConditionAttacked: return "an enemy in a state this laid strikes at you";
                case UpgradeTrigger.RipeBurst: return "a ripe seed bursts";
                case UpgradeTrigger.ReactionStruck: return "a reaction strikes an enemy";
                case UpgradeTrigger.AfflictionLaid: return "an affliction is laid on an enemy";
                case UpgradeTrigger.DanceInTime: return "every cut of a dance lands in time";
                case UpgradeTrigger.GrownBurst: return "something you grew bursts";
                default: return "its moment comes";
            }
        }
    }
}
