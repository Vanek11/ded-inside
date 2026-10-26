using UnityEngine;
using UnityEngine.SceneManagement;

namespace DI
{
    /// Главное меню (IMGUI-серый бокс; на арт-пасе заменяется на нормальный UI).
    public class MenuBootstrap : MonoBehaviour
    {
        [SerializeField, HideInInspector] bool built;
        string ip = "127.0.0.1";
        Texture2D bg;

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
                fontSize = 84,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            var sub = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 1f, 1f, 0.75f) }
            };
            var cash = new GUIStyle(sub) { fontSize = 16, normal = { textColor = new Color(0.65f, 0.9f, 0.55f) } };

            float top = Screen.height * 0.10f;
            GUI.Label(new Rect(0, top, Screen.width, 110f), "ДЕД ВНУТРИ", big);
            GUI.Label(new Rect(0, top + 108f, Screen.width, 30f), "Кооп-комедия о самой бестолковой похоронной службе", sub);
            GUI.Label(new Rect(0, top + 140f, Screen.width, 26f), $"В кассе бюро: {SessionState.Money} ₽", cash);

            float w = 420f, h = 52f, x = (Screen.width - w) / 2f, y = Screen.height * 0.42f;
            if (GUI.Button(new Rect(x, y, w, h), "НАЧАТЬ СМЕНУ (СОЛО)")) Go(SessionMode.Solo);
            y += h + 10f;
            if (GUI.Button(new Rect(x, y, w, h), "ХОСТИТЬ СМЕНУ (ДРУГ ПОДКЛЮЧИТСЯ)")) Go(SessionMode.Host);
            y += h + 10f;
            GUI.Label(new Rect(x, y + 6f, 50f, 30f), "IP:");
            ip = GUI.TextField(new Rect(x + 50f, y, w - 50f, h - 14f), ip, 64);
            y += h + 10f;
            if (GUI.Button(new Rect(x, y, w, h), "ПОДКЛЮЧИТЬСЯ К ДРУГУ")) Go(SessionMode.Client);
            y += h + 24f;
            if (GUI.Button(new Rect(x, y, w, 40f), "ВЫХОД")) Application.Quit();
        }

        void Go(SessionMode mode)
        {
            SessionState.Mode = mode;
            SessionState.Ip = ip;
            SceneManager.LoadScene("Village1");
        }
    }
}
