namespace Clover.Player.Forms
{
    /// <summary>
    /// Lightning's Waiting Thunder: bolts of lightning hang at the shoulder, crackling, and do nothing by themselves.
    /// Each light blow of the weapon that lands sends one down on the enemy it struck; a heavy blow that lands sends
    /// every one that is left. What has not flown when the time is up goes out. Everything else is as
    /// <see cref="PhalanxForm"/> has it.
    /// Reads: Count, Seconds, Damage, Stagger, Speed.
    /// </summary>
    public class WaitingThunderForm : PhalanxForm
    {
        public override string Id => SpellForms.Waiting;

        protected override FormMesh Piece => FormMesh.Bolt;

        protected override bool FliesByItself => false;
    }
}
