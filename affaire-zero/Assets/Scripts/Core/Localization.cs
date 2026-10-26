using System;
using System.Collections.Generic;
using System.Text;

namespace AffaireZero.Core
{
    public enum GameLanguage { English, Arabic }

    public static class Localization
    {
        public static GameLanguage Language { get; private set; } = GameLanguage.English;
        public static event Action LanguageChanged;

        private static readonly Dictionary<string, string[]> Values = new Dictionary<string, string[]>
        {
            ["tagline"] = new[] { "A case that should be impossible.", "قضية يستحيل تفسيرها." },
            ["start"] = new[] { "START INVESTIGATION", "ابدأ التحقيق" },
            ["continue"] = new[] { "CONTINUE CASE", "تابع القضية" },
            ["language"] = new[] { "LANGUAGE  ·  EN / AR", "اللغة  ·  عربي / English" },
            ["chapter"] = new[] { "CASE FILE 01  ·  THE IMPOSSIBLE ABSENCE", "ملف القضية ٠١  ·  الغياب المستحيل" },
            ["briefing_title"] = new[] { "THE IMPOSSIBLE ABSENCE", "الغياب المستحيل" },
            ["briefing"] = new[] { "At 02:17, architect Elias Varen vanished from a locked study. No door opened. The corridor camera showed an empty hall, yet three witnesses heard footsteps walking toward a wall. Some call it a haunting. Your task is to prove what happened using evidence, not guesswork.", "في الساعة ٠٢:١٧ اختفى المهندس إلياس فارن من غرفة دراسة مغلقة. لم يُفتح أي باب، وأظهرت كاميرا الممر ممرًا خاليًا، ومع ذلك سمع ثلاثة شهود خطوات تتجه نحو جدار. يظن البعض أن الأمر خارق للطبيعة. مهمتك إثبات ما حدث بالأدلة لا بالتخمين." },
            ["case_progress"] = new[] { "EVIDENCE", "الأدلة" },
            ["file"] = new[] { "CASE FILE", "ملف القضية" },
            ["theory"] = new[] { "FORM A THEORY", "كوّن فرضية" },
            ["inspect"] = new[] { "INSPECT", "افحص" },
            ["move_hint"] = new[] { "MOVE", "تحرّك" },
            ["look_hint"] = new[] { "DRAG TO LOOK", "اسحب للنظر" },
            ["close"] = new[] { "CLOSE", "إغلاق" },
            ["evidence_tab"] = new[] { "EVIDENCE", "الأدلة" },
            ["suspects_tab"] = new[] { "INTERVIEWS", "الإفادات" },
            ["notes_tab"] = new[] { "CASE NOTES", "ملاحظات القضية" },
            ["evidence_none"] = new[] { "No evidence collected yet. Explore the rooms and inspect the amber markers.", "لم تجمع أي دليل بعد. استكشف الغرف وافحص العلامات الكهرمانية." },
            ["interviews_intro"] = new[] { "Three accounts. Each leaves something unexplained. Compare their statements with the physical evidence.", "ثلاث إفادات، وفي كل واحدة شيء غير مفسر. قارن أقوالهم بالأدلة المادية." },
            ["interview_nora"] = new[] { "NORA RAHAL · HOUSE CURATOR", "نورا رحال · مسؤولة المنزل" },
            ["interview_malik"] = new[] { "MALIK HADDAD · SECURITY CONTRACTOR", "مالك حداد · مسؤول الأمن" },
            ["interview_elias"] = new[] { "ELIAS VAREN · MISSING ARCHITECT", "إلياس فارن · المهندس المفقود" },
            ["nora_statement"] = new[] { "“I was in the kitchen after two. I heard the footsteps too. Elias was frightened by the old service wing, but he never went near it.”", "«كنت في المطبخ بعد الثانية. سمعت الخطوات أيضًا. كان إلياس يخاف الجناح الخدمي القديم، لكنه لم يقترب منه قط.»" },
            ["malik_statement"] = new[] { "“The hallway camera was live all night. I checked it at 02:10. The control panel is old, but nobody can make a recording look live.”", "«كانت كاميرا الممر تعمل طوال الليل. تحققت منها عند ٠٢:١٠. لوحة التحكم قديمة، لكن لا أحد يستطيع جعل تسجيل يبدو مباشرًا.»" },
            ["elias_statement"] = new[] { "“A building is only as secure as the plans its owner forgets. There is always a route nobody remembers.” — from Elias's last recorded design review.", "«لا يكون المبنى آمنًا إلا بقدر ما ينسى مالكه من مخططاته. يوجد دائمًا طريق لا يتذكره أحد.» — من آخر مراجعة تصميم مسجلة لإلياس." },
            ["interview_hint"] = new[] { "Interview records are leads, not proof. Find physical evidence before deciding.", "الإفادات خيوط وليست أدلة قاطعة. ابحث عن أدلة مادية قبل اتخاذ القرار." },
            ["select_suspect"] = new[] { "WHO STAGED THE DISAPPEARANCE?", "من دبّر الاختفاء؟" },
            ["select_evidence"] = new[] { "Choose at least three pieces of evidence to support your conclusion.", "اختر ثلاثة أدلة على الأقل لدعم استنتاجك." },
            ["confirm_theory"] = new[] { "SUBMIT CONCLUSION", "قدّم الاستنتاج" },
            ["not_enough"] = new[] { "Your conclusion needs at least three pieces of evidence. Return to the scene and investigate further.", "يحتاج استنتاجك إلى ثلاثة أدلة على الأقل. عُد إلى المكان وتابع التحقيق." },
            ["not_supported"] = new[] { "That theory is not sufficiently supported by the evidence selected. Re-examine the clues and try again.", "لا تدعم الأدلة المحددة هذه الفرضية بما يكفي. أعد فحص الأدلة وحاول مجددًا." },
            ["case_solved"] = new[] { "CASE RESOLVED", "حُلّت القضية" },
            ["solution_title"] = new[] { "NO GHOST. NO IMPOSSIBLE EXIT.", "لا شبح، ولا خروج مستحيل." },
            ["solution_body"] = new[] { "Elias staged his own disappearance. He looped the corridor camera, scheduled a recording of footsteps on the smart speaker, and used the concealed service passage shown in his notebook. The door log never recorded an opening because he never used the door. The dust trail and fresh hatch marks prove the route was used that night.\n\nHis motive was to escape the building's monitored exits with the original audit files documenting a hidden surveillance system. He made the departure look impossible so the people tracking the front door would search for a phantom instead of the old service route.\n\nNora's fear and Malik's confident account were distractions, not proof. The explanation fits every physical clue without requiring a supernatural event.", "دبّر إلياس اختفاءه بنفسه. أعاد تشغيل مقطع كاميرا الممر، وجدول تسجيل خطوات على مكبر الصوت الذكي، واستخدم الممر الخدمي المخفي المرسوم في دفتره. لم يسجل قفل الباب أي فتح لأنه لم يستخدم الباب أصلًا. يثبت أثر الغبار والخدوش الحديثة على الفتحة أن الطريق استُخدم تلك الليلة.\n\nكان هدفه الهروب من المخارج الخاضعة للمراقبة ومعه ملفات التدقيق الأصلية التي توثق نظام مراقبة خفيًا. جعل خروجه يبدو مستحيلًا كي يبحث من يراقب الباب الرئيسي عن شبح بدلًا من الطريق الخدمي القديم.\n\nكان خوف نورا وثقة مالك في كلامه عاملَي تشتيت لا دليلين. يفسر هذا الحل كل القرائن المادية دون الحاجة إلى حدث خارق للطبيعة." },
            ["continue_explore"] = new[] { "RETURN TO SCENE", "العودة إلى المشهد" },
            ["restart"] = new[] { "PLAY AGAIN", "العب مجددًا" },
            ["solved_badge"] = new[] { "YOUR CASE FILE IS COMPLETE", "اكتمل ملف قضيتك" },
            ["pause_title"] = new[] { "CASE PAUSED", "القضية متوقفة مؤقتًا" },
            ["resume"] = new[] { "RETURN TO INVESTIGATION", "العودة إلى التحقيق" },
            ["new_case"] = new[] { "EPISODE 01  ·  FREE", "الحلقة ٠١  ·  مجانية" },
            ["future_case"] = new[] { "MORE CASES ARE IN DEVELOPMENT", "المزيد من القضايا قيد التطوير" },
            ["case_note_1"] = new[] { "RULE: a theory must explain the lock, the camera, and the route out.", "قاعدة: يجب أن تفسر الفرضية القفل والكاميرا وطريق الخروج." },
            ["case_note_2"] = new[] { "A witness account can be wrong without the witness being the culprit.", "قد تكون إفادة الشاهد خاطئة دون أن يكون الشاهد هو الجاني." },
            ["case_note_3"] = new[] { "Look for evidence that independently confirms another clue.", "ابحث عن دليل يؤكد قرينة أخرى بصورة مستقلة." },
            ["credits"] = new[] { "A procedural 3D mystery · Episode 01", "لغز ثلاثي الأبعاد · الحلقة الأولى" },
            ["quit"] = new[] { "BACK TO TITLE", "العودة إلى العنوان" },
            ["found"] = new[] { "EVIDENCE LOGGED", "تم تسجيل الدليل" },
            ["voice_warning"] = new[] { "Audio transcript recovered from Elias's phone.", "تم استرجاع نص تسجيل صوتي من هاتف إلياس." },
            ["hatch_puzzle_title"] = new[] { "THE OLD SERVICE HATCH", "فتحة الخدمة القديمة" },
            ["hatch_puzzle_riddle"] = new[] { "The six-digit latch is tied to three events. Put them in chronological order: camera loop, circuit reset, speaker footsteps. Enter the minute part of each timestamp, oldest to newest.", "يرتبط القفل ذو الأرقام الستة بثلاثة أحداث. رتّبها زمنيًا: تكرار الكاميرا، إعادة ضبط الدائرة، وتشغيل الخطوات عبر المكبر. أدخل جزء الدقائق من كل توقيت من الأقدم إلى الأحدث." },
            ["hatch_locked"] = new[] { "The console needs three event records first: camera loop at 02:11, circuit reset at 02:14, and speaker routine at 02:16. Find and inspect all three clues before entering the code.", "تحتاج اللوحة أولًا إلى ثلاثة سجلات: تكرار الكاميرا عند ٠٢:١١، وإعادة ضبط الدائرة عند ٠٢:١٤، وتشغيل المكبر عند ٠٢:١٦. اعثر على الأدلة الثلاثة وافحصها قبل إدخال الرمز." },
            ["code_placeholder"] = new[] { "6-DIGIT CODE", "الرمز المكوّن من ٦ أرقام" },
            ["unlock_hatch"] = new[] { "UNLOCK HATCH", "افتح الفتحة" },
            ["hint_button"] = new[] { "SHOW HINT", "أظهر التلميح" },
            ["investigation_hint_next"] = new[] { "REVEAL NEXT INVESTIGATION HINT", "أظهر التلميح التالي للتحقيق" },
            ["investigation_hint_complete"] = new[] { "ALL INVESTIGATION HINTS REVEALED", "تم إظهار جميع تلميحات التحقيق" },
            ["investigation_hint_1"] = new[] { "Hint 1 · Timing: the corridor camera repeats its image while the speaker plays footsteps. Compare those times instead of assuming a haunting.", "التلميح ١ · التوقيت: تعيد كاميرا الممر الصورة بينما يشغّل مكبر الصوت تسجيل الخطوات. قارن الأوقات بدل افتراض وجود ظاهرة خارقة." },
            ["investigation_hint_2"] = new[] { "Hint 2 · The study door stayed locked. Compare Elias's marked plans with the white plaster dust to trace a hidden service route.", "التلميح ٢ · بقي باب غرفة الدراسة مغلقًا. قارن مخططات إلياس المحددة بغبار الجص الأبيض لتتبع طريق خدمي مخفي." },
            ["investigation_hint_3"] = new[] { "Hint 3 · To justify the conclusion, select Elias, a route clue (plans or hatch), a staging clue (camera, speaker or voice note), and independent corroboration (dust, lock log or service panel).", "التلميح ٣ · لدعم الاستنتاج، اختر إلياس ودليلًا على الطريق (المخططات أو الفتحة)، ودليلًا على التدبير (الكاميرا أو المكبر أو التسجيل)، ودليلًا مستقلًا (الغبار أو سجل القفل أو لوحة الخدمات)." },
            ["hatch_hint_text"] = new[] { "The minute marks are 11, 14 and 16. Keep that order: camera, circuit, speaker.", "أرقام الدقائق هي ١١ و١٤ و١٦. حافظ على هذا الترتيب: الكاميرا، الدائرة، مكبر الصوت." },
            ["wrong_code"] = new[] { "The sequence does not release the latch. Re-check the event times and their order.", "لم يفتح التسلسل المزلاج. راجع أوقات الأحداث وترتيبها." },
            ["error"] = new[] { "Something went wrong. Your investigation has been saved locally.", "حدث خطأ ما. تم حفظ التحقيق على الجهاز." },
        };

