using UnityEngine;

namespace DI
{
    /// «Деревня-2 · Огород»: дом → дорога с грядками (заноси гроб через борты!)
    /// → пугало → брод через ручей → колодец → подъём → кладбище.
    public class LevelVillage2 : MonoBehaviour
    {
        public static LevelVillage2 I { get; private set; }

        [SerializeField, HideInInspector] bool built;

        void Awake()
        {
            I = this;
            if (built) return;
            BuildOnce();
        }

        public void EditorBuild()
        {
            if (built) return;
            BuildOnce();
        }

        void BuildOnce()
        {
            built = true;
            LevelHelpers.Sky();
            LevelHelpers.TerrainMesh();
            LevelHelpers.StartHouse();

            // дорога: до огорода, через брод, после — до подъёма
            LevelHelpers.Slab("Road0", new Vector3(0f, 0.03f, -2f), new Vector3(3.4f, 0.1f, 24f), 0f, LevelHelpers.Dirt);
            LevelHelpers.Slab("Road1", new Vector3(0f, 0.03f, 42f), new Vector3(3.4f, 0.1f, 4f), 0f, LevelHelpers.Dirt);
            LevelHelpers.Slab("RoadFord", new Vector3(0f, -0.16f, 48f), new Vector3(3.4f, 0.1f, 8.4f), 0f, LevelHelpers.Dirt);
            LevelHelpers.Slab("Road2", new Vector3(0f, 0.03f, 58f), new Vector3(3.4f, 0.1f, 8f), 0f, LevelHelpers.Dirt);
            LevelHelpers.Slab("RoadSlopeUp", new Vector3(0f, 0.84f, 68f), new Vector3(3.4f, 0.1f, 12.3f), -8f, LevelHelpers.Dirt);
            LevelHelpers.Slab("Plateau", new Vector3(0f, 1.70f, 81f), new Vector3(10f, 0.1f, 14f), 0f, LevelHelpers.Dirt);

            Garden();
            Ford();
            Well();

            LevelHelpers.Cemetery(1.70f, 82f);

            LevelHelpers.Tree(-10f, -4f); LevelHelpers.Tree(9f, 8f); LevelHelpers.Tree(-9f, 30f);
            LevelHelpers.Tree(10f, 56f); LevelHelpers.Tree(-11f, 68f); LevelHelpers.Tree(11f, 84f);

            LevelHelpers.CoffinWithGrandpa(new Vector3(0f, 0.84f, -19f), Quaternion.identity);
            LevelHelpers.SpawnGoose(new Vector3(-2f, 0.6f, 24f));
            LevelHelpers.SpawnGoose(new Vector3(2.5f, 0.6f, 54f));
            Pedestrian.Spawn(new Vector3(0.8f, 1.1f, 4f), Vector3.forward, 8f);
            Pedestrian.Spawn(new Vector3(-0.9f, 1.1f, 58f), Vector3.forward, 7f);
            LevelHelpers.SpawnPlayer(new Vector3(1.5f, 1.2f, -15f));
            LevelHelpers.BuildCane(new Vector3(2.4f, 0.35f, -17.5f));
            LevelHelpers.WillPost(new Vector3(6f, 0.05f, 44f));
            Tolya.Spawn(new Vector3(4.6f, 0.1f, 36f), Vector3.forward);

            LevelHelpers.Checkpoint(new Vector3(0f, 1.2f, 36f), new Vector3(4f, 3f, 3f), "Огород");
            LevelHelpers.Checkpoint(new Vector3(0f, 1.2f, 56f), new Vector3(4f, 3f, 3f), "Брод");
            LevelHelpers.Checkpoint(new Vector3(0f, 2.8f, 76f), new Vector3(5f, 3f, 3f), "Кладбище");

            LevelHelpers.AttachCore(gameObject);
        }

