using UnityEngine;

namespace DI
{
    /// Ворона-воровка: выжидает, крадёт оставленную без присмотра трость
    /// и кружит с ней над уровнем. Сбить — прыгнув в неё (физический контакт).
    public class Crow : MonoBehaviour
    {
        enum State { Waiting, Approaching, Circling, Fleeing }

        State state = State.Waiting;
        float readyAt;
        bool hasCane;
        Transform cane;
        Rigidbody caneRb;
        Rigidbody rb;
        Transform wingL, wingR;
        readonly Vector3 orbitCenter = new Vector3(0f, 7.5f, 20f);
        float angle;

        public static Crow Spawn(Vector3 pos, float delay)
        {
            var go = new GameObject("Crow");
            go.transform.position = pos;
            go.AddComponent<NetDynamicMarker>();

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 3f;
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            var col = go.AddComponent<SphereCollider>();
            col.radius = 0.5f;

            var black = new Color(0.08f, 0.08f, 0.09f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.transform.SetParent(go.transform, false);
            body.transform.localScale = new Vector3(0.55f, 0.5f, 0.9f);
            body.GetComponent<Renderer>().material.color = black;

            var beak = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beak.name = "Beak";
            Object.DestroyImmediate(beak.GetComponent<Collider>());
            beak.transform.SetParent(go.transform, false);
            beak.transform.localPosition = new Vector3(0f, -0.05f, 0.5f);
            beak.transform.localScale = new Vector3(0.09f, 0.09f, 0.28f);
            beak.GetComponent<Renderer>().material.color = new Color(0.9f, 0.6f, 0.15f);

            var crow = go.AddComponent<Crow>();
            crow.rb = rb;
            crow.readyAt = Time.time + delay;
            crow.wingL = MakeWing(go.transform, -1f, black);
            crow.wingR = MakeWing(go.transform, 1f, black);
            return crow;
        }

        static Transform MakeWing(Transform parent, float side, Color color)
        {
            var wing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wing.name = "Wing";
            Object.DestroyImmediate(wing.GetComponent<Collider>());
            wing.transform.SetParent(parent, false);
            wing.transform.localPosition = new Vector3(side * 0.5f, 0.1f, 0f);
            wing.transform.localScale = new Vector3(0.9f, 0.05f, 0.4f);
            wing.GetComponent<Renderer>().material.color = color;
            return wing.transform;
        }

        void Start()
        {
            var caneGo = GameObject.Find("Cane");
            if (caneGo != null)
            {
                cane = caneGo.transform;
                caneRb = caneGo.GetComponent<Rigidbody>();
            }
        }

        void Update()
        {
            float s = Mathf.Sin(Time.time * 18f) * 35f;
            if (wingL != null) wingL.localRotation = Quaternion.Euler(0f, 0f, s);
            if (wingR != null) wingR.localRotation = Quaternion.Euler(0f, 0f, -s);
        }

        void FixedUpdate()
        {
            if (state != State.Fleeing
                && (cane == null || (DIGameManager.I != null && DIGameManager.I.Phase == OrderPhase.Done)))
            {
                DropCane();
                BeginFlee();
                return;
            }

            switch (state)
            {
                case State.Waiting:
                    if (Time.time >= readyAt && caneRb != null
                        && caneRb.linearVelocity.magnitude < 0.25f)
                        state = State.Approaching;
                    break;
                case State.Approaching:
                    FlyTo(cane.position + Vector3.up * 1.1f, 6f);
                    if ((cane.position - transform.position).magnitude < 1.2f) Snatch();
                    break;
                case State.Circling:
                    angle += 0.7f * Time.fixedDeltaTime;
                    var target = orbitCenter + new Vector3(Mathf.Cos(angle) * 9f, 0f, Mathf.Sin(angle) * 9f);
                    FlyTo(target, 5f);
                    break;
                case State.Fleeing:
                    FlyTo(transform.position + Vector3.up * 20f + transform.forward * 10f, 6f);
                    break;
            }
        }

        void FlyTo(Vector3 target, float speed)
        {
            Vector3 to = target - transform.position;
            rb.linearVelocity = Vector3.ClampMagnitude(to * 2.5f, speed);
            if (to.sqrMagnitude > 0.5f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(to.normalized), 4f * Time.fixedDeltaTime);
        }

        void Snatch()
        {
            if (cane == null || hasCane) return;
            cane.SetParent(transform);
            cane.localPosition = new Vector3(0f, -0.6f, 0f);
            if (caneRb != null) caneRb.isKinematic = true;
            hasCane = true;
            state = State.Circling;
            angle = Random.Range(0f, 6.28f);
            DIGameManager.I?.Event("Ворона утащила трость! Сбей её прыжком!");
        }

        void DropCane()
        {
            if (cane == null || !hasCane) return;
            cane.SetParent(null);
            if (caneRb != null)
            {
                caneRb.isKinematic = false;
                caneRb.linearVelocity = rb.linearVelocity;
            }
            hasCane = false;
            DIGameManager.I?.Event("Ворона выронила трость!");
        }

        void BeginFlee()
        {
            state = State.Fleeing;
            Destroy(gameObject, 6f);
        }

        void OnCollisionEnter(Collision c)
        {
            var rbOther = c.collider.attachedRigidbody;
            if (state == State.Circling && rbOther != null && rbOther.CompareTag("Player"))
            {
                DropCane();
                BeginFlee();
                DIGameManager.I?.Event("Птица повержена! +10 стиля");
                if (DIGameManager.I != null) DIGameManager.I.AddStyle(10f);
            }
        }
    }
}
