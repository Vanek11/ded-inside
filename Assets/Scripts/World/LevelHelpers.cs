using UnityEngine;

namespace DI
{
    /// Общий строительный набор уровней: рельеф, свет/небо, плиты, трость,
    /// столб завещания, кладбище, чекпоинты, игрок, гуси.
    public static class LevelHelpers
    {
        public static readonly Color Grass = new Color(0.44f, 0.56f, 0.32f);
        public static readonly Color Dirt = new Color(0.47f, 0.40f, 0.31f);
        public static readonly Color Wood = new Color(0.56f, 0.42f, 0.27f);
        public static readonly Color Stone = new Color(0.57f, 0.57f, 0.58f);

        // лёгкие волны рельефа, чтобы не было «бесконечной плоскости»
        public static float Ground(float x, float z) => Mathf.Clamp(
            0.18f * Mathf.Sin(x * 0.23f) * Mathf.Sin(z * 0.19f) +
            0.09f * Mathf.Sin(x * 0.53f + 1.7f) * Mathf.Sin(z * 0.47f + 2.3f),
            -0.22f, 0.22f);

        /// Небо, солнце, туман, окружающий свет — сцена перестаёт быть «плоской заливкой».
        public static void Sky()
        {
            var sunGo = new GameObject("Sun");
            var l = sunGo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.1f;
            l.shadows = LightShadows.Soft;
            l.color = new Color(1f, 0.94f, 0.82f);
            sunGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            var sky = new Material(Shader.Find("Skybox/Procedural"));
            sky.SetFloat("_SunSize", 0.06f);
            sky.SetFloat("_AtmosphereThickness", 0.85f);
            sky.SetFloat("_Exposure", 1.25f);
            sky.SetColor("_SkyTint", new Color(0.55f, 0.7f, 0.9f));
            RenderSettings.skybox = sky;
            RenderSettings.sun = l;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.65f, 0.85f);
            RenderSettings.ambientEquatorColor = new Color(0.45f, 0.5f, 0.55f);
            RenderSettings.ambientGroundColor = new Color(0.35f, 0.32f, 0.28f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 70f;
            RenderSettings.fogEndDistance = 170f;
            RenderSettings.fogColor = new Color(0.72f, 0.8f, 0.9f);

            Clouds();
        }

        public static GameObject Slab(string name, Vector3 center, Vector3 size, float rotX, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = center;
            go.transform.localScale = size;
            go.transform.rotation = Quaternion.Euler(rotX, 0f, 0f);
            var m = go.GetComponent<Renderer>().material;
            m.color = color;
            m.SetFloat("_Glossiness", 0.12f);
            return go;
        }

        public static void TerrainMesh()
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
            var m = go.AddComponent<MeshRenderer>().material;
            m.color = Grass;
            m.SetFloat("_Glossiness", 0f);
            var col = go.AddComponent<MeshCollider>();
            col.sharedMesh = mesh;
        }

        /// Дом с крыльцом на старте (крыльцо топом на y=0.5, z -23..-13).
        public static void StartHouse()
        {
            Slab("Porch", new Vector3(0f, 0.25f, -18f), new Vector3(7f, 0.5f, 10f), 0f, Wood);
            Slab("PorchStep", new Vector3(0f, 0.17f, -12.6f), new Vector3(3f, 0.34f, 0.9f), 0f, Wood);
            Slab("House", new Vector3(0f, 3.2f, -24.5f), new Vector3(10f, 6.4f, 2f), 0f,
                new Color(0.75f, 0.62f, 0.5f));
            Slab("Door", new Vector3(0f, 1.5f, -23.4f), new Vector3(1.2f, 3f, 0.2f),
                0f, new Color(0.25f, 0.16f, 0.1f));
        }

        public static void Tree(float x, float z)
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

        public static void Checkpoint(Vector3 pos, Vector3 size, string id)
        {
            var go = new GameObject($"Checkpoint_{id}");
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = size;
            go.transform.position = pos;
            var cp = go.AddComponent<CheckpointZone>();
            cp.checkpointId = id;
        }

