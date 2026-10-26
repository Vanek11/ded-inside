using UnityEngine;

namespace DI
{
    /// «Деревня-1» — серый бокс: дом с крыльцом → дорога → пригорок (санки) →
    /// пруд с мостком и гусями → подъём → кладбище с могилой. Строится кодом.
    public class LevelVillage1 : MonoBehaviour
    {
        public static LevelVillage1 I { get; private set; }

        [SerializeField, HideInInspector] bool built;

        static readonly Color Grass = new Color(0.42f, 0.55f, 0.30f);
        static readonly Color Dirt = new Color(0.45f, 0.38f, 0.29f);
        static readonly Color Wood = new Color(0.55f, 0.40f, 0.26f);
        static readonly Color Stone = new Color(0.55f, 0.55f, 0.56f);

        void Awake()
        {
            I = this;
            if (built) return;
            BuildOnce();
        }

        /// Строит уровень в редакторе (из SceneSetup): у обычных скриптов Awake
        /// в edit mode не вызывается, поэтому редакторский путь — через этот метод.
        public void EditorBuild()
        {
            if (built) return;
            BuildOnce();
        }

        void BuildOnce()
        {
            built = true;

            if (DIGameManager.I == null) gameObject.AddComponent<DIGameManager>();
            gameObject.AddComponent<HUD>();
            Sun();
            TerrainMesh();
            StartHouse();
            Road();
            Pond();
            Cemetery();
            Trees();
            CoffinAndGrandpa();
            Player();
            Checkpoints();
        }

        public void OnCheckpoint(CheckpointZone cp) =>
            DIGameManager.I?.Event($"Чекпоинт «{cp.checkpointId}» пройден!");

        // рельеф: лёгкие волны, чтобы не было «бесконечной плоскости»
        static float Ground(float x, float z) => Mathf.Clamp(
            0.18f * Mathf.Sin(x * 0.23f) * Mathf.Sin(z * 0.19f) +
            0.09f * Mathf.Sin(x * 0.53f + 1.7f) * Mathf.Sin(z * 0.47f + 2.3f),
            -0.22f, 0.22f);

        static void Sun()
        {
            var go = new GameObject("Sun");
            var l = go.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.15f;
            l.shadows = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
        }

        static void TerrainMesh()
        {
            const int half = 60, cell = 2;
            int n = half * 2 / cell;
            var verts = new Vector3[(n + 1) * (n + 1)];
            var uv = new Vector2[verts.Length];
            var tris = new int[n * n * 6];

            for (int j = 0; j <= n; j++)
                for (int i = 0; i <= n; i++)
                {
                    float x = -half + i * cell, z = -half + j * cell;
                    int idx = j * (n + 1) + i;
                    verts[idx] = new Vector3(x, Ground(x, z), z);
                    uv[idx] = new Vector2(i / (float)n, j / (float)n);
                }

            int t = 0;
            for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++)
                {
                    int a = j * (n + 1) + i, b = a + 1, c = a + n + 1, d = c + 1;
                    tris[t++] = a; tris[t++] = c; tris[t++] = b;
                    tris[t++] = b; tris[t++] = c; tris[t++] = d;
                }

            var mesh = new Mesh { vertices = verts, uv = uv, triangles = tris };
            mesh.RecalculateNormals();

            var go = new GameObject("Terrain");
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().material.color = Grass;
            var col = go.AddComponent<MeshCollider>();
            col.sharedMesh = mesh;
        }

        static GameObject Slab(string name, Vector3 center, Vector3 size, float rotX, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = center;
            go.transform.localScale = size;
            go.transform.rotation = Quaternion.Euler(rotX, 0f, 0f);
            go.GetComponent<Renderer>().material.color = color;
            return go;
        }

        static void StartHouse()
        {
            Slab("Porch", new Vector3(0f, 0.25f, -18f), new Vector3(7f, 0.5f, 10f), 0f, Wood);
            Slab("PorchStep", new Vector3(0f, 0.17f, -12.6f), new Vector3(3f, 0.34f, 0.9f), 0f, Wood);
            Slab("House", new Vector3(0f, 3.2f, -24.5f), new Vector3(10f, 6.4f, 2f), 0f,
                new Color(0.75f, 0.62f, 0.5f));
            Slab("Door", new Vector3(0f, 1.5f, -23.4f), new Vector3(1.2f, 3f, 0.2f),
                0f, new Color(0.25f, 0.16f, 0.1f));
        }

