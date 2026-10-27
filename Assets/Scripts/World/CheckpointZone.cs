using UnityEngine;

namespace DI
{
    /// Чекпоинт сегмента: дед внутри проезжающего гроба → событие уровня.
    [RequireComponent(typeof(BoxCollider))]
    public class CheckpointZone : MonoBehaviour
    {
        public string checkpointId;
        public bool used;

        void OnTriggerStay(Collider other)
        {
            if (used) return;
            var rb = other.attachedRigidbody;
            var coffin = rb != null ? rb.GetComponent<CoffinBody>() : null;
            if (coffin == null || coffin.Grandpa == null) return;

            // дед живой и внутри своего гроба — не слипся с чужим телом
            foreach (var part in coffin.Grandpa.GetComponentsInChildren<GrandpaPart>())
            {
                var prb = part.GetComponent<Rigidbody>();
                if (prb.position.y < coffin.transform.position.y - 1.2f) return; // потерялся внизу
            }

            used = true;
            DIGameManager.I?.Event($"Чекпоинт «{checkpointId}» пройден!");
        }
    }
}
