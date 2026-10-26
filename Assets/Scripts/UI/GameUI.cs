using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using AffaireZero.Core;
using AffaireZero.Gameplay;

namespace AffaireZero.UI
{
    public sealed class GameUI : MonoBehaviour
    {
        private GameBootstrap owner;
        private Canvas canvas;
        private GameObject menuRoot, hudRoot, modalRoot;
        private RectTransform modalPanel;
        private Text evidenceCounter, nearbyPrompt, titleLabel, languageLabel;
        private Button interactButton;
        private WorldInteractable nearby;
        private VirtualJoystick joystick;
        private LookPad lookPad;
        private readonly HashSet<string> selectedEvidence = new HashSet<string>();
        private string selectedSuspect = string.Empty;
        private string modalKind = string.Empty;
        private string modalArgument = string.Empty;
        private GameLanguage menuLanguage = GameLanguage.English;
        private bool hatchHintVisible;
        private int investigationHintLevel;
        private Font uiFont;

        public bool IsPlaying => hudRoot != null && hudRoot.activeSelf && (modalRoot == null || !modalRoot.activeSelf);
        public Vector2 MoveInput => joystick != null ? joystick.Value : Vector2.zero;

        private static readonly Color Ink = new Color(0.035f, 0.047f, 0.063f, 0.97f);
        private static readonly Color Panel = new Color(0.07f, 0.086f, 0.106f, 0.985f);
        private static readonly Color PanelRaised = new Color(0.105f, 0.13f, 0.15f, 0.98f);
        private static readonly Color Amber = new Color(0.93f, 0.55f, 0.19f, 1f);
        private static readonly Color Teal = new Color(0.13f, 0.58f, 0.63f, 1f);
        private static readonly Color OffWhite = new Color(0.88f, 0.87f, 0.81f, 1f);
        private static readonly Color Muted = new Color(0.57f, 0.62f, 0.65f, 1f);

