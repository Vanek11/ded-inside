using UnityEngine;

namespace DI
{
    /// «Деревня-1 · Пригорок»: дом → дорога → пригорок (санки) → пруд с мостком
    /// и гусями → подъём → кладбище. Всё строится кодом из примитивов.
    public class LevelVillage1 : MonoBehaviour
    {
        public static LevelVillage1 I { get; private set; }

        [SerializeField, HideInInspector] bool built;

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
            LevelHelpers.SetCarves((new Vector3(0f, 0f, 42f), 3.2f, 5.5f, 0.9f)); // котлован пруда
            LevelHelpers.Sky();
            LevelHelpers.TerrainMesh();
            LevelHelpers.StartHouse();

            LevelHelpers.Slab("Road0", new Vector3(0f, 0.03f, -4f), new Vector3(3.4f, 0.1f, 20f), 0f, LevelHelpers.Dirt);
            LevelHelpers.Slab("RoadSlopeUp", new Vector3(0f, 0.525f, 11f), new Vector3(3.4f, 0.1f, 10.2f), -6f, LevelHelpers.Dirt);
            LevelHelpers.Slab("RoadHillTop", new Vector3(0f, 1.06f, 20f), new Vector3(3.4f, 0.1f, 8.4f), 0f, LevelHelpers.Dirt);
            LevelHelpers.Slab("RoadSlopeDown", new Vector3(0f, 0.52f, 29f), new Vector3(3.4f, 0.1f, 10.2f), 6f, LevelHelpers.Dirt);
            LevelHelpers.Slab("Road1", new Vector3(0f, 0.03f, 35f), new Vector3(3.4f, 0.1f, 2.4f), 0f, LevelHelpers.Dirt);
            LevelHelpers.Slab("Road2", new Vector3(0f, 0.03f, 56f), new Vector3(3.4f, 0.1f, 12f), 0f, LevelHelpers.Dirt);
            LevelHelpers.Slab("RoadSlopeUp2", new Vector3(0f, 0.84f, 68f), new Vector3(3.4f, 0.1f, 12.3f), -8f, LevelHelpers.Dirt);
            LevelHelpers.SlabEarth("Plateau", new Vector3(0f, 1.75f, 81f), new Vector2(15f, 14f), LevelHelpers.Dirt);

            // пруд в котловане: вода точно по урезу, мосток поперёк без зазоров
            LevelHelpers.WaterEllipse(new Vector2(0f, 42f), 3.0f, 5.2f, 0.02f);
            // мост одним куском, растянутым через весь пруд
            var pondBridge = Art.Model("nature/bridge_woodNarrow", new Vector3(0f, 0.05f, 42f), 90f, 1f, 0.25f);
            if (pondBridge != null)
            {
                pondBridge.transform.localScale = new Vector3(5.3f, 1f, 1f);
                var b = new Bounds(pondBridge.transform.position, Vector3.zero);
                foreach (var r in pondBridge.GetComponentsInChildren<MeshRenderer>()) b.Encapsulate(r.bounds);
                pondBridge.transform.position += Vector3.up * (0.05f - b.min.y);
            }

            LevelHelpers.Cemetery(1.70f, 82f);

            LevelHelpers.Tree(-9f, -6f); LevelHelpers.Tree(9f, 2f); LevelHelpers.Tree(-10f, 14f);
            LevelHelpers.Tree(10f, 26f); LevelHelpers.Tree(-9f, 44f); LevelHelpers.Tree(10f, 55f);
            LevelHelpers.Tree(-11f, 66f); LevelHelpers.Tree(11f, 86f);

            LevelHelpers.CoffinWithGrandpa(new Vector3(0f, 0.84f, -19f), Quaternion.identity);
            LevelHelpers.SpawnGoose(new Vector3(2.5f, 0.6f, 50f));
            LevelHelpers.SpawnGoose(new Vector3(-2f, 0.6f, 51.5f));
            Pedestrian.Spawn(new Vector3(0.8f, 1.1f, -4f), Vector3.forward, 8f);
            Pedestrian.Spawn(new Vector3(-0.9f, 1.1f, 56f), Vector3.forward, 9f);
            LevelHelpers.SpawnPlayer(new Vector3(1.5f, 1.2f, -15f));
            LevelHelpers.BuildCane(new Vector3(2.4f, 0.35f, -17.5f));
            LevelHelpers.WillPost(new Vector3(5.5f, 0.05f, 46f));
            Tolya.Spawn(new Vector3(4.5f, 0.1f, 6f), Vector3.forward);

            LevelHelpers.Checkpoint(new Vector3(0f, 1.6f, 26f), new Vector3(4f, 3f, 3f), "Пригорок");
            LevelHelpers.Checkpoint(new Vector3(0f, 1.2f, 51.5f), new Vector3(4f, 2.5f, 3f), "Мосток");
            LevelHelpers.Checkpoint(new Vector3(0f, 2.8f, 76f), new Vector3(5f, 3f, 3f), "Кладбище");

            LevelHelpers.AttachCore(gameObject);
        }

        public void OnCheckpoint(CheckpointZone cp) =>
            DIGameManager.I?.Event($"Чекпоинт «{cp.checkpointId}» пройден!");

        // точка входа для сетевой пересборки гусей
        public static GameObject SpawnGoose(Vector3 pos) => LevelHelpers.SpawnGoose(pos);
    }
}
