using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DI
{
    /// Главное меню: выбор заказа (со звёздами и разблокировкой), лавка снаряжения,
    /// сетевые кнопки. IMGUI-серый бокс; на арт-пасе заменяется на нормальный UI.
    /// Вёрстка адаптивная: верхний блок от верха, «подвал» от низа экрана.
    public class MenuBootstrap : MonoBehaviour
    {
        [SerializeField, HideInInspector] bool built;
        int tab; // 0 = смена, 1 = лавка
        string ip = "127.0.0.1";
        Texture2D bg, rowTex;
        readonly Dictionary<Color, Texture2D> texCache = new Dictionary<Color, Texture2D>();

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
            camGo.AddComponent<AudioListener>();
            var lightGo = new GameObject("Sun");
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1f;
        }

        Texture2D Solid(Color c)
        {
            if (texCache.TryGetValue(c, out var t)) return t;
            t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            texCache[c] = t;
            return t;
        }

        void OnGUI()
        {
            if (bg == null)
            {
                bg = Solid(new Color(0.07f, 0.07f, 0.09f));
                rowTex = Solid(new Color(0.12f, 0.12f, 0.15f, 0.95f));
            }
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), bg);

            float H = Screen.height;
            float W = Screen.width;
            bool compact = H < 640f;            // маленькое окно Game-вью
            float w = Mathf.Min(760f, W - 40f); // ширина колонки не шире экрана
            float x = (W - w) / 2f;
            float rowH = compact ? 48f : 58f;
            float titleSize = compact ? 44f : 60f;

            var big = new GUIStyle(GUI.skin.label)
            {
                fontSize = (int)titleSize,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            var sub = new GUIStyle(GUI.skin.label)
            {
                fontSize = compact ? 14 : 17,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 1f, 1f, 0.75f) }
            };
            var cash = new GUIStyle(sub) { fontSize = compact ? 14 : 16, normal = { textColor = new Color(0.65f, 0.9f, 0.55f) } };

            // --- верхний блок ---
            float y = 8f;
            GUI.Label(new Rect(0, y, W, titleSize + 14f), "ДЕД ВНУТРИ", big);
            y += titleSize + 16f;
            GUI.Label(new Rect(0, y, W, 24f), $"В кассе бюро: {SessionState.Money} ₽", cash);
            y += 28f;

            float tw = w / 2f;
            if (GUI.Button(new Rect(x, y, tw, 34f), tab == 0 ? "[ СМЕНА ]" : "СМЕНА")) tab = 0;
            if (GUI.Button(new Rect(x + tw, y, tw, 34f), tab == 1 ? "[ ЛАВКА ]" : "ЛАВКА")) tab = 1;
            y += 44f;

            if (tab == 0) DrawOrders(x, y, w, rowH);
            else DrawShop(x, y, w, rowH);

            // --- подвал (от низа экрана) ---
            var footerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = compact ? 12 : 14,
                normal = { textColor = Color.white }
            };

            y = H - (compact ? 84f : 96f);
            GUI.Label(new Rect(x, y + 4f, 220f, 22f), $"Мышь: {SessionState.Sens:0.0}", footerStyle);
            SessionState.Sens = GUI.HorizontalSlider(new Rect(x + 225f, y + 8f, 130f, 22f), SessionState.Sens, 1f, 6f);

            GUI.Label(new Rect(x + 370f, y + 4f, 40f, 22f), "IP:", footerStyle);
            ip = GUI.TextField(new Rect(x + 405f, y + 2f, w - 405f, 24f), ip, 64);

            float bw = (w - 20f) / 3f;
            y = H - (compact ? 46f : 56f);
            if (GUI.Button(new Rect(x, y, bw, 32f), "ХОСТИТЬ СМЕНУ")) Go(SessionMode.Host);
            if (GUI.Button(new Rect(x + bw + 10f, y, bw, 32f), "ПОДКЛЮЧИТЬСЯ К ДРУГУ")) Go(SessionMode.Client);
            if (GUI.Button(new Rect(x + 2f * (bw + 10f), y, bw, 32f), "ВЫХОД")) Application.Quit();
        }

        void DrawOrders(float x, float y, float w, float rowH)
        {
            DrawOrder(x, y, w, rowH, "Village1", "Пригорок", "пруд с мостком и гусями");
            DrawOrder(x, y + rowH + 6f, w, rowH, "Village2", "Огород", "грядки, брод через ручей, пугало");
            DrawOrder(x, y + 2f * (rowH + 6f), w, rowH, "Village3", "Сеновал", "подъём, мост через овраг, корова");
        }

        void DrawOrder(float x, float y, float w, float h, string scene, string title, string desc)
        {
            bool unlocked = SessionState.IsUnlocked(scene);
            int stars = SessionState.Stars(scene);
            var starText = new string('★', stars) + new string('☆', 3 - stars);

            GUI.DrawTexture(new Rect(x, y, w, h), rowTex);
            GUI.Label(new Rect(x + 12f, y + 4f, 240f, 24f), $"Деревня · {title}",
                new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } });
            GUI.Label(new Rect(x + 12f, y + h - 22f, 330f, 18f), desc,
                new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(1, 1, 1, 0.7f) } });
            GUI.Label(new Rect(x + 350f, y + h / 2f - 15f, 110f, 30f), starText,
                new GUIStyle(GUI.skin.label) { fontSize = 20, normal = { textColor = new Color(1f, 0.85f, 0.3f) } });

            if (unlocked)
            {
                if (GUI.Button(new Rect(x + w - 130f, y + h / 2f - 16f, 115f, 32f), "ИГРАТЬ"))
                    SceneManager.LoadScene(scene);
            }
            else
            {
                GUI.Label(new Rect(x + w - 250f, y + h / 2f - 10f, 240f, 20f), "нужна ★ на предыдущем",
                    new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(1, 1, 1, 0.5f) } });
            }
        }

        void DrawShop(float x, float y, float w, float rowH)
        {
            for (int i = 0; i < Gear.Length; i++)
            {
                var g = Gear[i];
                float ry = y + i * (rowH + 6f);
                GUI.DrawTexture(new Rect(x, ry, w, rowH), rowTex);
                GUI.Label(new Rect(x + 12f, ry + 4f, 300f, 24f), g.name,
                    new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } });
                GUI.Label(new Rect(x + 12f, ry + rowH - 22f, 460f, 18f), g.desc,
                    new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(1, 1, 1, 0.7f) } });

                bool owned = SessionState.HasGear(g.id);
                if (owned)
                {
                    GUI.Label(new Rect(x + w - 140f, ry + rowH / 2f - 12f, 130f, 28f), "куплено",
                        new GUIStyle(GUI.skin.label) { fontSize = 15, normal = { textColor = new Color(0.65f, 0.9f, 0.55f) } });
                }
                else if (GUI.Button(new Rect(x + w - 130f, ry + rowH / 2f - 16f, 115f, 32f), $"{g.price} ₽"))
                {
                    SessionState.BuyGear(g.id, g.price);
                }
            }
        }

        void Go(SessionMode mode)
        {
            SessionState.Mode = mode;
            SessionState.Ip = ip;
            SceneManager.LoadScene("Village1");
        }
    }
}
