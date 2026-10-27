using UnityEngine;

namespace DI
{
    /// Состояние деда: дед-метр, урон от ударов, спасения в падении, утопление.
    public class GrandpaState : MonoBehaviour
    {
        public float Condition { get; private set; } = 100f;
        public bool InWater { get; private set; }

        Rigidbody rootRb;
        float lastCatch = -10f;
        float waterTimer;
        int waterTouches;
        float nextGroan;

        void Awake()
        {
            rootRb = GetComponent<Rigidbody>();
            nextGroan = Time.time + Random.Range(15f, 30f);
        }

        void Update()
        {
            // пасхалка «А он точно?..»: дед иногда постанывает
            if (DIGameManager.I != null && DIGameManager.I.Phase != OrderPhase.Carry) return;
            if (Time.time < nextGroan) return;
            nextGroan = Time.time + Random.Range(25f, 45f);
            Sfx.PlayAt("groan", transform.position);
            if (Random.value < 0.35f) DIGameManager.I?.Event("Дед: «М-м-м...»");
        }

        void FixedUpdate()
        {
            InWater = waterTouches > 0;
            waterTouches = 0;

            if (InWater)
            {
                waterTimer += Time.fixedDeltaTime;
                if (waterTimer > 4f)
                {
                    Condition = Mathf.Max(0f, Condition - 3f * Time.fixedDeltaTime);
                    if (Condition <= 0f)
                        DIGameManager.I?.Event("Дед утонул окончательно.");
                }
            }
            else waterTimer = 0f;
        }

        public void RegisterImpact(Collision c, GrandpaPart part)
        {
            float impact = c.relativeVelocity.magnitude;
            if (impact < 3.2f) return;

            bool byPlayer = c.collider != null
                            && c.collider.attachedRigidbody != null
                            && c.collider.attachedRigidbody.CompareTag("Player");

            if (byPlayer)
            {
                bool falling = rootRb.linearVelocity.y < -2.2f;
                if (falling && Time.time - lastCatch > 2.5f)
                {
                    lastCatch = Time.time;
                    Condition = Mathf.Min(100f, Condition + 15f);
                    DIGameManager.I?.ReportCatch(part.IsHead);
                }
                return;
            }

            float dmg = (impact - 3f) * (part.IsHead ? 4f : 2f);
            if (SessionState.HasGear("padding")) dmg *= 0.5f; // мягкая обивка гроба
            Sfx.PlayAt("thump", transform.position);
            Condition = Mathf.Max(0f, Condition - dmg);
            DIGameManager.I?.ReportDamage(dmg, part.IsHead);
        }

        public void MarkWaterTouch() => waterTouches++;

        public void AddCondition(float delta) => Condition = Mathf.Clamp(Condition + delta, 0f, 100f);
    }
}
