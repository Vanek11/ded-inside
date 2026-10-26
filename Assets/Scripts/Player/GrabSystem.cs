using UnityEngine;

namespace DI
{
    /// Физический захват «как в Moving Out»: держишь кнопку — пружина тянет точку
    /// захвата к руке. Сила одной руки ограничена: один грузчик гроб еле тащит,
    /// двое — несут, четверо — летят. Тяжесть определяется массой тела.
    public class GrabSystem : MonoBehaviour
    {
        public float reach = 3.4f;
        public float spring = 2200f;
        public float damping = 160f;
        public float maxForcePerHand = 1100f;
        public float heavyMass = 60f;

        Transform hand;
        Rigidbody held;
        Vector3 localPoint;

        public bool IsHolding => held != null;
        public bool IsCarryingHeavy => held != null && held.mass >= heavyMass;

        void Awake()
        {
            hand = new GameObject("HandAnchor").transform;
            hand.SetParent(transform, false);
            hand.localPosition = new Vector3(0.5f, 1.1f, 1.0f);
        }

        void Update()
        {
            bool want = Input.GetMouseButton(0);
            if (want && held == null) TryGrab();
            else if (!want && held != null) held = null;
        }

        void TryGrab()
        {
            var cam = Camera.main;
            if (cam == null) return;

            int mask = LayerMask.GetMask("Grabbable", "Grandpa");
            if (mask == 0) mask = ~0;

            // луч — через центр экрана, но дистанция захвата меряется ОТ ИГРОКА,
            // а не от камеры (камера висит в 5-6 м позади спины)
            if (!Physics.Raycast(cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out var hit, 100f, mask))
                return;

            if (Vector3.Distance(transform.position, hit.point) > reach) return;

            var target = hit.rigidbody;
            if (target == null || target.isKinematic) return;

            held = target;
            localPoint = target.transform.InverseTransformPoint(hit.point);
            DIGameManager.I?.Event("Взялись!");
        }

        void OnGUI()
        {
            // прицел: белый пока руки пустые, зелёный пока что-то держим
            var prev = GUI.color;
            GUI.color = held != null ? new Color(0.4f, 1f, 0.4f) : Color.white;
            var style = new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(Screen.width / 2f - 30f, Screen.height / 2f - 25f, 60f, 50f),
                held != null ? "\u25CF" : "+", style);
            GUI.color = prev;
        }

        void FixedUpdate()
        {
            if (held == null) return;

            Vector3 point = held.transform.TransformPoint(localPoint);
            Vector3 vel = held.GetPointVelocity(point);
            Vector3 force = (hand.position - point) * spring - vel * damping;
            if (force.sqrMagnitude > maxForcePerHand * maxForcePerHand)
                force = force.normalized * maxForcePerHand;
            held.AddForceAtPosition(force, point);
        }
    }
}
