using Clover.Core;
using Clover.Player;
using Clover.Player.Way;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// A Body piece lying in the world (way_plan.md 0.4; task 5.4): a vessel (+5 health), a breath stone (+5 stamina) or a
    /// flask shard (three make a flask), kept for good once taken (<see cref="BodyPieces"/>). Taking it says what it is in
    /// a short status line and the player's numbers grow at once. Where each land's pieces go is decided once its levels
    /// are signed off (way_build_plan.md question 8); until then they are tested in the Proving Ground.
    /// </summary>
    public class BodyPieceCache : Interactable
    {
        [SerializeField] private GameObject shown;

        /// <summary>What kind of piece it is.</summary>
        public BodyPiece Piece = BodyPiece.Vessel;
        /// <summary>Where it lies, unique for its kind: <c>fire</c>, <c>fire.god</c>.</summary>
        public string Where = "";

        public bool Taken { get; private set; }
        public override bool Available => !Taken;
        public string Key => BodyPieces.KeyOf(Piece, Where);

        public void Configure(BodyPiece piece, string where, GameObject whatIsShown)
        {
            Piece = piece;
            Where = where;
            shown = whatIsShown;
            Prompt = "Take";
        }

        // Found in an earlier sitting: it is not here again.
        private void Start()
        {
            if (string.IsNullOrEmpty(Key) || !Progress.Data.Found.Contains(Key)) return;
            Taken = true;
            if (shown != null) shown.SetActive(false);
        }

        public override void Use(PlayerInteraction who)
        {
            // One with no key could never be told apart from another: it gives nothing and is left as it is.
            if (Taken || who == null || string.IsNullOrEmpty(Key)) return;
            Taken = true;
            if (shown != null) shown.SetActive(false);
            if (!BodyPieces.Take(Key, Progress.Data.Found)) return;
            Progress.Save();
            // The numbers grow at once: the bars are the proof.
            var upgrades = who.GetComponent<PlayerUpgrades>();
            if (upgrades != null) upgrades.RefreshNow();
            Vfx.Play(VfxKind.Heal, who.transform.position + Vector3.up, Vector3.up);
            if (who.Defence != null) who.Defence.Callout(BodyPieces.WordsOf(Piece));
        }
    }
}