        /// Две грядки поперёк дороги: справа проход 1.6 м — гроб пролезает впритирку,
        /// либо перетаскивай через борты. Пугало охраняет вторую.
        void Garden()
        {
            for (int i = 0; i < 2; i++)
            {
                float z = 18f + i * 10f;
                LevelHelpers.Slab($"BedTop{i}", new Vector3(-0.8f, 0.28f, z), new Vector3(4f, 0.32f, 4f), 0f,
                    new Color(0.3f, 0.22f, 0.14f));
                LevelHelpers.Slab($"BedSide{i}", new Vector3(-0.8f, 0.16f, z), new Vector3(4.2f, 0.1f, 4.2f), 0f,
                    LevelHelpers.Wood);
                // грядки-растения: маленькие зелёные кубики, чисто визуал
                for (int r = 0; r < 3; r++)
                    for (int c = 0; c < 4; c++)
                        LevelHelpers.Slab($"Crop{i}_{r}_{c}",
                            new Vector3(-2.3f + c * 1f, 0.5f, z - 1.3f + r * 1.3f),
                            new Vector3(0.4f, 0.35f, 0.4f), 0f, new Color(0.3f, 0.55f, 0.25f));
            }

            // пугало: палка, перекладина, голова, шляпа
            float sx = -2.6f, sz = 30f;
            LevelHelpers.Slab("ScarecrowPole", new Vector3(sx, 0.9f, sz), new Vector3(0.12f, 1.8f, 0.12f), 0f, LevelHelpers.Wood);
            LevelHelpers.Slab("ScarecrowArms", new Vector3(sx, 1.45f, sz), new Vector3(1.4f, 0.1f, 0.1f), 0f, LevelHelpers.Wood);
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "ScarecrowHead";
            head.transform.position = new Vector3(sx, 1.95f, sz);
            head.transform.localScale = Vector3.one * 0.34f;
            head.GetComponent<Renderer>().material.color = new Color(0.85f, 0.75f, 0.45f);
            var hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hat.name = "ScarecrowHat";
            hat.transform.position = new Vector3(sx, 2.18f, sz);
            hat.transform.localScale = new Vector3(0.7f, 0.18f, 0.7f);
            hat.GetComponent<Renderer>().material.color = new Color(0.45f, 0.3f, 0.15f);

            // покосившийся забор вокруг огорода
            for (int i = 0; i < 9; i++)
            {
                float z = 14f + i * 2f;
                Art.Model(i % 4 == 3 ? "town/fence-broken" : "town/fence",
                    new Vector3(-4.6f, LevelHelpers.Ground(-4.6f, z), z), 90f, 1f, 0.5f);
            }
            Art.Model("town/fence-gate", new Vector3(-4.6f, LevelHelpers.Ground(-4.6f, 34f), 34f), 90f, 1f, 0f);
        }

        /// Брод: дорога уходит под мелкую воду, по краям камни.
        void Ford()
        {
            LevelHelpers.Water(new Vector3(0f, -0.35f, 48f), new Vector3(12f, 0.5f, 8f), -0.12f);
            foreach (float x in new[] { -2.6f, 2.6f })
                foreach (float z in new[] { 44.5f, 48f, 51.5f })
                {
                    var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    rock.name = $"Rock_{x}_{z}";
                    rock.transform.position = new Vector3(x, -0.05f, z);
                    rock.transform.localScale = new Vector3(0.7f, 0.5f, 0.9f);
                    rock.GetComponent<Renderer>().material.color = LevelHelpers.Stone;
                }
        }

        void Well()
        {
            float wx = 6f, wz = 32f;
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "WellRing";
            ring.transform.position = new Vector3(wx, 0.4f, wz);
            ring.transform.localScale = new Vector3(1.5f, 0.4f, 1.5f);
            ring.GetComponent<Renderer>().material.color = LevelHelpers.Stone;
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "WellRoof";
            roof.transform.position = new Vector3(wx, 1.9f, wz);
            roof.transform.localScale = new Vector3(1.3f, 0.12f, 1.3f);
            roof.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            roof.GetComponent<Renderer>().material.color = LevelHelpers.Wood;
        }
    }
}
