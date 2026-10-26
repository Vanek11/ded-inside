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
        public float maxForcePerHand = 950f;
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

            if (!Physics.Raycast(cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out var hit, reach, mask))
                return;

            var target = hit.rigidbody;
            if (target == null || target.isKinematic) return;

            held = target;
            localPoint = target.transform.InverseTransformPoint(hit.point);
            DIGameManager.I?.Event("Взялись!");
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
