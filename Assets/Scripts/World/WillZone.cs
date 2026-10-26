using UnityEngine;

namespace DI
{
    /// Завещание деда: «Заедь к Буканову — оставь конверт». Триггер у дороги.
    [RequireComponent(typeof(BoxCollider))]
    public class WillZone : MonoBehaviour
    {
        void OnTriggerStay(Collider other)
        {
            var game = DIGameManager.I;
            if (game == null || game.WillDone || game.Phase != OrderPhase.Carry) return;

            var rb = other.attachedRigidbody;
            if (rb == null || !rb.CompareTag("Player")) return;

            game.WillDone = true;
            game.Event("Завещание: конверт оставлен Буканову. +150 ₽ чаевых!");
        }
    }
}
