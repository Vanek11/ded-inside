using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DI
{
    public enum OrderPhase { Carry, Ceremony, Done }

    /// Результат оплаты заказа (расчёт в FinishOrder).
    public class OrderResult
    {
        public float Condition, Time;
        public int BasePay, ConditionPay, SpeedBonus, StyleBonus, WillTip, CaneTip, Total, Stars;
    }

    /// Центральный менеджер заказа: фазы, таймер, лента, очки, церемония, оплата.
    public class DIGameManager : MonoBehaviour
    {
        public static DIGameManager I { get; private set; }

        public CoffinBody coffin;
        public GrandpaState grandpa;

        public OrderPhase Phase { get; private set; } = OrderPhase.Carry;
        public float OrderTime { get; private set; }
        public float StylePoints { get; private set; }
        public int Catches { get; private set; }
        public bool WillDone { get; set; }
        public bool CaneDelivered { get; set; }
        public OrderResult Result { get; private set; }

        public const float CeremonyDuration = 3f;
        public float CeremonyTimer { get; private set; }
        public bool CeremonyRunning => Phase == OrderPhase.Ceremony && ceremonyRunning;
        bool ceremonyRunning;

        // могила сообщает каждый физический шаг, что гроб внутри (см. GoalZone)
        bool goalMarkedThisStep;
        bool goalInside;

        readonly List<string> feed = new List<string>();

        public class ChronicleShot { public Texture2D tex; public string caption; }
        public readonly List<ChronicleShot> Chronicle = new List<ChronicleShot>();

        public long FeedVersion { get; private set; }
        public IReadOnlyList<string> Feed => feed;

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
            if (Phase == OrderPhase.Done)
            {
                if (Input.GetKeyDown(KeyCode.R))
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            if (Phase == OrderPhase.Carry) OrderTime += Time.deltaTime;

            if (Phase == OrderPhase.Ceremony)
            {
                if (!ceremonyRunning)
                {
                    if (goalInside && Input.GetKeyDown(KeyCode.E))
                    {
                        ceremonyRunning = true;
                        CeremonyTimer = CeremonyDuration;
                        Event("«ЗАСЫПАТЬ!» — смирно, три секунды...");
                    }
                }
                else
                {
                    CeremonyTimer -= Time.deltaTime;
                    if (!goalInside)
                    {
                        ceremonyRunning = false;
                        StylePoints = Mathf.Max(0f, StylePoints - 30f);
                        Event("Гроб выкатился из могилы! Церемония сорвана. −30 стиля");
                    }
                    else if (CeremonyTimer <= 0f)
                    {
                        ceremonyRunning = false;
                        FinishOrder();
                    }
                }
            }
        }

        void FixedUpdate()
        {
            goalInside = goalMarkedThisStep;
            goalMarkedThisStep = false;
        }

        public void MarkGoalInside() => goalMarkedThisStep = true;

        public void Event(string message)
        {
            feed.Add($"[{OrderTime:0.0}с] {message}");
            if (feed.Count > 8) feed.RemoveAt(0);
            FeedVersion++;
            Debug.Log($"[ДИ] {message}");
        }

        public void ReportDamage(float amount, bool head)
        {
            Event(head ? $"Дед ударился головой! −{amount:0} состояния" : $"Дед получил урон −{amount:0}");
            if (amount >= 12f) CaptureChronicle(head ? "Голова!" : "Приехали...");
        }

        public void ReportCatch(bool head)
        {
            Catches++;
            if (head)
            {
                StylePoints += 20f;
                if (grandpa != null) grandpa.AddCondition(-5f);
                Event("«Упругий череп» — спасение головой! +20 стиля");
            }
            else
            {
                StylePoints += 15f;
                Event("«Спасение» — поймали деда в воздухе! +15 стиля");
            }
        }

        public void ReportLidBroken()
        {
            Event("Крышку сорвало! Дед теперь без защиты.");
            CaptureChronicle("Крышку сорвало!");
        }

        /// Гроб устоял в могиле — начать церемонию.
        public void CompleteOrder()
        {
            if (Phase != OrderPhase.Carry) return;
            Phase = OrderPhase.Ceremony;
            Event("Гроб в могиле! Жмите [E], чтобы засыпать.");
        }

        void FinishOrder()
        {
            Phase = OrderPhase.Done;
            Event("Заказ выполнен. Дед доставлен (почти).");

            // трость должна лежать у могилы к моменту расчёта
            var cane = GameObject.Find("Cane");
            var goal = FindFirstObjectByType<GoalZone>();
            CaneDelivered = cane != null && goal != null
                && Vector3.Distance(cane.transform.position, goal.transform.position) < 3.5f;
            if (CaneDelivered) Event("Трость деда доставлена! Чаевые +30%");

            var r = new OrderResult();
            r.Condition = grandpa != null ? grandpa.Condition : 0f;
            r.Time = OrderTime;
            r.BasePay = 300;
            r.ConditionPay = Mathf.RoundToInt(r.Condition * 4f);
            r.SpeedBonus = Mathf.Clamp(Mathf.RoundToInt((240f - OrderTime) * 1.5f), 0, 200);
            r.StyleBonus = Mathf.RoundToInt(StylePoints * 2f);
            r.WillTip = WillDone ? 150 : 0;
            int subtotal = r.BasePay + r.ConditionPay + r.SpeedBonus + r.StyleBonus + r.WillTip;
            r.CaneTip = CaneDelivered ? Mathf.RoundToInt(subtotal * 0.3f) : 0;
            r.Total = subtotal + r.CaneTip;
            r.Stars = r.Total >= 900 ? 3 : r.Total >= 650 ? 2 : 1;
            Result = r;

            SessionState.AddMoney(r.Total);
            Event($"Касса пополнена на {r.Total} ₽");
        }

        /// Автоскриншот «Хроники смены» (максимум 3 кадра на заказ).
        public void CaptureChronicle(string caption)
        {
            if (Chronicle.Count >= 3 || Phase == OrderPhase.Done) return;
            StartCoroutine(CaptureNextFrame(caption));
        }

        IEnumerator CaptureNextFrame(string caption)
        {
            yield return new WaitForEndOfFrame();
            Chronicle.Add(new ChronicleShot
            {
                tex = ScreenCapture.CaptureScreenshotAsTexture(),
                caption = caption
            });
        }
    }
}
