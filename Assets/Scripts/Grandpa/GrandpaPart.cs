using UnityEngine;

namespace DI
{
    /// Часть тела деда: пересылает столкновения и воду в GrandpaState.
    public class GrandpaPart : MonoBehaviour
    {
        public GrandpaState Root { get; set; }
        public bool IsHead { get; set; }

        void OnCollisionEnter(Collision c) => Root?.RegisterImpact(c, this);

        void OnTriggerStay(Collider other)
        {
            if (other.GetComponent<WaterVolume>() != null)
                Root?.MarkWaterTouch();
        }
    }
}
