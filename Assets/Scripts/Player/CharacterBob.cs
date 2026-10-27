using UnityEngine;

namespace DI
{
    /// Оживление блочного персонажа: наклон в сторону движения, покачивание,
    /// лёгкий поворот. Вешается на визуальную модель.
    public class CharacterBob : MonoBehaviour
    {
        Rigidbody body;          // чьё движение отслеживаем
        float bobPhase;
        Vector3 lastPos;
        bool initialized;

        public static void Attach(GameObject visual, Rigidbody bodyToTrack)
        {
            var bob = visual.AddComponent<CharacterBob>();
            bob.body = bodyToTrack;
        }

        void LateUpdate()
        {
            if (body == null) return;
            if (!initialized)
            {
                lastPos = body.position;
                initialized = true;
                return;
            }

            Vector3 vel = body.linearVelocity;
            vel.y = 0f;
            float speed = vel.magnitude;

            // наклон по ходу движения
            if (speed > 0.5f)
            {
                Vector3 flat = new Vector3(vel.x, 0f, vel.z).normalized;
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(flat), 8f * Time.deltaTime);
                transform.rotation *= Quaternion.Euler(6f, 0f, 0f); // вперёд лёгкий наклон
            }

            // покачивание при ходьбе
            bobPhase += speed * 9f * Time.deltaTime;
            float bob = speed > 0.5f ? Mathf.Abs(Mathf.Sin(bobPhase)) * 0.06f : 0f;
            var lp = transform.localPosition;
            transform.localPosition = new Vector3(lp.x, bob, lp.z);
            lastPos = body.position;
        }
    }
}