        public void Initialize(GameBootstrap bootstrap, bool hasSave)
        {
            owner = bootstrap;
            uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildCanvas();
            BuildMenu(hasSave);
            BuildHud();
            modalRoot = MakeRect("Modal overlay", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            modalRoot.GetComponent<RectTransform>().SetAsLastSibling();
            var overlay = modalRoot.AddComponent<Image>(); overlay.color = new Color(0.008f, 0.014f, 0.022f, 0.88f);
            modalRoot.SetActive(false);
            ShowMenu(hasSave);
            RefreshAll();
        }

        private void BuildCanvas()
        {
            var go = new GameObject("Affaire Zero Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.pixelPerfect = false;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight = 0.42f;
            if (FindObjectOfType<EventSystem>() == null) new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private void BuildMenu(bool hasSave)
        {
            menuRoot = MakeRect("Title screen", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            Image bg = menuRoot.AddComponent<Image>(); bg.color = Ink;
            AddDecoration(menuRoot.transform);
            AddText(menuRoot.transform, "AZ / 01", new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.88f), 30, Amber, TextAnchor.MiddleCenter, FontStyle.Bold);
            AddText(menuRoot.transform, "AFFAIRE\nZÉRO", new Vector2(0.06f, 0.54f), new Vector2(0.94f, 0.78f), 68, OffWhite, TextAnchor.MiddleCenter, FontStyle.Bold);
            AddText(menuRoot.transform, Localization.T("tagline"), new Vector2(0.08f, 0.49f), new Vector2(0.92f, 0.55f), 17, Muted, TextAnchor.MiddleCenter, FontStyle.Normal);
            AddText(menuRoot.transform, Localization.T("chapter"), new Vector2(0.07f, 0.44f), new Vector2(0.93f, 0.48f), 14, Amber, TextAnchor.MiddleCenter, FontStyle.Bold);
            AddButton(menuRoot.transform, Localization.T("start"), new Vector2(0.12f, 0.31f), new Vector2(0.88f, 0.375f), () => owner.StartInvestigation(false), Amber, Ink, 17, true);
            if (hasSave)
                AddButton(menuRoot.transform, Localization.T("continue"), new Vector2(0.12f, 0.23f), new Vector2(0.88f, 0.29f), () => owner.StartInvestigation(true), PanelRaised, OffWhite, 16, false);
            AddButton(menuRoot.transform, Localization.T("language"), new Vector2(0.2f, 0.12f), new Vector2(0.8f, 0.18f), ToggleLanguage, PanelRaised, OffWhite, 15, false);
            AddText(menuRoot.transform, Localization.T("credits"), new Vector2(0.07f, 0.045f), new Vector2(0.93f, 0.085f), 12, Muted, TextAnchor.MiddleCenter, FontStyle.Normal);
        }

        private void AddDecoration(Transform parent)
        {
            for (int i = 0; i < 7; i++)
            {
                var rect = MakeRect("Blueprint line", parent, new Vector2(0.03f + i * 0.155f, 0.14f), new Vector2(0.031f + i * 0.155f, 0.9f), Vector2.zero, Vector2.zero);
                var img = rect.gameObject.AddComponent<Image>(); img.color = new Color(0.28f, 0.37f, 0.4f, 0.12f);
            }
            var stripe = MakeRect("Accent stripe", parent, new Vector2(0.08f, 0.405f), new Vector2(0.92f, 0.408f), Vector2.zero, Vector2.zero);
            stripe.gameObject.AddComponent<Image>().color = new Color(Amber.r, Amber.g, Amber.b, 0.8f);
        }

        private void BuildHud()
        {
            hudRoot = MakeRect("Investigation HUD", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            hudRoot.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            // Transparent right-side drag area, created before controls so buttons receive input first.
            var lookRect = MakeRect("Look drag area", hudRoot.transform, new Vector2(0.42f, 0.2f), new Vector2(1f, 0.87f), Vector2.zero, Vector2.zero);
            var lookImage = lookRect.gameObject.AddComponent<Image>(); lookImage.color = new Color(1, 1, 1, 0.001f); lookImage.raycastTarget = true;
            lookPad = lookRect.gameObject.AddComponent<LookPad>();
            var header = MakeRect("HUD top bar", hudRoot.transform, new Vector2(0.025f, 0.91f), new Vector2(0.975f, 0.985f), Vector2.zero, Vector2.zero);
            header.gameObject.AddComponent<Image>().color = new Color(0.025f, 0.04f, 0.052f, 0.88f);
            titleLabel = AddText(header, Localization.T("chapter"), new Vector2(0.025f, 0.1f), new Vector2(0.7f, 0.9f), 15, OffWhite, TextAnchor.MiddleLeft, FontStyle.Bold);
            evidenceCounter = AddText(hudRoot.transform, Localization.T("case_progress") + "  0 / 8", new Vector2(0.035f, 0.855f), new Vector2(0.39f, 0.90f), 14, Amber, TextAnchor.MiddleLeft, FontStyle.Bold);
            AddButton(hudRoot.transform, Localization.T("file"), new Vector2(0.61f, 0.845f), new Vector2(0.76f, 0.90f), OpenDossier, Panel, OffWhite, 13, false);
            AddButton(hudRoot.transform, Localization.T("theory"), new Vector2(0.775f, 0.845f), new Vector2(0.925f, 0.90f), OpenTheory, Amber, Ink, 13, true);
            AddButton(hudRoot.transform, Localization.Pick("EN / AR", "عربي / English"), new Vector2(0.84f, 0.935f), new Vector2(0.965f, 0.973f), ToggleLanguage, PanelRaised, OffWhite, 11, false);
            // Minimal crosshair.
            var cross = MakeRect("Crosshair", hudRoot.transform, new Vector2(0.493f, 0.499f), new Vector2(0.507f, 0.501f), Vector2.zero, Vector2.zero);
            cross.gameObject.AddComponent<Image>().color = new Color(0.95f, 0.82f, 0.6f, 0.88f);
            nearbyPrompt = AddText(hudRoot.transform, "", new Vector2(0.45f, 0.19f), new Vector2(0.96f, 0.235f), 13, OffWhite, TextAnchor.MiddleCenter, FontStyle.Bold);
            interactButton = AddButton(hudRoot.transform, Localization.T("inspect"), new Vector2(0.66f, 0.08f), new Vector2(0.96f, 0.155f), () => { if (owner != null && owner.Player != null) owner.Player.InteractNearby(); }, Amber, Ink, 16, true);
            // Joystick well and movable knob.
            var joyRect = MakeRect("Movement joystick", hudRoot.transform, new Vector2(0.035f, 0.035f), new Vector2(0.235f, 0.205f), Vector2.zero, Vector2.zero);
            var joyImage = joyRect.gameObject.AddComponent<Image>(); joyImage.color = new Color(0.08f, 0.12f, 0.15f, 0.58f);
            var outline = joyRect.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(0.72f, 0.58f, 0.39f, 0.35f); outline.effectDistance = new Vector2(2, -2);
            var knob = MakeRect("Joystick knob", joyRect, new Vector2(0.29f, 0.29f), new Vector2(0.71f, 0.71f), Vector2.zero, Vector2.zero);
            knob.gameObject.AddComponent<Image>().color = new Color(0.82f, 0.67f, 0.46f, 0.68f);
            joystick = joyRect.gameObject.AddComponent<VirtualJoystick>(); joystick.Knob = knob;
            AddText(hudRoot.transform, Localization.T("move_hint"), new Vector2(0.055f, 0.005f), new Vector2(0.215f, 0.035f), 10, Muted, TextAnchor.MiddleCenter, FontStyle.Bold);
            AddText(hudRoot.transform, Localization.T("look_hint"), new Vector2(0.65f, 0.02f), new Vector2(0.96f, 0.055f), 10, Muted, TextAnchor.MiddleCenter, FontStyle.Bold);
        }

        private void ShowMenu(bool hasSave)
        {
            menuRoot.SetActive(true); hudRoot.SetActive(false); modalRoot.SetActive(false);
        }

        public void EnterInvestigation(bool showBriefing = false)
        {
            menuRoot.SetActive(false); hudRoot.SetActive(true); modalRoot.SetActive(false); modalKind = string.Empty;
            selectedEvidence.Clear(); selectedSuspect = string.Empty;
            RefreshEvidenceCounter();
            if (showBriefing) ShowBriefing();
        }

        private void ReturnToScene()
        {
            if (owner != null) owner.ReturnToScene();
            else if (modalRoot != null) modalRoot.SetActive(false);
        }

        public void SetNearby(WorldInteractable target)
        {
            nearby = target;
            bool visible = target != null && IsHudVisible();
            if (interactButton != null) interactButton.gameObject.SetActive(visible);
            if (nearbyPrompt != null)
            {
                nearbyPrompt.text = visible ? target.Prompt : string.Empty;
                nearbyPrompt.gameObject.SetActive(visible);
            }
        }

        private bool IsHudVisible() => hudRoot != null && hudRoot.activeSelf && modalRoot != null && !modalRoot.activeSelf;

        public void ShowEvidence(EvidenceDefinition clue, bool isNew)
        {
            modalKind = "evidence"; modalArgument = clue.Id;
            BuildModalBase(clue.Title, () => ReturnToScene());
            AddText(modalPanel, isNew ? Localization.T("found") : Localization.Pick("ALREADY LOGGED", "تم تسجيله سابقًا"), new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.9f), 12, Amber, TextAnchor.MiddleCenter, FontStyle.Bold);
            AddText(modalPanel, clue.Description, new Vector2(0.09f, 0.28f), new Vector2(0.91f, 0.82f), 18, OffWhite, TextAnchor.UpperLeft, FontStyle.Normal);
            AddText(modalPanel, Localization.Pick("LOCATION  ·  " + clue.RoomEn.ToUpperInvariant(), "الموقع  ·  " + clue.RoomAr), new Vector2(0.08f, 0.2f), new Vector2(0.92f, 0.26f), 13, Teal, TextAnchor.MiddleCenter, FontStyle.Bold);
            AddButton(modalPanel, Localization.T("close"), new Vector2(0.15f, 0.07f), new Vector2(0.85f, 0.145f), ReturnToScene, Amber, Ink, 15, true);
            modalRoot.SetActive(true);
        }

        private void ShowBriefing()
        {
            modalKind = "briefing"; modalArgument = string.Empty;
            BuildModalBase(Localization.T("briefing_title"), null);
            AddText(modalPanel, Localization.T("briefing"), new Vector2(0.09f, 0.24f), new Vector2(0.91f, 0.84f), 19, OffWhite, TextAnchor.UpperLeft, FontStyle.Normal);
            AddText(modalPanel, Localization.T("chapter"), new Vector2(0.08f, 0.16f), new Vector2(0.92f, 0.22f), 12, Teal, TextAnchor.MiddleCenter, FontStyle.Bold);
            AddButton(modalPanel, Localization.Pick("ENTER THE SCENE", "دخول المشهد"), new Vector2(0.12f, 0.06f), new Vector2(0.88f, 0.13f), ReturnToScene, Amber, Ink, 15, true);
            modalRoot.SetActive(true);
        }

        public void OpenDossier() => BuildDossier("evidence");

        private void BuildDossier(string tab)
        {
            modalKind = "dossier"; modalArgument = tab;
            BuildModalBase(Localization.T("file"), null);
            AddButton(modalPanel, Localization.T("evidence_tab"), new Vector2(0.06f, 0.85f), new Vector2(0.36f, 0.92f), () => BuildDossier("evidence"), tab == "evidence" ? Amber : PanelRaised, tab == "evidence" ? Ink : OffWhite, 12, tab == "evidence");
            AddButton(modalPanel, Localization.T("suspects_tab"), new Vector2(0.37f, 0.85f), new Vector2(0.68f, 0.92f), () => BuildDossier("interviews"), tab == "interviews" ? Amber : PanelRaised, tab == "interviews" ? Ink : OffWhite, 12, tab == "interviews");
            AddButton(modalPanel, Localization.T("notes_tab"), new Vector2(0.69f, 0.85f), new Vector2(0.94f, 0.92f), () => BuildDossier("notes"), tab == "notes" ? Amber : PanelRaised, tab == "notes" ? Ink : OffWhite, 11, tab == "notes");
            var scroll = CreateScrollList(modalPanel, new Vector2(0.055f, 0.14f), new Vector2(0.945f, 0.82f));
            if (tab == "evidence")
            {
                foreach (EvidenceDefinition clue in CaseCatalog.Evidence)
                {
                    bool found = owner.Case.HasEvidence(clue.Id);
                    string title = found ? "✓  " + clue.Title + "   ·   " + clue.Room : "—  " + Localization.Pick("Undiscovered evidence", "دليل لم يُكتشف بعد");
                    AddListButton(scroll, title, () => { if (found) ShowEvidence(clue, false); }, found ? PanelRaised : Panel, found ? OffWhite : Muted, 70, found);
                }
            }
            else if (tab == "interviews")
            {
                AddListText(scroll, Localization.T("interviews_intro"), 96, Teal);
                AddListButton(scroll, Localization.T("interview_nora"), () => OpenInterview("nora"), PanelRaised, OffWhite, 76, true);
                AddListButton(scroll, Localization.T("interview_malik"), () => OpenInterview("malik"), PanelRaised, OffWhite, 76, true);
                AddListButton(scroll, Localization.T("interview_elias"), () => OpenInterview("elias"), PanelRaised, OffWhite, 76, true);
                AddListText(scroll, Localization.T("interview_hint"), 84, Muted);
            }
            else
            {
                AddListText(scroll, Localization.T("case_note_1"), 90, OffWhite);
                AddListText(scroll, Localization.T("case_note_2"), 90, OffWhite);
                AddListText(scroll, Localization.T("case_note_3"), 90, OffWhite);
                if (investigationHintLevel >= 1) AddListText(scroll, Localization.T("investigation_hint_1"), 110, Teal);
                if (investigationHintLevel >= 2) AddListText(scroll, Localization.T("investigation_hint_2"), 110, Teal);
                if (investigationHintLevel >= 3) AddListText(scroll, Localization.T("investigation_hint_3"), 132, Amber);
                if (investigationHintLevel < 3)
                    AddListButton(scroll, Localization.T("investigation_hint_next"), () => { investigationHintLevel = Mathf.Min(3, investigationHintLevel + 1); BuildDossier("notes"); }, PanelRaised, OffWhite, 62, true);
                else AddListText(scroll, Localization.T("investigation_hint_complete"), 62, Amber);
                AddListText(scroll, Localization.Pick("The episode is fully playable offline. Purchases for future episodes will be enabled when those episodes are released.", "يمكن لعب هذه الحلقة كاملة دون اتصال. ستُتاح مشتريات الحلقات المستقبلية عند إصدارها."), 106, Teal);
            }
            AddButton(modalPanel, Localization.T("close"), new Vector2(0.34f, 0.045f), new Vector2(0.66f, 0.115f), ReturnToScene, PanelRaised, OffWhite, 13, false);
            modalRoot.SetActive(true);
        }

        public void OpenInterview(string suspect)
        {
            modalKind = "interview"; modalArgument = suspect;
            string title = suspect == "nora" ? Localization.T("interview_nora") : suspect == "malik" ? Localization.T("interview_malik") : Localization.T("interview_elias");
            BuildModalBase(title, null);
            string statement;
            if (suspect == "nora")
            {
                statement = Localization.T("nora_statement");
                if (owner.Case.HasEvidence("speaker")) statement += "\n\n" + Localization.Pick("When shown the sound routine, Nora admits Elias controlled the owner account and often tested house systems after midnight.", "عندما عُرض عليها جدول الصوت، أقرت نورا بأن إلياس كان يتحكم بحساب المالك ويختبر أنظمة المنزل بعد منتصف الليل.");
            }
            else if (suspect == "malik")
            {
                statement = Localization.T("malik_statement");
                if (owner.Case.HasEvidence("camera_loop") || owner.Case.HasEvidence("breaker")) statement += "\n\n" + Localization.Pick("Confronted with the loop, Malik clarifies that he checked the monitor image, not the live camera source. He had access, but this does not place him at the panel.", "بعد مواجهته بالمقطع المعاد، أوضح مالك أنه فحص صورة الشاشة لا مصدر الكاميرا المباشر. كان لديه صلاحية الوصول، لكن ذلك لا يثبت وجوده عند اللوحة.");
            }
            else
            {
                statement = Localization.T("elias_statement");
                if (owner.Case.HasEvidence("voice_note")) statement += "\n\n" + Localization.Pick("The recovered draft is more direct: Elias planned the timed footsteps, a clean door log, and the removal of the audit files that night.", "المسودة المستعادة أكثر وضوحًا: خطط إلياس لتشغيل الخطوات في موعد محدد والإبقاء على سجل الباب خاليًا ونقل ملفات التدقيق تلك الليلة.");
                if (owner.Case.HasEvidence("notebook") || owner.Case.HasEvidence("passage")) statement += "\n\n" + Localization.Pick("The annotated route and fresh hatch marks connect the statement to a usable exit, not merely a metaphor.", "يربط الطريق المعلّم والخدوش الحديثة على الفتحة العبارة بمخرج قابل للاستخدام، لا بمجرد استعارة.");
            }
            AddText(modalPanel, statement, new Vector2(0.09f, 0.22f), new Vector2(0.91f, 0.82f), 18, OffWhite, TextAnchor.UpperLeft, FontStyle.Normal);
            AddText(modalPanel, Localization.T("interview_hint"), new Vector2(0.08f, 0.16f), new Vector2(0.92f, 0.21f), 12, Teal, TextAnchor.MiddleCenter, FontStyle.Normal);
            AddButton(modalPanel, Localization.Pick("BACK TO CASE FILE", "العودة إلى ملف القضية"), new Vector2(0.1f, 0.06f), new Vector2(0.9f, 0.13f), () => BuildDossier("interviews"), PanelRaised, OffWhite, 14, false);
            modalRoot.SetActive(true);
        }

        public void OpenTheory()
        {
            selectedEvidence.Clear();
            selectedSuspect = string.Empty;
            BuildTheory();
        }

        private void BuildTheory()
        {
            modalKind = "theory"; modalArgument = string.Empty;
            BuildModalBase(Localization.T("select_suspect"), null);
            AddText(modalPanel, Localization.T("select_evidence"), new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.84f), 14, Muted, TextAnchor.MiddleCenter, FontStyle.Normal);
            AddButton(modalPanel, Localization.T("interview_nora"), new Vector2(0.06f, 0.69f), new Vector2(0.36f, 0.765f), () => { selectedSuspect = "nora"; BuildTheory(); }, selectedSuspect == "nora" ? Amber : PanelRaised, selectedSuspect == "nora" ? Ink : OffWhite, 11, selectedSuspect == "nora");
            AddButton(modalPanel, Localization.T("interview_malik"), new Vector2(0.37f, 0.69f), new Vector2(0.68f, 0.765f), () => { selectedSuspect = "malik"; BuildTheory(); }, selectedSuspect == "malik" ? Amber : PanelRaised, selectedSuspect == "malik" ? Ink : OffWhite, 11, selectedSuspect == "malik");
            AddButton(modalPanel, Localization.T("interview_elias"), new Vector2(0.69f, 0.69f), new Vector2(0.94f, 0.765f), () => { selectedSuspect = "elias"; BuildTheory(); }, selectedSuspect == "elias" ? Amber : PanelRaised, selectedSuspect == "elias" ? Ink : OffWhite, 10, selectedSuspect == "elias");
            var scroll = CreateScrollList(modalPanel, new Vector2(0.055f, 0.19f), new Vector2(0.945f, 0.66f));
            foreach (EvidenceDefinition clue in CaseCatalog.Evidence)
            {
                bool found = owner.Case.HasEvidence(clue.Id);
                bool selected = selectedEvidence.Contains(clue.Id);
                string title = (selected ? "☑  " : "□  ") + (found ? clue.Title : Localization.Pick("Undiscovered evidence", "دليل لم يُكتشف بعد"));
                AddListButton(scroll, title, () =>
                {
                    if (!found) return;
                    if (selectedEvidence.Contains(clue.Id)) selectedEvidence.Remove(clue.Id); else selectedEvidence.Add(clue.Id);
                    BuildTheory();
                }, selected ? new Color(0.19f, 0.26f, 0.25f) : (found ? PanelRaised : Panel), selected ? Amber : (found ? OffWhite : Muted), 61, found);
            }
            AddText(modalPanel, Localization.Pick("Selected: " + selectedEvidence.Count, "المحدد: " + ToArabicDigits(selectedEvidence.Count.ToString())), new Vector2(0.08f, 0.135f), new Vector2(0.92f, 0.18f), 12, Teal, TextAnchor.MiddleCenter, FontStyle.Bold);
            AddButton(modalPanel, Localization.T("confirm_theory"), new Vector2(0.08f, 0.055f), new Vector2(0.61f, 0.12f), SubmitTheory, Amber, Ink, 12, true);
            AddButton(modalPanel, Localization.T("close"), new Vector2(0.64f, 0.055f), new Vector2(0.92f, 0.12f), ReturnToScene, PanelRaised, OffWhite, 12, false);
            modalRoot.SetActive(true);
        }

        private void SubmitTheory()
        {
            if (string.IsNullOrEmpty(selectedSuspect)) { ShowTheoryFeedback(Localization.Pick("Select a suspect first.", "اختر مشتبهًا به أولًا.")); return; }
            owner.TrySubmitTheory(selectedSuspect, new HashSet<string>(selectedEvidence));
        }

        public void OpenHatchPuzzle(bool canAttempt)
        {
            hatchHintVisible = false;
            BuildHatchPuzzle(canAttempt);
        }

        private void BuildHatchPuzzle(bool canAttempt)
        {
            modalKind = canAttempt ? "hatch-puzzle" : "hatch-locked";
            modalArgument = string.Empty;
            BuildModalBase(Localization.T("hatch_puzzle_title"), null);
            string body = canAttempt ? Localization.T("hatch_puzzle_riddle") : Localization.T("hatch_locked");
            AddText(modalPanel, body, new Vector2(0.09f, 0.49f), new Vector2(0.91f, 0.82f), 18, OffWhite, TextAnchor.MiddleCenter, FontStyle.Normal);
            if (canAttempt)
            {
                if (hatchHintVisible) AddText(modalPanel, Localization.T("hatch_hint_text"), new Vector2(0.09f, 0.4f), new Vector2(0.91f, 0.49f), 13, Teal, TextAnchor.MiddleCenter, FontStyle.Normal);
                var input = AddCodeInput(modalPanel, Localization.T("code_placeholder"), new Vector2(0.15f, 0.27f), new Vector2(0.85f, 0.36f));
                AddButton(modalPanel, Localization.T("unlock_hatch"), new Vector2(0.12f, 0.14f), new Vector2(0.88f, 0.22f), () => owner.SubmitHatchCode(input.text), Amber, Ink, 14, true);
                AddButton(modalPanel, Localization.T("hint_button"), new Vector2(0.12f, 0.06f), new Vector2(0.48f, 0.12f), () => { hatchHintVisible = true; BuildHatchPuzzle(true); }, PanelRaised, OffWhite, 12, false);
                AddButton(modalPanel, Localization.T("close"), new Vector2(0.52f, 0.06f), new Vector2(0.88f, 0.12f), ReturnToScene, PanelRaised, OffWhite, 12, false);
            }
            else AddButton(modalPanel, Localization.T("close"), new Vector2(0.15f, 0.07f), new Vector2(0.85f, 0.145f), ReturnToScene, Amber, Ink, 15, true);
            modalRoot.SetActive(true);
        }

        public void ShowHatchFeedback(string message)
        {
            modalKind = "hatch-feedback"; modalArgument = message;
            BuildModalBase(Localization.T("hatch_puzzle_title"), null);
            AddText(modalPanel, message, new Vector2(0.09f, 0.29f), new Vector2(0.91f, 0.79f), 18, OffWhite, TextAnchor.MiddleCenter, FontStyle.Normal);
            AddButton(modalPanel, Localization.Pick("TRY AGAIN", "حاول مجددًا"), new Vector2(0.1f, 0.07f), new Vector2(0.56f, 0.15f), () => BuildHatchPuzzle(true), Amber, Ink, 13, true);
            AddButton(modalPanel, Localization.T("close"), new Vector2(0.6f, 0.07f), new Vector2(0.9f, 0.15f), ReturnToScene, PanelRaised, OffWhite, 13, false);
            modalRoot.SetActive(true);
        }

        private InputField AddCodeInput(Transform parent, string placeholderText, Vector2 anchorMin, Vector2 anchorMax)
        {
            var inputRect = MakeRect("Six digit code", parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            inputRect.gameObject.AddComponent<Image>().color = new Color(0.02f, 0.03f, 0.04f, 1f);
            var input = inputRect.gameObject.AddComponent<InputField>();
            input.contentType = InputField.ContentType.IntegerNumber;
            input.characterLimit = 6;
            input.lineType = InputField.LineType.SingleLine;
            input.keyboardType = TouchScreenKeyboardType.NumberPad;
            input.shouldHideMobileInput = false;
            var textRect = MakeRect("Entered code text", inputRect, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.92f), Vector2.zero, Vector2.zero);
            var text = textRect.gameObject.AddComponent<Text>(); text.font = uiFont; text.fontSize = 26; text.color = OffWhite; text.alignment = TextAnchor.MiddleCenter; text.supportRichText = false;
            input.textComponent = text;
            var placeholderRect = MakeRect("Code placeholder", inputRect, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.92f), Vector2.zero, Vector2.zero);
            var placeholder = placeholderRect.gameObject.AddComponent<Text>(); placeholder.font = uiFont; placeholder.fontSize = 17; placeholder.color = Muted; placeholder.alignment = TextAnchor.MiddleCenter; placeholder.text = placeholderText;
            input.placeholder = placeholder;
            return input;
        }

        public void ShowTheoryFeedback(string message)
        {
            modalKind = "feedback"; modalArgument = message;
            BuildModalBase(Localization.Pick("CONCLUSION NOT ACCEPTED", "لم يُقبل الاستنتاج"), null);
            AddText(modalPanel, message, new Vector2(0.09f, 0.27f), new Vector2(0.91f, 0.8f), 19, OffWhite, TextAnchor.MiddleCenter, FontStyle.Normal);
            AddButton(modalPanel, Localization.Pick("REVISE THEORY", "مراجعة الفرضية"), new Vector2(0.1f, 0.07f), new Vector2(0.9f, 0.15f), BuildTheory, Amber, Ink, 14, true);
            modalRoot.SetActive(true);
        }

        public void ShowSolution()
        {
            modalKind = "solution"; modalArgument = string.Empty;
            BuildModalBase(Localization.T("case_solved"), null);
            AddText(modalPanel, Localization.T("solution_title"), new Vector2(0.07f, 0.82f), new Vector2(0.93f, 0.9f), 18, Amber, TextAnchor.MiddleCenter, FontStyle.Bold);
            AddText(modalPanel, Localization.T("solution_body"), new Vector2(0.08f, 0.2f), new Vector2(0.92f, 0.8f), 16, OffWhite, TextAnchor.UpperLeft, FontStyle.Normal);
            AddText(modalPanel, Localization.T("solved_badge"), new Vector2(0.08f, 0.15f), new Vector2(0.92f, 0.19f), 11, Teal, TextAnchor.MiddleCenter, FontStyle.Bold);
            AddButton(modalPanel, Localization.T("continue_explore"), new Vector2(0.07f, 0.055f), new Vector2(0.49f, 0.12f), ReturnToScene, PanelRaised, OffWhite, 12, false);
            AddButton(modalPanel, Localization.T("quit"), new Vector2(0.52f, 0.055f), new Vector2(0.93f, 0.12f), ShowTitle, Amber, Ink, 12, true);
            modalRoot.SetActive(true);
            RefreshEvidenceCounter();
        }

        private void ShowTitle()
        {
            if (owner != null && owner.Case != null) owner.Case.Save();
            modalRoot.SetActive(false); hudRoot.SetActive(false); menuRoot.SetActive(true); modalKind = string.Empty;
        }

        private void BuildModalBase(string title, Action close)
        {
            modalRoot.SetActive(false);
            foreach (Transform child in modalRoot.transform) if (child != modalRoot.transform) Destroy(child.gameObject);
            // Keep the full-screen image; add a centered panel above it.
            var panelRect = MakeRect("Dialog panel", modalRoot.transform, new Vector2(0.045f, 0.035f), new Vector2(0.955f, 0.965f), Vector2.zero, Vector2.zero);
            modalPanel = panelRect;
            panelRect.gameObject.AddComponent<Image>().color = Panel;
            var outline = panelRect.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(Amber.r, Amber.g, Amber.b, 0.55f); outline.effectDistance = new Vector2(1f, -1f);
            AddText(panelRect, title, new Vector2(0.06f, 0.91f), new Vector2(0.86f, 0.98f), 20, Amber, TextAnchor.MiddleCenter, FontStyle.Bold);
            var line = MakeRect("Title underline", panelRect, new Vector2(0.08f, 0.9f), new Vector2(0.92f, 0.903f), Vector2.zero, Vector2.zero);
            line.gameObject.AddComponent<Image>().color = new Color(Amber.r, Amber.g, Amber.b, 0.62f);
            AddButton(panelRect, "×", new Vector2(0.88f, 0.92f), new Vector2(0.96f, 0.985f), close ?? (Action)ReturnToScene, PanelRaised, OffWhite, 20, false);
        }

        private RectTransform CreateScrollList(RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var viewportRect = MakeRect("List viewport", parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            var viewImage = viewportRect.gameObject.AddComponent<Image>(); viewImage.color = new Color(0, 0, 0, 0.12f);
            viewportRect.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var content = MakeRect("List content", viewportRect, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            content.pivot = new Vector2(0.5f, 1f);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter; layout.spacing = 7; layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childControlHeight = false; layout.childControlWidth = true; layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = viewportRect.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewportRect; scroll.content = content; scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 30f;
            return content;
        }

        private void AddListButton(RectTransform content, string label, Action action, Color background, Color textColor, float height, bool enabled)
        {
            var button = AddButton(content, label, Vector2.zero, Vector2.one, action, background, textColor, 13, background == Amber);
            var rect = button.GetComponent<RectTransform>(); rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1); rect.pivot = new Vector2(0.5f, 1); rect.anchoredPosition = Vector2.zero; rect.sizeDelta = new Vector2(0, height);
            var layout = button.gameObject.AddComponent<LayoutElement>(); layout.preferredHeight = height; layout.minHeight = height; layout.flexibleWidth = 1;
            button.interactable = enabled;
            Text text = button.GetComponentInChildren<Text>(); if (text != null) { text.alignment = Localization.Language == GameLanguage.Arabic ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft; text.supportRichText = false; }
        }

        private void AddListText(RectTransform content, string label, float height, Color color)
        {
            var rect = MakeRect("Case note row", content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(0.04f, 0.055f, 0.067f, 0.9f);
            var layout = rect.gameObject.AddComponent<LayoutElement>(); layout.preferredHeight = height; layout.minHeight = height; layout.flexibleWidth = 1;
            AddText(rect, label, new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f), 13, color, TextAnchor.MiddleLeft, FontStyle.Normal);
        }

        private Button AddButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Action action, Color background, Color foreground, int fontSize, bool emphasized)
        {
            var rect = MakeRect("Button · " + label, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Image image = rect.gameObject.AddComponent<Image>(); image.color = background;
            Button button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            ColorBlock colors = button.colors; colors.normalColor = background; colors.highlightedColor = Color.Lerp(background, Color.white, 0.12f); colors.pressedColor = Color.Lerp(background, Color.black, 0.16f); colors.selectedColor = background; colors.disabledColor = new Color(background.r, background.g, background.b, 0.5f); colors.colorMultiplier = 1f; button.colors = colors;
            var outline = rect.gameObject.AddComponent<Outline>(); outline.effectColor = emphasized ? new Color(Amber.r, Amber.g, Amber.b, 0.45f) : new Color(1, 1, 1, 0.06f); outline.effectDistance = new Vector2(1f, -1f);
            AddText(rect, label, new Vector2(0.035f, 0.04f), new Vector2(0.965f, 0.96f), fontSize, foreground, TextAnchor.MiddleCenter, emphasized ? FontStyle.Bold : FontStyle.Normal);
            if (action != null) button.onClick.AddListener(() => action());
            return button;
        }

        private Text AddText(Transform parent, string value, Vector2 anchorMin, Vector2 anchorMax, int size, Color color, TextAnchor alignment, FontStyle style)
        {
            var rect = MakeRect("Text · " + (value.Length > 20 ? value.Substring(0, 20) : value), parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = uiFont; text.text = value; text.fontSize = size; text.color = color; text.alignment = alignment; text.fontStyle = style;
            if (Localization.Language == GameLanguage.Arabic)
            {
                if (alignment == TextAnchor.UpperLeft) text.alignment = TextAnchor.UpperRight;
                else if (alignment == TextAnchor.MiddleLeft) text.alignment = TextAnchor.MiddleRight;
            }
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Overflow; text.resizeTextForBestFit = true; text.resizeTextMinSize = Mathf.Max(10, size - 3); text.resizeTextMaxSize = size;
            text.raycastTarget = false; text.lineSpacing = 1.08f;
            return text;
        }

        private RectTransform MakeRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false); rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            return rect;
        }

        private void ToggleLanguage()
        {
            owner.SelectLanguage(Localization.Language == GameLanguage.English ? GameLanguage.Arabic : GameLanguage.English);
        }

        public void RefreshAll()
        {
            if (canvas == null) return;
            bool menuWasActive = menuRoot != null && menuRoot.activeSelf;
            bool hudWasActive = hudRoot != null && hudRoot.activeSelf;
            bool modalWasActive = modalRoot != null && modalRoot.activeSelf;
            string kind = modalKind, arg = modalArgument;

            if (menuRoot != null) Destroy(menuRoot);
            BuildMenu(owner != null && owner.HasPreviousSave);
            menuRoot.SetActive(menuWasActive);

            if (hudRoot != null) Destroy(hudRoot);
            BuildHud();
            hudRoot.SetActive(hudWasActive);
            SetNearby(nearby);
            RefreshEvidenceCounter();

            if (modalWasActive)
            {
                if (kind == "dossier") BuildDossier(arg);
                else if (kind == "interview") OpenInterview(arg);
                else if (kind == "theory") BuildTheory();
                else if (kind == "solution") ShowSolution();
                else if (kind == "evidence") { var clue = CaseCatalog.Get(arg); if (clue != null) ShowEvidence(clue, false); }
                else if (kind == "briefing") ShowBriefing();
                else if (kind == "feedback") ShowTheoryFeedback(arg);
                else if (kind == "hatch-puzzle") BuildHatchPuzzle(true);
                else if (kind == "hatch-locked") BuildHatchPuzzle(false);
                else if (kind == "hatch-feedback") ShowHatchFeedback(arg);
            }
            else if (modalRoot != null) modalRoot.SetActive(false);
            if (modalRoot != null) modalRoot.transform.SetAsLastSibling();
        }

        private void RefreshEvidenceCounter()
        {
            if (evidenceCounter != null && owner != null && owner.Case != null)
                evidenceCounter.text = Localization.Pick("EVIDENCE  " + owner.Case.EvidenceCount + " / " + CaseCatalog.Evidence.Count, "الأدلة  " + ToArabicDigits(owner.Case.EvidenceCount.ToString()) + " / " + ToArabicDigits(CaseCatalog.Evidence.Count.ToString()));
            if (interactButton != null && nearby == null) interactButton.gameObject.SetActive(false);
        }

        private static string ToArabicDigits(string value)
        {
            const string latin = "0123456789"; const string arabic = "٠١٢٣٤٥٦٧٨٩";
            for (int i = 0; i < latin.Length; i++) value = value.Replace(latin[i], arabic[i]);
            return value;
        }
    }

    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform Knob;
        public Vector2 Value { get; private set; }
        private RectTransform rect;
        private float radius;
        private void Awake() { rect = GetComponent<RectTransform>(); radius = 0.34f * Mathf.Min(rect.rect.width, rect.rect.height); }
        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);
        public void OnDrag(PointerEventData eventData)
        {
            if (rect == null) rect = GetComponent<RectTransform>();
            if (radius <= 1f) radius = 0.34f * Mathf.Min(rect.rect.width, rect.rect.height);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out Vector2 local))
            {
                Vector2 normalized = new Vector2(local.x / (rect.rect.width * 0.5f), local.y / (rect.rect.height * 0.5f));
                Value = Vector2.ClampMagnitude(normalized, 1f);
                if (Knob != null) Knob.anchoredPosition = Value * radius;
            }
        }
        public void OnPointerUp(PointerEventData eventData) { Value = Vector2.zero; if (Knob != null) Knob.anchoredPosition = Vector2.zero; }
    }

    public sealed class LookPad : MonoBehaviour, IDragHandler
    {
        private PlayerController player;
        private void Awake() { player = FindObjectOfType<PlayerController>(); }
        public void OnDrag(PointerEventData eventData)
        {
            if (player == null) player = FindObjectOfType<PlayerController>();
            if (player != null) player.ApplyLook(eventData.delta);
        }
    }
}
