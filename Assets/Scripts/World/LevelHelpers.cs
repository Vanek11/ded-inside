using System.Collections.Generic;
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

        // вырезы рельефа (пруды, броды, овраги) — уровень задаёт перед постройкой
        struct Carve { public Vector3 center; public float halfW, halfL, depth; }
        static readonly List<Carve> carves = new List<Carve>();

        public static void SetCarves(params (Vector3 center, float halfW, float halfL, float depth)[] list)
        {
            carves.Clear();
            foreach (var (c, w, l, d) in list) carves.Add(new Carve { center = c, halfW = w, halfL = l, depth = d });
        }

        static float CarveDepth(float x, float z)
        {
            float result = 0f;
            foreach (var cv in carves)
            {
                float dx = (x - cv.center.x) / cv.halfW;
                float dz = (z - cv.center.z) / cv.halfL;
                float d = Mathf.Sqrt(dx * dx + dz * dz);
                float t = 1f - Mathf.SmoothStep(0.6f, 1.05f, d); // 1 в центре, 0 за краем эллипса
                result = Mathf.Max(result, cv.depth * t);
            }
            return result;
        }

        // дорога в низине: у дороги пологие берега, большие холмы — вдали (рамка мира)
        public static float Ground(float x, float z)
        {
            float waves = 0.18f * Mathf.Sin(x * 0.23f) * Mathf.Sin(z * 0.19f) +
                          0.09f * Mathf.Sin(x * 0.53f + 1.7f) * Mathf.Sin(z * 0.47f + 2.3f);
            float shoulder = Mathf.SmoothStep(4f, 14f, Mathf.Abs(x))
                             * (0.7f + 0.35f * Mathf.Sin(x * 0.3f + z * 0.2f));
            float far = Mathf.SmoothStep(14f, 32f, Mathf.Abs(x));
            float hill = far * (2.6f + 2.4f * Mathf.Sin(x * 0.07f + 1.2f) * Mathf.Cos(z * 0.06f + 0.5f)
                                 + 1.4f * Mathf.Sin(z * 0.13f + x * 0.05f));
            return Mathf.Clamp(waves + shoulder + hill - CarveDepth(x, z), -4f, 8f);
        }

        /// Небо, солнце, туман, окружающий свет — сцена перестаёт быть «плоской заливкой».
        public static void Sky()
        {
            var sunGo = new GameObject("Sun");
            var l = sunGo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.05f;
            l.shadows = LightShadows.Soft;
            l.color = new Color(1f, 0.93f, 0.8f);
            sunGo.transform.rotation = Quaternion.Euler(55f, -35f, 0f);

            var sky = new Material(Shader.Find("Skybox/Procedural"));
            sky.SetFloat("_SunSize", 0.06f);
            sky.SetFloat("_AtmosphereThickness", 0.85f);
            sky.SetFloat("_Exposure", 1.25f);
            sky.SetColor("_SkyTint", new Color(0.55f, 0.7f, 0.9f));
            RenderSettings.skybox = sky;
            RenderSettings.sun = l;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.4f, 0.48f, 0.6f);
            RenderSettings.ambientEquatorColor = new Color(0.3f, 0.34f, 0.38f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.17f);
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
            // земляные плиты (дороги) — толстые, с «бермой» ниже уровня травы
            if (color == Dirt)
            {
                float top = center.y + size.y * 0.5f;
                size.y = Mathf.Max(size.y, 0.9f);
                center.y = top - size.y * 0.5f;
            }
            go.transform.position = center;
            go.transform.localScale = size;
            go.transform.rotation = Quaternion.Euler(rotX, 0f, 0f);
            string tex = color == Dirt ? "dirt" : color == Wood ? "wood"
                : color == Stone ? "stone" : color == Grass ? "grass" : null;
            Art.Paint(go.GetComponent<Renderer>(), color, 0.08f, 0f, tex);
            return go;
        }

        /// Плато от верха centerTop.y до глубоко под землёй: массив, не доска.
        public static GameObject SlabEarth(string name, Vector3 centerTop, Vector2 xz, Color color)
        {
            float top = centerTop.y, bottom = -3f;
            return Slab(name, new Vector3(centerTop.x, (top + bottom) * 0.5f, centerTop.z),
                new Vector3(xz.x, top - bottom, xz.y), 0f, color);
        }

        /// Найти поверхность земли над точкой (для респаунов) — чтобы не родиться под землёй.
        public static Vector3 SurfacePoint(Vector3 pos)
        {
            if (Physics.Raycast(new Ray(pos + Vector3.up * 80f, Vector3.down), out var hit, 300f))
                return hit.point + Vector3.up * 0.2f;
            return pos;
        }

        /// Последний пройденный чекпоинт — сюда возвращаем упавших за край мира.
        public static Vector3 LastCheckpoint = new Vector3(1.5f, 1.2f, -15f);

        public static void TerrainMesh()
        {
            const float half = 100f, cell = 1.25f; // мир 200×200 м, гладкие холмы
            int n = Mathf.RoundToInt(half * 2 / cell);
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

            // пятнистая раскраска луга vertex color'ами
            var colors = new Color[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                float v = verts[i].x, w = verts[i].z;
                float patch = Mathf.Sin(v * 0.21f + 1.3f) * Mathf.Cos(w * 0.17f + 0.4f)
                              + 0.5f * Mathf.Sin(v * 0.55f + w * 0.41f);
                float shade = Mathf.Clamp01(0.5f + patch * 0.35f);
                colors[i] = Color.Lerp(new Color(0.34f, 0.46f, 0.26f), new Color(0.2f, 0.32f, 0.17f), shade);
                if (verts[i].y > 1.4f) colors[i] = Color.Lerp(colors[i], new Color(0.4f, 0.38f, 0.26f), 0.4f);
            }
            mesh.colors = colors;

            var go = new GameObject("Terrain");
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            var m = Art.VertexColorMaterial();
            if (m != null) r.sharedMaterial = m; else Art.Paint(r, Grass, 0f, 0f, "grass", 44f);
            var col = go.AddComponent<MeshCollider>();
            col.sharedMesh = mesh;
        }

        /// Дом с крыльцом, двускатной крышей, трубой и тёплыми окнами.
        public static void StartHouse()
        {
            Slab("Porch", new Vector3(0f, 0.25f, -18f), new Vector3(7f, 0.5f, 10f), 0f, Wood);
            Slab("PorchStep", new Vector3(0f, 0.17f, -12.6f), new Vector3(3f, 0.34f, 0.9f), 0f, Wood);

            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "House";
            wall.transform.position = new Vector3(0f, 3.2f, -24.5f);
            wall.transform.localScale = new Vector3(10f, 6.4f, 2f);
            Art.Paint(wall.GetComponent<Renderer>(), new Color(0.78f, 0.66f, 0.52f), 0.05f);

            Slab("Door", new Vector3(0f, 1.5f, -23.4f), new Vector3(1.2f, 3f, 0.2f),
                0f, new Color(0.25f, 0.16f, 0.1f));

            // двускатная крыша: конёк по центру, свес над стеной
            foreach (float side in new[] { -1f, 1f })
            {
                var slope = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slope.name = "RoofSlope";
                slope.transform.position = new Vector3(side * 3.1f, 7.85f, -24.5f);
                slope.transform.localScale = new Vector3(6.8f, 0.16f, 11.2f);
                slope.transform.rotation = Quaternion.Euler(0f, 0f, -side * 25f);
                Art.Paint(slope.GetComponent<Renderer>(), new Color(0.42f, 0.24f, 0.18f), 0.04f);
            }
            Slab("Chimney", new Vector3(3.4f, 7.6f, -26f), new Vector3(0.55f, 1.3f, 0.55f), 0f, Stone);

            // тёплые окна (светятся с bloom'ом)
            foreach (float wx in new[] { -3f, 3f })
            {
                var win = GameObject.CreatePrimitive(PrimitiveType.Cube);
                win.name = "Window";
                win.transform.position = new Vector3(wx, 3.4f, -23.45f);
                win.transform.localScale = new Vector3(0.95f, 1.15f, 0.08f);
                Art.Emissive(win.GetComponent<Renderer>(), new Color(1f, 0.75f, 0.35f), 2f);
                Slab($"WindowFrame{wx}", new Vector3(wx, 3.4f, -23.36f), new Vector3(1.1f, 1.3f, 0.05f), 0f, Wood);
            }

            // телега у дома и забор по краю участка
            Art.Model("town/cart", new Vector3(-6.3f, Ground(-6.3f, -20.5f), -20.5f), 25f, 1f, 0.5f);
            for (int i = 0; i < 5; i++)
                Art.Model(i == 2 ? "town/fence-gate" : "town/fence",
                    new Vector3(6.5f, Ground(6.5f, -24f + i * 2f), -24f + i * 2f), 90f, 1f, 0.5f);
        }

        public static void Tree(float x, float z)
        {
            float y = Ground(x, z);
            // три варианта деревьев Kenney, стабильный «случайный» выбор по координатам
            float h = Mathf.Repeat(x * 0.37f + z * 0.53f, 1f);
            string kind = h < 0.4f ? "nature/tree_detailed"
                : h < 0.7f ? "nature/tree_detailed_dark" : "nature/tree_cone";
            float scale = 0.9f + h * 0.35f;
            Art.Model(kind, new Vector3(x, y, z), h * 360f, scale, 0.35f);
        }

        /// Рассыпать траву, пеньки и грибы по лугам (не на дороге).
        public static void ScatterNature()
        {
            var rnd = new System.Random(20261027);
            for (int i = 0; i < 34; i++)
            {
                float x = (float)(rnd.NextDouble() * 110 - 55);
                float z = (float)(rnd.NextDouble() * 130 - 25);
                if (Mathf.Abs(x) < 3.5f) continue; // не на дороге
                float y = Ground(x, z);
                float pick = (float)rnd.NextDouble();
                if (CarveDepth(x, z) > 0.25f) continue; // не в воде
                string rel = pick < 0.55f ? "nature/grass" : pick < 0.8f ? "nature/grass_leafs"
                    : pick < 0.92f ? "nature/mushroom_redGroup" : "nature/stump_roundDetailed";
                float rot = (float)rnd.NextDouble() * 360f;
                Art.Model(rel, new Vector3(x, y, z), rot, 1f, 0f);
            }
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
            // надгробия Kenney, 4 варианта по кругу
            var stones = new[] { "graveyard/gravestone-round", "graveyard/gravestone-cross",
                                 "graveyard/gravestone-wide", "graveyard/gravestone-roof" };
            int idx = 0;
            for (int r = 0; r < 3; r++)
                foreach (float x in new[] { -6f, -4f, -2f, 2f, 4f, 6f })
                    if ((r + Mathf.Abs(x)) % 3f != 0f)
                    {
                        float gz = goalZ - 4f + r * 3.2f;
                        Art.Model(stones[idx % stones.Length], new Vector3(x, baseY, gz),
                            (idx % 2) * 180f, 1f, 0.9f);
                        idx++;

                        // цветы у подножия
                        for (int f = 0; f < 2; f++)
                        {
                            var flower = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            flower.name = $"Flower_{r}_{x}_{f}";
                            flower.transform.position = new Vector3(
                                x + (f == 0 ? 0.22f : -0.25f), baseY + 0.06f, gz + 0.18f);
                            flower.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
                            Art.Paint(flower.GetComponent<Renderer>(),
                                f == 0 ? new Color(0.9f, 0.5f, 0.65f) : new Color(0.95f, 0.85f, 0.4f), 0.3f);
                        }
                    }

            Slab("Soil", new Vector3(0f, baseY + 0.055f, goalZ), new Vector3(2.4f, 0.02f, 3f), 0f,
                new Color(0.3f, 0.22f, 0.14f));

            // крест на могиле-цели
            Art.Model("graveyard/cross", new Vector3(0f, baseY, goalZ + 1.3f), 0f, 1.2f, 0.9f);

            var goalGo = new GameObject("GoalZone");
            var col = goalGo.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(3f, 1.6f, 3.2f);
            goalGo.transform.position = new Vector3(0f, baseY + 0.8f, goalZ);
            goalGo.AddComponent<GoalZone>();

            // железная ограда с воротами
            for (int z = (int)goalZ - 8; z <= (int)goalZ + 6; z += 2)
                foreach (float x in new[] { -7f, 7f })
                    if (z != (int)goalZ - 2 && z != (int)goalZ)
                        Art.Model("graveyard/iron-fence", new Vector3(x, baseY, z), 90f, 1f, 0.6f);
            Art.Model("graveyard/iron-fence-border-gate", new Vector3(-7f, baseY, goalZ - 1f), 90f, 1f, 0f);
            Art.Model("graveyard/iron-fence-border-gate", new Vector3(7f, baseY, goalZ - 1f), 90f, 1f, 0f);

            // фонари у входа: тёплые светящиеся
            foreach (float x in new[] { -3.2f, 3.2f })
            {
                var lantern = Art.Model("graveyard/lantern-glass", new Vector3(x, baseY, goalZ - 9.5f), 0f, 1f, 0.3f);
                if (lantern != null)
                {
                    var light = lantern.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = new Color(1f, 0.75f, 0.4f);
                    light.intensity = 1.6f;
                    light.range = 9f;
                }
            }

            Caretaker(new Vector3(3.2f, baseY, goalZ - 11f));
        }

        /// Ворчливый смотритель: модель смотрителя кладбища Kenney + логика ворчания.
        static void Caretaker(Vector3 basePos)
        {
            var go = Art.Model("graveyard/character-keeper", basePos, 180f, 1f, 0.9f);
            if (go == null)
            {
                // запасной примитив, если модель не загрузилась
                var fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                fallback.transform.position = basePos + Vector3.up * 0.85f;
                fallback.name = "Caretaker";
                fallback.AddComponent<GrumpCaretaker>();
                return;
            }
            go.name = "Caretaker";
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
            Art.Transparent(visual.GetComponent<Renderer>(), new Color(0.3f, 0.5f, 0.68f, 0.8f), 0.85f);
        }

        /// Вода-эллипс точно по урезу берега: диск по сегментам, края под рельефом.
        public static void WaterEllipse(Vector2 center, float rx, float rz, float surfaceY)
        {
            var waterGo = new GameObject("Water");
            var col = waterGo.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(rx * 2f, 3f, rz * 2f);
            waterGo.transform.position = new Vector3(center.x, surfaceY - 1.4f, center.y);
            waterGo.AddComponent<WaterVolume>().surfaceY = surfaceY;

            const int seg = 40;
            var verts = new Vector3[seg + 1];
            var tris = new int[seg * 3];
            verts[seg] = Vector3.zero;
            for (int i = 0; i < seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                verts[i] = new Vector3(Mathf.Cos(a) * rx, 0f, Mathf.Sin(a) * rz);
                tris[i * 3] = seg; tris[i * 3 + 1] = (i + 1) % seg; tris[i * 3 + 2] = i;
            }
            var mesh = new Mesh { vertices = verts, triangles = tris };
            mesh.RecalculateNormals();

            var disc = new GameObject("WaterVisual");
            disc.transform.position = new Vector3(center.x, surfaceY, center.y);
            disc.AddComponent<MeshFilter>().sharedMesh = mesh;
            var wr = disc.AddComponent<MeshRenderer>();
            Art.Transparent(wr, new Color(0.26f, 0.46f, 0.62f, 0.82f), 0.9f);
            wr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // иначе «парит» своя тень
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
            ScatterNature();
            Crow.Spawn(new Vector3(0f, 9f, 10f), 40f);
            var cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (cam != null) Art.SetupPostFx(cam);
#if ADMIN
            levelGo.AddComponent<AdminPanel>(); // только в dev-сборке
#endif
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