        public static void SetLanguage(GameLanguage language)
        {
            if (Language == language) return;
            Language = language;
            LanguageChanged?.Invoke();
        }

        public static string T(string key)
        {
            if (!Values.TryGetValue(key, out string[] value)) return key;
            string text = value[(int)Language];
            return Language == GameLanguage.Arabic ? ArabicTextProcessor.ShapeAndReverse(text) : text;
        }

        public static string Pick(string english, string arabic) =>
            Language == GameLanguage.Arabic ? ArabicTextProcessor.ShapeAndReverse(arabic) : english;
    }

    /// <summary>
    /// Small dependency-free Arabic renderer for Unity's built-in UGUI Text. It shapes common Arabic letters
    /// into presentation forms and reverses the run for left-to-right text renderers. For mixed Arabic/Latin
    /// typography, a production release should use a dedicated RTL-capable text package.
    /// </summary>
    internal static class ArabicTextProcessor
    {
        private struct Forms { public char Isolated, Final, Initial, Medial; public bool JoinsPrevious, JoinsNext; }
        private static readonly Dictionary<char, Forms> Map = new Dictionary<char, Forms>
        {
            ['ء']=new Forms{Isolated='\uFE80'}, ['آ']=new Forms{Isolated='\uFE81',Final='\uFE82',JoinsPrevious=true},
            ['أ']=new Forms{Isolated='\uFE83',Final='\uFE84',JoinsPrevious=true}, ['ؤ']=new Forms{Isolated='\uFE85',Final='\uFE86',JoinsPrevious=true},
            ['إ']=new Forms{Isolated='\uFE87',Final='\uFE88',JoinsPrevious=true}, ['ئ']=new Forms{Isolated='\uFE89',Final='\uFE8A',Initial='\uFE8B',Medial='\uFE8C',JoinsPrevious=true,JoinsNext=true},
            ['ا']=new Forms{Isolated='\uFE8D',Final='\uFE8E',JoinsPrevious=true}, ['ب']=new Forms{Isolated='\uFE8F',Final='\uFE90',Initial='\uFE91',Medial='\uFE92',JoinsPrevious=true,JoinsNext=true},
            ['ة']=new Forms{Isolated='\uFE93',Final='\uFE94',JoinsPrevious=true}, ['ت']=new Forms{Isolated='\uFE95',Final='\uFE96',Initial='\uFE97',Medial='\uFE98',JoinsPrevious=true,JoinsNext=true},
            ['ث']=new Forms{Isolated='\uFE99',Final='\uFE9A',Initial='\uFE9B',Medial='\uFE9C',JoinsPrevious=true,JoinsNext=true}, ['ج']=new Forms{Isolated='\uFE9D',Final='\uFE9E',Initial='\uFE9F',Medial='\uFEA0',JoinsPrevious=true,JoinsNext=true},
            ['ح']=new Forms{Isolated='\uFEA1',Final='\uFEA2',Initial='\uFEA3',Medial='\uFEA4',JoinsPrevious=true,JoinsNext=true}, ['خ']=new Forms{Isolated='\uFEA5',Final='\uFEA6',Initial='\uFEA7',Medial='\uFEA8',JoinsPrevious=true,JoinsNext=true},
            ['د']=new Forms{Isolated='\uFEA9',Final='\uFEAA',JoinsPrevious=true}, ['ذ']=new Forms{Isolated='\uFEAB',Final='\uFEAC',JoinsPrevious=true}, ['ر']=new Forms{Isolated='\uFEAD',Final='\uFEAE',JoinsPrevious=true}, ['ز']=new Forms{Isolated='\uFEAF',Final='\uFEB0',JoinsPrevious=true},
            ['س']=new Forms{Isolated='\uFEB1',Final='\uFEB2',Initial='\uFEB3',Medial='\uFEB4',JoinsPrevious=true,JoinsNext=true}, ['ش']=new Forms{Isolated='\uFEB5',Final='\uFEB6',Initial='\uFEB7',Medial='\uFEB8',JoinsPrevious=true,JoinsNext=true},
            ['ص']=new Forms{Isolated='\uFEB9',Final='\uFEBA',Initial='\uFEBB',Medial='\uFEBC',JoinsPrevious=true,JoinsNext=true}, ['ض']=new Forms{Isolated='\uFEBD',Final='\uFEBE',Initial='\uFEBF',Medial='\uFEC0',JoinsPrevious=true,JoinsNext=true},
            ['ط']=new Forms{Isolated='\uFEC1',Final='\uFEC2',Initial='\uFEC3',Medial='\uFEC4',JoinsPrevious=true,JoinsNext=true}, ['ظ']=new Forms{Isolated='\uFEC5',Final='\uFEC6',Initial='\uFEC7',Medial='\uFEC8',JoinsPrevious=true,JoinsNext=true},
            ['ع']=new Forms{Isolated='\uFEC9',Final='\uFECA',Initial='\uFECB',Medial='\uFECC',JoinsPrevious=true,JoinsNext=true}, ['غ']=new Forms{Isolated='\uFECD',Final='\uFECE',Initial='\uFECF',Medial='\uFED0',JoinsPrevious=true,JoinsNext=true},
            ['ف']=new Forms{Isolated='\uFED1',Final='\uFED2',Initial='\uFED3',Medial='\uFED4',JoinsPrevious=true,JoinsNext=true}, ['ق']=new Forms{Isolated='\uFED5',Final='\uFED6',Initial='\uFED7',Medial='\uFED8',JoinsPrevious=true,JoinsNext=true},
            ['ك']=new Forms{Isolated='\uFED9',Final='\uFEDA',Initial='\uFEDB',Medial='\uFEDC',JoinsPrevious=true,JoinsNext=true}, ['ل']=new Forms{Isolated='\uFEDD',Final='\uFEDE',Initial='\uFEDF',Medial='\uFEE0',JoinsPrevious=true,JoinsNext=true},
            ['م']=new Forms{Isolated='\uFEE1',Final='\uFEE2',Initial='\uFEE3',Medial='\uFEE4',JoinsPrevious=true,JoinsNext=true}, ['ن']=new Forms{Isolated='\uFEE5',Final='\uFEE6',Initial='\uFEE7',Medial='\uFEE8',JoinsPrevious=true,JoinsNext=true},
            ['ه']=new Forms{Isolated='\uFEE9',Final='\uFEEA',Initial='\uFEEB',Medial='\uFEEC',JoinsPrevious=true,JoinsNext=true}, ['و']=new Forms{Isolated='\uFEED',Final='\uFEEE',JoinsPrevious=true}, ['ى']=new Forms{Isolated='\uFEEF',Final='\uFEF0',JoinsPrevious=true},
            ['ي']=new Forms{Isolated='\uFEF1',Final='\uFEF2',Initial='\uFEF3',Medial='\uFEF4',JoinsPrevious=true,JoinsNext=true}, ['پ']=new Forms{Isolated='\uFB56',Final='\uFB57',Initial='\uFB58',Medial='\uFB59',JoinsPrevious=true,JoinsNext=true},
            ['چ']=new Forms{Isolated='\uFB7A',Final='\uFB7B',Initial='\uFB7C',Medial='\uFB7D',JoinsPrevious=true,JoinsNext=true}, ['گ']=new Forms{Isolated='\uFB92',Final='\uFB93',Initial='\uFB94',Medial='\uFB95',JoinsPrevious=true,JoinsNext=true}
        };

