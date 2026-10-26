using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DI
{
    /// Центральный менеджер заказа: таймер, лента событий, очки, завершение.
    public class DIGameManager : MonoBehaviour
    {
        public static DIGameManager I { get; private set; }

        public CoffinBody coffin;
        public GrandpaState grandpa;

        public float OrderTime { get; private set; }
        public bool Completed { get; private set; }
        public float StylePoints { get; private set; }
        public int Catches { get; private set; }

        readonly List<string> feed = new List<string>();

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
        }

        void Start()
        {
            if (coffin == null) coffin = FindFirstObjectByType<CoffinBody>();
            if (grandpa == null) grandpa = FindFirstObjectByType<GrandpaState>();
        }

        void Update()
        {
            if (!Completed) OrderTime += Time.deltaTime;
            else if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Event(string message)
        {
            feed.Add($"[{OrderTime:0.0}с] {message}");
            if (feed.Count > 7) feed.RemoveAt(0);
            Debug.Log($"[ДИ] {message}");
        }

        public void ReportDamage(float amount, bool head) =>
            Event(head ? $"Дед ударился головой! −{amount:0} состояния" : $"Дед получил урон −{amount:0}");

        public void ReportCatch(bool head)
        {
            Catches++;
            if (head)
            {
                StylePoints += 20f;
                grandpa.AddCondition(-5f);
                Event("«Упругий череп» — спасение головой! +20 стиля");
            }
            else
            {
                StylePoints += 15f;
                Event("«Спасение» — поймали деда в воздухе! +15 стиля");
            }
        }

        public void ReportLidBroken() => Event("Крышку сорвало! Дед теперь без защиты.");

        public void CompleteOrder()
        {
            if (Completed) return;
            Completed = true;
            Event("Заказ выполнен. Дед доставлен (почти).");
        }

        public IReadOnlyList<string> Feed => feed;
    }
}
