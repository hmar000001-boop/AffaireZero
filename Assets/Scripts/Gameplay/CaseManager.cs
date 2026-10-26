using System;
using System.Collections.Generic;
using UnityEngine;
using AffaireZero.Core;

namespace AffaireZero.Gameplay
{
    [Serializable]
    public sealed class EvidenceDefinition
    {
        public string Id;
        public string TitleEn, TitleAr, DescriptionEn, DescriptionAr;
        public string RoomEn, RoomAr;
        public bool IsCritical;
        public string Title => Localization.Pick(TitleEn, TitleAr);
        public string Description => Localization.Pick(DescriptionEn, DescriptionAr);
        public string Room => Localization.Pick(RoomEn, RoomAr);
    }

    public static class CaseCatalog
    {
        public static readonly List<EvidenceDefinition> Evidence = new List<EvidenceDefinition>
        {
            new EvidenceDefinition { Id="locklog", TitleEn="The sealed lock", TitleAr="القفل المغلق", RoomEn="Study", RoomAr="غرفة الدراسة", IsCritical=true,
                DescriptionEn="The smart lock's local log records no bolt movement from 01:54 to 02:39. The study door was not opened at the time of the disappearance. Whoever left the room did not use this door.",
                DescriptionAr="لا يسجل سجل القفل الذكي أي حركة للمزلاج بين ٠١:٥٤ و٠٢:٣٩. لم يُفتح باب غرفة الدراسة وقت الاختفاء. من غادر الغرفة لم يستخدم هذا الباب." },
            new EvidenceDefinition { Id="camera_loop", TitleEn="The impossible camera", TitleAr="الكاميرا المستحيلة", RoomEn="Hallway", RoomAr="الممر", IsCritical=true,
                DescriptionEn="The hallway feed repeats the same eleven-second sequence between 02:11 and 02:22. A tiny reflection on the clock is identical in every loop. The image was replayed, not live.",
                DescriptionAr="تعيد كاميرا الممر المشهد نفسه لمدة إحدى عشرة ثانية بين ٠٢:١١ و٠٢:٢٢. انعكاس صغير على الساعة متطابق في كل مرة. كان المشهد معادًا وليس مباشرًا." },
            new EvidenceDefinition { Id="speaker", TitleEn="Footsteps on a timer", TitleAr="خطوات مبرمجة", RoomEn="Hallway", RoomAr="الممر", IsCritical=true,
                DescriptionEn="The smart speaker has a local sound routine at 02:16: a recording of footsteps, panned from the study toward the blank east wall. The schedule was created from the home's owner account.",
                DescriptionAr="يحتوي مكبر الصوت الذكي على تشغيل مجدول عند ٠٢:١٦: تسجيل خطوات تنتقل أصواته من غرفة الدراسة نحو الجدار الشرقي المصمت. أُنشئ الجدول من حساب مالك المنزل." },
            new EvidenceDefinition { Id="breaker", TitleEn="The service panel", TitleAr="لوحة الخدمات", RoomEn="Utility room", RoomAr="غرفة الخدمات", IsCritical=false,
                DescriptionEn="Fresh tool marks surround an old bypass switch. A maintenance log shows a brief circuit reset at 02:14. The panel can isolate the corridor camera without cutting power to the rest of the house.",
                DescriptionAr="تحيط آثار أدوات حديثة بمفتاح تجاوز قديم. يسجل سجل الصيانة إعادة ضبط قصيرة للدائرة عند ٠٢:١٤. تستطيع اللوحة فصل كاميرا الممر دون قطع الكهرباء عن بقية المنزل." },
            new EvidenceDefinition { Id="footprints", TitleEn="White dust trail", TitleAr="أثر الغبار الأبيض", RoomEn="Hallway", RoomAr="الممر", IsCritical=true,
                DescriptionEn="Fine plaster dust runs from the bookcase in the study to the east wing. It is dry and chalky, not outdoor soil. Someone crossed an old, dusty route recently, then brushed the prints near the doorway.",
                DescriptionAr="يمتد غبار جص ناعم من خزانة الكتب في غرفة الدراسة نحو الجناح الشرقي. إنه جاف وطباشيري وليس ترابًا خارجيًا. عبر شخص طريقًا قديمًا مغبرًا مؤخرًا ثم مسح الآثار قرب المدخل." },
            new EvidenceDefinition { Id="notebook", TitleEn="Elias's marked plans", TitleAr="مخططات إلياس المحددة", RoomEn="Study", RoomAr="غرفة الدراسة", IsCritical=true,
                DescriptionEn="A pencil plan shows a forgotten service passage linking the study, archive, and the outer maintenance stair. Beside it Elias wrote: 'If the exits are watched, use the route nobody remembers.' The blue mark is fresh.",
                DescriptionAr="يُظهر مخطط بالقلم ممرًا خدميًا منسيًا يصل غرفة الدراسة بالأرشيف ودرج الصيانة الخارجي. كتب إلياس بجانبه: «إذا كانت المخارج مراقبة، فاستخدم الطريق الذي لا يتذكره أحد». والعلامة الزرقاء حديثة." },
            new EvidenceDefinition { Id="passage", TitleEn="The concealed hatch", TitleAr="الفتحة المخفية", RoomEn="Archive", RoomAr="الأرشيف", IsCritical=true,
                DescriptionEn="The archive floor hatch sits beneath a fitted rug. Its latch has fresh scratches, and the dust is disturbed in one direction toward the outer maintenance stair. The old route is passable.",
                DescriptionAr="تقع فتحة أرضية الأرشيف تحت سجادة مثبتة. على مزلاجها خدوش حديثة، والغبار مضطرب باتجاه واحد نحو درج الصيانة الخارجي. الطريق القديم صالح للعبور." },
            new EvidenceDefinition { Id="voice_note", TitleEn="A recovered voice note", TitleAr="ملاحظة صوتية مستعادة", RoomEn="Study", RoomAr="غرفة الدراسة", IsCritical=true,
                DescriptionEn="A draft voice note on Elias's phone says: 'At seventeen past two, play the steps. Keep the door log clean. The audit files must leave this house tonight.' It was saved, not sent.",
                DescriptionAr="تقول مسودة تسجيل صوتي على هاتف إلياس: «عند الثانية وسبع عشرة دقيقة، شغّل الخطوات. أبقِ سجل الباب نظيفًا. يجب أن تغادر ملفات التدقيق هذا المنزل الليلة». حُفظ التسجيل ولم يُرسل." }
        };