        public static GameObject BuildCane(Vector3 position)
        {
            var cane = new GameObject("Cane");
            cane.layer = LayerMask.NameToLayer("Grabbable");
            cane.transform.position = position;
            cane.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
            cane.AddComponent<NetDynamicMarker>();

            var rb = cane.AddComponent<Rigidbody>();
            rb.mass = 2f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stick.name = "Stick";
            stick.transform.SetParent(cane.transform, false);
            stick.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);
            stick.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            stick.GetComponent<Renderer>().material.color = new Color(0.42f, 0.28f, 0.14f);

            var handle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handle.name = "Handle";
            handle.transform.SetParent(cane.transform, false);
            handle.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
            handle.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            handle.GetComponent<Renderer>().material.color = new Color(0.2f, 0.14f, 0.08f);
            return cane;
        }

        /// Столб с ящиком + зона завещания (конверт Буканову).
        public static void WillPost(Vector3 basePos)
        {
            Slab("WillPost", basePos + new Vector3(0f, 1.1f, 0f), new Vector3(0.25f, 2.2f, 0.25f), 0f, Wood);
            Slab("Mailbox", basePos + new Vector3(0f, 2.35f, 0f), new Vector3(0.7f, 0.5f, 0.45f), 0f,
                new Color(0.35f, 0.55f, 0.3f));

            var zone = new GameObject("WillZone");
            var col = zone.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(4f, 2.5f, 4f);
            zone.transform.position = basePos + new Vector3(0f, 1.2f, 0f);
            zone.AddComponent<WillZone>();
        }

        /// Кладбище: могила-цель, надгробия, ограда. baseY — высота плиты-плато.
        public static void Cemetery(float baseY, float goalZ)
        {
            for (int r = 0; r < 3; r++)
                foreach (float x in new[] { -6f, -4f, -2f, 2f, 4f, 6f })
                    if ((r + Mathf.Abs(x)) % 3f != 0f)
                        Slab($"Grave_{r}_{x}", new Vector3(x, baseY + 0.58f, goalZ - 4f + r * 3.2f),
                            new Vector3(0.7f, 1.1f, 0.18f), 0f, Stone);

            Slab("Soil", new Vector3(0f, baseY + 0.055f, goalZ), new Vector3(2.4f, 0.02f, 3f), 0f,
                new Color(0.3f, 0.22f, 0.14f));

            var goalGo = new GameObject("GoalZone");
            var col = goalGo.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(3f, 1.6f, 3.2f);
            goalGo.transform.position = new Vector3(0f, baseY + 0.8f, goalZ);
            goalGo.AddComponent<GoalZone>();

            for (int z = (int)goalZ - 8; z <= (int)goalZ + 6; z += 2)
                foreach (float x in new[] { -7f, 7f })
                    if (z != (int)goalZ - 2 && z != (int)goalZ)
                        Slab($"Fence{x}_{z}", new Vector3(x, baseY + 0.65f, z),
                            new Vector3(0.15f, 1.2f, 0.15f), 0f, Stone);
            Slab("FenceLeft", new Vector3(-7f, baseY + 1.1f, goalZ - 1f), new Vector3(0.1f, 0.12f, 13f), 0f, Stone);
            Slab("FenceRight", new Vector3(7f, baseY + 1.1f, goalZ - 1f), new Vector3(0.1f, 0.12f, 13f), 0f, Stone);

            Caretaker(new Vector3(3.2f, baseY, goalZ - 11f));
        }

        /// Ворчливый смотритель: стоит у ограды, комментирует доставку.
        static void Caretaker(Vector3 basePos)
        {
            var go = new GameObject("Caretaker");
            go.transform.position = basePos + new Vector3(0f, 0.85f, 0f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(go.transform, false);
            body.transform.localScale = new Vector3(0.9f, 0.85f, 0.9f);
            body.GetComponent<Renderer>().material.color = new Color(0.25f, 0.32f, 0.24f);

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(go.transform, false);
            head.transform.localPosition = new Vector3(0f, 1f, 0f);
            head.transform.localScale = Vector3.one * 0.42f;
            head.GetComponent<Renderer>().material.color = new Color(0.8f, 0.65f, 0.5f);

            var hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hat.name = "Hat";
            hat.transform.SetParent(go.transform, false);
            hat.transform.localPosition = new Vector3(0f, 1.28f, 0f);
            hat.transform.localScale = new Vector3(0.85f, 0.15f, 0.85f);
            hat.GetComponent<Renderer>().material.color = new Color(0.14f, 0.14f, 0.16f);

            go.AddComponent<GrumpCaretaker>();
        }

        public static void SpawnPlayer(Vector3 pos) => PlayerFactory.BuildPlayer(pos, true);

        public static GameObject SpawnGoose(Vector3 pos)
        {
            var go = new GameObject($"Goose_{pos.x}_{pos.z}");
            go.transform.position = pos;
            go.AddComponent<NetDynamicMarker>();

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 6f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(go.transform, false);
            body.transform.localScale = new Vector3(0.4f, 0.4f, 0.6f);
            body.GetComponent<Renderer>().material.color = Color.white;
            Object.DestroyImmediate(body.GetComponent<Collider>());

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
            return go;
        }

        /// Мелкая вода: триггер-объём + «зеркальная» плоскость без коллайдера.
        public static void Water(Vector3 center, Vector3 size, float surfaceY)
        {
            var waterGo = new GameObject("Water");
            var col = waterGo.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = size;
            waterGo.transform.position = center;
            waterGo.AddComponent<WaterVolume>().surfaceY = surfaceY;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "WaterVisual";
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.transform.position = new Vector3(center.x, surfaceY, center.z);
            visual.transform.localScale = new Vector3(size.x, 0.04f, size.z);
            var m = visual.GetComponent<Renderer>().material;
            m.color = new Color(0.3f, 0.5f, 0.68f, 0.85f);
            m.SetFloat("_Glossiness", 0.75f);
        }

        /// Гроб с дедом внутри, дед и гроб получают ссылки друг на друга.
        public static CoffinBody CoffinWithGrandpa(Vector3 pos, Quaternion rot)
        {
            var coffin = CoffinFactory.Build(pos, rot);
            var grandpa = GrandpaFactory.Build(coffin.transform);
            coffin.Grandpa = grandpa;
            return coffin;
        }

        /// Хвост уровня: менеджер + HUD + пауза + ворона + сетевой бутстрап.
        public static void AttachCore(GameObject levelGo)
        {
            if (DIGameManager.I == null) levelGo.AddComponent<DIGameManager>();
            levelGo.AddComponent<HUD>();
            levelGo.AddComponent<PauseMenu>();
            Crow.Spawn(new Vector3(0f, 9f, 10f), 40f);
            if (SessionState.Mode != SessionMode.Solo)
                levelGo.AddComponent<DINetworkManager>().Bootstrap(SessionState.Mode, SessionState.Ip);
        }

        /// Плоские облака — дешёвый шарм для неба.
        public static void Clouds()
        {
            var cloudMat = new Material(Shader.Find("Standard"));
            cloudMat.color = new Color(1f, 1f, 1f);
            cloudMat.SetFloat("_Glossiness", 0f);

            foreach (var p in new[]
                     {
                         new Vector3(-22f, 30f, -10f), new Vector3(18f, 34f, 15f),
                         new Vector3(-14f, 32f, 45f), new Vector3(25f, 28f, 60f),
                         new Vector3(-28f, 36f, 80f), new Vector3(10f, 33f, 95f)
                     })
            {
                var cloud = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cloud.name = "Cloud";
                Object.DestroyImmediate(cloud.GetComponent<Collider>());
                cloud.transform.position = p;
                cloud.transform.localScale = new Vector3(9f, 2.2f, 5f);
                cloud.GetComponent<Renderer>().sharedMaterial = cloudMat;
            }
        }
    }
}
