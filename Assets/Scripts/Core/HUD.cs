using System.Collections.Generic;
using UnityEngine;

namespace DI
{
    /// HUD серого бокса v2: панели статуса и цели, лента смены, церемония,
    /// финальный экран оценки с деньгами, звёздами и «Хроникой смены».
    /// На арт-пасе заменяется на нормальный UI (TextMeshPro).
    public class HUD : MonoBehaviour
    {
        DIGameManager game;
        CoffinBody coffin;
        GrandpaState grandpa;
        readonly List<string> feed = new List<string>();
        long feedVersion = -1;

        Texture2D panelTex, darkTex, barBgTex;
        GUIStyle title, line, small, center, big;

        void Update()
        {
            game = DIGameManager.I;
            if (game == null) return;
            coffin = game.coffin;
            grandpa = game.grandpa;
            if (game.FeedVersion != feedVersion)
            {
                feedVersion = game.FeedVersion;
                feed.Clear();
                feed.AddRange(game.Feed);
            }
        }

        Texture2D Solid(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        void EnsureStyles()
        {
            if (panelTex != null) return;
            panelTex = Solid(new Color(0.08f, 0.08f, 0.1f, 0.82f));
            darkTex = Solid(new Color(0f, 0f, 0f, 0.55f));
            barBgTex = Solid(new Color(0.2f, 0.2f, 0.2f, 1f));

            title = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            line = new GUIStyle(GUI.skin.label) { fontSize = 17, normal = { textColor = Color.white } };
            small = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = new Color(1f, 1f, 1f, 0.85f) } };
            center = new GUIStyle(GUI.skin.label) { fontSize = 17, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
            big = new GUIStyle(GUI.skin.label) { fontSize = 40, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
        }

        void Panel(Rect r) => GUI.DrawTexture(r, panelTex);

        void OnGUI()
        {
            EnsureStyles();
            if (game == null) { GUI.Label(new Rect(20f, 20f, 400f, 30f), "Менеджер не найден", line); return; }

            if (game.Phase == OrderPhase.Done) DrawResult();
            else
            {
                DrawStatus();
                DrawObjective();
                DrawFeed();
                DrawControls();
                if (game.Phase == OrderPhase.Ceremony) DrawCeremony();
            }
        }

        void DrawStatus()
        {
            var r = new Rect(12f, 12f, 320f, 148f);
            Panel(r);
            float x = r.x + 14f, y = r.y + 10f, w = r.width - 28f;

            GUI.Label(new Rect(x, y, w, 26f), $"ЗАКАЗ · {game.OrderTime:0} сек", title);
            y += 30f;

            // дед-метр
            GUI.Label(new Rect(x, y, 110f, 20f), "Дед-метр", line);
            float cond = grandpa != null ? grandpa.Condition : 0f;
            var bar = new Rect(x + 110f, y + 3f, w - 110f, 16f);
            GUI.DrawTexture(bar, barBgTex);
            GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width * cond / 100f, bar.height),
                Solid(cond > 55f ? new Color(0.35f, 0.75f, 0.3f) : cond > 25f ? new Color(0.9f, 0.7f, 0.15f) : new Color(0.85f, 0.25f, 0.2f)));
            y += 24f;

            GUI.Label(new Rect(x, y, w, 22f), $"Стиль: {game.StylePoints:0}   Спасений: {game.Catches}", line);
            y += 24f;
            string lid = coffin != null ? (coffin.LidClosed ? "крышка на месте" : "КРЫШКА СОРВАНА") : "—";
            float tilt = coffin != null ? coffin.TiltDeg : 0f;
            GUI.Label(new Rect(x, y, w, 22f), $"Крен: {tilt:0}°   {lid}", line);
        }

        void DrawObjective()
        {
            var r = new Rect(Screen.width - 332f, 12f, 320f, 120f);
            Panel(r);
            float x = r.x + 14f, y = r.y + 10f, w = r.width - 28f;

            string goal = game.Phase == OrderPhase.Carry
                ? "Донесите гроб с дедом до могилы на кладбище"
                : "Церемония — см. центр экрана";
            GUI.Label(new Rect(x, y, w, 24f), "ЗАДАЧА", title);
            y += 28f;
            GUI.Label(new Rect(x, y, w, 20f), goal, small);
            y += 24f;
            GUI.Label(new Rect(x, y, w, 20f),
                $"{(game.WillDone ? "[x]" : "[ ]")} Завещание: конверт Буканову", small);
            y += 20f;
            GUI.Label(new Rect(x, y, w, 20f),
                $"{(game.CaneDelivered ? "[x]" : "[ ]")} Трость деда — донести до могилы (+30%)", small);
        }