        static void Road()
        {
            Slab("Road0", new Vector3(0f, 0.03f, -4f), new Vector3(3.4f, 0.1f, 20f), 0f, Dirt);
            Slab("RoadSlopeUp", new Vector3(0f, 0.525f, 11f), new Vector3(3.4f, 0.1f, 10.2f), -6f, Dirt);
            Slab("RoadHillTop", new Vector3(0f, 1.06f, 20f), new Vector3(3.4f, 0.1f, 8.4f), 0f, Dirt);
            Slab("RoadSlopeDown", new Vector3(0f, 0.52f, 29f), new Vector3(3.4f, 0.1f, 10.2f), 6f, Dirt);
            Slab("Road1", new Vector3(0f, 0.03f, 35f), new Vector3(3.4f, 0.1f, 2.4f), 0f, Dirt);
            Slab("Road2", new Vector3(0f, 0.03f, 56f), new Vector3(3.4f, 0.1f, 12f), 0f, Dirt);
            Slab("RoadSlopeUp2", new Vector3(0f, 0.84f, 68f), new Vector3(3.4f, 0.1f, 12.3f), -8f, Dirt);
            Slab("Plateau", new Vector3(0f, 1.70f, 81f), new Vector3(10f, 0.1f, 14f), 0f, Dirt);
        }

        static void Pond()
        {
            // вода: триггер-объём + полупрозрачная «зеркальная» плоскость без коллайдера
            var waterGo = new GameObject("Water");
            var col = waterGo.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(16f, 0.8f, 12f);
            waterGo.transform.position = new Vector3(0f, -0.3f, 42f);
            waterGo.AddComponent<WaterVolume>().surfaceY = 0.1f;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "WaterVisual";
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.transform.position = new Vector3(0f, 0.1f, 42f);
            visual.transform.localScale = new Vector3(16f, 0.04f, 12f);
            visual.GetComponent<Renderer>().material.color = new Color(0.25f, 0.45f, 0.65f, 0.85f);

            // мосток из досок через пруд
            for (int z = 34; z < 50; z += 2)
                Slab($"Plank{z}", new Vector3(0f, 0.36f, z + 1f), new Vector3(3.2f, 0.08f, 1.8f), 0f, Wood);
        }

        static void Cemetery()
        {
            for (int r = 0; r < 3; r++)
                foreach (float x in new[] { -6f, -4f, -2f, 2f, 4f, 6f })
                    if ((r + x) % 3 != 0)
                        Slab($"Grave_{r}_{x}", new Vector3(x, 2.25f + r * 0f, 78f + r * 3.2f),
                            new Vector3(0.7f, 1.1f, 0.18f), 0f, Stone);

            Slab("Soil", new Vector3(0f, 1.755f, 82f), new Vector3(2.4f, 0.02f, 3f), 0f,
                new Color(0.3f, 0.22f, 0.14f));

            var goalGo = new GameObject("GoalZone");
            var col = goalGo.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(3f, 1.6f, 3.2f);
            goalGo.transform.position = new Vector3(0f, 2.4f, 82f);
            goalGo.AddComponent<GoalZone>();

            // ограда
            for (int z = 74; z <= 88; z += 2)
                foreach (float x in new[] { -7f, 7f })
                    if (z != 80 && z != 82)
                        Slab($"Fence{x}_{z}", new Vector3(x, 2.35f, z), new Vector3(0.15f, 1.2f, 0.15f), 0f, Stone);
            Slab("FenceLeft", new Vector3(-7f, 2.8f, 81f), new Vector3(0.1f, 0.12f, 13f), 0f, Stone);
            Slab("FenceRight", new Vector3(7f, 2.8f, 81f), new Vector3(0.1f, 0.12f, 13f), 0f, Stone);
        }

