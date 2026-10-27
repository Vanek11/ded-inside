#if ADMIN
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DI
{
    /// Тестовая админка: существует ТОЛЬКО в dev-сборке (define ADMIN).
    /// F1 — открыть/закрыть. Уровни, телепорты, деньги, разблокировки.
    public class AdminPanel : MonoBehaviour
    {
        bool open;
        Texture2D tex;

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.F1)) return;
            open = !open;
            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = open;
        }

        Texture2D Solid(Color c)
        {
            if (tex != null) return tex;
            tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, c);
            tex.Apply();
            return tex;
        }

        void OnGUI()
        {
            if (!open || !Application.isPlaying) return;
            var r = new Rect(10f, 10f, 280f, 560f);
            GUI.DrawTexture(r, Solid(new Color(0.05f, 0.05f, 0.2f, 0.95f)));

            float x = r.x + 12f, y = r.y + 10f, w = r.width - 24f;
            var title = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            var small = new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(1, 1, 1, 0.7f) } };

            GUI.Label(new Rect(x, y, w, 24f), "АДМИНКА (dev-сборка)", title);
            y += 28f;

            GUI.Label(new Rect(x, y, w, 18f), "— Уровни —", small);
            y += 20f;
            foreach (var scene in new[] { "Menu", "Village1", "Village2", "Village3" })
            {
                if (GUI.Button(new Rect(x, y, w, 26f), scene)) SceneManager.LoadScene(scene);
                y += 30f;
            }

            GUI.Label(new Rect(x, y, w, 18f), "— Игрок → чекпоинт —", small);
            y += 20f;
            int i = 1;
            foreach (var cp in FindObjectsByType<CheckpointZone>(FindObjectsSortMode.None))
            {
                var pos = cp.transform.position;
                if (GUI.Button(new Rect(x, y, w, 24f), $"Игрок → {cp.checkpointId}"))
                    TeleportPlayer(pos + Vector3.up * 1.5f);
                y += 27f;
                i++;
            }
            if (GUI.Button(new Rect(x, y, w, 24f), "Игрок → старт дома"))
            {
                TeleportPlayer(new Vector3(1.5f, 1.2f, -15f));
                y += 27f;
            }
            y += 30f;

            if (GUI.Button(new Rect(x, y, w, 26f), "Гроб с дедом → к игроку"))
            {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null) TeleportCoffin(player.transform.position + Vector3.forward * 2f);
                y += 30f;
            }
            y += 4f;

            GUI.Label(new Rect(x, y, w, 18f), "— Читы —", small);
            y += 20f;
            if (GUI.Button(new Rect(x, y, w, 26f), "+500 ₽")) SessionState.AddMoney(500);
            y += 30f;
            if (GUI.Button(new Rect(x, y, w, 26f), "Разблокировать все заказы"))
            {
                SessionState.SetStars("Village1", 3);
                SessionState.SetStars("Village2", 3);
                SessionState.SetStars("Village3", 3);
            }
            y += 30f;
            if (GUI.Button(new Rect(x, y, w, 26f), "+50 стиля")) DIGameManager.I?.AddStyle(50f);
            y += 30f;
            if (GUI.Button(new Rect(x, y, w, 26f), "Медленное время вкл/выкл"))
                Time.timeScale = Time.timeScale > 0.5f ? 0.25f : 1f;
            y += 34f;

            GUI.Label(new Rect(x, y, w, 18f), "F1 — закрыть панель", small);
        }

        static void TeleportPlayer(Vector3 pos)
        {
            var player = FindFirstObjectByType<PlayerController>();
            if (player == null) return;
            var rb = player.GetComponent<Rigidbody>();
            rb.position = pos;
            rb.linearVelocity = Vector3.zero;
        }

        static void TeleportCoffin(Vector3 pos)
        {
            var game = DIGameManager.I;
            if (game == null || game.coffin == null) return;
            var coffin = game.coffin;

            if (game.grandpa != null)
            {
                int i = 0;
                foreach (var part in game.grandpa.GetComponentsInChildren<GrandpaPart>())
                {
                    part.GetComponent<Rigidbody>().position = pos + new Vector3(0f, 0.5f, i * 0.3f);
                    part.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                    i++;
                }
            }

            coffin.GetComponent<Rigidbody>().position = pos;
            coffin.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            if (coffin.Lid != null)
            {
                coffin.Lid.position = pos + Vector3.up * 0.6f;
                coffin.Lid.linearVelocity = Vector3.zero;
            }
        }
    }
}
#endif
