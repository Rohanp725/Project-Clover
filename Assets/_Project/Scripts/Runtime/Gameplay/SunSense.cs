using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// What a sun-blind priest knows. He cannot see: he finds someone who has stood in the sun without a break for a
    /// moment (a roll through a bar of light is too quick), someone close enough to feel the heat of, and for a few
    /// seconds anyone who trades blows near him. Plain C#: the enemy tells it each frame whether the target is lit
    /// and how far off, and asks whether the target is found.
    /// </summary>
    [Serializable]
    public class SunSense
    {
        /// <summary>Seconds in the sun without a break before the target is found.</summary>
        public float LitSeconds = 0.4f;
        /// <summary>Metres inside which a lit target is found.</summary>
        public float LitRange = 18f;
        /// <summary>Metres inside which the heat of a body gives a shaded target away.</summary>
        public float HeatRange = 2.5f;
        /// <summary>A blow given or taken within this many metres is felt, for this many seconds.</summary>
        public float FightRange = 8f;
        public float FightSeconds = 4f;

        public bool Found { get; private set; }
        /// <summary>True while the target is found and standing in the sun: only then can a ray be thrown.</summary>
        public bool FoundLit { get; private set; }

        private float litFor;
        private float fightLeft;

        public void Reset()
        {
            Found = FoundLit = false;
            litFor = 0f;
            fightLeft = 0f;
        }

        /// <summary>A blow was given or taken this far from him.</summary>
        public void Fight(float distance)
        {
            if (distance <= FightRange) fightLeft = FightSeconds;
        }

        public bool Tick(float deltaTime, bool targetLit, float distance)
        {
            litFor = targetLit ? litFor + deltaTime : 0f;
            if (fightLeft > 0f) fightLeft -= deltaTime;
            bool bySun = targetLit && litFor >= LitSeconds && distance <= LitRange;
            FoundLit = bySun;
            Found = bySun || distance <= HeatRange || fightLeft > 0f;
            return Found;
        }
    }
}
