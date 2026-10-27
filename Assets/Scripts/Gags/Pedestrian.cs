using UnityEngine;

namespace DI
{
    /// Пешеход: ходит по дороге туда-сюда, недоволен гробом на дороге,
    /// а при ударе падает рэгдоллом и через пару секунд встаёт.
    public class Pedestrian : MonoBehaviour
    {
        static readonly string[] WalkLines =
        {
            "Куда прёшь?!",
            "Гроб посреди дороги — это дно!",
            "Ну и служба у них...",
            "Мама, смотри — гробик!",
            "Я в этот район квартиры не советую.",
        };
        static readonly string[] HitLines =
        {
            "Меня гробом сбили!!",
            "Скорая!.. то есть ритуальная!",
            "Я на вас жалуюсь!!",
        };

        public Vector3 axis = Vector3.forward;
        public float range = 8f;
        public float speed = 1.1f;

        Vector3 origin;
        float phase;
        int dir = 1;
        Rigidbody rb;
        bool downed;
        float upAt;
        float nextPhrase;

        public static Pedestrian Spawn(Vector3 pos, Vector3 axis, float range)
        {
            var go = new GameObject("Pedestrian");
            go.layer = LayerMask.NameToLayer("Grabbable"); // чтобы захват игрока не цеплял лишний раз, но коллизии были
            go.transform.position = pos;
            go.AddComponent<NetDynamicMarker>();

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 70f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var col = go.AddComponent<CapsuleCollider>();
            col.radius = 0.3f;
            col.height = 1.6f;
            col.center = new Vector3(0f, 0.8f, 0f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.transform.SetParent(go.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            body.transform.localScale = new Vector3(0.9f, 0.85f, 0.9f);
            body.GetComponent<Renderer>().material.color = new Color(0.55f, 0.45f, 0.6f);
            body.SetActive(false); // заглушка

            // блочный персонаж вместо капсулы
            var model = Art.Model("characters/character-b", Vector3.zero, 0f, 1f, 0f, keepMaterials: true);
            if (model != null)
            {
                model.name = "Character";
                model.transform.SetParent(go.transform, false);
                CharacterBob.Attach(model, rb);
            }

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.transform.SetParent(go.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            head.transform.localScale = Vector3.one * 0.36f;
            head.GetComponent<Renderer>().material.color = new Color(0.85f, 0.68f, 0.55f);

            var p = go.AddComponent<Pedestrian>();
            p.rb = rb;
            p.origin = pos;
            p.axis = axis.normalized;
            p.range = range;
            return p;
        }

        void Update()
        {
            if (downed)
            {
                if (Time.time >= upAt) StandUp();
                return;
            }

            phase += dir * speed * Time.deltaTime;
            if (phase > range) { phase = range; dir = -1; }
            if (phase < 0f) { phase = 0f; dir = 1; }

            var coffin = DIGameManager.I != null ? DIGameManager.I.coffin : null;

            // недовольство издалека
            if (coffin != null && Time.time >= nextPhrase
                && (coffin.transform.position - transform.position).magnitude < 6f)
            {
                DIGameManager.I?.Event($"Пешеход: «{WalkLines[Random.Range(0, WalkLines.Length)]}»");
                nextPhrase = Time.time + Random.Range(6f, 12f);
            }

            // цель — точка маршрута; гроб близко — обойти стороной
            Vector3 target = origin + axis * phase;
            Vector3 to = target - transform.position;
            to.y = 0f;
            Vector3 v = to.normalized * speed;

            if (coffin != null)
            {
                Vector3 away = transform.position - coffin.transform.position;
                away.y = 0f;
                if (away.magnitude < 2.2f)
                    v += away.normalized * 1.6f;
            }

            var vel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(v.x, vel.y, v.z);
            if (v.sqrMagnitude > 0.1f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(new Vector3(v.x, 0f, v.z)), 6f * Time.deltaTime);
        }

        void OnCollisionEnter(Collision c)
        {
            if (downed) return;
            var other = c.collider.attachedRigidbody;
            if (other == null || other.mass < 60f) return;
            if (c.relativeVelocity.magnitude < 2.2f) return;

            downed = true;
            upAt = Time.time + 3.5f;
            rb.freezeRotation = false;
            rb.AddForce(c.relativeVelocity * 0.6f + Vector3.up * 2.5f, ForceMode.VelocityChange);
            Sfx.PlayAt("thump", transform.position);
            DIGameManager.I?.Event($"Пешеход: «{HitLines[Random.Range(0, HitLines.Length)]}»");
            DIGameManager.I?.AddStyle(5f); // сбил пешехода — плюс к стилю, минус к репутации
        }

        void StandUp()
        {
            downed = false;
            rb.freezeRotation = true;
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        }
    }
}
