using UnityEngine;
using AffaireZero.Core;

namespace AffaireZero.Gameplay
{
    public abstract class WorldInteractable : MonoBehaviour
    {
        public string InteractionId;
        public abstract string Prompt { get; }
        public abstract void Activate();
    }

    public sealed class EvidenceSpot : WorldInteractable
    {
        public GameBootstrap Owner;
        public override string Prompt
        {
            get
            {
                var clue = CaseCatalog.Get(InteractionId);
                if (clue == null) return Localization.T("inspect");
                return Localization.Pick("Inspect · " + clue.TitleEn, "افحص · " + clue.TitleAr);
            }
        }
        public override void Activate() { if (Owner != null) Owner.InspectEvidence(this); }
    }

    /// <summary>The archive hatch is a small evidence-gated escape-room puzzle, not an immediately collectible clue.</summary>
    public sealed class HatchPuzzleSpot : WorldInteractable
    {
        public GameBootstrap Owner;
        public override string Prompt => Localization.Pick("Inspect service hatch", "افحص فتحة الخدمة");
        public override void Activate() { if (Owner != null) Owner.OpenHatchPuzzle(); }
    }
}