        public static EvidenceDefinition Get(string id) => Evidence.Find(x => x.Id == id);
        public static int GetIndex(string id) => Evidence.FindIndex(x => x.Id == id);
    }

    [Serializable]
    public sealed class CaseSaveData
    {
        public string[] EvidenceIds = Array.Empty<string>();
        public bool Solved;
        public bool HasStarted;
        public string Language = "English";
    }

    public sealed class CaseManager
    {
        private const string SaveKey = "AFFAIRE_ZERO_SAVE_V1";
        private readonly HashSet<string> discovered = new HashSet<string>();
        public bool Solved { get; private set; }
        public bool HasStarted { get; private set; }
        public int EvidenceCount => discovered.Count;
        public IReadOnlyCollection<string> DiscoveredIds => discovered;

        public bool HasEvidence(string id) => discovered.Contains(id);
        public bool Discover(string id)
        {
            if (CaseCatalog.Get(id) == null || !discovered.Add(id)) return false;
            Save();
            return true;
        }

        public void StartNew()
        {
            discovered.Clear();
            Solved = false;
            HasStarted = true;
            Save();
        }

        public void MarkSolved()
        {
            Solved = true;
            Save();
        }

        public void Save()
        {
            try
            {
                var data = new CaseSaveData
                {
                    EvidenceIds = new List<string>(discovered).ToArray(),
                    Solved = Solved,
                    HasStarted = HasStarted,
                    Language = Localization.Language.ToString()
                };
                PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
                PlayerPrefs.Save();
            }
            catch (Exception ex) { Debug.LogWarning("Affaire Zero save failed: " + ex.Message); }
        }

        public bool Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return false;
            try
            {
                var data = JsonUtility.FromJson<CaseSaveData>(PlayerPrefs.GetString(SaveKey));
                if (data == null) return false;
                discovered.Clear();
                if (data.EvidenceIds != null)
                    foreach (string id in data.EvidenceIds) if (CaseCatalog.Get(id) != null) discovered.Add(id);
                Solved = data.Solved;
                HasStarted = data.HasStarted;
                if (Enum.TryParse(data.Language, out GameLanguage language)) Localization.SetLanguage(language);
                return HasStarted;
            }
            catch (Exception ex) { Debug.LogWarning("Affaire Zero save could not be read: " + ex.Message); return false; }
        }

        public bool CanJustifyElias(HashSet<string> selected)
        {
            // The solution must connect the route, the staged recording/camera, and independent physical corroboration.
            bool route = selected.Contains("passage") || selected.Contains("notebook");
            bool staging = selected.Contains("camera_loop") || selected.Contains("speaker") || selected.Contains("voice_note");
            bool corroboration = selected.Contains("footprints") || selected.Contains("locklog") || selected.Contains("breaker");
            return selected.Count >= 3 && route && staging && corroboration;
        }
    }
}