        public static string ShapeAndReverse(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            // Keep line order intact; shape and visually reverse each Arabic line independently.
            string[] lines = input.Replace("\r", string.Empty).Split('\n');
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var chars = lines[lineIndex].ToCharArray();
                for (int i = 0; i < chars.Length; i++)
                {
                    if (!Map.TryGetValue(chars[i], out Forms current)) continue;
                    bool joinsPrev = i > 0 && Map.TryGetValue(chars[i - 1], out Forms prev) && prev.JoinsNext && current.JoinsPrevious;
                    bool joinsNext = i + 1 < chars.Length && Map.TryGetValue(chars[i + 1], out Forms next) && current.JoinsNext && next.JoinsPrevious;
                    char shaped = current.Isolated;
                    if (joinsPrev && joinsNext && current.Medial != '\0') shaped = current.Medial;
                    else if (joinsPrev && current.Final != '\0') shaped = current.Final;
                    else if (joinsNext && current.Initial != '\0') shaped = current.Initial;
                    chars[i] = shaped;
                }
                Array.Reverse(chars);
                // Restore left-to-right tokens after reversing the Arabic line. This keeps English labels,
                // digit strings, Arabic-Indic timestamps such as ٠٢:١٧, and simple codes readable.
                int runStart = -1;
                for (int i = 0; i <= chars.Length; i++)
                {
                    bool inLtrToken = i < chars.Length && IsLtrTokenChar(chars[i]);
                    if (inLtrToken && runStart < 0) runStart = i;
                    if (!inLtrToken && runStart >= 0)
                    {
                        Array.Reverse(chars, runStart, i - runStart);
                        runStart = -1;
                    }
                }
                lines[lineIndex] = new string(chars);
            }
            return string.Join("\n", lines);
        }

        private static bool IsDigit(char c) => (c >= '0' && c <= '9') || (c >= '\u0660' && c <= '\u0669') || (c >= '\u06F0' && c <= '\u06F9');

        private static bool IsLtrTokenChar(char c) =>
            (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || IsDigit(c) ||
            c == ':' || c == '.' || c == '/' || c == '-' || c == '_' || c == '\'' || c == '@';
    }
}
