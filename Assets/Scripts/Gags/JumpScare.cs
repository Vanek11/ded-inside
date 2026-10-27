using UnityEngine;

namespace DI
{
    /// Гусь-хулиган: гусиная засада. Патрулирует, при приближении гроба —
    /// подпрыгивает и толкается в борт. Полностью физический.
    public class JumpScare : MonoBehaviour
    {
        public float detectRadius = 3.5f;
        public float jumpImpulse = 4.5f;
        public float pushImpulse = 3.5f;
        public float cooldown = 2.5f;
        public string gag = "Гусь атакует!";
        public string soundId = "goose";

        Rigidbody rb;
        float lastJump;

        void Awake() => rb = GetComponent<Rigidbody>();

        void FixedUpdate()
        {
            var coffin = DIGameManager.I != null ? DIGameManager.I.coffin : null;
            if (coffin == null) return;

            Vector3 to = coffin.transform.position - transform.position;
            to.y = 0f;
            if (to.magnitude > detectRadius) return;

            if (Time.time - lastJump < cooldown) return;
            lastJump = Time.time;

            Vector3 dir = to.normalized;
            rb.AddForce((Vector3.up * 1.6f + dir * 0.7f) * jumpImpulse, ForceMode.VelocityChange);
            coffin.GetComponent<Rigidbody>().AddForceAtPosition(dir * pushImpulse,
                coffin.transform.position + Vector3.up * 0.1f);

            Sfx.PlayAt(soundId, transform.position);
            DIGameManager.I?.Event(gag);
        }
    }
}
