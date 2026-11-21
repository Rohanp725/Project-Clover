namespace Clover.Player.Forms
{
    /// <summary>
    /// Three Weasels' first weasel (the owner's answer of 2026-11-20): a short rush of wind that stops just in front of the
    /// enemy faced, still facing it, and cuts it there. It does not pass through: the second weasel is the one that goes
    /// through and out beyond. Everything else is as <see cref="StepForm"/> has it, its promised critical too.
    /// Reads: Reach, Damage, Stagger, Stacks, Seconds.
    /// </summary>
    // NEEDS: a look of its own, a gust that runs ahead of the player and a sickle cut in front of the enemy (looks); now the
    // step's streak and cut, stopped short.
    public class RushForm : StepForm
    {
        public override string Id => SpellForms.Rush;

        protected override bool Through => false;

        protected override float RunTo(float gap, float reach) => FormRules.RushRun(gap, reach);
    }
}
