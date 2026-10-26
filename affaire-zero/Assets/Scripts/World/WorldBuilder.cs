using UnityEngine;
using AffaireZero.Core;
using AffaireZero.Gameplay;

namespace AffaireZero.World
{
    public sealed class WorldBuilder
    {
        private Material wall, floor, trim, wood, darkWood, metal, glass, teal, amber, cloth, white, concrete, black, red;
        private readonly GameObject root = new GameObject("AFFAIRE_ZERO_WORLD");
        public Transform PlayerStart { get; private set; }
        public Transform CameraPivot { get; private set; }

        public void Build(GameBootstrap owner)
        {
            CreateMaterials();
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.19f, 0.21f, 0.25f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.035f, 0.045f, 0.065f);
            RenderSettings.fogDensity = 0.012f;
            BuildRooms();
            BuildFurniture();
            BuildEvidence(owner);
            BuildLighting();
            BuildPlayer();
            BuildSigns();
        }

        private void CreateMaterials()
        {
            wall = Mat("Deep slate plaster", new Color(0.105f, 0.135f, 0.17f), 0.05f);
            floor = Mat("Smoked stone", new Color(0.095f, 0.105f, 0.12f), 0.16f);
            trim = Mat("Brushed brass", new Color(0.45f, 0.29f, 0.10f), 0.72f);
            wood = Mat("Walnut", new Color(0.19f, 0.105f, 0.06f), 0.12f);
            darkWood = Mat("Black walnut", new Color(0.07f, 0.055f, 0.047f), 0.16f);
            metal = Mat("Graphite metal", new Color(0.16f, 0.19f, 0.22f), 0.72f);
            glass = Mat("Night glass", new Color(0.045f, 0.12f, 0.18f), 0.9f);
            teal = Mat("Signal teal", new Color(0.03f, 0.44f, 0.5f), 0.25f, true);
            amber = Mat("Evidence amber", new Color(1f, 0.52f, 0.09f), 0.1f, true);
            cloth = Mat("Midnight upholstery", new Color(0.08f, 0.11f, 0.14f), 0.02f);
            white = Mat("Warm paper", new Color(0.68f, 0.62f, 0.47f), 0.0f);
            concrete = Mat("Aged concrete", new Color(0.25f, 0.26f, 0.24f), 0.03f);
            black = Mat("Near black", new Color(0.012f, 0.016f, 0.02f), 0.22f);
            red = Mat("Signal red", new Color(0.55f, 0.06f, 0.055f), 0.18f, true);
        }

