using UnityEngine;

namespace DI
{
    /// Могила: помечает, что гроб внутри; когда устоял — менеджер начинает церемонию.
    [RequireComponent(typeof(BoxCollider))]
    public class GoalZone : MonoBehaviour
    {
        public float settleSeconds = 1.2f;
        float insideTimer;
        bool countedThisStep;

        void FixedUpdate() => countedThisStep = false;

        void OnTriggerStay(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb == null) return;

            var coffin = rb.GetComponent<CoffinBody>();
            if (coffin == null) return;

            DIGameManager.I?.MarkGoalInside();

            if (countedThisStep) return;
            countedThisStep = true;

            var game = DIGameManager.I;
            if (game == null || game.Phase != OrderPhase.Carry) return;

            if (rb.linearVelocity.magnitude < 0.6f)
            {
                insideTimer += Time.fixedDeltaTime;
                if (insideTimer >= settleSeconds) game.CompleteOrder();
            }
            else insideTimer = 0f;
        }
    }
}
