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
            LevelHelpers.Slab("Plateau", new Vector3(0f, 1.70f, 81f), new Vector3(10f, 0.1f, 14f), 0f, LevelHelpers.Dirt);

            // пруд с мостком
            LevelHelpers.Water(new Vector3(0f, -0.3f, 42f), new Vector3(16f, 0.8f, 12f), 0.1f);
            for (int z = 34; z < 50; z += 2)
                LevelHelpers.Slab($"Plank{z}", new Vector3(0f, 0.36f, z + 1f), new Vector3(3.2f, 0.08f, 1.8f), 0f, LevelHelpers.Wood);

            LevelHelpers.Cemetery(1.70f, 82f);

            LevelHelpers.Tree(-9f, -6f); LevelHelpers.Tree(9f, 2f); LevelHelpers.Tree(-10f, 14f);
            LevelHelpers.Tree(10f, 26f); LevelHelpers.Tree(-9f, 44f); LevelHelpers.Tree(10f, 55f);
            LevelHelpers.Tree(-11f, 66f); LevelHelpers.Tree(11f, 86f);

            LevelHelpers.CoffinWithGrandpa(new Vector3(0f, 0.84f, -19f), Quaternion.identity);
            LevelHelpers.SpawnGoose(new Vector3(2.5f, 0.6f, 50f));
            LevelHelpers.SpawnGoose(new Vector3(-2f, 0.6f, 51.5f));
            LevelHelpers.SpawnPlayer(new Vector3(1.5f, 1.2f, -15f));
            LevelHelpers.BuildCane(new Vector3(2.4f, 0.35f, -17.5f));
            LevelHelpers.WillPost(new Vector3(5.5f, 0.05f, 46f));

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
