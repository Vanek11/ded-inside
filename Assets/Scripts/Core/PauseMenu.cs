using UnityEngine;
using UnityEngine.SceneManagement;

namespace DI
{
    /// Пауза по Esc: продолжить, в меню, выход. Останавливает физику временем.
    public class PauseMenu : MonoBehaviour
    {
        public static bool Open { get; private set; }

        /// Сброс паузы (после перезагрузки сцены из паузы).
        public static void ForceClosed()
        {
            Open = false;
            Time.timeScale = 1f;
        }

        Texture2D dim, panel;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Toggle();
        }

        void Toggle()
        {
            Open = !Open;
            Time.timeScale = Open ? 0f : 1f;
            Cursor.lockState = Open ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = Open;
        }

        void OnGUI()
        {
            if (!Open) return;
            if (dim == null)
            {
                dim = Solid(new Color(0f, 0f, 0f, 0.6f));
                panel = Solid(new Color(0.1f, 0.1f, 0.13f, 0.97f));
            }

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dim);
            float w = 360f, h = 240f;
            var r = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.DrawTexture(r, panel);

            GUI.Label(new Rect(r.x, r.y + 16f, w, 40f), "ПАУЗА",
                new GUIStyle(GUI.skin.label) { fontSize = 28, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } });

            if (GUI.Button(new Rect(r.x + 40f, r.y + 70f, w - 80f, 44f), "ПРОДОЛЖИТЬ")) Toggle();
            if (GUI.Button(new Rect(r.x + 40f, r.y + 124f, w - 80f, 44f), "В ГЛАВНОЕ МЕНЮ"))
            {
                Time.timeScale = 1f;
                Open = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                var nm = Unity.Netcode.NetworkManager.Singleton;
                if (nm != null) nm.Shutdown();
                SceneManager.LoadScene("Menu");
            }
            if (GUI.Button(new Rect(r.x + 40f, r.y + 178f, w - 80f, 44f), "ВЫХОД")) Application.Quit();
        }

        Texture2D Solid(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }
    }
}
