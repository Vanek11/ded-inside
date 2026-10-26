using UnityEngine;

namespace DI
{
    /// Могила: гроб внутри и неподвижен пару секунд — заказ выполнен.
    [RequireComponent(typeof(BoxCollider))]
    public class GoalZone : MonoBehaviour
    {
        public float settleSeconds = 1.2f;

        float insideTimer;
        bool done;
        bool countedThisStep;

        void FixedUpdate() => countedThisStep = false;

        void OnTriggerStay(Collider other)
        {
            if (done || countedThisStep) return;

            var rb = other.attachedRigidbody;
            var coffin = rb != null ? rb.GetComponent<CoffinBody>() : null;
            countedThisStep = true;

            if (coffin != null && rb.linearVelocity.magnitude < 0.6f)
            {
                insideTimer += Time.fixedDeltaTime;
                if (insideTimer >= settleSeconds)
                {
                    done = true;
                    DIGameManager.I?.CompleteOrder();
                }
            }
            else insideTimer = 0f;
        }
    }
}
