using UnityEngine;

namespace DI
{
    /// Гроб: следит за креном (скрип-предупреждение), включает режим «санок»
    /// на склонах и знает, на месте ли крышка. Выпадение деда — честная физика:
    /// стенки и дно держат его, пока крышка закрыта.
    public class CoffinBody : MonoBehaviour
    {
        public float tiltCreakDeg = 50f;
        public float slopeSlideDeg = 20f;

        [SerializeField, HideInInspector] Rigidbody lid;    // сериализуется: назначается фабрикой
        [SerializeField, HideInInspector] GrandpaState grandpaRef;
        public Rigidbody Lid { get => lid; set => lid = value; }
        public GrandpaState Grandpa { get => grandpaRef; set => grandpaRef = value; }
        public float TiltDeg { get; private set; }
        public bool Sliding { get; private set; }

        Rigidbody rb;
        Joint lidJoint;
        bool lidChecked;
        Collider[] ownCols;
        PhysicsMaterial grip, ice;
        float lastCreak;

        public bool LidClosed => lidJoint != null;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // только коллайдеры самого корпуса (крышка — отдельное тело)
            var all = GetComponentsInChildren<Collider>();
            var list = new System.Collections.Generic.List<Collider>();
            foreach (var c in all)
                if (!c.isTrigger && c.attachedRigidbody == rb) list.Add(c);
            ownCols = list.ToArray();

            grip = new PhysicsMaterial("coffin_grip") { dynamicFriction = 0.65f, staticFriction = 0.75f };
            ice = new PhysicsMaterial("coffin_slide") { dynamicFriction = 0.03f, staticFriction = 0.05f };
            SetMaterial(grip);
        }

        void FixedUpdate()
        {
            if (!lidChecked && Lid != null)
            {
                lidJoint = Lid.GetComponent<Joint>();
                lidChecked = true;
            }

            TiltDeg = Vector3.Angle(transform.up, Vector3.up);

            // санки: на крутом склоне и в движении гроб скользит
            bool onSlope = Physics.Raycast(new Ray(transform.position, Vector3.down), out var hit, 2f)
                           && Vector3.Angle(hit.normal, Vector3.up) > slopeSlideDeg
                           && rb.linearVelocity.magnitude > 0.5f;
            if (onSlope != Sliding)
            {
                Sliding = onSlope;
                SetMaterial(Sliding ? ice : grip);
            }

            if (TiltDeg > tiltCreakDeg && Time.time - lastCreak > 1f)
            {
                lastCreak = Time.time;
                Sfx.PlayAt("creak", transform.position);
                DIGameManager.I?.Event("Скри-и-ип... дед сползает!");
            }
        }

        void SetMaterial(PhysicsMaterial m)
        {
            foreach (var c in ownCols) c.sharedMaterial = m;
        }
    }
}
