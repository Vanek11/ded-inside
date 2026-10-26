using System.Collections.Generic;
using UnityEngine;

namespace DI
{
    /// Серобоксный HUD: время заказа, дед-метр, крен гроба, лента событий, подсказки.
    /// На арт-пасе заменяется на нормальный UI (TextMeshPro).
    public class HUD : MonoBehaviour
    {
        DIGameManager game;
        CoffinBody coffin;
        GrandpaState grandpa;
        readonly List<string> feed = new List<string>();
        float lastFeedCount = -1f;

        void Update()
        {
            game = DIGameManager.I;
            if (game != null)
            {
                coffin = game.coffin;
                grandpa = game.grandpa;
                if (game.Feed.Count != (int)lastFeedCount)
                {
                    lastFeedCount = game.Feed.Count;
                    feed.Clear();
                    feed.AddRange(game.Feed);
                }
            }
        }

        void OnGUI()
        {
            var s = new GUIStyle(GUI.skin.label) { fontSize = 20 };
            var small = new GUIStyle(GUI.skin.label) { fontSize = 14 };

            GUILayout.BeginArea(new Rect(12f, 12f, 460f, Screen.height - 24f));

            if (game == null)
            {
                GUILayout.Label("Менеджер не найден", s);
                GUILayout.EndArea();
                return;
            }

            if (game.Completed)
            {
                GUILayout.Label($"ЗАКАЗ ВЫПОЛНЕН — {game.OrderTime:0.0} сек", s);
                GUILayout.Label($"Состояние деда: {grandpa?.Condition ?? 0:0}/100   Стиль: {game.StylePoints:0}   Спасений: {game.Catches}", s);
                GUILayout.Label("R — начать смену заново", s);
            }
            else
            {
                GUILayout.Label($"Время заказа: {game.OrderTime:0.0} сек", s);
                if (grandpa != null)
                    GUILayout.Label($"Дед-метр: {grandpa.Condition:0}/100", s);
                if (coffin != null)
                    GUILayout.Label($"Крен гроба: {coffin.TiltDeg:0}°  Крышка: {(coffin.LidClosed ? "на месте" : "СОРВАНА")}", s);
            }

            GUILayout.Space(8f);
            GUILayout.Label("Лента смены:", small);
            foreach (var line in feed)
                GUILayout.Label(line, small);

            GUILayout.FlexibleSpace();
            GUILayout.Label("ЛКМ — схватить/держать   WASD — идти   Space — прыжок", small);
            GUILayout.Label("Esc — показать курсор", small);
            GUILayout.EndArea();
        }
    }
}
