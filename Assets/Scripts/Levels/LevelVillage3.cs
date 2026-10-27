using UnityEngine;

namespace DI
{
    /// «Деревня-3 · Сеновал»: дом → КРУТЫЙ подъём → сеновал со стогами →
    /// узкий мост через овраг (внизу ручей — упал, так упал) → КОРОВА на дороге
    /// → длинный спуск-санки → кладбище.
    public class LevelVillage3 : MonoBehaviour
    {
        public static LevelVillage3 I { get; private set; }

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
            LevelHelpers.SetCarves((new Vector3(0f, 0f, 32f), 4.5f, 6f, 1.0f, -0.7f)); // овраг с ручьём
            LevelHelpers.Sky();
            LevelHelpers.TerrainMesh();
            LevelHelpers.StartHouse();

            LevelHelpers.Slab("Road0", new Vector3(0f, 0.03f, -7f), new Vector3(3.4f, 0.1f, 14f), 0f, LevelHelpers.Dirt);
            LevelHelpers.Slab("RoadClimb", new Vector3(0f, 1.76f, 7f), new Vector3(3.4f, 0.1f, 14.6f), -14f, LevelHelpers.Dirt);
            LevelHelpers.Slab("Top", new Vector3(0f, 3.53f, 20f), new Vector3(3.4f, 0.1f, 12.4f), 0f, LevelHelpers.Dirt);

            Haystacks();
            Bridge();
            Cow();
            Descent();

            LevelHelpers.Cemetery(0.05f, 74f);

            LevelHelpers.Tree(-10f, -6f); LevelHelpers.Tree(9f, 2f); LevelHelpers.Tree(-9f, 44f);
            LevelHelpers.Tree(10f, 62f); LevelHelpers.Tree(-11f, 70f); LevelHelpers.Tree(11f, 80f);

            LevelHelpers.CoffinWithGrandpa(new Vector3(0f, 0.84f, -19f), Quaternion.identity);
            Pedestrian.Spawn(new Vector3(0.8f, 1.1f, -7f), Vector3.forward, 5f);
            Pedestrian.Spawn(new Vector3(0.9f, 4.6f, 20f), Vector3.forward, 5f);
            LevelHelpers.SpawnPlayer(new Vector3(1.5f, 1.2f, -15f));
            LevelHelpers.BuildCane(new Vector3(2.4f, 0.35f, -17.5f));
            LevelHelpers.WillPost(new Vector3(6f, 0.05f, 24f));
            Tolya.Spawn(new Vector3(4.2f, 0.1f, 2f), Vector3.forward);

            LevelHelpers.Checkpoint(new Vector3(0f, 4.1f, 20f), new Vector3(4f, 3f, 3f), "Сеновал");
            LevelHelpers.Checkpoint(new Vector3(0f, 4.1f, 40f), new Vector3(4f, 3f, 3f), "Мост");
            LevelHelpers.Checkpoint(new Vector3(0f, 4.1f, 48f), new Vector3(4f, 3f, 3f), "Корова");
            LevelHelpers.Checkpoint(new Vector3(0f, 0.7f, 68f), new Vector3(4f, 3f, 3f), "Кладбище");

            LevelHelpers.AttachCore(gameObject);
        }

        void Haystacks()
        {
            foreach (var p in new[] { new Vector3(-5f, 0f, 16f), new Vector3(5f, 0f, 18f),
                                       new Vector3(-5.5f, 0f, 24f), new Vector3(5.5f, 0f, 22f) })
            {
                var stack = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stack.name = "Haystack";
                stack.transform.position = new Vector3(p.x, LevelHelpers.Ground(p.x, p.z) + 1.4f, p.z);
                stack.transform.localScale = new Vector3(4.4f, 1.4f, 4.4f);
                var m = stack.GetComponent<Renderer>().material;
                m.color = new Color(0.85f, 0.7f, 0.3f);
                m.SetFloat("_Glossiness", 0.05f);
            }
        }

        /// Мост через овраг (модели моста), внизу ручей в вырезе рельефа.
        void Bridge()
        {
            for (float z = 26.2f; z < 38.2f; z += 1.95f)
                Art.Model("nature/bridge_wood", new Vector3(0f, 3.58f, z), 90f, 1f, 0.25f);

            LevelHelpers.WaterEllipse(new Vector2(0f, 32f), 3.7f, 4.9f, -0.7f);

            // пандус из ручья наверх, к выходу с моста
            LevelHelpers.Slab("RavineRamp", new Vector3(5.5f, 1.6f, 33f), new Vector3(2.4f, 0.1f, 10.4f), -20f, LevelHelpers.Wood);
        }

        /// Корова легла на дороге: физическая, толкается, обойти нельзя.
        void Cow()
        {
            var cow = new GameObject("Cow");
            cow.layer = LayerMask.NameToLayer("Grabbable");
            cow.transform.position = new Vector3(0f, 4.1f, 44f);
            cow.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            cow.AddComponent<NetDynamicMarker>();

            var rb = cow.AddComponent<Rigidbody>();
            rb.mass = 250f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "CowBody";
            body.transform.SetParent(cow.transform, false);
            body.transform.localScale = new Vector3(1.4f, 1.1f, 2.8f);
            body.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            body.GetComponent<Renderer>().material.color = new Color(0.92f, 0.9f, 0.86f);

            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "CowHead";
            head.transform.SetParent(cow.transform, false);
            head.transform.localScale = new Vector3(0.8f, 0.8f, 0.9f);
            head.transform.localPosition = new Vector3(0f, 0.75f, 1.75f);
            head.GetComponent<Renderer>().material.color = new Color(0.92f, 0.9f, 0.86f);

            var spot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spot.name = "CowSpot";
            spot.transform.SetParent(cow.transform, false);
            spot.transform.localScale = new Vector3(0.7f, 0.02f, 1.2f);
            spot.transform.localPosition = new Vector3(-0.36f, 1.1f, -0.3f);
            spot.GetComponent<Renderer>().material.color = new Color(0.25f, 0.2f, 0.18f);
            Object.DestroyImmediate(spot.GetComponent<Collider>());

            var scare = cow.AddComponent<JumpScare>();
            scare.detectRadius = 5f;
            scare.jumpImpulse = 0.4f;
            scare.pushImpulse = 2.2f;
            scare.cooldown = 4f;
            scare.gag = "МУУУ! Корова не двигается с дороги.";
            scare.soundId = "moo";
        }

        /// Длинный спуск-санки с вершины вниз к кладбищу.
        void Descent()
        {
            LevelHelpers.Slab("RoadDescent", new Vector3(0f, 1.8f, 58f), new Vector3(3.4f, 0.1f, 16.7f), 12f, LevelHelpers.Dirt);
            LevelHelpers.Slab("RoadFinal", new Vector3(0f, 0.03f, 66.5f), new Vector3(3.4f, 0.1f, 3f), 0f, LevelHelpers.Dirt);
        }
    }
}
