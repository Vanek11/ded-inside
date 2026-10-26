using UnityEngine;

namespace DI
{
    /// Пузатый грузчик: ходьба-бег, прыжок. Направление — относительно камеры.
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 6.5f;
        public float jumpImpulse = 5.6f;
        public float airControl = 0.35f;
        public float carrySpeedMul = 0.5f; // во сколько раз медленнее с тяжёлым грузом

        [HideInInspector] public Transform CameraTransform; // назначается при сборке сцены

        Rigidbody rb;
        GrabSystem grab;
        bool grounded;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            grab = GetComponent<GrabSystem>();
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        void Update()
        {
            if (grounded && Input.GetKeyDown(KeyCode.Space))
            {
                var v = rb.linearVelocity;
                rb.linearVelocity = new Vector3(v.x, jumpImpulse, v.z);
            }
        }

        void FixedUpdate()
        {
            grounded = CheckGrounded();

            var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            Vector3 fwd = Vector3.forward, right = Vector3.right;
            if (CameraTransform)
            {
                fwd = Vector3.ProjectOnPlane(CameraTransform.forward, Vector3.up);
                right = Vector3.ProjectOnPlane(CameraTransform.right, Vector3.up);
                if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
                fwd.Normalize();
                right.Normalize();
            }

            Vector3 wish = fwd * input.y + right * input.x;
            if (wish.sqrMagnitude > 1f) wish.Normalize();

            float speed = moveSpeed * (grab != null && grab.IsCarryingHeavy ? carrySpeedMul : 1f);
            Vector3 target = wish * speed;
            var v = rb.linearVelocity;
            Vector3 accel = new Vector3(target.x - v.x, 0f, target.z - v.z) * (grounded ? 12f : 12f * airControl);
            rb.AddForce(accel, ForceMode.Acceleration);
        }

        bool CheckGrounded()
        {
            Vector3 origin = transform.position + Vector3.up * 0.3f;
            return Physics.SphereCast(origin, 0.32f, Vector3.down, out _, 0.42f);
        }
    }
}
