using UnityEngine;
using UnityEngine.SceneManagement;

namespace DI
{
    /// Главное меню: выбор заказа (со звёздами и разблокировкой), лавка снаряжения,
    /// сетевые кнопки. IMGUI-серый бокс; на арт-пасе заменяется на нормальный UI.
    public class MenuBootstrap : MonoBehaviour
    {
        [SerializeField, HideInInspector] bool built;
        int tab; // 0 = смена, 1 = лавка
        string ip = "127.0.0.1";
        Texture2D bg;

        struct GearItem
        {
            public string id, name, desc;
            public int price;
            public GearItem(string id, string name, string desc, int price)
            {
                this.id = id; this.name = name; this.desc = desc; this.price = price;
            }
        }

        static readonly GearItem[] Gear =
        {
            new GearItem("boots", "Скороходы", "Ботинки с гвоздями: ходьба +15%", 200),
            new GearItem("padding", "Мягкая обивка", "Урон деду от ударов −50%", 300),
            new GearItem("rems", "Похоронные ремни", "Крышка держится вдвое крепче", 400),
            new GearItem("tape", "Скотч-пистолет", "Крышку почти невозможно сорвать", 500),
        };

        void Awake()
        {
            if (built) return;
            BuildOnce();
        }

        /// В edit mode Awake не вызывается — SceneSetup строит через этот метод.
        public void EditorBuild()
        {
            if (built) return;
            BuildOnce();
        }

        void BuildOnce()
        {
            built = true;
            var camGo = new GameObject("MainCamera");
            camGo.tag = "MainCamera";
            camGo.AddComponent<Camera>();
            var lightGo = new GameObject("Sun");
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1f;
        }

        Texture2D Solid(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        void OnGUI()
        {
            if (bg == null) bg = Solid(new Color(0.07f, 0.07f, 0.09f));
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), bg);

            var big = new GUIStyle(GUI.skin.label)
            {
                fontSize = 64,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            var sub = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 1f, 1f, 0.75f) }
            };
            var cash = new GUIStyle(sub) { fontSize = 16, normal = { textColor = new Color(0.65f, 0.9f, 0.55f) } };

            float top = Screen.height * 0.06f;
            GUI.Label(new Rect(0, top, Screen.width, 84f), "ДЕД ВНУТРИ", big);
            GUI.Label(new Rect(0, top + 86f, Screen.width, 26f), $"В кассе бюро: {SessionState.Money} ₽", cash);

            // вкладки
            float w = 760f, x = (Screen.width - w) / 2f;
            float y = top + 130f;
            float tw = w / 2f;
            if (GUI.Button(new Rect(x, y, tw, 40f), tab == 0 ? "[ СМЕНА ]" : "СМЕНА")) tab = 0;
            if (GUI.Button(new Rect(x + tw, y, tw, 40f), tab == 1 ? "[ ЛАВКА ]" : "ЛАВКА")) tab = 1;
            y += 52f;

            if (tab == 0) DrawOrders(x, y, w);
            else DrawShop(x, y, w);

            // сетевые кнопки и выход — внизу
            y = Screen.height - 216f;
            GUI.Label(new Rect(x, y, 330f, 24f), $"Чувствительность мыши: {SessionState.Sens:0.0}", sub);
            SessionState.Sens = GUI.HorizontalSlider(new Rect(x + 340f, y + 5f, 240f, 22f), SessionState.Sens, 1f, 6f);

            y = Screen.height - 170f;
            GUI.Label(new Rect(x, y, 50f, 30f), "IP:");
            ip = GUI.TextField(new Rect(x + 50f, y, 220f, 30f), ip, 64);
            if (GUI.Button(new Rect(x + 285f, y, 220f, 34f), "ХОСТИТЬ СМЕНУ")) Go(SessionMode.Host, null);
            if (GUI.Button(new Rect(x + 515f, y, 220f, 34f), "ПОДКЛЮЧИТЬСЯ К ДРУГУ")) Go(SessionMode.Client, null);
            if (GUI.Button(new Rect(x, y + 44f, 220f, 34f), "ВЫХОД")) Application.Quit();
        }

        void DrawOrders(float x, float y, float w)
        {
            DrawOrder(x, y, w, "Village1", "Пригорок", "пруд с мостком и гусями");
            DrawOrder(x, y + 64f, w, "Village2", "Огород", "грядки, брод через ручей, пугало");
            DrawOrder(x, y + 128f, w, "Village3", "Сеновал", "крутой подъём, мост через овраг, корова");
        }

        void DrawOrder(float x, float y, float w, string scene, string title, string desc)
        {
            bool unlocked = SessionState.IsUnlocked(scene);
            var stars = new string('★', SessionState.Stars(scene)) + new string('☆', 3 - SessionState.Stars(scene));

            GUI.DrawTexture(new Rect(x, y, w, 56f), Solid(new Color(0.12f, 0.12f, 0.15f, 0.95f)));
            GUI.Label(new Rect(x + 14f, y + 6f, 220f, 24f), $"Деревня · {title}",
                new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } });
            GUI.Label(new Rect(x + 14f, y + 30f, 340f, 20f), desc,
                new GUIStyle(GUI.skin.label) { fontSize = 13, normal = { textColor = new Color(1, 1, 1, 0.7f) } });
            GUI.Label(new Rect(x + 380f, y + 14f, 120f, 30f), stars,
                new GUIStyle(GUI.skin.label) { fontSize = 22, normal = { textColor = new Color(1f, 0.85f, 0.3f) } });

            if (unlocked)
            {
                if (GUI.Button(new Rect(x + w - 150f, y + 11f, 130f, 34f), "ИГРАТЬ"))
                    SceneManager.LoadScene(scene);
            }
            else
            {
                GUI.Label(new Rect(x + w - 220f, y + 17f, 210f, 24f), "закрыто — нужна звезда на предыдущем",
                    new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(1, 1, 1, 0.5f) } });
            }
        }

        void DrawShop(float x, float y, float w)
        {
            for (int i = 0; i < Gear.Length; i++)
            {
                var g = Gear[i];
                float ry = y + i * 64f;
                GUI.DrawTexture(new Rect(x, ry, w, 56f), Solid(new Color(0.12f, 0.12f, 0.15f, 0.95f)));
                GUI.Label(new Rect(x + 14f, ry + 6f, 300f, 24f), g.name,
                    new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } });
                GUI.Label(new Rect(x + 14f, ry + 30f, 460f, 20f), g.desc,
                    new GUIStyle(GUI.skin.label) { fontSize = 13, normal = { textColor = new Color(1, 1, 1, 0.7f) } });

                bool owned = SessionState.HasGear(g.id);
                if (owned)
                {
                    GUI.Label(new Rect(x + w - 150f, ry + 15f, 140f, 30f), "куплено",
                        new GUIStyle(GUI.skin.label) { fontSize = 16, normal = { textColor = new Color(0.65f, 0.9f, 0.55f) } });
                }
                else if (GUI.Button(new Rect(x + w - 150f, ry + 11f, 130f, 34f), $"{g.price} ₽"))
                {
                    SessionState.BuyGear(g.id, g.price);
                }
            }
        }

        void Go(SessionMode mode, string scene)
        {
            SessionState.Mode = mode;
            SessionState.Ip = ip;
            SceneManager.LoadScene(scene ?? "Village1");
        }
    }
}
