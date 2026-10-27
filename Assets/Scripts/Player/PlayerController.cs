using UnityEngine;

namespace DI
{
    /// Пузатый грузчик: ходьба-бег, прыжок. Направление — относительно камеры.
    /// Капсула скользкая (без трения), чтобы не прилипать к стенам; позицию
    /// держит сам контроллер. Прыжок с койот-таймом и двойной проверкой земли.
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 6.5f;
        public float jumpImpulse = 5.6f;
        public float airControl = 0.35f;
        public float carrySpeedMul = 0.5f; // во сколько раз медленнее с тяжёлым грузом
        public float coyoteTime = 0.12f;

        [HideInInspector] public Transform CameraTransform; // назначается при сборке сцены

        Rigidbody rb;
        GrabSystem grab;
        float lastGrounded = -10f;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            grab = GetComponent<GrabSystem>();
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // без трения: не липнем к стенам и потолкам; горизонт удерживает контроллер
            var slide = new PhysicsMaterial("player_slide")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                bounciness = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };
            GetComponent<Collider>().sharedMaterial = slide;
        }

        void Update()
        {
            // страховка: упал за край мира — на последний чекпоинт
            if (transform.position.y < -15f)
            {
                rb.position = LevelHelpers.LastCheckpoint;
                rb.linearVelocity = Vector3.zero;
                DIGameManager.I?.Event("Ты выпал из мира и вернулся на чекпоинт.");
            }

            if (Time.time - lastGrounded <= coyoteTime && Input.GetKeyDown(KeyCode.Space))
            {
                var v = rb.linearVelocity;
                rb.linearVelocity = new Vector3(v.x, jumpImpulse, v.z);
                lastGrounded = -10f;
            }
        }

        void FixedUpdate()
        {
            bool grounded = CheckGrounded();
            if (grounded) lastGrounded = Time.time;

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

            float speed = moveSpeed * (SessionState.HasGear("boots") ? 1.15f : 1f)
                          * (grab != null && grab.IsCarryingHeavy ? carrySpeedMul : 1f);
            Vector3 target = wish * speed;
            var v = rb.linearVelocity;
            Vector3 accel = new Vector3(target.x - v.x, 0f, target.z - v.z) * (grounded ? 12f : 12f * airControl);
            rb.AddForce(accel, ForceMode.Acceleration);
        }

        bool CheckGrounded()
        {
            Vector3 feet = transform.position;

            // 1) динамика под ногами (гроб, дед, крышка): SphereCast стартует выше,
            //    поэтому не игнорируется собственная капсула, но игнорирует старт внутри коллайдера
            if (Physics.SphereCast(feet + Vector3.up * 0.6f, 0.3f, Vector3.down, out _, 0.72f))
                return true;

            // 2) статика (пол, дороги, крыльцо, доски): сфера чуть ниже ступней
            int staticMask = ~LayerMask.GetMask("Player", "Grabbable", "Grandpa");
            return Physics.CheckSphere(feet + Vector3.up * 0.1f, 0.34f, staticMask,
                QueryTriggerInteraction.Ignore);
        }
    }
}