        static void Tree(float x, float z)
        {
            float y = Ground(x, z);
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.position = new Vector3(x, y + 1.5f, z);
            trunk.transform.localScale = new Vector3(0.5f, 3f, 0.5f);
            trunk.GetComponent<Renderer>().material.color = new Color(0.35f, 0.25f, 0.15f);

            var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "Crown";
            crown.transform.position = new Vector3(x, y + 4.2f, z);
            crown.transform.localScale = new Vector3(3.4f, 2.6f, 3.4f);
            crown.GetComponent<Renderer>().material.color = new Color(0.28f, 0.45f, 0.22f);
        }

        static void Trees()
        {
            Tree(-9f, -6f); Tree(9f, 2f); Tree(-10f, 14f); Tree(10f, 26f);
            Tree(-9f, 44f); Tree(10f, 55f); Tree(-11f, 66f); Tree(11f, 86f);
        }

        static void Goose(float x, float z)
        {
            var go = new GameObject($"Goose_{x}_{z}");
            go.transform.position = new Vector3(x, Ground(x, z) + 0.4f, z);
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 6f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(go.transform, false);
            body.transform.localScale = new Vector3(0.4f, 0.4f, 0.6f);
            body.GetComponent<Renderer>().material.color = Color.white;
            Object.DestroyImmediate(body.GetComponent<Collider>()); // коллайдер — на корне

            var rootCol = go.AddComponent<SphereCollider>();
            rootCol.radius = 0.3f;

            var beak = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beak.name = "Beak";
            beak.transform.SetParent(go.transform, false);
            beak.transform.localPosition = new Vector3(0f, 0f, 0.32f);
            beak.transform.localScale = new Vector3(0.08f, 0.08f, 0.16f);
            beak.GetComponent<Renderer>().material.color = new Color(0.95f, 0.6f, 0.1f);
            Object.DestroyImmediate(beak.GetComponent<Collider>());

            go.AddComponent<JumpScare>();
        }

        static void CoffinAndGrandpa()
        {
            var coffin = CoffinFactory.Build(new Vector3(0f, 0.84f, -19f), Quaternion.identity);
            var grandpa = GrandpaFactory.Build(coffin.transform);
            coffin.Grandpa = grandpa;

            Goose(2.5f, 50f);
            Goose(-2f, 51.5f);
        }

        static void Player()
        {
            var go = new GameObject("Player");
            go.tag = "Player";
            go.layer = LayerMask.NameToLayer("Player"); // нужен проверке земли
            go.transform.position = new Vector3(1.5f, 1.2f, -15f);

            var col = go.AddComponent<CapsuleCollider>();
            col.radius = 0.34f;
            col.height = 1.7f;
            col.center = new Vector3(0f, 0.85f, 0f);

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 80f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Body";
            Object.Destroy(visual.GetComponent<Collider>());
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            visual.transform.localScale = new Vector3(0.62f, 1.2f, 0.5f);
            visual.GetComponent<Renderer>().material.color = new Color(0.9f, 0.5f, 0.15f);

            var controller = go.AddComponent<PlayerController>();
            go.AddComponent<GrabSystem>();

            var camGo = new GameObject("MainCamera");
            camGo.tag = "MainCamera";
            camGo.transform.position = go.transform.position + new Vector3(0f, 3f, -6f);
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            var cam = camGo.AddComponent<ThirdPersonCamera>();
            cam.target = go.transform;
            controller.CameraTransform = camGo.transform;
        }

        static void Checkpoints()
        {
            Checkpoint(new Vector3(0f, 1.6f, 26f), new Vector3(4f, 3f, 3f), "Пригорок");
            Checkpoint(new Vector3(0f, 1.2f, 51.5f), new Vector3(4f, 2.5f, 3f), "Мосток");
            Checkpoint(new Vector3(0f, 2.8f, 76f), new Vector3(5f, 3f, 3f), "Кладбище");
        }

        static void Checkpoint(Vector3 pos, Vector3 size, string id)
        {
            var go = new GameObject($"Checkpoint_{id}");
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = size;
            go.transform.position = pos;
            var cp = go.AddComponent<CheckpointZone>();
            cp.checkpointId = id;
        }
    }
}
