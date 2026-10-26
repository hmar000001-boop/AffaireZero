using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using AffaireZero.Gameplay;
using AffaireZero.UI;
using AffaireZero.World;

namespace AffaireZero.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        public CaseManager Case { get; private set; }
        public GameUI UI { get; private set; }
        public PlayerController Player { get; private set; }
        public bool HasPreviousSave { get; private set; }
        public bool IsInvestigationStarted { get; private set; }

        private WorldBuilder world;

        private void Start()
        {
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 2;
            QualitySettings.shadowDistance = 24f;
            Physics.autoSyncTransforms = true;

            Case = new CaseManager();
            HasPreviousSave = Case.Load();
            world = new WorldBuilder();
            world.Build(this);
            Player = FindObjectOfType<PlayerController>();
            UI = new GameObject("Affaire Zero · UI").AddComponent<GameUI>();
            UI.Initialize(this, HasPreviousSave);
            if (Player != null) Player.UI = UI;
            IsInvestigationStarted = false;
        }

        public void StartInvestigation(bool resume)
        {
            if (!resume || !HasPreviousSave)
            {
                Case.StartNew();
                HasPreviousSave = true;
                if (Player != null) Player.ResetToStart();
            }
            IsInvestigationStarted = true;
            if (UI != null) UI.EnterInvestigation(!resume);
            if (Case.Solved && UI != null) UI.ShowSolution();
        }

        public void InspectEvidence(EvidenceSpot spot)
        {
            if (spot == null || UI == null || !IsInvestigationStarted) return;
            EvidenceDefinition data = CaseCatalog.Get(spot.InteractionId);
            if (data == null) return;
            bool isNew = Case.Discover(data.Id);
            UI.ShowEvidence(data, isNew);
        }

        public void TrySubmitTheory(string suspectId, HashSet<string> selectedEvidence)
        {
            if (UI == null || Case == null) return;
            if (selectedEvidence == null || selectedEvidence.Count < 3)
            {
                UI.ShowTheoryFeedback(Localization.T("not_enough"));
                return;
            }
            foreach (string id in selectedEvidence)
            {
                if (!Case.HasEvidence(id))
                {
                    UI.ShowTheoryFeedback(Localization.T("not_supported"));
                    return;
                }
            }
            if (suspectId == "elias" && Case.CanJustifyElias(selectedEvidence))
            {
                Case.MarkSolved();
                UI.ShowSolution();
            }
            else UI.ShowTheoryFeedback(Localization.T("not_supported"));
        }

        public void OpenHatchPuzzle()
        {
            if (UI == null || Case == null || !IsInvestigationStarted) return;
            if (Case.HasEvidence("passage"))
            {
                UI.ShowEvidence(CaseCatalog.Get("passage"), false);
                return;
            }
            bool ready = Case.HasEvidence("camera_loop") && Case.HasEvidence("breaker") && Case.HasEvidence("speaker");
            UI.OpenHatchPuzzle(ready);
        }

        public void SubmitHatchCode(string code)
        {
            if (UI == null || Case == null) return;
            bool ready = Case.HasEvidence("camera_loop") && Case.HasEvidence("breaker") && Case.HasEvidence("speaker");
            if (!ready) { UI.OpenHatchPuzzle(false); return; }
            if (string.Equals((code ?? string.Empty).Trim(), "111416", System.StringComparison.Ordinal))
            {
                bool isNew = Case.Discover("passage");
                UI.ShowEvidence(CaseCatalog.Get("passage"), isNew);
            }
            else UI.ShowHatchFeedback(Localization.T("wrong_code"));
        }

        public void ReturnToScene()
        {
            if (UI != null) UI.EnterInvestigation();
            if (Player != null) Player.ClearNearby();
        }

        public void SelectLanguage(GameLanguage language)
        {
            Localization.SetLanguage(language);
            if (Case != null) Case.Save();
            if (UI != null) UI.RefreshAll();
        }
    }
}