        void DrawFeed()
        {
            var r = new Rect(12f, Screen.height - 34f - feed.Count * 22f, 460f, feed.Count * 22f + 24f);
            GUI.DrawTexture(r, darkTex);
            float y = r.y + 6f;
            foreach (var l in feed)
            {
                GUI.Label(new Rect(r.x + 10f, y, r.width - 20f, 22f), l, small);
                y += 22f;
            }
        }

        void DrawControls()
        {
            var s = new GUIStyle(center) { fontSize = 14, normal = { textColor = new Color(1f, 1f, 1f, 0.8f) } };
            GUI.Label(new Rect(Screen.width / 2f - 330f, Screen.height - 32f, 660f, 26f),
                "ЛКМ держать — схватить · мышь ↑↓ — поднять/опустить · WASD — идти · Space — прыжок", s);
        }

        void DrawCeremony()
        {
            string text = !game.CeremonyRunning
                ? "ЦЕРЕМОНИЯ — жмите [E], чтобы засыпать"
                : $"ЗАСЫПАЕМ... {game.CeremonyTimer:0.0} — СМИРНО!";
            var box = new Rect(Screen.width / 2f - 260f, 70f, 520f, 46f);
            GUI.DrawTexture(box, panelTex);
            GUI.Label(new Rect(box.x, box.y + 8f, box.width, 30f), text, center);
        }

        void DrawResult()
        {
            var r = game.Result;
            if (r == null) return;

            float w = 900f, h = 560f;
            var panel = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), darkTex);
            GUI.DrawTexture(panel, panelTex);

            float x = panel.x + 30f, y = panel.y + 20f;

            GUI.Label(new Rect(x, y, w - 60f, 52f), "ЗАКАЗ ВЫПОЛНЕН", big);
            y += 56f;
            GUI.Label(new Rect(x, y, w - 60f, 34f), new string('★', Mathf.Max(1, r.Stars)),
                new GUIStyle(center) { fontSize = 30, normal = { textColor = new Color(1f, 0.85f, 0.3f) } });
            y += 42f;

            void Row(string label, string value)
            {
                GUI.Label(new Rect(x, y, 420f, 24f), label, line);
                GUI.Label(new Rect(x + 420f, y, 160f, 24f), value, line);
                y += 26f;
            }

            Row("База заказа", $"{r.BasePay} ₽");
            Row($"Состояние деда ({r.Condition:0}/100)", $"{r.ConditionPay} ₽");
            Row($"Скорость ({r.Time:0} сек)", $"{r.SpeedBonus} ₽");
            Row("Стиль и спасения", $"{r.StyleBonus} ₽");
            Row("Завещание (конверт Буканову)", r.WillTip > 0 ? $"+{r.WillTip} ₽" : "—");
            Row("Трость деда (+30%)", r.CaneTip > 0 ? $"+{r.CaneTip} ₽" : "—");
            y += 8f;
            GUI.Label(new Rect(x, y, 420f, 40f), "ИТОГО", title);
            GUI.Label(new Rect(x + 420f, y, 200f, 40f), $"{r.Total} ₽",
                new GUIStyle(title) { normal = { textColor = new Color(0.55f, 0.9f, 0.45f) } });
            y += 48f;

            // Хроника смены: автоскриншоты
            GUI.Label(new Rect(x, y, 400f, 24f), "ХРОНИКА СМЕНЫ", title);
            y += 30f;
            float th = 150f, tw = 266f;
            for (int i = 0; i < game.Chronicle.Count && i < 3; i++)
            {
                var shot = game.Chronicle[i];
                var tr = new Rect(x + i * (tw + 12f), y, tw, th);
                if (shot.tex != null) GUI.DrawTexture(tr, shot.tex, ScaleMode.ScaleToFit);
                GUI.DrawTexture(new Rect(tr.x, tr.y + th, tw, 24f), darkTex);
                GUI.Label(new Rect(tr.x, tr.y + th, tw, 24f), shot.caption, center);
            }

            GUI.Label(new Rect(x, panel.y + h - 40f, w - 60f, 26f),
                "R — повторить смену   ·   Esc — показать курсор", center);
        }
    }
}