        private Material Mat(string name, Color color, float metallic, bool emission = false)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            var m = new Material(shader) { name = name, color = color };
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", metallic * 0.65f);
            if (emission && m.HasProperty("_EmissionColor")) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", color * 0.42f); }
            return m;
        }

        private GameObject Box(string name, Vector3 pos, Vector3 scale, Material material, bool collider = true, Transform parent = null)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.position = pos;
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) { var c = obj.GetComponent<Collider>(); if (c != null) Object.Destroy(c); }
            if (parent != null) obj.transform.SetParent(parent, true);
            return obj;
        }

        private GameObject Sphere(string name, Vector3 pos, Vector3 scale, Material material, Transform parent = null)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = name; obj.transform.position = pos; obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            var c = obj.GetComponent<Collider>(); if (c != null) Object.Destroy(c);
            if (parent != null) obj.transform.SetParent(parent, true);
            return obj;
        }

        private GameObject Cylinder(string name, Vector3 pos, Vector3 scale, Material material, Transform parent = null)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.name = name; obj.transform.position = pos; obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            if (parent != null) obj.transform.SetParent(parent, true);
            return obj;
        }

        private void BuildRooms()
        {
            // One connected, walkable floor with distinct study, corridor, utility and archive areas.
            Box("Study floor", new Vector3(0, -0.12f, 0), new Vector3(8, 0.24f, 8), floor);
            Box("Hall floor", new Vector3(0, -0.12f, 9), new Vector3(4.7f, 0.24f, 10.2f), floor);
            Box("Utility floor", new Vector3(-4.65f, -0.12f, 9.5f), new Vector3(4.7f, 0.24f, 5.2f), concrete);
            Box("Archive floor", new Vector3(4.65f, -0.12f, 9.5f), new Vector3(4.7f, 0.24f, 5.2f), floor);

            // Study enclosure and the opening to the hallway.
            Box("Study north wall", new Vector3(0, 1.65f, -4), new Vector3(8, 3.3f, 0.22f), wall);
            Box("Study west wall", new Vector3(-4, 1.65f, 0), new Vector3(0.22f, 3.3f, 8), wall);
            Box("Study east wall", new Vector3(4, 1.65f, 0), new Vector3(0.22f, 3.3f, 8), wall);
            Box("Study front wall left", new Vector3(-2.5f, 1.65f, 4), new Vector3(3, 3.3f, 0.22f), wall);
            Box("Study front wall right", new Vector3(2.5f, 1.65f, 4), new Vector3(3, 3.3f, 0.22f), wall);
            Box("Study doorway lintel", new Vector3(0, 3.05f, 4), new Vector3(2, 0.5f, 0.3f), darkWood);
            Box("Study doorway trim L", new Vector3(-1.03f, 1.4f, 4), new Vector3(0.08f, 2.8f, 0.28f), trim);
            Box("Study doorway trim R", new Vector3(1.03f, 1.4f, 4), new Vector3(0.08f, 2.8f, 0.28f), trim);

            // Corridor. Doors into side rooms are represented by gaps in the partition walls.
            Box("Hall west wall A", new Vector3(-2.36f, 1.65f, 6.0f), new Vector3(0.2f, 3.3f, 3.8f), wall);
            Box("Hall west wall B", new Vector3(-2.36f, 1.65f, 12.4f), new Vector3(0.2f, 3.3f, 3.2f), wall);
            Box("Hall east wall A", new Vector3(2.36f, 1.65f, 6.0f), new Vector3(0.2f, 3.3f, 3.8f), wall);
            Box("Hall east wall B", new Vector3(2.36f, 1.65f, 12.4f), new Vector3(0.2f, 3.3f, 3.2f), wall);
            Box("Hall end wall", new Vector3(0, 1.65f, 14), new Vector3(4.8f, 3.3f, 0.22f), wall);

            // Side rooms and outer walls.
            Box("Utility west wall", new Vector3(-7, 1.65f, 9.5f), new Vector3(0.2f, 3.3f, 5.2f), wall);
            Box("Utility far wall", new Vector3(-4.65f, 1.65f, 12.1f), new Vector3(4.7f, 3.3f, 0.2f), wall);
            Box("Utility near wall", new Vector3(-4.65f, 1.65f, 6.9f), new Vector3(4.7f, 3.3f, 0.2f), wall);
            Box("Archive east wall", new Vector3(7, 1.65f, 9.5f), new Vector3(0.2f, 3.3f, 5.2f), wall);
            Box("Archive far wall", new Vector3(4.65f, 1.65f, 12.1f), new Vector3(4.7f, 3.3f, 0.2f), wall);
            Box("Archive near wall", new Vector3(4.65f, 1.65f, 6.9f), new Vector3(4.7f, 3.3f, 0.2f), wall);
            // Doorway frames on the side-room entries.
            Box("Utility frame top", new Vector3(-2.36f, 3.02f, 9.5f), new Vector3(0.35f, 0.42f, 2.0f), darkWood);
            Box("Archive frame top", new Vector3(2.36f, 3.02f, 9.5f), new Vector3(0.35f, 0.42f, 2.0f), darkWood);

            // Low ceiling helps the environment feel enclosed while keeping lighting simple.
            Box("Study ceiling", new Vector3(0, 3.35f, 0), new Vector3(8, 0.16f, 8), darkWood, false);
            Box("Hall ceiling", new Vector3(0, 3.35f, 9), new Vector3(4.7f, 0.16f, 10.2f), darkWood, false);
            Box("Utility ceiling", new Vector3(-4.65f, 3.35f, 9.5f), new Vector3(4.7f, 0.16f, 5.2f), darkWood, false);
            Box("Archive ceiling", new Vector3(4.65f, 3.35f, 9.5f), new Vector3(4.7f, 0.16f, 5.2f), darkWood, false);

            // A cold night window and thin metallic frame.
            Box("Study night window", new Vector3(-3.88f, 2.0f, -0.2f), new Vector3(0.04f, 1.35f, 2.1f), glass, false);
            Box("Window vertical trim", new Vector3(-3.83f, 2.0f, -0.2f), new Vector3(0.08f, 1.55f, 0.08f), trim, false);
            Box("Window horizontal trim", new Vector3(-3.83f, 2.0f, -0.2f), new Vector3(0.08f, 0.08f, 2.25f), trim, false);
            Box("Hall baseboard west", new Vector3(-2.25f, 0.12f, 9), new Vector3(0.08f, 0.24f, 9.7f), darkWood, false);
            Box("Hall baseboard east", new Vector3(2.25f, 0.12f, 9), new Vector3(0.08f, 0.24f, 9.7f), darkWood, false);
        }

        private void BuildFurniture()
        {
            // Study desk, papers and notebook table.
            Box("Walnut desk top", new Vector3(-1.55f, 0.78f, 0.45f), new Vector3(2.35f, 0.14f, 1.05f), wood);
            Box("Desk leg 1", new Vector3(-2.55f, 0.38f, 0.05f), new Vector3(0.12f, 0.75f, 0.12f), darkWood);
            Box("Desk leg 2", new Vector3(-0.55f, 0.38f, 0.05f), new Vector3(0.12f, 0.75f, 0.12f), darkWood);
            Box("Desk leg 3", new Vector3(-2.55f, 0.38f, 0.85f), new Vector3(0.12f, 0.75f, 0.12f), darkWood);
            Box("Desk leg 4", new Vector3(-0.55f, 0.38f, 0.85f), new Vector3(0.12f, 0.75f, 0.12f), darkWood);
            Box("Desk papers", new Vector3(-1.8f, 0.88f, 0.35f), new Vector3(0.72f, 0.02f, 0.42f), white, false);
            Box("Desk pen", new Vector3(-1.15f, 0.9f, 0.22f), new Vector3(0.08f, 0.025f, 0.42f), black, false).transform.rotation = Quaternion.Euler(0, 24, 8);

            // Bookcase near the concealed route; it is intentionally heavy and quiet-looking.
            Box("Study bookcase", new Vector3(2.95f, 1.35f, -2.8f), new Vector3(1.65f, 2.7f, 0.42f), darkWood);
            for (int y = 0; y < 4; y++)
            {
                Box("Bookcase shelf " + y, new Vector3(2.95f, 0.55f + y * 0.61f, -2.55f), new Vector3(1.56f, 0.055f, 0.46f), wood, false);
                for (int j = 0; j < 5; j++)
                {
                    Material bookMat = j % 2 == 0 ? red : (j % 3 == 0 ? teal : trim);
                    Box("Old volume", new Vector3(2.38f + j * 0.27f, 0.77f + y * 0.61f, -2.55f), new Vector3(0.2f, 0.4f + (j % 2) * 0.08f, 0.28f), bookMat, false);
                }
            }

            // Hall console with a small security display.
            Box("Security console pedestal", new Vector3(-1.5f, 0.65f, 7.35f), new Vector3(1.0f, 1.25f, 0.6f), darkWood);
            Box("Security display", new Vector3(-1.5f, 1.48f, 7.02f), new Vector3(0.82f, 0.52f, 0.07f), black, false);
            Box("Display signal", new Vector3(-1.5f, 1.48f, 6.975f), new Vector3(0.68f, 0.36f, 0.012f), teal, false);
            // Utility cabinets.
            Box("Utility rack", new Vector3(-5.7f, 1.0f, 8.0f), new Vector3(1.45f, 1.95f, 0.55f), metal);
            Box("Archive shelves", new Vector3(5.95f, 1.45f, 11.15f), new Vector3(1.65f, 2.8f, 0.5f), darkWood);
            for (int i = 0; i < 3; i++)
            {
                Box("Archive shelf " + i, new Vector3(5.95f, 0.55f + i * 0.72f, 10.85f), new Vector3(1.5f, 0.055f, 0.5f), wood, false);
                for (int j = 0; j < 4; j++) Box("Archive ledger", new Vector3(5.42f + j * 0.32f, 0.82f + i * 0.72f, 10.84f), new Vector3(0.22f, 0.42f, 0.32f), j % 2 == 0 ? white : trim, false);
            }
            Box("Archive rug", new Vector3(4.45f, 0.015f, 9.2f), new Vector3(2.0f, 0.03f, 1.75f), cloth, false);

            // Ceiling fixtures with warm brass accents.
            Cylinder("Study pendant shade", new Vector3(0.0f, 3.02f, 0.1f), new Vector3(0.4f, 0.09f, 0.4f), trim);
            Cylinder("Hall pendant shade", new Vector3(0.0f, 3.02f, 8.8f), new Vector3(0.35f, 0.08f, 0.35f), trim);
        }

        private void BuildEvidence(GameBootstrap owner)
        {
            AddEvidence(owner, "notebook", new Vector3(-1.55f, 0.0f, 0.42f), "book");
            AddEvidence(owner, "locklog", new Vector3(3.3f, 0.0f, 2.5f), "panel");
            AddEvidence(owner, "voice_note", new Vector3(-0.72f, 0.0f, 0.62f), "phone");
            AddEvidence(owner, "camera_loop", new Vector3(-1.5f, 0.0f, 7.35f), "screen");
            AddEvidence(owner, "footprints", new Vector3(0.75f, 0.0f, 5.7f), "footprints");
            AddEvidence(owner, "speaker", new Vector3(1.1f, 0.0f, 11.25f), "speaker");
            AddEvidence(owner, "breaker", new Vector3(-5.7f, 0.0f, 9.25f), "panel");
            AddEvidence(owner, "passage", new Vector3(4.5f, 0.0f, 9.3f), "hatch");
        }

        private void AddEvidence(GameBootstrap owner, string id, Vector3 position, string kind)
        {
            var definition = CaseCatalog.Get(id);
            if (definition == null) return;
            var spotObject = new GameObject("Evidence · " + id);
            spotObject.transform.SetParent(root.transform, true);
            spotObject.transform.position = position;
            if (id == "passage")
            {
                var hatchPuzzle = spotObject.AddComponent<HatchPuzzleSpot>();
                hatchPuzzle.InteractionId = id;
                hatchPuzzle.Owner = owner;
            }
            else
            {
                var spot = spotObject.AddComponent<EvidenceSpot>();
                spot.InteractionId = id;
                spot.Owner = owner;
            }
            var col = spotObject.AddComponent<SphereCollider>();
            col.radius = 0.52f;
            col.isTrigger = true;
            switch (kind)
            {
                case "book":
                    Box("Marked field notebook", position + new Vector3(0.02f, 0.98f, 0.01f), new Vector3(0.52f, 0.08f, 0.37f), darkWood, false, spotObject.transform).transform.rotation = Quaternion.Euler(0, -12, 0);
                    Box("Blue pencil mark", position + new Vector3(0.02f, 1.03f, 0.01f), new Vector3(0.3f, 0.012f, 0.025f), teal, false, spotObject.transform);
                    break;
                case "panel":
                    Box("Service panel casing", position + new Vector3(0, 1.1f, 0), new Vector3(0.62f, 1.05f, 0.15f), metal, false, spotObject.transform);
                    Box("Panel display", position + new Vector3(0, 1.27f, -0.085f), new Vector3(0.38f, 0.22f, 0.025f), teal, false, spotObject.transform);
                    for (int i = 0; i < 3; i++) Sphere("Panel indicator", position + new Vector3(-0.15f + i * 0.15f, 0.92f, -0.095f), new Vector3(0.045f, 0.045f, 0.045f), i == 1 ? red : amber, spotObject.transform);
                    break;
                case "phone":
                    Box("Recovered mobile phone", position + new Vector3(0, 0.72f, 0), new Vector3(0.22f, 0.035f, 0.42f), black, false, spotObject.transform);
                    Box("Phone screen", position + new Vector3(0, 0.742f, 0), new Vector3(0.18f, 0.008f, 0.35f), teal, false, spotObject.transform);
                    break;
                case "screen":
                    Box("Camera loop terminal", position + new Vector3(0, 1.25f, 0), new Vector3(0.9f, 0.58f, 0.1f), black, false, spotObject.transform);
                    Box("Camera loop screen", position + new Vector3(0, 1.25f, -0.058f), new Vector3(0.77f, 0.42f, 0.015f), teal, false, spotObject.transform);
                    Box("Loop frame", position + new Vector3(0.16f, 1.25f, -0.07f), new Vector3(0.28f, 0.18f, 0.02f), red, false, spotObject.transform);
                    break;
                case "footprints":
                    for (int i = 0; i < 5; i++)
                    {
                        var mark = Box("Plaster dust print", position + new Vector3((i % 2 == 0 ? -0.13f : 0.13f), 0.015f, i * 0.18f - 0.36f), new Vector3(0.14f, 0.018f, 0.24f), white, false, spotObject.transform);
                        mark.transform.rotation = Quaternion.Euler(0, i % 2 == 0 ? 20 : -20, 0);
                    }
                    break;
                case "speaker":
                    Cylinder("Smart speaker", position + new Vector3(0, 0.62f, 0), new Vector3(0.36f, 0.57f, 0.36f), black, spotObject.transform);
                    Cylinder("Speaker light ring", position + new Vector3(0, 0.97f, 0), new Vector3(0.32f, 0.025f, 0.32f), teal, spotObject.transform);
                    break;
                case "hatch":
                    Box("Concealed service hatch", position + new Vector3(0, 0.025f, 0), new Vector3(1.4f, 0.035f, 1.4f), metal, false, spotObject.transform);
                    Box("Hatch seam", position + new Vector3(0, 0.047f, 0), new Vector3(1.18f, 0.008f, 0.018f), black, false, spotObject.transform);
                    Box("Hatch pull", position + new Vector3(0.48f, 0.065f, 0.1f), new Vector3(0.18f, 0.025f, 0.05f), trim, false, spotObject.transform);
                    break;
            }
            var marker = Sphere("Evidence marker", position + new Vector3(0, 1.85f, 0), new Vector3(0.19f, 0.19f, 0.19f), amber, spotObject.transform);
            marker.AddComponent<EvidenceBeacon>();
            var stem = Cylinder("Evidence marker stem", position + new Vector3(0, 1.58f, 0), new Vector3(0.025f, 0.22f, 0.025f), amber, spotObject.transform);
        }

        private void BuildLighting()
        {
            var ambient = new GameObject("Moonlight directional").AddComponent<Light>();
            ambient.type = LightType.Directional;
            ambient.color = new Color(0.32f, 0.43f, 0.62f);
            ambient.intensity = 0.22f;
            ambient.transform.rotation = Quaternion.Euler(35f, -25f, 0f);
            ambient.shadows = LightShadows.Soft;

            Point("Study warm pool", new Vector3(-0.7f, 2.65f, 0.0f), new Color(1f, 0.61f, 0.31f), 3.2f, 7.2f);
            Point("Hall cold pool", new Vector3(0f, 2.7f, 8.7f), new Color(0.2f, 0.65f, 0.82f), 2.5f, 6.7f);
            Point("Utility warning light", new Vector3(-4.8f, 2.6f, 9.5f), new Color(0.85f, 0.24f, 0.13f), 1.45f, 4.5f);
            Point("Archive desk light", new Vector3(4.8f, 2.6f, 9.5f), new Color(0.9f, 0.62f, 0.3f), 2.0f, 4.8f);
        }

        private void Point(string name, Vector3 pos, Color color, float intensity, float range)
        {
            var go = new GameObject(name); go.transform.position = pos;
            var light = go.AddComponent<Light>(); light.type = LightType.Point; light.color = color; light.intensity = intensity; light.range = range; light.shadows = LightShadows.None;
        }

        private void BuildPlayer()
        {
            var player = new GameObject("Player · first person");
            player.transform.position = new Vector3(0f, 0.06f, -1.7f);
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f; controller.radius = 0.32f; controller.center = new Vector3(0, 0.9f, 0); controller.stepOffset = 0.25f; controller.slopeLimit = 45f;
            var pivot = new GameObject("Camera pivot").transform;
            pivot.SetParent(player.transform, false); pivot.localPosition = new Vector3(0, 1.58f, 0);
            var cameraObject = new GameObject("Investigation camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(pivot, false); cameraObject.transform.localPosition = Vector3.zero;
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 72f; camera.nearClipPlane = 0.05f; camera.farClipPlane = 70f;
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.05f);
            cameraObject.AddComponent<AudioListener>();
            var controllerScript = player.AddComponent<PlayerController>();
            controllerScript.CameraPivot = pivot;
            PlayerStart = player.transform;
            CameraPivot = pivot;
        }

        private void BuildSigns()
        {
            WorldLabel("STUDY  /  01", "غرفة الدراسة  /  ٠١", new Vector3(0, 2.8f, 3.6f), new Color(0.78f, 0.68f, 0.48f));
            WorldLabel("SERVICE  /  02", "الجناح الخدمي  /  ٠٢", new Vector3(0, 2.8f, 13.65f), new Color(0.35f, 0.72f, 0.78f));
            WorldLabel("UTILITY", "الخدمات", new Vector3(-4.6f, 2.9f, 7.05f), new Color(0.8f, 0.36f, 0.25f));
            WorldLabel("ARCHIVE", "الأرشيف", new Vector3(4.6f, 2.9f, 7.05f), new Color(0.78f, 0.68f, 0.48f));
        }

        private void WorldLabel(string english, string arabic, Vector3 pos, Color color)
        {
            var go = new GameObject("Room sign · " + english); go.transform.position = pos; go.transform.rotation = Quaternion.Euler(0, 180, 0);
            var mesh = go.AddComponent<TextMesh>(); mesh.fontSize = 48; mesh.characterSize = 0.045f; mesh.anchor = TextAnchor.MiddleCenter; mesh.alignment = TextAlignment.Center; mesh.color = color;
            var localizer = go.AddComponent<LocalizedWorldLabel>(); localizer.Configure(mesh, english, arabic);
        }
    }

    public sealed class LocalizedWorldLabel : MonoBehaviour
    {
        private TextMesh label;
        private string english, arabic;
        public void Configure(TextMesh textMesh, string englishText, string arabicText)
        {
            label = textMesh; english = englishText; arabic = arabicText; Refresh();
        }
        private void OnEnable() { Localization.LanguageChanged += Refresh; }
        private void OnDisable() { Localization.LanguageChanged -= Refresh; }
        private void Start() { Refresh(); }
        private void Refresh() { if (label != null) label.text = Localization.Pick(english, arabic); }
    }

    public sealed class EvidenceBeacon : MonoBehaviour
    {
        private Vector3 baseScale;
        private void Start() { baseScale = transform.localScale; }
        private void Update()
        {
            transform.Rotate(0f, 45f * Time.deltaTime, 0f, Space.World);
            float pulse = 1f + Mathf.Sin(Time.time * 2.8f) * 0.11f;
            transform.localScale = baseScale * pulse;
        }
    }
}
